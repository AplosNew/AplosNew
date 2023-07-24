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
        var ey = parseInt(endYear.getFullYear())-5;
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
        angular.element(document.querySelector("#assetmodal")).modal("show");
    };

    $scope.materialMasterList = [];
    $scope.getSearchData = function (faType) {
        var url = "FixedAssets/FixedAssetRegister/GetAUCCIExpenseData?faType=" + faType;
        baseService.setCurrentPage("materialMasterList");
        $scope.loadMaterialMasterModalList = function (pageno) {
            baseService.paginationBase(url, pageno, $scope.searchMaterialMasterParameters)
                .then(function (result) {
                    $scope.materialMasterList = result.Rows;
                    $scope.searchMaterialMasterParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        $scope.loadMaterialMasterModalList();
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
    $scope.selectChValueId = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.selectedmaterialMasterList, data.ArticleId, data.VoucherId) === false) {

                    if ($scope.FaType == 'AUC') {
                        $scope.register.GRNAmount += data.Amount;
                    } else if ($scope.FaType == 'CI') {
                        $scope.register.IssueAmount += data.Amount;
                    }
                    else {
                        $scope.register.ExpensesAmount += data.Amount;
                    }


                    var ob = {};
                    ob.Id = null;
                    ob.InventoryReceiveDetailId = data.InventoryReceiveDetailId;
                    ob.InventoryIssueHistoryId = data.InventoryIssueHistoryId;
                    ob.VoucherDetailId = data.VoucherDetailNo;
                    ob.Amount = data.Amount;
                    ob.CurrencyId = data.CurrencyId;
                    ob.VoucherNo = data.VoucherNo;
                    ob.MaterialMasterName = data.MaterialMasterName;
                    ob.ArticleStandardName = data.ArticleStandardName;
                    ob.Qty = data.Qty;
                    ob.GRNNo = data.GRNNo;

                    if ($scope.FaType == 'AUC') {
                        ob.Source = 'AUC';
                        ob.Qty = data.BaseQty;
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
            else {
                for (var i = 0; i < $scope.selectedmaterialMasterList.length; i++) {
                    if ($scope.selectedmaterialMasterList[i].Id === data.Id) {
                        $scope.selectedmaterialMasterList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };

    function checkExistTempList(list, ArticleId, VoucherId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ArticleId === ArticleId && list[i].VoucherId === VoucherId) {
                return true;
            }
        }
        return false;
    }

    $scope.CheckAll = function (event) {
        var _isselected = event.target.checked;
        for (var i = 0; i < $scope.materialMasterList.length; i++) {
            $scope.materialMasterList[i].Flag = _isselected;
        }

        for (var i = 0; i < baseService.arrayLength($scope.materialMasterList); i++) {
            if (_isselected) {
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
                    ob.Qty = data.BaseQty;
                } else if ($scope.FaType == 'CI') {
                    ob.Source = 'CI';
                }
                else {
                    ob.Source = 'Expense';
                }

                $scope.selectedmaterialMasterList.push(ob);
                ob = {};
            }
            else
                for (var j = 0; j < $scope.selectedmaterialMasterList.length; j++) {
                    if ($scope.selectedmaterialMasterList[j].Id === $scope.materialMasterList[i].Id) {
                        $scope.selectedmaterialMasterList.splice(j, 1);
                        break;
                    }
                }
        }
    };

    $scope.VoucherNo = null;
    $scope.message_VoucherRemoveconfirmation = null;
    $scope.VId = null;
    $scope.RemoveItemVoucher = function (obj) {
        $scope.VoucherNo = obj.VoucherNo;
        $scope.VId = obj.Id;
        if (!baseService.isUndefinedOrNull(obj.VoucherNo))
            $scope.message_VoucherRemoveconfirmation = 'Are you sure want to delete permanently [ ' + obj.VoucherNo + ' ]';
        angular.element(document.querySelector('#confirmVoucherRemovePopUp')).modal('show');
    }

    $scope.RemoveVoucher = function () {
        if (baseService.isUndefinedOrNull($scope.VId)) {
            for (var i = 0; i < $scope.selectedmaterialMasterList.length; i++) {
                if ($scope.selectedmaterialMasterList[i].VoucherNo == $scope.VoucherNo) {
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
                        if ($scope.selectedmaterialMasterList[i].VoucherNo == $scope.VoucherNo) {
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

    $scope.CloseMMPopUp = function () {
        angular.element(document.querySelector("#assetmodal")).modal("hide");
    }

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
                if ($scope.selectedmaterialMasterList[i].Source =='AUC') {
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