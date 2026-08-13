using BookRec.Models;
using Microsoft.EntityFrameworkCore;

namespace BookRec.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<SearchHistory> SearchHistories { get; set; }
    public DbSet<SearchFeedback> SearchFeedbacks { get; set; }
    public DbSet<UserToReadBook> UserToReadBooks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // explicitly define the relationships
        
        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SearchHistory>()
            .HasOne(sh => sh.User)
            .WithMany(u => u.SearchHistories)
            .HasForeignKey(sh => sh.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SearchFeedback>()
            .HasOne(sf => sf.User)
            .WithMany(u => u.SearchFeedbacks)
            .HasForeignKey(sf => sf.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserToReadBook>()
            .HasOne(urb => urb.User)
            .WithMany(u => u.SavedBooks)
            .HasForeignKey(urb => urb.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}