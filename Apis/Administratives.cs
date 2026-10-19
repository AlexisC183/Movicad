using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movicad.Persistence;
using Movicad.Utils;

namespace Movicad.Apis;

public static class Administratives
{
    /// <summary>
    /// GET
    /// </summary>
    public static async Task Read(
        HttpContext http,
        MovicadContext db,
        string key
    )
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative, Roles.Student);

        if (idRolePair is null)
        {
            return;
        }

        string trimmedKey = key.Trim();

        try
        {
            var adminJoin = db.Users
                .Where(user =>
                    !user.Deleted &&
                    user.Key == trimmedKey
                )
                .Join(
                    db.Administratives,
                    user => user.UserId,
                    admin => admin.UserId,
                    (user, admin) => new
                    {
                        user.UserId,
                        user.Key,
                        admin.Acronym,
                        admin.Icon,
                        admin.IconMediaType,
                        admin.Name,
                        admin.Type,
                        admin.CountryId,
                        admin.Website
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
                        admin.Icon,
                        admin.IconMediaType,
                        admin.Name,
                        admin.Type,
                        Country = country == null ? null : country.Name,
                        admin.Website
                    }
                )
                .SingleOrDefault();

            if (adminJoin is null)
            {
                http.Response.StatusCode = 404;
                await http.Response.WriteAsJsonAsync(new { Status = "not_found" });
                return;
            }

            // Ratings from deleted users are also counted
            long ratings = await db.Database.SqlQuery<long>($"""
                select count(*) as "Value" from ratings
                where administrative_id = {adminJoin.UserId}
            """).SingleAsync();

            var admin = new
            {
                adminJoin.Key,
                adminJoin.Acronym,
                adminJoin.Icon,
                adminJoin.IconMediaType,
                adminJoin.Name,
                adminJoin.Type,
                adminJoin.Country,
                adminJoin.Website,
                Ratings = ratings
            };

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "ok",
                Data = admin,
                IdRolePair = idRolePair
            });
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }

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
        const int PageSize = 20;

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
                .Take(PageSize)
                .ToList();

            long newCursorPos = paginatedJoins
                .Select(admin => (long?)admin.UserId)
                .ElementAtOrDefault(PageSize - 1) ?? LastPossiblePos;

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

    /// <summary>
    /// GET
    /// </summary>
    public static async Task Dashboard(HttpContext http, MovicadContext db)
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative);

        if (idRolePair is null)
        {
            return;
        }

        try
        {
            User? admin = db.Users.SingleOrDefault(user =>
                user.Key == idRolePair.Id &&
                !user.Deleted
            );
            if (admin is null)
            {
                http.Response.StatusCode = 401;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "Su cuenta ha sido eliminada. No se puede proseguir."
                });
                return;
            }

            DateTime now = DateTime.UtcNow;

            Task<long> openCallForCountTask = db.CallsFors
                .Select(cf => new CallsFor()
                {
                    CallForId = cf.CallForId,
                    Key = cf.Key,
                    Title = cf.Title,
                    InitialDate = cf.InitialDate == null
                    ?
                        new(DateTime.MinValue.Ticks, DateTimeKind.Utc)
                    :
                        cf.InitialDate,
                    FinalDate = cf.FinalDate == null
                    ?
                        new(DateTime.MaxValue.Ticks, DateTimeKind.Utc)
                    :
                        cf.FinalDate,
                    Description = cf.Description,
                    Requirements = cf.Requirements,
                    PublishDate = cf.PublishDate,
                    Modification = cf.Modification,
                    AdministrativeId = cf.AdministrativeId,
                    Deleted = cf.Deleted
                })
                .Where(cf =>
                    !cf.Deleted &&
                    cf.AdministrativeId == admin.UserId &&
                    now >= cf.InitialDate &&
                    now <= cf.FinalDate
                )
                .LongCountAsync();
            
            Task<long> callForCountTask = db.CallsFors
                .Where(cf =>
                    !cf.Deleted &&
                    cf.AdministrativeId == admin.UserId
                )
                .LongCountAsync();

            Task<long> callForHistoryCountTask = db.CallsFors
                .Where(cf => cf.AdministrativeId == admin.UserId)
                .LongCountAsync();

            Task<long?> applicantCountTask = db.CallsFors
                .Where(cf =>
                    !cf.Deleted &&
                    cf.AdministrativeId == admin.UserId
                )
                .GroupJoin(
                    db.Applications.Where(appl => !appl.Deleted),
                    cf => cf.CallForId,
                    appl => appl.CallForId,
                    (cf, appls) => new { cf, appls }
                )
                .Select(jn => (long?)jn.appls.LongCount())
                .SumAsync();

            
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }
}
