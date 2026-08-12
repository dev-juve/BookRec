namespace BookRec.Models;

public class SearchFeedback
{
    public int Id { get; set; }
    public string Query { get; set; } = string.Empty;
    public bool IsHelpful { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}