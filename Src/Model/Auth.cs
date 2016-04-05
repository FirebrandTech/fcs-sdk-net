// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Runtime.Serialization;
using Fcs.Framework;
using ServiceStack;

namespace Fcs.Model {
    [Route("/auth", "GET", Summary = "Get Authorization or Redirect if not Authenticated")]
    [Route("/auth", "POST", Summary = "Authenticate or Login User")]
    [Route("/auth", "DELETE", Summary = "Unauthenticate or Log off User")]
    public class AuthRequest : IReturn<AuthResponse> {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ImpersonateUserName { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Continue { get; set; }
        public string Uri { get; set; }
        public string Token { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool UserIsGuest { get; set; }
    }

    [Route("/auth/library/uri", "GET", Summary = "Redirect to Login Page")]
    public class AuthLibraryUriRequest {
        public string Site { get; set; }
        public string Email { get; set; }
    }

    [Route("/auth/uri", "GET")]
    public class AuthUriRequest {
        public string App { get; set; }
        public string Uri { get; set; }
    }

    [Route("/access/{token}", "GET")]
    public class AccessRequest {
        public string Token { get; set; }
        public string Uri { get; set; }
    }

    [Route("/auth/connected", "GET")]
    public class VerifyApiConnectRequest : IReturn<bool> {}

    public class AuthResponse {
        public string Continue { get; set; }
        public string Token { get; set; }
        public DateTime? Expires { get; set; }
        public string Session { get; set; }
        public string UserName { get; set; }
    }

    public class AuthToken {
        public string Pre { get; set; }
        public string App { get; set; }
        public string Sub { get; set; }
        public string Ori { get; set; }
        public string Usr { get; set; }
        public int Exp { get; set; }
        public string Sid { get; set; }

        [IgnoreDataMember]
        public string AppPrefix { get; set; }

        [IgnoreDataMember]
        public Guid? ApplicationId {
            get { return this.App.ToGuid(); }
            set { this.App = value.ToShortString(); }
        }

        [IgnoreDataMember]
        public Guid? UserId {
            get { return this.Sub.ToGuid(); }
            set { this.Sub = value.ToShortString(); }
        }

        [IgnoreDataMember]
        public Guid? OriginId {
            get { return this.Ori.ToGuid(); }
            set { this.Ori = value.ToShortString(); }
        }

        [IgnoreDataMember]
        public string UserName {
            get { return this.Usr; }
            set { this.Usr = value; }
        }

        [IgnoreDataMember]
        public DateTime Expires {
            get { return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(this.Exp); }
            set { this.Exp = (int)Math.Round((value - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds); }
        }

        [IgnoreDataMember]
        public string SessionId {
            get { return this.Sid; }
            set { this.Sid = value; }
        }
    }
}