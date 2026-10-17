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
}
