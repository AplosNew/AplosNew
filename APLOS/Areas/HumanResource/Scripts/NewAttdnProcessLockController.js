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


    $scope.GetEmpData = function () {
       
            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'Date': $scope.ModelNew.lockDate },
                url: $scope.path + 'GetEmpData'

            }).then(function successCallback(response) {
                $scope.UnApprovedEmployees = response.data.UnlockedEmp;
                $scope.allShift = response.data.shift;

                var gridObj = $("#GridChangeAttendance").data("ejGrid");
                gridObj.refreshContent();
            });

    }
   
}