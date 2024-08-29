'use strict';
BarcodeGeneratorSettingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function BarcodeGeneratorSettingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Barcode Generator Setting';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Materials/BarcodeGeneratorSetting/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    //for tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


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
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GetSavedEntityData();
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
                    $scope.ModelNew.Id = response.data.Data.Id;
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
                    ClearFields(response.data.Sequence);
                    $scope.getData();
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
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
        $scope.ChildModelList = [];
        $scope.SavedEntityList = [];
    }


    $scope.entityList = [];
    $scope.SavedEntityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            if (baseService.arrayLength($scope.SavedEntityList) > 0) {
                for (var i = 0; i < $scope.SavedEntityList.length; i++) {
                    for (var j = 0; j < $scope.entityList.length; j++) {
                        if ($scope.SavedEntityList[i].EntityId == $scope.entityList[j].Id) {
                            $scope.entityList.splice(j, 1);
                        }
                    }
                }
            }
            angular.element(document.querySelector('#EntityPopup')).modal('show');
        });
    }

    $scope.refreshTemplate = function (args) {
        $("#headschk").ejCheckBox({ "change": CheckBoxSelectAllItemWise });
    };
    function CheckBoxSelectAllItemWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEPopUp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.entityList.length; i++) {
                $scope.entityList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEPopUp").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SavedEntityList = [];
    $scope.CloseEntityPopUp = function () {
        for (var i = 0; i < $scope.entityList.length; i++) {
            if ($scope.entityList[i].Flag == true) {
                if (checkItemExist($scope.SavedEntityList, $scope.entityList[i].Id) === false) {
                    var moi = {};
                    moi.Id = null;
                    moi.BarcodeGeneratorSettingId = $scope.ModelNew.Id;
                    moi.EntityId = $scope.entityList[i].Id;
                    moi.Code = $scope.entityList[i].Code;
                    moi.UserName = $scope.entityList[i].UserName;
                    moi.IsProduction = $scope.entityList[i].IsProduction;
                    moi.Active = $scope.entityList[i].Active;
                    $scope.SavedEntityList.push(moi);
                    moi = {};
                }
            }
        }
        $scope.SaveEntity();
    }

    function checkItemExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EntityId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.saveEntityUrl = 'Materials/BarcodeGeneratorSetting/CreateEntity';
    $scope.SaveEntity = function () {
        $http({
            method: 'POST',
            url: $scope.saveEntityUrl,
            data: { 'data': $scope.SavedEntityList, 'masterId': $scope.ModelNew.Id},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetSavedEntityData();
                angular.element(document.querySelector('#EntityPopup')).modal('hide');
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };



    $scope.GetSavedEntityData = function () {
        try {
            $http({
                method: 'GET',
                url: 'Materials/BarcodeGeneratorSetting/GetSavedEntity?masterId=' + $scope.ModelNew.Id
            }).then(function successCallback(response) {
                $scope.SavedEntityList = response.data;
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



}