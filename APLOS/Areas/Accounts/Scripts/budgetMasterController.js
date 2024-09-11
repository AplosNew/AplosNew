"use strict";
budgetMasterController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http"];
function budgetMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $http) {
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
    baseService.init($scope.getListUrl, null, 15, null, "GLGeneralInfoName, BudgetName", "BudgetName");
    $scope.onCOAChange = function () {
        $scope.getData = function (pageno) {
            $rootScope.parameters.coaId = $scope.budgetMaster.COAId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.budgetMasters = result.Rows;
                    $scope.GetMaxRefNo();
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        $scope.getData();
        
    };

    $scope.GetMaxRefNo = function () {
        $scope.budgetMaster.RefNo = '';
        $http({
            method: "GET",
            url: "accounts/budgetmaster/GetMaxRefNo"
        }).then(function successCallback(response) {
            $scope.budgetMaster.RefNo = response.data;
        });
    };
    // Combo list

    $scope.ActivityOrderTypeList = [];
    cboService.getEnumCbo("enum/GetActivityOrderTypeEnumCbo", function (result) {
        $scope.ActivityOrderTypeList = result;
    });

    $scope.valueOfDistributionList = [];
    cboService.getEnumCbo("enum/GetValueOfDistributionEnumCbo", function (result) {
        $scope.valueOfDistributionList = result;
    });

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

    //cboService.getEnumCbo("enum/GetActivityTypeCbo", function (result) {
    //    $scope.activityTypeList = result;
    //});
    cboService.getEnumCbo("enum/GetExpensesActivityTypeCbo", function (result) {
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

    //*******************End Cbo****************************

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
        GLGeneralInfoCode: null,
        GLGeneralInfoName: null,
        IsProject: false,
        IsOrderSpecific:false,
        RefNo: null,
        DefaultTransactionStatus: null,
        TransactionParameter: null,
        PaymentVoucherType: null,
        PaymentRuleDeduction: null,
        Narration: null,
        IsNarrationEditable: null,
        IsDirectPaymentVoucher: null,
        IsCarryforward: null,
        PaymentMode: null,
        IsPartyListing: null
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
        Archive: null,
        IsDefault: null,
        IsServiceApplicable: null
        
    };

    $scope.searchByList = [
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
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
            "value": "BudgetName"
        },
        {
            "name": "Budget Type",
            "value": "BudgetType"
        },
        {
            "name": "Ref No",
            "value": "RefNo"
        }
    ];

    // #region ReturnToRequiredTab
    function reDirectToRequiredTab() {
        if ($scope.budgetMaster1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.budgetMaster2.$invalid) {
            $scope.setTab(2);
        }
        else if ($scope.budgetMaster3.$invalid) {
            $scope.setTab(3);
        }
    }
    // #endregion

    $scope.PaymentVoucherTypeList = [
        {
            Value: "Accrual",
            Text: "Accrual"
        },
        {
            Value: "Cash",
            Text: "Cash"
        }
    ];

    //************Bdg Payment**********************//
    $scope.getBudgetPaymentType = function () {
        $http({
            method: "GET",
            url: "accounts/BudgetMaster/GetBudgetPaymentTypeList?budgetMasterId=" + $scope.budgetMaster.Id
        }).then(function successCallback(response) {
            $scope.budgetPaymentTypeList = response.data;
        });
    };

    $scope.budgetPaymentType = {
        Id: null,
        BudgetMasterId: null,
        UpToMonth: null,
        PaymentAfterDays: null
    };

    $scope.budgetPaymentTypeNew = Object.assign({}, $scope.budgetPaymentType);
    $scope.bdgPaymentIndex = -1;
    $scope.budgetPaymentTypeList = [];
    $scope.addRowPaymentType = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.budgetPaymentTypeNew.UpToMonth)) {
                ShowResult("Please input up-to month!", "failure");
                return;
            }
            if (baseService.isUndefinedOrNull($scope.budgetPaymentTypeNew.PaymentAfterDays)) {
                ShowResult("Please input payment after days!", "failure");
                return;
            }
            $scope.budgetPaymentType = Object.assign({}, $scope.budgetPaymentTypeNew);
            if ($scope.bdgPaymentIndex === -1) {
                $scope.budgetPaymentTypeList.push({
                    Id: null,
                    BudgetMasterId: $scope.budgetMaster.Id,
                    UpToMonth: $scope.budgetPaymentType.UpToMonth,
                    PaymentAfterDays: $scope.budgetPaymentType.PaymentAfterDays
                });
            }
            else {
                $scope.budgetPaymentTypeList[$scope.bdgPaymentIndex] = $scope.budgetPaymentType;
            }
            $scope.clearBdgPayment();
            $scope.bdgPaymentIndex = -1;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.getEditBdgPaymentRow = function (index) {
        $scope.bdgPaymentIndex = index;
        $scope.budgetPaymentType = $scope.budgetPaymentTypeList[$scope.bdgPaymentIndex];
        $scope.budgetPaymentTypeNew = Object.assign({}, $scope.budgetPaymentType);
    };
    $scope.bdgPaymentRemoveRow = function (index) {
        $scope.bdgPaymentIndex = index;
        $scope.payConfirmMessage = "Are you sure want to delete?";
        angular.element(document.querySelector("#payPopUpDelete")).modal("show");
    };
    $scope.removePayment = function () {
        $scope.budgetPaymentTypeList.splice($scope.bdgPaymentIndex, 1);
        $scope.bdgPaymentIndex = null;
    };
    $scope.clearBdgPayment = function () {
        $scope.budgetPaymentTypeNew = {};
        $scope.budgetPaymentType = {};
    };

    //************Bdg Payment**********************//

    $scope.transactionParameterList = [
        {
            Value: "Mileage",
            Text: "Mileage"
        },
        {
            Value: "FuelConsumption",
            Text: "Fuel Consumption"
        },
        {
            Value: "MileageFuelConsumption",
            Text: "Mileage Fuel Consumption"
        }
    ];

    $scope.searchglByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL Name',
            'value': 'GLGeneralInfoName'
        },
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        }
    ];

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'GLGeneralInfoCode',
        searchBy: 'GLGeneralInfoCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.cOAICodeList = [];
    $scope.GetCOAICodeList = function () {
        $scope.GLUrl1 = "accounts/glitem/GetAllGLListSetup?coaId=" + $scope.budgetMaster.COAId;
        baseService.setCurrentPage('cOAICodeList');
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

    $scope.closeCOAICodeListPopUpSelected = function (x) {
        if ($scope.rowSelected !== null) {
            $scope.budgetMaster.GLGeneralInfoId = x.GLGeneralInfoId;
            $scope.budgetMaster.GLGeneralInfoCode = x.GLGeneralInfoCode;
            $scope.budgetMaster.GLGeneralInfoName = x.GLGeneralInfoName;
            $scope.set();
            $scope.selectedCode = x.GLGeneralInfoCode;
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
    };

    // ********************************** Activity Modal ****************************
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
                    //angular.forEach($scope.activityList, function (item) {
                    //    for (var i = 0; i < $scope.budgetActivityList.length; i++) {
                    //        if ($scope.activityList[i]["ActivityId"] === item.Id) {
                    //            $scope.activityList.splice(i, 1);
                    //        }
                    //    }
                    //});
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
                        IsSpecific: item.IsSpecific,
                        Sequence: item.Sequence,
                        IsOrderSpecific: item.IsOrderSpecific,
                        ActivityOrderType: item.ActivityOrderType,
                        IsServiceApplicable: item.IsServiceApplicable
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
    //****************************************** End Activity Modal **********************************
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
        Active: true,
        ValueOfDistribution: null,
        IsServiceApplicable: false
    };

    $scope.addActivity = function () {
        if ($scope.activity.Sequence === undefined) {
            return ShowResult("Please input Sequence.", "failure");
        }
        else if ($scope.activity.IsOrderSpecific == true && ($scope.activity.ActivityOrderType === null || $scope.activity.ActivityOrderType === undefined)) {
            return ShowResult("Please select Activity Order Type.", "failure");
        }
        else if ($scope.activityAction === "Add") {
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

    $scope.onChangeActivityOrderType = function () {
        if ($scope.activity.ActivityOrderType !=null)
            $scope.activity.ValueOfDistribution = 'Amount';
        else
            $scope.activity.ValueOfDistribution = null;

    }
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
                parentRow.BudgetMasterActivityFixedAssetViewModel = [];
                angular.forEach($scope.budgetActivityFixedAssetList, function (item, i) {
                    if (parentRow.ActivityId === item.ActivityId)
                        parentRow.BudgetMasterActivityFixedAssetViewModel.push(item);
                });
            }
            //$scope.getFARegisterLinkList();
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
                    angular.forEach(budgetActivityFixedAssetRegisterList, function (item, i) {
                        if (parentRow.ActivityId === item.ActivityId)
                            parentRow.BudgetMasterActivityFixedAssetViewModel.push(item);
                    });
                }
            }
        });
    };

    $scope.activityEdit = function (index) {
        //budgetActivityList
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
        $scope.clearActivity();
    };

    $scope._activityIndex = -1;
    $scope.activityRemoveRow = function (index) {
        $scope.activityIndex = index;
        $scope.activityConfirmMessage = "Are you sure want to delete?";
        angular.element(document.querySelector("#activityDeletePopUp")).modal("show");
    };

    $scope.confirmActivityRemoveRow = function () {
        var datarow = $scope.budgetActivityList[$scope.activityIndex];
        $scope.GetCheckUsingActivityInTransaction(datarow.ActivityId);

       
    };

    $scope.UsingActivityInTransaction =false;
    $scope.GetCheckUsingActivityInTransaction = function (id) {
        $http({
            method: "GET",
            url: "accounts/budgetmaster/CheckUsingActivityInTransaction?id="+id
        }).then(function successCallback(response) {
            $scope.UsingActivityInTransaction = response.data;
            if ($scope.UsingActivityInTransaction === "False") {
                $scope.budgetActivityList.splice($scope.activityIndex, 1);
                
            } else {
                ShowResult("This Activity used in Transaction", "failure");
            }
            $scope.activityIndex = null;
        });
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
        $scope.getBudgetPaymentType();
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        reDirectToRequiredTab();
        if ($scope.budgetMasterForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: {
                        "budgetmaster": $scope.budgetMaster,
                        "budgetActivities": $scope.budgetActivityList
                        , "budgetMasterPaymentTypeList": $scope.budgetPaymentTypeList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        //$scope.budgetMasters.push(data.BudgetMaster);
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrl,
                    data: {
                        "budgetmaster": $scope.budgetMaster,
                        "budgetActivities": $scope.budgetActivityList
                        , "budgetMasterPaymentTypeList": $scope.budgetPaymentTypeList
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
        }
        return true;
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.budgetMaster.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrl + $scope.budgetMaster.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.budgetMasters.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
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
        $scope.clearBdgPayment();
        $scope.budgetPaymentTypeList = [];
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
            location.href = "Accounts/budgetmaster/GetBudgetMasterReport?COAId=" + COAId;
        }
    };

    //*********************** FA Master PopUp Start *************************************
    $rootScope.tempList = [];
    $scope.faIndex = -1;
    $scope.faMasterSearchList = [];
    $scope.faMasterDataList = [];
    $scope.faMasterUrl = "fixedassets/fixedassetmaster/QueryAsMaterialMasterBudgetMaster";
    $scope.faMasterParameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "AssetItem",
        searchBy: "AssetItem",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.faMasterPopUp = function () {
        angular.forEach($scope.activityFAMasterList, function (a) {
            $rootScope.tempList.push({
                Id: a.Id
                , MaterialMasterId: a.MaterialMasterId
                , AssetItem: a.AssetItem
                , FixedAssetMasterId: a.FixedAssetMasterId
                , FixedAssetMasterName: a.FixedAssetMasterName
                , FixedAssetCategoryName: a.FixedAssetCategoryName
                , FixedAssetSubCategoryName: a.FixedAssetSubCategoryName
                , AssetType: a.AssetType
                , ActivityId: $scope.activityId
            });
        });
        $scope.getFAMasterData = function (pageno) {
            $scope.faMasterParameters.ids = baseService.getColumnValueList($scope.activityFAMasterList, "MaterialMasterId");
            baseService.paginationBase($scope.faMasterUrl, pageno, $scope.faMasterParameters)
                .then(function (response) {
                    $scope.faMasterDataList = response.Rows;
                    $scope.faMasterParameters.total_count = response.Total;
                    if (baseService.arrayLength($scope.faMasterSearchList) === 0) {
                        baseService.getDDLSearchColumn(response.Rows, $scope.faMasterSearchList);
                    }
                    for (var t = 0; t < baseService.arrayLength($scope.faMasterDataList); t++) {
                        $scope.faMasterDataList[t].Flag = baseService.valueCheckInList($rootScope.tempList, "MaterialMasterId", $scope.faMasterDataList[t].MaterialMasterId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
            angular.element(document.querySelector("#faMasterPopUp")).modal("show");
        };
        $scope.getFAMasterData();
    };

    $scope.closeFAMasterPopUp = function () {
        $rootScope.tempList = [];
        angular.element(document.querySelector("#faMasterPopUp")).modal("hide");
    };

    $scope.clearFAMaster = function () {
        $scope.budgetActivityList[$scope.faIndex].FixedAssetMasterId = null;
        $scope.budgetActivityList[$scope.faIndex].FixedAssetName = null;
    };
    $scope.selectFAMasterPopUp = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.activityFAMasterList, "MaterialMasterId", a.MaterialMasterId)) {
                    var data = {
                        MaterialMasterId: a.MaterialMasterId
                        , AssetItem: a.AssetItem
                        , FixedAssetMasterId: a.FixedAssetMasterId
                        , FixedAssetMasterName: a.FixedAssetMaster
                        , FixedAssetCategoryName: a.FixedAssetCategory
                        , FixedAssetSubCategoryName: a.FixedAssetSubCategory
                        , AssetType: a.AssetType
                        , MaterialTypeName: a.MaterialType
                        , MaterialGroupMasterName: a.MaterialGroupMaster
                        , Code: a.Code
                    };
                    if (!baseService.isUndefinedOrNull(a.Id)) {
                        data.Id = a.Id;
                    }
                    if (!baseService.isUndefinedOrNull(a.ActivityId)) {
                        data.ActivityId = a.ActivityId;
                    }
                    $scope.activityFAMasterList.push(data);
                }
            });
        }
        else
            $scope.activityFAMasterList = [];
        angular.forEach($scope.activityFAMasterList, function (a) {
            if (!baseService.valueCheckInList($rootScope.tempList, "MaterialMasterId", a.MaterialMasterId))
                $scope.activityFAMasterList.splice(a, 1);
        });
        $scope.closeFAMasterPopUp();
    };
    //*********************** FA Master PopUp End *************************************

    //*********************** FA Register PopUp Start *************************************
    $scope.faRegisterSearchList = [];
    $scope.faRegisterDataList = [];
    $scope.faRegisterUrl = "fixedassets/fixedassetregister/getlist";
    $scope.faRegisterParameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "FixedAssetMasterName",
        searchBy: "FixedAssetMasterName",
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
                , FixedAssetMasterName: a.FixedAssetMasterName
                , PurchasePrice: a.PurchasePrice
                , ActivityId: $scope.activityId
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
                        , FixedAssetMasterName: a.FixedAssetMasterName
                        , PurchasePrice: a.PurchasePrice
                        , ActivityId: $scope.activityId
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
        $scope.activityFAMasterList = data.BudgetMasterActivityFixedAssetViewModel;
        angular.element(document.querySelector("#activityFAMasterPopUp")).modal("show");
        //else if (data.FALinked === "Register") {
        //    $scope.activityFARegisterList = data.BudgetMasterActivityFixedAssetViewModel;
        //    angular.element(document.querySelector("#activityFARegisterPopUp")).modal("show");
        //}
    };

    $scope.removeFaMasterRow = function () {
        $scope.activityFAMasterList.splice($scope.faMasterIndex, 1);
        $scope.faMasterIndex = null;
    };

    $scope.confirmFAMasterRemoveRow = function (index) {
        $scope.faMasterIndex = index;
        $scope.activityConfirmMessage = "Are you sure want to delete?";
        angular.element(document.querySelector("#faMasterDeletePopUp")).modal("show");
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