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

    // Tab Attendance Process Code

    $scope.EmployeeList = [];

    $scope.EmployeePopUp = function () {
        if ($scope.selectedValues.FromDate != null) {

            angular.element(document.querySelector("#EmployeePop")).modal("show");
            //$scope.getEmpDetailsData();
        }
        else {
            ShowResult("Please Select Effective Date", 'failure');
        }
    }
    $scope.getEmpDetailsData = function () {

        $http({
            method: 'POST',
            data: { EffectiveDate: $scope.selectedValues.FromDate },
            url: $scope.path + 'getDistinctEmployeesToBeProcessed'
        }).then(function successCallback(response) {
            $scope.EmployeeList = response.data;

        });
    }

    $scope.closeEmpPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    $scope.EmpSelectedData = [];
    $scope.SelectEmPDetails = function () {
        $scope.EmpSelectedData = [];
        for (var j = 0; j < $scope.EmployeeList.length; j++) {
            if ($scope.EmployeeList[j].isSelected == true) {

                $scope.EmpSelectedData.push($scope.EmployeeList[j]);
                $scope.EmployeeList[j].isSelected = true;
            }
            else {
                $scope.EmployeeList[j].isSelected = false;
            }
        }
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }

   

    $scope.ProcessAttendance = function () {
        if ($scope.selectedValues.FromDate != null && $scope.EmpSelectedData != null) {
            var EmpString = "''";

            for (var j = 0; j < $scope.EmpSelectedData.length; j++) {
             
                EmpString+= ",'" + $scope.EmpSelectedData[j].EmpSystemId + "'";

            }
            $http({
                method: 'POST',
                data: { EffectiveDate: $scope.selectedValues.FromDate, EmpData: EmpString },
                url: $scope.path + 'ProcessAttendance'
            }).then(function successCallback(response) {

                ShowResult("Saved Successfully ...", 'success');
            });
        }
        else {
            ShowResult("Please Select Prerequisite Data", 'failure');
        }
    }


    $scope.selectedValues = {
        FromDate: null
    };



}