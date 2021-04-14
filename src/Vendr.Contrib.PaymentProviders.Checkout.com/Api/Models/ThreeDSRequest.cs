using Newtonsoft.Json;

namespace Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api.Models
{
    /// <summary>
    /// Information required for 3D-Secure payments
    /// </summary>
    public class ThreeDSRequest
    {
        /// <summary>
        /// Gets or sets a value that indicates whether to process this payment as a 3D-Secure.
        /// </summary>
        [JsonProperty(PropertyName = "enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether to attempt a 3D-Secure payment as non-3DS should the card issuer not be enrolled.
        /// </summary>
        [JsonProperty(PropertyName = "attempt_n3d")]
        public bool? AttemptN3D { get; set; }
    }
}
