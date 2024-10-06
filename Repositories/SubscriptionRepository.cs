using Microsoft.EntityFrameworkCore;
using study4_be.Models;

namespace study4_be.Repositories
{
    public class SubscriptionRepository
    {
        private readonly Study4Context _context;

        public SubscriptionRepository(Study4Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Subscriptionplan>> GetAllPlans()
        {
            return await _context.Subscriptionplans.ToListAsync();
        }
        public async Task DeleteAllPlansAsync()
        {
            var plans = await _context.Subscriptionplans.ToListAsync();
            _context.Subscriptionplans.RemoveRange(plans);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<UserSub>> Get_PlanFromUser(string idUser)
        {
            // Retrieve all UserSubs for the user
            var userSubs = await _context.UserSubs
                .Where(us => us.UserId == idUser)
                .ToListAsync();

            // Assuming UserSubs has a property named SubscriptionPlanId or similar
            return userSubs;
        }

    }
}
