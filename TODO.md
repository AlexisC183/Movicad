# TODO

All GET APIs

# FIXME
Administratives.ReadAll doesn't respect collation when filtering by searchTerm.
Removing the record and using implicit types was tested but there was a LINQ translation error.

# TEST
Administratives.ReadAll

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
