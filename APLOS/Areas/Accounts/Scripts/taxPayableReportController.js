"use strict";
taxPayableReportController.$inject = ["addressService", "cboService", "$scope", "$rootScope", "$filter", "bankService", "$window", "baseService"];
function taxPayableReportController(addressService, cboService, $scope, $rootScope, $filter, bankService, $window, baseService) {
    $rootScope.title = "Tax Payable";
    $scope.report = {
        BankMasterId: null,
        ReportFormat: "Pdf",
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now())
    };

    $scope.countryList = [];
    addressService.getCountryCbo(function (result) {
        $scope.countryList = result;
    });

    $scope.GetTaxCategory = function(id) {
        $scope.report.CountryId = id;
        $scope.TaxCategoryList = [];
        cboService.getTaxCategoryCboByCountry(id, function (result) {
            $scope.TaxCategoryList = result;
        });
    };

    $scope.getReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.TaxCategoryId)) {
            manualValidation("div_Bank", true, "Tax Category is required.");
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
            var url = "Accounts/InvoiceTax/GetTaxPayableReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + "&taxCategoryId=" + $scope.report.TaxCategoryId;
            $window.open(url, "_blank");
        }
    };
}