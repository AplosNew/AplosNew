'use strict';
ESICStatementsCompanyController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ESICStatementsCompanyController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.path = 'Payrolls/ESICStatementsCompany/';
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

    $scope.month = null;
    $scope.year = null;
    $scope.isCompletedMonth = null;
    $scope.salaryProcessId = null;

    $scope.isActive = true;
    $scope.isSeperated = false;
    $scope.isMaternity = false;
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

        var DropDownListYear = $("#ddlYearList").data("ejDropDownList");
        DropDownListYear.selectItemByText($scope.year);

    };
    $scope.GetESICReports = function (isPFEligible) {
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
                    url: $scope.path + 'GetESICReports',
                    data: {
                        'month': $scope.month,
                        'year': $scope.year,
                        'isActive': $scope.isActive,
                        'isSeperated': $scope.isSeperated,
                        'isMaternity': $scope.isMaternity   
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

}