'use strict';
function MachineSubClassController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Machine SubClass";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.machinesubclasses = [];
    $scope.getListUrl = 'Machines/machinesubclass/getlist/';
    baseService.init($scope.getListUrl, null, 10, null, 'Sequence', 'Sequence');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.machinesubclasses = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.machinesubclass = {
        Id: null,
        Sequence: null,
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
        $http.get("Machines/machinesubclass/getautosequence")
            .then(function (response) {
                $scope.machinesubclass.Sequence = response.data;
            });
    }
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.machinesubclass = $scope.machinesubclasses[$scope.index];
        $scope.machinesubclass.AddedDate = $filter('dateFilter')($scope.machinesubclass.AddedDate);
        $scope.machinesubclass.UpdatedDate = $filter('dateFilter')($scope.machinesubclass.UpdatedDate);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.machinesubclassForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: "Machines/machinesubclass/create",
                    data: $scope.machinesubclass,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.machinesubclasses.push(response.data.MachineSubClass);
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
                    url: "Machines/machinesubclass/edit",
                    data: $scope.machinesubclass,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.machinesubclasses[$scope.index] = $scope.machinesubclass;
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
        if (!baseService.isUndefinedOrNull($scope.machinesubclass.Id)) {
            $http({
                method: 'POST',
                url: "Machines/machinesubclass/delete/" + $scope.machinesubclass.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.machinesubclasses.splice($scope.index, 1);
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
        $scope.machinesubclass = {};
        $scope.machinesubclass.Sequence = seq;
        $scope.machinesubclass.Active = true;
    }
};
MachineSubClassController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
