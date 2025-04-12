'use strict';
ComplianceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ComplianceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Compliance';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Commercial/Compliance/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    $scope.searchBy = "Code"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'Remarks', name: "Remarks" }];

    $scope.CriticalityLevelList = [
        { 'Value': "Normal", 'Text': "Normal" },
        { 'Value': "Critical", 'Text': "Critical" },
        { 'Value': "Important", 'Text': "Important" }
    ];

    $scope.ComplianceValueList = [
        { 'Value': "0", 'Text': "0" },
        { 'Value': "1", 'Text': "1" },
        { 'Value': "2", 'Text': "2" },
        { 'Value': "3", 'Text': "3" }
    ];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();



    $scope.ModelTemp = {
        Id: null,
        ComplianceGroup: null,
        Code: null,
        Category: null,
        SubCategory: null,
        ItemName: null,
        CriticalityLevel: null,
        ComplianceValue: null,
        Remarks: null,
        LocationReference: null,
        ScanApplicable: null,
        CodeApplicable: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);



    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.ModelNew.ComplianceValue = $scope.ModelNew.ComplianceValue.toString();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

    $scope.ShowResponsiblePerson = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
                throw "Select Master data first.";
            }
            $scope.GetRPList();
            angular.element(document.querySelector('#ResponsiblePersonPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }

    $scope.CloseResponsiblePerson = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopUp')).modal('hide');

    }

    $scope.popUpDataList = [];
    $scope.popUp = function () {
        try {

            if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
                $scope.popUpDataList = [];
                $http({
                    method: 'GET',
                    url: 'employees/authorizationconfig/getallemployeedata'

                }).then(function successCallback(response) {
                    $scope.popUpDataList = response.data;
                });
                angular.element(document.querySelector('#popUp')).modal('show');
            }
            else {
                throw "Select Master data first.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.refreshTemplate = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPopUp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                $scope.popUpDataList[i].Flag = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPopUp").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    };

    $scope.RPDataList = [];

    $scope.GetRPList = function () {
        $http({
            method: 'GET',
            url: "Commercial/Compliance/GetRP?masterId="+$scope.ModelNew.Id,
        }).then(function successCallback(response) {
            $scope.RPDataList = response.data;
        });
    }

    $scope.SaveRP = function () {
        for (var i = 0; i < $scope.popUpDataList.length; i++) {
            if ($scope.popUpDataList[i].Flag == true) {
                if (checkExists($scope.RPDataList, $scope.popUpDataList[i].SystemId) === false) {
                    var ob = {};
                    ob.Id = Math.floor(Math.random() * 9) - 10 ;
                    ob.ComplianceMasterId = $scope.ModelNew.Id;
                    ob.EmpSystemID = $scope.popUpDataList[i].SystemId;
                    ob.EmployeeCode = $scope.popUpDataList[i].EmployeeCode;
                    ob.EmployeeName = $scope.popUpDataList[i].EmployeeName;
                    ob.Plant = $scope.popUpDataList[i].Plant;
                    ob.LegalDesignation = $scope.popUpDataList[i].LegalDesignation;
                    ob.Department = $scope.popUpDataList[i].Department;
                    ob.Section = $scope.popUpDataList[i].Section;
                    ob.SubSection = $scope.popUpDataList[i].SubSection;
                    ob.Line = $scope.popUpDataList[i].Line;
                    $scope.RPDataList.push(ob);
                }
            }
        }
        if ($scope.RPDataList.length > 0) {
            $http({
                method: 'POST',
                url: 'Commercial/Compliance/CreateRP',
                data: { 'RPDataList': $scope.RPDataList, 'masterId': $scope.ModelNew.Id},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                   
                    $scope.closePopUp();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }

        
    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemID === id) {
                return true;
            }
        }
        return false;
    }

}