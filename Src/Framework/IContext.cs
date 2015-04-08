// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Web;
using System.Web.Security;
using ServiceStack;
using ServiceStack.Logging;

namespace Fcs.Framework {
    public interface IContext {
        string CurrentUserName { get; }
        HttpCookie GetRequestCookie(string name);
        void SetResponseCookie(string name, string value, DateTime expires);
    }

    public class AspNetContext : IContext {
        private static readonly ILog Logger = LogManager.GetLogger("FcsClient");

        public string CurrentUserName {
            get {
                if (HttpContext.Current == null) return null;
                if (HttpContext.Current.User == null) return null;
                if (HttpContext.Current.User.Identity == null) return null;
                return HttpContext.Current.User.Identity.Name;
            }
        }

        public HttpCookie GetRequestCookie(string name) {
            if (HttpContext.Current == null) return null;
            var cookies = HttpContext.Current.Request.Cookies;
            return cookies.Get(name);
        }

        public void SetResponseCookie(string name, string value, DateTime expires) {
            if (HttpContext.Current == null) return;
            var cookies = HttpContext.Current.Response.Cookies;
            value = HttpUtility.UrlEncode(value);
            var cookie = new HttpCookie(name) {
                Value = value,
                Path = FormsAuthentication.FormsCookiePath,
                HttpOnly = false,
                Secure = FormsAuthentication.RequireSSL,
                Expires = expires
            };

            if (!string.IsNullOrWhiteSpace(FormsAuthentication.CookieDomain)) {
                cookie.Domain = FormsAuthentication.CookieDomain;
            }

            Logger.DebugFormat("SET COOKIE: {0}", cookie.ToJsv());
            if (cookies.Get(name) != null) cookies.Remove(name);
            cookies.Add(cookie);
        }
    }
}