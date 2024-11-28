using Newtonsoft.Json;

namespace study4_be.Services.Exam;

public class OutstandingExamResponse
{
    public List<ExamPreviewDetails> OutstandingExams { get; set; } = new();
    public string Message { get; set; } = "Success"; // Default success message
    public int StatusCode { get; set; } = 200;
    public string Error { get; set; } // Error message, if any
}