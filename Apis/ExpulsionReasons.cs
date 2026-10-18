using Microsoft.AspNetCore.Mvc;
using Movicad.Persistence;
using Movicad.Utils;
using System.Security.Cryptography;

namespace Movicad.Apis;

public static class ExpulsionReasons
{
    /// <summary>
    /// GET
    /// </summary>
    public static async Task ReadByStudent(
        HttpContext http,
        MovicadContext db,
        [FromQuery(Name = "cursor-pos")] long? cursorPosMaybe
    )
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Student);

        if (idRolePair is null)
        {
            return;
        }

        const long LastPossiblePos = 1;
        const int PageSize = 20;

        try
        {
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

            var expulReasons = db.ExpulsionReasons
                .Where(reason => !reason.Deleted);
            var studentApplications = db.Applications
                .Where(appl => appl.StudentId == student.UserId);
            var joins = expulReasons
                .Join(
                    studentApplications,
                    reason => reason.ApplicationId,
                    appl => appl.ApplicationId,
                    (reason, appl) => new
                    {
                        reason.ExpulsionReasonId,
                        reason.Key,
                        reason.Creation,
                        reason.Content,
                        appl.CallForId
                    }
                )
                .Join(
                    db.CallsFors,
                    reason => reason.CallForId,
                    callFor => callFor.CallForId,
                    (reason, callFor) => new
                    {
                        reason.ExpulsionReasonId,
                        reason.Key,
                        reason.Creation,
                        reason.Content,
                        callFor.Title,
                        callFor.AdministrativeId
                    }
                )
                .Join(
                    db.Administratives,
                    reason => reason.AdministrativeId,
                    admin => admin.UserId,
                    (reason, admin) => new
                    {
                        reason.ExpulsionReasonId,
                        reason.Key,
                        reason.Creation,
                        reason.Content,
                        reason.Title,
                        admin.Acronym,
                        admin.Icon,
                        admin.IconMediaType
                    }
                );

            long cursorPos;

            if (cursorPosMaybe.HasValue)
            {
                cursorPos = cursorPosMaybe.Value;
            }
            else
            {
                long? maxId = joins
                    .OrderByDescending(reason => reason.ExpulsionReasonId)
                    .Select(reason => (long?)reason.ExpulsionReasonId)
                    .FirstOrDefault();
                cursorPos = maxId.HasValue ? maxId.Value + 1 : LastPossiblePos;
            }

            var paginatedJoins = joins
                .Where(reason => reason.ExpulsionReasonId < cursorPos)
                .OrderByDescending(reason => reason.ExpulsionReasonId)
                .Take(PageSize)
                .ToList();

            long newCursorPos = paginatedJoins
                .Select(reason => (long?)reason.ExpulsionReasonId)
                .ElementAtOrDefault(PageSize - 1) ?? LastPossiblePos;

            var directMessages = paginatedJoins
                .Select(reason => new
                {
                    reason.Key,
                    reason.Creation,
                    reason.Content,
                    reason.Title,
                    reason.Acronym,
                    reason.Icon,
                    reason.IconMediaType
                })
                .ToArray();

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "ok",
                Data = new
                {
                    CursorPosition = newCursorPos,
                    DirectMessages = directMessages
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
        string StudentKey,
        string CallForKey,
        string Reason
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

        string trimmedStudentKey = r.StudentKey?.Trim() ?? "";
        string trimmedCallForKey = r.CallForKey?.Trim() ?? "";
        string trimmedReason = r.Reason?.Trim() ?? "";

        if (trimmedReason.Length > 300)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "El motivo es muy largo"
            });
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

            User? currentAdministrative = db.Users.SingleOrDefault(user =>
                user.Key == idRolePair.Id &&
                !user.Deleted
            );

            if (currentAdministrative is null)
            {
                http.Response.StatusCode = 401;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "Su cuenta ha sido eliminada. No se puede proseguir."
                });
                return;
            }

            if (currentAdministrative.UserId != callFor.AdministrativeId)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "No es miembro"
                });
                return;
            }

            application.Banned = true;
            application.Deleted = true;
            ExpulsionReason expulsionReason = new()
            {
                Key = RandomNumberGenerator.GetHexString(32),
                Content = trimmedReason,
                Creation = DateTime.UtcNow,
                ApplicationId = application.ApplicationId
            };

            db.Applications.Update(application);
            db.ExpulsionReasons.Add(expulsionReason);

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
