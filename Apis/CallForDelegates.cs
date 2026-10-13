using BC = BCrypt.Net.BCrypt;
using Movicad.Persistence;
using Movicad.Utils;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;

namespace Movicad.Apis;

public static class CallForDelegates
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
            var callForJoin = db.CallsFors
                .Where(cf =>
                    !cf.Deleted &&
                    cf.Key == trimmedKey
                )
                .Join(
                    db.Administratives,
                    cf => cf.AdministrativeId,
                    admin => admin.UserId,
                    (cf, admin) => new
                    {
                        admin.UserId,
                        admin.Acronym,
                        cf.CallForId,
                        cf.Title,
                        cf.Description,
                        cf.Requirements,
                        cf.InitialDate,
                        cf.FinalDate,
                        cf.Modification
                    }
                )
                .Join(
                    db.Users,
                    cf => cf.UserId,
                    user => user.UserId,
                    (cf, user) => new
                    {
                        AdminKey = user.Key,
                        cf.Acronym,
                        cf.CallForId,
                        cf.Title,
                        cf.Description,
                        cf.Requirements,
                        cf.InitialDate,
                        cf.FinalDate,
                        cf.Modification
                    }
                )
                .GroupJoin(
                    db.DestinationCountries,
                    cf => cf.CallForId,
                    destCountry => destCountry.CallForId,
                    (cf, destCountries) => new
                    {
                        cf.AdminKey,
                        cf.Acronym,
                        cf.Title,
                        cf.Description,
                        cf.Requirements,
                        cf.InitialDate,
                        cf.FinalDate,
                        cf.Modification,
                        Countries = destCountries
                            .Join(
                                db.Countries,
                                destCountry => destCountry.CountryId,
                                country => country.CountryId,
                                (_, country) => country.Name
                            )
                            .ToArray()
                    }
                )
                .SingleOrDefault();

            if (callForJoin is null)
            {
                http.Response.StatusCode = 404;
                await http.Response.WriteAsJsonAsync(new { Status = "not_found" });
                return;
            }

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "ok",
                Data = callForJoin,
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
        string country = "",
        [FromQuery(Name = "admin-key")] string adminKey = "",
        [FromQuery(Name = "open-only")] bool openOnly = false
    )
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative, Roles.Student);

        if (idRolePair is null)
        {
            return;
        }

        const long LastPossiblePos = 1;
        const int PageSize = 20;
        DateTime now = DateTime.UtcNow;

        var normalizedCallFors = db.CallsFors
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
            });

        var joins = (openOnly
            ?
                normalizedCallFors.Where(cf =>
                    !cf.Deleted &&
                    now >= cf.InitialDate &&
                    now <= cf.FinalDate
                )
            :
                normalizedCallFors.Where(cf => !cf.Deleted)
        )
            .Join(
                db.Administratives,
                cf => cf.AdministrativeId,
                admin => admin.UserId,
                (cf, admin) => new
                {
                    admin.UserId,
                    admin.Icon,
                    admin.IconMediaType,
                    cf.CallForId,
                    CallForKey = cf.Key,
                    cf.Title,
                    cf.PublishDate
                }
            )
            .Join(
                db.Users,
                cf => cf.UserId,
                user => user.UserId,
                (cf, user) => new
                {
                    AdminKey = user.Key,
                    cf.Icon,
                    cf.IconMediaType,
                    cf.CallForId,
                    cf.CallForKey,
                    cf.Title,
                    cf.PublishDate
                }
            )
            .GroupJoin(
                db.DestinationCountries,
                cf => cf.CallForId,
                destCountry => destCountry.CallForId,
                (cf, destCountries) => new
                {
                    cf.AdminKey,
                    cf.Icon,
                    cf.IconMediaType,
                    cf.CallForId,
                    cf.CallForKey,
                    cf.Title,
                    cf.PublishDate,
                    Countries = destCountries
                        .Join(
                            db.Countries,
                            destCountry => destCountry.CountryId,
                            country => country.CountryId,
                            (_, country) => country.Name
                        )
                        .ToArray()
                }
            );
        
        var filteredJoins = joins;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string trimmedSearchTerm = searchTerm.Trim();
            filteredJoins = joins
                .Where(cf =>
                    cf.Title.Contains(trimmedSearchTerm) ||
                    cf.CallForKey == trimmedSearchTerm
                );
        }
        else if (!string.IsNullOrWhiteSpace(country))
        {
            filteredJoins = joins.Where(cf => cf.Countries.Contains(country));
        }
        else if (!string.IsNullOrWhiteSpace(adminKey))
        {
            string trimmedAdminKey = adminKey.Trim();
            filteredJoins = joins.Where(cf => cf.AdminKey == trimmedAdminKey);
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
                    .OrderByDescending(cf => cf.CallForId)
                    .Select(cf => (long?)cf.CallForId)
                    .FirstOrDefault();
                cursorPos = maxId.HasValue ? maxId.Value + 1 : LastPossiblePos;
            }

            var paginatedJoins = filteredJoins
                .Where(cf => cf.CallForId < cursorPos)
                .OrderByDescending(cf => cf.CallForId)
                .Take(PageSize)
                .ToList();

            long newCursorPos = paginatedJoins
                .Select(cf => (long?)cf.CallForId)
                .ElementAtOrDefault(PageSize - 1) ?? LastPossiblePos;

            var callsFor = paginatedJoins
                .Select(cf => new
                {
                    cf.Icon,
                    cf.IconMediaType,
                    Key = cf.CallForKey,
                    cf.Title,
                    cf.PublishDate,
                    cf.Countries
                })
                .ToArray();

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "ok",
                Data = new
                {
                    CursorPosition = newCursorPos,
                    CallsFor = callsFor
                },
                IdRolePair = idRolePair
            });
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }

    public record CreateReq(
        string Title,
        string Description,
        string Requirements,
        string[] DestinationCountries,
        string? InitialDate,
        string? FinalDate
    );

    /// <summary>
    /// POST
    /// </summary>
    public static async Task Create(HttpContext http, MovicadContext db, CreateReq r)
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative);

        if (idRolePair is null)
        {
            return;
        }

        CreateReq req = new(
            r.Title ?? "",
            r.Description ?? "",
            r.Requirements ?? "",
            r.DestinationCountries ?? new string[0],
            r.InitialDate,
            r.FinalDate
        );
        string trimmedTitle = req.Title.Trim();
        string trimmedDescription = req.Description.Trim();
        string trimmedRequirements = req.Requirements.Trim();

        if (trimmedTitle.Length < 1 || trimmedTitle.Length > 200)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "El título debe contener entre 1 y 200 caracteres"
            });
            return;
        }

        bool correctInitialDate = DateTime.TryParse(req.InitialDate, out DateTime initialDate);

        if (req.InitialDate is not null && !correctInitialDate)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Fecha de apertura no válida"
            });
            return;
        }

        DateTime? initialDateUtc = req.InitialDate is null
        ?
            null
        :
            new(initialDate.Ticks, DateTimeKind.Utc);

        bool correctFinalDate = DateTime.TryParse(req.FinalDate, out DateTime finalDate);

        if (req.FinalDate is not null && !correctFinalDate)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Fecha de cierre no válida"
            });
            return;
        }

        DateTime? finalDateUtc = req.FinalDate is null
        ?
            null
        :
            new(finalDate.Ticks, DateTimeKind.Utc);

        if (
            initialDateUtc is not null &&
            finalDateUtc is not null &&
            initialDateUtc > finalDateUtc
        )
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "La fecha de apertura no puede ser mayor que la fecha de cierre"
            });
            return;
        }

        try
        {
            DateTime now = DateTime.UtcNow;
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

            ICollection<DestinationCountry> destinationCountries = db.Countries
                .Join(
                    req.DestinationCountries.ToHashSet(),
                    country => country.Name,
                    name => name,
                    (country, _) => country
                )
                .Select(country => new DestinationCountry()
                {
                    CountryId = country.CountryId
                })
                .ToList();

            CallsFor callFor = new()
            {
                Key = RandomNumberGenerator.GetHexString(32),
                Title = trimmedTitle,
                InitialDate = initialDateUtc,
                FinalDate = finalDateUtc,
                Description = trimmedDescription,
                Requirements = trimmedRequirements,
                PublishDate = now,
                Modification = now,
                AdministrativeId = user.UserId,
                DestinationCountries = destinationCountries
            };

            db.CallsFors.Add(callFor);
            await db.SaveChangesAsync();

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new { Status = "ok" });
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }

    public record UpdateReq(
        string Key,
        string Title,
        string Description,
        string Requirements,
        string[] DestinationCountries,
        string? InitialDate,
        string? FinalDate
    );

    /// <summary>
    /// PATCH
    /// </summary>
    public static async Task Update(HttpContext http, MovicadContext db, UpdateReq r)
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative);

        if (idRolePair is null)
        {
            return;
        }

        UpdateReq req = new(
            r.Key ?? "",
            r.Title ?? "",
            r.Description ?? "",
            r.Requirements ?? "",
            r.DestinationCountries ?? new string[0],
            r.InitialDate,
            r.FinalDate
        );
        string trimmedKey = req.Key.Trim();
        string trimmedTitle = req.Title.Trim();
        string trimmedDescription = req.Description.Trim();
        string trimmedRequirements = req.Requirements.Trim();

        if (trimmedTitle.Length < 1 || trimmedTitle.Length > 200)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "El título debe contener entre 1 y 200 caracteres"
            });
            return;
        }

        bool correctInitialDate = DateTime.TryParse(req.InitialDate, out DateTime initialDate);

        if (req.InitialDate is not null && !correctInitialDate)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Fecha de apertura no válida"
            });
            return;
        }

        DateTime? initialDateUtc = req.InitialDate is null
        ?
            null
        :
            new(initialDate.Ticks, DateTimeKind.Utc);

        bool correctFinalDate = DateTime.TryParse(req.FinalDate, out DateTime finalDate);

        if (req.FinalDate is not null && !correctFinalDate)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Fecha de cierre no válida"
            });
            return;
        }

        DateTime? finalDateUtc = req.FinalDate is null
        ?
            null
        :
            new(finalDate.Ticks, DateTimeKind.Utc);

        if (
            initialDateUtc is not null &&
            finalDateUtc is not null &&
            initialDateUtc > finalDateUtc
        )
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "La fecha de apertura no puede ser mayor que la fecha de cierre"
            });
            return;
        }

        try
        {
            CallsFor? callFor = db.CallsFors.SingleOrDefault(callFor =>
                callFor.Key == trimmedKey &&
                !callFor.Deleted
            );

            if (callFor is null)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "La convocatoria no existe"
                });
                return;
            }

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

            if (user.UserId != callFor.AdministrativeId)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "No es miembro"
                });
                return;
            }

            List<DestinationCountry> oldDestinationCountries = db.DestinationCountries
                .Where(destination => destination.CallForId == callFor.CallForId)
                .ToList();

            List<long> oldCountryIds = oldDestinationCountries
                .Select(destination => destination.CountryId)
                .ToList();

            List<long> incomingCountryIds = db.Countries
                .Join(
                    req.DestinationCountries.ToHashSet(),
                    country => country.Name,
                    name => name,
                    (country, _) => country.CountryId
                )
                .ToList();

            List<long> removalCountryIds = oldCountryIds
                .Except(incomingCountryIds)
                .ToList();

            foreach (DestinationCountry destination in oldDestinationCountries)
            {
                destination.Deleted = destination switch
                {
                    { Deleted: false } when removalCountryIds.Contains(destination.CountryId) => true,
                    { Deleted: true } when incomingCountryIds.Contains(destination.CountryId) => false,
                    _ => destination.Deleted
                };
            }

            IEnumerable<DestinationCountry> insertionDestinationCountries = incomingCountryIds
                .Except(oldCountryIds)
                .Select(id => new DestinationCountry()
                {
                    CallForId = callFor.CallForId,
                    CountryId = id
                });

            callFor.Title = trimmedTitle;
            callFor.InitialDate = initialDateUtc;
            callFor.FinalDate = finalDateUtc;
            callFor.Description = req.Description;
            callFor.Requirements = req.Requirements;
            callFor.Modification = DateTime.UtcNow;

            db.CallsFors.Update(callFor);
            db.DestinationCountries.UpdateRange(oldDestinationCountries);
            db.DestinationCountries.AddRange(insertionDestinationCountries);
            await db.SaveChangesAsync();

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new { Status = "ok" });
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }

    public record DeleteReq(string CallForKey, string Password);

    /// <summary>
    /// DELETE
    /// </summary>
    public static async Task Delete(
        HttpContext http,
        MovicadContext db,
        [FromBody] DeleteReq r
    )
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative);

        if (idRolePair is null)
        {
            return;
        }

        string trimmedCallForKey = r.CallForKey?.Trim() ?? "";
        string password = r.Password ?? "";

        try
        {
            CallsFor? callFor = db.CallsFors.SingleOrDefault(callFor =>
                callFor.Key == trimmedCallForKey &&
                !callFor.Deleted
            );

            if (callFor is null)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "La convocatoria no existe"
                });
                return;
            }

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

            if (user.UserId != callFor.AdministrativeId)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "No es miembro"
                });
                return;
            }

            if (!BC.Verify(password, user.Password))
            {
                http.Response.StatusCode = 401;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "Contraseña incorrecta"
                });
                return;
            }

            callFor.Deleted = true;

            db.CallsFors.Update(callFor);
            await db.SaveChangesAsync();

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new { Status = "ok" });
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }
}
