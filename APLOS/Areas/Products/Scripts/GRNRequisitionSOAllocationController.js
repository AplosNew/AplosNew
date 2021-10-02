'use strict';
GRNRequisitionSOAllocationController.$inject = ['accountService', 'addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller', '$location'];
function GRNRequisitionSOAllocationController(accountService, addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $location) {
    


	// #region All Tab Control
	$scope.GRN = "";
	$scope.tab = 1;
	$scope.setTabpou = function (newTab) {
		$scope.tab = newTab;
		$scope.getListForGRNUnchecked();

	};
	$scope.isSetpou = function (tabNum) {
		return $scope.tab === tabNum;
		$scope.GRN = 0;
	};


	$scope.setTabpou1 = function (newTab) {
		$scope.tab = newTab;
		$scope.getalldata1();
	};
	$scope.isSetpou1 = function (tabNum) {
		return $scope.tab === tabNum;
		$scope.GRN = 0;
	};
	// $scope.tab = 2;

	$scope.setTabpoa = function (newTab) {

		$scope.tab = newTab;
		$scope.GRN = 1;
		$scope.getListForGRNCheckedList();
		//alert('Checked Tab');
	};
	$scope.isSetpoa = function (tabNum) {
		return $scope.tab === tabNum;
		$scope.GRN = 1;
	};
	// End PO approve

	$scope.setTabGRNRejectHoldList = function (newTab) {

		$scope.tab = newTab;
		//$scope.getListForGRNRejectHoldList();
		$scope.getListForGRNRejectHoldList();
	};
	$scope.isSetGRNRejectHoldList = function (tabNum) {
		return $scope.tab === tabNum;

	};

	$scope.setTabGRNRejectHoldList1 = function (newTab) {

		$scope.tab = newTab;
		//$scope.getListForGRNRejectHoldList();
		$scope.GRNNGriddataHoldReject();
	};
	$scope.isSetGRNRejectHoldList1 = function (tabNum) {
		return $scope.tab === tabNum;

	};

	$scope.setTabpoApproval = function (newTab) {

		$scope.tab = newTab;
		$scope.GRN = 1;
		$scope.Griddataapprovpo1();

		//alert('Checked Approval');
	};
	$scope.isSetpoApproval = function (tabNum) {
		return $scope.tab === tabNum;
		$scope.GRN = 1;
	};




	$scope.setTabGRNApproval = function (newTab) {

		$scope.tab = newTab;
		$scope.GRN = 1;
		$scope.Griddataapprovpo1();

		//alert('Checked Approval');
	};
	$scope.isSetGRNApproval = function (tabNum) {
		return $scope.tab === tabNum;
		$scope.GRN = 1;
	};

	
// #endregion
	$scope.JWGRNData = [];
	$scope.GetJWGRData = function () {
		//debugger;
		$http({
			method: 'GET',
			dataType: 'JSON',
			url: 'Products/GoodsReceiveNote/GetJWReceiptDataForAllocation',
		}).then(function successCallback(response) {
			$scope.JWGRNData = response.data;
		});
	};
	
	$scope.GetJWGRData();
}