using Newtonsoft.Json;

namespace Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api.Models
{
    /// <summary>
    /// Defines a response for a payment page session.
    /// </summary>
    public class PaymentPageSessionResponse : Resource
    {
        /// <summary>
        /// Gets or sets the reference.
        /// </summary>
        [JsonProperty("reference")]
        public string Reference { get; set; }
    }
}
