using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api.Events;
using Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api.Models;
using Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api.Payments;
using Vendr.Core;
using Vendr.Core.Models;
using Vendr.Core.Web.Api;
using Vendr.Core.Web.PaymentProviders;
using PaymentStatus = Vendr.Core.Models.PaymentStatus;

namespace Vendr.Contrib.PaymentProviders.CheckoutDotCom
{
    public abstract class CheckoutDotComPaymentProviderBase<TSettings> : PaymentProviderBase<TSettings>
        where TSettings : CheckoutDotComSettingsBase, new()
    {
        public CheckoutDotComPaymentProviderBase(VendrContext vendr)
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

        public override OrderReference GetOrderReference(HttpRequestBase request, TSettings settings)
        {
            try
            {
                var webhookEvent = GetWebhookEvent(request, settings);
                if (webhookEvent != null && webhookEvent.Data?.Count > 0)
                {
                    if (webhookEvent.Data.TryGetValue("metadata", out object obj))
                    {
                        var metadata = JObject.FromObject(obj)
                            .ToObject<Dictionary<string, object>>();

                        if (metadata != null)
                        {
                            if (metadata.TryGetValue("orderReference", out object orderReference))
                            {
                                return OrderReference.Parse(orderReference.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Vendr.Log.Error<CheckoutDotComPaymentProvider>(ex, "Checkout.com - GetOrderReference");
                //Vendr.Log.Error<CheckoutDotComPaymentProviderBase<TSettings>>(ex, "Checkout.com - GetOrderReference");
            }

            return base.GetOrderReference(request, settings);
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

        protected EventResponse GetWebhookEvent(HttpRequestBase request, CheckoutDotComSettingsBase settings)
        {
            EventResponse webhookEvent = null;

            if (HttpContext.Current.Items["Vendr_CheckoutDotComEvent"] != null)
            {
                webhookEvent = (EventResponse)HttpContext.Current.Items["Vendr_CheckoutDotComEvent"];
            }
            else
            {
                try
                {
                    if (request.InputStream.CanSeek)
                        request.InputStream.Seek(0, SeekOrigin.Begin);

                    using (var sr = new StreamReader(request.InputStream))
                    {
                        var json = sr.ReadToEnd();

                        // Just validate the webhook signature
                        VerifySignature(request.Headers["CKO-Signature"], settings.SecretKey, json);

                        webhookEvent = JsonConvert.DeserializeObject<EventResponse>(json);

                        HttpContext.Current.Items["Vendr_CheckoutDotComEvent"] = webhookEvent;
                    }
                }
                catch (Exception ex)
                {
                    Vendr.Log.Error<CheckoutDotComPaymentProviderBase<TSettings>>(ex, "Checkout.com - GetWebhookEvent");
                }
            }

            return webhookEvent;
        }

        private void VerifySignature(string signature, string secretKey, string payload)
        {
            // Webhook signatures: https://docs.checkout.com/reporting-and-insights/webhooks#Webhooks-Webhooksignatures

            if (HMACSHA256Hash(secretKey, payload) != signature)
                throw new Exception("The signature of the webhook event could not be verified.");
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