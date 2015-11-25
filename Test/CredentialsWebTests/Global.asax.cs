// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using Fcs;
using NLog;
using ServiceStack.Logging.NLogger;

namespace WebFormsTest {
    public class Global : HttpApplication {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private void Application_Start(object sender, EventArgs e) {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            FcsClientPortable.LogFactory = new NLogFactory();
            FcsClientPortable.InitApplication(this);
        }

        //private void Application_BeginRequest(object sender, EventArgs e) {
        //    Logger.Debug("Application_BeginRequest: {0}", HttpContext.Current.Request.Url);
        //}

        private void Session_Start(object sender, EventArgs e) {
            Logger.Debug("Session_Start: {0}", HttpContext.Current.Request.Url);
        }

        private void Application_AuthenticateRequest(object sender, EventArgs e) {
            Logger.Debug(
                "Application_AuthenticateRequest: {0} : {1}",
                HttpContext.Current.Request.Url,
                (HttpContext.Current.User != null ? HttpContext.Current.User.Identity.Name : ""));
        }
    }
}