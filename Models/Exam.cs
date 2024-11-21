using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class Exam
{
    public string ExamId { get; set; } = null!;

    public string ExamAudio { get; set; } = null!;

    public string ExamImage { get; set; } = null!;

    public string ExamName { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<UsersExam> UsersExams { get; set; } = new List<UsersExam>();
}
