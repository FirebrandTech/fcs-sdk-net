// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using ServiceStack.Logging;

namespace Fcs.Framework {
    public interface IContext {
        string CurrentUserName { get; }
        object GetSessionItem(string key);
        void SetSessionItem(string key, object item);
        void SetCacheItem(string key, object item, DateTime expiration);
        object GetCacheItem(string key);
        string GetRequestParam(string name);
        HttpCookie GetRequestCookie(string name);
        void SetResponseCookie(string name, string value, DateTime? expires);
        Uri GetRequestUri();
        void Redirect(string url);
    }

    public class AspNetContext : IContext {
        private static readonly ILog Logger = LogManager.GetLogger("FcsClient.Context");

        //public string ClientAddress {
        //    get {
        //        if (HttpContext.Current == null) return null;
        //        if (HttpContext.Current.User.Identity == null) return null;
        //        return HttpContext.Current.Request.                
        //    }
        //}
        public string CurrentUserName {
            get {
                if (HttpContext.Current == null) return null;
                if (HttpContext.Current.User == null) return null;
                if (HttpContext.Current.User.Identity == null) return null;
                return string.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name)
                           ? null
                           : HttpContext.Current.User.Identity.Name;
            }
        }

        public object GetSessionItem(string key) {
            if (HttpContext.Current == null) return null;
            if (HttpContext.Current.Session == null) return null;
            return HttpContext.Current.Session[key];
        }

        public void SetSessionItem(string key, object item) {
            if (HttpContext.Current == null) return;
            if (HttpContext.Current.Session == null) return;
            HttpContext.Current.Session[key] = item;
        }

        public object GetCacheItem(string key) {
            if (HttpContext.Current == null) return null;
            return HttpContext.Current.Cache == null 
                ? null 
                : HttpContext.Current.Cache[key];
        }

        public void SetCacheItem(string key, object item, DateTime expiration) {
            if (HttpContext.Current == null) return;
            if (HttpContext.Current.Cache == null) return;
            HttpContext.Current.Cache.Insert(key, item, null, expiration, Cache.NoSlidingExpiration);
        }

        public string GetRequestParam(string name) {
            if (HttpContext.Current == null) return null;
            return HttpContext.Current.Request.QueryString.Get(name);
        }

        public Uri GetRequestUri() {
            if (HttpContext.Current == null) return null;
            return HttpContext.Current.Request.Url;
        }

        public HttpCookie GetRequestCookie(string name) {
            if (HttpContext.Current == null) return null;
            var cookies = HttpContext.Current.Request.Cookies;
            return cookies.Get(name);
        }

        public void SetResponseCookie(string name, string value, DateTime? expires = null) {
            if (HttpContext.Current == null) return;
            var res = HttpContext.Current.Response;
            var cookies = res.Cookies;
            var cookie = new HttpCookie(name, value)
                         {
                             Value = value,
                             Path = "/",
                             HttpOnly = false,
                             Secure = FormsAuthentication.RequireSSL,
                             Expires = expires != null ? expires.Value : DateTime.MinValue
                         };

            if (FormsAuthentication.CookieDomain.IsFull()) {
                cookie.Domain = FormsAuthentication.CookieDomain;
            }

            Logger.DebugFormat("SET-RESPONSE-COOKIE: {0};{1};{2};{3};{4};{5}",
                               cookie.Value, cookie.Path, cookie.HttpOnly, cookie.Secure, cookie.Expires, cookie.Domain);

            cookies.Remove(name);
            cookies.Add(cookie);
        }

        public void Redirect(string url) {
            if (HttpContext.Current == null) return;
            HttpContext.Current.Response.Redirect(url);
        }
    }
}