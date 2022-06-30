'use strict';
salesRegisterController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService', '$window', '$controller'];
function salesRegisterController(fileReader, commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService, $window, $controller) {
    $rootScope.title = "Sales Register";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/SalesRegister/';
    $scope.path1 = 'Accounts/InventoryPayable/';
    //$scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportJson';

    $scope.downloadgriddataUrl = 'GridReports/Download';
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.RowColor = "";
    $scope.isAlternative = -1;
    $scope.rowDataBound = function rowDataBound(e) {

        if ($scope.RowColor != e.data.Id) {
            $scope.isAlternative = $scope.isAlternative * -1;
            $scope.RowColor = e.data.Id;
        }
        if ($scope.isAlternative > 0)
            e.row.css("background-color", '#D3D3D3');
        else
            e.row.css("background-color", '#ffffff');


    }
    $scope.Print = function () {

        var gridObj1 = $("#GridPO").data("ejGrid");
        var data1 = gridObj1.model.dataSource();
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            //data: { 'data': data1 }
            data: JSON.stringify(data1)
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                // ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');

            }
            else {

                location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });
    }
    $scope.PrintPurchaseRegister = function () {

        var gridObj11 = $("#GridPrint").data("ejGrid");
        var data11 = gridObj11.model.dataSource();

        $http({

            method: "POST",
            url: $scope.exportgriddataUrl,
            data: { 'data': data11 }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                // ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');

            }
            else {

                location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });

    }


    $scope.productNew = {
        Type: null,
        WithStock: true,
        WithoutStock: false,
        Storage: false
    };
    $scope.changeSourceFrom = function (from) {
        debugger;
        if (from === 'AsOnDate') {
            $scope.report.FromDate = "";

            $scope.productNew.Type = 'Posted';

        }
        if (from === 'ForThePeriod') {
            $scope.productNew.Type = 'NonPosted';


        }
    };

    $scope.GriddataMaterialLedger = [];
    $scope.getaldataMaterialLedger = function () {

        $http({
            method: 'POST',
            //url: $scope.getSearchListUrl,
            url: 'Materials/MaterialLedger/GetMaterialLedger',
            data: {
                fromDate: $scope.report.FromDate,
                toDate: $scope.report.ToDate,
                Type: $scope.productNew.Type
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GriddataMaterialLedger = response.data;

            //entrydata = copy(searchdata);
        });
    };


    $scope.SalesRegisterList = [];
    $scope.SalesRegisterPartyList = [];
    $scope.SalesRegisterItemList = [];
    $scope.pivotTableFieldListID = [];



    $scope.GetSalesRegister = function () {
        if ($scope.report.FromDate === null || $scope.report.FromDate === "") {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        else if ($scope.report.ToDate === null || $scope.report.ToDate === "") {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        else if ($scope.report.ReportType === null || $scope.report.ReportType === "") {
            ShowResult('Please select Report Type', 'failure');
            return false;
        }

        if ($scope.report.ReportType == 'SaleWise') {
            $scope.gridDataURL = 'Products/salesRegister/GetSalesRegister'
        }
        else if ($scope.report.ReportType == 'PartyWise') {
            $scope.gridDataURL = 'Products/salesRegister/SalesRegisterCustomerWiseData'
        }
        else if ($scope.report.ReportType == 'ItemWise') {
            $scope.gridDataURL = 'Products/salesRegister/GetSalesRegisterItemWiseData'
        }
        //'Materials/MaterialLedger/GetPurchaseRegister'
        $http({
            method: 'POST',
            //url: $scope.getSearchListUrl,
            url: $scope.gridDataURL,
            data: {
                fromDate: $scope.report.FromDate,
                toDate: $scope.report.ToDate,
                Type: $scope.productNew.Type
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if ($scope.report.ReportType == 'SaleWise') {
                $scope.SalesRegisterList = response.data.NewData;
                for (var i = 0; i < $scope.SalesRegisterList.length; i++) {
                    response.data[i].GRNEntryDate = new Date($scope.SalesRegisterList[i].GRNEntryDate);
                }
            }
            else if ($scope.report.ReportType == 'PartyWise') {
                $scope.SalesRegisterPartyList = response.data.NewData;
                for (var i = 0; i < $scope.SalesRegisterPartyList.length; i++) {
                    response.data[i].GRNEntryDate = new Date($scope.SalesRegisterPartyList[i].GRNEntryDate);
                }
            }
            else if ($scope.report.ReportType == 'ItemWise') {
                $scope.SalesRegisterItemList = response.data.NewData;
                for (var i = 0; i < $scope.SalesRegisterItemList.length; i++) {
                    response.data[i].GRNEntryDate = new Date($scope.SalesRegisterItemList[i].GRNEntryDate);
                }
            }

            $scope.load();
        });

    };

    $scope.InventorySalesReportExcels = function () {
        var Type = null;
        if ($scope.productNew.AsOnDate === 'AsOnDate') {

            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }
            Type = 'AsOnDate';
        }
        else {

            if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
                ShowResult('Select From Date', 'failure');
                return false;
            }
            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }
            Type = 'ForThePeriod';
        }

        var reportFormat = "Excel";
        $scope.report.Summary = 'Summary';
        $window.open('Products/SalesRegister/InventorySalesReportExcel?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2 + '&RcptIssue=' + $scope.report.RcptIssue + '&Summary=' + $scope.productNew.Summary + '&WithTax=' + true + '&Type=' + Type);
    };

    $scope.downloadReport = function () {
        if ($scope.report.ReportType == 'SaleWise') {
            $scope.InventorySalesReportExcels();
        }
        else if ($scope.report.ReportType == 'PartyWise') {
            $scope.SalesPartyWiseReportExcel();
        }
        else if ($scope.report.ReportType == 'ItemWise') {
            $scope.SalesRegisterItemWiseReport();
        }
    }

    $scope.SalesRegisterItemWiseReport = function () {
        if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        $window.open('Products/SalesRegister/SalesRegisterItemWiseReport?reportFormat=' + 'Excel' + '&PlantId=' + null + '&FromDate=' + $scope.report.FromDate + '&ToDate=' + $scope.report.ToDate);
    };

    $scope.SalesPartyWiseReportExcel = function () {
        if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        $window.open('Products/SalesRegister/SalesRegisterCustomerWiseReport?reportFormat=' + 'Excel' + '&PlantId=' + null + '&FromDate=' + $scope.report.FromDate + '&ToDate=' + $scope.report.ToDate);
    };

}



