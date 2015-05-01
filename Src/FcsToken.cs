// Copyright © 2010-2015 Firebrand Technologies

using System;
using Fcs.Framework;
using Fcs.Model;

namespace Fcs {
    public class FcsToken {
        public FcsToken() {}

        public FcsToken(AuthResponse res) {
            this.Value = res.Token;
            DateTime? expires = res.Expires.ToUtc();
            if (expires == null) {
                throw new InvalidOperationException("Expires is null");
            }
            this.Expires = expires.Value;
            this.Session = res.Session;
            this.User = res.UserName;
        }

        public FcsToken(string serializedToken, string session) {
            AccessToken token = serializedToken.DeserializeToken();
            this.Value = token.T;
            this.Expires = token.E;
            this.User = token.U;
            this.Session = session;
        }

        public string Value { get; private set; }
        public DateTime Expires { get; private set; }
        public string User { get; private set; }
        public string Session { get; private set; }

        public bool IsValid() {
            return this.Value.IsFull() &&
                   this.Expires > DateTime.UtcNow;
        }
    }
}