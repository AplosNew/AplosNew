'use strict';
AccountController.$inject = ['$scope', '$rootScope', '$routeParams', '$http', '$filter'];
function AccountController($scope, $rootScope, $routeParams, $http, $filter) {
    $rootScope.title = 'Administration::Login';
    $scope.authenticationToken = $routeParams.authenticationToken;
    $scope.companyGroupId = $routeParams.groupId;
    $scope.companyGroupName = null;
    // Default/alt company group logo.
    $scope.companyGroupLogo = 'organization-alt.png';
    $scope.errorText = null;
    $http.get('Organizations/companygroup/getnameandlogo/' + $routeParams.groupId)
        .then(function (response) {
            if (response.data[0] != null) {
                $scope.companyGroupName = response.data[0];
            }
            else {
                $scope.companyGroupName = 'Company group name not found!';
            }
            if (response.data[1] != null) {
                $scope.companyGroupLogo = response.data[1];
            }
        });
};