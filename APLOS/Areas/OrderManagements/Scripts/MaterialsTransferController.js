'use strict';
MaterialsTransferController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$window'];
function MaterialsTransferController(commonMessage, $scope, $rootScope, baseService, $http, $window) {
    $rootScope.title = "Material Transfer";
    $scope.ModelList = [];
    $scope.searchBy = "Purpose"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Purpose', name: "Purpose" }, { value: 'Grade', name: "Grade" }, { value: 'Remarks', name: "Remarks" }];

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/MasterOrder/GetMatTransferList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.Model = {
        Id: null,
        Purpose: null,
        FromLocationId: null,
        ToLocationId: null,
        ByWhomId: null,
        ToWhomId: null,
        Remark: '',
        Grade: null
    };
    $scope.ModelNew = angular.copy($scope.Model);


    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.purposeList = [];
    $http({
        method: 'GET',
        url: "Productions/MovementMaterialMaster/getPurposeCbo",
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.purposeList = response.data;
    });

    $scope.getLocation = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.PurposeId)) {
            var paymentTerm = $.grep($scope.purposeList, function (item) { return item.Value === $scope.ModelNew.PurposeId; })[0];
            $scope.ModelNew.Location = paymentTerm.FromToLocation;
            $scope.ModelNew.MaterialMovementMasterId = paymentTerm.MaterialMovementMasterId;
            $scope.ModelNew.ProcessId = paymentTerm.ProcessId;

        }
    };

    $scope.name = null;
    $scope.popUpDataList = [];
    $scope.getEmpPopUpData = function (name) {
        try {
            $scope.name = name;

            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'employees/authorizationconfig/getallemployeedata'

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectEmployee = function (obj) {
        if ($scope.name == 'By') {
            $scope.ModelNew.ByWhomId = obj.data.SystemId;
            $scope.ModelNew.ByWhom = obj.data.EmployeeName;
        }
        else {
            $scope.ModelNew.ToWhomId = obj.data.SystemId;
            $scope.ModelNew.ToWhom = obj.data.EmployeeName;
        }
        angular.element(document.querySelector('#popUp')).modal('hide');
    };


    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.GeneralForm.$valid) {
            $http({
                method: 'POST',
                url: "OrderManagements/MasterOrder/CreateMatTransfer",
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelNew.Id = response.data.Data.Id;
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.modalNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.modalNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        $scope.Model = {
            Id: null,
            Purpose: null,
            FromLocationId: null,
            ToLocationId: null,
            ByWhomId: null,
            ToWhomId: null,
            Remark: '',
            Grade: null
        };
        $scope.ModelNew = angular.copy($scope.Model);
    };

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
    $scope.Save_ = function () {
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
