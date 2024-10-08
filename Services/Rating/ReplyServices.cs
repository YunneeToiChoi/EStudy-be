
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using study4_be.Interface.Rating;
using study4_be.Models;
using study4_be.Models.DTO;
using study4_be.Services.Request;
using study4_be.Services.Request.Rating;
using study4_be.Services.Response;

namespace study4_be.Services.Rating
{
    public class ReplyService : IReplyService
    {
        private readonly Study4Context _context;
        private readonly FireBaseServices _fireBaseServices;

        public ReplyService(Study4Context context, FireBaseServices fireBaseServices)
        {
            _context = context;
            _fireBaseServices = fireBaseServices;
        }
        public async Task<ShowReplyResponse> ShowReplyAsync(ShowReplyRequest _req)
        {
            var rating = await _context.Ratings
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == _req.ratingId);

            if (rating == null)
            {
                throw new NotFoundException("Rating not found");
            }

            // Xác định số trang và số phản hồi mỗi trang (mặc định là 5)
            int pageNumber = _req.pageNumber > 0 ? _req.pageNumber : 1;
            int pageSize = _req.pageSize > 0 ? _req.pageSize : 5;

            List<RatingReply> replies;

            if (_req.parentId == 0) // trường hợp hiển thị các phản hồi cấp 1 (trả lời trực tiếp rating)
            {
                replies = await _context.RatingReplies
                    .AsNoTracking()
                    .Where(rp => rp.RatingId == _req.ratingId && rp.ParentReplyId == null) // parentId null để lọc các phản hồi cấp 1
                    .Include(rp => rp.RatingImages)
                    .Include(rp => rp.User)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            else // nếu có parentId thì hiển thị các phản hồi cấp 2 hoặc cấp 3
            {
                replies = await _context.RatingReplies
                    .AsNoTracking()
                    .Where(rp => rp.RatingId == _req.ratingId && rp.ParentReplyId == _req.parentId) // lọc các phản hồi cấp 2 hoặc cấp 3
                    .Include(rp => rp.RatingImages)
                    .Include(rp => rp.User)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }

            // Chuẩn bị response
            var response = new ShowReplyResponse
            {
                RatingId = rating.Id,
                RatingContent = rating.Review,
                Replies = replies.Select(rp => new ReplyDto
                {
                    ReplyId = rp.ReplyId,
                    RatingId = rp.RatingId,
                    ReplyContent = rp.ReplyContent,
                    ReplyDate = rp.ReplyDate,
                    ParentReplyId = rp.ParentReplyId,
                    User = new UserDto
                    {
                        UserId = rp.User.UserId,
                        UserName = rp.User.UserName,
                        UserImage = rp.User.UserImage
                    },
                    Images = rp.RatingImages.Select(img => new ImageDto
                    {
                        ImageUrl = img.ImageUrl
                    }).ToList(),
                    // Check if the reply has any child replies (cấp dưới)
                    ReplyExist = _context.RatingReplies.Any(child => child.ParentReplyId == rp.ReplyId),
                    childAmount = _context.RatingReplies.Count(child => child.ParentReplyId == rp.ReplyId)
                }).ToList()
            };

            return response;
        }
    }
}
