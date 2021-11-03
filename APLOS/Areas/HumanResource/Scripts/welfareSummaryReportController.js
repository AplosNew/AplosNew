'use strict';
welfareSummaryReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function welfareSummaryReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.calendarYearlist = [];
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    cboService.getCboLeaveYear(function (result) {
        $scope.calendarYearlist = result;
        $scope.YearNo = $filter("filter")($scope.leaveYearlist, { Text: new Date().getFullYear() })[0].Value;
    });
    $scope.GetWelfareSummarydReport = function () {
        try {
            
            $http({
                method: 'POST',
                url: 'humanresource/WelfareSummaryReport/XlsEmployeeWalfareSummary',
                data: {                    
                    'year': $scope.YearNo                    
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

 
}