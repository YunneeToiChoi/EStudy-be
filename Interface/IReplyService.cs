using study4_be.Services.Request.Rating;
using study4_be.Services.Response;

namespace study4_be.Interface
{
    public interface IReplyService
    {
        Task<ShowReplyResponse> ShowReplyAsync(ShowReplyRequest request);
    }
}
