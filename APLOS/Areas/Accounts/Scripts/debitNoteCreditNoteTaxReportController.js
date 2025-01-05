"use strict";
debitNoteCreditNoteTaxReportController.$inject = ["addressService", "cboService", "$scope", "$rootScope", "$filter", "bankService", "$window", "baseService"];
function debitNoteCreditNoteTaxReportController(addressService, cboService, $scope, $rootScope, $filter, bankService, $window, baseService) {
    $rootScope.title = "Debit Note and Credit Note Status Report";
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    $scope.report = {
        BankMasterId: null,
        ReportFormat: "Excel",
        PartyType: 'Party',
        NoteType: 'Both',
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
        else if (baseService.isUndefinedOrNull($scope.report.PartyType)) {
            manualValidation('div_PartyType', true, "Party Type is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.report.NoteType)) {
            manualValidation('div_NoteType', true, "Note Type is required.");
        }
        else {
            var url = "Accounts/TaxReport/GetDebitNoteCreditNoteTaxReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + '&partyType=' + $scope.report.PartyType + '&noteType=' + $scope.report.NoteType;
            $rootScope.report(url);

        }
    };
   
}