"use strict";
TDSDeductionReportController.$inject = ["addressService", "cboService", "$scope", "$rootScope", "$filter", "bankService", "$window", "baseService"];
function TDSDeductionReportController(addressService, cboService, $scope, $rootScope, $filter, bankService, $window, baseService) {
    $rootScope.title = "TDS Deduction";
    $scope.report = {
        //BankMasterId: null,
        ReportFormat: "Excel",
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now()),
        NONTDSFromDate: $filter("dateFiltering")(Date.now()),
        NONTDSToDate: $filter("dateFiltering")(Date.now()),
        TCSFromDate: $filter("dateFiltering")(Date.now()),
        TCSToDate: $filter("dateFiltering")(Date.now())
    };

    $scope.getReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
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
            var url = "Accounts/TaxReport/GetTdsDeductionReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate /*+ "&taxCategoryId=" + $scope.report.TaxCategoryId*/;
            $window.open(url, "_blank");
        }
    };

    $scope.getTCSReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.TCSFromDate)) {
            manualValidation("div_TCSFromDate", true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.report.TCSToDate)) {
            manualValidation("div_TCSToDate", true, "To Date is required.");
        }
        else if (new Date($scope.report.TCSFromDate) > new Date($scope.report.TCSToDate)) {
            manualValidation("div_TCSFromDate", true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.report.TCSToDate) < new Date($scope.report.TCSFromDate)) {
            manualValidation("div_TCSToDate", true, "To date must be above or equal to From Date.");
        }
        else {
            var url = "Accounts/TaxReport/GetTCSDeductionReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.TCSFromDate + "&toDate=" + $scope.report.TCSToDate /*+ "&taxCategoryId=" + $scope.report.TaxCategoryId*/;
            $window.open(url, "_blank");
        }
    };

    $scope.getNonTDSReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
            manualValidation("div_FromDate", true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.report.NONTDSToDate)) {
            manualValidation("div_ToDate", true, "To Date is required.");
        }
        else if (new Date($scope.report.FromDate) > new Date($scope.report.NONTDSToDate)) {
            manualValidation("div_FromDate", true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.report.NONTDSToDate) < new Date($scope.report.NONTDSFromDate)) {
            manualValidation("div_ToDate", true, "To date must be above or equal to From Date.");
        }
        else {
            var url = "Accounts/TaxReport/GetNonTDSInvoiceReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.NONTDSFromDate + "&toDate=" + $scope.report.NONTDSToDate /*+ "&taxCategoryId=" + $scope.report.TaxCategoryId*/;
            $window.open(url, "_blank");
        }
    };
}