using System.Web.Optimization;
//nurul huda
namespace Aplos
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            #region css

            bundles.Add(new StyleBundle("~/bundles/css")
                   .Include(
                     "~/Content/bootstrap.min.css"
                   , "~/Content/bootstrap-datepicker3.min.css"
                   , "~/Content/jquery.timepicker.min.css"
                   , "~/Content/toaster.css"
                   , "~/Content/css/select2.css"
                   , "~/Content/angucomplete-alt.css"
                   , "~/Content/style.css"
                   , "~/Content/style-dashboard.css"
                   , "~/SyncfusionLib/css/web/default-theme/ej.web.all.min.css"
                    ));

            bundles.Add(new StyleBundle("~/bundles/fonts")
                   .Include("~/Content/OpenSans.css")
                   .Include("~/css/font-awesome.css", new CssRewriteUrlTransform()));

            #endregion css

            #region js

            bundles.Add(new ScriptBundle("~/bundles/jq").Include(
                     "~/Scripts/jquery-3.2.1.min.js"
                   , "~/Scripts/jquery.timepicker.min.js"
                   , "~/Scripts/bootstrap.min.js"
                   , "~/Scripts/bootstrap-datepicker.js"
                   , "~/Scripts/select2.js"
                   , "~/Scripts/jquery-menu.js"
                   , "~/Scripts/jquery-common.js"
                   , "~/Scripts/moment.min.js"

                   ));

            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                     "~/Scripts/angular.js"
                   , "~/Scripts/angular-route.js"
                   , "~/Scripts/angular-cookies.js"
                   , "~/Scripts/dirPagination.js"
                   , "~/Scripts/toaster.js"
                   , "~/Scripts/angular-directive.js"
                   , "~/Scripts/angular-factory.js"
                   , "~/Scripts/angular-cbo-factory.js"
                   , "~/Scripts/angular-service-factory.js"
                   , "~/Scripts/angular-filter.js"
                   , "~/Scripts/angular-constant.js"
                   , "~/Scripts/angular-constant-path.js"
                   , "~/Scripts/angularjs-dropdown-multiselect.min.js"
                   , "~/Scripts/ag-grid-enterprise.min.js"
                   , "~/Scripts/angucomplete-alt.js"
                   , "~/Scripts/jquery.signalR-2.4.1.min.js"
                   , "~/Scripts/SignalR-factory.js"
                   , "~/Scripts/SignalRInit.js"));

            bundles.Add(new ScriptBundle("~/bundles/syncfusion").Include(
                "~/SyncfusionLib/JSP/Scripts/external/jsrender.min.js"
                 , "~/SyncfusionLib/JSP/Scripts/web/ej.web.all.min.js"
                 , "~/SyncfusionLib/JSP/Scripts/common/ej.widget.angular.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/ie").Include("~/Areas/IE/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/securities").Include("~/Areas/Securities/Scripts/*.js"));



            bundles.Add(new ScriptBundle("~/bundles/chart").Include(
                   "~/Scripts/Chart.min.js",
                   "~/Scripts/loader.js"));

            bundles.Add(new ScriptBundle("~/bundles/login").Include(

                   "~/Scripts/Apps/portalController.js",
                   "~/Scripts/Apps/passwordChangeFirstLoginController.js",
                   "~/Scripts/Apps/downloadAuthTokenController.js",
                   "~/Scripts/Apps/accountController.js",
                   "~/Scripts/Apps/hrmsPanelLogin.js",
                   "~/Scripts/Apps/loginApp.js"));

            bundles.Add(new ScriptBundle("~/bundles/upanellogin").Include(
                  "~/Scripts/UPanelApp/upanelLoginController.js",
                  "~/Scripts/UPanelApp/upanelLoginApp.js"));

            bundles.Add(new ScriptBundle("~/bundles/upanel").Include(
                   "~/Scripts/Apps/manpowerbudgetDashboardController.js",
                   "~/Scripts/UPanelApp/upanelDashboardController.js",
                   "~/Scripts/UPanelApp/plantSelectionController.js",
                   "~/Scripts/UPanelApp/upanelLoginController.js",
                   "~/Scripts/UPanelApp/upanelLogoutController.js",
                   "~/Scripts/UPanelApp/upanelApp.js"));


            #endregion js

#if DEBUG
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}