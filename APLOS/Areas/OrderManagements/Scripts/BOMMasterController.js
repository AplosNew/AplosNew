'use strict';
BOMMasterController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'cboService', '$window'];
function BOMMasterController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, cboService, $window) {
    $rootScope.title = "BOM";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.lsds = [];
    $scope.path = 'OrderManagements/BOMMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveMasterUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.partyType = "Vendor";
    $controller("partyBaseController", { $scope: $scope, $http: $http });

    $scope.bom = {
        Id: null, FGMaterialMasterId: null, FGArticleId: null, FGMaterialMaster: null, FGArticle: null, Description: null, WithSKU: false, ProductMasterName: null, UnitOfMeasurementId: null
    };
    $scope.bomNew = Object.assign({}, $scope.bom);

    $scope.bomDetailNew = {
        Id: null, BOMMasterId: null, Sequence: 0, RMMaterialMasterId: null, RMArticleId: null, Description: null, CustomerSpec: null, VendorSpec: null, Consumption: 0, UoMId: null, ProcessId: null, VendorId: null, WastagePer: 0, FirstCharacteristicsId: null, SecondCharacteristicsId: null, ThirdCharacteristicsId: null, FirstCharacteristicsValueId: null, SecondCharacteristicsValueId: null, ThirdCharacteristicsValueId: null, IsSKUCommon: true, WithSKU: false, IsConsumptionDetail: false, Specific: true, SKUMatrix: false, IsDestinationSpecific: false, IsPOSpecific: false, ConsumptionSpecificToSKU1: false, ConsumptionSpecificToSKU2: false, ConsumptionSpecificToSKU3: false, SalesOrderSpecificMaterial: true
    }

    $scope.BOMSKUMapping = {
        Id: null, BOMDetailId: null, FGFirstCharacteristicsId: null, FGFirstCharacteristicsValueId: null, FGSecondCharacteristicsId: null, FGSecondCharacteristicsValueId: null, FGThirdCharacteristicsId: null, FGThirdCharacteristicsValueId: null, RMFirstCharacteristicsId: null, RMFirstCharacteristicsValueId: null, RMSecondCharacteristicsId: null, RMSecondCharacteristicsValueId: null, RMThirdCharacteristicsId: null, RMThirdCharacteristicsValueId: null, Description: null, FGName1: null, FGName2: null, FGName3: null, ValueAssignmentLevel: null, ValueAssignmentLevel2: null, ValueAssignmentLevel3: null, IsFirstCharacteristicCommon: false, IsSecondCharacteristicCommon: false, IsThirdCharacteristicCommon: false
    };

    $scope.detailConsumption = {
        Id: null, BOMDetailId: null, Sequence: 0, RMMaterialMasterId: null, RMArticleId: null, Description: null, CustomerSpec: null, VendorSpec: null, Consumption: 0, UoMId: null, ProcessId: null, VendorId: null, WastagePer: 0, FirstCharacteristicsId: null, SecondCharacteristicsId: null, ThirdCharacteristicsId: null, FirstCharacteristicsValueId: null, SecondCharacteristicsValueId: null, ThirdCharacteristicsValueId: null, IsSKUCommon: true, WithSKU: false, IsConsumptionDetail: false, Specific: true, SKUMatrix: false
    }

    $scope.DetailConsumptionSKUMapping = {
        Id: null, DetailConsumptionId: null, RMFirstCharacteristicsId: null, RMFirstCharacteristicsValueId: null, RMSecondCharacteristicsId: null, RMSecondCharacteristicsValueId: null, RMThirdCharacteristicsId: null, RMThirdCharacteristicsValueId: null, SubFirstCharacteristicsId: null, SubFirstCharacteristicsValueId: null, SubSecondCharacteristicsId: null, SubSecondCharacteristicsValueId: null, SubThirdCharacteristicsId: null, SubThirdCharacteristicsValueId: null, Description: null, RMName1: null, RMName2: null, RMName3: null, ValueAssignmentLevel: null, ValueAssignmentLevel2: null, ValueAssignmentLevel3: null
    };

    //ProductDefinition 
    $scope.getMaterial = function (index) {

        $scope.materialType = 'ProductDefinition';
        $scope.itemIndex = index;
        $scope.getMaterialMasterbyTypePopUp();


    };

    $scope.FGmsg = null;
    $scope.selectMaterialByType = function (ob) {
        try {
            $scope.FGMId = $scope.bomNew.FGMaterialMasterId;

            $http({
                method: 'GET',
                url: 'OrderManagements/BOMMaster/GetBOMSKUMappingDataForValidation?BOMMasterId=' + $scope.bomNew.Id
            }).then(function successCallback(response) {
                if (baseService.arrayLength(response.data) > 0 && $scope.FGMId !== ob.Id) {
                    ShowResult("As this Finish Goods has Matrix level SKU, so Finish Goods change is not acceptable.", 'failure');
                }
                else {
                    $scope.bomNew.FGMaterialMasterId = ob.Id;
                    $scope.bomNew.FGMaterialMaster = ob.UserName;
                    $scope.bomNew.ProductMasterName = ob.ProductMasterName;
                    $scope.bomNew.FGArticleId = null;
                    $scope.bomNew.FGArticle = null;
                    $scope.bomNew.HasAttribute = ob.HasAttribute;
                    $scope.bomNew.WithSKU = ob.WithSKU;
                    if ($scope.bomNew.HasAttribute) {
                        $scope.materialType = null;
                        $scope.getArticleSearchList(ob.Id);
                    } else {
                        $scope.closeMaterialMasterbyTypePopUp();
                        return ShowResult('This material has no attribute', 'failure');
                    }
                    if ($scope.bomNew.WithSKU) {
                        $scope.FGmsg = "has";
                    } else {
                        $scope.FGmsg = "has no";
                    }
                    $scope.getFGCharacteristicsList($scope.bomNew.FGMaterialMasterId);
                    $scope.HSNCodeId = ob.HSNCodeId;
                    UomCboByFGMaterialMaster($scope.bomNew.FGMaterialMasterId);
                    $scope.closeMaterialMasterbyTypePopUp();
                }
            })
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.getArticle = function (index) {
        $scope.itemIndex = index;
        //if (!baseService.isUndefinedOrNull($scope.bomNew.FGMaterialMasterId) && !$scope.bomNew.HasAttribute)
        //    return ShowResult('This material has no attribute', 'failure');
        $scope.getArticleSearchList($scope.bomNew.FGMaterialMasterId);
    };

    $scope.selectarticle = function (ob) {
        try {
            $scope.bomNew.FGMaterialMasterId = ob.MaterialMasterId;
            $scope.bomNew.FGMaterialMaster = ob.MaterialMasterName;
            $scope.bomNew.FGArticleId = ob.Id;
            $scope.bomNew.FGArticle = ob.StandardName;
            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };

    $scope.clearArticle = function () {
        $scope.bomNew.ArticleId = null;
        $scope.bomNew.FGArticle = null;
    };

    $scope.clearRMArticle = function () {
        $scope.bomDetailNew.RMArticleId = null;
        $scope.bomDetailNew.RMArticle = null;
    };
    $scope.clearCRMArticle = function () {
        $scope.detailConsumption.RMArticleId = null;
        $scope.detailConsumption.RMArticle = null;
    };
    $scope.clearCharNames = function () {
        $scope.char1 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };
        $scope.char2 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };
        $scope.char3 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };
    };

    $scope.getArticleValue = function (articleId, mName, aName) {
        $scope.articleValueList = [];
        $scope.mName = mName;
        $scope.aName = aName;
        $http({
            method: 'GET',
            url: 'Materials/MaterialMasterArticle/GetMaterialArticleValue?articleId=' + articleId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) === 0)
                return ShowResult('This material has no article value', 'failure');
            $scope.articleValueList = response.data;
            angular.element(document.querySelector('#articleValuePoUp')).modal('show');
        });
    };

    $scope.closeArticleValuePopUp = function () {
        angular.element(document.querySelector('#articleValuePoUp')).modal('hide');
    };

    $scope.FGCharacteristicsValueList = [];
    $scope.GetFGCharacteristicsValueCbo = function (FGMaterialMasterId, CharacteristicsId, valueAssignmentLevel) {
        cboService.getCharacteristicsValueCboByCharacteristicsId(FGMaterialMasterId, CharacteristicsId, valueAssignmentLevel, function (response) {
            $scope.FGCharacteristicsValueList = response;
        });
    }
    $scope.FGCharacteristicsValue2List = [];
    $scope.Get2FGCharacteristicsValueCbo = function (FGMaterialMasterId, CharacteristicsId, valueAssignmentLevel) {
        cboService.getCharacteristicsValueCboByCharacteristicsId(FGMaterialMasterId, CharacteristicsId, valueAssignmentLevel, function (response) {
            $scope.FGCharacteristicsValue2List = response;
        });
    }

    $scope.getFGCharacteristicsList = function (id) {
        $scope.clearCharNames();
        $http({
            method: 'GET',
            url: 'Materials/MaterialMaster/getcharacteristicsbymaterialmasterid/',
            params: {
                materialMasterId: id
            }
        }).then(function (response) {
            $scope.characteristicsList = [];
            $scope.characteristicsList = response.data.charData;
            if (baseService.arrayLength($scope.characteristicsList) > 0) {
                $scope.isSearch = $scope.characteristicsList[0].FreeText !== null ? true : false;
                $scope.char1 = {
                    CharacteristicsId: $scope.characteristicsList[0].Value
                    , CharacteristicsValueId: $scope.characteristicsList[0].CharacteristicsValueId
                    , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                    , Name: $scope.characteristicsList[0].Text
                    , IsFreeField: $scope.characteristicsList[0].IsFreeField
                    , IsPreDefinedField: $scope.characteristicsList[0].IsPreDefinedField
                    , IsMandatory: $scope.characteristicsList[0].IsMandatory
                    , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                    , Sequence: $scope.characteristicsList[0].Sequence
                    , FlagDisable: $scope.IsFreeOrNot($scope.characteristicsList[0].IsFreeField)

                    , FreeText: $scope.characteristicsList[0].FreeText
                    , show: true
                };
                $scope.BOMSKUMapping.FGFirstCharacteristicsId = $scope.char1.CharacteristicsId;
                $scope.BOMSKUMapping.ValueAssignmentLevel = $scope.char1.ValueAssignmentLevel;
                $scope.BOMSKUMapping.FGName1 = $scope.char1.Name !== null ? $scope.char1.Name : 'N/A';

                $scope.GetFGCharacteristicsValueCbo($scope.bomNew.FGMaterialMasterId, $scope.BOMSKUMapping.FGFirstCharacteristicsId, $scope.BOMSKUMapping.ValueAssignmentLevel);
            }
            if (baseService.arrayLength($scope.characteristicsList) > 1) {
                $scope.isSearch = $scope.characteristicsList[1].FreeText !== null ? true : false;
                $scope.char2 = {
                    CharacteristicsId: $scope.characteristicsList[1].Value
                    , CharacteristicsValueId: $scope.characteristicsList[1].CharacteristicsValueId
                    , MaterialMasterId: $scope.characteristicsList[1].MaterialMasterId
                    , Name: $scope.characteristicsList[1].Text
                    , IsFreeField: $scope.characteristicsList[1].IsFreeField
                    , IsPreDefinedField: $scope.characteristicsList[1].IsPreDefinedField
                    , IsMandatory: $scope.characteristicsList[1].IsMandatory
                    , ValueAssignmentLevel: $scope.characteristicsList[1].ValueAssignmentLevel
                    , Sequence: $scope.characteristicsList[1].Sequence
                    , FlagDisable: $scope.IsFreeOrNot($scope.characteristicsList[1].IsFreeField)
                    , FreeText: $scope.characteristicsList[1].FreeText
                    , show: true
                };
                $scope.BOMSKUMapping.FGSecondCharacteristicsId = $scope.char2.CharacteristicsId;
                $scope.BOMSKUMapping.FGName2 = $scope.char2.Name !== null ? $scope.char2.Name : 'N/A';
                $scope.BOMSKUMapping.ValueAssignmentLevel2 = $scope.char2.ValueAssignmentLevel;

                $scope.Get2FGCharacteristicsValueCbo($scope.bomNew.FGMaterialMasterId, $scope.BOMSKUMapping.FGSecondCharacteristicsId, $scope.BOMSKUMapping.ValueAssignmentLevel2);
            }
            if (baseService.arrayLength($scope.characteristicsList) > 2) {
                $scope.isSearch = $scope.characteristicsList[2].FreeText !== null ? true : false;
                $scope.char3 = {
                    CharacteristicsId: $scope.characteristicsList[2].Value
                    , CharacteristicsValueId: $scope.characteristicsList[2].CharacteristicsValueId
                    , MaterialMasterId: $scope.characteristicsList[2].MaterialMasterId
                    , Name: $scope.characteristicsList[2].Text
                    , IsFreeField: $scope.characteristicsList[2].IsFreeField
                    , IsPreDefinedField: $scope.characteristicsList[2].IsPreDefinedField
                    , IsMandatory: $scope.characteristicsList[2].IsMandatory
                    , ValueAssignmentLevel: $scope.characteristicsList[2].ValueAssignmentLevel
                    , Sequence: $scope.characteristicsList[2].Sequence
                    , FlagDisable: $scope.IsFreeOrNot($scope.characteristicsList[2].IsFreeField)
                    , FreeText: $scope.characteristicsList[2].FreeText
                    , show: true
                };
            }
            $scope.BOMSKUMapping.FGThirdCharacteristicsId = $scope.char3.CharacteristicsId;
            $scope.BOMSKUMapping.FGName3 = $scope.char3.Name !== null ? $scope.char3.Name : 'N/A';
            $scope.BOMSKUMapping.ValueAssignmentLevel3 = $scope.char3.ValueAssignmentLevel;

        });

    };

    $scope.SaveMaster = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.bomNew.FGMaterialMasterId)) {
                throw "Finish Goods Material is required.";
            }
            if (baseService.isUndefinedOrNull($scope.bomNew.FGArticleId)) {
                throw "Finish Goods Article is required.";
            }
            if (baseService.isUndefinedOrNull($scope.bomNew.UnitOfMeasurementId)) {
                throw "Finish Goods UoM is required.";
            }

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.bomNewForm.$valid) {
                if ($scope.Action == "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveMasterUrl,
                        data: {
                            'entity': $scope.bomNew
                        },
                        dataType: 'JSON'
                        , contentType: "application/json charset=utf-8"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.bomNew.Id = response.data.Data.Id;
                            $scope.getmasterData();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action == "Update") {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: {
                            'entity': $scope.bomNew
                        },
                        dataType: 'JSON'
                        , contentType: "application/json charset=utf-8"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.bomNew.Id = response.data.Data.Id;
                            $scope.getmasterData();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SelectedBOMRow = {};
    $scope.SelectedBOMRowForCopy = function (data) {
        $scope.SelectedBOMRow = data;

        var eDialog = $("#confirmBOMCopy").data("ejDialog");
        eDialog.open();
        $("#dialogAPI_wrapper").css({ 'position': 'fixed' }).css({ 'top': '200px' });
    }

    $scope.ConfirmSavePopUpClose = function () {
        var eDialog = $("#confirmBOMCopy").data("ejDialog");
        eDialog.close();
    };

    $scope.CopyBOM = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'CopyBOM?Id=' + $scope.SelectedBOMRow.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getmasterData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    }

    $scope.CopyBOMWithoutSKU = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'CopyBOMWithoutSKU?Id=' + $scope.SelectedBOMRow.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getmasterData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.copymessage_detailconfirmation = null;
    $scope.copyBoMDetail = function (obj) {
        $scope.bomDetailNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.bomDetailNew.Id))
            $scope.copymessage_detailconfirmation = 'Are you sure want to copy';
        angular.element(document.querySelector('#confirmCopyBoMDetailPopUp')).modal('show');
    }

    $scope.CopyBomDetailData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'CopyBomDetailData?BOMMasterId=' + $scope.bomDetailNew.BOMMasterId + '&Id=' + $scope.bomDetailNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getDetailData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.bomNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.bomNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getmasterData();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.closePartyPopUp = function (x) {
        var party = x.data;
        $scope.bomDetailNew.VendorId = party.Id;
        $scope.bomDetailNew.PartyCode = party.Code;
        $scope.bomDetailNew.PartyName = party.UserName;

        $scope.hidePartyPopUp();
    };

    $scope.clearVendor = function () {
        $scope.bomDetailNew.VendorId = null;
        $scope.bomDetailNew.PartyCode = null;
        $scope.bomDetailNew.PartyName = null;
    }

    $scope.masterDataList = [];
    $scope.getmasterData = function () {
        $http.get("OrderManagements/BOMMaster/GetList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.masterDataList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getmasterData();

    $scope.detailDataList = [];
    $scope.getDetailData = function () {
        $scope.detailDataList = [];
        $http.get("OrderManagements/BOMMaster/GetDetailList?masterId=" + $scope.bomNew.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.detailDataList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $window.onresize = function (event) {
        $scope.actionComplete();
        $scope.DetailConsumptionActionComplete();
    };

    $scope.actionComplete = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#Grid1").ejGrid("instance");
                var scrollerwidth = $("#consumptionpopup").width();//Obtain the width of the container
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 300, width: 1080 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();

                if (args.action == "rowReordering") {
                    gridObj = $("#Grid1").data("ejGrid");
                    // Gets current view data of grid control
                    var data = gridObj.getCurrentViewData();
                    var sorteddata = ej.DataManager(data).executeLocal(ej.Query().select(["Id"]));
                    $http({
                        method: 'POST',
                        url: $scope.path + "UpdateMaterialSequence",
                        data: { data: sorteddata }
                    }).then(function successCallback(response) {

                    });
                    //for (var i = 0; i < data.length; i++) {
                    //    data[i].Seq = (i + 1);
                    //    data[i].Sequence = (i + 1);
                    //}
                    //gridObj.dataSource(data); 
                    //gridObj.refreshContent(true);
                    //gridObj.refreshTemplate();
                }
            }
        } catch (e) {
            // $scope.ShowResultCustom(e, 'failure');
        }
    };

    $scope.DetailConsumptionActionComplete = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#DetailConsumptionGrid").ejGrid("instance");
                var scrollerwidth = $("#DetailAdd").width();//Obtain the width of the container
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 150, width: 900 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();

                if (args.action == "rowReordering") {
                    gridObj = $("#DetailConsumptionGrid").data("ejGrid");
                    // Gets current view data of grid control
                    var data = gridObj.getCurrentViewData();
                    var sorteddata = ej.DataManager(data).executeLocal(ej.Query().select(["Id"]));
                    $http({
                        method: 'POST',
                        url: $scope.path + "UpdatDetailConsumptionSequence",
                        data: { data: sorteddata }
                    }).then(function successCallback(response) {

                    });

                }
            }
        } catch (e) {
            // $scope.ShowResultCustom(e, 'failure');
        }
    };

    $scope.Get = function (obj) {
        $scope.detailDataList = [];
        $scope.bom = obj.data;
        $scope.bomNew = Object.assign({}, $scope.bom);
        UomCboByFGMaterialMaster($scope.bomNew.FGMaterialMasterId);
        $scope.getFGCharacteristicsList($scope.bomNew.FGMaterialMasterId);
        if ($scope.bomNew.WithSKU) {
            $scope.FGmsg = "has";
        } else {
            $scope.FGmsg = "has no";
        }
        $scope.getDetailData();
        $scope.LoadIssueDocumentsData($scope.bomNew.Id);
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

    $scope.getAutoSequence = function (BOMMasterId) {
        $http.get("OrderManagements/BOMMaster/GetAutoSequence?BOMMasterId=" + BOMMasterId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.bomDetailNew.Sequence = response.data[0].Sequence;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.GetRawMaterialDetail = function (obj) {
        $scope.bomDetailNew = {
            Id: null, BOMMasterId: null, Sequence: 0, RMMaterialMasterId: null, RMArticleId: null, Description: null, CustomerSpec: null, VendorSpec: null, Consumption: 0, UoMId: null, ProcessId: null, VendorId: null, WastagePer: 0, FirstCharacteristicsId: null, SecondCharacteristicsId: null, ThirdCharacteristicsId: null, FirstCharacteristicsValueId: null, SecondCharacteristicsValueId: null, ThirdCharacteristicsValueId: null, IsSKUCommon: true, WithSKU: false, IsConsumptionDetail: false, Specific: true, SKUMatrix: false, IsDestinationSpecific: false, IsPOSpecific: false, ConsumptionSpecificToSKU1: false, ConsumptionSpecificToSKU2: false, ConsumptionSpecificToSKU3: false, SalesOrderSpecificMaterial: true
        }
        //$scope.getRMCharacteristicsList(obj.data.RMMaterialMasterId);

        $scope.bomDetailNew = obj.data;

        if ($scope.bomDetailNew.WithSKU) {
            $scope.msg = "has";
        } else {
            $scope.msg = "has no";
        }

        UomCboByMaterialMaster($scope.bomDetailNew.RMMaterialMasterId);


        if ($scope.bomDetailNew.IsSKUCommon === true) {
            $scope.bomDetailNew.Specific = true;
            $scope.bomDetailNew.SKUMatrix = false;
        } else {
            $scope.bomDetailNew.SKUMatrix = true;
            $scope.bomDetailNew.Specific = false;
        }

        $scope.ShowHide();

        if ($scope.bomDetailNew.Specific === false) {
            $scope.matrixrad = false;

        } else {
            $scope.matrixrad = true;
        }

        $scope.rmchar1.CharacteristicsId = $scope.bomDetailNew.FirstCharacteristicsId;
        $scope.rmchar2.CharacteristicsId = $scope.bomDetailNew.SecondCharacteristicsId;
        $scope.rmchar3.CharacteristicsId = $scope.bomDetailNew.ThirdCharacteristicsId;

        $scope.rmchar1.CharacteristicsValueId = $scope.bomDetailNew.FirstCharacteristicsValueId;
        $scope.rmchar2.CharacteristicsValueId = $scope.bomDetailNew.SecondCharacteristicsValueId;
        $scope.rmchar3.CharacteristicsValueId = $scope.bomDetailNew.ThirdCharacteristicsValueId;

        $scope.rmchar1.Name = $scope.bomDetailNew.SKU1Name;
        $scope.rmchar2.Name = $scope.bomDetailNew.SKU2Name;
        $scope.rmchar3.Name = $scope.bomDetailNew.SKU3Name;

        $scope.rmchar1.FreeText = $scope.bomDetailNew.SKU1;
        $scope.rmchar2.FreeText = $scope.bomDetailNew.SKU2;
        $scope.rmchar3.FreeText = $scope.bomDetailNew.SKU3;

        $scope.rmchar1.ValueAssignmentLevel = $scope.bomDetailNew.C1ValueAssignmentLevel;
        $scope.rmchar2.ValueAssignmentLevel = $scope.bomDetailNew.C2ValueAssignmentLevel;
        $scope.rmchar3.ValueAssignmentLevel = $scope.bomDetailNew.C3ValueAssignmentLevel;

        $scope.rmchar1.MaterialMasterId = $scope.bomDetailNew.RMMaterialMasterId;
        $scope.rmchar2.MaterialMasterId = $scope.bomDetailNew.RMMaterialMasterId;
        $scope.rmchar3.MaterialMasterId = $scope.bomDetailNew.RMMaterialMasterId;

        $scope.getRMCharacteristicsCboList($scope.bomDetailNew.RMMaterialMasterId);



        var DropDownListObj = $("#destinationList").data("ejDropDownList");
        DropDownListObj.uncheckAll();
        $scope.GetBOMDestinationData($scope.bomDetailNew.Id);
        angular.element(document.querySelector('#detailpopup')).modal('show');
    };

    $scope.CloseDetail = function () {
        angular.element(document.querySelector('#detailpopup')).modal('hide');
    }

    $scope.getRMCharacteristicsCboList = function (id) {
        $scope.clearCharNames();
        $http({
            method: 'GET',
            url: 'Materials/MaterialMaster/getcharacteristicsbymaterialmasterid/',
            params: {
                materialMasterId: id
            }
        }).then(function (response) {
            $scope.rm1characteristicsList = [];
            $scope.rm2characteristicsList = [];
            if (baseService.arrayLength(response.data.charData) > 0) {
                $scope.rmchar1.Name = response.data.charData[0].Text;
                $scope.rm1characteristicsList = response.data.charData;
                $scope.rmchar1.CharacteristicsId = response.data.charData[0].Value;
            }
            if (baseService.arrayLength(response.data.charData) > 1) {
                $scope.rmchar2.Name = response.data.charData[1].Text;;
                $scope.rm2characteristicsList = response.data.charData
                $scope.rmchar2.CharacteristicsId = response.data.charData[1].Value;
            }
        });
    };

    $scope.rmchar1 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };
    $scope.rmchar2 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };
    $scope.rmchar3 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };

    $scope.char1 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };
    $scope.char2 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };
    $scope.char3 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };

    $scope.ClearDetail = function () {
        $scope.bomDetailNew = { Id: null, BOMMasterId: null, Sequence: 0, RMMaterialMasterId: null, RMArticleId: null, Description: null, CustomerSpec: null, VendorSpec: null, Consumption: 0, UoMId: null, ProcessId: null, VendorId: null, WastagePer: 0, FirstCharacteristicsId: null, SecondCharacteristicsId: null, ThirdCharacteristicsId: null, FirstCharacteristicsValueId: null, SecondCharacteristicsValueId: null, ThirdCharacteristicsValueId: null, IsSKUCommon: true, WithSKU: false, IsConsumptionDetail: false, SKUMatrix: false, Specific: true, IsDestinationSpecific: false, IsPOSpecific: false, ConsumptionSpecificToSKU1: false, ConsumptionSpecificToSKU2: false, ConsumptionSpecificToSKU3: false, SalesOrderSpecificMaterial: true };
        $scope.rmchar1 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };
        $scope.rmchar2 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };
        $scope.rmchar3 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };
        $scope.msg = null;

        $scope.showradiodiv = false;
        $scope.showradiocommon = false;
        $scope.showradiomatrix = false;
        $scope.matrixrad = false;
        $scope.getAutoSequence($scope.bomNew.Id);
        var DropDownListObj = $("#destinationList").data("ejDropDownList");
        DropDownListObj.uncheckAll();
    };

    $scope.ShowDetailPopup = function () {
        $scope.ClearDetail();
        $scope.getAutoSequence($scope.bomNew.Id);
        angular.element(document.querySelector('#detailpopup')).modal('show');
    };

    $scope.CloseDetailPopup = function () {
        angular.element(document.querySelector('#detailpopup')).modal('hide');
    };

    $scope.processList = [];
    cboService.getProductionProcessCbo(function (response) {
        $scope.processList = response;
    });

    // #region Raw Material

    $scope.businessProcesses = "BOM";
    $scope.materialType = null;

    // #region Material Search By Business Process

    $scope.uOMList = [];
    function UomCboByMaterialMaster(materilaMasterId) {
        var mmId = []; mmId.push(materilaMasterId);
        cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (response) {
            $scope.uOMList = response;
            if (baseService.arrayLength($scope.uOMList) == 1) {
                $scope.bomDetailNew.UoMId = $scope.uOMList[0].Value;
            }
            $http({
                method: 'GET',
                url: 'OrderManagements/BOMMaster/GetValueAssignmentLevel?MaterialMasterId=' + $scope.bomDetailNew.RMMaterialMasterId
            }).then(function successCallback(response) {
                if (baseService.arrayLength(response.data) > 0) {
                    for (var i = 0; i < response.data.length; i++) {
                        if (response.data[i].Sequence == 1) {
                            $scope.bomDetailNew.C1ValueAssignmentLevel = response.data[i].ValueAssignmentLevel;
                            $scope.rmchar1.ValueAssignmentLevel = response.data[i].ValueAssignmentLevel;
                        } else {
                            $scope.bomDetailNew.C2ValueAssignmentLevel = response.data[i].ValueAssignmentLevel;
                            $scope.rmchar2.ValueAssignmentLevel = response.data[i].ValueAssignmentLevel;
                        }
                    }
                }
            });
        });
    }
    $scope.unitOfMeasurementList = [];
    function UomCboByFGMaterialMaster(materilaMasterId) {
        var mmId = []; mmId.push(materilaMasterId);
        cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (response) {
            $scope.unitOfMeasurementList = response;
            if (baseService.arrayLength($scope.unitOfMeasurementList) == 1) {
                $scope.bomNew.UnitOfMeasurementId = $scope.unitOfMeasurementList[0].Value;
            }
        });
    }

    $scope.searchList = [];
    $scope.dataPlate = [];
    $scope.searchbyMaterialMasterDatalist = [
        {
            'name': 'Material Type',
            'value': 'MaterialTypeName'
        },
        {
            'name': 'Material Group',
            'value': 'MaterialGroupMasterName'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Material',
            'value': 'UserName'
        },
        {
            'name': 'Product',
            'value': 'ProductMasterName'
        },
        {
            'name': 'Id',
            'value': 'Id'
        },
        {
            'Text': 'Base UoM',
            'Value': 'BaseUoM'
        }
    ];

    $scope.getRMaterialMasterSearchData = function () {
        $scope.mmPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.materialTitle = 'Material';
        CloseShowResult();
        CloseModalShowResult();
        $scope.searchList = [];
        //$scope.popUpUrl = 'Materials/MaterialMaster/GetNonAssetMaterialList';
        $scope.popUpUrl = 'Materials/MaterialMaster/MaterialSearchByBusinessProcess?type=' + $scope.businessProcesses;
        baseService.setCurrentPage('materialmasterSearchData');
        $scope.loadMMData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.mmPopUpParameters)
                .then(function (result) {
                    $scope.materialmasterSearchData = result.Rows;
                    $scope.mmPopUpParameters.total_count = result.Total;
                    angular.element(document.querySelector('#rmaterialmastersearchpopup')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        $scope.loadMMData();
    };

    $scope.msg = null;

    $scope.setRMaterialMasterData = function (ob) {
        $scope.bomDetailNew.RMMaterialMasterId = ob.Id;
        $scope.bomDetailNew.RMMaterialMaster = ob.UserName;
        $scope.bomDetailNew.RMArticleId = null;
        $scope.bomDetailNew.RMArticle = null;
        $scope.bomDetailNew.HasAttribute = ob.HasAttribute;
        $scope.bomDetailNew.WithSKU = ob.WithSKU;

        $scope.bomDetailNew.FirstCharacteristicsValueId = null;
        $scope.bomDetailNew.SecondCharacteristicsValueId = null;
        $scope.bomDetailNew.ThirdCharacteristicsValueId = null;

        $scope.bomDetailNew.FirstCharacteristicsId = null;
        $scope.bomDetailNew.SecondCharacteristicsId = null;
        $scope.bomDetailNew.ThirdCharacteristicsId = null;

        $scope.clearCharNames();

        if ($scope.bomDetailNew.HasAttribute) {
            $scope.materialType = null;
            $scope.getRMArticleSearchList(ob.Id);
        } else {
            $scope.closeMaterialMasterbyTypePopUp();
            return ShowResult('This material has no attribute', 'failure');
        }
        if ($scope.bomDetailNew.WithSKU) $scope.getRMCharacteristicsList(ob.Id);
        if ($scope.bomDetailNew.WithSKU) {
            $scope.msg = "has";
        } else {
            $scope.msg = "has no";
        }

        $scope.HSNCodeId = ob.HSNCodeId;
        $scope.closeRMMaterialMasterbyTypePopUp();
        UomCboByMaterialMaster($scope.bomDetailNew.RMMaterialMasterId);

        $scope.bomDetailNew.Specific = true;
        $scope.bomDetailNew.SKUMatrix = false;
    };

    $scope.rm1characteristicsList = [];
    $scope.rm2characteristicsList = [];
    $scope.getRMCharacteristicsList = function (id) {
        $scope.clearCharNames();
        $http({
            method: 'GET',
            url: 'Materials/MaterialMaster/getcharacteristicsbymaterialmasterid/',
            params: {
                materialMasterId: id
            }
        }).then(function (response) {
            $scope.rmcharacteristicsList = [];
            $scope.rm1characteristicsList = [];
            $scope.rm2characteristicsList = [];
            $scope.rmcharacteristicsList = response.data.charData;
            $scope.rm1characteristicsList = response.data.charData;
            $scope.rm2characteristicsList = response.data.charData;
            if (baseService.arrayLength($scope.rmcharacteristicsList) > 0) {
                $scope.isSearch = $scope.rmcharacteristicsList[0].FreeText !== null ? true : false;
                $scope.rmchar1 = {
                    CharacteristicsId: $scope.rmcharacteristicsList[0].Value
                    , CharacteristicsValueId: $scope.rmcharacteristicsList[0].CharacteristicsValueId
                    , MaterialMasterId: $scope.rmcharacteristicsList[0].MaterialMasterId
                    , Name: $scope.rmcharacteristicsList[0].Text
                    , IsFreeField: $scope.rmcharacteristicsList[0].IsFreeField
                    , IsPreDefinedField: $scope.rmcharacteristicsList[0].IsPreDefinedField
                    , IsMandatory: $scope.rmcharacteristicsList[0].IsMandatory
                    , ValueAssignmentLevel: $scope.rmcharacteristicsList[0].ValueAssignmentLevel
                    , Sequence: $scope.rmcharacteristicsList[0].Sequence
                    , FlagDisable: $scope.IsFreeOrNot($scope.rmcharacteristicsList[0].IsFreeField)

                    , FreeText: $scope.rmcharacteristicsList[0].FreeText
                    , show: true
                };
            }
            if (baseService.arrayLength($scope.rmcharacteristicsList) > 1) {
                $scope.isSearch = $scope.rmcharacteristicsList[1].FreeText !== null ? true : false;
                $scope.rmchar2 = {
                    CharacteristicsId: $scope.rmcharacteristicsList[1].Value
                    , CharacteristicsValueId: $scope.rmcharacteristicsList[1].CharacteristicsValueId
                    , MaterialMasterId: $scope.rmcharacteristicsList[1].MaterialMasterId
                    , Name: $scope.rmcharacteristicsList[1].Text
                    , IsFreeField: $scope.rmcharacteristicsList[1].IsFreeField
                    , IsPreDefinedField: $scope.rmcharacteristicsList[1].IsPreDefinedField
                    , IsMandatory: $scope.rmcharacteristicsList[1].IsMandatory
                    , ValueAssignmentLevel: $scope.rmcharacteristicsList[1].ValueAssignmentLevel
                    , Sequence: $scope.rmcharacteristicsList[1].Sequence
                    , FlagDisable: $scope.IsFreeOrNot($scope.rmcharacteristicsList[1].IsFreeField)
                    , FreeText: $scope.rmcharacteristicsList[1].FreeText
                    , show: true
                };
            }
            if (baseService.arrayLength($scope.rmcharacteristicsList) > 2) {
                $scope.isSearch = $scope.rmcharacteristicsList[2].FreeText !== null ? true : false;
                $scope.rmchar3 = {
                    CharacteristicsId: $scope.rmcharacteristicsList[2].Value
                    , CharacteristicsValueId: $scope.rmcharacteristicsList[2].CharacteristicsValueId
                    , MaterialMasterId: $scope.rmcharacteristicsList[2].MaterialMasterId
                    , Name: $scope.rmcharacteristicsList[2].Text
                    , IsFreeField: $scope.rmcharacteristicsList[2].IsFreeField
                    , IsPreDefinedField: $scope.rmcharacteristicsList[2].IsPreDefinedField
                    , IsMandatory: $scope.rmcharacteristicsList[2].IsMandatory
                    , ValueAssignmentLevel: $scope.rmcharacteristicsList[2].ValueAssignmentLevel
                    , Sequence: $scope.rmcharacteristicsList[2].Sequence
                    , FlagDisable: $scope.IsFreeOrNot($scope.rmcharacteristicsList[2].IsFreeField)
                    , FreeText: $scope.rmcharacteristicsList[2].FreeText
                    , show: true
                };
            }
        });
    };


    $scope.get1RMCharacteristicsList = function (id) {

        $http({
            method: 'GET',
            url: 'Materials/MaterialMaster/getcharacteristicsbymaterialmasterid/',
            params: {
                materialMasterId: id
            }
        }).then(function (response) {
            $scope.GetBOMSKU1MappingListBySKU($scope.bomDetailNew.Id, $scope.BOMSKUMapping.FGFirstCharacteristicsId);
            $scope.rm1characteristicsList = [];
            //$scope.rm1characteristicsList = response.data.charData;
            angular.copy(response.data.charData, $scope.rm1characteristicsList);


            $scope.RMFirstCharacteristicsId = $scope.BOMSKUMapping.RMFirstCharacteristicsId;
            $scope.RMSecondCharacteristicsId = $scope.BOMSKUMapping.RMSecondCharacteristicsId;

            if (baseService.arrayLength($scope.rm2characteristicsList) > 0) {
                for (var i = 0; i < $scope.rm2characteristicsList.length; i++) {
                    if ($scope.rm1characteristicsList[i].Value == $scope.BOMSKUMapping.RMSecondCharacteristicsId) {
                        $scope.rm1characteristicsList.splice(i, 1);
                        $scope.BOMSKUMapping.RMFirstCharacteristicsId = $scope.RMFirstCharacteristicsId;
                        $scope.BOMSKUMapping.RMSecondCharacteristicsId = $scope.RMSecondCharacteristicsId;
                    }
                }
            }

            $scope.BOMSKUMapping.RMFirstCharacteristicsId = $scope.RMFirstCharacteristicsId;
            $scope.BOMSKUMapping.RMSecondCharacteristicsId = $scope.RMSecondCharacteristicsId;
            if (!baseService.isUndefinedOrNull($scope.BOMSKUMapping.RMFirstCharacteristicsId)) {
                $scope.GetRMCharacteristicsValueCbo($scope.bomDetailNew.RMMaterialMasterId, $scope.BOMSKUMapping.RMFirstCharacteristicsId, $scope.rmchar1.ValueAssignmentLevel);
            }
            if (!baseService.isUndefinedOrNull($scope.BOMSKUMapping.RMSecondCharacteristicsId)) {
                $scope.GetRMSKU2CharacteristicsValueCbo($scope.bomDetailNew.RMMaterialMasterId, $scope.BOMSKUMapping.RMSecondCharacteristicsId, $scope.rmchar1.ValueAssignmentLevel);
            }
        });
    };

    $scope.get2RMCharacteristicsList = function (id) {

        $http({
            method: 'GET',
            url: 'Materials/MaterialMaster/getcharacteristicsbymaterialmasterid/',
            params: {
                materialMasterId: id
            }
        }).then(function (response) {
            $scope.GetBOMSKU2MappingListBySKU($scope.bomDetailNew.Id, $scope.BOMSKUMapping.FGSecondCharacteristicsId);
            $scope.rm2characteristicsList = [];
            //$scope.rm2characteristicsList = response.data.charData;

            angular.copy(response.data.charData, $scope.rm2characteristicsList);

            $scope.RMFirstCharacteristicsId = $scope.BOMSKUMapping.RMFirstCharacteristicsId;
            $scope.RMSecondCharacteristicsId = $scope.BOMSKUMapping.RMSecondCharacteristicsId;

            if (baseService.arrayLength($scope.rm2characteristicsList) > 0) {
                for (var i = 0; i < $scope.rm2characteristicsList.length; i++) {
                    if ($scope.rm2characteristicsList[i].Value == $scope.BOMSKUMapping.RMFirstCharacteristicsId) {
                        $scope.rm2characteristicsList.splice(i, 1);
                    }
                }
            }

            $scope.BOMSKUMapping.RMFirstCharacteristicsId = $scope.RMFirstCharacteristicsId;
            $scope.BOMSKUMapping.RMSecondCharacteristicsId = $scope.RMSecondCharacteristicsId;
            if (!baseService.isUndefinedOrNull($scope.BOMSKUMapping.RMFirstCharacteristicsId)) {
                $scope.GetRMCharacteristicsValueCbo($scope.bomDetailNew.RMMaterialMasterId, $scope.BOMSKUMapping.RMFirstCharacteristicsId, $scope.rmchar1.ValueAssignmentLevel);
            }
            if (!baseService.isUndefinedOrNull($scope.BOMSKUMapping.RMSecondCharacteristicsId)) {
                $scope.GetRMSKU2CharacteristicsValueCbo($scope.bomDetailNew.RMMaterialMasterId, $scope.BOMSKUMapping.RMSecondCharacteristicsId, $scope.rmchar1.ValueAssignmentLevel);
            }
        });
    };

    $scope.Sequence = 0;
    $scope.RMSequence = 0;
    $scope.RMName = null;
    $scope.RM2Name = null;
    $scope.RMCharacteristicsValueList = [];
    $scope.GetRMCharacteristicsValueCbo = function (RMMaterialMasterId, CharacteristicsId) {
        for (var i = 0; i < $scope.rm1characteristicsList.length; i++) {
            if ($scope.BOMSKUMapping.RMFirstCharacteristicsId === $scope.rm1characteristicsList[i].Value) {
                $scope.Sequence = $scope.rm1characteristicsList[i].Sequence;
                $scope.ValueAssignmentLevel = $scope.rm1characteristicsList[i].ValueAssignmentLevel;
            }
        }

        cboService.getCharacteristicsValueCboByCharacteristicsId(RMMaterialMasterId, CharacteristicsId, $scope.ValueAssignmentLevel, function (response) {
            $scope.RMCharacteristicsValueList = response;
            $scope.RMName = $("#RMFirstCharacteristics option:selected").text();
        });


    };

    $scope.RMSKU2CharacteristicsValueList = [];
    $scope.GetRMSKU2CharacteristicsValueCbo = function (RMMaterialMasterId, CharacteristicsId) {

        for (var i = 0; i < $scope.rm2characteristicsList.length; i++) {
            if ($scope.BOMSKUMapping.RMSecondCharacteristicsId === $scope.rm2characteristicsList[i].Value) {
                $scope.RMSequence = $scope.rm2characteristicsList[i].Sequence;
                $scope.ValueAssignmentLevel = $scope.rm2characteristicsList[i].ValueAssignmentLevel;
            }
        }

        cboService.getCharacteristicsValueCboByCharacteristicsId(RMMaterialMasterId, CharacteristicsId, $scope.ValueAssignmentLevel, function (response) {
            $scope.RMSKU2CharacteristicsValueList = response;
            $scope.RM2Name = $("#RMCharacteristics option:selected").text();
        });


    };


    $scope.setCharData = function (data) {
        $scope[$scope.charValueSearchFor].CharacteristicsValueId = data.CharacteristicsValueId;
        $scope[$scope.charValueSearchFor].FreeText = data.UserName;
        $scope[$scope.charValueSearchFor].FlagDisable = $scope.isSearch;
        angular.element(document.querySelector('#searchcharactervaluepopup')).modal('hide');
    };

    $scope.closeRMMaterialMasterbyTypePopUp = function () {
        CloseModalShowResult('rmaterialmastersearchpopup');
        angular.element(document.querySelector('#rmaterialmastersearchpopup')).modal('hide');
        $scope.ShowHide();
    };

    $scope.getRMArticle = function (index) {
        //$scope.itemIndex = index;
        //if (!baseService.isUndefinedOrNull($scope.bomDetailNew.RMMaterialMasterId) && !$scope.bomNew.HasAttribute)
        //    return ShowResult('This material has no attribute', 'failure');
        $scope.getRMArticleSearchList($scope.bomDetailNew.RMMaterialMasterId);
    };

    $scope.selectRMarticle = function (ob) {
        try {
            $scope.bomDetailNew.RMMaterialMasterId = ob.MaterialMasterId;
            $scope.bomDetailNew.RMMaterialMaster = ob.MaterialMasterName;
            $scope.bomDetailNew.RMArticleId = ob.Id;
            $scope.bomDetailNew.RMArticle = ob.StandardName;
            angular.element(document.querySelector('#rarticleSearchPop')).modal('hide');
        } catch (e) {
            ShowResult(e, '', 'rarticleSearchPop');
        }
    };

    $scope.showradiodiv = false;
    $scope.showradiocommon = false;
    $scope.showradiomatrix = false;

    $scope.ShowHide = function () {
        if ($scope.bomDetailNew.WithSKU === true && $scope.bomNew.WithSKU) {
            $scope.showradiodiv = true;
            $scope.showradiocommon = true;
            $scope.showradiomatrix = true;
            $scope.matrixrad = true;
        }
        else if ($scope.bomDetailNew.WithSKU === true && $scope.bomNew.WithSKU === false) {
            $scope.showradiodiv = false;
            $scope.showradiocommon = true;
            $scope.showradiomatrix = false;
            $scope.matrixrad = true;
        }
        else {
            $scope.showradiodiv = false;
            $scope.showradiomatrix = false;
            $scope.showradiocommon = false;
            $scope.matrixrad = true;
        }
    }

    $scope.matrixrad = true;
    $scope.MatrixClick = function () {
        $scope.matrixrad = false;
        $scope.bomDetailNew.Specific = false;
        $scope.bomDetailNew.SKUMatrix = true;
        $scope.bomDetailNew.IsSKUCommon = false;

        $scope.bomDetailNew.FirstCharacteristicsId = null;
        $scope.bomDetailNew.SecondCharacteristicsId = null;
        $scope.bomDetailNew.ThirdCharacteristicsId = null;

        $scope.bomDetailNew.FirstCharacteristicsValueId = null;
        $scope.bomDetailNew.SecondCharacteristicsValueId = null;
        $scope.bomDetailNew.ThirdCharacteristicsValueId = null;

    }

    $scope.SpecificClick = function () {
        $scope.matrixrad = true;
        $scope.bomDetailNew.Specific = true;
        $scope.bomDetailNew.SKUMatrix = false;
    }

    $scope.ShowBOMSKUMappingPopup = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.bomDetailNew.Id)) {
                throw 'First add Raw Material.';
            }
            $scope.getFGCharacteristicsList($scope.bomNew.FGMaterialMasterId);
            //$scope.GetFGCharacteristicsValueCbo($scope.BOMSKUMapping.FGFirstCharacteristicsId, $scope.BOMSKUMapping.ValueAssignmentLevel);
            //$scope.Get2FGCharacteristicsValueCbo($scope.BOMSKUMapping.FGSecondCharacteristicsId, $scope.BOMSKUMapping.ValueAssignmentLevel2);
            $scope.GetBOMSKU1MappingListBySKU($scope.bomDetailNew.Id, $scope.BOMSKUMapping.FGFirstCharacteristicsId);
            $scope.GetBOMSKU2MappingListBySKU($scope.bomDetailNew.Id, $scope.BOMSKUMapping.FGSecondCharacteristicsId);

            angular.element(document.querySelector('#matrixPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure', 'detailpopup');
        }
    }

    // #endregion Material Search By Business Process

    // #region Material Article Search

    $scope.getRMArticleSearchList = function (id) {
        try {
            CloseShowResult();
            CloseModalShowResult();
            $scope.articlePopUpParameters = {
                limit: 10
                , offset: 0
                , order: 'asc'
                , sort: 'StandardName'
                , searchBy: "StandardName"
                , pageSize: 10
                , total_count: 0
                , search: null
                , serverPagination: true
            };
            $scope.searchList = [];
            $scope.dataPlate = [];
            //$scope.popUpUrl = 'Materials/MaterialMasterArticle/GetMaterialArticle';
            $scope.materialType = null;
            baseService.setCurrentPage('dataPlate');
            $scope.articlePopUpParameters.materialMasterId = id;
            $scope.articlePopUpParameters.materialType = JSON.stringify($scope.materialType);
            $scope.loadArticleData = function (pageno) {
                baseService.paginationBase('Materials/MaterialMasterArticle/GetMaterialArticle', pageno, $scope.articlePopUpParameters)
                    .then(function (result) {
                        //if (baseService.arrayLength(result.Rows) === 0) return ShowResult('This material has no article', 'failure');
                        $scope.dataPlate = result.Rows;
                        $scope.articlePopUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.searchList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchList);
                        if ($scope.articlePopUpParameters.total_count == 0) {
                            ShowResult("This material has no article ", 'failure');
                        }
                        else {

                            angular.element(document.querySelector('#rarticleSearchPop')).modal('show');
                        }

                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.loadArticleData();
        } catch (e) {
            ShowResult(e, '');
        }
    };
    $scope.closeMaterialArticlePopUp = function () {
        $scope.searchList = [];
        $scope.dataPlate = [];
        $scope.popUpUrl = '';
        CloseModalShowResult('rarticleSearchPop');
        angular.element(document.querySelector('#rarticleSearchPop')).modal('hide');
    };

    // #endregion Material Article Search

    // #endregion

    $scope.SaveDetail = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.detailFormNew.$valid) {

                if ($scope.bomDetailNew.Specific) {
                    $scope.bomDetailNew.IsSKUCommon = true;
                    $scope.bomDetailNew.FirstCharacteristicsId = $scope.rmchar1.CharacteristicsId;
                    $scope.bomDetailNew.FirstCharacteristicsValueId = $scope.rmchar1.CharacteristicsValueId;
                    $scope.bomDetailNew.SecondCharacteristicsId = $scope.rmchar2.CharacteristicsId;
                    $scope.bomDetailNew.SecondCharacteristicsValueId = $scope.rmchar2.CharacteristicsValueId;
                    $scope.bomDetailNew.ThirdCharacteristicsId = $scope.rmchar3.CharacteristicsId;
                    $scope.bomDetailNew.ThirdCharacteristicsValueId = $scope.rmchar3.CharacteristicsValueId;
                } else {
                    $scope.bomDetailNew.IsSKUCommon = false;
                    $scope.bomDetailNew.FirstCharacteristicsId = null;
                    $scope.bomDetailNew.FirstCharacteristicsValueId = null;
                    $scope.bomDetailNew.SecondCharacteristicsId = null;
                    $scope.bomDetailNew.SecondCharacteristicsValueId = null;
                    $scope.bomDetailNew.ThirdCharacteristicsId = null;
                    $scope.bomDetailNew.ThirdCharacteristicsValueId = null;
                }

                $scope.bomDetailNew.BOMMasterId = $scope.bomNew.Id;


                if (baseService.isUndefinedOrNull($scope.bomDetailNew.Consumption) || $scope.bomDetailNew.Consumption < 0 || $scope.bomDetailNew.Consumption === 0 || isNaN($scope.bomDetailNew.Consumption)) {
                    throw "Consumption should greater than 0.";
                }
                if (baseService.isUndefinedOrNull($scope.bomDetailNew.UoMId)) {
                    throw "Consumption UoM is required.";
                }
                if (baseService.isUndefinedOrNull($scope.bomDetailNew.WastagePer) || isNaN($scope.bomDetailNew.WastagePer)) {
                    throw "Wastage Percentage should greater than 0.";
                }

                var DropDownListObj = $("#destinationList").data("ejDropDownList");
                var dayStatus = DropDownListObj.getSelectedValue();
                $scope.DestinationId = dayStatus;

                if ($scope.bomDetailNew.IsDestinationSpecific == false) {
                    $scope.DestinationId = null;
                }
                else {
                    if (baseService.isUndefinedOrNull($scope.DestinationId)) {
                        throw "Destination is required.";
                    }
                }


                if ($scope.bomDetailNew.WithSKU) {
                    if ($scope.bomDetailNew.Specific === true) {
                        if (!baseService.isUndefinedOrNull($scope.rmchar1.CharacteristicsId)) {
                            if (baseService.isUndefinedOrNull($scope.bomDetailNew.FirstCharacteristicsValueId)) {
                                throw "" + $scope.rmchar1.Name + " is required.";
                            }
                        }
                        if (!baseService.isUndefinedOrNull($scope.rmchar2.CharacteristicsId)) {
                            if (baseService.isUndefinedOrNull($scope.bomDetailNew.SecondCharacteristicsValueId)) {
                                throw "" + $scope.rmchar2.Name + " is required.";
                            }
                        }
                    }
                }


                $http({
                    method: 'POST',
                    url: 'OrderManagements/BOMMaster/CreateDetail',
                    data: { 'data': $scope.bomDetailNew, 'Destination': $scope.DestinationId },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getDetailData();
                        $scope.bomDetailNew.Id = response.data.Data.Id;
                        if ($scope.bomDetailNew.WithSKU === false) {
                            $scope.ClearDetail();
                        }
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }

            }
        } catch (e) {
            ShowResult(e, 'failure', 'detailpopup');
        }
    };

    $scope.UnCheckDestination = function () {
        if ($scope.bomDetailNew.IsDestinationSpecific === false) {
            var DropDownListObj = $("#destinationList").data("ejDropDownList");
            DropDownListObj.uncheckAll();

            //$http({
            //    method: 'POST',
            //    url: 'OrderManagements/BOMMaster/DeleteDestination?id=' + $scope.bomDetailNew.Id
            //}).then(function successCallback(response) {
            //    if (response.data.Error === true) {
            //        ShowResult(response.data.Message, 'failure');
            //    }
            //    else {
            //        ShowResult(response.data.Message, 'success');
            //    }
            //}, function () {
            //    ShowResult(commonMessage.NetworkError, 'failure');
            //}).finally(function () {
            //});

        }
    }

    $scope.message_detailconfirmation = null;
    $scope.removeBoMDetail = function (obj) {

        $scope.bomDetailNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.bomDetailNew.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.bomDetailNew.RMMaterialMaster + ' ]';
        angular.element(document.querySelector('#confirmBoMDetailPopUp')).modal('show');
    }

    $scope.DeleteBomDetail = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/BOMMaster/DeleteBomDetail?id=' + $scope.bomDetailNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getDetailData();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.ClearBOMSKUMapping = function () {
        $scope.BOMSKUMapping.Id = null;
        $scope.BOMSKUMapping.FGFirstCharacteristicsValueId = null;
        $scope.BOMSKUMapping.FGSecondCharacteristicsValueId = null;
        $scope.BOMSKUMapping.RMFirstCharacteristicsValueId = null;
        $scope.BOMSKUMapping.RMSecondCharacteristicsValueId = null;
        $scope.BOMSKUMapping.Description = null;
        $scope.BOMSKUMapping.IsFirstCharacteristicCommon = false;
        $scope.BOMSKUMapping.IsSecondCharacteristicCommon = false;
        $scope.BOMSKUMapping.IsThirdCharacteristicCommon = false;
        $scope.BOMSKUMapping.ConsumptionSpecificToSKU1 = false;
        $scope.BOMSKUMapping.ConsumptionSpecificToSKU2 = false;
        $scope.BOMSKUMapping.ConsumptionSpecificToSKU3 = false;
    }

    $scope.SaveBOMSKU1Mapping = function () {
        try {
            $scope.RMSecondChId = $scope.BOMSKUMapping.RMSecondCharacteristicsId;
            $scope.BOMSKUMapping.RMSecondCharacteristicsId = null;
            if (baseService.isUndefinedOrNull($scope.bomDetailNew.Id)) {
                throw 'First add Raw Material.';
            }
            $scope.BOMSKUMapping.BOMDetailId = $scope.bomDetailNew.Id;

            if (baseService.isUndefinedOrNull($scope.BOMSKUMapping.RMFirstCharacteristicsId)) {
                throw "RM SKU is required.";
            }

            if (!$scope.BOMSKUMapping.IsFirstCharacteristicCommon) {
                if (baseService.isUndefinedOrNull($scope.BOMSKUMapping.FGFirstCharacteristicsValueId)) {
                    throw "" + $scope.BOMSKUMapping.FGName1 + " value is required.";
                }
            } else {
                $scope.BOMSKUMapping.FGFirstCharacteristicsValueId = null;
            }

            if (!baseService.isUndefinedOrNull($scope.BOMSKUMapping.RMFirstCharacteristicsId) && baseService.isUndefinedOrNull($scope.BOMSKUMapping.RMFirstCharacteristicsValueId)) {
                throw "" + $scope.RMName + " value is required.";
            }
            //if ($scope.Sequence === 1) {
            //}
            //else {
            //    if (!baseService.isUndefinedOrNull($scope.BOMSKUMapping.RMFirstCharacteristicsId) && baseService.isUndefinedOrNull($scope.BOMSKUMapping.RMSecondCharacteristicsValueId)) {
            //        throw "" + $scope.RMName + " value is required.";
            //    }
            //}

            $http({
                method: 'POST',
                url: 'OrderManagements/BOMMaster/CreateBOMSKU1Mapping',
                data: { 'data': $scope.BOMSKUMapping, 'bomDetail': $scope.bomDetailNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure', 'matrixPopUp');
                }
                else {
                    ShowResult(response.data.Message, 'success', 'matrixPopUp');
                    $scope.GetBOMSKU1MappingListBySKU($scope.bomDetailNew.Id, $scope.BOMSKUMapping.FGFirstCharacteristicsId);
                    $scope.ClearBOMSKUMapping();
                    $scope.BOMSKUMapping.RMSecondCharacteristicsId = $scope.RMSecondChId;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'matrixPopUp');
            }

        } catch (e) {
            ShowResult(e, 'failure', 'matrixPopUp');
        }
    };

    $scope.SaveBOMSKU2Mapping = function () {
        try {
            $scope.RMFirstChId = $scope.BOMSKUMapping.RMFirstCharacteristicsId;
            $scope.BOMSKUMapping.RMFirstCharacteristicsId = null;
            if (baseService.isUndefinedOrNull($scope.bomDetailNew.Id)) {
                throw 'First add Raw Material.';
            }

            $scope.BOMSKUMapping.BOMDetailId = $scope.bomDetailNew.Id;

            if (baseService.isUndefinedOrNull($scope.BOMSKUMapping.RMSecondCharacteristicsId)) {
                throw "RM SKU is required.";
            }

            if (!$scope.BOMSKUMapping.IsSecondCharacteristicCommon) {
                if (baseService.isUndefinedOrNull($scope.BOMSKUMapping.FGSecondCharacteristicsValueId)) {
                    throw "" + $scope.BOMSKUMapping.FGName2 + " value is required.";
                }
            } else {
                $scope.BOMSKUMapping.FGSecondCharacteristicsValueId = null;
            }

            //if ($scope.RMSequence === 1) {
            //    if (!baseService.isUndefinedOrNull($scope.BOMSKUMapping.RMSecondCharacteristicsId) && baseService.isUndefinedOrNull($scope.BOMSKUMapping.RMFirstCharacteristicsValueId)) {
            //        throw "" + $scope.RM2Name + " value is required.";
            //    }
            //}
            //else {
            //}
            if (!baseService.isUndefinedOrNull($scope.BOMSKUMapping.RMSecondCharacteristicsId) && baseService.isUndefinedOrNull($scope.BOMSKUMapping.RMSecondCharacteristicsValueId)) {
                throw "" + $scope.RM2Name + " value is required.";
            }


            $http({
                method: 'POST',
                url: 'OrderManagements/BOMMaster/CreateBOMSKU2Mapping',
                data: { 'data': $scope.BOMSKUMapping, 'bomDetail': $scope.bomDetailNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure', 'matrixPopUp');
                }
                else {
                    ShowResult(response.data.Message, 'success', 'matrixPopUp');
                    $scope.GetBOMSKU2MappingListBySKU($scope.bomDetailNew.Id, $scope.BOMSKUMapping.FGSecondCharacteristicsId);
                    $scope.ClearBOMSKUMapping();
                    $scope.BOMSKUMapping.RMFirstCharacteristicsId = $scope.RMFirstChId;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'matrixPopUp');
            }

        } catch (e) {
            ShowResult(e, 'failure', 'matrixPopUp');
        }
    };

    $scope.bomSKUMappingDataList = [];
    $scope.getBOMSKUMappingDataData = function (bomDetailId) {
        $scope.bomSKUMappingDataList = [];
        $http.get("OrderManagements/BOMMaster/GetBOMSKUMappingList?bomDetailId=" + bomDetailId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.bomSKUMappingDataList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.bOMDestinationList = [];
    $scope.GetBOMDestinationData = function (bomDetailId) {
        $scope.bOMDestinationList = [];
        $http.get("OrderManagements/BOMMaster/GetBOMDestination?bomDetailId=" + bomDetailId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.bOMDestinationList = response.data;


                        var DropDownListObj = $("#destinationList").data("ejDropDownList");
                        for (var j = 0; j < $scope.bOMDestinationList.length; j++) {
                            DropDownListObj.selectItemByValue($scope.bOMDestinationList[j].DestinationId);
                        }

                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.bomSKU1MappingDataList = [];
    $scope.disablebtnSave1 = false;
    $scope.GetBOMSKU1MappingListBySKU = function (bomDetailId, characteristicsId) {
        $scope.bomSKU1MappingDataList = [];
        $http.get("OrderManagements/BOMMaster/GetBOMSKUMappingListBySKU1?bomDetailId=" + bomDetailId + '&characteristicsId=' + characteristicsId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.bomSKU1MappingDataList = response.data;
                        for (var i = 0; i < $scope.bomSKU1MappingDataList.length; i++) {
                            if ($scope.bomSKU1MappingDataList[i].IsFirstCharacteristicCommon == true) {
                                $scope.disablebtnSave1 = true;
                                break;
                            } else {
                                $scope.disablebtnSave1 = false;
                            }
                        }
                        $scope.BOMSKUMapping.RMFirstCharacteristicsId = response.data[0].RMFirstCharacteristicsId;
                        $scope.RMFirstCharacteristicsId = response.data[0].RMFirstCharacteristicsId;
                        $scope.GetRMCharacteristicsValueCbo($scope.bomDetailNew.RMMaterialMasterId, $scope.BOMSKUMapping.RMFirstCharacteristicsId, $scope.rmchar1.ValueAssignmentLevel);

                    } else {
                        $scope.disablebtnSave1 = false;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.bomSKU2MappingDataList = [];
    $scope.disablebtnSave2 = false;
    $scope.GetBOMSKU2MappingListBySKU = function (bomDetailId, characteristicsId) {
        $scope.bomSKU2MappingDataList = [];
        $http.get("OrderManagements/BOMMaster/GetBOMSKUMappingListBySKU2?bomDetailId=" + bomDetailId + '&characteristicsId=' + characteristicsId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.bomSKU2MappingDataList = response.data;
                        for (var i = 0; i < $scope.bomSKU2MappingDataList.length; i++) {
                            if ($scope.bomSKU2MappingDataList[i].IsSecondCharacteristicCommon == true) {
                                $scope.disablebtnSave2 = true;
                                break;
                            } else {
                                $scope.disablebtnSave2 = false;
                            }
                        }
                        $scope.BOMSKUMapping.RMSecondCharacteristicsId = response.data[0].RMSecondCharacteristicsId;
                        $scope.RMSecondCharacteristicsId = response.data[0].RMSecondCharacteristicsId;
                        $scope.GetRMSKU2CharacteristicsValueCbo($scope.bomDetailNew.RMMaterialMasterId, $scope.BOMSKUMapping.RMSecondCharacteristicsId, $scope.rmchar1.ValueAssignmentLevel);
                    } else {
                        $scope.disablebtnSave2 = false;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab1 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet1 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    $scope.tab3 = 1;
    $scope.setTab3 = function (newTab) {
        $scope.tab3 = newTab;
    };
    $scope.isSet3 = function (tabNum) {
        return $scope.tab3 === tabNum;
    };

    $scope.GetMatrixDetail = function (obj) {
        $scope.bomDetailId = obj.data.Id;
        $scope.getBOMSKUMappingDataData($scope.bomDetailId);

        angular.element(document.querySelector('#matrixDetailPopUp')).modal('show');
    }

    $scope.Clear = function () {
        $scope.disablebtnSave1 = false;
        $scope.disablebtnSave2 = false;
        $scope.bom = {
            Id: null, FGMaterialMasterId: null, FGArticleId: null, FGMaterialMaster: null, FGArticle: null, Description: null, WithSKU: false, ProductMasterName: null, UnitOfMeasurementId: null
        };
        $scope.bomNew = Object.assign({}, $scope.bom);
        $scope.FGmsg = null;
        $scope.ClearDetail();
        $scope.detailDataList = [];
        $scope.DetailConsumptionDataList = [];
    }

    $scope.message_confirmation = null;
    $scope.RemoveMatrix = function (data) {
        $scope.BOMSKUMapping = data;
        if (!baseService.isUndefinedOrNull($scope.BOMSKUMapping.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
    }

    $scope.DeleteMatrix = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/BOMMaster/DeleteMatrix?id=' + $scope.BOMSKUMapping.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');

                $scope.getFGCharacteristicsList($scope.bomNew.FGMaterialMasterId);
                //$scope.GetFGCharacteristicsValueCbo($scope.BOMSKUMapping.FGFirstCharacteristicsId, $scope.BOMSKUMapping.ValueAssignmentLevel);
                //$scope.Get2FGCharacteristicsValueCbo($scope.BOMSKUMapping.FGSecondCharacteristicsId, $scope.BOMSKUMapping.ValueAssignmentLevel2);
                $scope.GetBOMSKU1MappingListBySKU($scope.bomDetailNew.Id, $scope.BOMSKUMapping.FGFirstCharacteristicsId);
                $scope.GetBOMSKU2MappingListBySKU($scope.bomDetailNew.Id, $scope.BOMSKUMapping.FGSecondCharacteristicsId);
                $scope.ClearBOMSKUMapping();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };


    //#region Consumption

    $scope.GetDetailConsumptionSequence = function (BOMDetailId) {
        $http.get("OrderManagements/BOMMaster/GetDetailConsumptionSequence?BOMDetailId=" + BOMDetailId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.detailConsumption.Sequence = response.data[0].Sequence;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };

    $scope.DetailConsumptionDataList = [];

    $scope.crmchar1 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };
    $scope.crmchar2 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };
    $scope.crmchar3 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };

    $scope.ClearDetailConsumption = function () {
        $scope.detailConsumption = {
            Id: null, BOMDetailId: null, RMMaterialMasterId: null, RMArticleId: null, Description: null, CustomerSpec: null, VendorSpec: null, Consumption: 0, UoMId: null, ProcessId: null, VendorId: null, WastagePer: 0, FirstCharacteristicsId: null, SecondCharacteristicsId: null, ThirdCharacteristicsId: null, FirstCharacteristicsValueId: null, SecondCharacteristicsValueId: null, ThirdCharacteristicsValueId: null, IsSKUCommon: true, WithSKU: false, Specific: true, SKUMatrix: false
        }
        $scope.crmchar1 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };
        $scope.crmchar2 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };
        $scope.crmchar3 = { CharacteristicsId: null, CharacteristicsValueId: null, MaterialMasterId: null, Name: null, IsFreeField: null, IsPreDefinedField: null, IsMandatory: null, FreeText: null, FlagDisable: null, Sequence: null, ValueAssignmentLevel: null, show: false };
        $scope.msg = null;

        $scope.cshowradiodiv = false;
        $scope.cshowradiocommon = false;
        $scope.cshowradiomatrix = false;
        $scope.cmatrixrad = false;

    };

    $scope.GetConsumption = function (obj) {
        $scope.bomDetailNew = {
            Id: null, BOMMasterId: null, Sequence: 0, RMMaterialMasterId: null, RMArticleId: null, Description: null, CustomerSpec: null, VendorSpec: null, Consumption: 0, UoMId: null, ProcessId: null, VendorId: null, WastagePer: 0, FirstCharacteristicsId: null, SecondCharacteristicsId: null, ThirdCharacteristicsId: null, FirstCharacteristicsValueId: null, SecondCharacteristicsValueId: null, ThirdCharacteristicsValueId: null, IsSKUCommon: true, WithSKU: false, IsConsumptionDetail: false, Specific: true, SKUMatrix: false, IsDestinationSpecific: false, IsPOSpecific: false, ConsumptionSpecificToSKU1: false, ConsumptionSpecificToSKU2: false, ConsumptionSpecificToSKU3: false, SalesOrderSpecificMaterial: true
        }
        $scope.ClearDetailConsumption();
        $scope.bomDetailNew = obj.data;
        $scope.GetDetailConsumptionSequence($scope.bomDetailNew.Id);
        if ($scope.bomDetailNew.IsConsumptionDetail != false) {
            $scope.DetailConsumptionDataList = [];
            $scope.getDCCharacteristicsList($scope.bomDetailNew.RMMaterialMasterId);
            //$scope.getDCCharacteristicsList($scope.detailConsumption.RMMaterialMasterId);
            $scope.getDetailConsumptionData();

            angular.element(document.querySelector('#consumptionpopup')).modal('show');
        }
    }

    $scope.getDCCharacteristicsList = function (id) {
        $scope.clearCharNames();
        $http({
            method: 'GET',
            url: 'Materials/MaterialMaster/getcharacteristicsbymaterialmasterid/',
            params: {
                materialMasterId: id
            }
        }).then(function (response) {
            $scope.characteristicsList = [];
            $scope.characteristicsList = response.data.charData;
            if (baseService.arrayLength($scope.characteristicsList) > 0) {
                $scope.isSearch = $scope.characteristicsList[0].FreeText !== null ? true : false;
                $scope.char1 = {
                    CharacteristicsId: $scope.characteristicsList[0].Value
                    , CharacteristicsValueId: $scope.characteristicsList[0].CharacteristicsValueId
                    , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                    , Name: $scope.characteristicsList[0].Text
                    , IsFreeField: $scope.characteristicsList[0].IsFreeField
                    , IsPreDefinedField: $scope.characteristicsList[0].IsPreDefinedField
                    , IsMandatory: $scope.characteristicsList[0].IsMandatory
                    , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                    , Sequence: $scope.characteristicsList[0].Sequence
                    , FlagDisable: $scope.IsFreeOrNot($scope.characteristicsList[0].IsFreeField)

                    , FreeText: $scope.characteristicsList[0].FreeText
                    , show: true
                };
                $scope.DetailConsumptionSKUMapping.RMFirstCharacteristicsId = $scope.char1.CharacteristicsId;
                $scope.DetailConsumptionSKUMapping.ValueAssignmentLevel = $scope.char1.ValueAssignmentLevel;
                $scope.DetailConsumptionSKUMapping.RMName1 = $scope.char1.Name !== null ? $scope.char1.Name : 'N/A';

                $scope.GetDCCharacteristicsValueCbo($scope.DetailConsumptionSKUMapping.RMFirstCharacteristicsId, $scope.DetailConsumptionSKUMapping.ValueAssignmentLevel);
            }
            if (baseService.arrayLength($scope.characteristicsList) > 1) {
                $scope.isSearch = $scope.characteristicsList[1].FreeText !== null ? true : false;
                $scope.char2 = {
                    CharacteristicsId: $scope.characteristicsList[1].Value
                    , CharacteristicsValueId: $scope.characteristicsList[1].CharacteristicsValueId
                    , MaterialMasterId: $scope.characteristicsList[1].MaterialMasterId
                    , Name: $scope.characteristicsList[1].Text
                    , IsFreeField: $scope.characteristicsList[1].IsFreeField
                    , IsPreDefinedField: $scope.characteristicsList[1].IsPreDefinedField
                    , IsMandatory: $scope.characteristicsList[1].IsMandatory
                    , ValueAssignmentLevel: $scope.characteristicsList[1].ValueAssignmentLevel
                    , Sequence: $scope.characteristicsList[1].Sequence
                    , FlagDisable: $scope.IsFreeOrNot($scope.characteristicsList[1].IsFreeField)
                    , FreeText: $scope.characteristicsList[1].FreeText
                    , show: true
                };
                $scope.DetailConsumptionSKUMapping.RMSecondCharacteristicsId = $scope.char2.CharacteristicsId;
                $scope.DetailConsumptionSKUMapping.RMName2 = $scope.char2.Name !== null ? $scope.char2.Name : 'N/A';
                $scope.DetailConsumptionSKUMapping.ValueAssignmentLevel2 = $scope.char2.ValueAssignmentLevel;
                $scope.GetDCCharacteristicsValue2Cbo($scope.DetailConsumptionSKUMapping.RMSecondCharacteristicsId, $scope.DetailConsumptionSKUMapping.ValueAssignmentLevel2);

            }
            if (baseService.arrayLength($scope.characteristicsList) > 2) {
                $scope.isSearch = $scope.characteristicsList[2].FreeText !== null ? true : false;
                $scope.char3 = {
                    CharacteristicsId: $scope.characteristicsList[2].Value
                    , CharacteristicsValueId: $scope.characteristicsList[2].CharacteristicsValueId
                    , MaterialMasterId: $scope.characteristicsList[2].MaterialMasterId
                    , Name: $scope.characteristicsList[2].Text
                    , IsFreeField: $scope.characteristicsList[2].IsFreeField
                    , IsPreDefinedField: $scope.characteristicsList[2].IsPreDefinedField
                    , IsMandatory: $scope.characteristicsList[2].IsMandatory
                    , ValueAssignmentLevel: $scope.characteristicsList[2].ValueAssignmentLevel
                    , Sequence: $scope.characteristicsList[2].Sequence
                    , FlagDisable: $scope.IsFreeOrNot($scope.characteristicsList[2].IsFreeField)
                    , FreeText: $scope.characteristicsList[2].FreeText
                    , show: true
                };
            }
            $scope.DetailConsumptionSKUMapping.RMThirdCharacteristicsId = $scope.char3.CharacteristicsId;
            $scope.DetailConsumptionSKUMapping.RMName3 = $scope.char3.Name !== null ? $scope.char3.Name : 'N/A';
            $scope.DetailConsumptionSKUMapping.ValueAssignmentLevel3 = $scope.char3.ValueAssignmentLevel;

        });

    };

    // #region Raw Material

    $scope.businessProcesses = "BOM";
    $scope.materialType = null;

    // #region Material Search By Business Process

    $scope.cuOMList = [];
    function ConsumptionUomCboByMaterialMaster(materilaMasterId) {
        var mmId = []; mmId.push(materilaMasterId);
        cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (response) {
            $scope.cuOMList = response;
            if (baseService.arrayLength($scope.cuOMList) == 1) {
                $scope.detailConsumption.UoMId = $scope.cuOMList[0].Value;
            }
        });
    }

    $scope.searchList = [];
    $scope.dataPlate = [];
    $scope.searchbyMaterialMasterDatalist = [
        {
            'name': 'Material Type',
            'value': 'MaterialTypeName'
        },
        {
            'name': 'Material Group',
            'value': 'MaterialGroupMasterName'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Material',
            'value': 'UserName'
        },
        {
            'name': 'Product',
            'value': 'ProductMasterName'
        },
        {
            'name': 'Id',
            'value': 'Id'
        }
    ];

    $scope.getCRMaterialMasterSearchData = function () {
        $scope.mmPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.materialTitle = 'Material';
        CloseShowResult();
        CloseModalShowResult();
        $scope.searchList = [];
        //$scope.popUpUrl = 'Materials/MaterialMaster/GetNonAssetMaterialList';
        $scope.popUpUrl = 'Materials/MaterialMaster/MaterialSearchByBusinessProcess?type=' + $scope.businessProcesses;
        baseService.setCurrentPage('materialmasterSearchData');
        $scope.loadMMData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.mmPopUpParameters)
                .then(function (result) {
                    $scope.materialmasterSearchData = result.Rows;
                    $scope.mmPopUpParameters.total_count = result.Total;
                    angular.element(document.querySelector('#crmaterialmastersearchpopup')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        $scope.loadMMData();
    };

    $scope.msg = null;

    $scope.setCRMaterialMasterData = function (ob) {
        $scope.detailConsumption.RMMaterialMasterId = ob.Id;
        $scope.detailConsumption.RMMaterialMaster = ob.UserName;
        $scope.detailConsumption.RMArticleId = null;
        $scope.detailConsumption.RMArticle = null;
        $scope.detailConsumption.HasAttribute = ob.HasAttribute;
        $scope.detailConsumption.WithSKU = ob.WithSKU;

        $scope.detailConsumption.FirstCharacteristicsValueId = null;
        $scope.detailConsumption.SecondCharacteristicsValueId = null;
        $scope.detailConsumption.ThirdCharacteristicsValueId = null;

        $scope.detailConsumption.FirstCharacteristicsId = null;
        $scope.detailConsumption.SecondCharacteristicsId = null;
        $scope.detailConsumption.ThirdCharacteristicsId = null;

        $scope.clearCharNames();

        if ($scope.detailConsumption.HasAttribute) {
            $scope.materialType = null;
            $scope.getCRMArticleSearchList(ob.Id);
        } else {
            $scope.closeCMaterialMasterbyTypePopUp();
            // return ShowResult('This material has no attribute', 'failure');
        }
        if ($scope.detailConsumption.WithSKU) $scope.getCRMCharacteristicsList(ob.Id);
        if ($scope.detailConsumption.WithSKU) {
            $scope.msg = "has";
        } else {
            $scope.msg = "has no";
        }

        $scope.HSNCodeId = ob.HSNCodeId;
        $scope.closeCRMMaterialMasterbyTypePopUp();
        ConsumptionUomCboByMaterialMaster($scope.detailConsumption.RMMaterialMasterId);

        $scope.detailConsumption.Specific = true;
    };
    $scope.closeCMaterialMasterbyTypePopUp = function () {
        CloseModalShowResult('crmaterialmastersearchpopup');
        angular.element(document.querySelector('#crmaterialmastersearchpopup')).modal('hide');
    };
    $scope.crmcharacteristicsList = [];
    $scope.getCRMCharacteristicsList = function (id) {
        $scope.clearCharNames();
        $http({
            method: 'GET',
            url: 'Materials/MaterialMaster/getcharacteristicsbymaterialmasterid/',
            params: {
                materialMasterId: id
            }
        }).then(function (response) {
            $scope.crmcharacteristicsList = [];
            $scope.crmcharacteristicsList = response.data.charData;
            console.log('$scope.crmcharacteristicsList', $scope.crmcharacteristicsList);
            if (baseService.arrayLength($scope.crmcharacteristicsList) > 0) {
                $scope.isSearch = $scope.crmcharacteristicsList[0].FreeText !== null ? true : false;
                $scope.crmchar1 = {
                    CharacteristicsId: $scope.crmcharacteristicsList[0].Value
                    , CharacteristicsValueId: $scope.crmcharacteristicsList[0].CharacteristicsValueId
                    , MaterialMasterId: $scope.crmcharacteristicsList[0].MaterialMasterId
                    , Name: $scope.crmcharacteristicsList[0].Text
                    , IsFreeField: $scope.crmcharacteristicsList[0].IsFreeField
                    , IsPreDefinedField: $scope.crmcharacteristicsList[0].IsPreDefinedField
                    , IsMandatory: $scope.crmcharacteristicsList[0].IsMandatory
                    , ValueAssignmentLevel: $scope.crmcharacteristicsList[0].ValueAssignmentLevel
                    , Sequence: $scope.crmcharacteristicsList[0].Sequence
                    , FlagDisable: $scope.IsFreeOrNot($scope.crmcharacteristicsList[0].IsFreeField)

                    , FreeText: $scope.crmcharacteristicsList[0].FreeText
                    , show: true
                };



            }
            if (baseService.arrayLength($scope.crmcharacteristicsList) > 1) {
                $scope.isSearch = $scope.crmcharacteristicsList[1].FreeText !== null ? true : false;
                $scope.crmchar2 = {
                    CharacteristicsId: $scope.crmcharacteristicsList[1].Value
                    , CharacteristicsValueId: $scope.crmcharacteristicsList[1].CharacteristicsValueId
                    , MaterialMasterId: $scope.crmcharacteristicsList[1].MaterialMasterId
                    , Name: $scope.crmcharacteristicsList[1].Text
                    , IsFreeField: $scope.crmcharacteristicsList[1].IsFreeField
                    , IsPreDefinedField: $scope.crmcharacteristicsList[1].IsPreDefinedField
                    , IsMandatory: $scope.crmcharacteristicsList[1].IsMandatory
                    , ValueAssignmentLevel: $scope.crmcharacteristicsList[1].ValueAssignmentLevel
                    , Sequence: $scope.crmcharacteristicsList[1].Sequence
                    , FlagDisable: $scope.IsFreeOrNot($scope.crmcharacteristicsList[1].IsFreeField)
                    , FreeText: $scope.crmcharacteristicsList[1].FreeText
                    , show: true
                };



            }
            if (baseService.arrayLength($scope.crmcharacteristicsList) > 2) {
                $scope.isSearch = $scope.crmcharacteristicsList[2].FreeText !== null ? true : false;
                $scope.crmchar3 = {
                    CharacteristicsId: $scope.crmcharacteristicsList[2].Value
                    , CharacteristicsValueId: $scope.crmcharacteristicsList[2].CharacteristicsValueId
                    , MaterialMasterId: $scope.crmcharacteristicsList[2].MaterialMasterId
                    , Name: $scope.crmcharacteristicsList[2].Text
                    , IsFreeField: $scope.crmcharacteristicsList[2].IsFreeField
                    , IsPreDefinedField: $scope.crmcharacteristicsList[2].IsPreDefinedField
                    , IsMandatory: $scope.crmcharacteristicsList[2].IsMandatory
                    , ValueAssignmentLevel: $scope.crmcharacteristicsList[2].ValueAssignmentLevel
                    , Sequence: $scope.crmcharacteristicsList[2].Sequence
                    , FlagDisable: $scope.IsFreeOrNot($scope.crmcharacteristicsList[2].IsFreeField)
                    , FreeText: $scope.crmcharacteristicsList[2].FreeText
                    , show: true
                };



            }
        });
    };

    $scope.CSequence = 0;
    $scope.CRMSequence = 0;
    $scope.CRMName = null;
    $scope.CRM2Name = null;
    $scope.CRMCharacteristicsValueList = [];
    $scope.GetCRMCharacteristicsValueCbo = function (CharacteristicsId) {
        cboService.getCharacteristicsValueCboByCharacteristicsId(CharacteristicsId, $scope.crmchar1.ValueAssignmentLevel, function (response) {
            $scope.CRMCharacteristicsValueList = response;
            $scope.CRMName = $("#CRMFirstCharacteristics option:selected").text();
        });

        $scope.CSequence = $.grep($scope.crmcharacteristicsList, function (item) {
            return item.Value === $scope.DetailConsumptionSKUMapping.SubFirstCharacteristicsId;
        })[0].Sequence;

    };

    $scope.CRMSKU2CharacteristicsValueList = [];
    $scope.GetCRMSKU2CharacteristicsValueCbo = function (CharacteristicsId) {
        cboService.getCharacteristicsValueCboByCharacteristicsId(CharacteristicsId, $scope.crmchar1.ValueAssignmentLevel, function (response) {
            $scope.CRMSKU2CharacteristicsValueList = response;
            $scope.CRM2Name = $("#CRMCharacteristics option:selected").text();
        });

        $scope.CRMSequence = $.grep($scope.crmcharacteristicsList, function (item) {
            return item.Value === $scope.DetailConsumptionSKUMapping.SubSecondCharacteristicsId;
        })[0].Sequence;

    };

    $scope.setCharData = function (data) {
        $scope[$scope.charValueSearchFor].CharacteristicsValueId = data.CharacteristicsValueId;
        $scope[$scope.charValueSearchFor].FreeText = data.UserName;
        $scope[$scope.charValueSearchFor].FlagDisable = $scope.isSearch;
        angular.element(document.querySelector('#searchcharactervaluepopup')).modal('hide');
    };

    $scope.closeCRMMaterialMasterbyTypePopUp = function () {
        CloseModalShowResult('crmaterialmastersearchpopup');
        angular.element(document.querySelector('#crmaterialmastersearchpopup')).modal('hide');
        $scope.CShowHide();
    };

    $scope.getCRMArticle = function (index) {
        $scope.itemIndex = index;

        $scope.getCRMArticleSearchList($scope.detailConsumption.RMMaterialMasterId);
    };

    $scope.selectCRMarticle = function (ob) {
        try {
            $scope.detailConsumption.RMMaterialMasterId = ob.MaterialMasterId;
            $scope.detailConsumption.RMMaterialMaster = ob.MaterialMasterName;
            $scope.detailConsumption.RMArticleId = ob.Id;
            $scope.detailConsumption.RMArticle = ob.StandardName;
            angular.element(document.querySelector('#crarticleSearchPop')).modal('hide');
        } catch (e) {
            ShowResult(e, '', 'crarticleSearchPop');
        }
    };

    $scope.cshowradiodiv = false;
    $scope.cshowradiocommon = false;
    $scope.cshowradiomatrix = false;

    $scope.CShowHide = function () {
        if ($scope.detailConsumption.WithSKU === true && $scope.bomDetailNew.WithSKU) {
            $scope.cshowradiodiv = true;
            $scope.cshowradiocommon = true;
            $scope.cshowradiomatrix = true;
            $scope.cmatrixrad = true;
        }
        else if ($scope.detailConsumption.WithSKU === true && $scope.bomDetailNew.WithSKU === false) {
            $scope.cshowradiodiv = false;
            $scope.cshowradiocommon = true;
            $scope.cshowradiomatrix = false;
            $scope.cmatrixrad = true;
        }
        else {
            $scope.cshowradiodiv = false;
            $scope.cshowradiomatrix = false;
            $scope.cshowradiocommon = false;
            $scope.cmatrixrad = true;
        }
    }

    $scope.cmatrixrad = true;
    $scope.CMatrixClick = function () {
        $scope.cmatrixrad = false;
        $scope.detailConsumption.Specific = false;
        $scope.detailConsumption.SKUMatrix = true;
        $scope.detailConsumption.IsSKUCommon = false;

        $scope.detailConsumption.FirstCharacteristicsId = null;
        $scope.detailConsumption.SecondCharacteristicsId = null;
        $scope.detailConsumption.ThirdCharacteristicsId = null;

        $scope.detailConsumption.FirstCharacteristicsValueId = null;
        $scope.detailConsumption.SecondCharacteristicsValueId = null;
        $scope.detailConsumption.ThirdCharacteristicsValueId = null;
    }

    $scope.CSpecificClick = function () {
        $scope.cmatrixrad = true;
        $scope.detailConsumption.Specific = true;
        $scope.detailConsumption.SKUMatrix = false;
    }


    // #endregion Consumption Material Search By Business Process

    // #region Consumption Material Article Search

    $scope.getCRMArticleSearchList = function (id) {
        try {
            CloseShowResult();
            CloseModalShowResult();
            $scope.articlePopUpParameters = {
                limit: 10
                , offset: 0
                , order: 'asc'
                , sort: 'StandardName'
                , searchBy: "StandardName"
                , pageSize: 10
                , total_count: 0
                , search: null
                , serverPagination: true
            };
            $scope.searchList = [];
            $scope.dataPlate = [];
            $scope.materialType = null;
            baseService.setCurrentPage('dataPlate');
            $scope.articlePopUpParameters.materialMasterId = id;
            $scope.articlePopUpParameters.materialType = JSON.stringify($scope.materialType);
            $scope.loadArticleData = function (pageno) {
                baseService.paginationBase('Materials/MaterialMasterArticle/GetMaterialArticle', pageno, $scope.articlePopUpParameters)
                    .then(function (result) {
                        //if (baseService.arrayLength(result.Rows) === 0) return ShowResult('This material has no article', 'failure');
                        $scope.dataPlate = result.Rows;
                        $scope.articlePopUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.searchList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchList);
                        if ($scope.articlePopUpParameters.total_count == 0) {
                            ShowResult("This material has no article ", 'failure');
                        }
                        else {

                            angular.element(document.querySelector('#crarticleSearchPop')).modal('show');
                        }

                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.loadArticleData();
        } catch (e) {
            ShowResult(e, '');
        }
    };
    $scope.closeCMaterialArticlePopUp = function () {
        $scope.searchList = [];
        $scope.dataPlate = [];
        $scope.popUpUrl = '';
        CloseModalShowResult('crarticleSearchPop');
        angular.element(document.querySelector('#crarticleSearchPop')).modal('hide');
    };

    // #endregion Material Article Search

    // #endregion

    //#region Vendor

    $scope.showVendorPartyPopUp = function () {
        baseService.setCurrentPage('partyList');
        $scope.getPartyList = function (pageno) {
            if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList?partyType=' + $scope.partyType;
            }
            else if ($scope.partyType === 'Party') {
                $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList';
            }
            else if ($scope.partyType === 'Director') {
                $scope.partyUrl = 'Parties/party/GetCompanyDirectorDataList';
            }
            else if ($scope.partyType === 'Other') {
                $scope.partyUrl = 'Parties/party/GetCompanyOtherDataList';
            }
            baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
                .then(function (result) {
                    $scope.partyList = result.Rows;
                    $scope.partyParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#VpartyPopUp')).modal('show');
        $scope.getPartyList();
    };

    $scope.hideVPartyPopUp = function () {
        angular.element(document.querySelector('#VpartyPopUp')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };
    $scope.closeVPartyPopUp = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedParty = id;
        angular.element(document.querySelector('#VpartyPopUp')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };

    $scope.closeVendorPartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            $scope.detailConsumption.VendorId = party.Id;
            $scope.detailConsumption.PartyCode = party.Code;
            $scope.detailConsumption.PartyName = party.UserName;
        }
        $scope.hideVPartyPopUp();
    };

    $scope.clearVendorConsumption = function () {
        $scope.detailConsumption.VendorId = null;
        $scope.detailConsumption.PartyCode = null;
        $scope.detailConsumption.PartyName = null;
    }

    //#endregion Vendor

    $scope.SaveDetailConsumption = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.detailconsumptionFormNew.$valid) {

                $scope.detailConsumption.FirstCharacteristicsId = $scope.crmchar1.CharacteristicsId;
                $scope.detailConsumption.FirstCharacteristicsValueId = $scope.crmchar1.CharacteristicsValueId;
                $scope.detailConsumption.SecondCharacteristicsId = $scope.crmchar2.CharacteristicsId;
                $scope.detailConsumption.SecondCharacteristicsValueId = $scope.crmchar2.CharacteristicsValueId;
                $scope.detailConsumption.ThirdCharacteristicsId = $scope.crmchar3.CharacteristicsId;
                $scope.detailConsumption.ThirdCharacteristicsValueId = $scope.crmchar3.CharacteristicsValueId;

                $scope.detailConsumption.BOMDetailId = $scope.bomDetailNew.Id;

                if (baseService.isUndefinedOrNull($scope.detailConsumption.Consumption) || $scope.detailConsumption.Consumption < 0 || $scope.detailConsumption.Consumption === 0 || isNaN($scope.detailConsumption.Consumption)) {
                    throw "Consumption should greater than 0.";
                }
                if (baseService.isUndefinedOrNull($scope.detailConsumption.UoMId)) {
                    throw "Consumption UoM is required.";
                }
                if (baseService.isUndefinedOrNull($scope.detailConsumption.WastagePer) || isNaN($scope.detailConsumption.WastagePer)) {
                    throw "Wastage Percentage should greater than 0.";
                }

                if ($scope.detailConsumption.WithSKU) {
                    if ($scope.detailConsumption.Specific === true) {
                        if (!baseService.isUndefinedOrNull($scope.crmchar1.CharacteristicsId)) {
                            if (baseService.isUndefinedOrNull($scope.detailConsumption.FirstCharacteristicsValueId)) {
                                throw "" + $scope.crmchar1.Name + " is required.";
                            }
                        }
                        if (!baseService.isUndefinedOrNull($scope.rmchar2.CharacteristicsId)) {
                            if (baseService.isUndefinedOrNull($scope.detailConsumption.SecondCharacteristicsValueId)) {
                                throw "" + $scope.crmchar2.Name + " is required.";
                            }
                        }
                    }
                }


                $http({
                    method: 'POST',
                    url: 'OrderManagements/BOMMaster/CreateDetailConsumption',
                    data: { 'data': $scope.detailConsumption },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getDetailConsumptionData();;
                        $scope.detailConsumption.Id = response.data.Data.Id;
                        if ($scope.detailConsumption.WithSKU === false) {
                            $scope.ClearDetailConsumption();
                        }
                        $scope.GetDetailConsumptionSequence($scope.bomDetailNew.Id);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }

            }
        } catch (e) {
            ShowResult(e, 'failure', 'consumptionpopup');
        }
    };

    $scope.getDetailConsumptionData = function () {
        $http.get("OrderManagements/BOMMaster/GetDetailConsumptionList?masterId=" + $scope.bomDetailNew.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.DetailConsumptionDataList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.GetDetailConsumption = function (obj) {
        $scope.detailConsumption = {
            Id: null, BOMDetailId: null, RMMaterialMasterId: null, RMArticleId: null, Description: null, CustomerSpec: null, VendorSpec: null, Consumption: 0, UoMId: null, ProcessId: null, VendorId: null, WastagePer: 0, FirstCharacteristicsId: null, SecondCharacteristicsId: null, ThirdCharacteristicsId: null, FirstCharacteristicsValueId: null, SecondCharacteristicsValueId: null, ThirdCharacteristicsValueId: null, IsSKUCommon: true, WithSKU: false, IsConsumptionDetail: false, Specific: true, SKUMatrix: false
        }
        $scope.detailConsumption = obj.data;

        if ($scope.detailConsumption.WithSKU) {
            $scope.msg = "has";
        } else {
            $scope.msg = "has no";
        }

        //UomCboByMaterialMaster($scope.detailConsumption.RMMaterialMasterId);

        ConsumptionUomCboByMaterialMaster($scope.detailConsumption.RMMaterialMasterId);

        if ($scope.detailConsumption.IsSKUCommon === true) {
            $scope.detailConsumption.Specific = true;
            $scope.detailConsumption.SKUMatrix = false;
        } else {
            $scope.detailConsumption.SKUMatrix = true;
            $scope.detailConsumption.Specific = false;
        }

        $scope.CShowHide();

        if ($scope.detailConsumption.Specific === false) {
            $scope.cmatrixrad = false;

        } else {
            $scope.cmatrixrad = true;
        }

        $scope.crmchar1.CharacteristicsId = $scope.detailConsumption.FirstCharacteristicsId;
        $scope.crmchar2.CharacteristicsId = $scope.detailConsumption.SecondCharacteristicsId;
        $scope.crmchar3.CharacteristicsId = $scope.detailConsumption.ThirdCharacteristicsId;

        $scope.crmchar1.CharacteristicsValueId = $scope.detailConsumption.FirstCharacteristicsValueId;
        $scope.crmchar2.CharacteristicsValueId = $scope.detailConsumption.SecondCharacteristicsValueId;
        $scope.crmchar3.CharacteristicsValueId = $scope.detailConsumption.ThirdCharacteristicsValueId;

        $scope.crmchar1.Name = $scope.detailConsumption.SKU1Name;
        $scope.crmchar2.Name = $scope.detailConsumption.SKU2Name;
        $scope.crmchar3.Name = $scope.detailConsumption.SKU3Name;

        $scope.crmchar1.FreeText = $scope.detailConsumption.SKU1;
        $scope.crmchar2.FreeText = $scope.detailConsumption.SKU2;
        $scope.crmchar3.FreeText = $scope.detailConsumption.SKU3;

        $scope.crmchar1.ValueAssignmentLevel = $scope.detailConsumption.C1ValueAssignmentLevel;
        $scope.crmchar2.ValueAssignmentLevel = $scope.detailConsumption.C2ValueAssignmentLevel;
        $scope.crmchar3.ValueAssignmentLevel = $scope.detailConsumption.C3ValueAssignmentLevel;

        $scope.crmchar1.MaterialMasterId = $scope.detailConsumption.RMMaterialMasterId;
        $scope.crmchar2.MaterialMasterId = $scope.detailConsumption.RMMaterialMasterId;
        $scope.crmchar3.MaterialMasterId = $scope.detailConsumption.RMMaterialMasterId;

        $scope.getCRMCharacteristicsCboList($scope.detailConsumption.RMMaterialMasterId);
    }

    $scope.getCRMCharacteristicsCboList = function (id) {
        $scope.clearCharNames();
        $http({
            method: 'GET',
            url: 'Materials/MaterialMaster/getcharacteristicsbymaterialmasterid/',
            params: {
                materialMasterId: id
            }
        }).then(function (response) {
            $scope.crmcharacteristicsList = [];
            $scope.crmcharacteristicsList = response.data.charData;
            console.log('$scope.crmcharacteristicsList', $scope.crmcharacteristicsList);
        });
    };

    $scope.message_detailConsumptionconfirmation = null;
    $scope.removeBoMDetailConsumption = function (obj) {

        $scope.detailConsumption = obj.data;
        if (!baseService.isUndefinedOrNull($scope.detailConsumption.Id))
            $scope.message_detailConsumptionconfirmation = 'Are you sure want to delete permanently [ ' + $scope.detailConsumption.RMMaterialMaster + ' ]';
        angular.element(document.querySelector('#confirmBoMDetailConsumptionPopUp')).modal('show');
    }

    $scope.DeleteBomDetailConsumption = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/BOMMaster/DeleteBomDetailConsumption?id=' + $scope.detailConsumption.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.ClearDetailConsumption();
                $scope.getDetailConsumptionData();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    //#endregion Consumption

    //#region DetailConsumtionSKUMapping

    $scope.getDC1RMCharacteristicsList = function (id) {

        $http({
            method: 'GET',
            url: 'Materials/MaterialMaster/getcharacteristicsbymaterialmasterid/',
            params: {
                materialMasterId: id
            }
        }).then(function (response) {
            $scope.GetDetailConsumptionSKU1MappingListBySKU($scope.detailConsumption.Id, $scope.DetailConsumptionSKUMapping.RMFirstCharacteristicsId);
            $scope.crm1characteristicsList = [];
            angular.copy(response.data.charData, $scope.crm1characteristicsList);


            $scope.SubFirstCharacteristicsId = $scope.DetailConsumptionSKUMapping.SubFirstCharacteristicsId;
            $scope.SubSecondCharacteristicsId = $scope.DetailConsumptionSKUMapping.SubSecondCharacteristicsId;

            if (baseService.arrayLength($scope.crm2characteristicsList) > 0) {
                for (var i = 0; i < $scope.rm2characteristicsList.length; i++) {
                    if ($scope.crm1characteristicsList[i].Value == $scope.DetailConsumptionSKUMapping.SubSecondCharacteristicsId) {
                        $scope.crm1characteristicsList.splice(i, 1);
                        $scope.DetailConsumptionSKUMapping.SubFirstCharacteristicsId = $scope.SubFirstCharacteristicsId;
                        $scope.DetailConsumptionSKUMapping.SubSecondCharacteristicsId = $scope.SubSecondCharacteristicsId;
                    }
                }
            }

            $scope.DetailConsumptionSKUMapping.SubFirstCharacteristicsId = $scope.SubFirstCharacteristicsId;
            $scope.DetailConsumptionSKUMapping.SubSecondCharacteristicsId = $scope.SubSecondCharacteristicsId;
            if (!baseService.isUndefinedOrNull($scope.DetailConsumptionSKUMapping.SubFirstCharacteristicsId)) {
                $scope.GetDCCharacteristicsValueCbo($scope.DetailConsumptionSKUMapping.SubFirstCharacteristicsId, $scope.crmchar1.ValueAssignmentLevel);
            }
            if (!baseService.isUndefinedOrNull($scope.DetailConsumptionSKUMapping.SubSecondCharacteristicsId)) {
                $scope.GetDCCharacteristicsValue2Cbo($scope.DetailConsumptionSKUMapping.SubSecondCharacteristicsId, $scope.crmchar1.ValueAssignmentLevel);
            }

        });
    };

    $scope.getDC2RMCharacteristicsList = function (id) {

        $http({
            method: 'GET',
            url: 'Materials/MaterialMaster/getcharacteristicsbymaterialmasterid/',
            params: {
                materialMasterId: id
            }
        }).then(function (response) {
            $scope.GetDetailConsumptionSKU2MappingListBySKU($scope.detailConsumption.Id, $scope.DetailConsumptionSKUMapping.RMSecondCharacteristicsId);
            $scope.crm2characteristicsList = [];
            //$scope.rm2characteristicsList = response.data.charData;

            angular.copy(response.data.charData, $scope.crm2characteristicsList);

            $scope.SubFirstCharacteristicsId = $scope.DetailConsumptionSKUMapping.SubFirstCharacteristicsId;
            $scope.SubSecondCharacteristicsId = $scope.DetailConsumptionSKUMapping.SubSecondCharacteristicsId;

            if (baseService.arrayLength($scope.crm2characteristicsList) > 0) {
                for (var i = 0; i < $scope.crm2characteristicsList.length; i++) {
                    if ($scope.crm2characteristicsList[i].Value == $scope.DetailConsumptionSKUMapping.SubFirstCharacteristicsId) {
                        $scope.crm2characteristicsList.splice(i, 1);
                    }
                }
            }

            $scope.DetailConsumptionSKUMapping.SubFirstCharacteristicsId = $scope.SubFirstCharacteristicsId;
            $scope.DetailConsumptionSKUMapping.SubSecondCharacteristicsId = $scope.SubSecondCharacteristicsId;
            if (!baseService.isUndefinedOrNull($scope.DetailConsumptionSKUMapping.SubFirstCharacteristicsId)) {
                $scope.GetDCCharacteristicsValueCbo($scope.DetailConsumptionSKUMapping.SubFirstCharacteristicsId, $scope.crmchar1.ValueAssignmentLevel);
            }
            if (!baseService.isUndefinedOrNull($scope.DetailConsumptionSKUMapping.SubSecondCharacteristicsId)) {
                $scope.GetDCCharacteristicsValue2Cbo($scope.DetailConsumptionSKUMapping.SubSecondCharacteristicsId, $scope.crmchar1.ValueAssignmentLevel);
            }
        });
    };

    $scope.DCCharacteristicsValueList = [];
    $scope.GetDCCharacteristicsValueCbo = function (CharacteristicsId, valueAssignmentLevel) {
        cboService.getCharacteristicsValueCboByCharacteristicsId(CharacteristicsId, valueAssignmentLevel, function (response) {
            $scope.DCCharacteristicsValueList = response;
        });
    }
    $scope.DCCharacteristicsValue2List = [];
    $scope.GetDCCharacteristicsValue2Cbo = function (CharacteristicsId, valueAssignmentLevel) {
        cboService.getCharacteristicsValueCboByCharacteristicsId(CharacteristicsId, valueAssignmentLevel, function (response) {
            $scope.DCCharacteristicsValue2List = response;
        });
    }

    $scope.disablebtnDCSave1 = false;
    $scope.ShowDetailConsumptionSKUMappingPopup = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.detailConsumption.Id)) {
                throw 'First add Detail Consumption.';
            }
            $scope.getDCCharacteristicsList($scope.detailConsumption.RMMaterialMasterId);
            //$scope.GetDCCharacteristicsValueCbo($scope.DetailConsumptionSKUMapping.RMFirstCharacteristicsId, $scope.DetailConsumptionSKUMapping.ValueAssignmentLevel);
            //$scope.GetDCCharacteristicsValue2Cbo($scope.DetailConsumptionSKUMapping.RMSecondCharacteristicsId, $scope.DetailConsumptionSKUMapping.ValueAssignmentLevel);
            $scope.GetDetailConsumptionSKU1MappingListBySKU($scope.detailConsumption.Id, $scope.DetailConsumptionSKUMapping.RMFirstCharacteristicsId);
            $scope.GetDetailConsumptionSKU2MappingListBySKU($scope.detailConsumption.Id, $scope.DetailConsumptionSKUMapping.RMSecondCharacteristicsId);

            angular.element(document.querySelector('#ConsumptionMatrixPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure', 'detailpopup');
        }
    }

    $scope.ClearDetailConsumptionSKUMapping = function () {
        $scope.DetailConsumptionSKUMapping.Id = null;
        $scope.DetailConsumptionSKUMapping.SubFirstCharacteristicsValueId = null;
        $scope.DetailConsumptionSKUMapping.SubSecondCharacteristicsValueId = null;
        $scope.DetailConsumptionSKUMapping.RMFirstCharacteristicsValueId = null;
        $scope.DetailConsumptionSKUMapping.RMSecondCharacteristicsValueId = null;
        $scope.DetailConsumptionSKUMapping.Description = null;
        $scope.DetailConsumptionSKUMapping.IsFirstCharacteristicCommon = false;
        $scope.DetailConsumptionSKUMapping.IsSecondCharacteristicCommon = false;
        $scope.DetailConsumptionSKUMapping.IsThirdCharacteristicCommon = false;

    }

    $scope.SaveDetailConsumptionSKU1Mapping = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.detailConsumption.Id)) {
                throw 'First add Detail Consumption.';
            }
            $scope.DetailConsumptionSKUMapping.DetailConsumptionId = $scope.detailConsumption.Id;

            if (baseService.isUndefinedOrNull($scope.DetailConsumptionSKUMapping.SubFirstCharacteristicsId)) {
                throw "RM SKU is required.";
            }

            if (!$scope.DetailConsumptionSKUMapping.IsFirstCharacteristicCommon) {
                if (baseService.isUndefinedOrNull($scope.DetailConsumptionSKUMapping.RMFirstCharacteristicsValueId)) {
                    throw "" + $scope.DetailConsumptionSKUMapping.RMName1 + " value is required.";
                }
            } else {
                $scope.DetailConsumptionSKUMapping.RMFirstCharacteristicsValueId = null;
            }

            if (!baseService.isUndefinedOrNull($scope.DetailConsumptionSKUMapping.SubFirstCharacteristicsId) && baseService.isUndefinedOrNull($scope.DetailConsumptionSKUMapping.SubFirstCharacteristicsValueId)) {
                throw "" + $scope.RMName1 + " value is required.";
            }

            $http({
                method: 'POST',
                url: 'OrderManagements/BOMMaster/CreateDetailConsumptionSKUMapping',
                data: { 'data': $scope.DetailConsumptionSKUMapping },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetDetailConsumptionSKU1MappingListBySKU($scope.detailConsumption.Id, $scope.DetailConsumptionSKUMapping.RMFirstCharacteristicsId);
                    $scope.ClearDetailConsumptionSKUMapping();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure', 'ConsumptionMatrixPopUp');
        }
    };

    $scope.SaveDetailConsumptionSKU2Mapping = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.detailConsumption.Id)) {
                throw 'First add Raw Material.';
            }
            $scope.DetailConsumptionSKUMapping.DetailConsumptionId = $scope.detailConsumption.Id;

            if (baseService.isUndefinedOrNull($scope.DetailConsumptionSKUMapping.SubSecondCharacteristicsId)) {
                throw "RM SKU is required.";
            }

            if (!$scope.DetailConsumptionSKUMapping.IsSecondCharacteristicCommon) {
                if (baseService.isUndefinedOrNull($scope.DetailConsumptionSKUMapping.RMSecondCharacteristicsValueId)) {
                    throw "" + $scope.DetailConsumptionSKUMapping.RMName2 + " value is required.";
                }
            } else {
                $scope.DetailConsumptionSKUMapping.RMSecondCharacteristicsValueId = null;
            }

            if (!baseService.isUndefinedOrNull($scope.DetailConsumptionSKUMapping.SubSecondCharacteristicsId) && baseService.isUndefinedOrNull($scope.DetailConsumptionSKUMapping.SubSecondCharacteristicsValueId)) {
                throw "" + $scope.CRM2Name + " value is required.";
            }


            $http({
                method: 'POST',
                url: 'OrderManagements/BOMMaster/CreateDetailConsumptionSKUMapping',
                data: { 'data': $scope.DetailConsumptionSKUMapping },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetDetailConsumptionSKU2MappingListBySKU($scope.detailConsumption.Id, $scope.DetailConsumptionSKUMapping.RMSecondCharacteristicsId);
                    $scope.ClearDetailConsumptionSKUMapping();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure', 'ConsumptionMatrixPopUp');
        }
    };

    $scope.bomDetailConsumptionSKU1MappingDataList = [];
    $scope.GetDetailConsumptionSKU1MappingListBySKU = function (detailConsumptionId, characteristicsId) {
        $scope.bomDetailConsumptionSKU1MappingDataList = [];
        $http.get("OrderManagements/BOMMaster/GetDetailConsumptionSKUMappingListBySKU1?detailConsumptionId=" + detailConsumptionId + '&characteristicsId=' + characteristicsId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.bomDetailConsumptionSKU1MappingDataList = response.data;

                        for (var i = 0; i < $scope.bomDetailConsumptionSKU1MappingDataList.length; i++) {
                            if ($scope.bomDetailConsumptionSKU1MappingDataList[i].IsFirstCharacteristicCommon == true) {
                                $scope.disablebtnDCSave1 = true;
                                break;
                            } else {
                                $scope.disablebtnDCSave1 = false;
                            }

                        }

                        $scope.DetailConsumptionSKUMapping.SubFirstCharacteristicsId = response.data[0].SubFirstCharacteristicsId;
                        $scope.GetCRMCharacteristicsValueCbo($scope.DetailConsumptionSKUMapping.SubFirstCharacteristicsId);

                    } else {
                        $scope.disablebtnDCSave1 = false;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.disablebtnDCSave2 = false;
    $scope.bomDetailConsumptionSKU2MappingDataList = [];
    $scope.GetDetailConsumptionSKU2MappingListBySKU = function (detailConsumptionId, characteristicsId) {
        $scope.bomDetailConsumptionSKU2MappingDataList = [];
        $http.get("OrderManagements/BOMMaster/GetDetailConsumptionSKUMappingListBySKU2?detailConsumptionId=" + detailConsumptionId + '&characteristicsId=' + characteristicsId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.bomDetailConsumptionSKU2MappingDataList = response.data;

                        for (var i = 0; i < $scope.bomDetailConsumptionSKU2MappingDataList.length; i++) {
                            if ($scope.bomDetailConsumptionSKU2MappingDataList[i].IsSecondCharacteristicCommon == true) {
                                $scope.disablebtnDCSave2 = true;
                                break;
                            } else {
                                $scope.disablebtnDCSave2 = false;
                            }
                        }

                        $scope.DetailConsumptionSKUMapping.SubSecondCharacteristicsId = response.data[0].SubSecondCharacteristicsId;
                        $scope.GetCRMSKU2CharacteristicsValueCbo($scope.DetailConsumptionSKUMapping.SubSecondCharacteristicsId);
                    } else {
                        $scope.disablebtnDCSave2 = false;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.message_Detailconfirmation = null;
    $scope.RemoveDetailConsumptionMatrix = function (data) {
        $scope.DetailConsumptionSKUMapping = data;
        if (!baseService.isUndefinedOrNull($scope.DetailConsumptionSKUMapping.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUp')).modal('show');
    }

    $scope.DeleteDetailConsumptionMatrix = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/BOMMaster/DeleteDetailConsumptionMatrix?id=' + $scope.DetailConsumptionSKUMapping.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');

                $scope.GetDetailConsumptionSKU1MappingListBySKU($scope.detailConsumption.Id, $scope.DetailConsumptionSKUMapping.RMFirstCharacteristicsId);
                $scope.GetDetailConsumptionSKU2MappingListBySKU($scope.detailConsumption.Id, $scope.DetailConsumptionSKUMapping.RMSecondCharacteristicsId);
                $scope.getDCCharacteristicsList($scope.detailConsumption.RMMaterialMasterId);
                // $scope.getDCCharacteristicsList($scope.bomDetailNew.RMMaterialMasterId);
                $scope.ClearDetailConsumptionSKUMapping();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };


    //#endregion  DetailConsumtionSKUMapping


    // #region Characteristics

    $scope.findCharValueSearchData = function (data, searchFor) {
        $scope.charValueSearchFor = searchFor;
        $scope.charValueCharName = data.Name;
        $scope.getCharData(data);
    };

    $scope.IsMandatoryButNull = function (isMandatory, value) {
        if (isMandatory) {
            if (baseService.isUndefinedOrNull(value)) return true;
            else return false;
        }
        else return false;
    };
    $scope.isSearch = false;
    $scope.IsFreeOrNot = function (IsFreeField) {
        if (IsFreeField) {
            if ($scope.isSearch) {
                return true;//disabled true
            }
            else
                return false;//disabled false
        }
        else {
            return true;//disabled true
        }
    };
    $scope.clearCharValueField = function (valueFor) {
        $scope[valueFor].CharacteristicsValueId = null;
        $scope[valueFor].FreeText = null;
        $scope[valueFor].FlagDisable = $scope.IsFreeOrNot($scope.char1.IsFreeField);
        $scope.isSearch = false;
    };
    $scope.nullByFreeText = function (valueFor) {
        $scope[valueFor].CharacteristicsValueId = null;
    };
    $scope.getCharData = function (data) {
        $scope.charValueParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'Code'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.charDataList = [];
        baseService.setCurrentPage('charDataList');
        $scope.url = '';
        $scope.getSearchCharData = function (pageno) {
            $scope.charValueParameters.assignment = data.ValueAssignmentLevel;
            $scope.charValueParameters.materialMasterId = data.MaterialMasterId;
            $scope.charValueParameters.charId = data.CharacteristicsId;
            //baseService.paginationBase('Materials/CharacteristicsValue/getcharacteristicsvaluesearchdata/', pageno, $scope.charValueParameters)
            baseService.paginationBase('Materials/CharacteristicsValue/getcharacteristicsvaluesearchdata?assignment=' + data.ValueAssignmentLevel + '&materialMasterId=' + data.MaterialMasterId + '&charId=' + data.CharacteristicsId, pageno, $scope.charValueParameters)
                .then(function (result) {
                    $scope.charDataList = result.Rows;
                    $scope.charValueParameters.total_count = result.Total;
                    $scope.isSearch = true;
                    angular.element(document.querySelector('#searchcharactervaluepopup')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getSearchCharData();

    };

    // #endregion Characteristics

    // #region SKU
    $scope.characteristicsValueList = [];
    $scope.SKU = null;
    $scope.SKULevel = null;
    $scope.name = null;
    $scope.state = null;

    $scope.AddSKU = function (state, name) {
        $scope.SKU = null;
        $scope.SKULevel = null;

        $scope.name = name;
        $scope.state = state;

        if ($scope.state == '1st') {
            $scope.charId = $scope.rmchar1.CharacteristicsId;
            $scope.SKU = $scope.rmchar1.Name;
            $scope.SKULevel = $scope.rmchar1.ValueAssignmentLevel;
        }
        if ($scope.state == '2nd') {
            $scope.charId = $scope.rmchar2.CharacteristicsId;
            $scope.SKU = $scope.rmchar2.Name;
            $scope.SKULevel = $scope.rmchar2.ValueAssignmentLevel;
        }
        if ($scope.state == '3rd') {
            $scope.charId = $scope.rmchar3.CharacteristicsId;
            $scope.SKU = $scope.rmchar3.Name;
            $scope.SKULevel = $scope.rmchar3.ValueAssignmentLevel;
        }
        $scope.characteristicsValue = {
            Id: baseService.pk()
            , MaterialMasterId: $scope.bomDetailNew.RMMaterialMasterId
            , CharacteristicsId: $scope.charId
            , Sequence: null
            , Code: null
            , ShortName: null
            , StandardName: null
            , UserName: null
            , SourceType: $scope.SKULevel
            , Description: null
            , Remarks: null
            , IsDefault: false
            , Active: true
        };
        $scope.characteristicsvalueNew = angular.copy($scope.characteristicsValue);
        $scope.GetMaterialMasterCharacteristicsValueSequence();
        angular.element(document.querySelector('#SKUpopup')).modal('show');
    }

    $scope.GetMaterialMasterCharacteristicsValueSequence = function () {
        $http.get('Materials/characteristicsvalue/getautosequence?characteristicsId=' + $scope.charId + '&materialId=' + $scope.bomDetailNew.RMMaterialMasterId)
            .then(function (response) {
                $scope.characteristicsvalueNew.Sequence = response.data;
            });
    };

    $scope.SaveBOMSKUState = function () {
        if ($scope.state == '1st') {
            $scope.characteristicsvalueNew.SourceType = $scope.rmchar1.ValueAssignmentLevel;
        }
        if ($scope.state == '2nd') {
            $scope.characteristicsvalueNew.SourceType = $scope.rmchar2.ValueAssignmentLevel;
        }
        if ($scope.characteristicsvalueNew.SourceType === 'General') {
            $scope.characteristicsvalueNew.MaterialMasterId = null;
        }
        if (baseService.isUndefinedOrNull($scope.name)) {
            $scope.SaveBOMSKU();
        }
        if ($scope.name == 'mat') {

            $scope.SaveBOMSKUMatrix();
        }
    }

    $scope.SaveBOMSKU = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.skuForm.$valid) {
                $http({
                    method: 'POST',
                    url: 'OrderManagements/BOMMaster/CreateCharacteristicsValue',
                    data: { 'entity': $scope.characteristicsvalueNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'SKUpopup');
                    }
                    else {
                        ShowResult(response.data.Message, 'success', 'SKUpopup');

                        if ($scope.state == '1st') {
                            $scope.bomDetailNew.FirstCharacteristicsId = $scope.charId;
                            $scope.rmchar1.FreeText = response.data.CharacteristicsValue.UserName;
                            $scope.bomDetailNew.FirstCharacteristicsValueId = response.data.CharacteristicsValue.Id;

                            $scope.rmchar1.CharacteristicsId = $scope.bomDetailNew.FirstCharacteristicsId;
                            $scope.rmchar1.CharacteristicsValueId = $scope.bomDetailNew.FirstCharacteristicsValueId;
                        }
                        if ($scope.state == '2nd') {
                            $scope.bomDetailNew.SecondCharacteristicsId = $scope.charId;
                            $scope.rmchar2.FreeText = response.data.CharacteristicsValue.UserName;
                            $scope.bomDetailNew.SecondCharacteristicsValueId = response.data.CharacteristicsValue.Id;

                            $scope.rmchar2.CharacteristicsId = $scope.bomDetailNew.SecondCharacteristicsId;
                            $scope.rmchar2.CharacteristicsValueId = $scope.bomDetailNew.SecondCharacteristicsValueId;
                        }
                        if ($scope.state == '3rd') {
                            $scope.bomDetailNew.ThirdCharacteristicsId = $scope.charId;
                            $scope.rmchar3.FreeText = response.data.CharacteristicsValue.UserName;
                            $scope.bomDetailNew.ThirdCharacteristicsValueId = response.data.CharacteristicsValue.Id;

                            $scope.rmchar3.CharacteristicsId = $scope.bomDetailNew.ThirdCharacteristicsId;
                            $scope.rmchar3.CharacteristicsValueId = $scope.bomDetailNew.ThirdCharacteristicsValueId;
                        }


                        $scope.clearMasterCharacteristicsValue();
                        angular.element(document.querySelector('#SKUpopup')).modal('hide');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'SKUpopup');
                }

            }
        } catch (e) {
            ShowResult(e, 'failure', 'SKUpopup');
        }
    };

    $scope.SaveBOMSKUMatrix = function () {

        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.skuForm.$valid) {
                $http({
                    method: 'POST',
                    url: 'OrderManagements/BOMMaster/CreateCharacteristicsValue',
                    data: { 'entity': $scope.characteristicsvalueNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'SKUpopup');
                    }
                    else {
                        ShowResult(response.data.Message, 'success', 'SKUpopup');

                        if ($scope.state == '1st') {
                            $scope.BOMSKUMapping.RMFirstCharacteristicsId = $scope.charId;

                            $scope.GetRMCharacteristicsValueCbo($scope.bomDetailNew.RMMaterialMasterId, $scope.BOMSKUMapping.RMFirstCharacteristicsId);

                        }
                        if ($scope.state == '2nd') {
                            $scope.BOMSKUMapping.RMSecondCharacteristicsId = $scope.charId;

                            $scope.GetRMSKU2CharacteristicsValueCbo($scope.bomDetailNew.RMMaterialMasterId, $scope.BOMSKUMapping.RMSecondCharacteristicsId);
                        }
                        //if ($scope.state == '3rd') {
                        //    $scope.bomDetailNew.ThirdCharacteristicsId = $scope.charId;
                        //    $scope.rmchar3.FreeText = response.data.CharacteristicsValue.UserName;
                        //    $scope.bomDetailNew.ThirdCharacteristicsValueId = response.data.CharacteristicsValue.Id;

                        //    $scope.rmchar3.CharacteristicsId = $scope.bomDetailNew.ThirdCharacteristicsId;
                        //    $scope.rmchar3.CharacteristicsValueId = $scope.bomDetailNew.ThirdCharacteristicsValueId;
                        //}


                        $scope.clearMasterCharacteristicsValue();
                        angular.element(document.querySelector('#SKUpopup')).modal('hide');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'SKUpopup');
                }
            }

        } catch (e) {
            ShowResult(e, 'failure', 'SKUpopup');
        }
    };

    $scope.CloseCharacteristicsValuePopUp = function () {
        angular.element(document.querySelector('#SKUpopup')).modal('hide');
    }
    $scope.clearMasterCharacteristicsValue = function () {
        $scope.characteristicsValue = {};
        $scope.characteristicsvalueNew = {
            Id: baseService.pk()
            , MaterialMasterId: $scope.bomDetailNew.RMMaterialMasterId
            , CharacteristicsId: $scope.charId
            , Sequence: 0, Active: true, IsDefault: false
        };
        $scope.GetMaterialMasterCharacteristicsValueSequence();
    }

    $scope.destinationList = [];
    $scope.getDestination = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/destination/GetCbo/'
        }).then(function successCallback(response) {
            $scope.destinationList = response.data;
        });
    };
    $scope.getDestination();

    // #endregion SKU

    //#region Document

    //$scope.ClearImage = function () {
    //    document.getElementById('uploadBtn').value = '';
    //    document.getElementById("uploadFile").value = '';
    //    $scope.issueTransactionDocuments = {};
    //    $scope.filedata = null;
    //};

    $scope.ClearDoc = function () {
        $scope.bomDocuments = {
            Id: null,
            BoMId: null,
            FileName: null,
            Description: null
        }
    };

    $scope.bomDocuments = {
        Id: null,
        BoMId: null,
        FileName: null,
        Description: null
    }

    $scope.SaveDocuments = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb.';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.bomDocuments.FileName = fileName;

            $scope.bomDocuments.BoMId = $scope.bomNew.Id;
            var formData = new FormData();

            $http({
                method: 'POST',
                url: 'OrderManagements/BOMMaster/CreateDocuments',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append("bomDocuments", angular.toJson(data.bomDocuments));
                    if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                        formData.append('file', data.file);
                    }
                    return formData;
                },
                data: { 'bomDocuments': $scope.bomDocuments, 'file': $scope.filedata }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {

                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.ClearImage();
                    $scope.ClearDoc();
                    $scope.LoadIssueDocumentsData($scope.bomNew.Id);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
            //}
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.onBeginUpload = function (args) {
        try {
            if (baseService.isUndefinedOrNull($scope.bomNew.Id))
                throw 'Please select/save Issue Transaction first.';

            if (baseService.isUndefinedOrNull($scope.bomDocuments.Description)) {
                throw 'Description is required.';
            }
            else {
                var _data = [{ Id: null, BomId: $scope.bomNew.Id, Description: $scope.bomDocuments.Description }];

                args.data = JSON.stringify(_data);
            }


        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadUrl = 'OrderManagements/BOMMaster/SaveDefault';
    $scope.fileselect = function (e) {

    }
    //$scope.errorPicUpload = function (e) {
    //    if (baseService.isUndefinedOrNull($scope.bomNew.Id))
    //        ShowResult('Please select/save Issue Transaction first', 'Error');
    //    else
    //        ShowResult("The selected file size is too large. Please select a file less than 10 MB", 'failure');
    //}


    $scope.issueTransactionDocumentList = [];
    $scope.LoadIssueDocumentsData = function () {

        $scope.ClearDoc();
        $http.get('OrderManagements/BOMMaster/GetBoMDocumentsData?bomId=' + $scope.bomNew.Id)
            .then(function (response) {
                $scope.issueTransactionDocumentList = response.data;
            });
    };

    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.BOMPath + '/' + data.Id + extention;
    };

    $scope.indexQua = -1;
    $scope.GetDocumentData = function (data, index) {
        $scope.filedata = {};
        $scope.bomDocuments = Object.assign({}, data);
        $scope.filedata.name = data.FileName;
        $scope.bomDocuments.FileName = data.FileName;

        $scope.indexQua = index;
    };
    $scope.message_confirmation_doc = null;
    $scope.confirmQualificationDelete = function (data) {
        $scope.deleteQualificationId = data.Id;
        $scope.message_confirmation_doc = "Are you sure to delete [" + data.FileName + "]? ";
    };

    $scope.DeleteDocument = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/BOMMaster/DeleteDocument',
            dataType: 'JSON',
            data: { 'Id': $scope.deleteQualificationId }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadIssueDocumentsData($scope.bomNew.Id);
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };

    //#endregion 

    $scope.AttahedBoMIList = [];
    $scope.GetAttahedBoMInfo = function (obj) {
        $scope.SOItemList = [];
        $http.get('OrderManagements/BOMMaster/GetAttahedBoMInfo?Id=' + obj.Id)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.AttahedBoMIList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#TaggedDetailPopup')).modal('show');
    };

    $scope.CopyBOMDetail = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'CopyBOM?Id=' + $scope.SelectedBOMRow.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getmasterData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    }
}




