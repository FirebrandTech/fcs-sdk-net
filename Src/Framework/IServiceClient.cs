// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Collections.Generic;
using System.Net;
using ServiceStack;

namespace Fcs.Framework {
    public class Headers : Dictionary<string, string> {}

    public interface IServiceClient : IDisposable {
        TResponse Post<TResponse>(IReturn<TResponse> request,
                                  Headers requestHeaders,
                                  Headers responseHeaders);

        TResponse Delete<TResponse>(IReturn<TResponse> request,
                                    Headers requestHeaders,
                                    Headers responseHeaders);

        TResponse Post<TResponse>(object request,
                                  Headers requestHeaders,
                                  Headers responseHeaders);
    }

    public interface IServiceClientFactory {
        IServiceClient CreateClient(string url);
    }

    public class JsonServiceClientFactory : IServiceClientFactory {
        public IServiceClient CreateClient(string url) {
            return new JsonServiceClient(url);
        }
    }

    public class JsonServiceClient : IServiceClient {
        private readonly ServiceStack.JsonServiceClient _client;

        public JsonServiceClient(string url) {
            this._client = new ServiceStack.JsonServiceClient(url)
                           {
                               AllowAutoRedirect = false,
                               StoreCookies = false,
                               CookieContainer = null
                           };
        }

        public TResponse Post<TResponse>(IReturn<TResponse> request,
                                         Headers requestHeaders,
                                         Headers responseHeaders) {
            if (requestHeaders != null) this._client.RequestFilter = RequestFilter(requestHeaders);
            if (responseHeaders != null) this._client.ResponseFilter = ResponseFilter(responseHeaders);
            return this._client.Post(request);
        }

        public TResponse Post<TResponse>(object request,
                                         Headers requestHeaders,
                                         Headers responseHeaders) {
            if (requestHeaders != null) this._client.RequestFilter = RequestFilter(requestHeaders);
            if (responseHeaders != null) this._client.ResponseFilter = ResponseFilter(responseHeaders);
            return this._client.Post<TResponse>(request);
        }

        public TResponse Delete<TResponse>(IReturn<TResponse> request,
                                           Headers requestHeaders,
                                           Headers responseHeaders) {
            this._client.RequestFilter = RequestFilter(requestHeaders);
            return this._client.Delete(request);
        }

        public void Dispose() {
            this._client.Dispose();
        }

        private static Action<HttpWebRequest> RequestFilter(Headers headers) {
            return r => {
                       if (headers == null) return;
                       foreach (var header in headers) {
                           r.Headers.Add(header.Key, header.Value);
                       }
                   };
        }

        private static Action<HttpWebResponse> ResponseFilter(Headers headers) {
            return r => {
                       if (headers == null) return;
                       foreach (string key in r.Headers) {
                           var value = r.Headers[key];
                           headers[key] = value;
                       }
                   };
        }
    }
}