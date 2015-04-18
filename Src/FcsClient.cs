// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Web;
using System.Web.UI.HtmlControls;
using Cloud.Api.V2.Model;
using Fcs.Framework;
using Fcs.Model;
using ServiceStack;
using ServiceStack.Logging;
using IServiceClient = Fcs.Framework.IServiceClient;

namespace Fcs {
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class FcsClient : IDisposable {
        private const string AppHeader = "X-Fcs-App";
        //private const string SessionHeader = "X-Fcs-Session";
        private const string SessionKey = "FCS-TOKEN";
        //private const int SessionExpiryDays = 14;
        private static readonly object Sync = new object();
        private static bool _applicationInitialized;
        private static ILog _logger;
        private readonly FcsConfig _config;
        private IServiceClient _client;
        private IContext _context;
        private static FcsToken _appToken;
        private FcsToken _token;

        // ReSharper disable once MemberCanBePrivate.Global
        public FcsClient() : this(new FcsConfig()) {}

        public FcsClient(string clientId, string clientSecret, string appId = "fcs", string apiUrl = null)
            : this(new FcsConfig(clientId, clientSecret, appId, apiUrl)) {}

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
            var url = HttpContext.Current.Request.Url;
            if (url.PathAndQuery.Contains("/__browserLink/")) return;
            Logger.DebugFormat("EnsureAuthorized: {0}", url);

            using (var fcs = new FcsClient()) {
                fcs.Auth(ignoreContextUser:ignoreContextUser);
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

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedMethodReturnValue.Global
        public AuthResponse Auth(AuthRequest request, bool ignoreContextUser = false) {
            lock (Sync) {
                var token = this.GetToken();

                if (request.UserName.IsNullOrWhiteSpace()) {
                    request.UserName = this.Context.CurrentUserName ?? (token ?? new FcsToken()).User;
                }

                //if (token != null) {
                //    if (token.User.IsNullOrWhiteSpace()) {
                //        // The token is an app token.  Clear out client credentials as they are not needed.
                //        // the app token will be passed on the Authorization header.
                //        request.ClientId = null;
                //        request.ClientSecret = null;
                //    }
                //    else if (token.User.EqualsIgnoreCase(request.UserName))
                //}
                if (token != null &&
                    !string.Equals(token.User, request.UserName, StringComparison.OrdinalIgnoreCase) &&
                    !ignoreContextUser) {
                    request.ClientId = null;
                    request.ClientSecret = null;
                    token = null;
                }

                if (token == null) {
                    var requestHeaders = this.GetHeaders();
                    Logger.DebugFormat("POST AUTH REQUEST: {0}", request.ToJsv());
                    Logger.DebugFormat("POST AUTH REQUEST HEADERS: {0}", requestHeaders.ToJsv());

                    var responseHeaders = new Headers();
                    var response = this.ServiceClient.Post(request, requestHeaders, responseHeaders);
                    Logger.DebugFormat("POST AUTH RESPONSE: {0}", response.ToJsv());
                    Logger.DebugFormat("POST AUTH RESPONSE HEADERS: {0}", responseHeaders.ToJsv());
                    token = new FcsToken
                            {
                                Value = response.Token,
                                Expires = response.Expires.ToUtc(),
                                User = request.UserName
                            };

                    //var session = responseHeaders.Value(SessionHeader);
                    //if (session.IsFull()) {
                    //    this.Context.SetResponseCookie(this._config.SessionCookie,
                    //                                   session,
                    //                                   DateTime.UtcNow.AddDays(SessionExpiryDays));
                    //}
                }

                this.SaveToken(token);

                return new AuthResponse
                       {
                           Token = token.Value,
                           Expires = token.Expires
                       };
            }
        }

        private void SaveToken(FcsToken token) {
            this._token = token;

            if (token.User.IsNullOrWhiteSpace()) {
                // Token is app token.  Save it as the static appToken to minimize token creation.
                _appToken = token;
            }
            //this.Context.SetSessionItem(SessionKey, token);
            this.Context.SetResponseCookie(this._config.TokenCookie, token.Value, token.Expires ?? DateTime.MinValue);
            if (token.User.IsFull()) {
                this.Context.SetResponseCookie(this._config.UserCookie, token.User, token.Expires ?? DateTime.MinValue);
            }
        }

        // ReSharper disable once UnusedMember.Global
        public AuthResponse Unauth() {
            var response = this.ServiceClient.Delete(new AuthRequest(), this.GetHeaders(), null);
            this.Context.SetResponseCookie(this._config.TokenCookie, response.Token, response.Expires ?? DateTime.MinValue);
            //this.Context.SetResponseCookie(this._config);
            return response;
        }

        // ReSharper disable once UnusedMember.Global
        public object PlaceOrder(Order order) {
            this.Auth(order.UserName ?? (order.User ?? new User()).Email);
            return this.ServiceClient.Post(order, this.GetHeaders(), null);
        }

        public Catalog PublishCatalog(Catalog catalog) {
            this.Auth();
            var headers = this.GetHeaders();
            return this.ServiceClient.Post(catalog, headers, null);
        }

        private Headers GetHeaders() {
            var headers = new Headers
                          {
                              {AppHeader, this._config.AppId}
                          };

            //var session = this.Context.GetRequestCookie(this._config.SessionCookie);
            //if (session != null && session.Value.IsFull()) {
            //    headers.Add(SessionHeader, session.Value);
            //}

            var token = this.GetToken(); // Call this to update this._user from cookie if possible.
            if (token == null) return headers;
            headers.Add("Authorization", String.Format("Bearer {0}", token.Value));
            //headers.Add("X-NoRedirect", "true");
            return headers;
        }

        private FcsToken GetToken() {
            var tokenCookie = this.Context.GetRequestCookie(this._config.TokenCookie);
            var userCookie = this.Context.GetRequestCookie(this._config.UserCookie);
            var token = this._token;
            if (token != null && token.IsValid()) return token;

            if (tokenCookie != null &&
                tokenCookie.Value.IsFull() &&
                tokenCookie.Expires > DateTime.UtcNow) {
                var user = userCookie != null && userCookie.Value.IsFull() ? userCookie.Value : null;

                token = new FcsToken
                        {
                            Value = tokenCookie.Value,
                            Expires = tokenCookie.Expires,
                            User = user
                        };
                if (token.IsValid()) return token;
            }

            token = this.Context.GetSessionItem(SessionKey) as FcsToken;
            if (token != null && token.IsValid()) return token;

            token = _appToken;
            if (token != null && token.IsValid()) return token;

            return null;
        }
    }
}