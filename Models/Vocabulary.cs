using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace study4_be.Models;

public partial class Vocabulary
{
    [Required]
    public int VocabId { get; set; }

    public string? VocabType { get; set; }

    public string? Mean { get; set; }

    public string? Example { get; set; }

    public string? Explanation { get; set; }

    public string? AudioUrlUs { get; set; }

    public string? AudioUrlUk { get; set; }

    public int? LessonId { get; set; }

    public string? VocabTitle { get; set; }

    public virtual Lesson? Lesson { get; set; }
}
