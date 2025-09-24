using System;
using System.Collections.Generic;

namespace Domain.Model
{
  public class Product
  {
    public Guid? Product_id { get; set; }
    public int? Identifier { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal? Price { get; set; }
    public string Type { get; set; }
    public Guid? Image_default { get; set; }
    public string Url { get; set; }
    public List<Category> Categories { get; set; }
    public Guid? Admin_id { get; set; }
    public List<Image> Images { get; set; }
  }

  public class Image
  {
    public Guid Product_image_id { get; set; }
    public string Url { get; set; }
    public Guid Created_by { get; set; }
    public DateTime Created_at { get; set; }
    public Guid? Updated_by { get; set; }
    public DateTime? Updated_at { get; set; }
  }

  public class Category
  {
    public Guid? Category_id { get; set; }
    public string Description { get; set; }
    public string? Category_parent_name { get; set; }
    public Guid? Category_parent_id { get; set; }
  }
}
