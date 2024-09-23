using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Services;
using study4_be.Services.Request;

namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserSubs_APIController : Controller
    {
        private readonly DateTimeService _datetimeService = new();
        private readonly Study4Context _context = new();

        [HttpPost("Check_Expire")]
        public async Task<IActionResult> CheckExpireSubscription(UserRequest request)
        {
            var result = await _datetimeService.GetPlanTimeRemaining(request.userId);
            if (result == null)
            {
                return NotFound(new { messagse = "Your plan is not exist" });
            }

            if (result.Days <= 5 && result.Days > 0)
            {
                return Ok(new { message = "Your subscription is about to end", days = result.Days, hours = result.Hours, minutes = result.Minutes });
            }
            else if (result.Days <= 0) 
            {
                var userSubscription = await _context.UserSubs
                .FirstOrDefaultAsync(u => u.UserId == request.userId);
                userSubscription.State = false;
                await _context.SaveChangesAsync();
                return BadRequest(new {message = "Your plan is disable"});
            }
            return Ok(new { days = result.Days, hours = result.Hours, minutes = result.Minutes});
        }

        [HttpDelete("Cancel_Plan")]
        public async Task<IActionResult> Cancel_Plan(UserRequest request)
        {
            var userSubscription = await _context.UserSubs
                .FirstOrDefaultAsync(u => u.UserId == request.userId && u.State == true);
            _context.UserSubs.Remove(userSubscription);
            await _context.SaveChangesAsync();
            return BadRequest(new { message = "Your plan is cancel"});
        }
    }
}
