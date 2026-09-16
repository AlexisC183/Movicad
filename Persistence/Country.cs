using System;
using System.Collections.Generic;

namespace Movicad.Persistence;

public partial class Country
{
    public long CountryId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Administrative> Administratives { get; set; } = new List<Administrative>();

    public virtual ICollection<DestinationCountry> DestinationCountries { get; set; } = new List<DestinationCountry>();
}
