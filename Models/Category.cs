using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
