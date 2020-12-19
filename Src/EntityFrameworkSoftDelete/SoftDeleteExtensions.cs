using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EntityFrameworkSoftDelete.Abstractions;
using EntityFrameworkSoftDelete.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFrameworkSoftDelete
{
    public static class SoftDeleteExtensions
    {
        public static IQueryable<T> WithDeleted<T>(this IQueryable<T> source) where T : class, ISoftDeletable =>
            source.IgnoreQueryFilters();

        public static IQueryable<T> WithDeleted<T>(this DbSet<T> source) where T : class, ISoftDeletable =>
            source.IgnoreQueryFilters();

        public static IQueryable<T> OnlyDeleted<T>(this IQueryable<T> source) where T : class, ISoftDeletable =>
            source
                .IgnoreQueryFilters()
                .Where(entity => EF.Property<DateTime?>(entity, SoftDeleteConstants.DeletedDateProperty) != null);

        public static IQueryable<T> OnlyDeleted<T>(this DbSet<T> source) where T : class, ISoftDeletable =>
            source
                .IgnoreQueryFilters()
                .Where(entity => EF.Property<DateTime?>(entity, SoftDeleteConstants.DeletedDateProperty) != null);

        public static void Restore<TEntity>(this DbSet<TEntity> dbSet, [NotNull] TEntity entity) where TEntity : class, ISoftDeletable
        {
            var context = dbSet.GetService<ICurrentDbContext>().Context;
            var softDeleteContext = (SoftDeleteDbContext) context;
            softDeleteContext.Restore(entity);
        }
        
        public static void RestoreRange<TEntity>(this DbSet<TEntity> dbSet, [NotNull] params TEntity[] entities) where TEntity : class, ISoftDeletable
        {
            var context = dbSet.GetService<ICurrentDbContext>().Context;
            var softDeleteContext = (SoftDeleteDbContext) context;
            softDeleteContext.RestoreRange(entities);
        }
        
        public static void HardRemove<TEntity>(this DbSet<TEntity> dbSet, [NotNull] TEntity entity) where TEntity : class, ISoftDeletable
        {
            var context = dbSet.GetService<ICurrentDbContext>().Context;
            var softDeleteContext = (SoftDeleteDbContext) context;
            softDeleteContext.HardRemove(entity);
        }
        
        public static void HardRemoveRange<TEntity>(this DbSet<TEntity> dbSet, [NotNull] params TEntity[] entities) where TEntity : class, ISoftDeletable
        {
            var context = dbSet.GetService<ICurrentDbContext>().Context;
            var softDeleteContext = (SoftDeleteDbContext) context;
            softDeleteContext.HardRemoveRange(entities);
        }
        
        

    }
}