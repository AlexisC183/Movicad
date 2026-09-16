using System;
using System.Collections.Generic;

namespace Movicad.Persistence;

public partial class ForumMessage
{
    public long ForumMessageId { get; set; }

    public string Key { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime Creation { get; set; }

    public byte[] AttachedImage { get; set; } = null!;

    public string ImageMediaType { get; set; } = null!;

    public long UserId { get; set; }

    public long CallForId { get; set; }

    public bool Deleted { get; set; }

    public virtual CallsFor CallFor { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
