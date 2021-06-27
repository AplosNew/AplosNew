'use strict';
PORollBackController.$inject = ['accountService', 'addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller', '$location'];
function PORollBackController(accountService, addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $location) {

    $rootScope.title = "PO Roll Back";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/PurchaseOrder/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';

    $scope.tabh = 11;

    $scope.setTab11 = function (newTab) {
        $scope.getalldata();
        $scope.tabh = newTab;
    };
    $scope.isSet11 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.setTab22 = function (newTab) {
        $scope.getalldataPoApp();
        $scope.tabh = newTab;

    };
    $scope.isSet22 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.Griddata = [];
    $scope.getalldata = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetPOCheckedRollBack',
        }).then(function successCallback(response) {
            $scope.Griddata = response.data;
            for (var i = 0; i < $scope.Griddata.length; i++) {
                response.data[i].PODate = new Date($scope.Griddata[i].PODate);
            }
        });
    };
    $scope.getalldata();
    $scope.GriddataPoApp = [];
    $scope.getalldataPoApp = function () {

        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Products/PurchaseOrder/GetPORollBackAproved',
        }).then(function successCallback(response) {
            $scope.GriddataPoApp = response.data;
        });
    };
    $scope.detailTemp = "#tabGridContents";
    $scope.AllTabPrint = function (z) {
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/PurchaseOrder/GePurchaseOrderReport?purchaseOrderId=" + data.Id;
    };
    $scope.lst = [];
    $scope.POListDetails = function () {

        $http({
            method: 'GET',
            url: 'Products/PurchaseOrder/GetInventoryMaterialListPoByReqDetail'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            window.lst = response.data;

        });
    }
    $scope.POListDetails();
    $scope.detailgrid = function detailGridData(e) {
        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("POmasterId", "equal", parseInt(filteredData), true).take(1000));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
            columns: ["MaterialGroupName", "MaterialName", "Article", "Sku1", "Sku2", "Sku3", "MaterialDetail", "TransactionQty", "TransactionUoM", "TransactionRate", "CurrencyName", "TotalAmount"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
        var dataImg = ej.DataManager(window.Img).executeLocal(ej.Query().where("POId", "equal", parseInt(filteredData), true).take(1000));
        e.detailsElement.find("#detailGrid1").ejGrid({
            dataSource: dataImg,
            columns: [{ field: "UserFilename", headerText: "UserFilename", width: 100 },
            { field: "Description", headerText: "Description", width: 100 },
            { field: "Remarks", headerText: "Remarks", width: 100 }

            ]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }
    $scope.Checked = function () {

        $http({
            method: 'POST',
            url: 'Products/PurchaseOrder/PORollBackChecked',
            data:
            {
                'InventoryReceiveId': $scope.GRNUncheckDataForUpfdate.Id,
                'UserSendData': $scope.GRNUncheckDataForUpfdate
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getalldata();

            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
    $scope.onClickGRNCheckedUpdate = function (z) {
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.GRNUncheckDataForUpfdate = gridObj.getSelectedRecords()[0];
        $scope.url = $location.absUrl().split('!/')[1]
        if (x == '#GridApproved') {
            $scope.message = 'Are you sure to UnApproved?';
            angular.element(document.querySelector('#poapprovealert2')).modal('show');
        }
        else {
            $scope.message = 'Are you sure to UnCheck?';
            angular.element(document.querySelector('#poapprovealert')).modal('show');
        }        
    };
    $scope.Approved = function () {
        $http({
            method: 'POST',
            url: 'Products/PurchaseOrder/PORollBackApproved',
            data:
            {
                'InventoryReceiveId': $scope.GRNUncheckDataForUpfdate.Id,
                'UserSendData': $scope.GRNUncheckDataForUpfdate
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getalldataPoApp();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
}