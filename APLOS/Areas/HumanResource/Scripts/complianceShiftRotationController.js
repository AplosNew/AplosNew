'use strict';
complianceShiftRotationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster'];
function complianceShiftRotationController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster) {
    $rootScope.title = 'Shift Rotation';
    $scope.PathShiftChange = 'HumanResource/CompliedShiftAssignment/';

    $scope.compliedShiftRotationDate = null;
   
    $scope.CompliedshiftChange = function () {
        $http({
            method: 'POST',
            url: $scope.PathShiftChange + 'CompliedshiftChange',
            params: {
                'rotationDate': $scope.compliedShiftRotationDate,
                'addedBy': "",
                'ip': "",
                'appVersion': "",
                'requestType':"Application"
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
}