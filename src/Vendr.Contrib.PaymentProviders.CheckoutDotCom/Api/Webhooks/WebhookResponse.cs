using System.Collections.Generic;
using Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api.Models;

namespace Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api.Webhooks
{
    /// <summary>
    /// Defines a <see cref="WebhookResponse"/>.
    /// </summary>
    public class WebhookResponse : Resource
    {
        /// <summary>
        /// Gets or sets the webhook identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the webhook receiver endpoint.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets whether the webhook is active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the headers to be sent with the webhook notification.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        /// Gets or sets the event types for which the webhook should send notifications.
        /// </summary>
        public List<string> EventTypes { get; set; }
    }
}
