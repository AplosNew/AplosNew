'use strict';
ELReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function ELReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Earn Leave Report';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.maternityLeaveTransactions = [];
    $scope.path = 'Payrolls/Encashment/';
    $scope.year = null;
    $scope.yearlist = [];
    $scope.isActive = true;
    $scope.isSeperated = true;
    $scope.year = new Date().getFullYear().toString();

    $scope.GetCbo = function () {
        $http.get('Attendances/AttendanceProcessUI/GetCbo')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.yearlist = [];
                        $scope.yearlist = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetCbo();
    $scope.isDetail = true;
    $scope.GetEncashReport = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.year)) {
                manualValidation('div_FromDate', true, "From Date is required.");
                ShowResult("Year No is required.", 'failure');
            }
            else {
                var url = 'Payrolls/Encashment/GetEncashReport?reportFormat=Excel' + ' &YearNo=' + $scope.year ;
                $rootScope.report(url);
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.isDetail = true;

    $scope.GetEarnLeaveReport = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.year)) {
                //manualValidation('div_FromDate', true, "From Date is required.");
                ShowResult("Year is required.", 'failure');
            }
            else {
                var url = 'Payrolls/Encashment/GetEarnLeaveReport?reportFormat=Excel' + ' &YearNo=' + $scope.year + ' &isDetail=true &isActive=' + $scope.isActive + ' &isSeperated=' + $scope.isSeperated;
                $rootScope.report(url);
            }
        } catch (e) {
            ShowResult(e, 'failure');

        }
    };
    $scope.GetEarnLeaveReportSummary = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.year)) {
                //manualValidation('div_FromDate', true, "From Date is required.");
                ShowResult("Year No is required.", 'failure');
            }
            else {
                var url = 'Payrolls/Encashment/GetEarnLeaveReport?reportFormat=Excel' + ' &YearNo=' + $scope.year + ' &isDetail=false &isActive=' + $scope.isActive + ' &isSeperated=' + $scope.isSeperated;
                $rootScope.report(url);
            }
            //location.href = 


        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.SelectDefaultValue = function (args) {
        var x = new Date();
        x.setDate(10);
        x.setMonth(x.getMonth() - 1);

        for (var i = 0; i < $scope.yearlist.length; i++) {
            if ($scope.yearlist[i].YearNo === x.getFullYear().toString()) {
                $scope.year = $scope.yearList[i].Id;
                $scope.month = (x.getMonth() + 1).toString();
                continue;
            }
        }

        //$scope.year = "2018";
        var DropDownListYear = $("#ddlYearList").data("ejDropDownList");
        DropDownListYear.selectItemByText($scope.year);

    };

}