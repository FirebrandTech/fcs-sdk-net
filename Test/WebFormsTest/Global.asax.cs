using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using Fcs;
using Microsoft.Owin;

namespace WebFormsTest
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e) {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Trace.Listeners.Add(new TextWriterTraceListener("C:\\logs\\WebFormsTest.log"));
            Trace.AutoFlush = true;
        }

        void Application_BeginRequest(object sender, EventArgs e) {
            Trace.WriteLine("Application_BeginRequest: "+HttpContext.Current.Request.Url);
        }

        void Session_Start(object sender, EventArgs e) {
            Trace.WriteLine("Session_Start: "+HttpContext.Current.Request.Url);
        }

        void Application_AuthenticateRequest(object sender, EventArgs e) {
            Trace.WriteLine(
                "Application_AuthenticateRequest: "+
                HttpContext.Current.Request.Url + " : "+
                (HttpContext.Current.User != null ? HttpContext.Current.User.Identity.Name : ""));

            new FcsClient("http://cloud.local/api/v2", "", "", "").Auth();
        }
    }
}