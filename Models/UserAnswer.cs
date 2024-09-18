using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class UserAnswer
{
    public long UserAnswerId { get; set; }

    public string UserExamId { get; set; } = null!;

    public int QuestionId { get; set; }

    public string? Answer { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual UsersExam UserExam { get; set; } = null!;
}
