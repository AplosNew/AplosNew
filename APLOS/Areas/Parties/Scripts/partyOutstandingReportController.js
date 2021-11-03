'use strict';
partyOutstandingReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$window'];
function partyOutstandingReportController(commonMessage, $scope, $rootScope, baseService, $http, $filter, $window) {
    $rootScope.title = 'Party Outstanding Ledger Report';
    $scope.parallelCurrencyList = [];
    $scope.report = {
        ToDate: $filter('dateFiltering')(Date.now()),
        ReportFormat: 'Pdf'
    };

    $scope.getReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
            manualValidation('div_ToDate', true, "To Date is required.");
        }
        else {
            var url = 'Parties/PartyReport/GetPartyOutstadningReport?reportFormat=' + $scope.report.ReportFormat + '&toDate=' + $scope.report.ToDate;
            $window.open(url, '_blank');
        }
    };
}