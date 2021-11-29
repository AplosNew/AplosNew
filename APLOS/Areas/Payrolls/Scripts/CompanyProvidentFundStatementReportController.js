'use strict';
CompanyProvidentFundStatementReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CompanyProvidentFundStatementReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.path = 'Payrolls/CompanyProvidentFundStatementReport/';
    $rootScope.title = 'Company Provident FundStatement Report';
    $scope.employeeCategoryId = null;
    $scope.dailyComplianceReport = {
        WorkDate: null
    };
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.empBankId = null;
    $scope.letterDate = null;
    $scope.languageId = null;
    $scope.chequeNo = null;
    $scope.isActive = true;
    $scope.isSeperated = true;

    $scope.SalaryTopSheetCategory = 'PayrollGroup';

    $scope.year = new Date().getFullYear().toString();
    $scope.month = new Date().getMonth().toString();

    $scope.salaryProcessId = null;

    $scope.getSalaryProcessIdList = function (args) {
        $scope.isCompletedMonth = 1;

        var DropDownListMonth = $("#ddlMonthList").data("ejDropDownList");
        var DropDownListYear = $("#ddlYearList").data("ejDropDownList");


        $scope.month = DropDownListMonth.getSelectedValue();
        $scope.year = DropDownListYear.getSelectedValue();
        if (angular.isUndefinedOrNull($scope.year)) {
            ShowResult("Select Year", 'failure');
        }
        else {
            cboService.getSalaryProcessIdCboByYearMonth($scope.month, $scope.year, $scope.isCompletedMonth, function (result) {
                $scope.cboSalaryProcessIdList = result;
            });
        }


    };

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

    $scope.empBankList = [];
    cboService.getEmployeeBankCbo(function (result) {
        $scope.empBankList = result;
    });
    $scope.year = new Date().getFullYear().toString();
    $scope.month = new Date().getMonth().toString();

    $scope.SelectDefaultValue = function (args) {
        var x = new Date();
        x.setDate(10);
        x.setMonth(x.getMonth() - 1);

        for (var i = 0; i < $scope.yearList.length; i++) {
            if ($scope.yearList[i].Text === x.getFullYear().toString()) {
                $scope.year = $scope.yearList[i].Text;
                $scope.month = (x.getMonth() + 1).toString();
                continue;
            }
        }

        //$scope.year = "2018";
        var DropDownListYear = $("#ddlYearList").data("ejDropDownList");
        DropDownListYear.selectItemByText($scope.year);

    };

    $scope.GetProvidentFundStatement = function (isPFEligible) {
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
                    url: $scope.path + 'GetProvidentFundStatement',
                    data: {
                        'month': $scope.month,
                        'year': $scope.year,
                        'isActive': $scope.isActive,
                        'isSeperated': $scope.isSeperated,
                        'isPFEligible': isPFEligible
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
    $scope.GetPFcsv = function (isPFEligible) {
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

                $scope.parameters = 'month=' + $scope.month + '&year=' + $scope.year + '&isActive=' + $scope.isActive + '&isSeperated=' +$scope.isSeperated;

                location.href = $scope.path + 'GetPFcsv?' + $scope.parameters;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

}