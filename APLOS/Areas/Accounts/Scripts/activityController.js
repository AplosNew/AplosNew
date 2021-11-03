"use strict";
activityController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function activityController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Activity";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.activities = [];
    $scope.path = "accounts/activity/";
    $scope.saveUrl = $scope.path + "create";
    $scope.updateUrl = $scope.path + "edit";
    $scope.deleteUrl = $scope.path + "delete/";
    $scope.getListUrl = "accounts/CompanyGroupActivity/getlist";
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.activities = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.activity = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        ActivityType: null,
        ActivityId: null,
        FALinked: null,
        Description: null,
        Remarks: null,
        Active: true
    };

    $scope.GetSequence = function () {
        cboService.getSequence("accounts/activity/getautosequence", function (result) {
            $scope.activity.Sequence = result;
        });
    };
    $scope.GetSequence();

    cboService.getEnumCbo("enum/GetActivityTypeCbo", function (result) {
        $scope.activityTypeList = result;
    });

    cboService.getEnumCbo("enum/GetCboFALinked", function (result) {
        $scope.fALinkedList = result;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.activity = $scope.activities[$scope.index];
        $scope.activity.AddedDate = $filter("dateFilter")($scope.activity.AddedDate);
        $scope.activity.UpdatedDate = $filter("dateFilter")($scope.activity.UpdatedDate);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.invalidFALink = false;
    $scope.fALinkValidation = function () {
        $scope.invalidFALink = baseService.isUndefinedOrNull($scope.activity.FALinked);
        return manualValidation("div_FA", $scope.invalidFALink, "FA Link is required.");
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.activity.IsFABased) {
            $scope.fALinkValidation();
        }
        if ($scope.activityForm.$valid && !$scope.invalidFALink) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: $scope.activity,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.activities.push(response.data.Activity);
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrl,
                    data: $scope.activity,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.activities[$scope.index] = $scope.activity;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
        }
        return true;
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.activity.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrl + $scope.activity.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.activities.splice($scope.index, 1);
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
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.activity = {};
        $scope.activity.Sequence = seq;
        $scope.activity.Active = true;
    }
}