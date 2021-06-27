'use strict';
AccountTypeController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function AccountTypeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Account Type';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.accountTypes = [];
    $scope.path = 'accounts/accountType/';
    $scope.getListUrl = $scope.path + 'getaccountTypelist';
    $scope.getUrl = $scope.path + 'get';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "Id", "Id");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.accountTypes = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.accountType = {
        Id: null,
        BalanceType: null,
        IsBalanceSheet: false,
        Description: null,
        //Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };

    $scope.accounttypebalancetypeList = [];
    $http({
        method: 'GET',
        url: 'Enum/getaccountgroupbalancetypelistcbo/'
    }).then(function successCallback(response) {
        $scope.accounttypebalancetypeList = response.data;
    });

    $scope.coaaccounttypeList = [];
    $http({
        method: 'GET',
        url: 'Enum/GetAccountTypeEnumCbo/'
    }).then(function successCallback(response) {
        $scope.coaaccounttypeList = response.data;
    });

    $scope.CheckIdUse = function (id) {
        $http.get('accounts/accountType/checkiduse?id=' + id)
            .then(function (response) {
                $scope.checkIdUsedValue = response.data;
            });
    };

    $scope.Get = function (username, index) {
        $scope.index = index;
        $scope.CheckIdUse(username);
        $scope.accountType = $scope.accountTypes[$scope.index];
        $scope.accountType.AddedDate = $filter('dateFilter')($scope.accountType.AddedDate);
        $scope.accountType.UpdatedDate = $filter('dateFilter')($scope.accountType.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.accountTypeForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.accountType,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.accountTypes.push(response.data.ChartOfAccountLevel1);
                        baseService.paginationAdd();
                        $scope.getData();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.accountType,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.accountTypes[$scope.index] = $scope.accountType;
                        }
                        $scope.getData();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.accountType.username)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.accountType.username,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.accountTypes.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.accountType = {};
        $scope.accountType.Active = true;
        $scope.checkIdUsedValue = false;
    }
}