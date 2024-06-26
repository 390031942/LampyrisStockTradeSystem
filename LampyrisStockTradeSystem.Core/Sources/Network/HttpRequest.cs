﻿/*
** Author: wushuhong
** Contact: gameta@qq.com
** Description: 异步HTTP请求实现
*/

using LampyrisStockTradeSystem;

namespace LampyrisStockTradeSystemInternal
{
    public class HttpRequestInternal
    {
        private HttpClient _client;

        public HttpRequestInternal()
        {
            _client = new HttpClient();
        }

        public void Get(string url, Action<string> callback = null)
        {
            Task.Run(async () =>
            {
                HttpResponseMessage response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string rawJsonString = await response.Content.ReadAsStringAsync();
                    if (callback != null)
                    {
                        callback(rawJsonString);
                    }
                }

                HttpRequest.Recycle(this);
            });
        }

        public void GetSync(string url, Action<string> callback)
        { 
            HttpResponseMessage response = _client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                string rawJsonString = response.Content.ReadAsStringAsync().Result;
                if(callback != null)
                {
                    callback(rawJsonString);
                }
            }
        }
    }
}

namespace LampyrisStockTradeSystem
{
    using HttpRequestInternal = LampyrisStockTradeSystemInternal.HttpRequestInternal;

    public static class HttpRequest
    {
        private static Stack<HttpRequestInternal> ms_httpRequestInternals = new Stack<HttpRequestInternal>();

        private static HttpRequestInternal m_httpRequestSync = new HttpRequestInternal();

        public static void Get(string url, Action<string> callback)
        {
            lock (ms_httpRequestInternals)
            {
                if (ms_httpRequestInternals.TryPop(out HttpRequestInternal httpRequest))
                {
                    httpRequest.Get(url, callback);
                }
                else
                {
                    httpRequest = new HttpRequestInternal();
                    httpRequest.Get(url, callback);
                }
            }
        }

        public static void GetSync(string url, Action<string> callback = null)
        {
            m_httpRequestSync.GetSync(url, callback);
        }

        public static void Recycle(HttpRequestInternal httpRequest)
        {
            ms_httpRequestInternals.Push(httpRequest);
        }
    }
}