'use strict';
PaymentModeController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function PaymentModeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Payment Mode";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.paymentModes = [];
    $scope.path = 'Setups/paymentMode/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "Sequence", "UserName");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.paymentModes = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.paymentMode = {
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
    $scope.paymentModeNew = Object.assign({}, $scope.paymentMode);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.paymentModeNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.paymentMode = $scope.paymentModes[$scope.index];
        $scope.paymentModeNew = Object.assign({}, $scope.paymentMode);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.paymentModeNew, $scope.paymentMode);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.paymentModeForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.paymentMode,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.paymentModes.push(response.data.PaymentMode);
                        $scope.paymentModes = $filter('orderBy')($scope.paymentModes, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.paymentMode,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.paymentModes[$scope.index] = $scope.paymentMode;
                            $scope.paymentModes = $filter('orderBy')($scope.paymentModes, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.paymentModeNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.paymentModeNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.paymentModes.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    }

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.paymentMode = {};
        $scope.paymentModeNew = {};
        $scope.paymentModeNew.Sequence = seq;
        $scope.paymentModeNew.Active = true;
    }
}