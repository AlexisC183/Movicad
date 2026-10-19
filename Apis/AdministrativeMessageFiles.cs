using Microsoft.AspNetCore.Mvc;
using Movicad.Persistence;
using Movicad.Utils;

namespace Movicad.Apis;

public static class AdministrativeMessageFiles
{
    /// <summary>
    /// GET
    /// </summary>
    public static async Task Read(
        HttpContext http,
        MovicadContext db,
        string key = ""
    )
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative);

        if (idRolePair is null)
        {
            return;
        }

        string trimmedKey = key.Trim();

        try
        {
            User? requester = db.Users.SingleOrDefault(user =>
                user.Key == idRolePair.Id &&
                !user.Deleted
            );
            if (requester is null)
            {
                http.Response.StatusCode = 401;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "Su cuenta ha sido eliminada. No se puede proseguir."
                });
                return;
            }

            AdministrativeMessageFile? file = db.AdministrativeMessageFiles
                .SingleOrDefault(file => file.Key == trimmedKey);

            if (file is null)
            {
                http.Response.StatusCode = 404;
                await http.Response.WriteAsJsonAsync(new { Status = "not_found" });
                return;
            }

            AdministrativeMessage? msg = db.AdministrativeMessages
                .SingleOrDefault(msg =>
                    !msg.Deleted &&
                    msg.AdministrativeMessageId == file.AdministrativeMessageId
                );

            if (msg is null)
            {
                http.Response.StatusCode = 404;
                await http.Response.WriteAsJsonAsync(new { Status = "not_found" });
                return;
            }

            if (!(
                requester.UserId == msg.SenderId ||
                requester.UserId == msg.RecipientId
            ))
            {
                http.Response.StatusCode = 403;
                await http.Response.WriteAsJsonAsync(new { Status = "forbidden" });
                return;
            }

            http.Response.ContentType = file.MediaType;
            http.Response.Headers.Append(
                "Content-Disposition",
                $"attachment; filename={file.Name}"
            );

            await using Stream stream = new MemoryStream(file.Content);
            await stream.CopyToAsync(http.Response.Body);
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }
}
