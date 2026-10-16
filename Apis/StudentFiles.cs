using Microsoft.AspNetCore.Mvc;
using Movicad.Persistence;
using Movicad.Utils;

namespace Movicad.Apis;

public static class StudentFiles
{
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
        const int PageSize = 3; // 20
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

            // var joins = db.FileRequests
            //     .Where(req =>
            //         req.CallForId == callFor.CallForId &&
            //         !req.Deleted
            //     )
            //     .LeftJoin(
            //         db.StudentFiles,
            //         req => req.FileRequestId,
            //         file => file.FileRequestId,
            //         (req, file) => new
            //         {
            //             req.FileRequestId,
            //             FileRequestKey = req.Key,
            //             req.Title,
            //             UploadedFile = file == null
            //             ?
            //                 null
            //             :
            //                 new
            //                 {
            //                     file.ApplicationId,
            //                     Key = file.ApplicationId + "," + file.FileRequestId,
            //                     file.Name,
            //                     Size = file.Content.Length,
            //                     file.Modification
            //                 }
            //         }
            //     )
            //     .Where(e =>
            //         e.UploadedFile == null ||
            //         e.UploadedFile.ApplicationId == application.ApplicationId
            //     );
            var joins = db.FileRequests
                .Where(req =>
                    req.CallForId == callFor.CallForId &&
                    !req.Deleted
                )
                .LeftJoin(
                    db.StudentFiles,
                    req => req.FileRequestId,
                    file => file.FileRequestId,
                    (req, file) => new { req, file }
                )
                .Where(e =>
                    e.file == null ||
                    e.file.ApplicationId == application.ApplicationId
                )
                .Select(e => new
                {
                    e.req.FileRequestId,
                    FileRequestKey = e.req.Key,
                    e.req.Title,
                    UploadedFile = e.file == null
                    ?
                        null
                    :
                        new
                        {
                            Key = e.file.ApplicationId + "," + e.file.FileRequestId,
                            e.file.Name,
                            Size = e.file.Content.Length,
                            e.file.Modification
                        }
                });

            long cursorPos;

            if (cursorPosMaybe.HasValue)
            {
                cursorPos = cursorPosMaybe.Value;
            }
            else
            {
                long? maxId = joins
                    .OrderByDescending(e => e.FileRequestId)
                    .Select(e => (long?)e.FileRequestId)
                    .FirstOrDefault();
                cursorPos = maxId.HasValue ? maxId.Value + 1 : LastPossiblePos;
            }

            var paginatedJoins = joins
                .Where(e => e.FileRequestId < cursorPos)
                .OrderByDescending(e => e.FileRequestId)
                .Take(PageSize)
                .ToList();

            long newCursorPos = paginatedJoins
                .Select(e => (long?)e.FileRequestId)
                .ElementAtOrDefault(PageSize - 1) ?? LastPossiblePos;

            var fileEntries = paginatedJoins
                .Select(e => new
                {
                    e.FileRequestKey,
                    e.Title,
                    e.UploadedFile
                })
                .ToArray();

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "ok",
                Data = new
                {
                    CursorPosition = newCursorPos,
                    FileEntries = fileEntries
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
        string FileRequestKey,
        string Name,
        string FileUri
    );

    /// <summary>
    /// POST
    /// </summary>
    public static async Task Create(HttpContext http, MovicadContext db, CreateReq r)
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Student);

        if (idRolePair is null)
        {
            return;
        }

        CreateReq req = new(
            r.FileRequestKey ?? "",
            r.Name ?? "",
            r.FileUri ?? ""
        );
        string trimmedFileRequestKey = req.FileRequestKey.Trim();

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
            FileRequest? fileRequest = db.FileRequests
                .SingleOrDefault(fileReq =>
                    fileReq.Key == trimmedFileRequestKey &&
                    !fileReq.Deleted
                );

            if (fileRequest is null)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "La solicitud de archivo (espacio para subir archivo) no existe"
                });
                return;
            }

            User? student = db.Users.SingleOrDefault(user =>
                user.Key == idRolePair.Id &&
                !user.Deleted
            );

            if (student is null)
            {
                http.Response.StatusCode = 401;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "Su cuenta ha sido eliminada. No se puede proseguir."
                });
                return;
            }

            Application? application = db.Applications
                .SingleOrDefault(appl =>
                    appl.StudentId == student.UserId &&
                    appl.CallForId == fileRequest.CallForId &&
                    !appl.Deleted
                );

            if (application is null)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "No es miembro"
                });
                return;
            }

            StudentFile? pastStudentFile = db.StudentFiles
                .SingleOrDefault(file =>
                    file.ApplicationId == application.ApplicationId &&
                    file.FileRequestId == fileRequest.FileRequestId
                );

            if (pastStudentFile is null)
            {
                StudentFile newFile = new()
                {
                    ApplicationId = application.ApplicationId,
                    FileRequestId = fileRequest.FileRequestId,
                    Name = req.Name,
                    MediaType = parsedFileUri.MediaType,
                    Content = parsedFileUri.Content,
                    Modification = DateTime.UtcNow
                };
                db.StudentFiles.Add(newFile);
            }
            else
            {
                pastStudentFile.Name = req.Name;
                pastStudentFile.MediaType = parsedFileUri.MediaType;
                pastStudentFile.Content = parsedFileUri.Content;
                pastStudentFile.Modification = DateTime.UtcNow;
                pastStudentFile.Deleted = false;

                db.StudentFiles.Update(pastStudentFile);
            }

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