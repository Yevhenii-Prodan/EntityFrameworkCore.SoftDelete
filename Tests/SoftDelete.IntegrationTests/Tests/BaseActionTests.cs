using System;
using System.Threading.Tasks;
using EntityFrameworkSoftDelete;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using SoftDelete.IntegrationTests.Database;
using SoftDelete.IntegrationTests.Helpers;
using Xunit;

namespace SoftDelete.IntegrationTests.Tests
{
    public class BaseActionTests : TestBase
    {

        [Fact]
        public async Task CanSoftDelete()
        {
            var book = TestHelper.CreateBook;
            var author = TestHelper.CreateAuthor;
            book.Author = author;
            
            await DbContext.Authors.AddAsync(author);
            await DbContext.Books.AddAsync(book);
            await DbContext.SaveChangesAsync();

            var bookId = book.Id;

            DbContext.Books.Remove(book);
            await DbContext.SaveChangesAsync();

            var bookFromDb = await DbContext.Books.FindAsync(bookId);
            bookFromDb.ShouldBeNull();
            
            bookFromDb = await DbContext.Books.WithDeleted().FirstOrDefaultAsync(x => x.Id == bookId);

            bookFromDb.ShouldNotBeNull();
        }

        [Fact]
        public async Task CanRestore()
        {
            var book = TestHelper.CreateBook;
            var author = TestHelper.CreateAuthor;
            book.Author = author;
            
            await DbContext.Authors.AddAsync(author);
            await DbContext.Books.AddAsync(book);
            await DbContext.SaveChangesAsync();
            
            var bookId = book.Id;

            DbContext.Books.Remove(book);
            await DbContext.SaveChangesAsync();
            
            var deletedBook = await DbContext.Books.WithDeleted().FirstOrDefaultAsync(x => x.Id == bookId);
            
            DbContext.Books.Restore(deletedBook);
            await DbContext.SaveChangesAsync();

            var restoredBook = await DbContext.Books.FindAsync(bookId);
            restoredBook.ShouldNotBeNull();

        }

        [Fact]
        public async Task CanHardRemove()
        {
            var book = TestHelper.CreateBook;
            var author = TestHelper.CreateAuthor;

            book.Author = author;

            await DbContext.Authors.AddAsync(author);
            await DbContext.Books.AddAsync(book);
            await DbContext.SaveChangesAsync();
            
            var bookId = book.Id;
            
            
            
            DbContext.HardRemove(book);
            await DbContext.SaveChangesAsync();

            var deletedBook = await DbContext.Books.WithDeleted().FirstOrDefaultAsync(x => x.Id == bookId);
            
            deletedBook.ShouldBeNull();


        }
    }
}