'use strict';
function machineCategoryController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Machine Category";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.machinecategories = [];
    $scope.getListUrl = 'Machines/machinecategory/getlist/';
    baseService.init($scope.getListUrl, null, null, null, 'Sequence', 'Sequence');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.machinecategories = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.machinecategory = {
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
        $http.get("Machines/machinecategory/getautosequence")
            .then(function (response) {
                $scope.machinecategory.Sequence = response.data;
            });
    }
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.machinecategory = $scope.machinecategories[$scope.index];
        $scope.machinecategory.AddedDate = $filter('dateFilter')($scope.machinecategory.AddedDate);
        $scope.machinecategory.UpdatedDate = $filter('dateFilter')($scope.machinecategory.UpdatedDate);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.machineCategoryForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: "Machines/machinecategory/create",
                    data: $scope.machinecategory,
                    dataType: 'JSON'
                }).then(
                    function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.machinecategories.push(response.data.MachineCategory);
                            baseService.paginationAdd();
                            ClearFields(response.data.Sequence);
                        }
                    }
                    , function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                return true;
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: "Machines/machinecategory/edit",
                    data: $scope.machinecategory,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.machinecategories[$scope.index] = $scope.machinecategory;
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
        if (!baseService.isUndefinedOrNull($scope.machinecategory.Id)) {
            $http({
                method: 'POST',
                url: "Machines/machinecategory/delete/" + $scope.machinecategory.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.machinecategories.splice($scope.index, 1);
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
        $scope.machinecategory = {};
        $scope.machinecategory.Sequence = seq;
        $scope.machinecategory.Active = true;
    }
};
machineCategoryController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
