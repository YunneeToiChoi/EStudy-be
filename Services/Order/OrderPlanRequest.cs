﻿using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Order
{
    public class OrderPlanRequest
    {
        public string? UserId { get; set; }
        [Required]
        public required int PlanId { get; set; }
    }
}