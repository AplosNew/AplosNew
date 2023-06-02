"use strict";
paymentPendingforSetOffReportController.$inject = ["addressService", "cboService", "$scope", "$rootScope", "$filter", "bankService", "$window", "baseService"];
function paymentPendingforSetOffReportController(addressService, cboService, $scope, $rootScope, $filter, bankService, $window, baseService) {
    $rootScope.title = "Payment Pending for Set Off Report";
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    $scope.report = {
        BankMasterId: null,
        ReportFormat: "Excel", 
        ReportType: "Advance",
        FromDate: $filter("dateFiltering")(firstDay),
        ToDate: $filter("dateFiltering")(Date.now())
    };

    $scope.getReport = function () {
        var url = "Accounts/TaxReport/GetPaymentPendingforSetOffReport?reportFormat=" + $scope.report.ReportFormat + "&reportType=" + $scope.report.ReportType + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate;
        $rootScope.report(url);
    };
   
}