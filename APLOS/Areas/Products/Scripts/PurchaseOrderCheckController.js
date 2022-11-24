'use strict';
PurchaseOrderCheckController.$inject = ['accountService', 'addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller', '$location'];
function PurchaseOrderCheckController(accountService, addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $location) {
	$rootScope.title = "Purchase Order Check";
	$scope.Action = 'Save';
	$scope.index = -1;
	$scope.products = [];
	$scope.path = 'Products/PurchaseOrder/';

	//#region all Tab Function
	$scope.tab = 1;
	$scope.setTabpou = function (newTab) {

		$scope.tab = newTab;
		$scope.getPendingList();

	};
	$scope.isSetpou = function (tabNum) {
		return $scope.tab === tabNum;
	};


	$scope.setTabpoHoldR = function (newTab) {
		$scope.tab = newTab;
		$scope.getCheckedHoldReject();

	};
	$scope.isSetpoHoldR = function (tabNum) {
		return $scope.tab === tabNum;
	};


	$scope.setTabpoa = function (newTab) {
		$scope.tab = newTab;
		$scope.getCheckedList();

	};
	$scope.isSetpoa = function (tabNum) {
		return $scope.tab === tabNum;
	};


    //#endregion

    //#region  Grid Data Display Load for Check UI all tab Function
	$scope.Griddata1 = [];
	$scope.getPendingList = function () {

		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/getPendingList',
		}).then(function successCallback(response) {
			$scope.Griddata1 = response.data;
		});
	};
	$scope.getPendingList();




	$scope.GriddataHoldReject = [];
	$scope.getCheckedHoldReject = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/getCheckedHoldReject',
		}).then(function successCallback(response) {
			$scope.GriddataHoldReject = response.data;
		});
	};
	//$scope.getCheckedHoldReject();



	$scope.Griddataapprovpo = [];
	$scope.getCheckedList = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/getCheckedList',
		}).then(function successCallback(response) {
			$scope.Griddataapprovpo = response.data;
		});
	};
	//$scope.getCheckedList();
    //#endregion


	//#region  PO Check UI Detail
	$scope.lst = [];
	$scope.POListDetails = function () {

		$http({
			method: 'GET',
			url: 'Products/PurchaseOrder/GetInventoryMaterialListPoByReqDetail'
		}).then(function successCallback(response) {
			$scope.lst = response.data;
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

    //#region Save ON Click Function for PO Check

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
				$scope.getCheckedList();
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

	$scope.poApp = function () {
		try {

		var str1 = $('#combo-default1').val();
		var str = $scope.podata.SystemId;
		if ($scope.podata.CheckedStatus === "Hold" || $scope.podata.CheckedStatus === "Reject") {

		}
		else {
			if ($scope.podata.AuthorizedBy === "" || $scope.podata.AuthorizedBy === null || $scope.podata.AuthorizedBy === undefined) {
				ShowResult("Please Select To be Approved By", 'failure');
				return false;
			}
		}

		if ($scope.podata.CheckedStatus === null || $scope.podata.CheckedStatus === "") {
			ShowResult("Please Select Checked By Status", 'failure');
			return false;
		}
		else if ($scope.podata.CheckedStatus === "For Checked" || $scope.podata.CheckedStatus === "Select") {
			ShowResult("Please Select Checked By Status", 'failure');
			return false;
		}
		else if ($scope.podata.CheckedStatus === "Hold" || $scope.podata.CheckedStatus === "Reject") {
			if ($scope.podata.CheckedRejectReason === "" || $scope.podata.CheckedRejectReason === null || $scope.podata.CheckedRejectReason === undefined) {
				ShowResult("Enter The Reason", 'failure');
				return false;
			}

		}
		else if ($scope.podata.CheckedStatus === "Approved") {
			ShowResult("Refresh Your Page", 'failure');
			return false;
		}

		var filteredData = $scope.podata.Id;
		var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("POmasterId", "equal", parseInt(filteredData), true).take(100));
		if (data.length == 0) {
			throw "PO Details is reuired.";
		}

		$http({
			method: 'POST',
			url: 'Products/PurchaseOrder/PoApproved',
			data: {
				'PoId': $scope.podata.Id,
				'PoValue': $scope.podata.TotalQty,
				'CheckedStataus': $scope.podata.CheckedStatus,//$('#combo-default').val(),
				'AuthorizedBy': $scope.podata.AuthorizedBy,
				'CheckedRejectReason': $scope.podata.CheckedRejectReason
			},

			dataType: 'JSON'
		}).then(function successCallback(response) {
			if (response.data.Error === true) {
				ShowResult(response.data.Message, 'failure');
			}
			else {
				ShowResult(response.data.Message, 'success');
				$scope.getPendingList(); 
				$scope.getCheckedHoldReject();
				$scope.GetSupervisorCboList1();
				$scope.POApproval();
			}
		}, function errorCallBack(response) {
			ShowResult(response.data.Message, 'failure');
		});
		}
		catch (e) {
			ShowResult(e, 'failure');
		}
	}


    //#endregion

 //#region To be Approved by and Checked status  textbox function

	$scope.checkedByList1 = [];
	$scope.GetSupervisorCboList1 = function () {

		$http({
			method: 'GET',
			url: 'Products/PurchaseOrder/GetSupervisorCboApproved'
		}).then(function successCallback(response) {
			$scope.checkedByList1 = response.data;
		});
	}
	$scope.GetSupervisorCboList1();


	$scope.GetPOApprovalStatusCbo = function () {
		cboService.getEnumCbo("enum/GetPOApprovalStatusCbo", function (result) {
			$scope.POApprovalList = result;
		});
	}
	$scope.GetPOApprovalStatusCbo();//sk1

	  //#endregion

    //#region AllTabPrint Report  Function 



	$scope.AllTabPrint = function (z) {

		var x = "#" + z;
		var gridObj = $(x).data("ejGrid");
		var data = gridObj.getSelectedRecords()[0];
		location.href = "Products/PurchaseOrder/GePurchaseOrderReport?purchaseOrderId=" + data.Id + '&plantId=' + data.PlantId;
		//location.href = "Products/PurchaseOrder/GePurchaseOrderBOQReport?purchaseOrderBOQId=" + data.Id;

	};
    //#endregion



    



    //#region Scroll Function for 

	$window.onresize = function (event) {

		$scope.PurchaseOrderUncheckedScroll1();

	};
	$scope.PurchaseOrderUncheckedScroll1 = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridPO").ejGrid("instance");
				var scrollerwidth = $("#POUnChecked").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 5, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};



	$window.onresize = function (event) {

		$scope.PurchaseOrdercheckedScroll2();

	};
	$scope.PurchaseOrdercheckedScroll2 = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridPO1").ejGrid("instance");
				var scrollerwidth = $("#POChecked").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 5, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};








	$window.onresize = function (event) {

		$scope.PurchaseOrderHoldandRejectScroll();

	};
	$scope.PurchaseOrderHoldandRejectScroll = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridPOHR").ejGrid("instance");
				var scrollerwidth = $("#POHoldReject").width();//Obtain the width of the container

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