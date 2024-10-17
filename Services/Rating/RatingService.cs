
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PusherServer;
using SendGrid.Helpers.Errors.Model;
using study4_be.Interface.Rating;
using study4_be.Models;
using study4_be.Services.Course;
using study4_be.Services.Document;
using study4_be.Services.User;

namespace study4_be.Services.Rating
{
    public class RatingService : IRatingService
    {
        private readonly Study4Context _context;
        private readonly FireBaseServices _fireBaseServices;
        private readonly DateTimeService _dateService;
        private readonly PusherServer.Pusher _pusher;

        public RatingService(Study4Context context, 
            FireBaseServices fireBaseServices, 
            DateTimeService dateService,
            IOptions<PusherOptions> options)
        {
            _context = context;
            _fireBaseServices = fireBaseServices;
            _dateService = dateService;
            var pusherOptions = options.Value;
            _pusher = new PusherServer.Pusher(
            pusherOptions.AppId,
            pusherOptions.Key,
            pusherOptions.Secret,
            new PusherServer.PusherOptions
            {
                Cluster = pusherOptions.Cluster,
                Encrypted = true
            }
        );
        }
        //public async Task<object> SubmitRatingOrReplyAsync(RatingOrReplySubmitRequest request, List<IFormFile> ratingImages)
        //{
        //    // Kiểm tra người dùng
        //    var user = await _context.Users.FindAsync(request.userId);
        //    if (user == null) throw new NotFoundException("Người dùng không tồn tại.");

        //    // Bắt đầu giao dịch
        //    using (var transaction = await _context.Database.BeginTransactionAsync())
        //    {
        //        int referenceId;
        //        string referenceType;

        //        if (request.isRating)
        //        {
        //            referenceId = await CreateRating(request);
        //            referenceType = "RATING";
        //        }
        //        else
        //        {
        //            referenceId = await CreateReply(request);
        //            referenceType = "REPLY";
        //        }
        //        // Tải lên hình ảnh
        //        var imageUrls = await UploadImages(ratingImages, referenceId, request.rootId, referenceType);

        //        // Cam kết giao dịch
        //        await transaction.CommitAsync();

