"use strict";
WeeklyReceiptAndPaymentStatementController.$inject = ["$scope", "$rootScope", "bankService", "$filter", "$window", "baseService"];
function WeeklyReceiptAndPaymentStatementController($scope, $rootScope, bankService, $filter, $window, baseService) {
    $rootScope.title = "Weekly Receipt And Payment Statement";
    $scope.report = {
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now())
    };
    $scope.cashMasterList = [];
    bankService.getCashMasterCboListByEntity(null, function (result) {
        $scope.cashMasterList = result;
    });
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
             var url = "Accounts/VoucherReport/GetWeeklyReceiptAndPaymentStatement?reportFormat=Excel" + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + "&cashMasterId=" + $scope.report.CashMasterId;
            //var url = "Banks/CashReport/GetCashBookReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + "&cashMasterId=" + $scope.report.CashMasterId;
            $window.open(url, "_blank");
        }
    };
}