﻿using study4_be.Models.DTO;
using study4_be.Services.Rating;

namespace study4_be.Interface.Rating
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    }
}
