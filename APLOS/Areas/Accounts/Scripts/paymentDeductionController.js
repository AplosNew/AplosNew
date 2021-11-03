'use strict';
paymentDeductionController.$inject = ['cboService', '$route', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function paymentDeductionController(cboService, $route, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Payment Deduction';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.paymentDeductions = [];
    $scope.path = 'accounts/FinancingType/';
    $scope.getUrl = $scope.path + 'GetFinancingType';
    $scope.saveUrl = $scope.path + 'CreatePaymentDeduction';
    $scope.updateUrl = $scope.path + 'EditPaymentDeduction';
    $scope.deleteUrl = $scope.path + 'DeleteFinancingType/';
    baseService.init('accounts/FinancingType/GetPaymentDeductionList');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.paymentDeductions = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.paymentDeduction = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        AssetUserName: null,
        LiabilityUserName: null,
        RevenueUserName: null,
        SourceType: null,
        IsOthers: true,
        Description: null,
        Remarks: null,
        Active: true
    };

    $scope.getSequence = function () {
        cboService.getSequence('accounts/FinancingType/GetPaymentDeductionTypeAutoSequence', function (result) {
            $scope.paymentDeduction.Sequence = result;
        });
    };
    $scope.getSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.paymentDeduction = baseService.find($scope.paymentDeductions, id, null);
        $scope.paymentDeduction.IsOthers = true;
        $scope.paymentDeduction.AddedDate = $filter('dateFilter')($scope.paymentDeduction.AddedDate);
        $scope.paymentDeduction.UpdatedDate = $filter('dateFilter')($scope.paymentDeduction.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.moduleForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.paymentDeduction,
                    dataType: 'JSON'
                }).then(
                    function success(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.paymentDeductions.push(response.data.ModelData);
                            baseService.paginationAdd();
                            ClearFields(response.data.Sequence);
                        }
                    }, function error(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.paymentDeduction,
                    dataType: 'JSON'
                }).then(function success(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.paymentDeductions[$scope.index] = $scope.paymentDeduction;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function error(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.paymentDeduction.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.paymentDeduction.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.paymentDeductions.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields($scope.getSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.paymentDeduction = {};
        $scope.paymentDeduction.Sequence = seq;
        $scope.paymentDeduction.Active = true;
        $scope.paymentDeduction.IsOthers = true;
    }
}