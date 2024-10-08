using study4_be.Services.Response;

namespace study4_be.Interface
{
    public interface IDocumentService
    {
        Task<(bool success, string message, List<DocumentResponse> documents)> GetDocumentsByCourseAsync(int courseId);
    }
}
