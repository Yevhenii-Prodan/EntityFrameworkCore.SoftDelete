using System;
using System.Collections.Generic;

namespace SoftDelete.IntegrationTests.Database.Entities
{
    public class AuthorEntity : BaseEntity
    {
        public string Name { get; set; }

        public Guid? MainBookId { get; set; }
        public BookEntity MainBook { get; set; }

        public IList<BookEntity> Books { get; set; }
    }
}