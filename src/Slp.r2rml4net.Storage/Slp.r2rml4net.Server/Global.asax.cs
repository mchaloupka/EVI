using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Slp.r2rml4net.Server.R2RML;

namespace Slp.r2rml4net.Server
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            StorageWrapper.AppStart();
        }

        protected void Application_End()
        {
            StorageWrapper.AppEnd();
        }
    }
}
