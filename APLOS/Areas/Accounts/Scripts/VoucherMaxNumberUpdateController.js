'use strict';
VoucherMaxNumberUpdateController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function VoucherMaxNumberUpdateController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Voucher MaxNumberUpdate';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.voucherTypeConfigList = [];
    $scope.path = 'accounts/VoucherType/';
    //$scope.getVouchetTypeConfigListUrl = $scope.path + 'GetVoucherTypeConfigList';
    $scope.saveUrl = $scope.path + 'UpdateMaxNumber';

    $scope.voucherTypeConfig = {
        Id: null,
        PlantId: null,
        VoucherTypeId: null,
        Period: null,
        MaxNumber: null,
        NewMaxNumber:null
    };

    $scope.voucherTypeList = [];
    cboService.getCboVoucherType(function (result) {
        $scope.voucherTypeList = result;
    });

    $scope.periodEnumList = [];
    $scope.GetVoucherConfigPeriodCbo = function () {
        $http({
            method: "GET",
            url: "accounts/voucherType/GetVoucherConfigPeriodCbo"
        }).then(function successCallback(response) {
            $scope.periodEnumList = response.data;
        });
    };
    $scope.GetVoucherConfigPeriodCbo();
    $scope.maxNumberObj = {};
    $scope.onchangePeriod = function () {
        $scope.voucherTypeConfig.Id = null;
        $scope.voucherTypeConfig.MaxNumber = null;
        $http({
            method: "GET",
            url: "accounts/voucherType/GetVoucherMaxNumberCbo?period=" + $scope.voucherTypeConfig.Period + '&voucherTypeId=' + $scope.voucherTypeConfig.VoucherTypeId
        }).then(function successCallback(response) {
            $scope.voucherTypeConfig.Id = response.data[0].Id;
            $scope.voucherTypeConfig.MaxNumber = response.data[0].MaxNumber;
        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.voucherTypeConfigForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'id': $scope.voucherTypeConfig.Id, 'maxNUmber': $scope.voucherTypeConfig.NewMaxNumber },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Clear = function () {
        $scope.Action = 'Save';
        $scope.voucherTypeConfig = {
        };
    };
}