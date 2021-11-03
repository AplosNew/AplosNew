'use strict';
esicSummaryController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function esicSummaryController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.path = 'Payrolls/ESICSummary/';
    $scope.employeeCategoryId = null;
    $scope.dailyComplianceReport = {
        WorkDate: null
    };   
    $scope.year = new Date().getFullYear().toString();
    $scope.year = null;    

    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
  
    $scope.GetESICSummaryReports = function () {
        try {            
            var DropDownListYear = $("#ddlYearList").data("ejDropDownList");
            $scope.year = DropDownListYear.getSelectedValue();
            if (angular.isUndefinedOrNull($scope.year)) {
                ShowResult("Select Year", 'failure');
            }
            else {               
                $rootScope.report($scope.path + 'GetESICSummaryReport' + "?year=" + $scope.year);
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };   

}