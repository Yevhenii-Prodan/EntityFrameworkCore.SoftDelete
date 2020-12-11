using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SoftDelete.IntegrationTests.Database.Entities;
using SoftDelete.IntegrationTests.Helpers;
using Xunit;

namespace SoftDelete.IntegrationTests.Tests
{
    public class RelationshipTests : TestBase
    {
        [Fact]
        public async Task NotDeletePrincipalEntity()
        {
            
            // Seed data
            var author = TestHelper.CreateAuthor;

            var book1 = TestHelper.CreateBook;
            var book2 = TestHelper.CreateBook;

            book1.Author = author;
            book2.Author = author;
            
            await DbContext.AddRangeAsync(author, book1, book2);

            await DbContext.SaveChangesAsync();


            DbContext.Books.Remove(book1);
            await DbContext.SaveChangesAsync();

            var authorFromDb = await DbContext.Authors.FindAsync(author.Id);
            
            authorFromDb.ShouldNotBeNull();

        }

        [Fact]
        public async Task CascadeDelete_OneToMany()
        {
            var book = TestHelper.CreateBook;

            var review1 = TestHelper.CreateReview;
            var review2 = TestHelper.CreateReview;


            review1.Book = book;
            review2.Book = book;
            
            
            await DbContext.AddRangeAsync(book, review1, review2);
            await DbContext.SaveChangesAsync();

            DbContext.Remove(book);
            
            await DbContext.SaveChangesAsync();

            var reviewsCount = DbContext.Reviews.Count(x => x.Id == review1.Id || x.Id == review2.Id);
            
            reviewsCount.ShouldBe(0);
        }

        [Fact]
        public async Task CascadeDelete_OneToOne()
        {

            var author = TestHelper.CreateAuthor;
            
            var book = TestHelper.CreateBook;
            book.Author = author;

            var authorReview = TestHelper.CreateAuthorReview;
            authorReview.Author = author;
            
            authorReview.Book = book;

            await DbContext.AddRangeAsync(author, book, authorReview);
            
            await DbContext.SaveChangesAsync();

            DbContext.Remove(book);
            
            await DbContext.SaveChangesAsync();

            var authorReviewFromDb = await DbContext.AuthorReviews.FindAsync(authorReview.Id);
            authorReviewFromDb.ShouldBeNull();  

        }

        [Fact]
        public async Task CascadeDelete_ManyToMany()
        {
            var book = TestHelper.CreateBook;
            var user = TestHelper.CreateUser;

            var bookId = book.Id;


            var bookUser = new UserBookEntity
            {
                Book = book,
                User = user
            };


            await DbContext.AddRangeAsync(book, user, bookUser);
            await DbContext.SaveChangesAsync();


            DbContext.Remove(book);
            await DbContext.SaveChangesAsync();

            var userFromDb = await DbContext.Users.FindAsync(user.Id);
            
            userFromDb.Books.FirstOrDefault(x => x.BookId == bookId).ShouldBeNull();


        }
        [Fact]
        public async Task SetNull_OneToMany()
        {
            var book = TestHelper.CreateBook;
            var user = TestHelper.CreateUser;

            user.FavouriteBook = book;

            await DbContext.AddRangeAsync(book, user);
            await DbContext.SaveChangesAsync();
            
            DbContext.Remove(book);
            await DbContext.SaveChangesAsync();
            
            user.FavouriteBook.ShouldBeNull();
            user.FavouriteBookId.ShouldBeNull();
            
            var userFromDb = await DbContext.Users.FindAsync(user.Id);
            
            userFromDb.FavouriteBook.ShouldBeNull();
            userFromDb.FavouriteBookId.ShouldBeNull();
            
        }
        
        
        [Fact]
        public async Task SetNull_OneToOne()
        {
            var author = TestHelper.CreateAuthor;

            var book = TestHelper.CreateBook;

            book.Author = author;
            author.MainBook = book;
            
            
            await DbContext.AddRangeAsync(author, book);

            await DbContext.SaveChangesAsync();


            DbContext.Books.Remove(book);
            await DbContext.SaveChangesAsync();
            
            author.MainBook.ShouldBeNull();
            author.MainBookId.ShouldBeNull();

            var authorFromDb = await DbContext.Authors.FindAsync(author.Id);
            
            
            authorFromDb.MainBook.ShouldBeNull();
            authorFromDb.MainBookId.ShouldBeNull();
        }

    }
}