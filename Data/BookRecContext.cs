using Microsoft.EntityFrameworkCore;
using BookRec.Models;

namespace BookRec.Data;

public class BookRecContext : DbContext
{
    public BookRecContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet <Book> Books { get; set; }
    public DbSet <Review> Reviews { get; set; }

}