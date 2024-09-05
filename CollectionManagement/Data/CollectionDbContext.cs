using CollectionManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CollectionManagement.Data;

public class CollectionDbContext : DbContext
{
    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Collection> Collections { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Like> Likes { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<ItemTag> ItemTags { get; set; }

    public CollectionDbContext(DbContextOptions<CollectionDbContext> options) : base(options)
    {
        //Database.Migrate();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Like>()
            .HasOne(l => l.LikedUser)
            .WithMany(u => u.Likes)
            .HasForeignKey(l => l.LikedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Like>()
            .HasOne(l => l.Item)
            .WithMany(i => i.Likes)
            .HasForeignKey(l => l.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Item)
            .WithMany(i => i.Comments)
            .HasForeignKey(c => c.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}