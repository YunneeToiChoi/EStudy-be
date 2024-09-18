using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using System.Linq;

namespace study4_be.Repositories
{
    public class ContainerRepository
    {
        private readonly Study4Context _context = new Study4Context();
        //public async Task<UnitDetail> GetAllContainersAndLessonsByUnitAsync(int unitId)
        //{
        
        //}
        public async Task DeleteAllUnitsAsync()
        {
            var units = await _context.Units.ToListAsync();
            _context.Units.RemoveRange(units);
            await _context.SaveChangesAsync();
        }
    }
}
