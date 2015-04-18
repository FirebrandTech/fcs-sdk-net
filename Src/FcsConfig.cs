// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Configuration;
using System.Web;

namespace Fcs {
    public class FcsConfig {
        public FcsConfig() {
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

        public FcsConfig(string clientId, string clientSecret, string appId = "fcs", string apiUrl = null) {
            this.Init(clientId, clientSecret, appId, apiUrl);
        }

        public string ApiUrl { get; private set; }
        public string AppId { get; private set; }
        public string ClientId { get; private set; }
        public string ClientSecret { get; private set; }
        public string TokenCookie { get; private set; }
        public string SessionCookie { get; private set; }
        public string UserCookie { get; private set; }

        private void Init(string clientId, string clientSecret, string appId, string apiUrl) {
            if (apiUrl != null && apiUrl.ToLower() == "auto") {
                var req = HttpContext.Current.Request;
                if (req.ApplicationPath == null) return;
                apiUrl = req.Url.Scheme + "://" +
                         req.Url.Authority +
                         req.ApplicationPath.TrimEnd('/') +
                         "/api/v2";
            }
            this.ApiUrl = apiUrl ?? "https://cloud.firebrandtech.com/api/v2";
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
            this.AppId = appId ?? "fcs";
            this.TokenCookie = this.AppId + "-token";
            this.UserCookie = this.AppId + "-user";
            this.SessionCookie = this.AppId + "-session";
        }
    }
}