using System;

namespace SoftDelete.IntegrationTests.Database.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
    }
}