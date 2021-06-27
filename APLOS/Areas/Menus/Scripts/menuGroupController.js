'use strict';
MenuGroupController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function MenuGroupController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Menu Group";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.menuGroups = [];
    $scope.path = 'Menus/menugroup/';
    $scope.getListUrl = $scope.path + 'getmenugrouplist';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.menuGroups = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.menuGroup = {
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
        $http.get('Menus/menugroup/getautosequence')
            .then(function (response) {
                $scope.menuGroup.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.menuGroup = $scope.menuGroups[$scope.index];
        $scope.menuGroup.AddedDate = $filter('dateFilter')($scope.menuGroup.AddedDate);
        $scope.menuGroup.UpdatedDate = $filter('dateFilter')($scope.menuGroup.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.menuGroupForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Menus/menugroup/create',
                    data: $scope.menuGroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.menuGroup = response.data.MenuGroup;
                        $scope.menuGroup.AddedDate = $filter('dateFilter')($scope.menuGroup.AddedDate);
                        $scope.menuGroups.push($scope.menuGroup);
                        $scope.menuGroups = $filter('orderBy')($scope.menuGroups, 'Sequence');
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
                    url: 'Menus/menugroup/edit',
                    data: $scope.menuGroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.menuGroups[$scope.index] = $scope.menuGroup;
                        }
                        $scope.menuGroups = $filter('orderBy')($scope.menuGroups, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.menuGroup.Id)) {
            $http({
                method: 'POST',
                url: 'Menus/menugroup/delete/' + $scope.menuGroup.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.menuGroups.splice($scope.index, 1);
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
        $scope.menuGroup = {};
        $scope.menuGroup.Sequence = seq;
        $scope.menuGroup.Active = true;
    }
}