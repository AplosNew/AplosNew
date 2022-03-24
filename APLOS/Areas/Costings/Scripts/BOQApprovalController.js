'use strict';
BOQApprovalController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', '$window'];
function BOQApprovalController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, $window) {
    $rootScope.title = 'BOQ Approve';
    $scope.path = "Costings/BOQGeneration/";
    $scope.ModelBase = { Id: null, CustomerId: null, CustomerName: null, EmployeeSystemId: null, EmployeeName: null, Remarks: null, UserName: null };
    $scope.Model = Object.assign({}, $scope.ModelBase);

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

}

