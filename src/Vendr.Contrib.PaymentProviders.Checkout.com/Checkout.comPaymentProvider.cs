using System;
using System.Web;
using System.Web.Mvc;
using Vendr.Core;
using Vendr.Core.Models;
using Vendr.Core.Web.Api;
using Vendr.Core.Web.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders.Checkout.com
{
    [PaymentProvider("checkout.com", "Checkout.com", "Checkout.com payment provider", Icon = "icon-invoice")]
    public class Checkout.comPaymentProvider : PaymentProviderBase<Checkout.comSettings>
    {
        public Checkout.comPaymentProvider(VendrContext vendr)
            : base(vendr)
        { }

        public override bool FinalizeAtContinueUrl => true;

        public override PaymentFormResult GenerateForm(OrderReadOnly order, string continueUrl, string cancelUrl, string callbackUrl, Checkout.comSettings settings)
        {
            return new PaymentFormResult()
            {
                Form = new PaymentForm(continueUrl, FormMethod.Post)
            };
        }

        public override string GetCancelUrl(OrderReadOnly order, Checkout.comSettings settings)
        {
            return string.Empty;
        }

        public override string GetErrorUrl(OrderReadOnly order, Checkout.comSettings settings)
        {
            return string.Empty;
        }

        public override string GetContinueUrl(OrderReadOnly order, Checkout.comSettings settings)
        {
            settings.MustNotBeNull("settings");
            settings.ContinueUrl.MustNotBeNull("settings.ContinueUrl");

            return settings.ContinueUrl;
        }

        public override CallbackResult ProcessCallback(OrderReadOnly order, HttpRequestBase request, Checkout.comSettings settings)
        {
            return new CallbackResult
            {
                TransactionInfo = new TransactionInfo
                {
                    AmountAuthorized = order.TransactionAmount.Value,
                    TransactionFee = 0m,
                    TransactionId = Guid.NewGuid().ToString("N"),
                    PaymentStatus = PaymentStatus.Authorized
                }
            };
        }
    }
}
