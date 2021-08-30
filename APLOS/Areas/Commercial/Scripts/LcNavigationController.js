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
    $scope.FilterList = [];

    $scope.searchCol = "";
    $scope.searchVal = "";

    $scope.LCsearchBy = "LCNo";
    $scope.LCsearch = "";

    $scope.NonLCFilterList = [
        { 'name': 'PO No.', 'value': 'PONo' },
        { 'name': 'PO Date', 'value': 'PODate' },
        { 'name': 'Payment Mode', 'value': 'PaymentMode' },
        { 'name': 'Vendor Ref', 'value': 'VendorRef' },
        { 'name': 'Value', 'value': 'POAmount' },
        { 'name': 'Currency', 'value': 'Currency' },
        { 'name': 'Vendor', 'value': 'Vendor' },
        { 'name': 'GRN Value', 'value': 'GRNTotalAmount' },
    ];
    $scope.LCFilterList =
        [
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
            { 'name': 'GRN Value', 'value': 'GRNValue' },
            { 'name': 'Is Closed', 'value': 'IsClosed' },
            { 'name': 'Contract No', 'value': 'ContractNo' },
            { 'name': 'Customer', 'value': 'Customer' },
        ];



    $scope.FilterList = Object.assign([], $scope.LCFilterList);



    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
        $scope.FilterList = [];
        if ($scope.tab == 1) {
            $scope.LCsearchBy = "LCNo";
            $scope.LCsearch = "";
            $scope.FilterList = Object.assign([], $scope.LCFilterList);
        }
        else if ($scope.tab == 2) {
            $scope.LCsearchBy = "PONo";
            $scope.LCsearch = "";
            $scope.FilterList = Object.assign([], $scope.NonLCFilterList);
        }
      
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };



    //$scope.getPurcheseLcReport = function () {
    //    try {
    //        var file_src = $scope.path + 'GetPurchaseLCReport';
    //        $rootScope.report(file_src);

    //    } catch (e) {
    //    }
    //}
    //$scope.getPurcheseLcReport = function () {
    //    try {
    //        $http({
    //            method: 'POST',
    //            url: $scope.path + "GetPurchaseLCReport",
    //            data: { Filter: $scope.FilterModel, FilterFields: getString() },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error == false) {
    //                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //        }), function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        };
    //    } catch (e) {
    //    }
    //}

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
        if ($scope.isSet(2)) {
            $scope.LoadNonTagLcGrid();
        } else {

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
    }


    $scope.NonTagLcList = [];
    $scope.LoadNonTagLcGrid = function () {

        if ($scope.LCGrid.Type == 'SearchByDate') {
            $scope.LCsearch = '';
            $scope.NonTagLcList = [];
            if (new Date($scope.LCGrid.FromDate) > new Date($scope.LCGrid.ToDate))
                throw " From date can not be greater than To date.";
            $http({
                method: 'POST',
                url: $scope.path + "GetNonTagLCSearchByDate",
                data: { 'fromDate': $scope.LCGrid.FromDate, 'toDate': $scope.LCGrid.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {

                if (response.data.Error == false) {
                    for (var i = 0; i < response.data.DATA.length; i++) {
                        response.data.DATA[i].OpeningDate = new Date(response.data.DATA[i].OpeningDate);
                    }
                    $scope.NonTagLcList = [];
                    $scope.NonTagLcList = response.data.DATA;
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
            $scope.NonTagLcList = [];
            try {
                if ($scope.LCsearch == '')
                    throw "Please insert search value.";
                $http({
                    method: 'POST',
                    url: $scope.path + "GetNonTagLcSearchList",
                    data: { 'column': $scope.LCsearchBy, 'value': $scope.LCsearch },
                    dataType: 'JSON'

                }).then(function successCallback(response) {
                    $scope.NonTagLcList = [];
                    $scope.NonTagLcList = response.data;
                });
            }
            catch (e) {
                ShowResult(e, 'failure');
            }

        }
    }



    $scope.PurchaseLCMaterialPOList = [];
    $scope.SelectedLCRow = {};
    $scope.LoadMaterialPOList = function (MaterialPOData) {
        $scope.SelectedLCRow = MaterialPOData;
        $http({
            method: 'POST',
            url: $scope.path + "GetPurchaseLCPOList",
            data: { 'PurchaseLCId': MaterialPOData.LCId },
            dataType: 'JSON'
        })

            .then(function successCallback(response) {
                if (response.data.Error == false) {
                    $scope.PurchaseLCMaterialPOList = response.data.MaterialPODATA;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }),
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        $rootScope.openPopupAngular('MaterialPOPopup');
    }
    $scope.summaryMaterialPO = [{
        title: "Total :", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalValue", dataMember: "TotalValue", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "AcceptanceValue", dataMember: "AcceptanceValue", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "GRNAmount", dataMember: "GRNAmount", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "setOffValue", dataMember: "setOffValue", format: "{0:N2}" }],
        showCaptionSummary: true

    }];

    $scope.PurchaseLCPOBreakDownList = [];
    $scope.LoadPOBreakDownList = function (POBreakDownData) {
        // $scope.SelectedLCRow = POBreakDownData;
        $http({
            method: 'POST',
            url: $scope.path + "POBreakDownDataList",
            data: { 'POID': POBreakDownData.PONo },
            dataType: 'JSON'
        })

            .then(function successCallback(response) {
                if (response.data.Error == false) {
                    $scope.PurchaseLCPOBreakDownList = response.data.POBrDATA;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }),
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        $rootScope.openPopupAngular('sdgfsdfs');
    }
    $scope.sumPO = [{
        title: "Total :", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "GRNValue", dataMember: "GRNValue", format: "{0:N2}" }
        ],
        showCaptionSummary: true

    }];

    $scope.PurchaseLCServicePOList = [];
    $scope.SelectedLCRow = {};
    $scope.LoadServicePOList = function (ServicePOData) {
        $scope.SelectedLCRow = ServicePOData;
        $http({
            method: 'POST',
            url: $scope.path + "GetPurchaseLCServicePOList",
            data: { 'PurchaseLCId': ServicePOData.LCId },
            dataType: 'JSON'
        })

            .then(function successCallback(response) {
                if (response.data.Error == false) {
                    $scope.PurchaseLCServicePOList = response.data.ServicePODATA;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }),
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        $rootScope.openPopupAngular('ServicePOPopup');
    }
    $scope.summaryServicePO = [{
        title: "Total :", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "sTotalValue", dataMember: "sTotalValue", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "sAcceptanceValue", dataMember: "sAcceptanceValue", format: "{0:N2}" }
            
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "sSetOffValue", dataMember: "sSetOffValue", format: "{0:N2}" }],
        showCaptionSummary: true

    }];

    $scope.ServicePOBreakDownList = [];
    $scope.LoadServicePOBreakDownList = function (ServicePOBreakDownData) {
        // $scope.SelectedLCRow = POBreakDownData;
        $http({
            method: 'POST',
            url: $scope.path + "ServicePOBreakDownDataList",
            data: { 'POID': ServicePOBreakDownData.sPONo },
            dataType: 'JSON'
        })

            .then(function successCallback(response) {
                if (response.data.Error == false) {
                    $scope.ServicePOBreakDownList = response.data.ServicePOBrDATA;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }),
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        $rootScope.openPopupAngular('ServicePOBreakDownPopUp');
    }
    $scope.sumServicePO = [{
        title: "Total :", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "sGRNValue", dataMember: "sGRNValue", format: "{0:N2}" }
        ],
        showCaptionSummary: true

    }];

    $scope.PurchaseLCJWPOList = [];
    $scope.SelectedLCRow = {};
    $scope.LoadJWPOList = function (JWPOData) {
        $scope.SelectedLCRow = JWPOData;
        $http({
            method: 'POST',
            url: $scope.path + "GetPurchaseLCJWPOList",
            data: { 'PurchaseLCId': JWPOData.LCId },
            dataType: 'JSON'
        })

            .then(function successCallback(response) {
                if (response.data.Error == false) {
                    $scope.PurchaseLCJWPOList = response.data.JWPODATA;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }),
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        $rootScope.openPopupAngular('JWPOPopup');
    }
    $scope.summaryJWPO = [{
        title: "Total :", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "JWTotalValue", dataMember: "JWTotalValue", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "JWAcceptanceValue", dataMember: "JWAcceptanceValue", format: "{0:N2}" }
           /* , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "GRNAmount", dataMember: "GRNAmount", format: "{0:N2}" }*/
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "JWSetOffValue", dataMember: "JWSetOffValue", format: "{0:N2}" }],
        showCaptionSummary: true

    }];




    $scope.JWPOBreakDownList = [];
    $scope.LoadJWPOBreakDownList = function (JWPOBreakDownData) {
        // $scope.SelectedLCRow = POBreakDownData;
        $http({
            method: 'POST',
            url: $scope.path + "JWPOBreakDownDataList",
            data: { 'POID': JWPOBreakDownData.JWPONo },
            dataType: 'JSON'
        })

            .then(function successCallback(response) {
                if (response.data.Error == false) {
                    $scope.JWPOBreakDownList = response.data.JWPOBrDATA;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }),
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        $rootScope.openPopupAngular('JWPOBreakDownPopUp');
    }
    $scope.sumJWBreakDownPO = [{
        title: "Total :", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "GRNValue", dataMember: "GRNValue", format: "{0:N2}" }
        ],
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

    $scope.PurchaseLCGRNBreakDownList = [];
    $scope.LoadGRNBreakDownList = function (GRNBreakDownData) {
        //$scope.SelectedLCRow = GRNBreakDownData;
        $http({
            method: 'POST',
            url: $scope.path + "GRNBreakDownDataList",
            data: { 'GRNID': GRNBreakDownData.GRNNo },
            dataType: 'JSON'
        })

            .then(function successCallback(response) {
                if (response.data.Error == false) {
                    $scope.PurchaseLCGRNBreakDownList = response.data.GRNBrDATA;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }),
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        $rootScope.openPopupAngular('GRNBreakDownPopup');
    }
    $scope.sumGRN = [{
        title: "Total :", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "GRNValue", dataMember: "GRNValue", format: "{0:N2}" }
        ],
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
    $scope.summaryAC = [{
        title: "Total :", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "AcceptanceValue", dataMember: "AcceptanceValue", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "SetOffValue", dataMember: "SetOffValue", format: "{0:N2}" }],
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
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Amount", dataMember: "Amount", format: "{0:N2}" }
            , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "LoanSetOff", dataMember: "LoanSetOff", format: "{0:N2}" }],
        showCaptionSummary: true

    }];


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

    $scope.SetoffList = [];
    $scope.LoadSetoffList = function (SetOffData) {
        $scope.SelectedLCRow = SetOffData;

        $http({
            method: 'POST',
            url: $scope.path + "GetPurchaseLCSetOff",
            data: { 'PurchaseLCId': SetOffData.LCId },
            dataType: 'JSON'

        })
            .then(function successCallback(response) {
                if (response.data.Error == false) {

                    $scope.SetoffList = response.data.SetOffDATA;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }),
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        $rootScope.openPopupAngular('SetOffPoP');
    }
    $scope.SetOff = [{
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


    $scope.NonLcGRNList = [];
    $scope.LoadNonLcGRNList = function (NonLCGRNData) {
        $scope.SelectedLCRow = NonLCGRNData;
        $http({
            method: 'POST',
            url: $scope.path + "NonLcGRNBreakDownDataList",
            data: { 'POID': NonLCGRNData.PONo },
            dataType: 'JSON'
        })

            .then(function successCallback(response) {
                if (response.data.Error == false) {
                    $scope.NonLcGRNList = response.data.NonLCGRNData;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }),
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        $rootScope.openPopupAngular('NonLcGRNPopup');
    }
    $scope.summaryNONLcGRN = [{
        title: "Total :", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Amount", dataMember: "Amount", format: "{0:N2}" }
        ],
        showCaptionSummary: true

    }];

    $scope.today = $filter('dateFiltering')(Date.now());
    $scope.rowDataBoundOrder = function rowDataBoundOrder(e) {
        try {
            if (new Date(e.data.ExpiryDate) < new Date()) {
                e.row.css("background-color","#FF502A");
                return;
            }
        } catch (e) {

        }
    }

    $scope.EmptyGrid = function () {
        $scope.PurchaseLCList = [];
    }

}


