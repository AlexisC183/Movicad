using Movicad.Persistence;
using Movicad.Utils;
using System.Security.Cryptography;

namespace Movicad.Apis;

public static class FrequentQuestions
{
    public record CreateReq(
        string Question,
        string Answer,
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

        CreateReq req = new(r.Question ?? "", r.Answer ?? "", r.CallForKey ?? "");
        string trimmedQuestion = req.Question.Trim();
        string trimmedAnswer = req.Answer.Trim();
        string trimmedCallForKey = req.CallForKey.Trim();

        if (trimmedQuestion.Length < 1 || trimmedQuestion.Length > 150)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "La pregunta debe contener entre 1 y 150 caracteres"
            });
            return;
        }
        if (trimmedAnswer.Length == 0)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "La respuesta es muy corta"
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

            FrequentQuestion question = new()
            {
                Key = RandomNumberGenerator.GetHexString(32),
                Question = trimmedQuestion,
                Answer = trimmedAnswer,
                CallForId = callFor.CallForId
            };

            db.FrequentQuestions.Add(question);
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
