'use strict';
NewAttendanceProcessController.$inject = ['$window', '$timeout', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function NewAttendanceProcessController($window, $timeout, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'New Attendance Process';


    $scope.path = 'Attendances/NewAttendanceProcess/';


    $scope.RunShiftProcess = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.NewAttdnProcess.$valid) {
            $http({
                method: 'GET',
                url: $scope.path + 'RunShiftProcess?Date=' + $scope.Attnd.Date,
            }).then(function successCallback(response) {
                if (response.data.Error == false) {                    
                    ShowResult(response.data.Message, 'success');
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
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
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                } else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.RunTBS_LA_Process  = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.NewAttdnProcess.$valid) {

            $http({
                method: 'GET',
                url: $scope.path + 'RunTBS_LA_Process?Date=' + $scope.Attnd.Date,
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                } else {
                    ShowResult(response.data.Message, 'failure');
                }
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
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                } else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.RunDOJProcess = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.NewAttdnProcess.$valid) {
            $http({
                method: 'GET',
                url: $scope.path + 'RunDOJProcess?Date=' + $scope.Attnd.Date,
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');

                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
            });
        }
    }

    $scope.RunManualScheduler = function () {
       
            $http({
                method: 'GET',
                url: $scope.path + 'ManualScheduler',
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                } else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
       
    }   

    $scope.RunRoster = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.NewAttdnProcess.$valid) {

            $http({
                method: 'GET',
                url: $scope.path + 'RunRoster?Date=' + $scope.Attnd.Date,
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                } else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.Run_LeaveProcess = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.NewAttdnProcess.$valid) {

            $http({
                method: 'GET',
                url: $scope.path + 'Run_LeaveProcess?Date=' + $scope.Attnd.Date,
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                } else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }
 }