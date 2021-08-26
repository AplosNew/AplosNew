'use strict';
POParameterChangeController.$inject = ['accountService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService', 'bankService', '$controller'];
function POParameterChangeController(accountService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService, bankService, $controller) {
    $rootScope.title = "PO";
    $scope.Action = 'Save';
    $scope.path = 'Products/POParameterChange/';
    $scope.getListUrl = $scope.path + 'getlist';

	$scope.POdataList = [];
	$scope.POTypeStatus = 'Pending';
	$scope.getPOdata = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/GetPOTypeList?POTypeStatus=' + $scope.POTypeStatus,
		}).then(function successCallback(response) {
			$scope.POdataList = response.data;
		});
	};
	$scope.getPOdata();

	$scope.lst = [];
	$scope.POListDetails = function () {

		$http({
			method: 'GET',
			//url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
			url: 'Products/PurchaseOrder/GetInventoryMaterialListPoByReqDetail'
		}).then(function successCallback(response) {
			$scope.lst = response.data;
			//$scope.detailgrid($scope.lst);
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

}
