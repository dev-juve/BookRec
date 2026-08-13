using System.ComponentModel.DataAnnotations;

namespace BookRec.Models;

public class Review
{
    public int Id { get; set; }
    
    // foreign key to the User table
    [Required]
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; } 

    [Required]
    public string BookId { get; set; } = string.Empty;
    
    [Range(1, 5)]
    public int StarRating { get; set; } 
    
    [MaxLength(500)]
    public string Comment { get; set; } = string.Empty;
    
    public DateTime DateSubmitted { get; set; } = DateTime.UtcNow;
}