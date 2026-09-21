using System.Security.Claims;

namespace Movicad.Utils;

public static class HttpExtensions
{
    public static async Task<IdRolePair?> VerifyClaimsAsync(this HttpContext http, params string[] allowedRoles)
    {
        ArgumentNullException.ThrowIfNull(http, nameof(http));
        ArgumentNullException.ThrowIfNull(allowedRoles, nameof(allowedRoles));

        ClaimsIdentity? identity = http.User.Identities
            .FirstOrDefault(i => i.AuthenticationType == "password");

        if (identity is null)
        {
            http.Response.StatusCode = 401;
            await http.Response.WriteAsJsonAsync(new { Status = "logout" });
            
            return null;
        }

        Claim? roleClaim = http.User.FindFirst(ClaimTypes.Role);

        if (roleClaim is null || !allowedRoles.Contains(roleClaim.Value))
        {
            http.Response.StatusCode = 403;
            await http.Response.WriteAsJsonAsync(new { Status = "forbidden" });
            
            return null;
        }

        Claim? idClaim = http.User.FindFirst(ClaimTypes.NameIdentifier);

        if (idClaim is null)
        {
            throw new InvalidOperationException("NameIdentifier claim not found.");
        }

        return new(idClaim.Value, roleClaim.Value);
    }
}
