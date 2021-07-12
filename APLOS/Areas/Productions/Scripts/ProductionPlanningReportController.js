
'use strict';
ProductionPlanningReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ProductionPlanningReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Production Planning Report";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'Productions/ProductionPlanningReport/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
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
    $scope.year = new Date().getFullYear().toString();
    $scope.month = new Date().getMonth().toString();


    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });

    $scope.SelectDefaultValue = function (args) {
        var x = new Date();
        x.setDate(10);
        x.setMonth(x.getMonth());

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




    $scope.ProductionPlanningReport = function () {

        try {

            var monthName = $scope.monthList.filter(function (month) {
                return month.Value == $scope.month;
            });
            $scope.fromDate = 1 + '-' + monthName[0].Text + '-' + $scope.year;
            $scope.toDate = daysInMonth($scope.month, $scope.year) + '-' + monthName[0].Text + '-' + $scope.year;

            if (angular.isUndefinedOrNull($scope.year))
                throw 'Plase select year';

            if (angular.isUndefinedOrNull($scope.month))
                throw 'Plase select month';

            var file_src = $scope.path + 'GetProductionPlanningReport?fromDate=' + $scope.fromDate + '&toDate=' + $scope.toDate;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }


}