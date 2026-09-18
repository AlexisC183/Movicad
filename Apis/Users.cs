using Movicad.Persistence;
using System.Text.RegularExpressions;

namespace Movicad.Apis;

public static class Users
{
    public record CreateReq(
        string Id,
        string Password,
        string Password1,
        string Role
    );

    public static async Task Create(HttpContext http, MovicadContext db, CreateReq req)
    {
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
        if (req.Role is not ("estudiante" or "administrativo"))
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Rol no válido"
            });
            return;
        }

        string lowerKey = req.Id.ToLower();
        User user = new()
        {
            Key = lowerKey,

        };
    }
}
