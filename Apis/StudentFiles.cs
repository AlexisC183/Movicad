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

            // keys of reqs <- to render the UI
            // keys of files <- 'read/{key}' endpoint
            var joins = db.StudentFiles
                .Where(file =>
                    file.ApplicationId == application.ApplicationId &&
                    !file.Deleted
                )
                .RightJoin(
                    db.FileRequests,
                    file => file.FileRequestId,
                    req => req.FileRequestId,
                    (file, req) => new
                    {
                        
                    }
                )
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