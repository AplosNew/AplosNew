"use strict";
activityPhoneController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$controller"];
function activityPhoneController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = "Activity Phone";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.activityPhones = [];
    $scope.path = "accounts/activityphone/";
    $scope.saveUrl = $scope.path + "create";
    $scope.updateUrl = $scope.path + "edit";
    $scope.deleteUrl = $scope.path + "delete/";
    $scope.getListUrl = $scope.path + "getlist";

    $scope.partyType = "Vendor";
    $scope.isAdvance = false;
    $scope.partyUrl = "Parties/party/GetCompanyPartyDataList?partyType=Vendor";
    $controller("partyBaseController", { $scope: $scope, $http: $http });

    baseService.init($scope.getListUrl, null, null, null, "CellNumber", "CellNumber");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.activityPhones = result.Rows;
                console.log($scope.activityPhones);
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.activityPhone = {
        Id: null,
        PartyId: null,
        ActivityId: null,
        CellNumber: null,
        IsLandPhone: false,
        Active: true
    };

    cboService.getCboActivityPhone(function (result) {
        $scope.activityList = result;
    });

    $rootScope.searchByActivityList = [
        {
            "name": "Cell Number",
            "value": "CellNumber"
        }
    ];

    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            $scope.activityPhone.PartyName = party.Code + " - " + party.UserName;
            $scope.activityPhone.PartyId = party.Id;
        }
        $scope.hidePartyPopUp();
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.activityPhone = $scope.activityPhones[$scope.index];
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.activityPhoneForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: $scope.activityPhone,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        //baseService.paginationAdd();
                        ClearFields();
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
                    data: $scope.activityPhone,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.activityPhones[$scope.index] = $scope.activityPhone;
                            baseService.paginationAdd();
                        }
                        ClearFields();
                    }
                });
                return true;
            }
        }
        return true;
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.activityPhone.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrl + $scope.activityPhone.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.activityPhones.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.activityPhone = {};
        $scope.activityPhone.Active = true;
    }

    $scope.valueData = "";
    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    };

    $scope.SelectByButton = function () {
        if ($scope.valueData === "") {
            alert("Please at first select row");
            return;
        }
        $scope.selectdblClick($scope.valueData);
        $scope.valueData = "";
        angular.element(document.querySelector("#popUp")).modal("hide");
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector("#popUp")).modal("hide");
    };
}