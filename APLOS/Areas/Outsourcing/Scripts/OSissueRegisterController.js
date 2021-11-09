'use strict';
OSissueRegisterController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService', '$window', '$controller'];
function OSissueRegisterController(fileReader, commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService, $window, $controller) {
	$rootScope.title = "OutSource Issue Register";
	$scope.Action = 'Save';
	$scope.index = -1;
	$scope.products = [];
	$scope.path = 'Outsourcing/OSissueRegister/';

	$scope.exportgriddataUrl = 'GridReports/ExcelExport';
	$scope.downloadgriddataUrl = 'GridReports/Download';
	$scope.Print = function () {
		;
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
		;
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
	//$scope.model = {
	//    Id: null,
	//    CompanyGroupId: null,
	//    Sequence: null,
	//    Code: null,
	//    ShortName: null,
	//    StandardName: null,
	//    UserName: null,
	//    OperationActivityId: null,
	//    OperationTypeId: null,
	//    OperationCategoryId: null,
	//    SkillId: null,
	//    Type: null,
	//    MachineMasterId: null,
	//    SkillGroupId: null,
	//    LegalDesignationId: null,
	//    ProcessId: null,
	//    ProposedSalary: null,
	//    Remarks: null,
	//    Active: null
	//};
	//$scope.modelNew = Object.assign({}, $scope.model);
	$scope.productNew = {
		Type: null
	};
	$scope.productNew.Type = 'NonPosted';
	$scope.changeSourceFrom = function (from) {
		;
		if (from === 'Posted') {
			$scope.productNew.Type = 'Posted';

		}
		if (from === 'NonPosted') {
			$scope.productNew.Type = 'NonPosted';


		}
	};

	$scope.GriddataMaterialLedger = [];
	$scope.getaldataMaterialLedger = function () {
		;
		$http({
			method: 'POST',
			//url: $scope.getSearchListUrl,
			url: 'Outsourcing/OSissueRegister/GetMaterialLedger',
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


	//$scope.PurchaseRegisterLst = [];
	//$scope.GetPurchaseRegister = function () {
	//	;
	//	if ($scope.report.FromDate === null || $scope.report.FromDate === "") {
	//		ShowResult('Select From Date', 'failure');
	//		return false;
	//	}
	//	else if ($scope.report.ToDate === null || $scope.report.ToDate === "") {
	//		ShowResult('Select To Date', 'failure');
	//		return false;
	//	}
	//	$http({
	//		method: 'POST',
	//		//url: $scope.getSearchListUrl,
	//		url: 'Materials/MaterialLedger/GetPurchaseRegister',
	//		data: {
	//			fromDate: $scope.report.FromDate,
	//			toDate: $scope.report.ToDate,
	//			Type: $scope.productNew.Type 
	//		},
	//		dataType: 'JSON'
	//	}).then(function successCallback(response) {
	//		$scope.PurchaseRegisterLst = response.data;

	//		//entrydata = copy(searchdata);
	//	});
	//   };



	$scope.IssueRegisterList = [];
	$scope.GetIssueRegister = function () {
		;
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
			url: 'Outsourcing/OSissueRegister/GetOSIssueRegister',
			data: {
				fromDate: $scope.report.FromDate,
				toDate: $scope.report.ToDate,
				Type: $scope.productNew.Type
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			$scope.IssueRegisterList = response.data;

			//entrydata = copy(searchdata);
		});

	};


	$scope.IssueRegisterListByGRN = [];
	$scope.GetIssueRegisterListByGRN = function () {
		;
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
			url: 'Outsourcing/OSissueRegister/GetOSIssueRegisterBYGRN',
			data: {
				fromDate: $scope.report.FromDate,
				toDate: $scope.report.ToDate,
				Type: $scope.productNew.Type
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			$scope.IssueRegisterListByGRN = response.data;

			//entrydata = copy(searchdata);
		});
	};










	$scope.getReport = function () {
		$scope.getaldataMaterialLedger();
	}
	$scope.getPurchaseRegisterReport = function () {
		$scope.GetPurchaseRegister();
	}
	$scope.getPurchaseRegisterReport1 = function () {
		//$scope.GetPurchaseRegister();
		$scope.ShowResultCustom("Coming Soon...");
	}
	//$scope.getalldata1 = function () {
	//    $http({
	//        method: "GET",
	//        dataType: 'JSON',
	//        //url: $scope.getSearchListUrl,
	//        url: 'Products/PurchaseOrder/GetListForPOApproval',
	//    }).then(function successCallback(response) {
	//        $scope.Griddata1 = response.data;
	//        //entrydata = copy(searchdata);
	//    });
	//};


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

		;
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
		;
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
		;
		if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
			ShowResult('Select From Date', 'failure');
			return false;
		}
		if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
			ShowResult('Select To Date', 'failure');
			return false;
		}
		//$scope.MaterialConsumptionReportPdf = function (id, reportFormat) {
			;
			var reportFormat = "Pdf";
			//if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
		$window.open('Outsourcing/OSissueRegister/Report?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Type=' + $scope.productNew.Type, '_blank');
		//};

	}
	//$scope.IssueRegisterReportExcel = function (id, reportFormat) {
	//	;
	//	if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
	//		ShowResult('Select From Date', 'failure');
	//		return false;
	//	}
	//	if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
	//		ShowResult('Select To Date', 'failure');
	//		return false;
	//	}
	//	var reportFormat = "Excel";
	//	//if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
	//	$window.open('Materials/IssueRegister/Report?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Type=' + $scope.productNew.Type, '_blank');
	//};


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
			var file_src = 'Outsourcing/OSissueRegister/Report?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Type=' + $scope.productNew.Type;
			$rootScope.report(file_src);

		} catch (e) {

		}
	}




	$scope.GRNIssueRegisterReportPdf = function (id, reportFormat) {
		;
		if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
			ShowResult('Select From Date', 'failure');
			return false;
		}
		if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
			ShowResult('Select To Date', 'failure');
			return false;
		}
		var reportFormat = "Pdf";
		//if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
		$window.open('Outsourcing/OSissueRegister/CreateOSIssueRegisterGRNIssueReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Type=' + $scope.productNew.Type, '_blank');
	};
	//$scope.GRNIssueRegisterReportExcel = function (id, reportFormat) {
		
	//	if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
	//		ShowResult('Select From Date', 'failure');
	//		return false;
	//	}
	//	if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
	//		ShowResult('Select To Date', 'failure');
	//		return false;
	//	}
	//	var reportFormat = "Excel";
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
			var file_src = 'Outsourcing/OSissueRegister/CreateOSIssueRegisterGRNIssueReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Type=' + $scope.productNew.Type;
			$rootScope.report(file_src);

		} catch (e) {

		}
	}


}