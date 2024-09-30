﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services;
using study4_be.Services.Request;
using study4_be.Services.Response;
using study4_be.Services.Request.Document;
using study4_be.Validation;
using System.Collections.Immutable;
using PdfSharpCore.Pdf;
using System.Drawing.Imaging;
using PdfiumViewer;

namespace study4_be.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserDocumentAPIController : Controller
    {
        private readonly UserRepository _userRepository;
        private Study4Context _context;
        private UserRegistrationValidator _userRegistrationValidator;
        private readonly FireBaseServices _fireBaseServices;

        public UserDocumentAPIController(FireBaseServices fireBaseServices, Study4Context context)
        {
            _fireBaseServices = fireBaseServices;
            _context = context;
            _userRepository = new(context);
            _userRegistrationValidator = new(_userRepository);
        }
        //################################################### DOCUMENT ##########################################//

        [HttpPost("GetDocByCourse")]
        public async Task<IActionResult> GetDocByCourse(OfCourseIdRequest _req)
        {
            if (_req.courseId == null || _req.courseId <= 0)
            {
                return BadRequest($"Course id is invalid: {_req.courseId}");
            }

            try
            {
                // Check if the course exists
                var courseExist = await _context.Courses.Where(c => c.CourseId == _req.courseId).FirstOrDefaultAsync();
                if (courseExist != null)
                {
                 
                    var docByCourse = await _context.Documents
                        .Where(d => d.CourseId == _req.courseId)
                        .Join(_context.Users,
                              doc => doc.UserId,
                              user => user.UserId,
                              (doc, user) => new
                              {
                                  documentId = doc.DocumentId,
                                  documentPrice = doc.Price,
                                  documentTotalDownload = doc.DownloadCount,
                                  documentName = doc.Title,
                                  documentPublic = doc.IsPublic,
                                  documentUploadDate = doc.UploadDate,
                                  documentType = doc.FileType,
                                  thumbnailUrl = doc.ThumbnailUrl,
                                  userId = user.UserId,
                                  userName = user.UserName,       
                                  userImage = user.UserImage    
                              })
                        .ToListAsync();

                    // Build the response
                    var documentResponse = docByCourse.Select(c => new
                    {
                        documentId = c.documentId,
                        documentTotalDownload = c.documentTotalDownload,
                        documentName = c.documentName,
                        documentPublic = c.documentPublic,
                        thumbnailUrl = c.thumbnailUrl,
                        userId = c.userId,
                        userName = c.userName,            
                        userImage = c.userImage           
                    }).ToList();

                    return Ok(new
                    {
                        status = 200,
                        message = "Get All Documents Successful",
                        documents = documentResponse
                    });
                }
                else
                {
                    return NotFound($"Course with Id {_req.courseId} does not exist.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //################################################### DOCUMENT ##########################################//

        //################################################### BASE #############################################//
        [HttpGet("GetAllCate")]
        public async Task<IActionResult> GetAllCate()
        {
            try
            {
                var cate = await _context.Categories.ToListAsync();
                var cateResponse = cate.Select(c => new
                {
                    categoryId = c.CategoryId,
                    categoryName = c.CategoryName
                }).ToList();
                return Ok(new
                {
                    status = 200,
                    message = "Get All Cate Successful",
                    category = cateResponse
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetAllCourse")]
        public async Task<IActionResult> GetAllCourse()
        {
            try
            {
                var course = await _context.Courses.ToListAsync();
                var courseResponse = course.Select(c => new
                {
                    courseId = c.CourseId,
                    courseName = c.CourseName
                }).ToList();
                return Ok(new
                {
                    status = 200,
                    message = "Get All Courses Successful",
                    course = courseResponse
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetAllDoc")]
        public async Task<IActionResult> GetAllDoc()
        {
            try
            {
                var document = await _context.Documents.ToListAsync();
                var documentResponse = document.Select(c => new
                {
                    documentId = c.DocumentId,
                    documentName = c.Title,
                    documentPublic = c.IsPublic,
                    documentFileUrl = c.FileUrl,
                    documentType= c.FileType,
                    documentUploadDate= c.UploadDate,
                    documentPrice = c.Price,
                    documentDescription = c.Description,
                    thumbnailUrl = c.ThumbnailUrl

                }).ToList();
                return Ok(new
                {
                    status = 200,
                    message = "Get All Document Successful",
                    document = documentResponse
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("GetDocByUser")]
        public async Task<IActionResult> GetDocByUser([FromBody] OfUserIdRequest _req)
        {
            try
            {
                var user = await _context.Users
                                          .Where(u => u.UserId == _req.userId)
                                          .SingleOrDefaultAsync();

                if (user != null)
                {
                    var userDocs = await _context.Documents
                               .Where(d => d.UserId == user.UserId)
                               .Include(d => d.Category) 
                               .Include(d => d.Course)   // Eager load Course
                               .ToListAsync();

                    var userDocResponses = userDocs.Select(doc => new UserDocumentResponse
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

                    return Ok(new
                    {
                        status = 200,
                        userDoc = userDocResponses
                    });
                }
                else
                {
                    return BadRequest("User ID is not valid or does not exist.");
                }
            }
            catch (Exception e)
            {
                return BadRequest($"Error occurred: {e.Message}");
            }
        }

        //################################################### BASE #############################################//
        //################################################### UPLOAD ##########################################//
        [HttpPost("GetCourseOfUser")]
        public async Task<IActionResult> GetCourseOfUser([FromBody] OfUserIdRequest _req) // course free and course bought -> name id 
        {
            try
            {
                var user = await _context.Users
                                          .Where(u => u.UserId == _req.userId)
                                          .FirstOrDefaultAsync();

                if (user != null)
                {
                    // Lấy danh sách khóa học miễn phí
                    var courseFree = await _context.Courses
                                                          .Where(c => c.CoursePrice == 0)
                                                          .ToListAsync();

                    // Lấy danh sách khóa học đã thanh toán của người dùng
                    var courseOfUser_Paied = await _context.UserCourses
                                                           .Where(uc => uc.UserId == _req.userId)
                                                           .Select(uc => uc.Course)
                                                           .ToListAsync();

                    // Tạo đối tượng để trả về kết quả
                    var courseResponse = new
                    {
                        FreeCourses = courseFree,
                        PaidCourses = courseOfUser_Paied
                        // missing other 
                    };

                    return Ok(new
                    {
                        status = 200,
                        course = courseResponse
                    });
                }
                else
                {
                    return BadRequest("User id is not valid");
                }
            }
            catch (Exception e)
            {
                return BadRequest($"Error occurred: {e.Message}");
            }
        }
        [HttpPost("Upload")]
        public async Task<IActionResult> UploadDocuments(UploadDocumentRequest _req)
        {
            if (_req.files == null || !_req.files.Any())
            {
                return BadRequest("No files uploaded.");
            }

            try
            {
                var userExist = await _context.Users.AnyAsync(u => u.UserId == _req.userId);
                if (!userExist)
                {
                    return BadRequest("User does not exist.");
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
                        // Read the entire file stream into a MemoryStream
                        using (var memoryStream = new MemoryStream())
                        {
                            await fileStream.CopyToAsync(memoryStream);
                            memoryStream.Seek(0, SeekOrigin.Begin); // Reset the position to the beginning for the upload

                            // Upload the document to Firebase
                            var fileUrl = await _fireBaseServices.UploadFileDocAsync(memoryStream, fileName, _req.userId);

                            // Initialize a thumbnail URL (in case it's not a PDF)
                            string thumbnailUrl = null;

                            // Check if the file is a PDF
                            if (fileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                            {
                                // Extract first page as image
                                using (var thumbnailStream = new MemoryStream())
                                {
                                    // Copy memoryStream to thumbnailStream for thumbnail extraction
                                    memoryStream.Seek(0, SeekOrigin.Begin); // Ensure memoryStream is at the beginning
                                    await memoryStream.CopyToAsync(thumbnailStream);
                                    thumbnailStream.Seek(0, SeekOrigin.Begin); // Reset thumbnailStream position

                                    // Extract the first page image from the PDF stream
                                    using (var firstPageImage = ExtractFirstPageAsImage(thumbnailStream))
                                    {
                                        if (firstPageImage != null)
                                        {
                                            // Ensure the position of firstPageImage is reset before upload
                                            firstPageImage.Seek(0, SeekOrigin.Begin);

                                            // Upload thumbnail to Firebase
                                            var thumbnailFileName = Path.GetFileNameWithoutExtension(fileName) + "_thumbnail.jpg";
                                            thumbnailUrl = await _fireBaseServices.UploadFileDocAsync(firstPageImage, thumbnailFileName, _req.userId);
                                        }
                                    }
                                }
                            }

                            // Save document data to the database
                            var userDoc = new Document
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

                            // Add both FileUrl and DocumentId to the response
                            uploadedFiles.Add(new
                            {
                                DocumentId = userDoc.DocumentId,
                                DocumentName = fileName,
                                FileSize = fileSizeReadable,
                                FileUrl = fileUrl,
                                ThumbnailUrl = thumbnailUrl
                            });
                        }
                    }
                }

                return Ok(new { status = 200, Files = uploadedFiles });
            }
            catch (Exception e)
            {
                return BadRequest($"Error occurred: {e.Message}");
            }
        }

        private Stream ExtractFirstPageAsImage(Stream pdfStream)
        {
            // Open the PDF document using PdfiumViewer
            using (var pdfDocument = PdfiumViewer.PdfDocument.Load(pdfStream))
            {
                if (pdfDocument.PageCount < 1)
                {
                    return null; // No pages in the PDF
                }

                // Render the first page of the PDF to a bitmap
                using (var firstPageImage = pdfDocument.Render(0, 300, 300, true)) // 300 DPI
                {
                    // Convert Bitmap to MemoryStream
                    var memoryStream = new MemoryStream();
                    firstPageImage.Save(memoryStream, ImageFormat.Jpeg); // Save the image as JPEG
                    memoryStream.Seek(0, SeekOrigin.Begin); // Reset stream position for reading
                    return memoryStream; // Return the stream containing the JPEG image
                }
            }
        }


        // Helper method to convert file size to KB or MB
        private string ConvertFileSize(long fileSizeInBytes)
        {
            if (fileSizeInBytes < 1024)
                return $"{fileSizeInBytes} B";
            else if (fileSizeInBytes < 1024 * 1024)
                return $"{fileSizeInBytes / 1024.0:F2} KB"; // Convert to KB
            else
                return $"{fileSizeInBytes / (1024.0 * 1024.0):F2} MB"; // Convert to MB
        }
        [HttpPost("Detail")]
        public async Task<IActionResult> Detail(UploadDetailRequest _req)
        {
            if (_req.idDocuments == null || !_req.idDocuments.Any())
            {
                return BadRequest("No Documents Id uploaded.");
            }

            try
            {
                var userExist = await _context.Users.AnyAsync(u => u.UserId == _req.userId);
                if (!userExist)
                {
                    return BadRequest("User does not exist.");
                }
                // Truy vấn các document theo idDocuments và cập nhật thông tin
                var documentsToUpdate = await _context.Documents
                                        .Where(d => _req.idDocuments.Contains(d.DocumentId) && d.UserId == _req.userId)
                                        .ToListAsync();

                if (!documentsToUpdate.Any())
                {
                    return BadRequest("No matching documents found for this user.");
                }

                // Cập nhật thông tin cho từng document
                foreach (var document in documentsToUpdate)
                {
                    document.CategoryId = _req.categoryId;
                    document.CourseId = _req.courseId;
                    document.IsPublic = _req.state;
                    document.Description = _req.description;
                    document.Price = _req.price;
                    document.Title = _req.title;
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();

                return Ok(new { status = 200, message = "Documents updated successfully.",  });
            }
            catch (Exception e)
            {
                return BadRequest($"Error occurred: {e.Message}");
            }
        }
        //################################################### UPLOAD ##########################################//

    }
}
