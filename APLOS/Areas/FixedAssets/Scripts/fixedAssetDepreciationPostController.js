"use strict";
fixedAssetDepreciationPostController.$inject = ["accountService", "cboService","commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function fixedAssetDepreciationPostController(accountService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Fixed Asset Depreciation Post";
    $scope.Action = "Save";
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
    $scope.searchBy = "FixedAssetMasterId"; $scope.search = "";
    $scope.searchByList = [{ value: 'FixedAssetMasterId', name: "Asset Master Id" }, { value: 'FixedAssetMaster', name: "Asset Master" }, { value: 'FixedAssetCategory', name: "Asset Category" }, { value: 'FixedAssetSubCategory', name: "Asset Sub Category" }, { value: 'DepreciationProcessDate', name: "Depreciation Process Date" }];

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
    
    $scope.voucherDetailList = [];
    $scope.DepreciationSearchBy = "FixedAssetMasterId"; $scope.search = "";
    $scope.DepreciationSearchByList = [{ value: 'FixedAssetMasterId', name: "Asset Master Id" }, { value: 'FixedAssetMaster', name: "Asset Master" }, { value: 'FixedAssetCategory', name: "Asset Category" }, { value: 'FixedAssetSubCategory', name: "Asset Sub Category" }, { value: 'DepreciationProcessDate', name: "Depreciation Process Date" }];

    $scope.fixedAssetDepreciationList = [];
    $scope.getDepreciationData = function () {
        $http({
            method: 'Post'
            , url: 'FixedAssets/FixedAssetRegister/GetFixedAssetDepreciationListForPosting'
            , data: { column: $scope.DepreciationSearchBy, value: $scope.DepreciationSearch }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.fixedAssetDepreciationList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
        angular.element(document.querySelector('#DepreciationPopUp')).modal('show');
    };
    //$scope.getDepreciationData();
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
        $scope.voucher.FixedAssetMasterId = data.FixedAssetMasterId;
        $scope.voucher.FixedAssetMaster = data.FixedAssetMaster;
        $scope.voucher.CurrencyId = data.trnCurrencyId;
        $scope.voucher.CompanyCurrencyRate = data.ToCurrencyRate;
        $scope.voucher.FixedAssetCategory = data.FixedAssetCategory;
        $scope.voucher.FixedAssetSubCategory = data.FixedAssetSubCategory;
        $scope.voucher.BaseCurrency = data.BaseCurrency;
        $scope.voucher.FixedAssetDepreciationAmount = data.FixedAssetDepreciationAmount;
        $scope.voucher.DepreciationProcessDate = $filter("dateFiltering")(data.DepreciationProcessDate);

        $scope.fixedAssetDepreciationDetailList.push($scope.voucher);
        $scope.getDepreciationJV(data.FixedAssetMasterId);
        
        angular.element(document.querySelector('#DepreciationPopUp')).modal('hide');
    };

    $scope.fixedAssetDepreciationJVList = [];
    $scope.getDepreciationJV = function (id) {
        $scope.fixedAssetDepreciationJVList = [];
        $scope.jvurl = 'FixedAssets/FixedAssetRegister/GetFixedAssetDepreciationSingleJVList?fixedAssetMasterId=' + id + "&depreciationProcessDate=" + $scope.voucher.DepreciationProcessDate
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

   

    //$scope.Get = function (data) {
    //    $scope.voucher.Id = data.Id;
    //    $scope.Action = "Update";
    //    if (!$rootScope.isCollapsed) {
    //        $rootScope.toggle();
    //    }

    //};
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

    //$scope.getCboVoucherTypeFixedAssetDepreciationJournalList = function () {
    //    cboService.getCboVoucherTypeFixedAssetDepreciationJournalList(function (result) {
    //        $scope.voucherTypeList = result;
    //        if ($scope.voucherTypeList.length === 1) {
    //            $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
    //            $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
    //            $scope.voucher.DocDate = $scope.voucher.PostingDate;
    //            $scope.GetCurrencyExchangeRateList();
    //        }
    //    });
    //};
    //$scope.getCboVoucherTypeFixedAssetDepreciationJournalList();
    //$scope.GetCurrencyExchangeRateList = function () {
    //    if (!baseService.isUndefinedOrNull($scope.voucher.PostingDate) && !baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
    //        $http({
    //            method: "GET",
    //            url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.voucher.PostingDate + "&currencyId=" + $scope.voucher.CurrencyId
    //        }).then(function successCallback(response) {
    //            $scope.currencyExchangeRate = response.data;
    //            $scope.voucher.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
    //        });
    //    }
    //    else {
    //        $scope.currencyExchangeRate = null;
    //    }
    //};
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
    $scope.removeRow = function (index) {
        $scope.voucherDetailList.splice(index, 1);
    };

    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher.Active = true;
        $scope.voucher.Amount = 0;
        $scope.voucher.DocRefNo = null;
        $scope.voucher.Narration = null;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.fixedAssetDepreciationJVList = [];
        $scope.fixedAssetDepreciationDetailList = []; 
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.form0.$valid) {
                $scope.SaveUrl = "fixedassets/FixedAssetRegister/CreateFixedAssetDepreciationPost"
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.SaveUrl,
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.fixedAssetDepreciationJVList,
                        "farDepreciationDetailList": $scope.fixedAssetDepreciationDetailList
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

    $scope.voucherId = null;
    $scope.confirmPost = function (voucherId) {
        $scope.voucherId = voucherId;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.onClickReportDownloadExcel = function (args) {
        var reportFormat = "Excel";
        try {
            var file_src = $scope.path + 'FixedAssetsDepreciationPost?reportFormat=' + reportFormat + '&DepreciationdVoucherId=' + args.Id
            $rootScope.report(file_src);
        } catch (e) {

        }
    };

    $scope.onClickReportDownloadWord = function (args) {
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(args.Id)) return ShowResult('No Id found', 'failure');
        try {
            var file_src = $scope.path + 'FixedAssetsDepreciationPost?reportFormat=' + reportFormat + '&DepreciationdVoucherId=' + args.Id
            $rootScope.report(file_src);
        } catch (e) {

        }
    };

}