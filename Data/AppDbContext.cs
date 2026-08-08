using Microsoft.EntityFrameworkCore;
using BookRec.Models;

namespace BookRec.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserBook> UserBooks => Set<UserBook>();
    public DbSet<BookReview> BookReviews => Set<BookReview>();
    public DbSet<SearchHistory> SearchHistories => Set<SearchHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserBook>()
            .HasOne(ub => ub.User)
            .WithMany(u => u.SavedBooks)
            .HasForeignKey(ub => ub.UserId);

        modelBuilder.Entity<BookReview>()
            .HasOne(br => br.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(br => br.UserId);

        modelBuilder.Entity<SearchHistory>()
            .HasOne(sh => sh.User)
            .WithMany(u => u.SearchHistories)
            .HasForeignKey(sh => sh.UserId);
    }
}