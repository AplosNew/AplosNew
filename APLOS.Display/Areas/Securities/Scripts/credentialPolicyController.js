'use strict';
function CredentialPolicyController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.credentialpolicies = [];
    $scope.path = 'Securities/credentialpolicy/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveGet = $scope.path + 'getcredentialpolicy';
    $rootScope.searchByList = [
        {
            'name': 'PasswordExpireAfterDay',
            'value': 'PwdExpAfterDay'
        },
        {
            'name': 'AuthTokenLockTimeDifference',
            'value': 'AuthTokenLockTimeDifference'
        }
    ];

    $scope.credentialpolicy = {
        Id: null,
        CompanyGroupId: null,
        PwdExpAfterDay: null,
        PwdExpAlertBeforeDay: null,
        PwdCheckingFromLastHistory: null,
        TwoFactorEnable: true,
        PwdFailCount: null,
        AuthTokenFailCount: null,
        PwdLockTimeDifference: null,
        AuthTokenLockTimeDifference: null,
        AllowConcurrentUser: true,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };

    $http.get($scope.saveGet)
        .then(function (response) {
            if (response.data != null) {
                $scope.Action = 'Update';
                $scope.credentialpolicy = response.data;
            }
        });

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.credentialpolicyForm.$valid) {
            if ($scope.credentialpolicy.UpdatedDate != null) {
                $scope.credentialpolicy.UpdatedDate = $filter('dateFilter')($scope.credentialpolicy.UpdatedDate);
            }
            $scope.credentialpolicy.AddedDate = $filter('dateFilter')($scope.credentialpolicy.AddedDate);
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: $scope.credentialpolicy,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
            }, function errorCallback(response) {
                ShowResult(response.statusText.Message, 'failure');
            });
            return true;
        }
    }
}
CredentialPolicyController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];