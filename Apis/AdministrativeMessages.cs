using Microsoft.AspNetCore.Mvc;
using Movicad.Persistence;
using Movicad.Utils;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Movicad.Apis;

public static class AdministrativeMessages
{
    /// <summary>
    /// GET
    /// </summary>
    public static async Task ReadAs(
        HttpContext http,
        MovicadContext db,
        string entityRole,
        [FromQuery(Name = "cursor-pos")] long? cursorPosMaybe
    )
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative);

        if (idRolePair is null)
        {
            return;
        }

        if (entityRole is not ("sender" or "recipient"))
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Quiso decir 'sender' o 'recipient'"
            });
            return;
        }

        const long LastPossiblePos = 1;
        const int PageSize = 20;

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

            var filteredMsgs = entityRole == "sender"
            ?
                db.AdministrativeMessages
                    .Where(msg =>
                        msg.SenderId == requester.UserId &&
                        !msg.Deleted
                    )
            :
                db.AdministrativeMessages
                    .Where(msg =>
                        msg.RecipientId == requester.UserId &&
                        !msg.Deleted
                    );
            var msgAdminJoins = entityRole == "sender"
            ?
                filteredMsgs
                    .Join(
                        db.Administratives,
                        msg => msg.RecipientId,
                        admin => admin.UserId,
                        (msg, admin) => new { msg, admin }
                    )
            :
                filteredMsgs
                    .Join(
                        db.Administratives,
                        msg => msg.SenderId,
                        admin => admin.UserId,
                        (msg, admin) => new { msg, admin }
                    );
            var joins = msgAdminJoins
                .Join(
                    db.Users,
                    msgAdmin => msgAdmin.admin.UserId,
                    user => user.UserId,
                    (msgAdmin, user) => new { msgAdmin, user }
                )
                .GroupJoin(
                    db.AdministrativeMessageFiles,
                    msgAdminUser => msgAdminUser.msgAdmin.msg.AdministrativeMessageId,
                    file => file.AdministrativeMessageId,
                    (msgAdminUser, files) => new { msgAdminUser, files }
                );

            long cursorPos;

            if (cursorPosMaybe.HasValue)
            {
                cursorPos = cursorPosMaybe.Value;
            }
            else
            {
                long? maxId = joins
                    .OrderByDescending(jn => jn.msgAdminUser.msgAdmin.msg.AdministrativeMessageId)
                    .Select(jn => (long?)jn.msgAdminUser.msgAdmin.msg.AdministrativeMessageId)
                    .FirstOrDefault();
                cursorPos = maxId.HasValue ? maxId.Value + 1 : LastPossiblePos;
            }

            var paginatedJoins = joins
                .Where(jn => jn.msgAdminUser.msgAdmin.msg.AdministrativeMessageId < cursorPos)
                .OrderByDescending(jn => jn.msgAdminUser.msgAdmin.msg.AdministrativeMessageId)
                .Take(PageSize)
                .ToList();

            long newCursorPos = paginatedJoins
                .Select(jn => (long?)jn.msgAdminUser.msgAdmin.msg.AdministrativeMessageId)
                .ElementAtOrDefault(PageSize - 1) ?? LastPossiblePos;

            var messages = paginatedJoins
                .Select(jn => new
                {
                    MessageKey = jn.msgAdminUser.msgAdmin.msg.Key,
                    AdministrativeKey = jn.msgAdminUser.user.Key,
                    jn.msgAdminUser.msgAdmin.admin.Icon,
                    jn.msgAdminUser.msgAdmin.admin.IconMediaType,
                    jn.msgAdminUser.msgAdmin.admin.Acronym,
                    jn.msgAdminUser.msgAdmin.msg.Creation,
                    jn.msgAdminUser.msgAdmin.msg.Subject,
                    jn.msgAdminUser.msgAdmin.msg.Body,
                    AttachedFiles = jn.files
                        .Select(file => new
                        {
                            file.Key,
                            file.Name
                        })
                        .ToArray()
                })
                .ToArray();

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "ok",
                Data = new
                {
                    CursorPosition = newCursorPos,
                    Messages = messages
                },
                IdRolePair = idRolePair
            });
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }

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

                if (bytes is null || bytes.Length > 30 * StorageConstants.Megabyte)
                {
                    return new DbFile(dataUriFile.Name, null);
                }

                return new DbFile(dataUriFile.Name, new(mediaType, bytes));
            })
            .ToList();

        ICollection<AdministrativeMessageFile> wellFormedFiles = dbFiles
            .Where(dbFile => dbFile.DataUri is not null)
            .Select(dbFile => new AdministrativeMessageFile()
            {
                Key = RandomNumberGenerator.GetHexString(32),
                Name = dbFile.Name,
                MediaType = dbFile.DataUri!.MediaType,
                Content = dbFile.DataUri.Content
            })
            .ToList();
        IEnumerable<DbFile> malformedFiles = dbFiles.Where(dbFile => dbFile.DataUri is null);

        try
        {
            User? recipient = db.Users.SingleOrDefault(user =>
                user.Key == trimmedKey &&
                user.Role == Roles.Administrative &&
                !user.Deleted
            );

            if (recipient is null)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "El administrativo destinatario no existe"
                });
                return;
            }

            User? sender = db.Users.SingleOrDefault(user =>
                user.Key == idRolePair.Id &&
                !user.Deleted
            );

            if (sender is null)
            {
                http.Response.StatusCode = 401;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "Su cuenta ha sido eliminada. No se puede proseguir."
                });
                return;
            }

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
                    RejectedFiles = malformedFiles
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
