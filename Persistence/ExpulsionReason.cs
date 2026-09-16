using System;
using System.Collections.Generic;

namespace Movicad.Persistence;

public partial class ExpulsionReason
{
    public long ExpulsionReasonId { get; set; }

    public string Key { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime Creation { get; set; }

    public long ApplicationId { get; set; }

    public bool Deleted { get; set; }

    public virtual Application Application { get; set; } = null!;
}
