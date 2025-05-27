"use strict";
faRegisterController.$inject = ["addressService", "commonMessage", "$scope", "$rootScope", "baseService", "cboService", "$http", "$filter", "$controller", "$window"];
function faRegisterController(addressService, commonMessage, $scope, $rootScope, baseService, cboService, $http, $filter, $controller, $window) {
    $rootScope.title = "Fixed Asset Register";
    $scope.Action = "Save";
    $scope.message_confirmation = "";
    $scope.path = "fixedassets/fixedassetregister/";

    $scope.register = {
        Id: null, FixedAssetItemId: null, CapitalizationDate: null, Qty: 0, GRNAmount: 0, IssueAmount: 0, ExpensesAmount: 0, Other: null, TotalAmount: 0, ApprovedById: null, IsApproved: false, Status: null, Type: 'New', VoucherRowId: null, Remark: null, InstallationYear: null, Lifetime: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
    };

    $scope.yearList = [];
    $scope.getYearOfHaving = function () {
        $scope.yearList = [];
        var endYear = new Date();
        var ey = parseInt(endYear.getFullYear()) - 5;
        for (var i = ey; i <= 2099; i++) {
            var ob = {
                Value: i,
                Text: i
            };
            $scope.yearList.push(ob);
        }

        var d = new Date();
        var n = d.getFullYear();
        for (var i = 0; i < $scope.yearList.length; i++) {
            if ($scope.yearList[i].Text === n) {
                $scope.register.InstallationYear = $scope.yearList[i].Text;
                break;
            }
        }

    };
    $scope.getYearOfHaving();

    $scope.typeList = [
        { Value: "New", Text: "New" },
        { Value: "Addition", Text: "Addition" }
    ];

    $scope.searchByCapitalizeData = "FixedAssetItem"; $scope.searchCapitalizeData = "";
    $scope.searchByListCapitalizeData = [{ value: 'Id', name: "Master Id" }, { value: 'CapitalizationDate', name: "Capitalization Date" }, { value: 'AddedDate', name: "Added Date" }, { value: 'FixedAssetMasterId', name: "Asset Master Id" }, { value: 'FixedAssetItemId', name: "Asset Item Id" }, { value: 'FixedAssetMaster', name: "Asset Master" }, { value: 'FixedAssetItem', name: "Asset Item" }, { value: 'Type', name: "Type" }, { value: 'CMStatus', name: "Status" }];

    $scope.masterList = [];
    $scope.getData = function () {
        $scope.masterList = [];
        $http({
            method: 'Post'
            , url: 'FixedAssets/FixedAssetRegister/GetCapitalizeDataList'
            , data: { column: $scope.searchByCapitalizeData, value: $scope.searchCapitalizeData }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.masterList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.getData();

    $scope.selectedmaterialMasterList = [];
    $scope.GetCapitalizationMasterDetail = function () {
        $scope.selectedmaterialMasterList = [];
        $scope.register.TotalAmount = 0;
        $scope.register.GRNAmount = 0;
        $scope.register.IssueAmount = 0;
        $scope.register.ExpensesAmount = 0;
        $http.get("fixedassets/fixedassetregister/GetCapitalizationMasterDetail?masterId=" + $scope.register.Id)
            .then(
                function successCallback(response) {
                    $scope.selectedmaterialMasterList = response.data;
                    
                    $scope.register.GRNAmount = Math.round($filter("sumByKey")($filter("filter")($filter("filter")($scope.selectedmaterialMasterList, { Source: 'AUC' })), "Amount") * 100 + Number.EPSILON) / 100;
                    $scope.register.IssueAmount = Math.round($filter("sumByKey")($filter("filter")($filter("filter")($scope.selectedmaterialMasterList, { Source: 'CI' })), "Amount") * 100 + Number.EPSILON) / 100;
                    $scope.register.ExpensesAmount = Math.round($filter("sumByKey")($filter("filter")($filter("filter")($scope.selectedmaterialMasterList, { Source: 'Expense' })), "Amount") * 100 + Number.EPSILON) / 100;
                    $scope.register.TotalAmount = Math.round($filter("sumByKey")($filter("filter")($scope.selectedmaterialMasterList), "Amount") * 100 + Number.EPSILON) / 100;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetAssetRegisterChildAddition = function () {
        $scope.checkedAssetRegisterList = [];
        $http.get("fixedassets/fixedassetregister/GetAssetRegisterChildAdditionList?masterId=" + $scope.register.Id)
            .then(
                function successCallback(response) {
                    $scope.checkedAssetRegisterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.SelectMaster = function (obj) {
        $scope.register = obj.data;
        $scope.register.CapitalizationDate = $filter('dateFiltering')(new Date($scope.register.CapitalizationDate), 'dd-MM-yyyy');
        $scope.register.InstallationYear = parseInt($scope.register.InstallationYear);
        $scope.GetCapitalizationMasterDetail();
        $scope.GetAssetRegisterChildAddition();

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.Action = 'Update';
    };

    $scope.ItemName = null;
    $scope.showSearchData = function (faType) {
        $scope.FaType = faType;
        if (faType == 'AUC') {
            $scope.ItemName = 'AUC';
        } else if (faType == 'CI') {
            $scope.ItemName = 'Capitalize Inventory';
        }
        else {
            $scope.ItemName = 'Expense';
        }
        $scope.getSearchData(faType);

        if (faType == 'AUC') {
            var gridObj = $("#GridAUC").data("ejGrid");        
            angular.element(document.querySelector("#assetmodal")).modal("show");
        } else if (faType == 'CI') {
            var gridObj = $("#GridCI").data("ejGrid");
            angular.element(document.querySelector("#assetmodalCI")).modal("show");
        }
        else {
            var gridObj = $("#GridEX").data("ejGrid");
            angular.element(document.querySelector("#assetmodalEx")).modal("show");
        }
        gridObj.clearFiltering();  // clears all the filtering
    };
    $scope.searchBy = "FiscalYearName"; $scope.search = "";
    $scope.searchByList = [
        {
            'name': 'Fiscal Year',
            'value': 'FiscalYearName'
        },
        {
            'name': 'VoucherNo',
            'value': 'VoucherNo'
        },
        {
            'name': 'Cost Center',
            'value': 'CostCenter'
        },
        {
            'name': 'GL',
            'value': 'AssetGLName'
        },
        {
            'name': 'Budget',
            'value': 'AssetBudgetName'
        },
        {
            'name': 'Activity',
            'value': 'ActivityName'
        }
    ];

    $scope.materialMasterList = [];
    $scope.getSearchData = function (faType) {
        $scope.materialMasterList = [];
        var AUCCIExpenseurl = "";
        if (faType == 'AUC') {
            AUCCIExpenseurl = 'FixedAssets/FixedAssetRegister/GetAUCCIExpenseData?column=' + $scope.searchBy + '&value=' + $scope.search + '&faType=' + faType;
        } else if (faType == 'CI') {
            AUCCIExpenseurl = 'FixedAssets/FixedAssetRegister/GetAUCCIExpenseData?column=' + $scope.searchBy + '&value=' + $scope.search + '&faType=' + faType;
        }
        else {
            AUCCIExpenseurl = 'FixedAssets/FixedAssetRegister/GetAUCCIExpenseData?column=' + $scope.searchBy + '&value=' + $scope.search + '&faType=' + faType;
        }
        $http.get(AUCCIExpenseurl)
            .then(function (response) {
                $scope.materialMasterList = response.data;
            });
    };
    $scope.searchByMaterialMasterModalList = [
        {
            "name": "Voucher No",
            "value": "VoucherNo"
        }
        ,
        {
            "name": "Material Master",
            "value": "MaterialMasterName"
        }
        ,
        {
            "name": "Article",
            "value": "ArticleStandardName"
        }
        ,
        {
            "name": "Asset Master",
            "value": "AssetMasterName"
        },
        {
            "name": "Material Type",
            "value": "MaterialTypeName"
        },
        {
            "name": "Material Group",
            "value": "MaterialGroupMasterName"
        },
        {
            "name": "GRN No",
            "value": "GRNNo"
        },
        {
            "name": "Issue No",
            "value": "IssueNo"
        },
        {
            "name": "GL",
            "value": "AssetGLName"
        },
        {
            "name": "Budget",
            "value": "AssetBudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "Base UOM",
            "value": "BaseUOMName"
        }
        ,
        {
            "name": "Vendor",
            "value": "VendorName"
        }
    ];

    $scope.searchMaterialMasterParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "VoucherNo",
        searchBy: "VoucherNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };


    // #region checkbox all

    $scope.refreshTemplate = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllWise });
    };

    function CheckBoxSelectAllWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridAUC").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.materialMasterList.length; i++) {
                $scope.materialMasterList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridAUC").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.refreshTemplateCI = function (args) {
        $("#headchkCI").ejCheckBox({ "change": CheckBoxSelectAllCI });
    };

    function CheckBoxSelectAllCI(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridCI").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.materialMasterList.length; i++) {
                $scope.materialMasterList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridCI").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.refreshTemplateEx = function (args) {
        $("#headchkEx").ejCheckBox({ "change": CheckBoxSelectAllEx });
    };

    function CheckBoxSelectAllEx(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEx").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.materialMasterList.length; i++) {
                $scope.materialMasterList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEx").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.CloseMMPopUp = function () {
        MakeData();
        angular.element(document.querySelector("#assetmodal")).modal("hide");
        angular.element(document.querySelector("#assetmodalCI")).modal("hide");
        angular.element(document.querySelector("#assetmodalEx")).modal("hide");
    }

    $scope.selectedmaterialMasterList = [];
    function checkAUCExistTempList(list, VoucherDetailId, InventoryReceiveDetailId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].VoucherDetailId === VoucherDetailId && list[i].InventoryReceiveDetailId == InventoryReceiveDetailId) {
                return true;
            }
        }
        return false;
    }

    function checkCIExistTempList(list, VoucherDetailId, InventoryIssueHistoryId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].VoucherDetailId === VoucherDetailId && list[i].InventoryIssueHistoryId == InventoryIssueHistoryId) {
                return true;
            }
        }
        return false;
    }

    function checkExistTempList(list, VoucherDetailId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].VoucherDetailId === VoucherDetailId) {
                return true;
            }
        }
        return false;
    }

    function MakeData() {
        for (var i = 0; i < $scope.materialMasterList.length; i++) {
            if ($scope.materialMasterList[i].Flag == true) {
                if ($scope.FaType == 'AUC') {
                    if (checkAUCExistTempList($scope.selectedmaterialMasterList, $scope.materialMasterList[i].VoucherDetailId, $scope.materialMasterList[i].InventoryReceiveDetailId) === false) {
                        var ob = {};
                        ob.Id = null;
                        ob.InventoryReceiveDetailId = $scope.materialMasterList[i].InventoryReceiveDetailId;
                        ob.InventoryIssueHistoryId = $scope.materialMasterList[i].InventoryIssueHistoryId;
                        ob.VoucherDetailId = $scope.materialMasterList[i].VoucherDetailId;
                        ob.Amount = $scope.materialMasterList[i].Amount;
                        ob.CurrencyId = $scope.materialMasterList[i].CurrencyId;
                        ob.VoucherNo = $scope.materialMasterList[i].VoucherNo;
                        ob.MaterialMasterName = $scope.materialMasterList[i].MaterialMasterName;
                        ob.ArticleStandardName = $scope.materialMasterList[i].ArticleStandardName;
                        ob.Qty = $scope.materialMasterList[i].Qty;
                        ob.GRNNo = $scope.materialMasterList[i].GRNNo;
                        ob.Qty = $scope.materialMasterList[i].BaseQty;
                        ob.Source = 'AUC';
                        $scope.selectedmaterialMasterList.push(ob);
                        ob = {};
                    }
                } else if ($scope.FaType == 'CI') {
                    if (checkCIExistTempList($scope.selectedmaterialMasterList, $scope.materialMasterList[i].VoucherDetailId, $scope.materialMasterList[i].InventoryIssueHistoryId) === false) {
                        var ob = {};
                        ob.Id = null;
                        ob.InventoryReceiveDetailId = $scope.materialMasterList[i].InventoryReceiveDetailId;
                        ob.InventoryIssueHistoryId = $scope.materialMasterList[i].InventoryIssueHistoryId;
                        ob.VoucherDetailId = $scope.materialMasterList[i].VoucherDetailId;
                        ob.Amount = $scope.materialMasterList[i].Amount;
                        ob.CurrencyId = $scope.materialMasterList[i].CurrencyId;
                        ob.VoucherNo = $scope.materialMasterList[i].VoucherNo;
                        ob.MaterialMasterName = $scope.materialMasterList[i].MaterialMasterName;
                        ob.ArticleStandardName = $scope.materialMasterList[i].ArticleStandardName;
                        ob.Qty = $scope.materialMasterList[i].Qty;
                        ob.GRNNo = $scope.materialMasterList[i].GRNNo;
                        ob.Qty = $scope.materialMasterList[i].BaseQty;
                        ob.Source = 'CI';
                        $scope.selectedmaterialMasterList.push(ob);
                        ob = {};
                    }
                }
                else {

                    if (checkExistTempList($scope.selectedmaterialMasterList, $scope.materialMasterList[i].VoucherDetailId) === false) {
                        var ob = {};
                        ob.Id = null;
                        ob.InventoryReceiveDetailId = $scope.materialMasterList[i].InventoryReceiveDetailId;
                        ob.InventoryIssueHistoryId = $scope.materialMasterList[i].InventoryIssueHistoryId;
                        ob.VoucherDetailId = $scope.materialMasterList[i].VoucherDetailId;
                        ob.Amount = $scope.materialMasterList[i].Amount;
                        ob.CurrencyId = $scope.materialMasterList[i].CurrencyId;
                        ob.VoucherNo = $scope.materialMasterList[i].VoucherNo;
                        ob.MaterialMasterName = $scope.materialMasterList[i].MaterialMasterName;
                        ob.ArticleStandardName = $scope.materialMasterList[i].ArticleStandardName;
                        ob.Qty = $scope.materialMasterList[i].Qty;
                        ob.GRNNo = $scope.materialMasterList[i].GRNNo;
                        ob.Qty = $scope.materialMasterList[i].BaseQty;
                        ob.Source = 'Expense';
                        $scope.selectedmaterialMasterList.push(ob);
                        ob = {};
                    }
                }

                $scope.register.GRNAmount = Math.round($filter("sumByKey")($filter("filter")($filter("filter")($scope.selectedmaterialMasterList, { Source: 'AUC' })), "Amount") * 100 + Number.EPSILON) / 100;
                $scope.register.IssueAmount = Math.round($filter("sumByKey")($filter("filter")($filter("filter")($scope.selectedmaterialMasterList, { Source: 'CI' })), "Amount") * 100 + Number.EPSILON) / 100;
                $scope.register.ExpensesAmount = Math.round($filter("sumByKey")($filter("filter")($filter("filter")($scope.selectedmaterialMasterList, { Source: 'Expense' })), "Amount") * 100 + Number.EPSILON) / 100;
                $scope.register.TotalAmount = Math.round($filter("sumByKey")($filter("filter")($scope.selectedmaterialMasterList), "Amount") * 100 + Number.EPSILON) / 100;
            }
        }

    }

    // #endregion checkbox all


    $scope.VNo = null;
    $scope.message_VoucherRemoveconfirmation = null;
    $scope.VId = null;
    $scope.RemoveItemVoucher = function (obj) {
        $scope.VNo = obj.VoucherNo;
        $scope.VId = obj.Id;
        if (!baseService.isUndefinedOrNull(obj.VoucherNo))
            $scope.message_VoucherRemoveconfirmation = 'Are you sure want to delete permanently [ ' + obj.VoucherNo + ' ]';
        angular.element(document.querySelector('#confirmVoucherRemovePopUp')).modal('show');
    }

    $scope.RemoveVoucher = function () {
        if (baseService.isUndefinedOrNull($scope.VId)) {
            for (var i = 0; i < $scope.selectedmaterialMasterList.length; i++) {
                if ($scope.selectedmaterialMasterList[i].VoucherNo == $scope.VNo) {
                    $scope.selectedmaterialMasterList.splice(i, 1);
                }
            }
        }
        else {
            $scope.DeleteDetail();
        }
    };

    $scope.GetCapitalizationMasterAfterDelDetail = function () {
        $scope.selectedmaterialMasterList = [];
        $scope.register.TotalAmount = 0;
        $scope.register.GRNAmount = 0;
        $scope.register.IssueAmount = 0;
        $scope.register.ExpensesAmount = 0;
        $http.get("fixedassets/fixedassetregister/GetCapitalizationMasterDetail?masterId=" + $scope.register.Id)
            .then(
                function successCallback(response) {
                    $scope.selectedmaterialMasterList = response.data;
                    
                    $scope.register.GRNAmount = Math.round($filter("sumByKey")($filter("filter")($filter("filter")($scope.selectedmaterialMasterList, { Source: 'AUC' })), "Amount") * 100 + Number.EPSILON) / 100;
                    $scope.register.IssueAmount = Math.round($filter("sumByKey")($filter("filter")($filter("filter")($scope.selectedmaterialMasterList, { Source: 'CI' })), "Amount") * 100 + Number.EPSILON) / 100;
                    $scope.register.ExpensesAmount = Math.round($filter("sumByKey")($filter("filter")($filter("filter")($scope.selectedmaterialMasterList, { Source: 'Expense' })), "Amount") * 100 + Number.EPSILON) / 100;
                    $scope.register.TotalAmount = Math.round($filter("sumByKey")($filter("filter")($scope.selectedmaterialMasterList), "Amount") * 100 + Number.EPSILON) / 100;
                    $scope.SaveRegister();
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.DeleteDetail = function () {
        try {
            $http({
                method: "POST",
                url: $scope.path + "DeleteDetail",
                dataType: "JSON",
                data: { "id": $scope.VId }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.GetCapitalizationMasterAfterDelDetail();

                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        } catch (e) {
            ShowResult(e, "Error");
        }
    };


    $scope.FixedAssetMasterItemList = [];
    $scope.ShowFixedAssetMasterItem = function () {
        $scope.Url = 'FixedAssets/FixedAssetRegister/GetFixedAssetMasterItem';
        $http({
            method: 'Get',
            url: $scope.Url,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.FixedAssetMasterItemList = response.data;
        });
        angular.element(document.querySelector('#fixedAssetMasterItemPoUp')).modal('show');
    };

    $scope.SetFAMI = function (obj) {
        $scope.register.FixedAssetItem = obj.data.UserName;
        $scope.register.FixedAssetItemId = obj.data.Id;
        angular.element(document.querySelector('#fixedAssetMasterItemPoUp')).modal('hide');

    }

    $scope.ApprovedByList = [];
    $scope.GetApprovedCboList = function () {
        $http({
            method: 'GET',
            url: 'fixedassets/fixedassetregister/GetCapitalizeAssetRegisterApproveByCbo'
        }).then(function successCallback(response) {
            $scope.ApprovedByList = response.data;
        });
    }
    $scope.GetApprovedCboList();

    $scope.SaveRegister = function () {
        try {
            $scope.register.TotalAmount = 0;
            $scope.register.GRNAmount = 0;
            $scope.register.ExpensesAmount = 0;
            $scope.register.IssueAmount = 0;
            
            $scope.register.GRNAmount = Math.round($filter("sumByKey")($filter("filter")($filter("filter")($scope.selectedmaterialMasterList, { Source: 'AUC' })), "Amount") * 100 + Number.EPSILON) / 100;
            $scope.register.IssueAmount = Math.round($filter("sumByKey")($filter("filter")($filter("filter")($scope.selectedmaterialMasterList, { Source: 'CI' })), "Amount") * 100 + Number.EPSILON) / 100;
            $scope.register.ExpensesAmount = Math.round($filter("sumByKey")($filter("filter")($filter("filter")($scope.selectedmaterialMasterList, { Source: 'Expense' })), "Amount") * 100 + Number.EPSILON) / 100;
            $scope.register.TotalAmount = Math.round($filter("sumByKey")($filter("filter")($scope.selectedmaterialMasterList), "Amount") * 100 + Number.EPSILON) / 100;

            if ($scope.register.Type === "Addition" ) {
                if ($scope.checkedAssetRegisterList.length === 0) {
                    ShowResult("Please select Asset Register!", "failure");
                    return true;
                }
                if ($scope.checkedAssetRegisterList.length !== parseFloat($scope.register.Qty)) {
                    ShowResult("Please select " + $scope.register.Qty + " Asset Register!", "failure");
                    return true;
                }
                if (parseFloat($filter("sumByKey")($filter("filter")($scope.checkedAssetRegisterList), "Amount")) !== parseFloat($scope.register.TotalAmount)) {
                    ShowResult("Distributed Amount must be equal Total Amount.!", "failure");
                    return true;
                }
            }

            $scope.$broadcast("show-errors-check-validity");
            if ($scope.form0.$valid) {
                $scope.saveBtnDisable = true;
                $http({
                    method: "POST",
                    url: $scope.path + "CreateCapitalize",
                    dataType: "JSON",
                    data: {
                        "data": $scope.register,
                        "items": $scope.selectedmaterialMasterList,
                        "assetRegisterList": $scope.checkedAssetRegisterList,
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                        $scope.saveBtnDisable = false;
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.register.Id = response.data.Id;
                        $scope.getData();
                        //$scope.GetCapitalizationMasterDetail();
                        $scope.saveBtnDisable = false;
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                    $scope.saveBtnDisable = false;
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Clear = function () {
        $scope.register = {
            Id: null, FixedAssetItemId: null, CapitalizationDate: null, Qty: 0, GRNAmount: 0, IssueAmount: 0, ExpensesAmount: 0, Other: null, TotalAmount: 0, ApprovedById: null, IsApproved: false, Status: null, Type: null, VoucherRowId: null, Remark: null, InstallationYear: null, Lifetime: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
        };

        $scope.selectedmaterialMasterList = [];
        $scope.checkedAssetRegisterList = [];
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    $scope.DownloadReport = function () {
        var dataList = [];
        var g = null;

        if ($scope.FaType == 'AUC') {
            g = $("#GridAUC").data("ejGrid");
        } else if ($scope.FaType == 'CI') {
            g = $("#GridCI").data("ejGrid");
        }
        else {
            g = $("#GridEx").data("ejGrid");
        }

        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.materialMasterList;
        }
        for (var i = 0; i < dataList.length; i++) {
            if ($scope.FaType == 'AUC') {
                dataList[i].Qty = dataList[i].BaseQty;
                dataList[i].IssueNo = null;
                dataList[i].CapitalizeDate = null;
                dataList[i].Entity = null;
                dataList[i].CostCenter = null;
            }
            if ($scope.FaType == 'Expense') {
                dataList[i].InventoryReceiveDetailId = null;
                dataList[i].MaterialMasterName = null;
                dataList[i].ArticleStandardName = null;
                dataList[i].BaseUoM = null;
                dataList[i].IssueNo = null;
                dataList[i].CapitalizeDate = null;
            }
            if ($scope.FaType == 'CI') {
                dataList[i].BaseUoM = dataList[i].TransactionUoM;
            }

        }

        $scope.fileName = 'AssetCapitalizationReport.xlsx';
        $http({
            method: "POST",
            url: 'FixedAssets/FixedAssetRegister/GetAUCCIExpenseReport',
            data: {
                'data': dataList,
                'reportFileName': $scope.fileName,
            },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                //$window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };
    $scope.checkedAssetRegisterUpdateList = [];
    $scope.AssetRegisterUpdateAvailableList = [];
    $scope.searchByAssetRegisterUpdate = "AssetRegisterId"; $scope.searchAssetRegisterUpdate = "";
    $scope.searchByAssetRegisterUpdateList = [{ value: 'AssetRegisterId', name: "AssetRegisterId" }, { value: 'FixedAssetItemId', name: "FixedAssetItemId" }, { value: 'FixedAssetItem', name: "FixedAssetItem" }, { value: 'AssetSlNo', name: "AssetSlNo" }];
    $scope.searchCapitalizationMasterId = "";
    $scope.onClickAssetRegisterpopUpByCapitalizationMasterId = function (args) {
        $scope.searchCapitalizationMasterId = args.Id;
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


                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        }
    };

    var AssetRegisterIdItem = "";
    var AssetRegisterChildIdItem = "";
    $scope.FixedAssetindex = null;
    $scope.ShowFixedAssetMasterItemAssetRegisterUpdate = function (index, AssetRegisterId, AssetRegisterChildId) {
        $scope.FixedAssetMasterItemList = [];
        $scope.FixedAssetindex = index.data;
        AssetRegisterIdItem = AssetRegisterId;
        AssetRegisterChildIdItem = AssetRegisterChildId;
        $scope.Url = 'FixedAssets/FixedAssetRegister/GetFixedAssetMasterItem';
        $http({
            method: 'Get',
            url: $scope.Url,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.FixedAssetMasterItemList = response.data;
        });
        angular.element(document.querySelector('#fixedAssetMasterItemAssetRegisterUpdatePoUp')).modal('show');
    };

    $scope.SetFAMIAssetRegisterUpdate = function (obj) {
        $scope.FixedAssetindex.FixedAssetItemId = obj.data.Id;
        $scope.FixedAssetindex.FixedAssetItem = obj.data.UserName;
        $scope.FixedAssetindex.FixedAssetMaster = obj.data.FixedAssetMaster;
        $scope.UpdateAssetRegisterItem(obj.data.Id);
        angular.element(document.querySelector('#fixedAssetMasterItemAssetRegisterUpdatePoUp')).modal('hide');

    }
    $scope.hideAssetRegisterItemUpdatePopUp = function () {
        angular.element(document.querySelector("#fixedAssetMasterItemAssetRegisterUpdatePoUp")).modal("hide");
    };

    $scope.UpdateAssetRegisterItem = function (Id) {
        $scope.SaveUrl = "fixedassets/FixedAssetRegister/UpdateAssetRegisterItem"
        $http({
            method: "POST",
            url: $scope.SaveUrl,
            data: {
                "assetRegisterId": AssetRegisterIdItem,
                "assetRegisterChildId": AssetRegisterChildIdItem,
                "fixedAssetItemId": Id
            },
            dataType: "JSON"
            , contentType: "application/json charset=utf-8"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                //ShowResult(response.data.Message, "success");
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });

    };
    $scope.checkedAssetRegisterList = [];
    $scope.AssetRegisterAvailableList = [];
    $scope.searchByAssetRegister = "AssetRegisterId"; $scope.searchAssetRegister = "";
    $scope.searchByAssetRegisterList = [{ value: 'AssetRegisterId', name: "AssetRegisterId" }, { value: 'FixedAssetItemId', name: "FixedAssetItemId" }, { value: 'FixedAssetItem', name: "FixedAssetItem" }, { value: 'AssetSlNo', name: "AssetSlNo" }];
    $scope.showAssetRegisterPopUp = function () {
        if ($scope.register.TotalAmount === 0) {
            ShowResult("Please select voucher first!", "failure");
            return true;
        }
        if ($scope.register.Qty === 0) {
            ShowResult("Please input Qty first!", "failure");
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
                $scope.checkedAssetRegisterList[i].Amount = (parseFloat($scope.register.TotalAmount) - $scope.TotalDistributedInvoiceAmount).toFixed(2);
            }
            else {
                $scope.checkedAssetRegisterList[i].Amount = parseFloat(parseFloat($scope.register.TotalAmount) / parseFloat($scope.register.Qty)).toFixed(2);
            }
        }
    };

    $scope.AddAssetRegister = function () {
        if (baseService.arrayLength($scope.AssetRegisterAvailableList) > 0) {
            angular.forEach($scope.AssetRegisterAvailableList, function (a) {
                if (checkAssetRegisterExist($scope.checkedAssetRegisterList, a.AssetRegisterId) === false) {
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
                }
            });
        }

        $scope.hideAssetRegisterPopUp();
        $scope.calDistributedAmount();
    };
    function checkAssetRegisterExist(list, AssetRegisterId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].AssetRegisterId === AssetRegisterId) {
                return true;
            }
        }
        return false;
    }
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

        if (parseFloat($scope.TotalDistributedAmounts) > parseFloat($scope.register.TotalAmount)) {
            $scope.checkedAssetRegisterList[index].Amount = 0;
            ShowResult("Distributed Amount must be equal Total Amount.!", "failure");
        }
    };
    $scope.capitalizationMasterId = null;
    $scope.confirmDelete = function (data) {
        if (data.data.VoucherNo != "") {
            ShowResult("Posted data cann't delete!" + " VoucherNo: " + data.data.VoucherNo + " delete first!");
            return false;
        }
        $scope.capitalizationMasterId = data.data.Id;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };
    $scope.deleteUrl = $scope.path + "/DeleteCapitalizationMaster";
    $scope.delete = function (capitalizationMasterId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "capitalizationMasterId": capitalizationMasterId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.capitalizationMasterId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };
}