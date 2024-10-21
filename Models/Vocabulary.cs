using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class Vocabulary
{
    public int VocabId { get; set; }

    public string VocabType { get; set; } = null!;

    public string Mean { get; set; } = null!;

    public string Example { get; set; } = null!;

    public string Explanation { get; set; } = null!;

    public string? AudioUrlUs { get; set; }

    public string? AudioUrlUk { get; set; }

    public int LessonId { get; set; }

    public string VocabTitle { get; set; } = null!;

    public virtual Lesson? Lesson { get; set; } = null!;
}
