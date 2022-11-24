'use strict';
voucherTypeConfigController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function voucherTypeConfigController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Voucher Type Config';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.voucherTypeConfigList = [];
    $scope.path = 'accounts/VoucherTypeConfig/';
    $scope.getVouchetTypeConfigListUrl = $scope.path + 'GetVoucherTypeConfigList';
    $scope.saveUrl = $scope.path + 'CreateVoucherTypeConfig';
    $scope.updateUrl = $scope.path + 'UpdateVoucherTypeConfig';
    $scope.deleteUrl = $scope.path + 'DeleteVoucherTypeConfig/';

    $scope.voucherTypeConfig = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null,
        VoucherTypeId: null,
        Period: null,
        Prefix: null,
        PadLeftWidth: null,
        PadLeftChar: null,
        IsBackDatePostingAllow: false
    };

    $scope.companyGroupList = [];
    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });
    $scope.companyList = [];
    $scope.companyLoad = function () {
        cboService.getCboCompanyByCompanyGroup($scope.voucherTypeConfig.CompanyGroupId, function (result) {
            $scope.companyList = result;
        });
    }
   

    $scope.plantList = [];
    $scope.getPlantList = function () {
        cboService.getCboPlantByCompany($scope.voucherTypeConfig.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };

    $scope.voucherTypeList = [];
    cboService.getCboVoucherType(function (result) {
        $scope.voucherTypeList = result;
    });

    $scope.periodEnumList = [];
    cboService.getEnumCbo("enum/GetPeriodListCbo", function (result) {
        $scope.periodEnumList = result;
    });

    $scope.getPadding = function () {
        $scope.paddingList = [];
        for (var i = 0; i < 10; i++) {
            $scope.paddingList.push({
                'Text': i,
                'Value': i
            });
        }
    };
    $scope.getPadding();

    $scope.searchByList = [
        {
            'name': 'Voucher Type',
            'value': 'VoucherTypeName'
        },
        {
            'name': 'Period',
            'value': 'Period'
        },
        {
            'name': 'Prefix',
            'value': 'Prefix'
        },
        {
            'name': 'PadLeft Length',
            'value': 'PadLeftWidth'
        }
    ];

    $scope.getListData = function () {
        baseService.init('accounts/VoucherTypeConfig/GetVoucherTypeConfigList?companyId=' + $scope.voucherTypeConfig.CompanyId + '&plantId=' + $scope.voucherTypeConfig.PlantId, null, null, null, 'VoucherTypeName', 'VoucherTypeName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.voucherTypeConfigList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.voucherTypeConfigForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'voucherTypeConfige': $scope.voucherTypeConfig },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getListData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: { 'voucherTypeConfige': $scope.voucherTypeConfig },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getListData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.voucherTypeConfig.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.voucherTypeConfig.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.voucherTypeConfigList.splice($scope.index, 1);
                    baseService.paginationRemove();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Get = function (index) {
        $scope.index = index;
        $scope.voucherTypeConfig = $scope.voucherTypeConfigList[$scope.index];
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Clear = function () {
        $scope.Action = 'Save';
        $scope.voucherTypeConfig = {
            CompanyId: $scope.voucherTypeConfig.CompanyId
            , PlantId: $scope.voucherTypeConfig.PlantId
        };
    };
}