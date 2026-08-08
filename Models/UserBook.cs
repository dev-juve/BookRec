using System.ComponentModel.DataAnnotations;

namespace BookRec.Models;

public class UserBook
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }

    [Required]
    public string GoogleBookId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string Status { get; set; } = "To-Read"; // "To-Read", "Reading", "Completed"

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}