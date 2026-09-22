using Movicad.Persistence;
using Movicad.Utils;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Movicad.Apis;

public static class AdministrativeMessages
{
    public record CreateReq(
        string Key,
        string Subject,
        string Body,
        DataUriFile[] Files
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
            r.Key ?? "",
            r.Subject ?? "",
            r.Body ?? "",
            r.Files ?? new DataUriFile[0]
        );
        string trimmedKey = req.Key.Trim();
        string trimmedSubject = req.Subject.Trim();
        string trimmedBody = req.Body.Trim();

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
        if (trimmedBody.Length == 0)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "El cuerpo del mensaje es muy corto"
            });
            return;
        }

        Regex dataUriPattern = new(@"^data:.+/.+;base64,");

        List<DbFile> dbFiles = req.Files
            .OfType<DataUriFile>()
            .Select(duf =>
            {
                DataUriFile dataUriFile = new(duf.Name ?? "", duf.DataUri ?? "");

                if (!dataUriPattern.IsMatch(dataUriFile.DataUri))
                {
                    return new DbFile(dataUriFile.Name, null);
                }

                string[] metaAndData = dataUriFile.DataUri.Split(',');
                string mediaType = string.Concat(
                    metaAndData[0]
                        .Skip(5)
                        .TakeWhile(ch => ch != ';')
                );

                Converters.TryB64ToBytes(metaAndData[1], out byte[]? bytes);

                if (bytes is null)
                {
                    return new DbFile(dataUriFile.Name, null);
                }

                return new DbFile(dataUriFile.Name, new(mediaType, bytes));
            })
            .ToList();

        ICollection<AdministrativeMessageFile> wellFormedFiles = dbFiles
            .Where(dbFile => dbFile.Uri is not null)
            .Select(dbFile => new AdministrativeMessageFile()
            {
                Key = RandomNumberGenerator.GetHexString(32),
                Name = dbFile.Name,
                MediaType = dbFile.Uri!.MediaType,
                Content = dbFile.Uri.Content
            })
            .ToList();
        IEnumerable<DbFile> malformedFiles = dbFiles.Where(dbFile => dbFile.Uri is null);

        try
        {
            User? recipient = db.Users.SingleOrDefault(user => user.Key == trimmedKey);

            if (recipient is null)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "El destinatario no existe"
                });
                return;
            }

            User sender = db.Users.Single(user => user.Key == idRolePair.Id);

            AdministrativeMessage message = new()
            {
                Key = RandomNumberGenerator.GetHexString(32),
                Subject = trimmedSubject,
                Body = trimmedBody,
                Creation = DateTime.UtcNow,
                SenderId = sender.UserId,
                RecipientId = recipient.UserId,
                AdministrativeMessageFiles = wellFormedFiles
            };

            db.AdministrativeMessages.Add(message);
            await db.SaveChangesAsync();

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "ok",
                Data = new
                {
                    MalformedFiles = malformedFiles
                        .Select(dbFile => dbFile.Name)
                        .ToArray()
                }
            });
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }
}
