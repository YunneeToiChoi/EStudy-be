using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace study4_be.Models;

public partial class Video
{
    [Required]
    public int VideoId { get; set; }

    public int? LessonId { get; set; }

    public string? VideoUrl { get; set; }

    public virtual Lesson? Lesson { get; set; }
}
