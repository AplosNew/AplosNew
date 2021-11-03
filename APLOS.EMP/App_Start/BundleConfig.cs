using System.Web.Optimization;

namespace Aplos
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            #region css
            bundles.Add(new StyleBundle("~/bundles/css")
                   .Include(
                   "~/Content/bootstrap.min.css",
                   "~/Content/bootstrap-datepicker3.min.css",
                   "~/Content/jquery.timepicker.min.css",
                   "~/Content/toaster.css",
                   "~/Content/style.css",
                   "~/Content/dashboard.css",
                   "~/Content/angucomplete-alt.css"));

            bundles.Add(new StyleBundle("~/bundles/fonts")
                   .Include("~/Content/OpenSans.css")
                   .Include("~/css/font-awesome.css", new CssRewriteUrlTransform()));
            #endregion

            #region js
            bundles.Add(new ScriptBundle("~/bundles/jq")
                   .Include(
                    "~/Scripts/jquery-{version}.js",
                    "~/Scripts/jquery.timepicker.min.js",
                    "~/Scripts/popper.min.js",
                    "~/Scripts/bootstrap.min.js",
                    "~/Scripts/bootstrap-datepicker.js",
                    "~/Scripts/jquery-menu.js",
                    "~/Scripts/jquery-common.js",
                    "~/Scripts/Chart.min.js",
                    "~/Scripts/loader.js"
                    ));

            bundles.Add(new ScriptBundle("~/bundles/angular")
                   .Include(
                   "~/Scripts/angular.js",
                   "~/Scripts/angular-route.js",
                   "~/Scripts/angular-cookies.js",
                   "~/Scripts/dirPagination.js",
                   "~/Scripts/toaster.js",
                   "~/Scripts/angular-directive.js",
                   "~/Scripts/angular-factory.js",
                   "~/Scripts/angular-cbo-factory.js",
                   "~/Scripts/angular-filter.js",
                   "~/Scripts/angular-constant.js",
                   "~/Scripts/angularjs-dropdown-multiselect.min.js",
                   "~/Scripts/angucomplete-alt.js",
				   "~/Scripts/xlsx.full.min.js"
				   ));

            bundles.Add(new ScriptBundle("~/bundles/aplosemp")
                    .Include("~/Scripts/EmpApp/*.js"));

            bundles.Add(new ScriptBundle("~/bundles/apps")
                   .Include(
                   "~/Scripts/Apps/aplosEmpApp.js"
                   ));

            bundles.Add(new ScriptBundle("~/bundles/empapps")
                 .Include(
                 "~/Scripts/Apps/employeeAccessApp.js"
                 ));
            #endregion
        }
    }
}
