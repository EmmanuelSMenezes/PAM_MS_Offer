using Microsoft.AspNetCore.Mvc;

namespace Domain.Model
{
  public class Coordinates
  {
    [FromQuery(Name = "latitude")]
    public string latitude { get; set; }

    [FromQuery(Name = "longitude")]
    public string longitude { get; set; }
  }
}
