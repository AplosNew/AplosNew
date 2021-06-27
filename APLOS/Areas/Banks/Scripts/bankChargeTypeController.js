"use strict";
bankChargeTypeController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http"];
function bankChargeTypeController(cboService, commonMessage, $scope, $rootScope, baseService, $http) {
    $rootScope.title = "Bank Charge Type";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.receiveDeductions = [];
    $scope.path = "Banks/BankChargeType/";
    $scope.getUrl = $scope.path + "GetFinancingType";
    $scope.saveUrl = $scope.path + "CreateBankChargeType";
    $scope.updateUrl = $scope.path + "EditBankChargeType";
    $scope.deleteUrl = $scope.path + "DeleteFinancingType/";
    baseService.init("Banks/BankChargeType/GetBankChargeTypeList");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.receiveDeductions = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.receiveDeduction = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        AssetUserName: null,
        LiabilityUserName: null,
        SourceType: null,
        IsOthers: true,
        Description: null,
        Remarks: null,
        Active: true,
        IsAtSourceDeduction: false
    };

    $scope.getSequence = function () {
        cboService.getSequence("Banks/BankChargeType/GetBankChargeTypeAutoSequence", function (result) {
            $scope.receiveDeduction.Sequence = result;
        });
    };
    $scope.getSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.receiveDeduction = baseService.find($scope.receiveDeductions, id, null);
        $scope.receiveDeduction.IsOthers = true;
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.form0.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: $scope.receiveDeduction,
                    dataType: "JSON"
                }).then(
                    function success(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.receiveDeductions.push(response.data.ModelData);
                            baseService.paginationAdd();
                            ClearFields($scope.getSequence());
                        }
                    }, function error(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrl,
                    data: $scope.receiveDeduction,
                    dataType: "JSON"
                }).then(function success(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.receiveDeductions[$scope.index] = $scope.receiveDeduction;
                        }
                        ClearFields($scope.getSequence());
                    }
                }, function error(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.receiveDeduction.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrl + $scope.receiveDeduction.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.receiveDeductions.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields($scope.getSequence());
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
        ClearFields($scope.getSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.receiveDeduction = {};
        $scope.receiveDeduction.Sequence = seq;
        $scope.receiveDeduction.Active = true;
        $scope.receiveDeduction.IsOthers = true;
    }
}