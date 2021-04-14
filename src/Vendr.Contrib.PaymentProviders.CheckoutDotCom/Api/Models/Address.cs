using Newtonsoft.Json;

namespace Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api.Models
{
    /// <summary>
    /// Defines an address.
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Gets or sets the address line 1.
        /// </summary>
        [JsonProperty("address_line1")]
        public string Line1 { get; set; }

        /// <summary>
        /// Gets or sets the address line 2.
        /// </summary>
        [JsonProperty("address_line2")]
        public string Line2 { get; set; }

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        [JsonProperty("city")]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        [JsonProperty("state")]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the zip.
        /// </summary>
        [JsonProperty("zip")]
        public string Zip { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        [JsonProperty("country")]
        public string Country { get; set; }
    }
}
