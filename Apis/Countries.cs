using Movicad.Persistence;
using Movicad.Utils;

namespace Movicad.Apis;

public static class Countries
{
    /// <summary>
    /// GET
    /// </summary>
    public static async Task ReadAll(HttpContext http, MovicadContext db)
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative, Roles.Student);

        if (idRolePair is null)
        {
            return;
        }

        try
        {
            string[] countries = db.Countries
                .Select(c => c.Name)
                .ToArray();

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "ok",
                Data = countries
            });
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }
}
