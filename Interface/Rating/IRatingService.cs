using study4_be.Services.Rating;
using study4_be.Services.Course;
using study4_be.Services.Document;
using study4_be.Services.User;

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
