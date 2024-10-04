using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace study4_be.Models;

public partial class Container
{
    [JsonRequired]
    public int ContainerId { get; set; } 

    public string? ContainerTitle { get; set; }

    public int? UnitId { get; set; }

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual Unit? Unit { get; set; }
}
