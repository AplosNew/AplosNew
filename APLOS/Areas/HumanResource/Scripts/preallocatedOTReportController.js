'use strict';
preallocatedOTReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function preallocatedOTReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Preallocated OT Report";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'humanresource/preallocatedot/';
    $scope.saveUrl = $scope.path + 'create';

    $scope.departmentList = [];
    cboService.getCboDepartmentByCompanyGroup(null, function (result) {
        $scope.departmentList = result;
    });

    $scope.SectionList = [];
    cboService.getSectionCbo(function (result) {
        $scope.SectionList = result;
    });

    $scope.modelNew = {
        SectionId: null,
        departmentId: null,
        WorkDate: $filter('dateFiltering')(Date.now()), 
        ReportFormat: 'Excel'
    }

    $scope.GetPereAllocatedReport = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.modelNew.WorkDate)) {
                throw 'Please Select WorkDate';
            }
            var url = 'HumanResource/AttendanceManagement/GetPreAllocatedReport?reportFormat=' + 'Excel' + '&WorkDate=' + $scope.modelNew.WorkDate;
            $rootScope.report(url);
        } catch (e) {
            ShowResult(e, 'failure');

        }
    };

    $scope.GetOTPlanningMatrixReport = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.modelNew.WorkDate)) {
                throw 'Please Select WorkDate';
            }
            var url = 'HumanResource/PreallocatedOT/GetOTPlanningMatrixReport?reportFormat=' + 'Excel' + '&WorkDate=' + $scope.modelNew.WorkDate;
            $rootScope.report(url);
        } catch (e) {
            ShowResult(e, 'failure');

        }
    };


}