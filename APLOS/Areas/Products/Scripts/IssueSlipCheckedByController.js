'use strict';
IssueSlipCheckedByController.$inject = ['addressService', '$window',  'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function IssueSlipCheckedByController(addressService, $window,  cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
	
	$scope.Action = 'Save';

	$rootScope.title = 'Issue Slip CheckBy';
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
	$scope.IUCsetTabIndex1 = function (newTab) {
		$scope.tab = newTab;
		$scope.IssuStatus = 'ForChecked';
		$scope.getaldataIssueSlipUnChecked();
		$scope.IssueSlipDetail();
	};
	$scope.SetTabIssueHR3 = function (newTab) {
		$scope.tab = newTab;
		$scope.IssuStatus = 'HoldReject';
		$scope.getaldataIssueSlipUnChecked();
		$scope.IssueSlipDetail();
	};
	$scope.isSetIssueHR3 = function (tabNum) {
		return $scope.tab === tabNum;
	};
	$scope.ICsetTabIndex2 = function (newTab) {
		$scope.tab = newTab;
		$scope.IssuStatus = 'Checked';
		$scope.getaldataIssueSlipUnChecked();
		$scope.IssueSlipDetail();
	};
	$scope.isICSetIndex2 = function (tabNum) {
		return $scope.tab === tabNum;
	};
	// #region **********IssueSlipChecked **************


	$scope.GriddataISUnCheckedList = [];
	$scope.IssuStatus = 'ForChecked';
	$scope.getaldataIssueSlipUnChecked = function () {

		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/GoodsReceiveNote/IssueSlipUnChecked?IssuStatus=' + $scope.IssuStatus
		}).then(function successCallback(response) {
			$scope.GriddataISUnCheckedList = response.data;
		});
	};
	$scope.getaldataIssueSlipUnChecked();


	$scope.IssuStatus = 'ForChecked';


	$scope.isIUCSetIndex1 = function (tabNum) {
		return $scope.tab === tabNum;
	};




 //#endregion Requisition Tab

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


	$scope.commandpoAuth = [{
		type: "details", buttonOptions: {
			text: "Save",
			width: "100",
			height: "30",
			click: $scope.onClickPOAUTH
		}
	}];

    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/GoodsReceiveNote/IssueRequestReport?issueId=" + data.Id;

    };

	$scope.GriddataISCkedList = [];
	$scope.getaldataIssueSlipChecked = function () {
		//debugger;
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/GoodsReceiveNote/IssueSlipChecked',
		}).then(function successCallback(response) {
			$scope.GriddataISCkedList = response.data;
		});
	};
	$scope.getaldataIssueSlipChecked();



	$scope.IssueSlipChecked = function (data) {
		//debugger; 
		var str = $scope.podata.SystemId;
		var Status = $('#combo-default').val();
		var Id = str;//str.substring(0, str.indexOf('-'));
		if ($scope.podata.AuthorizedBy === "" || $scope.podata.AuthorizedBy === null) {
			ShowResult("Please Select To be Approved By", 'failure');
			return false;
		}

		else if ($scope.podata.CheckedStatus === null || $scope.podata.CheckedStatus === "") {
			ShowResult("Please Select Checked By Status", 'failure');
			return false;
		}
		else if ($scope.podata.CheckedStatus === "ForChecked" || $scope.podata.CheckedStatus === "Select") {
			ShowResult("Please Select Checked By Status", 'failure');
			return false;
		}


		else if ($scope.podata.CheckedStatus === "Hold" || $scope.podata.CheckedStatus === "Reject") {
			if ($scope.podata.CheckedRejectReason === "" || $scope.podata.CheckedRejectReason === null || $scope.podata.CheckedRejectReason === undefined) {
				ShowResult("Enter The Reason", 'failure');
				return false;
			}

		}
		//debugger;
		$http({
			method: 'POST',
			url: 'Products/GoodsReceiveNote/IssueSlipToChecked',
			data: {
				'PoId': $scope.podata.Id,
				'PoValue': $scope.podata.TotalQty,
				'CheckedStataus': $scope.podata.CheckedStatus,
				'AuthorizedBy': $scope.podata.AuthorizedBy,

			},

			dataType: 'JSON'
		}).then(function successCallback(response) {
			if (response.data.Error === true) {
				ShowResult(response.data.Message, 'failure');
			}
			else {
				ShowResult(response.data.Message, 'success');
				$scope.getaldataIssueSlipUnChecked();
				//$scope.getaldataIssueSlipChecked();
				//$scope.getaldataIssueSlipUnChecked();
				$scope.getalldata1();

			}
		}, function errorCallBack(response) {
			ShowResult(response.data.Message, 'failure');
		});
	}

	$scope.FilterList123 = [];
	$scope.lst = [];
	$scope.IssueSlipDetail = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GoodsReceiveNote/IssueSlipDetail?slipstatus=' + $scope.IssuStatus
		}).then(function successCallback(response) {
			$scope.lst = response.data;
			window.lst = response.data;
		});
	}
	$scope.IssueSlipDetail();



	$scope.data1 = $scope.lst;
	$scope.detailTemp = "#tabGridContents";
	//$scope.detailgrid = "detailGridData(e)";
	$scope.detailgrid = function detailGridData(e) {
		//debugger;

		var filteredData = e.data["Id"];
		var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("IssueRequestMasterId", "equal", parseInt(filteredData), true).take(100));
		e.detailsElement.find("#detailGrid").ejGrid({

			dataSource: data,
			columns: ["EntityName", "CostCenterName", "ExpenceActivityCode", "Activity", "MaterialType", "MaterialMasterGroupName", "MaterialMasterName", "ArticleName", "RequisitionNo", "RequisitionDetailId", "DepartmentName", "AddedBy", "RequestedQty", "RejectedQty"]
		});
		e.detailsElement.find(".tabcontrol").ejTab();
	}
	//#endregion
	$scope.requisitionIssueDetailList = [];
	$scope.ApprovedIssueSlipGridDataaList = [];
	$scope.LoadIssueSlipApproveData = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GoodsReceiveNote/ApprovedIssueSlipGridData'
		}).then(function successCallback(response) {
			$scope.ApprovedIssueSlipGridDataaList = response.data;
		});
	}
	$scope.LoadIssueSlipApproveData();

	$scope.IssueSlipAppList = [];
	$scope.IssueSlipApprovedByListFn = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/InventoryCheckApproved/GetIssueSlipApprovedList'
		}).then(function successCallback(response) {
			$scope.IssueSlipAppList = response.data;
		});
	}
	$scope.IssueSlipApprovedByListFn();


	$scope.poApproved = function () {
		cboService.getEnumCbo("enum/GetPOApprovalStatusCbo", function (result) {
			$scope.POApprovalList = result;
		});
	}
	$scope.poApproved();


	$scope.PrintMICData = function (data) {
		location.href = 'Materials/MaterialIssueControl/GetMaterialIssueCheckApproveReportPdf?reportFormat=' + 'Pdf' + '&masterId=' + data.data.Id;

	};
}