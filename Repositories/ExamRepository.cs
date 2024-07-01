using study4_be.Models;
using Microsoft.EntityFrameworkCore;

namespace study4_be.Repositories
{
    public class ExamRepository
    {
        private readonly STUDY4Context _context = new STUDY4Context();
        public async Task<IEnumerable<Exam>> GetAllExamsAsync()
        {
            return await _context.Exams.ToListAsync();
        }
        public async Task DeleteAllExamsAsync()
        {
            var exams = await _context.Exams.ToListAsync();
            _context.Exams.RemoveRange(exams);
            await _context.SaveChangesAsync();
        }
    }
}
