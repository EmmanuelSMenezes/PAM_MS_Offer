using System.Collections.Generic;

namespace Domain.Model
{
  public class Offer
  {
    public List<BranchOffer> Branches { get; set; }
    public Pagination Pagination { get; set; }
  }
   
    public class ProductsBranchOffer
  {
    public List<ListByProduct> Products { get; set; }
    public Pagination Pagination { get; set; }
  }

  public class ListByProduct: Product
  {
    public Branch Branch { get; set; }
  }
}