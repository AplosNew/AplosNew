"use strict";
GSTPayableSalesReportController.$inject = ["addressService", "cboService", "$scope", "$rootScope", "$filter", "bankService", "$window", "baseService"];
function GSTPayableSalesReportController(addressService, cboService, $scope, $rootScope, $filter, bankService, $window, baseService) {
    $rootScope.title = "GST Payable Sales";
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    $scope.report = {
        BankMasterId: null,
        ReportFormat: "Excel",      
        FromDate: $filter("dateFiltering")(firstDay),
        ToDate: $filter("dateFiltering")(Date.now())
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
            var url = "Accounts/TaxReport/GetGSTPayableSalesReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate;
            $rootScope.report(url);

        }
    };



    $scope.getReport2 = function () {

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
            var url = "Accounts/TaxReport/GetGSTPayableSalesReport2?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate;
            $rootScope.report(url);

        }
    };

    $scope.getReport3 = function () {

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
            var url = "Accounts/TaxReport/GetGSTPayableSalesReport3?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate;
            $rootScope.report(url);

        }
    };
}