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



            bundles.Add(new ScriptBundle("~/bundles/accounts").Include("~/Areas/Accounts/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/addresses").Include("~/Areas/Addresses/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/banks").Include("~/Areas/Banks/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/currencies").Include("~/Areas/Currencies/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/employees").Include("~/Areas/Employees/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/attendances").Include("~/Areas/Attendances/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/fixedassets").Include("~/Areas/FixedAssets/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/ie").Include("~/Areas/IE/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/productions").Include("~/Areas/Productions/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/projects").Include("~/Areas/Projects/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/logs").Include("~/Areas/Logs/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/machines").Include("~/Areas/Machines/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/materials").Include("~/Areas/Materials/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/menus").Include("~/Areas/Menus/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/modules").Include("~/Areas/Modules/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/ordermanagements").Include("~/Areas/OrderManagements/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/taskmanagement").Include("~/Areas/TaskManagement/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/sm").Include("~/Areas/SalesManagements/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/Biometric").Include("~/Areas/Biometric/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/IssueTracker").Include("~/Areas/IssueTracker/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/TaskScheduler").Include("~/Areas/TaskScheduler/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/Products").Include("~/Areas/Products/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/Commercial").Include("~/Areas/Commercial/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/Costings").Include("~/Areas/Costings/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/QMS").Include("~/Areas/QMS/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/Farming").Include("~/Areas/Farming/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/EmployeeServices").Include("~/Areas/EmployeeServices/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/JobWork").Include("~/Areas/JobWork/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/Outsourcing").Include("~/Areas/Outsourcing/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/Leave").Include("~/Areas/Leave/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/PerformanceManagement").Include("~/Areas/PerformanceManagement/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/MeetingManagement").Include("~/Areas/MeetingManagement/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/Administration").Include("~/Areas/Administration/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/mis").Include("~/Areas/mis/Scripts/*.js"));


            bundles.Add(new ScriptBundle("~/bundles/organizations").Include(
                  "~/Areas/Organizations/Scripts/Designations/*.js"
                , "~/Areas/Organizations/Scripts/ManpowerBudgets/*.js"
                , "~/Areas/Organizations/Scripts/Positions/*.js"
                , "~/Areas/Organizations/Scripts/*.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/parties").Include("~/Areas/Parties/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/processes").Include("~/Areas/Processes/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/products").Include("~/Areas/Products/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/securities").Include("~/Areas/Securities/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/setups").Include("~/Areas/Setups/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/skills").Include("~/Areas/Skills/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/workcenters").Include("~/Areas/WorkCenters/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/humanresource").Include("~/Areas/HumanResource/Scripts/*.js"));
            bundles.Add(new ScriptBundle("~/bundles/payrolls").Include("~/Areas/Payrolls/Scripts/*.js"));

            bundles.Add(new ScriptBundle("~/bundles/chart").Include(
                   "~/Scripts/Chart.min.js",
                   "~/Scripts/loader.js"));

            bundles.Add(new ScriptBundle("~/bundles/login").Include(
                   "~/Areas/aPanel/Scripts/apanelLoginController.js",
                   "~/Areas/mPanel/Scripts/mpanelLoginController.js",
                   "~/Areas/uPanel/Scripts/upanelLoginController.js",
                   "~/Areas/Recruitments/Scripts/preRecruitmentLoginController.js",
                   "~/Scripts/Apps/portalController.js",
                   "~/Scripts/Apps/passwordChangeFirstLoginController.js",
                   "~/Scripts/Apps/downloadAuthTokenController.js",
                   "~/Scripts/Apps/accountController.js",
                   "~/Scripts/Apps/hrmsPanelLogin.js",
                   "~/Scripts/Apps/loginApp.js"));

            bundles.Add(new ScriptBundle("~/bundles/cpanellogin").Include(
                  "~/Scripts/CPanelApp/cpanelLoginController.js",
                  "~/Scripts/CPanelApp/cpanelLoginApp.js"));

            bundles.Add(new ScriptBundle("~/bundles/cpanel").Include(
                  "~/Scripts/CPanelApp/cpanelLoginController.js",
                  "~/Scripts/CPanelApp/cpanelLogoutController.js",
                  "~/Scripts/CPanelApp/queryEditorController.js",
                  "~/Scripts/CPanelApp/cpanelApp.js"));

            bundles.Add(new ScriptBundle("~/bundles/apanellogin").Include(
                  "~/Scripts/APanelApp/apanelLoginController.js",
                  "~/Scripts/APanelApp/apanelLoginApp.js"));

            bundles.Add(new ScriptBundle("~/bundles/apanel").Include(
                   "~/Scripts/APanelApp/apanelLoginController.js",
                   "~/Scripts/APanelApp/apanelLogoutController.js",
                   "~/Scripts/Apps/manpowerBudgetDashboardController.js",
                   "~/Scripts/APanelApp/apanelApp.js"));

            bundles.Add(new ScriptBundle("~/bundles/mpanellogin").Include(
                  "~/Scripts/MPanelApp/mpanelLoginController.js",
                  "~/Scripts/MPanelApp/mpanelLoginApp.js"));

            bundles.Add(new ScriptBundle("~/bundles/mpanel").Include(
                  "~/Scripts/MPanelApp/mpanelDashboardController.js",
                  "~/Scripts/MPanelApp/plantSelectionController.js",
                  "~/Scripts/MPanelApp/mpanelLoginController.js",
                  "~/Scripts/MPanelApp/mpanelLogoutController.js",
                  "~/Scripts/MPanelApp/mPanelApp.js"));

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

            bundles.Add(new ScriptBundle("~/bundles/epanellogin").Include(
                   "~/Scripts/MyApp/epanelLoginController.js",
                   "~/Scripts/MyApp/epanelLogoutController.js",
                   "~/Scripts/MyApp/epanelLoginApp.js"));

            bundles.Add(new ScriptBundle("~/bundles/epanel").Include(
                  "~/Areas/Accounts/Scripts/expenseBookingPotalController.js",
                  "~/Areas/Accounts/Scripts/expenseBookingApprovalPotalController.js",
                  "~/Areas/Accounts/Scripts/expenseBookingDepartmentApprovalPotalController.js",
                  "~/Areas/Employees/Scripts/profileViewController.js",
                  "~/Areas/Accounts/Scripts/expenseBookingDepartmentApprovalPotalController.js",
                  "~/Areas/Accounts/Scripts/expenseBookingCheckedByPotalController.js",
                  "~/Areas/Employees/Scripts/profileViewController.js",
                  "~/Areas/Employees/Scripts/jobCardInformationController.js",
                  "~/Areas/Employees/Scripts/leaveApplicationController.js",
                  "~/Areas/TaskManagement/Scripts/taskMasterController.js",
                  "~/Areas/Setups/Scripts/tnaSettingMasterController.js",
                  "~/Areas/Accounts/Scripts/employeeAdvanceRequisitionController.js",
                  "~/Scripts/MyApp/epanelLoginController.js",
                  "~/Scripts/MyApp/empPasswordChangeController.js",
                  "~/Scripts/MyApp/epanelLogoutController.js",
                  "~/Scripts/MyApp/myAppCalendarController.js",
                  "~/Areas/Products/Scripts/RequisitionController.js",
                  "~/Areas/Products/Scripts/InventoryCheckApprovedController.js",
                  "~/Areas/MeetingManagement/Scripts/MeetingPointsController.js",
                  "~/Areas/MeetingManagement/Scripts/MeetingReportsController.js",
                  "~/Areas/HumanResource/Scripts/EmployeeUnderstandingHeadController.js",
                  "~/Areas/HumanResource/Scripts/EmployeeGoalSettingController.js",
                  "~/Areas/HumanResource/Scripts/FuguaiTransactionController.js",
                  "~/Areas/HumanResource/Scripts/FuguaiReportController.js",
                  "~/Areas/Materials/Scripts/DetentionLogController.js",
                  "~/Areas/Materials/Scripts/DetentionLogoutController.js",
                  "~/Areas/Materials/Scripts/DetentionLogReportController.js",
                  "~/Areas/Employees/Scripts/myappEmployeeLedgerReportController.js",
                  "~/Areas/HumanResource/Scripts/VehicleMovementRequisitionController.js",
                  "~/Areas/FixedAssets/Scripts/CapitalizeAssetRegisterApprovalController.js",
                  "~/Areas/Administration/Scripts/GeneralApprovedController.js",
                  "~/Scripts/MyApp/epanelApp.js"
                  ));

            bundles.Add(new ScriptBundle("~/bundles/tpanellogin").Include(
                     "~/Scripts/MyTeacher/tpanelLoginController.js",
                     "~/Scripts/MyTeacher/tpanelLogoutController.js",
                     "~/Scripts/MyTeacher/tpanelLoginApp.js"));

            bundles.Add(new ScriptBundle("~/bundles/tpanel").Include(

                  "~/Scripts/MyTeacher/tpanelLoginController.js",
                  "~/Scripts/MyTeacher/teacherPasswordChangeController.js",
                  "~/Scripts/MyTeacher/tpanelLogoutController.js",
                  "~/Scripts/MyTeacher/myTeacherCalendarController.js",
                    "~/Areas/TaskManagement/Scripts/TeacherScheduleController.js",
                  "~/Scripts/MyTeacher/tpanelApp.js"
                  ));

            bundles.Add(new ScriptBundle("~/bundles/ppanellogin").Include(
                   "~/Scripts/MyParents/ppanelLoginController.js",
                   "~/Scripts/MyParents/ppanelLogoutController.js",
                   "~/Scripts/MyParents/ppanelLoginApp.js"));

            bundles.Add(new ScriptBundle("~/bundles/ppanel").Include(

                  "~/Scripts/MyParents/ppanelLoginController.js",
                  "~/Scripts/MyParents/parentsPasswordChangeController.js",
                  "~/Scripts/MyParents/ppanelLogoutController.js",
                  "~/Scripts/MyParents/myParentsCalendarController.js",
                    "~/Areas/TaskManagement/Scripts/TeacherScheduleController.js",
                  "~/Scripts/MyParents/ppanelApp.js"
                  ));

            bundles.Add(new ScriptBundle("~/bundles/recruitment").Include(
                 "~/Areas/Recruitments/Scripts/preRecruitmentLoginController.js",
                 "~/Areas/Recruitments/Scripts/changePinController.js",
                 "~/Areas/Recruitments/Scripts/preRecruitmentController.js",
                 "~/Areas/Recruitments/Scripts/dashBoardController.js",
                 "~/Areas/Addresses/Scripts/addressService.js",
                 "~/Areas/Recruitments/Scripts/recruitmentApp.js"));

            bundles.Add(new ScriptBundle("~/bundles/dailyattendance").Include(
                 "~/Areas/DailyAttendance/Scripts/dailyAttendanceInOutLoginController.js",
                 "~/Areas/DailyAttendance/Scripts/changePinController.js",
                 "~/Areas/DailyAttendance/Scripts/dailyAttdInOutController.js",
                 "~/Areas/DailyAttendance/Scripts/dashBoardController.js",

                 "~/Areas/DailyAttendance/Scripts/dailyattendanceApp.js"));


            bundles.Add(new ScriptBundle("~/bundles/dapanelLogin").Include(
                   "~/Scripts/DailyAttendances/dapanelLoginController.js",
                  "~/Scripts/DailyAttendances/ppanelLogoutController.js",
                   "~/Scripts/DailyAttendances/dapanelLoginApp.js"));

            bundles.Add(new ScriptBundle("~/bundles/dapanel").Include(

                  "~/Scripts/DailyAttendances/ppanelLogoutController.js",
                  "~/Scripts/DailyAttendances/daPasswordChangeController.js",
                  "~/Scripts/DailyAttendances/dapanelApp.js",
                  "~/Scripts/HumanResource/DailyAttendanceStatusReportController.js"
                  ));

            #endregion js

#if DEBUG
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}