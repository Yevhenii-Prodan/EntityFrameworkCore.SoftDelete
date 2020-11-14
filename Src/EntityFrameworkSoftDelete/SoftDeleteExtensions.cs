using System.Linq;
using EntityFrameworkSoftDelete.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkSoftDelete
{
    public static class SoftDeleteExtensions
    {
        public static IQueryable<T> WithDeleted<T>(this IQueryable<T> query) where T : class, ISoftDeletable =>
            query.IgnoreQueryFilters();

        public static IQueryable<T> WithDeleted<T>(this DbSet<T> query) where T : class, ISoftDeletable =>
            query.IgnoreQueryFilters();

        public static IQueryable<T> OnlyDeleted<T>(this IQueryable<T> query) where T : class, ISoftDeletable =>
            query
                .IgnoreQueryFilters()
                .Where(entity => EF.Property<bool>(entity, SoftDeleteConstants.IsDeletedProperty));

        public static IQueryable<T> OnlyDeleted<T>(this DbSet<T> query) where T : class, ISoftDeletable =>
            query
                .IgnoreQueryFilters()
                .Where(entity => EF.Property<bool>(entity, SoftDeleteConstants.IsDeletedProperty));
    }
}