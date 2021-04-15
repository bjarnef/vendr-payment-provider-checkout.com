using Newtonsoft.Json;
using System.Collections.Generic;

namespace Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api.Models
{
    /// <summary>
    /// Base class for API resources.
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// Creates a new <see cref="Resource"/> instance initialising the links collection.
        /// </summary>
        public Resource()
        {
            Links = new Dictionary<string, Link>();
        }

        /// <summary>
        /// Gets or sets the links related to the resource.
        /// </summary>
        [JsonProperty(PropertyName = "_links")]
        public Dictionary<string, Link> Links { get; set; }

        /// <summary>
        /// Gets the resource's self link.
        /// </summary>
        /// <returns>The link if it exists, otherwise null.</returns>
        public Link GetSelfLink()
        {
            return GetLink("self");
        }

        /// <summary>
        /// Gets the resource's redirect link.
        /// </summary>
        /// <returns>The redirect if it exists, otherwise null.</returns>
        public Link GetRedirectLink()
        {
            return GetLink("redirect");
        }

        /// <summary>
        /// Checks if a link with the specified <paramref="relation"/> type exists.
        /// </summary>
        /// <param name="relation">The link relation type.</param>
        /// <returns>True if the link exists, otherwise false.</returns>
        public bool HasLink(string relation)
        {
            return Links.ContainsKey(relation);
        }

        /// <summary>
        /// Gets the link with the specified <paramref="relation"/> type.
        /// </summary>
        /// <param name="relation">The link relation type.</param>
        /// <returns>The link if it exists, otherwise null.</returns>
        public Link GetLink(string relation)
        {
            Links.TryGetValue(relation, out var link);
            return link;
        }
    }
}
