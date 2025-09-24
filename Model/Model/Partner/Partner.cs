using System;
using System.Collections.Generic;

namespace Domain.Model
{
  public class Partner
  {
    public Guid Partner_id { get; set; }
    public int Identifier { get; set; }
    public string Legal_name { get; set; }
    public string Fantasy_name { get; set; }
    public string Document { get; set; }
    public string Email { get; set; }
    public string Phone_number { get; set; }
    public List<Branch> Branch { get; set; }
    public Guid User_id { get; set; }
    public Guid? Admin_id { get; set; }
    public string? Avatar { get; set; }
    public bool Active { get; set; }
    public Guid Created_by { get; set; }
    public Guid? Updated_by { get; set; }
  }
}
