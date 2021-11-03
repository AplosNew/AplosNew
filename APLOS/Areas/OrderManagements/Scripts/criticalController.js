'use strict';
CriticalController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter","cboService"];
function CriticalController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter,cboService) {
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.criticals = [];
    $scope.getListUrl = 'OrderManagements/critical/getList';
    baseService.init($scope.getListUrl, null, 10, null, 'Sequence', 'Sequence');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.criticals = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.critical = {
        Id: null,
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

    $scope.criticalNew = Object.assign({}, $scope.critical);

    $scope.GetSequence = function () {
        $http.get("OrderManagements/critical/getautosequence")
            .then(function (response) {
                $scope.criticalNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.critical = $scope.criticals[$scope.index];
        $scope.criticalNew = Object.assign({}, $scope.critical);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.criticalNewForm.$valid) {
            angular.copy($scope.criticalNew, $scope.critical);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: "OrderManagements/critical/create",
                    data: $scope.critical,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.criticals.push(response.data.Critical);
                        $scope.criticals = $filter('orderBy')($scope.criticals, 'Sequence');
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
                    url: "OrderManagements/critical/edit",
                    data: $scope.critical,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.criticals[$scope.index] = $scope.critical;
                            $scope.criticals = $filter('orderBy')($scope.criticals, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.criticalNew.Id)) {
            $http({
                method: 'POST',
                url: "OrderManagements/critical/delete/" + $scope.criticalNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.criticals.splice($scope.index, 1);
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
        $scope.critical = {};
        $scope.criticalNew = {};
        $scope.criticalNew.Sequence = seq;
        $scope.criticalNew.Active = true;
    }
}