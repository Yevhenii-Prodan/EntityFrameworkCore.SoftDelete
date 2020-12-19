using EntityFrameworkSoftDelete.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EntityFrameworkSoftDelete.Implementations
{
    public abstract class SoftDeleteDbContext : DbContext
    {
        protected SoftDeleteDbContext(DbContextOptions options) : base(options)
        {
            
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var entriesToDetached = OnBeforeSaving();
            var result =  base.SaveChanges(acceptAllChangesOnSuccess);
            DetachEntries(entriesToDetached);
            return result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entriesToDetached = OnBeforeSaving();
            var result =  await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            DetachEntries(entriesToDetached);

            return result;
        }
        
        public void Restore(ISoftDeletable entity)
        {
            var entry = ChangeTracker.Entries().First(en => en.Entity == entity);
            if ((DateTime?)entry.Property(SoftDeleteConstants.DeletedDateProperty).CurrentValue != null)
                entry.Property(SoftDeleteConstants.DeletedDateProperty).CurrentValue = null;
        }

        public void RestoreRange(params ISoftDeletable[] entities)
        {
            foreach (var entity in entities)
                Restore(entity);
        }

        public void HardRemove(ISoftDeletable entity)
        {
            var entry = ChangeTracker.Entries().First(en => en.Entity == entity);
            
            // a hack to detect hard deleted entities
            entry.CurrentValues[SoftDeleteConstants.DeletedDateProperty] = DateTime.MinValue;
            
            Remove(entity);

        }

        public void HardRemoveRange(params ISoftDeletable[] entities)
        {
            foreach (var entity in entities)
            {
                HardRemove(entity);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var entity in builder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDeletable).IsAssignableFrom(entity.ClrType) != true) continue;
                entity.AddProperty(SoftDeleteConstants.DeletedDateProperty, typeof(DateTime?));

                builder
                    .Entity(entity.ClrType)
                    .HasQueryFilter(GetIsDeletedRestriction(entity.ClrType))
                    .HasIndex(SoftDeleteConstants.DeletedDateProperty);
            }
        }

        #region Private
        
        private static readonly MethodInfo PropertyMethod = typeof(EF).GetMethod(nameof(EF.Property), BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(typeof(DateTime?));


        private static LambdaExpression GetIsDeletedRestriction(Type type)
        {
            var parm = Expression.Parameter(type, "it");
            var prop = Expression.Call(PropertyMethod, parm, Expression.Constant(SoftDeleteConstants.DeletedDateProperty));
            var condition = Expression.MakeBinary(ExpressionType.Equal, prop, Expression.Constant(null));
            var lambda = Expression.Lambda(condition, parm);
            return lambda;
        }
        
        private void DetachEntries(IEnumerable<EntityEntry<ISoftDeletable>> entries)
        {
            foreach (var entityEntry in entries)
            {
                entityEntry.State = EntityState.Detached;
            }
        }

        private void SetNull(EntityEntry entry, IForeignKey fk)
        {
            foreach (var property in fk.Properties)
                entry.Property(property.Name).CurrentValue = null;
        }


        private IEnumerable<EntityEntry<ISoftDeletable>> OnBeforeSaving()
        {

            var softDeleteEntries = ChangeTracker.Entries<ISoftDeletable>()
                // a hack to detect hard deleted entities
                .Where(entry => (DateTime?)entry.Property(SoftDeleteConstants.DeletedDateProperty).CurrentValue != DateTime.MinValue)
                .ToList();
            
            var entriesToDetached = softDeleteEntries.Where(entry => entry.State == EntityState.Deleted).ToList(); 
            
            
            foreach (var entry in softDeleteEntries)
            {
                
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.CurrentValues[SoftDeleteConstants.DeletedDateProperty] = null;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.CurrentValues[SoftDeleteConstants.DeletedDateProperty] = DateTime.UtcNow;
                        
                        foreach (var navigationEntry in entry.Navigations.Where(n => !n.Metadata.IsDependentToPrincipal()))
                        {
                            
                            switch (navigationEntry)
                            {
                                case CollectionEntry collectionEntry:
                                    ProcessEntry(collectionEntry);
                                    break;
                                case ReferenceEntry referenceEntry:
                                    ProcessEntry(referenceEntry);
                                    break;
                            }
                        }
                        break;
                }
            }

            return entriesToDetached;
        }
        
        private void ProcessEntry(CollectionEntry collectionEntry)
        {
            if (!collectionEntry.IsLoaded)
                collectionEntry.Load();
            
            if (collectionEntry.CurrentValue == null)
                return;

            var collection = new List<EntityEntry>();

            switch (collectionEntry.Metadata.ForeignKey.DeleteBehavior)
            {
                // We only have to process changes in database,
                // about changes on the client side the ef core will take care.
                case DeleteBehavior.SetNull:
                    collection.AddRange(from object entity in collectionEntry.CurrentValue select Entry(entity));
                    foreach (var dependentEntry in collection)
                        SetNull(dependentEntry, collectionEntry.Metadata.ForeignKey);
                    break;
                                    
                case DeleteBehavior.Cascade:
                    foreach (var entity in collectionEntry.CurrentValue)
                        Remove(entity);
                    break;
                                    
                // Do nothing for other cases

            }
        }

        private void ProcessEntry(ReferenceEntry referenceEntry)
        {
            if (!referenceEntry.IsLoaded)
                referenceEntry.Load();
            
            var dependentEntry = referenceEntry.CurrentValue;
            if (dependentEntry == null)
                return;

            switch (referenceEntry.Metadata.ForeignKey.DeleteBehavior)
            {
                // We only have to process changes in database,
                // about changes on the client side the ef core will take care.
                case DeleteBehavior.Cascade:
                    Remove(dependentEntry);
                    break;
                case DeleteBehavior.SetNull:
                    SetNull(Entry(dependentEntry), referenceEntry.Metadata.ForeignKey);
                    break;
                // Do nothing for other cases
            }
        }

        #endregion
    }
}
