'use strict';
rawDataSetInOutController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function rawDataSetInOutController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Raw Data Set In Out';
    $scope.path = 'Attendances/RawDataSetInOut/';
    $scope.processUrl = $scope.path + 'Process';
    $scope.Action = 'Process';
    
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    $scope.AttendanceProcess = {
        FromDate: $filter('dateFiltering')(Date.now()),      
    };

    $scope.Process = function () {
        try {            
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.AttendanceProceForm.$valid) {
                if ($scope.Action === 'Process') {
                    $http({
                        method: 'POST',
                        url: $scope.processUrl,
                        data: { 'pFromDate': $scope.AttendanceProcess.FromDate},
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                        }

                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    
}