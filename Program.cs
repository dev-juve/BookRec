using BookRec.Components;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BookRec.Data;
using BookRec.Models;
using Microsoft.AspNetCore.Authentication;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<BookRec.Services.GoogleBooksApiService>();
builder.Services.AddSingleton<BookRec.Services.BookService>();

// retrieve github OAuth credentials from .env
var githubClientId = Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID") 
    ?? throw new InvalidOperationException("GITHUB_CLIENT_ID is missing from .env");

var githubClientSecret = Environment.GetEnvironmentVariable("GITHUB_CLIENT_SECRET") 
    ?? throw new InvalidOperationException("GITHUB_CLIENT_SECRET is missing from .env");

// Add authentication
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

// register DbContext with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=bookrec.db"));

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// for authentication
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
