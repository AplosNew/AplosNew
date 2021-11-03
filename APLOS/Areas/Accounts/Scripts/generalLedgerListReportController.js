"use strict";
generalLedgerListReportController.$inject = ["$scope", "$rootScope", "$filter", "baseService", "$window", "cboService"];
function generalLedgerListReportController($scope, $rootScope, $filter, baseService, $window, cboService) {
    $rootScope.title = "GL List Report";
    $scope.report = {
        COAId: null,
        ReportFormat: "Pdf"
    };

    cboService.getCboChartOfAccount("", function (result) {
        $scope.cOAList = result;
    });

    $scope.getReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.COAId)) {
            manualValidation("div_COA", true, "COA is required.");
        }
        else {
            var url = "Accounts/GLItem/GetGeneralLedgerListReport?reportFormat=" + $scope.report.ReportFormat + "&coaId=" + $scope.report.COAId;
            $window.open(url, "_blank");
        }
    };
}