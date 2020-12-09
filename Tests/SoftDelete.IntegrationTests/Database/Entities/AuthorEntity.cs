using System.Collections.Generic;

namespace SoftDelete.IntegrationTests.Database.Entities
{
    public class AuthorEntity : BaseEntity
    {
        public string Name { get; set; }

        public IList<BookEntity> Books { get; set; }
    }
}