using Checkout;
using Checkout.Payments;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Vendr.Core;
using Vendr.Core.Models;
using Vendr.Core.Web;
using Vendr.Core.Web.Api;
using Vendr.Core.Web.PaymentProviders;
using CheckoutSdk = Checkout;

namespace Vendr.Contrib.PaymentProviders.CheckoutDotCom
{
    [PaymentProvider("checkout-dot-com", "Checkout.com", "Checkout.com payment provider", Icon = "icon-invoice")]
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

            var metadata = new Dictionary<string, object>
            {
                { "orderReference", order.GenerateOrderReference().ToString() },
                { "orderId", order.Id.ToString("D") },
                { "orderNumber", order.OrderNumber }
            };

            string paymentFormLink = string.Empty;

            try
            {
                // https://api-reference.checkout.com/#operation/createAHostedPaymentsSession

                //var api = CheckoutApi.Create(settings.SecretKey, settings.TestMode);
                //var client = GetClient(settings);

                var config = GetClientConfig(settings);
                var client = new Api.ApiClient(config);

                var request = new Api.Models.PaymentPageSessionRequest
                {
                    Amount = orderAmount,
                    Currency = currencyCode,
                    Reference = order.OrderNumber,
                    Billing = new Api.Models.Address
                    {
                        Line1 = "",
                        Line2 = "",
                        Zip = "",
                        City = "",
                        State = "",
                        Country = billingCountry?.Code
                    },
                   
                    Customer = new Api.Models.Customer
                    {
                        Email = order.CustomerInfo.Email,
                        Name = order.CustomerInfo.FirstName + " " + order.CustomerInfo.LastName
                    },
                    SuccessUrl = continueUrl,
                    FailureUrl = settings.ErrorUrl,
                    CancelUrl = cancelUrl,
                    Metadata = metadata
                };

                // Create payment session
                var paymentSession = client.CreateChargeSession(request);
                if (paymentSession != null)
                {
                    // Get session url
                    paymentFormLink = paymentSession.Links.Redirect.Href;
                }
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<CheckoutPaymentProvider>(ex, "Checkout.com - error creating payment.");
                throw ex;
            }

            return new PaymentFormResult()
            {
                Form = new PaymentForm(paymentFormLink, FormMethod.Get)
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
