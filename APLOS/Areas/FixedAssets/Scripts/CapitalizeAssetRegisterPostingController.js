"use strict";
CapitalizeAssetRegisterPostingController.$inject = ["addressService", "commonMessage", "$scope", "$rootScope", "baseService", "cboService", "$http", "$filter", "$controller", "$window"];
function CapitalizeAssetRegisterPostingController(addressService, commonMessage, $scope, $rootScope, baseService, cboService, $http, $filter, $controller, $window) {
    $rootScope.title = "Capitalize Asset Register Posting";
    $scope.Action = "Save";
    $scope.message_confirmation = "";
    $scope.path = "fixedassets/fixedassetregister/";

    $scope.searchBy = "FixedAssetMasterId"; $scope.search = "";
    $scope.searchByList = [{ value: 'VoucherNo', name: "Voucher No" }, { value: 'PostingDate', name: "Posting Date" }, { value: 'FixedAssetMasterId', name: "Asset Master Id" }, { value: 'FixedAssetMaster', name: "Asset Master" }, { value: 'FixedAssetCategory', name: "Asset Category" }, { value: 'FixedAssetSubCategory', name: "Asset Sub Category" }, { value: 'DepreciationProcessDate', name: "Depreciation Process Date" }];

    $scope.voucherList = [];
    $scope.getData = function () {
        $http({
            method: 'Post'
            , url: 'FixedAssets/FixedAssetRegister/GetFixedAssetDepreciationPostedList'
            , data: { column: $scope.searchBy, value: $scope.search }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.voucherList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.getData();

    $scope.modelNew = {
        Id: null, FixedAssetItemId: null, CapitalizationDate: null, Qty: 0, GRNAmount: 0, IssueAmount: 0, ExpensesAmount: 0, Other: null, TotalAmount: 0, ApprovedById: null, IsApproved: false, Status: null, Type: null, VoucherRowId: null, Remark: null, InstallationYear: null, Lifetime: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
    };
    $scope.voucher = {
        Id: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        DocDate: null,
        DocRefNo: null,
        Amount: 0,
        Narration: null,
        Remarks: null,
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
        CompanyCurrencyRate: 1,

        RepaymentStartDate: null,
        LifeOfYear: null,
        ProfitRate: null,
        NoOfInstallmentPerYear: null,
        ProfitAmount: null,
        TotalNoOfInstallment: null,
        PaymentTermId: null,
        BaseOnDueDate: null,
        BaseNoOfDays: null,
        MatureDate: null
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

    $scope.masterList = [];
    $scope.getMasterData = function () {
        $scope.purchaseLCList = [];
        $http.get("fixedassets/fixedassetregister/GetApprovedCapitalizeData")
            .then(
                function successCallback(response) {
                    $scope.masterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#CapitalpopUp')).modal('show');
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#CapitalpopUp')).modal('hide');
    }

    $scope.selectedmaterialMasterList = [];
    $scope.GetCapitalizationMasterDetail = function () {
        $scope.purchaseLCList = [];
        $http.get("fixedassets/fixedassetregister/GetCapitalizationMasterDetail?masterId=" + $scope.voucher.CapitalizationMasterId)
            .then(
                function successCallback(response) {
                    $scope.selectedmaterialMasterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
   
    $scope.SelectMaster = function (x) {
        var data = x.data;
        $scope.voucher.CapitalizationMasterId = data.Id;
        $scope.voucher.FixedAssetMaster = data.FixedAssetMaster;
        $scope.voucher.CurrencyId = data.trnCurrencyId;
        $scope.voucher.CompanyCurrencyRate = data.ToCurrencyRate;
        $scope.voucher.FixedAssetCategory = data.FixedAssetCategory;
        $scope.voucher.FixedAssetSubCategory = data.FixedAssetSubCategory;
        $scope.voucher.BaseCurrency = data.BaseCurrency;
        $scope.voucher.FixedAssetDepreciationAmount = data.FixedAssetDepreciationAmount;
        $scope.voucher.CapitalizationDate = $filter("dateFiltering")(data.CapitalizationDate);

        $scope.GetCapitalizationMasterDetail();
        
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.Action = 'Update';
        angular.element(document.querySelector('#CapitalpopUp')).modal('hide');
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

}