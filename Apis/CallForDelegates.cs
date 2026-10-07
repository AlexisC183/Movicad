using BC = BCrypt.Net.BCrypt;
using Movicad.Persistence;
using Movicad.Utils;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;

namespace Movicad.Apis;

public static class CallForDelegates
{
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
