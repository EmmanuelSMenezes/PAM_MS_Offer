using System;

namespace Domain.Model
{
    public class Branch
    {
        public Guid Branch_id { get; set; }
        public string Branch_name { get; set; }
        public string Document { get; set; }
        public string Phone { get; set; }
        public Guid Partner_id { get; set; }
        public Address Address { get; set; }
        public Guid Created_by { get; set; }
        public DateTime Created_at { get; set; }
        public Guid? Updated_by { get; set; }
        public DateTime? Updated_at { get; set; }
        public bool Active { get; set; }
        public int? Ratings { get; set; }
        public string? Avatar { get; set; }
        public decimal? Distance { get; set; }
        public int? OrdersNumbers { get; set; }
    }
}
