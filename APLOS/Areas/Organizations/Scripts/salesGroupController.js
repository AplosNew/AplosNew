'use strict';
function SalesGroupController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Sales Group";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.salesGroups = [];
    $scope.path = 'Organizations/salesgroup/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "Sequence", "UserName");
    $scope.getData = function (pageno) {
        $rootScope.parameters.salesOrganizationId = $scope.salesGroupNew.SalesOrganizationId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.salesGroups = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.salesGroup = {
        Id: null,
        SalesOrganizationId: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };

    $scope.salesGroupNew = Object.assign({}, $scope.salesGroup);
    $http({
        method: 'GET',
        url: 'Organizations/salesorganisation/getcbo'
    }).then(function successCallback(response) {
        $scope.salesOrganizationList = response.data;
    });
    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.salesGroupNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.salesGroup = $scope.salesGroups[$scope.index];
        $scope.salesGroupNew = Object.assign({}, $scope.salesGroup);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.salesGroupNew, $scope.salesGroup);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.salesGroupNewForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.salesGroup,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.salesGroups.push(response.data.SalesGroup);
                        $scope.salesGroups = $filter('orderBy')($scope.salesGroups, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.salesGroup,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.salesGroups[$scope.index] = $scope.salesGroup;
                            $scope.salesGroups = $filter('orderBy')($scope.salesGroups, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.salesGroupNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.salesGroupNew.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.salesGroups.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.salesGroup = {};
        $scope.salesGroupNew = { SalesOrganizationId: $scope.salesGroupNew.SalesOrganizationId };
        $scope.salesGroupNew.Sequence = seq;
        $scope.salesGroupNew.Active = true;
    }
}
SalesGroupController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];