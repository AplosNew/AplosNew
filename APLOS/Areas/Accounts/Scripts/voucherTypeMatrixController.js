'use strict';
VoucherTypeMatrixController.$inject = ['cboService', 'commonMessage', '$window', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function VoucherTypeMatrixController(cboService, commonMessage, $window, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.path = 'accounts/VoucherTypeMatrix/';
   
    $scope.saveUrl = $scope.path + 'CreateVoucherTypeMatrix/';
    $scope.updateUrl = $scope.path + 'EditVoucherTypeMatrix/';
    $scope.deleteUrl = $scope.path + 'DeleteVoucherTypeMatrix/';

    $scope.voucherTypeMatrix = {
        Id: null,
        CompanyGroupId: null,
        VoucherTypeId: null,
        SourceType: null,
        AddedDate: new Date()
    };
    $scope.voucherTypeMatrixList = [];
  

    $scope.onCompanyGroupChange = function (item) {
        $scope.paginationShow = true;
        baseService.init('accounts/VoucherTypeMatrix/GetVoucherTypeMatrixList?companyGroupId=' + item, null, null, null, "VoucherTypeName", "VoucherTypeName");
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.voucherTypeMatrixList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };



    $scope.companyGroupList = [];
    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    $scope.voucherTypeList = [];
    cboService.getCboVoucherType(function (result) {
        $scope.voucherTypeList = result;
    });

    cboService.getEnumCbo('Enum/GetCboSourceType', function (result) {
        $scope.sourceTypeList = result;
    });

    $scope.searchByList = [
        {
            'name': 'Voucher Type',
            'value': 'VoucherTypeName'
        },
        {
            'name': 'Name',
            'value': 'SourceType'
        }
    ];

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form0.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'voucherTypeMatrix': $scope.voucherTypeMatrix },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
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
                    data: { 'voucherTypeMatrix': $scope.voucherTypeMatrix },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
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
        if (!baseService.isUndefinedOrNull($scope.voucherTypeMatrix.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.voucherTypeMatrix.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.voucherTypeMatrixList.splice($scope.index, 1);
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
        $scope.voucherTypeMatrix = $scope.voucherTypeMatrixList[$scope.index];
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Clear = function () {
        $scope.Action = 'Save';
        $scope.voucherTypeMatrix = {};
    };
}