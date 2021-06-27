'use strict';
ChartOfAccountLevel1Controller.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ChartOfAccountLevel1Controller(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Chart Of Account Level 1';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.chartOfAccountLevel1s = [];
    $scope.path = 'accounts/chartofaccountlevel1/';
    $scope.getListUrl = $scope.path + 'getchartofaccountlevel1list';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.chartOfAccountLevel1s = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.chartOfAccountLevel1 = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.chartOfAccountLevel1.Sequence = response.data;
            });
    };

    $scope.CheckIdUse = function (id) {
        $http.get('accounts/chartofaccountlevel1/checkiduse?id=' + id)
            .then(function (response) {
                $scope.checkIdUsedValue = response.data;
            });
    };

    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.CheckIdUse(id);
        $scope.chartOfAccountLevel1 = $scope.chartOfAccountLevel1s[$scope.index];
        $scope.chartOfAccountLevel1.AddedDate = $filter('dateFilter')($scope.chartOfAccountLevel1.AddedDate);
        $scope.chartOfAccountLevel1.UpdatedDate = $filter('dateFilter')($scope.chartOfAccountLevel1.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.chartOfAccountLevel1Form.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.chartOfAccountLevel1,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.chartOfAccountLevel1s.push(response.data.ChartOfAccountLevel1);
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
                    data: $scope.chartOfAccountLevel1,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.chartOfAccountLevel1s[$scope.index] = $scope.chartOfAccountLevel1;
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
        if (!baseService.isUndefinedOrNull($scope.chartOfAccountLevel1.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.chartOfAccountLevel1.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.chartOfAccountLevel1s.splice($scope.index, 1);
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
        $scope.chartOfAccountLevel1 = {};
        $scope.chartOfAccountLevel1.Sequence = seq;
        $scope.chartOfAccountLevel1.Active = true;
        $scope.checkIdUsedValue = false;
    }
}