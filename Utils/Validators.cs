using System.Text.RegularExpressions;

namespace Movicad.Utils;

public static class Validators
{
    public static async Task<DataUri?> ValidateFile(
        HttpContext http,
        string dataUri,
        int byteMaxSize,
        string mediaTypePattern = ".+/.+"
    )
    {
        ArgumentNullException.ThrowIfNull(http, nameof(http));
        ArgumentNullException.ThrowIfNull(dataUri, nameof(dataUri));
        ArgumentNullException.ThrowIfNull(mediaTypePattern, nameof(mediaTypePattern));
        
        Regex dataUriPattern = new($"^data:{mediaTypePattern};base64,");

        if (!dataUriPattern.IsMatch(dataUri))
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "URI de archivo no válida"
            });
            return null;
        }

        string[] metaAndData = dataUri.Split(',');
        string mediaType = string.Concat(
            metaAndData[0]
                .Skip(5)
                .TakeWhile(ch => ch != ';')
        );

        if (!Converters.TryB64ToBytes(metaAndData[1], out byte[]? bytes))
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = "Cadena Base64 de archivo no válida"
            });
            return null;
        }
        if (bytes!.Length > byteMaxSize)
        {
            http.Response.StatusCode = 400;
            await http.Response.WriteAsJsonAsync(new
            {
                Status = "err",
                Message = $"El archivo no puede exceder los {byteMaxSize / (float)StorageConstants.Megabyte} MB"
            });
            return null;
        }

        return new(mediaType, bytes);
    }
}
