
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using study4_be.Interface;
using study4_be.Models;
using study4_be.Services.Request;
using study4_be.Services.Request.Rating;

namespace study4_be.Services
{
    public class RatingService : IRatingService
    {
        private readonly Study4Context _context;
        private readonly FireBaseServices _fireBaseServices;

        public RatingService(Study4Context context, FireBaseServices fireBaseServices)
        {
            _context = context;
            _fireBaseServices = fireBaseServices;
        }

        public async Task<object> SubmitRatingOrReplyAsync(RatingOrReplySubmitRequest request, List<IFormFile> ratingImages)
        {
            // Kiểm tra người dùng
            var user = await _context.Users.FindAsync(request.userId);
            if (user == null) throw new NotFoundException("Người dùng không tồn tại.");

            // Bắt đầu giao dịch
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                int referenceId;
                string referenceType;

                if (request.isRating)
                {
                    referenceId = await CreateRating(request);
                    referenceType = "RATING";
                }
                else
                {
                    referenceId = await CreateReply(request);
                    referenceType = "REPLY";
                }
                // Tải lên hình ảnh
                var imageUrls = await UploadImages(ratingImages, referenceId,request.rootId, referenceType);

                // Cam kết giao dịch
                await transaction.CommitAsync();

                // Trả về phản hồi
                return new
                {
                    ReferenceId = referenceId,
                    UserId = request.userId,
                    ReferenceType = referenceType,
                    RatingImages = imageUrls
                };
            }
        }

        private async Task<int> CreateRating(RatingOrReplySubmitRequest request)
        {
            var rating = new Rating
            {
                UserId = request.userId,
                EntityType = request.ratingEntityType,
                CourseId = request.courseId,
                DocumentId = request.documentId, 
                RatingValue = request.ratingValue,
                Review = request.ratingReview,
                RatingDate = DateTime.Now
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
                ReplyDate = DateTime.UtcNow, // Sử dụng UTC để đảm bảo tính nhất quán
                ParentReplyId = request.parentReply ?? null // Trả về null nếu đang reply cho rating
            };

            await _context.RatingReplies.AddAsync(reply);
            await _context.SaveChangesAsync();

            return reply.ReplyId;
        }
        private async Task<List<string>> UploadImages(List<IFormFile> images, int reference,int rootId ,string referenceType)
        {
            if(referenceType == "RATING" && rootId == 0)
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
                        ReferenceId = rootId,
                        ReferenceType = referenceType,
                    };

                    await _context.RatingImages.AddAsync(ratingImage);
                }
                await _context.SaveChangesAsync();
                return imageUrls;
            }
        }
    }
}
