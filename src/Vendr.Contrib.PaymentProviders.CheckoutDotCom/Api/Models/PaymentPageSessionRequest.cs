using Newtonsoft.Json;
using System.Collections.Generic;

namespace Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api.Models
{
    /// <summary>
    /// Defines a request for a payment page session.
    /// </summary>
    public class PaymentPageSessionRequest
    {
        /// <summary>
        /// Gets the payment amount in the major currency.
        /// </summary>
        [JsonProperty("amount")]
        public long? Amount { get; set; }

        /// <summary>
        /// Gets the three-letter ISO currency code.
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; }

        /// <summary>
        /// Gets or sets a reference you can later use to identify this payment such as an order number.
        /// </summary>
        [JsonProperty("reference")]
        public string Reference { get; set; }

        /// <summary>
        /// Gets or sets a description of the payment.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to capture the payment automatically (default).
        /// </summary>
        [JsonProperty("capture")]
        public bool? Capture { get; set; } = false;

        /// <summary>
        /// Gets or sets a customer of the payment.
        /// </summary>
        [JsonProperty("customer")]
        public Customer Customer { get; set; }

        /// <summary>
        /// Gets or sets the billing of the payment.
        /// </summary>
        [JsonProperty("billing")]
        public Billing Billing { get; set; }

        /// <summary>
        /// Gets or sets the products of the payment.
        /// </summary>
        [JsonProperty("products")]
        public List<Product> Products { get; set; }

        /// <summary>
        /// Gets or sets the configuration of the risk assessment performed during the processing of the payment.
        /// If not specified, a risk assessment using Checkout.com's risk engine will be performed.
        /// </summary>
        public RiskRequest Risk { get; set; }

        /// <summary>
        /// Gets or sets the success redirect URL overridding the default URL configured in the Checkout Hub.
        /// </summary>
        [JsonProperty("success_url")]
        public string SuccessUrl { get; set; }

        /// <summary>
        /// Gets or sets the failure redirect URL overridding the default URL configured in the Checkout Hub.
        /// </summary>
        [JsonProperty("failure_url")]
        public string FailureUrl { get; set; }

        /// <summary>
        /// Gets or sets the cancel redirect URL overridding the default URL configured in the Checkout Hub.
        /// </summary>
        [JsonProperty("cancel_url")]
        public string CancelUrl { get; set; }

        /// <summary>
        /// Gets or sets the metadata for the payment.
        /// </summary>
        [JsonProperty("metadata")]
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether to process this payment as a 3D-Secure.
        /// </summary>
        [JsonProperty(PropertyName = "3ds")]
        public ThreeDSRequest ThreeDS { get; set; }
    }
}
