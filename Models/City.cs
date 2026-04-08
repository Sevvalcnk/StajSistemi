using System.Collections.Generic;

namespace StajSistemi.Models
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // ✅ MÜHÜR: İlişkiler
        public virtual ICollection<AppUser>? Users { get; set; }
        public virtual ICollection<Internship>? Internships { get; set; }
    }
}