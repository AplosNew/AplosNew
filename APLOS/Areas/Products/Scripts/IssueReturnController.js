'use strict';
IssueReturnController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function IssueReturnController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = "Issue Return";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/InventoryIssue/';
    $scope.getListUrl = $scope.path + 'GetDataByInventoryIssue';
    $scope.saveUrl = $scope.path + 'CreateIssueReturn';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.currentDate = new Date(Date.now());

    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $scope.product = {
        Id: null
        , ComapnyGroupId: null
        , CompanyId: null
        , PlantId: null
        , PlantName: null
        , EntityId: null
        , EntityName: null
        , MaterialStorageId: null
        , IssueDate: $filter("dateFiltering")(Date.now())
        , Remarks: null
        , EmployeeId: null
        , EmployeeName: null
        , IssueType: 'Revenue'
        , IssueRequestMasterId: null
        , SlipAssetIssueTypeStatus: 'Asset'
        , OrderRefNo: null
        , FromDate1: $filter("dateFiltering")(Date.now())
        , ToDate: null
    };
    $scope.IssueType = 'Revenue';
    $scope.productNew = Object.assign({}, $scope.product);
    //#region Material Issue icon Detail        $scope.POPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.productNew.CostCenterId)) {
            ShowResult('Select Cost center', 'failure');
            return false;
        }
        $scope.GetApprovedIssueSlipListGrid();
        angular.element(document.querySelector('#POPopUp1')).modal('show');
    };
    $scope.POPopUpClose = function () {
        angular.element(document.querySelector('#POPopUp1')).modal('hide');
    };

    $scope.GetArticleList = [];
    $scope.GetApprovedIssueSlipListGrid = function () {
        //debugger;
        try {            $http({                method: 'GET',                url: 'Products/InventoryIssue/IssueSlipMaterialAndArticleList?fromDate=' + $scope.productNew.FromDate1 + '&toDate=' + $scope.productNew.ToDate + '&CostCenterId=' + $scope.productNew.CostCenterId + '&MaterialStorageId=' + $scope.productNew.MaterialStorageId,                dataType: 'JSON'            }).then(function successCallback(response) {                if (response.data.Error == true) {                    ShowResult(response.data.Message, 'failure');                }                else {                    $scope.GetArticleList = response.data;                }            }, function errorCallback(response) {                ShowResult(response.status.Message, 'failure');            });        } catch (e) {            ShowResult(e, 'failure');        }

    };    $scope.lst = [];    $scope.POListDetails = function () {        //debugger;        $http({            method: 'GET',            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData            url: 'Products/InventoryIssue/MaterialIssueDetailsData'        }).then(function successCallback(response) {            $scope.lst = response.data;            //$scope.detailgrid($scope.lst);            window.lst = response.data;        });    }    $scope.POListDetails();    $scope.data1 = $scope.lst;    $scope.detailTemp = "#tabGridContents";    //$scope.detailgrid = "detailGridData(e)";    $scope.detailgrid = function detailGridData(e) {        //debugger;        var filteredData = e.data["Id"];        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("IssueNo", "equal", parseInt(filteredData), true).take(200));        e.detailsElement.find("#detailGrid").ejGrid({            dataSource: data,            columns: ["CostCenter", "StorageLocation", "Materials", "Article", "SKU1", "SKU2", "SKU3", "Qty", "UOM"]//"UOM", "TransactionRate", "CurrencyName", "TotalMaterialTranAmount"        });        e.detailsElement.find(".tabcontrol").ejTab();    }    $scope.detailListForArticle = [];    $scope.recorddoubleclickloadArticleDetails = function ($event) {
        //debugger;
        var x = $event;
        //var Id = x.data.Id;
        var MaterialMasterId1 = "''";
        var ArticleId1 = "''";
        var FirstCharacteristicsValueId1 = "''";
        var SecondCharacteristicsValueId1 = "''";
        var ThirdCharacteristicsValueId1 = "''";
        for (var i = 0; i < $scope.GetArticleList.length; i++) {
            if ($scope.GetArticleList[i].Active === true) {
                MaterialMasterId1 += ",'" + $scope.GetArticleList[i].MaterialMasterId + "'";
                ArticleId1 += ",'" + $scope.GetArticleList[i].ArticleId + "'";
                FirstCharacteristicsValueId1 += ",'" + $scope.GetArticleList[i].FirstCharacteristicsValueId + "'";
                SecondCharacteristicsValueId1 += ",'" + $scope.GetArticleList[i].SecondCharacteristicsValueId + "'";
                ThirdCharacteristicsValueId1 += ",'" + $scope.GetArticleList[i].ThirdCharacteristicsValueId + "'";

            }
        }
        $http({
            method: 'GET',
            url: 'Products/InventoryIssue/IssueSlipMaterialAndArticleListForIssued?MaterialMasterId=' + MaterialMasterId1 + '&ArticleId=' + ArticleId1 + '&FirstCharacteristicsValueId=' + FirstCharacteristicsValueId1 + '&SecondCharacteristicsValueId=' + SecondCharacteristicsValueId1 + '&ThirdCharacteristicsValueId=' + ThirdCharacteristicsValueId1 + '&MaterialStorageId=' + $scope.productNew.MaterialStorageId + '&CostCenterId=' + $scope.productNew.CostCenterId + '&fromDate=' + $scope.productNew.FromDate1 + '&toDate=' + $scope.productNew.ToDate
        }).then(function (response) {
            $scope.detailListForArticle = response.data;
        });
        // $scope.loadArticleData(x.data);

        $scope.POPopUpClose();


    }    $scope.CloseArticlePopUp = function () {
        $scope.POPopUpClose();
    }        $scope.staus = true;
    $scope.enableid = true;
    $scope.Change = function (event, index, x) {
        //debugger;
        if (baseService.isUndefinedOrNull(x.TransactionQty)) {
            ShowResult('Enter the current qty', 'failure');
        }
        else {
            if (event.currentTarget.checked) {
                $scope.index = index;
                //$scope.staus = false;
                x.enableid = false;

                if (x.POQty === (x.GRNRcvQty + x.TransactionQty)) {
                    x.POClosStatus = true;
                }
                else if (x.POQty > (x.GRNRcvQty + x.TransactionQty)) {
                    $scope.PODetailId = x.PODetailId;
                    $scope.message = 'Are you want to close this PO line item?';
                    angular.element(document.querySelector('#ConfirmationForReqClosePopUp')).modal('show');
                }
            }
            else {
                x.enableid = true;
                //$scope.index = index;
                x.POClosStatus = false;
                x.TransactionQty = "";
                x.Balance = x.POQty - x.GRNRcvQty;//parseFloat(x.POQty - x.GRNRcvQty).toFixed(2);
            }
        }

    }    $scope.detailListForArticleNew = [];    $scope.SaveSlipIssue = function () {
        $scope.detailListForArticleNew = [];
        //debugger;
        var gridObj = $("#GridTest1").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];

        for (var i = 0; i < $scope.detailListForArticle.length; i++) {

            if ($scope.detailListForArticle[i].Active === true) {
                $scope.detailListForArticleNew.push($scope.detailListForArticle[i])
            }
        }
        for (var j = 0; j < $scope.detailListForArticle.length; j++) {
            // if ($scope.detailListForArticle[j].Active === true) {
            if ($scope.detailListForArticle[j].InventoryIssueHistoryId === data.InventoryIssueHistoryId) {
                if ((parseFloat($scope.detailListForArticle[j].TransactionQty) + parseFloat($scope.detailListForArticle[j].IssueReturnQty)) <= $scope.detailListForArticle[j].IssuedQty) {

                }
                else {
                    ShowResult("Return qty can not gaterthen Issued Qty");
                    $scope.detailListForArticle[j].TransactionQty = "";
                    return false;
                }

            }

        }

        $scope.productNew.IssueRequestMasterId = $scope.issueId;
        if ($scope.Action === "Save") {
            $http({
                method: 'POST'
                , url: $scope.saveUrl
                , data: {
                    entities: null
                    , specificStockList: $scope.detailListForArticleNew
                    , inventoryIssue: $scope.productNew
                    , IssueTypeStatus: null

                }
                , dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getdataInventoryIssue();
                    $scope.POListDetails();
                    //$scope.productNew.Id = response.data.inventoryIssue.Id;
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
        else if ($scope.Action === "Update") {
            $scope.productNew.Id = $scope.productNew.Id;
            $http({
                method: 'POST'
                , url: $scope.saveUrl
                , data: {
                    entities: null
                    , specificStockList: $scope.detailListForArticleNew
                    , inventoryIssue: $scope.productNew
                    , IssueTypeStatus: null

                }
                , dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getdataInventoryIssue();
                    $scope.POListDetails();
                    $scope.productNew.Id = response.data.inventoryIssue.Id;
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
    };

    $scope.Clear = function () {
        $scope.detailListForArticle = [];
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.product = {};
        $scope.productNew = { FixedAssetOrInventory: 'Inventory', PODepended: false, AlongwithInvoice: false, IssueType: 'Revenue' };
        $scope.detailModel = {};
        $scope.clearCharNames();
        $scope.detailList = [];
        $scope.specificStockList = [];
        $scope.IssueType = 'Revenue';
    }    $scope.GridInventoryIssuedata = [];
    $scope.getdataInventoryIssue = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/InventoryIssue/GetDataByInventoryReturnIssue',
        }).then(function successCallback(response) {
            $scope.GridInventoryIssuedata = response.data;
            //entrydata = copy(searchdata);
        });

    };
    $scope.getdataInventoryIssue(); 
    $scope.recorddoubleclickIssueReturnForUpdate = function ($event) {
        //debugger;
        var x = $event;
        $http({
            method: 'GET',
            url: 'Products/InventoryIssue/IssueReturnForUpdate?Id=' + x.data.Id
        }).then(function (response) {
            $scope.detailListForArticle = response.data;
            $scope.productNew.MaterialStorageId = response.data[0].StorageLocationId;
            $scope.productNew.CostCenterId = response.data[0].CostCenterId;
            $scope.productNew.Id = response.data[0].InventoryIssueReturnId;
            $scope.productNew.Remarks = x.data.Remarks;
        });
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    }    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/InventoryIssue/InventoryIssueReturnReport?grnId=" + data.Id;

    };    //#endregion




































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
    baseService.init($scope.getListUrl, null, null, 'DESC', 'Id', 'Id');
    $scope.getData = function (pageno) {
        //debugger;
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


    $scope.changeType = function (data) {
        $scope.IssueType = data;
    }

    $scope.Get = function (index) {
        //debugger;
        $scope.index = index;
        $scope.product = $scope.issueList[index];
        $scope.productNew = Object.assign({}, $scope.product);
        $scope.materialStockList = [];
        $scope.specificStockList = [];

        getIssueDetailList();

        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };
    $scope.GetDataList = function ($event) {
        //debugger;

        //$scope.index = index;
        // $scope.product = $scope.issueList[index.rowIndex];
        var a = $event;
        var id = a.data;
        $scope.product = a.data;
        $scope.productNew = Object.assign({}, $scope.product);
        $scope.materialStockList = [];
        $scope.specificStockList = [];

        getIssueDetailList();

        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };









 
    //$scope.SavePOPUpConfirm = function () {
    //    $scope.message_confirmation = "Are you sure want to do Auto Issue?";
    //    angular.element(document.querySelector('#confirmSavePopUp')).modal('show');
    //};

    $scope.Save = function () {
        //debugger;
        // $scope.SavePOPUpConfirm();
        if ($scope.detailList.length === 0) {
            ShowResult('Please select Atlest one material');
            return false;
        }
        var UIStatus = $("#SlipAssetIssueUI").val();
        $scope.productNew.IssueRequestMasterId = $scope.issueId;
        if ($scope.Action === "Save") {
            $http({
                method: 'POST'
                , url: $scope.saveUrl
                , data: {
                    entities: $scope.detailList
                    , specificStockList: $scope.specificStockList
                    , inventoryIssue: $scope.productNew
                    , IssueTypeStatus: UIStatus
                }
                , dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.productNew.Id = response.data.inventoryIssue.Id;
                    $scope.getData();
                    $scope.GetDataList();

                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
        else ShowResult('Please issue material', 'failure');
    };


    // #region Details

    $scope.detailPopUp = function () {
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

            angular.element(document.querySelector('#detailPopUp')).modal('show');
        }
    };
    $scope.closeDetaiPopUp = function () {
        $scope.detailModel = {};
        $scope.clearCharNames();
        angular.element(document.querySelector('#detailPopUp')).modal('hide');
    };

    $scope.materialType = ['Asset', 'Consumable', 'Spare', 'RawMaterial'];
    //$scope.setMaterialMasterData
    $scope.selectMaterialByType = function (ob) {
        //debugger;
        if (ob.IsAsset) return ShowResult('Fixed Asset  can not Issue through this Screen .', '', 'materialMasterbyTypePopup');
        if (!ob.hasInventory) return ShowResult('Material stock does not exist.', '', 'materialMasterbyTypePopup');
        $scope.detailModel.MaterialMasterId = ob.Id;
        $scope.detailModel.MaterialMasterName = ob.UserName;
        $scope.detailModel.BaseUOMId = ob.BaseUOMId;
        $scope.detailModel.BaseUoM = ob.BaseUoM;
        $scope.detailModel.OurStyleName = ob.OurStyleName;
        $scope.detailModel.MaterialGroupMasterName = ob.MaterialGroupMasterName;
        $scope.detailModel.MaterialGroupMasterId = ob.MaterialGroupMasterId;
        $scope.detailModel.ProductMasterName = ob.ProductMasterName;
        $scope.detailModel.IsOurStyleRequired = ob.IsOurStyleRequired;
        $scope.detailModel.IsProductMstRequired = ob.IsProductMstRequired;
        $scope.detailModel.TransactionUoMId = ob.BaseUOMId;
        $scope.detailModel.ArticleId = null;;
        $scope.detailModel.ArticleName = null;
        $scope.detailModel.FirstCharacteristicsValueId = null;
        $scope.detailModel.SecondCharacteristicsValueId = null;
        $scope.detailModel.ThirdCharacteristicsValueId = null;

        $scope.hasArticle = ob.HasAttribute;
        $scope.hasSku = ob.WithSKU;
        if (ob.HasAttribute) $scope.getArticleSearchList(ob.Id);
        if (ob.WithSKU) $scope.getCharacteristicsList(ob.Id);
        if (!ob.HasAttribute && !ob.WithSKU) getMaterialStock();

        var mmId = []; mmId.push(ob.Id);
        cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (result) {
            $scope.uoMList = result;
        });
        manualValidation('div_mm', false);
        manualValidation('div_qty', false);
        if ($scope.IssueType == 'Revenue') {
            if (!ob.HasAttribute && !ob.WithSKU) $scope.getBudgetActivityInIssueMaterial(ob.MaterialGroupMasterId);
        }
        $scope.closeMaterialMasterbyTypePopUp();
    };



    $scope.selectarticle = function (ob) {
        //debugger;
        try {
            $scope.detailModel.ArticleId = ob.Id;
            $scope.detailModel.ArticleName = ob.StandardName;
            manualValidation('div_ar', false);
            if (!ob.WithSKU) getMaterialStock();
            if ($scope.IssueType == 'Revenue') {
                if (!ob.WithSKU) $scope.getBudgetActivityInIssueMaterial($scope.detailModel.MaterialGroupMasterId);
            }
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
            if ($scope.detailModel.BudgetMasterId === '' || $scope.detailModel.BudgetMasterId === null || $scope.detailModel.BudgetMasterId === undefined) {
                ShowResult('Budget is required', 'failure', 'detailPopUp');
                return false;
            }
            if ($scope.detailModel.CostCenterId === '' || $scope.detailModel.CostCenterId === null || $scope.detailModel.CostCenterId === undefined) {
                ShowResult('Cost center is required', 'failure', 'detailPopUp');
                return false;
            }
            if ($scope.detailModel.ActivityId === '' || $scope.detailModel.ActivityId === null || $scope.detailModel.ActivityId === undefined) {
                ShowResult('Activity is required', 'failure', 'detailPopUp');
                return false;
            }
            $scope.detailModel.TransactionQty = baseService.isUndefinedOrNull($scope.detailModel.TransactionQty) === true ? 0 : parseFloat($scope.detailModel.TransactionQty);
            if ($scope.detailModel.TransactionQty === 0)
                throw 'Please insert issue qty.';
            else {
                if ($scope.detailModel.TransactionUoMId === $scope.detailModel.BaseUOMId) {
                    if ($scope.detailModel.TransactionQty > parseFloat($scope.detailModel.PostingQuantity))
                        throw 'Issue qty must be less than or equal Ready for Issue Qty.';
                    $scope.detailModel.BaseQty = $scope.detailModel.TransactionQty;
                }
                else {
                    var tQty = parseFloat($scope.detailModel.TransactionQty) * parseFloat($.grep($scope.uoMList, function (item) { return item.Value === $scope.detailModel.TransactionUoMId; })[0].BaseUoMFactor);
                    if (tQty > parseFloat($scope.detailModel.PostingQuantity))
                        throw 'Issue qty must be less than or equal Ready for Issue Qty.';
                    $scope.detailModel.BaseQty = tQty;
                }
            }

            for (var i = 0; i < baseService.arrayLength($scope.detailList); i++) {
                if ($scope.detailList[i].MaterialMasterId === $scope.detailModel.MaterialMasterId &&
                    $scope.detailList[i].ArticleId === $scope.detailModel.ArticleId &&
                    $scope.detailList[i].FirstCharacteristicsValueId === $scope.detailModel.FirstCharacteristicsValueId &&
                    $scope.detailList[i].SecondCharacteristicsValueId === $scope.detailModel.SecondCharacteristicsValueId &&
                    $scope.detailList[i].ThirdCharacteristicsValueId === $scope.detailModel.ThirdCharacteristicsValueId)
                    throw 'This material already issued.';
            }
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
            $scope.detailModel.BaseUoMFactor = $.grep($scope.uoMList, function (item) { return item.Value === $scope.detailModel.TransactionUoMId; })[0].BaseUoMFactor;
            $scope.detailModel.TransactionUoM = angular.element("#issueUoM :selected").text();
            $http({
                method: 'Post'
                , url: $scope.path + 'getInvMaterialId'
                , data: $scope.detailModel
                , dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    $scope.detailModel.InventoryMaterialId = response.data;
                    var row = Object.assign({}, $scope.detailModel);
                    $scope.detailList.push(row);
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };

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

    $scope.getCharacteristicsList = function (id) {
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
                $scope.detailModel.FirstCharacteristicsValueId = $scope.characteristicsList[0].CharacteristicsValueId;
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
                $scope.detailModel.SecondCharacteristicsValueId = $scope.characteristicsList[1].CharacteristicsValueId;
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
                $scope.detailModel.ThirdCharacteristicsValueId = $scope.characteristicsList[2].CharacteristicsValueId;
            }
        });
    };
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
        $http.get($scope.path + 'GetIssueDetailByIssueId?issueId=' + $scope.productNew.Id)
            .then(function (response) {
                $scope.detailList = response.data;
                $scope.detailModel.IssueId = $scope.detailList[0].InventoryIssueId;
            });
    }

    // #endregion Details

    // #region Specific Stock

    $scope.materialStockList = [];
    $scope.specificStockList = [];
    $scope.getSpecificMaterialStock = function (data, index) {
        $scope.index = index;
        $http({
            method: 'POST'
            , url: $scope.path + 'GetSpecificMaterialStock'
            , data: { entity: data, issueDate: $scope.productNew.IssueDate }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.materialStockList = response.data;

            for (var i = 0; i < baseService.arrayLength($scope.specificStockList); i++) {
                var row = $scope.specificStockList[i];
                for (var t = 0; t < baseService.arrayLength($scope.materialStockList); t++) {
                    var newRow = $scope.materialStockList[t];
                    if (newRow.InventoryReceiveDetailId === row.InventoryReceiveDetailId) {
                        newRow.Flag = true;
                        newRow.RequisitionQty = row.RequisitionQty;
                        break;
                    }
                }
            }

            angular.element(document.querySelector('#stockPopUp')).modal('show');
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.addMaterialStock = function () {
        //debugger;
        try {
            qtyValidation($scope.materialStockList);
            validationWithTotal($scope.materialStockList);
            for (var i = baseService.arrayLength($scope.specificStockList) - 1; i >= 0; i--) {
                var row = $scope.specificStockList[i];
                for (var t = 0; t < baseService.arrayLength($scope.materialStockList); t++) {
                    var newRow = $scope.materialStockList[t];
                    if (row.InventoryReceiveDetailId === newRow.InventoryReceiveDetailId) { // update or delete
                        if (newRow.Flag) row.RequisitionQty = newRow.RequisitionQty;
                        else $scope.specificStockList.splice(i, 1);
                    }
                }
            }
            for (var n = 0; n < baseService.arrayLength($scope.materialStockList); n++) { // add
                var nRow = $scope.materialStockList[n];
                nRow.BaseQty = $scope.materialStockList[n].BaseIssueQty;
                if (!baseService.valueCheckInList($scope.specificStockList, 'InventoryReceiveDetailId', nRow.InventoryReceiveDetailId) && nRow.Flag)

                    $scope.specificStockList.push(nRow);
            }
            //$scope.detailList[$scope.index].TransactionQty = issueQty;
            angular.element(document.querySelector('#stockPopUp')).modal('hide');
            CloseModalShowResult();
        } catch (e) {
            ShowResult(e, 'failure', 'stockPopUp');
        }
    };

    //$scope.calculateBaseQty = function (data) {
    //    data.BaseIssueQty = parseFloat(data.BaseUoMFactor * data.RequisitionQty).toFixed(4);
    //}

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
                    else totalQty += parseFloat(list[i].RequisitionQty).toFixed(2);
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

    //$scope.PostingStockList = [];
    //$scope.getPostingStock = function (data) {
    //    $http({
    //        method: 'POST'
    //        , url: $scope.path + 'GetPostingStockDetail'
    //        , data: { entity: data, issueDate: $scope.productNew.IssueDate }
    //        , dataType: 'JSON'
    //    }).then(function (response) {
    //        $scope.PostingStockList = response.data;
    //        angular.element(document.querySelector('#PostingStockPopUp')).modal('show');
    //    }), function (response) {
    //        ShowResult(response.data.Message, 'failure');
    //    };
    //};
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

    $scope.UnApprovedStockList = [];
    $scope.getUnApprovedStock = function (data) {
        $http({
            method: 'POST'
            , url: $scope.path + 'GetUnApprovedStockDetail'
            , data: { entity: data, issueDate: $scope.productNew.IssueDate }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.UnApprovedStockList = response.data;
            angular.element(document.querySelector('#UnApprovedStockPopUp')).modal('show');
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.closeUnApprovedStockPopUp = function () {
        angular.element(document.querySelector('#UnApprovedStockPopUp')).modal('hide');
    };

    $scope.UnApprovedStockDetailBeyondIssueDateList = [];
    $scope.getUnApprovedStockDetailBeyondIssueDate = function (data) {
        $http({
            method: 'POST'
            , url: $scope.path + 'GetUnApprovedStockDetailBeyondIssueDate'
            , data: { entity: data, issueDate: $scope.productNew.IssueDate }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.UnApprovedStockDetailBeyondIssueDateList = response.data;
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

    //$scope.redirectTab = function () {
    //    if ($scope.tabForm1.$invalid) {
    //        $scope.setTab(1);
    //    }
    //    else if ($scope.tabForm2.$invalid) {
    //        $scope.setTab(2);
    //    }
    //};
    $scope.IssueReport = function (data) {
        location.href = "Products/InventoryIssue/IssueReport?grnId=" + data.Id;
    };




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
        $scope.setSelectedforGL(data);
    };

    $scope.addRow = function (data) {
        $scope.detailModel.GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.detailModel.BudgetMasterId = data.BudgetMasterId;
        $scope.detailModel.ActivityId = data.ActivityId;
        $scope.detailModel.BudgetName = data.BudgetName;
        $scope.getActivity(data);
    };
    $scope.activityList = [];
    $scope.getActivity = function (data) {
        cboService.getBudgetMasterActivityCbo(data.BudgetMasterId, function (result) {
            $scope.detailModel.ActivityId = null;
            $scope.activityList = [];
            $scope.activityList = result;
            $scope.detailModel.ActivityId = data.ActivityId;

        });
    };
    $scope.searchglByList = [
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

    $scope.glListParameters = {
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
    $scope.GetCOAICodeList = function () {
        $scope.GLUrl1 = "Accounts/glitem/GetExpenseTypeGLBudgetActivityList";
        $scope.GetCOAICodeListData = function (pageno) {

            baseService.paginationBase($scope.GLUrl1, pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetCOAICodeListData();
    };

    $scope.closeCOAICodeListPopUp = function () {
        if ($scope.productNew.IssueType === 'Capital')
            angular.element(document.querySelector("#IssueAUCGLPopUp")).modal("hide");
        else
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
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
        $scope.getActivity(data);
    };

    $scope.changeType = function (data) {
        $scope.IssueType = data;
    }

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

    //$scope.recorddoubleclick = function ($event) {
    //    //debugger;
    //    var x = $event;
    //    var Id = x.data.Id;
    //    $scope.issueId = x.data.Id;
    //    $scope.isuuedate = x.data.AddedDate;
    //    // var gridObj = $("#GridTest").ejGrid("instance");
    //    angular.element(document.querySelector('#POPopUp1')).modal('hide');


    //}


    //#region SlipWise Issue Code----








    //function ($event) {    //   //debugger;    //   var x = $event;    //   var Id = x.data.Id;    //   //alert('Id'+Id);    //   $scope.productNew = x.data;    //   $scope.productId = "";


    $scope.slipdetailList = [];


    $scope.qtyFunc = function (x) {
        //debugger;
        // alert('qtyalert');
        for (var i = 0; i < $scope.slipdetailList.length; i++) {

            if (x.TransactionQty > $scope.slipdetailList[i].PostingQty) {
                ShowResult("Issue qty must be less than or equal Ready for Issue Qty");
                return false;
                //throw 'Issue qty must be less than or equal Ready for Issue Qty.';
            }



        }

    }


    $scope.ViewSlipDetail = function () {
        //debugger;
        //if ($scope.issueId === '' || $scope.issueId === null || $scope.issueId === undefined) {
        //    ShowResult("Please select Slip Id");
        //    return false;
        //}
        //else if ($scope.productNew.MaterialStorageId === '' || $scope.productNew.MaterialStorageId === null || $scope.productNew.MaterialStorageId === undefined) {
        //    ShowResult("Please select StorageLocation ");
        //    return false;
        //}

        //else if ($scope.productNew.IssueDate === '' || $scope.productNew.IssueDate === null || $scope.productNew.IssueDate === undefined) {
        //    ShowResult("Please select Issue Date ");
        //    return false;
        //}

        //else if ($scope.productNew.EmployeeName === '' || $scope.productNew.EmployeeName === null || $scope.productNew.EmployeeName === undefined) {
        //    ShowResult("Please select Employee ");
        //    return false;
        //}


        //else if ($scope.productNew.EntityId === '' || $scope.productNew.EntityId === null || $scope.productNew.EntityId === undefined) {
        //    ShowResult("Please select Entity ");
        //    return false;
        //}

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

    $scope.materialStockList = [];
    $scope.specificStockList = [];
    //debugger;
    $scope.getSpecificMaterialStockForSlipIssue = function (data, index) {




        for (var i = 0; i < $scope.detailList.length; i++) {
            if ($scope.detailList[i].TransactionQty > $scope.detailList[i].PostingQty) {
                ShowResult("Issue qty can not gaterthen  Ready for issue Qty");
                return false;
            }


        }
        for (var i = 0; i < $scope.detailList.length; i++) {
            if ($scope.detailList[i].TransactionQty > $scope.detailList[i].RequestedQty) {
                ShowResult("Issue qty can not gaterthen Requested Qty");
                return false;
            }
        }

        $scope.index = index;
        $http({
            method: 'POST'
            , url: $scope.path + 'GetSpecificMaterialStock'
            , data: { entity: data, issueDate: $scope.productNew.IssueDate }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.materialStockList = response.data;

            for (var i = 0; i < baseService.arrayLength($scope.specificStockList); i++) {
                var row = $scope.specificStockList[i];
                for (var t = 0; t < baseService.arrayLength($scope.materialStockList); t++) {
                    var newRow = $scope.materialStockList[t];
                    if (newRow.InventoryReceiveDetailId === row.InventoryReceiveDetailId) {
                        newRow.Flag = true;
                        newRow.RequisitionQty = row.RequisitionQty;
                        break;
                    }
                }
            }

            angular.element(document.querySelector('#stockPopUp')).modal('show');
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };



    $scope.popUp = function (index) {
        //debugger;
        $scope.customerInvoiceGLList = [];
        //baseService.setCurrentPage("cOAICodeList");
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase("Accounts/GLItem/GetAllGLBudgetActivityPostingAutomaticOnly", pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure", "GLPopUp");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.GetCOAICodeListData();
        $scope.issueSlipDetailIndex = index;
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };

    $scope.closeCOAICodeListPopUpSelected = function (x) {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };


    $scope.setSelectedforGL = function (data) {
        //debugger;
        $scope.detailList[$scope.issueSlipDetailIndex].GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.detailList[$scope.issueSlipDetailIndex].BudgetMasterId = data.BudgetMasterId;
        $scope.detailList[$scope.issueSlipDetailIndex].ExpenseActivityId = data.ActivityId;
        $scope.detailList[$scope.issueSlipDetailIndex].ActivityName = data.GLGeneralInfoCode + '-' + data.ActivityName;
        $scope.detailList[$scope.issueSlipDetailIndex].BudgetName = data.BudgetName;
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };


    //#endregion






    //#region Slip Asset Issue

    $scope.POPopUpAssetIssue = function () {

        $scope.GetAssetApprovedIssueSlipListGrid();

        angular.element(document.querySelector('#POPopUp1')).modal('show');
    };
    $scope.POPopUpClose = function () {
        angular.element(document.querySelector('#POPopUp1')).modal('hide');
    };


    $scope.GetAssetApprovedIssueSlipList = [];
    $scope.GetAssetApprovedIssueSlipListGrid = function () {
        //debugger;
        try {            $http({                method: 'GET',                url: 'Products/InventoryIssue/GetAssetIssueSlip',                dataType: 'JSON'            }).then(function successCallback(response) {                if (response.data.Error == true) {                    ShowResult(response.data.Message, 'failure');                }                else {                    $scope.GetAssetApprovedIssueSlipList = response.data;                }            }, function errorCallback(response) {                ShowResult(response.status.Message, 'failure');            });        } catch (e) {            ShowResult(e, 'failure');        }

    };

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


    //$scope.recorddoubleclick = function ($event) {
    //    //debugger;
    //    var x = $event;
    //    $scope.issueId = x.data.Id;
    //    $scope.isuuedate = x.data.AddedDate;
    //    $scope.POPopUpClose();
    //};


    //$scope.recorddoubleclick = function ($event) {
    //    //debugger;
    //    var x = $event;
    //    var Id = x.data.Id;
    //    $scope.issueId = x.data.Id;
    //    $scope.isuuedate = x.data.AddedDate;
    //    // var gridObj = $("#GridTest").ejGrid("instance");
    //    angular.element(document.querySelector('#POPopUp1')).modal('hide');


    //}
    //#endregion









}