namespace BookRec.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public BookCategory Category { get; set; }
    public string Publisher { get; set; } = string.Empty;
    public bool IsBestseller { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = "https://via.placeholder.com/150x220?text=No+Cover";
    public string GoogleBookId { get; set; } = string.Empty;
    
    public List<Review> Reviews { get; set; } = new();

    public string? DisplayCategory { get; set; }
    
    public double AverageRating => Reviews.Any() ? Reviews.Average(r => r.StarRating) : 0.0;
}