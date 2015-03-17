// Copyright © 2010-2014 Firebrand Technologies

using System;
using ServiceStack;

namespace Fcs.Model {
    [Route("/auth", "GET", Summary = "Get Authorization or Redirect if not Authenticated")]
    [Route("/auth", "POST", Summary = "Authenticate or Login User")]
    [Route("/auth", "DELETE", Summary = "Unauthenticate or Log off User")]
    public class AuthRequest : IReturn<AuthResponse> {
        public string UserName { get; set; }
        public string Password { get; set; }
        //public string ImpersonateUserName { get; set; }
        //public string AccessToken { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Continue { get; set; }
        public string Uri { get; set; }
    }

    [Route("/auth/uri", "GET", Summary = "Redirect to Login Page")]
    public class AuthUriRequest {
        public string Site { get; set; }
        public string Email { get; set; }
    }

    public class AuthUriResponse {
        public string Uri { get; set; }
    }

    public class AuthResponse {
        public string Continue { get; set; }
        public string Token { get; set; }
        public DateTime? Expires { get; set; }
    }
}