
using Microsoft.EntityFrameworkCore;
using study4_be.Interface;
using study4_be.Models;
using study4_be.Services.Request;
using study4_be.Services.Request.Document;
using study4_be.Services.Request.Rating;
using study4_be.Services.Response;

namespace study4_be.Services.Rating
{
    public class DocumentService : IDocumentService
    {
        private readonly Study4Context _context;
        private readonly FireBaseServices _fireBaseServices;

        public DocumentService(Study4Context context, FireBaseServices fireBaseServices)
        {
            _context = context;
            _fireBaseServices = fireBaseServices;
        }
        public async Task<(bool success, string message, List<DocumentResponse> documents)> GetDocumentsByCourseAsync(int courseId)
        {
            if (courseId <= 0)
            {
                return (false, $"Course id is invalid: {courseId}", null);
            }

            try
            {
                // Check if the course exists
                var courseExist = await _context.Courses
                    .Where(c => c.CourseId == courseId)
                    .FirstOrDefaultAsync();

                if (courseExist == null)
                {
                    return (false, $"Course with Id {courseId} does not exist.", null);
                }

                var docByCourse = await _context.Documents
                    .Where(d => d.CourseId == courseId)
                    .Join(_context.Users,
                        doc => doc.UserId,
                        user => user.UserId,
                        (doc, user) => new
                        {
                            documentId = doc.DocumentId,
                            price = doc.Price,
                            downloadCount = doc.DownloadCount,
                            title = doc.Title,
                            isPublic = doc.IsPublic,
                            uploadDate = doc.UploadDate,
                            fileType = doc.FileType,
                            thumbnailUrl = doc.ThumbnailUrl,
                            userId = user.UserId,
                            userName = user.UserName,
                            userImage = user.UserImage,
                            documentDescription = doc.Description
                        })
                    .ToListAsync();

                var documentResponse = docByCourse.Select(c => new DocumentResponse
                {
                    documentId = c.documentId,
                    downloadCount = c.downloadCount,
                    title= c.title,
                    isPublic = c.isPublic,
                    fileType = c.fileType,
                    thumbnailUrl = c.thumbnailUrl,
                    userId = c.userId,
                    userName = c.userName,
                    userImage = c.userImage,
                    documentDescription = c.documentDescription,
                }).ToList();

                return (true, "Get All Documents Successful", documentResponse);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

    }
}
