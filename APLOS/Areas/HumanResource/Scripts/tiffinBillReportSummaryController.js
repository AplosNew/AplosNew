'use strict';
tiffinBillReportSummaryController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function tiffinBillReportSummaryController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Tiffin Bill Summary Report";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'humanresource/preallocatedot/';
    $scope.saveUrl = $scope.path + 'create';

    $scope.dailyAllowanceList = [];
    cboService.getDailyAllowanceCbo(function (result) {
        $scope.dailyAllowanceList = result;
    });

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    $scope.modelNew = {       
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        DailyAllowance: null,
        ReportFormat: 'Excel'
    }

    $scope.GetTiffinBillReport = function () {
        try {
            $scope.ReportName = $("#Id option:selected").text();

            if (baseService.isUndefinedOrNull($scope.modelNew.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
            }
            else if (baseService.isUndefinedOrNull($scope.modelNew.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
            }
            else if (new Date($scope.modelNew.FromDate) > new Date($scope.modelNew.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
            }
            else if (new Date($scope.modelNew.ToDate) < new Date($scope.modelNew.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
            }
            else if (baseService.isUndefinedOrNull($scope.modelNew.DailyAllowance)) {
                throw 'Please Select Daily Allowance';
            }

            else if($scope.modelNew.ReportFormat === 'Excel') {

                var url = 'HumanResource/AttendanceManagement/GetTiffinBillFinalSummaryReport?reportFormat=' + $scope.modelNew.ReportFormat + '&FromDate=' + $scope.modelNew.FromDate + '&ToDate=' + $scope.modelNew.ToDate + '&DailyAllowance=' + $scope.modelNew.DailyAllowance + '&ReportName=' + $scope.ReportName;
                $rootScope.report(url);
            }

        } catch (e) {
            ShowResult(e, 'failure');

        }
    };
}