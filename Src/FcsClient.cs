﻿// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using Cloud.Api.V2.Model;
using Fcs.Framework;
using Fcs.Model;
using ServiceStack.Logging;
using StringExtensions = ServiceStack.StringExtensions;

namespace Fcs {
    public class FcsClient : IDisposable {
        private readonly ILog _logger = LogManager.GetLogger("FcsClient");

        private static readonly object Sync = new object();

        private const string AppHeader = "X-Fcs-App";
        private string _apiUrl;
        private string _appId;
        private string _clientId;
        private string _clientSecret;
        private string _tokenCookie;
        private string _userCookie;
        private IServiceClient _client;
        private IContext _context;
        private string _token;
        private DateTime? _tokenExpires;
        private string _user;

        public FcsClient() {
            var settings = ConfigurationManager.AppSettings;
            var clientId = settings["FcsClientId"];
            var clientSecret = settings["FcsClientSecret"];
            var appId = settings["FcsAppId"];
            var url = settings["FcsApiUrl"];

            if (string.IsNullOrWhiteSpace(clientId) ||
                string.IsNullOrWhiteSpace(clientSecret) ||
                string.IsNullOrWhiteSpace(appId)) {
                throw new InvalidOperationException("FcsClient is not configured properly!  Please add the following to your appSettings: FcsClientId, FcsClientSecret, FcsAppId");
            }
            this.Init(clientId, clientSecret, appId, url);
        }

        public FcsClient(string clientId, string clientSecret, string appId = "fcs", string apiUrl = null) {
            this.Init(clientId, clientSecret, appId, apiUrl);
        }

        private void Init(string clientId, string clientSecret, string appId, string apiUrl) {
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
            this._appId = appId ?? "fcs";
            this._tokenCookie = appId + "-token";
            this._userCookie = appId + "-user";
            this.ServiceClientFactory = new JsonServiceClientFactory();
        }

        public static void StartSession() {
            using (var fcs = new FcsClient()) {
                fcs.Auth();
            }
        }

        public static ILogFactory LogFactory {
            get { return LogManager.LogFactory; }
            set { LogManager.LogFactory = value; }
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
            if (this._client == null || !disposing) return;
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
            lock (Sync) {
                if (request == null) {
                    request = new AuthRequest
                              {
                                  ClientId = this._clientId,
                                  ClientSecret = this._clientSecret,
                                  UserName = this.Context.CurrentUserName
                              };
                }
                if (this.IsAuthed(request)) return null;
                var headers = this.GetHeaders();
                this._logger.DebugFormat("POST AUTH REQUEST: {0}", StringExtensions.ToJsv(request));
                this._logger.DebugFormat("POST AUTH REQUEST HEADERS: {0}", StringExtensions.ToJsv(headers));

                var auth = this.ServiceClient.Post(request, this.GetHeaders());
                this._logger.DebugFormat("POST AUTH RESPONSE: {0}", StringExtensions.ToJsv(auth));

                this._token = auth.Token;
                this._tokenExpires = auth.Expires;
                this._user = request.UserName;
                this.Context.SetResponseCookie(this._tokenCookie, auth.Token, auth.Expires ?? DateTime.MinValue);
                if (!StringExtensions.IsNullOrEmpty(this._user)) this.Context.SetResponseCookie(this._userCookie, this._user, auth.Expires ?? DateTime.MinValue);

                return auth;
            }
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
            this.Auth(this._user);
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