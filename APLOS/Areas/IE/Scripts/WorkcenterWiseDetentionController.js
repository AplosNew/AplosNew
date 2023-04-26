'use strict';
WorkcenterWiseDetentionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function WorkcenterWiseDetentionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Workcenter Wise Detention';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'IE/WorkcenterWiseDetention/';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    $scope.employeeUrl = $scope.path + 'GetEmployeeListByWhom';
    $scope.year = new Date().getFullYear().toString();

    $scope.ModelTransaction = {
        Id: null,
        EntityId: null,
        Entity: null,
        
        Date: null,
        
        ProcessId: null,
        Process: null,
        
        ShiftId: null,
        Shift: null,
        //IfAssetApplicable: false,
       
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTransaction);

    // #region Shift
    $scope.selectShift = function () {
        $scope.getsS();
        angular.element(document.querySelector('#ShiftPop')).modal('show');
    }

    $scope.ShiftList = [];
    $scope.getsS = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getShift',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ShiftList = resp.data;
        });
    }

    $scope.doubleShift = function (e) {
        $scope.ModelNew.ShiftId = e.data.ShiftId;
        $scope.ModelNew.Shift = e.data.ShiftDefination;
        angular.element(document.querySelector('#ShiftPop')).modal('hide');
    }

    $scope.closeShiftPopUp = function () {
        angular.element(document.querySelector('#ShiftPop')).modal('hide');
    }
    // #endregion Shift

    // #region Entity
    $scope.EntityList = [];
    $scope.selectEntity = function () {
        $http({
            method: 'POST',
            
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity",
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EntityList = resp.data;
        });
        angular.element(document.querySelector('#EntityPop')).modal('show');
    }

    $scope.doubleEntity = function (e) {
        $scope.ModelNew.EntityId = e.data.Id;
        $scope.ModelNew.Entity = e.data.UserName;
       // $scope.GetworkcenterData();
        angular.element(document.querySelector('#EntityPop')).modal('hide');
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#EntityPop')).modal('hide');
    }
    // #endregion Entity

    // #region Process
    $scope.selectProcess = function () {
        $scope.getsP();
        angular.element(document.querySelector('#ProcessPop')).modal('show');
    }

    $scope.ProcessList = [];
    $scope.getsP = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getProcess',
           // data: { 'machineMasterId': $scope.ModelNew.MachineMasterId },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ProcessList = resp.data;
        });
    }

    $scope.doubleProcess = function (e) {
        $scope.ModelNew.ProcessId = e.data.Id;
        $scope.ModelNew.Process = e.data.Process;
        //$scope.GetworkcenterData();
        angular.element(document.querySelector('#ProcessPop')).modal('hide');
    }

    $scope.closeProcessPopUp = function () {
        angular.element(document.querySelector('#ProcessPop')).modal('hide');
    }
    // #endregion Process
}