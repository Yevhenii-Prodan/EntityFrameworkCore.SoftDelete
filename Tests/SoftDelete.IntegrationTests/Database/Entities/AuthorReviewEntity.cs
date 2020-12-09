using System;

namespace SoftDelete.IntegrationTests.Database.Entities
{
    public class AuthorReviewEntity : BaseEntity
    {

        public string Text { get; set; }
        
        public Guid BookId { get; set; }
        public BookEntity Book { get; set; }
        
        public Guid AuthorId { get; set; }
        public AuthorEntity Author { get; set; }
    }
}