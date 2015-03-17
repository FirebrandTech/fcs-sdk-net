// Copyright © 2010-2014 Firebrand Technologies

using System;
using System.Net;
using System.Web;
using System.Web.Security;
using Fcs.Model;
using ServiceStack;

namespace Fcs {
    public sealed class FcsClient : IDisposable {
        private const string AppHeader = "X-Fcs-App";
        private readonly string _appId;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _cookieDomain;
        private readonly string _serviceUrl;
        private readonly string _tokenCookie;
        private JsonServiceClient _client;

        public FcsClient(string serviceUrl, string clientId, string clientSecret, string appId) {
            this._serviceUrl = serviceUrl;
            this._clientId = clientId;
            this._clientSecret = clientSecret;
            this._appId = appId;
            this._tokenCookie = appId + "-token";
            this._cookieDomain = new Uri(this._serviceUrl).Authority;
        }

        private JsonServiceClient Client {
            get {
                if (this._client != null) return this._client;

                this._client = new JsonServiceClient(this._serviceUrl)
                               {
                                   RequestFilter = this.OnRequest
                               };

                return this._client;
            }
        }

        public void Dispose() {
            if (this._client == null) return;
            this._client.Dispose();
            this._client = null;
        }

        public void Auth(string userName = null) {
            this.Auth(userName != null ? new AuthRequest {UserName = userName} : null);
        }

        public AuthResponse Auth(AuthRequest request) {
            if (request == null) {
                request = new AuthRequest
                          {
                              ClientId = this._clientId,
                              ClientSecret = this._clientSecret,
                              UserName = GetUserName()
                          };
            }
            var auth = this.Client.Post(request);
            SetCookie(this._tokenCookie, auth.Token, auth.Expires ?? DateTime.MinValue, false);

            return auth;
        }

        public AuthResponse Unauth() {
            RemoveCookie(this._tokenCookie);
            return this.Client.Delete(new AuthRequest());
        }

        public object PlaceOrder(Order order) {
            return this.Client.Post(order);
        }

        private void OnRequest(HttpWebRequest r) {
            r.Headers.Add(AppHeader, this._appId);
            if (HttpContext.Current == null) return;
            var token = GetCookie(this._tokenCookie);
            if (token.IsNullOrEmpty()) return;
            r.Headers.Add("Authorization", String.Format("Bearer {0}", token));
        }

        private static string GetUserName() {
            if (HttpContext.Current == null) return null;
            return HttpContext.Current.User.Identity.Name;
        }

        private static void RemoveCookie(string name) {
            var cookies = HttpContext.Current.Response.Cookies;
            cookies.Remove(name);
        }

        private static string GetCookie(string name) {
            if (HttpContext.Current == null) return null;
            var cookies = HttpContext.Current.Request.Cookies;
            var cookie = cookies.Get(name);
            if (cookie == null) return null;
            if (DateTime.UtcNow > cookie.Expires) return null;
            return cookie.Value;
        }

        private static void SetCookie(string name, string value, DateTime expires, bool httpOnly = true) {
            var cookies = HttpContext.Current.Response.Cookies;
            value = HttpUtility.UrlEncode(value);
            var cookie = new HttpCookie(name, value)
                         {
                             Path = FormsAuthentication.FormsCookiePath,
                             HttpOnly = httpOnly,
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