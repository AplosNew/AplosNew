'use strict';
inventoryScrapController.$inject = ['accountService','$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function inventoryScrapController(accountService,$window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = "Inventory Scrap";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.partyType = "Customer";
    $scope.path1 = 'Products/PurchaseOrder/';
    $scope.path3 = 'Products/Requisition/';
    $scope.path = 'Products/InventoryIssue/';
    $scope.getListUrl = $scope.path + 'GetDataByInventoryScrap';
    $scope.saveUrl = $scope.path + 'InventoryScrapCreate';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.currentDate = new Date(Date.now());
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    //#region notification setting

    $scope.NotificationSettingStatus = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/InventoryIssue/NotificationSettingScrap',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.NotificationSetting = response.data;
            $scope.CheckedByStatusForNoti = $scope.NotificationSetting[0].RequiredChecking;
            $scope.ApprovedByStatusForNoti = $scope.NotificationSetting[0].RequiredApproval;
            $scope.GetCheckedByAndApprovedBy1();
            if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === false) {
                $scope.productNew.labelCheckAndApproved = 'To be checked by';
            }
            else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true) {
                $scope.productNew.labelCheckAndApproved = 'To be approved by';
            }
            else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true) {
                $scope.productNew.labelCheckAndApproved = 'To be checked by';
            }
            //else {
            //    $scope.productNew.labelCheckAndApproved = 'To be checked/approved by';
            //}

        });
    }
    $scope.NotificationSettingStatus();
    $scope.GetCheckedByAndApprovedBy1 = function () {
        //debugger;

        if (!baseService.isUndefinedOrNull($scope.CheckedByStatusForNoti) && !baseService.isUndefinedOrNull($scope.ApprovedByStatusForNoti)) {
            $http({
                method: 'GET',
                url: 'Products/InventoryIssue/GetCheckedByAndApprovedBYScrap?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.checkedByList = response.data;
            });

        }
        else {

        }

    }




    //#endregion





    //$scope.searchByList = [
    //    {
    //        value: 'Id'
    //        , name: 'Issue No'
    //    },
    //    {
    //        value: 'MaterialStorage'
    //        , name: 'Storage Location'
    //    },
    //    {
    //        value: 'IssueDate'
    //        , name: 'Issue Date'
    //    }
    //];
    //baseService.init($scope.getListUrl, null, null, 'DESC', 'Id', 'Id');
    //$scope.getData = function (pageno) {
    //    //debugger;
    //    baseService.pagination(pageno)
    //        .then(function (result) {
    //            $scope.issueList = [];
    //            $scope.issueList = result.Rows;

    //        }, function () {
    //            ShowResult(commonMessage.NetworkError, 'failure');
    //        }).finally(function () {
    //        });
    //};
    //$scope.getData();

    $http({
        method: 'GET',
        url: 'Materials/MaterialStorage/getcbo'
    }).then(function (response) {
        $scope.storageList = response.data;
    });

    $scope.GridInventoryScrapsdata = [];
    $scope.getdataInventoryScrap = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/InventoryIssue/GetDataByInventoryScrap?tabType=' + $scope.tabType,
        }).then(function successCallback(response) {
            $scope.GridInventoryScrapsdata = response.data;
            //entrydata = copy(searchdata);
        });

    };
    //$scope.getdataInventorySales();

    $scope.lst = [];    $scope.SalesDetails = function () {        //debugger;        $http({            method: 'GET',            //url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData            url: 'Products/InventoryIssue/MaterialScrapDetails'        }).then(function successCallback(response) {            $scope.lst = response.data;            //$scope.detailgrid($scope.lst);            window.lst = response.data;        });    }    $scope.SalesDetails();    $scope.data1 = $scope.lst;    $scope.detailTemp = "#tabGridContents";    //$scope.detailgrid = "detailGridData(e)";    $scope.detailgrid = function detailGridData(e) {        //debugger;        var filteredData = e.data["Id"];        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("IssueNo", "equal", parseInt(filteredData), true).take(200));        e.detailsElement.find("#detailGrid").ejGrid({            dataSource: data,            columns: ["Materials", "Article", "SKU1", "SKU2", "SKU3", "Qty", "UOM", "Comments"]        });        e.detailsElement.find(".tabcontrol").ejTab();    }

    $scope.product = {
        Id: null
        , ComapnyGroupId: null
        , CompanyId: null
        , PlantId: null
        , PlantName: null
        , EntityId: null
        , EntityName: null
        , MaterialStorageId: null
        , ScrapDate: null
        , Remarks: null
        , EmployeeId: null
        , EmployeeName: null
        , IssueType: 'Revenue'
        , IssueRequestMasterId: null
        , SlipAssetIssueTypeStatus: 'Asset'
        , OrderRefNo: null
        , PartyId: null
        , PartyName: null
        , CheckedBy: null
        , CheckedByStatus: null
        , ApprovedBy: null
        , ApprovedByStatus: null
        , CustomerId: null
        , ChangeInvoicingStateId: null
        , PlantStateId: null
        , InvoicingPartyPlantId: null
        , DeliveryPartyPlantId: null
        , InvoicingByAddress: null
        , DeliveryByAddress: null
        , InvoicingState: null
        , InvoicingGSTIN: null
        , DeliveryState: null
        , DeliveryGSTIN: null
        , InvoicingStateId: null
        , ToCurrencyRate: null
        , DocRefNo: null
        , DocDate: null
        , NoteForAccounts: null
        , CurrencyId: null
    };
    $scope.IssueType = 'Revenue';
    $scope.productNew = Object.assign({}, $scope.product);

    $scope.changeType = function (data) {
        $scope.IssueType = data;
    }

    

    $scope.currencyList = [];
    //cboService.getCboTransactionCurrencyByCompany('', function (result) {
    //    $scope.currencyList = result;


    //});
    $scope.loadCurrency=function() {
        ////debugger;
        cboService.getCompanyGroupCurrencyCbo(null, function (result) {
            $scope.currencyList = result;
            $scope.productNew.CurrencyId = $scope.selectBaseCurrency();
            for (var i = 0; i < baseService.arrayLength($scope.currencyList); i++) {
                if ($scope.currencyList[i].Value === $scope.productNew.CurrencyId) {
                    $scope.productNew.CurrencyId = $scope.currencyList[i].Value;

                }
            }
            $scope.getToCurrencyRate();


        });
    }
    $scope.loadCurrency();


    //$scope.getToCurrencyRate = function () {
    //    //debugger;
    //    if (baseService.isUndefinedOrNull($scope.productNew.DocDate)) {
    //        $scope.productNew.ToCurrencyRate = 1;
    //        return;
    //    }
    //    $http.get($scope.path1 + 'GetToCurrencyRate?currencyId=' + $scope.productNew.CurrencyId + '&baseCurrencyId=' + $scope.productNew.BaseCurrencyId + '&docDate=' + $filter('dateFiltering')($scope.productNew.DocDate))
    //        .then(function (response) {
    //            if (parseFloat(response.data) === 0)
    //                $scope.productNew.ToCurrencyRate = 1;
    //            else
    //                $scope.productNew.ToCurrencyRate = response.data;
    //        });
    //};

    $scope.getToCurrencyRate = function () {
        //debugger;
        //if (baseService.isUndefinedOrNull($scope.productNew.DocDate)) {s
        //    $scope.productNew.ToCurrencyRate = 1;
        //    return;
        //}
        $http.get($scope.path3 + 'GetToCurrencyRate?currencyId=' + $scope.productNew.CurrencyId)
            .then(function (response) {
                if (parseFloat(response.data) === 0) {


                    $scope.productNew.ToCurrencyRate = 1;
                    //$scope.productNew.CurrencyName = angular.element("#currency :selected").text();
                }
                else {


                    $scope.productNew.ToCurrencyRate = response.data;
                    //$scope.productNew.CurrencyName = angular.element("#currency :selected").text();
                }
            });
    };








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
        $scope.product.IssueType = a.data.issuetype;

        $scope.productNew = Object.assign({}, $scope.product);
        $scope.materialStockList = [];
        $scope.specificStockList = [];
        getIssueDetailList();
        $scope.GettaxAfterSave(a.data.Id);
        if (baseService.isUndefinedOrNull(a.data.CheckedBy) && !baseService.isUndefinedOrNull(a.data.AuthorizedBy)) {
            $scope.CheckedByStatusForNoti = false;
            $scope.ApprovedByStatusForNoti = true;
            $scope.productNew.CheckedBy = a.data.ApprovedById;
        }
        else if (!baseService.isUndefinedOrNull(a.data.CheckedBy) && !baseService.isUndefinedOrNull(a.data.AuthorizedBy)) {
            $scope.CheckedByStatusForNoti = true;
            $scope.ApprovedByStatusForNoti = true;
            $scope.productNew.CheckedBy = a.data.CheckedById;
        }

        $scope.GetCheckedByAndApprovedBy1();


        if (baseService.isUndefinedOrNull(a.data.CheckedBy) && !baseService.isUndefinedOrNull(a.data.ApprovedBy)) {

            $scope.productNew.CheckedBy = a.data.ApprovedById;
            $scope.productNew.labelCheckAndApproved = 'To be approved by';
        }
        else if (!baseService.isUndefinedOrNull(a.data.CheckedBy) && baseService.isUndefinedOrNull(a.data.ApprovedBy)) {

            $scope.productNew.CheckedBy = a.data.CheckedBy;
            $scope.productNew.labelCheckAndApproved = 'To be checked by';
        }

        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };






    


    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/InventoryIssue/InventoryScrapReportPrint?grnId=" + data.Id;

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
        else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
            ShowResult("Please select to be approved by", 'failure');
            return false;
        }
        else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
            ShowResult("Please select to be checked by", 'failure');
            return false;
        }
        var UIStatus = $("#SlipAssetIssueUI").val();
        $scope.productNew.IssueRequestMasterId = $scope.issueId;
        $scope.productNew.CustomerId = $scope.productNew.PartyId;
        if ($scope.Action === "Save") {
            $http({
                method: 'POST'
                , url: $scope.saveUrl
                , data: {
                    entities: $scope.detailList
                    , specificStockList: $scope.specificStockList
                    , inventoryIssue: $scope.productNew
                    , IssueTypeStatus: UIStatus,
                    'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
                    'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti                    
                }
                , dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();                  
                    $scope.productNew.Id = response.data.inventoryIssue.Id;
                    $scope.getdataInventoryScrap();
                    $scope.getData();
                    $scope.GetDataList();         
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
        else ShowResult('Please issue material', 'failure');
    };
    $scope.SaveSlipIssue = function () {
        var UIStatus = $("#SlipAssetIssueUI").val();
        if (UIStatus==='Asset')
        {
            if ($scope.materialStockList.length === 0)
                {
                ShowResult('Please select Specific GRN');
                return false;
            }
        }
        //debugger;
        if ($scope.detailList.length === 0) {
            ShowResult('Please select Atlest one material');
            return false;
        }
        //debugger;
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
                    $scope.getData();
                    $scope.productNew.Id = response.data.inventoryIssue.Id;
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
        else ShowResult('Please issue material', 'failure');
    };

    $scope.Clear = function () {
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
                , IssueId: null
                , CostCenterId: null
                , CountryName: null
                , IsSpecific: false
                , Comments: null
                , TaxAmount: null
            };
            $scope.clearCharNames();
            $scope.detailModel.CostCenterId = $scope.CostCenterIdTemp;
         
            angular.element(document.querySelector('#detailPopUp')).modal('show');
        }
    };
    $scope.closeDetaiPopUp = function () {
        //debugger;
        $scope.CostCenterIdTemp = $scope.detailModel.CostCenterId;
        $scope.detailModel = {};
        $scope.clearCharNames();
        angular.element(document.querySelector('#detailPopUp')).modal('hide');
    };
    $scope.CountryLoadData = function () {
        $scope.countryList = [];
        $http({
            method: 'POST',
            url: 'Products/inventoryIssue/CountryLoad',//?entity=' + $scope.detailModel, 
            data: { entity: $scope.detailModel},
            dataType: 'JSON'
        }).then(function (response) {          
            $scope.countryList = response.data;
        });
    }

    //$scope.CountryLoadData();

    
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
        $scope.detailModel.IsOriginApplicable = ob.IsOriginApplicable;
       
        $scope.hasArticle = ob.HasAttribute;
        $scope.hasSku = ob.WithSKU;
        if (ob.HasAttribute) $scope.getArticleSearchList(ob.Id);
        if (ob.WithSKU) $scope.getCharacteristicsList(ob.Id);
        if (!ob.HasAttribute && !ob.WithSKU)
            getMaterialStock();
        $scope.CountryLoadData();
        getTaxCategoryList(ob.HSNCodeId);
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

    //function getTaxCategoryList(hsnCodeId) {
    //    $scope.taxCategoryList = [];
    //    $http({
    //        method: 'GET'
    //        , url: $scope.path1 + 'GetTaxCategoryList?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId
    //    }).then(function (response) {
    //        $scope.taxCategoryList = response.data;
    //    });
    //}
    function getTaxCategoryList(hsnCodeId) {
        $http({
            method: 'GET',
            url: 'Accounts/TaxCategory/GetTaxCategoryList?partyPlantId=' + $scope.productNew.InvoicingPartyPlantId + '&hsnCodeId=' + hsnCodeId
        }).then(function (response) {
            $scope.materialtaxCategoryList = response.data;
            // $scope.sevtaxCategoryList = response.data;
        });
    }
    $scope.selectarticle = function (ob) {
        //debugger;
        try {
            $scope.detailModel.ArticleId = ob.Id;
            $scope.detailModel.ArticleName = ob.StandardName;
            manualValidation('div_ar', false);
            if (!ob.WithSKU)
                getMaterialStock();
            $scope.CountryLoadData();
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
        $scope.CountryLoadData();
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
            //if ($scope.detailModel.CostCenterId === '' || $scope.detailModel.CostCenterId === null || $scope.detailModel.CostCenterId === undefined) {
            //    ShowResult('Cost center is required', 'failure', 'detailPopUp');
            //    return false;
            //}
            if ($scope.detailModel.ActivityId === '' || $scope.detailModel.ActivityId === null || $scope.detailModel.ActivityId === undefined) {
                ShowResult('Activity is required', 'failure', 'detailPopUp');
                return false;
            }
            if ($scope.detailModel.IsOriginApplicable === true) {
                if (baseService.isUndefinedOrNull($scope.detailModel.CountryId)) {
                    ShowResult('Please select the country', 'failure', 'detailPopUp');
                    return false;
                }
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
                if ($scope.detailModel.MaterialMasterId === $scope.detailList[i].MaterialMasterId &&
                    $scope.detailModel.ArticleId === $scope.detailList[i].ArticleId &&
                    $scope.detailModel.FirstCharacteristicsValueId === $scope.detailList[i].FirstCharacteristicsValueId &&
                    $scope.detailModel.SecondCharacteristicsValueId === $scope.detailList[i].SecondCharacteristicsValueId &&
                    $scope.detailModel.ThirdCharacteristicsValueId === $scope.detailList[i].ThirdCharacteristicsValueId &&
                    $scope.detailModel.CountryId === $scope.detailList[i].CountryId) {
                    //return ShowResult('This material already received');
                    throw 'This material already received';
                }
            }
            //for (var i = 0; i < baseService.arrayLength($scope.detailList); i++) {
            //    if ($scope.detailList[i].FirstCharacteristicsValueId === undefined)
            //        $scope.detailList[i].FirstCharacteristicsValueId = null;
            //    if ($scope.detailList[i].SecondCharacteristicsValueId === undefined)
            //        $scope.detailList[i].SecondCharacteristicsValueId = null;
            //    if ($scope.detailList[i].ThirdCharacteristicsValueId === undefined)
            //        $scope.detailList[i].ThirdCharacteristicsValueId = null;
            //    if (baseService.isUndefinedOrNull($scope.detailList[i].MaterialMasterId) === baseService.isUndefinedOrNull($scope.detailModel.MaterialMasterId) &&
            //        baseService.isUndefinedOrNull($scope.detailList[i].ArticleId) === baseService.isUndefinedOrNull($scope.detailModel.ArticleId) &&
            //        baseService.isUndefinedOrNull($scope.detailList[i].FirstCharacteristicsValueId) === baseService.isUndefinedOrNull($scope.detailModel.FirstCharacteristicsValueId) &&
            //        baseService.isUndefinedOrNull($scope.detailList[i].SecondCharacteristicsValueId) === baseService.isUndefinedOrNull($scope.detailModel.SecondCharacteristicsValueId) &&
            //        baseService.isUndefinedOrNull($scope.detailList[i].ThirdCharacteristicsValueId) === baseService.isUndefinedOrNull($scope.detailModel.ThirdCharacteristicsValueId) &&
            //        baseService.isUndefinedOrNull($scope.detailList[i].CountryId) === baseService.isUndefinedOrNull($scope.detailModel.CountryId))
            //        throw 'This material already issued.';
            //}
            $scope.detailModel.FirstCharacteristicsId = $scope.char1.CharacteristicsId;
            $scope.detailModel.FirstCharacteristicsValueId = $scope.char1.CharacteristicsValueId;
            $scope.detailModel.FirstCharacteristicText = $scope.char1.FreeText;

            $scope.detailModel.SecondCharacteristicsId = $scope.char2.CharacteristicsId;
            $scope.detailModel.SecondCharacteristicText = $scope.char2.FreeText;
            $scope.detailModel.SecondCharacteristicsValueId = $scope.char2.CharacteristicsValueId;

            $scope.detailModel.ThirdCharacteristicsId = $scope.char3.CharacteristicsId;
            $scope.detailModel.ThirdCharacteristicText = $scope.char3.FreeText;
            $scope.detailModel.ThirdCharacteristicsValueId = $scope.char3.CharacteristicsValueId;

            $scope.detailModel.IssueDate = $scope.productNew.IssueDate;
            $scope.detailModel.Remarks = $scope.productNew.Remarks;
            $scope.detailModel.EmployeeId = $scope.productNew.EmployeeId;
            $scope.detailModel.CountryId = $scope.detailModel.CountryId;
            $scope.detailModel.CountryName = $scope.detailModel.CountryName;
            $scope.detailModel.IsSpecific = false;
            $scope.detailModel.BaseUoMFactor = $.grep($scope.uoMList, function (item) { return item.Value === $scope.detailModel.TransactionUoMId; })[0].BaseUoMFactor;
            $scope.detailModel.TransactionUoM = angular.element("#issueUoM :selected").text();
            $scope.detailModel.TaxAmount = $scope.detailModel.TotalTaxAmount;
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
            url: $scope.path + 'GetStockScrap',
            data: { entity: $scope.detailModel, issueDate: $scope.productNew.ScrapDate },
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


    
    $scope.getMaterialStockCountryWise = function (id) {

        //debugger;
        $scope.detailModel.CountryName = $("#CountryName option:selected").text();
        $http({
            method: 'POST',
            url: $scope.path + 'GetStockCountryWise',
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
        $http.get($scope.path + 'GetScrapDetailByIssueId?issueId=' + $scope.productNew.Id)
            .then(function (response) {
                $scope.detailList = response.data;
                $scope.detailModel.IssueId = $scope.detailList[0].InventoryScrapId;
            });
    }

    // #endregion Details

    // #region Specific Stock

    $scope.materialStockList = [];
    $scope.specificStockList = [];
    $scope.getSpecificMaterialStock = function (data, index) {
        //debugger;
        $scope.index = index;
        $http({
            method: 'POST'
            , url: $scope.path + 'GetSpecificMaterialStock'
            , data: { entity: data, issueDate: $scope.productNew.ScrapDate }
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
                    //$scope.detailModel.IsSpecific = true;
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
    //-----------------------------------------------------------------------------------------------------Need to take decision-----------------------------
    $scope.getBudgetActivityInIssueMaterial = function (materialGroupMasterId) {
        $http({
            method: "GET",
            url: 'Products/InventoryIssue/GetBudgetActivityInScrapMaterial?materialGroupMasterId=' + materialGroupMasterId
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




    //#region SlipWise Issue Code----
    



    $scope.recorddoubleclick = function ($event) {
        //debugger;
        var x = $event;
        $scope.issueId = x.data.Id;
        $scope.isuuedate = x.data.AddedDate;
        $scope.POPopUpClose();
    };



    //function ($event) {    //   //debugger;    //   var x = $event;    //   var Id = x.data.Id;    //   //alert('Id'+Id);    //   $scope.productNew = x.data;    //   $scope.productId = "";


    $scope.slipdetailList = [];
    $scope.recorddoubleclick = function ($event) {
        //debugger;
        var x = $event;
        var Id = x.data.Id;
        $scope.issueId = x.data.Id;
        $scope.isuuedate = x.data.AddedDate;
        // var gridObj = $("#GridTest").ejGrid("instance");
        angular.element(document.querySelector('#POPopUp1')).modal('hide');


    }

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
                , CostCenterId:null
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



    //#region Material Issue icon Detail    $scope.POPopUp = function () {

        $scope.GetApprovedIssueSlipListGrid();
        angular.element(document.querySelector('#POPopUp1')).modal('show');
    };
    $scope.POPopUpClose = function () {
        angular.element(document.querySelector('#POPopUp1')).modal('hide');
    };




    $scope.GetApprovedIssueSlipList = [];
    $scope.GetApprovedIssueSlipListGrid = function () {
        //debugger;
        try {            $http({                method: 'GET',                url: 'Products/InventoryIssue/GetApprovedIssueSlip',                dataType: 'JSON'            }).then(function successCallback(response) {                if (response.data.Error == true) {                    ShowResult(response.data.Message, 'failure');                }                else {                    $scope.GetApprovedIssueSlipList = response.data;                }            }, function errorCallback(response) {                ShowResult(response.status.Message, 'failure');            });        } catch (e) {            ShowResult(e, 'failure');        }

    };        //#endregion


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
                , TotalAmount: null
                , SalesRate: null
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
    $scope.CustomerList = [];
    $scope.GetMasterOrderByContractList = function () {
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/InventoryIssue/LoadCustomer',
        }).then(function successCallback(response) {
            $scope.CustomerList = response.data;
            //entrydata = copy(searchdata);

        });
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('show');
    }

    $scope.SelectedOrder = function (obj) {
        //debugger;
        //var data = obj.data.ContractId;
        $scope.productNew.PartyId = obj.data.PartyId;
        $scope.productNew.PartyName = obj.data.UserName;
        
        angular.element(document.querySelector('#MasterOrderPopUp')).modal('hide');
    }
    $scope.ClearMasterOrder = function () {
        $scope.productNew.PartyName = "";

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

    //#region Customer Load

    $scope.invoicingPartyPopUp = function () {
        //debugger;
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };
    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            $scope.productNew.PartyName = party.Code + " - " + party.UserName;
            $scope.productNew.PartyId = party.Id;
            $scope.productNew.PaymentTermId = party.PaymentTermId;
            $scope.productNew.CurrencyId = party.CurrencyId;
           // $scope.GetCurrencyExchangeRateList();
          //  $scope.changePaymentTerm($scope.salesVM.PaymentTermId);
            $scope.partyPlantList = [];
            $scope.getCboPartyPlantList(party.Id, function (result) {
                $scope.partyPlantList = result;
                angular.forEach($scope.partyPlantList, function (item, i) {
                    if (item.IsDefault) {
                        $scope.partyPlantId = item.Value;
                        $scope.productNew.InvoicingPartyPlantId = item.Value;
                        $scope.productNew.DeliveryPartyPlantId = item.Value;
                        $scope.productNew.InvoicingByAddress = item.Address1;
                        $scope.productNew.DeliveryByAddress = item.Address1;
                        $scope.productNew.InvoicingState = item.StateName;
                        $scope.productNew.InvoicingGSTIN = item.GSTIN;
                        $scope.productNew.DeliveryState = item.StateName;
                        $scope.productNew.DeliveryGSTIN = item.GSTIN;
                        $scope.productNew.InvoicingStateId = item.StateId;
                    }
                });
            });
        }
        $scope.hidePartyPopUp();
    };
    $scope.closeInvoicingPartyPopUp = function () {
        //if ($scope.salesMaterialList.length || $scope.chargesList.length) {
            
            if (!baseService.isUndefinedOrNull($scope.productNew.ChangeInvoicingStateId)) {
                if ($scope.productNew.PlantStateId == $scope.productNew.InvoicingStateId == $scope.productNew.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else if ($scope.productNew.InvoicingStateId == $scope.productNew.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else if ($scope.productNew.PlantStateId != $scope.productNew.InvoicingStateId && $scope.productNew.PlantStateId != $scope.productNew.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else
                    ShowResult('Change is not allowed', 'failure', 'invoicingPartyPopUp');
            }
            else
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        //}
        //else
           // angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');

    };

    $scope.billShippAddress = function (id, flag) {
        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateName;
            var stateId = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateId;
            if (flag === 'billTo') {
                $scope.productNew.InvoicingState = state;
                $scope.productNew.ChangeInvoicingStateId = stateId;
                $scope.productNew.InvoicingGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.productNew.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.productNew.DeliveryState = state;
                $scope.productNew.DeliveryGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.productNew.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.productNew.InvoicingState = null;
                $scope.productNew.InvoicingGSTIN = null;
                return $scope.productNew.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.productNew.DeliveryState = null;
                $scope.productNew.DeliveryGSTIN = null;
                return $scope.productNew.DeliveryByAddress = null;
            }
        }
    };

    //#endregion

    //#region Index Tab
    $scope.tab = 1;
    $scope.tabType = 1;
    $scope.getdataInventoryScrap($scope.tabType);
    $scope.setTabFirst = function (newTab) {

        $scope.tab = newTab;
        $scope.tabType = '1';
        $scope.getdataInventoryScrap($scope.tabType);

    };
    $scope.isSetFirst = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.setTabSecond = function (newTab) {
        //debugger;
        $scope.tabType = '2';
        $scope.tab = newTab;

        $scope.getdataInventoryScrap($scope.tabType);

    };
    $scope.isSetSecond = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabThird = function (newTab) {
        $scope.tab = newTab;
        $scope.tabType = '3';
        $scope.getdataInventoryScrap($scope.tabType);
    };
    $scope.isSetThird = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabFourth = function (newTab) {
        $scope.tab = newTab;
        $scope.tabType = '4';
        $scope.getdataInventoryScrap($scope.tabType);

    };
    $scope.isSetFourth = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabFifth = function (newTab) {
        $scope.tab = newTab;
        $scope.tabType = '5';

        $scope.getdataInventoryScrap($scope.tabType);

    };
    $scope.isSetFifth = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabSixth = function (newTab) {
        $scope.tab = newTab;
        $scope.tabType = '6';
        $scope.getdataInventoryScrap($scope.tabType);

    };
    $scope.isSetSixth = function (tabNum) {
        return $scope.tab === tabNum;
    };

    
    $scope.AmountCalculation = function () {
        $scope.detailModel.TotalAmount = parseFloat($scope.detailModel.TransactionQty * $scope.detailModel.SalesRate).toFixed(2);
        $scope.calculateTaxCategory();
    }
    $scope.calculateTaxCategory = function () {
        //debugger;
        $scope.detailModel.TotalTaxAmount = 0;
        var tQty = baseService.isUndefinedOrNull($scope.detailModel.TransactionQty) ? 0 : parseFloat($scope.detailModel.TransactionQty);
        var tAmount = baseService.isUndefinedOrNull($scope.detailModel.TotalAmount) ? 0 : parseFloat($scope.detailModel.TotalAmount);
        if (tQty > 0 && tAmount > 0)
            $scope.detailModel.SalesRate = tAmount / tQty;
        else
            $scope.detailModel.SalesRate = 0;
        for (var i = 0; i < baseService.arrayLength($scope.materialtaxCategoryList); i++) {
            $scope.materialtaxCategoryList[i].TaxAmount = ((parseFloat($scope.materialtaxCategoryList[i].Percentage) * $scope.detailModel.TotalAmount) / 100).toFixed($rootScope.currencyPrecision);
            $scope.detailModel.TotalTaxAmount = (parseFloat($scope.detailModel.TotalTaxAmount) + parseFloat($scope.materialtaxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
        if (isNaN($scope.detailModel.TotalTaxAmount)) $scope.detailModel.TotalTaxAmount = 0;
    };

    //#endregion
    $scope.LoadTaxButtonClick = function () {
        accountService.getTaxCategoryCbo(" ", function (result) {
            $scope.taxCategoryList = result;
        });
    }

    $scope.getReceiveTaxList = function (data, flag, index, Id) {
        //debugger;
        $scope.LoadTaxButtonClick();


        //$scope.Currency = $("#currency option:selected").text();
        //$scope.currentMaterialRow = index;
        //$scope.currentInventoryReceiveDetailIdRow = Id;
        //$scope.taxAbleAmnt = data.TrnAmount;
        //$scope.percentageColumn = flag;
        //$scope.currentMaterialRow = index;        
        //$scope.materialtaxCategoryList = [];
        //if (data.TaxList.length > 0) {
        //    $scope.HSNCode = data.TaxList[0].HSNCode;
        //    $scope.materialtaxCategoryList = data.TaxList;
        //}
        $scope.total = 0;
        for (var j = 0; j < $scope.materialtaxCategoryList.length; j++) {
            $scope.HSNCode = $scope.materialtaxCategoryList[j].HSNCode;           
            $scope.total = $scope.total + $scope.materialtaxCategoryList[j].TaxAmount;
           
        }
       
        for (var i = 0; i < $scope.detailList.length; i++) {
            $scope.taxAbleAmnt = $scope.detailList[i].TotalAmount;
		}
      
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
        
    };

    $scope.closeReceiveTaxPopUpwindow = function () {
        //for (var i = 0; i < $scope.detailList.length; i++) {
        //    $scope.detailList[i].TaxAmount = $scope.materialtaxCategoryList.TaxAmount;
        //}
        
       // getInventoryMaterialList($scope.productNew.Id);
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    }

    $scope.addTax = function () {
        var data = {
            TotalAmount: 0,
            Id: null,
            HSNCode: $scope.HSNCode,
            HSNCodeId: null,
            UserName: null,
            TaxCategoryId: null
        };
        $scope.materialtaxCategoryList.push(data);
    };
    $scope.calculateTaxAmount = function (data) {
        //data.TotalAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
        data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
    };
    $scope.Del = function (Id, index) {
        $scope.dindex = index;
        for (var i = 0; i < $scope.materialtaxCategoryList.length; i++) {
            if ($scope.materialtaxCategoryList[i].Id === Id) {
                $scope.materialtaxCategoryList.splice($scope.dindex, 1);
                return true;
                break;
            }
        }
        $scope.dindex = -1;
    };

    $scope.GettaxAfterSave = function (Id) {
        $http({
            method: "GET",
            dataType: 'JSON',            
            url: 'Products/InventoryIssue/GetTaxInfo?Id=' + Id,
        }).then(function successCallback(response) {
            $scope.materialtaxCategoryList = response.data;            

        });
    }

    //Inventory Scrap Report
  
    $scope.InventoryScrapReportExcel = function () {

        if ($scope.productNew.AsOnDate === 'AsOnDate') {

            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }


            //if ($scope.productNew.Qty) {
            //    $scope.choice1 = 'Qty';
            //    $scope.choice2 = '';
            //}
            //if ($scope.productNew.Amount) {
            //    $scope.choice1 = '';
            //    $scope.choice2 = 'Amount';
            //}
            //if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
            //    $scope.choice1 = 'Qty';
            //    $scope.choice2 = 'Amount';
            //}
            //if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
            //    ShowResult('Select Qty OR Amount', 'failure');
            //    return false;
            //}

        }
        else {

            if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
                ShowResult('Select From Date', 'failure');
                return false;
            }
            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }
            //if ($scope.productNew.RcptIssue != true) {
            //    ShowResult('Select With Receipts & Issue', 'failure');
            //    return false;
            //}
            //if ($scope.productNew.Qty) {
            //    $scope.choice1 = 'Qty';
            //    $scope.choice2 = '';
            //}
            //if ($scope.productNew.Amount) {
            //    $scope.choice2 = 'Amount';
            //    $scope.choice1 = '';
            //}
            //if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
            //    $scope.choice1 = 'Qty';
            //    $scope.choice2 = 'Amount';
            //}
            //if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
            //    ShowResult('Select Qty OR Amount', 'failure');
            //    return false;
            //}
        }

        var reportFormat = "Excel";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Products/InventoryIssue/InventoryScrapExcel?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate  /*'&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2*/, '_blank');
    };

    //InventoryScrapReportPdf
    $scope.InventoryScrapReportPdf = function () {

        if ($scope.productNew.AsOnDate === 'AsOnDate') {

            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }


            //if ($scope.productNew.Qty) {
            //    $scope.choice1 = 'Qty';
            //    $scope.choice2 = '';
            //}
            //if ($scope.productNew.Amount) {
            //    $scope.choice1 = '';
            //    $scope.choice2 = 'Amount';
            //}
            //if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
            //    $scope.choice1 = 'Qty';
            //    $scope.choice2 = 'Amount';
            //}
            //if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
            //    ShowResult('Select Qty OR Amount', 'failure');
            //    return false;
            //}

        }
        else {

            if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
                ShowResult('Select From Date', 'failure');
                return false;
            }
            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }
            //if ($scope.productNew.RcptIssue != true) {
            //    ShowResult('Select With Receipts & Issue', 'failure');
            //    return false;
            //}
            //if ($scope.productNew.Qty) {
            //    $scope.choice1 = 'Qty';
            //    $scope.choice2 = '';
            //}
            //if ($scope.productNew.Amount) {
            //    $scope.choice2 = 'Amount';
            //    $scope.choice1 = '';
            //}
            //if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
            //    $scope.choice1 = 'Qty';
            //    $scope.choice2 = 'Amount';
            //}
            //if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
            //    ShowResult('Select Qty OR Amount', 'failure');
            //    return false;
            //}
        }


        var reportFormat = "Pdf";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Products/InventoryIssue/InventoryScrapPdf?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate  /*'&Qty=' + $scope.productNew.Qty + '&Amount=' + $scope.productNew.Amount*/ , '_blank');

    };
   
}