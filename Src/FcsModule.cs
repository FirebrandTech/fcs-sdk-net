// Copyright © 2010-2015 Firebrand Technologies

using System;
using System.Web;

namespace Fcs {
    public class FcsModule : IHttpModule {
        public String ModuleName {
            get { return "FcsModule"; }
        }

        // In the Init function, register for HttpApplication 
        // events by adding your handlers.
        public void Init(HttpApplication application) {
            application.BeginRequest +=
                (this.Application_BeginRequest);
            application.EndRequest +=
                (this.Application_EndRequest);
        }

        private void Application_BeginRequest(Object source,
                                              EventArgs e) {
            // Create HttpApplication and HttpContext objects to access
            // request and response properties.
            var application = (HttpApplication) source;
            var context = application.Context;
            var filePath = context.Request.FilePath;
            var fileExtension =
                VirtualPathUtility.GetExtension(filePath);
            if (fileExtension.Equals(".aspx")) {
                context.Response.Write("<h1><font color=red>" +
                                       "HelloWorldModule: Beginning of Request" +
                                       "</font></h1><hr>");
            }
        }

        private void Application_EndRequest(Object source, EventArgs e) {
            var application = (HttpApplication) source;
            var context = application.Context;
            var filePath = context.Request.FilePath;
            var fileExtension =
                VirtualPathUtility.GetExtension(filePath);
            if (fileExtension.Equals(".aspx")) {
                context.Response.Write("<hr><h1><font color=red>" +
                                       "HelloWorldModule: End of Request</font></h1>");
            }
        }

        public void Dispose() {}
    }
}