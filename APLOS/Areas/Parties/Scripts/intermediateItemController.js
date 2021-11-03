'use strict';
IntermediateItemController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "cboService"];
function IntermediateItemController(commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService) {
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.intermediateItems = [];
    $scope.getListUrl = 'Parties/intermediateItem/getList';
    baseService.init($scope.getListUrl, null, 10, null, 'Sequence', 'Sequence');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.intermediateItems = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.intermediateItem = {
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

    $scope.intermediateItemNew = Object.assign({}, $scope.intermediateItem);

    $scope.GetSequence = function () {
        $http.get("Parties/intermediateItem/getautosequence")
            .then(function (response) {
                $scope.intermediateItemNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.intermediateItem = $scope.intermediateItems[$scope.index];
        $scope.intermediateItemNew = Object.assign({}, $scope.intermediateItem);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.intermediateItemNewForm.$valid) {
            angular.copy($scope.intermediateItemNew, $scope.intermediateItem);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: "Parties/intermediateItem/create",
                    data: $scope.intermediateItem,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.intermediateItems.push(response.data.IntermediateItem);
                        $scope.intermediateItems = $filter('orderBy')($scope.intermediateItems, 'Sequence');
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
                    url: "Parties/intermediateItem/edit",
                    data: $scope.intermediateItem,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.intermediateItems[$scope.index] = $scope.intermediateItem;
                            $scope.intermediateItems = $filter('orderBy')($scope.intermediateItems, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.intermediateItemNew.Id)) {
            $http({
                method: 'POST',
                url: "Parties/intermediateItem/delete/" + $scope.intermediateItemNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.intermediateItems.splice($scope.index, 1);
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
        $scope.intermediateItem = {};
        $scope.intermediateItemNew = {};
        $scope.intermediateItemNew.Sequence = seq;
        $scope.intermediateItemNew.Active = true;
    }
}