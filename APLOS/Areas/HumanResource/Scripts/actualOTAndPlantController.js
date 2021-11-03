'use strict';
actualOTAndPlantController.$inject = ['cboService','commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function actualOTAndPlantController(cboService,commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Actual OT And Plant Controller";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'humanresource/preallocatedot/';
    $scope.saveUrl = $scope.path + 'create';

    $scope.modelNew = {
        WorkDate: $filter('dateFiltering')(Date.now()), 
        ReportFormat: 'Excel'
    }

    $scope.GetActualOTReport = function () {
        try {
             if (baseService.isUndefinedOrNull($scope.modelNew.WorkDate)) {
                throw 'Please Select WorkDate';
                }
            
            else if ($scope.modelNew.ReportFormat === 'Excel') {

                 var url = 'HumanResource/AttendanceManagement/GetActualOTAndPlanReport?reportFormat=' + $scope.modelNew.ReportFormat  + '&WorkDate=' + $scope.modelNew.WorkDate;
                $rootScope.report(url);
            }

        } catch (e) {
            ShowResult(e, 'failure');

        }
    };



}