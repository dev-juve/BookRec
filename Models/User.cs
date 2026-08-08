using System.ComponentModel.DataAnnotations;

namespace BookRec.Models;

public class User
{
    [Key]
    public string Id { get; set; } = string.Empty; 

    [Required]
    public string Username { get; set; } = string.Empty;

    public string? Email { get; set; }
    
    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public List<UserBook> SavedBooks { get; set; } = new();
    public List<BookReview> Reviews { get; set; } = new();
    public List<SearchHistory> SearchHistories { get; set; } = new();
}