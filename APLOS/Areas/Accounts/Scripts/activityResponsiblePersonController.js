"use strict";
activityResponsiblePersonController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "cboService"];
function activityResponsiblePersonController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Activity Responsible Person";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.routinebudgets = [];
    $scope.path = "accounts/routineBudget/";
    $scope.getListUrl = $scope.path + "getlist";
    $scope.getUrl = $scope.path + "get";
    $scope.getSeqUrl = $scope.path + "getautosequence";
    $scope.saveUrl = $scope.path + "create";
    $scope.updateUrl = $scope.path + "edit";
    $scope.deleteUrl = $scope.path + "delete/";
    baseService.populateSearchList();
    $scope.getData = function () {
        $http({
            method: "GET",
            url: "accounts/routinebudget/getroutinebudgetactivityList?routineBudgetId=" + $scope.routineBudget.Id
        }).then(function successCallback(response) {
            $scope.activityDataList = response.data;
        });
    };

    $scope.routineBudget = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        FiscalYearId: null,
        EntityId: null,
        BudgetMasterId: null
    };

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
        else if ($scope.routineBudget4.$invalid) {
            $scope.setTab(4);
        }
    }
    // #endregion
    $scope.entityList = [];
    cboService.getCboEntityByCompanyGroup(null, function (result) {
        $scope.entityList = result;
    });

    $scope.getCboFiscalYear = function (entityId) {
        cboService.getCboFiscalYear(entityId, function (result) {
            $scope.fiscalYearList = result;
        });
    };

    $scope.getCboRoutineBudgetMasterByEntityAndFY = function (entityId, fiscalYearId) {
        cboService.getCboRoutineBudgetMasterByEntityAndFY(entityId, fiscalYearId, function (result) {
            $scope.budgetMasterList = result;
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
            searchBy = "Code";
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

    // ********************************** Activity Modal ****************************

    $scope.activityList = [];
    $scope._activityIndex = -1;
    $scope.ShowActivityList = function () {
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
        $scope.getActivityData = function (pageno) {
            baseService.setCurrentPage("activityList");
            $scope.activityParameters.budgetMasterId = $scope.routineBudget.BudgetMasterId;
            baseService.paginationBase("Accounts/companygroupactivity/GetForBudgetMasterPopUp", pageno, $scope.activityParameters)
                .then(function (result) {
                    $scope.activityList = result.Rows;
                    $scope.activityParameters.total_count = result.Total;
                    angular.forEach($scope.activityList, function (item) {
                        for (var i = 0; i < $scope.activityDataList.length; i++) {
                            if ($scope.activityList[i]["ActivityId"] === item.ActivityId) {
                                $scope.activityList.splice(i, 1);
                            }
                        }
                    });
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
            angular.element(document.querySelector("#activityPopUp")).modal("show");
        };
        $scope.getActivityData();
    };
    $scope.closeActivityPopUp = function () {
        angular.forEach($scope.activityList, function (item) {
            if (item.Flag) {
                $scope.activityDataList.push(
                    {
                        Id: null,
                        Sequence: item.Sequence,
                        Code: item.Code,
                        ShortName: item.ShortName,
                        StandardName: item.StandardName,
                        UserName: item.UserName,
                        Archive: false,
                        ActivityId: item.Id,
                        FALinked: item.FALinked,
                        FixedAssetMasterId: null,
                        FixedAssetRegisterId: null,
                        FixedAssetName: null,
                        Active: true
                    }
                );
            }
        });
        angular.element(document.querySelector("#activityPopUp")).modal("hide");
    };
    $scope.activityRemoveRow = function (index) {
        $scope._activityIndex = index;
        $scope.activityConfirmMessage = "Are you sure want to delete?";
        angular.element(document.querySelector("#activityPopUpDelete")).modal("show");
    };
    $scope.confirmactivityRemoveRow = function () {
        $scope.activityDataList.splice($scope._activityIndex, 1);
        $scope._activityIndex = -1;
    };

    //****************************************** End Activity Modal **********************************

    //****************************************** Other Head **********************************
    $scope.OtherHeadDataList = [];

    $scope.getBudgetOtherHead = function () {
        $http({
            method: "GET",
            url: "accounts/routinebudget/GetBudgetOtherHeadList?routineBudgetId=" + $scope.routineBudget.Id
        }).then(function successCallback(response) {
            $scope.OtherHeadDataList = response.data;
        });
    };
    $scope.getBudgetPaymentType = function () {
        $http({
            method: "GET",
            url: "accounts/routinebudget/GetBudgetPaymentTypeList?routineBudgetId=" + $scope.routineBudget.Id
        }).then(function successCallback(response) {
            $scope.budgetPaymentTypeList = response.data;
        });
    };
    $scope.getResponsiblePerson = function () {
        $http({
            method: "GET",
            url: "accounts/routinebudget/GetResponsiblePersonList?routineBudgetId=" + $scope.routineBudget.Id
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
            var isAvailable;
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
        if ($scope._otherHeadIndex === -1) {
            if (oldValue === newValue) {
                return true;
            }
        }
        else {
            if ($scope._otherHeadIndex !== index) {
                if (oldValue === newValue) {
                    return true;
                }
            }
        }
        return false;
    }
    //****************************************** Other Head **********************************

    //************Bdg Payment**********************//

    $scope.budgetPaymentType = {
        Id: null,
        RoutineBudgetId: null,
        UpToMonth: null,
        PaymentAfterDays: null
    };

    $scope.budgetPaymentTypeNew = Object.assign({}, $scope.budgetPaymentType);
    $scope.bdgPaymentIndex = -1;
    $scope.budgetPaymentTypeList = [];
    $scope.addRowPaymentType = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.budgetPaymentTypeNew.UpToMonth)) {
                ShowResult("Please input upto month!", "failure");
                return;
            }
            if (baseService.isUndefinedOrNull($scope.budgetPaymentTypeNew.PaymentAfterDays)) {
                ShowResult("Please input payment afterdays!", "failure");
                return;
            }
            $scope.budgetPaymentType = Object.assign({}, $scope.budgetPaymentTypeNew);
            if ($scope.bdgPaymentIndex === -1) {
                $scope.budgetPaymentTypeList.push({
                    Id: null,
                    RoutineBudgetId: $scope.routineBudgetNew.Id,
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
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.routineBudgetNew = $scope.routinebudgets[$scope.index];
        $scope.routineBudget = Object.assign({}, $scope.routineBudgetNew);
        $scope.getActivityList();
        $scope.getBudgetOtherHead();
        $scope.getBudgetPaymentType();
        $scope.getResponsiblePerson();
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        reDirectToRequiredTab();
        if ($scope.routineBudgetForm.$valid) {
            $scope.routineBudgetNew = Object.assign({}, $scope.routineBudget);
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: {
                        "routineBudget": $scope.routineBudgetNew
                        , "routineBudgetActivities": $scope.activityDataList
                        , "budgetOtherHeads": $scope.OtherHeadDataList
                        , "budgetPaymentTypes": $scope.budgetPaymentTypeList
                        , "responsiblePersons": $scope.responsiblePersonDetailList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
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
                        "routineBudget": $scope.routineBudgetNew
                        , "routineBudgetActivities": $scope.activityDataList
                        , "budgetOtherHeads": $scope.OtherHeadDataList
                        , "budgetPaymentTypes": $scope.budgetPaymentTypeList
                        , "responsiblePersons": $scope.responsiblePersonDetailList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.routinebudgets[$scope.index] = $scope.routineBudgetNew;
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
        $scope.Action = "Save";
        $scope.routineBudget = { EntityId: $scope.routineBudget.EntityId, FiscalYearId: $scope.routineBudget.FiscalYearId, BudgetLimit: "Limited", Active: true, Archive: false };
        $scope.activityDataList = [];
        $scope.clearResponsiblePersonDetail();
        $scope.responsiblePersonDetailList = [];
        $scope.clearOthHead();
        $scope.OtherHeadDataList = [];
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
    $scope.positionParameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "UserName",
        searchBy: "Code",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.positionUrl = "Organizations/Position/querybyentityid";
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
}