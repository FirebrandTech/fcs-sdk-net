// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Collections.Generic;
using System.Web;
using Fcs.Framework;
using Fcs.Model;
using JWT;
using ServiceStack.Logging;
using StringExtensions = ServiceStack.StringExtensions;

// ReSharper disable UnusedMember.Global

namespace Fcs {
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class FcsClient : IDisposable {
        private const string AppHeader = "X-Fcs-App";
        //private const string SessionHeader = "X-Fcs-Session";
        //private const string SessionKey = "FCS-TOKEN";
        //private const int SessionExpiryDays = 14;
        private static readonly object Sync = new object();
        private static bool _applicationInitialized;
        private static ILog _logger;
        private readonly FcsConfig _config;
        private Access _access;
        private IServiceClient _client;

        static FcsClient() {
            JsonWebToken.JsonSerializer = new ServiceStackJsonSerializer();
        }
        // ReSharper disable once MemberCanBePrivate.Global
        public FcsClient() : this(new FcsConfig()) {}

        public FcsClient(string clientId, string clientSecret, string app = "fcs", string apiUrl = null)
            : this(new FcsConfig(clientId, clientSecret, app, apiUrl)) {}

        // ReSharper disable once MemberCanBePrivate.Global
        public FcsClient(FcsConfig config) {
            this._config = config;
            this.Context = new AspNetContext();
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

        public Access Access {
            get { return this._access; }
        }


        private static ILog Logger {
            get { return _logger ?? (_logger = LogManager.GetLogger("FcsClient")); }
        }

        public IContext Context { private get; set; }

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
                fcs.Auth(ignoreContextUser: ignoreContextUser);
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

        public AuthResponse Auth(string userName = null, bool ignoreContextUser = false) {
            return this.Auth(new AuthRequest
                             {
                                 ClientId = this._config.ClientId,
                                 ClientSecret = this._config.ClientSecret,
                                 UserName = userName
                             },
                             ignoreContextUser);
        }

        public AuthResponse Auth(AuthRequest request, bool ignoreContextUser = false) {
            lock (Sync) {
                var tokenParam = this.Context.GetRequestParam(this._config.TokenParam);
                if (request.Token.IsNullOrWhiteSpace()) {
                    request.Token = tokenParam;
                }

                var access = request.Token.IsFull() ? null : this.GetAccess();

                if (access == null && request.Token.IsNullOrWhiteSpace()) {
                    // There is no token.  Assume this is at least an App Auth or maybe a fast-tracked User Auth.
                    request.ClientId = this._config.ClientId;
                    request.ClientSecret = this._config.ClientSecret;
                }
                if (request.UserName.IsNullOrWhiteSpace() &&
                    request.ImpersonateUserName.IsNullOrWhiteSpace()) {
                    // No UserName passed in.  Check the context for the current UserName.
                    request.UserName = this.Context.CurrentUserName ?? (access ?? new Access()).User;
                }

                if (access != null &&
                    !string.Equals(access.User, request.UserName, StringComparison.OrdinalIgnoreCase) &&
                    !ignoreContextUser) {
                    // We have a token, but the UserName does not match that of the found token.
                    // This will be a User Auth with a foundational App or User auth token.
                    request.ClientId = null;
                    request.ClientSecret = null;
                    access = null;
                }

                if (access == null) {
                    // We have determined that an Auth is necessary. Proceed...
                    var requestHeaders = this.GetHeaders();
                    Logger.DebugFormat("POST AUTH REQUEST: {0}", StringExtensions.ToJsv(request));
                    Logger.DebugFormat("POST AUTH REQUEST HEADERS: {0}", StringExtensions.ToJsv(requestHeaders));

                    var responseHeaders = new Headers();
                    var response = this.ServiceClient.Post(request, requestHeaders, responseHeaders);
                    Logger.DebugFormat("POST AUTH RESPONSE: {0}", StringExtensions.ToJsv(response));
                    Logger.DebugFormat("POST AUTH RESPONSE HEADERS: {0}", StringExtensions.ToJsv(responseHeaders));
                    access = new Access
                             {
                                 Token = response.Token,
                                 Expires = (response.Expires ?? DateTime.MinValue).ToUniversalTime(),
                                 User = response.UserName,
                                 Session = response.Session,
                                 Continue = response.Continue
                             };
                }

                this.SaveAccess(access);

                if (tokenParam.IsFull()) {
                    var uri = this.Context.GetRequestUri();
                    var url = uri.RemoveParam(this._config.TokenParam);
                    this.Context.Redirect(url);
                    return null;
                }

                return new AuthResponse
                       {
                           Token = access.Token,
                           Expires = access.Expires,
                           Session = access.Session,
                           UserName = access.User,
                           Continue = access.Continue
                       };
            }
        }

        /// <summary>
        ///     Save the token information in the following places:  static, instance, and cookies.
        /// </summary>
        /// <param name="access">token information</param>
        private void SaveAccess(Access access) {
            this._access = access;
            this.Context.SetResponseCookie(this._config.TokenCookie, access.Token, null);
            //if (access.Session.IsFull()) {
            //    this.Context.SetResponseCookie(this._config.SessionCookie,
            //                                   access.Session,
            //                                   DateTime.UtcNow.AddDays(SessionExpiryDays));
            //}
        }

        public AuthResponse Unauth() {
            var response = this.ServiceClient.Delete(new AuthRequest(), this.GetHeaders(), null);
            var token = new Access
                        {
                            Token = response.Token,
                            Expires = response.Expires ?? DateTime.MinValue,
                            User = response.UserName,
                            Session = response.Session
                        };
            this.SaveAccess(token);
            return response;
        }

        public void ResetPassword(PasswordReset req) {
            this.Auth();
            var headers = this.GetHeaders();
            this.ServiceClient.Post(req, headers, null);
        }

        public object PlaceOrder(Order order) {
            this.Auth(order.UserName ?? (order.User ?? new User()).Email);
            return this.ServiceClient.Post(order, this.GetHeaders(), null);
        }

        public Catalog UpdateCatalog(Catalog catalog) {
            this.Auth();
            var headers = this.GetHeaders();
            return this.ServiceClient.Post(catalog, headers, null);
        }

        public CatalogStatus UpdateCatalogStatus(CatalogStatus commit) {
            this.Auth();
            var headers = this.GetHeaders();
            return this.ServiceClient.Post(commit, headers, null);
        }

        public CatalogComment UpdateCatalogComment(CatalogComment catalogComment) {
            this.Auth();
            var headers = this.GetHeaders();
            return this.ServiceClient.Post(catalogComment, headers, null);
        }

        public CatalogCategory UpdateCatalogCategory(CatalogCategory catalogCategory) {
            this.Auth();
            var headers = this.GetHeaders();
            return this.ServiceClient.Post(catalogCategory, headers, null);
        }

        public CatalogCategoryCommit CommitCatalogCategory(CatalogCategoryCommit categoryCommit) {
            this.Auth();
            var headers = this.GetHeaders();
            return this.ServiceClient.Post(categoryCommit, headers, null);
        }

        public CatalogProduct UpdateCatalogProduct(CatalogProduct catalogProduct) {
            this.Auth();
            var headers = this.GetHeaders();
            return this.ServiceClient.Post(catalogProduct, headers, null);
        }

        public CatalogProductCategory UpdateCatalogProductCategory(CatalogProductCategory catalogProductCategory) {
            this.Auth();
            var headers = this.GetHeaders();
            return this.ServiceClient.Post(catalogProductCategory, headers, null);
        }

        public Promo PublishPromo(Promo promo) {
            this.Auth();
            var headers = this.GetHeaders();
            return this.ServiceClient.Post(promo, headers, null);
        }

        public PromoCode PublishPromoCode(PromoCode promoCode) {
            this.Auth();
            var headers = this.GetHeaders();
            return this.ServiceClient.Post(promoCode, headers, null);
        }

        public ValidatePromoCodeResponse ValidatePromoCode(string code) {
            this.Auth(ignoreContextUser: true);
            var headers = this.GetHeaders();
            return this.ServiceClient.Post(new PromoCodeValidation {Code = code}, headers, null);
        }

        public List<DomainSummary> GetDomains(DomainsFull domains) {
            this.Auth();
            var headers = this.GetHeaders();
            return this.ServiceClient.Get(domains, headers, null);
        }

        public bool VerifyApiConnection() { 
            return this.VerifyApiConnection(new VerifyApiConnectRequest());
        }

        public bool VerifyApiConnection(VerifyApiConnectRequest verifyApiConnect) {
            this.Auth();
            var headers = this.GetHeaders();
            return this.ServiceClient.Get(verifyApiConnect, headers, null);
        }

        public AuthResponse Register(User user) {
            this.Auth();
            var requestHeaders = this.GetHeaders();
            var responseHeaders = new Headers();
            var response = this.ServiceClient.Post<AuthResponse>(user, requestHeaders, responseHeaders);
            Logger.DebugFormat("POST REGISTER RESPONSE: {0}", StringExtensions.ToJsv(response));
            Logger.DebugFormat("POST REGISTER RESPONSE HEADERS: {0}", StringExtensions.ToJsv(responseHeaders));
            var access = new Access
                         {
                             Token = response.Token,
                             Expires = (response.Expires ?? DateTime.MinValue).ToUniversalTime(),
                             User = response.UserName,
                             Session = response.Session
                         };
            this.SaveAccess(access);

            return new AuthResponse
                   {
                       Token = access.Token,
                       Expires = access.Expires,
                       Session = access.Session,
                       UserName = access.User,
                   };
        }

        private Headers GetHeaders() {
            var headers = new Headers
                          {
                              {AppHeader, this._config.App}
                          };

            //var session = this.Context.GetRequestCookie(this._config.SessionCookie);
            //if (session != null && session.Value.IsFull()) {
            //    headers.Add(SessionHeader, session.Value);
            //}

            var token = this.GetAccess(); // Call this to update this._user from cookie if possible.
            if (token == null) return headers;
            headers.Add("Authorization", String.Format("Bearer {0}", token.Token));
            //headers.Add("X-NoRedirect", "true");
            return headers;
        }

        private Access GetAccess() {
            var tokenCookie = this.Context.GetRequestCookie(this._config.TokenCookie);
            //var sessionCookie = this.Context.GetRequestCookie(this._config.SessionCookie);
            //var userCookie = this.Context.GetRequestCookie(this._config.UserCookie);
            var access = this._access;
            if (access != null && access.Expires > DateTime.UtcNow) return access;

            if (tokenCookie != null &&
                tokenCookie.Value.IsFull()) {
                //var user = userCookie != null && userCookie.Value.IsFull() ? userCookie.Value : null;
                //var session = sessionCookie != null && sessionCookie.Value.IsFull() ? sessionCookie.Value : null;

                var token = JsonWebToken.DecodeToObject<AuthToken>(tokenCookie.Value, "UNKNOWN", false);
                access = new Access
                         {
                             Token = tokenCookie.Value,
                             Expires = token.Expires,
                             Session = token.SessionId,
                             User = token.UserName
                         };
                if (access.Expires > DateTime.UtcNow) return access;
            }

            return null;
        }
    }
}

// ReSharper restore UnusedMember.Global