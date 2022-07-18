'use strict';
OTCompensatoryAllocationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function OTCompensatoryAllocationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'OT Compensatory Allocation';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/OTCompensatoryAllocation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    // All Lists Are Here
    $scope.EntityList = [];
    $scope.EmployeeTypeList = [];
    $scope.DepartmentList = [];
    $scope.SectionList = [];
    $scope.SubSectionLIst = [];
    $scope.UserGroup = [];

    $scope.ModelTemp = {
        Id: null,
        EntityId: null,
        DepartmentId: null,
        SubSectionId: null,
        CompensetoryOTId: null,
        Duration:null,
        EmployeeTypeId:null,
        SectionId: null,
        OTCompensationId: null,
        Remarks: null,
        Active:null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    // All get function
    $scope.getEntity = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getEntity",
            dataType: 'JSON'

        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
        })
    }
    $scope.getEntity();

    $scope.getEmployeeType = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getEmployeeType",
            dataType:'JSON',
        }).then(function succesCalback(response) {
            $scope.EmployeeTypeList = response.data;
        })
    }
    $scope.getEmployeeType();

    $scope.getDepartment = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getDepartment",
            dataType:'JSON',
        }).then(function succesCalback(response) {
            $scope.DepartmentList = response.data
        })
    }
    $scope.getDepartment();

    $scope.getSection = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getSection",
            dataType: 'JSON',
        }).then(function succesCalback(response) {
            $scope.SectionList = response.data
        })
    }
    $scope.getSection();

    $scope.getSubSection = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getSubSection",
            dataType: 'JSON',
        }).then(function succesCalback(response) {
            $scope.SubSectionList = response.data
        })
    }
    $scope.getSubSection();

    $scope.viewOTCompensatory = function () {
        $http({
            method: 'POST',
            url: $scope.path + "viewOTCompensatory",
            data: {
                'un': $scope.ModelNew.EntityId,
                'ec': $scope.ModelNew.EmployeeTypeId,
                'dp': $scope.ModelNew.DepartmentId,
                'sc': $scope.ModelNew.SectionId,
                'sbc': $scope.ModelNew.SubSectionId
            },
            dataType: 'JSON',
        }).then(function succesCalback(response) {
            $scope.ModelList = response.data
        })
    }

    $scope.ActiveEmpcbx = function (args) {
        $("#cbxhead").ejCheckBox({ "change": chkFilteredData });
    };

    function chkFilteredData(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridOTCompensation").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ModelList.length; i++) {
                $scope.ModelList[i].IsSelectSlrProc = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].IsSelectSlrProc = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridOTCompensation").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.OTCompesationList = [];
    $scope.check_OR_uncheck = function () {
       
        var filtered = $("#GridOTCompensation").data("ejGrid").getFilteredRecords();

        //if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ModelList.length; i++) {
                if ($scope.ModelList[i].isSelected == true) {
                    $scope.OTCompesationList.push($scope.ModelList[i])
                }
                else {
                    $scope.OTCompesationList.pop($scope.ModelList[i])
                }
            }
       /* }*/

    }
    
}