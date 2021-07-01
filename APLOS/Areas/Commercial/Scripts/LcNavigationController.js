'use strict';
LcNavigationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function LcNavigationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'LC Navigation';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.MasterLCList = [];
    $scope.path = 'Commercial/LcNavigation/';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportJson';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';
    $scope.getListUrl = $scope.path + 'getlist';

    $scope.getPurcheseLcReport = function () {
        try {
            var file_src = $scope.path + 'GetPurchaseLCReport';
            $rootScope.report(file_src);

        } catch (e) {
        }
    }




    $scope.LCGrid = {
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now()),
        Type: 'Search'
    };
    $scope.PurchaseLCList = [];  
    $scope.LoadLCGrid = function () {
        if ($scope.LCGrid.Type == 'SearchByDate') {
            $http({
                method: 'POST',
                url: $scope.path + "GetPurchaseLCSearchByDate",
                data: { 'fromDate': $scope.LCGrid.FromDate, 'toDate': $scope.LCGrid.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {
              
                if (response.data.Error == false) {
                    for (var i = 0; i < response.data.DATA.length; i++) {
                        response.data.DATA[i].OpeningDate = new Date(response.data.DATA[i].OpeningDate);
                    }
                    $scope.PurchaseLCList = response.data.DATA;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }
        else {
            $scope.PurchaseLCList = [];
            $http({
                method: 'POST',
                url: $scope.path + "GetList",
                data: { 'column': $scope.LCsearchBy, 'value': $scope.LCsearch },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                $scope.PurchaseLCList = response.data;
            });

        }
    }
    $scope.PurchaseLCPOList = [];
    $scope.SelectedLCRow = {};
    $scope.LoadPOList = function (LCData) {
        $scope.SelectedLCRow = LCData;
        $http({
            method: 'POST',
            url: $scope.path + "GetPurchaseLCPOList",
            data: { 'PurchaseLCId': LCData.LCId },
            dataType: 'JSON'

        })

            .then(function successCallback(response) {
                if (response.data.Error == false) {                   
                    $scope.PurchaseLCPOList = response.data.PODATA;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }

            }),
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        $rootScope.openPopupAngular('POPopup');
    }

    $scope.PurchaseLCGRNList = [];
    $scope.LoadGRNList = function (LCGRNData) {
        $scope.SelectedLCRow = LCGRNData;

        $http({
            method: 'POST',
            url: $scope.path + "GetPurchaseLCGRNList",
            data: { 'PurchaseLCId': LCGRNData.LCId },
            dataType: 'JSON'

        })
            .then(function successCallback(response) {
                if (response.data.Error == false) {
                    $scope.PurchaseLCGRNList = response.data.GRNDATA;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }),
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        $rootScope.openPopupAngular('GRNPopup');
    }

    $scope.PurchaseLCACList = [];
    $scope.LoadACList = function (LCACData) {
        $scope.SelectedLCRow = LCACData;

        $http({
            method: 'POST',
            url: $scope.path + "GetPurchaseLCACList",
            data: { 'PurchaseLCId': LCACData.LCId },
            dataType: 'JSON'

        })
            .then(function successCallback(response) {
                if (response.data.Error == false) {

                    $scope.PurchaseLCACList = response.data.ACDATA;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }),
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        $rootScope.openPopupAngular('ACPopup');
    }

    $scope.PurchaseLCLoanList = [];
    $scope.LoadLoanList = function (LCLoanData) {
        $scope.SelectedLCRow = LCLoanData;

        $http({
            method: 'POST',
            url: $scope.path + "GetPurchaseLCLoanList",
            data: { 'PurchaseLCId': LCLoanData.LCId },
            dataType: 'JSON'

        })
            .then(function successCallback(response) {
                if (response.data.Error == false) {

                    $scope.PurchaseLCLoanList = response.data.LoanDATA;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }),
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        $rootScope.openPopupAngular('LoanPopup');
    }

    $scope.searchCol = "";
    $scope.searchVal = "";
    $scope.LCsearchBy = "LCNo";
    $scope.LCsearch = "";
    $scope.LCFilterList = [
       /* { 'name': 'Purchase LC Id', 'value': 'LCId' },*/
        { 'name': 'Purchase LC No.', 'value': 'LCNo' },
        { 'name': 'Opening Bank', 'value': 'OpeningBank' },
        { 'name': 'Opening Date', 'value': 'OpeningDate' },
        { 'name': 'Vendor', 'value': 'Vendor' },
        { 'name': 'Value', 'value': 'Value' },
        { 'name': 'Currency', 'value': 'Currency' },
        { 'name': 'LCA No', 'value': 'LCANo' },
        { 'name': 'LC Type', 'value': 'LCType' },
        { 'name': 'Tenure', 'value': 'Tenure' },
        { 'name': 'Benificiary Bank', 'value': 'BenificiaryBank' },
        { 'name': 'PO Value', 'value': 'POValue' },
        { 'name': 'Acceptance Value', 'value': 'AcceptanceValue' },
       /* { 'name': 'GRN Count', 'value': 'GRNCount' },*/
        { 'name': 'GRN Value', 'value': 'GRNValue' },
        /*{ 'name': 'Payment Made', 'value': 'PaymentMade' },*/
        { 'name': 'Contract No', 'value': 'ContractNo' },
        { 'name': 'Customer', 'value': 'Customer' },
    ];
}


