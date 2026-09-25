using Movicad.Persistence;
using Movicad.Utils;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Movicad.Apis;

public static class Links
{
    public record CreateReq(
        string Title,
        string Url,
        string CallForKey
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

        CreateReq req = new(r.Title ?? "", r.Url ?? "", r.CallForKey ?? "");
        string trimmedTitle = req.Title.Trim();
        string trimmedUrl = req.Url.Trim();
        string trimmedCallForKey = req.CallForKey.Trim();

        if (trimmedTitle.Length < 1 || trimmedTitle.Length > 150)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "El título debe contener entre 1 y 150 caracteres"
            });
            return;
        }
        if (trimmedUrl.Length == 0)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "La URL es muy corta"
            });
            return;
        }

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

            Regex urlPattern = new("^https?://", RegexOptions.IgnoreCase);

            if (!urlPattern.IsMatch(trimmedUrl))
            {
                trimmedUrl = "http://" + trimmedUrl;
            }

            Link link = new()
            {
                Key = RandomNumberGenerator.GetHexString(32),
                Title = trimmedTitle,
                Url = trimmedUrl,
                CallForId = callFor.CallForId
            };

            db.Links.Add(link);
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
