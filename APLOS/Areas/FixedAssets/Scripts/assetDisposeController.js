"use strict";
assetDisposeController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "accountService"];
function assetDisposeController(commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, accountService) {
    $rootScope.title = "Fixed Asset Dispose";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherDetailList = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.voucherList = [];
    $scope.isCrBankAmount = false;
    $scope.isDrBankAmount = false;
    $scope.currencyDisable = false;
    $scope.isAdvance = true;


    $scope.partyType = "Customer";
    $scope.postUrl = "accounts/OpeningBalance/PostOBAdvanceJournal";
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $scope.path = 'FixedAssets/FixedAssetRegister/'

    $scope.voucherDetailList = [];
    $scope.searchBy = "DisposeNo"; $scope.search = "";
    $scope.searchByList = [{ value: 'DisposeNo', name: "Dispose No" }, { value: 'EmployeeName', name: "Employee" }, { value: 'Status', name: "Status" }];

    $scope.voucherList = [];
    $scope.getData = function () {
        $http({
            method: 'Post'
            , url: 'FixedAssets/FixedAssetRegister/GetCapitalizeAssetDisposeList'
            , data: { column: $scope.searchBy, value: $scope.search }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.voucherList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.getData();
    $scope.voucher = {
        Id: null,
        DocDate: $filter("dateFiltering")(Date.now()),
        DocRefNo: null,
        Amount: 0,
        Narration: null,
        Remarks: null,
        LorryNo: null,
        IsPark: false,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        EmployeeId: null,
        Designation: null,
        DOJ: null,
        GivenDesignation: null,
        Department: null,
        LegalDesignation: null,
        CurrencyId: null,
        CompanyCurrencyRate: null,
        ToCurrencyRate: null,
        ToCurrencyRate: null,
        PostingDate: $filter("dateFiltering")(Date.now()),

    };

    $scope.voucherDetail = {
        Id: null,
        GLGeneralInfoId: null,
        BudgetMasterId: null,
        ActivityId: null,
        COAICode: null,
        AccountTypeId: null,
        CurrencyId: null,
        DocRefNo: null,
        DrAmount: null,
        CrAmount: null,
        Narration: null,
        BankMasterId: null,
        CashMasterId: null,
        PartyId: null,
        PartyPlantId: null,
        TransactionTypeId: null,
        FAType: null,
        DrDisable: false,
        CrDisable: false,
        CashCurrencyId: null,
        BankCurrencyId: null,
        BankAmount: null
    };
    $scope.Get = function (x) {
        var data = x.rowData;
        $scope.voucher.Status = data.Status;
        $scope.voucher.Id = data.Id;
        $scope.voucher.Remarks = data.Remarks;
        $scope.voucher.LorryNo = data.LorryNo;
        $scope.voucher.TrnCurrency = data.TrnCurrency;
        $scope.voucher.CurrencyId = data.trnCurrencyId;
        $scope.voucher.PartyName = data.CustomerName;
        $scope.voucher.PartyId = data.PartyId;
        $scope.voucher.CompanyCurrencyRate = data.ToCurrencyRate;
        $scope.voucher.ToCurrencyRate = data.ToCurrencyRate;
        $scope.voucher.BaseNagotiationValue = data.BaseNagotiationValue;
        $scope.voucher.DocDate = data.DocDate;
        $scope.voucher.VoucherNo = data.VoucherNo;

        if ($scope.voucher.Status == 'Sales') {
        $scope.getCboPartyPlantList($scope.voucher.PartyId, function (result) {
            $scope.partyPlantList = result;
            angular.forEach($scope.partyPlantList, function (item, i) {
                if (item.IsDefault) {
                    $scope.voucher.InvoicingPartyPlantId = data.PartyPlantId;
                    $scope.voucher.DeliveryPartyPlantId = data.DeliveryPartyPlantId;
                    $scope.voucher.InvoicingByAddress = data.InvoicingByAddress;
                    $scope.voucher.DeliveryByAddress = data.DeliveryByAddress;

                    $scope.voucher.InvoicingState = item.StateName;
                    $scope.voucher.InvoicingGSTIN = item.GSTIN;
                    $scope.voucher.DeliveryState = item.StateName;
                    $scope.voucher.DeliveryGSTIN = item.GSTIN;
                    $scope.voucher.InvoicingStateId = item.StateId;
                }
            });
        });
        }

        if ($scope.voucher.Status == 'Sales') {
            $scope.DisposeTpye();
            // return true;
        }

        $scope.getFARDisposeDetail(data.Id);
        $scope.getFARDisposeAdditionalTax(data.Id);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

    };

    $scope.voucherDetailList = [];
    $scope.getFARDisposeDetail = function (fixedAssetRegisterDisposeId) {
        $http({
            method: 'GET',
            url: $scope.path + "GetCapitalizedAssetRegisterDisposeEditList?fixedAssetRegisterDisposeId=" + fixedAssetRegisterDisposeId,
        }).then(function successCallback(response) {
            $scope.voucherDetailList = response.data;
            var TaxDocDate = $filter('dateFiltering')(new Date($scope.voucher.DocDate), 'dd-MM-yyyy');
            $scope.getTaxCodeByTaxYearWithhold(TaxDocDate);
        });
    }
    $scope.advanceTaxesList = [];
    $scope.getFARDisposeAdditionalTax = function (fixedAssetRegisterDisposeId) {
        $scope.advanceTaxesList = [];
        $http({
            method: 'GET',
            url: $scope.path + "GetCapitalizedAssetRegisterDisposeAdditionalTaxList?fixedAssetRegisterDisposeId=" + fixedAssetRegisterDisposeId,
        }).then(function successCallback(response) {
            $scope.advanceTaxesList = response.data;
        });
    }


    $scope.removeRow = function (index) {
        $scope.voucherDetailList.splice(index, 1);
    };

    $scope.fixedAssetDisposeStatusList = [];
    $http({
        method: 'GET',
        url: 'Enum/GetFixedAssetDisposeStatusEnumCbo/'
    }).then(function successCallback(response) {
        $scope.fixedAssetDisposeStatusList = response.data;
    });

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.voucher.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else $scope.invalidDocDate = false;
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    };

    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher.Id = null;
        $scope.voucher.Active = true;
        $scope.voucher.Amount = 0;
        $scope.voucher.DocRefNo = null;
        $scope.voucher.Narration = null;
        $scope.voucher.Status = null;
        $scope.voucher.Remarks = null;
        $scope.voucher.LorryNo = null;
        $scope.voucher.PartyId = null;
        $scope.voucher.PartyPlantId = null;
        $scope.voucher.DeliveryPartyPlantId = null;
        $scope.voucher.InvoicingByAddress = null;
        $scope.voucher.DeliveryByAddress = null;
        $scope.voucher.ToCurrencyRate = null;
        $scope.voucher.CurrencyId = null;
        $scope.voucher.PartyName = null;
        $scope.voucher.DocDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucherDetailList = [];
        $scope.advanceTaxesList = [];
        $scope.advanceTax = {};
    };

    $scope.showEmployeeListPopUp = function () {
        baseService.setCurrentPage('employeeList');
        $scope.getEmployeeData = function (pageno) {
            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'accounts/EmployeePayable/GetEmployeeListAllPlant';
            }
            else {
                url = $scope.employeeUrl;
            }
            baseService.paginationBase(url, pageno, $scope.employeeParameters)
                .then(function (result) {
                    $scope.employeeList = result.Rows;
                    $scope.employeeParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeePopUp')).modal('show');
        $scope.getEmployeeData();
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.voucher.EmployeeId = employee.SystemId;
            $scope.voucher.EmployeeName = employee.EmployeeCode + ' - ' + employee.EmployeeName;
            $scope.voucher.DOJ = employee.DOJ;
            $scope.voucher.Department = employee.Department;
            $scope.voucher.Designation = employee.Designation;
            $scope.voucher.GivenDesignation = employee.GivenDesignation;
            $scope.voucher.LegalDesignation = employee.LegalDesignation;
        }
        $scope.hideEmployeePopUp();
    };

    $scope.validation = function () {
        if ($scope.voucher.Status == 'CompensateByEmployee' && baseService.isUndefinedOrNull($scope.voucher.EmployeeId)) {
            ShowResult("Please select Employee!", "failure");
            return true;
        }
        if ($scope.voucher.Status == 'Sales' && baseService.isUndefinedOrNull($scope.voucher.PartyId)) {
            ShowResult("Please select Customer!", "failure");
            return true;
        }
        if ($scope.voucher.Status == 'Sales' && baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure");
            return true;
        }
        if ($scope.voucher.Status == 'Sales' && baseService.isUndefinedOrNull($scope.voucher.CompanyCurrencyRate)) {
            ShowResult("Please Input Rate!", "failure");
            return true;
        }
        if ($scope.voucherDetailList.length == 0) {
            ShowResult("Please select Fixed Asset Register!", "failure");
            return true;
        }
        if ($scope.voucherDetailList.length > 0) {
            for (var i = 0; i < $scope.voucherDetailList.length; i++) {
                if (new Date($scope.voucherDetailList[i].PurchaseDate) > new Date($scope.voucher.DocDate)) {
                    ShowResult("Doc date must be greater or equal to Invoice Date!", "failure");
                    return true;
                }
            } 
        }
        else {
            return false;
        }

    };


    $scope.Save = function () {
        $scope.voucher.ToCurrencyRate = $scope.voucher.CompanyCurrencyRate;
        $scope.voucher.PartyPlantId = $scope.voucher.InvoicingPartyPlantId;
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.form0.$valid && !$scope.validation()) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "fixedassets/fixedassetregister/CreateCapitalizeAssetLost",
                    data: {
                        "fixedAssetDisposed": $scope.voucher,
                        "assetRegisterList": $scope.voucherDetailList,
                        "disposedTaxList": $scope.receiveTaxList
                        
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        var TaxDocDate = $filter('dateFiltering')(new Date($scope.voucher.DocDate), 'dd-MM-yyyy');
                        $scope.voucher = response.data.Data;
                        $scope.getTaxCodeByTaxYearWithhold(TaxDocDate);
                        //$scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            
            return true;
        }
    };

    $scope.voucherId = null;
    $scope.confirmPost = function (voucherId) {
        $scope.voucherId = voucherId;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };




    $scope.searchByFARegister = "AssetRegisterId"; $scope.searchFARegister = "";
    $scope.searchByFARegisterList = [{ value: 'AssetRegisterId', name: "AssetRegisterId" }, { value: 'AssetSlNo', name: "AssetSlNo" }, { value: 'AssetCondition', name: "Asset Condition" }, { value: 'UserReference', name: "UserReference" }, { value: 'OldReference', name: "OldReference" }, { value: 'UserGroup', name: "UserGroup" }, { value: 'FixedAssetMaster', name: "FixedAssetMaster" }, { value: 'FixedAssetItem', name: "FixedAssetItem" }];

    $scope.assetRegisterPopUpList = [];
    $scope.getFixedAssetRegisterPopUpList = function () {
        $http({
            method: 'Post'
            , url: 'FixedAssets/FixedAssetRegister/GetCapitalizeAssetRegisterPopUpList'
            , data: { column: $scope.searchByFARegister, value: $scope.searchFARegister }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.assetRegisterPopUpList = response.data;
            if (baseService.arrayLength($scope.voucherDetailList) > 0) {
                for (var i = 0; i < baseService.arrayLength($scope.voucherDetailList); i++) {
                    for (var j = 0; j < baseService.arrayLength($scope.assetRegisterPopUpList); j++) {
                        if ($scope.voucherDetailList[i].FixedAssetRegisterId == $scope.assetRegisterPopUpList[j].FixedAssetRegisterId) {
                            $scope.assetRegisterPopUpList[j].Active = true;
                        }
                    }
                }
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
        angular.element(document.querySelector("#assetRegisterPopUpmodal")).modal("show");
    };
    $scope.closeFARegisterPopUp = function () {
        angular.element(document.querySelector("#assetRegisterPopUpmodal")).modal("hide");
    }
    function checkFAExist(list, FixedAssetRegisterId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].FixedAssetRegisterId === FixedAssetRegisterId) {

                return true;
            }
        }
        return false;
    }
    $scope.selectFARegisterPopUp = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.Status)) {

            if (baseService.arrayLength($scope.assetRegisterPopUpList) > 0) {
                $scope.voucherDetailList = [];
                angular.forEach($scope.assetRegisterPopUpList, function (a) {
                        if (a.Active) {
                            $scope.voucherDetail.BudgetMasterId = a.BudgetMasterId;
                            $scope.voucherDetail.BudgetName = a.BudgetName;
                            $scope.voucherDetail.ActivityId = a.ActivityId;
                            $scope.voucherDetail.ActivityName = a.ActivityName;
                            $scope.voucherDetail.GLGeneralInfoId = a.GLGeneralInfoId;
                            $scope.voucherDetail.GLGeneralInfoName = a.GLGeneralInfoCode + '-' + a.GLGeneralInfoName;

                            $scope.voucherDetail.FixedAssetRegisterId = a.FixedAssetRegisterId;
                            $scope.voucherDetail.AssetRegisterId = a.AssetRegisterId;
                            $scope.voucherDetail.FixedAssetMaster = a.FixedAssetMaster;
                            $scope.voucherDetail.FixedAssetItem = a.FixedAssetItem;
                            $scope.voucherDetail.AssetSlNo = a.AssetSlNo;
                            $scope.voucherDetail.Status = a.Status;
                            $scope.voucherDetail.AssetCondition = a.AssetCondition;
                            $scope.voucherDetail.UserReference = a.UserReference;
                            $scope.voucherDetail.OldReference = a.OldReference;
                            $scope.voucherDetail.UserGroup = a.UserGroup;
                            $scope.voucherDetail.Remarks = a.Remarks;
                            $scope.voucherDetail.AssetAmount = a.AssetAmount;
                            $scope.voucherDetail.DepreciationAmount = a.DepreciationAmount;
                            $scope.voucherDetail.NetAmount = a.NetAmount;
                            $scope.voucherDetail.AdjustmentDepreciationAmount = 0;
                            $scope.voucherDetail.NegotiationValue = 0;

                            if ($scope.voucher.Status == 'Scrap') {
                                $scope.voucherDetail.NegotiationValue = a.NetAmount;
                            }
                  
                            $scope.voucherDetail.PartyType = 'Capitalize Asset';
                            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                            $scope.voucherDetail = {};
                        }
                });
                $scope.closeFARegisterPopUp();
            }
            

        }
        else {
            ShowResult('Please select Type !!', 'failure', 'assetRegisterPopUpmodal');
        }

    };
    $scope.showPopup = function () {
        angular.element(document.querySelector('#employeeSelectionPopUp')).modal('show');
    }
    $scope.hidePopup = function () {
        angular.element(document.querySelector('#employeeSelectionPopUp')).modal('hide');
    }

    $scope.invoicingPartyPopUp = function () {
        //debugger;
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };
    $scope.closePartyPopUp = function (x) {
        //if ($scope.partyIndex !== -1) {


        var party = x.data;// $scope.partyList[$scope.partyIndex];
        $scope.voucher.PartyName = party.Code + " - " + party.UserName;
        $scope.voucher.PartyId = party.Id;
        $scope.voucher.PaymentTermId = party.PaymentTermId;
        $scope.voucher.CurrencyId = party.CurrencyId;
        $scope.GetCurrencyExchangeRateList();
        //  $scope.changePaymentTerm($scope.salesVM.PaymentTermId);
        $scope.partyPlantList = [];
        $scope.getCboPartyPlantList(party.Id, function (result) {
            $scope.partyPlantList = result;
            angular.forEach($scope.partyPlantList, function (item, i) {
                if (item.IsDefault) {
                    $scope.partyPlantId = item.Value;
                    $scope.voucher.InvoicingPartyPlantId = item.Value;
                    $scope.voucher.DeliveryPartyPlantId = item.Value;
                    $scope.voucher.InvoicingByAddress = item.Address1;
                    $scope.voucher.DeliveryByAddress = item.Address1;
                    $scope.voucher.InvoicingState = item.StateName;
                    $scope.voucher.InvoicingGSTIN = item.GSTIN;
                    $scope.voucher.DeliveryState = item.StateName;
                    $scope.voucher.DeliveryGSTIN = item.GSTIN;
                    $scope.voucher.InvoicingStateId = item.StateId;
                }
            });
        });
        //}
        $scope.hidePartyPopUp();
    };
    $scope.closeInvoicingPartyPopUp = function () {
        //if ($scope.salesMaterialList.length || $scope.chargesList.length) {

        if (!baseService.isUndefinedOrNull($scope.voucher.ChangeInvoicingStateId)) {
            if ($scope.voucher.PlantStateId == $scope.voucher.InvoicingStateId == $scope.voucher.ChangeInvoicingStateId)
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
            else if ($scope.voucher.InvoicingStateId == $scope.voucher.ChangeInvoicingStateId)
                angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
            else if ($scope.voucher.PlantStateId != $scope.voucher.InvoicingStateId && $scope.voucher.PlantStateId != $scope.voucher.ChangeInvoicingStateId)
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
                $scope.voucher.InvoicingState = state;
                $scope.voucher.ChangeInvoicingStateId = stateId;
                $scope.voucher.InvoicingGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.voucher.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.voucher.DeliveryState = state;
                $scope.voucher.DeliveryGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.voucher.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.voucher.InvoicingState = null;
                $scope.voucher.InvoicingGSTIN = null;
                return $scope.voucher.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.voucher.DeliveryState = null;
                $scope.voucher.DeliveryGSTIN = null;
                return $scope.voucher.DeliveryByAddress = null;
            }
        }
    };

    $scope.currencyExchangeRate = [];
    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.PostingDate) && !baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.voucher.PostingDate + "&currencyId=" + $scope.voucher.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.voucher.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
                if ($scope.voucherDetailList.length > 0) {
                    $scope.updateBooksNegotiationValue();
                }
            });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    $scope.DisposeTpye = function () {
        $scope.voucherDetailList = [];
    };

    $scope.calBooksNegotiationValue = function (data) {
        var assetNegotiationValue = parseFloat(data.NegotiationValue);
        if (assetNegotiationValue === 0 || assetNegotiationValue <0) {
            data.NegotiationValue = "";
            ShowResult("Negotiation Amount should be greater than 0(zero).", "failure");
        }
        if ($scope.voucher.Status == 'CompensateByEmployee') {
            data.BaseNagotiationValue = data.NegotiationValue;
        }
        else {
            data.BaseNagotiationValue = data.NegotiationValue * $scope.voucher.CompanyCurrencyRate;
        }
    }
    $scope.calBooksAdjustmentDepreciation = function (data) {
        var assetNetBaseBookValue = parseFloat(data.NetBaseBookValue), assetAdjustmentDepreciation = parseFloat(data.AdjustmentDepreciationAmount);
        if (assetAdjustmentDepreciation > assetNetBaseBookValue) {
            data.AdjustmentDepreciationAmount = "";
            ShowResult("Adjustment Depreciation Amount should not exceed Net Base Amount.", "failure");
        }
    }
    $scope.updateBooksNegotiationValue = function () {
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            $scope.voucherDetailList[i].BaseNagotiationValue = $scope.voucherDetailList[i].NegotiationValue * $scope.voucher.CompanyCurrencyRate
        }
    }

    $scope.onClickExcelPrints = function (args) {

        try {
            var data = args.data;
            var reportFormat = "Excel";

            var file_src = 'FixedAssets/FixedAssetRegister/GetBulletinTamplateIndexReport?reportFormat=' + reportFormat + '&fixedAssetRegisterDisposeId=' + data.Id
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    $scope.onClickPdfPrints = function (args) {

        try {
            var data = args.data;
            var reportFormat = "Pdf";

            var file_src = 'FixedAssets/FixedAssetRegister/GetFixedAssetDisposePdfReport?reportFormat=' + reportFormat + '&fixedAssetRegisterDisposeId=' + data.Id
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
        $scope.taxCategoryList = result;
    });

    $scope.receiveTaxList = [];
    $scope.currentMaterialRow = 0;
    $scope.AssetRegisterId = "";
    $scope.getMaterialTaxList = function (data, flag, index) {
        $scope.percentageColumn = flag;
        $scope.currentMaterialRow = index;
        $scope.taxAbleAmnt = data.NegotiationValue;
        $scope.AssetRegisterId = data.AssetRegisterId;

        $scope.totalTaxAmount = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.totalTaxAmount = $scope.totalTaxAmount + $scope.receiveTaxList[j].Amount;
        }
        $scope.voucherDetailList[$scope.currentMaterialRow].TaxAmount = parseFloat($scope.totalTaxAmount);
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
    };
    $scope.closeReceiveTaxPopUp = function () {
        try {
            $scope.totalTaxAmount = 0;
            for (var j = 0; j < $scope.receiveTaxList.length; j++) {
                $scope.totalTaxAmount = $scope.totalTaxAmount + $scope.receiveTaxList[j].Amount;
            }
            $scope.voucherDetailList[$scope.currentMaterialRow].TaxAmount = parseFloat($scope.totalTaxAmount);
            angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure', 'receiveTaxPopUp');
        }
    };

    $scope.closeReceiveTaxPopUpwindow = function () {
        $scope.totalTaxAmount = 0;
        for (var j = 0; j < $scope.receiveTaxList.length; j++) {
            $scope.totalTaxAmount = $scope.totalTaxAmount + $scope.receiveTaxList[j].Amount;
        }
        $scope.voucherDetailList[$scope.currentMaterialRow].TaxAmount = parseFloat($scope.totalTaxAmount);
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
    }
    $scope.addTax = function () {
        var data = {
            Amount: 0,
            Id: null,
            AssetRegisterId: $scope.AssetRegisterId,
            Percentage: null,
            UserName: null,
            TaxCategoryId: null
        };
        $scope.receiveTaxList.push(data);
    };
    $scope.taxDel = function (Id, index) {
        if (Id === null) {
            $(this).remove();
            $scope.receiveTaxList.splice(index);
            return false;
        }
    };
    $scope.calculateTaxAmount = function (data) {

        data.Amount = parseFloat($scope.taxAbleAmnt * data.Percentage / 100).toFixed(2);
    };
    $scope.checkRowValidation = function (x) {
        for (var i = 0; i < $scope.receiveTaxList.length; i++) {

            if ($scope.receiveTaxList[i].Id === x.Id) {
                $scope.receiveTaxList[i].Percentage = (parseFloat(x.Amount / $scope.taxAbleAmnt).toFixed(2) * 100);
            }

        }
    }
    $scope.onchangeFunction1 = function (id) {
        $scope.TaxCategoryId = id;

        var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": id });
        if (getRow.length === 2) {
            ShowResult("You can't add Same Tax two times", 'failure', 'receiveTaxPopUp');
        }
    };

    $scope.FixedDisposeTaxInvoice = function (data) {
        location.href = "FixedAssets/FixedAssetRegister/FixedDisposeTaxInvoice?disposeId=" + data.data.Id;
    };

    //#region Additional TAX Code
    $scope.advanceTax = { TotalSumAfterTCSVal: 0 };
    $scope.advanceTaxesList = [];
    $scope.additionalTax = function () {
        for (var i = 0; i < $scope.advanceTaxesList.length; i++) {
            if ($scope.advanceTaxesList[i].TaxCodeId === $scope.advanceTax.TaxCodeId) {
                ShowResult("Tax Already Added");
                return false;
            }

        }

        if (manualValidation("td_TaxCode", baseService.isUndefinedOrNull($scope.advanceTax.TaxCodeId), "Tax Code is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TaxCodeAmount", baseService.isUndefinedOrNull($scope.advanceTax.TaxAmount), "Amount is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_TaxCodeCompanyCurrencyAmount", baseService.isUndefinedOrNull($scope.advanceTax.CompanyCurrencyAmount), $scope.companyCurrencyCode + " is required.")) {
            $scope.invalidRow = true;
        }
        else {
            $scope.advanceTax.TaxName = $.grep($scope.taxCodCboListWithhold, function (item) {
                return item.Id === $scope.advanceTax.TaxCodeId;
            })[0].UserName;

            $scope.advanceTaxesList.push($scope.advanceTax);
            $scope.advanceTax = {};
            $scope.TotalSumAfterTCS();
        }

    };

    $scope.taxCodCboListWithhold = [];
    $scope.taxcodelistMessage = "";
    $scope.getTaxCodeByTaxYearWithhold = function (date) {
        $scope.voucher.TaxOptionAddiTax = 'Yes';
        $http({
            method: "Get",
            url: "accounts/TaxCode/GetAdditionalTaxOutputCbo?postingDate=" + $filter("dateFiltering")(date)
        }).then(
            function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.taxcodelistMessage = response.data.Message;
                }
                else {
                    $scope.taxCodCboListWithhold = response.data;;
                }
            },
            function errorCallback(response) {
            });
    };
    //$scope.getTaxCodeByTaxYearWithhold($filter("dateFiltering")(Date.now()));
    $scope.selectadditionalTax = function () {
        $scope.advanceTax.ValueOfFixed = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].ValueOfFixed;
        $scope.advanceTax.Type = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].Type;
        $scope.advanceTax.TaxCategoryId = $.grep($scope.taxCodCboListWithhold, function (item) {
            return item.Id === $scope.advanceTax.TaxCodeId;
        })[0].TaxCategoryId;
        if ($scope.advanceTax.Type == 'FixedPercentage' && !baseService.isUndefinedOrNull($scope.advanceTax.ValueOfFixed)) {//* $scope.advanceTax.ValueOfFixed / 100

            $scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "NegotiationValue")) + parseFloat($filter("sumByKey")($filter("filter")($scope.receiveTaxList), "Amount"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);
            
        } else {
            $scope.advanceTax.TaxAmount = $scope.advanceTax.ValueOfFixed;
        }
        $scope.TotalSumAfterTCS();
    }

    $scope.SaveAdditinalTax = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.voucher.ToCurrencyRate) || $scope.voucher.ToCurrencyRate == 0) {
                $scope.voucher.ToCurrencyRate = $scope.voucher.CompanyCurrencyRate;
            }

            if ($scope.voucher.DisposedVoucherId != null) {
                throw "Posted data cann't save";
            }
            if (baseService.arrayLength($scope.advanceTaxesList) == 0) {
                throw "Add row for Additional Tax.";
            }
            $http({
                method: 'POST',
                url: 'FixedAssets/FixedAssetRegister/SaveAdditinalTax',
                data:
                {
                    'fixedAssetRegisterDisposedId': $scope.voucher.Id,
                    'BooksCurrencyBaseRate': $scope.voucher.ToCurrencyRate,
                    'UserSendData': $scope.advanceTaxesList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.TotalSumAfterTCS();
                    $scope.Clear();

                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }



    $scope.GetAdvanceTaxInfo = function (Id) {

        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'SalesManagements/Sales/GetAdvanceTaxInfo?SalesId=' + Id,
        }).then(function successCallback(response) {
            $scope.advanceTaxesList = response.data;

            $scope.advanceTax.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "NegotiationValue")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.receiveTaxList), "Amount"))).toFixed(2);

        });

    }
    $scope.removeTaxesRow = function (Id, index) {
        if (baseService.isUndefinedOrNull(Id)) {
            $scope.advanceTaxesList.splice(index, 1);

        }
        else {
            $scope.DeleteAdditinalTax(Id);
            $scope.GetAdvanceTaxInfo($scope.salesVM.Id);
        }
    };
    $scope.DeleteAdditinalTax = function (Id) {
        $http({
            method: 'POST',
            url: 'SalesManagements/Sales/AdditionalTaxDelete?Id=' + Id,
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    $scope.TaxOptionAdditax = function (data) {
        $scope.salesVM.TaxOptionAddiTax = data;
    };

    $scope.calculateTaxAmountForAdditionalTax = function (data) {
        $scope.TaxAmountVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "NegotiationValue")) + parseFloat($filter("sumByKey")($filter("filter")($scope.receiveTaxList), "Amount"))).toFixed(2);

        $scope.advanceTax.TaxAmount = (($scope.TaxAmountVal * data) / 100).toFixed(2);

    };
    $scope.checkRowValidationSdditionalTax = function (data) {

        $scope.TaxAmountVal1 = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "NegotiationValue")) + parseFloat($filter("sumByKey")($filter("filter")($scope.receiveTaxList), "Amount"))).toFixed(2);
        $scope.advanceTax.ValueOfFixed = ((data / $scope.TaxAmountVal1) * 100).toFixed(4);
    }
    //$scope.TotalSumAfterTCSVal = "";
    $scope.TotalSumAfterTCS = function () {
        $scope.advanceTax.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "NegotiationValue")) + parseFloat($filter("sumByKey")($filter("filter")($scope.receiveTaxList), "Amount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);
    }

    //#endregion
}
