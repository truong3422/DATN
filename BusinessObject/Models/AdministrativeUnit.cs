using System;
using System.Collections.Generic;

namespace BusinessObject.Models
{
    public partial class AdministrativeUnit
    {
        public AdministrativeUnit()
        {
            Districts = new HashSet<District>();
            Provinces = new HashSet<Province>();
            Wards = new HashSet<Ward>();
        }

        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? FullNameEn { get; set; }
        public string? ShortName { get; set; }
        public string? ShortNameEn { get; set; }
        public string? CodeName { get; set; }
        public string? CodeNameEn { get; set; }

        public virtual ICollection<District> Districts { get; set; }
        public virtual ICollection<Province> Provinces { get; set; }
        public virtual ICollection<Ward> Wards { get; set; }
    }
}
