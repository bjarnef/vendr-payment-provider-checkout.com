using Checkout;
using System;
using System.Web;
using Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api.Models;
using Vendr.Core;
using Vendr.Core.Models;
using Vendr.Core.Web.Api;
using Vendr.Core.Web.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders.CheckoutDotCom
{
    public abstract class CheckoutPaymentProviderBase<TSettings> : PaymentProviderBase<TSettings>
        where TSettings : CheckoutSettingsBase, new()
    {
        public CheckoutPaymentProviderBase(VendrContext vendr)
            : base(vendr)
        { }

        public override string GetCancelUrl(OrderReadOnly order, TSettings settings)
        {
            settings.MustNotBeNull("settings");
            settings.CancelUrl.MustNotBeNull("settings.CancelUrl");

            return settings.CancelUrl;
        }

        public override string GetContinueUrl(OrderReadOnly order, TSettings settings)
        {
            settings.MustNotBeNull("settings");
            settings.ContinueUrl.MustNotBeNull("settings.ContinueUrl");

            return settings.ContinueUrl;
        }

        public override string GetErrorUrl(OrderReadOnly order, TSettings settings)
        {
            settings.MustNotBeNull("settings");
            settings.ErrorUrl.MustNotBeNull("settings.ErrorUrl");

            return settings.ErrorUrl;
        }

        //protected ApiClient GetClient(CheckoutSettingsBase settings)
        //{
        //    var config = new CheckoutConfiguration(settings.SecretKey, settings.TestMode);
        //    var client = new ApiClient(config);

        //    return client;
        //}

        protected ClientConfig GetClientConfig(CheckoutSettingsBase settings)
        {
            var testMode = settings.TestMode;
            var secretKey = settings.SecretKey;

            return new ClientConfig
            {
                BaseUrl = testMode ? "https://api.sandbox.checkout.com" : "https://api.checkout.com",
                Authorization = secretKey,
                Secret = secretKey
            };
        }
    }
}