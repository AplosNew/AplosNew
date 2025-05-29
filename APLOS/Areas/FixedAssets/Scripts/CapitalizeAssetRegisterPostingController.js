"use strict";
CapitalizeAssetRegisterPostingController.$inject = ["addressService", "commonMessage", "$scope", "$rootScope", "baseService", "cboService", "$http", "$filter", "$controller", "$window"];
function CapitalizeAssetRegisterPostingController(addressService, commonMessage, $scope, $rootScope, baseService, cboService, $http, $filter, $controller, $window) {
    $rootScope.title = "Capitalize Asset Register Posting";
    $scope.Action = "Save";
    $scope.message_confirmation = "";
    $scope.path = "fixedassets/fixedassetregister/";

    $scope.searchBy = "VoucherNo"; $scope.search = "";
    $scope.searchByList = [{ value: 'VoucherNo', name: "Voucher No" }, { value: 'CapitalizationMasterId', name: "Capitalization Master Id" }, { value: 'PostingDate', name: "Posting Date" }, { value: 'FixedAssetMasterId', name: "Asset Master Id" }, { value: 'FixedAssetItemId', name: "Asset Item Id" }, { value: 'FixedAssetMaster', name: "Asset Master" }, { value: 'FixedAssetItem', name: "Asset Item" }, { value: 'FixedAssetCategory', name: "Asset Category" }, { value: 'FixedAssetSubCategory', name: "Asset Sub Category" }];

    $scope.voucherList = [];
    $scope.getData = function () {
        $http({
            method: 'Post'
            , url: 'FixedAssets/FixedAssetRegister/GetCapitalizeAssetRegisterPostedList'
            , data: { column: $scope.searchBy, value: $scope.search, type: "New" }
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
        $scope.masterList = [];
        $http.get("fixedassets/fixedassetregister/GetApprovedCapitalizeData?type=" + "New")
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
        $scope.selectedmaterialMasterList = [];
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
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.CapitalizationDate);
        $scope.voucher.DocDate = $filter("dateFiltering")(data.CapitalizationDate);
        $scope.voucher.Narration = null;

        $scope.capitalizationMaster.Id = data.Id;
        $scope.capitalizationMaster.FixedAssetItemId = data.FixedAssetItemId;
        $scope.capitalizationMaster.Qty = data.Qty;
        $scope.capitalizationMaster.TotalAmount = data.TotalAmount;

        //$scope.getAssetRegister(data.Id);
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
        $scope.jvurl = 'FixedAssets/FixedAssetRegister/GetCapitalizationSingleJVListFromAssetRegister?capitalizationMasterId=' + Id
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
    $scope.AssetRegisterList = [];
    $scope.getAssetRegister = function (Id) {
        $scope.AssetRegisterList = [];
        $http({
            method: 'POST',
            url: 'fixedassets/FixedAssetRegister/GetAssetRegisterUpdateList',
            data: { column: $scope.searchByAssetRegisterUpdate, value: $scope.searchAssetRegisterUpdate, capitalizationMasterId: Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.AssetRegisterList = response.data;
        });

    };
    
    // #region TAB CHANGE Main
    $scope.tabMain = 1;
    $scope.setTabMain = function (newTab) {
        $scope.tabMain = newTab;
    };
    $scope.isSetMain = function (tabNum) {
        return $scope.tabMain === tabNum;
    };
    // #endregion TAB CHANGE Main

    // #region TAB CHANGE New
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion TAB CHANGE New

    // #region TAB CHANGE Addition
    $scope.tabAddition = 1;
    $scope.setTabAddition = function (newTab) {
        $scope.tabAddition = newTab;
    };
    $scope.isSetAddition = function (tabNum) {
        return $scope.tabAddition === tabNum;
    };
    // #endregion TAB CHANGE Addition

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
        cboService.getCboEntityByPlant(null, null, "", function (result) {
            $scope.entityList = result;
        });
    });

    cboService.getCboTransactionCurrencyByCompany("", function (result) {
        $scope.tranCurrencyList = result;
        $scope.voucher.CurrencyId = $scope.baseCurrencyId;
        $scope.voucherAddition.CurrencyId = $scope.baseCurrencyId;
        $scope.GetCurrencyExchangeRateList();
    });

    $scope.getCboVoucherTypeFixedAssetCapitalizeJournalList = function () {
        cboService.getCboVoucherTypeFixedAssetCapitalizeJournalList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
                $scope.voucherAddition.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucherAddition.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucherAddition.DocDate = $scope.voucher.PostingDate;
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
                $scope.voucherAddition.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = null;
        }
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
        $scope.voucher.Qty = null;
        $scope.voucher.CapitalizationDate = null;
        $scope.selectedmaterialMasterList = [];
        $scope.capitalizationJVList = [];

        $scope.capitalizationMaster.Id = null;
        $scope.capitalizationMaster.FixedAssetItemId = null;
        $scope.capitalizationMaster.Qty = null;
        $scope.capitalizationMaster.TotalAmount = null;
        
    };

    $scope.Post = function () {
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

    $scope.checkedAssetRegisterUpdateList = [];
    $scope.AssetRegisterUpdateAvailableList = [];
    $scope.searchByAssetRegisterUpdate = "AssetRegisterId"; $scope.searchAssetRegisterUpdate = "";
    $scope.searchByAssetRegisterUpdateList = [{ value: 'AssetRegisterId', name: "AssetRegisterId" }, { value: 'FixedAssetItemId', name: "FixedAssetItemId" }, { value: 'FixedAssetItem', name: "FixedAssetItem" }, { value: 'AssetSlNo', name: "AssetSlNo" }];
    $scope.searchCapitalizationMasterId = "";
    $scope.onClickAssetRegisterpopUpByCapitalizationMasterId = function (args) {
        $scope.searchCapitalizationMasterId = args.CapitalizationMasterId;
        $http({
            method: 'POST',
            url: 'fixedassets/FixedAssetRegister/GetAssetRegisterUpdateList',
            data: { column: $scope.searchByAssetRegisterUpdate, value: $scope.searchAssetRegisterUpdate, capitalizationMasterId: $scope.searchCapitalizationMasterId},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.AssetRegisterUpdateAvailableList = response.data;
        });

        angular.element(document.querySelector('#AssetRegisterUpdatePopUp')).modal('show');

    };

    $scope.showAssetRegisterUpdatePopUp = function () {
        $http({
            method: 'POST',
            url: 'fixedassets/FixedAssetRegister/GetAssetRegisterUpdateList',
            data: { column: $scope.searchByAssetRegisterUpdate, value: $scope.searchAssetRegisterUpdate, capitalizationMasterId: $scope.searchCapitalizationMasterId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.AssetRegisterUpdateAvailableList = response.data;
        });

        angular.element(document.querySelector('#AssetRegisterUpdatePopUp')).modal('show');

    };

    $scope.hideAssetRegisterUpdatePopUp = function () {
        angular.element(document.querySelector("#AssetRegisterUpdatePopUp")).modal("hide");
    };
    $scope.TotalAssetAmount = 0;
    $scope.AddAssetRegisterUpdate = function () {
        if (baseService.arrayLength($scope.AssetRegisterUpdateAvailableList) > 0) {
            $scope.checkedAssetRegisterUpdateList = [];
            angular.forEach($scope.AssetRegisterUpdateAvailableList, function (a) {
                $scope.TotalAssetAmount = a.TotalAmount;
                $scope.checkedAssetRegisterUpdateList.push({
                    AssetRegisterId: a.AssetRegisterId
                    , FixedAssetItemId: a.FixedAssetItemId
                    , FixedAssetItem: a.FixedAssetItem
                    , AssetSlNo: a.AssetSlNo
                    , RFId: a.RFId
                    , BarCode: a.BarCode
                    , Status: a.Status
                    , AssetCondition: a.AssetCondition
                    , UserReference: a.UserReference
                    , OldReference: a.OldReference
                    , UserGroup: a.UserGroup
                    , Remarks: a.Remarks
                    , Amount: a.Amount
                    , AssetRegisterChildId: a.AssetRegisterChildId
                    , Active: true
                });
            });
        }

    };
    $scope.validationUpdateAssetRegister = function () {
        if ($scope.checkedAssetRegisterUpdateList.length === 0) {
            ShowResult("Please select Asset Register!", "failure");
            return true;
        }
        if (parseFloat($filter("sumByKey")($filter("filter")($scope.AssetRegisterUpdateAvailableList), "Amount")) !== parseFloat($scope.TotalAssetAmount)) {
            ShowResult("Asset Register Amount must be equal Total Amount " + $scope.TotalAssetAmount, "failure");
            return true;
        }
    };
    
    $scope.UpdateAssetRegister = function () {
        $scope.AddAssetRegisterUpdate();
        $scope.validationUpdateAssetRegister();
        if (!$scope.validationUpdateAssetRegister()) {
            $scope.SaveUrl = "fixedassets/FixedAssetRegister/UpdateAssetRegister"
            $http({
                method: "POST",
                url: $scope.SaveUrl,
                data: {
                    "assetRegisterList": $scope.checkedAssetRegisterUpdateList
                },
                dataType: "JSON"
                , contentType: "application/json charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    //$scope.getData();

                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
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
    $scope.deleteUrl = $scope.path + "/DeleteCapitalizationMasterPost";
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

    // #region  Addition Posting
    $scope.searchByAddition = "VoucherNo"; $scope.searchAddition = "";
    $scope.searchByListAddition = [{ value: 'VoucherNo', name: "Voucher No" }, { value: 'PostingDate', name: "Posting Date" }, { value: 'FixedAssetMasterId', name: "Asset Master Id" }, { value: 'FixedAssetItemId', name: "Asset Item Id" }, { value: 'FixedAssetMaster', name: "Asset Master" }, { value: 'FixedAssetItem', name: "Asset Item" }, { value: 'FixedAssetCategory', name: "Asset Category" }, { value: 'FixedAssetSubCategory', name: "Asset Sub Category" }];

    $scope.voucherListAddition = [];
    $scope.getDataAddition = function () {
        $http({
            method: 'Post'
            , url: 'FixedAssets/FixedAssetRegister/GetCapitalizeAssetRegisterPostedList'
            , data: { column: $scope.searchByAddition, value: $scope.searchAddition, type: "Addition" }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.voucherListAddition = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.getDataAddition();
    $scope.voucherAddition = {
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
    $scope.capitalizationMasterAddition = {
        Id: null,
        FixedAssetItemId: null,
        Qty: null,
        TotalAmount: null
    };

    $scope.masterListAddition = [];
    $scope.getMasterDataAddition = function () {
        $scope.masterListAddition = [];
        $http.get("fixedassets/fixedassetregister/GetApprovedCapitalizeData?type=" + "Addition")
            .then(
                function successCallback(response) {
                    $scope.masterListAddition = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#CapitalpopUpAddition')).modal('show');
    };

    $scope.closePopUpAddition = function () {
        angular.element(document.querySelector('#CapitalpopUpAddition')).modal('hide');
    }

    $scope.selectedmaterialMasterListAddition = [];
    $scope.GetCapitalizationMasterDetailAddition = function () {
        $scope.selectedmaterialMasterListAddition = [];
        $http.get("fixedassets/fixedassetregister/GetCapitalizationMasterDetail?masterId=" + $scope.voucherAddition.CapitalizationMasterId)
            .then(
                function successCallback(response) {
                    $scope.selectedmaterialMasterListAddition = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.SelectMasterAddition = function (x) {
        var data = x.data;
        $scope.voucherAddition.CapitalizationMasterId = data.Id;
        $scope.voucherAddition.Amount = data.TotalAmount;
        $scope.voucherAddition.FixedAssetItem = data.FixedAssetItem;
        $scope.voucherAddition.Qty = data.Qty;
        $scope.voucherAddition.CapitalizationDate = $filter("dateFiltering")(data.CapitalizationDate);

        $scope.capitalizationMasterAddition.Id = data.Id;
        $scope.capitalizationMasterAddition.FixedAssetItemId = data.FixedAssetItemId;
        $scope.capitalizationMasterAddition.Qty = data.Qty;
        $scope.capitalizationMasterAddition.TotalAmount = data.TotalAmount;

        $scope.GetCapitalizationMasterDetailAddition();
        $scope.getCapitalizationJVAddition(data.Id);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

        angular.element(document.querySelector('#CapitalpopUpAddition')).modal('hide');
    };

    $scope.capitalizationJVListAddition = [];
    $scope.getCapitalizationJVAddition = function (Id) {
        $scope.capitalizationJVListAddition = [];
        $scope.jvurl = 'FixedAssets/FixedAssetRegister/GetCapitalizationSingleJVList?capitalizationMasterId=' + Id
        $http({
            method: 'Post'
            , url: $scope.jvurl
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.capitalizationJVListAddition = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.invalidDocDateAddition = false;
    $scope.checkDocDateAddition = function () {
        var msg = "";
        if (new Date($scope.voucherAddition.DocDate) > new Date()) {
            $scope.invalidDocDateAddition = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else if (new Date($scope.voucherAddition.PostingDate) < new Date($scope.voucherAddition.DocDate)) {
            msg = "Doc date must be below or equal to Posting Date!";
            $scope.invalidDocDateAddition = true;
        }
        else if (baseService.isUndefinedOrNull($scope.voucherAddition.DocDate)) {
            msg = "Doc Date is required.";
            $scope.invalidDocDateAddition = true;
        }
        else $scope.invalidDocDateAddition = false;
        return manualValidation("div_DocDateAddition", $scope.invalidDocDateAddition, msg);
    };

    $scope.invalidPostingDateAddition = false;
    $scope.checkPostingDateAddition = function () {
        var msg = "";
        if (new Date($scope.voucherAddition.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDateAddition = true;
        }
        else {
            $scope.invalidPostingDateAddition = false;
        }
        return manualValidation("div_PostingDateAddition", $scope.invalidPostingDateAddition, msg);
    };

    $scope.ClearAddition = function () {
        $scope.Action = "Save";
        $scope.voucherAddition.Active = true;
        $scope.voucherAddition.Amount = 0;
        $scope.voucherAddition.DocRefNo = null;
        $scope.voucherAddition.Narration = null;
        $scope.voucherAddition.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.voucherAddition.CapitalizationMasterId = null;
        $scope.voucherAddition.Amount = null;
        $scope.voucherAddition.FixedAssetItem = null;
        $scope.voucherAddition.Qty = null;
        $scope.voucherAddition.CapitalizationDate = null;
        $scope.selectedmaterialMasterListAddition = [];
        $scope.capitalizationJVListAddition = [];
        $scope.checkedAssetRegisterList = [];

        $scope.capitalizationMasterAddition.Id = null;
        $scope.capitalizationMasterAddition.FixedAssetItemId = null;
        $scope.capitalizationMasterAddition.Qty = null;
        $scope.capitalizationMasterAddition.TotalAmount = null;

    };

    $scope.validation = function () {
        if ($scope.checkedAssetRegisterList.length === 0) {
            ShowResult("Please select Asset Register!", "failure");
            return true;
        }
        if ($scope.checkedAssetRegisterList.length !== parseFloat($scope.voucherAddition.Qty)) {
            ShowResult("Please select " + $scope.voucherAddition.Qty +" Asset Register!", "failure");
            return true;
        }
        if (parseFloat($filter("sumByKey")($filter("filter")($scope.checkedAssetRegisterList), "Amount")) !== parseFloat($scope.voucherAddition.Amount)) {
            ShowResult("Distributed Amount must be equal Total Amount.!", "failure");
            return true;
        }
    };
    $scope.saveBtnDisable = false;
    $scope.PostAddition = function () {
        $scope.validation();
        if ($scope.formAddition.$valid && !$scope.validation()) {
            $scope.SaveUrl = "fixedassets/FixedAssetRegister/CreatetCapitalizeAssetRegisterPostAddition"
            if ($scope.Action === "Save") {
                $scope.saveBtnDisable = true;
                $http({
                    method: "POST",
                    url: $scope.SaveUrl,
                    data: {
                        "voucherVM": $scope.voucherAddition,
                        "voucherDetailVMList": $scope.capitalizationJVListAddition,
                        "assetRegisterList": $scope.checkedAssetRegisterList,
                        "capitalizationMasterdata": $scope.capitalizationMasterAddition
                    },
                    dataType: "JSON"
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.saveBtnDisable = false;
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getDataAddition();
                        $scope.ClearAddition();
                        $scope.saveBtnDisable = false;
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                    $scope.saveBtnDisable = false;
                });
                return true;
            }
            return true;
        }
    };

    $scope.checkedAssetRegisterList = [];
    $scope.AssetRegisterAvailableList = [];
    $scope.searchByAssetRegister = "AssetRegisterId"; $scope.searchAssetRegister = "";
    $scope.searchByAssetRegisterList = [{ value: 'AssetRegisterId', name: "AssetRegisterId" }, { value: 'FixedAssetItemId', name: "FixedAssetItemId" }, { value: 'FixedAssetItem', name: "FixedAssetItem" }, { value: 'AssetSlNo', name: "AssetSlNo" }];
    $scope.showAssetRegisterPopUp = function () {
        if ($scope.capitalizationJVListAddition.length === 0) {
            ShowResult("Please select Master Id first!", "failure");
            return true;
        }
        $http({
            method: 'POST',
            url: 'fixedassets/FixedAssetRegister/GetAssetRegisterList',
            data: { column: $scope.searchByAssetRegister, value: $scope.searchAssetRegister },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.AssetRegisterAvailableList = response.data;

            if (baseService.arrayLength($scope.checkedAssetRegisterList) > 0) {
                for (var i = 0; i < baseService.arrayLength($scope.checkedAssetRegisterList); i++) {
                    for (var j = 0; j < baseService.arrayLength($scope.AssetRegisterAvailableList); j++) {
                        if ($scope.checkedAssetRegisterList[i].AssetRegisterId == $scope.AssetRegisterAvailableList[j].AssetRegisterId) {
                            $scope.AssetRegisterAvailableList[j].Active = true;
                        }
                    }
                }
            }
        });

        angular.element(document.querySelector('#AssetRegisterPopUp')).modal('show');

    };
    
    $scope.hideAssetRegisterPopUp = function () {
        angular.element(document.querySelector("#AssetRegisterPopUp")).modal("hide");
    };
    $scope.calDistributedAmount = function myfunction() {
        $scope.TotalDistributedInvoiceAmount = 0;

        for (var i = 0; i < $scope.checkedAssetRegisterList.length; i++) {
            $scope.checkedAssetRegisterList[i].Amount = 0;
        }

        for (var i = 0; i < $scope.checkedAssetRegisterList.length; i++) {
            if ($scope.checkedAssetRegisterList.length - 1 == i) {

                $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedAssetRegisterList), "Amount"));
                $scope.checkedAssetRegisterList[i].Amount = (parseFloat($scope.voucherAddition.Amount) - $scope.TotalDistributedInvoiceAmount).toFixed(2);
            }
            else {
                $scope.checkedAssetRegisterList[i].Amount = parseFloat(parseFloat($scope.voucherAddition.Amount) / parseFloat($scope.voucherAddition.Qty)).toFixed(2);
            }
        }
    };

    $scope.AddAssetRegister = function () {
        if (baseService.arrayLength($scope.AssetRegisterAvailableList) > 0) {
            $scope.checkedAssetRegisterList = [];
            angular.forEach($scope.AssetRegisterAvailableList, function (a) {
                if (a.Active) {
                    $scope.checkedAssetRegisterList.push({
                          CapitalizationMasterId: a.CapitalizationMasterId
                        , CapitalizationChildId: a.CapitalizationChildId
                        , AssetAmount: a.AssetAmount
                        , FixedAssetItem: a.FixedAssetItem
                        , FixedAssetItemId: a.FixedAssetItemId
                        , AssetRegisterId: a.AssetRegisterId
                        , AssetSlNo: a.AssetSlNo
                        , Status: a.Status
                        , AssetCondition: a.AssetCondition
                        , UserReference: a.UserReference
                        , OldReference: a.OldReference
                        , UserGroup: a.UserGroup
                        , Remarks: a.Remarks
                        , Amount: 0
                        , Active: true
                    });
                }
            });
        }

        $scope.hideAssetRegisterPopUp();
        $scope.calDistributedAmount();
    };

    $scope.DeleteConfirmation = function (AssetRegisterId) {
        $scope.AssetRegisterId = AssetRegisterId;
        $scope.message_conf = "Are you sure to Delete?";
        angular.element(document.querySelector("#DeleteConfirmationPopUp")).modal("show");
    };

    $scope.RemoveAssetRegisterId = function () {
        for (var i = 0; i < baseService.arrayLength($scope.checkedAssetRegisterList); i++) {
            if ($scope.checkedAssetRegisterList[i].AssetRegisterId == $scope.AssetRegisterId)
                $scope.checkedAssetRegisterList.splice(i, 1);
        }
        $scope.calDistributedAmount();
    };

    $scope.checkDistributedAmount = function myfunction(index, item) {
        $scope.TotalDistributedAmounts = 0;
        $scope.TotalDistributedAmounts = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedAssetRegisterList), "Amount"));

        if (parseFloat($scope.TotalDistributedAmounts) > parseFloat($scope.voucherAddition.Amount)) {
            $scope.checkedAssetRegisterList[index].Amount = 0;
            ShowResult("Distributed Amount must be equal Total Amount.!", "failure");
        }
    };
    // #endregion Addition Posting
}