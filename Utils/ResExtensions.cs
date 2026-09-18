namespace Movicad.Utils;

public static class ResExtensions
{
    public static async Task DbErr(this HttpResponse res, Exception cause)
    {
        Console.WriteLine(cause);

        res.StatusCode = 500;
        await res.WriteAsJsonAsync(new
        {
            Status = "err",
            Message = "No se pudieron obtener o guardar datos"
        });
    }
}
