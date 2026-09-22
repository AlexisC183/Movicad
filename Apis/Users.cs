using BC = BCrypt.Net.BCrypt;
using Movicad.Persistence;
using Movicad.Utils;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Movicad.Apis;

public static class Users
{
    public record CreateReq(
        string Id,
        string Password,
        string Password1,
        string Role
    );

    /// <summary>
    /// POST
    /// </summary>
    public static async Task Create(HttpContext http, MovicadContext db, CreateReq r)
    {
        CreateReq req = new(r.Id ?? "", r.Password ?? "", r.Password1 ?? "", r.Role ?? "");

        if (req.Id.Length == 0)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Su identificador es muy corto"
            });
            return;
        }
        if (req.Id.Length > 50)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Su identificador es muy largo"
            });
            return;
        }
        if (!new Regex(@"^\w+$", RegexOptions.ECMAScript).IsMatch(req.Id))
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Introducir un identificador válido"
            });
            return;
        }
        if (req.Password != req.Password1)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Las contraseñas no coinciden"
            });
            return;
        }
        if (req.Password.Length < 8)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Introducir una contraseña más larga"
            });
            return;
        }
        if (req.Password.Length > 50)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Su contraseña es muy larga"
            });
            return;
        }
        if (req.Password.All(char.IsLetterOrDigit))
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Introducir una contraseña con un carácter especial"
            });
            return;
        }
        if (req.Password.All(ch => ch == req.Password[0]))
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Introducir una contraseña con caracteres diferentes"
            });
            return;
        }
        if (req.Role is not (Roles.Student or Roles.Administrative))
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Rol no válido"
            });
            return;
        }

        User user = new()
        {
            Key = req.Id.ToLower(),
            Role = req.Role,
            Password = BC.HashPassword(req.Password),
            Creation = DateTime.UtcNow
        };

        if (req.Role == Roles.Student)
        {
            Student studentData = new()
            {
                Name = "",
                Icon = new byte[] {},
                IconMediaType = "image/gif"
            };
            user.Student = studentData;
        }
        if (req.Role == Roles.Administrative)
        {
            Administrative administrativeData = new()
            {
                Name = "",
                Acronym = "",
                Type = "publica",
                Website = "",
                Icon = new byte[] {},
                IconMediaType = "image/gif"
            };
            user.Administrative = administrativeData;
        }

        try
        {
            if (db.Users.Any(u => u.Key == user.Key))
            {
                http.Response.StatusCode = 500;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "Intentar un identificador único diferente"
                });
                return;
            }

            db.Users.Add(user);
            await db.SaveChangesAsync();

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
}
