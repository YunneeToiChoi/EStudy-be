﻿using study4_be.Models;
using Microsoft.EntityFrameworkCore;


namespace study4_be.Repositories
{
    public class StaffRepository
    {
        private readonly Study4Context _context;
        public StaffRepository(Study4Context context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Staff>> GetAllStaffsByDepartmentAsync(int idDepartment)
        {
            var staffs = await _context.Staff.Where(u => u.DepartmentId == idDepartment).ToListAsync();
            return staffs;
        }
        public async Task DeleteAllStaffsAsync()
        {
            var staffs = await _context.Staff.ToListAsync();
            _context.Staff.RemoveRange(staffs);
            await _context.SaveChangesAsync();
        }
    }
}
