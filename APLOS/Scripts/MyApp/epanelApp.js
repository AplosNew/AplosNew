/// <reference path="../angular-constant-path.js" />
'use strict';
var epanelApp = angular.module('epanelApp', ['ngRoute', 'ngCookies', 'angularUtils.directives.dirPagination', 'toaster', 'ui.calendar', 'ui.bootstrap', "ejangular"])
    .controller('expenseBookingApprovalPotalController', expenseBookingApprovalPotalController)
    .controller('expenseBookingDepartmentApprovalPotalController', expenseBookingDepartmentApprovalPotalController)
    .controller('expenseBookingCheckedByPotalController', expenseBookingCheckedByPotalController)
    .controller('expenseBookingPotalController', expenseBookingPotalController)
    .controller('epanelLogoutController', epanelLogoutController)
    .controller('empPasswordChangeController', empPasswordChangeController)
    .controller("profileViewController", profileViewController)
    .controller("jobCardInformationController", jobCardInformationController)
    .controller("leaveApplicationController", leaveApplicationController)
    .controller("currencyBaseController", currencyBaseController)
    .controller("employeeBaseController", employeeBaseController)
    .controller("myAppCalendarController", myAppCalendarController)
    .controller("partyBaseController", partyBaseController)
    .controller("employeeJobDescriptionController", employeeJobDescriptionController)
    .controller("taskListController", taskListController)
    //.controller("issueTransactionController", issueTransactionController)
    .controller("taskScheduleController", taskScheduleController)
    .controller("employeeAdvanceRequisitionController", employeeAdvanceRequisitionController)
    .controller("employeeAdvanceRequisitionEditController", employeeAdvanceRequisitionEditController)
    .controller("employeeAdvanceRequisitionApprovalController", employeeAdvanceRequisitionApprovalController)
    .controller("baseMaterialAndArticleController", baseMaterialAndArticleController)
    .controller("RequisitionController", RequisitionController)
    .controller("InventoryrequisitionCheckbyController", InventoryrequisitionCheckbyController)
    .controller("InventoryrequisitionapprovedbyController", InventoryrequisitionapprovedbyController)
    .controller("PurchaseOrderController", PurchaseOrderController)
    .controller("PurchaseOrderCheckController", PurchaseOrderCheckController)
    .controller("PurchaseOrderApproveController", PurchaseOrderApproveController)
    .controller("grnApprovalController", grnApprovalController)
    .controller("IssueSlipController", IssueSlipController)
    .controller("IssueSlipCheckedByController", IssueSlipCheckedByController)
    .controller("IssueSlipApprovedByController", IssueSlipApprovedByController)
    .controller("AssetIssueSlipController", AssetIssueSlipController)
    .controller("MaterialIssueSlipController", MaterialIssueSlipController)
    .controller("TNAReportsController", TNAReportsController)
    .controller("ServiceRequisitionController", ServiceRequisitionController)
    .controller("ServiceRequisitionCheckApprovedController", ServiceRequisitionCheckApprovedController)
    .controller("ServicePOByRequisitionController", ServicePOByRequisitionController)
    .controller("ServicePOCheckController", ServicePOCheckController)
    .controller("ServicePOApprovedController", ServicePOApprovedController)
    .controller("GatePassController", GatePassController)
    .controller("InoutGetpassCheckedController", InoutGetpassCheckedController)
    .controller("PendingGateoutListController", PendingGateoutListController)
    .controller('EmployeeGoalSettingController', EmployeeGoalSettingController)
    .controller("ServiceAckCheckedApprovedByController", ServiceAckCheckedApprovedByController)
    .controller("PurchaseReturnCheckedApprovedByController", PurchaseReturnCheckedApprovedByController)
    .controller("inventorySalesCheckApproveController", inventorySalesCheckApproveController)
    .controller("inventoryScrapCheckApproveController", inventoryScrapCheckApproveController)
    .controller("employeeMyAppLeaveApplicationController", employeeMyAppLeaveApplicationController)
    .controller("firstAuthEmpLeaveApprovalController", firstAuthEmpLeaveApprovalController)
    .controller("GatePassPotalController", GatePassPotalController)
    .controller("MeetingPointsController", MeetingPointsController)
    .controller("MeetingReportsController", MeetingReportsController)
    .controller("EmployeeUnderstandingHeadController", EmployeeUnderstandingHeadController)
    .controller("FuguaiTransactionController", FuguaiTransactionController)
    .controller("FuguaiReportController", FuguaiReportController)
    .controller("DetentionLogController", DetentionLogController)
    .controller("DetentionLogoutController", DetentionLogoutController)
    .controller("DetentionLogReportController", DetentionLogReportController)
    .controller("myappEmployeeLedgerReportController", myappEmployeeLedgerReportController)
    .controller("VehicleMovementRequisitionController", VehicleMovementRequisitionController)
    .controller("CapitalizeAssetRegisterApprovalController", CapitalizeAssetRegisterApprovalController)
    .controller("multipleVPController", multipleVPController)
    .controller("GeneralApprovedController", GeneralApprovedController)
    //#endregion

    .config(['$routeProvider', '$locationProvider', '$httpProvider', function ($routeProvider, $locationProvider, $httpProvider) {
        $routeProvider
            .when('/', {
                templateUrl: 'MyApp/dashboard'
            })
            .when('epanel', {
                templateUrl: 'MyApp/dashboard'
            })
            .when('/dashboard', {
                templateUrl: 'MyApp/dashboard'
            })
            .when("/expense-booking-potal", {
                templateUrl: "Accounts/ExpenseBooking/ExpenseBookingPotal",
                controller: "expenseBookingPotalController"
            })
            .when("/expense-booking-approval-potal", {
                templateUrl: "Accounts/ExpenseBooking/ExpenseBookingApprovalPotal",
                controller: "expenseBookingApprovalPotalController"
            })
            .when("/expense-department-approval-potal", {
                templateUrl: "Accounts/ExpenseBooking/ExpenseBookingDepartmentApprovalPotal",
                controller: "expenseBookingDepartmentApprovalPotalController"
            })
            .when("/expense-checkedby-potal", {
                templateUrl: "Accounts/ExpenseBooking/ExpenseBookingCheckedByPotal",
                controller: "expenseBookingCheckedByPotalController"
            })
            .when("/employee-profile-view", {
                templateUrl: "Employees/EmployeeInformation/ProfileView",
                controller: "profileViewController"
            })
            .when("/employee-job-card", {
                templateUrl: "Employees/EmployeeInformation/JobCard",
                controller: "jobCardInformationController"
            })
            .when("/employee-leave-application", {
                templateUrl: "Employees/EmployeeMyAppLeaveApplication/Aplos",
                controller: "employeeMyAppLeaveApplicationController"
            })
            .when("/first-auth-employee-leave-approval", {
                templateUrl: "Leave/FirstAuthEmpLeaveApproval",
                controller: "firstAuthEmpLeaveApprovalController"
            })
            .when("/employee-advance-requisition", {
                templateUrl: "Accounts/Advance/employeeAdvanceRequisition/",
                controller: "employeeAdvanceRequisitionController"
            })
            .when("/employee-advance-requisition-edit", {
                templateUrl: "Accounts/Advance/EmployeeAdvanceRequisitionEdit",
                controller: "employeeAdvanceRequisitionEditController"
            })
            .when("/employee-advance-requisition-approval", {
                templateUrl: "Accounts/Advance/EmployeeAdvanceRequisitionApprove",
                controller: "employeeAdvanceRequisitionApprovalController"
            })
            .when('/employee-job-description', {
                templateUrl: 'employees/employeejobdescription/',
                controller: 'employeeJobDescriptionController'
            })
            .when('/login', {
                templateUrl: 'MyApp/login',
                controller: 'portalLoginController'
            })
            .when('/password-change/:id', {
                templateUrl: 'MyApp/passwordchange',
                controller: 'empPasswordChangeController'
            })
            .when('/employee-calendar', {
                templateUrl: 'MyApp/Calendar',
                controller: 'myAppCalendarController'
            })
            .when('/task-list', {
                templateUrl: 'TaskManagement/TaskList/',
                controller: 'taskListController'
            })
            .when('/detention-log-report', {
                templateUrl: 'Materials/DetentionLogReport',
                controller: 'DetentionLogReportController'
            })
            
            //.when('/issue-transaction', {
            //    templateUrl: 'IssueTracker/IssueTransaction/Aplos',
            //    controller: "issueTransactionController"
            //})



            //#region Requisition
            .when('/general-approved', {
                templateUrl: 'Administration/GeneralCheckedApproved/GeneralApproved',
                controller: 'GeneralApprovedController'
            })
            .when('/requisition', {
                templateUrl: 'Products/Requisition/Aplos',
                controller: 'RequisitionController'
            })

            .when('/requisition-approvedby', {
                templateUrl: 'Products/InventoryCheckApproved/ReqAuthorized',
                controller: 'InventoryrequisitionapprovedbyController'
            })
            .when('/requisition-checkby', {
                templateUrl: 'Products/InventoryCheckApproved/Aplos',
                controller: 'InventoryrequisitionCheckbyController'
            })

            .when('/purchaseOrder-Checked-By', {
                templateUrl: 'Products/PurchaseOrder/POChecke',
                controller: 'PurchaseOrderCheckController'
            })
            .when('/purchaseOrder-Authorized', {
                templateUrl: 'Products/PurchaseOrder/POApprove',
                controller: 'PurchaseOrderApproveController'
            })
            .when('/poclosed', {
                templateUrl: 'Products/PurchaseOrder/POClosed',
                controller: 'PurchaseOrderController'
            })
            .when('/purchaseOrder-unapproval', {
                templateUrl: 'Products/PurchaseOrder/POUnApproval',
                controller: 'PurchaseOrderController'
            })
            .when('/Grn-Check', {
                templateUrl: 'Products/GoodsReceiveNote/GRNCheck',
                controller: 'grnApprovalController'
            })
            .when('/grn-approval', {
                templateUrl: 'Products/GoodsReceiveNote/GRNApproval',
                controller: 'grnApprovalController'
            })


            .when('/issueslip-check', {
                templateUrl: 'Products/GoodsReceiveNote/IssueSlipCheck',
                controller: 'IssueSlipCheckedByController'
            })
            .when('/approving-issue-slip', {
                templateUrl: 'Products/GoodsReceiveNote/ApprovingIssueSlip',
                controller: 'IssueSlipApprovedByController'
            })
            .when('/Material-Wise-issue-slip', {
                templateUrl: 'Products/GoodsReceiveNote/MaterialIssueSlip',
                controller: 'MaterialIssueSlipController'
            })
            .when('/asset-issue-slip', {
                templateUrl: 'Products/GoodsReceiveNote/AssetIssueSlip',
                controller: 'AssetIssueSlipController'
            })
            //#endregion

            //#region service Requisition
            .when('/service-requisition-creation', {
                templateUrl: 'Products/ServiceRequisition/ServiceReqCreation',
                controller: 'ServiceRequisitionController'
            })



            .when('/service-requisition-checking', {
                templateUrl: 'Products/ServiceRequisitionCheckApproved/ServiceReqCheck',
                controller: 'ServiceRequisitionCheckApprovedController'
            })



            .when('/service-requisition-Approval', {
                templateUrl: 'Products/ServiceRequisitionCheckApproved/ServiceReqApprove',
                controller: 'ServiceRequisitionCheckApprovedController'
            })

            .when('/Service-PO-Checking', {
                templateUrl: 'Products/PurchaseOrder/ServicePOCheck',
                controller: 'ServicePOCheckController'
            })

            .when('/gate-pass-checked', {
                templateUrl: 'Products/GateentryToken/GatePassChecked',
                controller: 'GatePassPotalController'
            })
            .when('/gate-pass-approved', {
                templateUrl: 'Products/GateentryToken/GatePassApproved',
                controller: 'GatePassPotalController'
            })
            .when('/gate-pass-dispatch', {
                templateUrl: 'Products/GateentryToken/GatePassApprovedBySecurity',
                controller: 'GatePassController'
            })

            .when('/In-out-gate-pass-Checking', {
                templateUrl: 'Products/GateentryToken/InOutGatePassCheck',
                controller: 'InoutGetpassCheckedController'
            })

            .when('/Pending-Gate-out-List', {
                templateUrl: 'Products/GateentryToken/PendingGateoutList',
                controller: 'PendingGateoutListController'
            })

            .when('/Service-PO-Approval', {
                templateUrl: 'Products/PurchaseOrder/ServicePOApproval',
                controller: 'ServicePOApprovedController'
            })
            .when('/Service-Ack-Checked', {
                templateUrl: 'Products/PurchaseOrder/ServiceAcknowledgementChecked',
                controller: 'ServiceAckCheckedApprovedByController'
            })
            .when('/Service-Ack-Approval', {
                templateUrl: 'Products/PurchaseOrder/ServiceAcknowledgementApproved',
                controller: 'ServiceAckCheckedApprovedByController'
            })

            .when('/Purchase-Return-Checked', {
                templateUrl: 'Products/GoodsReceiveNote/PurchaseReturnChecked',
                controller: 'PurchaseReturnCheckedApprovedByController'
            })

            .when('/Purchase-Return-Approved', {
                templateUrl: 'Products/GoodsReceiveNote/PurchaseReturnApprove',
                controller: 'PurchaseReturnCheckedApprovedByController'
            })

            .when('/meeting-points', {
                templateUrl: 'MeetingManagement/MeetingPoints',
                controller: 'MeetingPointsController'
            })

            .when('/inventory-sales-checking', {
                templateUrl: 'Products/InventoryIssue/InventorySalesChecked',
                controller: 'inventorySalesCheckApproveController'
            })

            .when('/inventory-sales-Approval', {
                templateUrl: 'Products/InventoryIssue/InventorySalesApproved',
                controller: 'inventorySalesCheckApproveController'
            })

            .when('/inventory-scrap-checking', {
                templateUrl: 'Products/InventoryIssue/InventoryScrapChecked',
                controller: 'inventoryScrapCheckApproveController'
            })

            .when('/inventory-scrap-approval', {
                templateUrl: 'Products/InventoryIssue/InventoryScrapApproved',
                controller: 'inventoryScrapCheckApproveController'
            })

            .when('/meeting-reports', {
                templateUrl: 'MeetingManagement/MeetingReports/ReportView',
                controller: 'MeetingReportsController'
            })
            .when('/employee-understanding-head', {
                templateUrl: 'humanResource/EmployeeUnderstandingHead/Aplos',
                controller: 'EmployeeUnderstandingHeadController'
            })
            .when('/employee-goal-setting', {
                templateUrl: 'humanResource/EmployeeGoalSetting/Aplos',
                controller: 'EmployeeGoalSettingController'
            })

            .when('/fuguai-transaction', {
                templateUrl: 'humanResource/FuguaiTransaction/Aplos',
                controller: 'FuguaiTransactionController'
            })

            .when('/fuguai-report', {
                templateUrl: 'humanResource/FuguaiReport/Aplos',
                controller: 'FuguaiReportController'
            })
            .when('/detention-log', {
                templateUrl: 'Materials/DetentionLog/Aplos',
                controller: 'DetentionLogController'
            })

            .when('/detention-logout', {
                templateUrl: 'materials/DetentionLogout/Aplos',
                controller: 'DetentionLogoutController'
            })
            .when('/myapp-employee-ledger', {
                templateUrl: 'Employees/EmployeeReport/MyappEmployeeLedger',
                controller: 'myappEmployeeLedgerReportController'
            })
            .when('/vehicle-movement-requisition',
                {
                    templateUrl: 'humanresource/VehicleMovementMaster/VehicleMovementRequisition',
                    controller: 'VehicleMovementRequisitionController'
                })
            .when('/capitalize-asset-register-approval', {
                templateUrl: 'FixedAssets/FixedAssetRegister/CARApproval',
                controller: 'CapitalizeAssetRegisterApprovalController'
            }) 
            .when("/multiple-vendor-payment", {
                templateUrl: "Accounts/Invoice/multipleVP",
                controller: "multipleVPController"
            })
            //#endregion

            .when('/logout', {
                template: ' ',
                controller: 'epanelLogoutController'
            })
            //.when('/task-create', {
            //    templateUrl: 'taskmanagement/TaskMaster/aplos',
            //    controller: 'taskMasterController'
            //})
            .otherwise({
                redirectTo: 'MyApp/login'
            });
    }])
    .run(['$rootScope', '$timeout', '$cookies', '$window', "$filter", "$http", function ($rootScope, $timeout, $cookies, $window, $filter, $http) {
        $rootScope.title = 'MyApp';
        $rootScope.bootPoint = '#!/';

        $window.employeeId = $cookies.get("MyAppemployeeId");
        $window.employeeName = $cookies.get("MyAppemployeeName");
        $window.companyGroupId = $cookies.get("MyAppgroupId");
        $window.companyId = $cookies.get("MyAppcompanyId");
        $window.plantId = $cookies.get("MyAppplantId");
        $rootScope.plantName = $cookies.get("MyAppplantName");
        $rootScope.companyGroupLogo = virtualPath.LogoOrImage + $cookies.get("gImage");

        $rootScope.Message = '';
        $rootScope.HeaderText = '';
        $rootScope.ShowError = function (message, headerText) {
            $rootScope.Message = message;
            $rootScope.HeaderText = headerText;
            $("#dialogMessage").ejDialog("setTitle", headerText);
            $("#dialogMessage").ejDialog("open");
        }
        $rootScope.MyAppuserImage = virtualPath.EmployeePic;

        $rootScope.report = function (file_src) {
            $("#iframe_div_for_report").empty();
            var frame = $('<iframe id="report">')
                .attr('height', '0px')
                .attr('visibility', 'hidden')
                .attr('width', '0px');
            frame.on('load', function () {

                try {
                    var text = angular.fromJson($('#report')[0].contentDocument.body.innerText);

                    if (text.hasOwnProperty('Message')) {
                        if (angular.isUndefinedOrNull(text.Message) === false) {
                            $('<div id="message">').attr('height', '0px')
                                .attr('visibility', 'hidden')
                                .attr('width', '0px').appendTo('#iframe_div_for_report');
                            $("#message").ejDialog({
                                title: "Error"
                            });
                            $("#message").ejDialog("setContent", text.Message);

                        }
                    }
                    else {
                        var text1 = $('#report')[0].contentDocument.body.innerText;

                        $('<div id="message">').attr('height', '0px')
                            .attr('visibility', 'hidden')
                            .attr('width', '0px').appendTo('#iframe_div_for_report');
                        $("#message").ejDialog({
                            title: "Error"
                        });
                        $("#message").ejDialog("setContent", text1);
                    }

                } catch (e) {


                }

            });


            frame.attr('src', file_src);
            frame.appendTo('#iframe_div_for_report');
        };


    }])
    .filter('safecontent', safecontent)
    .filter('dateFiltering', dateFiltering)
    .filter('dateFilter', dateFilter)

    .filter('myDate', myDate)
    .filter("sumByKey", sumByKey)
    .filter('find', find)
    .directive('panelBody', panelBody)
    .directive('datepicker', datepicker)
    .directive('togglable', togglable)
    .directive('showErrors', showErrors)
    .directive('compile', compile)
    .directive('archiveRow', archiveRow)
    .directive('nDecimals', nDecimals)
    .directive('onlyNumbers', onlyNumbers)
    .directive('confirmModal', confirmModal)
    .directive('confirmArchive', confirmArchive)
    .directive('loader', loader)
    .directive('tooltip', tooltip)
    .directive('input', inputFocus)
    .directive('textarea', inputFocus)
    .directive('select', inputFocus)
    .directive('input', CodeChecker)
    .directive('dateFormatter', dateFormatter)
    .directive('ngEnter', ngEnter)
    .directive('ngFileSelect', ngFileSelect)
    .directive('confirmArchiveGeneric', confirmArchiveGeneric)
    .directive('headerSearch', headerSearch)
    .directive("capitalize", capitalize)
    .factory('errorInterceptor', errorInterceptor)
    .factory('baseService', baseService)
    .factory('cboService', cboService)
    .factory('fileReader', fileReader)
    .factory('exportToExcel', exportToExcel)
    .filter("setDecimal", setDecimal)
    .factory("accountService", accountService)
    .factory('addressService', addressService)
    .factory('signalR', signalR)
    .constant('commonMessage', {
        appName: 'aPOP',
        appVersion: 2.0,
        primaryKeyNullMessage: 'Please select any Rows.',
        NetworkError: 'Error occur, please try again.'
    })
    ;