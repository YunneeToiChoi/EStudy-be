using Microsoft.EntityFrameworkCore;
using study4_be.Models;

namespace study4_be.Repositories
{
	public class VideoRepository
	{
		private readonly Study4Context _context = new Study4Context();
		public async Task<IEnumerable<Video>> GetAllVideosAsync()
		{
			return await _context.Videos.ToListAsync();
		}
		public async Task DeleteAllVideosAsync()
		{
			var videos = await _context.Videos.ToListAsync();
			_context.Videos.RemoveRange(videos);
			await _context.SaveChangesAsync();
		}
	}
}
