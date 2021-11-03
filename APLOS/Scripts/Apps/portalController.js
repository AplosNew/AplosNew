'use strict';
PortalController.$inject = ['commonMessage', '$scope', '$rootScope', '$routeParams', '$http', '$filter'];
function PortalController(commonMessage, $scope, $rootScope, $routeParams, $http, $filter) {
    $rootScope.title = 'Portal';
    $scope.authenticationToken = angular.element(document.querySelector('#authToken')).val();
    $scope.companyGroupId = angular.element(document.querySelector('#groupId')).val();
    $scope.companyGroupName = null;
    // Default/alt company group logo.
    $scope.companyGroupLogo = 'images/group-alt.png';
    $scope.errorText = null;
    $http.get('Organizations/companygroup/getnameandlogo/' + $scope.companyGroupId)
        .then(function (response) {
            if (response.data[0] != null) {
                $scope.companyGroupName = response.data[0];
            }
            else {
                $scope.companyGroupName = 'Company group name not found!';
            }
            if (response.data[1] != null) {
                $scope.companyGroupLogo = virtualPath.LogoOrImage + response.data[1];
            }
        });
};