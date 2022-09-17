'use strict';
DetentionLogController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function DetentionLogController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Detention Log";
    $scope.Action = 'Save';
    $scope.path = 'Materials/DetentionLog/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getStorage = $scope.path + 'StorageSql';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'Delete';

    $scope.ModalTemp = {
        Id: null,
        DetentionId: null,
        ProcessId: null,
        WorkCenterId: null,
        ResponsibleContactNo: null,
        IssueByNo: null,
        Remarks: null,
        EntityId:null
        
    };
    $scope.ModalNew = Object.assign({}, $scope.ModalTemp);

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            
        });
    }
    $scope.getAllEntities();

    
    // Department
    $scope.openDeprtmentPopUp = function () {
        $scope.getDepartment();
        angular.element(document.querySelector('#DepartmentPop')).modal('show');
    }

    $scope.DepartmentList = [];
    $scope.getDepartment = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/GetDetentionDepartment',
            dataType: 'JSON'
        }).then(function successCallback(resp) {
            $scope.DepartmentList = resp.data;
        });
    }
    $scope.getDepartment();

    $scope.doubleDepartment = function (e) {
        $scope.Newobject.DepartmentId = e.data.DepartmentId;
        $scope.Newobject.DepartmentName = e.data.DepartmentName;
        angular.element(document.querySelector('#DepartmentPop')).modal('hide');
        $scope.getDetentionTypeListByDepartment($scope.Newobject.DepartmentId);
    }

    $scope.closeDepartmentPopUp = function () {
        angular.element(document.querySelector('#DepartmentPop')).modal('hide');
    }


    // Responsible Person
    $scope.openEmployeePopUp = function () {
        $scope.getsR();
        angular.element(document.querySelector('#ResponiblePersonPop')).modal('show');
    }
    $scope.ResponsibleList = [];
    $scope.getsR = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/GetDetentionResponsible?detentionId=' + $scope.ModalNew.DetentionId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ResponsibleList = resp.data;
        });
    }

    $scope.doubleResponsible = function (e) {
        $scope.Newobject.ResponsiblePersonId = e.data.ResponsiblePersonId;
        $scope.Newobject.ResponsiblePerson = e.data.ResponsiblePerson;
        angular.element(document.querySelector('#ResponiblePersonPop')).modal('hide');
    }

    $scope.closeResponsiblePopUp = function () {
        angular.element(document.querySelector('#ResponiblePersonPop')).modal('hide');
    }


    // Detention Type By Department
    
    
    $scope.getDetentionTypeListByDepartment = function (departmentid) {
        $http({
            method: 'GET',
            url: 'Materials/DetentionLog/getDetentionTypeListByDepartment?departmentid=' + departmentid
        }).then(function successCallback(response) {
            //$scope.DetentionList = null;
            for (var i = 0; i < $scope.ProcessDetentionLists.length; i++) {
                if ($scope.ProcessDetentionLists[i].DetentionId == null) {
                    $scope.ProcessDetentionLists[i].DetentionTypeList = response.data;
                }
            }
        });
    }

    $scope.ProcesssList = [];
    $scope.getProcessList = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/getProcessList'
        }).then(function successCallback(response) {
            $scope.ProcesssList = response.data;
           
        });
    }
    $scope.getProcessList();

    $scope.WorkCenterList = [];
    $scope.WorkCenter = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/GetWorkCenter?processId=' + $scope.ModalNew.ProcessId
        }).then(function successCallback(response) {
            $scope.WorkCenterList = response.data;

        });
    }

    $scope.DetentionList = [];
    $scope.getDetentionList = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/getDetention?processId=' + $scope.ModalNew.ProcessId
        }).then(function successCallback(response) {
            $scope.DetentionList = response.data;
           
           
        });
    }
    //$scope.getDetentionList();
}