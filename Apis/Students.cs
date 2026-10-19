using Microsoft.AspNetCore.Mvc;
using Movicad.Persistence;
using Movicad.Utils;

namespace Movicad.Apis;

public static class Students
{
    /// <summary>
    /// GET
    /// </summary>
    public static async Task Read(HttpContext http, MovicadContext db)
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Student);

        if (idRolePair is null)
        {
            return;
        }

        try
        {
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
            var student = db.Students
                .Where(s => s.UserId == user.UserId)
                .Select(s => new
                {
                    s.Name,
                    s.Icon,
                    s.IconMediaType
                })
                .Single();
           
           http.Response.StatusCode = 200;
           await http.Response.WriteAsJsonAsync(new
           {
               Status = "ok",
               Data = student,
               IdRolePair = idRolePair
           });
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }

    /// <summary>
    /// GET
    /// </summary>
    public static async Task ReadBy(
        HttpContext http,
        MovicadContext db,
        [FromQuery(Name = "cursor-pos")] long? cursorPosMaybe,
        [FromQuery(Name = "call-for-key")] string callForKey = "",
        string status = ""
    )
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative);

        if (idRolePair is null)
        {
            return;
        }

        if (status.Length > 0 && status is not ("applied" or "banned"))
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Quiso decir 'applied' o 'banned'"
            });
            return;
        }

        const long LastPossiblePos = 1;
        const int PageSize = 20;
        string trimmedCallForKey = callForKey.Trim();

        try
        {
            CallsFor? callFor = db.CallsFors.SingleOrDefault(callFor =>
                callFor.Key == trimmedCallForKey &&
                !callFor.Deleted
            );
            if (callFor is null)
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

            if (requester.UserId != callFor.AdministrativeId)
            {
                http.Response.StatusCode = 403;
                await http.Response.WriteAsJsonAsync(new { Status = "forbidden" });
                return;
            }

            var filteredApplications = status switch
            {
                "applied" => db.Applications
                    .Where(appl =>
                        appl.CallForId == callFor.CallForId &&
                        !appl.Deleted
                    ),
                "banned" => db.Applications
                    .Where(appl =>
                        appl.CallForId == callFor.CallForId &&
                        appl.Banned
                    ),
                _ => db.Applications
                    .Where(appl =>
                        appl.CallForId == callFor.CallForId &&
                        (appl.Banned || !appl.Deleted)
                    )
            };
            var joins = from appl in filteredApplications
                join student in db.Students
                on appl.StudentId equals student.UserId
                join user in db.Users
                on student.UserId equals user.UserId
                select new
                {
                    appl.ApplicationId,
                    user.Key,
                    student.Name,
                    student.Icon,
                    student.IconMediaType
                };

            long cursorPos;

            if (cursorPosMaybe.HasValue)
            {
                cursorPos = cursorPosMaybe.Value;
            }
            else
            {
                long? maxId = joins
                    .OrderByDescending(jn => jn.ApplicationId)
                    .Select(jn => (long?)jn.ApplicationId)
                    .FirstOrDefault();
                cursorPos = maxId.HasValue ? maxId.Value + 1 : LastPossiblePos;
            }

            var paginatedJoins = joins
                .Where(jn => jn.ApplicationId < cursorPos)
                .OrderByDescending(jn => jn.ApplicationId)
                .Take(PageSize)
                .ToList();

            long newCursorPos = paginatedJoins
                .Select(jn => (long?)jn.ApplicationId)
                .ElementAtOrDefault(PageSize - 1) ?? LastPossiblePos;

            var students = paginatedJoins
                .Select(jn => new
                {
                    jn.Key,
                    jn.Name,
                    jn.Icon,
                    jn.IconMediaType
                })
                .ToArray();

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "ok",
                Data = new
                {
                    CursorPosition = newCursorPos,
                    Students = students
                },
                IdRolePair = idRolePair
            });
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }
}
