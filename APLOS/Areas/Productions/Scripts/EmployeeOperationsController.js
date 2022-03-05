'use strict';
EmployeeOperationsController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeOperationsController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Operations';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/EmployeeOperations/';

    

    //Variables
    $scope.workCenterId = null;
    $scope.processId = null;
    $scope.shiftId = null;
    $scope.POId = null;
    $scope.Date = null;
    $scope.periodId = null;
    $scope.change = null;

    //Arrays
    $scope.workCenterList = [];
    $scope.ProcessList = [];
    $scope.ShiftList = [];
    $scope.POList = [];
    $scope.ModelList = [];


    //Get Operations
    $scope.getStartUp = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetWorkCenter',
        }).then(function succ(resp) {
            $scope.workCenterList = resp.data;
        });

        $http({
            method: 'GET',
            url: $scope.path + 'GetProcess',
        }).then(function succ(resp) {
            $scope.ProcessList = resp.data;
        });

        $http({
            method: 'GET',
            url: $scope.path + 'GetShift',
        }).then(function succ(resp) {
            $scope.ShiftList = resp.data;
        });

        
    }

    $scope.getStartUp();

    // Getting the POs
    $scope.getPo = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetPOs',
            data: { 'wk': $scope.workCenterId},
        }).then(function succ(resp) {
            $scope.POList = resp.data;
        });
    }

    ///// Setting of the Period Buttons
    //Period Buttons
    var p1 = document.getElementById("pp1");
    var p2 = document.getElementById("pp2");
    var p3 = document.getElementById("pp3");
    var p4 = document.getElementById("pp4");
    var p5 = document.getElementById("pp5");
    var p6 = document.getElementById("pp6");

  

    $scope.PeriodValidation = function (e) {
        document.getElementById("pp1").disable = true;
        document.getElementById("pp2").disable = true;
        document.getElementById("pp3").disable = true;
        document.getElementById("pp4").disable = true;
        document.getElementById("pp5").disable = true;
        document.getElementById("pp6").disable = true;
        document.getElementById('p'+$scope.periodId).disabled = false;
    }

    $scope.refreshPage = function (e) {
        if (e.requestType == "paging") {
            var gridObj = $("#GridEdit").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        }
        //var k = 100;
    }
    
    //Getting All the Data For the Saving
    $scope.getAllData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetOperationsData',
            data: { 'PId': $scope.POId},
        }).then(function succ(resp) {
            $scope.ModelList = resp.data;
            for (var i = 0; i < $scope.ModelList.length; i++) {
                Object.assign($scope.ModelList[i], {'Serial': parseInt(i+1) ,'isChanged': 0 });
                //$scope.refreshPage();
            }
        });
    }

   // While Changing the Places
    $scope.changeInData = function (e, col) {

        if (col == $scope.periodId) {
            e.isChanged = 1;
        }
        else if (col == 'emp' || col == 'rem') {
            e.isChanged = 1;
        }
        else {
            ShowResult('Please Enter Value in the Selected Period', 'failure');
        }
    }

    //Saving of the Data
    $scope.saveData = function () {

        $scope.NewList = [];

        for (var i = 0; i < $scope.ModelList.length; i++) {
            if ($scope.ModelList[i].isChanged == true) {
                $scope.NewList.push($scope.ModelList[i]);
            }
        }

        $http({
            method: 'POST',
            url: $scope.path + 'saveData',
            data: {
                'data': $scope.NewList, 'WorkCenter': $scope.workCenterId,
                'ProcessId': $scope.ProcessId,
                'ShiftId': $scope.shiftId,
                'POId': $scope.POId ,
                'Date': $scope.Date,
                  },
        }).then(function succ(resp) {

            if (resp.data.Error === true) {
                ShowResult(resp.data.Message, 'failure');
            }
            else {
                ShowResult(resp.data.Message, 'success');
                $scope.ClearGrid();
            }

        });
    }

    //Clearing the grid
    $scope.ClearGrid = function(){
        $scope.ModelList = [];
    }
}