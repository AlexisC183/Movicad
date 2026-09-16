using System;
using System.Collections.Generic;

namespace Movicad.Persistence;

public partial class AdministrativeMessageFile
{
    public long AdministrativeMessageFileId { get; set; }

    public string Key { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string MediaType { get; set; } = null!;

    public byte[] Content { get; set; } = null!;

    public long AdministrativeMessageId { get; set; }

    public virtual AdministrativeMessage AdministrativeMessage { get; set; } = null!;
}
