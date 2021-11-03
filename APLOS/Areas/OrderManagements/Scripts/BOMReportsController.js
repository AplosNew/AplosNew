'use strict';
BOMReportsController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'cboService', '$window'];
function BOMReportsController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, cboService, $window) {
    $rootScope.title = "BOM Reports";

    //#region  segment
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.path = 'OrderManagements/BOMReports/';
    $scope.SelectionType = 'MASTERORDER';
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;

    };

    $scope.MasterOrder = {};
    $scope.ProductionOrder = {};

    $scope.SalesOrderList = [];
    $scope.MasterOrderList = [];
    $scope.ProductionOrderList = [];
    $scope.ItemList = [];

    $scope.ChangeSelectionType = function () {
        $scope.MasterOrder = {};
        $scope.ProductionOrder = {};

        $scope.SalesOrderList = [];
        $scope.MasterOrderList = [];
        $scope.ProductionOrderList = [];
        $scope.ItemList = [];
    }

    $scope.ProductionOrderSearchValue = "Id"; $scope.ProductionOrderSearchText = "";
    $scope.modelFilterByProductionOrder = [
        { 'name': 'Prod. Order#', 'value': 'Id' }, { 'name': 'Prod. Status', 'value': 'ProductionStatus' },
        { 'name': 'Material', 'value': 'Material' }, { 'name': 'Product', 'value': 'Product' }, { 'name': 'Product Category', 'value': 'ProductCategory' },
        { 'name': 'Master Order No', 'value': 'MasterOrderId' },
        { 'name': 'Buyer Order#', 'value': 'BuyerRefNo' }, { 'name': 'Own Order#', 'value': 'OwnRefNo' }, { 'name': 'Buyer Item#', 'value': 'StyleNo' },
        { 'name': 'Own Item#', 'value': 'OwnStyleNo' }, { 'name': 'SO No', 'value': 'SONo' },
        { 'name': 'SO Desc', 'value': 'SODesc' }, { 'name': 'Buyer', 'value': 'buyer' }, { 'name': 'Customer', 'value': 'Customer' },
    ];
    $scope.SearchProductionOrder = function () {
        $http({
            method: 'POST',
            data: { 'column': $scope.ProductionOrderSearchValue, 'value': $scope.ProductionOrderSearchText },
            url: $scope.path + "SearchProductionOrder"
        }).then(function successCallback(response) {
            $scope.ProductionOrderList = response.data;
        });
    };
    $scope.SelectProductionOrder = function (args) {
        $scope.ProductionOrder = args.data;
        $scope.GetSalesOrderList('PR', $scope.ProductionOrder.Id);

        $scope.MasterOrder = {};
        $scope.MasterOrderList = [];
        $scope.SalesOrderList = [];
        $scope.ItemList = [];
        $rootScope.closePopup('searchNewPopupProductionOrder');
    }


    $scope.MasterOrderSearchValue = "Id"; $scope.MasterOrderSearchText = "";
    $scope.modelFilterByMasterOrder = [
        { 'name': 'Master Order#', 'value': 'Id' }, { 'name': 'Customer', 'value': 'CustomerName' },
        { 'name': 'Buyer', 'value': 'Buyer' }, { 'name': 'Order Status', 'value': 'OrderStatus' }, { 'name': 'Order Category', 'value': 'OrderCategory' },
        { 'name': 'Responsible Person Name', 'value': 'ResponsiblePersonName' },
        { 'name': 'Own Order#', 'value': 'OwnReferenceNo' }, { 'name': 'Buyer Order#', 'value': 'BuyerReferenceNo' }, { 'name': 'Own Item#', 'value': 'OwnItem' },
        { 'name': 'Customer Item#', 'value': 'BuyerItem' }, { 'name': 'Contract No', 'value': 'ContractNo' },
        { 'name': 'Master LC No', 'value': 'MasterLCNo' }
    ];
    $scope.SearchMasterOrder = function () {
        $http({
            method: 'POST',
            data: { 'column': $scope.MasterOrderSearchValue, 'value': $scope.MasterOrderSearchText },
            url: $scope.path + "SearchMasterOrder"
        }).then(function successCallback(response) {
            $scope.MasterOrderList = response.data;
        });
    };
    $scope.SelectMasterOrder = function (args) {

        $scope.MasterOrder = args.data;
        $scope.GetSalesOrderList('MASTERORDER', $scope.MasterOrder.Id);

        $scope.ProductionOrder = {};
        $scope.ProductionOrderList = [];
        $scope.SalesOrderList = [];
        $scope.ItemList = [];
        $rootScope.closePopup('searchNewPopupMasterOrder');
    }


    $scope.GetSalesOrderList = function (flag, Id) {
        $scope.ItemList = [];
        $http({
            method: 'POST',
            data: { 'flag': flag, 'Id': Id },
            url: $scope.path + "GetSalesOrderList"
        }).then(function successCallback(response) {
            $scope.SalesOrderList = response.data;
        });
    }

    $scope.ApplyForItem = function () {
        var exists = ej.DataManager($scope.SalesOrderList).executeLocal(ej.Query().where("Checked", "equal", true));
        var ids = getString(exists, "SalesOrderId");

        $http({
            method: 'POST',
            data: { 'SalesOrderIds': ids },
            url: $scope.path + "GetBOMItemListForReport"
        }).then(function successCallback(response) {
            $scope.ItemList = response.data;
        });
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



    $scope.getBOMReport = function () {

        var _salesOrderIds = ej.DataManager($scope.SalesOrderList).executeLocal(ej.Query().where("Checked", "equal", true));
        var Soids = getString(_salesOrderIds, "SalesOrderId");

        var _itemIds = ej.DataManager($scope.ItemList).executeLocal(ej.Query().where("Checked", "equal", true));
        var itemids = getString(_itemIds, "ArticleId");


        try {
            var file_src = $scope.path + 'GetBOMReport?ItemIds=' + itemids + '&SOIds=' + Soids;
            $rootScope.report(file_src);

        } catch (e) {

        }

    }

    $scope.refreshTemplateSO = function (args) {
        if (args.rowIndex == 0) {
            $("#headchkSO").ejCheckBox({ "change": CheckAllSO });
        }
    }

    function CheckAllSO(e) {
        if (!e.isInteraction)
            return;


        var gridObj = $("#GridSOItemSelected").data("ejGrid");
        var filteredRecords = gridObj.getFilteredRecords();
        if (angular.isUndefinedOrNull(filteredRecords) == true || filteredRecords.length == 0) {
            filteredRecords = $scope.SalesOrderList;
        }

        if (e.checkState == 'check') {

            for (var i = 0; i < filteredRecords.length; i++) {
                filteredRecords[i].Checked = true;
            }
        }
        else {
            for (var i = 0; i < filteredRecords.length; i++) {
                filteredRecords[i].Checked = false;
            }
        }

        for (var i = 0; i < filteredRecords.length; i++) {
            for (var KK = 0; KK < $scope.SalesOrderList.length; KK++) {
                if (filteredRecords[i].Id == $scope.SalesOrderList[kk]) {
                    $scope.SalesOrderList[kk].Checked = filteredRecords[i].Checked;
                    break;
                }
            }
        }


        gridObj.refreshContent();
    }

    $scope.refreshTemplateItem = function (args) {
        if (args.rowIndex == 0) {
            $("#headchkItem").ejCheckBox({ "change": CheckAllItem });
        }
    }

    function CheckAllItem(e) {
        if (!e.isInteraction)
            return;

        var gridObj = $("#BOQItems").data("ejGrid");
        var filteredRecords = gridObj.getFilteredRecords();
        if (angular.isUndefinedOrNull(filteredRecords) == true || filteredRecords.length == 0) {
            filteredRecords = $scope.ItemList;
        }

        if (e.checkState == 'check') {

            for (var i = 0; i < filteredRecords.length; i++) {
                filteredRecords[i].Checked = true;
            }
        }
        else {
            for (var i = 0; i < filteredRecords.length; i++) {
                filteredRecords[i].Checked = false;
            }
        }

        for (var i = 0; i < filteredRecords.length; i++) {
            for (var KK = 0; KK < $scope.ItemList.length; KK++) {
                if (filteredRecords[i].Id == $scope.ItemList[kk]) {
                    $scope.ItemList[kk].Checked = filteredRecords[i].Checked;
                    break;
                }
            }
        }

        var gridObj = $("#BOQItems").data("ejGrid");
        gridObj.refreshContent();
    }

}
