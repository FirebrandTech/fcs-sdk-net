// Copyright © 2010-2014 Firebrand Technologies

using System;
using System.Collections.Generic;
using ServiceStack;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Fcs.Model {
    public enum UserAddressType {
        Billing,
        Shipping
    }

    public interface IPassword {
        string Password { get; }
        string ConfirmPassword { get; }
    }

    public interface IIdentifiable {
        Guid? Id { get; }
    }

    [Route("/users/{id}/password", "POST", Summary = "Change a user's password")]
    public class ChangePasswordRequest : IPassword, IIdentifiable {
        public Guid? Id { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    [Route("/users/password/reset", "POST", Summary = "Send password reset email for a user")]
    public class PasswordReset : IReturnVoid {
        public Guid? Id { get; set; }
        public string Email { get; set; }
    }

    public class Country : KeyValue {
        public Country(string key, string value, List<KeyValue> states) : base(key, value) {
            this.States = states;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public List<KeyValue> States { get; set; }
    }

    [Route("/users", "POST,PUT", Summary = "Register myself / Create User")]
    [Route("/users/me", "GET", Summary = "Get Me")]
    [Route("/users/{id}", "GET", Summary = "Get User")]
    [Route("/users/{id}", "POST,PUT", Summary = "Update User")]
    [Route("/users/{id}", "DELETE", Summary = "Delete User")]
    public class User : IPassword, IIdentifiable {
        public string Tag { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Continue { get; set; }
        public bool? IsAuthentic { get; set; }
        public UserAddress BillingAddress { get; set; }
        public UserAddress ShippingAddress { get; set; }
        public List<Country> Countries { get; set; }
        public string Phone { get; set; }
        public string GravatarUri { get; set; }
        public bool? OptInMail { get; set; }

        [ApiMember(Name = "id", ParameterType = "path")]
        public Guid? Id { get; set; }

        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    [Route("/users/{id}/address/{type}", "PUT", Summary = "Create / Update a User Address")]
    public class UserAddress : IIdentifiable {
        [ApiMember(Name = "type", ParameterType = "path")]
        public UserAddressType Type { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }

        [ApiMember(Name = "id", ParameterType = "path")]
        public Guid? Id { get; set; }
    }

    [Route("/users", "GET", Summary = "Get a list of Users")]
    public class UsersRequest : Filter {}
}