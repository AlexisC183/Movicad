using System;
using System.Collections.Generic;

namespace Movicad.Persistence;

public partial class AdministrativeMessage
{
    public long AdministrativeMessageId { get; set; }

    public string Key { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string Body { get; set; } = null!;

    public DateTime Creation { get; set; }

    public long SenderId { get; set; }

    public long RecipientId { get; set; }

    public bool Deleted { get; set; }

    public virtual ICollection<AdministrativeMessageFile> AdministrativeMessageFiles { get; set; } = new List<AdministrativeMessageFile>();

    public virtual Administrative Recipient { get; set; } = null!;

    public virtual Administrative Sender { get; set; } = null!;
}
