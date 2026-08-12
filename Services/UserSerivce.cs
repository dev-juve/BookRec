using System.Security.Claims;
using BookRec.Data;
using BookRec.Models;

public class UserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly BookRec.Data.AppDbContext _context;

    public UserService(IHttpContextAccessor httpContextAccessor, BookRec.Data.AppDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

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
    public ClaimsPrincipal? User => 
        _httpContextAccessor.HttpContext?.User;

    public string? UserName => User?.Identity?.Name;

    public string? Email => 
        User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

    public string? Id => 
        User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

    public string? Name => 
        User?.FindFirst("urn:github:name")?.Value;

    public string? AvatarUrl => User?.FindFirst("urn:github:avatar")?.Value;

}
