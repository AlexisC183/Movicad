using System;
using System.Collections.Generic;

namespace Movicad.Persistence;

public partial class Administrative
{
    public long UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Acronym { get; set; } = null!;

    public string Type { get; set; } = null!;

    public long? CountryId { get; set; }

    public string Website { get; set; } = null!;

    public byte[] Icon { get; set; } = null!;

    public string IconMediaType { get; set; } = null!;

    public virtual ICollection<AdministrativeMessage> AdministrativeMessageRecipients { get; set; } = new List<AdministrativeMessage>();

    public virtual ICollection<AdministrativeMessage> AdministrativeMessageSenders { get; set; } = new List<AdministrativeMessage>();

    public virtual ICollection<CallsFor> CallsFors { get; set; } = new List<CallsFor>();

    public virtual Country? Country { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<User> Senders { get; set; } = new List<User>();
}
