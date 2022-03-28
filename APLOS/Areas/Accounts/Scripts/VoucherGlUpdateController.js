'use strict';
VoucherGlUpdateController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function VoucherGlUpdateController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Voucher GL Update';
    $scope.Action = 'Save';
    $scope.path = 'Accounts/VoucherGlUpdate/';
    $scope.url = "Accounts/VoucherGlUpdate";
    $scope.parkUrl = $scope.url + "/parkModeVoucher";
    $scope.saveUrl = $scope.path + 'create';
    var dt = new Date();

    $scope.voucher = {
        Id: null,
        VoucherNo: null
    };


    $scope.VoucherDataList = [];
    $scope.getVoucherData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "getVoucherDataList",
                data: { voucherNo: $scope.voucher.VoucherNo},
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.VoucherDataList = response.data.DATA;

            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
   // $scope.getVoucherData();

    $scope.voucherId = null;
    $scope.confirmPost = function (voucherId,sourceType) {
        $scope.voucherId = voucherId;
        $scope.sourceType = sourceType;
        $scope.message_confirmation = "Are you sure to Park Mode?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };
    $scope.park = function (vId, vSourceType) {
        $http({
            method: "POST",
            url: $scope.parkUrl,
            data: {
                "voucherId": vId,
                "sourceType": vSourceType
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getVoucherData();
                //$scope.Clear();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };
};






