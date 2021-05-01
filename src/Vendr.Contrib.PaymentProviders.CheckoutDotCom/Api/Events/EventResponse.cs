using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api.Models;

namespace Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api.Events
{
    /// <summary>
    /// Defines an <see cref="EventResponse"/>.
    /// </summary>
    public class EventResponse : Resource
    {
        /// <summary>
        /// Gets or sets the unique identifier of the event.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the version of the event.
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the date/time the event occurred.
        /// </summary>
        [JsonProperty("created_on")]
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the data of the event.
        /// </summary>
        [JsonProperty("data")]
        public Dictionary<string, object> Data { get; set; }

        /// <summary>
        /// Gets or sets the notifications (e.g. webhooks) that have been sent for the event.
        /// </summary>
        //public List<EventNotificationSummary> Notifications { get; set; }
    }
}
