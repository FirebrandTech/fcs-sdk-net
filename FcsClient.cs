// Copyright © 2010-2014 Firebrand Technologies

using System;
using System.Net;
using System.Web;
using System.Web.Security;
using Cloud.Api.V2.Model;
using ServiceStack;

namespace Fcs {
    public sealed class FcsClient : IDisposable {
        private const string ApiKeyHeader = "X-Fcs-ApiKey";
        private const string NoRedirectHeader = "X-NoRedirect";
        private const string FcsTicketCookie = "fcs-ticket";

        private readonly string _apiKey;
        private readonly string _serviceUrl;
        private readonly string _cookieDomain;
        private JsonServiceClient _client;

        public FcsClient(string serviceUrl, string apiKey) {
            this._serviceUrl = serviceUrl;
            this._apiKey = apiKey;
            this._cookieDomain = new Uri(this._serviceUrl).Authority;
        }

        private JsonServiceClient Client {
            get {
                if (this._client != null) return this._client;

                this._client = new JsonServiceClient(this._serviceUrl)
                               {
                                   RequestFilter = r => {
                                                       r.Headers.Add(ApiKeyHeader, this._apiKey);
                                                       r.Headers.Add(NoRedirectHeader, "true");
                                                            if (HttpContext.Current != null) {
                                                                var cookies = HttpContext.Current.Request.Cookies;
                                                                for (var i = 0; i < cookies.Count; i++)
                                                                {
                                                                    var cookie = cookies[i];
                                                                    if (cookie == null || cookie.Name != FcsTicketCookie)
                                                                        continue;
                                                                    r.CookieContainer.Add(new Cookie(cookie.Name,
                                                                        cookie.Value)
                                                                    {
                                                                        Domain = this._cookieDomain
                                                                    });
                                                                }
                                                            }
                                   },
                                   ResponseFilter = r => {
                                                        foreach (Cookie cookie in r.Cookies) {
                                                            if (cookie.Name != FcsTicketCookie) continue;
                                                            SetCookie(cookie.Name, cookie.Value, cookie.Expires);
                                                        }
                                                    }
                               };

                return this._client;
            }
        }

        public AuthResponse Auth(AuthRequest request) {
            return this.Client.Post(request);
        }

        public AuthResponse Unauth() {
            return this.Client.Delete(new AuthRequest());
        }

        public object PlaceOrder(OrderDto order) {
            return this.Client.Post(order);
        }

        public void Dispose() {
            if (this._client == null) return;
            this._client.Dispose();
            this._client = null;
        }

        private static void SetCookie(string name, string value, DateTime expires) {
            var cookies = HttpContext.Current.Response.Cookies;
            value = HttpUtility.UrlEncode(value);
            var cookie = new HttpCookie(name, value) {
                Path = FormsAuthentication.FormsCookiePath,
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL,
                Expires = expires
            };

            if (!string.IsNullOrWhiteSpace(FormsAuthentication.CookieDomain)) {
                cookie.Domain = FormsAuthentication.CookieDomain;
            }
            cookies.Remove(name);
            cookies.Add(cookie);
        }

    }
}