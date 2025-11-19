using MudBlazor.Services;
using sims_web_app.Components;
using sims_web_app.Services;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using sims_web_app.Components.Identity.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

//Add Blazored LocalStorage
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddBlazoredLocalStorage();
//Add AuthService
builder.Services.AddHttpClient<AuthService>(options =>
{
    options.BaseAddress = new Uri("http://localhost:5000/api/");
});
builder.Services.AddAuthentication("Identity.Application") // beliebiger Scheme-Name
    .AddCookie("Identity.Application"); // Cookie-Auth wird benötigt, sonst meckert IAuthenticationService

builder.Services.AddAuthorizationCore(); // für [Authorize] in Blazor
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();