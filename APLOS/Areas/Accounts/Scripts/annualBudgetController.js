"use strict";
annualBudgetController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "cboService"];
function annualBudgetController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Annual Budget";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.routinebudgets = [];
    $scope.annualBudgetDetailList = [];
    $scope.TotalActualAmount = 0;
    $scope.averageActualAmount = 0;
    $scope.TotalStandardAmount = 0;
    $scope.averageStandardAmount = 0;
    $scope.path = "accounts/AnnualBudget/";
    $scope.getListUrl = $scope.path + "getlist";
    $scope.getUrl = $scope.path + "get";
    $scope.getSeqUrl = $scope.path + "getautosequence";
    $scope.saveUrl = $scope.path + "create";
    $scope.updateUrl = $scope.path + "edit";
    $scope.deleteUrl = $scope.path + "delete/";
    $scope.CAction = "Add";
    //baseService.init($scope.getListUrl, null, null, null, "BudgetName", "BudgetName");
    //$scope.getData = function (pageno) {
    //    baseService.init($scope.getListUrl, null, null, null, "BudgetName", "BudgetName");
    //    $rootScope.parameters.entityId = $scope.routineBudget.EntityId;
    //    $rootScope.parameters.fiscalYearId = $scope.routineBudget.FiscalYearId;
    //    $rootScope.parameters.budgetMasterId = $scope.routineBudget.BudgetMasterId;
    //    baseService.pagination(pageno)
    //        .then(function (result) {
    //            $scope.routinebudgets = result.Rows;
    //        }, function () {
    //            ShowResult(commonMessage.NetworkError, "failure");
    //        }).finally(function () {
    //        });
    //};

    //$rootScope.searchByList.push(
    //    {
    //        "name": "Budget",
    //        "value": "BudgetName"
    //    },
    //    {
    //        "name": "Narration",
    //        "value": "Narration"
    //    });

    $scope.routineBudgetNew = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        FiscalYearId: null,
        EntityId: null,
        BudgetMasterId: null,
        CurrencyId: null,
        PositionId: null,
        ManpowerBudgetId: null,
        EmployeeId: null,
        ResponsiblePersonBy: "Employee",
        ResponsiblePerson: null,
        ApprovalCategory: null,
        BudgetLimit: "Limited",
        Amount: null,
        VariationPercent: 0,
        Remarks: null,
        Active: true,
        Archive: false
    };
    $scope.routineBudget = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null,
        FiscalYearId: null,
        EntityId: null,
        BudgetMasterId: null,
        CurrencyId: null,
        PositionId: null,
        ManpowerBudgetId: null,
        EmployeeId: null,
        ResponsiblePersonBy: "Employee",
        ResponsiblePerson: null,
        ApprovalCategory: null,
        BudgetLimit: "Limited",
        Amount: null,
        VariationPercent: 0,
        Remarks: null,
        Active: true,
        Archive: false
    };
    //$scope.routineBudget = Object.assign({}, $scope.routineBudgetNew);
    $scope.routineBudgetDetail = {
        Id: null,
        BudgetActivityId: null,
        RoutineBudgetId: null,
        ActivityType: null,
        ActivityId: null,
        FixedAssetMasterId: null,
        FixedAssetMasterName: null,
        FixedAssetRegisterId: null,
        FixedAssetRegisterName: null,
        IsFAMasterBased: false,
        IsFARegisterBased: false,
        Amount: 0,
        FALinked: null
    };
    $scope.routineBudgetOtherHead = {
        Id: null,
        RoutineBudgetId: null,
        BudgetMasterId: null,
        BudgetMaster: null,
        Percentage: null
    };
    $scope.routineBudgetOtherHeadNew = Object.assign({}, $scope.routineBudgetOtherHead);

    // #region ReturnToRequiredTab
    function reDirectToRequiredTab() {
        if ($scope.routineBudget1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.routineBudget2.$invalid) {
            $scope.setTab(2);
        }
        else if ($scope.routineBudget3.$invalid) {
            $scope.setTab(3);
        }
    }
    // #endregion

    $scope.entityList = [];
    $scope.budgetMasterList = [];
    $scope.currencyList = [];
    $scope.bankMasterList = [];

    $scope.getCboFiscalYear = function (entityId) {
        cboService.getCboFiscalYear(null, entityId, function (result) {
            $scope.fiscalYearList = result;
        });
    };
    $scope.fiscalYearList = [];
    $scope.getfiscalYearList = function () {
        $http({
            method: "GET",
            url: "accounts/companyfiscalyear/getfiscalyearbyentity?entityId=" + $scope.routineBudget.EntityId
        }).then(function successCallback(response) {
            $scope.fiscalYearList = response.data;
        });
    };

    $scope.getannualBudgetList = function (budgetMasterId, entityId, fiscalYearId) {

        $http({
            method: "GET",
            url: "accounts/AnnualBudget/getlist?entityId=" + entityId + '&fiscalYearId=' + fiscalYearId + '&budgetMasterId=' + budgetMasterId
        }).then(function successCallback(response) {
            $scope.routinebudgets = response.data;
            if ($scope.routinebudgets.length) {
                $scope.routineBudget = {
                    BudgetMasterId: $scope.routineBudget.BudgetMasterId, EntityId: $scope.routineBudget.EntityId, FiscalYearId: $scope.routineBudget.FiscalYearId
                    , BudgetLimit: "Limited", Active: true, Archive: false, BudgetName: $scope.routineBudget.BudgetName
                };
                $scope.routineBudget.Id = $scope.routinebudgets[0].Id;
                $scope.routineBudget.Amount = $scope.routinebudgets[0].Amount;
                $scope.routineBudget.ResponsiblePersonBy = $scope.routinebudgets[0].ResponsiblePersonBy;
                $scope.routineBudget.ResponsiblePerson = $scope.routinebudgets[0].ResponsiblePerson;
                $scope.routineBudget.ApprovalCategory = $scope.routinebudgets[0].ApprovalCategory;
                $scope.routineBudget.BudgetLimit = $scope.routinebudgets[0].BudgetLimit;
                $scope.routineBudget.CurrencyId = $scope.routinebudgets[0].CurrencyId;
                $scope.routineBudget.VariationPercent = $scope.routinebudgets[0].VariationPercent;
                $scope.routineBudget.AddedBy = $scope.routinebudgets[0].AddedBy;
                $scope.routineBudget.AddedDate = $scope.routinebudgets[0].AddedDate;
                $scope.routineBudget.AddedFromIP = $scope.routinebudgets[0].AddedFromIP;
                $scope.routineBudget.CompanyGroupId = $scope.routinebudgets[0].CompanyGroupId;
                $scope.routineBudget.CompanyId = $scope.routinebudgets[0].CompanyId;
                $scope.routineBudget.PlantId = $scope.routinebudgets[0].PlantId;
                $scope.Action = "Update";
                $scope.getActivityList();
                $scope.getBudgetOtherHead();
                $scope.getResponsiblePerson();
                $scope.getActivityDetailList($scope.routineBudget.BudgetMasterId);
                $scope.getannualBudgetDetailList($scope.routineBudget.BudgetMasterId, $scope.routineBudget.EntityId, $scope.routineBudget.FiscalYearId);

            }
            else {
                $scope.routineBudget.Id = null;
                $scope.routineBudget.Amount = null;
                $scope.routineBudget.ResponsiblePersonBy = "Employee";
                $scope.routineBudget.ResponsiblePerson = null;
                $scope.routineBudget.ApprovalCategory = null;
                $scope.routineBudget.BudgetLimit = "Limited";
                $scope.routineBudget.CurrencyId = null;
                $scope.routineBudget.VariationPercent = 0;
                $scope.Action = "Save";
            }

        });
    };

    $scope.getannualBudgetDetailList = function (budgetMasterId, entityId, fiscalYearId) {
        $scope.annualBudgetDetailList = [];
        $http({
            method: "GET",
            url: "accounts/AnnualBudget/GetAnnualBudgetDetailList?budgetMasterId=" + budgetMasterId + '&entityId=' + entityId + '&fiscalYearId=' + fiscalYearId
        }).then(function successCallback(response) {
            $scope.annualBudgetDetailList = response.data;

            $scope.TotalActualAmount = $filter("sumByKey")($filter("filter")($scope.annualBudgetDetailList), "ActualAmount");
            $scope.averageActualAmount = $scope.TotalActualAmount / 12
            $scope.TotalStandardAmount = $filter("sumByKey")($filter("filter")($scope.annualBudgetDetailList), "StandardAmount");
            $scope.averageStandardAmount = $scope.TotalActualAmount / 12
        });
    };


    cboService.getCompanyGroupCurrencyCbo(null, function (result) {
        $scope.currencyList = result;
    });

    $scope.GetBudgetActivityList = function () {
        $http({
            method: "GET",
            url: $scope.path + "getbudgetactivitylist?budgetMasterId=" + $scope.routineBudget.Id
        }).then(function successCallback(response) {
            $scope.activityDataList = response.data;
        });
    };

    cboService.getCboEntityByPlant(null, null, '', function (result) {
        $scope.entityList = result;
    });

    cboService.getCboBudgetMasterForSetup(function (result) {
        $scope.budgetMasterList = result;
    });

    $scope.onActivityChange = function (activityId) {
        var activity = $.grep($scope.activityDataList, function (rg) {
            return rg.Value === activityId;
        })[0];
        $scope.routineBudgetDetail.ActivityType = activity.ActivityType;
        $scope.routineBudgetDetail.IsFABased = activity.IsFABased;
        $scope.routineBudgetDetail.FALinked = activity.FALinked;
        $scope.routineBudgetDetail.BudgetActivityId = activity.BudgetActivityId;
    };

    $scope.getCboProductionEntityByCompany = function (companyId) {
        cboService.getCboProductionEntityByCompany(null, companyId, function (result) {
            $scope.entityList = result;
        });
    };



    //*************************Responsible Person******************

    $scope.showEmployeeInformationModal = function () {
        getEmployeeInformationData();
        angular.element(document.querySelector("#employeepopup")).modal("show");
    };
    function getEmployeeInformationData() {
        if (baseService.isUndefinedOrNull($scope.routineBudget.EntityId)) {
            $scope.positionDataList = [];
            ShowResult("Please select entity.", "failure");
        }
        var name = $scope.routineBudget.ResponsiblePersonBy;
        $scope.popUpTitle = "";
        var popUpUrl;
        var sort;
        var searchBy;
        if (name === "Position") {
            $scope.popUpTitle = "Position Profile";
            popUpUrl = "Organizations/Position/querybyentityid?entityId=" + $scope.routineBudget.EntityId;
            sort = "UserName";
            searchBy = "UserName";
        }
        else if (name === "Budget") {
            $scope.popUpTitle = "ManPowerBudget Profile";
            popUpUrl = "Organizations/ManpowerBudget/GetListByEntity?entityId=" + $scope.routineBudget.EntityId;
            sort = "Code";
            searchBy = "Code";
        }
        else {
            $scope.popUpTitle = "Employee Profile";
            popUpUrl = "employees/employeeinformation/employeesearchbyentity?entityId=" + $scope.routineBudget.EntityId;
            sort = "EmployeeCode";
            searchBy = "FirstName";
        }
        baseService.setCurrentPage("employeeinformationData");
        baseService.init(popUpUrl, null, $scope.maxrow, null, searchBy, sort);
        $scope.loadEIData = function (pageno) {
            $rootScope.parameters.entityId = $scope.routineBudget.EntityId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.sbEmployeeInformation = [];
                    $scope.employeeinformationData = [];
                    $scope.employeeinformationData = result.Rows;
                    if (baseService.arrayLength($scope.sbEmployeeInformation) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.sbEmployeeInformation);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        }; $scope.loadEIData();
    }
    $scope.getEmployee = function (ob) {
        var name = $scope.routineBudget.ResponsiblePersonBy;
        if (name === "Position") {
            $scope.routineBudget.PositionId = ob.Id;
            $scope.routineBudget.ResponsiblePerson = ob.UserName;
            $scope.routineBudget.ManpowerBudgetId = null;
            $scope.routineBudget.EmployeeId = null;
        }
        else if (name === "Budget") {
            $scope.routineBudget.ManpowerBudgetId = ob.Id;
            $scope.routineBudget.ResponsiblePerson = ob.Code;
            $scope.routineBudget.PositionId = null;
            $scope.routineBudget.EmployeeId = null;
        }
        else {
            $scope.routineBudget.EmployeeId = ob.SystemId;
            $scope.routineBudget.ResponsiblePerson = ob.EmployeeName;
            $scope.routineBudget.PositionId = null;
            $scope.routineBudget.ManpowerBudgetId = null;
        }
        angular.element(document.querySelector("#employeepopup")).modal("hide");
    };
    //*************************End Responsible Person******************
    $scope.bankMasterList = [];

    $scope.getAprovalCategoryList = [
        {
            Value: "Other",
            Text: "Other"
        },
        {
            Value: "Self",
            Text: "Self"
        }
    ];

    //****************************************** Other Head **********************************
    $scope.OtherHeadDataList = [];
    $scope.getActivityList = function () {
        $http({
            method: "GET",
            url: "accounts/AnnualBudget/getroutinebudgetactivityList?routineBudgetId=" + $scope.routineBudget.Id
        }).then(function successCallback(response) {
            $scope.activityDataList = response.data;
        });
    };
    $scope.getBudgetOtherHead = function () {
        $http({
            method: "GET",
            url: "accounts/AnnualBudget/GetBudgetOtherHeadList?routineBudgetId=" + $scope.routineBudget.Id
        }).then(function successCallback(response) {
            $scope.OtherHeadDataList = response.data;
        });
    };

    $scope.getResponsiblePerson = function () {
        $http({
            method: "GET",
            url: "accounts/AnnualBudget/GetResponsiblePersonList?routineBudgetId=" + $scope.routineBudget.Id
        }).then(function successCallback(response) {
            $scope.responsiblePersonDetailList = response.data;
        });
    };

    $scope._otherHeadIndex = -1;
    $scope.otherHeadTbl = false;
    $scope.OtherHeadCaption = "Add Row";
    $scope.OtherHeadDataList = [];
    $scope.AddOtherHead = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.routineBudgetOtherHeadNew.BudgetMasterId)) {
                ShowResult("Please Budget Master!", "failure");
                return;
            }
            else if (baseService.isUndefinedOrNull($scope.routineBudgetOtherHeadNew.Percentage)) {
                ShowResult("Please input percentage!", "failure");
                return;
            }
            var isAvailable = false;
            var othBudgetMasterId = document.getElementById("othBudgetMasterId").options[document.getElementById("othBudgetMasterId").selectedIndex].text;
            for (var i = 0; i < $scope.OtherHeadDataList.length; i++) {
                isAvailable = listValidation($scope.OtherHeadDataList[i].BudgetMasterId, $scope.routineBudgetOtherHeadNew.BudgetMasterId, i);
                if (isAvailable)
                    throw "This Budget Master : [" + othBudgetMasterId + "] has been already taken";
            }
            angular.copy($scope.routineBudgetOtherHeadNew, $scope.routineBudgetOtherHead);
            if ($scope._otherHeadIndex === -1) {
                $scope.OtherHeadDataList.push({
                    Id: null,
                    BudgetMasterId: $scope.routineBudgetOtherHead.BudgetMasterId,
                    BudgetMaster: othBudgetMasterId,
                    Percentage: $scope.routineBudgetOtherHead.Percentage
                });
            }
            else {
                $scope.routineBudgetOtherHead.BudgetMaster = othBudgetMasterId;
                $scope.OtherHeadDataList[$scope._otherHeadIndex] = $scope.routineBudgetOtherHead;
            }
            $scope.clearOthHead();
            $scope._otherHeadIndex = -1;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.getEditBdgAmountRow = function (index) {
        $scope._otherHeadIndex = index;
        $scope.routineBudgetOtherHead = $scope.OtherHeadDataList[$scope._otherHeadIndex];
        $scope.routineBudgetOtherHeadNew = Object.assign({}, $scope.routineBudgetOtherHead);
        $scope.CAction = "Update";
    };

    $scope.bdgAmountRemoveRow = function (index) {
        $scope._otherHeadIndex = index;
        $scope.bdgConfirmMessage = "Are you sure want to delete?";
        angular.element(document.querySelector("#bdgPopUpDelete")).modal("show");
    };

    $scope.confirmBdgAmntRemoveRow = function () {
        $scope.OtherHeadDataList.splice($scope._otherHeadIndex, 1);
        $scope._otherHeadIndex = -1;
    };

    $scope.clearOthHead = function () {
        $scope.routineBudgetOtherHeadNew = {};
        $scope.routineBudgetOtherHead = {};
        $scope._otherHeadIndex = -1;
    };

    function listValidation(oldValue, newValue, index) {
        var isAvailable = false;
        if ($scope._otherHeadIndex === -1) {
            if (oldValue === newValue) {
                isAvailable = true;
                return isAvailable;
            }
        }
        else {
            if ($scope._otherHeadIndex !== index) {
                if (oldValue === newValue) {
                    isAvailable = true;
                    return isAvailable;
                }
            }
        }
        return isAvailable;
    }
    //****************************************** Other Head **********************************

    $scope.Get = function (data) {
        $scope.index = index;
        //$scope.routineBudgetNew = $scope.routinebudgets[$scope.index];
        //$scope.routineBudget = Object.assign({}, $scope.routineBudgetNew);

    };

    $scope.getActivityDetailList = function (budgetMasterId) {
        cboService.getEnumCbo("Accounts/AnnualBudget/GetResponsibleEmployeeList?budgetMasterId=" + budgetMasterId, function (result) {
            $scope.employeeList = result;
        });
    };

    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.routineBudget.BudgetMasterId)) {
            ShowResult("Please Select BudgetMaster", "failure");
            return true;
        }
        if ($scope.routineBudget.Amount != $scope.averageStandardAmount) {
            ShowResult("Standard Per Month amount and Average Standard Budget amount  not equal", "failure");
            return true;
        }
        if ($scope.routineBudget.Amount != $scope.averageActualAmount) {
            ShowResult("Standard Per Month amount and Average Monthly Budget amount is not equal", "failure");
            return true;
        }
        $scope.TotalActivityAmount = $filter("sumByKey")($filter("filter")($scope.activityDataList), "Amount");
        if ($scope.routineBudget.Amount != $scope.TotalActivityAmount) {
            ShowResult("Standard Per Month amount and Total Activity Amount is not equal", "failure");
            return true;
        }

        $scope.TotalActivityDetailAmount = $filter("sumByKey")($filter("filter")($scope.employeeList), "Amount");
        if ($scope.TotalActivityAmount < $scope.TotalActivityDetailAmount) {
            ShowResult("Employee Activity Total amount can not exceed Total Activity Amount.", "failure");
            return true;
        }
        return false;
    };


    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        reDirectToRequiredTab();
        if ($scope.routineBudgetForm.$valid && !$scope.validation()) {
            $scope.routineBudgetNew = Object.assign({}, $scope.routineBudget);
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: {
                        "annualBudget": $scope.routineBudgetNew
                        , "annualBudgetDetailList": $scope.annualBudgetDetailList
                        , "annualBudgetActivities": $scope.activityDataList
                        , "annualBudgetActivityDetailList": $scope.employeeList
                        , "budgetOtherHeads": $scope.OtherHeadDataList
                        , "responsiblePersons": $scope.responsiblePersonDetailList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getannualBudgetList($scope.routineBudget.BudgetMasterId, $scope.routineBudget.EntityId, $scope.routineBudget.FiscalYearId);
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
                        "annualBudget": $scope.routineBudgetNew
                        , "annualBudgetDetailList": $scope.annualBudgetDetailList
                        , "annualBudgetActivities": $scope.activityDataList
                        , "annualBudgetActivityDetailList": $scope.employeeList
                        , "budgetOtherHeads": $scope.OtherHeadDataList
                        , "responsiblePersons": $scope.responsiblePersonDetailList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getannualBudgetList($scope.routineBudget.BudgetMasterId, $scope.routineBudget.EntityId, $scope.routineBudget.FiscalYearId);
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
        if (!baseService.isUndefinedOrNull($scope.routineBudget.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrl + $scope.routineBudget.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.routinebudgets.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
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
        //$scope.Action = "Save";
        //$scope.routineBudget = { BudgetMasterId: $scope.routineBudget.BudgetMasterId, EntityId: $scope.routineBudget.EntityId, FiscalYearId: $scope.routineBudget.FiscalYearId, BudgetLimit: "Limited", Active: true, Archive: false, BudgetName: $scope.routineBudget.BudgetName };
       // $scope.activityDataList = [];
       // $scope.clearResponsiblePersonDetail();
       // $scope.responsiblePersonDetailList = [];
        //$scope.clearOthHead();
        //$scope.OtherHeadDataList = [];
        //$scope.annualBudgetDetailList = [];
       // $scope.activityDataList = [];
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    //********************Responsible Person********************************//
    $scope.responsiblePersonDetail = {
        Id: null,
        RoutineBudgetId: null,
        PositionId: null,
        PositionName: null,
        ManpowerBudgetId: null,
        ManpowerBudget: null,
        EmployeeId: null,
        Employee: null,
        ActivityId: null,
        ApprovalLevel: null,
        Type: null,
        Active: null
    };
    $scope.responsiblePersonDetailNew = Object.assign({}, $scope.responsiblePersonDetail);
    $scope.responsiblePersonDetailList = [];
    $scope.respIndex = -1;
    //****************************************************//

    //*********************** Recruitment Planning PopUp Start *************************************
    $scope.manpowerBudgetSearchList = [];
    $scope.manpowerBudgetDataList = [];
    $scope.manpowerBudgetSearch = [];
    $scope.manpowerBudgetParameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "Code",
        searchBy: "Id",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.manpowerBudgetPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.routineBudget.EntityId)) {
            $scope.positionDataList = [];
            ShowResult("Please select entity.", "failure");
        }
        else {
            $scope.manpowerBudgetUrl = "Organizations/ManpowerBudget/GetListByEntity?entityId=" + $scope.routineBudget.EntityId;
            $scope.getManpowerBudgetData = function (pageno) {
                baseService.paginationBase($scope.manpowerBudgetUrl, pageno, $scope.manpowerBudgetParameters)
                    .then(function (response) {
                        $scope.manpowerBudgetDataList = response.Rows;
                        $scope.manpowerBudgetParameters.total_count = response.Total;
                        if (baseService.arrayLength($scope.manpowerBudgetSearchList) === 0) {
                            $scope.manpowerBudgetSearchList.push(
                                {
                                    "Text": "Id",
                                    "Value": "Id"
                                });
                            baseService.getDDLSearchColumn($scope.manpowerBudgetDataList, $scope.manpowerBudgetSearchList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, "failure");
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector("#manpowerBudgetPopUp")).modal("show");
        }
        $scope.getManpowerBudgetData();
    };
    $scope.closeManpowerBudgetPopUp = function () {
        angular.element(document.querySelector("#manpowerBudgetPopUp")).modal("hide");
    };
    $scope.selectManpowerBudgetPopUp = function () {
        var entity = $scope.manpowerBudgetDataList[document.querySelector("#selectedManpowerBudget:checked").value];
        $scope.selectedManpowerBudgetId = entity.Id;
        $scope.responsiblePersonDetailNew.ManpowerBudgetId = $scope.selectedManpowerBudgetId;
        $scope.responsiblePersonDetailNew.ManpowerBudget = entity.Code;
        angular.element(document.querySelector("#manpowerBudgetPopUp")).modal("hide");
    };
    $scope.clearBudget = function () {
        $scope.responsiblePersonDetailNew.ManpowerBudgetId = null;
        $scope.responsiblePersonDetailNew.ManpowerBudget = null;
    };
    //*********************** Entity PopUp End *************************************

    //*********************** Position PopUp Start *************************************
    $scope.positionSearchList = [];
    $scope.positionDataList = [];
    $scope.positionSearch = [];
    $scope.positionUrl = "Organizations/Position/querybyentityid";
    $scope.positionParameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "UserName",
        searchBy: "Id",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.positionPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.routineBudget.EntityId)) {
            $scope.positionDataList = [];
            ShowResult("Please select entity.", "failure", "positionPopUp");
        }
        else {
            $scope.positionParameters.entityId = $scope.routineBudget.EntityId;
            $scope.getPositionData = function (pageno) {
                baseService.paginationBase($scope.positionUrl, pageno, $scope.positionParameters)
                    .then(function (response) {
                        $scope.positionDataList = response.Rows;
                        $scope.positionParameters.total_count = response.Total;
                        if (baseService.arrayLength($scope.positionSearchList) === 0) {
                            $scope.positionSearchList.push(
                                {
                                    "Text": "Id",
                                    "Value": "Id"
                                });
                            baseService.getDDLSearchColumn($scope.positionDataList, $scope.positionSearchList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, "failure");
                    }).finally(function () {
                    });
                angular.element(document.querySelector("#positionPopUp")).modal("show");
            };
            $scope.getPositionData();
        }
    };

    $scope.closePositionPopUp = function () {
        angular.element(document.querySelector("#positionPopUp")).modal("hide");
    };

    $scope.selectPositionPopUp = function () {
        var position = $scope.positionDataList[document.querySelector("#selectPosition:checked").value];
        $scope.selectedPositionId = position.Id;
        $scope.responsiblePersonDetailNew.PositionId = $scope.selectedPositionId;
        $scope.responsiblePersonDetailNew.PositionName = position.UserName;
        $scope.closePositionPopUp();
    };

    $scope.clearPosition = function () {
        $scope.responsiblePersonDetailNew.PositionId = null;
        $scope.responsiblePersonDetailNew.PositionName = null;
    };
    //*********************** Position PopUp End *************************************

    $scope.addResponsiblePersonDetail = function () {
        $scope.responsiblePersonDetail = Object.assign({}, $scope.responsiblePersonDetailNew);
        if ($scope.respIndex === -1) {
            $scope.responsiblePersonDetailList.push({
                Id: null,
                RoutineBudgetId: $scope.routineBudget.Id,
                PositionId: $scope.responsiblePersonDetail.PositionId,
                PositionName: $scope.responsiblePersonDetail.PositionName,
                ManpowerBudgetId: $scope.responsiblePersonDetail.ManpowerBudgetId,
                ManpowerBudget: $scope.responsiblePersonDetail.ManpowerBudget,
                EmployeeId: $scope.responsiblePersonDetail.EmployeeId,
                Employee: $scope.responsiblePersonDetail.Employee,
                ActivityId: null,
                ApprovalLevel: null,
                Type: "BudgetMaster",
                Active: true
            });
        }
        else {
            $scope.responsiblePersonDetailList[$scope.respIndex] = $scope.responsiblePersonDetail;
        }
        $scope.clearResponsiblePersonDetail();
        $scope.respIndex = -1;
    };

    $scope.respEditRow = function (index) {
        $scope.respIndex = index;
        $scope.responsiblePersonDetail = $scope.responsiblePersonDetailList[$scope.respIndex];
        $scope.responsiblePersonDetailNew = Object.assign({}, $scope.responsiblePersonDetail);
    };

    $scope.clearResponsiblePersonDetail = function () {
        $scope.responsiblePersonDetailNew = {};
        $scope.responsiblePersonDetail = {};
        $scope.respIndex = -1;
    };

    $scope.respRemoveRow = function (index) {
        $scope.respIndex = index;
        $scope.respConfirmMessage = "Are you sure want to delete?";
        angular.element(document.querySelector("#respPopUpDelete")).modal("show");
    };

    $scope.removeResponsiblePersonDetail = function () {
        $scope.responsiblePersonDetailList.splice($scope.respIndex, 1);
        $scope.respIndex = -1;
    };

    $scope.employeePopUpList = [];
    $scope.showEmployeePopUpList = function (activityId) {
        $scope.activityId = activityId;
        //$scope.employeePopUpList = $filter("filter")($scope.employeeList, { ActivityId: activityId });
        angular.element(document.querySelector("#employeeActivityPopUp")).modal("show");
    };
    $scope.searchByBudgetMasterList = [
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Budget Group",
            "value": "BudgetGroup"
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
            "name": "RefNo",
            "value": "RefNo"
        },
        {
            "name": "Level",
            "value": "MappingLevel"
        }
    ];

    $scope.budgetMasterParameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "BudgetName",
        searchBy: "BudgetName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetBudgetMasterList = function () {
        $scope.GLUrl1 = "accounts/BudgetMaster/GetBudgetMasterPopUpList";
        $scope.GetBudgetMasterListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.budgetMasterParameters)
                .then(function (result) {
                    $scope.budgetMasterList = result.Rows;
                    $scope.budgetMasterParameters.total_count = result.Total;
                },
                    function () {
                        ShowResult(commonMessage.NetworkError, "failure");
                    }).finally(function () {
                    });
        };
        angular.element(document.querySelector("#budgetMasterPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetBudgetMasterListData();
    };

    $scope.closeBudgetMasterPopUp = function () {
        angular.element(document.querySelector("#budgetMasterPopUp")).modal("hide");
    };

    $scope.closeBudgetMasterPopUpSelected = function () {
        if ($scope.setSelected !== null) {
            angular.element(document.querySelector("#budgetMasterPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };

    $scope.rowSelected = null;
    $scope.setSelected = function (x) {
        $scope.routineBudget = {};
        $scope.activityDataList = [];
       $scope.clearResponsiblePersonDetail();
        $scope.responsiblePersonDetailList = [];
        $scope.OtherHeadDataList = [];
        $scope.annualBudgetDetailList = [];
        $scope.routinebudgets = [];
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.routineBudget.BudgetCategoryId = x.BudgetCategoryId;
        $scope.routineBudget.BudgetCategory = x.BudgetCategory;
        $scope.routineBudget.BudgetSubCategoryId = x.BudgetSubCategoryId;
        $scope.routineBudget.BudgetSubCategory = x.BudgetSubCategory;
        $scope.routineBudget.BudgetId = x.BudgetId;
        $scope.routineBudget.BudgetName = x.BudgetName;
        $scope.routineBudget.GLGeneralInfoId = x.GLGeneralInfoId;
        $scope.routineBudget.GL = x.GL;
        $scope.routineBudget.BudgetMasterId = x.Id;
        $scope.getBudgetActivity();
        $scope.routineBudget.EntityId = null;
        $scope.routineBudget.FiscalYearId = null;
        $scope.selectedBudgetMasterId = x.Id;
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#budgetMasterPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };
    $scope.getBudgetActivity = function () {
        $http({
            method: "GET",
            url: "accounts/budgetmaster/getbudgetactivitylist?budgetMasterId=" + $scope.routineBudget.BudgetMasterId
        }).then(function successCallback(response) {
            $scope.activityDataList = response.data;
            $scope.getActivityDetailList($scope.routineBudget.BudgetMasterId);
        });
    };

    $scope.monthlybudgetallocation = function () {
        for (var i = 0; i < $scope.annualBudgetDetailList.length; i++) {
            $scope.annualBudgetDetailList[i].StandardAmount = $scope.routineBudget.Amount;
            $scope.annualBudgetDetailList[i].ActualAmount = $scope.routineBudget.Amount;
        }
    }
    $scope.copystandardBudgetallocation = function () {
        for (var i = 0; i < $scope.annualBudgetDetailList.length; i++) {
            $scope.annualBudgetDetailList[i].StandardAmount = $scope.routineBudget.Amount;
        }
        $scope.TotalStandardAmount = $filter("sumByKey")($filter("filter")($scope.annualBudgetDetailList), "StandardAmount");
        $scope.averageStandardAmount = $scope.TotalStandardAmount / 12
    }
    $scope.copymonthlyBudgetallocation = function () {
        for (var i = 0; i < $scope.annualBudgetDetailList.length; i++) {
            $scope.annualBudgetDetailList[i].ActualAmount = $scope.routineBudget.Amount;
        }
        $scope.TotalActualAmount = $filter("sumByKey")($filter("filter")($scope.annualBudgetDetailList), "ActualAmount");
        $scope.averageActualAmount = $scope.TotalActualAmount / 12
    }
    $scope.changemonthlyBudgetallocation = function () {
        $scope.TotalActualAmount = $filter("sumByKey")($filter("filter")($scope.annualBudgetDetailList), "ActualAmount");
        $scope.averageActualAmount = $scope.TotalActualAmount / 12
    }
    $scope.changestandardBudgetallocation = function () {
        $scope.TotalStandardAmount = $filter("sumByKey")($filter("filter")($scope.annualBudgetDetailList), "StandardAmount");
        $scope.averageStandardAmount = $scope.TotalStandardAmount / 12
    }

    $scope.limitchange = function () {
        if ($scope.routineBudget.BudgetLimit == 'Limited')
            $scope.routineBudget.VariationPercent = 0;
        if ($scope.routineBudget.BudgetLimit == 'Unlimited')
            $scope.routineBudget.VariationPercent = '';
    }
}