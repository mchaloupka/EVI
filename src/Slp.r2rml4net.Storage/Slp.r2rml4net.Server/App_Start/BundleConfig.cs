using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace Slp.r2rml4net.Server
{
    public static class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/Content/js/jquery").Include(
                 "~/Content/js/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/Content/js/bootstrap").Include(
                 "~/Content/js/bootstrap.js"));


            bundles.Add(new StyleBundle("~/Content/css/bootstrap").Include(
                "~/Content/css/bootstrap*"));

            bundles.Add(new StyleBundle("~/Content/css/style").Include(
                "~/Content/css/style.css"));
        }
    }
}