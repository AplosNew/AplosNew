'use strict';
BuyerDivisionController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "cboService"];
function BuyerDivisionController(commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService) {
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.buyerDivisions = [];
    $scope.getListUrl = 'Parties/buyerDivision/getbuyerDivisionlist';
    baseService.init($scope.getListUrl, null, 10, null, 'Sequence', 'Sequence');
    $scope.getData = function (pageno) {
        $rootScope.parameters.buyerId = $scope.buyerDivisionNew.BuyerId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.buyerDivisions = result.Rows;
                $scope.buyerDivisionNew = { BuyerId: $scope.buyerDivisionNew.BuyerId, Active: $scope.buyerDivisionNew.Active };
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.buyerDivision = {
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

    $scope.buyerDivisionNew = Object.assign({}, $scope.buyerDivision);

    $scope.buyerList = [];
    cboService.getCboBuyer(function (result) {
        $scope.buyerList = result;
    });

    $scope.GetSequence = function () {
        $http.get("Parties/buyerDivision/getautosequence?buyerId=" + $scope.buyerDivisionNew.BuyerId)
            .then(function (response) {
                $scope.buyerDivisionNew.Sequence = response.data;
            });
    }

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.buyerDivision = $scope.buyerDivisions[$scope.index];
        $scope.buyerDivisionNew = Object.assign({}, $scope.buyerDivision);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.buyerDivisionNewForm.$valid) {
            angular.copy($scope.buyerDivisionNew, $scope.buyerDivision);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: "Parties/buyerDivision/create",
                    data: $scope.buyerDivision,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.buyerDivisions.push(response.data.BuyerDivision);
                        $scope.buyerDivisions = $filter('orderBy')($scope.buyerDivisions, 'Sequence');
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
                    url: "Parties/buyerDivision/edit",
                    data: $scope.buyerDivision,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.buyerDivisions[$scope.index] = $scope.buyerDivision;
                            $scope.buyerDivisions = $filter('orderBy')($scope.buyerDivisions, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.buyerDivisionNew.Id)) {
            $http({
                method: 'POST',
                url: "Parties/buyerDivision/delete?id=" + $scope.buyerDivisionNew.Id + "&buyerId=" + $scope.buyerDivisionNew.BuyerId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.buyerDivisions.splice($scope.index, 1);
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
        $scope.buyerDivision = {};
        $scope.buyerDivisionNew = { BuyerId: $scope.buyerDivisionNew.BuyerId };
        $scope.buyerDivisionNew.Sequence = seq;
        $scope.buyerDivisionNew.Active = true;
        $scope.buyerDivisionNew.IsComercialUnit = true;
    }
}