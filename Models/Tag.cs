using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class Tag
{
    public string TagId { get; set; } = null!;

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
