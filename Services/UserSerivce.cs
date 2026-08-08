using System.Security.Claims;

public class UserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
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

}
