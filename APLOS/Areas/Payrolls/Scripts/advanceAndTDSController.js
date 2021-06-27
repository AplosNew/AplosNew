'use strict';
advanceAndTDSController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function advanceAndTDSController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.path = 'Payrolls/AdvanceAndTDS/';
    $scope.employeeCategoryId = null;
    $scope.dailyComplianceReport = {
        WorkDate: null
    };
    $scope.paymentDate = null;
    $scope.languageId = null;
    $scope.paymentMode = null;
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.SalaryTopSheetCategory = 'PayrollGroup';

    $scope.year = new Date().getFullYear().toString();
    $scope.month = new Date().getMonth().toString();
    $scope.toDate = null;
    $scope.fromDate = null;
    $scope.month = null;
    $scope.year = null;   
    $scope.isActive = false;

    $scope.toDate = $filter('dateFiltering')(Date.now()),
        $scope.fromDate =  $filter('dateFiltering')(Date.now()),
   
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
  
    $scope.GetAdvanceAndTDSports = function () {
        try {
            var DropDownListMonth = $("#ddlMonthList").data("ejDropDownList");
            var DropDownListYear = $("#ddlYearList").data("ejDropDownList");

            $scope.month = DropDownListMonth.getSelectedValue();
            $scope.year = DropDownListYear.getSelectedValue();
            if (angular.isUndefinedOrNull($scope.year)) {
                ShowResult("Select Year", 'failure');
            }
            if (angular.isUndefinedOrNull($scope.month)) {
                ShowResult("Select Month", 'failure');
            }
            else {

                $http({
                    method: 'POST',
                    url: $scope.path + 'GetAdvanceAndTDSReports',
                    data: {
                        'month': $scope.month,
                        'year': $scope.year,
                        'isActive': $scope.isActive,
                        'isSummary': false
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                    }
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }; 


    $scope.GetAdvanceAndTDSportsSummary = function () {
        try {           
            if (new Date($scope.toDate) < new Date($scope.fromDate)) {
                throw ShowResult("From date can not be greater then to date", 'failure');
            }
            else {

                $http({
                    method: 'POST',
                    url: $scope.path + 'GetAdvanceAndTDSReportsSummary',
                    data: {
                        'fromDate': $scope.fromDate,
                        'toDate': $scope.toDate,
                        'isSummary': $scope.isActive
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                    }
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }; 

    // #region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion

}