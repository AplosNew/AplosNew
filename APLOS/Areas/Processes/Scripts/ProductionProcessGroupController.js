'use strict';
ProductionProcessGroupController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter","cboService","$window"];
function ProductionProcessGroupController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter,cboService,$window) {
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.productionProcessGroups = [];
    $scope.getListUrl = 'Processes/productionProcessGroup/getList';
    baseService.init($scope.getListUrl, null, 10, null, 'Sequence', 'Sequence');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.productionProcessGroups = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.productionProcessGroup = {
        Id: null,
        CompanyGroupId: $window.companyGroupId,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        Archive: false
    };

    $scope.productionProcessGroupNew = Object.assign({}, $scope.productionProcessGroup);

    $scope.GetSequence = function () {
        $http.get("Processes/productionProcessGroup/getautosequence")
            .then(function (response) {
                $scope.productionProcessGroupNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.productionProcessGroup = $scope.productionProcessGroups[$scope.index];
        $scope.productionProcessGroupNew = Object.assign({}, $scope.productionProcessGroup);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.productionProcessGroupNewForm.$valid) {
            angular.copy($scope.productionProcessGroupNew, $scope.productionProcessGroup);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: "Processes/productionProcessGroup/create",
                    data: $scope.productionProcessGroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.productionProcessGroups.push(response.data.ProductionProcessGroup);
                        $scope.productionProcessGroups = $filter('orderBy')($scope.productionProcessGroups, 'Sequence');
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
                    url: "Processes/productionProcessGroup/edit",
                    data: $scope.productionProcessGroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.productionProcessGroups[$scope.index] = $scope.productionProcessGroup;
                            $scope.productionProcessGroups = $filter('orderBy')($scope.productionProcessGroups, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.productionProcessGroupNew.Id)) {
            $http({
                method: 'POST',
                url: "Processes/productionProcessGroup/delete/" + $scope.productionProcessGroupNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.productionProcessGroups.splice($scope.index, 1);
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
        $scope.productionProcessGroup = {};
        $scope.productionProcessGroupNew = {};
        $scope.productionProcessGroupNew.Sequence = seq;
        $scope.productionProcessGroupNew.Active = true;
    }
}