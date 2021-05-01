using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vendr.Core;
using Vendr.Core.Models;
using Vendr.Core.Web;
using Vendr.Core.Web.Api;
using Vendr.Core.Web.PaymentProviders;

namespace Vendr.Contrib.PaymentProviders.CheckoutDotCom
{
    [PaymentProvider("checkout-dot-com", "Checkout.com", "Checkout.com payment provider for one time payments")]
    public class CheckoutDotComPaymentProvider : CheckoutDotComPaymentProviderBase<CheckoutDotComSettings>
    {
        public CheckoutDotComPaymentProvider(VendrContext vendr)
            : base(vendr)
        { }

        public override bool CanCancelPayments => true;
        public override bool CanCapturePayments => true;
        public override bool CanRefundPayments => true;
        public override bool CanFetchPaymentStatus => true;

        // We'll finalize via webhook callback
        public override bool FinalizeAtContinueUrl => false;

        public override IEnumerable<TransactionMetaDataDefinition> TransactionMetaDataDefinitions => new[]{
            new TransactionMetaDataDefinition("checkoutSessionId", "Checkout.com Session ID")
        };

        public override PaymentFormResult GenerateForm(OrderReadOnly order, string continueUrl, string cancelUrl, string callbackUrl, CheckoutDotComSettings settings)
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

                var config = GetClientConfig(settings);
                var client = new Api.ApiClient(config);

                var products = order.OrderLines.Select(x => new Api.Models.Product
                {
                    Name = x.Name,
                    Quantity = (int)x.Quantity,
                    Price = (int)AmountToMinorUnits(x.TotalPrice.Value.WithoutTax)
                })
                .ToList();

                Api.Models.Phone phone = null;

                if (!string.IsNullOrWhiteSpace(settings.BillingPhonePropertyAlias) &&
                    !string.IsNullOrWhiteSpace(order.Properties[settings.BillingPhonePropertyAlias]))
                {
                    phone = new Api.Models.Phone
                    {
                        CountryCode = null,
                        Number = order.Properties[settings.BillingPhonePropertyAlias]
                    };
                }

                var request = new Api.Models.PaymentPageSessionRequest
                {
                    Amount = orderAmount,
                    Currency = currencyCode,
                    Reference = order.OrderNumber,
                    Billing = new Api.Models.Billing
                    {
                        Address = new Api.Models.Address
                        {
                            Line1 = !string.IsNullOrWhiteSpace(settings.BillingAddressLine1PropertyAlias)
                                ? order.Properties[settings.BillingAddressLine1PropertyAlias] : null,
                            Line2 = !string.IsNullOrWhiteSpace(settings.BillingAddressLine2PropertyAlias)
                                ? order.Properties[settings.BillingAddressLine2PropertyAlias] : null,
                            Zip = !string.IsNullOrWhiteSpace(settings.BillingAddressZipCodePropertyAlias)
                                ? order.Properties[settings.BillingAddressZipCodePropertyAlias] : null,
                            City = !string.IsNullOrWhiteSpace(settings.BillingAddressCityPropertyAlias)
                                ? order.Properties[settings.BillingAddressCityPropertyAlias] : null,
                            State = !string.IsNullOrWhiteSpace(settings.BillingAddressStatePropertyAlias)
                                ? order.Properties[settings.BillingAddressStatePropertyAlias] : null,
                            Country = billingCountry?.Code
                        },
                        Phone = phone
                    },
                    Customer = new Api.Models.Customer
                    {
                        Email = order.CustomerInfo.Email,
                        Name = order.CustomerInfo.FirstName + " " + order.CustomerInfo.LastName
                    },
                    Products = products,
                    SuccessUrl = continueUrl,
                    FailureUrl = cancelUrl, // GetErrorUrl(order, settings);
                    CancelUrl = cancelUrl,
                    Metadata = metadata
                };

                // Create payment session
                var paymentSession = client.CreatePaymentSession(request);
                if (paymentSession != null)
                {
                    // Get session url
                    paymentFormLink = paymentSession.GetLink("redirect").Href;
                }
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<CheckoutDotComPaymentProvider>(ex, "Checkout.com - error creating payment.");
                throw ex;
            }

