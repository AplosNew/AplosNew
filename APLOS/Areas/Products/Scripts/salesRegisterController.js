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
    $scope.exportgriddataUrlUpd = 'GridReports/ExcelExportUpd';
    $scope.exportgriddataUrlUpdate2 = 'GridReports/ExcelExportUpdate2';
    $scope.exportAssetgriddataUrl = 'GridReports/ExcelExportUpdate2';

    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadAssetgriddataUrl = 'GridReports/Download';
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

    $scope.SalesRegisterLists = [];
    $scope.SalesRegisterPartyList = [];
    $scope.SalesRegisterItemList = [];
    $scope.SalesAssetRegisterLists = [];
    $scope.SalesAssetRegisterPartyList = [];
    $scope.SalesAssetRegisterItemList = [];
    $scope.pivotTableFieldListID = [];
    $scope.GetAssetSalesRegisterView = function () {
        $scope.SalesAssetRegisterLists = [];
        $scope.SalesAssetRegisterPartyList = [];
        $scope.SalesAssetRegisterItemList = [];
        if ($scope.report.AssetFromDate === null || $scope.report.AssetFromDate === "") {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        else if ($scope.report.AssetToDate === null || $scope.report.AssetToDate === "") {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        else if ($scope.report.AssetReportType === null || $scope.report.AssetReportType === "") {
            ShowResult('Please select Report Type', 'failure');
            return false;
        }

        if ($scope.report.AssetReportType == 'SaleWise') {
            $scope.gridAssetDataURL = 'Products/salesRegister/GetAssetSalesRegister'
        }
        //else if ($scope.report.ReportType == 'PartyWise') {
        //    $scope.gridDataURL = 'Products/salesRegister/SalesRegisterCustomerWiseData'
        //}
        //else if ($scope.report.ReportType == 'ItemWise') {
        //    $scope.gridDataURL = 'Products/salesRegister/GetSalesRegisterItemWiseData'
        //}
        //'Materials/MaterialLedger/GetPurchaseRegister'
        $http({
            method: 'POST',
            url: $scope.gridAssetDataURL,
            data: {
                fromDate: $scope.report.AssetFromDate,
                toDate: $scope.report.AssetToDate,
                Type: 'ForThePeriod'
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if ($scope.report.AssetReportType == 'SaleWise') {
                $scope.SalesAssetRegisterLists = response.data.NewData;
                //for (var i = 0; i < $scope.SalesAssetRegisterLists.length; i++) {
                //    response.data[i].GRNEntryDate = new Date($scope.SalesRegisterLists[i].GRNEntryDate);
                //}
            }
            //else if ($scope.report.ReportType == 'PartyWise') {
            //    $scope.SalesRegisterPartyList = response.data.NewData;
            //    for (var i = 0; i < $scope.SalesRegisterPartyList.length; i++) {
            //        response.data[i].GRNEntryDate = new Date($scope.SalesRegisterPartyList[i].GRNEntryDate);
            //    }
            //}
            //else if ($scope.report.ReportType == 'ItemWise') {
            //    $scope.SalesRegisterItemList = response.data.NewData;
            //    for (var i = 0; i < $scope.SalesRegisterItemList.length; i++) {
            //        response.data[i].GRNEntryDate = new Date($scope.SalesRegisterItemList[i].GRNEntryDate);
            //    }
            //}

            $scope.load();
        });

    };

  

    $scope.GetSalesRegisterView = function () {
        $scope.SalesRegisterLists = [];
        $scope.SalesRegisterPartyList = [];
        $scope.SalesRegisterItemList = [];
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
                Type: 'ForThePeriod'
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if ($scope.report.ReportType == 'SaleWise') {
                $scope.SalesRegisterLists = response.data.NewData;
                for (var i = 0; i < $scope.SalesRegisterLists.length; i++)
                {
                    response.data[i].GRNEntryDate = new Date($scope.SalesRegisterLists[i].GRNEntryDate);
                }
            }
            else if ($scope.report.ReportType == 'PartyWise') {
                $scope.SalesRegisterPartyList = response.data.NewData;
                //for (var i = 0; i < $scope.SalesRegisterPartyList.length; i++) {
                //    response.data[i].GRNEntryDate = new Date($scope.SalesRegisterPartyList[i].GRNEntryDate);
                //}
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

        $scope.report.Summary = 'Summary';

        var dataList = [];
        var g = $("#GridSalesPrint").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.SalesRegisterLists;
        }

        //var ids = "";
        //if (baseService.arrayLength(dataList) > 0) {
        //    for (var i = 0; i < dataList.length; i++) {
        //        if (ids == "") {
        //            ids = "'','" + dataList[i].SalesId + "'";
        //        }
        //        else {
        //            ids += ",'" + dataList[i].SalesId + "'";
        //        }
        //    }
        //}
        //else {
        //    for (var i = 0; i < $scope.SalesRegisterLists.length; i++) {
        //        if (ids == "") {
        //            ids = "'','" + $scope.SalesRegisterLists[i].SalesId + "'";
        //        }
        //        else {
        //            ids += ",'" + $scope.SalesRegisterLists[i].SalesId + "'";
        //        }
        //    }
        //}
        $scope.fileName = 'Sales Register Sales Wise';
        $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
        //$window.open('Products/SalesRegister/InventorySalesReportExcel?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2 + '&RcptIssue=' + $scope.report.RcptIssue + '&Summary=' + $scope.productNew.Summary + '&WithTax=' + true + '&Type=' + Type);

        $http({
            method: 'POST',
            //url: $scope.path + "InventorySalesReportExcel",
            url: $scope.exportgriddataUrlUpdate2,
            data: {
                //'ToDate': $scope.report.ToDate,
                //'FromDate': $scope.report.FromDate,
                //'SalesId': ids,
                //'Summary': $scope.report.Summary,
                //'Type': 'ForThePeriod',
                //'WithTax': true
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else
            {
                //$rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                $window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };


    $scope.SalesRegisterItemWiseReport = function () {
        if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }

        var dataList = [];
        var g = $("#GridItemPrint").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.SalesRegisterItemList;
        }

        //var ids = "";
        //if (baseService.arrayLength(dataList) > 0) {
        //    for (var i = 0; i < dataList.length; i++) {
        //        if (ids == "") {
        //            ids = "'','" + dataList[i].SalesMaterialId + "'";
        //        }
        //        else {
        //            ids += ",'" + dataList[i].SalesMaterialId + "'";
        //        }
        //    }
        //}
        //else {
        //    for (var i = 0; i < $scope.SalesRegisterItemList.length; i++) {
        //        if (ids == "") {
        //            ids = "'','" + $scope.SalesRegisterItemList[i].SalesMaterialId + "'";
        //        }
        //        else {
        //            ids += ",'" + $scope.SalesRegisterItemList[i].SalesMaterialId + "'";
        //        }
        //    }
        //}
        $scope.fileName = 'Sales Register Item Wise';
        $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
        //$window.open('Products/SalesRegister/SalesRegisterItemWiseReport?reportFormat=' + 'Excel' + '&PlantId=' + null + '&FromDate=' + $scope.report.FromDate + '&ToDate=' + $scope.report.ToDate);

        $http({
            method: 'POST',
            //url: $scope.path + "SalesRegisterItemWiseReport",
            url: $scope.exportgriddataUrlUpdate2,
            data: {
                //'ToDate': $scope.report.ToDate,
                //'FromDate': $scope.report.FromDate,
                //'SMId': ids,

                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                //$rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                $window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
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

        var dataList = [];
        var g = $("#GridPartyPrint").data("ejGrid");
        dataList = g.getFilteredRecords();
        if (dataList.length == 0) {
            dataList = $scope.SalesRegisterPartyList;
        }

        //var ids = "";
        //if (baseService.arrayLength(dataList) > 0) {
        //    for (var i = 0; i < dataList.length; i++) {
        //        if (ids == "")
        //        {
        //            ids = "'','" + dataList[i].PartyId + "'";
        //        }
        //        else {
        //            ids += ",'" + dataList[i].PartyId + "'";
        //        }
        //    }
        //}
        //else {
        //    for (var i = 0; i < $scope.SalesRegisterPartyList.length; i++) {
        //        if (ids == "") {
        //            ids = "'','" + $scope.SalesRegisterPartyList[i].PartyId + "'";
        //        }
        //        else {
        //            ids += ",'" + $scope.SalesRegisterPartyList[i].PartyId + "'";
        //        }
        //    }
        //}
        $scope.fileName = 'Sales Register Customer Wise';
        $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
        //$window.open('Products/SalesRegister/SalesRegisterCustomerWiseReport?reportFormat=' + 'Excel' + '&PlantId=' + null + '&FromDate=' + $scope.report.FromDate + '&ToDate=' + $scope.report.ToDate);

        $http({
            method: 'POST',
            //url: $scope.path + "SalesRegisterCustomerWiseReport",
            url: $scope.exportgriddataUrlUpd,
            data: {
                //'ToDate': $scope.report.ToDate,
                //'FromDate': $scope.report.FromDate,
                //'PartyId': ids,
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                //$rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                $window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };

    $scope.AssetSalesReportExcels = function () {
        var Type = null;
        if ($scope.productNew.AsOnDate === 'AsOnDate') {

            if ($scope.report.AssetToDate === "" || $scope.report.AssetToDate === null || $scope.report.AssetToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }
            Type = 'AsOnDate';
        }
        else {

            if ($scope.report.AssetFromDate === "" || $scope.report.AssetFromDate === null || $scope.report.AssetFromDate === undefined) {
                ShowResult('Select From Date', 'failure');
                return false;
            }
            if ($scope.report.AssetToDate === "" || $scope.report.AssetToDate === null || $scope.report.AssetToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }
            Type = 'ForThePeriod';
        }

        $scope.report.Summary = 'Summary';

        var dataList = [];
        var g = $("#GridAssetSalesPrint").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.SalesAssetRegisterLists;
        }
        $scope.fileName = 'Sales Register Sales Wise';
        $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
        $http({
            method: 'POST',
            url: $scope.exportAssetgriddataUrl,
            data: {
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };
    $scope.downloadAssetReport = function () {
        if ($scope.report.AssetReportType == 'SaleWise') {
            $scope.AssetSalesReportExcels();
        }
        else if ($scope.report.AssetReportType == 'PartyWise') {
            $scope.AssetSalesReportExcels();
        }
        else if ($scope.report.AssetReportType == 'ItemWise') {
            $scope.AssetSalesRegisterItemWiseReport();
        }
    }
}



