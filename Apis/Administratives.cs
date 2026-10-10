using Microsoft.AspNetCore.Mvc;
using Movicad.Persistence;
using Movicad.Utils;

namespace Movicad.Apis;

public static class Administratives
{
    /// <summary>
    /// GET
    /// </summary>
    public static async Task ReadAll(
        HttpContext http,
        MovicadContext db,
        [FromQuery(Name = "from-id")] long? fromId,
        [FromQuery(Name = "search-term")] string searchTerm = "",
        string type = "",
        string country = ""
    )
    {
        /*
        pageSize = 4 (in real code is 50)

        fromId = 59 (max is default)
        next (front-end): fromId - pageSize

        .Where(r => r.Id <= fromId)
        .OrderByDescending(r => r.Id)
        .Take(pageSize)
        */
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative, Roles.Student);

        if (idRolePair is null)
        {
            return;
        }

        // IEnumerable<Administrative> administratives;

        // if (!string.IsNullOrWhiteSpace(searchTerm))
        // {
        //     administratives = 
        // }
        // else if ()
        // {
            
        // }
        // else if ()
        // {
            
        // }
        // else
        // {
            
        // }
    }
}
