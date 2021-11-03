'use strict';
budgetMasterReportController.$inject = ['$scope', '$rootScope', '$filter', "baseService", "$window", "cboService"];
function budgetMasterReportController($scope, $rootScope, $filter, baseService, $window, cboService) {
    $rootScope.title = 'Budget Master Report';
    $scope.report = {
        COAId: null,
        IsActivityLevel: false,
        ReportFormat: 'Pdf'
    };

    cboService.getCboChartOfAccount('', function (result) {
        $scope.cOAList = result;
    });

    $scope.getReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.COAId)) {
            manualValidation('div_COA', true, "COA is required.");
        }
        else {
            var url = 'Accounts/BudgetMaster/GetBudgetMasterReport?reportFormat=' + $scope.report.ReportFormat + '&coaId=' + $scope.report.COAId + '&isActivityLevel=' + $scope.report.IsActivityLevel;
            $window.open(url, '_blank');
        }
    };
}