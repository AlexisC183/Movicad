using System;
using System.Collections.Generic;

namespace Movicad.Persistence;

public partial class FrequentQuestion
{
    public long FrequentQuestionId { get; set; }

    public string Key { get; set; } = null!;

    public string Question { get; set; } = null!;

    public string Answer { get; set; } = null!;

    public long CallForId { get; set; }

    public bool Deleted { get; set; }

    public virtual CallsFor CallFor { get; set; } = null!;
}
