using Microsoft.EntityFrameworkCore;
using study4_be.Models;

namespace study4_be.Repositories
{
    public class UnitRepository
    {
        private readonly Study4Context _context = new Study4Context();
        public async Task<IEnumerable<Unit>> GetAllUnitsByCourseAsync(int idCourse)
        {
            var units = await _context.Units.Where(u => u.CourseId == idCourse).ToListAsync();
            return units;
        }
        public async Task DeleteAllUnitsAsync()
        {
            var units = await _context.Units.ToListAsync();
            _context.Units.RemoveRange(units);
            await _context.SaveChangesAsync();
        }
    }
}
