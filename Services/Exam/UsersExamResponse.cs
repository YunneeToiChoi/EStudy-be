namespace study4_be.Services.Exam;

public class UsersExamResponse
{
    public string UserId { get; set; } = null!;

    public string UserExamId { get; set; } = null!;

    public string ExamId { get; set; } = null!;

    public DateTime? DateTime { get; set; }

    public bool? State { get; set; }

    public int? Score { get; set; }

    public int? UserTime { get; set; }
    public string? ExamName { get; set; }
    public string? ExamImage {get; set;}
}