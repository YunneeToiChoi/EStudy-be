﻿using study4_be.Models;
using study4_be.Services.Container;

namespace study4_be.Services.Unit
{
    public class UnitDetailResponse
    {
        public int unitId { get; set; }
        public string unitName { get; set; } = string.Empty;
        public List<ContainerResponse> Containers { get; set; }
    }
}
