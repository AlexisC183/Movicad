namespace Movicad.Utils;

public record DbFile(string Name, DbFile.DataUri? Uri)
{
    public record DataUri(string MediaType, byte[] Content);
}
