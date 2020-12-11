using System;
using System.Collections.Generic;

namespace SoftDelete.IntegrationTests.Database.Entities
{
    public class UserEntity : BaseEntity
    {
        public string Name { get; set; }
        
        public Guid? FavouriteBookId { get; set; }
        public BookEntity FavouriteBook { get; set; }
        
        public IList<UserBookEntity> Books { get; set; }
    }
}