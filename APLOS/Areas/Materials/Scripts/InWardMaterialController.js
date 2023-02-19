'use strict';
InWardMaterialController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService', '$window', '$controller'];
function InWardMaterialController(fileReader, commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService, $window, $controller) {
    $rootScope.title = "In Ward Material";

    $scope.index = -1;

    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.exportgriddataUrlUpd = 'GridReports/ExcelExportUpd';

    $scope.downloadgriddataUrl = 'GridReports/Download';

    //In Ward Material-Start

    //$scope.fromDate = $filter('dateFiltering')(Date.now());
    $scope.toDate = $filter('dateFiltering')(Date.now());

    $scope.InWardMaterialReportExcel = function (reportFormat) {

        if (baseService.isUndefinedOrNull($scope.fromDate)) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if (baseService.isUndefinedOrNull($scope.toDate)) {
            ShowResult('Select To Date', 'failure');
            return false;
        }

        try {
            var Excel;
            var file_src = 'Materials/MaterialLedger/InWardMaterialReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.fromDate + '&toDate=' + $scope.toDate;
            $rootScope.report(file_src);
        } catch (e) {

        }
    }

    $scope.InWardMaterialReportPdf = function (reportFormat) {
        if (baseService.isUndefinedOrNull($scope.fromDate)) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if (baseService.isUndefinedOrNull($scope.toDate)) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        var reportFormat = "Pdf";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Materials/MaterialLedger/InWardMaterialReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.fromDate + '&toDate=' + $scope.toDate);

    };
    //End In ward material

}
 

