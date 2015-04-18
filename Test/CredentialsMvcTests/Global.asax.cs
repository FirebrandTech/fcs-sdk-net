// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Fcs;
using NLog;
using ServiceStack.Logging.NLogger;

namespace CredentialsMvcTests {
    public class MvcApplication : HttpApplication {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected void Application_Start() {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            FcsClient.LogFactory = new NLogFactory();
            Logger.Debug("Application_Start");
        }


        //private void Application_BeginRequest(object sender, EventArgs e) {
        //    Logger.Debug("Application_BeginRequest: {0}", HttpContext.Current.Request.Url);
        //}

        //private void Session_Start(object sender, EventArgs e) {
        //    Logger.Debug("Session_Start: {0}", HttpContext.Current.Request.Url);
        //}

        //private void Application_AuthenticateRequest(object sender, EventArgs e) {
        //    Logger.Debug(
        //        "Application_AuthenticateRequest: {0} : {1}",
        //        HttpContext.Current.Request.Url,
        //        (HttpContext.Current.User != null ? HttpContext.Current.User.Identity.Name : ""));

        //    //FcsClient.StartSession();
        //}

        private void Application_PreRequestHandlerExecute(object sender, EventArgs e) {
            FcsClient.EnsureAuthorized(true);
        }
    }
}