using Flurl.Http;
using System;
using System.Threading.Tasks;

namespace Vendr.Contrib.PaymentProviders.CheckoutDotCom.Api
{
    using Flurl.Http.Configuration;
    using Models;
    using Newtonsoft.Json;

    public class ApiClient
    {
        private ClientConfig _config;

        public ApiClient(ClientConfig config)
        {
            _config = config;
        }

        public PaymentPageSessionResponse CreateChargeSession(PaymentPageSessionRequest data)
        {
            return Request("/hosted-payments", (req) => req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data)
                .ReceiveJson<PaymentPageSessionResponse>());
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
}
