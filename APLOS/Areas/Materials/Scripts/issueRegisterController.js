'use strict';
issueRegisterController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService', '$window', '$controller'];
function issueRegisterController(fileReader, commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService, $window, $controller) {
	$rootScope.title = "Issue Register";
	$scope.Action = 'Save';
	$scope.index = -1;
	$scope.products = [];
	$scope.path = 'Materials/IssueRegister/';

	$scope.exportgriddataUrl = 'GridReports/ExcelExport';
	$scope.downloadgriddataUrl = 'GridReports/Download';
	$scope.Print = function () {
		var gridObj1 = $("#GridPO").data("ejGrid");
		var data1 = gridObj1.model.dataSource();
		$http({
			method: 'POST',
			url: $scope.exportgriddataUrl,
			data: { 'data': data1 }
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
		debugger;
		var gridObj11 = $("#GridPrint").data("ejGrid");
		var data11 = gridObj11.model.dataSource();
		$http({
			method: 'POST',
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
		Type: null
	};
	$scope.productNew.Type = 'All';
	$scope.changeSourceFrom = function (from) {
		if (from === 'All') {
			$scope.productNew.Type = 'All';
		}
		if (from === 'Posted') {
			$scope.productNew.Type = 'Posted';
		}
		if (from === 'NonPosted') {
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

	$scope.IssueRegisterList = [];
	$scope.GetIssueRegister = function () {
		if ($scope.report.FromDate === null || $scope.report.FromDate === "") {
			ShowResult('Select From Date', 'failure');
			return false;
		}
		else if ($scope.report.ToDate === null || $scope.report.ToDate === "") {
			ShowResult('Select To Date', 'failure');
			return false;
		}
		$http({
			method: 'POST',
			//url: $scope.getSearchListUrl,
			url: 'Materials/IssueRegister/GetIssueRegister',
			data: {
				fromDate: $scope.report.FromDate,
				toDate: $scope.report.ToDate,
				Type: $scope.productNew.Type
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {

			$scope.IssueRegisterList = response.data.NewData;
			//$scope.setTabGRNGRNIssueList();
		});

	};

	$scope.IssueRegisterListByGRN = [];
	$scope.GetIssueRegisterListByGRN = function () {
		if ($scope.report.FromDate === null || $scope.report.FromDate === "") {
			ShowResult('Select From Date', 'failure');
			return false;
		}
		else if ($scope.report.ToDate === null || $scope.report.ToDate === "") {
			ShowResult('Select To Date', 'failure');
			return false;
		}
		$http({
			method: 'POST',
			//url: $scope.getSearchListUrl,
			url: 'Materials/IssueRegister/GetIssueRegisterBYGRN',
			data: {
				fromDate: $scope.report.FromDate,
				toDate: $scope.report.ToDate,
				Type: $scope.productNew.Type
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			$scope.IssueRegisterListByGRN = response.data.NewData;
		});
	};

	$scope.getReport = function () {
		$scope.getaldataMaterialLedger();
	}
	$scope.getPurchaseRegisterReport = function () {
		$scope.GetPurchaseRegister();
	}
	$scope.getPurchaseRegisterReport1 = function () {
		$scope.ShowResultCustom("Coming Soon...");
	}
	
	$window.onresize = function (event) {
		$scope.actionCompleteSelected();
	};
	$scope.actionCompleteSelected = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridPrint").ejGrid("instance");
				var scrollerwidth = $("#Issues").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 300 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};

	$window.onresize = function (event) {
		$scope.actionCompleteSelected1();
	};
	$scope.actionCompleteSelected1 = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridPrint1").ejGrid("instance");
				var scrollerwidth = $("#Issues1").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 300 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};

	function IssueHistory(IssueDetailId) {
		$scope.IssueDetailList = [];
		$http.get($scope.path + 'GetIssueRegisterDetail?Id=' + IssueDetailId)
			.then(function (response) {
				$scope.IssueDetailList = response.data;

			});
		angular.element(document.querySelector('#ListOfRequisition1')).modal('show');
	}
	$scope.RequisitionListHide = function () {
		$scope.taxCategoryList = [];
		angular.element(document.querySelector('#ListOfRequisition')).modal('hide');
	};
	$scope.RequisitionListHide1 = function () {
		$scope.taxCategoryList = [];
		angular.element(document.querySelector('#ListOfRequisition1')).modal('hide');
	};
	$scope.recorddoubleclick = function ($event) {
		var x = $event;
		var Id = x.data.IssueDetailId;
		IssueHistory(Id);
	};

	$scope.tab = 1;
	$scope.setTabIssueList = function (newTab) {
		$scope.tab = newTab;
		//$scope.ReqStatus = 'ForChecked';
		$scope.GetIssueRegister();
	};
	$scope.isSetIssueList = function (tabNum) {
		return $scope.tab === tabNum;
	};
	$scope.setTabGRNGRNIssueList = function (newTab) {
		$scope.tab = newTab;
		//$scope.ReqStatus = 'HoldReject';
		$scope.GetIssueRegisterListByGRN();
	};
	$scope.isSetGRNIssueList = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.IssueRegisterReportPdf = function (id, reportFormat) {
		if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
			ShowResult('Select From Date', 'failure');
			return false;
		}
		if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
			ShowResult('Select To Date', 'failure');
			return false;
		}
		//$scope.MaterialConsumptionReportPdf = function (id, reportFormat) {
			debugger;
			var reportFormat = "Pdf";
			//if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
			$window.open('Materials/IssueRegister/Report?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Type=' + $scope.productNew.Type, '_blank');
		//};
	}

	$scope.IssueRegisterReportExcel = function (reportFormat) {
		if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
			ShowResult('Select From Date', 'failure');
			return false;
		}
		if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
			ShowResult('Select To Date', 'failure');
			return false;
		}
		try {
			var Excel;
			var file_src = 'Materials/IssueRegister/Report?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Type=' + $scope.productNew.Type;
			$rootScope.report(file_src);

		} catch (e) {

		}
	}

	//$scope.GRNIssueRegisterReportPdf = function (id, reportFormat) {
	//	debugger;
	//	if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
	//		ShowResult('Select From Date', 'failure');
	//		return false;
	//	}
	//	if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
	//		ShowResult('Select To Date', 'failure');
	//		return false;
	//	}
	//	var reportFormat = "Pdf";
	//	//if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
	//	$window.open('Materials/IssueRegister/CreateIssueRegisterGRNIssueReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Type=' + $scope.productNew.Type, '_blank');
	//};

	$scope.GRNIssueRegisterReportExcel = function (reportFormat) {
		if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
			ShowResult('Select From Date', 'failure');
			return false;
		}
		if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
			ShowResult('Select To Date', 'failure');
			return false;
		}
		try {
			var Excel;
			var file_src = 'Materials/IssueRegister/CreateIssueRegisterGRNIssueReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Type=' + $scope.productNew.Type;
			$rootScope.report(file_src);

		} catch (e) {

		}
	}


}