using study4_be.Services.Request.Rating;

namespace study4_be.Interface
{
    public interface IRatingService
    {
        Task<object> SubmitRatingOrReplyAsync(RatingOrReplySubmitRequest request, List<IFormFile> ratingImages);
    }
}
