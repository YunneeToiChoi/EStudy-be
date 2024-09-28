using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace study4_be.Models;

public partial class Unit
{
    [Required]
    public int UnitId { get; set; }

    public int? CourseId { get; set; }

    public string? UnitTittle { get; set; }

    public int? Process { get; set; }

    public virtual ICollection<Container> Containers { get; set; } = new List<Container>();

    public virtual Course? Course { get; set; }
}
