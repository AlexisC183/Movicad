using Microsoft.AspNetCore.Authentication.Cookies;
using Movicad.Apis;
using Movicad.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opts =>
    {
        opts.ExpireTimeSpan = TimeSpan.FromDays(1);
        opts.SlidingExpiration = false;
    });

builder.Services.AddDbContext<MovicadContext>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api", () =>
{
    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "index.html");

    return Results.File(filePath, "text/html");
}); //

app.MapPost("/api/calls-for/create", CallForDelegates.Create);

app.MapPost("/api/sessions/log-in", Sessions.LogIn);
app.MapGet("/api/sessions/log-out", Sessions.LogOut);
app.MapGet("/api/sessions/verify-session", Sessions.VerifySession);

app.MapPost("/api/users/create", Users.Create);

app.MapFallbackToFile("index.html");

app.Run();
