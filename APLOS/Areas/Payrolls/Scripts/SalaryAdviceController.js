'use strict';
SalaryAdviceController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$window'];
function SalaryAdviceController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $window) {
    $scope.FormTitle ="Salary Disbursment Report"
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
    $scope.month = null;
    $scope.PaymentMode = null;
    $scope.EmpCat = null;
    $scope.DisbursmentId = null;



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

    $scope.EmpCatList = [];
    $scope.getEmpCategory = function () {
        $http({
            method: 'GET',
            url: 'Leave/AnnualLeaveProcess/GetEmpCategory'
        }).then(function success(response) {
            $scope.EmpCatList = response.data;
        })
    }
    $scope.getEmpCategory();


    $scope.disbursementAdviceList = [];
    $scope.GetDisbursementAdviceCbo = function () {
        $http({
            method: 'GET',
            url: 'Payrolls/PaySlipsNew/GetDisbursementAdviceCbo?yearNo=' + $scope.year + '&monthNo=' + $scope.month + '&paymentMode=' + $scope.PaymentMode
        }).then(function success(response) {
            $scope.disbursementAdviceList = response.data;
        })
    }

    $scope.PrintData = function () {
        try {
            $scope.fileName = "BankAdvice.xls";

            $scope.EmployeeCategory = $("#EmployeeCategory option:selected").text();
            $scope.monthName = $("#Month option:selected").text();

              $scope.ReportFormat = 'Excel';
           // $scope.ReportFormat = 'Pdf';
            var url = 'Payrolls/PaySlipsNew/GetSalaryAdviseReportPdf?reportFormat=' + $scope.ReportFormat + '&empcat=' + $scope.EmployeeCategory + '&adviceId=' + $scope.DisbursmentId + '&yearNo=' + $scope.year + '&monthNo=' + $scope.month + '&monthName=' + $scope.monthName;
            $rootScope.report(url);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



}



