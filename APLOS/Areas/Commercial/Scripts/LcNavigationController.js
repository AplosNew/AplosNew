'use strict';
LcNavigationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function LcNavigationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Commercial Dashboard';
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


    $scope.getPurcheseLcReport = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetPurchaseLCReport",
                data: { Filter: $scope.FilterModel, FilterFields: getString() },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
        }
    }

    $scope.GridFilter = function (args) {
        if (args.requestType == "filtering") {
            $scope.ApplyFilter();
        }
    }
    $scope.ApplyFilter = function () {
        $scope.HideGrid = true;

        $scope.QueryString = [];

        var gridObj = $("#GridPurchaseLC").data("ejGrid");
        var filteredRecords = gridObj.getFilteredRecords();
        if (angular.isUndefinedOrNull(filteredRecords) == FilterModel) {
            if (filteredRecords.length > 0) {
                getString(filteredRecords, "LCId");
            }
            else {

            }
        }
        $scope.ClearFilter();
    }
    $scope.ClearFilter = function () {
        $scope.HideGrid = true;
        var gridObj = $("#GridPurchaseLC").data("ejGrid");
        gridObj.clearFiltering();

        var gridObj = $("#GridEdit").data("ejGrid");
        gridObj.clearFiltering();

    }
    var getString = function (data, column) {
        var string = "''";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }

        return string;
    }
    $scope.LCGrid = {
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now()),
        Type: 'Search'
    };
    $scope.PurchaseLCList = [];  
    $scope.LoadLCGrid = function () {
        if ($scope.LCGrid.Type == 'SearchByDate') {
            $scope.LCsearch = '';
            $scope.PurchaseLCList = [];
            if (new Date($scope.LCGrid.FromDate) > new Date($scope.LCGrid.ToDate)) 
                throw " From date can not be greater than To date.";
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
                    $scope.PurchaseLCList = [];
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
            $scope.LCGrid.FromDate = '';
            $scope.LCGrid.ToDate = '';
            $scope.PurchaseLCList = [];
            try {
                if ($scope.LCsearch == '')
                    throw "Please insert search value.";
                $http({
                    method: 'POST',
                    url: $scope.path + "GetList",
                    data: { 'column': $scope.LCsearchBy, 'value': $scope.LCsearch },
                    dataType: 'JSON'

                }).then(function successCallback(response) {
                    $scope.PurchaseLCList = [];
                    $scope.PurchaseLCList = response.data;
                });
            }
            catch (e) {
                ShowResult(e, 'failure');
            }

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

                for (var i = 0; i < $scope.SelectFGCharacteristicsValueList.length; i++) {
                if ($scope.SelectFGCharacteristicsValueList[i].Ratio != null) {
                    $scope.TotalRatio = parseFloat($scope.SelectFGCharacteristicsValueList[i].Ratio) + parseFloat($scope.TotalRatio);
                }
            }
            }),
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        $rootScope.openPopupAngular('POPopup');
    }

    $scope.summaryRows = [{
        title: "Total :", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalValue", dataMember: "TotalValue", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "AcceptanceValue", dataMember: "AcceptanceValue", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "GRNValue", dataMember: "GRNValue", format: "{0:N2}" }],
        showCaptionSummary: true

    }];

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
    $scope.summaryGRN = [{
        title: "Total :", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "GRNValue", dataMember: "GRNValue", format: "{0:N2}" }],
        showCaptionSummary: true

    }];

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
    $scope.summaryAC= [{
        title: "Total :", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "AcceptanceValue", dataMember: "AcceptanceValue", format: "{0:N2}" }],
        showCaptionSummary: true

    }];
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

    $scope.summaryLoan = [{
        title: "Total :", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Amount", dataMember: "Amount", format: "{0:N2}" }],
        showCaptionSummary: true

    }];

    $scope.PurchaseLCSetOffList = [];

    $scope.PurchaseLCSetoffList = [];
    $scope.LoadSetoffList = function (LCSetOffData) {
        $scope.SelectedLCRow = LCSetOffData;

        $http({
            method: 'POST',
            url: $scope.path + "GetPurchaseLCSetOff",
            data: { 'PurchaseLCId': LCSetOffData.LCId },
            dataType: 'JSON'

        })
            .then(function successCallback(response) {
                if (response.data.Error == false) {

                    $scope.PurchaseLCSetOffList = response.data.SetOffDATA;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }),
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        $rootScope.openPopupAngular('SetOffPopup');
    }

        $scope.summarySetoff = [{
        title: "Total :", summaryColumns: [
                { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Amount", dataMember: "Amount", format: "{0:N2}" }
           ],
        showCaptionSummary: true

    }];

    $scope.PurchaseLCLoanSetoffList = [];
    $scope.LoadLoanSetOff = function (LCLoanSetOffData) {
        $scope.SelectedLCRow = LCLoanSetOffData;

        $http({
            method: 'POST',
            url: $scope.path + "GetPurchaseLCLoanSetOff",
            data: { 'PurchaseLCId': LCLoanSetOffData.LCId },
            dataType: 'JSON'

        })
            .then(function successCallback(response) {
                if (response.data.Error == false) {

                    $scope.PurchaseLCLoanSetoffList = response.data.LoanSetOffDATA;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }),
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        $rootScope.openPopupAngular('LoanSetOffPopup');
    }

    $scope.summaryLoanSetOff = [{
        title: "Total :", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "LoanSetOff", dataMember: "LoanSetOff", format: "{0:N2}" }],
        showCaptionSummary: true

    }];

    $scope.searchCol = "";
    $scope.searchVal = "";
    $scope.LCsearchBy = "LCNo";
    $scope.LCsearch = "";
    $scope.LCFilterList = [
        /*{ 'name': 'Purchase LC Id', 'value': 'LCId' },*/
        { 'name': 'LC No.', 'value': 'LCNo' },
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
        /*{ 'name': 'GRN Count', 'value': 'GRNCount' },*/
        { 'name': 'GRN Value', 'value': 'GRNValue' },
        { 'name': 'Is Closed', 'value': 'IsClosed' },
        { 'name': 'Contract No', 'value': 'ContractNo' },
        { 'name': 'Customer', 'value': 'Customer' },
    ];

    $scope.EmptyGrid = function () {
        $scope.PurchaseLCList = [];
    }
}


