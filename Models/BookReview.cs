using System.ComponentModel.DataAnnotations;

namespace BookRec.Models;

public class BookReview
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }

    [Required]
    public string GoogleBookId { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; }

    public string ReviewText { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}