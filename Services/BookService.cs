using Microsoft.EntityFrameworkCore;
using BookRec.Models;
using BookRec.Data;

namespace BookRec.Services;

public class BookService
{
    private readonly AppDbContext _context;
    private readonly List<Book> _books = new();
    private readonly List<Book> _userSuggestions = new();

    public BookService(AppDbContext context)
    {
        _context = context;
        SeedMockData();
    }

    // gets the trending books from our db
    public async Task<List<Book>> GetTrendingLocalBooksAsync()
    {
        var localBooks = await _context.LocalBooks
            .Where(b => b.IsTrending)
            .OrderByDescending(b => b.AddedToDatabaseAt)
            .ToListAsync();

        return localBooks.Select(lb => new Book
        {
            Id = lb.Id,
            GoogleBookId = lb.GoogleBookId,
            Title = lb.Title,
            Author = lb.Author,
            Description = lb.Description ?? "No description available.",
            CoverImageUrl = lb.CoverImageUrl ?? "/images/default-cover.svg",
            DisplayCategory = lb.Category ?? "Trending",
            IsBestseller = lb.IsTrending
        }).ToList();
    }

    // adding some local books if the table is empty so it doesn't look weird
    public async Task SeedLocalBooksAsync()
    {
        if (await _context.LocalBooks.CountAsync() < 12)
        {
            var existingBooks = await _context.LocalBooks.ToListAsync();
            _context.LocalBooks.RemoveRange(existingBooks);
            await _context.SaveChangesAsync();

            var initialBooks = new List<LocalBook>
            {
                new LocalBook { GoogleBookId = "XfFvDwAAQBAJ", Title = "Atomic Habits", Author = "James Clear", Description = "An easy & proven way to build good habits & break bad ones.", CoverImageUrl = "/atomic-habits.webp", Category = "Self-Development", IsTrending = true },
                new LocalBook { GoogleBookId = "_i6bDeoCQzsC", Title = "Clean Code", Author = "Robert C. Martin", Description = "A handbook of agile software craftsmanship.", CoverImageUrl = "/clean-code.webp", Category = "Programming", IsTrending = true },
                new LocalBook { GoogleBookId = "2hIcDgAAQBAJ", Title = "Life 3.0", Author = "Max Tegmark", Description = "Being human in the age of Artificial Intelligence.", CoverImageUrl = "/life-3.0.webp", Category = "AI & Future", IsTrending = true },
                new LocalBook { GoogleBookId = "P_xcEAAAQBAJ", Title = "The Psychology of Money", Author = "Morgan Housel", Description = "Timeless lessons on wealth, greed, and happiness.", CoverImageUrl = "/the-psychology-of-money.webp", Category = "Finance", IsTrending = true },
                new LocalBook { GoogleBookId = "WW44zgEACAAJ", Title = "Deep Work", Author = "Cal Newport", Description = "Rules for focused success in a distracted world.", CoverImageUrl = "/deep-work.webp", Category = "Self-Development", IsTrending = true },
                new LocalBook { GoogleBookId = "OFSTEAAAQBAJ", Title = "Thinking, Fast and Slow", Author = "Daniel Kahneman", Description = "A groundbreaking tour of the mind and the two systems that drive the way we think.", CoverImageUrl = "/thinking-fast-and-slow.webp", Category = "Psychology", IsTrending = true },
                new LocalBook { GoogleBookId = "5wBQEp6ruIAC", Title = "The Pragmatic Programmer", Author = "David Thomas, Andrew Hunt", Description = "From journeyman to master: cutting through the specialization of modern software development.", CoverImageUrl = "/the-pragmatic-programmer.webp", Category = "Programming", IsTrending = true },
                new LocalBook { GoogleBookId = "RmdqCgAAQBAJ", Title = "Never Split the Difference", Author = "Chris Voss", Description = "Negotiating as if your life depended on it.", CoverImageUrl = "/never-split-the-difference.webp", Category = "Business", IsTrending = true },
                new LocalBook { GoogleBookId = "uEVoPgAACAAJ", Title = "Dune", Author = "Frank Herbert", Description = "A mythic and emotionally charged hero's journey on the desert planet Arrakis.", CoverImageUrl = "/dune.webp", Category = "Fiction", IsTrending = true },
                new LocalBook { GoogleBookId = "FmyBAwAAQBAJ", Title = "Sapiens", Author = "Yuval Noah Harari", Description = "A brief history of humankind.", CoverImageUrl = "/sapiens.webp", Category = "History", IsTrending = true },
                new LocalBook { GoogleBookId = "CnSJEQAAQBAJ", Title = "Project Hail Mary", Author = "Andy Weir", Description = "A lone astronaut must save the earth from disaster in this cinematic thriller.", CoverImageUrl = "/project-hail-hary.webp", Category = "Fiction", IsTrending = true },
                new LocalBook { GoogleBookId = "zFheDgAAQBAJ", Title = "Designing Data-Intensive Applications", Author = "Martin Kleppmann", Description = "The big ideas behind reliable, scalable, and maintainable backend systems.", CoverImageUrl = "/designing-data-intensive-applications.webp", Category = "Programming", IsTrending = true }
            };

            _context.LocalBooks.AddRange(initialBooks);
            await _context.SaveChangesAsync();
        }
    }

