namespace study4_be.Services.Exam;

public class ExamPreviewDetails
{
    public string ExamId { get; set; }
    public string ExamName { get; set; }
    public string ExamImage { get; set; }
    public int TotalMinutes { get; set; } // Duration of the exam
    public int TotalComments { get; set; } // Count of comments for the exam
    public int TotalUsersTest { get; set; }
}