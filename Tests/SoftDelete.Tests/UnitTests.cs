using System;
using System.Threading.Tasks;
using EntityFrameworkSoftDelete;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using TestProject1.Database;
using TestProject1.Database.Entities;
using Xunit;

namespace TestProject1
{
    public class UnitTest1
    {
        private readonly TestDbContext _dbContext;

        public UnitTest1()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new TestDbContext(options);
            _dbContext.Database.EnsureCreated();
        }

        [Fact]
        public async Task CanSoftDelete()
        {
            var book = new BookEntity
            {
                Name = "The Lord of the ring"
            };

            await _dbContext.Books.AddAsync(book);
            await _dbContext.SaveChangesAsync();


            var bookId = book.Id;

            _dbContext.Books.Remove(book);
            await _dbContext.SaveChangesAsync();

            var bookFromDb = await _dbContext.Books.FindAsync(bookId);
            bookFromDb.ShouldBeNull();
            
            bookFromDb = await _dbContext.Books.WithDeleted().FirstOrDefaultAsync(x => x.Id == bookId);

            bookFromDb.ShouldNotBeNull();
        }
    }
}