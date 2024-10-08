﻿using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class Lesson
{
    public int LessonId { get; set; }

    public string LessonType { get; set; } = null!;

    public string LessonTitle { get; set; } = null!;

    public int? ContainerId { get; set; }

    public string? TagId { get; set; }

    public virtual Container? Container { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual Tag? Tag { get; set; }

    public virtual ICollection<Video> Videos { get; set; } = new List<Video>();

    public virtual ICollection<Vocabulary> Vocabularies { get; set; } = new List<Vocabulary>();
}
