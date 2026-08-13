using System.ComponentModel.DataAnnotations;

namespace BookRec.Models;

public class User
{
    [Key]
    public string Id { get; set; } = string.Empty; // We will use the GitHub Provider ID here

    [Required]
    public string? Username { get; set; } = string.Empty;

    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    public List<UserToReadBook> SavedBooks { get; set; } = new();
    public List<Review> Reviews { get; set; } = new(); 
    public List<SearchHistory> SearchHistories { get; set; } = new();
    public List<SearchFeedback> SearchFeedbacks { get; set; } = new();
}