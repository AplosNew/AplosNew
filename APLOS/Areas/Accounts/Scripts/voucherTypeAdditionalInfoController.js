'use strict';
voucherTypeAdditionalInfoController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'toaster', '$compile'];
function voucherTypeAdditionalInfoController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, toaster, $compile) {
    $rootScope.title = 'GL Item';
    $scope.Action = 'Update';
    $scope.glItems = [];

    $scope.searchByVoucherTypeList = [
        {
            'name': 'Code', 'value': 'Code'
        },
        {
            'name': 'Standard Name', 'value': 'StandardName'
        },
        {
            'name': 'User Name', 'value': 'UserName'
        },
        {
            'name': 'Description', 'value': 'Description'
        },
        {
            'name': 'Category', 'value': 'Category'
        },
        {
            'name': 'GroupName', 'value': 'GroupName'
        }
    ];
    $scope.GetVoucherTypeAdditionalinfo = function () {
        $http({
            method: 'GET',
            url: 'accounts/voucherType/GetVoucherTypeAdditionalinfo'
        }).then(function successCallback(response) {
            $scope.voucherTypeAdditionalinfoList = response.data;
        });
    };

    $scope.GetVoucherTypeAdditionalinfo();

    $scope.Update = function () {
        $scope.$broadcast('show-errors-check-validity');
            if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'accounts/voucherType/UpdateVoucherTypeAdditionalInfo',
                    data: {
                        'voucherTypeAdditionalinfoList': JSON.stringify($scope.voucherTypeAdditionalinfoList),
                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        baseService.paginationAdd();
                        $scope.GetVoucherTypeAdditionalinfo();
                    }
                });
                return true;
            }
    };

   
}