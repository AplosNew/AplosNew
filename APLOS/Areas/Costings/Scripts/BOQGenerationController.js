'use strict';
BOQGenerationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function BOQGenerationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'BOQ Generation';
    $scope.path = "Costings/BOQGeneration/";
    $scope.ModelBase = { Id: null, CustomerId: null, CustomerName: null, EmployeeSystemId: null, EmployeeName: null, Remarks: null, UserName: null };
    $scope.Model = Object.assign({}, $scope.ModelBase);


    $scope.CustomerList = [];
    $scope.SelectedCustomer = {};
    $scope.searchByCustomer = [{ name: 'Customer', value: 'Customer' }]; $scope.searchCustomer = 'Customer'; $scope.searchCustomerValue = '';
    $scope.GetCustomerList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetCustomerList",
            data: { column: $scope.searchCustomer, value: $scope.searchCustomerValue },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CustomerList = response.data.DATA;
        });
    }
    $scope.SelectCustomer = function (args) {
        $scope.SelectedCustomer = args.data;
        $scope.Model.CustomerId = $scope.SelectedCustomer.Id;
        $scope.Model.CustomerName = $scope.SelectedCustomer.Customer;
        $scope.SalesOrderList = [];
        $rootScope.closePopup('dialogCustomerSearch');
    }


    function containsSpecialChars(str) {
        const specialChars = /[@!#$%^&*()_+\=\[\]{};':"|,.<>\?`~]/;
        return specialChars.test(str);
    }

    $scope.CheckSpecialCharecter = function () {
        try {
            if (containsSpecialChars($scope.Model.UserName)) {
                $scope.Model.UserName = $scope.Model.UserName.substring(0, $scope.Model.UserName.length - 1);
                throw "No special characters allowed for User Name.";
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.SalesOrderList = [];
    $scope.SelectedSalesOrder = {};
    $scope.searchBySalesOrder = [{ name: 'Sales Order Id', value: 'SalesOrderId' }, { name: 'Master Order Id', value: 'MasterOrderId' },
    { name: 'Buyer Order#', value: 'BuyerOrderNo' }, { name: 'Own Order#', value: 'OwnOrderNo' },
    { name: 'Buyer Item', value: 'BuyerItemNo' }, { name: 'Own Item', value: 'OwnItemNo' },
    { name: 'Description', value: 'Description' },
    { name: 'Order Status', value: 'OrderStatusName' }, { name: 'Order Category', value: 'OrderCategoryName' }]; $scope.searchSalesOrder = 'SalesOrderId'; $scope.searchSalesOrderValue = '';
    $scope.GetSalesOrderList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetSalesOrderList",
            data: { column: $scope.searchSalesOrder, value: $scope.searchSalesOrderValue, PartyId: $scope.Model.CustomerId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SalesOrderList = response.data.DATA;
            for (var i = 0; i < $scope.SalesOrderList.length; i++) {
                $scope.SalesOrderList[i].Selected = false;
                var sel = ej.DataManager($scope.SelectedSalesOrderList).executeLocal(ej.Query().where("SalesOrderId", "equal", $scope.SalesOrderList[i].SalesOrderId));
                if (sel.length > 0)
                    $scope.SalesOrderList[i].Selected = true;
            }
        });
    }
    $scope.SelectedSalesOrderList = [];
    $scope.ApplySelectSalesOrder = function () {

        try {
            var _sel = ej.DataManager($scope.SalesOrderList).executeLocal(ej.Query().where("Selected", "equal", true));
            for (var i = 0; i < _sel.length; i++) {
                if (_sel[i].CanSelect == false) {
                    throw "This SalesOrder "+_sel[i].SalesOrderId+" already used.";
                }
            }

            if (_sel.length > 0) {
                var _temp = _sel[0].OrderCostingMasterTemplateId;
                for (var i = 0; i < _sel.length; i++) {

                    if (_sel[i].OrderCostingMasterTemplateId !== null && _sel[i].Approved == "No") {
                        ShowResult('Costing should be approved.', 'failure');
                        return;
                    }

                    if (_sel[i].OrderCostingMasterTemplateId != _temp) {
                        ShowResult('Sales order with different costing id not allowed', 'failure');
                        return;
                    }

                    if (!_sel[i].OrderCostingMasterTemplateId) {
                        ShowResult('Sales order#' + _sel[i].SalesOrderId + ' does not have costing', 'failure');
                        return;
                    }
                }
            }
            $scope.SelectedSalesOrderList = _sel;
            $scope.Submit();
            $rootScope.closePopup('dialogSalesOrderSearch');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.SelectSalesOrder = function (args) {
        $scope.SelectedSalesOrder = args.data;
        $scope.Model.SalesOrderId = $scope.SelectedSalesOrder.SalesOrderId;
        $rootScope.closePopup('dialogSalesOrderSearch');
    }


    $scope.EditList = [];
    $scope.SelectedEdit = {};
    $scope.searchByEdit = [{ name: 'BOM Id', value: 'Id' }, { name: 'Sales Order Id', value: 'SalesOrderId' }, { name: 'Master Order Id', value: 'MasterOrderId' },
    { name: 'Buyer Order#', value: 'BuyerOrderNo' }, { name: 'Own Order#', value: 'OwnOrderNo' },
    { name: 'Buyer Item', value: 'BuyerItemNo' }, { name: 'Own Item', value: 'OwnItemNo' },
    { name: 'Resp. Person', value: 'EmployeeName' }, { name: 'User Name', value: 'UserName' },
    { name: 'Description', value: 'Description' }, { name: 'Selected Item', value: 'ItemList' },
    { name: 'Order Status', value: 'OrderStatusName' }, { name: 'Order Category', value: 'OrderCategoryName' }]; $scope.searchEdit = 'SalesOrderId'; $scope.searchEditValue = '';
    $scope.GetEditList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetEditList",
            data: { column: $scope.searchEdit, value: $scope.searchEditValue },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EditList = response.data.DATA;
        });
    }
    $scope.GetEditList();

    $scope.SelectEdit = function (args) {
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.Model = Object.assign({}, args.data);
        //$scope.Model.Id = args.data.Id;
        //$scope.Model.CustomerId = args.data.CustomerId;
        //$scope.Model.CustomerName = args.data.Customer;
        //$scope.Model.SalesOrderId = args.data.SalesOrderId;

        $scope.GetExistingSalesOrderList();
    }

    $scope.GetExistingSalesOrderList = function () {

        $http({
            method: 'POST',
            url: $scope.path + "GetExistingSalesOrderList",
            data: { BOMMasterId: $scope.Model.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SelectedSalesOrderList = response.data.DATA;
            $scope.Submit();
        });
    }

    $scope.Submit = function () {
        $scope.GetItemList();
    }


    $scope.MaterialList = [];
    $scope.GetItemList = function () {

        $http({
            method: 'POST',
            url: $scope.path + "GetItemList",
            data: { SelectedSalesOrders: $scope.SelectedSalesOrderList, SalesOrderId: $scope.SelectedSalesOrderList[0]["SalesOrderId"], CostingBOQMasterId: $scope.Model.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialList = response.data.DATA;
        });
    }


    $scope.employee = [];
    $scope.getPopUpData = function () {
        $scope.employee = [];
        $http({
            method: 'GET',
            url: 'Costings/QuickCostingMaster/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
    }
    $scope.getPopUpData();

    $scope.setEmpData = function (obj) {
        $scope.Model.EmployeeSystemId = obj.data.SystemID;
        $scope.Model.EmployeeName = obj.data.EmployeeName;

        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };


    $scope.Save = function () {
        $scope.MaterialCheckedData = [];
        for (var i = 0; i < $scope.MaterialList.length; i++) {
            if ($scope.MaterialList[i].Selected == true && $scope.MaterialList[i].Saved == false) {
                $scope.MaterialCheckedData.push($scope.MaterialList[i]);
            }
        }
        
        //var CheckedData = ej.DataManager($scope.MaterialList).executeLocal(ej.Query().where("Selected", "equal", true));
        var _SalesOrderData = ej.DataManager($scope.SelectedSalesOrderList).executeLocal(ej.Query().select(["SalesOrderId", "RN","CostingStage"]));
        $http({
            method: 'POST',
            url: $scope.path + "Save",
            data: { MasterData: $scope.Model, SalesOrderData: _SalesOrderData, ItemData: $scope.MaterialCheckedData },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.Model = response.data.DATA;
                $scope.GetEditList();
                $scope.GetItemList();
            }
           // $scope.closeEntryDialog();
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    }
    $scope.Delete = function () {

        $http({
            method: 'POST',
            url: $scope.path + "Delete",
            data: { Id: $scope.Model.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.Cancel();
                $scope.GetEditList();
            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    }
    $scope.Cancel = function () {
        $scope.Model = Object.assign({}, $scope.ModelBase);
        $scope.MaterialList = [];
        $scope.SelectedSalesOrderList = [];
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

    $scope.isAlternative = -1;
    var TempOrderCostingMasterTemplateId = '';
    $scope.rowDataBound = function rowDataBound(e) {

        if (TempOrderCostingMasterTemplateId != e.data.OrderCostingMasterTemplateId) {
            $scope.isAlternative = $scope.isAlternative * -1;
            TempOrderCostingMasterTemplateId = e.data.OrderCostingMasterTemplateId;
        }
        if ($scope.isAlternative > 0)
            e.row.css("background-color", '#fff6b7');
        else
            e.row.css("background-color", '#d1e5ff');
    }
    $scope.rowDataBoundMaterial = function rowDataBound(e) {
        var SelectedSOCount = $scope.SelectedSalesOrderList.length;
        if (e.data.SOCount == 0) {
            //virgin material for selected so
            e.row.css("background-color", '#BDFFF6');
        }
        else if (e.data.SOCount == SelectedSOCount) {
            //prostitute material, because all selected SO was processed for this material
            e.row.css("background-color", '#C3FFA5');
        }
        else {
            //not processed for all of the sales orders
            e.row.css("background-color", '#FCFFD3');
        }

    }

    $scope.SelectedSalesOrderListForMaterial = [];
    $scope.ShowProcessedSOList = function (data) {
        var soList = getString($scope.SelectedSalesOrderList, "SalesOrderId");
        $http({
            method: 'POST',
            url: $scope.path + "SalesOrderListForExistingProcess",
            data: { SalesOrderIds: soList, OrderProcurementCostingDirectMaterialId: data.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SelectedSalesOrderListForMaterial = response.data.DATA;
            $rootScope.openPopup('SalesOrderListForMaterialPopUp');
        });
    }
    $scope.ReportXls = function () {
        //var soList = getString($scope.SelectedSalesOrderList, "SalesOrderId");
        try {
            var file_src = $scope.path + "ReportXls?CostingBOQMasterId=" + $scope.Model.Id
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    $scope.NonProcessReportXls = function () {
        try {
            var file_src = $scope.path + "GetNonProcessReportXls?CostingBOQMasterId=" + $scope.Model.Id
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

}

