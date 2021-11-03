"use strict";
bankReconcileReportController.$inject = ["$scope", "$rootScope", "$filter", "bankService", "$window", "baseService"];
function bankReconcileReportController($scope, $rootScope, $filter, bankService, $window, baseService) {
    $rootScope.title = "Bank Reconcile";
    $scope.report = {
        BankMasterId: null,
        ReportFormat: "Pdf",
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now())
    };

    bankService.getBankMasterHouseBankCboList(function (result) {
        $scope.bankMasterList = result;
    });

    $scope.getReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.BankMasterId)) {
            manualValidation("div_Bank", true, "Bank is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
            manualValidation("div_FromDate", true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
            manualValidation("div_ToDate", true, "To Date is required.");
        }
        else if (new Date($scope.report.FromDate) > new Date($scope.report.ToDate)) {
            manualValidation("div_FromDate", true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.report.ToDate) < new Date($scope.report.FromDate)) {
            manualValidation("div_ToDate", true, "To date must be above or equal to From Date.");
        }
        else {
            var url = "Banks/BankReport/GetBankReconcileReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + "&bankMasterId=" + $scope.report.BankMasterId;
            $window.open(url, "_blank");
        }
    };
}