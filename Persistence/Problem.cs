using System;
using System.Collections.Generic;

namespace Movicad.Persistence;

public partial class Problem
{
    public long ProblemId { get; set; }

    public string AuthorEmail { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Body { get; set; } = null!;

    public DateTime Creation { get; set; }

    public bool Deleted { get; set; }
}
