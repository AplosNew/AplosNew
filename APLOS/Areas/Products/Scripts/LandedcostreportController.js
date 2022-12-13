'use strict';
LandedcostreportController.$inject = ['cboService', '$scope', '$rootScope', '$filter', "baseService", "$http", "$window"];
function LandedcostreportController(cboService, $scope, $rootScope, $filter, baseService, $http, $window) {
    $rootScope.title = 'Landed Cost Report';
    $scope.entityList = {};
    $scope.report = {
        ReportFormat: 'Pdf',
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now()),
        ReportType: 'GRNLandedCost'
    };
    $scope.entityId = null;
    cboService.getCboEntityPlantWise(null, null, null, function (result) {
        $scope.entityList = result;
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
            var url = 'Products/Landedcostreport/GetLandedCostReport?reportFormat=' + $scope.report.ReportFormat + '&fromdate=' + $scope.report.FromDate + '&todate=' + $scope.report.ToDate + '&reportType=' + $scope.report.ReportType;
            $window.open(url, '_blank');
        }



    };

}