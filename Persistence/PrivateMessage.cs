using System;
using System.Collections.Generic;

namespace Movicad.Persistence;

public partial class PrivateMessage
{
    public long PrivateMessageId { get; set; }

    public string Key { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime Creation { get; set; }

    public byte[] AttachedImage { get; set; } = null!;

    public string ImageMediaType { get; set; } = null!;

    public long ApplicationId { get; set; }

    public bool Deleted { get; set; }

    public virtual Application Application { get; set; } = null!;
}
