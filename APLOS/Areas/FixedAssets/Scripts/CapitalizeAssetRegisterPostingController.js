"use strict";
CapitalizeAssetRegisterPostingController.$inject = ["addressService", "commonMessage", "$scope", "$rootScope", "baseService", "cboService", "$http", "$filter", "$controller", "$window"];
function CapitalizeAssetRegisterPostingController(addressService, commonMessage, $scope, $rootScope, baseService, cboService, $http, $filter, $controller, $window) {
    $rootScope.title = "Capitalize Asset Register Posting";
    $scope.Action = "Save";
    $scope.message_confirmation = "";
    $scope.path = "fixedassets/fixedassetregister/";

    $scope.searchBy = "VoucherNo"; $scope.search = "";
    $scope.searchByList = [{ value: 'VoucherNo', name: "Voucher No" }, { value: 'PostingDate', name: "Posting Date" }, { value: 'FixedAssetMasterId', name: "Asset Master Id" }, { value: 'FixedAssetItemId', name: "Asset Item Id" }, { value: 'FixedAssetMaster', name: "Asset Master" }, { value: 'FixedAssetItem', name: "Asset Item" }, { value: 'FixedAssetCategory', name: "Asset Category" }, { value: 'FixedAssetSubCategory', name: "Asset Sub Category" }];

    $scope.voucherList = [];
    $scope.getData = function () {
        $http({
            method: 'Post'
            , url: 'FixedAssets/FixedAssetRegister/GetCapitalizeAssetRegisterPostedList'
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

    $scope.capitalizationMaster = {
        Id: null,
        FixedAssetItemId: null,
        Qty: null,
        TotalAmount: null
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
        $scope.voucher.Amount = data.TotalAmount;
        $scope.voucher.FixedAssetItem = data.FixedAssetItem;
        $scope.voucher.Qty = data.Qty;
        $scope.voucher.CapitalizationDate = $filter("dateFiltering")(data.CapitalizationDate);

        $scope.capitalizationMaster.Id = data.Id;
        $scope.capitalizationMaster.FixedAssetItemId = data.FixedAssetItemId;
        $scope.capitalizationMaster.Qty = data.Qty;
        $scope.capitalizationMaster.TotalAmount = data.TotalAmount;

        $scope.GetCapitalizationMasterDetail();
        $scope.getCapitalizationJV(data.Id);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        
        angular.element(document.querySelector('#CapitalpopUp')).modal('hide');
    };
    $scope.capitalizationJVList = [];
    $scope.getCapitalizationJV = function (Id) {
        $scope.capitalizationJVList = [];
        $scope.jvurl = 'FixedAssets/FixedAssetRegister/GetCapitalizationSingleJVList?capitalizationMasterId=' + Id
        $http({
            method: 'Post'
            , url: $scope.jvurl
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.capitalizationJVList = response.data;
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
    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
        cboService.getCboEntityByPlant(null, null, "", function (result) {
            $scope.entityList = result;
        });
    });

    cboService.getCboTransactionCurrencyByCompany("", function (result) {
        $scope.tranCurrencyList = result;
        //$scope.baseCurrencyId = $scope.selectBaseCurrency();
        $scope.voucher.CurrencyId = $scope.baseCurrencyId;
        $scope.GetCurrencyExchangeRateList();
    });

    $scope.getCboVoucherTypeFixedAssetCapitalizeJournalList = function () {
        cboService.getCboVoucherTypeFixedAssetCapitalizeJournalList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
                $scope.GetCurrencyExchangeRateList();
            }
        });
    };
    $scope.getCboVoucherTypeFixedAssetCapitalizeJournalList();
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
    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher.Active = true;
        $scope.voucher.Amount = 0;
        $scope.voucher.DocRefNo = null;
        $scope.voucher.Narration = null;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucher.CapitalizationMasterId = null;
        $scope.voucher.Amount = null;
        $scope.voucher.FixedAssetItem = null;
        $scope.voucher.Qty = data.Qty;
        $scope.voucher.CapitalizationDate = null;
        $scope.selectedmaterialMasterList = [];
        $scope.capitalizationJVList = [];

        $scope.capitalizationMaster.Id = null;
        $scope.capitalizationMaster.FixedAssetItemId = null;
        $scope.capitalizationMaster.Qty = null;
        $scope.capitalizationMaster.TotalAmount = null;
        
    };

    $scope.Post = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.form0.$valid) {
            $scope.SaveUrl = "fixedassets/FixedAssetRegister/CreatetCapitalizeAssetRegisterPost"
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.SaveUrl,
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.capitalizationJVList,
                        "capitalizationMasterdata": $scope.capitalizationMaster
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
            var file_src = $scope.path + 'CapitalizeAssetRegisterPostReport?reportFormat=' + reportFormat + '&voucherId=' + args.Id
            $rootScope.report(file_src);
        } catch (e) {

        }
    };

    $scope.onClickReportDownloadWord = function (args) {
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(args.Id)) return ShowResult('No Id found', 'failure');
        try {
            var file_src = $scope.path + 'CapitalizeAssetRegisterPostReport?reportFormat=' + reportFormat + '&voucherId=' + args.Id
            $rootScope.report(file_src);
        } catch (e) {

        }
    };

}