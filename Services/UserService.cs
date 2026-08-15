using System.Security.Claims;
using BookRec.Data;
using BookRec.Models;

public class UserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _context;

    public UserService(IHttpContextAccessor httpContextAccessor, AppDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    // checks if user is in db, if not it makes a new one
    public async Task GetUserAsync()
    {
        if (Id == null)
            return;

        var user = await _context.Users.FindAsync(Id);

        if (user == null)
        {
            user = new User
            {
                Id = Id,
                Username = UserName,
                Name = Name,
                Email = Email,
                AvatarUrl = AvatarUrl,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
        }
        else
        {
            user.Username = UserName;
            user.Name = Name;
            user.Email = Email;
            user.AvatarUrl = AvatarUrl;
            user.LastLoginAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    // helper properties to grab data from the cookies
    public ClaimsPrincipal? User => 
        _httpContextAccessor.HttpContext?.User;

    public string? UserName => User?.Identity?.Name;

    public string? Email => 
        User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

    public string? Id => 
        User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

    public string? Name => 
        User?.FindFirst("urn:github:name")?.Value;

    // gets the github profile pic or falls back to our local gray head image
    public string? AvatarUrl
    {
        get
        {
            var url = User?.FindFirst("urn:github:avatar_url")?.Value ?? User?.FindFirst("urn:github:avatar")?.Value;
            return !string.IsNullOrEmpty(url) ? url : "/images/default-avatar.svg";
        }
    }
}