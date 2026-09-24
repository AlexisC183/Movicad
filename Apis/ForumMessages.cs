using Movicad.Persistence;
using Movicad.Utils;
using System.Security.Cryptography;

namespace Movicad.Apis;

public static class ForumMessages
{
    public record CreateReq(
        string Content,
        string? AttachedImageUri,
        string CallForKey
    );

    /// <summary>
    /// POST
    /// </summary>
    public static async Task Create(HttpContext http, MovicadContext db, CreateReq r)
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative, Roles.Student);

        if (idRolePair is null)
        {
            return;
        }

        CreateReq req = new(r.Content ?? "", r.AttachedImageUri, r.CallForKey ?? "");
        string trimmedContent = req.Content.Trim();
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

        DataUri? parsedImageUri;

        if (req.AttachedImageUri is null)
        {
            parsedImageUri = new("image/gif", new byte[0]);
        }
        else
        {
            parsedImageUri = await Validators.ValidateFile(
                http,
                req.AttachedImageUri,
                10 * StorageConstants.Megabyte,
                @"image/(gif|jpeg|png|webp|svg\+xml)"
            );
        }

        if (parsedImageUri is null)
        {
            return;
        }

        try
        {
            CallsFor? callFor = db.CallsFors.SingleOrDefault(callFor =>
                callFor.Key == trimmedCallForKey &&
                !callFor.Deleted
            );

            if (callFor is null)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "La convocatoria no existe"
                });
                return;
            }

            User? msgAuthor = db.Users.SingleOrDefault(user =>
                user.Key == idRolePair.Id &&
                !user.Deleted
            );

            if (msgAuthor is null)
            {
                http.Response.StatusCode = 401;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "Su cuenta ha sido eliminada. No se puede proseguir."
                });
                return;
            }

            if (!(
                msgAuthor.UserId == callFor.AdministrativeId ||
                db.Applications
                    .Where(appl =>
                        appl.CallForId == callFor.CallForId &&
                        !appl.Deleted
                    )
                    .Any(appl => appl.StudentId == msgAuthor.UserId)
            ))
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "No es miembro"
                });
                return;
            }

            ForumMessage message = new()
            {
                Key = RandomNumberGenerator.GetHexString(32),
                Content = trimmedContent,
                Creation = DateTime.UtcNow,
                AttachedImage = parsedImageUri.Content,
                ImageMediaType = parsedImageUri.MediaType,
                UserId = msgAuthor.UserId,
                CallForId = callFor.CallForId
            };

            db.ForumMessages.Add(message);
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
