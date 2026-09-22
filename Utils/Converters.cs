namespace Movicad.Utils;

public static class Converters
{
    public static bool TryB64ToBytes(string? b64, out byte[]? bytes)
    {
        if (b64 is null)
        {
            bytes = null;
            return false;
        }

        int maxByteLength = b64.Length * 3 / 4;
        byte[] buffer = new byte[maxByteLength];

        bool correct = Convert.TryFromBase64String(b64, buffer, out int bytesWritten);
        bytes = correct ? buffer[..bytesWritten] : null;

        return correct;
    }
}
