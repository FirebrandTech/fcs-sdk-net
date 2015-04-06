// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Collections.Generic;
using System.Web;
using Cloud.Api.V2.Model;
using Fcs.Framework;
using Fcs.Model;
using StringExtensions = ServiceStack.StringExtensions;

namespace Fcs {
    public class FcsClient : IDisposable {
        private const string AppHeader = "X-Fcs-App";
        private readonly string _apiUrl;
        private readonly string _appId;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _tokenCookie;
        private readonly string _userCookie;
        private IServiceClient _client;
        private IContext _context;
        private string _token;
        private DateTime? _tokenExpires;
        private string _user;

        public FcsClient(string clientId, string clientSecret, string appId = "fcs", string apiUrl = null) {
            if (apiUrl != null && apiUrl.ToLower() == "auto") {
                var req = HttpContext.Current.Request;
                if (req.ApplicationPath == null) return;
                apiUrl = req.Url.Scheme + "://" +
                         req.Url.Authority +
                         req.ApplicationPath.TrimEnd('/') +
                         "/api/v2";
            }
            this._apiUrl = apiUrl ?? "https://cloud.firebrandtech.com/api/v2";
            this._clientId = clientId;
            this._clientSecret = clientSecret;
            this._appId = appId;
            this._tokenCookie = appId + "-token";
            this._userCookie = appId + "-user";
            this.ServiceClientFactory = new JsonServiceClientFactory();
        }

        public string Token {
            get { return this._token; }
        }

        public DateTime? TokenExpires {
            get { return this._tokenExpires; }
        }

        public string User {
            get { return this._user; }
        }

        public IContext Context {
            private get {
                if (this._context != null) return this._context;
                return this._context = new AspNetContext();
            }
            set { this._context = value; }
        }

        public IServiceClientFactory ServiceClientFactory { get; set; }

        private IServiceClient ServiceClient {
            get {
                if (this._client != null) return this._client;

                this._client = this.ServiceClientFactory.CreateClient(this._apiUrl);

                return this._client;
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (this._client == null) return;
            this._client.Dispose();
            this._client = null;
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
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
                              UserName = this.Context.CurrentUserName
                          };
            }
            if (this.IsAuthed(request)) return null;
            var auth = this.ServiceClient.Post(request, this.GetHeaders());
            this._token = auth.Token;
            this._tokenExpires = auth.Expires;
            this._user = request.UserName;
            this.Context.SetResponseCookie(this._tokenCookie, auth.Token, auth.Expires ?? DateTime.MinValue);
            if (!StringExtensions.IsNullOrEmpty(this._user)) this.Context.SetResponseCookie(this._userCookie, this._user, auth.Expires ?? DateTime.MinValue);

            return auth;
        }

        private bool IsAuthed(AuthRequest request) {
            var token = this.GetValidToken();
            if (StringExtensions.IsNullOrEmpty(token)) {
                request.ClientId = this._clientId;
                request.ClientSecret = this._clientSecret;
                return false;
            }
            request.ClientId = null;
            request.ClientSecret = null;
            var user = this.GetAuthedUser();
            if (StringExtensions.IsNullOrEmpty(request.UserName) &&
                StringExtensions.IsNullOrEmpty(user)) return true;

            return string.Equals(request.UserName, user, StringComparison.OrdinalIgnoreCase);
        }

        public AuthResponse Unauth() {
            this.Context.SetResponseCookie(this._tokenCookie, null, DateTime.UtcNow.AddDays(-1));
            return this.ServiceClient.Delete(new AuthRequest(), this.GetHeaders());
        }

        public object PlaceOrder(Order order) {
            this.Auth(order.UserName ?? (order.User ?? new User()).Email);
            return this.ServiceClient.Post(order, this.GetHeaders());
        }

        public Catalog PublishCatalog(Catalog catalog) {
            this.Auth();
            return this.ServiceClient.Post(catalog, this.GetHeaders());
        }

        private Dictionary<string, string> GetHeaders() {
            var headers = new Dictionary<string, string>
                          {
                              {AppHeader, this._appId}
                          };
            var token = this.GetValidToken();
            this.GetAuthedUser(); // Call this to update this._user from cookie if possible.
            if (StringExtensions.IsNullOrEmpty(token)) return headers;
            headers.Add("Authorization", String.Format("Bearer {0}", token));
            headers.Add("X-NoRedirect", "true");
            return headers;
        }

        private string GetValidToken() {
            if (this.TokenIsValid()) return this._token;
            var cookie = this.Context.GetRequestCookie(this._tokenCookie);
            if (cookie == null) return null;
            this._token = cookie.Value;
            this._tokenExpires = cookie.Expires;
            return this.TokenIsValid() ? this._token : null;
        }

        private bool TokenIsValid() {
            return !StringExtensions.IsNullOrEmpty(this._token) && DateTime.UtcNow < this._tokenExpires;
        }

        private string GetAuthedUser() {
            if (!StringExtensions.IsNullOrEmpty(this._user)) return this._user;
            var cookie = this.Context.GetRequestCookie(this._userCookie);
            if (cookie == null) return null;
            this._user = cookie.Value;
            return this._user;
        }
    }
}