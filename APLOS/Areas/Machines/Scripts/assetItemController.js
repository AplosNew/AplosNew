'use strict';
AssetItemController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", 'cboService'];
function AssetItemController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.machineTypes = [];

    $scope.machineTypeXLUrl = 'Machines/assetitem/machinetypereport';
    $scope.getListUrl = 'Machines/assetitem/getmachinetypelist';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.machineTypes = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.machineType = {
        Id: null,
        FixedAssetClassId: null,
        FixedAssetClassName: null,
        FixedAssetSubClassId: null,
        FixedAssetMasterId: null,
        FixedAssetSubClassName: null,
        FixedAssetMasterName: null,
        Dimension1: null,
        Dimension2: null,
        Dimension3: null,
        BaseUOMId: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        WithSKU: false,
        Active: true
    };
    $scope.machineTypeNew = Object.assign({}, $scope.machineType);

    $scope.searchMachineTypelList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'ShortName',
            'value': 'ShortName'
        },
        {
            'name': 'StandardName',
            'value': 'StandardName'
        },
        {
            'name': 'UserName',
            'value': 'UserName'
        },
        {
            'name': 'Fixed Asset Class',
            'value': 'FixedAssetClassName'
        },
        {
            'name': 'Fixed Asset SubClass',
            'value': 'FixedAssetSubClassName'
        }
    ];
    // #region DDL
    cboService.getUoMCbo(function (response) {
        $scope.uOMList = response;
    });
    $scope.classList = [];
    $http({
        method: 'GET',
        url: 'fixedassets/fixedassetclass/getcbo'
    }).then(function successCallback(response) {
        $scope.classList = response.data;
    });

    $scope.subClassList = [];
    $http({
        method: 'GET',
        url: 'fixedassets/fixedassetsubclass/getcbo'
    }).then(function successCallback(response) {
        $scope.subClassList = response.data;
    });

    $scope.dbSkillList = [];
    $http({
        method: 'GET',
        url: 'skills/skill/getcboformachinetype'
    }).then(function successCallback(response) {
        $scope.dbSkillList = response.data;
    });
    // #endregion

    $scope.GetSequence = function () {
        $http.get("Machines/assetitem/getautosequence")
            .then(function (response) {
                $scope.machineTypeNew.Sequence = response.data;
            });
    };
    $scope.getAssetItemAttribute = function () {
        $http.get("Machines/assetitem/GetAssetItemAttributeList?assetItem=" + $scope.machineType.Id)
            .then(function (response) {
                $scope.attributeMasters = response.data;
            });
    };
    $scope.GetSequence();
    $scope.assetItemCharacteristicsList = [];
    cboService.getCboAssetItemCharacteristics(function (result) {
        $scope.assetItemCharacteristicsList = result;
    });
    //#region =============UOM===========/
    $scope.AltUomAction = 'Add Alternative UOM';
    $scope.baseUOM = null;
    $scope.altUomIndex = -1;
    $scope.putValueInAltUom = function () {
        $scope.baseUOM = document.getElementById("baseUOMId").options[document.getElementById('baseUOMId').selectedIndex].text;
    }

    // #region AssetItemAlternativeUOM
    $scope.assetItemAlternativeUOMs = [];
    $scope.assetItemAlternativeUOM = {
        Id: null,
        AssetItemId: null,
        AlternativeUOMId: null,
        AlternativeUOMName: null,
        AlternativeUOMFactor: 1,
        BaseUOMId: null,
        BaseUOMName: null,
        BaseUOMFactor: null,
        Active: true,
        Archive: false,
    };
    $scope.assetItemAlternativeUOMNew = angular.copy($scope.assetItemAlternativeUOM);
    $scope.GetAlternativeUomListByAssetItem = function (id) {
        $http({
            method: 'GET',
            url: 'Machines/AssetItem/GetAssetItemAltUomList?assetItemId=' + id,
        }).then(function successCallback(response) {
            $scope.assetItemAlternativeUOMs = response.data;
        });
    }
    $scope.GetAssetItemAlternativeUom = function (id, index) {
        $scope.altUomIndex = index;
        $scope.assetItemAlternativeUOM = $scope.assetItemAlternativeUOMs[$scope.altUomIndex];
        $scope.assetItemAlternativeUOMNew = angular.copy($scope.assetItemAlternativeUOM);
        $scope.AltUomAction = 'Update Alternative UOM';
    }
    $scope.addRow = function () {
        try {
            if ($scope.machineTypeNew.BaseUOMId == null) {
                throw 'Please select base uom from uom tab';
            }
            if ($scope.assetItemAlternativeUOMNew.AlternativeUOMId == null) {
                throw 'Please select alternative uom';
            }
            if ($scope.machineTypeNew.BaseUOMId == $scope.assetItemAlternativeUOMNew.AlternativeUOMId) {
                throw 'Base uom and alternative uom can not be same. Please select another alternative uom.';
            }
            var isAvailable = false;
            $scope.altUomName = document.getElementById("altUOMId").options[document.getElementById('altUOMId').selectedIndex].text;
            $scope.baseUOM = document.getElementById("baseUOMId").options[document.getElementById('baseUOMId').selectedIndex].text;
            for (var i = 0; i < $scope.assetItemAlternativeUOMs.length; i++) {
                isAvailable = listValidation($scope.assetItemAlternativeUOMs[i].AlternativeUOMId,
                    $scope.assetItemAlternativeUOMNew.AlternativeUOMId,
                    $scope.assetItemAlternativeUOMs[i].Archive, i);
                if (isAvailable) {
                    throw 'This alternative uom : [' + $scope.altUomName + '] has been already taken. Please select another alternative uom';
                }
            }

            if ($scope.assetItemAlternativeUOMNew.BaseUOMFactor > 0) {
                angular.copy($scope.assetItemAlternativeUOMNew, $scope.assetItemAlternativeUOM);
                // isAvailable true == add new
                if (!isAvailable) {
                    if ($scope.altUomIndex == -1) {
                        this.assetItemAlternativeUOM.Id = null;
                        this.assetItemAlternativeUOM.AlternativeUOMId = $scope.assetItemAlternativeUOMNew.AlternativeUOMId;
                        this.assetItemAlternativeUOM.AlternativeUOMName = $scope.altUomName;
                        this.assetItemAlternativeUOM.BaseUOMId = $scope.machineTypeNew.BaseUOMId;
                        this.assetItemAlternativeUOM.BaseUOMName = $scope.baseUOM;
                        this.assetItemAlternativeUOM.Active = true;
                        $scope.assetItemAlternativeUOMs.push($scope.assetItemAlternativeUOM);
                        clearAltUOM();
                    }
                    else {
                        $scope.assetItemAlternativeUOMs[$scope.altUomIndex] = this.assetItemAlternativeUOM;
                        $scope.assetItemAlternativeUOMs[$scope.altUomIndex].AlternativeUOMName = $scope.altUomName;
                        $scope.assetItemAlternativeUOMs[$scope.altUomIndex].BaseUOMName = $scope.baseUOM;
                        $scope.altUomIndex = -1;
                        clearAltUOM();
                    }
                    $scope.AltUomAction = 'Add Alternative UOM';
                    $scope.index = -1;
                }
            } else
                throw 'Please insert base uom factor';
        } catch (err) {
            ShowResult(err, 'failure');
        }
    }

    //Check Alt UOM List
    function listValidation(oldValue, newValue, archive, index) {
        var isAvailable = false;
        // Id
        if ($scope.altUomIndex == -1) {
            if (!archive) {
                if (oldValue == newValue) {
                    isAvailable = true;
                    return isAvailable;
                }
            }
        }
        else {
            if ($scope.altUomIndex != index) {
                if (archive) {
                    if (oldValue == newValue) {
                        isAvailable = true;
                        return isAvailable;
                    }
                }
            }
        }
        return isAvailable;
    }
    $scope.valuePassInUomDelModal = function (id, index, altUomName) {
        $scope.mauid = id;
        $scope.mauindex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + altUomName + ' ]';
        angular.element(document.querySelector('#mmaltuom')).modal('show');
    };
    $scope.removeRow = function () {
        $scope.assetItemAlternativeUOMs.splice($scope.mauindex, 1);
        $scope.createUomList();
        $scope.AltUomAction = 'Add Alternative UOM';
        $scope.mauid = null;
        $scope.mauindex = -1;
    };
    function getLen(list) {
        var count = 0;
        for (var i = 0; i < list.length; i++) {
            if (!list[i].Archive) {
                count++;
            }
        }
        return count;
    }
    function clearAltUOM() {
        $scope.assetItemAlternativeUOMNew.AlternativeUOMId = null;
        $scope.assetItemAlternativeUOMNew.BaseUOMFactor = null;
        $scope.assetItemAlternativeUOM = {};
    };
    // #endregion
    //#endregion
    $scope.Get = function (data, index) {
        $scope.baseUOM = null;
        var check = $scope.machineTypeForm.$valid;
        $scope.index = index;
        $scope.machineType = data;
        $scope.machineTypeNew = angular.copy($scope.machineType);
        $scope.getSkillProcessList();
        $scope.getAssetItemAttribute();
        $scope.GetAlternativeUomListByAssetItem($scope.machineTypeNew.Id);
        $scope.baseUOM = data.BaseUomName;
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    function validateDimension() {
        if ($scope.machineTypeNew.WithSKU) {
            if (!baseService.isUndefinedOrNull($scope.machineTypeNew.Dimension1) && !baseService.isUndefinedOrNull($scope.machineTypeNew.Dimension3) && baseService.isUndefinedOrNull($scope.machineTypeNew.Dimension2)) {
                throw 'Please select dimension sequentialy.'
            }
            checkDimensionUsed($scope.machineTypeNew.Dimension1);
            checkDimensionUsed($scope.machineTypeNew.Dimension2);
            checkDimensionUsed($scope.machineTypeNew.Dimension3);
            if (!baseService.isUndefinedOrNull($scope.machineTypeNew.Dimension1) && !baseService.isUndefinedOrNull($scope.machineTypeNew.Dimension3) && baseService.isUndefinedOrNull($scope.machineTypeNew.Dimension2)) {
                throw 'Please select dimension sequentialy.'
            }
            if (baseService.isUndefinedOrNull($scope.machineTypeNew.Dimension1) && !baseService.isUndefinedOrNull($scope.machineTypeNew.Dimension3) && !baseService.isUndefinedOrNull($scope.machineTypeNew.Dimension2)) {
                throw 'Please select dimension sequentialy.'
            }
        }
    }
    function checkDimensionUsed(value) {
        var match = 1;
        if (value === $scope.machineTypeNew.Dimension1) {
            match++;
        }
        if (value === $scope.machineTypeNew.Dimension2) {
            match++;
        }
        if (value === $scope.machineTypeNew.Dimension3) {
            match++;
        }
        if (match > 2) {
            throw 'Same dimension can not be duplicate.'
        }
    }

    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            angular.copy($scope.machineTypeNew, $scope.machineType);
            if ($scope.machineTypeForm.$valid) {
                baseService.isSeqValid($scope.attributeMasters, 'IsFreeField', 'IsPreDefinedField', 'Please select free field or pre-defined field or both');
                $scope.machineClassName = document.getElementById("machineClassId").options[document.getElementById('machineClassId').selectedIndex].text;
                for (var i = 0; i < $scope.skillProcessList.length; i++) {
                    if ($scope.skillProcessList[i].SkillId == null)
                        throw 'Please select skill in process...........';
                }
                validateDimension();
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: "Machines/assetItem/create",
                        data: {
                            'machineType': $scope.machineType
                            , 'machineTypeProcess': $scope.skillProcessList
                            , 'assetItemAttribute': $scope.attributeMasters
                            , 'assetItemAlternativeUOM': $scope.assetItemAlternativeUOMs
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.getData();
                            $scope.machineTypes = $filter('orderBy')($scope.machineTypes, 'Sequence');
                            baseService.paginationAdd();
                            ClearFields(response.data.Sequence);
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
                else if ($scope.Action == "Update") {
                    $http({
                        method: 'POST',
                        url: "Machines/assetItem/edit",
                        data: {
                            'machineType': $scope.machineType
                            , 'machineTypeProcess': $scope.skillProcessList
                            , 'assetItemAttribute': $scope.attributeMasters
                            , 'assetItemAlternativeUOM': $scope.assetItemAlternativeUOMs
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            if ($scope.index > -1) {
                                $scope.getData();
                                $scope.machineTypes = $filter('orderBy')($scope.machineTypes, 'Sequence');
                            }
                            ClearFields(response.data.Sequence);
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.machineType.Id)) {
            $http({
                method: 'POST',
                url: "Machines/assetItem/delete/" + $scope.machineType.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.machineTypes.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    };
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.machineType = {};
        $scope.machineTypeNew = {};
        $scope.machineTypeNew.Sequence = seq;
        $scope.machineTypeNew.Active = true;
        $scope.machineTypeNew.IsUpdateAssetNumber = true;
        $scope.assetItemAlternativeUOMs = [];
        $scope.baseUOM = null;
        $scope.skillProcessList = [];
        $scope.tempProcessList = [];
        $scope.attributeMasters = [];
        clearAltUOM();
        $scope.processTblShow = false;
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #region FixedAssetMaster
    $scope.searchbyixedAssetMasterList = [];
    $scope.fixedAssetMasterSearchPopup = function () {
        $scope.getFixedAssetMasterData();
        angular.element(document.querySelector('#fixedAssetMasterModal')).modal('show');
    };
    $scope.fixedAssetMasterListParameters = {
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
    $scope.getFixedAssetMasterData = function () {
        baseService.setCurrentPage('fixedAssetMasterList');
        $scope.loadFixedAssetMasterData = function (pageno) {
            baseService.paginationBase('fixedassets/fixedassetmaster/GetListForDynamicPopup', pageno, $scope.fixedAssetMasterListParameters)
                .then(function (result) {
                    $scope.fixedAssetMasterList = result.Rows;
                    $scope.fixedAssetMasterListParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.searchbyixedAssetMasterList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyixedAssetMasterList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.loadFixedAssetMasterData();
    };
    $scope.selectFixedAssetMasterData = function (data) {
        $scope.machineTypeNew.FixedAssetMasterName = data.UserName;
        $scope.machineTypeNew.FixedAssetMasterId = data.Id;
        angular.element(document.querySelector('#fixedAssetMasterModal')).modal('hide');
    };
    //#endregion

    // #region Process

    $scope.tempProcessList = [];
    $scope.selectProcessChValueId = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempListId($scope.tempProcessList, data.Id) === false) {
                    $scope.tempProcessList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempProcessList.length; i++) {
                    if ($scope.tempProcessList[i].Id === data.Id) {
                        $scope.tempProcessList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }
    function checkExistTempListId(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }
    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return true;
            }
        }
        return false;
    }

    $scope.skillProcessList = [];
    $scope.processList = [];
    $scope.getSkillProcessList = function () {
        $http({
            method: 'GET',
            url: 'Machines/assetitem/getmachineprocesslist?machineTypeId=' + $scope.machineTypeNew.Id
        }).then(function successCallback(response) {
            $scope.skillProcessList = response.data.Rows;
            if ($scope.skillProcessList.length > 0) {
                for (var i = 0; i < $scope.skillProcessList.length; i++) {
                    $scope.skillProcessList[i].newSkillList = $scope.newSkillList($scope.skillProcessList[i].ProcessId);
                }
                $scope.processTblShow = true;
            }
            else
                $scope.processTblShow = false;
        });
    };

    $scope.processParameters = {
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
    $scope.processPopUp = function () {
        $scope.tempProcessList = [];
        baseService.setCurrentPage('processList');
        $scope.getProcessData = function (pageno) {
            $scope.getProcessUrl = 'Processes/process/GetList?processid=' + isProcessIdExistGrid($scope.skillProcessList);
            baseService.paginationBase($scope.getProcessUrl, pageno, $scope.processParameters)
                .then(function (result) {
                    $scope.processList = result.Rows;
                    $scope.processParameters.total_count = result.Total;
                    for (var i = 0; i < $scope.processList.length; i++) {
                        $scope.processList[i].Flag = getActive($scope.tempProcessList, $scope.processList[i].Id)
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#processPopUp')).modal('show');
        $scope.getProcessData();
    };
    $scope.CloseProcessPopUp = function () {
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };
    $rootScope.searchProcessByList = [

        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'UserName',
            'value': 'UserName'
        }
    ];
    function isProcessIdExistGrid(list) {
        $scope.ProcessIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i]['Archive'] == false) {
                    $scope.ProcessIds.push(list[i]['ProcessId']);
                }
            }
        }
        return JSON.stringify($scope.ProcessIds);
    }
    $scope.newSkillList = function (processId) {
        var list = [];
        for (var i = 0; i < $scope.dbSkillList.length; i++) {
            if ($scope.dbSkillList[i].ProcessId == processId) {
                list.push($scope.dbSkillList[i]);
            }
        }
        return list;
    }
    $scope.addProcess = function () {
        if (!isRowSelected($scope.processList)) {
            ShowResult('Please select at least one row...!', 'failure', 'processPopUp');
            return;
        }
        angular.forEach($scope.tempProcessList, function (a) {
            if (a.Flag) {
                $scope.skillProcessList.push({
                    Id: null,
                    ProcessId: a.Id,
                    Sequence: a.Sequence,
                    Code: a.Code,
                    ShortName: a.ShortName,
                    StandardName: a.StandardName,
                    UserName: a.UserName,
                    MaterialType: a.MaterialType,
                    SkillId: null,
                    newSkillList: $scope.newSkillList(a.Id),
                    Active: a.Active,
                    Archive: false
                });
            }
        });
        if (!$scope.processTblShow)
            $scope.processTblShow = true;
        $scope.CloseProcessPopUp();
    };
    function isRowSelected(ilst) {
        try {
            var flag = false;
            for (var i = 0; i < ilst.length; i++) {
                if (ilst[i].Flag) {
                    return flag = true;
                }
            }
        } catch (e) {
        }
    }
    $scope.valuePassInDelModal = function (data, index) {
        $scope.message_confirmation = '';
        $scope.processId = data.ProcessId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + name + ' ]';
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
    };
    $scope.removeProcessRow = function () {
        for (var i = 0; i < $scope.skillProcessList.length; i++) {
            if ($scope.skillProcessList[i].Id == null && $scope.skillProcessList[i].ProcessId == $scope.processId) {
                $scope.skillProcessList.splice(i, 1);
            }
            else if ($scope.skillProcessList[i].Id != null && $scope.skillProcessList[i].ProcessId == $scope.processId)
                $scope.skillProcessList[i].Archive = true;
        }
        if ($scope.skillProcessList.length > 0) {
            $scope.processTblShow = true;
        }
        else {
            $scope.processTblShow = false;
        }
    };

    $scope.machineTypeReport = function () {
        location.href = 'Machines/assetItem/machinetypereport';
    };

    // #endregion

    //#region Attribute
    $scope.fixedAssetAttributeList = [];
    $scope.getFixedAssetAttributeList = function () {
        $http.get('fixedassets/fixedassetattribute/getcbo')
            .then(function successCallback(response) {
                $scope.fixedAssetAttributeList = response.data;
            }, function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }
    $scope.getFixedAssetAttributeList();
    $scope.attribute = {
        Id: null,
        FixedAssetMasterId: $scope.machineTypeNew.Id,
        FixedAssetAttributeId: null,
        FixedAttributeName: null,
        Sequence: null,
        Active: true,
        IsFreeField: true,
        IsPreDefinedField: true,
        IsMandatory: true
    };
    $scope.attributeAction = 'Add Row';
    $scope.attributeIndex = -1;
    $scope.attributeMasters = [];

    $scope.addAttribute = function () {
        try {
            if ($scope.attributeMasters.length > 9)
                throw 'Total no of material attribute can not be more than 10...!';
            if (baseService.isUndefinedOrNull($scope.attribute.FixedAssetAttributeId))
                throw 'Please select fixed asset attribute';
            var isAvailable = false;
            $scope.fixedAttributeName = document.getElementById("attributeId").options[document.getElementById('attributeId').selectedIndex].text;
            for (var i = 0; i < $scope.attributeMasters.length; i++) {
                isAvailable = baseService.isAvailableInList($scope.attributeMasters[i].FixedAssetAttributeId, $scope.attribute.FixedAssetAttributeId, i, $scope.attributeIndex);
                if (isAvailable)
                    throw 'This fixed asset attribute : [' + $scope.fixedAttributeName + '] has been already taken';
            }

            // isAvailable true == add new
            if (!isAvailable) {
                if ($scope.attributeIndex == -1) {
                    $scope.attribute.FixedAttributeName = $scope.fixedAttributeName
                    $scope.attributeMasters.push($scope.attribute);
                }
                else {
                    $scope.attribute.FixedAttributeName = $scope.fixedAttributeName;
                    for (var i = 0; i < $scope.attributeMasters.length; i++) {
                        var b = $scope.attributeMasters[i];
                        if (b.FixedAssetAttributeId === $scope.tempAttribute.FixedAssetAttributeId) {
                            b.FixedAssetAttributeId = $scope.attribute.FixedAssetAttributeId
                            b.FixedAttributeName = $scope.attribute.FixedAttributeName
                        }
                    }
                }

                clearAttribute();
            }
        } catch (err) {
            ShowResult(err, 'failure');
        }
    }
    function clearAttribute() {
        $scope.attribute = {
            Id: null,
            FixedAssetMasterId: $scope.machineTypeNew.Id,
            FixedAssetAttributeId: null,
            FixedAttributeName: null,
            Sequence: null,
            Active: true,
            IsFreeField: true,
            IsPreDefinedField: true,
            IsMandatory: true
        };
        $scope.attributeAction = 'Add Row';
        $scope.attributeIndex = -1;
    }
    //#region ***************FixedAssetAttribute New Add*****************/
    $scope.showFixedAssetAttributePopUpShow = function () {
        $scope.fixedAssetAttributeAddPopUP();
        angular.element(document.querySelector('#attributeMasterPopUp')).modal('show');
    }
    $scope.showFixedAssetAttributePopUpClose = function () {
        $scope.fixedAssetAttributeAddPopUP();
        angular.element(document.querySelector('#attributeMasterPopUp')).modal('hide');
        $scope.getFixedAssetAttributeList();
    }
    $scope.fixedAssetAttributeAddPopUP = function () {
        $scope.FAttributeAction = 'Save';
        $scope.index = -1;
        $scope.attributefixedAssets = [];
        $scope.fixedAssetAttributePath = 'fixedassets/FixedAssetAttribute/';
        $scope.fixedAssetAttributeGetListUrl = $scope.fixedAssetAttributePath + 'GetList';
        $scope.fixedAssetAttributeGetSeqUrl = $scope.fixedAssetAttributePath + 'getautosequence';
        $scope.fixedAssetAttributeSaveUrl = $scope.fixedAssetAttributePath + 'create';
        $scope.fixedAssetAttributeUpdateUrl = $scope.fixedAssetAttributePath + 'edit';
        $scope.fixedAssetAttributeDeleteUrl = $scope.fixedAssetAttributePath + 'delete/';
        baseService.init($scope.fixedAssetAttributeGetListUrl);
        $scope.getFAData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.processes = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getFAData();

        $scope.searchByList = [
            {
                'name': 'Sequence',
                'value': 'Sequence'
            },
            {
                'name': 'Code',
                'value': 'Code'
            },
            {
                'name': 'Short Name',
                'value': 'ShortName'
            },
            {
                'name': 'Standard Name',
                'value': 'StandardName'
            },
            {
                'name': 'User Name',
                'value': 'UserName'
            }
        ];

        $scope.process = {
            Id: null,
            Sequence: null,
            Code: null,
            ShortName: null,
            StandardName: null,
            UserName: null,
            Remarks: null,
            Description: null,
            Active: true
        };
        $scope.processNew = Object.assign({}, $scope.process);

        $scope.GetFAttributeSequence = function () {
            $http.get($scope.fixedAssetAttributeGetSeqUrl)
                .then(function (response) {
                    $scope.processNew.Sequence = response.data;
                });
        }
        $scope.GetFAttributeSequence();

        $scope.GetFaAttribute = function (id, index) {
            $scope.index = index;
            angular.copy($scope.processes[$scope.index], $scope.process)
            $scope.processNew = Object.assign({}, $scope.process);
            $scope.FAttributeAction = "Update";
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        };

        $scope.FixedAssetAttributeSave = function () {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.form.$valid) {
                angular.copy($scope.processNew, $scope.process);
                if ($scope.FAttributeAction == "Save") {
                    $http({
                        method: 'POST',
                        url: 'Machines/AssetItem/AttributeCreate',
                        data: $scope.process,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, "failure", "attributeMasterPopUp");
                        }
                        else {
                            ShowResult(response.data.Message, "success", "attributeMasterPopUp");
                            $scope.process = response.data.FixedAssetAttribute;
                            $scope.processes.push($scope.process);
                            $scope.processes = $filter('orderBy')($scope.processes, 'Sequence');
                            FixedAssetAttributeClearFields(response.data.Sequence);
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure", "attributeMasterPopUp");
                    });
                    return true;
                }
                else if ($scope.Action == "Update") {
                    $http({
                        method: 'POST',
                        url: $scope.fixedAssetAttributeUpdateUrl,
                        data: $scope.process,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success", "attributeMasterPopUp");
                            if ($scope.index > -1) {
                                angular.copy($scope.process, $scope.processes[$scope.index])
                                $scope.processes = $filter('orderBy')($scope.processes, 'Sequence');
                            }
                            FixedAssetAttributeClearFields(response.data.Sequence);
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure", "attributeMasterPopUp");
                    });
                }
            }
        }
        $scope.deleteFaAttributePop = function () {
            $scope.confirmationMessage = 'Are you sure want to delete permanently?';
            angular.element(document.querySelector('#confirm_PopUpAttribute')).modal('show');
        }
        $scope.FixedAssetAttributeDelete = function () {
            if (!baseService.isUndefinedOrNull($scope.processNew.Id)) {
                $http({
                    method: 'POST',
                    url: $scope.fixedAssetAttributeDeleteUrl + $scope.processNew.Id,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.processes.splice($scope.index, 1);
                        FixedAssetAttributeClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure", "attributeMasterPopUp");
                });
            }
            else {
                ShowResult(commonMessage.primaryKeyNullMessage, "failure", "attributeMasterPopUp");
            }
        }

        $scope.FixedAssetAttributeClear = function () {
            FixedAssetAttributeClearFields($scope.GetFAttributeSequence());
            return true;
        }

        function FixedAssetAttributeClearFields(seq) {
            $scope.Action = "Save";
            $scope.process = {};
            $scope.processNew = {};
            $scope.processNew.Sequence = seq;
            $scope.processNew.Active = true;
        }
    }

    //#endregion
    //#region *************FixedAssetAttributeValue New Add************/
    $scope.showFixedAssetAttributeValuePopUpShow = function () {
        $scope.fixedAssetAttributeValueAddPopUP();
        angular.element(document.querySelector('#attributeValueMasterPopUp')).modal('show');
    }
    $scope.showFixedAssetAttributeValuePopUpClose = function () {
        $scope.fixedAssetAttributeValueAddPopUP();
        angular.element(document.querySelector('#attributeValueMasterPopUp')).modal('hide');
    }
    $scope.fixedAssetAttributeValueAddPopUP = function () {
        $scope.ActionFav = 'Save';
        $scope.faVPath = 'fixedassets/fixedassetattributevalue/';
        $scope.fixedAssetAttributeValueList = [];
        $scope.getFaVListUrl = $scope.faVPath + 'GetList';
        $scope.getFavSeqUrl = $scope.faVPath + 'getautosequence';
        $scope.saveFavUrl = $scope.faVPath + 'create';
        $scope.updateFavUrl = $scope.faVPath + 'edit';
        $scope.deleteFavUrl = $scope.faVPath + 'delete/';
        baseService.init($scope.getFaVListUrl);
        $scope.fixedAssetAttributeValue = {
            Id: null,
            CompanyGroupId: null,
            FixedAssetAttributeId: null,
            Sequence: null,
            Code: null,
            ShortName: null,
            StandardName: null,
            UserName: null,
            Remarks: null,
            Description: null,
            IsDefault: false,
            Active: true
        };

        $scope.fixedAssetAttributeValueNew = Object.assign({}, $scope.fixedAssetAttributeValue);

        $scope.getFavData = function (pageno) {
            $rootScope.parameters.fixedAssetAttributeId = $scope.fixedAssetAttributeValueNew.FixedAssetAttributeId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.fixedAssetAttributeValueList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };

        $scope.fixedAssetAttributeList = [];
        $http.get('fixedassets/fixedassetattribute/getcbo')
            .then(function successCallback(response) {
                $scope.fixedAssetAttributeList = response.data;
            }, function errorCallback(response) {
                ShowResult(response, 'failure');
            });
        $scope.searchByFavList = [
            {
                'name': 'Sequence',
                'value': 'Sequence'
            },
            {
                'name': 'Code',
                'value': 'Code'
            },
            {
                'name': 'Short Name',
                'value': 'ShortName'
            },
            {
                'name': 'Standard Name',
                'value': 'StandardName'
            },
            {
                'name': 'User Name',
                'value': 'UserName'
            }
        ];

        $scope.GetFavSequence = function () {
            $http.get($scope.getFavSeqUrl)
                .then(function (response) {
                    $scope.fixedAssetAttributeValueNew.Sequence = response.data;
                });
        }
        $scope.GetFavSequence();

        $scope.GetFav = function (id, index) {
            $scope.index = index;
            angular.copy($scope.fixedAssetAttributeValueList[$scope.index], $scope.fixedAssetAttributeValue)
            $scope.fixedAssetAttributeValueNew = Object.assign({}, $scope.fixedAssetAttributeValue);
            $scope.Action = "Update";
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        };

        $scope.SaveFav = function () {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.formfav.$valid) {
                angular.copy($scope.fixedAssetAttributeValueNew, $scope.fixedAssetAttributeValue);
                if ($scope.ActionFav == "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveFavUrl,
                        data: $scope.fixedAssetAttributeValue,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.fixedAssetAttributeValue = response.data.FixedAssetAttributeValue;
                            $scope.fixedAssetAttributeValueList.push($scope.fixedAssetAttributeValue);
                            $scope.fixedAssetAttributeValueList = $filter('orderBy')($scope.fixedAssetAttributeValueList, 'Sequence');
                            ClearFields(response.data.Sequence);
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
                else if ($scope.ActionFav == "Update") {
                    $http({
                        method: 'POST',
                        url: $scope.updateFavUrl,
                        data: $scope.fixedAssetAttributeValue,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            if ($scope.index > -1) {
                                angular.copy($scope.fixedAssetAttributeValue, $scope.fixedAssetAttributeValueList[$scope.index])
                                $scope.fixedAssetAttributeValueList = $filter('orderBy')($scope.fixedAssetAttributeValueList, 'Sequence');
                            }
                            ClearFieldsFav(response.data.Sequence);
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                }
            }
        }
        $scope.deleteFaAttributeValuePop = function () {
            $scope.confirmationMessage = 'Are you sure want to delete permanently?';
            angular.element(document.querySelector('#confirm_PopUpAttributeValue')).modal('show');
        }
        $scope.DeleteFav = function () {
            if (!baseService.isUndefinedOrNull($scope.fixedAssetAttributeValueNew.Id)) {
                $http({
                    method: 'POST',
                    url: $scope.deleteFavUrl + $scope.fixedAssetAttributeValueNew.Id,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.fixedAssetAttributeValueList.splice($scope.index, 1);
                        ClearFieldsFav(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
            }
            else {
                ShowResult(commonMessage.primaryKeyNullMessage, "failure");
            }
        }

        $scope.ClearFav = function () {
            ClearFieldsFav($scope.GetFavSequence());
            return true;
        }

        function ClearFieldsFav(seq) {
            $scope.ActionFav = "Save";
            $scope.fixedAssetAttributeValue = {};
            $scope.fixedAssetAttributeValueNew = { FixedAssetAttributeId: $scope.fixedAssetAttributeValueNew.FixedAssetAttributeId };
            $scope.fixedAssetAttributeValueNew.Sequence = seq;
            $scope.fixedAssetAttributeValueNew.Active = true;
            $scope.fixedAssetAttributeValueNew.IsDefault = false;
        }
    }

    //#endregion
    $scope.EditAttribute = function (index, data) {
        $scope.attributeAction = 'Update Row';
        $scope.attributeIndex = index;
        $scope.tempAttribute = data;
        angular.copy(data, $scope.attribute);
    };
    $scope.RemoveRowModal = function (name, index) {
        $scope.rowIndex = index;
        $scope.confirmationMessage = 'Are you sure want to delete [ ' + name + ' ] ?';
        angular.element(document.querySelector('#confirm_PopUp')).modal('show');
    }
    $scope.RemoveFromAttributeList = function () {
        $scope.attributeMasters.splice($scope.rowIndex, 1);
        $scope.rowIndex = -1;
        $scope.confirmationMessage = '';
        clearAttribute();
    };
    //#endregion Attribute
}