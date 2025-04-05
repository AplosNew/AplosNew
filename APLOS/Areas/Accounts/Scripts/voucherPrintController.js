'use strict';
voucherPrintController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function voucherPrintController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'VoucherPark';
    $scope.Action = 'Save';
    $scope.path = 'Accounts/VoucherPark/';
    $scope.url = "Accounts/VoucherPark";
    
    $scope.voucher = {
        Id: null,
        VoucherNo: null
    };


    $scope.VoucherDataList = [];
    $scope.getVoucherData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetVoucherDataListforPrint",
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

    $scope.PrintReport = function (reportFormat, SourceType, voucherId, beneficiaryType) {
        if (SourceType == 'VendorInvoice') {
            if (beneficiaryType == 'Vendor') {
                $window.open('Accounts/Invoice/ReportVendorInvoice?reportFormat=' + reportFormat + '&voucherId=' + voucherId, '_blank');
            }
            else {
                $window.open('Employees/EmployeeReport/GetEmployeePayableExpenseReport?reportFormat=' + reportFormat + '&voucherId=' + voucherId, '_blank');
            }
        }
        else if (SourceType == 'BankJournal') {
            $window.open('Banks/BankReport/GetBankJournalReport?reportFormat=' + reportFormat + '&voucherId=' + voucherId, '_blank');
        }
        else if (SourceType == 'CashJournal') {
            $window.open('Banks/CashReport/GetCashJournalReport?reportFormat=' + reportFormat + '&voucherId=' + voucherId, '_blank');
        }
        else {
            $window.open('Accounts/Invoice/ReportVendorInvoice?reportFormat=' + reportFormat + '&voucherId=' + voucherId, '_blank');
        }
    }
  
};






