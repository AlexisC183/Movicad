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
        [FromQuery(Name = "from-id")] long? fromIdMaybe,
        [FromQuery(Name = "search-term")] string searchTerm = "",
        string type = "",
        string country = ""
    )
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative, Roles.Student);

        if (idRolePair is null)
        {
            return;
        }

        IEnumerable<UserAdminCountry> userAdminCountries = db.Users
            .Where(user => !user.Deleted)
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

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string trimmedSearchTerm = searchTerm.Trim();
            filteredUserAdminCountries = userAdminCountries
                .Where(uac =>
                    uac.Admin.Name.Contains(trimmedSearchTerm) ||
                    uac.Admin.Acronym.Contains(trimmedSearchTerm) ||
                    uac.User.Key.Contains(trimmedSearchTerm)
                );
        }
        else if (!string.IsNullOrWhiteSpace(type))
        {
            filteredUserAdminCountries = userAdminCountries.Where(uac => uac.Admin.Type == type);
        }
        else if (!string.IsNullOrWhiteSpace(country))
        {
            filteredUserAdminCountries = userAdminCountries
                .Where(uac => uac.Country is Country c && c.Name == country);
        }
        else
        {
            filteredUserAdminCountries = userAdminCountries;
        }

        try
        {
            long fromId = fromIdMaybe ?? filteredUserAdminCountries
                .Select(uac => uac.User.UserId)
                .OrderDescending()
                .FirstOrDefault();

            var administratives = filteredUserAdminCountries
                .Where(uac => uac.User.UserId <= fromId)
                .OrderByDescending(uac => uac.User.UserId)
                .Take(6) // 50
                .Select(uac => new
                {
                    uac.User.Key,
                    uac.Admin.Name,
                    uac.Admin.Type,
                    Country = uac.Country?.Name,
                    uac.Admin.Icon,
                    uac.Admin.IconMediaType
                })
                .ToArray();

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "ok",
                Data = new
                {
                    FromId = fromId,
                    Administratives = administratives
                },
                IdRolePair = idRolePair
            });

            /* FRONT-END
            nextFromId = fromId - 50
            */
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }
}
