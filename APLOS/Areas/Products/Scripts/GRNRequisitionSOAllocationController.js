'use strict';
GRNRequisitionSOAllocationController.$inject = ['accountService', 'addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller', '$location'];
function GRNRequisitionSOAllocationController(accountService, addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $location) {
    


	// #region All Tab Control
	$scope.Action = "Update";
	$scope.GRN = "";
	$scope.tab = 1;
	$scope.setTabpou = function (newTab) {
		$scope.tab = newTab;
		$scope.GetJWGRData();

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



	$scope.UpdateJWSOAllocation = function () {

		$scope.detailListNew = [];
		for (var i = 0; i < $scope.JWGRNData.length; i++) {
			if ($scope.JWGRNData[i].Active === true) {
				//var JWGRNData = $filter("filter")($scope.JWGRNData, { "MaterialMasterId": $scope.JWGRNData[i].MaterialMasterId, "ArticleId": $scope.JWGRNData[i].ArticleId, "FirstCharacteristicsValueId": $scope.JWGRNData[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.JWGRNData[i].SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.JWGRNData[i].ThirdCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.JWGRNData[i].InventoryReceiveDetailId, "check": true }).TransactionQty;
				//$scope.JWGRNData[i].TransactionQty1 = $filter('sumByKey')($filter('filter')($scope.JWGRNData, {	MaterialMasterId: $scope.JWGRNData[i].MaterialMasterId, ArticleId: $scope.JWGRNData[i].ArticleId, FirstCharacteristicsValueId: $scope.JWGRNData[i].FirstCharacteristicsValueId, SecondCharacteristicsValueId: $scope.JWGRNData[i].SecondCharacteristicsValueId, ThirdCharacteristicsValueId: $scope.JWGRNData[i].ThirdCharacteristicsValueId, InventoryReceiveDetailId: $scope.JWGRNData[i].InventoryReceiveDetailId, "check": true }), 'TransactionQty1');
				$scope.detailListNew.push($scope.JWGRNData[i]);
			}
		}
		
		if ($scope.Action === "Update") {
			$http({
				method: 'POST'
				, url: 'Products/GoodsReceiveNote/CreateJWSOAllocation'
				, data: {'Data': $scope.detailListNew }
				, dataType: 'JSON'
			}).then(function (response) {
				if (response.data.Error === true)
					ShowResult(response.data.Message, 'failure');
				else {
					ShowResult(response.data.Message, 'success');
					//$scope.Clear();
					//$scope.getdataInventoryIssue();
					//$scope.productNew.Id = response.data.inventoryIssue.Id;
					//$scope.getData();
					//$scope.GetDataList();
				}
			}), function (response) {
				ShowResult(response.data.Message, 'failure');
			};
		}
		else ShowResult('Please issue material', 'failure');
	};

	$scope.CalculateBaseQty = function (data) {
		if (data.TransactionQty > data.TransactionQty1) {
			ShowResult('Current Transaction Qty can not grater than Transaction Qty ', 'failure');
			data.TransactionQty = 0;
			data.BaseQty = 0;
			return false;

		}
		if (data.BaseQty > data.BaseQty1) {
			ShowResult('Current Base Qty can not grater than Base Qty ', 'failure');
			data.BaseQty = 0;
			return false;
		}
		else {
			data.BaseQty = (data.TransactionQty * data.BaseUOMFactor);
		}
		
	}
}