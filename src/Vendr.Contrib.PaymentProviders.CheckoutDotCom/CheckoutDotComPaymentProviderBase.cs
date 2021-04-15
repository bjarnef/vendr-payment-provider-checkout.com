using Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api.Models;
using Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api.Payments;
using Vendr.Core;
using Vendr.Core.Models;
using Vendr.Core.Web.Api;
using Vendr.Core.Web.PaymentProviders;
using PaymentStatus = Vendr.Core.Models.PaymentStatus;

namespace Vendr.Contrib.PaymentProviders.CheckoutDotCom
{
    public abstract class CheckoutPaymentProviderBase<TSettings> : PaymentProviderBase<TSettings>
        where TSettings : CheckoutDotComSettingsBase, new()
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

        protected ClientConfig GetClientConfig(CheckoutDotComSettingsBase settings)
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

        protected PaymentStatus GetPaymentStatus(GetPaymentResponse payment)
        {
            if (payment.Status == Api.Payments.PaymentStatus.Authorized)
                return PaymentStatus.Authorized;

            if (payment.Status == Api.Payments.PaymentStatus.Captured)
                return PaymentStatus.Captured;

            if (payment.Status == Api.Payments.PaymentStatus.Refunded)
                return PaymentStatus.Refunded;

            if (payment.Status == Api.Payments.PaymentStatus.Canceled ||
                payment.Status == Api.Payments.PaymentStatus.Voided)
                return PaymentStatus.Cancelled;

            if (payment.Status == Api.Payments.PaymentStatus.Expired ||
                payment.Status == Api.Payments.PaymentStatus.Declined)
                return PaymentStatus.Error;

            return PaymentStatus.Initialized;
        }
    }
}