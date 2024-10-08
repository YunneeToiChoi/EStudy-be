
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using study4_be.Interface.Rating;
using study4_be.Models;
using study4_be.Models.DTO;
using study4_be.Services.Request.Course;
using study4_be.Services.Request.Document;
using study4_be.Services.Request.Rating;
using study4_be.Services.Request.User;
using study4_be.Services.Response;

namespace study4_be.Services.Rating
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
