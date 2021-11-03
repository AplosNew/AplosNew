'use strict';
tiffinBillReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function tiffinBillReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Tiffin Bill Report";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'humanresource/preallocatedot/';
    $scope.saveUrl = $scope.path + 'create';

    $scope.dailyAllowanceList = [];
    cboService.getDailyAllowanceCbo(function (result) {
        $scope.dailyAllowanceList = result;
    });

    $scope.modelNew = {      
        WorkDate: $filter('dateFiltering')(Date.now()),
        DailyAllowance: null,
        ReportFormat: 'Excel'
    }

    $scope.GetTiffinBillReport = function () {
        try {
            $scope.ReportName = $("#Id option:selected").text();

            if (baseService.isUndefinedOrNull($scope.modelNew.WorkDate)) {
                throw 'Please Select WorkDate';
            }
            if (baseService.isUndefinedOrNull($scope.modelNew.DailyAllowance)) {
                throw 'Please Select Daily Allowance';
            }

            else if ($scope.modelNew.ReportFormat === 'Excel') {

                var url = 'HumanResource/AttendanceManagement/GetTiffinBillFinalReport?reportFormat=' + $scope.modelNew.ReportFormat + '&WorkDate=' + $scope.modelNew.WorkDate + '&DailyAllowance=' + $scope.modelNew.DailyAllowance + '&ReportName=' + $scope.ReportName;
                $rootScope.report(url);
            }
        } catch (e) {
            ShowResult(e, 'failure');

        }
    };
}