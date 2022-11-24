'use strict';
EOTSheetController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function EOTSheetController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = '';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'Leave/EOTSheet/';
    $scope.FromDate = null;
    $scope.ToDate = null;

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);
    $scope.IndividualDailyOT = {
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        ReportFormat: 'Excel',
        OTDuration: 0,
        OTfinal: 'OverStay',
        CheckBox: false
    };
    $scope.FromDate = $filter('dateFiltering')(firstDay);
    $scope.ToDate = $filter('dateFiltering')(Date.now());

    $scope.GetModifiedDailyOTReport = function () {
        try {
            //if (baseService.isUndefinedOrNull($scope.IndividualDailyOT.OTDuration)) {
            //    throw ("OT Duration is required.");
            //}
            if (baseService.isUndefinedOrNull($scope.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
                //ShowResult("Year No is required.", 'failure');
            }
            else if (baseService.isUndefinedOrNull($scope.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
                // ShowResult("Month No is required.", 'failure');
            }
            else if (new Date($scope.FromDate) > new Date($scope.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
                ShowResult("From date must be below or equal to To Date", 'failure');
            }
            else if (new Date($scope.ToDate) < new Date($scope.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
                ShowResult("To date must be above or equal to From Date.", 'failure');

            }
            else {
                var url = $scope.path + 'GetMIndividualDailyOT?reportFormat=Excel' + ' &FromDate=' + $scope.FromDate + ' &ToDate=' + $scope.ToDate;
                $rootScope.report(url);
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetIndividualDailyOTReport = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.IndividualDailyOT.OTDuration)) {
                throw ("OT Duration is required.");
            }
            if (baseService.isUndefinedOrNull($scope.IndividualDailyOT.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
                ShowResult("Year No is required.", 'failure');
            }
            else if (baseService.isUndefinedOrNull($scope.IndividualDailyOT.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
                ShowResult("Month No is required.", 'failure');
            }
            else if (new Date($scope.IndividualDailyOT.FromDate) > new Date($scope.IndividualDailyOT.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
                ShowResult("From date must be below or equal to To Date", 'failure');
            }
            else if (new Date($scope.IndividualDailyOT.ToDate) < new Date($scope.IndividualDailyOT.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
                ShowResult("To date must be above or equal to From Date.", 'failure');

            }
            else {
                var url = $scope.path +'GetExtraIndividualDailyOT?reportFormat=Excel' + ' &FromDate=' + $scope.IndividualDailyOT.FromDate + ' &ToDate=' + $scope.IndividualDailyOT.ToDate + ' &OTDuration=' + $scope.IndividualDailyOT.OTDuration + '&OTfinal=' + $scope.IndividualDailyOT.OTfinal + '&CheckBox=' + $scope.IndividualDailyOT.CheckBox;
                $rootScope.report(url);
            }
        } catch (e) {
            ShowResult(e, 'failure');

        }
    };


}