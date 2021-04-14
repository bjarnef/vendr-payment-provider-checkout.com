using Newtonsoft.Json;

namespace Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api.Models
{
    public class Billing
    {
        [JsonProperty("address")]
        public Address Address { get; set; }

        [JsonProperty("phone")]
        public Phone Phone { get; set; }
    }
}