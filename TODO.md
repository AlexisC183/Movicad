# TODO

All GET APIs


# FIXME


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

# Problemas
- Es posible efectuar algunas peticiones aunque hayan ciertas eliminaciones, p. ej. enviar mensaje privado a administrativo que deleted = true.
No es un problema de seguridad, es un detalle que resulta en que el usuario no recibe beneficio alguno al permitirle estas peticiones.
Esto también puede verse en algunas peticiones GET como a `/api/private-messages/read`: es posible leer las conversaciones privadas con un administrativo aunque
esté eliminado; puede tener sentido, ya que en su momento el administrativo creó una convocatoria y pudieron enviarse mensajes un estudiante y un administrativo.


# temp
