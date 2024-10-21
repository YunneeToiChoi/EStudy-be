using Microsoft.AspNetCore.Mvc;
using study4_be.Models;
using study4_be.Models.DTO;
using study4_be.Services.Course;
using study4_be.Services.Document;
using study4_be.Services.User;

namespace study4_be.Interface
{
    public interface IDocumentService
    {
        Task<List<DocumentDto>> GetDocumentsByCourseAsync(OfCourseIdRequest request);
        Task<IEnumerable<UserDocumentResponse>> GetDocumentsByUserIdAsync(string userId);
        Task<IActionResult> GetDocumentsFromUserAsync(string  userId);
        Task<IActionResult> DownloadDocumentAsync(int documentId, string userId);
        Task<IActionResult> GetDocumentIdAsync(string orderId);
        Task<CourseResponse> GetCoursesByUserIdAsync(string userId);
        Task<IActionResult> UploadDocuments(UploadDocumentRequest request);
        Task<IActionResult> UploadDetail(UploadDetailRequest request);
        Task<DocumentDetailDto> GetDocumentDetailAsync(int documentId);

    }
}
