using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace study4_be.Models;

public partial class Vocabulary
{
    [JsonRequired]
    public int VocabId { get; set; }

    public string VocabType { get; set; } = null!;

    public string Mean { get; set; } = null!;

    public string Example { get; set; } = null!;

    public string Explanation { get; set; } = null!;

    public string? AudioUrlUs { get; set; }

    public string? AudioUrlUk { get; set; }
    [JsonRequired]
    public int LessonId { get; set; }

    public string VocabTitle { get; set; } = null!;

    public virtual Lesson Lesson { get; set; } = null!;
}
