# TODO
All DELETE APIs
All GET APIs

# TEST

# Código descartado
## Comprobando cuota de archivos subidos a un foro antes de crear la instancia de ForumFile
```csharp
int fileSizeSum = await db.ForumFiles
    .Where(file =>
        file.CallForId == callFor.CallForId &&
        !file.Deleted
    )
    .Select(file => (int?)file.Content.Length)
    .SumAsync() ?? 0;

if (
    fileSizeSum + parsedFileUri.Content.Length >
    100 * StorageConstants.Megabyte
)
{
    http.Response.StatusCode = 400;
    await http.Response.WriteAsJsonAsync(new
    {
        Status = "err",
        Message = "El foro tiene un límite de 100 MB en archivos subidos. Intente subir un archivo más pequeño o elimine otros."
    });
    return;
}
```
