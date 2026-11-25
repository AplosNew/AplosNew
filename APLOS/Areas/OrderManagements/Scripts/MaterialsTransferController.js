'use strict';
MaterialsTransferController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$window'];
function MaterialsTransferController(commonMessage, $scope, $rootScope, baseService, $http, $window) {
    $rootScope.title = "Material Transfer";

    // ------------------------------------------------------------------
    // 1. HEADER  (Purpose, From Location, To Location, By Whom, To Whom, Remark, Grade)
    // ------------------------------------------------------------------
    $scope.HeaderTemp = {
        Id: null,
        Purpose: null,
        FromLocationId: null,
        ToLocationId: null,
        ByWhomId: null,
        ToWhomId: null,
        Remark: '',
        Grade: null
    };
    $scope.Header = angular.copy($scope.HeaderTemp);

    $scope.PurposeList = [];
    $scope.LocationList = [];
    $scope.EmployeeList = [];
    $scope.GradeList = [];

    $scope.loadHeaderCbo = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetTransferCbo'
        }).then(function (response) {
            $scope.PurposeList = response.data.Purposes;
            $scope.LocationList = response.data.Locations;
            $scope.EmployeeList = response.data.Employees;
            $scope.GradeList = response.data.Grades;
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        });
    };
    $scope.loadHeaderCbo();

    // ------------------------------------------------------------------
    // 2. FILTER  (SOStatus, Customer, Style, Description, SONo, LineItemRef, GO)
    // ------------------------------------------------------------------
    $scope.Filter = {
        SOStatus: null,
        Customer: null,
        Style: null,
        Description: '',
        SONo: '',
        LineItemReference: ''
    };

    $scope.SOStatusList = [];
    $scope.CustomerList = [];
    $scope.StyleList = [];

    $scope.loadFilterCbo = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetFilterCbo'
        }).then(function (response) {
            $scope.SOStatusList = response.data.SOStatus;
            $scope.CustomerList = response.data.Customers;
            $scope.StyleList = response.data.Styles;
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        });
    };
    $scope.loadFilterCbo();

    // ------------------------------------------------------------------
    // 3. SALES ORDER LINE ITEM GRID  (result of GO)
    // ------------------------------------------------------------------
    $scope.SalesOrderItemList = [];
    $scope.SelectedItem = null;

    $scope.GetSalesOrderItems = function () {
        var parameters = [];
        angular.forEach($scope.Filter, function (value, key) {
            parameters.push({ "Key": key, "Value": baseService.isUndefinedOrNull(value) ? '' : String(value) });
        });

        $http({
            method: 'POST',
            url: 'OrderManagements/MasterOrder/GetSalesOrderItems',
            data: { 'parameters': parameters },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                $scope.SalesOrderItemList = response.data;
                $scope.SKUList = [];
                $scope.SelectedItem = null;
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        });
    };

    // ------------------------------------------------------------------
    // 4. SKU MATRIX  (SKU -> Colors (rows) x Sizes (columns) -> Qty)
    //    SKUList = [{ SKUId, SKUName,
    //                 Sizes:  [{SizeId, SizeName}],
    //                 Colors: [{ColorId, ColorName, Qty:{ <SizeId>: number }}] }]
    // ------------------------------------------------------------------
    $scope.SKUList = [];

    // called from the grid row button or double click (grid passes "this" / args)
    $scope.LoadSKUMatrix = function (row) {
        var item = row.data ? row.data : row.rowData;
        $scope.SelectedItem = item;

        $http({
            method: 'POST',
            url: 'OrderManagements/MasterOrder/GetSKUMatrix',
            data: { 'salesOrderItemId': item.MOIId },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                $scope.SKUList = response.data;
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        });
    };

    $scope.skuTotal = function (sku) {
        var total = 0;
        angular.forEach(sku.Colors, function (c) {
            angular.forEach(c.Qty, function (q) {
                total += Number(q) || 0;
            });
        });
        return total;
    };

    // flatten matrix -> rows (only cells with Qty > 0)
    function buildTransferItems() {
        var items = [];
        angular.forEach($scope.SKUList, function (sku) {
            angular.forEach(sku.Colors, function (color) {
                angular.forEach(sku.Sizes, function (size) {
                    var qty = Number(color.Qty[size.SizeId]) || 0;
                    if (qty > 0) {
                        items.push({
                            MOIId: $scope.SelectedItem.MOIId,
                            SKUId: sku.SKUId,
                            ColorId: color.ColorId,
                            SizeId: size.SizeId,
                            Qty: qty
                        });
                    }
                });
            });
        });
        return items;
    }

    // ------------------------------------------------------------------
    // 5. SAVE  (header + matrix)
    // ------------------------------------------------------------------
    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.Header.FromLocationId)) throw "Select From Location.";
            if (baseService.isUndefinedOrNull($scope.Header.ToLocationId)) throw "Select To Location.";
            if ($scope.Header.FromLocationId === $scope.Header.ToLocationId) throw "From and To Location cannot be same.";
            if (baseService.isUndefinedOrNull($scope.SelectedItem)) throw "Select a sales order line item first.";

            var items = buildTransferItems();
            if (items.length === 0) throw "Enter at least one quantity.";

            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/SaveTransfer',
                data: { 'header': $scope.Header, 'items': items },
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                } else {
                    $scope.Header.Id = response.data.TransferId;   // needed for the carton section
                    ShowResult(response.data.Message, 'success');
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    // ------------------------------------------------------------------
    // 6. CARTON / CHILD SECTION
    //    NoOfCarton | CartonRefNo | PackedBy | Remark | Save Child | GenerateQR(Qty)
    // ------------------------------------------------------------------
    $scope.CartonList = [];

    $scope.AddCartonRow = function () {
        $scope.CartonList.push({
            Id: -(Math.floor(Math.random() * 1000000) + 1),
            NoOfCarton: 1,
            CartonRefNo: '',
            PackedById: null,
            Remark: '',
            QRQty: 1
        });
    };
    $scope.AddCartonRow();

    $scope.RemoveCartonRow = function (index) {
        $scope.CartonList.splice(index, 1);
    };

    $scope.SaveChild = function (carton) {
        try {
            if (baseService.isUndefinedOrNull($scope.Header.Id)) throw "Save the transfer first.";
            if (!(carton.NoOfCarton > 0)) throw "Enter No Of Carton.";

            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/SaveCarton',
                data: { 'transferId': $scope.Header.Id, 'data': carton },
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                } else {
                    carton.Id = response.data.CartonId;
                    ShowResult(response.data.Message, 'success');
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GenerateQR = function (carton) {
        try {
            if (carton.Id <= 0 || baseService.isUndefinedOrNull(carton.Id)) throw "Save the carton row first.";
            if (!(carton.QRQty > 0)) throw "Enter QR quantity.";

            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/GenerateCartonQR',
                data: { 'cartonId': carton.Id, 'qty': carton.QRQty },
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                } else {
                    ShowResult(response.data.Message, 'success');
                    if (response.data.Url) { $window.open(response.data.Url, '_blank'); }
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Reset = function () {
        $scope.Header = angular.copy($scope.HeaderTemp);
        $scope.SalesOrderItemList = [];
        $scope.SKUList = [];
        $scope.SelectedItem = null;
        $scope.CartonList = [];
        $scope.AddCartonRow();
    };
}
