namespace BookRec.Models;

public class Review
{
    public int Id { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public int StarRating { get; set; } 
    public string Comment { get; set; } = string.Empty;
    public DateTime DateSubmitted { get; set; } = DateTime.UtcNow;
    public int BookId { get; set; }
    public Book? Book { get; set; } = null;
}