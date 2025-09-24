using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class OfferFilters
    {
        public List<BranchFilters> Branchs { get; set; }
        public List<CategoryFilters> Categories{ get; set; }
        public decimal Price_maximum { get; set; }
        public decimal Distance_maximum { get; set; }
    }

    public class BranchFilters
    {
        public Guid Branch_id { get; set; }
        public string Branch_name { get; set; }
    }

    public class CategoryFilters
    {
        public Guid Category_id { get; set; }
        public string Description { get; set; }
    }
}
