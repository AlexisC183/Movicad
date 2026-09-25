using Microsoft.EntityFrameworkCore;
using Movicad.Persistence;
using Movicad.Utils;
using System.Security.Cryptography;

namespace Movicad.Apis;

public static class ForumFiles
{
    public record CreateReq(
        string Title,
        string Name,
        string FileUri,
        string CallForKey
    );

    /// <summary>
    /// POST
    /// </summary>
    public static async Task Create(HttpContext http, MovicadContext db, CreateReq r)
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative);

        if (idRolePair is null)
        {
            return;
        }

        CreateReq req = new(
            r.Title ?? "",
            r.Name ?? "",
            r.FileUri ?? "",
            r.CallForKey ?? ""
        );
        string trimmedTitle = req.Title.Trim();
        string trimmedCallForKey = req.CallForKey.Trim();

        if (trimmedTitle.Length < 1 || trimmedTitle.Length > 150)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "El título debe contener entre 1 y 150 caracteres"
            });
            return;
        }

        DataUri? parsedFileUri = await Validators.ValidateFile(
            http,
            req.FileUri,
            30 * StorageConstants.Megabyte
        );

        if (parsedFileUri is null)
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

            User? user = db.Users.SingleOrDefault(user =>
                user.Key == idRolePair.Id &&
                !user.Deleted
            );

            if (user is null)
            {
                http.Response.StatusCode = 401;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "Su cuenta ha sido eliminada. No se puede proseguir."
                });
                return;
            }

            if (user.UserId != callFor.AdministrativeId)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "No es miembro"
                });
                return;
            }

            int fileSizeSum = await db.ForumFiles
                .Where(file =>
                    file.CallForId == callFor.CallForId &&
                    !file.Deleted
                )
                .Select(file => (int?)file.Content.Length)
                .SumAsync() ?? 0;

            if (
                fileSizeSum + parsedFileUri.Content.Length >
                100 * StorageConstants.Megabyte
            )
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "El foro tiene un límite de 100 MB en archivos subidos. Intente subir un archivo más pequeño o elimine otros."
                });
                return;
            }

            ForumFile file = new()
            {
                Key = RandomNumberGenerator.GetHexString(32),
                Title = trimmedTitle,
                Name = req.Name,
                MediaType = parsedFileUri.MediaType,
                Content = parsedFileUri.Content,
                Modification = DateTime.UtcNow,
                CallForId = callFor.CallForId
            };

            db.ForumFiles.Add(file);
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
