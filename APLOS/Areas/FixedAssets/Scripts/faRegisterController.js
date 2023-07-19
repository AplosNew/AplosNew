"use strict";
faRegisterController.$inject = ["addressService", "commonMessage", "$scope", "$rootScope", "baseService", "cboService", "$http", "$filter", "$controller", "$window"];
function faRegisterController(addressService, commonMessage, $scope, $rootScope, baseService, cboService, $http, $filter, $controller, $window) {
    $rootScope.title = "Fixed Asset Register";
    $scope.Action = "Save";
    $scope.message_confirmation = "";
    $scope.path = "fixedassets/fixedassetregister/";
    $scope.getListUrl = $scope.path + "getlist";

    $scope.register = {
        Id: null,
        FixedAssetMasterId: null,
        AssetGLId: null,
        BrandId: null,
        Plant: null,
        PlantId: $window.plantId,
        MaterialMasterArticleId: null,
        AssetBudgetMasterId: null,
        Model: null,
        Price: null,
        CurrencyId: null,
        SerialNo: null,
        InvoiceNo: null,
        InvoiceDate: null,
        YearOfManufacture: null,
        YearOfInstallation: null,
        Vendor: null,
        VendorId: null,
        FAMMachineTypeId: null,
        FABaseCurrencyId: null,
        FAGroupCurrencyId: null,
        FAHardCurrencyId: null,
        ADBaseCurrencyId: null,
        ADGroupCurrencyId: null,
        ADHardCurrencyId: null,
        MaterialMasterId: null,
        MaterialMasterName: null,
        AssetGLName: null,
        AssetBudgetName: null,
        BaseUOMName: null,
        FABaseAmount: null,
        FAGroupAmount: null,
        FAHardAmount: null,
        ADBaseAmount: null,
        ADGroupAmount: null,
        ADHardAmount: null,
        LifeTime: null,
        CapitalizationDate: null,
        CountryOfOriginId: null,
        CompanyId: $window.companyId,
        IsForProduction: false,
        IsFinancial: true,
        AssetActivityId: null,
        Archive: null,
        Description: null,
        ArticleStandardName: null,
        VoucherDetailId: null,
        TotalPrice: null,
        LCNumber: null,
        DepreciationRuleId: null,
        MultiplicationFactor: "1.0000"
    };

    $scope.registerListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "FixedAssetMasterName",
        searchBy: "FixedAssetMasterName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.searchByList = [
        {
            "name": "Material",
            "value": "MaterialMasterName"
        },
        {
            "name": "Article",
            "value": "Article"
        },
        {
            "name": "SerialNo",
            "value": "SerialNo"
        },
        {
            "name": "AssetNo",
            "value": "AssetNo"
        }
        ,
        {
            "name": "Voucher No",
            "value": "VoucherNo"
        },
        {
            "name": "InvoiceNo",
            "value": "InvoiceNo"
        },
        {
            "name": "FixedAssetMasterName",
            "value": "FixedAssetMasterName"
        },
        {
            "name": "FixedAssetCategory",
            "value": "FixedAssetCategory"
        },
        {
            "name": "FixedAssetSubCategory",
            "value": "FixedAssetSubCategory"
        },
        {
            "name": "AssetType",
            "value": "AssetType"
        }
    ];

    baseService.init('fixedassets/fixedassetregister/GetJVFixedAssetRegisterList', null, null, "DESC", "FixedAssetMasterName", "FixedAssetMasterName");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.registerList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();

  
    $scope.ItemName = null;
    $scope.showSearchData = function (faType) {
        $scope.FaType = faType;
        if (faType=='AUC') {
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

    $scope.selectedmaterialMasterList = [];
    $scope.selectChValueId = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.selectedmaterialMasterList, data.ArticleId, data.VoucherId) === false) {
                    $scope.selectedmaterialMasterList.push(data);
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
            if (_isselected)
                $scope.selectedmaterialMasterList.push($scope.materialMasterList[i]);
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
        $scope.VoucherNo= obj.VoucherNo;
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
        $scope.register.AssetItem = obj.data.UserName;
        $scope.register.AssetItemId = obj.data.Id;
        $scope.register.FixedAssetMasterId = obj.data.FixedAssetMasterId;
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
            $scope.parsetNOQtoNumber = $scope.NumberOfQuantity !== null ? $scope.NumberOfQuantity : 0;
            $scope.$broadcast("show-errors-check-validity");
            if ($scope.form0.$valid) {
                ValidationRegister();
                $scope.saveBtnDisable = true;
                $http({
                    method: "POST",
                    url: $scope.path + "CreateRegisterCapitalized",
                    dataType: "JSON",
                    data: {
                        "register": $scope.register, "subFixedAssetRegister": $scope.subAssetList
                        , "NumberOfQuantity": $scope.parsetNOQtoNumber
                        , "CompanyCurrencyCode": $scope.CompanyCurrencyCode
                        , "CompanyGroupCurrencyCode": $scope.CompanyGroupCurrencyCode
                        , "HardCurrencyCode": $scope.HardCurrencyCode
                        , "materialMasterValue": $scope.attributeList
                        , "fixedAssetRegisterSkuValue": $scope.assetRegisterCharactreristicsList
                        , "fixedAssetMasterId": $scope.register.FixedAssetMasterId
                        , "assetGLId": $scope.register.AssetGLId
                        , "assetBudgetId": $scope.register.AssetBudgetMasterId
                        , "assetActivityId": $scope.register.AssetActivityId
                        , "fixedAssetRegisterDetail": $scope.fixedAssetRegisterDetailList
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                        $scope.saveBtnDisable = false;
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        if (response.data.id !== "") {
                            if ($scope.temPAssetRegisterList === null) {
                                getSavedAssetRegisterList(response.data.id);
                            } else {
                                getSavedAssetRegisterList($scope.temPAssetRegisterList);
                            }
                            // assetRegisterCharactreristicsList();
                        }
                        $scope.registerEditMode = true;
                        $scope.getWithFAM();
                        $scope.getOpeningWithFAM();
                        if ($scope.Action === "Update") {
                            $scope.isSave = false;
                            $scope.saveBtnDisable = false;
                        } else {
                            $scope.isSave = true;
                            $scope.saveBtnDisable = true;
                        }
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
    $scope.SaveNonAssetRegister = function () {
        try {
            $scope.parsetNOQtoNumber = $scope.NumberOfQuantity !== null ? $scope.NumberOfQuantity : 0;
            $scope.$broadcast("show-errors-check-validity");
            if ($scope.form2.$valid) {
                ValidationNonAssetRegister();
                $scope.saveBtnDisable = true;
                $http({
                    method: "POST",
                    url: $scope.path + "CreateRegisterCapitalized",
                    dataType: "JSON",
                    data: {
                        "register": $scope.nonAssetregister, "subFixedAssetRegister": $scope.subAssetList
                        , "NumberOfQuantity": $scope.parsetNOQtoNumber
                        , "CompanyCurrencyCode": $scope.CompanyCurrencyCode
                        , "CompanyGroupCurrencyCode": $scope.CompanyGroupCurrencyCode
                        , "HardCurrencyCode": $scope.HardCurrencyCode
                        , "materialMasterValue": $scope.attributeList
                        , "fixedAssetRegisterSkuValue": $scope.assetRegisterCharactreristicsList
                        , "fixedAssetMasterId": $scope.nonAssetregister.FixedAssetMasterId
                        , "assetGLId": $scope.nonAssetregister.AssetGLId
                        , "assetBudgetId": $scope.nonAssetregister.AssetBudgetMasterId
                        , "assetActivityId": $scope.nonAssetregister.AssetActivityId
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                        $scope.saveBtnDisable = false;
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        if (response.data.id !== "") {
                            if ($scope.temPAssetRegisterList === null) {
                                getSavedAssetRegisterList(response.data.id);
                            } else {
                                getSavedAssetRegisterList($scope.temPAssetRegisterList);
                            }
                            // assetRegisterCharactreristicsList();
                        }
                        $scope.registerEditMode = true;
                        $scope.getWithFAM();
                        $scope.getOpeningWithFAM();
                        if ($scope.Action === "Update") {
                            $scope.isSave = false;
                            $scope.saveBtnDisable = false;
                        } else {
                            $scope.isSave = true;
                            $scope.saveBtnDisable = true;
                        }
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