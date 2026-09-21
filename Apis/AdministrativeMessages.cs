using Movicad.Persistence;
using Movicad.Utils;

namespace Movicad.Apis;

public static class AdministrativeMessages
{



    /// <summary>
    /// POST
    /// </summary>
    public static async Task Create(HttpContext http, MovicadContext db)
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative);

        if (idRolePair is null)
        {
            return;
        }

        // string trimmedSubject = req.S
    }
}
