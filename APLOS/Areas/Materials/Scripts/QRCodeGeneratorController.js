'use strict';
QRCodeGeneratorController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function QRCodeGeneratorController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "QR Code Generate";
    $scope.Action = 'Save';
    $scope.characterlist = [];
    $scope.lengthCheck = false;
    $scope.index = -1;
    $scope.path = 'Materials/QRCodeGenerator/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.QRCodeGeneratorTemp = {
        Id: null,
        PO: null,
        Article: null,
        LOT: null,
        ProductCode: null,
        NumberOfCones: null,
        NetWeight: null
    }
    $scope.QRCodeGenerateModel = Object.assign({}, $scope.QRCodeGeneratorTemp);

    $scope.SendDataToGenerateQR = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GenerateQRCode",
            data: { 'data': $scope.QRCodeGenerateModel },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');

            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');

        }
    }
}