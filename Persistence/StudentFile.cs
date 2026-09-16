using System;
using System.Collections.Generic;

namespace Movicad.Persistence;

public partial class StudentFile
{
    public long ApplicationId { get; set; }

    public long FileRequestId { get; set; }

    public string Name { get; set; } = null!;

    public string MediaType { get; set; } = null!;

    public byte[] Content { get; set; } = null!;

    public DateTime Modification { get; set; }

    public bool Deleted { get; set; }

    public virtual Application Application { get; set; } = null!;

    public virtual FileRequest FileRequest { get; set; } = null!;
}
