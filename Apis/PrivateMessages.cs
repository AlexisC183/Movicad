using Microsoft.AspNetCore.Mvc;
using Movicad.Persistence;
using Movicad.Utils;
using System.Security.Cryptography;

namespace Movicad.Apis;

public static class PrivateMessages
{
    /// <summary>
    /// GET
    /// </summary>
    public static async Task ReadBy(
        HttpContext http,
        MovicadContext db,
        [FromQuery(Name = "cursor-pos")] long? cursorPosMaybe,
        [FromQuery(Name = "student-key")] string studentKey = "",
        [FromQuery(Name = "call-for-key")] string callForKey = ""
    )
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative, Roles.Student);

        if (idRolePair is null)
        {
            return;
        }

        const long LastPossiblePos = 1;
        const int PageSize = 20;
        string trimmedStudentKey = studentKey.Trim();
        string trimmedCallForKey = callForKey.Trim();

        try
        {
            User? student = db.Users.SingleOrDefault(user =>
                user.Key == trimmedStudentKey &&
                !user.Deleted
            );
            CallsFor? callFor = db.CallsFors.SingleOrDefault(callFor =>
                callFor.Key == trimmedCallForKey &&
                !callFor.Deleted
            );
            if (student is null || callFor is null)
            {
                http.Response.StatusCode = 404;
                await http.Response.WriteAsJsonAsync(new { Status = "not_found" });
                return;
            }

            Application? application = db.Applications
                .SingleOrDefault(appl =>
                    appl.StudentId == student.UserId &&
                    appl.CallForId == callFor.CallForId &&
                    !appl.Deleted
                );
            if (application is null)
            {
                http.Response.StatusCode = 404;
                await http.Response.WriteAsJsonAsync(new { Status = "not_found" });
                return;
            }

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

            if (!(
                requester.UserId == student.UserId ||
                requester.UserId == callFor.AdministrativeId
            ))
            {
                http.Response.StatusCode = 403;
                await http.Response.WriteAsJsonAsync(new { Status = "forbidden" });
                return;
            }

            var joins = db.Users
                .LeftJoin(
                    db.Administratives,
                    user => user.UserId,
                    admin => admin.UserId,
                    (user, admin) => new
                    {
                        user.UserId,
                        user.Role,
                        Administrative = admin == null
                        ?
                            null
                        :
                            new
                            {
                                admin.Acronym,
                                admin.Icon,
                                admin.IconMediaType
                            }
                    }
                )
                .LeftJoin(
                    db.Students,
                    user => user.UserId,
                    student => student.UserId,
                    (user, student) => new
                    {
                        user.UserId,
                        user.Role,
                        user.Administrative,
                        Student = student == null
                        ?
                            null
                        :
                            new
                            {
                                student.Name,
                                student.Icon,
                                student.IconMediaType
                            }
                    }
                )
                .Join(
                    db.PrivateMessages,
                    user => user.UserId,
                    msg => msg.UserId,
                    (user, msg) => new
                    {
                        user.Role,
                        msg.PrivateMessageId,
                        msg.Key,
                        msg.ApplicationId,
                        user.Administrative,
                        user.Student,
                        msg.Creation,
                        msg.Content,
                        msg.AttachedImage,
                        msg.ImageMediaType,
                        msg.Deleted
                    }
                )
                .Where(msg =>
                    !msg.Deleted &&
                    msg.ApplicationId == application.ApplicationId
                );

            long cursorPos;

            if (cursorPosMaybe.HasValue)
            {
                cursorPos = cursorPosMaybe.Value;
            }
            else
            {
                long? maxId = joins
                    .OrderByDescending(msg => msg.PrivateMessageId)
                    .Select(msg => (long?)msg.PrivateMessageId)
                    .FirstOrDefault();
                cursorPos = maxId.HasValue ? maxId.Value + 1 : LastPossiblePos;
            }

            var paginatedJoins = joins
                .Where(msg => msg.PrivateMessageId < cursorPos)
                .OrderByDescending(msg => msg.PrivateMessageId)
                .Take(PageSize)
                .ToList();

            long newCursorPos = paginatedJoins
                .Select(msg => (long?)msg.PrivateMessageId)
                .ElementAtOrDefault(PageSize - 1) ?? LastPossiblePos;

            var privateMessages = paginatedJoins
                .Select(msg => new
                {
                    msg.Key,
                    msg.Role,
                    msg.Administrative,
                    msg.Student,
                    msg.Creation,
                    msg.Content,
                    msg.AttachedImage,
                    msg.ImageMediaType
                })
                .ToArray();

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "ok",
                Data = new
                {
                    CursorPosition = newCursorPos,
                    PrivateMessages = privateMessages
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
        string Content,
        string? AttachedImageUri,
        string StudentKey,
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

        CreateReq req = new(
            r.Content ?? "",
            r.AttachedImageUri,
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
            User? student = db.Users.SingleOrDefault(user =>
                user.Key == trimmedStudentKey &&
                !user.Deleted
            );

            if (student is null)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "El estudiante no existe"
                });
                return;
            }

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

            Application? application = db.Applications
                .SingleOrDefault(appl =>
                    appl.StudentId == student.UserId &&
                    appl.CallForId == callFor.CallForId &&
                    !appl.Deleted
                );

            if (application is null)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "La postulación no existe"
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
                msgAuthor.UserId == student.UserId ||
                msgAuthor.UserId == callFor.AdministrativeId
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

            PrivateMessage message = new()
            {
                Key = RandomNumberGenerator.GetHexString(32),
                Content = trimmedContent,
                Creation = DateTime.UtcNow,
                AttachedImage = parsedImageUri.Content,
                ImageMediaType = parsedImageUri.MediaType,
                UserId = msgAuthor.UserId,
                ApplicationId = application.ApplicationId
            };

            db.PrivateMessages.Add(message);
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
