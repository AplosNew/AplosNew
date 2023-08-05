'use strict';
IssueSlipApprovedByController.$inject = ['addressService', '$window',  'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function IssueSlipApprovedByController(addressService, $window,  cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
	
	$scope.Action = 'Save';

	$rootScope.title = 'Issue Slip';
	$scope.popUpList = [];
	$scope.valueData = '';
	$scope.filedata = '';
	$scope.message = null;
	$scope.imageSrc = null;
	$scope.Action = 'Save';
	$scope.maxDate = new Date().toDateString();
	$scope.exportgriddataUrl = 'GridReports/ExcelExport';
	$scope.downloadgriddataUrl = 'GridReports/Download';
	$scope.path = 'Products/GoodsReceiveNote/';
	$scope.Action1 = 'Save';
	$scope.loadstatus = false;
    $scope.lstIssueDetailData = [];

	$scope.tab = 1;
	$scope.SetTabIssueUnApp = function (newTab) {
		$scope.tab = newTab;
		$scope.IssuAppStatus = 'For Approval';
		$scope.getaldataIssueSlipUnApproved();
		$scope.IssueSlipDetail();

	};
	$scope.isSetIssueUnApp = function (tabNum) {
		return $scope.tab === tabNum;
	};
	$scope.SetTabIssueHR = function (newTab) {
		$scope.tab = newTab;
		$scope.IssuAppStatus = 'HoldReject';
		$scope.getaldataIssueSlipUnApproved();
		$scope.IssueSlipDetail();
	};
	$scope.isSetIssueHR = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.SetTabIssueApp = function (newTab) {
		$scope.tab = newTab;
		$scope.IssuAppStatus = 'Approved';
		$scope.getaldataIssueSlipUnApproved();
		$scope.IssueSlipDetail();
	};
	$scope.isSetIssueApp = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.GridIssueSlipUnApprovedList = [];
	$scope.IssuAppStatus = 'For Approval';
	$scope.getaldataIssueSlipUnApproved = function () {
	
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/GoodsReceiveNote/IssueSlipUnApproved?IssuAppStatus=' + $scope.IssuAppStatus
		}).then(function successCallback(response) {
			$scope.GridIssueSlipUnApprovedList = response.data;
		});
	};
	$scope.getaldataIssueSlipUnApproved();


    $scope.AllTabPrint = function (z) {
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/GoodsReceiveNote/IssueRequestReport?issueId=" + data.Id;

	};

	$scope.PrintMICData = function (data) {
		location.href = 'Materials/MaterialIssueControl/GetMaterialIssueCheckApproveReportPdf?reportFormat=' + 'Pdf' + '&masterId=' + data.data.Id;

	};

	
	$scope.FilterList123 = [];
	$scope.lst = [];
	$scope.IssueSlipDetail = function () {
		$http({
			method: 'GET',
			url: 'Products/GoodsReceiveNote/IssueSlipDetail?slipstatus=' + $scope.IssuAppStatus
		}).then(function successCallback(response) {
			$scope.lst = response.data;
			window.lst = response.data;
		});
	}
	$scope.IssueSlipDetail();



	$scope.data1 = $scope.lst;
	$scope.detailTemp = "#tabGridContents";
	$scope.detailgrid = function detailGridData(e) {
		var filteredData = e.data["Id"];
		var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("IssueRequestMasterId", "equal", parseInt(filteredData), true).take(100));
		e.detailsElement.find("#detailGrid").ejGrid({

			dataSource: data,
			columns: ["EntityName", "CostCenterName", "ExpenceActivityCode", "Activity", "MaterialType", "MaterialMasterGroupName", "MaterialMasterName", "ArticleName", "RequisitionNo", "RequisitionDetailId", "DepartmentName", "AddedBy", "RequestedQty", "RejectedQty"]
		});
		e.detailsElement.find(".tabcontrol").ejTab();
	}

	$scope.requisitionIssueDetailList = [];
	$scope.IssueSlipList = [];

	$scope.GridIssueSlipApprovedList = [];
	$scope.getaldataIssueSlipApproved = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/GoodsReceiveNote/IssueSlipApproved',
		}).then(function successCallback(response) {
			$scope.GridIssueSlipApprovedList = response.data;
		});
	};
	$scope.getaldataIssueSlipApproved();

	$scope.IssueSlipApproved = function () {

		if ($scope.podata.CheckedByStatus === "Select" || baseService.isUndefinedOrNull($scope.podata.CheckedByStatus)) {
			ShowResult("Please Select Approved By Status", 'failure');
			return false;
		}
		$http({
			method: 'POST',
			url: 'Products/GoodsReceiveNote/IssueSlipToApproved',
			data: {
				'PoId': $scope.podata.Id,
				'PoValue': $scope.podata.TotalQty,
				'CheckedStataus': $scope.podata.CheckedByStatus
			},

			dataType: 'JSON'
		}).then(function successCallback(response) {
			if (response.data.Error === true) {
				ShowResult(response.data.Message, 'failure');
			}
			else {
				ShowResult(response.data.Message, 'success');
				$scope.getaldataIssueSlipApproved();
				$scope.getaldataIssueSlipUnApproved();
				$scope.getalldata1();

			}
		}, function errorCallBack(response) {
			ShowResult(response.data.Message, 'failure');
		});
	}

	$scope.approvalAlert = function () {
		//debugger;
		$scope.message = 'Are you sure want to Approve?';
		angular.element(document.querySelector('#poapprovealert')).modal('show');
	};

	$scope.onClickSave = function (z) {
		//debugger;
		var x = "#" + z;
		var gridObj = $(x).data("ejGrid");
		$scope.podata = gridObj.getSelectedRecords()[0];
		$scope.approvalAlert();
	};


	$scope.LoadapprovalStatus = function () {
		cboService.getEnumCbo("enum/GetCheckedStatusCbo", function (result) {
			$scope.approvalStatusList = result;
		});
	}
	$scope.LoadapprovalStatus();

}