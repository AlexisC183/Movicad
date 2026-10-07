using Microsoft.AspNetCore.Mvc;
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

    public record UpdateReq(
        string Key,
        string Title,
        string Name,
        string FileUri
    );

    /// <summary>
    /// PATCH
    /// </summary>
    public static async Task Update(HttpContext http, MovicadContext db, UpdateReq r)
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative);

        if (idRolePair is null)
        {
            return;
        }

        UpdateReq req = new(
            r.Key ?? "",
            r.Title ?? "",
            r.Name ?? "",
            r.FileUri ?? ""
        );
        string trimmedKey = req.Key.Trim();
        string trimmedTitle = req.Title.Trim();

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
            ForumFile? file = db.ForumFiles.SingleOrDefault(file =>
                file.Key == trimmedKey &&
                !file.Deleted
            );

            if (file is null)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "El archivo a modificar no existe"
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

            if (!db.CallsFors.Any(callFor =>
                callFor.CallForId == file.CallForId &&
                callFor.AdministrativeId == user.UserId &&
                !callFor.Deleted
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
            
            file.Title = trimmedTitle;
            file.Name = req.Name;
            file.MediaType = parsedFileUri.MediaType;
            file.Content = parsedFileUri.Content;
            file.Modification = DateTime.UtcNow;

            db.ForumFiles.Update(file);
            await db.SaveChangesAsync();

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new { Status = "ok" });
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }

    public record DeleteReq(string Key);

    /// <summary>
    /// DELETE
    /// </summary>
    public static async Task Delete(
        HttpContext http,
        MovicadContext db,
        [FromBody] DeleteReq r
    )
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative);

        if (idRolePair is null)
        {
            return;
        }

        string trimmedKey = r.Key?.Trim() ?? "";

        try
        {
            ForumFile? file = db.ForumFiles.SingleOrDefault(file =>
                file.Key == trimmedKey &&
                !file.Deleted
            );

            if (file is null)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "El archivo no existe"
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

            
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }
}
