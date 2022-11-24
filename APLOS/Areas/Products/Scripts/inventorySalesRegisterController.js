'use strict';
inventorySalesRegisterController.$inject = ['accountService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function inventorySalesRegisterController(accountService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
	$rootScope.title = "Inventory Sales Register";
	$scope.Action = 'Save';
	$scope.index = -1;
	$scope.products = [];
	$scope.CustomerList = [];
	$scope.PostingStockBeyondIssueDateList = [];
	$scope.PostingStockList = [];
	$scope.UnApprovedStockDetailBeyondIssueDateList = [];
	$scope.ApprovedStockBeyondIssueDateList = [];
	$scope.UnApprovedStockList = [];
	$scope.ApprovedStockList = [];

	$scope.tax = {		
		IncludingTax: true
	};
	$scope.productNew = {
		AsOnDate: null,
		Summary:'Summary'
    }
	
	$scope.partyType = "Customer";
	$scope.path1 = 'Products/PurchaseOrder/';
	$scope.path = 'Products/InventoryIssue/';
	$scope.getListUrl = $scope.path + 'GetDataByInventoryIssue';
	$scope.saveUrl = $scope.path + 'InventorySalesCreate';
	$scope.updateUrl = $scope.path + 'edit';
	$scope.deleteUrl = $scope.path + 'DeleteSalesDetail/';
	$scope.sreviceSaveUrl = $scope.path + 'SalesServiceChargesCreate/';
	$scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';

	$scope.currentDate = new Date(Date.now());
	$controller("partyBaseController", { $scope: $scope, $http: $http });
	$controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
	$controller("employeeBaseController", { $scope: $scope, $http: $http });

	$scope.InventorySalesReportExcels = function (reportFormat) {
		var Type = null;
		if ($scope.productNew.AsOnDate === 'AsOnDate') {

			if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
				ShowResult('Select To Date', 'failure');
				return false;
			}
			Type = 'AsOnDate';
		}
		else {

			if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
				ShowResult('Select From Date', 'failure');
				return false;
			}
			if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
				ShowResult('Select To Date', 'failure');
				return false;
			}
			Type = 'ForThePeriod';
		}

		//var reportFormat = "Excel";
		
		$window.open('Products/SalesRegister/InventorySalesReportExcel?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2 + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Summary=' + $scope.productNew.Summary + '&WithTax=' + $scope.tax.IncludingTax + '&Type=' + Type);
	};

	//$scope.InventorySalesRepoReportPdf = function (id, reportFormat) {

	//	if ($scope.productNew.AsOnDate === 'AsOnDate') {

	//		if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
	//			ShowResult('Select To Date', 'failure');
	//			return false;
	//		}

	//	}
	//	else {

	//		if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
	//			ShowResult('Select From Date', 'failure');
	//			return false;
	//		}
	//		if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
	//			ShowResult('Select To Date', 'failure');
	//			return false;
	//		}
	//	}


	//	var reportFormat = "Pdf";
	//	//if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
	//	$window.open('Products/InventoryIssue/InventorySalesReportExcel?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.productNew.Qty + '&Amount=' + $scope.productNew.Amount + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Summery=' + $scope.productNew.Summery + '&WithTax=' + $scope.tax.IncludingTax);

	//};

}