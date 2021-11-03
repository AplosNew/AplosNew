'use strict';
welfareReportsController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function welfareReportsController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    $scope.welfareReport = {
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        //EmployeeId: null,
        ReportFormat: 'Excel',
        //chkAdditionInfo: false
    };
    $scope.GetWelFareReport = function (reportType) {
        try {
            if (baseService.isUndefinedOrNull($scope.welfareReport.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
                ShowResult("From Date is required.", 'failure');
            }
            else if (baseService.isUndefinedOrNull($scope.welfareReport.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
                ShowResult("To Date is required.", 'failure');
            }
            else if (new Date($scope.welfareReport.FromDate) > new Date($scope.welfareReport.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
                ShowResult("From date must be below or equal to To Date", 'failure');
            }
            else if (new Date($scope.welfareReport.ToDate) < new Date($scope.welfareReport.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
                ShowResult("To date must be above or equal to From Date.", 'failure');

            }
            else {

                $http({
                    method: 'POST',
                    url: 'humanresource/WelFareReports/GetWelFareReport',
                    data: {
                        'FromDate': $scope.welfareReport.FromDate,
                        'ToDate': $scope.welfareReport.ToDate,
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        if (reportType === 'EXCEL') {
                            $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

                        }
                        if (reportType === 'PDF') {
                            $rootScope.report($scope.downloadgriddataPDFUrl + "?FileName=" + response.data.FileName);

                        }

                    }
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
}