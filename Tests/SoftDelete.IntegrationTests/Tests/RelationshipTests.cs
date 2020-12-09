using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
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
        public Task CascadeDelete_ManyToMany() => throw new NotImplementedException();
        public Task SetNull_OneToMany() => throw new NotImplementedException();
        public Task SetNull_OneToOne() => throw new NotImplementedException();

    }
}