using EntityFrameworkSoftDelete.Abstractions;

namespace TestProject1.Database.Entities
{
    public class BookEntity: BaseEntity, ISoftDeletable
    {
        public string Name { get; set; }
    }
}