using System.ComponentModel.DataAnnotations;

namespace BookRec.Models;

public class SearchFeedback
{
    public int Id { get; set; }
    
    // foreign key to the user table
    [Required]
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }

    public string Query { get; set; } = string.Empty;
    public bool IsHelpful { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}