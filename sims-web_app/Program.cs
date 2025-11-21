using MudBlazor.Services;
using sims_web_app.Components;
using sims_web_app.Services;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using sims_web_app.Data.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMudServices();

builder.Services.AddBlazoredSessionStorage();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

<<<<<<< HEAD
builder.Services.AddHttpClient<AuthService>(options =>
{
    options.BaseAddress = new Uri("http://user-api:8080/api/");
});
=======
//Authentication
builder.Services.AddAuthorization();
>>>>>>> WebApp-JWT_implementation

//The cookie authentication is never used, but it is required to prevent a runtime error
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "auth_cookie";
        options.Cookie.MaxAge = TimeSpan.FromHours(24);
        options.LoginPath = "/login";
    });

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddBlazoredSessionStorage();
builder.Services.AddScoped<ICustomSessionService, CustomSessionService>();


builder.Services.AddScoped<AuthApiHandler>();
builder.Services.AddScoped<BackendApiHandler>();


// ----------------- Build -----------------
var app = builder.Build();

// ----------------- Middleware -----------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();