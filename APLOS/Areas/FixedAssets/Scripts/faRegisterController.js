"use strict";
faRegisterController.$inject = ["addressService", "commonMessage", "$scope", "$rootScope", "baseService", "cboService", "$http", "$filter", "$controller", "$window"];
function faRegisterController(addressService, commonMessage, $scope, $rootScope, baseService, cboService, $http, $filter, $controller, $window) {
    $rootScope.title = "Fixed Asset Register";
    $scope.Action = "Save";
    $scope.isSave = false;
    $scope.btndeleteregister = true;
    $scope.temPAssetRegisterList = null;
    $scope.getDataWithFAMSavedRow = 0;
    $scope.message_confirmation = "";
    $scope.path = "fixedassets/fixedassetregister/";
    $scope.getListUrl = $scope.path + "getlist";

    $scope.searchbyAssetlist = [];
    $scope.searchbyVendorlist = [];
    $scope.searchbyMachineTypelist = [];
    $scope.searchbyixedAssetMasterList = [];
    $scope.searchbyMaterialMasterlist = [];
    $scope.registerList = [];
    $scope.fixedAssetMasterList = [];
    $scope.fixedAssCatList = [];
    $scope.fixedAssSubcatList = [];
    $scope.fixedAssetClassList = [];
    $scope.fixedAssetSubClassList = [];
    $scope.tranCurrencyList = [];
    $scope.brandList = [];
    $scope.companyList = [];
    $scope.Data = [];
    $scope.partyType = "Vendor";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller("baseAttributeAndCharacteristicsValueController", { $scope: $scope, $http: $http });

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


    $scope.partySearchByList = [
        {
            "name": $scope.partyType + " Code",
            "value": "Code"
        },
        {
            "name": $scope.partyType + " Name",
            "value": "UserName"
        },
        {
            "name": "Account Group",
            "value": "PartyAccountGroupName"
        },
        {
            "name": "Country",
            "value": "CountryName"
        },
        {
            "name": "State",
            "value": "StateName"
        },
        {
            "name": "Currency",
            "value": "CurrencyCode"
        }
    ];

    function CurrencyList() {
        cboService.getCboTransactionCurrencyByCompany("", function (result) {
            $scope.tranCurrencyList = result;
        });
    }
    CurrencyList();

    $scope.loadPlant = function (companyId) {
        try {
            cboService.getCboPlantByCompany(companyId, function (result) {
                $scope.plantList = result;
            });
            cboService.getCboUnitByCompany(companyId, function (result) {
                $scope.unitList = result;
            });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };

    $scope.loadSequence = function () {
        try {
            $http.get($scope.getSeqUrl)
                .then(function (response) {
                    $scope.registermodal.Sequence = response.data;
                });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };

    $scope.loadDDL = function () {
        try {
            $http.get("fixedassets/companygroupfixedassetsubcategory/getlist/")
                .then(function (response) {
                    $scope.fixedAssSubcatList = response.data;
                });

            $http.get("fixedassets/companygroupfixedassetcategory/getlist/")
                .then(function (response) {
                    $scope.fixedAssCatList = response.data;
                });
            $http.get("fixedassets/fixedassetsubclass/getcbo/")
                .then(function (response) {
                    $scope.fixedAssetSubClassList = response.data;
                });
            $http.get("fixedassets/fixedassetclass/getcbo/")
                .then(function (response) {
                    $scope.fixedAssetClassList = response.data;
                });
            addressService.getCountryCbo(function (result) {
                $scope.countryList = result;
            });

            cboService.getCboBrand(function (result) {
                $scope.brandList = result;
            });

            cboService.getCboCompanyByCompanyGroup(null, function (result) {
                $scope.companyList = result;
            });
            getCutOffDate();
        } catch (e) {
            ShowResult(e, "Error");
        }
    };
    $scope.loadDDL();

    function getCutOffDate() {
        $http({
            method: "GET",
            url: "Accounts/OpeningBalance/GetACCCutOffDate"
        }).then(function successCallback(response) {
            $scope.cutOffDate = response.data.CutOffDate;
        });
    }

    $scope.loadDDLDetail = function () {
        try {
            $http.get($scope.path + "GetSubprocess?processid=" + $scope.register.ProcessId)
                .then(function (response) {
                    $scope.subProcessList = [];
                    $scope.subProcessList = response.data;
                });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };

    $scope.loadDDLDetailChild = function () {
        try {
            $http.get($scope.path + "getmmuomcbo?materialmasterid=" + $scope.detailchildmodal.MaterialItemId)
                .then(function (response) {
                    $scope.uomChildList = response.data;
                });
        } catch (e) {
            ShowResult(e, "Error");
        }
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

    //$scope.getData = function (pageno) {
    //    $scope.registerListParameters.ids = JSON.stringify([]);
    //    baseService.paginationBase($scope.path + "getlist", pageno, $scope.registerListParameters)
    //        .then(function (result) {
    //            $scope.registerList = result.Rows;
    //            $scope.registerListParameters.total_count = result.Total;
    //            if (baseService.arrayLength($scope.searchbyRegisterlist) === 0) {
    //                baseService.getDDLSearchColumn(result.Rows, $scope.searchbyRegisterlist);
    //            }
    //        }, function () {
    //            ShowResult(commonMessage.NetworkError, "failure");
    //        }).finally(function () {
    //        });
    //};
    //$scope.getData();

    $scope.getMMData = function () {
        baseService.init($scope.path + "getmaterialmasterlist", null, 25, null, "Description", "Description");
        $scope.loadMMData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.mmData = result.Rows;
                    if (baseService.arrayLength($scope.searchbyMaterialItemDatalist) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyMaterialItemDatalist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        }; $scope.loadMMData();
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

    //$scope.materialMasterList = [];
    //$scope.getAssetData = function (faType) {
    //    var url = "FixedAssets/FixedAssetRegister/GetCapitalizedAssetItem?faType=" + faType;
    //    baseService.setCurrentPage("materialMasterList");
    //    $scope.loadMaterialMasterModalList = function (pageno) {
    //        baseService.paginationBase(url, pageno, $scope.searchMaterialMasterParameters)
    //            .then(function (result) {
    //                $scope.materialMasterList = result.Rows;
    //                $scope.searchMaterialMasterParameters.total_count = result.Total;
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, "failure");
    //            }).finally(function () {
    //            });
    //    };
    //    $scope.loadMaterialMasterModalList();
    //};

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


    /****getfromLIst******/
    function IdList() {
        $scope.materialMasterIdstr = createIdList(validListWithStr($scope.materialMasterNewList, $scope.materialMasterIds));
    }
    function createIdList(list) {
        var value = "";
        for (var i = 0; i < list.length; i++) {
            if (value === "") {
                value = "" + list[i].Value + "";
            } else {
                value += "," + list[i].Value + "";
            }
        }
        return value;
    }
    function getListForm(list) {
        $scope.materialMasterNewList = createCbo(list, "MaterialMasterId", "MaterialMasterName");
    }
    function createCbo(dblist, value, text) {
        var list = [];
        for (var i = 0; i < dblist.length; i++) {
            if (!ddlFilter(list, dblist[i][value])) {
                list.push({
                    Text: dblist[i][text],
                    Value: dblist[i][value]
                })
            }
        }
        //Sorting with text A-Z
        return list.sort(function (a, b) {
            var nameA = a.Text.toUpperCase(); // ignore upper and lowercase
            var nameB = b.Text.toUpperCase(); // ignore upper and lowercase
            if (nameA < nameB) {
                return -1;
            }
            if (nameA > nameB) {
                return 1;
            }

            // names must be equal
            return 0;
        });
    }
    function ddlFilter(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Value === id)
                return true;
        }
        return false;
    }
    function newList(oldMainDDlList, values, name) {
        var list = [];
        for (var i = 0; i < oldMainDDlList.length; i++) {
            if (values.length > 0) {
                for (var ii = 0; ii < values.length; ii++) {
                    if (oldMainDDlList[i][name] === values[ii].Value) {
                        list.push({
                            Id: oldMainDDlList[i].Id,
                            FixedAssetMasterName: oldMainDDlList[i].FixedAssetMasterName,
                            MaterialMasterId: oldMainDDlList[i].MaterialMasterId,
                            MaterialMasterName: oldMainDDlList[i].MaterialMasterName,
                            FixedAssetMasterId: oldMainDDlList[i].FixedAssetMasterId,
                        });
                    }
                }
            }
            else {
                list.push({
                    Id: oldMainDDlList[i].Id,
                    FixedAssetMasterName: oldMainDDlList[i].FixedAssetMasterName,
                    MaterialMasterId: oldMainDDlList[i].MaterialMasterId,
                    MaterialMasterName: oldMainDDlList[i].MaterialMasterName,
                    FixedAssetMasterId: oldMainDDlList[i].FixedAssetMasterId
                });
            }
        }
        return list;
    }
    function ddlFilterByDDL(newlist, value, text) {
        var list = [];
        for (var i = 0; i < newlist.length; i++) {
            if (!ddlFilter(list, newlist[i][value])) {
                list.push({
                    Value: newlist[i][value],
                    Text: newlist[i][text]
                });
            }
        }
        return list.sort(function (a, b) {
            var nameA = a.Text.toUpperCase(); // ignore upper and lowercase
            var nameB = b.Text.toUpperCase(); // ignore upper and lowercase
            if (nameA < nameB) {
                return -1;
            }
            if (nameA > nameB) {
                return 1;
            }

            // names must be equal
            return 0;
        });
    }
    $scope.cboCratetor = function (val, name) {
        $scope.newList = [];
        $scope.newList = newList($scope.attributeMasters, val, name);
        if (name !== "MaterialMasterId")
            $scope.materialMasterNewList = ddlFilterByDDL($scope.newList, "MaterialMasterId", "MaterialMasterName");
    };
    $scope.multiSelectSettings = {
        scrollableHeight: "auto",
        smartButtonMaxItems: 3,
        scrollable: true,
        showCheckAll: false,
        showUncheckAll: false,
        enableSearch: true,
        dynamicTitle: true
    };

    $scope.materialMasterIds = [];
    $scope.multi3events = {
        onItemSelect: function (item) {
            //if ($scope.materialMasterIds.length > 0) {
            $scope.cboCratetor($scope.materialMasterIds, "MaterialMasterId");
            //}
        }, onItemDeselect: function (item) {
            //if ($scope.materialMasterIds.length > 0) {
            $scope.cboCratetor($scope.materialMasterIds, "MaterialMasterId");
            //}
        }
    };

    function validListWithStr(list, values) {
        var tempValues = [];
        for (var i = 0; i < values.length; i++) {
            for (var j = 0; j < list.length; j++) {
                if (values[i].Value === list[j].Value) {
                    tempValues.push(values[i]);
                }
            }
        }
        return tempValues;
    }
    $scope.fixedAssetRegisterDetailList = [];
    $scope.checkIsRegisterApplyInMaterialMaster = function (data) {
        if ($scope.FaType == 'AssetCapatalized') {
            $scope.IsOpeningBalancePostStatus = null;
            $scope.assetRegisterCharactreristicsList = [];
            $scope.register.InvoiceDate = data.InvoiceDate;
            $scope.register.InvoiceNo = data.InvoiceNo;
            $scope.register.InvoiceDate = data.InvoiceDate;
            $scope.register.CurrencyId = data.CurrencyId;
            $scope.register.FABaseCurrencyId = data.BaseCurrencyId;
            $scope.register.CurrencyCode = data.CurrencyCode;
            $scope.register.GRNCurrencyCode = data.GRNCurrencyCode;
            $scope.register.CountryOfOriginId = data.CountryId;
            $scope.register.VoucherNo = data.VoucherNo;
            $scope.register.VoucherDate = data.VoucherDate;

            $scope.register.BaseUOMName = data.TransactionUoM;
            $scope.register.CapitalizationDate = data.CapitalizeDate;
            $scope.register.Vendor = data.VendorName;
            $scope.register.VendorId = data.PartyId;
            $scope.register.MaterialMasterId = data.MaterialMasterId;
            $scope.register.MaterialMasterArticleId = data.ArticleId;
            $scope.register.FirstCharacteristicsValueId = data.FirstCharacteristicsValueId;
            $scope.register.VoucherDetailId = data.VoucherDetailNo;
            $scope.register.InventoryIssueHistoryId = data.InventoryIssueHistoryId;

            $scope.register.MaterialMasterName = data.MaterialMasterName;
            $scope.register.ArticleStandardName = data.ArticleStandardName;

            $scope.register.ADBaseAmount = 0;
            if (data.Qty > 1) {
                $scope.register.FABaseAmount = data.FABaseAmount / data.Qty;
                $scope.register.Price = data.Amount / data.Qty;

            }
            else {
                $scope.register.FABaseAmount = data.FABaseAmount;
                $scope.register.Price = data.Amount
            }

            $scope.NumberOfQuantity = data.Qty;
            $scope.register.TotalPrice = data.FABaseAmount
            $scope.register.TotalGRNAmount = data.Amount


            $scope.materialArticleInfo = {
                ArticleCode: null,
                ArticleStandardName: null
            };
            $scope.materialArticleInfo.ArticleStandardName = data.ArticleStandardName;
            $scope.fixedAssetRegisterDetailList = [];
            $scope.fixedAssetRegisterDetailList.push(data);
            $scope.getMaterialMasterCode(data);


        }
        else if ($scope.FaType == 'AssetNonCapitalized') {
            $scope.IsOpeningBalancePostStatus = null;
            $scope.assetRegisterCharactreristicsList = [];
            $scope.nonAssetregister.InvoiceDate = data.InvoiceDate;
            $scope.nonAssetregister.InvoiceNo = data.InvoiceNo;
            $scope.nonAssetregister.InvoiceDate = data.InvoiceDate;
            $scope.nonAssetregister.CurrencyId = data.CurrencyId;
            $scope.nonAssetregister.CountryOfOriginId = data.CountryId;
            $scope.nonAssetregister.VoucherNo = data.VoucherNo;
            $scope.nonAssetregister.VoucherDate = data.VoucherDate;

            $scope.nonAssetregister.BaseUOMName = data.TransactionUoM;
            $scope.nonAssetregister.CapitalizationDate = data.CapitalizeDate;
            $scope.nonAssetregister.Vendor = data.VendorName;
            $scope.nonAssetregister.VendorId = data.PartyId;
            $scope.nonAssetregister.MaterialMasterId = data.MaterialMasterId;
            $scope.nonAssetregister.MaterialMasterArticleId = data.ArticleId;
            $scope.nonAssetregister.FirstCharacteristicsValueId = data.FirstCharacteristicsValueId;
            $scope.nonAssetregister.VoucherDetailId = data.VoucherDetailNo;
            $scope.nonAssetregister.InventoryIssueHistoryId = data.InventoryIssueHistoryId;

            $scope.nonAssetregister.MaterialMasterName = data.MaterialMasterName;
            $scope.nonAssetregister.ArticleStandardName = data.ArticleStandardName;

            $scope.nonAssetregister.ADBaseAmount = 0;
            if (data.Qty > 1)
                $scope.nonAssetregister.Price = data.Amount / data.Qty;
            else
                $scope.nonAssetregister.Price = data.Amount
            $scope.nonAssetregister.FABaseAmount = $scope.register.Price;
            $scope.nonAssetNumberOfQuantity = data.Qty;
            $scope.nonAssetregister.TotalPrice = data.Amount


            $scope.materialArticleInfo = {
                ArticleCode: null,
                ArticleStandardName: null
            };
            $scope.materialArticleInfo.ArticleStandardName = data.ArticleStandardName;
            $scope.getMaterialMasterCode(data);

        }
        //$scope.register.MaterialMasterArticleId = null;
        //$http({
        //    method: "GET",
        //    url: "FixedAssets/FixedAssetRegister/CheckMasterIsRegisterApplyByAssetId?assetMasterId=" + data.FixedAssetMasterId
        //}).then(function successCallback(response) {
        //    $scope.IsRegisterApplyList = response.data;
        //    if (response.data.length > 0) {
        //        $scope.IsOpeningBalancePostStatus = $scope.IsRegisterApplyList[0].IsPark;
        //    }

        //});
    };

    $scope.getMaterialMasterCode = function (data) {
        try {
            if ($scope.FaType == 'AssetCapatalized') {

                $scope.register.AssetBudgetMasterId = data.BudgetMasterId;
                $scope.register.AssetGLId = data.AssetGLId;
                $scope.register.BaseUOMName = data.TransactionUoM;
                $scope.register.FixedAssetMasterId = data.FixedAssetMasterId;
                $scope.register.AssetMasterName = data.AssetMasterName;
                $scope.register.AssetGLName = data.AssetGLName;
                $scope.register.AssetBudgetName = data.AssetBudgetName;
                $scope.register.AssetActivityName = data.ActivityName;
                $scope.register.AssetActivityId = data.ActivityId;

                $scope.getWithFAM();
            }
            else if ($scope.FaType == 'AssetNonCapitalized') {

                $scope.nonAssetregister.AssetBudgetMasterId = data.BudgetMasterId;
                $scope.nonAssetregister.AssetGLId = data.AssetGLId;
                $scope.nonAssetregister.BaseUOMName = data.TransactionUoM;
                $scope.nonAssetregister.FixedAssetMasterId = data.FixedAssetMasterId;
                $scope.nonAssetregister.AssetMasterName = data.AssetMasterName;
                $scope.nonAssetregister.AssetGLName = data.AssetGLName;
                $scope.nonAssetregister.AssetBudgetName = data.AssetBudgetName;
                $scope.nonAssetregister.AssetActivityName = data.ActivityName;
                $scope.nonAssetregister.AssetActivityId = data.ActivityId;
                $scope.getWithFAM();
            }
        } catch (e) {
            throw e;
        }
    };
    $scope.clearAssetCode = function () {
        $scope.register.FixedAssetId = null;
        $scope.register.FixedAsset = null;
    };
    $scope.getMaterialMasterAttribute = function () {
        $http.get("Machines/assetitem/GetMaterialMasterAttributeList?materialMaster=" + $scope.register.MaterialMasterId)
            .then(function (response) {
                $scope.attributeMasters = response.data;
                console.log($scope.attributeMasters);
                getListForm($scope.attributeMasters);
            });
    };

    $scope.getMachineTypeCode = function (id, MachineType) {
        $scope.register.FAMMachineTypeId = id;
        $scope.register.MachineTypeName = MachineType;
        angular.element(document.querySelector("#fmMachineTypemodal")).modal("hide");
    };

    $scope.clearVendorCode = function () {
        $scope.register.VendorId = null;
        $scope.register.Vendor = null;
    };

    $scope.GetRegisterIndex = function (id) {
        $scope.getRegisterData(id);

        $scope.NumberOfQuantity = null;
        $scope.saveBtnDisable = false;
        angular.element(document.querySelector("#registersearchpopup")).modal("hide");
    };

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue === null || fieldValue === "") {
                throw "[" + fieldName + "] is required...";
            }
        } catch (e) {
            throw e;
        }
    }

    function CheckFieldTime(fieldValue, fieldName) {
        try {
            CheckField(fieldValue, fieldName);
            if (fieldValue.length !== 5) {
                throw fieldName + " is not correct format...Ex: 08:00, 15:30 (HH:mm)";
            }
            if (fieldValue.substr(2, 1) !== ":") {
                throw fieldName + " is not correct format...Ex: 08:00, 15:30 (HH:mm)";
            }
            var a = parseInt(fieldValue.substr(0, 2));
            if (a > 23) {
                throw fieldName + " can not be greater than 23...";
            }
            if (a < 0) {
                throw fieldName + " can not be negative...";
            }
            var b = parseInt(fieldValue.substr(3, 2));
            if (b > 59) {
                throw fieldName + " can not be greater than 59...";
            }
            if (b < 0) {
                throw fieldName + " can not be negative...";
            }

            if (a === 0 && b === 0) {
                throw fieldName + " can not be blank...";
            }
            //first 2 digit check integer
            //last 2 digit check integer
        } catch (e) {
            throw e;
        }
    }

    function getRemoveValue(value) {
        return value / $scope.getOpeningBalanceDataWithFAM.TotalRow;
    }

    function ValidationRegister() {
        try {
            CheckField($scope.register.VendorId, "Vendor");
            CheckField($scope.register.YearOfManufacture, "Year Of Manufacture");
            CheckField($scope.register.YearOfInstallation, "Year Of Installation");
            CheckField($scope.register.CurrencyId, "Currency");
            CheckField($scope.register.Price, "Price");
            if ($scope.register.IsFinancial === false) {
                $scope.register.FABaseAmount = 0;
                $scope.register.FAGroupAmount = 0;
                $scope.register.ADBaseAmount = 0;
            }
            if ($scope.register.IsFinancial) {
                if ($scope.IsOpeningBalancePostStatus && $scope.IsRegisterApplyList.length > 0) {
                    throw "<b>(" + $scope.register.MaterialMasterName + ")</b> is not yet posted in Opening balance. ";
                }

                //if ($scope.IsRegisterApplyList.length === 0) {
                //    throw $scope.register.MaterialMasterName + " is not available in Opening Balance. ";
                //}
                //converting to float
                $scope.register.FABaseAmount = parseFloat($scope.register.FABaseAmount === null ? 0 : $scope.register.FABaseAmount);
                $scope.register.FAGroupAmount = parseFloat($scope.register.FAGroupAmount === null ? 0 : $scope.register.FAGroupAmount);
                $scope.register.FAHardAmount = parseFloat($scope.register.FAHardAmount === null ? 0 : $scope.register.FAHardAmount);
                $scope.register.ADBaseAmount = parseFloat($scope.register.ADBaseAmount === null ? 0 : $scope.register.ADBaseAmount);
                $scope.register.Price = parseFloat($scope.register.Price);

                $scope.getOpeningBalanceDataWithFAM.FABaseAmountTotal = parseFloat($scope.getOpeningBalanceDataWithFAM.FABaseAmountTotal);
                $scope.getOpeningBalanceDataWithFAM.FAGroupAmountTotal = parseFloat($scope.getOpeningBalanceDataWithFAM.FAGroupAmountTotal);
                $scope.getOpeningBalanceDataWithFAM.FAHardAmountTotal = parseFloat($scope.getOpeningBalanceDataWithFAM.FAHardAmountTotal);
                if (baseService.isUndefinedOrNull($scope.register.FABaseAmount) || baseService.isUndefinedOrNull($scope.register.FAGroupAmount) || baseService.isUndefinedOrNull($scope.register.FAHardAmount)) {
                    throw "Asset Value can\"t be empty.";
                }

                if (!$scope.register.FABaseAmount > 0) {
                    throw "Asset Historical Value must be greater than ZERO";
                }
                if ($scope.register.ADBaseAmount > $scope.register.FABaseAmount) {
                    throw "Accumulated Depreciation value is more than Asset Historical value";
                }
                /// Validation for Currency Price with purchase price

                if ($scope.register.FABaseAmount !== 0 || $scope.register.FAGroupAmount !== 0 || $scope.register.FAHardAmount !== 0) {
                    if ($scope.register.CurrencyId === $scope.register.FABaseCurrencyId && $scope.register.Price < $scope.register.FABaseAmount) {
                        throw $scope.parallelCurrencyList[0].Code + " purchase price must be equal to asset historical value. ";
                    }
                    if ($scope.register.CurrencyId === $scope.register.FAGroupCurrencyId && $scope.register.Price < $scope.register.FAGroupAmount) {
                        throw $scope.parallelCurrencyList[1].Code + " purchase price must be equal to asset historical value.";
                    }
                    if ($scope.register.CurrencyId === $scope.register.FAHardCurrencyId && $scope.register.Price < $scope.register.FAHardAmount) {
                        throw $scope.parallelCurrencyList[2].Code + " purchase price must be equal to asset historical value.";
                    }

                    if ($scope.register.CurrencyId === $scope.register.FABaseCurrencyId && $scope.register.Price > $scope.register.FABaseAmount) {
                        throw $scope.parallelCurrencyList[0].Code + " purchase price must be equal to asset historical value.";
                    }
                    if ($scope.register.CurrencyId === $scope.register.FAGroupCurrencyId && $scope.register.Price > $scope.register.FAGroupAmount) {
                        throw $scope.parallelCurrencyList[1].Code + " purchase price must be equal to asset historical value.";
                    }
                    if ($scope.register.CurrencyId === $scope.register.FAHardCurrencyId && $scope.register.Price > $scope.register.FAHardAmount) {
                        throw $scope.parallelCurrencyList[2].Code + " purchase price must be equal to asset historical value.";
                    }
                    if ($scope.parsetNOQtoNumber + $scope.getDataWithFAM.TotalRow > $scope.getOpeningBalanceDataWithFAM.TotalRow) {
                        throw "Total number of asset can\'t be greater than opening balance quantity : " + $scope.getOpeningBalanceDataWithFAM.TotalRow;
                    }
                    if ($scope.register.FABaseAmount < $scope.register.ADBaseAmount) {
                        throw $scope.parallelCurrencyList[0].Code + " Acc Dep amount  can\'t be greater then Asset Historical Value : " + $scope.register.FABaseAmount;
                    }
                    if ($scope.register.FAGroupAmount < $scope.register.ADGroupAmount) {
                        throw $scope.parallelCurrencyList[1].Code + " Acc Dep amount  can\'t be greater then Asset Historical Value : " + $scope.register.FAGroupAmount;
                    }
                    if ($scope.register.FAHardAmount < $scope.register.ADHardAmount) {
                        throw $scope.parallelCurrencyList[2].Code + " Acc Dep amount  can\'t be greater then Asset Historical Value : " + $scope.register.FAHardAmount;
                    }
                }
            }


            if ($scope.register.YearOfManufacture > new Date($scope.register.InvoiceDate).getFullYear()) {
                throw " Manufacture year must be <=  Invoice date";
            }
            if ($scope.register.YearOfInstallation < new Date($scope.register.InvoiceDate).getFullYear()) {
                throw "Installation year must be  >=  Invoice date";
            }
            if (new Date($scope.register.CapitalizationDate) < new Date($scope.register.InvoiceDate)) {
                throw "CapitalizationDate must be >= Invoice date";
            }
        } catch (e) {
            throw e;
        }
    }


    function CheckDuplicate(ob) {
        try {
            for (var i = 0; i < baseService.arrayLength($scope.detailchildList); i++) {
                if (ob.Id !== $scope.detailchildList[i].Id) {
                    if (ob.MaterialItemId === $scope.detailchildList[i].RawMaterialId) {
                        throw "Material Item: [" + ob.MaterialItemDescription + "] has already been taken...";
                    }
                }
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.ShowHead = function (mid) {
        if (mid === null || mid === "") {
            return false;
        }
        else {
            return true;
        }
    };

    $scope.ShowHeadButton = function (mmid) {
        if (mmid === null || mmid === "") {
            return false;
        }
        else {
            return true;
        }
    };

    $scope.showCharacteristicsGrid = function (hasCharForMM) {
        if (hasCharForMM === null || hasCharForMM === "") {
            return false;
        }
        else {
            return true;
        }
    };

    $scope.loadProcessAsperConfig = function () {
        $http({
            method: "GET",
            url: $scope.path + "getprocessasperconfigcbo?materialmasterid=" + $scope.register.MaterialItemId
        }).then(function successCallback(response) {
            var r = response.data;
            if (baseService.arrayLength(r) > 0) {
                $scope.processList = r;
            }
        });
    };

    $scope.loadMMUomList = function () {
        $http({
            method: "GET",
            url: $scope.path + "getmmuomcbo?materialmasterid=" + $scope.register.MaterialItemId
        }).then(function successCallback(response) {
            $scope.mmUomList = response.data;
        });
    };

    $scope.loadMMUomList = function (obj) {
        $scope.uomChildList = [];
        $http({
            method: "GET",
            url: $scope.path + "getmmuomcbo?materialmasterid=" + obj.MaterialItemId
        }).then(function successCallback(response) {
            $scope.uomChildList = response.data;
            $scope.detailchildmodal = obj;
            $scope.SaveDetailChildDisabled = false;
            $scope.ActionDetailChild = "Update";
        });
    };

    $scope.getParallelCurrency = function (companyId) {
        cboService.getParallelCurrency(companyId, function (result) {
            $scope.parallelCurrencyList = result;
            $scope.parallelCurrencytext();
        });
    };
    $scope.getParallelCurrency($scope.register.CompanyId);

    $scope.CompanyCurrencyCode = null;
    $scope.CompanyGroupCurrencyCode = null;
    $scope.HardCurrencyCode = null;
    $scope.parallelCurrencytext = function () {
        for (var i = 0; i < $scope.parallelCurrencyList.length; i++) {
            if ($scope.parallelCurrencyList[i].ParallelCurrencyType === "CompanyCurrency") {
                $scope.register.FABaseCurrencyId = $scope.parallelCurrencyList[i].CurrencyId;
                $scope.register.ADBaseCurrencyId = $scope.parallelCurrencyList[i].CurrencyId;
            }
            else if ($scope.parallelCurrencyList[i].ParallelCurrencyType === "CompanyGroupCurrency") {
                $scope.register.FAGroupCurrencyId = $scope.parallelCurrencyList[i].CurrencyId;
                $scope.register.ADGroupCurrencyId = $scope.parallelCurrencyList[i].CurrencyId;
            }
            else if ($scope.parallelCurrencyList[i].ParallelCurrencyType === "HardCurrency") {
                $scope.register.FAHardCurrencyId = $scope.parallelCurrencyList[i].CurrencyId;
                $scope.register.ADHardCurrencyId = $scope.parallelCurrencyList[i].CurrencyId;
            }
        }
    };
    //*************load 6 col with assetMaster****************/
    function setBalanceOnEditMood(data) {
        $scope.isSave = false;
        if ($scope.Action === "Update") {
            $scope.register.FABaseAmount = data.FABaseAmount;
            $scope.register.FAGroupAmount = data.FAGroupAmount;
            $scope.register.FAHardAmount = data.FAHardAmount;
            $scope.register.ADBaseAmount = data.ADBaseAmount;
            $scope.register.ADGroupAmount = data.ADGroupAmount;
            $scope.register.ADHardAmount = data.ADHardAmount;
        }
    }

    $scope.getDataWithFAM = {
        ADBaseAmountTotal: 0,
        ADGroupAmountTotal: 0,
        ADHardAmountTotal: 0,
        FABaseAmountTotal: 0,
        FAGroupAmountTotal: 0,
        FAHardAmountTotal: 0,
        TotalRow: 0
    };
    $scope.getWithFAM = function () {
        $http({
            method: "GET",
            url: $scope.path + "GetJVRegisterInfoWithFAMId?fixedAssetMasterId=" + $scope.register.FixedAssetMasterId + "&budgetMasterId=" + $scope.register.AssetBudgetMasterId + "&assetGLId=" + $scope.register.AssetGLId + "&activityId=" + $scope.register.AssetActivityId + "&companyId=" + $scope.register.CompanyId
        }).then(function successCallback(response) {
            $scope.getDataWithFAM = {
                ADBaseAmountTotal: 0,
                ADGroupAmountTotal: 0,
                ADHardAmountTotal: 0,
                FABaseAmountTotal: 0,
                FAGroupAmountTotal: 0,
                FAHardAmountTotal: 0,
                TotalRow: 0
            };
            if (response.data.length > 0) {
                $scope.getDataWithFAM = response.data[0];
            }
            $scope.getOpeningWithFAM();
        });
    };

    $scope.getOpeningWithFAM = function () {
        $http({
            method: "GET",
            url: $scope.path + "GetCapitalizeAssetItemValue?fixedAssetMasterId=" + $scope.register.FixedAssetMasterId + "&assetGLId=" + $scope.register.AssetGLId + "&assetBudgetId=" + $scope.register.AssetBudgetMasterId
                + "&assetActivityId=" + $scope.register.AssetActivityId + "&companyId=" + $scope.register.CompanyId
        }).then(function successCallback(response) {
            $scope.getOpeningBalanceDataWithFAM = {};
            $scope.getOpeningBalanceDataWithFAM = response.data[0];
            angular.element(document.querySelector("#assetmodal")).modal("hide");

            // $scope.getWithFAMTotalRow();
        });
    };

    $scope.getWithFAMTotalRow = function () {
        $http({
            method: "GET",
            url: $scope.path + "GetRegisterSavedTotalRowWithFAMId?assetMasterId=" + $scope.register.MaterialMasterId + "&budgetMasterId=" + $scope.register.AssetBudgetMasterId + "&assetGLId=" + $scope.register.AssetGLId + "&companyId=" + $scope.register.CompanyId
        }).then(function successCallback(response) {
            $scope.getDataWithFAMSavedRow = 0;
            if (response.data.length > 0) {
                $scope.getDataWithFAMSavedRow = response.data[0].TotalSavedRow;
                //$scope.NumberOfQuantity = response.data[0].TotalSavedRow;
                //$scope.NumberOfQuantity = response.data[0].NumberOfQuantity;
            }
            // angular.element(document.querySelector("#assetmodal")).modal("hide");

            //To get SKU need to use below function;
            //getMaterialMasterCharacteristics();
        });
    };

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

    function getSavedAssetRegisterList(Idlist) {
        $scope.savedAssetRegisterList = [];
        $scope.temPAssetRegisterList = null;
        if (Idlist !== undefined) {
            $http({
                method: "GET",
                url: $scope.path + "GetAssetRegisterIdList?AssetRegisterIdList=" + Idlist
            }).then(function successCallback(response) {
                $scope.savedAssetRegisterList = response.data;
                $scope.temPAssetRegisterList = angular.copy(Idlist);
                console.log("getDataWithFAMList", response);
            });
        }
    }

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

    $scope.getPlantCompanyWise = function () {
        try {
            $scope.loadPlant($scope.register.CompanyId);
        } catch (e) {
            ShowResult(e, "Error");
        }
    };

    $scope.getPlantCompanyWise();
    $scope.registerAddEditPopup = function (flag) {
        try {
            if (flag === "NEW") {
                $scope.savedAssetRegisterList = [];
                ClearRegister();
                $scope.isSave = false;
                $scope.saveBtnDisable = false;
                $scope.temPAssetRegisterList = null;
            }
            else if (flag === "DELETE") {
                ClearRegister();
            }
            else {
                ClearRegister();
                $scope.btndeleteregister = true;
                $scope.Action = "Update";
            }
        } catch (e) {
            ShowResult(e, "Error");
        }
    };

    $scope.fixedAssetMasterSearchPopup = function () {
        $scope.getFixedAssetMasterData();
        angular.element(document.querySelector("#fixedAssetMasterModal")).modal("show");
    };

    $scope.registerSearchPopup = function () {
        $scope.getData();
        angular.element(document.querySelector("#registersearchpopup")).modal("show");
    };

    $scope.showMMModal = function () {
        $scope.getMMData();
        angular.element(document.querySelector("#mmmodal")).modal("show");
    };

    

    $scope.selectMaterialMasterData = function (data) {
        $scope.register.MaterialMasterId = data.Id;
        $scope.register.AssetMasterName = data.AssetMasterName;
        $scope.register.AssetGLName = data.AssetGLName;
        $scope.register.AssetBudgetName = data.AssetBudgetName;
        angular.element(document.querySelector("#assetModal")).modal("hide");
    };

    $scope.checkIsRegisterApplyInUpdate = function (id) {
        $scope.IsOpeningBalancePostStatus = null;
        $http({
            method: "GET",
            url: "FixedAssets/FixedAssetRegister/CheckFixedMasterIsRegisterApplyByJV?assetMasterId=" + id
        }).then(function successCallback(response) {
            $scope.IsRegisterApplyList = response.data;
        });
    };

    $scope.setFixedAssetMasterData = {};
    $scope.selectFixedAssetMasterData = function (data) {
        $scope.setFixedAssetMasterData = data;
        $scope.register.FixedAssetMasterId = data.Id;
        angular.element(document.querySelector("#fixedAssetMasterModal")).modal("hide");
    };

    $scope.deleteRegisterPop = function () {
        var _id = $scope.register.Id;
        $scope.message_confirmation = "Are you sure to delete [" + _id + "] ";
        angular.element(document.querySelector("#confirmregisterdelete")).modal("show");
    };
    $scope.removeRegisterYes = function () {
        angular.element(document.querySelector("#confirmregisterdelete")).modal("hide");
        $scope.DeleteRegister();
    };

    function getArticle() {
        $scope.articleHead = [];
        $scope.articleList = [];
        $http({
            method: "GET",
            url: "Materials/materialmasterarticle/getarticlvaluehead?materialMasterId=" + $scope.register.MaterialMasterId,
            contentType: "application/json; charset=utf-8"
        }).then(function successCallback(response) {
            $scope.articleHead = response.data;
            $http({
                method: "GET",
                url: "Materials/materialmasterarticle/getlist?materialMasterId=" + $scope.register.MaterialMasterId,
                contentType: "application/json; charset=utf-8"
            }).then(function successCallback(response) {
                $scope.articalesTempList = response.data;
                var articles = response.data;
                if ($scope.articleHead.length > 0 && $scope.articalesTempList.length === 0) {
                    return ShowResult("Article is required for this item", "failure", "assetmodal");
                }
                if (articles.length > 0) {
                    $http({
                        method: "GET",
                        url: "Materials/materialmasterarticle/GetArticleValueList?materialMasterId=" + $scope.register.MaterialMasterId,
                        contentType: "application/json; charset=utf-8"
                    }).then(function successCallback(response) {
                        if (baseService.arrayLength(response.data)) {
                            var valueData = response.data;
                            if (baseService.arrayLength($scope.articleHead)) {
                                for (var i = 0; i < articles.length; i++) {
                                    articles[i].MaterialMasterArticleValues = [];
                                    for (var a = 0; a < $scope.articleHead.length; a++) {
                                        articles[i].MaterialMasterArticleValues.push({
                                            Id: null
                                            , MaterialMasterId: null
                                            , MaterialMasterAttributeId: null
                                            , MaterialAttributeId: $scope.articleHead[a].MaterialAttributeId
                                            , MaterialAttributeName: $scope.articleHead[a].MaterialAttributeName
                                            , MaterialMasterArticleId: null
                                            , MaterialAttributeValueId: null
                                            , MaterialMasterAttributeValueId: null
                                            , MaterialAttributeValueFreeText: null
                                        });
                                    }
                                }
                            }

                            for (var t = 0; t < baseService.arrayLength(articles); t++) {
                                var articleRow = Object.assign({}, articles[t]);
                                checkValueSubMaterialId(valueData, articleRow);
                                $scope.articleList.push(articleRow);
                            }
                            if ($scope.articleList.length > 0 && $scope.registerEditMode === true)
                                getArticleInfoOnEdit($scope.registerList[0].MaterialMasterArticleId);
                            if ($scope.articleList.length > 0 && $scope.registerEditMode === false) {
                                angular.element(document.querySelector("#materialMasterArticlemodal")).modal("show");
                            }
                        }
                    });
                }
                getAttribute();
            });
        });
    }

    function checkValueSubMaterialId(valueData, articleRow) {
        for (var v = 0; v < baseService.arrayLength(articleRow.MaterialMasterArticleValues); v++) {
            var valueRow = articleRow.MaterialMasterArticleValues[v];
            for (var tt = 0; tt < baseService.arrayLength(valueData); tt++) {
                if (articleRow.Id === valueData[tt].MaterialMasterArticleId
                    && valueRow.MaterialAttributeId === valueData[tt].MaterialAttributeId) {
                    var newValue = valueData[tt];
                    valueRow.Id = newValue.Id;
                    valueRow.MaterialMasterId = newValue.MaterialMasterId;
                    valueRow.MaterialMasterAttributeId = newValue.MaterialMasterAttributeId;
                    valueRow.MaterialAttributeId = newValue.MaterialAttributeId;
                    valueRow.MaterialAttributeName = newValue.MaterialAttributeName;
                    valueRow.MaterialMasterArticleId = newValue.MaterialMasterArticleId;
                    valueRow.MaterialAttributeValueId = newValue.MaterialAttributeValueId;
                    valueRow.MaterialMasterAttributeValueId = newValue.MaterialMasterAttributeValueId;
                    valueRow.MaterialAttributeValueFreeText = newValue.MaterialAttributeValueFreeText;
                    break;
                }
            }
        }
    }

    $scope.materialArticleInfo = {
        ArticleCode: null,
        ArticleStandardName: null
    };

    $scope.selectMaterialMasterArticleInfo = function (data) {
        $scope.materialArticleInfo.ArticleCode = data.Code;
        $scope.materialArticleInfo.ArticleStandardName = data.StandardName;
        $scope.register.MaterialMasterArticleId = data.Id;
        angular.forEach($scope.articleHead, function (item) {
            angular.forEach(data.MaterialMasterArticleValues, function (itemx) {
                if (item.MaterialAttributeId === itemx.MaterialAttributeId) {
                    $scope.materialArticleInfo[item.MaterialAttributeName] = itemx.MaterialAttributeValueFreeText;
                }
            });
        });
        angular.element(document.querySelector("#materialMasterArticlemodal")).modal("hide");
    };

    $scope.skuValueGetMode = false;
    $scope.valueList = [];
    $scope.skuIndex = -1;
    $scope.getCharacteristicsValuePopUP = function (index, data) {
        $scope.skuValueGetMode = true;
        $scope.skuIndex = index;
        $scope.charValuePopUp(data);
    };

    $scope.assetRegisterCharactreristicsList = [];
    function getMaterialMasterCharacteristics() {
        $scope.assetRegisterCharactreristicsList = [];
        $scope.dimensionList = [];
        $http({
            method: "GET"
            , url: "Materials/materialmaster/GetMaterialMasterCharacteristics?masterId=" + $scope.register.MaterialMasterId
        }).then(function successCallback(response) {
            $scope.dimensionList = response.data;
            $scope.assetRegisterCharactreristicsList = [];
            angular.forEach($scope.dimensionList, function (item) {
                item.MaterialMasterCharacteristicsId = item.Id;
                item.Id = null;
                $scope.assetRegisterCharactreristicsList.push(item);
            });
            // getArticle();
        }), function errorCallBack(response) {
        };
    }
    function getMaterialMasterCharacteristicsValue() {
        $http({
            method: "GET"
            , url: "Materials/materialmaster/GetMaterialMasterCharacteristicsValue?masterId=" + $scope.register.MaterialMasterId
        }).then(function successCallback(response) {
            for (var t = 0; t < baseService.arrayLength($scope.assetRegisterCharactreristicsList); t++) {
                $scope.assetRegisterCharactreristicsList[t].MaterialMasterCharacteristicsValues = [];
                for (var a = 0; a < baseService.arrayLength(response.data); a++) {
                    if ($scope.assetRegisterCharactreristicsList[t].MaterialMasterCharacteristicsId === response.data[a].MaterialMasterCharacteristicsId) {
                        $scope.assetRegisterCharactreristicsList[t].MaterialMasterCharacteristicsValues.push(response.data[a]);
                    }
                }
            }
            if ($scope.skuValueGetMode) {
                $scope.valueList = $scope.assetRegisterCharactreristicsList[$scope.skuIndex].MaterialMasterCharacteristicsValues;
                $scope.searchFreeField = true;
                angular.element(document.querySelector("#valuePoUp")).modal("show");
            }
        }), function errorCallBack(response) {
        };
    }

    $scope.setCharData = function (data) {
        $scope.assetRegisterCharactreristicsList[$scope.skuIndex].CharacteristicsValueId = data.CharacteristicsValueId;
        $scope.assetRegisterCharactreristicsList[$scope.skuIndex].MaterialMasterCharacteristicsValueId = data.Id;
        $scope.assetRegisterCharactreristicsList[$scope.skuIndex].CharacteristicsValueFreeText = data.UserName;
        $scope.assetRegisterCharactreristicsList[$scope.skuIndex].FlagDisable = $scope.searchFreeField;
        $scope.skuIndex = -1;
        //baseService.manualValidation("div_arCh", false);
        $scope.closeCharValuePopUp();
    };

    function getAttribute() {
        $scope.assetRegisterCharactreristicsList = [];
        $http({
            method: "GET",
            url: "fixedassets/FixedAssetRegister/GetMaterialMasterCharacteristicsWithValueFreeText?materialMasterId=" + $scope.register.MaterialMasterId + "&registerid=" + $scope.register.Id,
        }).then(function successCallback(response) {
            $scope.assetRegisterCharactreristicsList = response.data;
            for (var i = 0; i < $scope.assetRegisterCharactreristicsList.length; i++) {
                $scope.searchFreeField = $scope.assetRegisterCharactreristicsList[i].CharacteristicsValueFreeText !== null ? true : false;
                var isFree = $scope.assetRegisterCharactreristicsList[i].IsFreeField;
                $scope.assetRegisterCharactreristicsList[i].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
            }
            // angular.element(document.querySelector("#assetmodal")).modal("hide");
        });
    }

    $scope.IsFreeFieldOrNot = function (IsFreeField) {
        if (IsFreeField) {
            if ($scope.searchFreeField)
                return true;
            else
                return false;
        }
        else
            return true;
    };

    $scope.IsMandatoryButNull = function (isMandatory, CharacteristicsValueFreeText) {
        if (isMandatory) {
            if (baseService.isUndefinedOrNull(CharacteristicsValueFreeText)) return true;
            else return false;
        }
        else return false;
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    cboService.getSubAssetTypeList(function (result) {
        $scope.subAssetTypeList = result;
    });

    $scope.subAsseType = {
        Id: null,
        SubAssetTypeId: null,
        Amount: '',
        SubAssetTypeName: null
    };

    $scope.subAssetList = [];
    $scope.addCharge = function () {
        if (manualValidation("td_FinancingType", baseService.isUndefinedOrNull($scope.subAsseType.SubAssetTypeId), "SubAsset Type is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_FinancingTypeAmount", baseService.isUndefinedOrNull($scope.subAsseType.Amount), "Amount is required.")) {
            $scope.invalidRow = true;
        }
        else {
            $scope.subAsseType.SubAssetTypeName = $.grep($scope.subAssetTypeList, function (item) {
                return item.FinancingTypeId === $scope.register.SubAssetTypeId;
            })[0].Text;
            $scope.subAssetList.push($scope.subAsseType);
            $scope.subAsseType.CurrencyId = $scope.register.CurrencyId;
            $scope.subAsseType.CapitalizationDate = $filter("dateFiltering")(Date.now());
            $scope.subAsseType = {};
        }
    };

    $scope.getSubAssetList = function (id) {
        $http({
            method: "GET",
            url: "FixedAssets/FixedAssetRegister/getJVSubAssetList?fixedAssetRegisterId=" + id
        }).then(function successCallback(response) {
            $scope.subAssetList = response.data;
        });
    };


    $http({
        method: 'GET',
        url: 'FixedAssets/FixedAssetRegister/getDepreciationRulelist/',
    }).then(function successCallback(response) {
        $scope.CompanyFADepRuleList = response.data;
    });


}