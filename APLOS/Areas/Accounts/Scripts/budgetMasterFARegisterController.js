"use strict";
budgetMasterFARegisterController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http"];
function budgetMasterFARegisterController(cboService, commonMessage, $scope, $rootScope, baseService, $http) {
    $rootScope.title = "Budget Master";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.budgetMasters = [];
    $scope.CurrencyList = [];
    $scope.path = "accounts/budgetmaster/";
    $scope.getListUrl = $scope.path + "getlist";
    $scope.saveUrl = $scope.path + "create";
    $scope.updateUrl = $scope.path + "edit";
    $scope.deleteUrl = $scope.path + "delete/";
    $scope.CAction = "Add";

    $scope.onCOAChange = function () {
        baseService.init($scope.getListUrl, null, 15, null, "GL, BudgetItem", "BudgetItem");
        $scope.getData = function (pageno) {
            $rootScope.parameters.coaId = $scope.budgetMaster.COAId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.budgetMasters = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    // Combo list
    cboService.getBudgetClassCbo(function (result) {
        $scope.BudgetClassList = result;
    });

    cboService.getCboBudgetGroupByCompanyGroup(null, function (result) {
        $scope.budgetGroupList = result;
    });

    cboService.getCboChartOfAccount('', function (result) {
        $scope.cOAList = result;
    });

    cboService.getCboRegister(function (result) {
        $scope.registerList = result;
    });

    cboService.getBudgetCategoryCbo(function (result) {
        $scope.BudgetCategoryList = result;
    });

    cboService.getBudgetGroupCbo(function (result) {
        $scope.BudgetGroupList = result;
    });

    cboService.getBudgetSubCategoryCbo(function (result) {
        $scope.BudgetSubCategoryList = result;
    });

    cboService.getBudgetCbo(function (result) {
        $scope.BudgetList = result;
    });

    cboService.getEnumCbo("enum/GetActivityTypeCbo", function (result) {
        $scope.activityTypeList = result;
    });

    cboService.getEnumCbo("enum/GetCboFALinked", function (result) {
        $scope.fALinkedList = result;
    });

    cboService.getCompanyGroupCurrencyCbo(null, function (result) {
        $scope.currencyList = result;
    });

    cboService.getEnumCbo("enum/getbudgetforcbo", function (result) {
        $scope.budgetForList = result;
    });

    cboService.getEnumCbo("enum/getpaymentmodecbo", function (result) {
        $scope.paymentModeList = result;
    });

    $scope.budgetMaster = {
        Id: null,
        BudgetGroupId: null,
        BudgetCategoryId: null,
        BudgetSubCategoryId: null,
        BudgetClassId: null,
        BudgetId: null,
        RegisterId: null,
        BudgetType: null,
        CurrencyId: null,
        Active: true,
        Remarks: null,
        COAId: null,
        COAICode: null,
        COAIText: null,
        GLGeneralInfoId: null,
        IsProject: false
    };

    $scope.budgetActivity = {
        Id: null,
        CompanyGroupId: null,
        BudgetMasterId: null,
        ActivityType: null,
        ActivityId: null,
        FALinked: null,
        ReviewDate: null,
        Remarks: null,
        Active: null,
        Archive: null
    };

    $scope.searchByList = [
        {
            "name": "GL",
            "value": "GL"
        },
        {
            "name": "Budget Category",
            "value": "BudgetCategory"
        },
        {
            "name": "Budget SubCategory",
            "value": "BudgetSubCategory"
        },
        {
            "name": "Budget",
            "value": "BudgetItem"
        },
        {
            "name": "Budget Type",
            "value": "BudgetType"
        }
    ];

    // #region ******GL Item******
    $scope.searchglByList = [
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL",
            "value": "GLGeneralInfoName"
        }
    ];
    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoCode",
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetCOAICodeList = function () {
        $scope.GLUrl1 = "accounts/glitem/GetALLGLListSetup?coaId=" + $scope.budgetMaster.COAId;
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetCOAICodeListData();
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };

    $scope.closeCOAICodeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };

    $scope.removeRow = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };
    $scope.set = function () {
        if ($scope.selectedCode !== null) {
            $scope.selectedCode = null;
        }
    };

    $scope.rowSelected = null;
    $scope.setSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.budgetMaster.COAICode = x.GLGeneralInfoCode;
        $scope.budgetMaster.GLGeneralInfoId = x.GLGeneralInfoId;
        $scope.set();
        $scope.selectedCode = x.GLGeneralInfoCode;
        $scope.budgetMaster.GL = x.GLGeneralInfoCode + " - " + x.GLGeneralInfoName;
    };

    $scope.activityList = [];
    $scope.activitySearchList = [
        {
            name: "Code",
            value: "Code"
        },
        {
            name: "Short Name",
            value: "ShortName"
        },
        {
            name: "Standard Name",
            value: "StandardName"
        },
        {
            name: "User Name",
            value: "UserName"
        }
    ];

    $scope.activityParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "UserName",
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.ShowActivityList = function () {
        if ($scope.budgetMaster.COAId === null) {
            return ShowResult("Please at first select COA......", "failure");
        }
        baseService.setCurrentPage("activityList");
        $scope.getActivityData = function (pageno) {
            $scope.activityParameters.budgetMasterId = $scope.budgetMaster.Id;
            baseService.paginationBase("Accounts/companygroupactivity/GetForBudgetMasterPopUp", pageno, $scope.activityParameters)
                .then(function (result) {
                    $scope.activityList = result.Rows;
                    $scope.activityParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#activityPopUp")).modal("show");
        $scope.getActivityData();
    };

    $scope.closeActivityPopUp = function () {
        angular.forEach($scope.activityList, function (item) {
            if (item.Flag) {
                $scope.budgetActivityList.push(
                    {
                        Id: null,
                        Code: item.Code,
                        ShortName: item.ShortName,
                        StandardName: item.StandardName,
                        UserName: item.UserName,
                        FALinked: item.FALinked,
                        Archive: false,
                        ActivityId: item.Id,
                        Active: true,
                        IsSpecific: item.IsSpecific
                    }
                );
            }
        });
        angular.element(document.querySelector("#activityPopUp")).modal("hide");
        if ($scope.budgetActivityList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };

    $scope.activityAction = "Add";
    $scope.showActivityAdd = function () {
        if ($scope.budgetMaster.COAId === null) {
            return ShowResult("Please at first select COA.", "failure");
        }
        $scope.activityAction = "Add";
        angular.element(document.querySelector("#activityAddPopUp")).modal("show");
    };

    $scope.activity = {
        Id: null,
        Sequence: 1,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        ActivityType: null,
        ActivityId: null,
        FALinked: null,
        Description: null,
        Remarks: null,
        Active: true
    };

    $scope.addActivity = function () {
        if ($scope.activityAction === "Add") {
            $scope.activity.IsSpecific = true;
            $scope.activity.BudgetMasterActivityFixedAssetViewModel = [];
            $scope.budgetActivityList.push($scope.activity);
            $scope.clearActivity();
            angular.element(document.querySelector("#activityAddPopUp")).modal("hide");
        }
        else if ($scope.activityAction === "Update") {
            $http({
                method: "POST",
                url: "Accounts/Activity/UpdateSpecial",
                data: {
                    "activityVM": $scope.activity,
                    "budgetMasterId": $scope.budgetMaster.Id
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure", "activityAddPopUp");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.clearActivity();
                    angular.element(document.querySelector("#activityAddPopUp")).modal("hide");
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure", "activityAddPopUp");
            });
        }
    };

    $scope.closeActivityAddPopUp = function () {
        $scope.clearActivity();
        angular.element(document.querySelector("#activityAddPopUp")).modal("hide");
    };

    // #region Activity
    $scope.budgetActivityList = [];
    $scope.budgetActivityFixedAssetList = [];
    $scope.getBudgetActivity = function () {
        $http({
            method: "GET",
            url: "accounts/budgetmaster/getbudgetactivitylist?budgetMasterId=" + $scope.budgetMaster.Id
        }).then(function successCallback(response) {
            $scope.budgetActivityList = response.data;
            $scope.budgetActivityFixedAssetList = [];
            $scope.getFAMasterLinkList();
        });
    };

    $scope.getFAMasterLinkList = function () {
        $http({
            method: "GET",
            url: "accounts/budgetmaster/GetFAMasterLinkList?budgetMasterId=" + $scope.budgetMaster.Id
        }).then(function successCallback(response) {
            $scope.budgetActivityFixedAssetList = response.data;
            for (var t = 0; t < baseService.arrayLength($scope.budgetActivityList); t++) {
                var parentRow = $scope.budgetActivityList[t];
                if (parentRow.FALinked === "Master") {
                    parentRow.BudgetMasterActivityFixedAssetViewModel = [];
                    angular.forEach($scope.budgetActivityFixedAssetList, function (item) {
                        if (parentRow.ActivityId === item.ActivityId)
                            parentRow.BudgetMasterActivityFixedAssetViewModel.push(item);
                    });
                }
            }
            $scope.getFARegisterLinkList();
        });
    };

    $scope.getFARegisterLinkList = function () {
        $http({
            method: "GET",
            url: "accounts/budgetmaster/GetFARegisterLinkList?budgetMasterId=" + $scope.budgetMaster.Id
        }).then(function successCallback(response) {
            var budgetActivityFixedAssetRegisterList = response.data;
            for (var t = 0; t < baseService.arrayLength($scope.budgetActivityList); t++) {
                var parentRow = $scope.budgetActivityList[t];
                if (parentRow.FALinked === "Register") {
                    parentRow.BudgetMasterActivityFixedAssetViewModel = [];
                    angular.forEach(budgetActivityFixedAssetRegisterList, function (item) {
                        if (parentRow.ActivityId === item.ActivityId)
                            parentRow.BudgetMasterActivityFixedAssetViewModel.push(item);
                    });
                }
            }
        });
    };

    $scope.activityEdit = function (index) {
        $scope.activity = $scope.budgetActivityList[index];
        $scope.activityAction = "Update";
        angular.element(document.querySelector("#activityAddPopUp")).modal("show");
    };

    $scope.clearActivity = function () {
        $scope.activity = {};
        $scope.activity.Active = true;
        $scope.activity.Sequence = 1;
        $scope.activityAction = "Add";
    };

    $scope.getEditRow = function (index) {
        $scope.editIndex = index;
        $scope.budgetActivity = $scope.budgetActivityList[$scope.editIndex];
        $scope.CAction = "Update";
    };

    $scope._activityIndex = -1;

    $scope.activityRemoveRow = function (index) {
        $scope.activityIndex = index;
        $scope.activityConfirmMessage = "Are you sure want to delete?";
        angular.element(document.querySelector("#activityDeletePopUp")).modal("show");
    };

    $scope.confirmActivityRemoveRow = function () {
        $scope.budgetActivityList.splice($scope.activityIndex, 1);
        $scope.activityIndex = null;
    };

    $scope.searchActivityByList = [
        {
            name: "Activity",
            value: "ActivityName"
        }
    ];

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.budgetMaster = $scope.budgetMasters[$scope.index];
        $scope.getBudgetActivity();
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        if ($scope.Action === "Update") {
            $http({
                method: "POST",
                url: "accounts/budgetmaster/FARegisterUpdate",
                data: {
                    "budgetmaster": $scope.budgetMaster,
                    "budgetActivities": $scope.budgetActivityList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.getData();
                    if ($scope.index > -1) {
                        $scope.budgetMasters[$scope.index] = $scope.budgetMaster;
                    }
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        var coa = $scope.budgetMaster.COAId;
        $scope.Action = "Save";
        $scope.budgetMaster = {};
        $scope.budgetMaster.COAId = coa;
        $scope.budgetMaster.Active = true;
        $scope.budgetActivityList = [];
        $scope.budgetActivity = {};
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.selectMessage = "";
    $scope.budgetMasterReport = function (COAId) {
        if (COAId === null) {
            $scope.selectMessage = "Select COA";
        }
        else {
            $scope.selectMessage = "";
            location.href = "accounts/budgetmaster/budgetmasterreport?COAId=" + COAId;
        }
    };

    //*********************** FA Register PopUp Start *************************************
    $scope.faRegisterSearchList = [];
    $scope.faRegisterDataList = [];
    $scope.faRegisterUrl = "fixedassets/fixedassetregister/GetListForBudgetMaster";
    $scope.faRegisterParameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "AssetMaster",
        searchBy: "AssetMaster",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.faRegisterPopUp = function () {
        angular.forEach($scope.activityFARegisterList, function (a) {
            $rootScope.tempList.push({
                Id: a.Id
                , FixedAssetRegisterId: a.FixedAssetRegisterId
                , SerialNo: a.SerialNo
                , AssetNo: a.AssetNo
                , InvoiceNo: a.InvoiceNo
                , MaterialMaster: a.MaterialMaster
                , AssetMaster: a.AssetMaster
                , ActivityId: $scope.activityId
                , AssetCategory: a.AssetCategory
                , AssetSubCategory: a.AssetSubCategory
                , AssetType: a.AssetType
            });
        });
        $scope.getFARegisterData = function (pageno) {
            $scope.faRegisterParameters.ids = baseService.getColumnValueList($scope.activityFARegisterList, "FixedAssetRegisterId");
            baseService.paginationBase($scope.faRegisterUrl, pageno, $scope.faRegisterParameters)
                .then(function (response) {
                    $scope.faRegisterDataList = response.Rows;
                    $scope.faRegisterParameters.total_count = response.Total;
                    if (baseService.arrayLength($scope.faRegisterSearchList) === 0)
                        baseService.getDDLSearchColumn(response.Rows, $scope.faRegisterSearchList);
                    for (var t = 0; t < baseService.arrayLength($scope.faRegisterDataList); t++) {
                        $scope.faRegisterDataList[t].Flag = baseService.valueCheckInList($rootScope.tempList, "FixedAssetRegisterId", $scope.faRegisterDataList[t].FixedAssetRegisterId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
            angular.element(document.querySelector("#faRegisterPopUp")).modal("show");
        };
        $scope.getFARegisterData();
    };

    $scope.closeFARegisterPopUp = function () {
        $rootScope.tempList = [];
        angular.element(document.querySelector("#faRegisterPopUp")).modal("hide");
    };

    $scope.selectFARegisterPopUp = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.activityFARegisterList, "FixedAssetRegisterId", a.FixedAssetRegisterId)) {
                    var data = {
                        FixedAssetRegisterId: a.FixedAssetRegisterId
                        , SerialNo: a.SerialNo
                        , AssetNo: a.AssetNo
                        , InvoiceNo: a.InvoiceNo
                        , AssetMaster: a.AssetMaster
                        , MaterialMaster: a.MaterialMaster
                        , AssetCategory: a.AssetCategory
                        , AssetSubCategory: a.AssetSubCategory
                        , ActivityId: $scope.activityId
                        , AssetType: a.AssetType
                    };
                    if (!baseService.isUndefinedOrNull(a.Id))
                        data.Id = a.Id;
                    if (!baseService.isUndefinedOrNull(a.ActivityId))
                        data.ActivityId = a.ActivityId;
                    $scope.activityFARegisterList.push(data);
                }
            });
        }
        else
            $scope.activityFARegisterList = [];
        angular.forEach($scope.activityFARegisterList, function (a) {
            if (!baseService.valueCheckInList($rootScope.tempList, "FixedAssetRegisterId", a.FixedAssetRegisterId))
                $scope.activityFARegisterList.splice(a, 1);
        });
        $scope.closeFARegisterPopUp();
    };

    $scope.clearFARegister = function () {
        $scope.budgetActivityList[$scope.faIndex].FixedAssetRegisterId = null;
        $scope.budgetActivityList[$scope.faIndex].FixedAssetName = null;
    };

    //*********************** FA Register PopUp End *************************************
    $scope.activityFAMasterList = [];
    $scope.faPopUpShow = function (data, index) {
        $scope.faIndex = index;
        $scope.activityId = data.ActivityId;
        if (data.FALinked === "Master") {
            $scope.activityFAMasterList = data.BudgetMasterActivityFixedAssetViewModel;
            angular.element(document.querySelector("#activityFAMasterPopUp")).modal("show");
        }
        else if (data.FALinked === "Register") {
            $scope.activityFARegisterList = data.BudgetMasterActivityFixedAssetViewModel;
            angular.element(document.querySelector("#activityFARegisterPopUp")).modal("show");
        }
    };

    $scope.confirmFARegisterRemoveRow = function (index) {
        $scope.faRegisterIndex = index;
        $scope.activityConfirmMessage = "Are you sure want to delete?";
        angular.element(document.querySelector("#faRegisterDeletePopUp")).modal("show");
    };

    $scope.removeFaRegisterRow = function () {
        $scope.activityFARegisterList.splice($scope.faRegisterIndex, 1);
    };

    $scope.faPopUpClose = function () {
        $scope.faIndex = -1;
        $scope.activityId = null;
        $scope.activityFAMasterList = [];
        $scope.activityFARegisterList = [];
        angular.element(document.querySelector("#activityFAMasterPopUp")).modal("hide");
        angular.element(document.querySelector("#activityFARegisterPopUp")).modal("hide");
    };
}