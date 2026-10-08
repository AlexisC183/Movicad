using Microsoft.AspNetCore.Mvc;
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

    public record UpdateReq(
        string Key,
        string Question,
        string Answer
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

        string trimmedKey = r.Key?.Trim() ?? "";
        string trimmedQuestion = r.Question?.Trim() ?? "";
        string trimmedAnswer = r.Answer?.Trim() ?? "";
        
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
            FrequentQuestion? frequentQuestion = db.FrequentQuestions
                .SingleOrDefault(fq =>
                    fq.Key == trimmedKey &&
                    !fq.Deleted
                );
            
            if (frequentQuestion is null)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "La pregunta frecuente a modificar no existe"
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

            if (!db.CallsFors.Any(callFor =>
                callFor.CallForId == frequentQuestion.CallForId &&
                callFor.AdministrativeId == user.UserId &&
                !callFor.Deleted
            ))
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "No es miembro"
                });
                return;
            }

            frequentQuestion.Question = trimmedQuestion;
            frequentQuestion.Answer = trimmedAnswer;

            db.FrequentQuestions.Update(frequentQuestion);
            await db.SaveChangesAsync();

            http.Response.StatusCode = 200;
            await http.Response.WriteAsJsonAsync(new { Status = "ok" });
        }
        catch (Exception e)
        {
            await http.Response.DbErr(e);
        }
    }

    public record DeleteReq(string Key);

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

        string trimmedKey = r.Key?.Trim() ?? "";

        try
        {
            FrequentQuestion? question = db.FrequentQuestions
                .SingleOrDefault(question =>
                    question.Key == trimmedKey &&
                    !question.Deleted
                );

            if (question is null)
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "La pregunta frecuente no existe"
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

            if (!db.CallsFors.Any(callFor =>
                callFor.CallForId == question.CallForId &&
                callFor.AdministrativeId == user.UserId &&
                !callFor.Deleted
            ))
            {
                http.Response.StatusCode = 400;
                await http.Response.WriteAsJsonAsync(new
                {
                    Status = "err",
                    Message = "No es miembro"
                });
                return;
            }

            question.Deleted = true;

            db.FrequentQuestions.Update(question);
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
