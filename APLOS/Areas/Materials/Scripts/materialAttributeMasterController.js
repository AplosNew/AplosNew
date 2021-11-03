'use strict';
MaterialAttributeMasterController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function MaterialAttributeMasterController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Material Attribute Master";
    $scope.Action = 'MatarialAttributeSave';
    $scope.ChAction = 'Add Row';
    $scope.index = -1;
    $scope.materialAttributeMasters = [];
    $scope.path = 'Materials/materialattributemaster/';
    $scope.getMaterialAttributeListUrl = $scope.path + 'getlist';
    $scope.MatarialAttributeSaveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getMatarialAttributList = function () {
        $http({
            method: 'GET',
            url: $scope.getMaterialAttributeListUrl,
            params: { materialGroupMasterId: $scope.materialAttributeMasterNew.MaterialGroupMasterId }
        }).then(function successCallback(response) {
            $scope.materialAttributeMasters = response.data;
            if (response.data.length > 0)
                $scope.gethierarchy();
        });
    };

    $scope.materialAttributeList = [];
    $http({
        method: 'GET',
        url: 'Materials/materialattribute/getcbo',
        params: { 'valueAssignment': 'G' }
    }).then(function successCallback(response) {
        $scope.materialAttributeList = response.data;
    });
    $scope.materialAttributeMaster = {
        Id: null
        , MaterialGroupMasterId: null
        , MaterialGroupMasterName: null
        , MaterialAttributeId: null
        , MaterialAttributeName: null
        , Sequence: null
        , Active: true
        , IsFreeField: false
        , IsPreDefinedField: true
        , IsMandatory: true
    };
    $scope.materialAttributeMasterNew = Object.assign({}, $scope.materialAttributeMaster);

    // #region Material GroupMaster

    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpTitle = 'Material Group (Mst)';
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.matarialGroupPopUp = function () {
        $scope.popUpUrl = 'Materials/materialgroupmaster/getlistbymaterialtype?materialTypeId=' + ''
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    }
    $scope.selectDoubleClick = function (data) {
        $scope.materialAttributeMasterNew.MaterialGroupMasterId = data.Id;
        $scope.materialAttributeMasterNew.MaterialGroupMaster = data.UserName;
        $scope.getMatarialAttributList();
        $scope.gethierarchy();
        $scope.closePopUp();
    }

    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    }
    $scope.selectByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            return ShowResult('Please at first select row', 'failure', 'popUpId');
        }
        $scope.selectDoubleClick($scope.valueData)
        $scope.closePopUp();
    }
    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    }
    $scope.materialGroupClear = function () {
        $scope.materialAttributeMasterNew.MaterialGroupMasterId = null;
        $scope.materialAttributeMasterNew.MaterialGroupMaster = null;
        $scope.gethierarchyList = null;
    }
    $scope.gethierarchy = function () {
        $http({
            method: 'GET',
            url: 'Materials/materialgroupmaster/gethierarchy?id=' + $scope.materialAttributeMasterNew.MaterialGroupMasterId,
        }).then(function successCallback(response) {
            if (response.data.Rows.length > 0) {
                $scope.gethierarchyList = response.data.Rows[0].Hierarchy;
            }
            else
                $scope.gethierarchyList = null;
        });
    }
    $scope.DeleteModal = function (index) {
        $scope.DelIndex = index;
        $scope.subMaterialMessage = 'Are you sure want to permanent delete this............?';
        angular.element(document.querySelector('#materialMaster')).modal('show');
    };
    $scope.removeMaterialAttributeRow = function () {
        $scope.materialAttributeMasters.splice($scope.DelIndex, 1);
        $scope.DelIndex = -1;
    };
    // #endregion

    $scope.change = function () {
        var obj = $.grep($scope.materialAttributeList, function (item) {
            return item.Value == $scope.materialAttributeMasterNew.MaterialAttributeId;
        })[0];
        $scope.materialAttributeMasterNew.IsFreeField = obj.IsFreeField;
        $scope.materialAttributeMasterNew.IsPreDefinedField = obj.IsPreDefinedField;
        $scope.materialAttributeMasterNew.IsMandatory = obj.IsMandatory;
    };
    //$scope.materialAttributes = [];
    $scope.addMatarialAttributeRow = function (flag) {
        try {
            if ($scope.materialAttributeMasters.length > 19)
                throw 'Total no of material attribute can not be more than 20...!';
            if ($scope.materialAttributeMasterNew.MaterialGroupMasterId == null)
                throw 'Please select material group(mst)';
            if ($scope.materialAttributeMasterNew.MaterialAttributeId == null)
                throw 'Please select material attribute';
            var isAvailable = false;
            $scope.materialAttributeName = document.getElementById("materialAttributeId").options[document.getElementById('materialAttributeId').selectedIndex].text;
            for (var i = 0; i < $scope.materialAttributeMasters.length; i++) {
                isAvailable = listValidation($scope.materialAttributeMasters[i].MaterialAttributeId, $scope.materialAttributeMasterNew.MaterialAttributeId, i);
                if (isAvailable) throw 'This material attribute : [' + $scope.materialAttributeName + '] has been already taken';
            }
            for (var i in $scope.materialAttributeMasterNew) {
                $scope.materialAttributeMaster[i] = $scope.materialAttributeMasterNew[i];
            }
            // isAvailable true == add new
            if (!isAvailable) {
                if ($scope.index == -1) {
                    $scope.materialAttributeMaster.Sequence = $scope.materialAttributeMasters.length + 1;
                    this.materialAttributeMaster.MaterialAttributeName = $scope.materialAttributeName
                    $scope.materialAttributeMasters.push($scope.materialAttributeMaster);
                }
                else {
                    $scope.materialAttributeMasters[$scope.index] = this.materialAttributeMaster;
                    $scope.materialAttributeMasters[$scope.index].MaterialAttributeName = $scope.materialAttributeName;
                }
                $scope.index = -1;
                ClearFields();
                if (!baseService.isUndefinedOrNull(flag)) CloseModalShowResult();
                else CloseShowResult();
            }
        } catch (err) {
            if (!baseService.isUndefinedOrNull(flag))
                ShowResult(err, 'failure', flag);
            else ShowResult(err, 'failure');
        }
    }
    function listValidation(oldValue, newValue, index) {
        var isAvailable = false;
        // MaterialAttributeId
        if ($scope.index == -1) {
            if (oldValue == newValue) {
                isAvailable = true;
                return isAvailable;
            }
        }
        else {
            if ($scope.index != index) {
                if (oldValue == newValue) {
                    isAvailable = true;
                    return isAvailable;
                }
            }
        }
        return isAvailable;
    }

    $scope.GetMaterialAttributes = function (id, index) {
        $scope.index = index;
        $scope.materialGroupMaster = $scope.materialAttributeMasterNew.MaterialGroupMaster;
        $scope.materialAttributeMaster = $scope.materialAttributeMasters[$scope.index];
        $scope.materialAttributeMasterNew = Object.assign({}, $scope.materialAttributeMaster);
        $scope.materialAttributeMasterNew.MaterialGroupMaster = $scope.materialGroupMaster;

        $scope.ChAction = 'Update Row';
    };

    $scope.Select = function (id, index) {
        $scope.GetMaterialAttributes(id, index);
    }
    $scope.MatarialAttributeSave = function () {
        try {
            CloseShowResult();
            $scope.materialAttributeName = document.getElementById("materialAttributeId").options[document.getElementById('materialAttributeId').selectedIndex].text;
            baseService.isSeqValid($scope.materialAttributeMasters, 'IsFreeField', 'IsPreDefinedField', 'Please select free field or pre-defined field or both');
            for (var t = 0; t < baseService.arrayLength($scope.materialAttributeMasters); t++) {
                var row = $scope.materialAttributeMasters[t];
                for (var a = 0; a < baseService.arrayLength($scope.materialAttributeMasters); a++) {
                    var row2 = $scope.materialAttributeMasters[a];
                    if (row.MaterialAttributeId !== row.MaterialAttributeId && row.Sequence === row.Sequence)
                        throw 'Duplicate sequence can\'t be allowed';
                }
            }
            $http({
                method: 'POST',
                url: $scope.MatarialAttributeSaveUrl,
                data: { materialAttributeMasters: $scope.materialAttributeMasters },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getMatarialAttributList();
                    ClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (err) {
            ShowResult(err, 'failure');
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.materialAttributeMasterNew.MaterialGroupMasterId)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.materialAttributeMasterNew.MaterialGroupMasterId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.Clear = function () {
        ClearFields();
        $scope.materialAttributeMasterNew.MaterialGroupMasterId = null;
        $scope.materialAttributeMasterNew.MaterialGroupMaster = null;
        $scope.materialAttributeMasters = [];
        $scope.gethierarchyList = null;
        return true;
    }

    function ClearFields() {
        //$scope.Action = "MatarialAttributeSave";
        $scope.ChAction = 'Add Row';
        $scope.materialGroupMasterId = $scope.materialAttributeMasterNew.MaterialGroupMasterId;
        $scope.materialGroupMaster = $scope.materialAttributeMasterNew.MaterialGroupMaster;
        $scope.materialAttributeMaster = {};
        $scope.materialAttributeMasterNew = { Active: true, IsFreeField: false, IsPreDefinedField: true, IsMandatory: true };
        $scope.materialAttributeMasterNew.MaterialGroupMasterId = $scope.materialGroupMasterId;
        $scope.materialAttributeMasterNew.MaterialGroupMaster = $scope.materialGroupMaster;
    }
};