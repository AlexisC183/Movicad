using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Movicad.Apis;
using Movicad.Persistence;
using Movicad.Utils;

int maxReqBodySize = 250 * StorageConstants.Megabyte;
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000");

builder.Services.Configure<KestrelServerOptions>(opts =>
{
    opts.Limits.MaxRequestBodySize = maxReqBodySize;
});

builder.Services.Configure<FormOptions>(opts =>
{
    opts.MultipartBodyLengthLimit = maxReqBodySize;
    opts.ValueLengthLimit = maxReqBodySize;
});

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

app.MapPost("/test", () =>
{


    return "{}";
}); //

app.MapPost("/api/administrative-messages/create", AdministrativeMessages.Create);

app.MapPost("/api/applications/create", Applications.Create);

app.MapPost("/api/calls-for/create", CallForDelegates.Create);

app.MapPost("/api/expulsion-reasons/create", ExpulsionReasons.Create);

app.MapPost("/api/file-requests/create", FileRequests.Create);

app.MapPost("/api/forum-files/create", ForumFiles.Create);

app.MapPost("/api/forum-messages/create", ForumMessages.Create);

app.MapPost("/api/frequent-questions/create", FrequentQuestions.Create);

app.MapPost("/api/links/create", Links.Create);

app.MapPost("/api/private-messages/create", PrivateMessages.Create);

app.MapPost("/api/problems/create", Problems.Create);

app.MapPost("/api/ratings/create", Ratings.Create);

app.MapPost("/api/sessions/log-in", Sessions.LogIn);
app.MapGet("/api/sessions/log-out", Sessions.LogOut);
app.MapGet("/api/sessions/verify-session", Sessions.VerifySession);

app.MapPost("/api/student-files/create", StudentFiles.Create);

app.MapPost("/api/users/create", Users.Create);

app.MapFallbackToFile("index.html");

app.Run();
