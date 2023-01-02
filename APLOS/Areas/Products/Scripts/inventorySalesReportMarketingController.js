'use strict';
InventorySalesReportMarketingController.$inject = ['accountService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function InventorySalesReportMarketingController(accountService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
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
		
		$window.open('Products/InventorySalesReportMarketing/InventorySalesReportExcel?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2 + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Summary=' + $scope.productNew.Summary + '&WithTax=' + $scope.tax.IncludingTax + '&Type=' + Type + '&partyId=' + $scope.PartyId);
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

	//#region Customer Load
	$scope.Customer = "AllCustomer";
	$scope.PartyId = null;
	$scope.PartyName = null;
	$scope.invoicingPartyPopUp = function () {
		//debugger;
		angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
	};
	$scope.closePartyPopUp = function (x) {
		
		var party = x.data;// $scope.partyList[$scope.partyIndex];
		$scope.PartyName = party.Code + " - " + party.UserName;
		$scope.PartyId = party.Id;
		
		$scope.partyPlantList = [];
		$scope.getCboPartyPlantList(party.Id, function (result) {
			$scope.partyPlantList = result;
			angular.forEach($scope.partyPlantList, function (item, i) {
				if (item.IsDefault) {
					$scope.partyPlantId = item.Value;
					$scope.InvoicingPartyPlantId = item.Value;
					$scope.DeliveryPartyPlantId = item.Value;
					$scope.InvoicingByAddress = item.Address1;
					$scope.DeliveryByAddress = item.Address1;
					$scope.InvoicingState = item.StateName;
					$scope.InvoicingGSTIN = item.GSTIN;
					$scope.DeliveryState = item.StateName;
					$scope.DeliveryGSTIN = item.GSTIN;
					$scope.InvoicingStateId = item.StateId;

				}
			});
		});
		//}
		$scope.hidePartyPopUp();
	};
	$scope.closeInvoicingPartyPopUp = function () {
		//if ($scope.salesMaterialList.length || $scope.chargesList.length) {

		if (!baseService.isUndefinedOrNull($scope.ChangeInvoicingStateId)) {
			if ($scope.PlantStateId == $scope.InvoicingStateId == $scope.ChangeInvoicingStateId)
				angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
			else if ($scope.InvoicingStateId == $scope.ChangeInvoicingStateId)
				angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
			else if ($scope.PlantStateId != $scope.InvoicingStateId && $scope.PlantStateId != $scope.ChangeInvoicingStateId)
				angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
			else
				ShowResult('Change is not allowed', 'failure', 'invoicingPartyPopUp');
		}
		else
			angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
	
	};

	//#endregion

}