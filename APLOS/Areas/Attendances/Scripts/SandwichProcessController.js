'use strict';
SandwichProcessController.$inject = ['$window','cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SandwichProcessController($window,cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Sandwich Process';
    $scope.path = 'Attendances/SandwichProcess/';
   
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

    $scope.EmpList = [];

    $scope.GetEmployeeInformation = function () {
        if ($scope.SandwichForm.$valid) {

            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'month': $scope.month, 'year': $scope.year },
                url: $scope.path + 'GetEmployeeInformation'

            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');

                }
                else
                {
                    $scope.EmpList = [];
                    $scope.EmpList = response.data;
                }
            });
        }
        else {
            ShowResult("Choose Required Fields");
            throw "Choose Required Fields";
        }
    }

    $scope.RunProcess = function () {
        if ($scope.SandwichForm.$valid) {

            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'month': $scope.month, 'year': $scope.year },
                url: $scope.path + 'RunProcess'

            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');

                }
                else {
                    ShowResult(response.data.Message, 'success');
                }

            });
        }
        else {
            ShowResult("Choose Required Fields");
            throw "Choose Required Fields";
        }
    }

}