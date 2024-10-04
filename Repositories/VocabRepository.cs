using Microsoft.EntityFrameworkCore;
using study4_be.Models;

namespace study4_be.Repositories
{
    public class VocabRepository
    {
        private readonly Study4Context _context;
        public VocabRepository(Study4Context context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Vocabulary>> GetAllVocabAsync()
        {
            return await _context.Vocabularies.ToListAsync();
        }
        public async Task DeleteAllVocabAsync()
        {
            var vocab = await _context.Vocabularies.ToListAsync();
            _context.Vocabularies.RemoveRange(vocab);
            await _context.SaveChangesAsync();
        }
    }
}
