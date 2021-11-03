'use strict';
BuyerDepartmentController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "cboService"];
function BuyerDepartmentController(commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService) {
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.buyerDepartments = [];
    $scope.getListUrl = 'Parties/buyerDepartment/getbuyerDepartmentlist';
    baseService.init($scope.getListUrl, null, 10, null, 'Sequence', 'Sequence');
    $scope.getData = function (pageno) {
        $rootScope.parameters.buyerId = $scope.buyerDepartmentNew.BuyerId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.buyerDepartments = result.Rows;
                $scope.buyerDepartmentNew = { BuyerId: $scope.buyerDepartmentNew.BuyerId, Active: $scope.buyerDepartmentNew.Active };
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.buyerDepartment = {
        Id: null,
        BuyerId: null,
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

    $scope.buyerDepartmentNew = Object.assign({}, $scope.buyerDepartment);

    $scope.buyerList = [];
    cboService.getCboBuyer(function (result) {
        $scope.buyerList = result;
    });

    $scope.GetSequence = function () {
        $http.get("Parties/buyerDepartment/getautosequence?buyerId=" + $scope.buyerDepartmentNew.BuyerId)
            .then(function (response) {
                $scope.buyerDepartmentNew.Sequence = response.data;
            });
    }

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.buyerDepartment = $scope.buyerDepartments[$scope.index];
        $scope.buyerDepartmentNew = Object.assign({}, $scope.buyerDepartment);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.buyerDepartmentNewForm.$valid) {
            angular.copy($scope.buyerDepartmentNew, $scope.buyerDepartment);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: "Parties/buyerDepartment/create",
                    data: $scope.buyerDepartment,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.buyerDepartments.push(response.data.BuyerDepartment);
                        $scope.buyerDepartments = $filter('orderBy')($scope.buyerDepartments, 'Sequence');
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
                    url: "Parties/buyerDepartment/edit",
                    data: $scope.buyerDepartment,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.buyerDepartments[$scope.index] = $scope.buyerDepartment;
                            $scope.buyerDepartments = $filter('orderBy')($scope.buyerDepartments, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.buyerDepartmentNew.Id)) {
            $http({
                method: 'POST',
                url: "Parties/buyerDepartment/delete?id=" + $scope.buyerDepartmentNew.Id + "&buyerId=" + $scope.buyerDepartmentNew.BuyerId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.buyerDepartments.splice($scope.index, 1);
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
        ClearFields($scope.GetSequence($scope.buyerDepartmentNew.BuyerId));
        return true;
    }
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.buyerDepartment = {};
        $scope.buyerDepartmentNew = { BuyerId: $scope.buyerDepartmentNew.BuyerId };
        $scope.buyerDepartmentNew.Sequence = seq;
        $scope.buyerDepartmentNew.Active = true;
        $scope.buyerDepartmentNew.IsComercialUnit = true;
    }
}