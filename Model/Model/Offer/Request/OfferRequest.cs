using Newtonsoft.Json;
using System;

namespace Domain.Model.Request
{
    public class OfferRequest
    {
        [JsonIgnore]
        public Guid Offer_id { get; set; }
    }
}
