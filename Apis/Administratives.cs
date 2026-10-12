using Microsoft.AspNetCore.Mvc;
using Movicad.Persistence;
using Movicad.Utils;

namespace Movicad.Apis;

public static class Administratives
{
    // public record UserAdminCountry(
    //     User User,
    //     Administrative Admin,
    //     Country? Country
    // );

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

        var joins = db.Users
            .Where(user => !user.Deleted)
            .Join(
                db.Administratives,
                user => user.UserId,
                admin => admin.UserId,
                (user, admin) => new
                {
                    user.UserId,
                    user.Key,
                    admin.Acronym,
                    admin.Name,
                    admin.Type,
                    admin.CountryId,
                    admin.Icon,
                    admin.IconMediaType
                }
            )
            .LeftJoin(
                db.Countries,
                admin => admin.CountryId,
                country => country.CountryId,
                (admin, country) => new
                {
                    admin.UserId,
                    admin.Key,
                    admin.Acronym,
                    admin.Name,
                    admin.Type,
                    Country = country == null ? null : country.Name,
                    admin.Icon,
                    admin.IconMediaType
                }
            );

        var filteredJoins = joins;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string trimmedSearchTerm = searchTerm.Trim();
            filteredJoins = joins
                .Where(admin =>
                    admin.Name.Contains(trimmedSearchTerm) ||
                    admin.Acronym.Contains(trimmedSearchTerm) ||
                    admin.Key.Contains(trimmedSearchTerm)
                );
        }
        else if (!string.IsNullOrWhiteSpace(type))
        {
            filteredJoins = joins.Where(admin => admin.Type == type);
        }
        else if (!string.IsNullOrWhiteSpace(country))
        {
            filteredJoins = joins.Where(admin => admin.Country == country);
        }

        try
        {
            long cursorPos;

            if (cursorPosMaybe.HasValue)
            {
                cursorPos = cursorPosMaybe.Value;
            }
            else
            {
                long? maxId = filteredJoins
                    .OrderByDescending(admin => admin.UserId)
                    .Select(admin => (long?)admin.UserId)
                    .FirstOrDefault();
                cursorPos = maxId.HasValue ? maxId.Value + 1 : LastPossiblePos;
            }

            var paginatedJoins = filteredJoins
                .Where(admin => admin.UserId < cursorPos)
                .OrderByDescending(admin => admin.UserId)
                .Take(6) // 20
                .ToList();

            long newCursorPos = paginatedJoins
                .Select(admin => admin.UserId)
                .LastOrDefault(LastPossiblePos);

            var administratives = paginatedJoins
                .Select(admin => new
                {
                    admin.Key,
                    admin.Name,
                    admin.Type,
                    admin.Country,
                    admin.Icon,
                    admin.IconMediaType
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
