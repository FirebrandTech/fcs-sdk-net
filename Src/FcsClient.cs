// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using Cloud.Api.V2.Model;
using Fcs.Framework;
using Fcs.Model;
using ServiceStack;
using ServiceStack.Logging;
using IServiceClient = Fcs.Framework.IServiceClient;

namespace Fcs {
    public class FcsClient : IDisposable {
        private const string AppHeader = "X-Fcs-App";
        private static readonly object Sync = new object();
        private static string _token;
        private static DateTime? _tokenExpires;
        private static string _user;
        private readonly ILog _logger = LogManager.GetLogger("FcsClient");
        private string _apiUrl;
        private string _appId;
        private IServiceClient _client;
        private string _clientId;
        private string _clientSecret;
        private IContext _context;
        private string _tokenCookie;
        private string _userCookie;
        //private static string _sharedToken;
        //private static DateTime? _sharedTokenExpires;
        //private static string _sharedUser;

        public FcsClient() {
            var settings = ConfigurationManager.AppSettings;
            var clientId = settings["FcsClientId"] ?? Environment.GetEnvironmentVariable("FcsClientId");
            var clientSecret = settings["FcsClientSecret"] ?? Environment.GetEnvironmentVariable("FcsClientSecret");
            var appId = settings["FcsAppId"] ?? Environment.GetEnvironmentVariable("FcsAppId");
            var url = settings["FcsApiUrl"] ?? Environment.GetEnvironmentVariable("FcsAppUrl");

            if (string.IsNullOrWhiteSpace(clientId) ||
                string.IsNullOrWhiteSpace(clientSecret)) {
                throw new InvalidOperationException("FcsClient is not configured properly!  Please add the following to your appSettings: FcsClientId, FcsClientSecret, FcsAppId");
            }
            this.Init(clientId, clientSecret, appId, url);
        }

        public FcsClient(string clientId, string clientSecret, string appId = "fcs", string apiUrl = null) {
            this.Init(clientId, clientSecret, appId, apiUrl);
        }

        public static ILogFactory LogFactory {
            get { return LogManager.LogFactory; }
            set { LogManager.LogFactory = value; }
        }

        public static string Token {
            get { return _token; }
        }

        public static DateTime? TokenExpires {
            get { return _tokenExpires; }
        }

        public static string User {
            get { return _user; }
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
            this._tokenCookie = this._appId + "-token";
            this._userCookie = this._appId + "-user";
            this.ServiceClientFactory = new JsonServiceClientFactory();
        }

        public static void Reset() {
            _token = null;
            _tokenExpires = null;
            _user = null;
        }

        public static void StartSession() {
            using (var fcs = new FcsClient()) {
                fcs.Auth();
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
                var auth = this.GetValidAuth(request);

                if (auth == null) {
                    var headers = this.GetHeaders();
                    this._logger.DebugFormat("POST AUTH REQUEST: {0}", StringExtensions.ToJsv(request));
                    this._logger.DebugFormat("POST AUTH REQUEST HEADERS: {0}", StringExtensions.ToJsv(headers));

                    auth = this.ServiceClient.Post(request, headers);
                    this._logger.DebugFormat("POST AUTH RESPONSE: {0}", StringExtensions.ToJsv(auth));
                }

                _token = auth.Token;
                _tokenExpires = auth.Expires;
                _user = request.UserName;
                this.Context.SetResponseCookie(this._tokenCookie, auth.Token, auth.Expires ?? DateTime.MinValue);
                //if (!StringExtensions.IsNullOrEmpty(this._user)) this.Context.SetResponseCookie(this._userCookie, this._user, auth.Expires ?? DateTime.MinValue);

                return auth;
            }
        }

        private AuthResponse GetValidAuth(AuthRequest request) {
            var auth = this.GetValidAuth();
            if (auth == null) {
                request.ClientId = this._clientId;
                request.ClientSecret = this._clientSecret;
                return null;
            }
            request.ClientId = null;
            request.ClientSecret = null;
            var user = this.GetAuthedUser();
            if (string.IsNullOrWhiteSpace(request.UserName) &&
                string.IsNullOrWhiteSpace(user)) return auth;

            return string.Equals(request.UserName, user, StringComparison.OrdinalIgnoreCase)
                       ? auth
                       : null;
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
            this.Auth(_user);
            return this.ServiceClient.Post(catalog, this.GetHeaders());
        }

        private Dictionary<string, string> GetHeaders() {
            var auth = this.GetValidAuth();
            var headers = new Dictionary<string, string>
                          {
                              {AppHeader, this._appId}
                          };
            this.GetAuthedUser(); // Call this to update this._user from cookie if possible.
            if (auth == null) return headers;
            headers.Add("Authorization", String.Format("Bearer {0}", auth.Token));
            headers.Add("X-NoRedirect", "true");
            return headers;
        }

        private AuthResponse GetValidAuth() {
            if (TokenIsValid()) return GetCurrentAuth();

            var cookie = this.Context.GetRequestCookie(this._tokenCookie);
            if (cookie == null) return null;
            _token = cookie.Value;
            _tokenExpires = cookie.Expires;
            return TokenIsValid() ? GetCurrentAuth() : null;
        }

        private static bool TokenIsValid() {
            return !string.IsNullOrWhiteSpace(_token) && DateTime.UtcNow < _tokenExpires;
        }

        private static AuthResponse GetCurrentAuth() {
            return new AuthResponse
                   {
                       Token = _token,
                       Expires = _tokenExpires
                   };
        }

        private string GetAuthedUser() {
            if (!string.IsNullOrWhiteSpace(_user)) return _user;
            var cookie = this.Context.GetRequestCookie(this._userCookie);
            if (cookie == null) return null;
            _user = cookie.Value;
            return _user;
        }
    }
}