'use strict';
function OperationTypeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.operationtypes = [];
    $scope.getListUrl = 'Machines/operationtype/getlist';
    baseService.init($scope.getListUrl, null, null, null, 'Sequence', 'Sequence');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.operationtypes = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.operationtype = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardTime: null,
        UserName: null,
        Remarks: null,
        Description: null,
        Active: true,
        Archive: false
    };
    $scope.operationtypeNew = Object.assign({}, $scope.operationtype);


    $scope.GetSequence = function () {
        $http.get("Machines/operationtype/getautosequence")
            .then(function (response) {
                $scope.operationtypeNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.operationtype = $scope.operationtypes[$scope.index];
        $scope.operationtypeNew = Object.assign({}, $scope.operationtype);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        for (var i in $scope.operationtypeNew) {
            $scope.operationtype[i] = $scope.operationtypeNew[i];
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.operationtypeNewForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: "Machines/operationtype/create",
                    data: $scope.operationtype,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.operationtype = response.data.OperationType;
                        $scope.operationtype.AddedDate = $filter("dateFilter")($scope.operationtype.AddedDate);
                        $scope.operationtypes.push($scope.operationtype);
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: "Machines/operationtype/edit",
                    data: $scope.operationtype,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.operationtype.AddedDate = $filter("dateFilter")($scope.operationtype.AddedDate);
                        if ($scope.index > -1) {
                            $scope.operationtypes[$scope.index] = $scope.operationtype;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.operationtype.Id)) {
            $http({
                method: 'POST',
                url: "Machines/operationtype/delete/" + $scope.operationtype.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.operationtypes.splice($scope.index, 1);
                    baseService.paginationRemove();
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
        $scope.operationtype = {};
        $scope.operationtypeNew = {};
        $scope.operationtypeNew.Sequence = seq;
        $scope.operationtypeNew.Active = true;
    }
};
OperationTypeController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
