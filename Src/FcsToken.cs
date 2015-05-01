// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Globalization;
using System.Web;
using Fcs.Framework;
using Fcs.Model;

namespace Fcs {
    public class FcsToken {
        public string Value { get; private set; }
        public DateTime Expires { get; private set; }
        public string User { get; private set; }
        public string Session { get; private set; }

        public FcsToken() { }

        public FcsToken(AuthResponse res) {
            this.Value = res.Token;
            var expires = res.Expires.ToUtc();
            if (expires == null) {
                throw new InvalidOperationException("Expires is null");
            }
            this.Expires = expires.Value;
            this.Session = res.Session;
            this.User = res.UserName;
        }

        public FcsToken(HttpCookie cookie, string user, string session) {
            var parts = cookie.Value.Split('|');
            this.Value = parts[0].Trim();
            this.Expires = DateTime.Parse(parts[1].Trim(), CultureInfo.InvariantCulture).ToUniversalTime();
            this.User = user;
            this.Session = session;
        }

        public bool IsValid() {
            return this.Value.IsFull() &&
                   this.Expires > DateTime.UtcNow;
        }

        public string ToCookieValue() {
            return string.Format("{0}|{1:s}", this.Value, this.Expires.ToUniversalTime());
        }
    }
}