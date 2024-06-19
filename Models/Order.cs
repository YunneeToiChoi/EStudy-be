using System;
using System.Collections.Generic;

namespace study4_be.Models
{
    public partial class Order
    {
        public Order()
        {
            Reports = new HashSet<Report>();
        }

        public string OrderId { get; set; } = null!;
        public string? UserId { get; set; }
        public int? CourseId { get; set; }
        public DateTime? OrderDate { get; set; }
        public double? TotalAmount { get; set; }
        public string? Address { get; set; }
        public bool? State { get; set; }
        public string? Email { get; set; }
        public string? Code { get; set; }

        public virtual Course? Course { get; set; }
        public virtual User? User { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
    }
}
