'use strict';
GeneralApprovedController.$inject = ['accountService', 'addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller', '$location'];
function GeneralApprovedController(accountService, addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $location) {
	$rootScope.title = "Approved";
	$scope.Action = 'Save';
	$scope.index = -1;
	$scope.products = [];
	$scope.path = 'Products/PurchaseOrder/';
	

	//#region PO Approval UI
	$scope.POTypeApprovalStatus = 'ForApproval';
	$scope.tab = 1;
	$scope.setTabUnApproved = function (newTab) {

		$scope.tab = newTab;
		$scope.POTypeApprovalStatus = 'ForApproval';
		$scope.getUNApprovalList();
	};
	$scope.isSetUnApproved = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabApprovedHoldReject = function (newTab) {

		$scope.tab = newTab;
		$scope.POTypeApprovalStatus = 'ApproveHoldReject';
		$scope.getUNApprovalList();
	};
	$scope.isSetApprovedHoldReject = function (tabNum) {
		return $scope.tab === tabNum;
	};
	$scope.setTabApproved = function (newTab) {

		$scope.tab = newTab;
		$scope.POTypeApprovalStatus = 'Approved';
		$scope.getApprovedHoldReject();
	};
	$scope.isSetApproved = function (tabNum) {
		return $scope.tab === tabNum;
	};


	//#endregion


	//#region  Grid Data Display Load for Approve UI all tab Function
	$scope.GriddataAUth = [];
	$scope.getUNApprovalList = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Administration/GeneralCheckedApproved/getUNApprovalList?POTypeApprovalStatus=' + $scope.POTypeApprovalStatus,
		}).then(function successCallback(response) {
			$scope.GriddataAUth = response.data;
			//entrydata = copy(searchdata);
		});
	};
	$scope.getUNApprovalList();

	$scope.GriddataAUth1 = [];
	$scope.getApprovedHoldReject = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Administration/GeneralCheckedApproved/GetApprovedList',
		}).then(function successCallback(response) {
			$scope.GriddataAUth1 = response.data;
			//entrydata = copy(searchdata);
		});
	};

	//#endregion


	//#region  PO Approve UI Detail
	$scope.lst = [];
	$scope.POListDetails = function () {

		$http({
			method: 'GET',
			//url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
			url: 'Products/PurchaseOrder/GetInventoryMaterialListPoByReqDetail'
		}).then(function successCallback(response) {
			$scope.lst = response.data;
			//$scope.detailgrid($scope.lst);
			window.lst = response.data;

		});
	}
	$scope.POListDetails();

	$scope.PODocumentMapDataAll = function () {
		//debugger;
		$http({
			method: 'GET',
			//url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
			url: 'Products/PurchaseOrder/PODocumentMapDataAll'
		}).then(function successCallback(response) {
			$scope.lst = response.data;
			//$scope.detailgrid($scope.lst);
			window.Img = response.data;

		});
	}
	$scope.PODocumentMapDataAll();


	$scope.data1 = $scope.lst;
	$scope.detailTemp = "#tabGridContents";
	//$scope.detailgrid = "detailGridData(e)";
	$scope.detailgrid = function detailGridData(e) {


		var filteredData = e.data["Id"];
		var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("POmasterId", "equal", parseInt(filteredData), true).take(100));
		e.detailsElement.find("#detailGrid").ejGrid({

			dataSource: data,
			columns: ["MaterialGroupName", "MaterialName", "Article", "Sku1", "Sku2", "Sku3", "MaterialDetail", "TransactionQty", "TransactionUoMId", "TransactionUoM", "TransactionRate", "CurrencyName", "TotalAmount"]
			//columns: ["MaterialGroupName", "MaterialName", "Article", "Sku1", "Sku2", "Sku3", "MaterialDetail", "TransactionQty", "TransactionUoMId", "TransactionUoM", "TransactionRate", "CurrencyName", "TotalAmount"]
		});
		e.detailsElement.find(".tabcontrol").ejTab();

		var dataImg = ej.DataManager(window.Img).executeLocal(ej.Query().where("POId", "equal", parseInt(filteredData), true).take(1000));
		e.detailsElement.find("#detailGrid1").ejGrid({
			dataSource: dataImg,
			columns: [{ field: "UserFilename", headerText: "UserFilename", width: 100 },
			{ field: "Description", headerText: "Description", width: 100 },
			{ field: "Remarks", headerText: "Remarks", width: 100 }
			]
		});
		e.detailsElement.find(".tabcontrol").ejTab();
	}
	//#endregion

	//#region Save ON Click Function for PO Approve

	$scope.onClickPOA = function (z) {

		var x = "#" + z;
		var gridObj = $(x).data("ejGrid");
		$scope.podata = gridObj.getSelectedRecords()[0];

		$scope.approvalAlert();
	};

	$scope.commandPOUNChecked = [{
		type: "details", buttonOptions: {
			text: "Checked",
			width: "100",
			height: "30",
			click: $scope.onClickPOA
		}
	}];

	$scope.approvalAlert = function () {
		$scope.message = 'Are you sure want to Approve?';
		angular.element(document.querySelector('#poapprovealert')).modal('show');
	};



	$scope.poApp1 = function () {
		$http({
			method: 'POST',
			url: 'Products/PurchaseOrder/PoApproved1',
			data: {
				'PoId': $scope.podata1.Id,
				'PoValue': $scope.podata1.TotalQty

			},

			dataType: 'JSON'
		}).then(function successCallback(response) {
			if (response.data.Error === true) {
				ShowResult(response.data.Message, 'failure');
			}
			else {
				ShowResult(response.data.Message, 'success');
				$scope.Griddataapprovpo1();
				$scope.ClosedPOPUp();
			}
		}, function errorCallBack(response) {
			ShowResult(response.data.Message, 'failure');
		});
	}

	$scope.ClosedPOPUp = function (args) {

		angular.element(document.querySelector('#poapprovalalert1')).modal('hide');
		//$scope.InActiveAlert();
	};


	$scope.poAppAuth = function () {
		
		if ($scope.podata.AuthorizedStatus === "Hold" || $scope.podata.AuthorizedStatus === "Reject") {
			if ($scope.podata.ApproveRejectReason === "" || $scope.podata.ApproveRejectReason === null || $scope.podata.ApproveRejectReason === undefined) {
				ShowResult("Enter The Reason", 'failure');
				return false;
			}
		}

		else if ($scope.podata.AuthorizedStatus === "" || $scope.podata.AuthorizedStatus === null || $scope.podata.AuthorizedStatus === undefined) {
			ShowResult("Please Select Approved Status ", 'failure');
			return false;
		}
		
		else if ($scope.podata.AuthorizedStatus === "For Approval" || $scope.podata.AuthorizedStatus === "Select") {
			ShowResult("Please Select Approval Status", 'failure');
			return false;
		}
		$scope.approvalUrl = '';
		if ($scope.podata.SourceType == 'PO') { $scope.approvalUrl = 'Products/PurchaseOrder/PoApprovedAuth'; }
		else if ($scope.podata.SourceType == 'CashJournal' || $scope.podata.SourceType == 'BankJournal' || $scope.podata.SourceType == 'JournalVoucher' || $scope.podata.SourceType == 'VendorInvoice')
		{
			$scope.approvalUrl = 'Administration/GeneralCheckedApproved/UpdateApprovalStatus';
			$scope.podata.Id = $scope.podata.VoucherId;
		}

		$http({
			method: 'POST',
			url: $scope.approvalUrl,
			data: {
				'PoId': $scope.podata.Id,
				'PoValue': $scope.podata.TotalQty,
				'CheckedStataus': $scope.podata.AuthorizedStatus,
				'ApproveRejectReason': $scope.podata.ApproveRejectReason

			},

			dataType: 'JSON'
		}).then(function successCallback(response) {
			if (response.data.Error === true) {
				ShowResult(response.data.Message, 'failure');
			}
			else {
				ShowResult(response.data.Message, 'success');
				$scope.getUNApprovalList();
				$scope.approval();
			}
		}, function errorCallBack(response) {
			ShowResult(response.data.Message, 'failure');
		});
	}


	//#endregion

	//#region  Approved  status  textbox function in grid

	$scope.approval = function () {
		cboService.getEnumCbo("enum/GetCheckedStatusCbo", function (result) {
			$scope.approvalStatusList = result;
		});
	}
	$scope.approval();


	//#endregion

	//#region AllTabPrint Report  Function 



	$scope.AllTabPrint = function (z) {

		var x = "#" + z;
		var gridObj = $(x).data("ejGrid");
		var data = gridObj.getSelectedRecords()[0];
		if (data.SourceType == 'PO') {
			location.href = "Products/PurchaseOrder/GePurchaseOrderReport?purchaseOrderId=" + data.Id + '&plantId=' + data.PlantId;
		}
		else if (data.SourceType == 'CashJournal') {
			$window.open("Banks/CashReport/GetCashJournalReport?reportFormat=" + 'Pdf' + '&voucherId=' + data.VoucherId, '_blank');
		}
		else if (data.SourceType == 'BankJournal') {
			$window.open("Banks/BankReport/GetBankJournalReport?reportFormat=" + 'Pdf' + '&voucherId=' + data.VoucherId, '_blank');
		}
		else if (data.SourceType == 'JournalVoucher') {
			$window.open("Accounts/Voucher/GetJournalVoucherReport?reportFormat=" + 'Pdf' + '&voucherId=' + data.VoucherId, '_blank');
		}
		else if (data.SourceType == 'VendorInvoice') {
			$window.open("Accounts/Invoice/ReportVendorInvoice?reportFormat=" + 'Pdf' + '&voucherId=' + data.VoucherId, '_blank');
		}
		//location.href = "Products/PurchaseOrder/GePurchaseOrderReportByReq?purchaseOrderId=" + data.Id;

	};
	//#endregion






	//#region Scroll Function for 


	$window.onresize = function (event) {

		$scope.PurchaseOrderUnApprovedScrollbar();

	};
	$scope.PurchaseOrderUnApprovedScrollbar = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridPOAPp").ejGrid("instance");
				var scrollerwidth = $("#POUnApproval").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 5, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};

	$window.onresize = function (event) {

		$scope.PoApprovedHoldandRejectScroll();

	};
	$scope.PoApprovedHoldandRejectScroll = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridPOAHR").ejGrid("instance");
				var scrollerwidth = $("#POApprovHR").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 5, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};


	$window.onresize = function (event) {

		$scope.PurchaseOrderApprovedScrollbar();

	};
	$scope.PurchaseOrderApprovedScrollbar = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridPO1").ejGrid("instance");
				var scrollerwidth = $("#POApproved").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 5, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};
    //#endregion



}