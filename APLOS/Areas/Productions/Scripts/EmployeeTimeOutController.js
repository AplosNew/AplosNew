'use strict';
EmployeeTimeOutController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function EmployeeTimeOutController($window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $scope.title = "Employee Time Out";
    $scope.path = "Productions/EmployeeTimeOut/";

    // Variables
    $scope.employee = null;
    $scope.EmpSystemId = null;
    $scope.EffectiveDate = new Date();
    $scope.ToTime = null;
    $scope.FromTime = null;


    //Arrays
    $scope.EmpGridList = [];
    $scope.EmployeesList = [];

    $http({
        method: 'GET',
        url: $scope.path + "getEmployees"
    }).then(function succ(resp) {
        $scope.EmployeesList = resp.data;
    });

   
    $scope.selectEmployee = function () {
        angular.element(document.querySelector('#employeesModal')).modal('show');
    }

    $scope.doubleEmployee = function (e) {
        $scope.employee = e.data.EmployeeName;
        $scope.EmpSystemId = e.data.SystemId;
        $scope.EmpGridList = [];
        angular.element(document.querySelector('#employeesModal')).modal('hide');
        getTimeOuts();
       

    }

    function getTimeOuts() {
        $http({
            method: 'POST',
            url: $scope.path + "getEmpTimeOut",
            data: { 'EmpId': $scope.EmpSystemId, 'Date': $scope.EffectiveDate }
        }).then(function succ(resp) {
            if (resp.data.length > 0) {
                $scope.EmpGridList = resp.data;
                var gridObj = $("#getEmployeesTime").data("ejGrid");
                gridObj.dataSource($scope.EmpGridList);
            }
            

        });
    }

    //Saving of the Data
    $scope.save = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'Create',
            data: { 'EmployeeId': $scope.EmpSystemId, 'Date': $scope.EffectiveDate , 'FromTime' : $scope.FromTime ,'ToTime' : $scope.ToTime  },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                getTimeOuts();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }

}