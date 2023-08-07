"use strict";
faRegisterController.$inject = ["addressService", "commonMessage", "$scope", "$rootScope", "baseService", "cboService", "$http", "$filter", "$controller", "$window"];
function faRegisterController(addressService, commonMessage, $scope, $rootScope, baseService, cboService, $http, $filter, $controller, $window) {
    $rootScope.title = "Fixed Asset Register";
    $scope.Action = "Save";
    $scope.message_confirmation = "";
    $scope.path = "fixedassets/fixedassetregister/";

    $scope.register = {
        Id: null, FixedAssetItemId: null, CapitalizationDate: null, Qty: 0, GRNAmount: 0, IssueAmount: 0, ExpensesAmount: 0, Other: null, TotalAmount: 0, ApprovedById: null, IsApproved: false, Status: null, Type: null, VoucherRowId: null, Remark: null, InstallationYear: null, Lifetime: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
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


    $scope.masterList = [];
    $scope.getSavedData = function () {
        $scope.purchaseLCList = [];
        $http.get("fixedassets/fixedassetregister/GetCapitalizeData")
            .then(
                function successCallback(response) {
                    $scope.masterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getSavedData();

    $scope.selectedmaterialMasterList = [];
    $scope.GetCapitalizationMasterDetail = function () {
        $scope.purchaseLCList = [];
        $http.get("fixedassets/fixedassetregister/GetCapitalizationMasterDetail?masterId=" + $scope.register.Id)
            .then(
                function successCallback(response) {
                    $scope.selectedmaterialMasterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.SelectMaster = function (obj) {
        $scope.register = obj.data;
        $scope.register.InstallationYear = parseInt($scope.register.InstallationYear);
        $scope.GetCapitalizationMasterDetail();

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
            angular.element(document.querySelector("#assetmodal")).modal("show");
        } else if (faType == 'CI') {
            angular.element(document.querySelector("#assetmodalCI")).modal("show");
        }
        else {
            angular.element(document.querySelector("#assetmodalEx")).modal("show");
        }

    };
    $scope.searchBy = "VoucherNo"; $scope.search = "";
    $scope.searchByList = [
        {
            'name': 'VoucherNo',
            'value': 'VoucherNo'
        },
        {
            'name': 'Material',
            'value': 'MaterialMasterName'
        },
        {
            'name': 'Article',
            'value': 'ArticleStandardName'
        }
    ];

    $scope.materialMasterList = [];
    $scope.getSearchData = function (faType) {
        $http.get('FixedAssets/FixedAssetRegister/GetAUCCIExpenseData?column=' + $scope.searchBy + '&value=' + $scope.search + '&faType=' + faType)
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

    $scope.selectedmaterialMasterList = [];
    function checkExistTempList(list, ArticleId, VoucherId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ArticleId === ArticleId && list[i].VoucherId === VoucherId) {
                return true;
            }
        }
        return false;
    }

    // #region checkbox all

    $scope.refreshTemplate = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllWise });
    };

    function CheckBoxSelectAllWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.materialMasterList.length; i++) {
                $scope.materialMasterList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#Grid").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.CloseMMPopUp = function () {
        MakeData();
        angular.element(document.querySelector("#assetmodal")).modal("hide");
    }

    function MakeData() {

        for (var i = 0; i < $scope.materialMasterList.length; i++) {
            if ($scope.materialMasterList[i].Flag == true) {
                if (checkExistTempList($scope.selectedmaterialMasterList, $scope.materialMasterList[i].ArticleId, $scope.materialMasterList[i].VoucherId) === false) {

                    if ($scope.FaType == 'AUC') {
                        $scope.register.GRNAmount += $scope.materialMasterList[i].Amount;
                    } else if ($scope.FaType == 'CI') {
                        $scope.register.IssueAmount += $scope.materialMasterList[i].Amount;
                    }
                    else {
                        $scope.register.ExpensesAmount += $scope.materialMasterList[i].Amount;
                    }

                    var ob = {};
                    ob.Id = null;
                    ob.InventoryReceiveDetailId = $scope.materialMasterList[i].InventoryReceiveDetailId;
                    ob.InventoryIssueHistoryId = $scope.materialMasterList[i].InventoryIssueHistoryId;
                    ob.VoucherDetailId = $scope.materialMasterList[i].VoucherDetailNo;
                    ob.Amount = $scope.materialMasterList[i].Amount;
                    ob.CurrencyId = $scope.materialMasterList[i].CurrencyId;
                    ob.VoucherNo = $scope.materialMasterList[i].VoucherNo;
                    ob.MaterialMasterName = $scope.materialMasterList[i].MaterialMasterName;
                    ob.ArticleStandardName = $scope.materialMasterList[i].ArticleStandardName;
                    ob.Qty = $scope.materialMasterList[i].Qty;
                    ob.GRNNo = $scope.materialMasterList[i].GRNNo;

                    if ($scope.FaType == 'AUC') {
                        ob.Source = 'AUC';
                        ob.Qty = $scope.materialMasterList[i].BaseQty;
                    } else if ($scope.FaType == 'CI') {
                        ob.Source = 'CI';
                    }
                    else {
                        ob.Source = 'Expense';
                    }

                    $scope.selectedmaterialMasterList.push(ob);
                    ob = {};
                }

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
                    for (var i = 0; i < $scope.selectedmaterialMasterList.length; i++) {
                        if ($scope.selectedmaterialMasterList[i].VoucherNo == $scope.VNo) {
                            $scope.selectedmaterialMasterList.splice(i, 1);
                        }
                    }
                    $scope.GetCapitalizationMasterDetail();
                    $scope.SaveRegister();
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
            for (var i = 0; i < $scope.selectedmaterialMasterList.length; i++) {
                $scope.register.TotalAmount += $scope.selectedmaterialMasterList[i].Amount;
                if ($scope.selectedmaterialMasterList[i].Source == 'AUC') {
                    $scope.register.GRNAmount += $scope.selectedmaterialMasterList[i].Amount;
                }
                else if ($scope.selectedmaterialMasterList[i].Source == 'CI') {
                    $scope.register.IssueAmount += $scope.selectedmaterialMasterList[i].Amount;
                }
                else {
                    $scope.register.ExpensesAmount += $scope.selectedmaterialMasterList[i].Amount;
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
                        "data": $scope.register, "items": $scope.selectedmaterialMasterList
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                        $scope.saveBtnDisable = false;
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.register.Id = response.data.Id;
                        $scope.getSavedData();
                        $scope.saveBtnDisable = false;
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
    }



    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

}