        //        // Trả về phản hồi
        //        return new
        //        {
        //            ReferenceId = referenceId,
        //            UserId = request.userId,
        //            ReferenceType = referenceType,
        //            RatingImages = imageUrls
        //        };
        //    }
        //}
        public async Task<object> SubmitRatingOrReplyAsync(RatingOrReplySubmitRequest request, List<IFormFile> ratingImages)
        {
            // Kiểm tra người dùng
            var user = await _context.Users.FindAsync(request.userId);
            if (user == null) throw new NotFoundException("Người dùng không tồn tại.");

            // Bắt đầu giao dịch
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    int referenceId;
                    string referenceType;
                    int? parentId = null; // Sửa thành nullable để tránh lỗi khi chưa có giá trị
                    int? rootId = null;
                    string channelId; // Biến để lưu trữ channelId dựa trên courseId hoặc documentId

                    if (request.isRating)
                    {
                        referenceId = await CreateRating(request);
                        referenceType = "RATING";
                    }
                    else
                    {
                        referenceId = await CreateReply(request);
                        referenceType = "REPLY";
                        parentId = request.parentReply; // Gán giá trị parentId từ request khi là reply
                        rootId = request.rootId;
                    }

                    // Tải lên hình ảnh
                    var imageUrls = ratingImages != null && ratingImages.Any()
                        ? await UploadImages(ratingImages, referenceId, rootId, referenceType)
                        : new List<string>();

                    // Xác định channelId theo courseId hoặc documentId
                    if (request.courseId.HasValue)
                    {
                        channelId = $"course_{request.courseId.Value}";
                    }
                    else if (request.documentId.HasValue)
                    {
                        channelId = $"document_{request.documentId.Value}";
                    }
                    else
                    {
                        throw new Exception("courseId hoặc documentId không hợp lệ.");
                    }

                    // Cam kết giao dịch
                    await transaction.CommitAsync();

                    // Lấy chi tiết user một lần để tránh multiple queries
                    var userDetail = await _context.Users
                        .Where(u => u.UserId == request.userId)
                        .Select(u => new { u.UserImage, u.UserName })
                        .FirstOrDefaultAsync();

                    // Phát sự kiện real-time tới các client qua Pusher
                    await _pusher.TriggerAsync(
                        channelId, // Channel theo courseId hoặc documentId
                        referenceType == "RATING" ? "new-rating" : "new-reply", // Event name
                        new
                        {
                            ReferenceId = referenceId,
                            UserId = request.userId,
                            UserImage = userDetail.UserImage,
                            UserName = userDetail.UserName,
                            RatingDate = DateTime.UtcNow, // Assuming it's created now
                            RatingReview = request.ratingReview ?? string.Empty, // Assuming request has a review text
                            ParentId = parentId, // Trả về parentId nếu là reply
                            RootId = rootId,
                            ReferenceType = referenceType,
                            RatingImages = imageUrls
                        });

                    // Trả về phản hồi
                    return new
                    {
                        ReferenceId = referenceId,
                        ParentId = parentId, // Trả về parentId nếu là reply
                        UserId = request.userId,
                        RootId = rootId,
                        ReferenceType = referenceType,
                        RatingImages = imageUrls
                    };
                }
                catch (Exception ex)
                {
                    // Rollback giao dịch nếu có lỗi
                    await transaction.RollbackAsync();
                    throw new Exception("Lỗi khi khởi tạo", ex);
                }
                finally
                {
                    await transaction.DisposeAsync();
                }
            }
        }


        private async Task<int> CreateRating(RatingOrReplySubmitRequest request)
        {
            var rating = new Models.Rating
            {
                UserId = request.userId,
                EntityType = request.ratingEntityType,
                CourseId = request.courseId,
                DocumentId = request.documentId,
                RatingValue = request.ratingValue,
                Review = request.ratingReview,
                RatingDate = DateTimeService.ConvertToVietnamTime(DateTime.UtcNow),     
            };

            await _context.Ratings.AddAsync(rating);
            await _context.SaveChangesAsync();
            return rating.Id;
        }

        private async Task<int> CreateReply(RatingOrReplySubmitRequest request)
        {
            // Nếu parentReply khác null, tức là đang trả lời cho rating
            if (request.parentReply != null)
            {
                // Kiểm tra xem parentReply có tồn tại không
                var parentReply = await _context.RatingReplies.FindAsync(request.parentReply);
                if (parentReply == null)
                {
                    throw new NotFoundException("Reply không tồn tại.");
                }
            }
            else
            {
                // Nếu parentReply là null, kiểm tra xem ratingEntityId có tồn tại không
                var rating = await _context.Ratings.FindAsync(request.rootId);
                if (rating == null)
                {
                    throw new NotFoundException("Rating không tồn tại.");
                }
            }

            // Gọi phương thức để thêm phản hồi vào cơ sở dữ liệu
            return await AddReplyToDatabase(request);
        }
        private async Task<int> AddReplyToDatabase(RatingOrReplySubmitRequest request)
        {
            var reply = new RatingReply
            {
                RatingId = request.rootId,
                UserId = request.userId,
                ReplyContent = request.ratingReview,
                ReplyDate = DateTimeService.ConvertToVietnamTime(DateTime.UtcNow),
                ParentReplyId = request.parentReply ?? null // Trả về null nếu đang reply cho rating
            };
            await _context.RatingReplies.AddAsync(reply);
            await _context.SaveChangesAsync();

            return reply.ReplyId;
        }
        private async Task<List<string>> UploadImages(List<IFormFile> images, int reference, int? rootId, string referenceType)
        {
            if (referenceType == "RATING" && rootId == 0)
            {
                var imageUrls = new List<string>();
                foreach (var image in images)
                {
                    string fileName = Path.GetFileName(image.FileName);
                    string imageUrl = await _fireBaseServices.UploadImageRatingToFirebaseStorageAsync(image, reference.ToString(), fileName);
                    imageUrls.Add(imageUrl);

                    var ratingImage = new RatingImage
                    {
                        ReferenceId = reference,
                        ImageUrl = imageUrl,
                        ReferenceType = referenceType,
                    };

                    await _context.RatingImages.AddAsync(ratingImage);
                }
                await _context.SaveChangesAsync();
                return imageUrls;
            }
            else
            {
                var imageUrls = new List<string>();
                foreach (var image in images)
                {
                    string fileName = Path.GetFileName(image.FileName);
                    string imageUrl = await _fireBaseServices.UploadImageRatingToFirebaseStorageAsync(image, reference.ToString(), fileName);
                    imageUrls.Add(imageUrl);

                    var ratingImage = new RatingImage
                    {
                        ReplyId = reference,
                        ImageUrl = imageUrl,
                        ReferenceId = rootId.Value,
                        ReferenceType = referenceType,
                    };

                    await _context.RatingImages.AddAsync(ratingImage);
                }
                await _context.SaveChangesAsync();
                return imageUrls;
            }
        }

        public async Task<IEnumerable<object>> GetAllRatingsAsync()
        {
            var ratings = await _context.Ratings
               .Select(r => new
               {
                   ratingId = r.Id,
                   userId = r.UserId,
                   ratingEntityType = r.EntityType,
                   ratingValue = r.RatingValue,
                   ratingReview = r.Review,
                   ratingDate = r.RatingDate,
                   ratingImageUrls = r.RatingImages.Select(ri => ri.ImageUrl).ToList(),
                   replyExist = _context.RatingReplies.Any(rr => rr.RatingId == r.Id),
                   childAmount = _context.RatingReplies.Count(rr => rr.RatingId == r.Id),
               })
               .ToListAsync();

            return ratings;
        }

        public async Task<IEnumerable<RatingDocumentResponse>> GetRatingsOfDocumentAsync(OfDocumentIdRequest _req)
        {
            var ratings = await _context.Ratings
               .Include(r => r.RatingImages)
               .Include(r => r.User)
               .Where(r => r.EntityType == "Document" && r.DocumentId == _req.documentId)
               .Select(r => new RatingDocumentResponse
               {
                   ratingId = r.Id,
                   userId = r.UserId,
                   userImage = r.User.UserImage,
                   userName = r.User.UserName,
                   documentId = r.DocumentId,
                   ratingValue = r.RatingValue,
                   ratingReview = r.Review,
                   ratingDate = r.RatingDate,
                   ratingImageUrls = r.RatingImages
                    .Where(ri => ri.ReferenceType == "RATING")
                    .Select(ri => ri.ImageUrl).ToList(),
                   replyExist = _context.RatingReplies.Any(rp => rp.RatingId == r.Id),
                   childAmount = _context.RatingReplies.Count(rr => rr.RatingId == r.Id),
               })
               .ToListAsync();

            return ratings;
        }

        public async Task<IEnumerable<RatingCourseResponse>> GetRatingsOfCourseAsync(OfCourseIdRequest _req)
        {
            var ratings = await _context.Ratings
                .Where(r => r.EntityType == "Course" && r.CourseId == _req.courseId)
                .Select(r => new RatingCourseResponse
                {
                    ratingId = r.Id,
                    userId = r.UserId,
                    userImage = r.User.UserImage,
                    userName = r.User.UserName,
                    courseId = r.CourseId,
                    ratingValue = r.RatingValue,
                    ratingReview = r.Review,
                    ratingRatingDate = r.RatingDate,
                    ratingImageUrls = r.RatingImages
                    .Where(ri => ri.ReferenceType == "RATING")
                    .Select(ri => ri.ImageUrl).ToList(),
                    replyExist = _context.RatingReplies.Any(rp => rp.RatingId == r.Id),
                    childAmount = _context.RatingReplies.Count(rr => rr.RatingId == r.Id),
                })
                .ToListAsync();

            return ratings;
        }
        public async Task<IEnumerable<RatingUserResponse>> GetRatingsOfUserAsync(OfUserIdRequest _req)
        {
            var ratings = await _context.Ratings
                .Where(r => r.UserId == _req.userId)
                .Select(r => new RatingUserResponse
                {
                    ratingId = r.Id,
                    userId = r.UserId,
                    userImage = r.User.UserImage,
                    ratingEntityType = r.EntityType,
                    ratingValue = r.RatingValue,
                    ratingReview = r.Review,
                    ratingDate = r.RatingDate,
                    ratingImageUrls = r.RatingImages.Select(ri => ri.ImageUrl).ToList()
                })
                .ToListAsync();

            return ratings;
        }
    }
}
