"use strict";
TDSDeductionReportController.$inject = ["addressService", "cboService", "$scope", "$rootScope", "$filter", "bankService", "$window", "baseService"];
function TDSDeductionReportController(addressService, cboService, $scope, $rootScope, $filter, bankService, $window, baseService) {
    $rootScope.title = "TDS Deduction";
    $scope.report = {
        //BankMasterId: null,
        ReportFormat: "Excel",
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now())
    };

    $scope.getReport = function () {
        //if (baseService.isUndefinedOrNull($scope.report.TaxCategoryId)) {
        //    manualValidation("div_Bank", true, "Tax Category is required.");
        //}
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
            //var url = "Accounts/InvoiceTax/GetTaxPayableReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + "&taxCategoryId=" + $scope.report.TaxCategoryId;
            //$window.open(url, "_blank");

            var url = "Accounts/TaxReport/GetTdsDeductionReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate /*+ "&taxCategoryId=" + $scope.report.TaxCategoryId*/;
            $window.open(url, "_blank");

            
        }
    };
}