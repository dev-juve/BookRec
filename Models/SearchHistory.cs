using System.ComponentModel.DataAnnotations;

namespace BookRec.Models;

public class SearchHistory
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }

    [Required]
    public string SearchQuery { get; set; } = string.Empty;

    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
}