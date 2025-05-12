"use strict";
assetDepreciationPostController.$inject = ["accountService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "$window"];
function assetDepreciationPostController(accountService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $window) {
    $rootScope.title = "Capitalize Asset Depreciation Post";
    $scope.Action = "Post";
    $scope.index = -1;
    $scope.voucherDetailList = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.voucherList = [];
    $scope.isCrBankAmount = false;
    $scope.isDrBankAmount = false;
    $scope.currencyDisable = false;
    $scope.isAdvance = true;

    $scope.path = 'FixedAssets/FixedAssetRegister/';
    $controller("currencyBaseController", { $scope: $scope, $http: $http });

    $controller('employeeBaseController', { $scope: $scope, $http: $http });

    $scope.voucherDetailList = [];
    $scope.searchBy = "VoucherNo"; $scope.search = "";
    $scope.searchByList = [{ value: 'VoucherNo', name: "Voucher No" }, { value: 'PostingDate', name: "Posting Date" }, { value: 'AssetDepreciationId', name: "Asset Depreciation Id" }, { value: 'ProcessName', name: "Process Name" }, { value: 'ProcessDate', name: "Depreciation Process Date" }, { value: 'Status', name: "Status" }];

    $scope.voucherList = [];
    $scope.getData = function () {
        $http({
            method: 'Post'
            , url: 'FixedAssets/FixedAssetRegister/GetAssetDepreciationPostedList'
            , data: { column: $scope.searchBy, value: $scope.search }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.voucherList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.getData();
    
    $scope.voucherDetailList = [];
    $scope.DepreciationSearchBy = "AssetDepreciationId"; $scope.search = "";
    $scope.DepreciationSearchByList = [{ value: 'AssetDepreciationId', name: "Asset Depreciation Id" }, { value: 'ProcessName', name: "Process Name" }, { value: 'ProcessDate', name: "Depreciation Process Date" }];

    $scope.fixedAssetDepreciationList = [];
    $scope.getDepreciationData = function () {
        $http({
            method: 'Post'
            , url: 'FixedAssets/FixedAssetRegister/GetAssetDepreciationListForPosting'
            , data: { column: $scope.DepreciationSearchBy, value: $scope.DepreciationSearch }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.fixedAssetDepreciationList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
        angular.element(document.querySelector('#DepreciationPopUp')).modal('show');
    };

    $scope.closeFixedAssetDepreciationPopUp = function () {
        angular.element(document.querySelector('#DepreciationPopUp')).modal('hide');
    }
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

    $scope.fixedAssetDepreciationDetailList = [];
    $scope.getDataByDepreciationId = function (x) {
        var data = x.data;
        $scope.voucher.AssetDepreciationId = data.AssetDepreciationId;
        $scope.voucher.ProcessName = data.ProcessName;
        $scope.voucher.CurrencyId = data.CurrencyId;
        $scope.voucher.CompanyCurrencyRate = data.ToCurrencyRate;
        $scope.voucher.BaseCurrency = data.BaseCurrency;
        $scope.voucher.DepreciationAmount = data.DepreciationAmount;
        $scope.voucher.ProcessDate = $filter("dateFiltering")(data.ProcessDate);
        $scope.voucher.DocDate = $filter("dateFiltering")(data.ProcessDate);
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.ProcessDate);

        $scope.getDepreciationJV(data.AssetDepreciationId);
        
        angular.element(document.querySelector('#DepreciationPopUp')).modal('hide');
    };

    $scope.fixedAssetDepreciationJVList = [];
    $scope.getDepreciationJV = function (assetDepreciationId) {
        $scope.fixedAssetDepreciationJVList = [];
        $scope.jvurl = 'FixedAssets/FixedAssetRegister/GetAssetDepreciationSingleJVList?assetDepreciationId=' + assetDepreciationId 
        $http({
            method: 'Post'
            , url: $scope.jvurl
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.fixedAssetDepreciationJVList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };

    };

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
        cboService.getCboEntityByPlant(null, null, "", function (result) {
            $scope.entityList = result;
        });
    });

    cboService.getCboTransactionCurrencyByCompany("", function (result) {
        $scope.tranCurrencyList = result;
        $scope.baseCurrencyId = $scope.selectBaseCurrency();
        $scope.voucher.CurrencyId = $scope.baseCurrencyId;
        $scope.GetCurrencyExchangeRateList();
    });

    $scope.getCboVoucherTypeFixedAssetDepreciationJournalList = function () {
        cboService.getCboVoucherTypeFixedAssetDepreciationJournalList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
                $scope.GetCurrencyExchangeRateList();
            }
        });
    };
    $scope.getCboVoucherTypeFixedAssetDepreciationJournalList();
    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.PostingDate) && !baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.voucher.PostingDate + "&currencyId=" + $scope.voucher.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.voucher.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = null;
        }
    };
    $scope.invalidPostingDate = false;
    $scope.checkPostingDate = function () {
        var msg = "";
        if (new Date($scope.voucher.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };
    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.voucher.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else if (new Date($scope.voucher.PostingDate) < new Date($scope.voucher.DocDate)) {
            msg = "Doc date must be below or equal to Posting Date!";
            $scope.invalidDocDate = true;
        }
        else if (baseService.isUndefinedOrNull($scope.voucher.DocDate)) {
            msg = "Doc Date is required.";
            $scope.invalidDocDate = true;
        }
        else $scope.invalidDocDate = false;
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    };
    
    $scope.Clear = function () {
        $scope.Action = "Post";
        $scope.voucher.Active = true;
        $scope.voucher.Amount = 0;
        $scope.voucher.DocRefNo = null;
        $scope.voucher.Narration = null;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucher.AssetDepreciationId = null;
        $scope.voucher.ProcessName = null;
        $scope.voucher.CurrencyId = null;
        $scope.voucher.CompanyCurrencyRate = null;
        $scope.voucher.BaseCurrency = null;
        $scope.voucher.DepreciationAmount = 0;
        $scope.voucher.ProcessDate = null;
        $scope.fixedAssetDepreciationJVList = [];
        $scope.fixedAssetDepreciationDetailList = []; 
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.form0.$valid) {
            $scope.SaveUrl = "fixedassets/FixedAssetRegister/SaveAssetDepreciationPost"
            if ($scope.Action === "Post") {
                $http({
                    method: "POST",
                    url: $scope.SaveUrl,
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.fixedAssetDepreciationJVList,
                        "assetDepreciationId": $scope.voucher.AssetDepreciationId
                    },
                    dataType: "JSON"
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            return true;
        }
    };

    $scope.onClickReportDownloadExcel = function (args) {
        var reportFormat = "Excel";
        try {
            var file_src = $scope.path + 'AssetsDepreciationPostReport?reportFormat=' + reportFormat + '&depreciationVoucherId=' + args.Id
            $rootScope.report(file_src);
        } catch (e) {

        }
    };

    $scope.onClickReportDownloadWord = function (args) {
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(args.Id)) return ShowResult('No Id found', 'failure');
        try {
            $window.open('FixedAssets/FixedAssetRegister/AssetsDepreciationPostReport?reportFormat=' + reportFormat + '&depreciationVoucherId=' + args.Id, '_blank');
            //var file_src = $scope.path + 'AssetsDepreciationPostReport?reportFormat=' + reportFormat + '&depreciationVoucherId=' + args.Id
            //$rootScope.report(file_src);
        } catch (e) {

        }
    };

    $scope.voucherId = null;
    $scope.deletedRemarks = "";
    $scope.confirmDelete = function (data) {
        $scope.voucherId = data.data.Id;
        angular.element(document.querySelector("#confirmDeletePopUp_Remarks")).modal("show");
    };
    $scope.closeconfirmDeletePopUp_Remarks = function () {
        angular.element(document.querySelector("#confirmDeletePopUp_Remarks")).modal("hide");
    };
    $scope.deleteUrl = $scope.path + "/DeleteDepreciationProcessPost";
    $scope.delete = function () {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "voucherId": $scope.voucherId,
                "deletedRemarks": $scope.deletedRemarks
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                $scope.deletedRemarks = "";
                $scope.closeconfirmDeletePopUp_Remarks();
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

}