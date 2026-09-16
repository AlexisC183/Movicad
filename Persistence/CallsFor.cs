using System;
using System.Collections.Generic;

namespace Movicad.Persistence;

public partial class CallsFor
{
    public long CallForId { get; set; }

    public string Key { get; set; } = null!;

    public string Title { get; set; } = null!;

    public DateTime? InitialDate { get; set; }

    public DateTime? FinalDate { get; set; }

    public string Description { get; set; } = null!;

    public string Requirements { get; set; } = null!;

    public DateTime PublishDate { get; set; }

    public DateTime Modification { get; set; }

    public long AdministrativeId { get; set; }

    public bool Deleted { get; set; }

    public virtual Administrative Administrative { get; set; } = null!;

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<DestinationCountry> DestinationCountries { get; set; } = new List<DestinationCountry>();

    public virtual ICollection<FileRequest> FileRequests { get; set; } = new List<FileRequest>();

    public virtual ICollection<ForumFile> ForumFiles { get; set; } = new List<ForumFile>();

    public virtual ICollection<ForumMessage> ForumMessages { get; set; } = new List<ForumMessage>();

    public virtual ICollection<FrequentQuestion> FrequentQuestions { get; set; } = new List<FrequentQuestion>();

    public virtual ICollection<Link> Links { get; set; } = new List<Link>();
}
