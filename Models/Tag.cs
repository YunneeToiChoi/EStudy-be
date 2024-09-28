using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace study4_be.Models;

public partial class Tag
{
    [Required]
    public string TagId { get; set; } = null!;

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
