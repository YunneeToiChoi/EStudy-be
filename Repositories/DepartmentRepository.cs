using study4_be.Models;
using Microsoft.EntityFrameworkCore;

namespace study4_be.Repositories
{
    public class DepartmentRepository
    {
        private readonly Study4Context _context;

        public DepartmentRepository(Study4Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
        {
            return await _context.Departments.ToListAsync();
        }
        public async Task DeleteAllDepartmentsAsync()
        {
            var departments = await _context.Departments.ToListAsync();
            _context.Departments.RemoveRange(departments);
            await _context.SaveChangesAsync();
        }
    }
}
