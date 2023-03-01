/// <reference path="../angular-constant-path.js" />
'use strict';
var tpanelApp = angular.module('tpanelApp', ['ngRoute', 'ngCookies', 'angularUtils.directives.dirPagination', 'toaster', 'ui.calendar', 'ui.bootstrap', "ejangular"])
    
    .controller("DailyAttendanceStatusReportController", DailyAttendanceStatusReportController)
   
    //#endregion

    .config(['$routeProvider', '$locationProvider', '$httpProvider', function ($routeProvider, $locationProvider, $httpProvider) {
        $routeProvider
            .when('/', {
                templateUrl: 'DailyAttdStatus/dashboard'
            })
            .when('tpanel', {
                templateUrl: 'DailyAttdStatus/dashboard'
            })
            .when('/dashboard', {
                templateUrl: 'DailyAttdStatus/dashboard'
            })
           
            .when('/login', {
                templateUrl: 'DailyAttdStatus/login',
                controller: 'portalLoginController'
            })
            .when('/password-change/:id', {
                templateUrl: 'DailyAttdStatus/passwordchange',
                controller: 'teacherPasswordChangeController'
            })
            .when('/employee-calendar', {
                templateUrl: 'DailyAttdStatus/Calendar',
                controller: 'myTeacherCalendarController'
            })
            .when('/task-list', {
                templateUrl: 'TaskManagement/TaskList/',
                controller: 'taskListController'
            })
            //.when('/issue-transaction', {
            //    templateUrl: 'IssueTracker/IssueTransaction/Aplos',
            //    controller: "issueTransactionController"
            //})



            //#region Requisition

            .when('/requisition', {
                templateUrl: 'Products/Requisition/Aplos',
                controller: 'RequisitionController'
            })

            .when('/requisition-approvedby', {
                templateUrl: 'Products/InventoryCheckApproved/ReqAuthorized',
                controller: 'InventoryCheckApprovedController'
            })
            .when('/requisition-checkby', {
                templateUrl: 'Products/InventoryCheckApproved/Aplos',
                controller: 'InventoryCheckApprovedController'
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
                controller: 'IssueSlipController'
            })
            .when('/approving-issue-slip', {
                templateUrl: 'Products/GoodsReceiveNote/ApprovingIssueSlip',
                controller: 'IssueSlipController'
            })
            .when('/Material-Wise-issue-slip', {
                templateUrl: 'Products/GoodsReceiveNote/MaterialIssueSlip',
                controller: 'MaterialIssueSlipController'
            })
            .when('/asset-issue-slip', {
                templateUrl: 'Products/GoodsReceiveNote/AssetIssueSlip',
                controller: 'MaterialIssueSlipController'
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
                controller: 'ServicePOCheckAndApprovedController'
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
                controller: 'ServicePOCheckAndApprovedController'
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

            .when('/daily-attendance-status-report', {
                templateUrl: 'humanResource/DailyAttendanceStatusReport/Aplos',
                controller: 'DailyAttendanceStatusReportController'
            })
           //#endregion

            .when('/logout', {
                template: ' ',
                controller: 'tpanelLogoutController'
            })
            //.when('/task-create', {
            //    templateUrl: 'taskmanagement/TaskMaster/aplos',
            //    controller: 'taskMasterController'
            //})
            .otherwise({
                redirectTo: 'DailyAttdStatus/login'
            });
    }])
    .run(['$rootScope', '$timeout', '$cookies', '$window', "$filter", "$http", function ($rootScope, $timeout, $cookies, $window, $filter, $http) {
        $rootScope.title = 'DailyAttdStatus';
        $rootScope.bootPoint = '#!/';

        $window.employeeId = $cookies.get("MyTeacheremployeeId");
        $window.employeeName = $cookies.get("MyTeacheremployeeName");
        $window.companyGroupId = $cookies.get("MyTeachergroupId");
        $window.companyId = $cookies.get("MyTeachercompanyId");
        $window.plantId = $cookies.get("MyTeacherplantId");
        $rootScope.plantName = $cookies.get("MyTeacherplantName");
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