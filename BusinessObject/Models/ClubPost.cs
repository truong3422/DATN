using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class ClubPost
    {
        public int PostId { get; set; }
        public int ClubId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string? Link { get; set; }
        public string? Flyer { get; set; }
        public bool? Status { get; set; }

        public virtual Club Club { get; set; } = null!;
    }
}
