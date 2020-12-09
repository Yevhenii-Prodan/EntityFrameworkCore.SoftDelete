using System;

namespace SoftDelete.IntegrationTests.Database.Entities
{
    public class ReviewEntity : BaseEntity
    {
        public string AuthorName { get; set; }
        public string Text { get; set; }

        public Guid BookId { get; set; }
        public BookEntity Book { get; set; }
    }
}