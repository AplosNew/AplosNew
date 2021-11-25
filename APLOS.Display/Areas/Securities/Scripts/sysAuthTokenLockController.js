'use strict';
function SysAuthTokenLockController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "SysAdmin AuthToken UnLock";
    $scope.index = -1;
    $scope.Action = 'UnLock';
    $scope.sysAdmins = [];
    $scope.path = 'Securities/systemadmin/';
    $scope.getListUrl = $scope.path + 'syadminauthtokenlockdate';
    baseService.init($scope.getListUrl, null, null, null, 'UserId', 'UserId');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.sysAdmins = result.Rows;
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

    $scope.sysAdmin = {
        Id: null,
        CompanyGroupId: null,
        UserId: null,
        FullName: null,
        AuthTokenFailCount: 0,
        AuthTokenLocked: false,
        AuthTokenLockedDate: null,
        AuthTokenUnlockDate: null,
        PowerUser: false,
        TotalAuthLockTime: null,
        LastLockTime: null
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.sysAdmin = $scope.sysAdmins[$scope.index];
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.authTokenUnLock = function () {
        if ($rootScope.id != null) {
            $http({
                method: 'POST',
                url: 'Securities/systemadmin/sysadminauthtokenunlock?id=' + $rootScope.id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.sysAdmin = {};
                    $scope.sysAdmins.splice($rootScope.index, 1);
                    $rootScope.id = null;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }
    };
}
SysAuthTokenLockController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];