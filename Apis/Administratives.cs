using Microsoft.AspNetCore.Mvc;
using Movicad.Persistence;
using Movicad.Utils;

namespace Movicad.Apis;

public static class Administratives
{
    public record UserAdminCountry(
        User User,
        Administrative Admin,
        Country? Country
    );

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

        IEnumerable<UserAdminCountry> userAdminCountries = db.Users
            .Join(
                db.Administratives,
                user => user.UserId,
                admin => admin.UserId,
                (user, admin) => new { user, admin }
            )
            .LeftJoin(
                db.Countries,
                userAdmin => userAdmin.admin.CountryId,
                country => country.CountryId,
                (userAdmin, country) => new UserAdminCountry(
                    userAdmin.user,
                    userAdmin.admin,
                    country
                )
            );

        IEnumerable<UserAdminCountry> filteredUserAdminCountries;

        if ()
        {
            
        }
        else if ()
        {
            
        }
        else if ()
        {
            
        }
        else
        {
            
        }

        // if (!string.IsNullOrWhiteSpace(searchTerm))
        // {
        //     string trimmedSearchTerm = searchTerm.Trim();
        //     filteredUserAdminPairs = userAdminPairs
        //         .Where(pair =>
        //             !pair.User.Deleted && (
        //                 pair.Admin.Name.Contains(trimmedSearchTerm) ||
        //                 pair.Admin.Acronym.Contains(trimmedSearchTerm) ||
        //                 pair.User.Key.Contains(trimmedSearchTerm)
        //             )
        //         );
        // }
        // else if (!string.IsNullOrWhiteSpace(type))
        // {
        //     filteredUserAdminPairs = userAdminPairs
        //         .Where(pair => pair.Admin.Type == type);
        // }
        // else if (!string.IsNullOrWhiteSpace(country))
        // {
        //     filteredUserAdminPairs = userAdminPairs
        //         .Join(
        //             db.Countries,
        //             pair => pair.Admin.CountryId,
        //             country => country.CountryId,
        //             (pair, country) => new { pair, country }
        //         )
        //         .Where(userAdminCountry =>
        //             userAdminCountry.country.Name == country
        //         )
        //         .Select()
        // }
        // else
        // {
            
        // }
    }
}
