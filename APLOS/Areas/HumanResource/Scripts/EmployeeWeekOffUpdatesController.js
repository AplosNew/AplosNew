'use strict';
EmployeeWeekOffUpdatesController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeWeekOffUpdatesController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Week Off Updates';
    $rootScope.title1 = 'Week Off Updates';
    $scope.Action = 'Save';
    var url = "humanresource/WeekOffUpdates/";
    $scope.path = "humanresource/WeekOffUpdates/";


    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // ***** Code For the Fist Tab

    $scope.employee = null;
    $scope.EmpSystemId = null;
    $scope.EffectiveDates = new Date();
    $scope.EmpGridList = [];
    $http({
        method: 'GET',
        url: $scope.path + "getEmployees"
    }).then(function succ(resp) {
        $scope.EmployeesList = resp.data;
    });

    $scope.EmployeesList = [];
    $scope.selectEmployee = function () {
        angular.element(document.querySelector('#employeesModal')).modal('show');
    }

    $scope.doubleEmployee = function (e) {
        $scope.employee = e.data.EmployeeName;
        $scope.EmpSystemId = e.data.SystemId;
        $scope.EmpGridList = [];
        angular.element(document.querySelector('#employeesModal')).modal('hide');

        $http({
            method: 'POST',
            url: $scope.path + "getEmpWeekOff",
            data: {'EmpId':$scope.EmpSystemId}
        }).then(function succ(resp) {
            if (resp.data.length > 0) {
                $scope.WekName = resp.data[0].UserName;
                $scope.WekId = resp.data[0].WOHeaderId;
                $scope.EffectiveDates = resp.data[0].EffectiveDate;

                $scope.EmpGridList = resp.data;
            }
            else {
                $scope.WekName =null;
                $scope.WekId = null;
                $scope.EffectiveDates = null;
            }
            
        });

    }

    $scope.weekList = [];

    function getWeekOff() {
        $http({
            method: 'GET',
            url: $scope.path + "getWeekOff"
        }).then(function succ(resp) {
            $scope.weekList = resp.data;
        })
    }
    getWeekOff();

    $scope.WekId = null;

    $scope.saveSingle = function () {

        if (angular.isUndefinedOrNull($scope.WekId) || angular.isUndefinedOrNull($scope.EffectiveDates) || angular.isUndefinedOrNull($scope.EmpSystemId)) {
            ShowResult("All Selections are Mandatory!!", 'failure');
            throw ("Invalid Request");
        }

        $http({
            method: 'POST',
            url: url + 'SaveSingle',
            data: { 'EmpId': $scope.EmpSystemId, 'EffectiveDate': $scope.EffectiveDates, 'WeekId': $scope.WekId }
           
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                try {

                    $http({
                        method: 'POST',
                        url: $scope.path + "getEmpWeekOff",
                        data: { 'EmpId': $scope.EmpSystemId }
                    }).then(function succ(resp) {
                        if (resp.data.length > 0) {
                            $scope.WekName = resp.data[0].UserName;
                            $scope.WekId = resp.data[0].WOHeaderId;
                            $scope.EffectiveDates = resp.data[0].EffectiveDate;
                        }
                        else {
                            $scope.WekName = null;
                            $scope.WekId = null;
                            $scope.EffectiveDates = null;
                        }

                    });
                    ShowResult(response.data.Message, 'success')
                }
                catch (e) {

                    ShowResult(e, "failure");
                }
            }
        });
    }

    $scope.clearSingle = function () {
        $scope.employee = null;
        $scope.EmpSystemId = null;
        $scope.EffectiveDates = new Date();
        $scope.WekId = null;
    }

}