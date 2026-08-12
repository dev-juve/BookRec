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

        _httpClient.Timeout = TimeSpan.FromSeconds(8);

        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BookRecApp/1.0");
        }
    }

    public async Task<List<Book>> SearchBooksAsync(int page, string categoryQuery = "subject:computers")
    {
        int pageSize = 8;
        int startIndex = (page - 1) * pageSize;

        var url = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(categoryQuery)}&orderBy=relevance&startIndex={startIndex}&maxResults={pageSize}&key={_apiKey}";

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
            Console.WriteLine($"[Service] Returning {result.Items.Count} live books from API!"); // test api
            return result.Items.Select((item, index) => new Book
            {
                Id = index + 100,
                GoogleBookId = item.Id ?? string.Empty,
                Title = item.VolumeInfo?.Title ?? "Untitled",
                Author = item.VolumeInfo?.Authors != null ? string.Join(", ", item.VolumeInfo.Authors) : "Unknown Author",
                Publisher = item.VolumeInfo?.Publisher ?? "Independent Publisher",
                Description = item.VolumeInfo?.Description ?? "No description available for this volume.",
                CoverImageUrl = item.VolumeInfo?.ImageLinks?.Thumbnail?
                .Replace("http://", "https://")
                .Replace("&edge=curl", "")
                .Replace("zoom=1", "zoom=2")
                ?? "https://via.placeholder.com/150x220?text=No+Cover",
                IsBestseller = index % 3 == 0
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
            new Book { Id = 1, Title = "Atomic Habits", Author = "James Clear", Category = BookCategory.SelfDevelopment, Publisher = "Penguin", IsBestseller = true, Description = "An easy & proven way to build good habits & break bad ones." },
            new Book { Id = 2, Title = "Clean Code", Author = "Robert C. Martin", Category = BookCategory.Programming, Publisher = "Prentice Hall", IsBestseller = true, Description = "A handbook of agile software craftsmanship." }
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
}

public class ImageLinks
{
    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }
}