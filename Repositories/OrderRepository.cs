using Microsoft.EntityFrameworkCore;
using study4_be.Models;

namespace study4_be.Repositories
{
	public class OrderRepository
	{
		private readonly Study4Context _context = new Study4Context();
		public async Task<IEnumerable<Order>> GetAllOrdersAsync()
		{
			return await _context.Orders.ToListAsync();
		}
		public async Task DeleteAllOrdersAsync()
		{
			var orders = await _context.Orders.ToListAsync();
			_context.Orders.RemoveRange(orders);
			await _context.SaveChangesAsync();
		}
	}
}
