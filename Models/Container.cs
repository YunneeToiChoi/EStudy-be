using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace study4_be.Models;

public partial class Container
{
    [Required]
    public int ContainerId { get; set; }
    [Required(ErrorMessage = "Tiêu đề container là bắt buộc.")]
    public string? ContainerTitle { get; set; }

    public int? UnitId { get; set; }

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual Unit? Unit { get; set; }
}
