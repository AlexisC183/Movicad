using System;
using System.Collections.Generic;

namespace Movicad.Persistence;

public partial class User
{
    public long UserId { get; set; }

    public string Key { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime Creation { get; set; }

    public bool Deleted { get; set; }

    public virtual Administrative? Administrative { get; set; }

    public virtual ICollection<ForumMessage> ForumMessages { get; set; } = new List<ForumMessage>();

    public virtual Student? Student { get; set; }

    public virtual ICollection<Administrative> Administratives { get; set; } = new List<Administrative>();
}
