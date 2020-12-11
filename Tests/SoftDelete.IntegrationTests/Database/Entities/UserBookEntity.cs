using System;

namespace SoftDelete.IntegrationTests.Database.Entities
{
    public class UserBookEntity
    {
        public Guid UserId { get; set; }
        public UserEntity User { get; set; }

        public Guid BookId { get; set; }
        public BookEntity Book { get; set; }
    }
}