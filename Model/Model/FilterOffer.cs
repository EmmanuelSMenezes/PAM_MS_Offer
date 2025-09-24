using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace Domain.Model
{
    public class FilterOffer
    {
        public string? Filter { get; set; }
        public string? Category_ids { get; set; }
        public string? Branch_ids { get; set; }
        public string? Delivery_option_ids { get; set; }
        public string? Ratings { get; set; }
        public string? Distance { get; set; }
        public string? Start_price { get; set; }
        public string? End_price { get; set; }
        public bool Shipping_free { get; set; }
        public int? Page { get; set; }
        public int? ItensPerPage { get; set; }
        public OrderByBranch OrderByBranch { get; set; }
        public OrderByProduct OrderByProduct { get; set; }
        public SortOrder SortOrder { get; set; }

    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderByBranch
    {
        [EnumMember(Value = "Ratings")]
        [Description("Ratings")]
        Ratings,

        [EnumMember(Value = "OrdersNumbers")]
        [Description("OrdersNumbers")]
        OrdersNumbers
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SortOrder
    {
        [EnumMember(Value = "Asc")]
        asc,

        [EnumMember(Value = "Desc")]
        desc
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum OrderByProduct
    {
        [EnumMember(Value = "Ratings")]
        [Description("ratings")]
        Ratings,

        [EnumMember(Value = "Price")]
        [Description("p.price")]
        Price
    }
}
