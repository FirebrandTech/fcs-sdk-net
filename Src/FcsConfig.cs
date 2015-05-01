// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Web;

namespace Fcs {
    public class FcsConfig {
        public FcsConfig() {
            NameValueCollection settings = ConfigurationManager.AppSettings;
            string clientId = settings["FcsClientId"] ?? Environment.GetEnvironmentVariable("FcsClientId");
            string clientSecret = settings["FcsClientSecret"] ?? Environment.GetEnvironmentVariable("FcsClientSecret");
            string app = settings["FcsApp"] ?? Environment.GetEnvironmentVariable("FcsApp");
            string url = settings["FcsApiUrl"] ?? Environment.GetEnvironmentVariable("FcsAppUrl");

            if (string.IsNullOrWhiteSpace(clientId) ||
                string.IsNullOrWhiteSpace(clientSecret)) {
                throw new InvalidOperationException("FcsClient is not configured properly!  Please add the following to your appSettings: FcsClientId, FcsClientSecret, FcsAppId");
            }
            this.Init(clientId, clientSecret, app, url);
        }

        public FcsConfig(string clientId, string clientSecret, string app = "fcs", string apiUrl = null) {
            this.Init(clientId, clientSecret, app, apiUrl);
        }

        public string ApiUrl { get; private set; }
        public string App { get; private set; }
        public string ClientId { get; private set; }
        public string ClientSecret { get; private set; }
        public string TokenCookie { get; private set; }
        public string SessionCookie { get; private set; }
        public string UserCookie { get; private set; }

        private void Init(string clientId, string clientSecret, string app, string apiUrl) {
            if (apiUrl != null && apiUrl.ToLower() == "auto") {
                HttpRequest req = HttpContext.Current.Request;
                if (req.ApplicationPath == null) return;
                apiUrl = req.Url.Scheme + "://" +
                         req.Url.Authority +
                         req.ApplicationPath.TrimEnd('/') +
                         "/api/v2";
            }
            this.ApiUrl = apiUrl ?? "https://cloud.firebrandtech.com/api/v2";
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
            this.App = app ?? "fcs";
            this.TokenCookie = this.App + "-token";
            this.UserCookie = this.App + "-user";
            this.SessionCookie = this.App + "-session";
        }
    }
}