'use strict';
assetInventoryIssueController.$inject = ['$window','cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function assetInventoryIssueController($window,cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = "Asset Issue";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/InventoryIssue/';
    $scope.getListUrl = $scope.path + 'GetAssetInventoryIssue';
    $scope.saveUrl = $scope.path + 'InsertAssetIssue';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.currentDate = new Date(Date.now());

    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });

    $scope.searchByList = [
        {
            value: 'Id'
            , name: 'Issue No'
        },
        {
            value: 'MaterialStorage'
            , name: 'Storage Location'
        },
        {
            value: 'IssueDate'
            , name: 'Issue Date'
        }
    ];

    $scope.closePopUp1 = function () {
        angular.element(document.querySelector('#popUpId')).modal('hide');
        
    }
    baseService.init($scope.getListUrl, null, null, 'DESC', 'IssueDate', 'IssueDate');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.issueList = [];
                $scope.issueList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $http({
        method: 'GET',
        url: 'Materials/MaterialStorage/getcbo'
    }).then(function (response) {
        $scope.storageList = response.data;
    });
    $scope.product = {
        Id: null
        , ComapnyGroupId: null
        , CompanyId: null
        , PlantId: null
        , PlantName: null
        , EntityId: null
        , EntityName: null
        , MaterialStorageId: null
        , IssueDate: null
        , Remarks: null
        , EmployeeId: null
        , EmployeeName: null
        , IssueType: 'Capital'
        , OrderRefNo: null
    };
    $scope.productNew = Object.assign({}, $scope.product);

    $scope.Get = function (index) {
        var gridObj = $("#GridIIssue").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.index = index;
        $scope.product = data;//$scope.issueList[index];//data;
        $scope.productNew = Object.assign({}, $scope.product);
        $scope.materialStockList = [];
        $scope.specificStockList = [];
        getIssueDetailList();
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };

    

    $scope.specificStock = function () {
        $scope.specificStockList = [];
        for (var i = 0; i < $scope.detailList.length; i++) {
            $scope.specificStockList.push($scope.detailList[i]);
        }
    }

    $scope.validation1 = function () {
        if ($scope.detailList.length > 0) {
            $scope.specificStock();
        }
        if ($scope.detailList.length < 1) {
            ShowResult('Please select Issue !', 'failure');
            return true;
        }
        else if ($scope.detailList.length) {
            for (var i = 0; i < $scope.detailList.length; i++) {
                if ($scope.detailList[i].RequisitionQty > $scope.detailList[i].BalanceStock) {
                    ShowResult('Issue Stock can not more than Balance Stock !!', 'failure');
                    return true;
                }
                if ($scope.detailList[i].RequisitionQty == 0 || baseService.isUndefinedOrNull($scope.detailList[i].RequisitionQty)=='NaN') {
                    ShowResult('Issue Stock can not 0 !!', 'failure');
                    return true;
                }
                else
                    return false;
            }
        }
        else
            return false;

    }
    $scope.Save = function () {

        
        if (!$scope.validation1()) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST'
                    , url: $scope.saveUrl
                    , data: {
                        entities: $scope.detailList
                        , specificStockList: $scope.specificStockList
                        , inventoryIssue: $scope.productNew
                    }
                    , dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult(response.data.Message, 'success');
                        //$scope.Clear();
                        $scope.productNew.Id = response.data.inventoryIssue.Id;
                        $scope.getData();
                    }
                }), function (response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else ShowResult('Please issue material', 'failure');
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.product = {};
        $scope.productNew = { FixedAssetOrInventory: 'Inventory', PODepended: false, AlongwithInvoice: false, IssueType: 'Capital' };
        $scope.detailModel = {};
        $scope.clearCharNames();
        $scope.detailList = [];
        $scope.specificStockList = [];
    }

    // #region Details

    $scope.detailPopUp = function () {
        //debugger;
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.productNewForm.$valid) {
            $scope.product = Object.assign({}, $scope.productNew);
            $scope.detailModel = {
                Id: null
                , InventoryReveiveId: null
                , MaterialStorageId: $scope.productNew.MaterialStorageId
                , InventoryMaterialId: null
                , MaterialMasterId: null
                , MaterialMasterName: null
                , ArticleId: null
                , ArticleName: null
                , MaterialTypeName: null
                , OurStyleName: null
                , Description: null
                , MaterialGroupMasterName: null
                , ProductMasterName: null
                , IsOurStyleRequired: false
                , IsProductMstRequired: false

                , FirstCharacteristicsId: null
                , FirstCharacteristicsValueId: null

                , SecondCharacteristicsId: null
                , SecondCharacteristicsValueId: null

                , ThirdCharacteristicsId: null
                , ThirdCharacteristicsValueId: null

                , TransactionQty: null
                , TransactionUoMId: null
                , TransactionUoM: null
                , BaseQty: null
                , BaseUOMId: null
                , BaseUoM: null
                , BaseUoMFactor: null
                , TransactionRate: null
                , TotalQty: 0
                , AvgRate: null

                , InventoryIssueId: $scope.productNew.Id
                , AvgAmount: null
                , PolicyRate: null
                , PolicyAmount: null
				, Policy: null
				, ActivityName: null
				, BudgetMasterId: null
                , ActivityId: null
                , CountryName: null              
                , Comments: null
            };
			$scope.clearCharNames();
			
           // angular.element(document.querySelector('#detailPopUp')).modal('show');
        }
    };
    $scope.closeDetaiPopUp = function () {
        $scope.detailModel = {};
        $scope.clearCharNames();
        angular.element(document.querySelector('#detailPopUp')).modal('hide');
    };

    $scope.materialType = ['Asset', 'Consumable', 'Spare', 'RawMaterial'];

    //$scope.setMaterialMasterData
    $scope.selectDoubleClick = function () {
        //debugger;
        $scope.detailPopUp();
        var gridObj = $("#popUpData").data("ejGrid");
        var ob = gridObj.getSelectedRecords()[0];
        var getRow = $filter("filter")($scope.detailList, { "InventoryReceiveDetailId": ob.InventoryReceiveDetailId, "MaterialMasterId": ob.MaterialMasterId, "ArticleId":ob.ArticleId});

        // if (!ob.hasInventory) return ShowResult('Material stock does not exist.', '', 'materialMasterbyTypePopup');
        if (getRow.length == 0) {

            
            $scope.detailModel.TransactionUoM = ob.TransactionUoM;
            $scope.detailModel.MaterialMasterId = ob.Id;
            $scope.detailModel.MaterialMasterName = ob.UserName;
            $scope.detailModel.BaseUOMId = ob.BaseUOMId;
            $scope.detailModel.BaseUoMFactor = ob.BaseUoMFactor;
            $scope.detailModel.BaseUoM = ob.BaseUoM;
            $scope.detailModel.TransactionUoMId = ob.TransactionUoMId;
            $scope.detailModel.OurStyleName = ob.OurStyleName;
            $scope.detailModel.MaterialGroupMasterName = ob.MaterialGroupMasterName;
            $scope.detailModel.MaterialGroupMasterId = ob.MaterialGroupMasterId;
            $scope.detailModel.ProductMasterName = ob.ProductMasterName;
            $scope.detailModel.IsOurStyleRequired = ob.IsOurStyleRequired;
            $scope.detailModel.IsProductMstRequired = ob.IsProductMstRequired;

            $scope.detailModel.ArticleId = ob.ArticleId;
            $scope.detailModel.ArticleName = ob.StandardName;
            $scope.detailModel.FirstCharacteristicsValueId = null;
            $scope.detailModel.SecondCharacteristicsValueId = null;
            $scope.detailModel.ThirdCharacteristicsValueId = null;
            $scope.detailModel.BudgetName = ob.BudgetName;
            $scope.detailModel.ActivityName = ob.ActivityName;
            $scope.detailModel.BudgetMasterId = ob.BudgetMasterId;
            $scope.detailModel.ActivityId = ob.ActivityId;

            $scope.detailModel.GLGeneralInfoId = ob.GLGeneralInfoId;
            $scope.detailModel.InventoryMaterialId = ob.InventoryMaterialId;
            $scope.detailModel.MaterialMasterId = ob.MaterialMasterId;
            $scope.detailModel.MaterialTranAmount = ob.MaterialTranAmount;
            $scope.detailModel.TotalMaterialTranAmount = ob.TotalMaterialTranAmount;
            $scope.detailModel.BooksCurrencyBaseRate = ob.BooksCurrencyBaseRate;
            $scope.detailModel.TotalMaterialBooksCurrencyAmount = ob.TotalMaterialBooksCurrencyAmount;
            $scope.detailModel.TransactionQty = ob.TransactionQty;
            $scope.detailModel.PostingQty = ob.BaseQty;
            $scope.detailModel.BaseQty = ob.BaseQty;
            $scope.detailModel.BaseRate = ob.BaseRate;
            $scope.detailModel.PostingQuantity = ob.BaseQty;
            $scope.detailModel.BaseIssueQty = ob.BaseIssueQty;
            $scope.detailModel.PurchaseReturnQty = ob.PurchaseReturnQty;
            $scope.detailModel.IssueReturnQty = ob.IssueReturnQty;
            $scope.detailModel.ReductionByAdjustmentQty = ob.ReductionByAdjustmentQty;


            $scope.detailModel.InventorySalesQty = ob.InventorySalesQty;
            $scope.detailModel.InventoryScrapQty = ob.InventoryScrapQty;
            $scope.detailModel.InventoryTransferQty = ob.InventoryTransferQty;




            $scope.detailModel.BalanceStock = ob.BalanceStock;
            $scope.detailModel.StockQty = ob.BalanceStock;
            $scope.detailModel.RequisitionQty = ob.BalanceStock;
            $scope.detailModel.IssueQty = ob.IssueQty;
            $scope.detailModel.InventoryReceiveDetailId = ob.InventoryReceiveDetailId;

            $scope.detailModel.CountryId = ob.CountryId;
            $scope.detailModel.CountryName = ob.CountryName;

            $scope.hasArticle = ob.HasAttribute;
            $scope.hasSku = ob.WithSKU;
            if (ob.HasAttribute) $scope.getArticleSearchList(ob.Id);
            if (ob.WithSKU) $scope.getCharacteristicsList(ob.Id);
            $scope.getActivity(ob.BudgetMasterId, ob.ActivityId);

            var mmId = []; mmId.push(ob.MaterialMasterId);
            cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (result) {
                $scope.uoMList = result;
            });
            manualValidation('div_mm', false);
            manualValidation('div_qty', false);
            //if (!ob.HasAttribute && !ob.WithSKU)$scope.getBudgetActivityInIssueMaterial(ob.MaterialGroupMasterId);
            $scope.detailAdd();
            angular.element(document.querySelector('#popUpId')).modal('hide');
        }
        else {
            ShowResult('Same item already  exist!!', 'failure','popUpId');
        }
    };
    
    $scope.activityList = [];
    $scope.getActivity = function (budgetMasterId,activityId) {
        cboService.getBudgetMasterActivityCbo(budgetMasterId, function (result) {
            $scope.detailModel.ActivityId = null;
            $scope.activityList = [];
            $scope.activityList = result;
            $scope.detailModel.ActivityId = activityId;

        });
    };

	$scope.selectarticle = function (ob) {
		//debugger;
        try {
            $scope.detailModel.ArticleId = ob.Id;
            $scope.detailModel.ArticleName = ob.StandardName;
            manualValidation('div_ar', false);
            if (!ob.WithSKU) getMaterialStock();
            if (!ob.WithSKU) $scope.getBudgetActivityInIssueMaterial($scope.detailModel.MaterialGroupMasterId);
            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };
    $scope.setCharData = function (data) {
        $scope[$scope.charValueSearchFor].CharacteristicsValueId = data.CharacteristicsValueId;
        $scope[$scope.charValueSearchFor].FreeText = data.UserName;
        $scope[$scope.charValueSearchFor].FlagDisable = $scope.isSearch;
        if ($scope.charValueSearchFor === 'char1') $scope.detailModel.FirstCharacteristicsValueId = data.CharacteristicsValueId;
        if ($scope.charValueSearchFor === 'char2') $scope.detailModel.SecondCharacteristicsValueId = data.CharacteristicsValueId;
        if ($scope.charValueSearchFor === 'char3') $scope.detailModel.ThirdCharacteristicsValueId = data.CharacteristicsValueId;
        getMaterialStock();
        angular.element(document.querySelector('#searchcharactervaluepopup')).modal('hide');
    };

    $scope.clearCharValueField = function (valueFor) {
        $scope[valueFor].CharacteristicsValueId = null;
        $scope[valueFor].FreeText = null;
        $scope[valueFor].FlagDisable = $scope.IsFreeOrNot($scope.char1.IsFreeField);
        $scope.isSearch = false;
        if (valueFor === 'char1') $scope.detailModel.FirstCharacteristicsValueId = null;
        if (valueFor === 'char2') $scope.detailModel.SecondCharacteristicsValueId = null;
        if (valueFor === 'char3') $scope.detailModel.ThirdCharacteristicsValueId = null;
    };
    $scope.manualValidationAddRemove = function (divId, fieldName, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull($scope.detailModel[str.replace(/\s/g, '')]))
            return manualValidation(divId, true, msg);
        else
            return manualValidation(divId, false);
    };
    $scope.validation = function () {
        $scope.manualValidationAddRemove('div_mm', 'MaterialMasterName');
        $scope.manualValidationAddRemove('div_ar', 'ArticleName');
        $scope.manualValidationAddRemove('div_qty', 'TransactionQty');
		$scope.manualValidationAddRemove('div_qty', 'TransactionUoMId', 'UoM is required');
		//$scope.manualValidationAddRemove('div_entity', 'EntityId', 'Entity is required');
		//$scope.manualValidationAddRemove('div_budget', 'BudgetMasterid', 'budget is required');

		
        if ($scope.hasSku) {
            if (!baseService.isUndefinedOrNull($scope.char1.CharacteristicsId))
                $scope.IsMandatoryButNull($scope.char1.IsMandatory, $scope.char1.FreeText);
            else if (!baseService.isUndefinedOrNull($scope.char2.CharacteristicsId))
                $scope.IsMandatoryButNull($scope.char2.IsMandatory, $scope.char2.FreeText);
            else if (!baseService.isUndefinedOrNull($scope.char3.CharacteristicsId))
                $scope.IsMandatoryButNull($scope.char3.IsMandatory, $scope.char3.FreeText);
            else throw 'Please insert SKU.';
        }
    };
    $scope.detailList = [];
   
    $scope.detailAdd = function () {
        //debugger;
        try {
            $scope.validation();
            if ($scope.detailModel.BudgetMasterId === '' || $scope.detailModel.BudgetMasterId === null) {
                ShowResult('Budget is required', 'failure', 'detailPopUp');
                return false;
            }
            if ($scope.detailModel.CostCenterId === '' || $scope.detailModel.CostCenterId === null) {
                ShowResult('Cost center is required', 'failure', 'detailPopUp');
                return false;
            }
            if ($scope.detailModel.ActivityId === '' || $scope.detailModel.ActivityId === null) {
                ShowResult('Activity is required', 'failure', 'detailPopUp');
                return false;
            }
            $scope.detailModel.TransactionQty = baseService.isUndefinedOrNull($scope.detailModel.TransactionQty) === true ? 0 : parseFloat($scope.detailModel.TransactionQty);
            //if ($scope.detailModel.TransactionQty === 0)
            //    throw 'Please insert issue Qty.';
            //else {
            //    if ($scope.detailModel.TransactionUoMId === $scope.detailModel.BaseUOMId) {
            //        if ($scope.detailModel.TransactionQty > parseFloat($scope.detailModel.PostingQuantity))
            //            throw 'Issue Qty must be less than or equal Ready for Issue Qty.';
            //        $scope.detailModel.BaseQty = $scope.detailModel.TransactionQty;
            //    }
            //    else {
            //        var tQty = parseFloat($scope.detailModel.TransactionQty) * parseFloat($.grep($scope.uoMList, function (item) { return item.Value === $scope.detailModel.TransactionUoMId; })[0].BaseUoMFactor);
            //        if (tQty > parseFloat($scope.detailModel.PostingQuantity))
            //            throw 'Issue Qty must be less than or equal Ready for Issue Qty.';
            //        $scope.detailModel.BaseQty = tQty;
            //    }
            //}

            for (var i = 0; i < baseService.arrayLength($scope.detailList); i++) {
                if ($scope.detailList[i].MaterialMasterId === $scope.detailModel.MaterialMasterId &&
                    $scope.detailList[i].ArticleId === $scope.detailModel.ArticleId &&
                    $scope.detailList[i].FirstCharacteristicsValueId === $scope.detailModel.FirstCharacteristicsValueId &&
                    $scope.detailList[i].SecondCharacteristicsValueId === $scope.detailModel.SecondCharacteristicsValueId &&
                    $scope.detailList[i].ThirdCharacteristicsValueId === $scope.detailModel.ThirdCharacteristicsValueId &&
                    $scope.detailList[i].CountryId === $scope.detailModel.CountryId)
                    throw 'This material already issued.';
            }
            $scope.detailModel.TransactionUoM = $scope.detailModel.TransactionUoM;
            $scope.detailModel.FirstCharacteristicsId = $scope.char1.CharacteristicsId;
            $scope.detailModel.FirstCharacteristicsValueId = $scope.char1.CharacteristicsValueId;
            $scope.detailModel.FirstCharacteristicText = $scope.char1.FreeText;
            $scope.detailModel.SecondCharacteristicsId = $scope.char2.CharacteristicsId;
            $scope.detailModel.SecondCharacteristicsValueId = $scope.char2.CharacteristicsValueId;
            $scope.detailModel.ThirdCharacteristicsId = $scope.char3.CharacteristicsId;
            $scope.detailModel.ThirdCharacteristicsValueId = $scope.char3.CharacteristicsValueId;
            $scope.detailModel.IssueDate = $scope.productNew.IssueDate;
            $scope.detailModel.Remarks = $scope.productNew.Remarks;
            $scope.detailModel.EmployeeId = $scope.productNew.EmployeeId;
            $scope.detailModel.CountryName = $scope.detailModel.CountryName;
            $scope.detailModel.CountryId = $scope.detailModel.CountryId;
            //$scope.detailModel.BaseUoMFactor = $.grep($scope.uoMList, function (item) { return item.Value === $scope.detailModel.TransactionUoMId; })[0].BaseUoMFactor;
            $scope.detailModel.BaseUoMFactor = $scope.detailModel.BaseUoMFactor;
            //$scope.detailModel.TransactionUoM = angular.element("#issueUoM :selected").text();
            var row = Object.assign({}, $scope.detailModel);
            $scope.detailList.push(row);
        } catch (e) {
            ShowResult(e, 'failure', 'detailPopUp');
        }
    };
    function getMaterialStock() {
        $http({
            method: 'POST',
            url: $scope.path + 'GetStock',
            data: { entity: $scope.detailModel, issueDate: $scope.productNew.IssueDate },
            dataType: 'JSON'
        }).then(function (response) {
            $scope.detailModel.TotalQty = response.data.TotalQty;
            $scope.detailModel.PostingQty = response.data.PostingQty;
            $scope.detailModel.PostingQuantity = response.data.PostingQuantity;
            $scope.detailModel.ApprovedQty = response.data.ApprovedQty;
            $scope.detailModel.UnApprovedQty = response.data.UnApprovedQty;
            if (baseService.isUndefinedOrNull($scope.detailModel.TotalQty))
                $scope.errorText = 'This material has no stock';
            else $scope.errorText = null;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    }

   
    $scope.removeRowModal = function (ob, index) {
        try {
            $scope.delData = ob;
            $scope.message_confirmation = "Are you sure want to permanent delete [" + ob.MaterialMasterName + "] ";
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
            $scope.popUpIndex = index;
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeRow = function () {
        if (!baseService.isUndefinedOrNull($scope.delData.Id)) {
            $http({
                method: 'POST'
                , url: $scope.deleteUrl + '?issueDetailId=' + $scope.delData.Id
                , dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else
                    ShowResult(response.data.Message, 'success');
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
        for (var i = 0; i < baseService.arrayLength($scope.specificStockList); i++) {
            if ($scope.specificStockList[i].InventoryMaterialId === $scope.delData.InventoryMaterialId)
                $scope.specificStockList.splice(i, 1);
        }
        $scope.detailList.splice($scope.popUpIndex, 1);
        $scope.delData = null;
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
    };

    function getIssueDetailList() {
        //debugger;
        $http.get($scope.path + 'GetIssueDetailByIssueId?issueId=' + $scope.productNew.Id)
            .then(function (response) {
                $scope.detailList = response.data;
            });
    }

    // #endregion Details

    // #region Specific Stock

    $scope.materialStockList = [];
    $scope.specificStockList = [];



   




    $scope.getRequisitionList = function (issueDetailId) {
        $scope.materialStockList = [];
        $scope.specificStockList = [];
        $http({
            method: 'POST'
            , url: $scope.path + 'GetRequisitionList'
            , data: { issueDetailId: issueDetailId }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.materialStockList = response.data;
            angular.element(document.querySelector('#stockPopUp')).modal('show');
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.closeStockPopUp = function () {
        angular.element(document.querySelector('#stockPopUp')).modal('hide');
    };
    function qtyValidation(list) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].Flag) {
                if (parseFloat(list[i].RequisitionQty) > parseFloat(list[i].StockQty)) throw 'Requisition Qty can\'t greater than stock qty.';
            }
        }
    }
    function validationWithTotal(list) {
        var totalQty = 0;
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            list[i].RequisitionQty = baseService.isUndefinedOrNull(list[i].RequisitionQty) === true ? 0 : parseFloat(list[i].RequisitionQty);
            if (list[i].Flag) {
                if (parseFloat(list[i].RequisitionQty) === 0)
                    throw 'Please input requisition qty';
                else {
                    if (list[i].TransactionUoMId !== list[i].BaseUOMId) totalQty += parseFloat(list[i].RequisitionQty) * parseFloat(list[i].BaseUoMFactor);
                    else totalQty += parseFloat(list[i].RequisitionQty);
                }
            }
        }
        var qty = parseFloat($scope.detailList[$scope.index].TransactionQty) * parseFloat($scope.detailList[$scope.index].BaseUoMFactor);
        if (totalQty > qty && qty !== totalQty) throw 'Issue qty can\'t over ' + qty + ' .';
        if (totalQty < qty && qty !== totalQty) throw 'Issue qty can\'t less ' + qty + ' .';

    }

    // #endregion Specific Stock

    $scope.ApprovedStockList = [];
    $scope.getApprovedStock = function (data) {
        $http({
            method: 'POST'
            , url: $scope.path + 'GetApprovedStockDetail'
            , data: { entity: data, issueDate: $scope.productNew.IssueDate }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.ApprovedStockList = response.data;
            angular.element(document.querySelector('#ApprovedStockPopUp')).modal('show');
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.closeApprovedStockPopUp = function () {
        angular.element(document.querySelector('#ApprovedStockPopUp')).modal('hide');
    };

    $scope.ApprovedStockBeyondIssueDateList = [];
    $scope.getApprovedStockDetailBeyondIssueDate = function (data) {
        $http({
            method: 'POST'
            , url: $scope.path + 'GetApprovedStockDetailBeyondIssueDate'
            , data: { entity: data, issueDate: $scope.productNew.IssueDate }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.ApprovedStockBeyondIssueDateList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.PostingStockList = [];
    $scope.getPostingStock = function (data) {
        $http({
            method: "POST",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/InventoryIssue/GetPostingStockDetail',
           data: { entity: data, issueDate: $scope.productNew.IssueDate }

        }).then(function successCallback(response) {
            $scope.PostingStockList = response.data;
            angular.element(document.querySelector('#PostingStockPopUp')).modal('show');
            //entrydata = copy(searchdata);
        });
    };

   
    $scope.closePostingStockPopUp = function () {
        angular.element(document.querySelector('#PostingStockPopUp')).modal('hide');
    };

    $scope.PostingStockBeyondIssueDateList = [];
    $scope.getPostingStockBeyondIssueDate = function (data) {
        $http({
            method: 'POST'
            , url: $scope.path + 'GetPostingStockDetailBeyondIssueDate'
            , data: { entity: data, issueDate: $scope.productNew.IssueDate }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.PostingStockBeyondIssueDateList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tabU = 1;
    $scope.setTabU = function (newTab) {
        $scope.tabU = newTab;
    };

    $scope.isSetU = function (tabNum) {
        return $scope.tabU === tabNum;
    };

    $scope.tabP = 1;
    $scope.setTabP = function (newTab) {
        $scope.tabP = newTab;
    };

    $scope.isSetP = function (tabNum) {
        return $scope.tabP === tabNum;
    };

    
    $scope.IssueReport = function (data) {
        location.href = "Products/InventoryIssue/IssueReport?grnId=" + data.Id;
    };



    $scope.AssetInventoryIssueReport = function (data) {
        location.href = "Products/InventoryIssue/AssetInventoryIssueReport?grnId=" + data.Id;
    };
    //$scope.AssetInventoryIssueReport = function (data) {
    //    /*location.href = */
    //    window.open("Products/InventoryIssue/AssetInventoryIssueReport?grnId=" + data.Id);
    //};

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.productNew.EmployeeName = employee.EmployeeName;
            $scope.productNew.EmployeeId = employee.SystemId;
        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector("#employeePopUp")).modal("hide");
    };

    $scope.clearEmployee = function () {
        $scope.productNew.EmployeeName = null;
        $scope.productNew.EmployeeId = null;
	};



	$scope.setSelected = function (data) {
		//debugger;
		$scope.addRow(data);
		$scope.closeCOAICodeListPopUp();
	};

	$scope.addRow = function (data) {
		$scope.detailModel.GLGeneralInfoId = data.GLGeneralInfoId;
		$scope.detailModel.BudgetMasterId = data.BudgetMasterId;
		$scope.detailModel.ActivityId = data.ActivityId;
        $scope.detailModel.BudgetName = data.BudgetName;
		$scope.getActivity(data);
	};
	
    $scope.setissueAUCglSelected = function (data) {
        $scope.addissueAUCglRow(data);
        $scope.closeIssueAUCglListPopUp();
    };

    $scope.addissueAUCglRow = function (data) {
        $scope.detailModel.GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.detailModel.BudgetMasterId = data.BudgetMasterId;
        $scope.detailModel.ActivityId = data.ActivityId;
        $scope.detailModel.BudgetName = data.BudgetName;
        $scope.getActivity(data.BudgetMasterId, data.ActivityId);
    };


    $scope.searchissueAUCglByList = [
        {
            "name": "Fixed Asset",
            "value": "FixedAssetName"
        },
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "RefNo",
            "value": "RefNo"
        }
    ];

    $scope.issueAUCglListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoCode",
        searchBy: "ActivityName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.issueAUCglList = [];
    $scope.GetIssueAUCList = function () {
        $scope.IssueAUCGLUrl = "Accounts/glitem/GetIssueAUCGLBudgetActivity";
        $scope.GetIssueAUCGLData = function (pageno) {

            baseService.paginationBase($scope.IssueAUCGLUrl, pageno, $scope.issueAUCglListParameters)
                .then(function (result) {
                    $scope.issueAUCglList = result.Rows;
                    $scope.issueAUCglListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#IssueAUCGLPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetIssueAUCGLData();
    };

    $scope.closeIssueAUCglListPopUp = function () {
        angular.element(document.querySelector("#IssueAUCGLPopUp")).modal("hide");
    };

    $scope.CostCenterLoad = function () {
        //debugger;
        cboService.getCostCenterCbo(function (result) {
            $scope.costCenterList = result;
        });
    }
	$scope.CostCenterLoad();
	baseService.getCompanyConfiguration(function (result) {
		$scope.companyConfig = result;
    });
    cboService.getCboEntityByPlant(null, null, '', function (result) {
        $scope.EntityList = result;
    });
    $scope.BudgetActivityList = [];

    $scope.getBudgetActivityInIssueMaterial = function (materialGroupMasterId) {
        $http({
            method: "GET",
            url: 'Products/InventoryIssue/GetBudgetActivityInIssueMaterial?materialGroupMasterId=' + materialGroupMasterId
        }).then(function successCallback(response) {
            $scope.BudgetActivityList = response.data;
            $scope.detailModel.GLGeneralInfoId = $scope.BudgetActivityList[0].GLGeneralInfoId;
            $scope.detailModel.BudgetMasterId = $scope.BudgetActivityList[0].BudgetMasterId;
            $scope.detailModel.BudgetName = $scope.BudgetActivityList[0].BudgetName;
            $scope.getActivity($scope.BudgetActivityList[0]);
        });
	};


	$window.onresize = function (event) {

		$scope.actionCompleteSelected3();

	};
	$scope.actionCompleteSelected3 = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#Grid").ejGrid("instance");
				var scrollerwidth = $("#Approved1").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};
	$window.onresize = function (event) {

		$scope.actionCompleteSelected2();

	};
	$scope.actionCompleteSelected2 = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#Grid1").ejGrid("instance");
				var scrollerwidth = $("#Approved2").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};





	
	$window.onresize = function (event) {

		$scope.actionCompleteSelected31();

	};
	$scope.actionCompleteSelected31 = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#Grid22").ejGrid("instance");
				var scrollerwidth = $("#Posting1").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};

	$window.onresize = function (event) {

		$scope.actionCompleteSelected21();

	};
	$scope.actionCompleteSelected21 = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#Grid33").ejGrid("instance");
				var scrollerwidth = $("#Posting2").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};
	$window.onresize = function (event) {

		$scope.actionCompleteSelected44();

	};
	$scope.actionCompleteSelected44 = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#Grid44").ejGrid("instance");
				var scrollerwidth = $("#UnApprovedStock1").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};
	$window.onresize = function (event) {

		$scope.actionCompleteSelected45();

	};
	$scope.actionCompleteSelected45 = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#Grid45").ejGrid("instance");
				var scrollerwidth = $("#UnApprovedStock2").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};

    $scope.popUpDataList = [];
    $scope.popUp = function () {
        $http({
            method: 'GET',
            url: 'Products/InventoryIssue/GetGRNFixedAssetList?materialStorageId=' + $scope.productNew.MaterialStorageId + '&issueDate='+ $scope.productNew.IssueDate
        }).then(function successCallback(response) {
            $scope.popUpDataList = response.data;
            angular.element(document.querySelector('#popUpId')).modal('show');
        });
    }
	

    $scope.IssueReport = function (data) {
        location.href = "Products/InventoryIssue/IssueReport?grnId=" + data.Id;
    };


    $scope.AssetInventoryIssueReport = function (data) {
        location.href = "Products/InventoryIssue/AssetIssueReport?grnId=" + data.Id;
    };



    //#region Asset Issue

    $scope.popUpDataList = [];
    $scope.popUpAssetIssue = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryIssue/GetAssetIssueSlipWithGRN?materialStorageId=' + $scope.productNew.MaterialStorageId
        }).then(function successCallback(response) {
            $scope.popUpDataList = response.data;
            angular.element(document.querySelector('#popUpId')).modal('show');
        });
    }



    $window.onresize = function (event) {

        $scope.popUpDataListScroll();

    };
    $scope.popUpDataListScroll = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#popUpData").ejGrid("instance");
                var scrollerwidth = $("#approved").width();
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
          
        }
    };



    $scope.POPopUp = function () {

        $scope.GetApprovedIssueSlipListGrid();

        angular.element(document.querySelector('#POPopUp1')).modal('show');
    };
    $scope.POPopUpClose = function () {
        angular.element(document.querySelector('#POPopUp1')).modal('hide');
    };




    $scope.GetApprovedIssueSlipList = [];
    $scope.GetApprovedIssueSlipListGrid = function () {
        //debugger;
        try {

            $http({
                method: 'GET',
                url: 'Products/InventoryIssue/GetApprovedIssueSlip',
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.GetApprovedIssueSlipList = response.data;

                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };



    $scope.ViewSlipDetail = function () {
        //debugger;
    

        $scope.$broadcast('show-errors-check-validity');
        if ($scope.productNewForm.$valid) {
            $scope.product = Object.assign({}, $scope.productNew);
            $scope.detailModel = {
                Id: null
                , InventoryReveiveId: null
                , MaterialStorageId: $scope.productNew.MaterialStorageId
                , InventoryMaterialId: null
                , MaterialMasterId: null
                , MaterialMasterName: null
                , ArticleId: null
                , ArticleName: null
                , MaterialTypeName: null
                , OurStyleName: null
                , Description: null
                , MaterialGroupMasterName: null
                , ProductMasterName: null
                , IsOurStyleRequired: false
                , IsProductMstRequired: false
                , FirstCharacteristicsId: null
                , FirstCharacteristicsValueId: null
                , SecondCharacteristicsId: null
                , SecondCharacteristicsValueId: null
                , ThirdCharacteristicsId: null
                , ThirdCharacteristicsValueId: null
                , TransactionQty: null
                , TransactionUoMId: null
                , TransactionUoM: null
                , BaseQty: null
                , BaseUOMId: null
                , BaseUoM: null
                , BaseUoMFactor: null
                , TransactionRate: null
                , TotalQty: 0
                , AvgRate: null
                , InventoryIssueId: $scope.productNew.Id
                , AvgAmount: null
                , PolicyRate: null
                , PolicyAmount: null
                , Policy: null
                , ActivityName: null
                , BudgetMasterId: null
                , ActivityId: null
                , IssueId: null
            };
            $scope.clearCharNames();
            $http.get($scope.path + 'GetApprovedIssueSlipDetails?Id=' + $scope.issueId + '&StorageLocationId=' + $scope.productNew.MaterialStorageId)
                .then(function (response) {
                    //$scope.slipdetailList = response.data;
                    $scope.detailList = response.data;
                });
            // angular.element(document.querySelector('#detailPopUp')).modal('show');
        }

    }


    $scope.recorddoubleclick = function ($event) {
        //debugger;
        var x = $event;
        $scope.issueId = x.data.Id;
        $scope.isuuedate = x.data.AddedDate;
        $scope.POPopUpClose();
    };


    $scope.recorddoubleclick = function ($event) {
        //debugger;
        var x = $event;
        var Id = x.data.Id;
        $scope.issueId = x.data.Id;
        $scope.isuuedate = x.data.AddedDate;
        // var gridObj = $("#GridTest").ejGrid("instance");
        angular.element(document.querySelector('#POPopUp1')).modal('hide');


    }
    //#endregion
    //#region Order Ref
    $scope.masterOrderCustomerList = [];
    $scope.GetMasterOrderByContractList = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/InventoryIssue/GetMasterOrderList',
        }).then(function successCallback(response) {
            $scope.masterOrderCustomerList = response.data;
            //entrydata = copy(searchdata);

        });
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('show');
    }

    $scope.SelectedOrder = function (obj) {
        //debugger;
        //var data = obj.data.ContractId;
        $scope.productNew.OrderRefNo = obj.data.MasterOrderNo;
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('hide');
    }
    $scope.ClearMasterOrder = function () {
        $scope.productNew.OrderRefNo = "";

    };

    $scope.CloseMasterOrder = function () {
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('hide');

    };
    $scope.GetPopUpMasterOrderDetails = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/InventoryIssue/GetMasterOrderDetailsList?MasterOrderId=' + $scope.productNew.OrderRefNo,
        }).then(function successCallback(response) {
            //$scope.productNew.masterOrderCustomerList = response.data;
            $scope.productNew.MasterOrderNo1 = response.data[0].MasterOrderNo;
            $scope.productNew.TotalQty1 = response.data[0].TotalQty;
            $scope.productNew.CustomerName1 = response.data[0].CustomerName;
            $scope.productNew.Contract1 = response.data[0].ContractNo;
            $scope.productNew.MasterLCNo1 = response.data[0].MasterLCNo;
            angular.element(document.querySelector('#MasterOrderPopUp1')).modal('show');

        });

    };
    $scope.CloseMasterOrder1 = function () {
        angular.element(document.querySelector('#MasterOrderPopUp1')).modal('hide');

    };
    //#endregions
    $scope.issueListNew = [];
    $scope.getdataInventoryIssue = function () {
        debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/InventoryIssue/GetAssetInventoryIssueNew',
        }).then(function successCallback(response) {
            $scope.issueListNew = response.data;
        });

    };
    $scope.getdataInventoryIssue(); 

    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/InventoryIssue/AssetIssueReport?grnId=" + data.Id;

    };

    $scope.lst = [];
    $scope.POListDetails = function () {
        //debugger;
        $http({
            method: 'GET',
            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
            url: 'Products/InventoryIssue/MaterialIssueDetailsData1'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            //$scope.detailgrid($scope.lst);
            window.lst = response.data;

        });
    }
    $scope.POListDetails();


    $scope.data1 = $scope.lst;
    $scope.detailTemp = "#tabGridContents";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgrid = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("IssueNo", "equal", parseInt(filteredData), true).take(200));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
            columns: ["CostCenter", "Materials", "Article", "SKU1", "SKU2", "SKU3", "Qty", "UOM", "TransactionRate", "CurrencyName", "TotalMaterialTranAmount", "Comments"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }
}