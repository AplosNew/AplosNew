"use strict";
OverHeadTypeGLParachasController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http"];
function OverHeadTypeGLParachasController(cboService, commonMessage, $scope, $rootScope, baseService, $http) {
    $rootScope.title = "OverHead Type GL Purchase";
    $scope.hadding = "OverHead Type GL Purchase";
    $scope.Action = "Save";
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.receiveDeductionGivenGLList = [];
    $scope.receiveDeductionGivenGLWithCombineList = [];
    $scope.path = "Banks/BankChargeType/";
    $scope.saveUrl = $scope.path + "SaveBankChargeTypeGL";
    $scope.deleteUrl = $scope.path + "DeleteBankChargeTypeGL";
    $scope.ChargesTypeGLList = [];

    $scope.ChargesTypeGL = {
        Id: null,
        COAId: null,
        ChargesTypeId: null,
        AssetGLId: null,
        AssetActivityId: null,
        RevenueGLId: '',
        RevenueActivityId: null,
        LiabilityGLId: null,
        LiabilityActivityId: null,
        ExpensesGLId: null,
        ExpensesActivityId: null,
        Remarks: null,
        //Active: false,
        AssetBudgetMasterId: null,
        RevenueBudgetMasterId: null,
        LiabilityBudgetMasterId: null,
        ExpensesBudgetMasterId: null,
        GLType: "Purchase"
    };


    $scope.itemSearchPopup = function () {
        angular.element(document.querySelector("#itemsearchpopup")).modal("show");
    };

    $scope.investmentTypeGivenList = [];
    $scope.COAList = [];
    cboService.getCboChartOfAccount("", function (result) {
        $scope.COAList = result;
    });

    $scope.investmentTypeGivenGLWithCombineList = [];
    $scope.tempList = [];
    $scope.tempcount = 0;
    $scope.showAll = function () {
        $scope.investmentTypeGivenGLWithCombineList = [];
        if (!baseService.isUndefinedOrNull($scope.ChargesTypeGL.COAId)) {
            $http({
                method: "GET",
                url: 'Commercial/OverHeadTypeGL/GetList?coaId=' + $scope.ChargesTypeGL.COAId + '&GLType=' + $scope.ChargesTypeGL.GLType,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    $scope.investmentTypeGivenGLWithCombineList = response.data;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, "failure");
            };
        }
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

    $scope.seclectedIndex = -1;
    $scope.data = null;
    $scope.getExpensesTypeList = function (obj) {

        var gridObj = $("#Grid").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];



        if ($scope.ChargesTypeGL.COAId === null || $scope.ChargesTypeGL.COAId === undefined) {
            return ShowResult("Select COA first", "failure");
        }
        $scope.GLUrl1 = "accounts/glitem/GetExpenseGLCOAWise?coaId=" + $scope.ChargesTypeGL.COAId;
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
    $scope.clearExpenseTypeGL = function (obj) {

        $scope.data = obj.data;
        $scope.data.ExpensesGLId = null;
        $scope.data.ExpensesGLInfo = null;
        $scope.data.BudgetList = null;
        $scope.data.ActivityList = null;

        var gridObj = $("#Grid").data("ejGrid");
        gridObj.refreshContent(true);
    }

    $scope.setExpensesGLSelected = function (x) {
        $scope.data.ExpensesGLId = x.GLGeneralInfoId;
        $scope.data.ExpensesGLInfo = x.GLGeneralInfoCode + " - " + x.GLGeneralInfoName;

        $scope.closeExpensesTypeListPopUpSelected();
        $scope.data.BudgetList = null;
        $scope.data.ActivityList = null;
        getExpensesBudget();

        var gridObj = $("#Grid").data("ejGrid");
        gridObj.refreshContent(true);
    };

    $scope.closeExpensesTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#expensesTypeListPopUp")).modal("hide");
        }
    };

    $scope.refreshExpensesGL = function () {
        $scope.ExpensesGLInfo = null;
        $scope.ChargesTypeGL.GLGeneralInfoId = null;
        $scope.expensesBudgetList = [];
        $scope.expensesActivityList = [];
        $scope.ChargesTypeGL.ExpensesBudgetMasterId = null;
        $scope.ChargesTypeGL.ExpensesActivityId = null;
    };

    $scope.expensesBudgetList = [];
    function getExpensesBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.ChargesTypeGL.COAId, $scope.data.ExpensesGLId, function (result) {

            $scope.data.ActivityList = null;
            $scope.data.BudgetList = result;
        });
    }

    var currRow = null;
    $scope.expensesActivityList = [];
    $scope.ActivityList = [];
    $scope.getExpensesActivity = function (args) {

        var gridObj = $("#Grid").ejGrid("instance");
        currRow = gridObj.model.currentViewData[this.element.closest("tr").index()];

        currRow.ActivityList = [];
        cboService.getBudgetMasterActivityCbo(currRow.ExpensesBudgetMasterId, function (result) {
            currRow.ActivityList = result;
        });
    };


    var expenseMsg = "";
    $scope.IsExpenseAmendmentChargeTypeGLValidate = function () {
        for (var i = 0; i < $scope.investmentTypeGivenGLWithCombineList.length; i++) {

            if (baseService.isUndefinedOrNull($scope.investmentTypeGivenGLWithCombineList[i].ExpensesGLId)) {
                ShowResult('GL is required', "failure");
                return true;
            }
        }
    }

    $scope.Save = function () {
        if ($scope.investmentTypeGivenGLWithCombineList.length > 0) {
            for (var i = 0; i < $scope.investmentTypeGivenGLWithCombineList.length; i++) {
                $scope.investmentTypeGivenGLWithCombineList[i].GLType = "Purchase";
            }
        }
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.investmentTypeGivenGLForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: 'Commercial/OverHeadTypeGL/create',
                    data: { dataList: $scope.investmentTypeGivenGLWithCombineList, CAId: $scope.ChargesTypeGL.COAId },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        baseService.paginationAdd();
                        $scope.showAll();
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


    $scope.Clear = function () {
        ClearFields();
        $scope.refreshExpensesGL();
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.investmentTypeGivenGL = { COAId: $scope.ChargesTypeGL.COAId };
        $scope.tempList = [];
        $scope.showAll();
        $scope.clearGlField();
        $scope.investmentTypeGivenGLWithCombineList = [];
    }

    $scope.clearGlField = function () {
        $scope.tempList = [];
    };


}