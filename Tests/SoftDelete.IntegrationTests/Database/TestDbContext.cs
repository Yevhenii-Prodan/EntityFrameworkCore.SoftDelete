using EntityFrameworkSoftDelete.Implementations;
using Microsoft.EntityFrameworkCore;
using SoftDelete.IntegrationTests.Database.Entities;

namespace SoftDelete.IntegrationTests.Database
{
    public class TestDbContext : SoftDeleteDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
        
        public DbSet<BookEntity> Books { get; set; }
        public DbSet<AuthorEntity> Authors { get; set; }
        public DbSet<ReviewEntity> Reviews { get; set; }
        public DbSet<AuthorReviewEntity> AuthorReviews { get; set; }

        public DbSet<UserEntity> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserBookEntity>(entity =>
            {

                entity.HasKey(x => new {x.BookId, x.UserId});

                entity
                    .HasOne(x => x.User)
                    .WithMany(x => x.Books)
                    .HasForeignKey(x => x.UserId);
                
                
                entity
                    .HasOne(x => x.Book)
                    .WithMany(x => x.Users)
                    .HasForeignKey(x => x.BookId);

            });


            builder.Entity<UserEntity>(entity =>
            {

                entity
                    .HasOne(x => x.FavouriteBook)
                    .WithMany(x => x.UserFavourites)
                    .HasForeignKey(x => x.FavouriteBookId)
                    .OnDelete(DeleteBehavior.SetNull);


            });

            builder.Entity<AuthorEntity>(entity =>
            {
                entity
                    .HasOne(x => x.MainBook)
                    .WithOne(x => x.AuthorMainBook)
                    .OnDelete(DeleteBehavior.SetNull);

                entity
                    .HasMany(x => x.Books)
                    .WithOne(x => x.Author)
                    .HasForeignKey(x => x.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

            });

        }
    }
}