    // keep track of what the user searches for
    public async Task LogUserSearchAsync(string userId, string query)
    {
        if (string.IsNullOrWhiteSpace(query) || string.IsNullOrWhiteSpace(userId))
            return;

        var normalizedQuery = query.Trim();

        var existingSearch = await _context.SearchHistories
            .FirstOrDefaultAsync(sh => sh.UserId == userId && sh.SearchQuery.ToLower() == normalizedQuery.ToLower());

        // delete the old search if they search the exact same thing again
        if (existingSearch != null)
        {
            _context.SearchHistories.Remove(existingSearch);
            await _context.SaveChangesAsync();
        }

        var newSearch = new SearchHistory
        {
            UserId = userId,
            SearchQuery = normalizedQuery
        };

        _context.SearchHistories.Add(newSearch);
        await _context.SaveChangesAsync();

        var userSearchesCount = await _context.SearchHistories.CountAsync(sh => sh.UserId == userId);
        
        // don't let the table get too huge, keep only the last 10
        if (userSearchesCount > 10)
        {
            var searchesToRemove = await _context.SearchHistories
                .Where(sh => sh.UserId == userId)
                .OrderByDescending(sh => sh.Id)
                .Skip(10)
                .ToListAsync();

            _context.SearchHistories.RemoveRange(searchesToRemove);
            await _context.SaveChangesAsync();
        }
    }

    // returns all the books
    public async Task<List<Book>> GetAllBooks()
    {
        return await Task.FromResult(_books);
    }

    public async Task<int> GetBookCount()
    {
        return await _context.Books.CountAsync();
    }

    // filters the book list based on what was passed in
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

    // grabs the user's reading list from the db
    public async Task<List<UserToReadBook>> GetToReadList(string userId)
    {
        return await _context.UserToReadBooks
            .Where(ub => ub.UserId == userId)
            .OrderByDescending(ub => ub.AddedAt)
            .ToListAsync();
    }

    // adds a book to the user's reading list
    public async Task AddToReadList(Book book, string userId)
    {
        var existing = await _context.UserToReadBooks
            .AnyAsync(ub =>
                ub.UserId == userId &&
                ub.GoogleBookId == book.GoogleBookId);

        if (!existing)
        {
            var userBook = new UserToReadBook
            {
                UserId = userId,
                GoogleBookId = book.GoogleBookId,
                Title = book.Title,
                Author = book.Author,
                CoverImageUrl = book.CoverImageUrl,
                Status = "To-Read",
                AddedAt = DateTime.UtcNow
            };

            _context.UserToReadBooks.Add(userBook);
            await _context.SaveChangesAsync();
        }
    }

    // deletes a book from the reading list
    public async Task RemoveFromReadList(int userBookId, string userId)
    {
        var userBook = await _context.UserToReadBooks
            .FirstOrDefaultAsync(ub =>
            ub.Id == userBookId &&
            ub.UserId == userId);

        if (userBook != null)
        {
            _context.UserToReadBooks.Remove(userBook);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateReadList(int userBookId, string userId, string readStatus)
    {
        var userBook = await _context.UserToReadBooks
            .FirstOrDefaultAsync(ub =>
            ub.Id == userBookId &&
            ub.UserId == userId);

        if (userBook != null)
        {
            userBook.Status = readStatus;
            await _context.SaveChangesAsync();
        }
    }

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

    private void SeedMockData()
    {
        _books.Add(new Book { Id = 1, GoogleBookId = "XfFvDwAAQBAJ", Title = "Atomic Habits", Author = "James Clear", Category = BookCategory.SelfDevelopment, Publisher = "Penguin", IsBestseller = true, Description = "An easy & proven way to build good habits & break bad ones.", CoverImageUrl = "/atomic-habits.webp" });
        _books.Add(new Book { Id = 2, GoogleBookId = "_i6bDeoCQzsC", Title = "Clean Code", Author = "Robert C. Martin", Category = BookCategory.Programming, Publisher = "Prentice Hall", IsBestseller = true, Description = "A handbook of agile software craftsmanship.", CoverImageUrl = "/clean-code.webp" });
        _books.Add(new Book { Id = 3, GoogleBookId = "2hIcDgAAQBAJ", Title = "Life 3.0", Author = "Max Tegmark", Category = BookCategory.AI, Publisher = "Knopf", IsBestseller = false, Description = "Being human in the age of Artificial Intelligence.", CoverImageUrl = "/life-3.0.webp" });
        _books.Add(new Book { Id = 4, GoogleBookId = "P_xcEAAAQBAJ", Title = "The Psychology of Money", Author = "Morgan Housel", Category = BookCategory.Finance, Publisher = "Harriman House", IsBestseller = true, Description = "Timeless lessons on wealth, greed, and happiness.", CoverImageUrl = "/the-psychology-of-money.webp" });
    }
}