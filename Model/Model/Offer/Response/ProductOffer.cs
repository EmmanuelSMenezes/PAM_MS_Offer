using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public class ProductOffer
    {
        public Guid Branch_id { get; set; }
        public string Branch_name { get; set; }
        public decimal Distance { get; set; }
        public long Ratings { get; set; }
        public long Ordersnumbers { get; set; }
        public Guid Product_id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public List<Category> Categories { get; set; }
    }
}
