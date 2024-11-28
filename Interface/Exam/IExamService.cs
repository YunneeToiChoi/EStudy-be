using study4_be.Models;
using study4_be.Services.Exam;

namespace study4_be.Interface.Exam;

public interface IExamService
{
    Task<IEnumerable<ExamPreviewDetails>> GetAllExamsAsync();
    Task<IEnumerable<UsersExamResponse>> GetUserExamsAsync(string userId);
}