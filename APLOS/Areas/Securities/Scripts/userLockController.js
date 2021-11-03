'use strict';
function UserLockController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "User UnLock";
    $scope.index = -1;
    $scope.Action = 'UnLock';
    $scope.users = [];
    $scope.path = 'Securities/user/';
    $scope.getListUrl = $scope.path + 'userlockdatewithoutsyadmin';
    baseService.init($scope.getListUrl, null, null, null, 'UserId', 'UserId');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.users = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $rootScope.searchByList = [
        {
            'name': 'Username',
            'value': 'UserId'
        },
        {
            'name': 'Full Name',
            'value': 'FullName'
        }
    ];

    $scope.user = {
        Id: null,
        CompanyGroupId: null,
        UserId: null,
        FullName: null,
        userFailCount: 0,
        userLocked: false,
        userLockedDate: null,
        userUnlockDate: null,
        PowerUser: false,
        TotalAuthLockTime: null,
        LastLockTime: null
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.user = $scope.users[$scope.index];
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.userUnLock = function () {
        if ($rootScope.id != null) {
            $http({
                method: 'POST',
                url: 'Securities/user/userunlock?id=' + $rootScope.id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.user = {};
                    $scope.users.splice($rootScope.index, 1);
                    $rootScope.id = null;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }
    };
}
UserLockController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];