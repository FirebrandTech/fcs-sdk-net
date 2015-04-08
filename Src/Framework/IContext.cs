// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Web;
using ServiceStack.Logging;

namespace Fcs.Framework {
    public interface IContext {
        string CurrentUserName { get; }
        HttpCookie GetRequestCookie(string name);
        void SetResponseCookie(string name, string value, DateTime expires);
    }

    public class AspNetContext : IContext {
        private static readonly ILog Logger = LogManager.GetLogger("FcsClient.Context");

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
            //var cookies = HttpContext.Current.Response.Cookies;
            var res = HttpContext.Current.Response;
            //var req = HttpContext.Current.Request;
            //var cookies = res.Cookies;
            //if (req.Cookies[name] != null) req.Cookies.Remove(name);
            //value = HttpUtility.UrlEncode(value);
            var cookie = new HttpCookie(name, value);
                         //{
                         //    Value = value,
                         //    Path = "/",
                         //    HttpOnly = true,
                         //    Secure = false,
                         //    Domain = "localhost",
                         //    Expires = expires
                         //};

            //if (!string.IsNullOrWhiteSpace(FormsAuthentication.CookieDomain)) {
            //    cookie.Domain = FormsAuthentication.CookieDomain;
            //}

            //Logger.DebugFormat("RESPONSE: mycookie1={0}", (cookies["mycookie1"] ?? new HttpCookie("")).Value);
            //Logger.DebugFormat("RESPONSE: mit-token={0}", (cookies["mit-token"] ?? new HttpCookie("")).Value);
            //Logger.DebugFormat("REQUEST: mit-token={0}", (req.Cookies["mit-token"] ?? new HttpCookie("")).Value);
            //Logger.DebugFormat("SET COOKIE: Name={0}, Value={1}, Path={2}, Expires={3}, Domain={4}",
            //                   cookie.Name, cookie.Value, cookie.Path, cookie.Expires, cookie.Domain);

            

            //if (cookies.Get(name) != null) cookies.Remove(name);
            //cookies.Remove(name);
            //cookies.Add(cookie);
            res.SetCookie(cookie);
        }
    }
}