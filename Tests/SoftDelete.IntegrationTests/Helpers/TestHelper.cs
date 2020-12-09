using System;
using Bogus;
using SoftDelete.IntegrationTests.Database.Entities;

namespace SoftDelete.IntegrationTests.Helpers
{
    public static class TestHelper
    {
        private static readonly Faker<AuthorEntity> AuthorFaker;
        private static readonly Faker<ReviewEntity> ReviewFaker;
        private static readonly Faker<BookEntity> BookFaker;
        private static readonly Faker<AuthorReviewEntity> AuthorReviewFaker;

        static TestHelper()
        {
            AuthorFaker = new Faker<AuthorEntity>();
            AuthorFaker.RuleFor(x => x.Name, x => x.Name.FullName());
            AuthorFaker.RuleFor(x => x.Id, x => new Guid());


            ReviewFaker = new Faker<ReviewEntity>();
            ReviewFaker.RuleFor(x => x.AuthorName, x => x.Name.FullName());
            ReviewFaker.RuleFor(x => x.Text, x => x.Lorem.Paragraph());
            ReviewFaker.RuleFor(x => x.Id, x => new Guid());

            BookFaker = new Faker<BookEntity>();
            BookFaker.RuleFor(x => x.Name, x => x.Lorem.Lines());
            BookFaker.RuleFor(x => x.Id, x => new Guid());
            
            
            AuthorReviewFaker = new Faker<AuthorReviewEntity>();
            AuthorReviewFaker.RuleFor(x => x.Text, x => x.Lorem.Paragraph());
            AuthorReviewFaker.RuleFor(x => x.Id, x => new Guid());

        }

        public static AuthorEntity CreateAuthor => AuthorFaker.Generate();
        public static BookEntity CreateBook => BookFaker.Generate();
        public static ReviewEntity CreateReview => ReviewFaker.Generate();
        public static AuthorReviewEntity CreateAuthorReview => AuthorReviewFaker.Generate();
    }
}