'use strict';
NewAttdnProcessLockController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$window', '$filter'];
function NewAttdnProcessLockController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $window, $filter) {
    $rootScope.title = 'Attendance Lock/UnLock';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'HumanResource/NewAttdnProcessLock/';


    $scope.ModelNew = {
        lockDate: null        
    };

    function Validation() {
        try {

            CheckField("Date", $scope.ModelNew.lockDate);

        } catch (ex) {
            throw ex;
        }
    }

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                ShowResult("" + fieldname + " can not be null...", 'failure');
                throw "" + fieldname + " can not be null...";
            }
        } catch (ex) {
            throw ex;
        }
    }


    $scope.UnlockedEmployees = [];
    $scope.LockedEmployees = [];

    $scope.GetEmpData = function () {
        
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'Date': $scope.ModelNew.lockDate },
            url: $scope.path + 'GetEmpData'

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {

                $scope.UnlockedEmployees = response.data.UnlockedEmp;
                $scope.LockedEmployees = response.data.LockedEmp;

                if (baseService.isUndefinedOrNull($scope.UnlockedEmployees)) {
                    $scope.UnlockedEmployeesCount = 0;
                } else {
                    $scope.UnlockedEmployeesCount = $scope.UnlockedEmployees.length;
                }

                if (baseService.isUndefinedOrNull($scope.LockedEmployees)) {
                    $scope.LockedEmployeesCount = 0;
                } else {
                    $scope.LockedEmployeesCount = $scope.LockedEmployees.length;
                }
            }

        }); 
    }

    $scope.actionCompleteSelected1 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridUnlockedEmployeesList").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width(); // Obtain the width of the container
                gridObj.clearFiltering();
                $("#GridUnlockedEmployeesList").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {

        }
    };


    $scope.actionCompleteSelected2 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridLockedEmployeesList").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width(); // Obtain the width of the container
                gridObj.clearFiltering();
                $("#GridLockedEmployeesList").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {

        }
    };

    // Tab Region

    $scope.tab = 1;

    $scope.setTab1 = function (newTab) {
        $scope.tab = newTab;
      
    };
    $scope.isSet1 = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTab2 = function (newTab) {
        $scope.tab = newTab;
        
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // Save Functions

    $scope.LockFunc = function () {
        Validation();
        if ($scope.UnlockedEmployeesCount > 0) {
            ShowResult("Please Lock Individual Employees before Locking Plant..", 'failure');
            throw "Please Lock Individual Employees before Locking Plant..";
        }
            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'Date': $scope.ModelNew.lockDate },
                url: $scope.path + 'LockAttdn'

            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                }

            });
        
    }

    $scope.UnLockFunc = function () {
        Validation();
            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'Date': $scope.ModelNew.lockDate },
                url: $scope.path + 'UnLockAttdn'

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