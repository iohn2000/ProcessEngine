using System.Web;
using System.Web.Optimization;

namespace Kapsch.IS.EMD.EMD20Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //telerik-Kendo Bundles
            bundles.IgnoreList.Clear();

                        bundles.Add(new ScriptBundle("~/bundles/kendo").Include(
                        "~/Scripts/kendo/2017.3.1026/kendo.all.min.js",
                        "~/Scripts/kendo/2017.3.1026/kendo.aspnetmvc.min.js"));
            //bundles.Add(new ScriptBundle("~/bundles/kendo").Include(
            //            "~/Scripts/kendo/kendo.all.min.js?" + System.Configuration.ConfigurationManager.AppSettings["JavaScriptVersion"],
            //            "~/Scripts/kendo/kendo.aspnetmvc.min.js?"  + System.Configuration.ConfigurationManager.AppSettings["JavaScriptVersion"]));
            bundles.Add(new StyleBundle("~/Content/kendo/2017.3.1026/css").Include(
                        "~/Content/kendo/2017.3.1026/kendo.common-bootstrap.min.css",
                        "~/Content/kendo/2017.3.1026/kendo.bootstrap.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/kapsch").Include("~/Scripts/kapsch/kapsch.js"));

            //MS MVC4 Bundles
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                       "~/Scripts/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            //Kapsch Bundles
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/typography.css"));

            // Material Icons
            // bundles.Add(new StyleBundle("~/Content/materialicons").Include("~/Content/materialicons/materialicons.css"));


            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));
        }
    }
}
