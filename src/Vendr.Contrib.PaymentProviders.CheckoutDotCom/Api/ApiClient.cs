using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api.Payments;

namespace Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api
{
    using Models;
    using System.Collections.Generic;

    public class ApiClient
    {
        private ClientConfig _config;

        public ApiClient(ClientConfig config)
        {
            _config = config;
        }

        public PaymentPageSessionResponse CreatePaymentSession(PaymentPageSessionRequest data)
        {
            return Request("/hosted-payments", (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<PaymentPageSessionResponse>());
        }

        public GetPaymentResponse GetPaymentDetails(string id)
        {
            return Request($"/payments/{id}", (req) => req
                .WithHeader("Content-Type", "application/json")
                .GetJsonAsync<GetPaymentResponse>());
        }

        public IEnumerable<PaymentAction> GetPaymentActions(string id)
        {
            return Request($"/payments/{id}/actions", (req) => req
                .WithHeader("Content-Type", "application/json")
                .GetJsonAsync<IEnumerable<PaymentAction>>());
        }

        public CaptureResponse CapturePayment(string id, CaptureRequest data)
        {
            return Request($"/payments/{id}/captures", (req) => req
                .PostJsonAsync(data)
                .ReceiveJson<CaptureResponse>());
        }

        public RefundResponse RefundPayment(string id, RefundRequest data)
        {
            return Request($"/payments/{id}/refunds", (req) => req
                .PostJsonAsync(data)
                .ReceiveJson<RefundResponse>());
        }

        public VoidResponse VoidPayment(string id, VoidRequest data)
        {
            return Request($"/payments/{id}/voids", (req) => req
                .PostJsonAsync(data)
                .ReceiveJson<VoidResponse>());
        }

        private TResult Request<TResult>(string url, Func<IFlurlRequest, Task<TResult>> func)
        {
            var result = default(TResult);

            try
            {
                var req = new FlurlRequest(_config.BaseUrl + url)
                        .ConfigureRequest(x =>
                        {
                            var jsonSettings = new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                DefaultValueHandling = DefaultValueHandling.Include,
                                MissingMemberHandling = MissingMemberHandling.Ignore
                            };
                            x.JsonSerializer = new NewtonsoftJsonSerializer(jsonSettings);
                        })
                        .WithHeader("Authorization", _config.Authorization);

                result = func.Invoke(req).Result;
            }
            catch (FlurlHttpException ex)
            {
                throw;
            }

            return result;
        }
    }
}