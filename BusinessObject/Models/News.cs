using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class News
    {
        public int NewsId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int AccId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string? Link { get; set; }
        public string? Flyer { get; set; }
        public bool? Status { get; set; }

        public virtual Account Acc { get; set; } = null!;
    }
}
