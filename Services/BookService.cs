using Microsoft.EntityFrameworkCore;
using BookRec.Models;
using BookRec.Data;


namespace BookRec.Services;

public class BookService
{
    private readonly AppDbContext _context;
    private readonly List<Book> _books = new();
    // **** Remove this --> private readonly List<Book> _userToReadList = new();
    private readonly List<Book> _userSuggestions = new();

    public BookService(AppDbContext context)
    {
        _context = context;
        SeedMockData();
    }


    // search/query
    public async Task<List<Book>> GetAllBooks()
    {
        return await _context.Books.ToListAsync();
    }


    public async Task<int> GetBookCount()
{
    return await _context.Books.CountAsync();
}

    public List<Book> FilterBooks(BookCategory? category, bool? onlyBestsellers, string? searchString)
    {
        var query = _books.AsEnumerable();

        if (category.HasValue)
            query = query.Where(b => b.Category == category.Value);

        if (onlyBestsellers.HasValue && onlyBestsellers.Value)
            query = query.Where(b => b.IsBestseller);

        if (!string.IsNullOrWhiteSpace(searchString))
            query = query.Where(b => b.Title.Contains(searchString, StringComparison.OrdinalIgnoreCase) || 
                             b.Author.Contains(searchString, StringComparison.OrdinalIgnoreCase));

        return query.ToList();
    }

    public async Task<List<UserBook>> GetToReadList(string userId)
    {
        return await _context.UserBooks
            .Where(ub => ub.UserId == userId)
            .OrderByDescending(ub => ub.AddedAt)
            .ToListAsync();
    }

    public async Task AddToReadList(Book book, string userId)
    {
        var existing = await _context.UserBooks
            .AnyAsync(ub =>
                ub.UserId == userId &&
                ub.GoogleBookId == book.GoogleBookId);

        if (!existing)
        {
            var userBook = new UserBook
            {
                UserId = userId,
                GoogleBookId = book.GoogleBookId,
                Title = book.Title,
                Author = book.Author,
                CoverImageUrl = book.CoverImageUrl,
                Status = "To-Read",
                AddedAt = DateTime.UtcNow
            };

            _context.UserBooks.Add(userBook);
            await _context.SaveChangesAsync();
        }

    }

    public async Task RemoveFromReadList(int userBookId, string userId)
    {
        var userBook = await _context.UserBooks
            .FirstOrDefaultAsync(ub =>
            ub.Id == userBookId &&
            ub.UserId == userId);

        if (userBook != null)
        {
            _context.UserBooks.Remove(userBook);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateReadList(int userBookId, string userId, string readStatus)
    {
        var userBook = await _context.UserBooks
            .FirstOrDefaultAsync(ub =>
            ub.Id == userBookId &&
            ub.UserId == userId);

        if (userBook != null)
        {
            userBook.Status = readStatus;
            await _context.SaveChangesAsync();
        }
    }

    // review
    public void AddReview(int bookId, Review review)
    {
        var book = _books.FirstOrDefault(b => b.Id == bookId);
        if (book != null)
        {
            review.Id = book.Reviews.Count + 1;
            book.Reviews.Add(review);
        }
    }

    public void SuggestBook(Book suggestedBook)
    {
        suggestedBook.Id = _userSuggestions.Count + 1;
        _userSuggestions.Add(suggestedBook);
    }

    // seed
    private void SeedMockData()
    {
        _books.Add(new Book { Id = 1, Title = "Atomic Habits", Author = "James Clear", Category = BookCategory.SelfDevelopment, Publisher = "Penguin", IsBestseller = true, Description = "An easy & proven way to build good habits & break bad ones." });
        _books.Add(new Book { Id = 2, Title = "Clean Code", Author = "Robert C. Martin", Category = BookCategory.Programming, Publisher = "Prentice Hall", IsBestseller = true, Description = "A handbook of agile software craftsmanship." });
        _books.Add(new Book { Id = 3, Title = "Life 3.0", Author = "Max Tegmark", Category = BookCategory.AI, Publisher = "Knopf", IsBestseller = false, Description = "Being human in the age of Artificial Intelligence." });
        _books.Add(new Book { Id = 4, Title = "The Psychology of Money", Author = "Morgan Housel", Category = BookCategory.Finance, Publisher = "Harriman House", IsBestseller = true, Description = "Timeless lessons on wealth, greed, and happiness." });
    }
}