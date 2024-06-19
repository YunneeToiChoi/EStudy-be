using System;
using System.Collections.Generic;

namespace study4_be.Models
{
    public partial class Report
    {
        public int ReportId { get; set; }
        public string? ReportType { get; set; }
        public decimal? Amount { get; set; }
        public string? Description { get; set; }
        public DateTime? DateTime { get; set; }
        public string? OrderId { get; set; }
        public int? StaffId { get; set; }

        public virtual Order? Order { get; set; }
        public virtual Staff? Staff { get; set; }
    }
}
