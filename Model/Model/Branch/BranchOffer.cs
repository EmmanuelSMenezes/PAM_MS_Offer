using System;

namespace Domain.Model
{
  public class BranchOffer
  {
    public Guid Branch_id { get; set; }
    public string Branch_name { get; set; }
    public Guid Partner_id { get; set; }
    public int? Ratings { get; set; }
    public decimal? Distance { get; set;}
    public string Avatar { get; set; }
    public int? OrdersNumbers { get; set; }
  }
}
