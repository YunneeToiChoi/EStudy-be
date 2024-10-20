
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Interface;
using study4_be.Models;
using study4_be.Models.DTO;
using study4_be.Services.Course;
using study4_be.Services.Document;
using study4_be.Services;
using study4_be.Services.Rating;
using study4_be.Services.User;
using System.Drawing.Imaging;

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

        public async Task<List<DocumentDto>> GetDocumentsByCourseAsync(OfCourseIdRequest _req)
        {
            try
            {
                // Check if the course exists
                var courseExist = await _context.Courses.AnyAsync(c => c.CourseId == _req.courseId);
                if (!courseExist)
                {
                    return null; // return null to indicate the course does not exist
                }

                var docByCourse = await _context.Documents
                    .Where(d => d.CourseId == _req.courseId)
                    .Join(_context.Users,
                          doc => doc.UserId,
                          user => user.UserId,
                          (doc, user) => new DocumentDto
                          {
                              documentId = doc.DocumentId,
                              downloadCount = doc.DownloadCount,
                              title = doc.Title,
                              isPublic = doc.IsPublic,
                              fileType = doc.FileType,
                              thumbnailUrl = doc.ThumbnailUrl,
                              userId = user.UserId,
                              userName = user.UserName,
                              userImage = user.UserImage,
                              documentDescription = doc.Description
                          })
                    .ToListAsync();

                return docByCourse; // return the list of documents
            }
            catch (Exception)
            {
                return null; // return null on error
            }
        }
        public async Task<IActionResult> GetDocumentIdAsync(string orderId)
        {
            var existingOrder = await _context.Orders.FindAsync(orderId);

            if (existingOrder == null)
            {
                throw new KeyNotFoundException("Order not found");
            }

            var existingDocument = await _context.Documents.FindAsync(existingOrder.DocumentId);
            if (existingDocument == null)
            {
                throw new KeyNotFoundException("Document not found");
            }

            var respon = new
            {
                documentId = existingDocument.DocumentId,
            };

            return new OkObjectResult(respon);
        }
        public async Task<IActionResult> GetDocumentsFromUserAsync(string userId)
        {
            var userDocument = await _context.UserDocuments
                .Where(d => d.UserId == userId)
                .ToListAsync();
            if (userDocument == null || !userDocument.Any())
            {
                throw new KeyNotFoundException("User didn't buy any documents");
            }
            var documentIds = userDocument.Select(d => d.DocumentId).ToList();

            var documents = await _context.Documents
                .Where (d => documentIds.Contains(d.DocumentId))
                .Select(d => new {d.DocumentId, d.Title, d.FileUrl })
                .ToListAsync();
           
            return new OkObjectResult(new {documents});
        }
        public async Task<IEnumerable<UserDocumentResponse>> GetDocumentsByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            var user = await _context.Users
                                      .SingleOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                throw new KeyNotFoundException("User ID does not exist.");
            }

            var userDocs = await _context.Documents
                           .Where(d => d.UserId == user.UserId)
                           .Include(d => d.Category)
                           .Include(d => d.Course)
                           .ToListAsync();

            return userDocs.Select(doc => new UserDocumentResponse
            {
                documentId = doc.DocumentId,
                userId = doc.UserId,
                categoryId = doc.CategoryId,
                categoryName = doc.Category?.CategoryName ?? string.Empty,
                courseId = doc.CourseId,
                courseName = doc.Course?.CourseName ?? string.Empty,
                documentDescription = doc.Description,
                thumbnailUrl = doc.ThumbnailUrl,
                documentSize = doc.DocumentSize,
                downloadCount = doc.DownloadCount,
                fileType = doc.FileType,
                fileUrl = doc.FileUrl,
                isPublic = doc.IsPublic,
                price = doc.Price,
                title = doc.Title,
                uploadDate = doc.UploadDate,
            }).ToList();
        }
        public async Task<IActionResult> DownloadDocumentAsync(int documentId, string userId)
        {
            try
            {
                // Check if the document exists in the database
                var document = await _context.Documents.FindAsync(documentId);
                if (document == null)
                {
                    return new NotFoundObjectResult($"Document with Id {documentId} does not exist.");
                }

                if (document.Price <= 0)
                {
                    var newUserDocument = new UserDocument
                    {
                        DocumentId = documentId,
                        UserId = userId,
                        OrderDate = DateTime.Now,
                        State = true,
                    };
                    await _context.UserDocuments.AddAsync(newUserDocument);
                    await _context.SaveChangesAsync();
                }
                // Check if the document is free or the price is 0
                if (document.Price > 0)
                {
                    var existingUserDocument = await _context.UserDocuments.FindAsync(userId, documentId);
                    if (existingUserDocument == null) 
                    {
                        // Nếu cần kiểm tra giá, bạn có thể thêm logic ở đây
                        return new BadRequestObjectResult("Document is not free. Please pay to download.");
                    }
                }
                // Increment the download count
                document.DownloadCount++;
                _context.Documents.Update(document);
                await _context.SaveChangesAsync();
                
                // Get the file URL from Firebase (assuming documents are stored in Firebase)
                var fileUrl = document.FileUrl;

                if (string.IsNullOrEmpty(fileUrl))
                {
                    return new BadRequestObjectResult("File URL is not valid.");
                }

                // Redirect to the Firebase file URL for download
                return new OkObjectResult(new { status = 200, fileUrl });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"An error occurred while downloading the document: {ex.Message}");
            }
        }
        public async Task<CourseResponse> GetCoursesByUserIdAsync(string userId)
        {
            var user = await _context.Users
                                      .Where(u => u.UserId == userId)
                                      .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new ArgumentException("User id is not valid");
            }

            var courseFree = await _context.Courses
                                           .Where(c => c.CoursePrice == 0)
                                           .ToListAsync();

            var courseOfUser_Paid = await _context.UserCourses
                                                   .Where(uc => uc.UserId == userId)
                                                   .Select(uc => uc.Course)
                                                   .ToListAsync();

            return new CourseResponse
            {
                FreeCourses = courseFree,
                PaidCourses = courseOfUser_Paid
            };
        }
        public async Task<IActionResult> UploadDocuments(UploadDocumentRequest _req)
        {
            if (_req.files == null || !_req.files.Any())
            {
                return new BadRequestObjectResult("No files uploaded.");
            }

            try
            {
                var userExist = await _context.Users.AnyAsync(u => u.UserId == _req.userId);
                if (!userExist)
                {
                    return new BadRequestObjectResult("User does not exist.");
                }

                var uploadedFiles = new List<object>();
                foreach (var file in _req.files)
                {
                    if (file == null || file.Length == 0)
                    {
                        continue; // Skip empty files
                    }

                    var fileName = file.FileName;
                    var fileSizeInBytes = file.Length;
                    var fileSizeReadable = ConvertFileSize(fileSizeInBytes);
                    var fileExtension = Path.GetExtension(fileName);

                    using (var fileStream = file.OpenReadStream())
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await fileStream.CopyToAsync(memoryStream);
                            memoryStream.Seek(0, SeekOrigin.Begin);

                            var fileUrl = await _fireBaseServices.UploadFileDocAsync(memoryStream, fileName, _req.userId);

                            string thumbnailUrl = null;
                            string extractedPdfUrl = null;

                            if (fileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                            {
                                using (var thumbnailStream = new MemoryStream())
                                {
                                    memoryStream.Seek(0, SeekOrigin.Begin);
                                    await memoryStream.CopyToAsync(thumbnailStream);
                                    thumbnailStream.Seek(0, SeekOrigin.Begin);

                                    using (var firstPageImage = ExtractFirstPageAsImage(thumbnailStream))
                                    {
                                        if (firstPageImage != null)
                                        {
                                            firstPageImage.Seek(0, SeekOrigin.Begin);
                                            var thumbnailFileName = Path.GetFileNameWithoutExtension(fileName) + "_thumbnail.jpg";
                                            thumbnailUrl = await _fireBaseServices.UploadFileDocAsync(firstPageImage, thumbnailFileName, _req.userId);
                                            // trang 2, 3 - > 5
                                            // _temp 
                                        }
                                    }
                                }

                                // Extract the first 7 pages of the PDF and upload them
                                using (var extractedPdfStream = ExtractFirst7PagesAsPdf(memoryStream))
                                {
                                    if (extractedPdfStream != null)
                                    {
                                        var extractedPdfFileName = Path.GetFileNameWithoutExtension(fileName) + "_extracted.pdf";
                                        extractedPdfUrl = await _fireBaseServices.UploadFileDocAsync(extractedPdfStream, extractedPdfFileName, _req.userId);
                                    }
                                }
                            }

                            var userDoc = new Models.Document
                            {
                                UserId = _req.userId,
                                DownloadCount = 0,
                                FileType = fileExtension,
                                FileUrl = fileUrl,
                                ThumbnailUrl = thumbnailUrl,
                                Title = fileName,
                                UploadDate = DateTime.UtcNow,
                                DocumentSize = fileSizeInBytes
                            };

                            await _context.Documents.AddAsync(userDoc);
                            await _context.SaveChangesAsync();

                            uploadedFiles.Add(new
                            {
                                DocumentId = userDoc.DocumentId,
                                DocumentName = fileName,
                                FileSize = fileSizeReadable,
                                FileUrl = fileUrl,
                                ThumbnailUrl = thumbnailUrl,
                                ExtractedPdfUrl = extractedPdfUrl
                            });
                        }
                    }
                }

                return new OkObjectResult(new { status = 200, Files = uploadedFiles });
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult($"Error occurred: {e.Message}");
            }
        }

        private Stream ExtractFirstPageAsImage(Stream pdfStream)
        {
            using (var pdfDocument = PdfiumViewer.PdfDocument.Load(pdfStream))
            {
                if (pdfDocument.PageCount < 1)
                {
                    return null; // No pages in the PDF
                }

                using (var firstPageImage = pdfDocument.Render(0, 300, 300, true))
                {
                    var memoryStream = new MemoryStream();
                    firstPageImage.Save(memoryStream, ImageFormat.Jpeg);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return memoryStream;
                }
            }
        }
        private Stream ExtractFirst7PagesAsPdf(Stream originalPdfStream)
        {
            // Load the original PDF
            var inputDocument = PdfSharp.Pdf.IO.PdfReader.Open(originalPdfStream, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);

            // Create a new PDF document to hold the extracted pages
            var outputDocument = new PdfSharp.Pdf.PdfDocument();

            // Copy up to 7 pages from the original PDF into the new PDF
            int pagesToExtract = Math.Min(7, inputDocument.PageCount);
            for (int i = 0; i < pagesToExtract; i++)
            {
                outputDocument.AddPage(inputDocument.Pages[i]);
            }

            // Save the new PDF to a MemoryStream
            var outputPdfStream = new MemoryStream();
            outputDocument.Save(outputPdfStream, false);
            outputPdfStream.Seek(0, SeekOrigin.Begin);

            return outputPdfStream;
        }

        private string ConvertFileSize(long fileSizeInBytes)
        {
            if (fileSizeInBytes < 1024)
                return $"{fileSizeInBytes} B";
            else if (fileSizeInBytes < 1024 * 1024)
                return $"{fileSizeInBytes / 1024.0:F2} KB";
            else
                return $"{fileSizeInBytes / (1024.0 * 1024.0):F2} MB";
        }
        public async Task<IActionResult> UploadDetail(UploadDetailRequest request)
        {
            if (request.idDocuments == null || !request.idDocuments.Any())
            {
                return new BadRequestObjectResult("No Document IDs uploaded.");
            }

            try
            {
                var userExists = await _context.Users.AnyAsync(u => u.UserId == request.userId);
                if (!userExists)
                {
                    return new BadRequestObjectResult("User does not exist.");
                }

                // Query documents by idDocuments and update information
                var documentsToUpdate = await _context.Documents
                    .Where(d => request.idDocuments.Contains(d.DocumentId) && d.UserId == request.userId)
                    .ToListAsync();

                if (!documentsToUpdate.Any())
                {
                    return new BadRequestObjectResult("No matching documents found for this user.");
                }

                // Update information for each document
                foreach (var document in documentsToUpdate)
                {
                    document.CategoryId = request.categoryId;
                    document.CourseId = request.courseId;
                    document.IsPublic = request.state;
                    document.Description = request.description;
                    document.Price = request.price;
                    document.Title = request.title;
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                return new OkObjectResult(new { status = 200, message = "Documents updated successfully." });
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult($"Error occurred: {e.Message}");
            }
        }
        public async Task<DocumentDetailDto> GetDocumentDetailAsync(int documentId)
        {
            // Truy vấn tài liệu theo documentId
            var document = await _context.Documents
                .Include(d => d.User) // Bao gồm thông tin người dùng
                .FirstOrDefaultAsync(d => d.DocumentId == documentId);

            // Kiểm tra tài liệu tồn tại
            if (document == null)
            {
                throw new Exception("Document not found.");
            }

            // Tính số lượng tài liệu mà người dùng đã đăng tải
            var userDocumentCount = await _context.Documents
                .CountAsync(d => d.UserId == document.UserId); // Số tài liệu mà người dùng đã đăng

            // Tính số lượng lượt tải tài liệu
            var userDownloadCount = await _context.Documents
                .Where(d => d.DocumentId == documentId)
                .Select(d => d.DownloadCount ?? 0)
                .FirstOrDefaultAsync();

            return new DocumentDetailDto
            {
                documentId = document.DocumentId,
                title = document.Title,
                documentDescription = document.Description,
                fileUrl = document.FileUrl,
                uploadDate = document.UploadDate,
                fileType = document.FileType,
                documentPublic = document.IsPublic,
                downloadCount = document.DownloadCount,
                price = document.Price,
                userDocumentCount = userDocumentCount, // Số tài liệu mà người dùng đã đăng tải
                userDownloadCount = userDownloadCount,   // Số lượt tải của tài liệu này
                user = new UserDto
                {
                    UserId = document.User.UserId,
                    UserName = document.User.UserName,
                    UserEmail = document.User.UserEmail,
                    UserImage = document.User.UserImage
                }
            };
        }
    }
}
