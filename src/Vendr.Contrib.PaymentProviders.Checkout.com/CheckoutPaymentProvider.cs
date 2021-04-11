using Checkout;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Vendr.Core;
using Vendr.Core.Models;
using Vendr.Core.Web;
using Vendr.Core.Web.Api;
using Vendr.Core.Web.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders.Checkout.com
{
    [PaymentProvider("checkout", "Checkout.com", "Checkout.com payment provider", Icon = "icon-invoice")]
    public class CheckoutPaymentProvider : CheckoutPaymentProviderBase<CheckoutSettings>
    {
        public CheckoutPaymentProvider(VendrContext vendr)
            : base(vendr)
        { }

        public override bool CanCancelPayments => true;
        public override bool CanCapturePayments => true;
        public override bool CanRefundPayments => true;
        public override bool CanFetchPaymentStatus => true;

        public override bool FinalizeAtContinueUrl => true;

        public override IEnumerable<TransactionMetaDataDefinition> TransactionMetaDataDefinitions => new[]{
            new TransactionMetaDataDefinition("checkoutSessionId", "Checkout.com Session ID")
        };

        public override PaymentFormResult GenerateForm(OrderReadOnly order, string continueUrl, string cancelUrl, string callbackUrl, CheckoutSettings settings)
        {
            var currency = Vendr.Services.CurrencyService.GetCurrency(order.CurrencyId);
            var currencyCode = currency.Code.ToUpperInvariant();

            // Ensure currency has valid ISO 4217 code
            if (!Iso4217.CurrencyCodes.ContainsKey(currencyCode))
            {
                throw new Exception("Currency must be a valid ISO 4217 currency code: " + currency.Name);
            }

            var orderAmount = AmountToMinorUnits(order.TransactionAmount.Value);

            //var paymentMethods = settings.PaymentMethods?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            //       .Where(x => !string.IsNullOrWhiteSpace(x))
            //       .Select(s => s.Trim())
            //       .ToList();

            var billingCountry = order.PaymentInfo.CountryId.HasValue
                    ? Vendr.Services.CountryService.GetCountry(order.PaymentInfo.CountryId.Value)
                    : null;

            var metadata = new Dictionary<string, string>
            {
                { "orderReference", order.GenerateOrderReference() },
                { "orderId", order.Id.ToString("D") },
                { "orderNumber", order.OrderNumber }
            };

            try
            {
                // https://api-reference.checkout.com/#operation/createAHostedPaymentsSession

                //var api = CheckoutApi.Create(settings.SecretKey, settings.TestMode);
                var client = GetClient(settings);

            }
            catch (Exception ex)
            {
                Vendr.Log.Error<CheckoutPaymentProvider>(ex, $"Request for payment failed::\n{ex.Message}\n");
                throw ex;
            }

            return new PaymentFormResult()
            {
                Form = new PaymentForm(continueUrl, FormMethod.Post)
            };
        }

        public override CallbackResult ProcessCallback(OrderReadOnly order, HttpRequestBase request, CheckoutSettings settings)
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
