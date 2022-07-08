'use strict';
PurchaseConfirmationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$window'];
function PurchaseConfirmationController(commonMessage, $scope, $rootScope, baseService, $http, $filter, $window) {
    $scope.title = 'Purchase Confirmation';
    $scope.TaskManagementDataList = [];
    $scope.path = 'Products/InventoryReceiveAddition/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath

    $scope.model = { FromDate:null,ToDate:null };
    $scope.modelNew = Object.assign({}, $scope.model);
   

    $scope.filters = [];
    $scope.getFiltersData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.model.FromDate)) {
                throw "From Date is required.";
            }
            else if (baseService.isUndefinedOrNull($scope.model.ToDate)) {
                throw "To Date is required.";
            }
            else if (new Date($scope.model.FromDate) > new Date($scope.model.ToDate)) {
                throw "From date must be below or equal to To Date";
            }
            else if (new Date($scope.model.ToDate) < new Date($scope.model.FromDate)) {
                throw "To date must be above or equal to From Date.";
            }


            $http({
                method: 'GET',
                url: 'Products/InventoryReceiveAddition/GetFiltersPurchaseconfirmationData?fromDate=' + $scope.model.FromDate + '&todate=' + $scope.model.ToDate,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.filters = response.data;
                var columnList = [
                    { field: 'Vendor', width: 20, headerText: "Vendor", type: "string" },
                    { field: 'MaterialType', width: 20, headerText: "MaterialType", type: "string" },
                    { field: 'Material', width: 20, headerText: "Material", type: "string" }
                ];
                $("#filters").ejGrid({
                    dataSource: $scope.filters,
                    minWidth: 450, minHeight: 400,
                    allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                    filterSettings: { filterType: "excel" },
                    columns: columnList
                });

                var gridObj = $("#filters").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
                $("#filters").children('.e-pager.e-js.e-pager').hide();
                $("#filters").children('.e-gridcontent.e-droppable.e-js').hide();
                $("#filters").children('.e-gridcontent').hide();
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "Vendor", "Value": getString(fl, "Vendor") });
        parameters.push({ "Key": "MaterialType", "Value": getString(fl, "MaterialType") });
        parameters.push({ "Key": "Material", "Value": getString(fl, "Material") });

        $scope.parameters = parameters;
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
    $scope.PurchaseConfirmationGRNDataList = [];
    $scope.GetPurchaseConfirmationGRNData = function () {
        if ($scope.model.FromDate === "" || $scope.model.FromDate === null || $scope.model.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if ($scope.model.ToDate === "" || $scope.model.ToDate === null || $scope.model.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        var dataList = [];
        var g = $("#filters").data("ejGrid");
        dataList = g.getFilteredRecords();
        var vendorids = "";
        var materialTypeids = "";
        var materialMasterids = "";
        if (baseService.arrayLength(dataList) > 0) {
            for (var i = 0; i < dataList.length; i++) {
                if (vendorids == "") {
                    vendorids = "'','" + dataList[i].VendorId + "'";
                }
                else {
                    vendorids += ",'" + dataList[i].VendorId + "'";
                }
                if (materialTypeids == "") {
                    materialTypeids = "'','" + dataList[i].MaterialTypeId + "'";
                }
                else {
                    materialTypeids += ",'" + dataList[i].MaterialTypeId + "'";
                }
                if (materialMasterids == "") {
                    materialMasterids = "'','" + dataList[i].MaterialMasterId + "'";
                }
                else {
                    materialMasterids += ",'" + dataList[i].MaterialMasterId + "'";
                }
            }
        }
        else {
            for (var i = 0; i < $scope.filters.length; i++) {
                if (vendorids == "") {
                    vendorids = "'','" + $scope.filters[i].VendorId + "'";
                }
                else {
                    vendorids += ",'" + $scope.filters[i].VendorId + "'";
                }
                if (materialTypeids == "") {
                    materialTypeids = "'','" + $scope.filters[i].MaterialTypeId + "'";
                }
                else {
                    materialTypeids += ",'" + $scope.filters[i].MaterialTypeId + "'";
                }
                if (materialMasterids == "") {
                    materialMasterids = "'','" + $scope.filters[i].MaterialMasterId + "'";
                }
                else {
                    materialMasterids += ",'" + $scope.filters[i].MaterialMasterId + "'";
                }
            }
        }
        $http({
            method: 'POST',
            url: 'Products/InventoryReceiveAddition/GetPurchaseConfirmationGRNData',
            data: {
                'fromDate': $scope.model.FromDate,
                'toDate': $scope.model.ToDate,
                'vendorId': vendorids,
                'materialTypeId': materialTypeids,
                'materialMasterId': materialMasterids,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.PurchaseConfirmationGRNDataList = response.data;
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
}