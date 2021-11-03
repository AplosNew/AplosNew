'use strict';
FinancialStatusCustomerReceivableInvoiceDetailController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function FinancialStatusCustomerReceivableInvoiceDetailController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Financial Status Customer Receivable Invoice Detail';
    $scope.Action = 'Save';
    $scope.MasterLCList = [];
    $scope.path = 'Accounts/Voucher/';
    //$scope.reportParameters = { FromDate: null, ToDate: null };

    //for tab
    // $scope.tab = 1;
    //$scope.setTab = function (newTab) {
    //    $scope.tab = newTab;
    //};
    //$scope.isSet = function (tabNum) {
    //    return $scope.tab === tabNum;
    //};

    $scope.CustomerReceivableInvoiceDetailList = [];
    $scope.getCustomerReceivableInvoiceDetailData = function (id) {
        $http({
            //var data = obj.data;
            method: 'GET',
            url: 'Accounts/AccountStatusDashboard/getCustomerReceivableInvoiceDetailData?partyId=' + id
        }).then(function successCallback(response) {
            //if (baseService.arrayLength(response.data) > 0) {
            //    $scope.CustomerReceivableInvoiceDetailList = response.data;
            //}
            $scope.CustomerReceivableInvoiceDetailList = response.data;
            $scope.partyName = $scope.CustomerReceivableInvoiceDetailList[0].PartyName
            
        });
    }

    function Get(id) {
        $scope.getCustomerReceivableInvoiceDetailData(id);
    }

    Get($routeParams.id);

    //Voucher Report Print
    $scope.printCRInvoiceDetailVoucherReport = function (objcrvno) {
        var data = objcrvno.data;
        if (data.SourceType == 'CustomerInvoice')
           // Accounts / Invoice / GetCustomerInvoiceVoucherReport ? reportFormat = Pdf & voucherId=20212914
            var file_src = 'Accounts/Invoice/GetCustomerInvoiceVoucherReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;

        if (data.SourceType == 'InventoryPayable')
            var file_src = 'Accounts/InventoryPayable/PabyableJournal?reportFormat=' + 'Excel' + '&inventoryReceiveId=' + data.InventoryReceiveId + '&employeeId=' + null + '&isReversCharge=' + false + '&isFoc=' + false;
        //Accounts / invoice / CustomerInvoiceReceiptReport ? reportFormat = Pdf & voucherId=20212915
        if (data.SourceType == 'CustomerReceipt')
            var file_src = 'Accounts/invoice/CustomerInvoiceReceiptReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;
        //<li><a class="btn btn-action" href="Accounts/invoice/CustomerInvoiceReceiptBanksReport?reportFormat=@Library.Model.Enums.ReportFormat.Excel.ToString()&invoiceWriteOffGroupNo={{x.InvoiceWriteOffGroupNo}}" target="_blank"><i class="fa fa-file-excel-o"></i></a></li>

        if (data.SourceType == 'CustomerBanksReceipt')
            var file_src = 'Accounts/invoice/CustomerInvoiceReceiptBanksReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;

        //if (data.SourceType == 'CashJournal')
        //    var file_src = 'Banks/CashReport/GetCashJournalReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;
        //if (data.SourceType == 'BankJournal')
        //    var file_src = 'Banks/BankReport/GetBankJournalReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;

        $rootScope.report(file_src);
    
    }

    $scope.Back = function () { 
        $window.history.back();
    };


    //Grid View for Party Payment status Detail 
    //$scope.DetailDataList = [];
    //$scope.getDetailData = function (obj) {
    //    var data = obj.data;
    //    $http({
    //        method: 'GET',
    //        url: 'Accounts/Voucher/getDetailData?partyId=' + data.PartyId
    //    }).then(function successCallback(response) {
    //        if (baseService.arrayLength(response.data) > 0) {
    //            $scope.DetailDataList = response.data;
    //        }
    //    });
    //}

}


