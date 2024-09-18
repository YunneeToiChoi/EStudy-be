using Microsoft.EntityFrameworkCore;
using study4_be.Models;

namespace study4_be.Repositories
{
    public class SubscriptionRepository
    {
        private readonly Study4Context _context = new();

        public async Task<IEnumerable<Subscriptionplan>> GetAllPlans()
        {
            return await _context.Subscriptionplans.ToListAsync();
        }
    }
}
