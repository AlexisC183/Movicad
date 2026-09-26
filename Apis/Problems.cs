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
        // email and subject are optional
        CreateReq req = new(r.Email, r.Subject, r.Message ?? "");
        string? trimmedEmail = req.Email?.Trim();
        string? trimmedSubject = req.Subject?.Trim();
        string trimmedMessage = req.Message.Trim();

        Regex emailPattern = new(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.IgnoreCase
        );

        if (trimmedEmail is not null && !emailPattern.IsMatch(trimmedEmail))
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Introducir un correo electrónico válido"
            });
            return;
        }

        // TODO
    }
}
