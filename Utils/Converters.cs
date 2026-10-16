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

    public static bool TryCompositeKeyToLongs(string? cKey, out long l1, out long l2)
    {
        if (cKey is null)
        {
            l1 = l2 = default;
            return false;
        }

        string[] fields = cKey.Split(',');

        if (fields.Length != 2)
        {
            l1 = l2 = default;
            return false;
        }

        if (
            !long.TryParse(fields[0], out long newL1) ||
            !long.TryParse(fields[1], out long newL2)
        )
        {
            l1 = l2 = default;
            return false;
        }

        l1 = newL1;
        l2 = newL2;
        return true;
    }
}
