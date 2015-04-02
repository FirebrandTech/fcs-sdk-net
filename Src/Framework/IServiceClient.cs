// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Collections.Generic;
using System.Net;
using ServiceStack;

namespace Fcs.Framework {
    public interface IServiceClient : IDisposable {
        TResponse Post<TResponse>(IReturn<TResponse> request, Dictionary<string, string> headers);
        TResponse Delete<TResponse>(IReturn<TResponse> request, Dictionary<string, string> headers);
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
            this._client = new ServiceStack.JsonServiceClient(url);
        }

        public TResponse Post<TResponse>(IReturn<TResponse> request, Dictionary<string, string> headers) {
            this._client.RequestFilter = RequestFilter(headers);
            return this._client.Post(request);
        }

        public TResponse Delete<TResponse>(IReturn<TResponse> request, Dictionary<string, string> headers) {
            this._client.RequestFilter = RequestFilter(headers);
            return this._client.Delete(request);
        }

        public void Dispose() {
            this._client.Dispose();
        }

        private static Action<HttpWebRequest> RequestFilter(Dictionary<string, string> headers) {
            return r => {
                       if (headers == null) return;
                       foreach (var header in headers) {
                           r.Headers.Add(header.Key, header.Value);
                       }
                   };
        }
    }
}