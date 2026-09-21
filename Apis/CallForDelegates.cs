using Movicad.Persistence;
using Movicad.Utils;

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
    public static async Task Create(HttpContext http, MovicadContext db, CreateReq req)
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative);

        if (idRolePair is null)
        {
            return;
        }

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
            
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }
}
