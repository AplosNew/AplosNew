"use strict";
PaymentAdviseReportController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter","cboService"];
function PaymentAdviseReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "PaymentAdviseReport";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.SalaryDisbursementes = [];
    $scope.path = "accounts/SalaryDisbursement/";

    $scope.monthList = [
        { Value: 1, Text: 'January' },
        { Value: 2, Text: 'February' },
        { Value: 3, Text: 'March' },
        { Value: 4, Text: 'April' },
        { Value: 5, Text: 'May' },
        { Value: 6, Text: 'June' },
        { Value: 7, Text: 'July' },
        { Value: 8, Text: 'August' },
        { Value: 9, Text: 'September' },
        { Value: 10, Text: 'October' },
        { Value: 11, Text: 'November' },
        { Value: 12, Text: 'December' }
    ];
    $scope.year = new Date().getFullYear().toString();
    $scope.month = new Date().getMonth().toString();


    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });

    $scope.employeeCategoryList = [];
    cboService.getCboEmployeeCategoryGroupByCompanyGroup(null, function (result) {
        $scope.employeeCategoryList = result;
    });

    function daysInMonth(month, year) {
        return new Date(year, month, 0).getDate();
    }


    $scope.EmployeeListTemp = [];
    $scope.GetEmployeeInformation = function () {
        $scope.isActive = true;
        $scope.isSeperated = false;
        $scope.isMaternity = false;
        var monthName = $scope.monthList.filter(function (mnth) {
            return mnth.Value == $scope.month;
        });
        $scope.effectiveDate = daysInMonth($scope.month, $scope.year) + '-' + monthName[0].Text + '-' + $scope.year;

        if (angular.isUndefinedOrNull($scope.month)) {
            ShowResult("Select Month", 'failure');
        }
        if (angular.isUndefinedOrNull($scope.year)) {
            ShowResult("Select Year", 'failure');
        }
        else {

            var parameters = {
                'effectiveDate': $scope.effectiveDate, 'salaryProcessId': $scope.salaryProcessId, 'payRollGroup': $scope.payGroupListSelected, 'isActive': $scope.isActive,
                'isSeperated': $scope.isSeperated,
                'isMaternity': $scope.isMaternity
            };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Accounts/SalaryDisbursement/GetEmployeeInformation',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.empdata.length > 0) {
                    for (var i = 0; i < response.data.empdata.length; i++) {
                        for (var j = 0; j < response.data.empNetPay.length; j++) {
                            if (response.data.empdata[i].EmpSystemId == response.data.empNetPay[j].EmpInfoSystemID) {
                                response.data.empdata[i].NetPayment = response.data.empNetPay[j].NetPayment;

                            }
                        }

                    }
                    $scope.empGrid = true;
                    $scope.EmployeeListDefault = response.data.empdata.filter(d => d.isSelect == true);
                    $scope.EmployeeList = $scope.EmployeeListDefault;
                    $scope.EmployeeListTemp = $scope.EmployeeListDefault;

                    
                    $scope.EmployeeListTemp = response.data.empdata

                }
                else {
                    ShowResult("No Data Found", 'failure');
                    $scope.empGrid = false;
                }
                var gridObj = $("#empInfoGrid").data("ejGrid");
                gridObj.windowonresize();
                gridObj.refreshContent(true);
            });
        }
    };



}