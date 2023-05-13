"use strict";
balanceSheetReportTreeViewController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$http", "$filter", "$controller", "$window"];
function balanceSheetReportTreeViewController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $http, $filter, $controller, $window) {
    $rootScope.title = "Balance Sheet Report Tree View";
    
    $scope.Back = function () {
        $window.history.back();
    };
    $scope.GLLevelDataList = [];
    $scope.BudgetLevelDataList = [];
    $scope.ActivityLevelDataList = [];
    $scope.GetBalanceSheetTreeViewData = function () {
        $http({
            method: 'POST',
            url: 'Accounts/MISAccountDashboard/GetBalanceSheetInfoGLLevel/',
            params: { 'date': $routeParams.FromDate, 'GLGeneralInfoId': $scope.gLGeneralInfoId, 'BudgetMasterId': $scope.budgetMasterId, 'ActivityId': $scope.activityId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GLLevelDataList = response.data;

            $http({
                method: 'POST',
                url: 'Accounts/MISAccountDashboard/GetBalanceSheetInfoBudgetLevel/',
                params: { 'date': $routeParams.FromDate, 'GLGeneralInfoId': $scope.gLGeneralInfoId, 'BudgetMasterId': $scope.budgetMasterId, 'ActivityId': $scope.activityId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.BudgetLevelDataList = response.data;
                $http({
                    method: 'POST',
                    url: 'Accounts/MISAccountDashboard/GetBalanceSheetInfoActivityLevel/',
                    params: { 'date': $routeParams.FromDate, 'GLGeneralInfoId': $scope.gLGeneralInfoId, 'BudgetMasterId': $scope.budgetMasterId, 'ActivityId': $scope.activityId },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.ActivityLevelDataList = response.data;

                    $scope.loadGrid($scope.GLLevelDataList, $scope.BudgetLevelDataList, $scope.ActivityLevelDataList);
                });
            });

        });

    };
    $scope.GetBalanceSheetTreeViewData();
    $scope.summaryRows = [{
        title: "Total Amount", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Amount", textAlign: ej.TextAlign.Right, dataMember: "Amount", format: "{0:N2}" }
        ],
        showCaptionSummary: true
    }];
    $scope.loadGrid = function (gLData, budgetData, activityData) {
        $scope.GLLevelDataList = gLData;
        $scope.BudgetLevelDataList = budgetData;
        $scope.ActivityLevelDataList = activityData;


        var gridObj = $("#Grid").data("ejGrid");

        if (gridObj !== undefined && typeof gridObj === 'object' && typeof gridObj.destroy === 'function') gridObj.destroy();

        $("#Grid").ejGrid({
            dataSource: $scope.GLLevelDataList,
            allowSelection: true,
            selectionType: ej.Grid.SelectionType.Single,
            selectionSettings: { selectionMode: ["cell"], cellSelectionMode: ej.Grid.CellSelectionMode.Box },
            columns: [
                { field: "GL", headerText: 'GL', textAlign: ej.TextAlign.Left, width: 185 },
                { field: "Amount", headerText: 'Amount', textAlign: ej.TextAlign.Right, width: 80, format: "{0:N2}" }
            ],
            childGrid: {
                dataSource: $scope.BudgetLevelDataList,
                queryString: "GLGeneralInfoId",
                showSummary: true,
                summaryRows: $scope.summaryRows,
                allowSelection: true,
                selectionType: ej.Grid.SelectionType.Single,
                selectionSettings: { selectionMode: ["cell"], cellSelectionMode: ej.Grid.CellSelectionMode.Box },
                columns: [
                    { field: "Budget", headerText: 'Budget', textAlign: ej.TextAlign.Left, width: 185 },
                    { field: "Amount", headerText: 'Amount', textAlign: ej.TextAlign.Right, width: 80, format: "{0:N2}" }
                ],
                childGrid: {
                    dataSource: $scope.ActivityLevelDataList,
                    queryString: "GLGeneralInfoIdBudgetMasterId",
                    showSummary: true,
                    summaryRows: $scope.summaryRows,
                    allowSelection: true,
                    selectionType: ej.Grid.SelectionType.Single,
                    selectionSettings: { selectionMode: ["cell"], cellSelectionMode: ej.Grid.CellSelectionMode.Box },
                    columns: [
                        { field: "Activity", headerText: 'Activity', textAlign: ej.TextAlign.Left, width: 185 },
                        { field: "Amount", headerText: 'Amount', textAlign: ej.TextAlign.Right, width: 80, format: "{0:N2}" }
                    ]
                }
            }
        }).render();
    };

}