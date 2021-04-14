using Newtonsoft.Json;

namespace Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api.Models
{
    /// <summary>
    /// Defines a phone number.
    /// </summary>
    public class Phone
    {
        /// <summary>
        /// Gets or sets the phone number country code.
        /// </summary>
        /// <example>+44</example>
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <example>415 555 2671</example>
        [JsonProperty("number")]
        public string Number { get; set; }
    }
}
