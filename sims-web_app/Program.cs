using Blazored.LocalStorage;
using MudBlazor.Services;
using sims_web_app.Components;
using sims_web_app.Services;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using sims_web_app.Components.Identity.Services;
using sims_web_app.Data.Model;

var builder = WebApplication.CreateBuilder(args);

// ----------------- Services -----------------

builder.Services.AddMudServices();

builder.Services.AddBlazoredSessionStorage();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<AuthService>(options =>
{
    options.BaseAddress = new Uri("http://localhost:5000/api/");
});


// ----------------- Authentication -----------------

builder.Services.AddAuthentication("Identity.Application")
    .AddCookie("Identity.Application");


builder.Services.AddAuthorizationCore();


builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();


builder.Services.AddScoped<AuthApiHandler>();
builder.Services.AddScoped<TokenProvider>();

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


app.UseRouting();


app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();