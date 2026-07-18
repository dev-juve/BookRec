using BookRec.Models;

namespace BookRec.Services;

public class BookService
{
    private readonly List<Book> _books = new();
    private readonly List<Book> _userToReadList = new();
    private readonly List<Book> _userSuggestions = new();

    public BookService()
    {
        SeedMockData();
    }

    // search/query
    public List<Book> GetAllBooks() => _books;

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

    // user book list
    public List<Book> GetToReadList() => _userToReadList;

    public void AddToToReadList(Book book)
    {
        if (!_userToReadList.Any(b => b.Id == book.Id))
        {
            _userToReadList.Add(book);
        }
    }

    public void RemoveFromToReadList(int bookId)
    {
        var book = _userToReadList.FirstOrDefault(b => b.Id == bookId);
        if (book != null) _userToReadList.Remove(book);
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