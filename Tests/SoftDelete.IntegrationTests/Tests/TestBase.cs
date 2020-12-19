using System;
using Microsoft.EntityFrameworkCore;
using SoftDelete.IntegrationTests.Database;

namespace SoftDelete.IntegrationTests.Tests
{
    public abstract class TestBase
    {
        protected readonly TestDbContext DbContext;

        
        public TestBase()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                // .UseSqlServer("Server=.\\SQLEXPRESS;Database=soft-delete-test;Trusted_Connection=True;MultipleActiveResultSets=true")
                .Options;

            DbContext = new TestDbContext(options);
            DbContext.Database.EnsureCreated();
        }
    }
}