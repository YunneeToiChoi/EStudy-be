﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace study4_be.Models.ViewModel
{
    public class UnitCreateVIewModel
    {
        public Unit Units { get; set; }
        public List<SelectListItem> Courses { get; set; }
    }
}
