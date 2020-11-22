using EntityFrameworkSoftDelete.Implementations;
using Microsoft.EntityFrameworkCore;
using TestProject1.Database.Entities;

namespace TestProject1.Database
{
    public class TestDbContext : SoftDeleteDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
        
        public DbSet<BookEntity> Books { get; set; }
    }
}