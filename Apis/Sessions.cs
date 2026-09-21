using BC = BCrypt.Net.BCrypt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Movicad.Persistence;
using Movicad.Utils;

namespace Movicad.Apis;

public static class Sessions
{
    public record LogInReq(string Id, string Password);

    /// <summary>
    /// POST
    /// </summary>
    public static async Task LogIn(HttpContext http, MovicadContext db, LogInReq req)
    {
        string key = req.Id.Trim().ToLower();

        try
        {
            User? user = db.Users.SingleOrDefault(u => u.Key == key);

            if (user is null || !BC.Verify(req.Password, user.Password))
            {
                http.Response.StatusCode = 401;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "El identificador o contraseña es incorrecto"
                });
                return;
            }

            List<Claim> claims = [
                new(ClaimTypes.NameIdentifier, user.Key),
                new(ClaimTypes.Role, user.Role)
            ];

            await http.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new(new ClaimsIdentity(claims, "password")),
                new() { IsPersistent = true }
            );

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new { Status = "ok" });
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }

    /// <summary>
    /// GET
    /// </summary>
    public static async Task LogOut(HttpContext http)
    {
        await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        http.Response.StatusCode = 200;
        await http.Response.WriteAsJsonAsync(new { Status = "ok" });
    }

    /// <summary>
    /// GET
    /// </summary>
    public static async Task VerifySession(HttpContext http)
    {
        Claim? idClaim = http.User.FindFirst(ClaimTypes.NameIdentifier);
    
        http.Response.StatusCode = 200;
        await http.Response.WriteAsJsonAsync(new
        {
            Status = "ok",
            Data = new { Exists = idClaim is not null }
        });
    }
}
