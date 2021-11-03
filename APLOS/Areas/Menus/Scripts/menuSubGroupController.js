'use strict';
MenuSubGroupController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function MenuSubGroupController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Menu SubGroup";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.menuSubGroups = [];
    $scope.path = 'Menus/menusubgroup/';
    $scope.getListUrl = $scope.path + 'getmenusubgrouplist';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.menuSubGroups = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.menuSubGroup = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Image: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };

    $scope.GetSequence = function () {
        $http.get('Menus/menusubgroup/getautosequence')
            .then(function (response) {
                $scope.menuSubGroup.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.menuSubGroup = $scope.menuSubGroups[$scope.index];
        $scope.menuSubGroup.AddedDate = $filter('dateFilter')($scope.menuSubGroup.AddedDate);
        $scope.menuSubGroup.UpdatedDate = $filter('dateFilter')($scope.menuSubGroup.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.menuSubGroupForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Menus/menusubgroup/create',
                    data: $scope.menuSubGroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.menuSubGroups.push(response.data.MenuSubGroup);
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
                    url: 'Menus/menusubgroup/edit',
                    data: $scope.menuSubGroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.menuSubGroups[$scope.index] = $scope.menuSubGroup;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
            return true;
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.menuSubGroup.Id)) {
            $http({
                method: 'POST',
                url: 'Menus/menusubgroup/delete/' + $scope.menuSubGroup.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.menuSubGroups.splice($scope.index, 1);
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
        $scope.menuSubGroup = {};
        $scope.menuSubGroup.Sequence = seq;
        $scope.menuSubGroup.Active = true;
    }
}