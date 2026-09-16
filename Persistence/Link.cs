using System;
using System.Collections.Generic;

namespace Movicad.Persistence;

public partial class Link
{
    public long LinkId { get; set; }

    public string Key { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Url { get; set; } = null!;

    public long CallForId { get; set; }

    public bool Deleted { get; set; }

    public virtual CallsFor CallFor { get; set; } = null!;
}
