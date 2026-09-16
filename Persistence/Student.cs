using System;
using System.Collections.Generic;

namespace Movicad.Persistence;

public partial class Student
{
    public long UserId { get; set; }

    public string Name { get; set; } = null!;

    public byte[] Icon { get; set; } = null!;

    public string IconMediaType { get; set; } = null!;

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual User User { get; set; } = null!;
}
