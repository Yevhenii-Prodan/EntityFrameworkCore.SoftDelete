﻿using System;
using System.Collections.Generic;
using EntityFrameworkSoftDelete.Abstractions;

namespace SoftDelete.IntegrationTests.Database.Entities
{
    public class BookEntity: BaseEntity, ISoftDeletable
    {
        public string Name { get; set; }

        
        
        public Guid AuthorId { get; set; }
        public AuthorEntity Author { get; set; }


        public AuthorReviewEntity AuthorReviewEntity { get; set; }

        public AuthorEntity AuthorMainBook { get; set; }
        
        
        
        public IList<ReviewEntity> Reviews { get; set; }

        public IList<UserBookEntity> Users { get; set; }

        public IList<UserEntity> UserFavourites { get; set; }
    }
}