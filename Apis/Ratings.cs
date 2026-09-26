using Microsoft.EntityFrameworkCore;
using Movicad.Persistence;
using Movicad.Utils;

namespace Movicad.Apis;

public static class Ratings
{
    public record CreateReq(string AdministrativeKey);

    /// <summary>
    /// POST
    /// </summary>
    public static async Task Create(HttpContext http, MovicadContext db, CreateReq r)
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Student, Roles.Administrative);

        if (idRolePair is null)
        {
            return;
        }

        string trimmedAdministrativeKey = r.AdministrativeKey?.Trim() ?? "";

        try
        {
            User? administrative = db.Users.SingleOrDefault(user =>
                user.Key == trimmedAdministrativeKey &&
                user.Role == Roles.Administrative &&
                !user.Deleted
            );

            if (administrative is null)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "El administrativo de destino no existe"
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

            await db.Database.ExecuteSqlInterpolatedAsync($@"
                insert into ratings
                values ({sender.UserId}, {administrative.UserId})
                on conflict (sender_id, administrative_id) do nothing;
            ");

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new { Status = "ok" });
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }
}
