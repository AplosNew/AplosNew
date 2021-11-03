'use strict';
partyPaymentStatusDetailController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function partyPaymentStatusDetailController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Party Payment Status Detail';
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

    $scope.DetailDataList = [];
    $scope.getDetailData = function (id) {
        $http({
            //var data = obj.data;
            method: 'GET',
            url: 'Accounts/Voucher/getDetailData?partyId=' + id
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.DetailDataList = response.data;
                $scope.partyName = $scope.DetailDataList[0].PartyName
                   // $scope.partyName = $scope.CustomerReceivableInvoiceDetailList[0].PartyName
            }
        });
    }

    function Get(id) {
        $scope.getDetailData(id);
    }

    Get($routeParams.id);

    //Voucher Report Print
    $scope.printVoucherReport = function (obj) {
        var data = obj.data;
        if (data.SourceType == 'VendorInvoice')
            var file_src = 'Accounts/Invoice/ReportVendorInvoice?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;
        if (data.SourceType == 'InventoryPayable')
            var file_src = 'Accounts/InventoryPayable/PabyableJournal?reportFormat=' + 'Excel' + '&inventoryReceiveId=' + data.InventoryReceiveId + '&employeeId=' + null + '&isReversCharge=' + false + '&isFoc=' + false;

        if (data.SourceType == 'VendorPayment')
            var file_src = 'Accounts/invoice/VendorInvoicePaymentReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;
        //if (data.SourceType == 'EmployeePayment')
        //    var file_src = 'Employees/EmployeeReport/GetEmployeePaymentReport?reportFormat=' + 'Excel' + '&voucherId=' + data.VoucherId;
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


