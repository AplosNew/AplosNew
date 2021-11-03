'use strict';
ProcurementController.$inject = ['accountService', 'addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function ProcurementController(accountService, addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {

    $rootScope.title = "Procurement";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/Procurement/';
    $scope.getListUrl = $scope.path + 'getlist';
    //$scope.saveUrl = '$scope.path + Create';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.updateByIdUrl = $scope.path + 'DetailEdit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.detailSaveUrl = $scope.path + 'detailcreate';
    //$scope.DetailDeleteUrl = $scope.path + 'DetailDelete/';
    $scope.detailDeleteUrl = $scope.path + 'DetailDelete/';
    $scope.sreviceSaveUrl = $scope.path + 'servicechargescreate';
    $scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';
    $scope.partyType = 'Vendor';
    $scope.isAdvance = false;
    $scope.currentDate = new Date(Date.now());
    $scope.grossTotal = 0;
    $scope.PartyId = null;
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $scope.inventoryMaterialList = [];
    $scope.chargesList = [];
    $scope.ChargeTaxList = [];
    $scope.StateData = [];
    


    $scope.getDataList = function () {
        baseService.init($scope.getListUrl, null, null, "DESC", 'Id', 'PartyName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.products = [];
                    $scope.products = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.storageList = [];
    $http({
        method: 'GET',
        url: 'Materials/MaterialStorage/getcbo'
    }).then(function (response) {
        $scope.storageList = response.data;
    });
    $scope.currencyList = [];

    addressService.getCountryCbo(function (result) {
        $scope.countryList = result;
    });
    $scope.product = {
        Id: null,
        CompanyGroupId: null,
        CountryId:null,
        CompanyId: null,
        PositionCode: null,
        PlantId: null,
        EntityId: null,
        ProcurementDays:null,
        ProcurementFrequency: null,
        MaterialType: null,
        QualityStdSet:null,
        CostReductionCategory: null,
        MaterialMasterId: null,
        ArticleId: null,
        ArticleCriticality: null,
        FirstCharacteristicsId: null,
        FirstCharacteristicsValueId: null,
        SecondCharacteristicsId: null,
        SecondCharacteristicsValueId: null,
        ThirdCharacteristicsId: null,
        ThirdCharacteristicsValueId: null,
        MinStockLevel: null,
        MaxStockLevel: null,
        CostingPercentage: null,
        ProcurementPercentage: null,
        QualityApprovalReq: null,
        QualityApprovedBy: null,
        PossitionCodeForApproval: null,
        QualityStdSet: null,
        SupplierQualityReportReq: null,
        RequisitionType: null,
        PriceApproval: null,
        POGroupId: null,
        Imported: null,
        ImportedCurrencyId: null,
        ImportedBaseRate: null,
        ImportedTgtLandedRate: null,
        ImportProcurementLedTimeDays: null,
        ImportedMinimumOrderQty: null,
        ImportedArticleLifeDays: null,
        Local: null,
        LocalCurrencyId: null,
        LocalBaseRate: null,
        LocalTgtLandedRate: null,
        LocalProcurementLedTimeDays: null,
        LocalMinimumOrderQty: null,
        LocalArticleLifeDays: null,
        AutoPoGeneration: null,
        POGenerationCriteria: null,
        PoGenerationDay: null,
        LastProcurementRate: null,
        MinimumProcurementRate: null,
        MaximumProcurementRate: null,
        MaterialMasterName: null,
        ArticleName: null,
        ProcurementsPlanDay: null,
        Remarks :null

    };

    $scope.productNew = Object.assign({}, $scope.product);

    $scope.detailModelNew = {
        id: null,
        ProcurementMasterId: null,
        PartyName: null,
        PartyId: null,
        PartyBaseRate: null,
        PartyPreference: null
        //AddedBy: null,
        //AddedDate: null,
        //AddedFromIP: null,
        //UpdatedBy: null,
        //UpdatedDate: null,
        //UpdatedFromIP: null

    };

    //function loadCurrency() {
    //    cboService.getCompanyGroupCurrencyCbo(null, function (result) {
    //        $scope.currencyList = result;
    //        $scope.detailModel.CurrencyId = $scope.selectBaseCurrency();
    //    });
    //}


    function loadCurrency() {
        //debugger;
        cboService.getCompanyGroupCurrencyCbo(null, function (result) {
            $scope.currencyList = result;
            $scope.productNew.CurrencyId = $scope.selectBaseCurrency();
            $scope.productNew.CurrencyName = $scope.currencyList[0].Text;

        });
        
    }

 

    $scope.searchByList = [
        {
            value: 'PartyCode'
            , name: 'Vendor Code'
        },
        {
            value: 'PartyName'
            , name: 'Vendor Name'
        },
        {
            value: 'PartyAccountGroupName'
            , name: 'Account Group'
        },
        {
            value: 'Id'
            , name: 'GRN No'
        },
        {
            value: 'GRNDate'
            , name: 'GRN Date'
        },
        {
            value: 'DocRefNo'
            , name: 'Vendor DocRefNo'
        },
        {
            value: 'InvoiceNo'
            , name: 'Invoice No'
        },
        {
            value: 'InvoiceDate'
            , name: 'Invoice Date'
        }
    ];

    $scope.partySearchByList = [
        {
            'name': $scope.partyType + ' Code',
            'value': 'Code'
        },
        {
            'name': $scope.partyType + ' Name',
            'value': 'UserName'
        },
        {
            'name': 'Account Group',
            'value': 'PartyAccountGroupName'
        },
        {
            'name': 'Country',
            'value': 'CountryName'
        },
        {
            'name': 'State',
            'value': 'StateName'
        },
        {
            'name': 'Currency',
            'value': 'CurrencyCode'
        }
    ];

    $scope.Get = function (event) {
        $scope.product = event.data;
        $scope.productNew = Object.assign({}, $scope.product);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
        getInventoryMaterialList(event.data.Id);

    };



    function ClearFields() {
        $scope.Action = "Save";
        $scope.product = {};
        $scope.IsBaseOnDueDateEnable = false;
        $scope.productNew = {
            FixedAssetOrInventory: 'Inventory'
            , PODepended: false
            , AlongwithInvoice: true
            , IsNonCreditable: false
            , BaseCurrencyId: $scope.baseCurrencyId
            , ToCurrencyRate: 1
            , TaxApplicable: null
            , IsTaxApplicable: false
            , IsTaxApplicableChangeable: false
            , PartyType: $scope.partyType
            , PlantId: $window.plantId
        };
        $scope.inventoryMaterialList = [];
        $scope.chargesList = [];
        $scope.grossTotal = 0;
        baseService.removeErrorClasses();
    }

    $scope.changeAllInvoice = function () {
        $scope.productNew.InvoiceNo = null;
        $scope.productNew.InvoiceDate = null;
    };
    $scope.showPartyPopUp = function () {
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
        angular.element(document.querySelector('#partyPopUp')).modal('show');
        $scope.getPartyList();
    };
    $scope.closePartyPopUp = function () {
        ////debugger;
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            $scope.detailModelNew.PartyCode = party.PartyCode;
            $scope.detailModelNew.PartyName = party.PartyName;
            $scope.detailModelNew.PartyId = party.PartyId;
           
            $scope.changePaymentTerm();
            getPartyPlantList();
            $scope.hidePartyPopUp();
        }
    };

    $scope.GetCurrencyExchangeRateList = function () {
        ////debugger;
        if (!baseService.isUndefinedOrNull(!baseService.isUndefinedOrNull($scope.productNew.CurrencyId))) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?currencyId=" + $scope.productNew.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.productNew.ToCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = null;
        }
    };
    $scope.getToCurrencyRate = function () {
        ////debugger;
        $http.get($scope.path + 'GetToCurrencyRate?currencyId=' + $scope.detailModelNew.CurrencyId)
            .then(function (response) {
                if (parseFloat(response.data) === 0) {


                    $scope.productNew.ToCurrencyRate = 1;
                    $scope.detailModelNew.CurrencyName = angular.element("#currency :selected").text();
                }
                else {


                    $scope.detailModelNew.ToCurrencyRate = response.data;
                    $scope.detailModelNew.CurrencyName = angular.element("#currency :selected").text();
                }
            });
    };
    $scope.invoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };
    $scope.closeInvoicingPartyPopUp = function () {
        ////debugger;
        if ($scope.inventoryMaterialList.length || $scope.chargesList.length) {
            if (!baseService.isUndefinedOrNull($scope.productNew.ChangeInvoicingStateId)) {
                if ($scope.productNew.PlantStateId === $scope.productNew.InvoicingStateId == $scope.productNew.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else if ($scope.productNew.InvoicingStateId === $scope.productNew.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else if ($scope.productNew.PlantStateId !== $scope.productNew.InvoicingStateId && $scope.productNew.PlantStateId != $scope.productNew.ChangeInvoicingStateId)
                    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
                else
                    ShowResult('Change is not allowed', 'failure', 'invoicingPartyPopUp');
            }
            else
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
        }
        else
            angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');

    };



    $scope.billShippAddress = function (id, flag) {
        ////debugger;

        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].StateName;
            var stateId = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].StateId;// 30-5
            if (flag === 'billTo') {
                $scope.productNew.InvoicingState = state;
                $scope.productNew.ChangeInvoicingStateId = stateId;//30-5
                $scope.productNew.InvoicingGSTIN = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.productNew.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.productNew.DeliveryState = state;
                $scope.productNew.DeliveryGSTIN = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].GSTIN;
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
    
    $scope.detailPopUp = function () {
        //debugger;
        angular.element(document.querySelector('#detailPopUp')).modal('show');
    };
   
    $scope.closeDetaiPopUp = function () {
        $scope.detailModelNew = {};
        $scope.taxCategoryList = [];
        removeValidationMsg();
        angular.element(document.querySelector('#detailPopUp')).modal('hide');
    };
    //test
    $scope.closeDetaiPopUpEdit = function () {
        $scope.detailModelNew = {};
        $scope.taxCategoryList = [];
        removeValidationMsg();
        angular.element(document.querySelector('#detailPopUpEdit')).modal('hide');
    };
    $scope.materialType = ['Asset', 'Consumable', 'Spare', 'RawMaterial'];

    $scope.selectMaterialByType = function (ob) {
        //debugger;

        $scope.productNew.MaterialMasterId = ob.Id;
        $scope.productNew.MaterialMasterName = ob.UserName;
        $scope.productNew.BaseUOMId = ob.BaseUOMId;
        $scope.productNew.BaseUoM = ob.BaseUoM;
        $scope.productNew.OurStyleName = ob.OurStyleName;
        $scope.productNew.MaterialGroupMasterName = ob.MaterialGroupMasterName;
        $scope.productNew.ProductMasterName = ob.ProductMasterName;
        $scope.productNew.IsOurStyleRequired = ob.IsOurStyleRequired;
        $scope.productNew.IsProductMstRequired = ob.IsProductMstRequired;
        $scope.productNew.TransactionUoMId = ob.BaseUOMId;
        $scope.productNew.ArticleId = null;
        $scope.productNew.ArticleName = null;
        $scope.productNew.FirstCharacteristicsValueId = null;
        $scope.productNew.SecondCharacteristicsValueId = null;
        $scope.productNew.ThirdCharacteristicsValueId = null;
        $scope.productNew.IsOriginApplicable = ob.IsOriginApplicable;
        $scope.productNew.CountryId = null;

        $scope.hasArticle = ob.HasAttribute;
        $scope.hasSku = ob.WithSKU;
        $scope.clearCharNames();
        if (ob.HasAttribute) $scope.getArticleSearchList(ob.Id);
        if (ob.WithSKU) $scope.getCharacteristicsList(ob.Id);

        var mmId = []; mmId.push(ob.Id);
        cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (result) {
            $scope.uoMList = result;
        });
        manualValidation('div_mm', false);
        manualValidation('div_country', false);
        $scope.closeMaterialMasterbyTypePopUp();
    };
    $scope.selectarticle = function (ob) {
        try {
            $scope.productNew.ArticleId = ob.Id;
            $scope.productNew.ArticleName = ob.StandardName;
            manualValidation('div_ar', false);
            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };

   
    $scope.setDaysTime = function () {
        if ($scope.productNew.ProcurementFrequency=== "Daily") {

            $scope.productNew.ProcurementDays = 1;
        }
        else if ($scope.productNew.ProcurementFrequency === "Weekly") {

            $scope.productNew.ProcurementDays = 7 ;
        }

        else if ($scope.productNew.ProcurementFrequency === "Bi-Weekly") {

            $scope.productNew.ProcurementDays = 14 ;
        }

        else if ($scope.productNew.ProcurementFrequency === "Monthly") {

            $scope.productNew.ProcurementDays = 30 ;
        }

        else if ($scope.productNew.ProcurementFrequency === "Quartely") {

            $scope.productNew.ProcurementDays = 90;
        }

        else if ($scope.productNew.ProcurementFrequency === "Bi-Annualy") {

            $scope.productNew.ProcurementDays = 180;
        }

        else if ($scope.productNew.ProcurementFrequency === "Annualy") {

            $scope.productNew.ProcurementDays = 365;
        }

      
        else

            $scope.productNew.Days = "";

          
        
    };







    $scope.setCharData = function (data) {
        $scope[$scope.charValueSearchFor].CharacteristicsValueId = data.CharacteristicsValueId;
        $scope[$scope.charValueSearchFor].FreeText = data.UserName;
        $scope[$scope.charValueSearchFor].FlagDisable = $scope.isSearch;
        angular.element(document.querySelector('#searchcharactervaluepopup')).modal('hide');
    };
    $scope.materialValidation = function () {
      
        $scope.invalid = true;

    }

    $scope.vendorValidation = function () {
        var getRow3 = $filter("filter")($scope.inventoryMaterialList, { "PartyName": $scope.detailModelNew.PartyName });
        if (getRow3 == 0) {
            $scope.invalid = true;
        }
        else {
            ShowResult('This Vendor  Already Exist', 'failure', 'detailPopUp');
            $scope.invalid = false;
        }

    }

    $scope.detailSave = function () {
        //debugger;
        $scope.vendorValidation();
        try {
            $scope.detailModelNew.ProcurementMasterId = $scope.productNew.Id;    
            //$scope.materialValidation();
            // $scope.entity = $scope.detailModelNew;
            //debugger;
            if ($scope.invalid) {
                $http({
                    method: 'POST',
                    url: $scope.detailSaveUrl,
                    data: $scope.detailModelNew,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true)
                        ShowResult(response.data.Message, 'failure', 'detailPopUp');
                    else {
                        ShowResult(response.data.Message, 'success', 'detailPopUp');
                        $scope.detailModelNew.Id = null;

                        getInventoryMaterialList($scope.productNew.Id);
                        //$scope.GetReq();
                        $scope.clearCharNames();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'detailPopUp');
                };
            }
        }
        catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };
    $scope.valuePassInDelModal = function (MaterialReqqusitionMasterId) {
        
        $scope.id = MaterialReqqusitionMasterId;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removerPopUp')).modal('show');
    };
    $scope.detailDelete = function () {
        //debugger;
        try {
            $http({
                method: 'POST',
                url: $scope.detailDeleteUrl + $scope.id
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.id = null;
                    getInventoryMaterialList($scope.productNew.Id);
                    //$scope.getDataList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'success');
        }
    };
    $scope.validation = function () {
        $scope.modelValidation('div_mm', 'detailModelNew', 'MaterialMasterName', 'Material Master');
        if ($scope.hasArticle) $scope.modelValidation('div_ar', 'detailModelNew', 'ArticleName');
        $scope.manualValidationAddRemove('div_qty', 'detailModelNew', 'TransactionQty');
        $scope.modelValidation('div_qty', 'detailModelNew', 'TransactionUoMId', 'UoM is required');
        if ($scope.detailModelNew.TransactionAmount === 0)
            throw manualValidation('div_tamnt', true, 'Total amount is required.');
        $scope.manualValidationAddRemove('div_tamnt', 'detailModelNew', 'TransactionAmount');
        if ($scope.detailModelNew.IsOriginApplicable)
            $scope.manualValidationAddRemove('div_country', 'detailModelNew', 'CountryId');

        var isSku = false;
        if ($scope.hasSku) {
            if (!baseService.isUndefinedOrNull($scope.char1.CharacteristicsId)) {
                isSku = $scope.IsMandatoryButNull($scope.char1.IsMandatory, $scope.char1.FreeText);
            }
            else if (!baseService.isUndefinedOrNull($scope.char2.CharacteristicsId)) {
                isSku = $scope.IsMandatoryButNull($scope.char2.IsMandatory, $scope.char2.FreeText);
            }
            else if (!baseService.isUndefinedOrNull($scope.char3.CharacteristicsId)) {
                isSku = $scope.IsMandatoryButNull($scope.char3.IsMandatory, $scope.char3.FreeText);
            }
            if (isSku) throw ShowResult('Please insert SKU.', 'failure', 'detailPopUp');
        }
    };
    $scope.manualValidationAddRemove = function (divId, modelName, fieldName, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else if (isNaN($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else
            return manualValidation(divId, false);
    };
    //manualDateValidation
    $scope.modelValidation = function (divId, modelName, fieldName, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else
            return manualValidation(divId, false);
    };
    $scope.GetSalesTaxData = function (salesId) {
        $scope.TaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetReceiveTaxList?receiveDetailId=' + $scope.masterId
        }).then(function (response) {
            $scope.TaxList = response.data;

            for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
                var linepk = $scope.inventoryMaterialList[i].InventoryReceiveDetailId;
                var list = gettaxlist(linepk);
                $scope.inventoryMaterialList[i].TaxList = list;
            }
        });
    };
    function gettaxlist(linepk) {
        var result = [];
        for (var i = 0; i < $scope.TaxList.length; i++) {
            if ($scope.TaxList[i].PODetailId === linepk) {
                result.push($scope.TaxList[i]);
            }
        }
        return result;
    }
    $scope.sumORnot = false;

    // Material Load 
    function getInventoryMaterialList(inveReveiveId) {
        //debugger;
        $scope.masterId = inveReveiveId;
        //debugger;
        $scope.inventoryMaterialList = [];
        $http.get($scope.path + 'GetInventoryMaterialList?materialId=' + inveReveiveId)
            .then(function (response) {

                $scope.inventoryMaterialList = response.data;
                // console.log('inventoryMaterialList',$scope.inventoryMaterialList);

            });
    }

    //function getInventoryMaterialLists(args) {
    //    //debugger;
    //    $scope.masterId = args.data.Id;
    //    //debugger;
    //    $scope.inventoryMaterialList = [];
    //    $http.get($scope.path + 'GetInventoryMaterialList?materialId=' + args.data.Id)
    //        .then(function (response) {
    //            $scope.inventoryMaterialList = response.data;
    //        });
    //}

    function checkSameValueInColumnList(list, fieldName) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i][fieldName] === (i > 0 ? list[i - 1][fieldName] : list[i][fieldName]))
                $scope.sumORnot = true;
            else return $scope.sumORnot = false;
        }
    }
    function getTaxCategoryList(hsnCodeId) {
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            , url: $scope.path + 'GetTaxCategoryList?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
        });
    }
    $scope.calculateTaxCategory = function () {
        $scope.detailModelNew.TotalTaxAmount = 0;
        var tQty = baseService.isUndefinedOrNull($scope.detailModelNew.TransactionQty) ? 0 : parseFloat($scope.detailModelNew.TransactionQty);
        var tAmount = baseService.isUndefinedOrNull($scope.detailModelNew.TransactionAmount) ? 0 : parseFloat($scope.detailModelNew.TransactionAmount);
        if (tQty > 0 && tAmount > 0)
            $scope.detailModelNew.TransactionRate = tAmount / tQty;
        else
            $scope.detailModelNew.TransactionRate = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.detailModelNew.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
            $scope.detailModelNew.TotalTaxAmount = (parseFloat($scope.detailModelNew.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
        if (isNaN($scope.detailModelNew.TotalTaxAmount)) $scope.detailModelNew.TotalTaxAmount = 0;
    };
    $scope.calculateTaxCategoryRate = function () {
        //debugger;
        $scope.detailModelNew.TotalTaxAmount = 0;
        var tQty = baseService.isUndefinedOrNull($scope.detailModelNew.TransactionQty) ? 0 : parseFloat($scope.detailModelNew.TransactionQty);
        var tAmount = baseService.isUndefinedOrNull($scope.detailModelNew.EstimatedRate) ? 0 : parseFloat($scope.detailModelNew.EstimatedRate);
        if (tQty > 0)
            $scope.detailModelNew.TotalAmount = tAmount * tQty;
        else
            $scope.detailModelNew.TotalAmount = 0;
    };
    $scope.sumTaxAmount = function () {
        $scope.detailModelNew.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.detailModelNew.TotalTaxAmount = (parseFloat($scope.detailModelNew.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
    };
    $scope.getReceiveTaxList = function (data, flag, index, Id) {
        $scope.LoadTaxButtonClick();

        //debugger;
        $scope.Currency = $("#currency option:selected").text();
        $scope.currentMaterialRow = index;
        $scope.currentInventoryReceiveDetailIdRow = Id;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;

        $scope.currentMaterialRow = index;
        $scope.receiveTaxList = [];
        if (data.TaxList.length > 0) {
            $scope.HSNCode = data.TaxList[0].HSNCode;
            $scope.receiveTaxList = data.TaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');

    };
    $scope.getTotalReceiveTaxList = function (amount, flag) {
        $scope.taxAbleAmnt = amount;
        $scope.percentageColumn = flag;
        $http({
            method: 'GET',
            url: $scope.path + 'GetTotalReceiveTaxList?receiveId=' + $scope.productNew.Id
        }).then(function (response) {
            $scope.receiveTaxList = response.data;
            angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
        });
    };
    $scope.closeReceiveTaxPopUp = function () { //hossain
        //debugger;
        $scope.detailModelNew = {};

        $scope.detailModelNew.InventoryReceiveDetailId = $scope.currentInventoryReceiveDetailIdRow;
        $scope.detailModelNew.InventoryReceiveId = $scope.productNew.Id;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": $scope.receiveTaxList[i].TaxCategoryId });
            if (getRow.length == 2) {
                ShowResult("You can't add Same Tax two times", 'failure', 'receiveTaxPopUp');
                return false;
            }

            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxCategoryId)) {
                ShowResult("Select Tax Category.", 'failure', 'receiveTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].Percentage)) {
                ShowResult("Input Percentage.", 'failure', 'receiveTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxAmount)) {
                ShowResult("Input Tax Amount.", 'failure', 'receiveTaxPopUp');
                return false;
            }
        }

        //if ($scope.TAction === "OK") {
        $http({
            method: 'POST',
            url: 'Products/Requisition/InsertExtraTax',
            data: {
                entity: $scope.detailModelNew
                , taxCategoryList: $scope.receiveTaxList
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
            }
            else {
                ShowResult(response.data.Message, 'success', 'receiveTaxPopUp');

                getInventoryMaterialList($scope.productNew.Id);
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
        };
    }
    $scope.closeServiceChargeTaxPopUp = function () { //hossain
        ////debugger;



        $scope.detailModelNew = {};
        $scope.detailModelNew.InventoryReceiveDetailId = $scope.ServiceId;
        $scope.detailModelNew.InventoryReceiveDetailId = $scope.DetailId;
        $scope.detailModelNew.InventoryReceiveId = $scope.productNew.Id;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": $scope.receiveTaxList[i].TaxCategoryId });
            if (getRow.length == 2) {
                ShowResult("You can't add Same Tax two times", 'failure', 'ServiceChargeTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxCategoryId)) {
                ShowResult("Select Tax Category.", 'failure', 'ServiceChargeTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].Percentage)) {
                ShowResult("Input Percentage.", 'failure', 'ServiceChargeTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxAmount)) {
                ShowResult("Input Tax Amount.", 'failure', 'ServiceChargeTaxPopUp');
                return false;
            }
        }

        $http({
            method: 'POST',
            url: 'Products/Requisition/InsertserviceTax',
            data: {
                entity: $scope.detailModelNew
                , taxCategoryList: $scope.receiveTaxList
                , ServiceId: $scope.ServiceId
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'ServiceChargeTaxPopUp');
            }
            else {
                ShowResult(response.data.Message, 'success', 'ServiceChargeTaxPopUp');
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure', 'ServiceChargeTaxPopUp');
        };
    }
    $scope.closeReceiveTaxPopUpwindow = function () {
        //debugger;
        getInventoryMaterialList($scope.productNew.Id);
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    }
    $scope.closeServiceChargeTaxPopUpwindow = function () {
        getServiceChargeList($scope.productNew.Id);
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
    }
    function removeValidationMsg() {
        CloseModalShowResult();
        $scope.clearCharNames();
        manualValidation('div_mm', false);
        manualValidation('div_ar', false);
        manualValidation('div_qty', false);
        manualValidation('div_qty', false);
        manualValidation('div_rate', false);

    }
    function getGrossAmount(list, key1, key2, key3, fieldName) {
        $scope[fieldName] = 0;
        for (var t = 0; t < baseService.arrayLength(list); t++) {
            $scope[fieldName] += parseFloat(list[t][key1]);// + parseFloat(list[t][key2]) + parseFloat(list[t][key3]);
        }
    }
    $http({
        method: 'GET',
        url: 'accounts/PaymentTerm/getvendorcbo'
    }).then(function successCallback(response) {
        $scope.paymentTermList = response.data;
    });


    $scope.GetTerms = function (id) {
        $http({
            method: 'GET',
            url: 'Products/Procurement/GetReqMaster?id=' + id
        }).then(function successCallback(response) {
            $scope.paymentTermList1 = response.data;
            $scope.productNew.Id = $scope.paymentTermList1[0].Id;
            $scope.productNew.CompanyGroupId = $scope.paymentTermList1[0].CompanyGroupId;
            $scope.productNew.EntityId = $scope.paymentTermList1[0].EntityId;
            $scope.productNew.RequisitionType = $scope.paymentTermList1[0].RequisitionType;
            $scope.productNew.RequirmentType = $scope.paymentTermList1[0].RequirmentType;
            $scope.productNew.Remarks = $scope.paymentTermList1[0].Remarks;
            $scope.productNew.ReasonWhyItIsNotPlanEarlier = $scope.paymentTermList1[0].ReasonWhyItIsNotPlanEarlier;
            $scope.productNew.RequisitionDate = $scope.paymentTermList1[0].RequisitionDate;
            $scope.productNew.QualityApprovalResponsiblePersonId = $scope.paymentTermList1[0].QualityApprovalResponsiblePersonId;
            $scope.productNew.NeedSpecialAppId = $scope.paymentTermList1[0].NeedSpecialAppId;


        });

    }

    $scope.changePaymentTerm = function () {
        if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
            var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
            $scope.productNew.PaymentTermCode = paymentTerm.PaymentTermCode;
            $scope.productNew.BaseNoOfDays = paymentTerm.NoOfDay;
            if (paymentTerm.BaseLineDate !== null)
                if (paymentTerm.BaseLineDate === 'documentdate') {
                    $scope.productNew.BaseOnDueDate = $filter('dateFiltering')($scope.productNew.DocDate);
                    $scope.IsBaseOnDueDateEnable = true;
                }
                else {
                    $scope.productNew.BaseOnDueDate = null;
                    $scope.IsBaseOnDueDateEnable = false;
                }
            $scope.getMatureDate($scope.productNew.BaseOnDueDate, $scope.productNew.BaseNoOfDays);
        }
    };
    $scope.getMatureDate = function (date, days) {
        if (baseService.isUndefinedOrNull(date)) return $scope.productNew.MatureDate = null;
        date = new Date(date);
        date.setDate(date.getDate() + days);
        $scope.productNew.MatureDate = $filter('date')(date, 'dd-MMM-yyyy');
    };
    // #endregion Payment Term

    // #region Service
    $scope.serviceChargePopUp = function () {
        if (baseService.arrayLength($scope.inventoryMaterialList) === 0)
            return ShowResult('Without material charges not aplicable.');
        $scope.serviceModel = {
            Id: null
            , ServiceMasterId: null
            , InventoryReceiveId: $scope.productNew.Id
            , CurrencyName: angular.element("#currency :selected").text()
            , CurrencyId: $scope.productNew.CurrencyId
            , BaseCurrencyId: $scope.baseCurrencyId
            , DocDate: $scope.productNew.DocDate
            , TransactionAmount: null
            , BaseAmount: 0
            , TotalTaxAmount: 0
            , ToCurrencyRate: $scope.productNew.ToCurrencyRate
            , IsNonCreditable: $scope.productNew.IsNonCreditable
        };
        angular.element(document.querySelector('#serviceChargePopUp')).modal('show');
    };
    $http.get('Setups/CompanyServiceMaster/GetCboList')

        .then(function (response) {
            $scope.serviceList = response.data;
        });
    $scope.closeServiceChargePopUp = function () {
        $scope.serviceModel = {};
        $scope.receiveTaxList = [];
        angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
    };
    $scope.changeService = function () {
        //debugger;
        if (baseService.isUndefinedOrNull($scope.serviceModel.ServiceMasterId))
            return $scope.taxCategoryList = [];
        var hsnCodeId = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCodeId;
        getTaxCategoryList(hsnCodeId);
    };

    $scope.calculateSvcTaxCategory = function () {
        $scope.serviceModel.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.serviceModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
            $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
        if (isNaN($scope.serviceModel.TotalTaxAmount)) $scope.serviceModel.TotalTaxAmount = 0;
    };
    $scope.sumSvcTaxAmount = function () {
        $scope.serviceModel.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
    };

    $scope.serviceSave = function () {
        try {
            $scope.manualValidationAddRemove('div_svc', 'serviceModel', 'ServiceMasterId');
            $scope.manualValidationAddRemove('div_svcRate', 'serviceModel', 'TransactionAmount', 'Amount');

            $http({
                method: 'POST',
                url: $scope.sreviceSaveUrl,
                data: {
                    entity: $scope.serviceModel
                    , taxCategoryList: $scope.taxCategoryList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
                else {
                    ShowResult(response.data.Message, 'success', 'serviceChargePopUp');
                    $scope.serviceModel = {
                        Id: null
                        , ServiceMasterId: null
                        , InventoryReceiveId: $scope.productNew.Id
                        , CurrencyName: angular.element("#currency :selected").text()
                        , CurrencyId: $scope.productNew.CurrencyId
                        , BaseCurrencyId: $scope.baseCurrencyId
                        , DocDate: $scope.productNew.DocDate
                        , TransactionAmount: null
                        , BaseAmount: 0
                        , TotalTaxAmount: 0
                        , ToCurrencyRate: $scope.productNew.ToCurrencyRate
                        , IsNonCreditable: $scope.productNew.IsNonCreditable
                    };
                    $scope.taxCategoryList = [];
                    getServiceChargeList($scope.productNew.Id);
                    getInventoryMaterialList($scope.productNew.Id);
                    $scope.getDataList();
                    $scope.getalldata();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
            };
        } catch (e) {
            //ShowResult(e, 'fail', 'detailPopUp');
        }
    };

    $scope.delModal = function (id) {
        $scope.id = id;
        $scope.message = 'Are you sure want to permanently delete this?';
        angular.element(document.querySelector('#removePopUp')).modal('show');
    };
    $scope.serviceDelete = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.sreviceDeleteUrl + $scope.id
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.id = null;
                    getServiceChargeList($scope.productNew.Id);
                    getInventoryMaterialList($scope.productNew.Id);
                    $scope.getDataList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'success');
        }
    };


    $scope.getServiceTaxList = function (data, flag, ServiceId, index) {

        //debugger;
        $scope.LoadTaxButtonClick();

        $scope.Currency = $("#currency option:selected").text();
        $scope.ServiceId = ServiceId;
        $scope.taxAbleAmnt = data.Amount;//+ data.TotalTaxAmount;
        $scope.percentageColumn = flag;

        $scope.currentMaterialRow = index;

        $scope.receiveTaxList = [];
        if (data.ChargeTaxList.length > 0) {
            $scope.HSNCode = data.ChargeTaxList[0].HSNCode;
            $scope.receiveTaxList = data.ChargeTaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('show');
    }
    //Load2
    $scope.GetServiceTaxData = function (masterId) {
        ////debugger;
        $scope.ChargeTaxList = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetServiceTaxList?serviceId=' + $scope.productNew.Id
        }).then(function (response) {
            $scope.ChargeTaxList = response.data;

            for (var i = 0; i < $scope.chargesList.length; i++) {
                var linepk1 = $scope.chargesList[i].Id;
                var list1 = gettaxlist1(linepk1);
                $scope.chargesList[i].ChargeTaxList = list1;
            }
        });
    };
    function gettaxlist1(linepk1) {
        var result1 = [];

        for (var i = 0; i < $scope.ChargeTaxList.length; i++) {
            if ($scope.ChargeTaxList[i].InventoryServiceId === linepk1) {
                result1.push($scope.ChargeTaxList[i]);
            }
        }
        return result1;
    }

    function getServiceChargeList(inveReveiveId) {
        //debugger;
        $scope.chargesList = [];
        $http.get($scope.path + 'GetServiceChargeList?receiveId=' + inveReveiveId)
            .then(function (response) {
                $scope.chargesList = response.data;
                $scope.ServiceId = $scope.chargesList[0].Id;
                $scope.GetServiceTaxData();
            });

    }

    $scope.serviceChargePopUpEdit = function (Id, Amount, TotalTaxAmount) {
        if (baseService.arrayLength($scope.inventoryMaterialList) === 0)
            return ShowResult('Without material charges not aplicable.');
        //debugger;

        for (var i = 0; i < $scope.chargesList.length; i++) {
            for (var t = 0; t < $scope.chargesList[i].ChargeTaxList.length; t++) {
                $scope.receiveTaxList.push($scope.chargesList[i].ChargeTaxList[t]);
            }

        }
        $scope.productNew.Id
        $http({
            method: 'POST',
            url: 'Products/Requisition/UpdateServiceAndTax',
            data: {
                entity: $scope.chargesList,
                receiveTaxList: $scope.receiveTaxList
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
        $scope.enable = true;
        $scope.MSAction = "Edit";

        //}
        //else {

        //}

        $scope.serviceModel = {
            Id: null
            , ServiceMasterId: null
            , InventoryReceiveId: $scope.productNew.Id
            , CurrencyName: angular.element("#currency :selected").text()
            , CurrencyId: $scope.productNew.CurrencyId
            , BaseCurrencyId: $scope.baseCurrencyId
            , DocDate: $scope.productNew.DocDate
            , TransactionAmount: null
            , BaseAmount: 0
            , TotalTaxAmount: 0
            , ToCurrencyRate: $scope.productNew.ToCurrencyRate
            , IsNonCreditable: $scope.productNew.IsNonCreditable
        };

    };

    $scope.inventoryReceiveReport = function (id, reportFormat) {
        if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Products/InventoryReceive/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + id + '&plantId=' + $scope.productNew.PlantId, '_blank');
    };
    $scope.Griddata = [];
    $scope.getalldata = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/Requisition/GetListForHold',
        }).then(function successCallback(response) {
            $scope.Griddata = response.data;
        });
    };

    $scope.Griddata = [];
    $scope.getApprovaldata = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/Requisition/GetListForPOApproval',
        }).then(function successCallback(response) {
            $scope.Griddata = response.data;
        });
    };
    $scope.getApprovaldata();

    //$scope.GriddataAUth = [];
    //$scope.getApprovaldataAUth = function () {
    //    $http({
    //        method: "GET",
    //        dataType: 'JSON',
    //        url: 'Products/Requisition/GetListForPOApprovalAuthorized',
    //    }).then(function successCallback(response) {
    //        $scope.GriddataAUth = response.data;
    //    });
    //};
    //$scope.getApprovaldataAUth();

    $scope.GriddataAUth1 = [];
    $scope.getApprovaldataAUth1 = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/Requisition/GetListForPOApproval1Auth',
        }).then(function successCallback(response) {
            $scope.GriddataAUth1 = response.data;
        });
    };
    $scope.getApprovaldataAUth1();

    $scope.GriddataVendor = [];
    $scope.getalldataVendor = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/Requisition/GetListByParty',
        }).then(function successCallback(response) {
            $scope.GriddataVendor = response.data;
        });
    };
    function getPartyPlantList() {
        //debugger;

        //var aa = $scope.Id;
        $scope.plantList = [];
        $http.get('Products/Requisition/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId + '&Id=' + $scope.Id).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.plantList.push(item);
                if (item.IsDefault) {
                    $scope.productNew.InvoicingPartyPlantId = item.Value;
                    $scope.productNew.DeliveryPartyPlantId = item.Value;
                    $scope.productNew.InvoicingByAddress = item.Address1;
                    $scope.productNew.DeliveryByAddress = item.Address2;
                    $scope.productNew.InvoicingState = item.StateName;
                    $scope.productNew.InvoicingGSTIN = item.GSTIN;
                    $scope.productNew.DeliveryState = item.StateName;
                    $scope.productNew.DeliveryGSTIN = item.GSTIN;
                }
            });
        });

    }
    function getPartyPlantEditList(invoicingPartyPlantId, invoAddress, deliveryplant, deliAddress, deliState, deliGSTIN) {
        $scope.plantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.plantList.push(item);
                if (item.Value == invoicingPartyPlantId) {
                    $scope.productNew.InvoicingPartyPlantId = item.Value;
                    $scope.productNew.DeliveryPartyPlantId = deliveryplant;
                    $scope.productNew.InvoicingByAddress = invoAddress;
                    $scope.productNew.DeliveryByAddress = deliAddress;
                    $scope.productNew.InvoicingState = item.StateName;
                    $scope.productNew.InvoicingGSTIN = item.GSTIN;
                    $scope.productNew.DeliveryState = deliState;
                    $scope.productNew.DeliveryGSTIN = deliGSTIN;

                }
            });
        });
    }

    $scope.getalldataVendor();
    $scope.getalldata();
    $scope.recorddoubleclick = function ($event) {
        //debugger;
        var x = $event;
        var Id = x.data.Id;

        $scope.Currency = $("#currency option:selected").text();
        $scope.productNew = x.data;
        $scope.Id = $scope.productNew.Id;
        $scope.GetTerms($scope.productNew.Id);
        getInventoryMaterialList($scope.productNew.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };

    $scope.closeServiceChargePopUpEdit = function () {
        $scope.serviceModel = {};
        $scope.receiveTaxList = [];
        angular.element(document.querySelector('#serviceChargePopUpEdit')).modal('hide');
    };
    $scope.dindex = -1;
    $scope.DelCharge = function (Id, index) {
        $scope.dindex = index;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            if ($scope.receiveTaxList[i].Id === Id) {
                $scope.receiveTaxList.splice($scope.dindex, 1);
                return true;
                break;
            }
        }
        $scope.dindex = -1;

    };
    $scope.Del = function (Id, index) {
        $scope.dindex = index;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            if ($scope.receiveTaxList[i].Id === Id) {
                $scope.receiveTaxList.splice($scope.dindex, 1);
                return true;
                break;
            }
        }
        $scope.dindex = -1;
    };

    $scope.calculateAmount = function (data) {
        //debugger;
        data.TotalAmount = (data.TransactionQty * data.EstimatedRate).toFixed(2);
        if (data.TotalAmount === 'NaN')
            data.TotalAmount = 0;
        //data.TaxAmount = 0;
        //angular.forEach(data.TaxList, function (item) {
        //    item.TaxAmount = data.TrnAmount * item.Percentage / 100;
        //    data.BaseTaxAmount += item.TaxAmount;
        //});
        // data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
        //data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
        //if ($scope.productNew.IsNonCreditable == 1) {
        //    //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
        //    if (data.BaseTaxAmount === null) {
        //        data.BaseTaxAmount = '0.00';
        //    }
        //    data.BaseAmount = parseFloat(data.TrnAmount + data.BaseTaxAmount);
        //}
        //else {
        //    // data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
        //    data.BaseAmount = data.TrnAmount;
        //}
    };
    $scope.calculateRate = function (data, event) {
        //debugger;
        data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
        if (data.TransactionRate === 'NaN')
            data.TransactionRate = 0;
        data.BaseTaxAmount = 0;
        angular.forEach(data.TaxList, function (item) {
            item.TaxAmount = data.TrnAmount * item.Percentage / 100;

            data.BaseTaxAmount += item.TaxAmount;
        });
        // data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
        if ($scope.productNew.IsNonCreditable == 1) {
            //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
            data.BaseAmount = data.TrnAmount + data.BaseTaxAmount;
        }
        else {
            // data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
            data.BaseAmount = data.TrnAmount;
        }

    };
    $scope.calculateAmountForServiceCharge = function (data) {
        //debugger;
        //data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
        //if (data.TrnAmount == 'NaN')
        //    data.TrnAmount = 0;
        //data.TaxAmount = 0;
        data.TotalTaxAmount = 0;
        for (var i = 0; i < $scope.ChargeTaxList.length; i++) {
            if ($scope.ChargeTaxList[i].InventoryServiceId === data.Id) {
                $scope.ChargeTaxList[i].TaxAmount = data.Amount * $scope.ChargeTaxList[i].Percentage / 100;
                data.TotalTaxAmount += $scope.ChargeTaxList[i].TaxAmount;
            }
        }
        // data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
        //data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
    };
    $scope.onchangeFunction = function (id) {
        $scope.TaxCategoryId = id;
        //debugger;
        var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": id });
        if (getRow.length === 2) {
            ShowResult("You can't add Same Tax two times", 'failure', 'ServiceChargeTaxPopUp');

        }

    }
    $scope.onchangeFunction1 = function (id) {
        $scope.TaxCategoryId = id;
        //debugger;
        var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": id });
        if (getRow.length === 2) {
            ShowResult("You can't add Same Tax two times", 'failure', 'receiveTaxPopUp');

        }

    };
    $scope.onClick = function (args) {

        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/Requisition/GePurchaseOrderReport?purchaseOrderId=" + data.Id;

    };



    $scope.onClick = function (args) {

        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

    };



    $scope.onClick = function (args) {

        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/ Procurement/GetDataByProcurementMasterId?ProcurementMasterId=" + data.Id;

    };

    $scope.command = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",

            click: $scope.onClick
        }
    }];

    $scope.onClickpoApprovalprint = function (args) {

        var gridObj = $("#GridPO1").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/Requisition/GePurchaseOrderReport?purchaseOrderId=" + data.Id;

    };
    $scope.commandprint = [{
        type: "details", buttonOptions: {
            text: "Print",
            width: "50",
            height: "20",

            click: $scope.onClickpoApprovalprint
        }
    }];

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";

        if (new Date($scope.productNew.DocDate) > new Date($scope.productNew.PODate)) {
            msg = "Doc date must be grater or equal to Vendor Doc. RefNo!";
            $scope.invalidDocDate = true;
        }

        else $scope.invalidDocDate = false;
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    };
    $scope.Griddata1 = [];
    $scope.onClickPO = function (args) {
        //debugger;
        var gridObj = $("#Grid").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];
        $scope.approveAlert();

    };
    cboService.getEnumCbo("enum/GetExpensesBookingApprovalStatusCbo", function (result) {
        $scope.approvalStatusList = result;
    });
    $scope.getalldata1 = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/Requisition/GetListForPOApproval',
        }).then(function successCallback(response) {
            $scope.Griddata1 = response.data;
        });
    };
    $scope.Status = null;
    $scope.getalldata1();
    $scope.poApp = function () {
        var str = $('#combo-default1').val();
        var Id = str.substring(0, str.indexOf('-'));

        //debugger;
        $http({
            method: 'POST',
            url: 'Products/Requisition/PoApproved',
            data: {
                'PoId': $scope.podata.Id,
                'PoValue': $scope.podata.TotalQty,
                'CheckedStataus': $('#combo-default').val(),
                'AuthorizedBy': Id

            },

            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getalldata1();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
    $scope.poAppAuth = function () {
        //debugger;
        $http({
            method: 'POST',
            url: 'Products/Requisition/PoApprovedAuth',
            data: {
                'PoId': $scope.podata.Id,
                'PoValue': $scope.podata.TotalQty,
                'CheckedStataus': $('#combo-default12').val()
            },

            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getApprovaldataAUth();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
    $scope.poAppUnApproved = function () {

        //debugger;
        $http({
            method: 'POST',
            url: 'Products/Requisition/PoUnApproved',
            data: {
                'PoId': $scope.podata1.Id,
                'PoValue': $scope.podata1.TotalQty
            },

            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getalldata1();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }




    $scope.onClickPOA = function (args) {

        var gridObj = $("#GridPO").data("ejGrid");
        $scope.podata = gridObj.getSelectedRecords()[0];
        $scope.approvalAlert();
    };
    $scope.commandpo = [{
        type: "details", buttonOptions: {
            text: "Save",
            width: "100",
            height: "30",
            click: $scope.onClickPOA
        }
    }];
    $scope.onClickPOAUTH = function (args) {

        var gridObj = $("#GridPOAPp").data("ejGrid");
        $scope.podata = gridObj.getSelectedRecords()[0];
        $scope.approvalAlert();
    };
    $scope.commandpoAuth = [{
        type: "details", buttonOptions: {
            text: "Save",
            width: "100",
            height: "30",
            click: $scope.onClickPOAUTH
        }
    }];
    $scope.approvalAlert = function () {
        $scope.message = 'Are you sure want to Approve?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');
    };
    $scope.GriddataPOClose = [];
    $scope.getalldataPOClose = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/Requisition/GetListForPOClose',
        }).then(function successCallback(response) { //datagatefun
            $scope.GriddataPOClose = response.data;
        });
    };
    $scope.getalldataPOClose();


    $scope.onClickPOlock = function (args) {
        //debugger;
        var gridObj = $("#Grid").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];
        $scope.approvalAlertlock();

    };
    $scope.approvalAlertlock = function () {
        $scope.message = 'Are you sure want to Approve?';
        angular.element(document.querySelector('#poapprovealertlock')).modal('show');
    };

    $scope.commandPoClose = [{

        type: "details", buttonOptions: {
            text: "Po Unlock",
            width: "120",
            height: "20",


            click: $scope.onClickPOlock
        }
    }];
    $scope.Poclosed = function () {
        $http({
            method: 'POST',
            url: 'Products/Requisition/POClose',
            data: {
                'PoId': $scope.data.Id,
                'PoValue': $scope.data.TotalQty
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getalldataPOClose();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });

    }
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.Griddataapprovpo = [];
    $scope.Griddataapprovpo1 = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/Requisition/GetListForPOApproval1',
        }).then(function successCallback(response) {
            $scope.Griddataapprovpo = response.data;
        });
    };
    $scope.Griddataapprovpo1();



    $scope.ListForPOApproval1UnApproved = [];
    $scope.GetListForPOApproval1UnApproved = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/Requisition/GetListForPOApproval1UnApproved',
        }).then(function successCallback(response) {
            $scope.ListForPOApproval1UnApproved = response.data;
        });
    };
    $scope.GetListForPOApproval1UnApproved();


    $scope.onClickPOA1 = function (args) {

        var gridObj = $("#GridPO1").data("ejGrid");
        $scope.podata1 = gridObj.getSelectedRecords()[0];
        $scope.approveAlert1();
    };

    $scope.commandpo1 = [{
        type: "details", buttonOptions: {
            text: "Un Approve",
            width: "100",
            height: "30",

            click: $scope.onClickPOA1
        }
    }];

    $scope.approveAlert1 = function () {
        $scope.message = 'Are you sure want to Approve?';
        angular.element(document.querySelector('#poapprovalalert1')).modal('show');
    };

    $scope.poApp1 = function () {
        $http({
            method: 'POST',
            url: 'Products/Requisition/PoApproved1',
            data: {
                'PoId': $scope.podata1.Id,
                'PoValue': $scope.podata1.TotalQty

            },

            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.Griddataapprovpo1();
                $scope.ClosedPOPUp();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.ClosedPOPUp = function (args) {

        angular.element(document.querySelector('#poapprovalalert1')).modal('hide');
    };
    $scope.GriddataPOlock = [];
    $scope.getalldataPOUnlock = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/Requisition/GetListForPOUnClose',
        }).then(function successCallback(response) { //datagatefun
            $scope.GriddataPOlock = response.data;
        });
    };

    $scope.getalldataPOUnlock();

    $scope.onClickPOlock = function (args) {
        //debugger;
        var gridObj = $("#GridUc").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];
        $scope.approvalAlertUnlock();

    };
    $scope.approvalAlertUnlock = function () {
        $scope.message = 'Are you sure want to Approve?';

        angular.element(document.querySelector('#POPUnlock')).modal('show');
    };
    $scope.PoUnlock = function () {
        $http({
            method: 'POST',
            url: 'Products/Requisition/POUnClose',
            data: {
                'PoId': $scope.data.Id,
                'PoValue': $scope.data.TotalQty
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getalldataPOUnlock();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });

    }

    $scope.commandPoUnlock = [{

        type: "details", buttonOptions: {
            text: "Po lock",
            width: "120",
            height: "20",


            click: $scope.onClickPOlock
        }
    }];
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.GriddataPOListforPoclosedui = [];
    $scope.getalldataPOListforPoclosedui = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/Requisition/GetListForAllPOList',
        }).then(function successCallback(response) { //datagatefun
            $scope.GriddataPOListforPoclosedui = response.data;
        });
    };

    $scope.getalldataPOListforPoclosedui();

    $scope.onClickPoList = function (args) {
        //debugger;
        var gridObj = $("#GridPOListforPoclosedui").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];
        $scope.approvalAlertPoList();

    };
    $scope.approvalAlertPoList = function () {
        $scope.message = 'Are you sure want to Approve?';

        angular.element(document.querySelector('#AllPoListmi')).modal('show');
    };
    $scope.PoListinClose = function () {
        $http({
            method: 'POST',
            url: 'Products/Requisition/POClose',
            data: {
                'PoId': $scope.data.Id,
                'PoValue': $scope.data.TotalQty
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getalldataPOListforPoclosedui();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });

    }

    $scope.commandAllPoList = [{

        type: "details", buttonOptions: {
            text: "Po lock",
            width: "120",
            height: "20",


            click: $scope.onClickPoList
        }
    }];
    $scope.tab = 1;
    $scope.setTabpou = function (newTab) {
        $scope.tab = newTab;
        $scope.getalldata1();

    };
    $scope.isSetpou = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.tab = 1;
    $scope.isSetpoa12 = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.MasterOrderList = function () {
        $scope.getalldataListForMasterOrder();
        angular.element(document.querySelector('#ListOfMasterOrder')).modal('show');
    };

    $scope.MasterOrderListHide = function () {
        angular.element(document.querySelector('#ListOfMasterOrder')).modal('hide');
    };

    $scope.GetListForMasterOrder = [];
    $scope.getalldataListForMasterOrder = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/Requisition/GetListForMasterOrder',
        }).then(function successCallback(response) { //datagatefun
            $scope.GetListForMasterOrder = response.data;
        });
    };


    $scope.Getrecorddoubleclick = function ($event, index) {
        //debugger;
        var x = $event;
        var Id = x.data.Id;
        $scope.MONo = Id;
        getMasterItemList();
        angular.element(document.querySelector('#ListOfMasterOrder')).modal('hide');

    };

    function getMasterItemList() {
        //debugger;
        $scope.inventoryMaterialList = [];
        $http.get($scope.path + 'GetMasterItemList?masterOrderId=' + $scope.MONo)
            .then(function (response) {

                $scope.inventoryMaterialList = response.data;
                $scope.GetSalesTaxData();
            });
    }
    $scope.calculateAmountByRateFG = function (data) {
        //debugger;
        data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
        if (data.TrnAmount === 'NaN')
            data.TrnAmount = 0;
        data.TaxAmount = 0;
        angular.forEach(data.TaxList, function (item) {
            item.TaxAmount = data.TrnAmount * item.Percentage / 100;
            data.BaseTaxAmount += item.TaxAmount;
        });
        data.BaseAmount = parseFloat($scope.productNew.ToCurrencyRate * data.TrnAmount).toFixed(2);
    };
    $scope.changeServiceForFG = function () {
        //debugger;

        $scope.serviceModel.CurrencyName = "INR";
        $scope.serviceModel.ToCurrencyRate = 1;
        if (baseService.isUndefinedOrNull($scope.serviceModel.ServiceMasterId))
            return $scope.taxCategoryList = [];
        var hsnCodeId = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCodeId;
        getTaxCategoryListForFGService(hsnCodeId);
    };
    function getTaxCategoryListForFGService(hsnCodeId) {
        $scope.taxCategoryList = [];
        $http({
            method: 'GET'
            , url: $scope.path + 'GetTaxCategoryListForFGService?partyPlantId=' + $scope.productNew.InvoicingPartyPlantId + '&hsnCodeId=' + hsnCodeId
        }).then(function (response) {
            $scope.taxCategoryList = response.data;
        });
    }

    $scope.ServiceListFGAdd = function () {

        //debugger;
        var TempList = [];
        TempList.Id = $scope.serviceModel.ServiceMasterId;

        TempList.ServiceMasterName = angular.element("#ServiceMasterId :selected").text();
        TempList.Amount = $scope.serviceModel.TransactionAmount;
        TempList.TotalTaxAmount = 0;
        TempList.TotalTaxAmount = $filter('sumByKey')($filter('filter')($scope.taxCategoryList), 'TaxAmount');

        $scope.chargesList.push(TempList);
        for (var i = 0; i < $scope.taxCategoryList.length; i++) {
            $scope.taxCategoryList[i].ServiceMasterId = $scope.serviceModel.ServiceMasterId;
            $scope.ChargeTaxList.push($scope.taxCategoryList[i]);
        }

        angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');

    }

    $scope.getServiceTaxFGList = function (data, flag, ServiceId, index) {

        //debugger;
        $scope.LoadTaxButtonClick();

        $scope.Currency = $("#currency option:selected").text();
        $scope.ServiceId = ServiceId;
        $scope.taxAbleAmnt = data.Amount;//+ data.TotalTaxAmount;
        $scope.percentageColumn = flag;

        $scope.currentMaterialRow = index;

        $scope.receiveTaxList = [];
        if ($scope.ChargeTaxList.length > 0) {
            $scope.HSNCode = $scope.ChargeTaxList[0].HSNCode;
            $scope.receiveTaxList = $filter('filter')($scope.ChargeTaxList, { 'ServiceMasterId': ServiceId });
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
        }
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('show');

    }

    $scope.AddReceiveTaxPopUpFG = function (Id, index) { //hossain
        //debugger;
        $scope.detailModelNew = {};
        var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.receiveTaxList), 'TaxAmount');
        for (var j = 0; j < $scope.inventoryMaterialList.length; j++) {

            if ($scope.inventoryMaterialList[j].Id === $scope.PODetailid) {
                $scope.inventoryMaterialList[j].BaseTaxAmount = TotalServiceTaxAmount;
            }
        }


        $scope.detailModelNew.InventoryReceiveDetailId = $scope.currentInventoryReceiveDetailIdRow;
        $scope.detailModelNew.InventoryReceiveId = $scope.productNew.Id;
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {
            var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": $scope.receiveTaxList[i].TaxCategoryId });
            if (getRow.length == 2) {
                ShowResult("You can't add Same Tax two times", 'failure', 'receiveTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxCategoryId)) {
                ShowResult("Select Tax Category.", 'failure', 'receiveTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].Percentage)) {
                ShowResult("Input Percentage.", 'failure', 'receiveTaxPopUp');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxAmount)) {
                ShowResult("Input Tax Amount.", 'failure', 'receiveTaxPopUp');
                return false;
            }
            $scope.TaxList.push($scope.receiveTaxList);


        }
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    }

    $scope.closeReceiveTaxPopUpFG = function () { //hossain        
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');

    }

    $scope.getReceiveTaxListFG = function (data, flag, index, Id) {
        //debugger;
        $scope.PODetailid = data.Id;
        $scope.LoadTaxButtonClick();
        $scope.Currency = $("#currency option:selected").text();
        $scope.currentMaterialRow = index;
        $scope.currentInventoryReceiveDetailIdRow = Id;
        $scope.taxAbleAmnt = data.TrnAmount;
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;
        if (data.TaxList.length > 0) {
            $scope.HSNCode = data.TaxList[0].HSNCode;
            $scope.receiveTaxList = data.TaxList;
        }
        $scope.total = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.receiveTaxList[j].Id = $scope.PODetailid;
            $scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;

        }
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
    }
    $scope.addTaxFG = function () {
        var data = {
            TotalAmount: 0,
            Id: $scope.PODetailid,
            HSNCode: $scope.HSNCode,
            HSNCodeId: null,
            UserName: null,
            TaxCategoryId: null
        };
        $scope.receiveTaxList.push(data);
    };
    $scope.sumSvcTaxAmountFG = function () {
        $scope.serviceModel.TotalTaxAmount = 0;
        for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
            $scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
        }
    };

    $scope.SaveFG = function () {
        ////debugger;
        try {
            $scope.dbval = $scope.StateData;
            $scope.UIval = $scope.productNew.InvoicingState;

            if ($scope.inventoryMaterialList.length === 0) {
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
            }
            else if ($scope.dbval.length === 0) {
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
            }
            else if ($scope.dbval === $scope.UIval) {
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
            }
            else {
                ShowResult('You can not change Invoicing party.Line is available', 'failure', 'invoicingPartyPopUp');

            }

            if (baseService.isUndefinedOrNull($scope.productNew.InvoicingPartyPlantId)) return ShowResult('Invoicing by is required', 'failure');
            if (baseService.isUndefinedOrNull($scope.productNew.DeliveryPartyPlantId)) return ShowResult('Delivery by is required', 'failure');
            $scope.modelValidation('div_docNo', 'productNew', 'DocRefNo');
            $scope.modelValidation('div_docDate', 'productNew', 'DocDate');
            $scope.modelValidation('div_PODate', 'productNew', 'PODate', 'PO Entry Date');
            $scope.manualValidationAddRemove('div_currency', 'productNew', 'CurrencyId');

            if ($scope.productNew.CurrencyId !== $scope.productNew.BaseCurrencyId)
                $scope.manualValidationAddRemove('div_rate  ', 'productNew', 'ToCurrencyRate');
            else
                manualValidation('div_rate', false);

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.productNewForm.$valid) {
                if (new Date($scope.productNew.PODate) < new Date($scope.productNew.DocDate))
                    return manualValidation('div_PODate', true, "PO date can't be less than Doc entry date");
                else
                    manualValidation('div_PODate', false);

                $scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
                $scope.product = Object.assign({}, $scope.productNew);
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrlFg,
                        data: $scope.product,
                        dataType: 'JSON'
                    }).then(function (response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.productNew.Id = response.data.entity.Id;
                            $scope.productNew.PartyName = $scope.product.PartyName;
                            $scope.Action = "Update";
                            $scope.getalldata();
                        }
                    }), function (response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === "Update") {

                    $http({
                        method: 'POST',
                        url: $scope.updateUrlFG,
                        data: $scope.product,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getalldata();
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            throw e;
        }
    };

    $scope.closeServiceChargeTaxPopUpwindowFG = function () {
        angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
    }
    $scope.checkedByList = [];
    $scope.GetSupervisorCboList = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/Requisition/GetSupervisorCbo'
        }).then(function successCallback(response) {
            $scope.checkedByList = response.data;
        });
    }
    $scope.GetSupervisorCboList();


    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
            

    });

    cboService.getCboEntityByPlant(null, null, '', function (result) {
        $scope.EntityList = result;
    });

    //Region default Cbo
    cboService.getCboPlantByCompany(null, function (result) {
        $scope.CboPlantByCompanyList = result;
    });
    $scope.CboPlantByCompanyList = null;


    //EndRegion default Cbo

    $scope.GetMaterialTypeList = [];
    $scope.GetMaterialTypeCboList = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/Procurement/GetMaterialTypeCbo'
        }).then(function successCallback(response) {
            $scope.GetMaterialTypeList = response.data;
        });
    }
    $scope.GetMaterialTypeCboList();



    $scope.GetQualityStdList = [];
    $scope.GetQualityStdCboList = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/Procurement/GetQualityStdCbo'
        }).then(function successCallback(response) {
            $scope.GetQualityStdList = response.data;
        });
    }
    $scope.GetQualityStdCboList();


    $scope.ReqList = [];
    $scope.GetReq = function () {
        //debugger;
        $http({
            method: 'GET',
            dataType: 'JSON',
            url: 'Products/Procurement/GetDataByProcurementMasterId'
        }).then(function successCallback(response) {
            $scope.ReqList = response.data;
        });
    }
    $scope.GetReq();

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
        $scope.GLUrl1 = "Accounts/glitem/GetAllGLBudgetActivityList";
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
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };

    $scope.closeCOAICodeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };

    $scope.setSelected = function (data) {
        $scope.addRow(data);
        $scope.closeCOAICodeListPopUp();
    };

    $scope.addRow = function (data) {
        $scope.detailModelNew.GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.detailModelNew.BudgetMasterId = data.BudgetMasterId;
        $scope.detailModelNew.ActivityId = data.ActivityId;
        $scope.detailModelNew.ActivityName = data.ActivityName
    };

    //Remove it
    $scope.addRow = function (data) {
        $scope.detailModelNew.GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.detailModelNew.BudgetMasterId = data.BudgetMasterId;
        $scope.detailModelNew.ActivityId = data.ActivityId;
        $scope.detailModelNew.ActivityName = data.ActivityName
    };


   

    $scope.Save = function () {
        //debugger;
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.productNewForm.$valid) {
                $scope.product = Object.assign({}, $scope.productNew);
                //$scope.materialValidation();
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.product,
                        dataType: 'JSON'
                    }).then(function (response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.productNew.Id = response.data.Id;
                            $scope.Action = "Update";
                            $scope.GetReq();
                        }
                    }), function (response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === "Update") {

                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: $scope.product,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getalldata();
                            $scope.GetReq();

                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            throw e;
        }
    };

    $scope.Delete = function () {
        //debugger;
        if (!baseService.isUndefinedOrNull($scope.productNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.productNew.Id,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.GetReq();
                    ClearFields();

                    //$scope.getaldataOperationMaster();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }

        else
            ShowResult('First delete all line item.', 'failure');

    };

    

   



    $scope.cboPositionList = [];
    cboService.getCboPositionByCompanyGroup(null, function (result) {
        $scope.cboPositionList = result;
    });


     //********************** Position PopUp Start ************************************

    $scope.positionSearchList = [];
    $scope.positionDataList = [];
    $scope.positionSearch = [];
    $scope.positionUrl = "Organizations/Position/GetList";
    $scope.positionParameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "UserName",
        searchBy: "Id",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.positionPopUp = function () {
       
            $scope.positionParameters.entityId = $scope.productNew.EntityId;
            $scope.getPositionData = function (pageno) {
                baseService.paginationBase($scope.positionUrl, pageno, $scope.positionParameters)
                    .then(function (response) {
                        $scope.positionDataList = response.Rows;
                        $scope.positionParameters.total_count = response.Total;
                        if (baseService.arrayLength($scope.positionSearchList) === 0) {
                            $scope.positionSearchList.push(
                                {
                                    "Text": "Id",
                                    "Value": "Id"
                                });
                            baseService.getDDLSearchColumn($scope.positionDataList, $scope.positionSearchList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, "failure");
                    }).finally(function () {
                    });
                angular.element(document.querySelector("#positionPopUp")).modal("show");
            };
            $scope.getPositionData();
    };

    $scope.closePositionPopUp = function () {
        angular.element(document.querySelector("#positionPopUp")).modal("hide");
    };

    $scope.selectPositionPopUp = function (data) {
        $scope.selectedPositionId = data.Id;
        $scope.productNew.PositionCode = $scope.selectedPositionId;
        $scope.closePositionPopUp();
    };

    $scope.clearPosition = function () {
        $scope.productNew.PositionCode = null;
    };
        //********************** Position PopUp End ************************************
    
    $scope.Clear = function () {
        ClearFields();
        if (!$rootScope.isCollapsed) $rootScope.toggle();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.product = {};
        $scope.IsBaseOnDueDateEnable = false;
        $scope.productNew = {
            Id: null,
            CompanyGroupId: null,
            CompanyId: null,
            PositionCode: null,
            PlantId: null,
            EntityId: null,
            Days: null,
            ProcurementFrequency: null,
            MaterialType: null,
            QualityStdSet: null,
            CostReductionCategory: null,
            MaterialMasterId: null,
            ArticleId: null,
            ArticleCriticality: null,
            FirstCharacteristicsId: null,
            FirstCharacteristicsValueId: null,
            SecondCharacteristicsId: null,
            SecondCharacteristicsValueId: null,
            ThirdCharacteristicsId: null,
            ThirdCharacteristicsValueId: null,
            MinStockLevel: null,
            MaxStockLevel: null,
            CostingPercentage: null,
            ProcurementPercentage: null,
            QualityApprovalReq: null,
            QualityApprovedBy: null,
            PossitionCodeForApproval: null,
            QualityStdSet: null,
            SupplierQualityReportReq: null,
            RequisitionType: null,
            PriceApproval: null,
            POGroupId: null,
            Imported: null,
            ImportedCurrencyId: null,
            ImportedBaseRate: null,
            ImportedTgtLandedRate: null,
            ImportProcurementLedTimeDays: null,
            ImportedMinimumOrderQty: null,
            ImportedArticleLifeDays: null,
            Local: null,
            LocalCurrencyId: null,
            LocalBaseRate: null,
            LocalTgtLandedRate: null,
            LocalProcurementLedTimeDays: null,
            LocalMinimumOrderQty: null,
            LocalArticleLifeDays: null,
            AutoPoGeneration: null,
            POGenerationCriteria: null,
            PoGenerationDay: null,
            LastProcurementRate: null,
            MinimumProcurementRate: null,
            MaximumProcurementRate: null,
            MaterialMasterName: null,
            ArticleName: null
        };


    }
    loadCurrency();


    $scope.ProcurementMasterReportPdf = function (id, reportFormat) {
        //debugger;
        var reportFormat = "Pdf";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Products/Procurement/ProcurementMasterReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Type=' + $scope.productNew.Type, '_blank');
    };
    $scope.ProcurementMasterReportExcel = function (id, reportFormat) {
        //debugger;
        var reportFormat = "Excel";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Products/Procurement/ProcurementMasterReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Type=' + $scope.productNew.Type, '_blank');
    };
}