'use strict';
ChartOfAccountLevel3Controller.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ChartOfAccountLevel3Controller(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Chart Of Account Level 3';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.chartOfAccountLevel3s = [];
    $scope.path = 'accounts/chartofaccountlevel3/';
    $scope.getListUrl = $scope.path + 'getchartofaccountlevel3list';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.chartOfAccountLevel3s = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.chartOfAccountLevel3 = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.chartOfAccountLevel3.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.CheckIdUse = function (id) {
        $http.get('accounts/chartofaccountlevel3/checkiduse?id=' + id)
            .then(function (response) {
                $scope.checkIdUsedValue = response.data;
            });
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.CheckIdUse(id);
        $scope.chartOfAccountLevel3 = $scope.chartOfAccountLevel3s[$scope.index];
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.chartOfAccountLevel3Form.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.chartOfAccountLevel3,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.chartOfAccountLevel3s.push(response.data.ChartOfAccountLevel3);
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
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
                    data: $scope.chartOfAccountLevel3,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.chartOfAccountLevel3s[$scope.index] = $scope.chartOfAccountLevel3;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.chartOfAccountLevel3.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.chartOfAccountLevel3.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.chartOfAccountLevel3s.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
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
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.chartOfAccountLevel3 = {};
        $scope.chartOfAccountLevel3.Sequence = seq;
        $scope.chartOfAccountLevel3.Active = true;
        $scope.checkIdUsedValue = false;
    }
}