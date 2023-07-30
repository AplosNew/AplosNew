'use strict';
ProcessManagementController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function ProcessManagementController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $scope.title = 'Process Management'
    $scope.path = 'Process/ProcessManagement/';
    $scope.Action = 'Save';

    $scope.EntityList = [];
    $scope.LoadEntityDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'QMS/QualityManagementMaster/LoadEntityDetails?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
        }
        )
    }

}