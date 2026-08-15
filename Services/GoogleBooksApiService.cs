using System.Text.Json;
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

        // get the api key from env file, crash if we forgot to add it
        _apiKey = Environment.GetEnvironmentVariable("GOOGLE_BOOKS_API_KEY")
            ?? throw new InvalidOperationException("GOOGLE_BOOKS_API_KEY is missing from environment/env file.");

        _httpClient.Timeout = TimeSpan.FromSeconds(15);

        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BookRecApp/1.0");
        }
    }

    // fetch a single book using the google api
    public async Task<Book?> GetBookByIdAsync(string bookId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"https://www.googleapis.com/books/v1/volumes/{bookId}?key={_apiKey}");
            
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.TryGetProperty("volumeInfo", out var volumeInfo))
            {
                return new Book
                {
                    Id = 0,
                    GoogleBookId = root.GetProperty("id").GetString() ?? bookId,
                    Title = volumeInfo.TryGetProperty("title", out var title) ? title.GetString() ?? "Unknown Title" : "Unknown Title",
                    Author = volumeInfo.TryGetProperty("authors", out var authors) && authors.GetArrayLength() > 0 ? authors[0].GetString() ?? "Unknown Author" : "Unknown Author",
                    Description = volumeInfo.TryGetProperty("description", out var desc) ? desc.GetString() ?? "No description available." : "No description available.",
                    // use default cover if google doesn't give us one
                    CoverImageUrl = volumeInfo.TryGetProperty("imageLinks", out var images) && images.TryGetProperty("thumbnail", out var thumbnail) ? thumbnail.GetString()?.Replace("http:", "https:") ?? "/images/default-cover.svg" : "/images/default-cover.svg"
                };
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    // searches google books, returns 12 items at a time
    public async Task<List<Book>> SearchBooksAsync(string categoryQuery = "a", int pageNumber = 1)
    {
        int maxResults = 12;
        int startIndex = (pageNumber - 1) * maxResults;

        var url = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(categoryQuery)}&orderBy=relevance&startIndex={startIndex}&maxResults={maxResults}&key={_apiKey}";

        // try 3 times just in case the wifi drops or something
        int maxRetries = 3;
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<GoogleBooksResponse>();

                    if (result?.Items != null && result.Items.Any())
                    {
                        return result.Items.Select((item, index) =>
                        {
                            var googleCategory = item.VolumeInfo?.Categories?.FirstOrDefault();
                            BookCategory mappedCategory = BookCategory.SelfDevelopment;

                            if (!string.IsNullOrEmpty(googleCategory))
                            {
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
                                CoverImageUrl = item.VolumeInfo?.ImageLinks?.Thumbnail?.Replace("http://", "https://") ?? "/images/default-cover.svg",
                                DisplayCategory = googleCategory ?? mappedCategory.ToString(),
                                IsBestseller = index % 3 == 0
                            };
                        }).ToList();
                    }
                }
            }
            catch
            {
                // ignore errors here so it can try again
            }

            if (attempt < maxRetries)
            {
                await Task.Delay(1000 * attempt);
            }
        }

        // return an empty list if it totally failed
        return new List<Book>();
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