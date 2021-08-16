'use strict';
NewAttendanceProcessController.$inject = ['$window', '$timeout', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function NewAttendanceProcessController($window, $timeout, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'New Attendance Process';


    $scope.path = 'Attendances/NewAttendanceProcess/';


    $scope.Run = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.NewAttdnProcess.$valid) {

            $http({
                method: 'GET',
                url: $scope.path + 'RunShiftProcess?Date=' + $scope.Attnd.Date,
            }).then(function success(response) {
                console.log("Done!!");
            });
        }
    }

    $scope.Attnd = {
        Date: null,
    };

    $scope.RunAttnd = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.NewAttdnProcess.$valid) {

            $http({
                method: 'GET',
                url: $scope.path + 'RunAttnd?Date=' + $scope.Attnd.Date,
            }).then(function success(response) {
                console.log("Done!!");
            });
        }
    }

    $scope.RunDayStatus = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.NewAttdnProcess.$valid)
        {
            $http({
                method: 'GET',
                url: $scope.path + 'RunDayStatus?Date=' + $scope.Attnd.Date,
            }).then(function success(response) {
                console.log("Done!!");
            });
        }
    }

    $scope.RunManualScheduler = function () {
       
            $http({
                method: 'GET',
                url: $scope.path + 'ManualScheduler',
            }).then(function success(response) {
                console.log("Done!!");
            });
       
    }

    $scope.RunMonthlySummary = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.NewAttdnProcess.$valid) {
            $http({
                method: 'GET',
                url: $scope.path + 'MonthlySummary?Date=' + $scope.Attnd.Date,
            }).then(function success(response) {
                console.log("Done!!");
            });
        }
    }

    $scope.RunRoster = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.NewAttdnProcess.$valid) {

            $http({
                method: 'GET',
                url: $scope.path + 'RunRoster?Date=' + $scope.Attnd.Date,
            }).then(function success(response) {
                console.log("Done!!");
            });
        }
    }
 }