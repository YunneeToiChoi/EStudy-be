using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using System.Linq;

namespace study4_be.Repositories
{
    public class ContainerRepository
    {
        private readonly Study4Context _context;
        public ContainerRepository(Study4Context context) { _context = context; }
        public async Task DeleteAllUnitsAsync()
        {
            var units = await _context.Units.ToListAsync();
            _context.Units.RemoveRange(units);
            await _context.SaveChangesAsync();
        }
    }
}
