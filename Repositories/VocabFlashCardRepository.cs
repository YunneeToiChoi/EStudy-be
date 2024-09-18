using Microsoft.EntityFrameworkCore;
using study4_be.Models;

namespace study4_be.Repositories
{
    public class VocabFlashCardRepository
    {
        private readonly Study4Context _context = new Study4Context();
        public async Task<IEnumerable<Vocabulary>> GetAllVocabDependLesson(int idLesson)
        {
            return await _context.Vocabularies.Where(v => v.LessonId == idLesson).ToListAsync();
        }

    }
}
