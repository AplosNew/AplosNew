'use strict';
shiftSummaryController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function shiftSummaryController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';
  
    $scope.ShiftSummaryModel = {
        WorkDate: $filter('dateFiltering')(Date.now()),
        ReportFormat: 'Excel',       
    };
    $scope.GetdailyattendanceReport = function (reportType) {
        try {
            if (baseService.isUndefinedOrNull($scope.ShiftSummaryModel.WorkDate)) {
                manualValidation('div_FromDate', true, "WorkDate is required.");
                ShowResult("WorkDate is required.", 'failure');
            }          
            else {
                $http({
                    method: 'POST',
                    url: 'humanresource/shiftSummary/Getdailyattendance',
                    data: {
                        'WorkDate': $scope.ShiftSummaryModel.WorkDate                     
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        if (reportType === 'EXCEL') {
                            $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                        }
                    }
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
}