'use strict';
gratuityReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function gratuityReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'Final Settlement';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    $scope.calculationDate = $filter('dateFiltering')(Date.now());

    $scope.payrollGroupId = null;
    $scope.employeeSystemId = null;
    $scope.reportType = null;

    $scope.payrollGroupList = [];
    
    cboService.getPayGroupCbo(function (result) {
        $scope.payrollGroupList = result;
    });

    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.getGratuityReport = function (reportType) {
        try {

            $http({
                method: 'POST',
                url: 'Payrolls/GratuityReport/XlsEmployeeGratuity',
                data: {
                    'calculationDate': $scope.calculationDate,
                    'payrollGroup': $scope.payrollGroupId,
                    'employeeSystemId': null,
                    'reportType': reportType
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    if (reportType == "EXCEL") {
                        $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                    }
                    if (reportType == "PDF") {
                        $rootScope.report($scope.downloadgriddataPDFUrl + "?FileName=" + response.data.FileName);
                    }
                }
            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };
}