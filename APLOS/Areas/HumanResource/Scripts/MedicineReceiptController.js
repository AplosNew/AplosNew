'use strict';
MedicineReceiptController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function MedicineReceiptController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Medicine Receipt';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/MedicineReceipt/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.saveUrlP = $scope.path + 'SavePurpose';
    $scope.deleteUrl = $scope.path + 'Delete/';
    baseService.init($scope.getListUrl);
    $scope.downloadgriddataUrl = 'GridReports/Download';

    var curDate = new Date()
    $scope.ModelTemp = {
        Id: null,
        InvoiceDate: curDate
    };
    $scope.ModalNew = Object.assign({}, $scope.ModelTemp);

    $scope.MedicineList = [];

    $scope.getMedicineData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getMedicineData',
            dataType:'JSON'
        }).then(function successCallback(response) {
            $scope.MedicineList = response.data;
        });
    }
    $scope.getMedicineData();
}