'use strict';
StockRegisterController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService', '$window', '$controller'];
function StockRegisterController(fileReader, commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService, $window, $controller) {
	$rootScope.title = "Stock Register Report";
	$scope.Action = 'Save';
	$scope.index = -1;
	$scope.products = [];
	$scope.path = 'Materials/StockRegister/';
	$scope.path1 = 'Accounts/InventoryPayable/';
	$scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
	$scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

	$scope.downloadgriddataUrl = 'GridReports/Download';
	$controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
	$scope.RowColor = "";
	$scope.isAlternative = -1;
	$scope.rowDataBound = function rowDataBound(e) {

		if ($scope.RowColor != e.data.Id) {
			$scope.isAlternative = $scope.isAlternative * -1;
			$scope.RowColor = e.data.Id;
		}
		if ($scope.isAlternative > 0)
			e.row.css("background-color", '#D3D3D3');
		else
			e.row.css("background-color", '#ffffff');


	}
	$scope.Print = function () {

		var gridObj1 = $("#GridPO").data("ejGrid");
		var data1 = gridObj1.model.dataSource();
		$http({
			method: 'POST',
			url: $scope.exportgriddataUrl,
			//data: { 'data': data1 }
			data: JSON.stringify(data1)
		}).then(function successCallback(response) {
			if (response.data.Error == true) {
				// ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');

			}
			else {

				location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
			}
		});
	}
	$scope.PrintPurchaseRegister = function () {

		var gridObj11 = $("#GridPrint").data("ejGrid");
		var data11 = gridObj11.model.dataSource();

		$http({

			method: "POST",
			url: $scope.exportgriddataUrl,
			data: { 'data': data11 }

		}).then(function successCallback(response) {
			if (response.data.Error == true) {
				// ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');

			}
			else {

				location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
			}
		});

	}


	$scope.productNew = {
		Type: null,
		WithStock: true,
		WithoutStock: false,
		Storage: false
	};
	$scope.changeSourceFrom = function (from) {
		debugger;
		if (from === 'AsOnDate') {
			$scope.report.FromDate = "";

			$scope.productNew.Type = 'Posted';

		}
		if (from === 'ForThePeriod') {
			$scope.productNew.Type = 'NonPosted';


		}
	};

	$scope.GriddataMaterialLedger = [];
	$scope.getaldataMaterialLedger = function () {

		$http({
			method: 'POST',
			//url: $scope.getSearchListUrl,
			url: 'Materials/MaterialLedger/GetMaterialLedger',
			data: {
				fromDate: $scope.report.FromDate,
				toDate: $scope.report.ToDate,
				Type: $scope.productNew.Type
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			$scope.GriddataMaterialLedger = response.data;

			//entrydata = copy(searchdata);
		});
	};


	$scope.PurchaseRegisterList = [];
	$scope.PurchaseRegisterItemWiseList = [];
	$scope.PurchaseRegisterPartyWiseList = [];
	$scope.pivotTableFieldListID = [];

	$scope.GetStockRegister = function () {
		debugger;

		if ($scope.report.FromDate === null || $scope.report.FromDate === "") {
			ShowResult('Select From Date', 'failure');
			return false;
		}
		else if ($scope.report.ToDate === null || $scope.report.ToDate === "") {
			ShowResult('Select To Date', 'failure');
			return false;
		}
		else if ($scope.report.ReportType === null || $scope.report.ReportType === "") {
			ShowResult('Please select Report Type', 'failure');
			return false;
		}

		$http({
			method: 'POST',
			url: 'Materials/StockRegister/StockRegisterData',
			data: {
				fromDate: $scope.report.FromDate,
				toDate: $scope.report.ToDate,
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			$scope.PurchaseRegisterList = response.data.NewData;

			for (var i = 0; i < $scope.PurchaseRegisterList.length; i++) {
				response.data[i].GRNEntryDate = new Date($scope.PurchaseRegisterList[i].GRNEntryDate);
			}

			$scope.load();
		});

	};

	$scope.PurchaseOrderGRNWiseReportExcel = function () {
		if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
			ShowResult('Select From Date', 'failure');
			return false;
		}
		if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
			ShowResult('Select To Date', 'failure');
			return false;
		}

		//var dataList = [];
		//var g = $("#GridStockRegister").data("ejGrid");
		//dataList = g.getFilteredRecords();
		//var ids = "";
		//if (baseService.arrayLength(dataList) > 0) {
		//	for (var i = 0; i < dataList.length; i++) {
		//		if (ids == "") {
		//			ids = "'','" + dataList[i].SLNo + "'";
		//		}
		//		else {
		//			ids += ",'" + dataList[i].SLNo + "'";
		//		}
		//	}
		//}
		//else {
		//	for (var i = 0; i < $scope.PurchaseRegisterList.length; i++) {
		//		if (ids == "") {
		//			ids = "'','" + $scope.PurchaseRegisterList[i].SLNo + "'";
		//		}
		//		else {
		//			ids += ",'" + $scope.PurchaseRegisterList[i].SLNo + "'";
		//		}
		//	}
		//}
		var gridObjStockReg = $("#GridStockRegister").data("ejGrid");
		var dataStockReg = gridObjStockReg.model.dataSource();

		$scope.fileName = 'Stock Register';

		$http({
			method: 'POST',
			//url: $scope.path + "StockRegisterReport",
			url: $scope.exportgriddataUrl,
			data: {
				'reportFileName': $scope.fileName,
				'data': dataStockReg
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			if (response.data.Error == true) {
				ShowResult(response.data.Message, 'failure');
			}
			else {
				//$rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
				$window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
			}
		}, function errorCallback(response) {
			ShowResult(response.data.Message, 'failure');
		});
	}
}
   

