'use strict';
SalaryAdviceController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$window'];
function SalaryAdviceController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $window) {
    $scope.FormTitle = "Salary Disbursment Report"
    $scope.path = 'humanresource/payrollReports/';
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';
    $scope.paymentMode = null;
    $scope.sheetType = false;
    $scope.cboSalaryProcessIdList = [];
    $scope.month = "";
    $scope.year = "";
    $scope.isCompletedMonth = null;
    $scope.IncludingZeroHeads = false;
    $scope.salaryProcessId = null;
    $scope.languageId = null;
    $scope.empGrid = false;


    $scope.PaymentMode = null;
    $scope.year = null;
    $scope.FromDate = null;
    $scope.ToDate = null;
    $scope.month = null;
    $scope.PaymentMode = null;
    $scope.EmpCat = null;
    $scope.DisbursmentId = null;
    $scope.ReportType = 'Salary';


    $scope.paymentModeList = [];
    $http({
        method: 'GET',
        url: 'Payrolls/PaySlipsNew/GetPaymentModeCbo'
    }).then(function successCallback(response) {
        $scope.paymentModeList = response.data;
    });


    $scope.monthList = [
        {
            Value: 1,
            Text: 'January'
        },
        {
            Value: 2,
            Text: 'February'
        },
        {
            Value: 3,
            Text: 'March'
        },
        {
            Value: 4,
            Text: 'April'
        },
        {
            Value: 5,
            Text: 'May'
        },
        {
            Value: 6,
            Text: 'June'
        },
        {
            Value: 7,
            Text: 'July'
        },
        {
            Value: 8,
            Text: 'August'
        },
        {
            Value: 9,
            Text: 'September'
        },
        {
            Value: 10,
            Text: 'October'
        },
        {
            Value: 11,
            Text: 'November'
        },
        {
            Value: 12,
            Text: 'December'
        }
    ];

    $scope.YearNoList = [];
    $http({
        method: 'GET',
        url: 'Payrolls/PaySlipsNew/GetYearNoCbo'
    }).then(function successCallback(response) {
        $scope.YearNoList = response.data;
    });


    $scope.EmpCatList = [
        { Value: "2", Text: "Pending" },
        { Value: "1", Text: "Parked" },
        { Value: "0", Text: "Posted" }
    ];


    $scope.disbursementAdviceList = [];
    $scope.GetDisbursementAdviceCbo = function () {
        try {
            if ($scope.ReportType == "Salary" || $scope.ReportType == "Bonus") {
                $http({
                    method: 'GET',
                    url: 'Payrolls/PaySlipsNew/GetDisbursementAdviceCbo?yearNo=' + $scope.year + '&monthNo=' + $scope.month + '&paymentMode=' + $scope.PaymentMode + '&ReportType=' + $scope.ReportType + '&status=' + $scope.EmpCat
                }).then(function success(response) {
                    $scope.disbursementAdviceList = response.data;
                })
            } else {
                if (baseService.isUndefinedOrNull($scope.FromDate)) {
                    throw "Select From Date.";
                }
                if (baseService.isUndefinedOrNull($scope.ToDate)) {
                    throw "Select To Date.";
                }

                $http({
                    method: 'GET',
                    url: 'Payrolls/PaySlipsNew/GetGEOTDisbursementAdviceCbo?FromDate=' + $scope.FromDate + '&ToDate=' + $scope.ToDate + '&paymentMode=' + $scope.PaymentMode + '&ReportType=' + $scope.ReportType
                }).then(function success(response) {
                    $scope.disbursementAdviceList = response.data;
                })
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.PrintData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ReportType)) {
                throw "Please select Report Type.";
            }
            if ($scope.ReportType == "Salary" || $scope.ReportType == "Bonus") {
                if (baseService.isUndefinedOrNull($scope.year)) {
                    throw "Please select Year.";
                }
            }
            if ($scope.ReportType == "Salary" || $scope.ReportType == "Bonus") {
                if (baseService.isUndefinedOrNull($scope.month)) {
                    throw "Please select Month.";
                }
            }

            if ($scope.ReportType == "GoodWork" || $scope.ReportType == "ExtraOT") {
                if (baseService.isUndefinedOrNull($scope.FromDate)) {
                    throw "Please select FromDate.";
                }
            }
            if ($scope.ReportType == "GoodWork" || $scope.ReportType == "ExtraOT") {
                if (baseService.isUndefinedOrNull($scope.ToDate)) {
                    throw "Please select ToDate.";
                }
            }

            if (baseService.isUndefinedOrNull($scope.PaymentMode)) {
                throw "Please select Disbursment Type.";
            }
            if (baseService.isUndefinedOrNull($scope.EmpCat)) {
                throw "Please select Status.";
            }
            if (baseService.isUndefinedOrNull($scope.DisbursmentId)) {
                throw "Please select Disbursment Id.";
            }

            $scope.fileName = "BankAdvice.xls";

            $scope.EmployeeCategory = $("#EmployeeCategory option:selected").text();
            $scope.monthName = $("#Month option:selected").text();

            $scope.ReportFormat = 'Excel';
            // $scope.ReportFormat = 'Pdf';
            var url = null;
            if ($scope.ReportType == "Salary" || $scope.ReportType == "Bonus") {
                url = 'Payrolls/PaySlipsNew/GetSalaryAdviseReportPdf?reportFormat=' + $scope.ReportFormat + '&empcat=' + $scope.EmployeeCategory + '&adviceId=' + $scope.DisbursmentId + '&yearNo=' + $scope.year + '&monthNo=' + $scope.month + '&monthName=' + $scope.monthName + '&status=' + $scope.EmpCat + '&ReportType=' + $scope.ReportType;
            } else {
                url = 'Payrolls/PaySlipsNew/GetGWOTAdviseReportPdf?reportFormat=' + $scope.ReportFormat + '&empcat=' + $scope.EmployeeCategory + '&adviceId=' + $scope.DisbursmentId + '&status=' + $scope.EmpCat + '&ReportType=' + $scope.ReportType + '&FromDate=' + $scope.FromDate + '&ToDate=' + $scope.ToDate;
            }
            $rootScope.report(url);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



}



