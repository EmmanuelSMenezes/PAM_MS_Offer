using System;

namespace Domain.Model
{
  public class ActuationAreaBD
  {
    public Guid Actuation_area_id { get; set; }
    public dynamic Geometry { get; set; }
    public string GeometryJson { get; set; }
    public Guid Partner_id { get; set; }
    public Guid Branch_id { get; set; }
    public Guid Created_by { get; set; }
    public DateTime? Created_at { get; set; }
    public DateTime? Updated_at { get; set; }
    public bool Active { get; set; }
    public Guid? Updated_by { get; set; }
    public string Name { get; set; }
  }
}