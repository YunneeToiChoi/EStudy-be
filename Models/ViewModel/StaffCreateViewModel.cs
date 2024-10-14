﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace study4_be.Models.ViewModel
{
    public class StaffCreateViewModel
    {
        public Staff Staffs { get; set; }

        public List<SelectListItem>? Departments { get; set; } 
        public List<SelectListItem>? Roles { get; set; }
    }
}
