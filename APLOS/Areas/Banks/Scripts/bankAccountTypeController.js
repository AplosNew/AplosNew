"use strict";
bankAccountTypeController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter"];
function bankAccountTypeController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Bank Account Type";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.bankAccountTypes = [];
    $scope.path = "banks/bankaccounttype/";
    $scope.getListUrl = $scope.path + "getlist";
    $scope.getUrl = $scope.path + "get";
    $scope.getSeqUrl = $scope.path + "getautosequence";
    $scope.saveUrl = $scope.path + "create";
    $scope.updateUrl = $scope.path + "edit";
    $scope.deleteUrl = $scope.path + "delete/";
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.bankAccountTypes = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.bankAccountType = {
        Id: null,
        Sequence: 0,
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
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.bankAccountType.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.bankAccountType = $scope.bankAccountTypes[$scope.index];
        $scope.bankAccountType.AddedDate = $filter("dateFilter")($scope.bankAccountType.AddedDate);
        $scope.bankAccountType.UpdatedDate = $filter("dateFilter")($scope.bankAccountType.UpdatedDate);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.bankAccountTypeForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: $scope.bankAccountType,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.bankAccountTypes.push(response.data.BankAccountType);
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallback(response) {
                    ShowResult(response.data.Message, "failure");
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrl,
                    data: $scope.bankAccountType,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.bankAccountTypes[$scope.index] = $scope.bankAccountType;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, "failure");
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.bankAccountType.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrl + $scope.bankAccountType.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.bankAccountTypes.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, "failure");
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.bankAccountType = {};
        $scope.bankAccountType.Sequence = seq;
        $scope.bankAccountType.Active = true;
    }
}