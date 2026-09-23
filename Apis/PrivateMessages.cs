using Movicad.Persistence;
using Movicad.Utils;
using System.Security.Cryptography;

namespace Movicad.Apis;

public static class PrivateMessages
{
    public record CreateReq(
        string Content,
        DataUriFile AttachedImage,
        string StudentKey,
        string CallForKey
    );

    /// <summary>
    /// POST
    /// </summary>
    public static async Task Create(HttpContext http, MovicadContext db, CreateReq r)
    {
        // req: <callForKey, studentKey>, content, attached

        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative, Roles.Student);

        if (idRolePair is null)
        {
            return;
        }

        CreateReq req = new(
            r.Content ?? "",
            new(r.AttachedImage?.Name ?? "", r.AttachedImage?.DataUri ?? ""),
            r.StudentKey ?? "",
            r.CallForKey ?? ""
        );
        string trimmedContent = req.Content.Trim();
        string trimmedStudentKey = req.StudentKey.Trim();
        string trimmedCallForKey = req.CallForKey.Trim();

        if (trimmedContent.Length < 1 || trimmedContent.Length > 1000)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "El contenido del mensaje debe contener entre 1 y 1000 caracteres"
            });
            return;
        }

        // TODO: Regex with valid MIMEs
    }
}
