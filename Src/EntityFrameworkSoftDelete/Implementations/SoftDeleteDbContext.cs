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

namespace EntityFrameworkSoftDelete.Implementations
{
    public class SoftDeleteDbContext : DbContext
    {
        private static readonly MethodInfo PropertyMethod = typeof(EF).GetMethod(nameof(EF.Property), BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(typeof(DateTime?));

        public SoftDeleteDbContext(DbContextOptions options) : base(options)
        {
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
                    .HasQueryFilter(GetIsDeletedRestriction(entity.ClrType));
            }
        }


        private static LambdaExpression GetIsDeletedRestriction(Type type)
        {
            var parm = Expression.Parameter(type, "it");
            var prop = Expression.Call(PropertyMethod, parm, Expression.Constant(SoftDeleteConstants.DeletedDateProperty));
            var condition = Expression.MakeBinary(ExpressionType.Equal, prop, Expression.Constant(null));
            var lambda = Expression.Lambda(condition, parm);
            return lambda;
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void SetNull(EntityEntry entry, IForeignKey fk)
        {
            foreach (var property in fk.Properties)
                entry.Property(property.Name).CurrentValue = null;
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
        
        
        private void OnBeforeSaving()
        {
            foreach (var entry in ChangeTracker.Entries<ISoftDeletable>().ToList())
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
                            if (navigationEntry is CollectionEntry collectionEntry)
                            {
                                collectionEntry.Load();
                                if (collectionEntry.CurrentValue == null)
                                    continue;

                                var collection = new List<EntityEntry>();

                                switch (collectionEntry.Metadata.ForeignKey.DeleteBehavior)
                                {
                                    case DeleteBehavior.SetNull:
                                        collection.AddRange(from object entity in collectionEntry.CurrentValue select Entry(entity));

                                        foreach (var dependentEntry in collection)
                                        {
                                            SetNull(dependentEntry, collectionEntry.Metadata.ForeignKey);
                                        }
                                        break;
                                    
                                    case DeleteBehavior.Cascade:
                                        foreach (var entity in collectionEntry.CurrentValue)
                                            Remove(entity);
                                        break;
                                    
                                    // We only have to process changes in database,
                                    // about changes on the client side the ef core will take care.
                                    
                                    case DeleteBehavior.ClientSetNull:
                                        // No Action required
                                        break;
                                    case DeleteBehavior.Restrict:
                                        // No action required
                                        break;
                                    case DeleteBehavior.ClientCascade:
                                        // No action required
                                        break;
                                    case DeleteBehavior.NoAction:
                                        // No action required
                                        break;
                                    case DeleteBehavior.ClientNoAction:
                                        // No action required
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }
                            else
                            {
                                var dependentEntry = navigationEntry.CurrentValue;
                                if (dependentEntry != null)
                                {
                                    Remove(Entry(dependentEntry));
                                }

                            }
                        }
                        break;
                }
            }
        }
        
        

    }
}
