﻿using study4_be.Models;

namespace study4_be.Services.Response
{
    public class UnitDetailResponse
    {
        public int unitId { get; set; }
        public string unitName { get; set; } = string.Empty;
        public List<ContainerResponse> Containers { get; set; } 
    }
}
