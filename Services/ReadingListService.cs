using BookRec.Data;
using BookRec.Models;
using Microsoft.EntityFrameworkCore;

namespace BookRec.Services;

public class ReadingListService
{
    private readonly AppDbContext _db;
    private readonly UserService _userService;

    public ReadingListService(AppDbContext db, UserService userService)
    {
        _db = db;
        _userService = userService;
    }

    public async Task<List<UserBook>> GetBooksAsync(string status)
    {
        var userId = _userService.Id;

        if (string.IsNullOrWhiteSpace(userId))
            return new List<UserBook>();

        return await _db.UserBooks
            .AsNoTracking()
            .Where(b => b.UserId == userId && b.Status == status)
            .OrderByDescending(b => b.AddedAt)
            .ToListAsync();
    }

    public async Task<bool> AddToToReadAsync(Book book)
    {
        var userId = _userService.Id;

        if (string.IsNullOrWhiteSpace(userId))
            return false;

        var googleBookId = book.Id.ToString();

        var existingBook = await _db.UserBooks
            .FirstOrDefaultAsync(b =>
                b.UserId == userId &&
                b.GoogleBookId == googleBookId);

        if (existingBook != null)
            return false;

        var userBook = new UserBook
        {
            UserId = userId,
            GoogleBookId = googleBookId,
            Title = book.Title,
            Author = book.Author,
            CoverImageUrl = book.CoverImageUrl,
            Status = "To-Read",
            AddedAt = DateTime.UtcNow
        };

        _db.UserBooks.Add(userBook);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task UpdateStatusAsync(int userBookId, string newStatus)
    {
        var userId = _userService.Id;

        if (string.IsNullOrWhiteSpace(userId))
            return;

        var allowedStatuses = new[]
        {
            "To-Read",
            "Reading",
            "Completed"
        };

        if (!allowedStatuses.Contains(newStatus))
            return;

        var book = await _db.UserBooks
            .FirstOrDefaultAsync(b =>
                b.Id == userBookId &&
                b.UserId == userId);

        if (book == null)
            return;

        book.Status = newStatus;

        await _db.SaveChangesAsync();
    }

    public async Task RemoveAsync(int userBookId)
    {
        var userId = _userService.Id;

        if (string.IsNullOrWhiteSpace(userId))
            return;

        var book = await _db.UserBooks
            .FirstOrDefaultAsync(b =>
                b.Id == userBookId &&
                b.UserId == userId);

        if (book == null)
            return;

        _db.UserBooks.Remove(book);
        await _db.SaveChangesAsync();
    }
}