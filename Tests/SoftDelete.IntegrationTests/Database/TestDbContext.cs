using EntityFrameworkSoftDelete.Implementations;
using Microsoft.EntityFrameworkCore;
using SoftDelete.IntegrationTests.Database.Entities;

namespace SoftDelete.IntegrationTests.Database
{
    public class TestDbContext : SoftDeleteDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
        
        public DbSet<BookEntity> Books { get; set; }
        public DbSet<AuthorEntity> Authors { get; set; }
        public DbSet<ReviewEntity> Reviews { get; set; }
        public DbSet<AuthorReviewEntity> AuthorReviews { get; set; }
    }
}