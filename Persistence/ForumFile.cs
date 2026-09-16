using System;
using System.Collections.Generic;

namespace Movicad.Persistence;

public partial class ForumFile
{
    public long ForumFileId { get; set; }

    public string Key { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string MediaType { get; set; } = null!;

    public byte[] Content { get; set; } = null!;

    public DateTime Modification { get; set; }

    public long CallForId { get; set; }

    public bool Deleted { get; set; }

    public virtual CallsFor CallFor { get; set; } = null!;
}
