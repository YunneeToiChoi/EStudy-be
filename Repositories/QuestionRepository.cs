using Microsoft.EntityFrameworkCore;
using study4_be.Models;

namespace study4_be.Repositories
{
    public class QuestionRepository
    {
        private readonly Study4Context _context;
        public QuestionRepository(Study4Context context) { _context = context; }
        public async Task<IEnumerable<Question>> GetAllQuestionsAsync()
        {
            return await _context.Questions.ToListAsync();
        }
        public async Task DeleteAllQuestionsAsync()
        {
            var questions = await _context.Questions.ToListAsync();
            _context.Questions.RemoveRange(questions);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Question>> GetAllQuestionsOfLesson(int lessonId)
        {
            return await _context.Questions.Where(q =>  q.LessonId == lessonId).ToListAsync();
        }

    }
}
