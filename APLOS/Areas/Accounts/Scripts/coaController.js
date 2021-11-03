'use strict';
COAController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http'];
function COAController(commonMessage, $scope, $rootScope, baseService, $http) {
    $rootScope.title = 'COA';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.coas = [];
    $scope.getListUrl = 'accounts/coa/getallcoalist/';
    $scope.glvoucherXLUrl = 'accounts/coa/generatecoareport';
    baseService.init($scope.getListUrl, null, null, null, 'Code', 'Code');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.coas = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.searchBycoaList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'COA',
            'value': 'UserName'
        },
        {
            'name': 'LengthOfGL',
            'value': 'LengthOfGL'
        },
        {
            'name': 'Description',
            'value': 'Description'
        }
    ];

    $scope.coa = {
        Id: null,
        Code: null,
        UserName: null,
        IsLevelMandatory: true,
        LengthOfGL: null,
        Description: null,
        Active: true
    };

    $scope.checkCOAIdUse = function (id) {
        $http.get('accounts/coa/checkcoaiduse?coaid=' + id)
            .then(function (response) {
                $scope.checkCOAIdUsedValue = response.data;
            });
    };

    $scope.Get = function (id, index) {
        $scope.checkCOAIdUse(id);
        $scope.index = index;
        $scope.coa = $scope.coas[$scope.index];
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.coaForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'accounts/coa/create',
                    data: $scope.coa,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.coas.push(response.data.ChartOfAccount);
                        baseService.paginationAdd();
                        ClearFields();
                        $scope.getData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update' && !$scope.lengthField) {
                $http({
                    method: 'POST',
                    url: 'accounts/coa/edit',
                    data: $scope.coa,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.coas[$scope.index] = $scope.coa;
                            baseService.paginationRemove();
                            ClearFields();
                            $scope.getData();
                        }
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if ($scope.checkCOAIdUsedValue) {
            if (!baseService.isUndefinedOrNull($scope.coa.Id)) {
                $http({
                    method: 'POST',
                    url: 'accounts/coa/delete/' + $scope.coa.Id,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.coas.splice($scope.index, 1);
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
        }
        else {
            ShowResult('COA Already used', 'failure');
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.coa = {};
        $scope.coa.Active = true;
        $scope.lengthField = false;
    }
}