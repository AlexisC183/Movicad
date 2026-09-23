using Movicad.Persistence;
using Movicad.Utils;

namespace Movicad.Apis;

public static class Applications
{
    public record CreateReq(string Key);

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

        string trimmedKey = r.Key?.Trim() ?? "";

        try
        {
            CallsFor? callFor = db.CallsFors.SingleOrDefault(callFor => callFor.Key == trimmedKey);

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

            User user = db.Users.Single(user => user.Key == idRolePair.Id);

            Application? pastApplication = db.Applications
                .SingleOrDefault(appl =>
                    appl.StudentId == user.UserId &&
                    appl.CallForId == callFor.CallForId
                );
            
            if (pastApplication is null)
            {
                Application application = new()
                {
                    StudentId = user.UserId,
                    CallForId = callFor.CallForId
                };

                db.Applications.Add(application);
                await db.SaveChangesAsync();
            }
            else if (pastApplication.Banned)
            {
                http.Response.StatusCode = 403;
                await http.Response.WriteAsJsonAsync(new { Status = "forbidden" });
                return;
            }
            else
            {
                pastApplication.Deleted = false;
                db.Applications.Update(pastApplication);
                await db.SaveChangesAsync();
            }

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new { Status = "ok" });
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }
}
