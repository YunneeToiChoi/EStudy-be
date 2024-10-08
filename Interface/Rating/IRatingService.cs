using study4_be.Services.Request.Document;
using study4_be.Services.Request.Rating;
using study4_be.Services.Response;
using study4_be.Services.Request.User;
using study4_be.Services.Request.Course;

namespace study4_be.Interface.Rating
{
    public interface IRatingService
    {
        Task<object> SubmitRatingOrReplyAsync(RatingOrReplySubmitRequest request, List<IFormFile> ratingImages);
        Task<IEnumerable<object>> GetAllRatingsAsync();
        Task<IEnumerable<RatingDocumentResponse>> GetRatingsOfDocumentAsync(OfDocumentIdRequest request);
        Task<IEnumerable<RatingCourseResponse>> GetRatingsOfCourseAsync(OfCourseIdRequest request);
        Task<IEnumerable<RatingUserResponse>> GetRatingsOfUserAsync(OfUserIdRequest request);
    }
}
