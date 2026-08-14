using BookRec.Components;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BookRec.Data;
using BookRec.Models;
using BookRec.Services;
using Microsoft.AspNetCore.Authentication;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient();
builder.Services.AddHttpClient<GoogleBooksApiService>();
builder.Services.AddScoped<BookService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserService>();

var githubClientId = Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID") 
    ?? throw new InvalidOperationException("GITHUB_CLIENT_ID is missing from .env");

var githubClientSecret = Environment.GetEnvironmentVariable("GITHUB_CLIENT_SECRET") 
    ?? throw new InvalidOperationException("GITHUB_CLIENT_SECRET is missing from .env");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Cookies";
    options.DefaultSignInScheme = "Cookies";
    options.DefaultChallengeScheme = "GitHub";
})
.AddCookie("Cookies")
.AddGitHub("GitHub", options =>
{
    options.ClientId = githubClientId;
    options.ClientSecret = githubClientSecret;
    options.Scope.Add("user:email");

    
    options.Events.OnCreatingTicket = async context =>
    {
        var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

        var githubId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = context.Principal?.FindFirst(ClaimTypes.Name)?.Value 
                       ?? context.Principal?.FindFirst("urn:github:login")?.Value 
                       ?? "GitHub User";
        var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value;
        var avatarUrl = context.User.GetProperty("avatar_url").GetString();

        if (!string.IsNullOrEmpty(githubId))
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == githubId);

            if (user == null)
            {
                user = new User
                {
                    Id = githubId,
                    Username = username,
                    Email = email,
                    AvatarUrl = avatarUrl,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };
                db.Users.Add(user);
            }
            else
            {
                user.Username = username;
                user.Email = email ?? user.Email;
                user.AvatarUrl = avatarUrl ?? user.AvatarUrl;
                user.LastLoginAt = DateTime.UtcNow;
                db.Users.Update(user);
            }

            await db.SaveChangesAsync();
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var dbPath = builder.Environment.IsDevelopment()
    ? "bookrec.db"
    : Path.Combine("/home", "bookrec.db");

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<AppDbContext>(p => 
    p.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/signin", async (HttpContext context) =>
{
    await context.ChallengeAsync("GitHub",
    new AuthenticationProperties
    {
        RedirectUri = "/"
    });
});

app.MapGet("/signout", async (HttpContext context) => 
{
    await context.SignOutAsync("Cookies");
    context.Response.Redirect("/");
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();