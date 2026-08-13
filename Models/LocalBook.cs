using System.ComponentModel.DataAnnotations;

namespace BookRec.Models;

public class LocalBook
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string GoogleBookId { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string Author { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string? CoverImageUrl { get; set; }
    
    public string? Category { get; set; }
    
    public bool IsTrending { get; set; } 
    
    public DateTime AddedToDatabaseAt { get; set; } = DateTime.UtcNow;
}