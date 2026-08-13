using System.Text.Json.Serialization;
using BookRec.Models;

namespace BookRec.Services;

public class GoogleBooksApiService
{
    private readonly HttpClient _httpClient;

    private readonly string _apiKey;

    public GoogleBooksApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        _apiKey = Environment.GetEnvironmentVariable("GOOGLE_BOOKS_API_KEY")
            ?? throw new InvalidOperationException("GOOGLE_BOOKS_API_KEY is missing from environment/env file.");

        _httpClient.Timeout = TimeSpan.FromSeconds(15);

        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BookRecApp/1.0");
        }
    }

    public async Task<List<Book>> SearchBooksAsync(string categoryQuery = "a", int pageNumber = 1)
    {
        int maxResults = 12;
        int startIndex = (pageNumber - 1) * maxResults;

        var url = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(categoryQuery)}&orderBy=relevance&startIndex={startIndex}&maxResults={maxResults}&key={_apiKey}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return GetFallbackBooks();
            }

            var result = await response.Content.ReadFromJsonAsync<GoogleBooksResponse>();

            if (result?.Items == null || !result.Items.Any())
                return GetFallbackBooks();

            return result.Items.Select((item, index) =>
            {
                // try to get the first category from Google Books
                var googleCategory = item.VolumeInfo?.Categories?.FirstOrDefault();

                BookCategory mappedCategory = BookCategory.SelfDevelopment;

                if (!string.IsNullOrEmpty(googleCategory))
                {
                    // remove spaces
                    Enum.TryParse<BookCategory>(googleCategory.Replace(" ", ""), true, out mappedCategory);
                }

                return new Book
                {
                    Id = index + 100,
                    GoogleBookId = item.Id ?? string.Empty,
                    Title = item.VolumeInfo?.Title ?? "Untitled",
                    Author = item.VolumeInfo?.Authors != null ? string.Join(", ", item.VolumeInfo.Authors) : "Unknown Author",
                    Publisher = item.VolumeInfo?.Publisher ?? "Independent Publisher",
                    Description = item.VolumeInfo?.Description ?? "No description available for this volume.",
                    CoverImageUrl = item.VolumeInfo?.ImageLinks?.Thumbnail?.Replace("http://", "https://") ?? "https://via.placeholder.com/150x220?text=No+Cover",

                    DisplayCategory = googleCategory ?? mappedCategory.ToString(),

                    IsBestseller = index % 3 == 0
                };
            }).ToList();
        }
        catch
        {
            return GetFallbackBooks();
        }
    }

    private List<Book> GetFallbackBooks()
    {
        return new List<Book>
        {
            new Book 
            { 
                Id = 1, 
                Title = "Atomic Habits", 
                Author = "James Clear", 
                Category = BookCategory.SelfDevelopment, 
                Publisher = "Penguin", 
                IsBestseller = true, 
                Description = "An easy & proven way to build good habits & break bad ones.",
                CoverImageUrl = "/images/fallback/atomic-habits.jpg"
            },
            new Book 
            { 
                Id = 2, 
                Title = "Clean Code", 
                Author = "Robert C. Martin", 
                Category = BookCategory.Programming, 
                Publisher = "Prentice Hall", 
                IsBestseller = true, 
                Description = "A handbook of agile software craftsmanship.",
                CoverImageUrl = "/images/fallback/clean-code.jpg" 
            }
        };
    }
}

public class GoogleBooksResponse
{
    [JsonPropertyName("items")]
    public List<GoogleBookItem>? Items { get; set; }
}

public class GoogleBookItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("volumeInfo")]
    public VolumeInfo? VolumeInfo { get; set; }

}

public class VolumeInfo
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("authors")]
    public List<string>? Authors { get; set; }

    [JsonPropertyName("publisher")]
    public string? Publisher { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("imageLinks")]
    public ImageLinks? ImageLinks { get; set; }

    [JsonPropertyName("categories")]
    public List<string>? Categories { get; set; }
}

public class ImageLinks
{
    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }
}