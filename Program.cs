using BookRec.Components;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<BookRec.Services.BookService>();

// Add authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Cookies";
    options.DefaultSignInScheme = "Cookies";
    options.DefaultChallengeScheme = "GitHub";
})
.AddCookie("Cookies")
.AddGitHub(options =>
 {
    options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"]!;
    options.Scope.Add("user:email");
 });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

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
