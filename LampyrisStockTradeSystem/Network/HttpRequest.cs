using LampyrisStockTradeSystem;
using LampyrisStockTradeSystemInternal;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LampyrisStockTradeSystemInternal
{
    public class HttpRequestInternal
    {
        private HttpClient _client;

        public HttpRequestInternal()
        {
            _client = new HttpClient();
        }

        public void Get(string url, Action<string> callback)
        {
            Task.Run(async () =>
            {
                HttpResponseMessage response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    callback(result);
                }

                HttpRequest.Recycle(this);
            });
        }
    }
}

namespace LampyrisStockTradeSystem
{
    using HttpRequestInternal = LampyrisStockTradeSystemInternal.HttpRequestInternal;

    public static class HttpRequest
    {
        private static Stack<HttpRequestInternal> ms_httpRequestInternals = new Stack<HttpRequestInternal>();

        public static void Get(string url, Action<string> callback)
        {
            if(ms_httpRequestInternals.TryPop(out HttpRequestInternal httpRequest))
            {
                httpRequest.Get(url, callback);
            }
            else
            {
                httpRequest = new HttpRequestInternal();
                httpRequest.Get(url, callback);
            }
        }

        public static void Recycle(HttpRequestInternal httpRequest)
        {
            ms_httpRequestInternals.Push(httpRequest);
        }
    }
}