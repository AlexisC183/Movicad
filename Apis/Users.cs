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

    public record UpdatePasswordReq(
        string OldPassword,
        string NewPassword,
        string NewPassword1
    );

    /// <summary>
    /// PATCH
    /// </summary>
    public static async Task UpdatePassword(HttpContext http, MovicadContext db, UpdatePasswordReq r)
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative, Roles.Student);

        if (idRolePair is null)
        {
            return;
        }

        UpdatePasswordReq req = new(r.OldPassword ?? "", r.NewPassword ?? "", r.NewPassword1 ?? "");

        if (req.NewPassword != req.NewPassword1)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Las contraseñas no coinciden"
            });
            return;
        }
        if (req.NewPassword.Length < 8)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Introducir una contraseña más larga"
            });
            return;
        }
        if (req.NewPassword.Length > 50)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Su contraseña es muy larga"
            });
            return;
        }
        if (req.NewPassword.All(char.IsLetterOrDigit))
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Introducir una contraseña con un carácter especial"
            });
            return;
        }
        if (req.NewPassword.All(ch => ch == req.NewPassword[0]))
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Introducir una contraseña con caracteres diferentes"
            });
            return;
        }

        try
        {
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
            if (!BC.Verify(req.OldPassword, user.Password))
            {
                http.Response.StatusCode = 401;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "Contraseña actual incorrecta"
                });
                return;
            }

            user.Password = BC.HashPassword(req.NewPassword);

            db.Users.Update(user);
            await db.SaveChangesAsync();

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new { Status = "ok" });
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }

    public interface IUpdateProfileReq
    {
        abstract string Name { get; }
        abstract string IconUri { get; }
    }

    public interface IUpdateAdministrativeProfileReq : IUpdateProfileReq
    {
        abstract string Acronym { get; }
        abstract string Type { get; }
        abstract string Country { get; }
        abstract string Website { get; }
    }

    public record UpdateProfileReq(
        string Name,
        string IconUri,
        string Acronym,
        string Type,
        string Country,
        string Website
    ) : IUpdateAdministrativeProfileReq;

    /// <summary>
    /// PATCH
    /// </summary>
    public static async Task UpdateProfile(HttpContext http, MovicadContext db, UpdateProfileReq r)
    {
        IdRolePair? idRolePair = await http.VerifyClaimsAsync(Roles.Administrative, Roles.Student);

        if (idRolePair is null)
        {
            return;
        }

        UpdateProfileReq request = new(
            r.Name ?? "",
            r.IconUri ?? "",
            r.Acronym ?? "",
            r.Type ?? "",
            r.Country ?? "",
            r.Website ?? ""
        );

        DataUri? parsedImageUri;

        if (request.IconUri.Length == 0)
        {
            parsedImageUri = new("image/gif", new byte[0]);
        }
        else
        {
            parsedImageUri = await Validators.ValidateFile(
                http,
                request.IconUri,
                10 * StorageConstants.Megabyte,
                @"image/(gif|jpeg|png|webp|svg\+xml)"
            );
        }

        if (parsedImageUri is null)
        {
            return;
        }

        string trimmedName = request.Name.Trim();

        if (idRolePair.Role == Roles.Student)
        {
            IUpdateProfileReq req = request;

            if (trimmedName.Length > 100)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Messag = "Su nombre es muy largo"
                });
                return;
            }

            try
            {
                Student? student = db.Users
                    .Join(
                        db.Students,
                        user => user.UserId,
                        student => student.UserId,
                        (user, student) => new { user, student }
                    )
                    .Where(userStudent =>
                        userStudent.user.Key == idRolePair.Id &&
                        !userStudent.user.Deleted
                    )
                    .Select(userStudent => userStudent.student)
                    .SingleOrDefault();

                if (student is null)
                {
                    http.Response.StatusCode = 401;
                    await http.Response.WriteAsJsonAsync(new
                    {
                        Status = "err",
                        Message = "Su cuenta ha sido eliminada. No se puede proseguir."
                    });
                    return;
                }

                student.Name = trimmedName;
                student.Icon = parsedImageUri.Content;
                student.IconMediaType = parsedImageUri.MediaType;

                db.Students.Update(student);
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                await http.Response.DbErr(e);
            }
        }
        else if (idRolePair.Role == Roles.Administrative)
        {
            IUpdateAdministrativeProfileReq req = request;
            string trimmedAcronym = req.Acronym.Trim();
            string trimmedWebsite = req.Website.Trim();

            if (trimmedName.Length > 200)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Messag = "Su nombre es muy largo"
                });
                return;
            }
        }
        else
        {
            throw new NotImplementedException();
        }

        http.Response.StatusCode = 200;
        await http.Response.WriteAsJsonAsync(new { Status = "ok" });
    }
}
