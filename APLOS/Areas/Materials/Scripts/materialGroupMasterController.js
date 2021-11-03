'use strict';
MaterialGroupMasterController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function MaterialGroupMasterController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Material GroupMaster";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.materialGroupMasters = [];
    $scope.path = 'Materials/materialgroupmaster/';
    $scope.materialGroupMasterXLUrl = 'Materials/materialgroupmaster/materialgroupmasterreport';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'MaterialTypeName,MaterialGroup1Name,MaterialGroup2Name,MaterialGroup3Name,MaterialGroup4Name,UserName', 'UserName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.materialGroupMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $rootScope.searchByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Defined Name',
            'value': 'UserName'
        },
        {
            'name': 'Material Group1 Name',
            'value': 'MaterialGroup1Name'
        },
        {
            'name': 'Material Group2 Name',
            'value': 'MaterialGroup2Name'
        },
        {
            'name': 'Material Group3 Name',
            'value': 'MaterialGroup3Name'
        },
        {
            'name': 'Material Group4 Name',
            'value': 'MaterialGroup4Name'
        },
        {
            'name': 'MaterialType',
            'value': 'MaterialTypeName'
        },
        {
            'name': 'Inventory Issue Policy',
            'value': 'InventoryIssuePolicy'
        }
    ];

    $scope.materialGroupMaster = {
        Id: null
        , MaterialGroup1Id: null
        , MaterialGroup2Id: null
        , MaterialGroup3Id: null
        , MaterialGroup4Id: null
        , MaterialGroup1Name: null
        , MaterialGroup2Name: null
        , MaterialGroup3Name: null
        , MaterialGroup4Name: null
        , MaterialTypeId: null
        , MaterialTypeName: null
        , BaseUoMId: null
        , HSNCodeId: null
        , InventoryIssuePolicy: 'FIFO'
        , Code: null
        , UserName: null
        , Active: true
    };
    $scope.materialGroupMasterNew = Object.assign({}, $scope.materialGroupMaster);

    $http({
        method: 'GET',
        url: 'Materials/materialGroup1/getcbo'
    }).then(function successCallback(response) {
        $scope.materialGroup1List = response.data;
    });
    $http({
        method: 'GET',
        url: 'Materials/materialgroup2/getcbo'
    }).then(function successCallback(response) {
        $scope.materialGroup2List = response.data;
    });
    $http({
        method: 'GET',
        url: 'Materials/materialGroup3/getcbo'
    }).then(function successCallback(response) {
        $scope.materialGroup3List = response.data;
    });
    $http({
        method: 'GET',
        url: 'Materials/materialGroup4/getcbo'
    }).then(function successCallback(response) {
        $scope.materialGroup4List = response.data;
    });
    $http({
        method: 'GET',
        url: 'Materials/materialtype/getcbo'
    }).then(function successCallback(response) {
        $scope.materialTypeList = response.data;
    });

    cboService.getUoMCbo(function (response) {
        $scope.uOMList = response;
    });
    cboService.getHNSCbo(function (response) {
        $scope.hsnCodeList = response;
    });
    cboService.getPackingFromCboByCompanyGroup(null, function (response) {
        $scope.pFormList = response;
    });
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.materialGroupMaster = $scope.materialGroupMasters[$scope.index];
        $scope.materialGroupMasterNew = Object.assign({}, $scope.materialGroupMaster);
        $scope.GetMGMAlternativeUoMList();
        $scope.GetPackingFormList();
        getMaterialProductProcessGroupList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    // #region MGMasterAlternativeUOM
    $scope.valueSetInAltUoM = function () {
        $scope.alternativeUoMNew.BaseUoMId = $scope.materialGroupMasterNew.BaseUoMId;
        $scope.alternativeUoMNew.BaseUoMName = document.getElementById("baseUOMId").options[document.getElementById('baseUOMId').selectedIndex].text;
    }
    $scope.AltUomAction = 'Add Alternative UOM';
    $scope.mgmAlternativeUoMList = [];
    $scope.altUomIndex = -1;
    $scope.alternativeUoM = {
        Id: null,
        MaterialGroupMasterId: null,
        AlternativeUoMId: null,
        AlternativeUoMName: null,
        AlternativeUoMFactor: 1,
        BaseUoMId: null,
        BaseUoMName: null,
        BaseUoMFactor: null
    };
    $scope.alternativeUoMNew = angular.copy($scope.alternativeUoM);
    $scope.GetMGMAlternativeUoMList = function () {
        $http({
            method: 'GET',
            url: 'Materials/materialgroupmaster/getmgmalternativeuomlist?masterId=' + $scope.materialGroupMasterNew.Id,
        }).then(function successCallback(response) {
            $scope.mgmAlternativeUoMList = response.data;
            $scope.alternativeUoMNew.BaseUoMName = document.getElementById("baseUOMId").options[document.getElementById('baseUOMId').selectedIndex].text;
        });
    }
    $scope.GetMGMAlternativeUoM = function (id, index) {
        $scope.altUomIndex = index;
        $scope.alternativeUoM = $scope.mgmAlternativeUoMList[$scope.altUomIndex];
        $scope.alternativeUoMNew = angular.copy($scope.alternativeUoM);
        $scope.AltUomAction = 'Update Alternative UoM';
    }
    $scope.addAlternativeUoM = function () {
        try {
            if ($scope.materialGroupMasterNew.BaseUoMId === null) {
                throw 'Please select base uom from general tab';
            }
            if ($scope.alternativeUoMNew.AlternativeUoMId === null) {
                throw 'Please select alternative uom';
            }
            if ($scope.materialGroupMasterNew.BaseUoMId === $scope.alternativeUoMNew.AlternativeUoMId) {
                throw 'Base uom and alternative uom can not be same. Please select another alternative uom.';
            }
            var isAvailable = false;
            $scope.alternativeUoMNew.AlternativeUoMName = document.getElementById("altUOMId").options[document.getElementById('altUOMId').selectedIndex].text;
            $scope.alternativeUoMNew.BaseUoMName = document.getElementById("baseUOMId").options[document.getElementById('baseUOMId').selectedIndex].text;
            for (var i = 0; i < $scope.mgmAlternativeUoMList.length; i++) {
                isAvailable = listValidation($scope.mgmAlternativeUoMList[i].AlternativeUoMId
                    , $scope.alternativeUoMNew.AlternativeUoMId, i);
                if (isAvailable) {
                    throw 'This alternative uom : [' + $scope.alternativeUoMNew.AlternativeUoMName + '] has been already taken. Please select another alternative uom';
                }
            }
            if ($scope.alternativeUoMNew.BaseUoMFactor > 0) {
                $scope.alternativeUoM = Object.assign({}, $scope.alternativeUoMNew);
                // isAvailable true == add new
                if (!isAvailable) {
                    if ($scope.altUomIndex === -1) {
                        $scope.alternativeUoM.BaseUoMId = $scope.materialGroupMasterNew.BaseUoMId;
                        $scope.mgmAlternativeUoMList.push($scope.alternativeUoM);
                        clearAltUOM($scope.alternativeUoMNew.BaseUoMId, $scope.alternativeUoMNew.BaseUoMName);
                    }
                    else {
                        $scope.mgmAlternativeUoMList[$scope.altUomIndex] = $scope.alternativeUoM;
                        $scope.altUomIndex = -1;
                        clearAltUOM($scope.alternativeUoMNew.BaseUoMId, $scope.alternativeUoMNew.BaseUoMName);
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
    function listValidation(oldValue, newValue, index) {
        var isAvailable = false;
        // Id
        if ($scope.altUomIndex === -1) {
            if (oldValue === newValue) {
                isAvailable = true;
                return isAvailable;
            }
        }
        else {
            if ($scope.altUomIndex != index) {
                if (oldValue == newValue) {
                    isAvailable = true;
                    return isAvailable;
                }
            }
        }
        return isAvailable;
    }
    $scope.mauid = null;
    $scope.mauindex = -1;
    //$scope.valuePassInDelModal = function (id, index, altUomName) {
    //    $scope.mauid = id;
    //    $scope.mauindex = index;
    //    $scope.message_confirmation = 'Are you sure want to delete [ ' + altUomName + ' ]';
    //    angular.element(document.querySelector('#mmaltuom')).modal('show');
    //};
    //$scope.removeRow = function () {
    //    for (var i = 0; i < $scope.mgmAlternativeUoMList.length; i++) {
    //        if ($scope.mgmAlternativeUoMList[i].AlternativeUoMId === $scope.mauid) {
    //            $scope.mgmAlternativeUoMList.splice($scope.mauindex, 1);
    //            break;
    //        }
    //    }
    //    $scope.mauid = null;
    //    $scope.mauindex = -1;
    //};
    function clearAltUOM(baseUoMId, baseUoM) {
        $scope.alternativeUoMNew = {
            Id: null,
            MaterialGroupMasterId: $scope.materialGroupMasterNew.Id,
            AlternativeUoMId: null,
            AlternativeUoMName: null,
            AlternativeUoMFactor: 1,
            BaseUoMId: $scope.materialGroupMasterNew.BaseUoMId,
            BaseUoMName: baseUoM,
            BaseUoMFactor: null
        };
        $scope.alternativeUoM = {};
    };
    // #endregion

    //#region Packing Form

    $scope.PackingAction = 'Add Packing Form';
    $scope.packingFormList = [];
    $scope.packingFormIndex = -1;
    $scope.packBtn = false;
    $scope.packingForm = {
        Id: null
        , MaterialGroupMasterId: $scope.materialGroupMasterNew.Id
        , PackingFormId: null
        , PackingFormName: null
        , Sequence: null
        , IsSingleEntry: false
    };
    $scope.packingFormNew = angular.copy($scope.packingForm);
    $scope.GetPackingFormList = function () {
        $http({
            method: 'GET',
            url: 'Materials/materialgroupmaster/GetPackingFormList?masterId=' + $scope.materialGroupMasterNew.Id,
        }).then(function successCallback(response) {
            $scope.packingFormList = response.data;
        });
    }
    $scope.GetPackingForm = function (id, index) {
        $scope.packingFormIndex = index;
        $scope.packingForm = $scope.packingFormList[$scope.packingFormIndex];
        $scope.packingFormNew = angular.copy($scope.packingForm);
        $scope.PackingAction = 'Update Packing Form';
        $scope.packBtn = true;
    }
    $scope.addPackingForm = function () {
        try {
            baseService.isSequenceValidInList($scope.packingFormList, 'Sequence', $scope.packingFormNew.Sequence, $scope.packingFormIndex);
            checkSequence($scope.packingFormList, 'Sequence', parseInt($scope.packingFormNew.Sequence), $scope.packingFormIndex);

            if (baseService.arrayLength($scope.packingFormList) === 2 && $scope.packingFormIndex === -1)
                throw 'Can not add packing form more then 2.....!'
            if (baseService.isUndefinedOrNull($scope.packingFormNew.Sequence))
                throw 'Please insert sequence';
            if (baseService.isUndefinedOrNull($scope.packingFormNew.PackingFormId))
                throw 'Please select packingform';

            baseService.isSequenceValidInList($scope.packingFormList, 'Sequence', $scope.packingFormNew.Sequence, $scope.packingFormIndex)
            var isAvailable = false;

            for (var i = 0; i < $scope.packingFormList.length; i++) {
                isAvailable = baseService.isAvailableInList($scope.packingFormList[i].PackingFormId, $scope.packingFormNew.PackingFormId, i, $scope.packingFormIndex);
                if (isAvailable)
                    throw 'This packingform : [' + $scope.packingFormName + '] has been already taken. Please select another packingform';
            }
            $scope.packingFormNew.PackingFormName = document.getElementById("packingFormId").options[document.getElementById('packingFormId').selectedIndex].text;
            $scope.packingForm = Object.assign({}, $scope.packingFormNew);
            // isAvailable true == add new
            if (!isAvailable) {
                if ($scope.packingFormIndex === -1) {
                    $scope.packingFormList.push($scope.packingForm);
                }
                else {
                    $scope.packingFormList[$scope.packingFormIndex] = $scope.packingForm;
                }
                clearPackingForm();
            }
        } catch (err) {
            ShowResult(err, 'failure');
        }
    }
    function checkSequence(list, fildName, newSeq, index) {
        try {
            if (index === -1) {
                if (list.length !== 0) {
                    if ((parseInt(list[list.length - 1][fildName]) + 1) !== newSeq)
                        throw 'Please input ' + fildName + ' in sequentially. EX: 1,2,3..';
                }
                else
                    if (1 !== newSeq)
                        throw 'Please input ' + fildName + ' 1..!';
            }
        } catch (e) {
            throw e;
        }
    }
    $scope.packingId = null;
    $scope.packingFormDelModal = function (id, index, packingFormName) {
        $scope.packingId = id;
        $scope.packingFormIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + packingFormName + ' ]';
        angular.element(document.querySelector('#delPackingFormPopUp')).modal('show');
    };
    $scope.removePackingFormRow = function () {
        if (baseService.arrayLength($scope.packingFormList) > ($scope.packingFormIndex + 1)) {
            return ShowResult('You can not delete before ' + $scope.packingFormList[$scope.packingFormIndex + 1].Sequence, 'failure');
        }
        $scope.packingFormList.splice($scope.packingFormIndex, 1);
        $scope.packingFormIndex = -1;
        $scope.packingId = null;
    };
    function clearPackingForm() {
        $scope.packingFormNew = {
            Id: null
            , MaterialGroupMasterId: $scope.materialGroupMasterNew.Id
            , PackingFormId: null
            , PackingFormName: null
            , Sequence: null
            , IsSingleEntry: false
        };
        $scope.packingForm = {};
        $scope.packingFormIndex = -1;
        $scope.PackingAction = 'Add Packing Form';
        $scope.packBtn = false;
    };
    //#endregion Packing Form

    $scope.Save = function () {
        $scope.materialGroup1 = $("#materialGroup1 option:selected").text();
        $scope.materialGroup2 = $("#materialGroup2 option:selected").text();
        $scope.materialGroup3 = $("#materialGroup3 option:selected").text();
        $scope.materialGroup4 = $("#materialGroup4 option:selected").text();
        $scope.materialType = $("#materialTypeId option:selected").text();
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.materialGroupMasterNewForm.$valid) {
            angular.copy($scope.materialGroupMasterNew, $scope.materialGroupMaster);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'entity': $scope.materialGroupMaster
                        , 'altUoMList': $scope.mgmAlternativeUoMList
                        , 'packing': $scope.packingFormList
                        , 'processGroupList': $scope.processGroupList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.materialGroupMaster = response.data.MaterialGroupMaster;

                        $scope.materialGroupMaster.MaterialGroup1Name = $scope.materialGroup1;
                        $scope.materialGroupMaster.MaterialGroup2Name = $scope.materialGroup2;
                        $scope.materialGroupMaster.MaterialGroup3Name = $scope.materialGroup3;
                        $scope.materialGroupMaster.MaterialGroup4Name = $scope.materialGroup4;
                        $scope.materialGroupMaster.MaterialTypeName = $scope.materialType;
                        $scope.materialGroupMasters.push($scope.materialGroupMaster);
                        $scope.materialGroupMasters = $filter('orderBy')($scope.materialGroupMasters, 'Code');
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: {
                        'entity': $scope.materialGroupMaster
                        , 'altUoMList': $scope.mgmAlternativeUoMList
                        , 'packing': $scope.packingFormList
                        , 'processGroupList': $scope.processGroupList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -4) {
                            $scope.materialGroupMaster.MaterialGroup1Name = $scope.materialGroup1;
                            $scope.materialGroupMaster.MaterialGroup2Name = $scope.materialGroup2;
                            $scope.materialGroupMaster.MaterialGroup3Name = $scope.materialGroup3;
                            $scope.materialGroupMaster.MaterialGroup4Name = $scope.materialGroup4;
                            $scope.materialGroupMaster.MaterialTypeName = $scope.materialType;
                            $scope.materialGroupMasters[$scope.index] = $scope.materialGroupMaster;
                            $scope.materialGroupMasters = $filter('orderBy')($scope.materialGroupMasters, 'Code');
                        }
                        ClearFields();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.materialGroupMasterNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.materialGroupMasterNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.materialGroupMasters.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
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
        $scope.Action = "Save";
        $scope.materialGroupMaster = {};
        $scope.materialGroupMasterNew = { InventoryIssuePolicy: 'FIFO', Active: true };
        $scope.isSet(1);
        $scope.setTab(1);
        $scope.mgmAlternativeUoMList = [];
        $scope.packingFormList = [];
        $scope.processGroupList = [];
        clearPackingForm();
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #region ReturnToRequiredTab
    function reDirectToRequiredTab() {
        if ($scope.materialGroupMasterNewFormTab1.$invalid)
            $scope.setTab(1);
        else if ($scope.materialGroupMasterNewFormTab2.$invalid)
            $scope.setTab(2);
        else if ($scope.materialGroupMasterNewFormTab3.$invalid)
            $scope.setTab(3);
    }
    // #endregion
    $scope.materialGroupMasterReport = function () {
        location.href = 'Materials/materialgroupmaster/materialgroupmasterreport';
    };

    // #region Production Process Group
    $scope.inputList = [];
    $scope.processGroupList = [];
    $scope.prdProcessPopUpList = [];
    $scope.processParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'ProdProcessGroupName',
        searchBy: "ProdProcessGroupName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.prdProcessPopUp = function () {
        $rootScope.tempList = [];
        angular.forEach($scope.processGroupList, function (a) {
            $rootScope.tempList.push({
                Id: a.Id
                , MaterialGroupMasterId: $scope.materialGroupMasterNew.Id
                , ProductionProcessGroupId: a.ProductionProcessGroupId
                , Code: a.Code
                , ShortName: a.ProdProcessGroupName
                , ProdProcessGroupName: a.ProdProcessGroupName
            });
        });
        $scope.getProcessGroupData = function (pageno) {
            $scope.getProcessUrl = $scope.path + 'GetProductProcessGroupList?ids=' + baseService.getColumnValueList($scope.processGroupList, 'ProductionProcessGroupId');
            baseService.paginationBase($scope.getProcessUrl, pageno, $scope.processParameters)
                .then(function (result) {
                    $scope.prdProcessList = result.Rows;
                    $scope.processParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.prdProcessPopUpList) == 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.prdProcessPopUpList);
                    for (var t = 0; t < baseService.arrayLength($scope.prdProcessList); t++) {
                        $scope.prdProcessList[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'ProductionProcessGroupId', $scope.prdProcessList[t].ProductionProcessGroupId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#prdProcessPopUp')).modal('show');
        $scope.getProcessGroupData();
    };
    $scope.addPrdProcess = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a, i) {
                if (!baseService.valueCheckInList($scope.processGroupList, 'ProductionProcessGroupId', a.ProductionProcessGroupId)) {
                    $scope.processGroupList.push({
                        Id: a.Id
                        , MaterialGroupMasterId: $scope.materialGroupMasterNew.Id
                        , ProductionProcessGroupId: a.ProductionProcessGroupId
                        , Code: a.Code
                        , ShortName: a.ProdProcessGroupName
                        , ProdProcessGroupName: a.ProdProcessGroupName
                        , Sequence: i + 1
                    });
                    addInInputList(a.ProductionProcessGroupId, a.ProdProcessGroupName);
                }
            });
        }
        else
            $scope.processGroupList = [];
        angular.forEach($scope.processGroupList, function (a) {
            if (!baseService.valueCheckInList($rootScope.tempList, 'ProductionProcessGroupId', a.ProductionProcessGroupId)) {
                $scope.processGroupList.splice(a, 1);
                listReset($scope.processGroupList);
            }
        });
        $scope.closePrdProcessPopUp();
    };
    $scope.closePrdProcessPopUp = function () {
        $rootScope.tempList = [];
        angular.element(document.querySelector('#prdProcessPopUp')).modal('hide');
    }
    $scope.removeRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure want to permanent delete [" + name + "] ";
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.removeRow = function () {
        for (var t = 0; t < baseService.arrayLength($rootScope.tempList); t++) {
            if ($rootScope.tempList[t][$scope.tempId] === $scope[$scope.listName][$scope.popUpIndex][$scope.listId])
                $rootScope.tempList.splice(t, 1);
        }
        $scope[$scope.listName].splice($scope.popUpIndex, 1);
        if ($scope.listName === 'processGroupList') {
            listReset($scope.processGroupList);
            removeFromInputList();
        }
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
    };
    function listReset(list) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            list[i].Sequence = i + 1;
        }
    }
    function getMaterialProductProcessGroupList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetMaterialProductProcessGroupList?masterId=' + $scope.materialGroupMasterNew.Id
        }).then(function successCallback(response) {
            $scope.processGroupList = response.data;
            $scope.inputList = [];
            angular.forEach($scope.processGroupList, function (a) {
                $scope.inputList.push({
                    Value: a.ProductionProcessGroupId
                    , Text: a.ProdProcessGroupName
                });
            });
        });
    }
    function addInInputList(value, text) {
        if (baseService.arrayLength($scope.inputList) === 0) {
            $scope.inputList.push({
                Value: value
                , Text: text
            });
        }
        else {
            if (!baseService.valueCheckInList($scope.inputList, 'Value', value)) {
                $scope.inputList.push({
                    Value: value
                    , Text: text
                });
            }
        }
    }

    function removeFromInputList() {
        angular.forEach($scope.inputList, function (a, i) {
            if (!baseService.valueCheckInList($scope.processGroupList, 'ProductionProcessGroupId', a.Value)) {
                angular.forEach($scope.processGroupList, function (b, t) {
                    if (b.ProductionProcessGroupId === a.Value)
                        b.InputId = null;
                });
                $scope.inputList.splice(i, 1);
                return;
            }
        });
    }
    // #endregion
}