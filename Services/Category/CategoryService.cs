
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using study4_be.Interface.Rating;
using study4_be.Models;
using study4_be.Models.DTO;
using study4_be.Services.Course;
using study4_be.Services.Document;
using study4_be.Services.Rating;
using study4_be.Services.User;
using study4_be.Services;

namespace study4_be.Services.Category
{
    public class CategoryService : ICategoryService
    {
        private readonly Study4Context _context;
        public CategoryService(Study4Context context)
        {
            _context = context;
        }
        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            return categories.Select(c => new CategoryDto
            {
                categoryId = c.CategoryId,
                categoryName = c.CategoryName
            });
        }
    }
}
