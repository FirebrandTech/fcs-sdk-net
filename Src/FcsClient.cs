﻿// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Web;
using Cloud.Api.V2.Model;
using Fcs.Framework;
using Fcs.Model;
using ServiceStack;
using ServiceStack.Logging;
using IServiceClient = Fcs.Framework.IServiceClient;

// ReSharper disable UnusedMember.Global

namespace Fcs {
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class FcsClient : IDisposable {
        private const string AppHeader = "X-Fcs-App";
        private const string SessionHeader = "X-Fcs-Session";
        //private const string SessionKey = "FCS-TOKEN";
        private const int SessionExpiryDays = 14;
        private static readonly object Sync = new object();
        private static bool _applicationInitialized;
        private static ILog _logger;
        private static FcsToken _appToken;
        private readonly FcsConfig _config;
        private IServiceClient _client;
        private IContext _context;
        private FcsToken _token;

        // ReSharper disable once MemberCanBePrivate.Global
        public FcsClient() : this(new FcsConfig()) {}

        public FcsClient(string clientId, string clientSecret, string app = "fcs", string apiUrl = null)
            : this(new FcsConfig(clientId, clientSecret, app, apiUrl)) {}

        // ReSharper disable once MemberCanBePrivate.Global
        public FcsClient(FcsConfig config) {
            this._config = config;
            this.ServiceClientFactory = new JsonServiceClientFactory();
        }

        public static ILogFactory LogFactory {
            // ReSharper disable once UnusedMember.Global
            get { return LogManager.LogFactory; }
            set { LogManager.LogFactory = value; }
        }

        public FcsConfig Config {
            get { return this._config; }
        }

        public FcsToken Token {
            get { return this._token; }
        }

        public static FcsToken AppToken {
            get { return _appToken; }
        }

        private static ILog Logger {
            get { return _logger ?? (_logger = LogManager.GetLogger("FcsClient")); }
        }

        public IContext Context {
            private get {
                if (this._context != null) return this._context;
                return this._context = new AspNetContext();
            }
            set { this._context = value; }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public IServiceClientFactory ServiceClientFactory { get; set; }

        private IServiceClient ServiceClient {
            get {
                if (this._client != null) return this._client;
                this._client = this.ServiceClientFactory.CreateClient(this._config.ApiUrl);
                return this._client;
            }
        }

        public static void InitApplication(HttpApplication app) {
            lock (Sync) {
                if (_applicationInitialized) return;
                app.PostAuthorizeRequest += PostAuthorizeRequest;
                _applicationInitialized = true;
            }
        }

        private static void PostAuthorizeRequest(object sender, EventArgs args) {
            EnsureAuthorized();
        }

        public static void EnsureAuthorized(bool ignoreContextUser = false) {
            Uri url = HttpContext.Current.Request.Url;
            if (url.PathAndQuery.Contains("/__browserLink/")) return;
            Logger.DebugFormat("EnsureAuthorized: {0}", url);

            using (var fcs = new FcsClient()) {
                fcs.Auth(ignoreContextUser: ignoreContextUser);
            }
        }

        public static void Reset() {
            _appToken = null;
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

        public void Auth(string userName = null, bool ignoreContextUser = false) {
            this.Auth(new AuthRequest
                      {
                          ClientId = this._config.ClientId,
                          ClientSecret = this._config.ClientSecret,
                          UserName = userName
                      },
                      ignoreContextUser);
        }

        public AuthResponse Auth(AuthRequest request, bool ignoreContextUser = false) {
            lock (Sync) {
                FcsToken token = this.GetToken();

                if (token == null) {
                    request.ClientId = this._config.ClientId;
                    request.ClientSecret = this._config.ClientSecret;
                }
                if (request.UserName.IsNullOrWhiteSpace()) {
                    request.UserName = this.Context.CurrentUserName ?? (token ?? new FcsToken()).User;
                }

                if (token != null &&
                    !string.Equals(token.User, request.UserName, StringComparison.OrdinalIgnoreCase) &&
                    !ignoreContextUser) {
                    request.ClientId = null;
                    request.ClientSecret = null;
                    token = null;
                }

                if (token == null) {
                    Headers requestHeaders = this.GetHeaders();
                    Logger.DebugFormat("POST AUTH REQUEST: {0}", StringExtensions.ToJsv(request));
                    Logger.DebugFormat("POST AUTH REQUEST HEADERS: {0}", StringExtensions.ToJsv(requestHeaders));

                    var responseHeaders = new Headers();
                    AuthResponse response = this.ServiceClient.Post(request, requestHeaders, responseHeaders);
                    Logger.DebugFormat("POST AUTH RESPONSE: {0}", StringExtensions.ToJsv(response));
                    Logger.DebugFormat("POST AUTH RESPONSE HEADERS: {0}", StringExtensions.ToJsv(responseHeaders));
                    token = new FcsToken(response);
                }

                this.SaveToken(token);

                return new AuthResponse
                       {
                           Token = token.Value,
                           Expires = token.Expires,
                           Session = token.Session,
                           UserName = token.User
                       };
            }
        }

        /// <summary>
        ///     Save the token information in the following places:  static, instance, and cookies.
        /// </summary>
        /// <param name="token">token information</param>
        private void SaveToken(FcsToken token) {
            this._token = token;

            if (token.User.IsNullOrWhiteSpace()) {
                // Token is app token.  Save it as the static appToken to minimize token creation.
                _appToken = token;
            }
            this.Context.SetResponseCookie(this._config.TokenCookie, token.SerializeToken(), null);
            if (token.Session.IsFull()) {
                this.Context.SetResponseCookie(this._config.SessionCookie,
                                               token.Session,
                                               DateTime.UtcNow.AddDays(SessionExpiryDays));
            }
        }

        public AuthResponse Unauth() {
            AuthResponse response = this.ServiceClient.Delete(new AuthRequest(), this.GetHeaders(), null);
            var token = new FcsToken(response);
            this.SaveToken(token);
            return response;
        }

        public object PlaceOrder(Order order) {
            this.Auth(order.UserName ?? (order.User ?? new User()).Email);
            return this.ServiceClient.Post(order, this.GetHeaders(), null);
        }

        public Catalog PublishCatalog(Catalog catalog) {
            this.Auth();
            Headers headers = this.GetHeaders();
            return this.ServiceClient.Post(catalog, headers, null);
        }

        public CatalogComment PublishCatalogComment(CatalogComment catalogComment) {
            this.Auth();
            Headers headers = this.GetHeaders();
            return this.ServiceClient.Post(catalogComment, headers, null);
        }

        public CatalogCategory PublishCatalogCategory(CatalogCategory catalogCategory) {
            this.Auth();
            Headers headers = this.GetHeaders();
            return this.ServiceClient.Post(catalogCategory, headers, null);
        }

        public CatalogProduct PublishCatalogProduct(CatalogProduct catalogProduct) {
            this.Auth();
            Headers headers = this.GetHeaders();
            return this.ServiceClient.Post(catalogProduct, headers, null);
        }

        public CatalogProductCategory PublishCatalogProductCategory(CatalogProductCategory catalogProductCategory) {
            this.Auth();
            Headers headers = this.GetHeaders();
            return this.ServiceClient.Post(catalogProductCategory, headers, null);
        }

        private Headers GetHeaders() {
            var headers = new Headers
                          {
                              {AppHeader, this._config.App}
                          };

            HttpCookie session = this.Context.GetRequestCookie(this._config.SessionCookie);
            if (session != null && session.Value.IsFull()) {
                headers.Add(SessionHeader, session.Value);
            }

            FcsToken token = this.GetToken(); // Call this to update this._user from cookie if possible.
            if (token == null) return headers;
            headers.Add("Authorization", String.Format("Bearer {0}", token.Value));
            //headers.Add("X-NoRedirect", "true");
            return headers;
        }

        private FcsToken GetToken() {
            HttpCookie tokenCookie = this.Context.GetRequestCookie(this._config.TokenCookie);
            HttpCookie sessionCookie = this.Context.GetRequestCookie(this._config.SessionCookie);
            //var userCookie = this.Context.GetRequestCookie(this._config.UserCookie);
            FcsToken token = this._token;
            if (token != null && token.IsValid()) return token;

            if (tokenCookie != null &&
                tokenCookie.Value.IsFull()) {
                //var user = userCookie != null && userCookie.Value.IsFull() ? userCookie.Value : null;
                string session = sessionCookie != null && sessionCookie.Value.IsFull() ? sessionCookie.Value : null;

                token = new FcsToken(tokenCookie.Value, session);
                if (token.IsValid()) return token;
            }

            //token = this.Context.GetSessionItem(SessionKey) as FcsToken;
            //if (token != null && token.IsValid()) return token;

            token = _appToken;
            if (token != null && token.IsValid()) return token;

            return null;
        }
    }
}

// ReSharper restore UnusedMember.Global