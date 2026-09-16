using System;
using System.Collections.Generic;

namespace Movicad.Persistence;

public partial class DestinationCountry
{
    public long CallForId { get; set; }

    public long CountryId { get; set; }

    public bool Deleted { get; set; }

    public virtual CallsFor CallFor { get; set; } = null!;

    public virtual Country Country { get; set; } = null!;
}
