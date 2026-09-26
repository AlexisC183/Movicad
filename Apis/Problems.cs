using Movicad.Persistence;
using Movicad.Utils;
using System.Text.RegularExpressions;

namespace Movicad.Apis;

public static class Problems
{
    public record CreateReq(
        string? Email,
        string? Subject,
        string Message
    );

    /// <summary>
    /// POST
    /// </summary>
    public static async Task Create(HttpContext http, MovicadContext db, CreateReq r)
    {
        string trimmedEmail = r.Email?.Trim() ?? "";
        string trimmedSubject = r.Subject?.Trim() ?? "";
        string trimmedMessage = r.Message?.Trim() ?? "";

        if (trimmedEmail.Length > 200)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "El correo electrónico es muy largo"
            });
            return;
        }

        Regex emailPattern = new(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.IgnoreCase
        );

        if (trimmedEmail.Length > 0 && !emailPattern.IsMatch(trimmedEmail))
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Introducir un correo electrónico válido"
            });
            return;
        }
        if (trimmedSubject.Length > 150)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "El asunto es muy largo"
            });
            return;
        }
        if (trimmedMessage.Length == 0)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "El mensaje es muy corto"
            });
            return;
        }

        try
        {
            Problem problem = new()
            {
                AuthorEmail = trimmedEmail,
                Subject = trimmedSubject,
                Body = trimmedMessage,
                Creation = DateTime.UtcNow
            };

            db.Problems.Add(problem);
            await db.SaveChangesAsync();

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new { Status = "ok" });
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }
}
