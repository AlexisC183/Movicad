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
        [FromQuery(Name = "cursor-pos")] long? cursorPosMaybe,
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

        const long LastPossiblePos = 1;

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
            long? maxId = filteredUserAdminCountries
                .OrderByDescending(uac => uac.User.UserId)
                .Select(uac => (long?)uac.User.UserId)
                .FirstOrDefault();

            long cursorPos = maxId.HasValue ? maxId.Value + 1 : LastPossiblePos;

            IEnumerable<UserAdminCountry> paginatedUserAdminCountries = filteredUserAdminCountries
                .Where(uac => uac.User.UserId < cursorPos)
                .OrderByDescending(uac => uac.User.UserId)
                .Take(6); // 50

            long newCursorPos = paginatedUserAdminCountries
                .Select(uac => uac.User.UserId)
                .LastOrDefault(LastPossiblePos);

            var administratives = paginatedUserAdminCountries
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
                    CursorPosition = newCursorPos,
                    Administratives = administratives
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
