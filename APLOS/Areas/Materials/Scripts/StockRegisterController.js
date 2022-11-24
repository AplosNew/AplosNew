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


	$window.onresize = function (event) {

		$scope.actionCompleteSelected();

	};
	$scope.actionCompleteSelected = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridPrint").ejGrid("instance");
				var scrollerwidth = $("#PR").width();//Obtain the width of the container
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 300 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
		}

	};

	$scope.report.Days = 0,
	$scope.report.Type = 'Regular',
	$scope.report.ToDate = $filter("date")(Date.now(), 'dd-MMM-yyyy'),

	$scope.MaterialStockList = [];
	$scope.GetStockRegister = function () {
		debugger;

		//if ($scope.report.FromDate === null || $scope.report.FromDate === "") {
		//	ShowResult('Select From Date', 'failure');
		//	return false;
		//}
		 if ($scope.report.ToDate === null || $scope.report.ToDate === "") {
			ShowResult('Select To Date', 'failure');
			return false;
		}
		
		$http({
			method: 'POST',
			url: 'Materials/StockRegister/StockRegisterData',
			data: {
				fromDate: $scope.report.FromDate,
				toDate: $scope.report.ToDate,
				Days: $scope.report.Days,
				Type: $scope.report.Type,
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			$scope.MaterialStockList = response.data.NewData;
		});

	};

	$scope.StockRegisterReportExcel = function () {
		

		var dataList = [];
		var g = $("#GridStockRegister").data("ejGrid");
		dataList = g.getFilteredRecords();

		if (dataList.length == 0)
		{
			dataList = $scope.MaterialStockList;
        }

		$scope.fileName = 'Stock Register';

		$http({
			method: 'POST',
			//url: $scope.path + "StockRegisterReport",
			url: $scope.exportgriddataUrl,
			data: {
				'reportFileName': $scope.fileName,
				'data': dataList
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
   