            return new PaymentFormResult()
            {
                Form = new PaymentForm(paymentFormLink, FormMethod.Get)
            };
        }

        public override CallbackResult ProcessCallback(OrderReadOnly order, HttpRequestBase request, CheckoutDotComSettings settings)
        {
            try
            {
                // Process callback

                var webhookEvent = GetWebhookEvent(request, settings);
                if (webhookEvent != null)
                {
                    return CallbackResult.Ok(new TransactionInfo
                    {
                        TransactionId = order.TransactionInfo.TransactionId,
                        AmountAuthorized = order.TransactionAmount.Value,
                        PaymentStatus = PaymentStatus.Authorized
                    });
                }
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<CheckoutDotComPaymentProvider>(ex, "Checkout.com - ProcessCallback");
            }

            return CallbackResult.BadRequest();
        }

        public override ApiResult FetchPaymentStatus(OrderReadOnly order, CheckoutDotComSettings settings)
        {
            try
            {
                var config = GetClientConfig(settings);
                var client = new Api.ApiClient(config);

                // Get payment details
                var payment = client.GetPaymentDetails(order.TransactionInfo.TransactionId);

                return new ApiResult()
                {
                    TransactionInfo = new TransactionInfoUpdate()
                    {
                        TransactionId = payment.Id,
                        PaymentStatus = GetPaymentStatus(payment)
                    }
                };
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<CheckoutDotComPaymentProvider>(ex, "Checkout.com - FetchPaymentStatus");
            }

            return ApiResult.Empty;
        }

        public override ApiResult CancelPayment(OrderReadOnly order, CheckoutDotComSettings settings)
        {
            try
            {
                var config = GetClientConfig(settings);
                var client = new Api.ApiClient(config);

                var data = new Api.Payments.VoidRequest
                {
                    
                };

                // Cancel payment
                var payment = client.VoidPayment(order.TransactionInfo.TransactionId, data);
                
                return new ApiResult()
                {
                    TransactionInfo = new TransactionInfoUpdate()
                    {
                        TransactionId = order.TransactionInfo.TransactionId, //GetTransactionId(payment),
                        PaymentStatus = PaymentStatus.Cancelled //GetPaymentStatus(payment)
                    }
                };
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<CheckoutDotComPaymentProvider>(ex, "Checkout.com - CancelPayment");
            }

            return ApiResult.Empty;
        }

        public override ApiResult CapturePayment(OrderReadOnly order, CheckoutDotComSettings settings)
        {
            try
            {
                var config = GetClientConfig(settings);
                var client = new Api.ApiClient(config);

                var data = new Api.Payments.CaptureRequest
                {
                    Amount = AmountToMinorUnits(order.TransactionInfo.AmountAuthorized.Value)
                };

                // Capture payment
                var payment = client.CapturePayment(order.TransactionInfo.TransactionId, data);

                return new ApiResult()
                {
                    TransactionInfo = new TransactionInfoUpdate()
                    {
                        TransactionId = order.TransactionInfo.TransactionId, //GetTransactionId(payment),
                        PaymentStatus = PaymentStatus.Captured //GetPaymentStatus(payment)
                    }
                };
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<CheckoutDotComPaymentProvider>(ex, "Checkout.com - CapturePayment");
            }

            return ApiResult.Empty;
        }

        public override ApiResult RefundPayment(OrderReadOnly order, CheckoutDotComSettings settings)
        {
            try
            {
                var config = GetClientConfig(settings);
                var client = new Api.ApiClient(config);

                var data = new Api.Payments.RefundRequest
                {
                    Amount = AmountToMinorUnits(order.TransactionInfo.AmountAuthorized.Value)
                };

                // Refund payment
                var refund = client.RefundPayment(order.TransactionInfo.TransactionId, data);

                return new ApiResult()
                {
                    TransactionInfo = new TransactionInfoUpdate()
                    {
                        TransactionId = order.TransactionInfo.TransactionId, //GetTransactionId(refund),
                        PaymentStatus = PaymentStatus.Refunded //GetPaymentStatus(refund)
                    }
                };
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<CheckoutDotComPaymentProvider>(ex, "Checkout.com - RefundPayment");
            }

            return ApiResult.Empty;
        }
    }
}
