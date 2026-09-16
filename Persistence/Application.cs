using System;
using System.Collections.Generic;

namespace Movicad.Persistence;

public partial class Application
{
    public long ApplicationId { get; set; }

    public long StudentId { get; set; }

    public long CallForId { get; set; }

    public bool Banned { get; set; }

    public bool Deleted { get; set; }

    public virtual CallsFor CallFor { get; set; } = null!;

    public virtual ICollection<ExpulsionReason> ExpulsionReasons { get; set; } = new List<ExpulsionReason>();

    public virtual ICollection<PrivateMessage> PrivateMessages { get; set; } = new List<PrivateMessage>();

    public virtual Student Student { get; set; } = null!;

    public virtual ICollection<StudentFile> StudentFiles { get; set; } = new List<StudentFile>();
}
