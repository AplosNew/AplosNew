'use strict';
EmployeeCodeGenerationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeCodeGenerationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Code Generation';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.ModelList = [];
    $scope.path = 'employees/EmployeeCodeGeneration/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        IsEmployeeCodeOpenField: false,
        EmpCodeGenType: null,
        IsAutoEmpCodeWithPrefix: false,
        EmpCodeStartValue: null,
        Prefix: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ModelDetailTemp = {
        Id: null,
        EmployeeCodeGenGroupId: null,
        PlantId: null,
        EmploymentType: null
    };
    $scope.ModelDetailNew = Object.assign({}, $scope.ModelTemp);

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

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.LoadData();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.ChangeEmployeeCodeOpenField = function () {
        if ($scope.ModelNew.IsEmployeeCodeOpenField == true) {
            $scope.ModelNew.EmpCodeGenType = null;
            $scope.ModelNew.EmpCodeStartValue = 0;
            $scope.ModelNew.Prefix = null;
            $scope.ModelNew.IsAutoEmpCodeWithPrefix = false;
        }
    };

    $scope.ChangeEmpCodeGenType = function () {
        if ($scope.ModelNew.EmpCodeGenType != 'AutoIncrement') {
            $scope.ModelNew.EmpCodeStartValue = 0;
            $scope.ModelNew.Prefix = null;
            $scope.ModelNew.IsAutoEmpCodeWithPrefix = false;
        }
    };

    $scope.ChangeAutoEmpCodeWithPrefix = function () {
        if ($scope.ModelNew.IsAutoEmpCodeWithPrefix == false) {
            $scope.ModelNew.Prefix = null;
        }
    };


    $scope.AllPlantList = [];
    $scope.LoadData = function () {
        $http.get('employees/EmployeeCodeGeneration/GetAllEmployeeCodeGenerationPlantData?masterId=' + $scope.ModelNew.Id)
            .then(function (response) {
                $scope.AllPlantList = [];
                $scope.AllPlantList = response.data;
            });
    }
    $scope.LoadData();

    // #region checkbox all

    $scope.refreshTemplate = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPlant").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.AllPlantList.length; i++) {
                $scope.AllPlantList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPlant").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    $scope.CheckValidation = function () {
        if (!$scope.ModelNew.IsEmployeeCodeOpenField) {
            if (baseService.isUndefinedOrNull($scope.ModelNew.EmpCodeGenType)) {
                throw "EmpCode Gen Type is required.";
            }
        }

        if ($scope.ModelNew.EmpCodeGenType == 'AutoIncrement') {
            if (baseService.isUndefinedOrNull($scope.ModelNew.EmpCodeStartValue)) {
                throw "Start Value is required.";
            }
        }
        if ($scope.ModelNew.IsAutoEmpCodeWithPrefix) {
            if (baseService.isUndefinedOrNull($scope.ModelNew.Prefix)) {
                throw "Prefix is required.";
            }
        }
    }

    $scope.Save = function () {
        try {
            $scope.SelectedPlantList = [];
            for (var i = 0; i < $scope.AllPlantList.length; i++) {
                if ($scope.AllPlantList[i].Flag) {
                    $scope.SelectedPlantList.push($scope.AllPlantList[i]);
                }
            }

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                $scope.CheckValidation();
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'data': $scope.ModelNew, 'detaildata': $scope.SelectedPlantList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.ModelNew.Id = response.data.Data.Id;
                        $scope.getData();
                        $scope.LoadData();
                        $scope.Action = 'Update';
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
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
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                    $scope.LoadData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelTemp = {
            Id: null,
            Sequence: 0,
            Code: null,
            ShortName: null,
            StandardName: null,
            UserName: null,
            Description: null,
            Remarks: null,
            Active: true,
            IsEmployeeCodeOpenField: false,
            EmpCodeGenType: null,
            IsAutoEmpCodeWithPrefix: false,
            EmpCodeStartValue: null,
            Prefix: null
        };
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
        $scope.LoadData();
    }

}