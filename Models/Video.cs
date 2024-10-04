using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace study4_be.Models;

public partial class Video
{
    [JsonRequired]
    public int VideoId { get; set; }

    public int? LessonId { get; set; }

    public string? VideoUrl { get; set; }

    public virtual Lesson? Lesson { get; set; }
}
