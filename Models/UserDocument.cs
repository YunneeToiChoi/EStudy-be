using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class UserDocument
{
    public string UserId { get; set; } = null!;

    public int DocumentId { get; set; }

    public DateTime OrderDate { get; set; }

    public bool State { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
