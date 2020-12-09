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
                .Options;

            DbContext = new TestDbContext(options);
            DbContext.Database.EnsureCreated();
        }
    }
}