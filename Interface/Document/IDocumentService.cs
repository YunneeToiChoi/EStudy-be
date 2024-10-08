using Microsoft.AspNetCore.Mvc;
using study4_be.Models.DTO;
using study4_be.Services.Request.Course;
using study4_be.Services.Request.Document;
using study4_be.Services.Response;
using study4_be.Services.Response.Course;
using study4_be.Services.Response.User;

namespace study4_be.Interface
{
    public interface IDocumentService
    {
        Task<List<DocumentDto>> GetDocumentsByCourseAsync(OfCourseIdRequest request);
        Task<IEnumerable<UserDocumentResponse>> GetDocumentsByUserIdAsync(string userId);
        Task<IActionResult> DownloadDocumentAsync(int documentId);
        Task<CourseResponse> GetCoursesByUserIdAsync(string userId);
        Task<IActionResult> UploadDocuments(UploadDocumentRequest request);
        Task<IActionResult> UploadDetail(UploadDetailRequest request);
        Task<DocumentDetailDto> GetDocumentDetailAsync(int documentId);
    }
}
