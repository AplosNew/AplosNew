"use strict";
GSTPayableReportController.$inject = ["addressService", "cboService", "$scope", "$rootScope", "$filter", "bankService", "$window", "baseService"];
function GSTPayableReportController(addressService, cboService, $scope, $rootScope, $filter, bankService, $window, baseService) {
    $rootScope.title = "TDS Payable";
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
            var url = "Accounts/TaxReport/GetGSTPayableReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate;
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
            var url = "Accounts/TaxReport/GetGSTPayableReport2?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate;
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
            var url = "Accounts/TaxReport/GetGSTPayableReport3?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate;
            $rootScope.report(url);

        }
    };
   
}