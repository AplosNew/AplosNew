"use strict";
bankChargeTypeGLController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http"];
function bankChargeTypeGLController(cboService, commonMessage, $scope, $rootScope, baseService, $http) {
    $rootScope.title = "Bank Charge Type GL";
    $scope.Action = "Save";
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.receiveDeductionGivenGLList = [];
    $scope.receiveDeductionGivenGLWithCombineList = [];
    $scope.path = "Banks/BankChargeType/";
    $scope.saveUrl = $scope.path + "SaveBankChargeTypeGL";
    $scope.deleteUrl = $scope.path + "DeleteBankChargeTypeGL";

    $scope.investmentTypeGivenGL = {
        Id: null,
        CountryId: null,
        InvestmentTypeGivenId: null,
        AssetGLId: null,
        AssetBudgetMasterId: null,
        AssetActivityId: null,
        RevenueGLId: null,
        RevenueBudgetMasterId: null,
        RevenueActivityId: null,
        COAId: null,
        InvestmentTypeTakenId: null,
        ExpensesGLId: null,
        ExpensesBudgetMasterId: null,
        ExpensesActivityId: null,
        LiabilityGLId: null,
        LiabilityBudgetMasterId: null,
        LiabilityActivityId: null
    };

    $scope.itemSearchPopup = function () {
        angular.element(document.querySelector("#itemsearchpopup")).modal("show");
    };

    $scope.investmentTypeGivenList = [];
    $scope.COAList = [];
    cboService.getCboChartOfAccount("", function (result) {
        $scope.COAList = result;
    });

    $scope.tempList = [];
    $scope.selectChValueId = function (event, data) {
        if (event.currentTarget.checked) {
            $scope.tempList.push(data);
        }
        else {
            for (var i = 0; i < $scope.tempList.length; i++) {
                if ($scope.tempList[i].InvestmentTypeGivenId === data.InvestmentTypeGivenId) {
                    $scope.tempList.splice(i, 1);
                }
                break;
            }
        }
    };

    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return true;
            }
        }
        return false;
    }

    $scope.showAll = function (str) {
        if (str === "all") {
            if ($scope.investmentTypeGivenGL.COAId === null) {
                return ShowResult("Select COA first", "failure");
            }
            $scope.url = "Banks/BankChargeType/GetBankChargeTypeGLAllList?coaId=" + $scope.investmentTypeGivenGL.COAId;
        }
        if (str === "notassing") {
            if ($scope.investmentTypeGivenGL.COAId === null) {
                return ShowResult("Select COA first", "failure");
            }
            $scope.url = "Banks/BankChargeType/GetBankChargeTypeGLNotAssingList?coaId=" + $scope.investmentTypeGivenGL.COAId;
        }
        if (str === "assing") {
            if ($scope.investmentTypeGivenGL.COAId === null) {
                return ShowResult("Select COA first", "failure");
            }
            $scope.url = "Banks/BankChargeType/GetBankChargeTypeGLAssingList?coaId=" + $scope.investmentTypeGivenGL.COAId;
        }
        $scope.investmentTypeGivenGLWithCombineList = [];
        baseService.init($scope.url, null, null, null, "AssetUserName", "AssetUserName");
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.investmentTypeGivenGLWithCombineList = result.Rows;
                    for (var i = 0; i < $scope.investmentTypeGivenGLWithCombineList.length; i++) {
                        $scope.investmentTypeGivenGLWithCombineList[i].Flag = getActive($scope.tempList, $scope.investmentTypeGivenGLWithCombineList[i].InvestmentTypeGivenId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.searchExpensesTypeByList = [
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "GL",
            "value": "GLGeneralInfoName"
        }
    ];

    $scope.expensesTypeListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "AccountGroupName, GLGeneralInfoName",
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getExpensesTypeList = function () {
        if ($scope.investmentTypeGivenGL.COAId === null || $scope.investmentTypeGivenGL.COAId === undefined) {
            return ShowResult("Select COA first", "failure");
        }
        $scope.GLUrl1 = "accounts/glitem/GetExpenseGLCOAWise?coaId=" + $scope.investmentTypeGivenGL.COAId;
        $scope.getExpensesTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.expensesTypeListParameters)
                .then(function (data) {
                    $scope.expensesTypeGLList = data.Rows;
                    $scope.expensesTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#expensesTypeListPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.getExpensesTypeListData();
    };

    $scope.closeExpensesTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#expensesTypeListPopUp")).modal("hide");
        }
    };

    $scope.setExpensesGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.ExpensesGLSelectedData = x;
        $scope.ExpensesGLInof = x.GLGeneralInfoCode + " - " + x.GLGeneralInfoName;
        $scope.investmentTypeGivenGL.ExpensesGLId = x.GLGeneralInfoId;
        getExpensesBudget();
    };

    $scope.refreshExpensesGL = function () {
        $scope.ExpensesGLInof = null;
        $scope.investmentTypeGivenGL.GLGeneralInfoId = null;
        $scope.expensesBudgetList = [];
        $scope.expensesActivityList = [];
        $scope.investmentTypeGivenGL.ExpensesBudgetMasterId = null;
        $scope.investmentTypeGivenGL.ExpensesActivityId = null;
    };

    $scope.expensesBudgetList = [];
    function getExpensesBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.investmentTypeGivenGL.COAId, $scope.investmentTypeGivenGL.ExpensesGLId, function (result) {
            $scope.expensesBudgetList = result;
        });
    }

    $scope.expensesActivityList = [];
    $scope.getExpensesActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.investmentTypeGivenGL.ExpensesBudgetMasterId, function (result) {
            $scope.expensesActivityList = result;
        });
    };

    $scope.glUntagId = null;
    $scope.glUntagIndex = -1;
    $scope.valuePassInDelModal = function (data, index, event) {
        $scope.glUntagId = data.Id;
        $scope.glUntagIndex = index;
        $scope.message_confirmation = "Are you sure want to untag GL on [ " + data.InvestmentTypeGivenName + " ]?";
        angular.element(document.querySelector("#glUntag")).modal("show");
    };
    $scope.removeRow = function () {
        for (var i = 0; i < $scope.investmentTypeGivenGLWithCombineList.length; i++) {
            if ($scope.glUntagId !== null) {
                if ($scope.investmentTypeGivenGLWithCombineList[i].Id === $scope.glUntagId) {
                    $scope.unTagGL($scope.glUntagId, i);
                    break;
                }
            } else {
                unTagFromList($scope.glUntagIndex);
                $scope.glUntagIndex = -1;
                break;
            }
        }
        $scope.mauid = null;
        $scope.mauindex = -1;
    };

    function unTagFromList(i) {
        $scope.investmentTypeGivenGLWithCombineList[i] = {
            AssetUserName: $scope.investmentTypeGivenGLWithCombineList[i].AssetUserName,
            COAId: $scope.investmentTypeGivenGLWithCombineList[i].COAId,
            COAName: $scope.investmentTypeGivenGLWithCombineList[i].COAName,
            Code: $scope.investmentTypeGivenGLWithCombineList[i].Code,
            FinancingTypeId: $scope.investmentTypeGivenGLWithCombineList[i].FinancingTypeId
        };
    }

    $scope.unTagGL = function (id, index) {
        try {
            $http({
                method: "POST",
                url: $scope.deleteUrl,
                dataType: "JSON",
                data: { "id": id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    for (var i = 0; i < $scope.tempList.length; i++) {
                        if ($scope.tempList[i].Id === id) {
                            document.getElementById($scope.tempList[i].FinancingTypeId).checked = false;
                            $scope.tempList.splice(i, 1);
                            break;
                        }
                    }
                    unTagFromList(index);
                    $scope.glUntagIndex = -1;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        } catch (e) {
            ShowResult(e, "Error");
        }
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.addGlForSelectble = function () {
        $scope.investmentTypeGivenGLListForSave = [];
        angular.forEach($scope.tempList, function (item) {
            if (item.Flag) {
                if ($scope.investmentTypeGivenGL.ExpensesGLId !== null) {
                    item.ExpensesGLId = $scope.investmentTypeGivenGL.ExpensesGLId;
                }
                if ($scope.investmentTypeGivenGL.ExpensesActivityId !== null) {
                    item.ExpensesActivityId = $scope.investmentTypeGivenGL.ExpensesActivityId;
                }
                if ($scope.investmentTypeGivenGL.ExpensesBudgetMasterId !== null) {
                    item.ExpensesBudgetMasterId = $scope.investmentTypeGivenGL.ExpensesBudgetMasterId;
                }
                item.COAId = $scope.investmentTypeGivenGL.COAId;
                $scope.investmentTypeGivenGLListForSave.push(item);
            }
        });
    };

    $scope.Save = function () {
        $scope.addGlForSelectble();
        if ($scope.investmentTypeGivenGLListForSave.length < 1) {
            return ShowResult("Please select Investment Type Given!", "failure");
        }
        if (baseService.isUndefinedOrNull($scope.investmentTypeGivenGL.ExpensesGLId)) {
            return ShowResult("Please select Expense GL!", "failure");
        }
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.investmentTypeGivenGLForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: {
                        "financingTypeGLList": $scope.investmentTypeGivenGLListForSave
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, "failure");
                };
            }
        }
    };

    $scope.btnSet = "";
    $scope.setActiveBtn = function (str) {
        $scope.btnSet = str;
    };

    $scope.getAllWithCoa = function () {
        if ($scope.btnSet !== "") {
            if ($scope.btnSet === "all") {
                $scope.getInvestmentTypeGivenWithCoa("all");
            }
        } else {
            $scope.getInvestmentTypeGivenWithCoa("all");
        }
    };

    $scope.clearGlField = function () {
        $scope.tempList = [];
    };

    $scope.Clear = function () {
        ClearFields();
        $scope.refreshExpensesGL();
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.investmentTypeGivenGL = { COAId: $scope.investmentTypeGivenGL.COAId };
        $scope.tempList = [];
        $scope.showAll("all");
        $scope.clearGlField();
        $scope.investmentTypeGivenGLWithCombineList = [];
    }
}