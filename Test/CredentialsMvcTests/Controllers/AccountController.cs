// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Web.Mvc;
using Fcs;
using Fcs.Model;

namespace CredentialsMvcTests.Controllers {
    public class AccountController : Controller {
        public ActionResult Login() {
            return this.View();
        }

        // POST: /Account/Login
        [HttpPost]
        public ActionResult Login(AuthRequest auth, string returnUrl) {
            try {
                using (var fcs = new FcsClient()) {
                    var response = fcs.Auth(auth);
                    return this.Redirect(response.Continue ?? returnUrl ?? "~/");
                }
            }
            catch (Exception e) {
                return this.Redirect("~/Account/Login?error=" + e.Message);
            }
        }

        [HttpPost]
        public ActionResult Logout() {
            using (var fcs = new FcsClient()) {
                var response = fcs.Unauth();
                return this.Redirect(response.Continue ?? "~/");
            }
        }
    }
}