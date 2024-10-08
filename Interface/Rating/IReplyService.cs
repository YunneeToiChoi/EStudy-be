using study4_be.Services.Rating;

namespace study4_be.Interface.Rating
{
    public interface IReplyService
    {
        Task<ShowReplyResponse> ShowReplyAsync(ShowReplyRequest request);
    }
}
