using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace study4_be.Models;

public partial class Exam
{
    [Required]
    public string ExamId { get; set; } = null!;

    public string? ExamAudio { get; set; }

    public string? ExamImage { get; set; }

    public string? ExamName { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<UsersExam> UsersExams { get; set; } = new List<UsersExam>();
}
