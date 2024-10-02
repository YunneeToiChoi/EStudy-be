using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class Video
{
    public int VideoId { get; set; }

    public int? LessonId { get; set; }

    public string VideoUrl { get; set; } = null!;

    public virtual Lesson? Lesson { get; set; }
}
