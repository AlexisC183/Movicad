using Movicad.Persistence;
using Movicad.Utils;

namespace Movicad.Apis;

public static class StudentFiles
{
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