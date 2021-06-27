'use strict';
monthlyAttendanceInformationReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function monthlyAttendanceInformationReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.path = 'humanresource/MonthlyAttendanceInformationReport/';
    $scope.employeeCategoryId = null;
    $scope.dailyComplianceReport = {
        WorkDate: null
    };
    $scope.paymentDate = null;
    $scope.languageId = null;
    $scope.paymentMode = null;

    

    $scope.month = null;
    $scope.year = null;
    $scope.isCompletedMonth = null;
    $scope.salaryProcessId = null;

    $scope.unitId = null;
    $scope.departmentId = null;
    $scope.divisionId = null;
    $scope.sectionId = null;
    $scope.subSenctionId = null;
    $scope.payGroupId = null;

   
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

    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });


    $scope.selectedPaymentMode = $("#paymentMode option:selected").text();
    $scope.selectedEmployeeCategory = $("#employeeCategoryId option:selected").text();


    //$scope.GetSalaryTopRegistrar = function () {
    //    try {
    //        if (baseService.isUndefinedOrNull($scope.month)) {
    //            throw "Select Month.";
    //        }
    //        if (baseService.isUndefinedOrNull($scope.year)) {
    //            throw "Select Year.";
    //        }
            
    //        //$scope.parameters = 'month=' + $scope.month + '&year=' + $scope.year + '&salaryProcessId=' + $scope.salaryProcessId + '&divisionId=' + $scope.divisionId + '&unitId=' + $scope.unitId + '&sectionId=' + $scope.sectionId + '&subSectionId=' + $scope.subSectionId + '&departmentId=' + $scope.departmentId + '&payGroupId=' + $scope.payGroupId + '&employeeCategoryId=' + $scope.employeeCategoryId + '&paymentDate=' + $scope.paymentDate + '&paymentMode=' + $scope.paymentMode + '&languageId=' + $scope.languageId + '&SalaryTopSheetCategory=' + $scope.SalaryTopSheetCategory ;
    //        //location.href = 'humanresource/SalaryTopSheet/GetSalaryTopSheet?' + $scope.parameters;
    //        $scope.parameters = 'month=' + $scope.month + '&year=' + $scope.year + '&salaryProcessId=' + $scope.salaryProcessId;
    //        location.href = 'humanresource/SalaryTopSheet/XlsSalarySummary?' + $scope.parameters;
    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }
    //};
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';

    $scope.GetSalaryTopRegistrar = function (reportType) {

        try {
            if (baseService.isUndefinedOrNull($scope.month)) {
                throw "Select Month.";
            }
            if (baseService.isUndefinedOrNull($scope.year)) {
                throw "Select Year.";
            }
            $http({
                method: 'POST',
                url: 'humanresource/SalaryTopSheet/XlsSalarySummary',
                data: {
                    'month': $scope.month,
                    'year': $scope.year,
                    'salaryProcessId': $scope.salaryProcessId,
                    'paymentDate': $scope.PaymentDate                    
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                   
                    $rootScope.report($scope.downloadgriddataPDFUrl + "?FileName=" + response.data.FileName);
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });

   
}