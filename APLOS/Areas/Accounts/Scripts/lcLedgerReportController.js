"use strict";
lcLedgerReportController.$inject = ["$scope", '$http', "$rootScope", "$filter", "accountService", "$window", "baseService"];
function lcLedgerReportController($scope, $http, $rootScope, $filter, accountService, $window, baseService) {
    $rootScope.title = "LC Ledger";
    $scope.report = {
        LCRef: null,
        ReportFormat: "Pdf",
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now()),
        Active: true
    };
    $scope.purchaseLCList = [];
    $scope.getpurchaseLCListData = function () {
        $scope.purchaseLCList = [];
        $http.get("Commercial/InvoiceTaggedWithLC/getpurchaseLCList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {

                        $scope.purchaseLCList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getpurchaseLCListData();

    $scope.getPurchaseLCData = function () {
        angular.element(document.querySelector("#PurchaseLCPopUp")).modal("show");
    }
    $scope.SetDetails = function (args) {
        $scope.report.LCRef = args.data.LCRef;
        angular.element(document.querySelector("#PurchaseLCPopUp")).modal("hide");
    }


    $scope.getReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.LCRef)) {
            manualValidation("div_LCRef", true, "LCR No is required.");
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
            var url = "";
            url = "Accounts/Voucher/GetLCLedgerReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + "&lCRef=" + $scope.report.LCRef;
            
            $window.open(url, "_blank");
        }
    };
  
}