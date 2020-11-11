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
        private const string IsDeletedProperty = "IsDeleted";
        private static readonly MethodInfo PropertyMethod = typeof(EF).GetMethod(nameof(EF.Property), BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(typeof(bool));

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            foreach (var entity in builder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDeletable).IsAssignableFrom(entity.ClrType) != true) continue;
                entity.AddProperty(IsDeletedProperty, typeof(bool));

                builder
                    .Entity(entity.ClrType)
                    .HasQueryFilter(GetIsDeletedRestriction(entity.ClrType));
            }
        }


        private static LambdaExpression GetIsDeletedRestriction(Type type)
        {
            var parm = Expression.Parameter(type, "it");
            var prop = Expression.Call(PropertyMethod, parm, Expression.Constant(IsDeletedProperty));
            var condition = Expression.MakeBinary(ExpressionType.Equal, prop, Expression.Constant(false));
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
            if ((bool)entry.Property(IsDeletedProperty).CurrentValue == true)
                entry.Property(IsDeletedProperty).CurrentValue = false;
        }

        public void RestoreRange(IEnumerable<ISoftDeletable> entities)
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
                        entry.CurrentValues["IsDeleted"] = false;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.CurrentValues["IsDeleted"] = true;
                        
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
