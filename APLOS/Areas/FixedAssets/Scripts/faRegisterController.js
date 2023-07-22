"use strict";
faRegisterController.$inject = ["addressService", "commonMessage", "$scope", "$rootScope", "baseService", "cboService", "$http", "$filter", "$controller", "$window"];
function faRegisterController(addressService, commonMessage, $scope, $rootScope, baseService, cboService, $http, $filter, $controller, $window) {
    $rootScope.title = "Fixed Asset Register";
    $scope.Action = "Save";
    $scope.message_confirmation = "";
    $scope.path = "fixedassets/fixedassetregister/";

    $scope.register = {
        Id: null, FixedAssetItemId: null, CapitalizationDate: null, Qty: 0, GRNAmount: 0, IssueAmount: 0, ExpensesAmount: 0, Other: null, TotalAmount: 0, ApprovedById: null, Status: null, Type: null, VoucherRowId: null, Remark: null, InstallationYear: null, Lifetime: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
    };

    $scope.yearList = [];
    $scope.getYearOfHaving = function () {
        $scope.yearList = [];
        var endYear = new Date();
        var ey = parseInt(endYear.getFullYear());
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
    $scope.RemoveItemVoucher = function (obj) {
        $scope.VoucherNo = obj.VoucherNo;
        if (!baseService.isUndefinedOrNull(obj.VoucherNo))
            $scope.message_VoucherRemoveconfirmation = 'Are you sure want to delete permanently [ ' + obj.VoucherNo + ' ]';
        angular.element(document.querySelector('#confirmVoucherRemovePopUp')).modal('show');
    }

    $scope.RemoveVoucher = function () {
        for (var i = 0; i < $scope.selectedmaterialMasterList.length; i++) {
            if ($scope.selectedmaterialMasterList[i].VoucherNo == $scope.VoucherNo) {
                $scope.selectedmaterialMasterList.splice(i, 1);
            }
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
    $scope.popUpDataList = [];
    $scope.showEmployeeListPopUp = function () {
        try {
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'OrderManagements/SalesOrderApproval/GetAllActiveEmployeeData'
            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUp')).modal('show');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectEmployee = function (arg) {
        $scope.register.ApprovedById = arg.data.SystemId;
        $scope.register.ApprovedByName = arg.data.EmployeeName;
        $scope.register.ApprovedByEmployeeCode = arg.data.EmployeeCode;
        $scope.closePopUp();
    }

    $scope.clearEmp = function () {
        $scope.register.ApprovedById = null;
        $scope.register.ApprovedByName = null;
        $scope.register.ApprovedByEmployeeCode = null;
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }

    $scope.SaveRegister = function () {
        try {
            $scope.register.TotalAmount = 0;
            for (var i = 0; i < $scope.selectedmaterialMasterList.length; i++) {
                $scope.register.TotalAmount += $scope.selectedmaterialMasterList[i].Amount;

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
            Id: null, FixedAssetItemId: null, CapitalizationDate: null, Qty: 0, GRNAmount: 0, IssueAmount: 0, ExpensesAmount: 0, Other: null, TotalAmount: 0, ApprovedById: null, Status: null, Type: null, VoucherRowId: null, Remark: null, InstallationYear: null, Lifetime: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
        };
        $scope.selectedmaterialMasterList = [];
    }

    $scope.DeleteRegister = function () {
        try {
            if ($scope.register.Id === null || $scope.register.Id === "") {
                throw "No Asset is selected...";
            }
            $http({
                method: "POST",
                url: $scope.path + "deleteregister",
                dataType: "JSON",
                data: { "registerid": $scope.register.Id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.registerAddEditPopup("NEW");
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        } catch (e) {
            ShowResult(e, "Error");
        }
    };


    $scope.MainPageToModal = function () {
        for (var i in $scope.registermodal) {
            $scope.registermodal[i] = $scope.register[i];
        }
    };

    function ClearRegister() {
        $scope.register = {
            CompanyId: $window.companyId
            , FABaseAmount: 0
            , FAGroupAmount: 0
            , FAHardAmount: 0
            , ADBaseAmount: 0
            , ADGroupAmount: 0
            , ADHardAmount: 0
            , IsFinancial: true
        };
        $scope.getParallelCurrency($scope.register.CompanyId);

        $scope.btndeleteregister = false;
        $scope.registerEditMode = false;
        $scope.Action = "Save";
        $scope.NumberOfQuantity = null;
        $scope.setFixedAssetMasterData = {};

        $scope.getDataWithFAM = {
            ADBaseAmountTotal: 0
            , ADGroupAmountTotal: 0
            , ADHardAmountTotal: 0
            , FABaseAmountTotal: 0
            , FAGroupAmountTotal: 0
            , FAHardAmountTotal: 0
            , TotalRow: 0
        };

        $scope.getOpeningBalanceDataWithFAM = {
            FABaseAmountTotal: 0
            , FAGroupAmountTotal: 0
            , FAHardAmountTotal: 0
            , ADBaseAmountTotal: 0
            , ADGroupAmountTotal: 0
            , ADHardAmountTotal: 0
        };

        $scope.machineTypeData = [];
        $scope.attributeList = [];
        $scope.articleHead = [];
        $scope.materialArticleInfo = {
            ArticleCode: null,
            ArticleStandardName: null
        };

        $scope.characteristicsList = [];
        $scope.assetRegisterCharactreristicsList = [];
        $scope.subAssetList = [];
        $scope.loadDDL();
    }

    $scope.ModalToMainPage = function () {
        for (var i in $scope.register) {
            $scope.register[i] = $scope.registermodal[i];
        }
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

}