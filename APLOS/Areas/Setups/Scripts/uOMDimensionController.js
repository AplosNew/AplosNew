'use strict';
UOMDimensionController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function UOMDimensionController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Unit Of Measurement Dimension';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.uomdimensions = [];
    $scope.getListUrl = "Setups/uomdimension/getlist/";
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.uomdimensions = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.uomdimension = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Active: true
    };
    $scope.uomdimensionNew = Object.assign({}, $scope.uomdimension);

    $scope.GetSequence = function () {
        $http.get("Setups/uomdimension/getautosequence")
            .then(function (response) {
                $scope.uomdimensionNew.Sequence = response.data;
            });
    }

    $scope.GetSequence();
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.uomdimension = $scope.uomdimensions[$scope.index];
        $scope.uomdimensionNew = Object.assign({}, $scope.uomdimension);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.uomdimensionForm.$valid) {
            angular.copy($scope.uomdimensionNew, $scope.uomdimension);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: 'Setups/uomdimension/create',
                    data: $scope.uomdimension,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.uomdimensions.push(response.data.UOMDimension);
                        $scope.uomdimensions = $filter('orderBy')($scope.uomdimensions, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: 'Setups/uomdimension/edit',
                    data: $scope.uomdimension,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.uomdimensions[$scope.index] = $scope.uomdimension;
                            $scope.uomdimensions = $filter('orderBy')($scope.uomdimensions, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.uomdimensionNew.Id)) {
            $http({
                method: 'POST',
                url: "Setups/uomdimension/delete/" + $scope.uomdimensionNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.uomdimensions.splice($scope.index, 1);
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    }
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    }
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.uomdimension = {};
        $scope.uomdimensionNew = {};
        $scope.uomdimensionNew.Active = true;
        $scope.uomdimensionNew.Sequence = seq;
    }
}