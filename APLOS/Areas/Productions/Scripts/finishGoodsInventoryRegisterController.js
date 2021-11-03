'use strict';
finishGoodsInventoryRegisterController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService', '$window', '$controller'];
function finishGoodsInventoryRegisterController(fileReader, commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService, $window, $controller) {
	$rootScope.title = "FG Inventory Register / Report";
	$scope.Action = 'Save';
	$scope.index = -1;
	$scope.products = [];
    //$scope.path = 'Productions/MaterialLedger/';
    $scope.path = 'Productions/FinishGoodsBooking/';
	$scope.path1 = 'Accounts/InventoryPayable/';
	//$scope.exportgriddataUrl = 'GridReports/ExcelExport';
	$scope.exportgriddataUrl = 'GridReports/ExcelExportJson';

    $scope.downloadgriddataUrl = 'GridReports/Download';
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
	$scope.RowColor = "";
	$scope.isAlternative = -1;
	$scope.rowDataBound = function rowDataBound(e) {

		if ($scope.RowColor != e.data.Id ) {
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

	
	//$scope.PrintPurchaseRegister1 = function () {
		
	//	//var gridObj11 = $("#PivotGrid1").data("ejGrid");
	//	var gridObj111 = $('#PivotGrid1').data("ejPivotGrid");
	//	var data111 = gridObj111.model.dataSource.data;

	//	$http({

	//		method: "POST",
	//		url: $scope.exportgriddataUrl,
	//		data: JSON.stringify(data111)

	//	}).then(function successCallback(response) {
	//		if (response.data.Error == true) {
	//			// ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');

	//		}
	//		else {

	//			location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
	//		}
	//	});

	//}

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
        Type: null,
        WithStock: true,
        WithoutStock: false
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


	$scope.PurchaseRegisterLst = [];
	$scope.pivotTableFieldListID = [];
	$scope.GetPurchaseRegister = function () {
		debugger;
		
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
            url: 'Productions/FinishGoodsBooking/GetPurchaseRegister',
			data: {
				fromDate: $scope.report.FromDate,
				toDate: $scope.report.ToDate,
				Type: $scope.productNew.Type
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			$scope.PurchaseRegisterLst = response.data;

			for (var i = 0; i < $scope.PurchaseRegisterLst.length; i++) {
				response.data[i].GRNEntryDate = new Date($scope.PurchaseRegisterLst[i].GRNEntryDate);
			}

			$scope.load();
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
				var scrollerwidth = $("#PR").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 300 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}

	};


	$scope.onClickReportDownloadWord = function (args) {
		
		var gridObj = $("#GridPrint").data("ejGrid");
		//getting corresponding record 
		var data = gridObj.getSelectedRecords()[0];
		var reportFormat = "Pdf";
		var IsTaxApplicable = false;
		if (baseService.isUndefinedOrNull(data.GRNId)) return ShowResult('No Id found', 'failure');
		$window.open('Accounts/InventoryPayable/PabyableJournal?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.GRNId + '&employeeId=' + data.EmployeeId + '&isReversCharge=' + IsTaxApplicable, '_blank');
		//location.href = "Accounts/InventoryPayable/PabyableJournal?inventoryReceiveId=" + data.GrnId;
	};

	$scope.commandPDF = [{
		type: "details", buttonOptions: {
			text: "PDF",
			width: "50",
			height: "20",
			//contentType: "imageonly",
			//prefixIcon: "e-icon e-dataexport",

			//prefixIcon: "e-icon e-edit" ,
			//prefixIcon: "e-icon e-delete",
			//prefixIcon: " e-icon e-save",
			//prefixIcon: " e-icon e-cancel",

			click: $scope.onClickReportDownloadWord
		}
	}];

	$scope.onClickReportDownloadExcel = function (args) {
		
		var gridObj = $("#GridPrint").data("ejGrid");
		//getting corresponding record 
		var data = gridObj.getSelectedRecords()[0];
		var reportFormat = "Excel";
		var IsTaxApplicable = false;
		if (baseService.isUndefinedOrNull(data.GRNId)) return ShowResult('No Id found', 'failure');
        $window.open('Accounts/InventoryPayable/FGInventoryJournal?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.GRNId + '&employeeId=' + data.EmployeeId + '&isReversCharge=' + IsTaxApplicable, '_blank');

	};
	$scope.commandExcel = [{
		type: "details", buttonOptions: {
			text: "Excel",
			width: "50",
			height: "20",
			//contentType: "imageonly",
			//prefixIcon: "e-icon e-dataexport",

			//prefixIcon: "e-icon e-edit" ,
			//prefixIcon: "e-icon e-delete",
			//prefixIcon: " e-icon e-save",
			//prefixIcon: " e-icon e-cancel",

			click: $scope.onClickReportDownloadExcel
		}
	}];


	$scope.onClickGRNID = function (args) {
		

		var gridObj = $("#GridPrint").data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		//alert('jj' + data.Id);
		// $scope.valuePassInDelModal(data); 
		location.href = "GoodsReceiveNote/GRNReport?grnId=" + data.GRNId;

	};
	$scope.commandGRN = [{


		type: "details", buttonOptions: {
			text: "GRN",
			width: "50",
			height: "20",

			click: $scope.onClickGRNID
		}
	}];


	$scope.load = function () {
		

		$("#PivotGrid1").ejPivotGrid({
			enableGroupingBar: true,
			enableConditionalFormatting: true,
			enableColumnResizing: true,
			resizeColumnsToFit: true,
			beforeExport: "Exporting",
			//beforeExport: Export,
			enableContextMenu: true,
			dataSource: {
				data: $scope.PurchaseRegisterLst,
				//rows: [{
				//	fieldName: "Country",
				//	fieldCaption: "Country"
				//}, {
				//	fieldName: "State",
				//	fieldCaption: "State"
				//}],
				//columns: [{
				//	//fieldName: "Product",
				//	//fieldCaption: "Product"
				//	showSubTotal: false
				//}],
				//values: [
				//	{
				//		fieldName: "TransactionQty",
				//		fieldCaption: "Quantity"
				//	},
				//	{
				//		fieldName: "MaterialTranRate",
				//		fieldCaption: "MaterialTranRate"
				//	},
				//	{
				//		fieldName: "MaterialTranAmount",
				//		fieldCaption: "MaterialTranAmount"
				//	},
				//	{
				//		fieldName: "TotalMaterialTranAmount",
				//		fieldCaption: "TotalMaterialTranAmount"
				//	},
				//	{
				//		fieldName: "TotalMaterialBaseAmount",
				//		fieldCaption: "TotalMaterialBaseAmount"
				//	}],
				//filters: [
				//	{
				//		fieldName: "GRNEntryDate",
				//		fieldCaption: "GRNEntryDate"

				//	},
				//	{
				//		fieldName: "GRNType",
				//		fieldCaption: "GRNType"

				//	},
				//	{
				//		fieldName: "PartyName",
				//		fieldCaption: "PartyName"

				//	},
				//	{
				//		fieldName: "FirstName",
				//		fieldCaption: "FirstName"

				//	},
				//	{
				//		fieldName: "MaterialType",
				//		fieldCaption: "MaterialType"

				//	},




				//	{
				//		fieldName: "MaterialGroupMasterName",
				//		fieldCaption: "MaterialGroupMasterName"

				//	},
				//	{
				//		fieldName: "MaterialMasterName",
				//		fieldCaption: "MaterialMasterName"

				//	},
				//	{
				//		fieldName: "ArticleName",
				//		fieldCaption: "ArticleName"

				//	},
				//	{
				//		fieldName: "FirstCharacteristicsValue",
				//		fieldCaption: "FirstCharacteristicsValue"

				//	},
				//	{
				//		fieldName: "SecondCharacteristicsValue",
				//		fieldCaption: "SecondCharacteristicsValue"

				//	},
				//	{
				//		fieldName: "ThirdCharacteristicsValue",
				//		fieldCaption: "ThirdCharacteristicsValue"

				//	},
				//]
			},
			renderSuccess: RenderFieldList,
		});
		$("#btnExport").ejButton({
			click: "exportBtnClick"
		});
		$("#Button1").ejButton({
			size: "normal",
			roundedCorner: true,
			click: btnClick
		});
		$("#btnExport").ejButton({
			click: exportBtnClick
		});
		//$("#btnExport").ejButton({
		//	click: "exportBtnClick"
		//});
	};
	function PrintPurchaseRegister1() {
		debugger;
		// var gridObj = $("#DetailGrid").data("ejGrid");
		//var gridObj = $("#PivotGrid1").ejGrid("instance");
		//var data = gridObj.model.dataSource;
		var gridObj111 = $('#PivotGrid1').data("ejPivotGrid");
		var data111 = gridObj111.model.dataSource.data;
		$http({
			method: 'POST',
			url: $scope.exportgriddataUrl,
			data: { 'obj': JSON.stringify(data111) }
		}).then(function successCallback(response) {
			if (response.data.Error == true) {
				// ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');

			}
			else {

				location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
			}
		});
	}  
	function exportBtnClick(args) {
		var pGridObj = $('#PivotGrid1').data("ejPivotGrid");
		//JSON export
		pGridObj.exportPivotGrid("https://js.syncfusion.com/ejservices/api/PivotGrid/Olap/ExcelExport", "fileName");
		//PivotEngine Export
		pGridObj.exportPivotGrid(ej.PivotGrid.ExportOptions.Excel);
	}
	function Exporting(args) {
		args.title = "PivotGrid";
		args.description = "Displays both OLAP and Relational datasource in tabular format";
		args.exportWithStyle = true;   // by default it sets as true. It improves performance on exporting huge data when it sets as false.
	}




	function RenderFieldList(args) {

		$("#PivotSchemaDesigner1").ejPivotSchemaDesigner({
			pivotControl: args,
			layout: ej.PivotSchemaDesigner.Layouts.Excel

		});
		//$("#PivotSchemaDesigner1").ejPivotSchemaDesigner({
		//	serviceMethod: { filtering: "FilteringMethod" }
		//});


	}
	function btnClick(e) {
		var pivotGridObj = $('#PivotGrid1').data("ejPivotGrid");
		if (pivotGridObj.model.enableConditionalFormatting) {
			pivotGridObj.openConditionalFormattingDialog();
		}
	}
	function exportBtnClick(args) {
		var pGridObj = $('#PivotGrid1').data("ejPivotGrid");
		pGridObj.exportPivotGrid(ej.PivotGrid.ExportOptions.Excel);
	}
	function Export(args) {
		args.exportMode = ej.PivotGrid.ExportMode.PivotEngine;
	}

	//$scope.load();	


	$scope.MaterialLedgerReportPdf = function (id, reportFormat) {
		
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
		$window.open('Materials/MaterialLedger/Report?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate, '_blank');
	};

    $scope.MaterialLedgerReportExcel = function (reportFormat) {
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
            var file_src = 'Productions/FinishGoodsBooking/Report?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.productNew.Qty + '&Amount=' + $scope.productNew.Amount;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }




	$scope.PurchaseOrderReportPdf = function (id, reportFormat) {
		
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
        $window.open('Productions/FinishGoodsBooking/PurchaseRegisterReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Type=' + $scope.productNew.Type, '_blank');
	};

    

    //$scope.PurchaseOrderReportExcel = function (reportFormat) {
    //    debugger;
    //    if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
    //        ShowResult('Select From Date', 'failure');
    //        return false;
    //    }
    //    if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
    //        ShowResult('Select To Date', 'failure');
    //        return false;
    //    }
    //    try {
    //        var Excel;
    //        var file_src = 'Materials/MaterialLedger/PurchaseRegisterReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Type=' + $scope.productNew.Type;
    //        $rootScope.report(file_src);

    //    } catch (e) {

    //    }
    //}


    //$scope.PurchaseOrderReportExcel = function (reportFormat) {
    //    debugger;
    //    if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
    //        ShowResult('Select From Date', 'failure');
    //        return false;
    //    }
    //    if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
    //        ShowResult('Select To Date', 'failure');
    //        return false;
    //    }
    //    try {
    //        var Excel;
    //        var file_src = 'Materials/MaterialLedger/PurchaseRegisterReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Type=' + $scope.productNew.Type;
    //        $rootScope.report(file_src);

    //    } catch (e) {

    //    }
    //}



    $scope.PurchaseOrderReportExcel = function (reportFormat) {
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
            var file_src = 'Productions/FinishGoodsBooking/PurchaseRegisterReportExcel?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2 + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Asset=' + $scope.productNew.WithStock + '&Inventory=' + $scope.productNew.WithoutStock;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

	$scope.productNew.AsOnDate = 'AsOnDate';
	$scope.tab = 1;
	$scope.setTabPR = function (newTab) {
		$scope.tab = newTab;
		//$scope.GetGRN();
	};
	$scope.isSetPR = function (tabNum) {
		return $scope.tab === tabNum;
		//$scope.GRN = 0;
	};
	$scope.setTabPRP = function (newTab) {
		$scope.tab = newTab;
		//$scope.GetGRN();
	};
	$scope.isSetPRP = function (tabNum) {
		return $scope.tab === tabNum;
		//$scope.GRN = 0;
	};

	
	

	$scope.report.ToDate = $filter("dateFiltering")(Date.now());
    $scope.productNew.Qty = true;
    $scope.productNew.Inventory = true;
	$scope.change = function (e) {
		
		$scope.status = e;
		if ($scope.status === 'ForThePeriod') {	
			var date = new Date(), y = date.getFullYear(), m = date.getMonth();
			var firstDay = new Date(y, m, 1);
			FromDate: $filter('dateFiltering')(new Date(firstDay.getFullYear(), firstDay.getMonth(), 1)),
		    //$scope.report.FromDate = $filter("dateFiltering")(Date.now());
		    $scope.report.FromDate = $filter('dateFiltering')(new Date(firstDay.getFullYear(), firstDay.getMonth(), 1));
			$scope.report.ToDate = $filter("dateFiltering")(Date.now());			
			$scope.productNew.ForThePeriod = 'ForThePeriod';
			//$scope.productNew.Qty = true;
			//$scope.productNew.Amount = false;
			
		}
		if ($scope.status  === 'AsOnDate') {
		
			$scope.productNew.RcptIssue = '';
			$scope.report.FromDate = '';		
			$scope.productNew.AsOnDate = 'AsOnDate';
			//$scope.productNew.Qty = true;
			//$scope.productNew.Amount = false;
			


		}
		
	}



 

	$scope.checkoptions = function (choice) {
		//var details = [];
		//angular.forEach(choice, function (value, key) {
		//	if (choice[key].checked) {
		//		details.push(choice[key].userid);
		//	}
		//});
		if (choice[key].checked) {
			details.push(choice[key].userid);
		}
    }
    $scope.detailModel = {

        MaterialMasterId: null
        , MaterialMasterName: null
        , ArticleId: null
        , ArticleName: null
        , CostCenterId: null
        , CostCenterName:null

        , FirstCharacteristicsId: null
        , FirstCharacteristicsValueId: null

        , SecondCharacteristicsId: null
        , SecondCharacteristicsValueId: null

        , ThirdCharacteristicsId: null
        , ThirdCharacteristicsValueId: null
     

    };
     
    $scope.closeDetaiPopUp = function () {
        $scope.detailModel = {};
        $scope.taxCategoryList = [];
        removeValidationMsg();
        angular.element(document.querySelector('#detailPopUp')).modal('hide');
    };


	$scope.selectMaterialByType = function (ob) {
		debugger;
		var a = ob.IsAsset
		if (a === true) {
			$scope.IsAsset = 'It is  fixed asset';
		}
		else {
			$scope.IsAsset = 'It is inventory';
		}
        $scope.detailModel = {};

        $scope.detailModel.MaterialMasterId = ob.Id;
        $scope.detailModel.MaterialMasterName = ob.UserName;
        $scope.detailModel.BaseUOMId = ob.BaseUOMId;
        $scope.detailModel.BaseUoM = ob.BaseUoM;
        $scope.detailModel.OurStyleName = ob.OurStyleName;
        $scope.detailModel.MaterialGroupMasterName = ob.MaterialGroupMasterName;
        $scope.detailModel.ProductMasterName = ob.ProductMasterName;
        $scope.detailModel.IsOurStyleRequired = ob.IsOurStyleRequired;
        $scope.detailModel.IsProductMstRequired = ob.IsProductMstRequired;
        $scope.detailModel.TransactionUoMId = ob.BaseUOMId;
        $scope.detailModel.ArticleId = null;
        $scope.detailModel.ArticleName = null;
        $scope.detailModel.FirstCharacteristicsValueId = null;
        $scope.detailModel.SecondCharacteristicsValueId = null;
        $scope.detailModel.ThirdCharacteristicsValueId = null;
        $scope.detailModel.IsOriginApplicable = ob.IsOriginApplicable;
        $scope.detailModel.CountryId = null;

        $scope.hasArticle = ob.HasAttribute;
        $scope.hasSku = ob.WithSKU;
        $scope.clearCharNames();
        if (ob.HasAttribute) $scope.getArticleSearchList(ob.Id);
        if (ob.WithSKU) $scope.getCharacteristicsList(ob.Id);

       // getTaxCategoryList(ob.HSNCodeId);
        var mmId = []; mmId.push(ob.Id);

        cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (result) {
            $scope.uoMList = result;
            //$scope.detailModel.BaseUOMId = $filter("filter")($scope.uoMList, { IsBaseUom: 1 })[0].Value;
        });

        manualValidation('div_mm', false);
        manualValidation('div_country', false);
        $scope.closeMaterialMasterbyTypePopUp();
    };
    $scope.selectarticle = function (ob) {
        try {
            $scope.detailModel.ArticleId = ob.Id;
            $scope.detailModel.ArticleName = ob.StandardName;
            manualValidation('div_ar', false);
            angular.element(document.querySelector('#articleSearchPop')).modal('hide');

        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
       
        }
    };

       //-------Material Stock Balance Report--------//

    $scope.MaterialStockBalanceReportPdf = function (id, reportFormat) {
        $scope.productNew.Asset === false;
        $scope.productNew.Inventory === false;
        $scope.productNew.Country === false;
        if ($scope.productNew.AsOnDate === 'AsOnDate') {

            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }



            if ($scope.productNew.Qty) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = '';
            }
            if ($scope.productNew.Amount) {
                $scope.choice1 = '';
                $scope.choice2 = 'Amount';
            }
            if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = 'Amount';
            }
            if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
                ShowResult('Select Qty OR Amount', 'failure');
                return false;
            }

            if (!$scope.productNew.Asset && !$scope.productNew.Inventory) {
                ShowResult('Select Asset OR Inventory', 'failure');
                return false;
            }

            if (($scope.productNew.Asset === true) && ($scope.productNew.Inventory === false || $scope.productNew.Inventory === undefined)) {
                debugger;
                $scope.productNew.Asset = 'Asset';
                $scope.productNew.Inventory = false;
                $scope.productNew.Asset = true;
            }
            if (($scope.productNew.Inventory === true) && ($scope.productNew.Asset === false || $scope.productNew.Asset === undefined)) {
                debugger;
                $scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Asset = false;
                $scope.productNew.Inventory = true;
            }

            if ($scope.productNew.Asset === true && $scope.productNew.Inventory === true) {
                $scope.productNew.Asset = 'Asset';
                $scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
            }

            if ($scope.productNew.Asset === true && $scope.productNew.Country === true) {
                $scope.productNew.Asset = 'Asset';
                $scope.productNew.Inventory = 'Inventory';
                 $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
                $scope.productNew.Country = true;
            }
            if ($scope.productNew.Inventory === true && $scope.productNew.Country === true) {
                $scope.productNew.Asset = 'Asset';
                $scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
                $scope.productNew.Country = true;
            }
            if ($scope.productNew.Inventory === true && $scope.productNew.Asset === true && $scope.productNew.Country === true) {
                $scope.productNew.Asset = 'Asset';
                $scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
                $scope.productNew.Country = true;
            }


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
            if ($scope.productNew.RcptIssue != true) {
            	ShowResult('Select With Receipts & Issue', 'failure');
            	return false;
            }



            if ($scope.productNew.Qty) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = '';
            }
            if ($scope.productNew.Amount) {
                $scope.choice2 = 'Amount';
                $scope.choice1 = '';
            }
            if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = 'Amount';
            }
            if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
                ShowResult('Select Qty OR Amount', 'failure');
                return false;
            }

            if (!$scope.productNew.Asset && !$scope.productNew.Inventory) {
                ShowResult('Select Asset OR Inventory', 'failure');
                return false;
            }
            if (($scope.productNew.Asset === true) && ($scope.productNew.Inventory === false || $scope.productNew.Inventory === undefined)) {
                debugger;
                $scope.productNew.Asset = 'Asset';
                $scope.productNew.Inventory = false;
                $scope.productNew.Asset = true;
            }
            if (($scope.productNew.Inventory === true) && ($scope.productNew.Asset === false || $scope.productNew.Asset === undefined)) {
                debugger;
                $scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Asset = false;
                $scope.productNew.Inventory = true;
            }

            if ($scope.productNew.Asset === true && $scope.productNew.Inventory === true) {
                $scope.productNew.Asset = 'Asset';
                $scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
            }

            if ($scope.productNew.Asset === true && $scope.productNew.Country === true) {
                $scope.productNew.Asset = 'Asset';
                $scope.productNew.Inventory = 'Inventory';
                 $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
                $scope.productNew.Country = true;
            }
            if ($scope.productNew.Inventory === true && $scope.productNew.Country === true) {
                $scope.productNew.Asset = 'Asset';
                $scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
                $scope.productNew.Country = true;
            }
            if ($scope.productNew.Inventory === true && $scope.productNew.Asset === true && $scope.productNew.Country === true) {
                $scope.productNew.Asset = 'Asset';
                $scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
                $scope.productNew.Country = true;
            }


        }


        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Materials/MaterialLedger/MaterialStockBalanceReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2 + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Asset=' + $scope.productNew.Asset + '&Inventory=' + $scope.productNew.Inventory + '&Country=' + $scope.productNew.Country, '_blank');

    };

    $scope.MaterialStockBalanceReportExcel = function (reportFormat) {
        $scope.productNew.Asset === false;
        $scope.productNew.Inventory === false;
        $scope.productNew.Country === false;
        if ($scope.productNew.AsOnDate === 'AsOnDate') {

            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }
            if ($scope.productNew.Qty) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = '';
            }
            if ($scope.productNew.Amount) {
                $scope.choice1 = '';
                $scope.choice2 = 'Amount';
            }
            if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = 'Amount';
            }
            if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
                ShowResult('Select Qty OR Amount', 'failure');
                return false;
            }

            if (!$scope.productNew.Asset && !$scope.productNew.Inventory) {
                ShowResult('Select Asset OR Inventory', 'failure');
                return false;
            }

            if (($scope.productNew.Asset === true) && ($scope.productNew.Inventory === false || $scope.productNew.Inventory === undefined)) {
                debugger;
                //$scope.productNew.Asset = 'Asset';
                $scope.productNew.Inventory = false;
                $scope.productNew.Asset = true;
            }
            if (($scope.productNew.Inventory === true) && ($scope.productNew.Asset === false || $scope.productNew.Asset === undefined)) {
                debugger;
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Asset = false;
                $scope.productNew.Inventory = true;
            }

            if ($scope.productNew.Asset === true && $scope.productNew.Inventory === true) {
                //$scope.productNew.Asset = 'Asset';
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
            }

            if ($scope.productNew.Asset === true && $scope.productNew.Country === true) {
                //$scope.productNew.Asset = 'Asset';
                //$scope.productNew.Inventory = 'Inventory';
                // $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
                $scope.productNew.Country = true;
            }
            if ($scope.productNew.Inventory === true && $scope.productNew.Country === true) {
                //$scope.productNew.Asset = 'Asset';
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Inventory = true;
                //$scope.productNew.Asset = true;
                $scope.productNew.Country = true;
            }
            if ($scope.productNew.Inventory === true && $scope.productNew.Asset === true && $scope.productNew.Country === true) {
                //$scope.productNew.Asset = 'Asset';
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
                $scope.productNew.Country = true;
            }


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
            //if ($scope.productNew.RcptIssue != true) {
            //	ShowResult('Select With Receipts & Issue', 'failure');
            //	return false;
            //}



            if ($scope.productNew.Qty) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = '';
            }
            if ($scope.productNew.Amount) {
                $scope.choice2 = 'Amount';
                $scope.choice1 = '';
            }
            if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = 'Amount';
            }
            if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
                ShowResult('Select Qty OR Amount', 'failure');
                return false;
            }

            if (!$scope.productNew.Asset && !$scope.productNew.Inventory) {
                ShowResult('Select Asset OR Inventory', 'failure');
                return false;
            }
            if (($scope.productNew.Asset === true) && ($scope.productNew.Inventory === false || $scope.productNew.Inventory === undefined)) {
                debugger;
                //$scope.productNew.Asset = 'Asset';
                $scope.productNew.Inventory = false;
                $scope.productNew.Asset = true;
            }
            if (($scope.productNew.Inventory === true) && ($scope.productNew.Asset === false || $scope.productNew.Asset === undefined)) {
                debugger;
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Asset = false;
                $scope.productNew.Inventory = true;
            }

            if ($scope.productNew.Asset === true && $scope.productNew.Inventory === true) {
                //$scope.productNew.Asset = 'Asset';
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
            }

            if ($scope.productNew.Asset === true && $scope.productNew.Country === true) {
                //$scope.productNew.Asset = 'Asset';
                //$scope.productNew.Inventory = 'Inventory';
                // $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
                $scope.productNew.Country = true;
            }
            if ($scope.productNew.Inventory === true && $scope.productNew.Country === true) {
                //$scope.productNew.Asset = 'Asset';
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Inventory = true;
                //$scope.productNew.Asset = true;
                $scope.productNew.Country = true;
            }
            if ($scope.productNew.Inventory === true && $scope.productNew.Asset === true && $scope.productNew.Country === true) {
                //$scope.productNew.Asset = 'Asset';
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
                $scope.productNew.Country = true;
            }


        }


        try {
            var Excel;
            var file_src ='Materials/MaterialLedger/MaterialStockBalanceReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2 + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Asset=' + $scope.productNew.Asset + '&Inventory=' + $scope.productNew.Inventory + '&Country=' + $scope.productNew.Country;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    


     //-------Material Stock Balance Report---- End ----//


  


    //-------Material Store Ledger Report----Start----//

    $scope.MaterialStoreLedgerReportExcel = function (reportFormat) {

        if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
                ShowResult('Select From Date', 'failure');
                return false;
            }
        if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
                ShowResult('Select To Date', 'failure');
                return false;
            }
        //}
        try {
            var Excel;
            var file_src = 'Materials/MaterialLedger/MaterialStoreLedgerReport?reportFormat=' + reportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + "&Qty=" + $scope.productNew.Qty + "&Amount=" + $scope.productNew.Amount + "&RcptIssue=" + $scope.productNew.RcptIssue + "&MaterialId=" + $scope.detailModel.MaterialMasterId + "&ArticleId=" + $scope.detailModel.ArticleId;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    $scope.MaterialStoreLedgerReportPdf = function (reportFormat) {

        if ($scope.productNew.AsOnDate === 'AsOnDate') {

            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }

            if ($scope.detailModel.MaterialMasterName === "" || $scope.detailModel.MaterialMasterName === null || $scope.detailModel.MaterialMasterName === undefined) {
                ShowResult('Please Select Material ', 'failure');
                return false;
            }

            if ($scope.productNew.Qty) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = '';
            }
            if ($scope.productNew.Amount) {
                $scope.choice1 = '';
                $scope.choice2 = 'Amount';
            }
            if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = 'Amount';
            }
            if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
                ShowResult('Select Qty OR Amount', 'failure');
                return false;
            }


            if ($scope.productNew.Asset) {
                $scope.choice1 = 'Asset';
                $scope.choice2 = '';
            }
            if ($scope.productNew.Inventory) {
                $scope.choice2 = 'Inventory';
                $scope.choice1 = '';
            }

            if ($scope.productNew.Asset === true && $scope.productNew.Inventory === true) {
                $scope.choice1 = 'Asset';
                $scope.choice2 = 'Inventory';
            }
            if (!$scope.productNew.Asset && !$scope.productNew.Inventory) {
                ShowResult('Select Asset OR Amount', 'failure');
                return false;
            }

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
            if ($scope.productNew.RcptIssue != true) {
                ShowResult('Select With Receipts & Issue', 'failure');
                return false;
            }
            if ($scope.productNew.Qty) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = '';
            }
            if ($scope.productNew.Amount) {
                $scope.choice2 = 'Amount';
                $scope.choice1 = '';
            }
            if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = 'Amount';
            }
            if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
                ShowResult('Select Qty OR Amount', 'failure');
                return false;
            }
        }


        var reportFormat = "Pdf";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Materials/MaterialLedger/MaterialStoreLedgerReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.productNew.Qty + '&Amount=' + $scope.productNew.Amount + '&RcptIssue=' + $scope.productNew.RcptIssue + '&MaterialId=' + $scope.detailModel.MaterialMasterId + '&ArticleId=' + $scope.detailModel.ArticleId, '_blank');

    };

     //-------Material Store Ledger Report---- End ----//



    //-------Material Consumption Report--------//

    $scope.MaterialConsumptionReportPdf = function (id, reportFormat) {
        
        $scope.detailModel.CostCenterId = null;
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
        $window.open('Materials/MaterialLedger/MaterialConsumptionReports?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&RcptIssue=' + $scope.detailModel.CostCenterId, '_blank');
    };
    $scope.MaterialConsumptionReportExcel = function (reportFormat) {
        $scope.detailModel.CostCenterId = null;
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
            var file_src = 'Materials/MaterialLedger/MaterialConsumptionReports?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&RcptIssue=' + $scope.detailModel.CostCenterId;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }


    $scope.CostCenterLoad = function () {
        
        $scope.costCenterList = [];
        cboService.getCostCenterCbo(function (result) {
            $scope.costCenterList = result;
        });
    }
    $scope.CostCenterLoad();

    $scope.MaterialConsumptionCCReportPdf = function (id, reportFormat) {
        
        
        if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
           
        }

        if ($scope.detailModel.CostCenterId === "" || $scope.detailModel.CostCenterId === null || $scope.detailModel.CostCenterId === undefined) {
            ShowResult('Select Cost Center', 'failure');
            return false;
        }
        var reportFormat = "Pdf";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Materials/MaterialLedger/MaterialConsumptionReports?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&RcptIssue=' + $scope.detailModel.CostCenterId, '_blank');

    };
    $scope.MaterialConsumptionCCReportExcel = function (reportFormat) {
        if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }


        if ($scope.detailModel.CostCenterId === "" || $scope.detailModel.CostCenterId === null || $scope.detailModel.CostCenterId === undefined) {
            ShowResult('Select Cost Center', 'failure');
            return false;
        }
        try {
            var Excel;
            var file_src = 'Materials/MaterialLedger/MaterialConsumptionReports?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&RcptIssue=' + $scope.detailModel.CostCenterId;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    $scope.tab = 1;
    $scope.setTab1 = function (newTab) {
       
        $scope.tab = newTab;       
        $scope.detailModel.CostCenterId === null;
        $scope.CostCenterLoad();
        //$scope.ReqStatus = 'ForChecked';
      //  $scope.GetIssueRegister();

    };
    $scope.isSet1 = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.setTab2 = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet2= function (tabNum) {
        return $scope.tab=== tabNum;

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
            url: 'Materials/MaterialLedger/GetMaterialConsumptionGL',
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
            url: 'Materials/MaterialLedger/GetMaterialConsumptionCostCenter',
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


    //-------Material Consumption Report---- End ----//

    //--------Material Receipt Report---------//


    $scope.MaterialReceiptsReportPdf = function (id, reportFormat) {
		debugger;
		$scope.productNew.Asset === false;
		$scope.productNew.Inventory === false;
		if ($scope.productNew.AsOnDate === 'AsOnDate') {

			if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
				ShowResult('Select To Date', 'failure');
				return false;
			}

			if ($scope.productNew.Qty) {
				$scope.choice1 = 'Qty';
				$scope.choice2 = '';
			}
			if ($scope.productNew.Amount) {
				$scope.choice1 = '';
				$scope.choice2 = 'Amount';
			}
			if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
				$scope.choice1 = 'Qty';
				$scope.choice2 = 'Amount';
			}
			if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
				ShowResult('Select Qty OR Amount', 'failure');
				return false;
			}

            if ($scope.productNew.Qty) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = '';
            }
            if ($scope.productNew.Amount) {
                $scope.choice1 = '';
                $scope.choice2 = 'Amount';
            }
          


            if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = 'Amount';
            }
            if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
                ShowResult('Select Qty OR Amount', 'failure');
                return false;
            }


			if (!$scope.productNew.Asset && !$scope.productNew.Inventory) {
				ShowResult('Select Asset OR Inventory', 'failure');
				return false;
			}


			if (($scope.productNew.Asset === true) && ($scope.productNew.Inventory === false || $scope.productNew.Inventory === undefined)) {
				debugger;
				//$scope.productNew.Asset = 'Asset';
				$scope.productNew.Inventory = false;
				$scope.productNew.Asset = true;
			}
			if (($scope.productNew.Inventory === true) && ($scope.productNew.Asset === false || $scope.productNew.Asset === undefined)) {
				debugger;
				//$scope.productNew.Inventory = 'Inventory';
				$scope.productNew.Asset = false;
				$scope.productNew.Inventory = true;
			}

			if ($scope.productNew.Asset === true && $scope.productNew.Inventory === true) {
				//$scope.productNew.Asset = 'Asset';
				//$scope.productNew.Inventory = 'Inventory';
				$scope.productNew.Inventory = true;
				$scope.productNew.Asset = true;
			}


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
			//if ($scope.productNew.RcptIssue != true) {
			//    ShowResult('Select With Receipts & Issue', 'failure');
			//    return false;
			//}


			if ($scope.productNew.Qty) {
				$scope.choice1 = 'Qty';
				$scope.choice2 = '';
			}
			if ($scope.productNew.Amount) {
				$scope.choice2 = 'Amount';
				$scope.choice1 = '';
			}
			if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
				$scope.choice1 = 'Qty';
				$scope.choice2 = 'Amount';
			}
			if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
				ShowResult('Select Qty OR Amount', 'failure');
				return false;
			}


			if (!$scope.productNew.Asset && !$scope.productNew.Inventory) {
				ShowResult('Select Asset OR Inventory', 'failure');
				return false;
			}


			if ($scope.productNew.Asset === true && $scope.productNew.Inventory === false) {
				debugger;
				//$scope.productNew.Asset = 'Asset';
				$scope.productNew.Inventory = false;
				$scope.productNew.Asset = true;
			}
			if ($scope.productNew.Inventory === true && $scope.productNew.Asset === false) {
				debugger;
				//$scope.productNew.Inventory = 'Inventory';
				$scope.productNew.Asset = false;
				$scope.productNew.Inventory = true;
			}

			if ($scope.productNew.Asset === true && $scope.productNew.Inventory === true) {
				//$scope.productNew.Asset = 'Asset';
				//$scope.productNew.Inventory = 'Inventory';
				$scope.productNew.Inventory = true;
				$scope.productNew.Asset = true;
			}

		}

        var reportFormat = "Pdf";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Materials/MaterialLedger/MaterialReceiptsReports?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.productNew.Qty + '&Amount=' + $scope.productNew.Amount + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Asset=' + $scope.productNew.Asset + '&Inventory=' + $scope.productNew.Inventory, '_blank');

    };

    $scope.MaterialReceiptsReportExcel = function (reportFormat) {
        $scope.productNew.Asset === false;
        $scope.productNew.Inventory === false;
        if ($scope.productNew.AsOnDate === 'AsOnDate') {

            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }
            if ($scope.productNew.Qty) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = '';
            }
            if ($scope.productNew.Amount) {
                $scope.choice1 = '';
                $scope.choice2 = 'Amount';
            }
            if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = 'Amount';
            }
            if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
                ShowResult('Select Qty OR Amount', 'failure');
                return false;
            }
            if (!$scope.productNew.Asset && !$scope.productNew.Inventory) {
                ShowResult('Select Asset OR Inventory', 'failure');
                return false;
            }

            if (($scope.productNew.Asset === true) && ($scope.productNew.Inventory === false || $scope.productNew.Inventory === undefined)) {
                debugger;
                //$scope.productNew.Asset = 'Asset';
                $scope.productNew.Inventory = false;
                $scope.productNew.Asset = true;
            }
            if (($scope.productNew.Inventory === true) && ($scope.productNew.Asset === false || $scope.productNew.Asset === undefined)) {
                debugger;
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Asset = false;
                $scope.productNew.Inventory = true;
            }

            if ($scope.productNew.Asset === true && $scope.productNew.Inventory === true) {
                //$scope.productNew.Asset = 'Asset';
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
            }
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
            if ($scope.productNew.Qty) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = '';
            }
            if ($scope.productNew.Amount) {
                $scope.choice2 = 'Amount';
                $scope.choice1 = '';
            }
            if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = 'Amount';
            }
            if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
                ShowResult('Select Qty OR Amount', 'failure');
                return false;
            }
            if (!$scope.productNew.Asset && !$scope.productNew.Inventory) {
                ShowResult('Select Asset OR Inventory', 'failure');
                return false;
            }
            if ($scope.productNew.Asset === true && $scope.productNew.Inventory === false || $scope.productNew.Inventory === undefined) {
                debugger;
                //$scope.productNew.Asset = 'Asset';
                $scope.productNew.Inventory = false;
                $scope.productNew.Asset = true;
            }
            if ($scope.productNew.Inventory === true && $scope.productNew.Asset === false || $scope.productNew.Asset === undefined) {
                debugger;
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Asset = false;
                $scope.productNew.Inventory = true;
            }

            if ($scope.productNew.Asset === true && $scope.productNew.Inventory === true) {
                $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
            }
        }
        try {
            var Excel;
            var file_src = 'Materials/MaterialLedger/MaterialReceiptsReports?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2 + '&RcptIssue=' + $scope.productNew.RcptIssue +
                '&Asset=' + $scope.productNew.Asset + '&Inventory=' + $scope.productNew.Inventory;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }
    //--------Material Receipt Report---- End ----//

  //#region --------Material Issue Report---------//


    $scope.MaterialIssueReportPdf = function (id, reportFormat) {
        
        if ($scope.productNew.AsOnDate === 'AsOnDate') {

            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }


            if ($scope.productNew.Qty) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = '';
            }
            if ($scope.productNew.Amount) {
                $scope.choice1 = '';
                $scope.choice2 = 'Amount';
            }
            if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = 'Amount';
            }
            if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
                ShowResult('Select Qty OR Amount', 'failure');
                return false;
            }

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
            //if ($scope.productNew.RcptIssue != true) {
            //    ShowResult('Select With Receipts & Issue', 'failure');
            //    return false;
            //}
            if ($scope.productNew.Qty) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = '';
            }
            if ($scope.productNew.Amount) {
                $scope.choice2 = 'Amount';
                $scope.choice1 = '';
            }
            if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = 'Amount';
            }
            if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
                ShowResult('Select Qty OR Amount', 'failure');
                return false;
            }
        }


        var reportFormat = "Pdf";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Materials/MaterialLedger/MaterialIssueReports?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.productNew.Qty + '&Amount=' + $scope.productNew.Amount + '&RcptIssue=' + $scope.productNew.RcptIssue, '_blank');

    };
    $scope.MaterialIssueReportExcel = function (reportFormat) {
        if ($scope.productNew.AsOnDate === 'AsOnDate') {

            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }


            if ($scope.productNew.Qty) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = '';
            }
            if ($scope.productNew.Amount) {
                $scope.choice1 = '';
                $scope.choice2 = 'Amount';
            }
            if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = 'Amount';
            }
            if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
                ShowResult('Select Qty OR Amount', 'failure');
                return false;
            }

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
        
            if ($scope.productNew.Qty) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = '';
            }
            if ($scope.productNew.Amount) {
                $scope.choice2 = 'Amount';
                $scope.choice1 = '';
            }
            if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = 'Amount';
            }
            if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
                ShowResult('Select Qty OR Amount', 'failure');
                return false;
            }
        }
        try {
            var Excel;
            var file_src = 'Materials/MaterialLedger/MaterialIssueReports?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2 + '&RcptIssue=' + $scope.productNew.RcptIssue;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }


 //#endregion --------Material Issue Report---------//


    $scope.PendingGRNList = function (x) {
      
        $scope.getStatusallGRNPendingList(x.Description);
        angular.element(document.querySelector('#ListOfPendingGRNList')).modal('show');
    };


    $scope.PendingGRNListHide = function () {
        $scope.taxCategoryList = [];
        angular.element(document.querySelector('#ListOfPendingGRNList')).modal('hide');
    };

    $scope.GetStatusGRNPendingList = [];
    $scope.getStatusallGRNPendingList = function (status) {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Materials/MaterialLedger/GetStatusAllGRNPendingList?GRNPendingStatus=' + status,
        }).then(function successCallback(response) { //datagatefun
            $scope.GetStatusGRNPendingList = response.data;
            //entrydata = copy(searchdata);
        });
    };
  //  $scope.getStatusallGRNPendingList();

    $scope.GetGRNPendingList = [];
    $scope.getallGRNPendingList = function (status) {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Materials/MaterialLedger/GetPendingListGRN?GRNPendingStatus=' + status,
        }).then(function successCallback(response) { //datagatefun
            $scope.GetGRNPendingList = response.data;
            //entrydata = copy(searchdata);
        });
    };
       $scope.getallGRNPendingList();

    $scope.changeRadio = function (e) {
        
        $scope.status = e;
        if ($scope.status === '1-3') {

            $scope.Lessthan3days = 'Lessthan3days';
            $scope.GRNPendingStatus = 'Lessthan3days';
            $scope.getStatusallGRNPendingList($scope.Lessthan3days);
            $scope.PendingGRNList();

        }
        if ($scope.status === '4-10') {

            $scope.Lessthan10days = 'Lessthan10days';
            $scope.GRNPendingStatus = 'Lessthan10days';
            $scope.getStatusallGRNPendingList($scope.Lessthan10days);
            $scope.PendingGRNList();
        }

        if ($scope.status === '11-20') {

            $scope.Lessthan20days = 'Lessthan20days';
            $scope.GRNPendingStatus = 'Lessthan20days';
            $scope.getStatusallGRNPendingList($scope.Lessthan20days);
            $scope.PendingGRNList();

        }
        if ($scope.status === '21-30') {

            $scope.Lessthan30days = 'Lessthan30days';
            $scope.GRNPendingStatus = 'Lessthan30days';
            $scope.getStatusallGRNPendingList($scope.Lessthan30days);
            $scope.PendingGRNList();
        }
        if ($scope.status === 'More Than 30 Days') {

            $scope.Morethan30days = 'Morethan30days';
            //$scope.GRNPendingStatus = 'Morethan30days';
            $scope.getStatusallGRNPendingList($scope.Morethan30days);
            $scope.PendingGRNList();
        }

    }


    $scope.productNew.WithStock === true;


    $scope.MaterialMasterStatusReportExcel = function (reportFormat) {
        $scope.productNew.WithStock === false;
        $scope.productNew.WithoutStock === false;

        if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        //if ($scope.productNew.RcptIssue != true) {
        //	ShowResult('Select With Receipts & Issue', 'failure');
        //	return false;
        //}



        if ($scope.productNew.Qty) {
            $scope.choice1 = 'Qty';
            $scope.choice2 = '';
        }
        if ($scope.productNew.Amount) {
            $scope.choice2 = 'Amount';
            $scope.choice1 = '';
        }
        if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
            $scope.choice1 = 'Qty';
            $scope.choice2 = 'Amount';
        }
        if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
            ShowResult('Select Qty OR Amount', 'failure');
            return false;
        }

        if ($scope.productNew.WithStock === false && $scope.productNew.WithoutStock === false) {
            ShowResult('Select WithStock2 OR WithoutStock', 'failure');
            return false;
        }
        if (($scope.productNew.WithStock === true) && ($scope.productNew.WithoutStock === false || $scope.productNew.WithoutStock === undefined)) {
            debugger;
            //$scope.productNew.Asset = 'Asset';
            $scope.productNew.WithoutStock = false;
            $scope.productNew.WithStock = true;
        }
        if (($scope.productNew.WithoutStock === true) && ($scope.productNew.WithStock === false || $scope.productNew.WithStock === undefined)) {
            debugger;
            //$scope.productNew.Inventory = 'Inventory';
            $scope.productNew.WithStock = false;
            $scope.productNew.WithoutStock = true;
        }

        if ($scope.productNew.WithStock === true && $scope.productNew.WithoutStock === true) {
            //$scope.productNew.Asset = 'Asset';
            //$scope.productNew.Inventory = 'Inventory';
            $scope.productNew.WithoutStock = true;
            $scope.productNew.WithStock = true;
        }
        try {
            var Excel;
            var file_src = 'Materials/MaterialLedger/MaterialMasterStatus?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2 + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Asset=' + $scope.productNew.WithStock + '&Inventory=' + $scope.productNew.WithoutStock;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    $scope.MaterialMasterStatusReportPdf = function (reportFormat) {
        debugger;
        $scope.productNew.WithStock === false;
        $scope.productNew.WithoutStock === false;

        if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        //if ($scope.productNew.RcptIssue != true) {
        //	ShowResult('Select With Receipts & Issue', 'failure');
        //	return false;
        //}



        if ($scope.productNew.Qty) {
            $scope.choice1 = 'Qty';
            $scope.choice2 = '';
        }
        if ($scope.productNew.Amount) {
            $scope.choice2 = 'Amount';
            $scope.choice1 = '';
        }
        if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
            $scope.choice1 = 'Qty';
            $scope.choice2 = 'Amount';
        }
        if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
            ShowResult('Select Qty OR Amount', 'failure');
            return false;
        }

        if ($scope.productNew.WithStock === false && $scope.productNew.WithoutStock === false) {
            ShowResult('Select WithStock2 OR WithoutStock', 'failure');
            return false;
        }
        if (($scope.productNew.WithStock === true) && ($scope.productNew.WithoutStock === false || $scope.productNew.WithoutStock === undefined)) {
            debugger;
            //$scope.productNew.Asset = 'Asset';
            $scope.productNew.WithoutStock = false;
            $scope.productNew.WithStock = true;
        }
        if (($scope.productNew.WithoutStock === true) && ($scope.productNew.WithStock === false || $scope.productNew.WithStock === undefined)) {
            debugger;
            //$scope.productNew.Inventory = 'Inventory';
            $scope.productNew.WithStock = false;
            $scope.productNew.WithoutStock = true;
        }

        if ($scope.productNew.WithStock === true && $scope.productNew.WithoutStock === true) {
            //$scope.productNew.Asset = 'Asset';
            //$scope.productNew.Inventory = 'Inventory';
            $scope.productNew.WithoutStock = true;
            $scope.productNew.WithStock = true;
        }
        try {
            var Excel;
            var file_src = 'Materials/MaterialLedger/MaterialMasterStatus?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2 + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Asset=' + $scope.productNew.WithStock + '&Inventory=' + $scope.productNew.WithoutStock;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }



  //#endregion -material - master - stock----

    //#region Material Stationery Request

    $scope.MaterialStationeryRequestReportPdf = function (id, reportFormat) {

            if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
                ShowResult('Select From Date', 'failure');
                return false;
            }
            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }
            //if ($scope.productNew.RcptIssue != true) {
            //    ShowResult('Select With Receipts & Issue', 'failure');
            //    return false;
            //}



            if ($scope.productNew.Qty) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = '';
            }
            if ($scope.productNew.Amount) {
                $scope.choice2 = 'Amount';
                $scope.choice1 = '';
            }
            if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = 'Amount';
            }
            if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
                ShowResult('Select Qty OR Amount', 'failure');
                return false;
            }

            if (!$scope.productNew.Asset && !$scope.productNew.Inventory) {
                ShowResult('Select Asset OR Inventory', 'failure');
                return false;
            }
            if (($scope.productNew.Asset === true) && ($scope.productNew.Inventory === false || $scope.productNew.Inventory === undefined)) {
                debugger;
                //$scope.productNew.Asset = 'Asset';
                $scope.productNew.Inventory = false;
                $scope.productNew.Asset = true;
            }
            if (($scope.productNew.Inventory === true) && ($scope.productNew.Asset === false || $scope.productNew.Asset === undefined)) {
                debugger;
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Asset = false;
                $scope.productNew.Inventory = true;
            }

            if ($scope.productNew.Asset === true && $scope.productNew.Inventory === true) {
                //$scope.productNew.Asset = 'Asset';
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
            }


       // }


        var reportFormat = "Pdf";
       
        $window.open('Materials/MaterialLedger/MaterialStationeryRequestReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.productNew.Qty + '&Amount=' + $scope.productNew.Amount + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Asset=' + $scope.productNew.Asset + '&Inventory=' + $scope.productNew.Inventory, '_blank');

    };
    $scope.MaterialStationeryRequestReportExcel = function (id, reportFormat) {
        debugger;
        $scope.productNew.Asset === false;
        $scope.productNew.Inventory === false;

            if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
                ShowResult('Select From Date', 'failure');
                return false;
            }
            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }

            if ($scope.productNew.Qty) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = '';
            }
            if ($scope.productNew.Amount) {
                $scope.choice2 = 'Amount';
                $scope.choice1 = '';
            }
            if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = 'Amount';
            }
            if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
                ShowResult('Select Qty OR Amount', 'failure');
                return false;
            }

            if (!$scope.productNew.Asset && !$scope.productNew.Inventory) {
                ShowResult('Select Asset OR Inventory', 'failure');
                return false;
            }
            if (($scope.productNew.Asset === true) && ($scope.productNew.Inventory === false || $scope.productNew.Inventory === undefined)) {
                debugger;
                //$scope.productNew.Asset = 'Asset';
                $scope.productNew.Inventory = false;
                $scope.productNew.Asset = true;
            }
            if (($scope.productNew.Inventory === true) && ($scope.productNew.Asset === false || $scope.productNew.Asset === undefined)) {
                debugger;
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Asset = false;
                $scope.productNew.Inventory = true;
            }

            if ($scope.productNew.Asset === true && $scope.productNew.Inventory === true) {
                //$scope.productNew.Asset = 'Asset';
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
            }





       // }

        var reportFormat = "Excel";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Materials/MaterialLedger/MaterialStationeryRequestReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2 + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Asset=' + $scope.productNew.Asset + '&Inventory=' + $scope.productNew.Inventory, '_blank');
    };


    //#endregion







    //#region Physical Inventory Report

    $scope.PhysicalInventoryReportPdf = function (id, reportFormat) {
      

            if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
                ShowResult('Select From Date', 'failure');
                return false;
            }
            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }
            if ($scope.productNew.Qty) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = '';
            }
            if ($scope.productNew.Amount) {
                $scope.choice2 = 'Amount';
                $scope.choice1 = '';
            }
            if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = 'Amount';
            }
            if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
                ShowResult('Select Qty OR Amount', 'failure');
                return false;
            }

            if (!$scope.productNew.Asset && !$scope.productNew.Inventory) {
                ShowResult('Select Asset OR Inventory', 'failure');
                return false;
            }
            if (($scope.productNew.Asset === true) && ($scope.productNew.Inventory === false || $scope.productNew.Inventory === undefined)) {
                debugger;
                //$scope.productNew.Asset = 'Asset';
                $scope.productNew.Inventory = false;
                $scope.productNew.Asset = true;
            }
            if (($scope.productNew.Inventory === true) && ($scope.productNew.Asset === false || $scope.productNew.Asset === undefined)) {
                debugger;
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Asset = false;
                $scope.productNew.Inventory = true;
            }

            if ($scope.productNew.Asset === true && $scope.productNew.Inventory === true) {
                //$scope.productNew.Asset = 'Asset';
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
            }


        //}


        var reportFormat = "Pdf";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Materials/MaterialLedger/PhysicalInventoryReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.productNew.Qty + '&Amount=' + $scope.productNew.Amount + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Asset=' + $scope.productNew.Asset + '&Inventory=' + $scope.productNew.Inventory, '_blank');

    };
    $scope.PhysicalInventoryReportExcel = function (id, reportFormat) {
        debugger;
        $scope.productNew.Asset === false;
        $scope.productNew.Inventory === false;
        //if ($scope.productNew.AsOnDate === 'AsOnDate') {
        //    ShowResult('Please Select for the Period', 'failure');
        //    return false;
            //if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
            //    ShowResult('Select To Date', 'failure');
            //    return false;
            //}



            //if ($scope.productNew.Qty) {
            //    $scope.choice1 = 'Qty';
            //    $scope.choice2 = '';
            //}
            //if ($scope.productNew.Amount) {
            //    $scope.choice1 = '';
            //    $scope.choice2 = 'Amount';
            //}
            //if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
            //    $scope.choice1 = 'Qty';
            //    $scope.choice2 = 'Amount';
            //}
            //if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
            //    ShowResult('Select Qty OR Amount', 'failure');
            //    return false;
            //}

            //if (!$scope.productNew.Asset && !$scope.productNew.Inventory) {
            //    ShowResult('Select Asset OR Inventory', 'failure');
            //    return false;
            //}

            //if (($scope.productNew.Asset === true) && ($scope.productNew.Inventory === false || $scope.productNew.Inventory === undefined)) {
            //    debugger;
            //    //$scope.productNew.Asset = 'Asset';
            //    $scope.productNew.Inventory = false;
            //    $scope.productNew.Asset = true;
            //}
            //if (($scope.productNew.Inventory === true) && ($scope.productNew.Asset === false || $scope.productNew.Asset === undefined)) {
            //    debugger;
            //    //$scope.productNew.Inventory = 'Inventory';
            //    $scope.productNew.Asset = false;
            //    $scope.productNew.Inventory = true;
            //}

            //if ($scope.productNew.Asset === true && $scope.productNew.Inventory === true) {
            //    //$scope.productNew.Asset = 'Asset';
            //    //$scope.productNew.Inventory = 'Inventory';
            //    $scope.productNew.Inventory = true;
            //    $scope.productNew.Asset = true;
            //}


        //}
        //else {

            if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
                ShowResult('Select From Date', 'failure');
                return false;
            }
            if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
                ShowResult('Select To Date', 'failure');
                return false;
            }
            //if ($scope.productNew.RcptIssue != true) {
            //	ShowResult('Select With Receipts & Issue', 'failure');
            //	return false;
            //}



            if ($scope.productNew.Qty) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = '';
            }
            if ($scope.productNew.Amount) {
                $scope.choice2 = 'Amount';
                $scope.choice1 = '';
            }
            if ($scope.productNew.Qty === true && $scope.productNew.Amount === true) {
                $scope.choice1 = 'Qty';
                $scope.choice2 = 'Amount';
            }
            if (!$scope.productNew.Qty && !$scope.productNew.Amount) {
                ShowResult('Select Qty OR Amount', 'failure');
                return false;
            }

            if (!$scope.productNew.Asset && !$scope.productNew.Inventory) {
                ShowResult('Select Asset OR Inventory', 'failure');
                return false;
            }
            if (($scope.productNew.Asset === true) && ($scope.productNew.Inventory === false || $scope.productNew.Inventory === undefined)) {
                debugger;
                //$scope.productNew.Asset = 'Asset';
                $scope.productNew.Inventory = false;
                $scope.productNew.Asset = true;
            }
            if (($scope.productNew.Inventory === true) && ($scope.productNew.Asset === false || $scope.productNew.Asset === undefined)) {
                debugger;
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Asset = false;
                $scope.productNew.Inventory = true;
            }

            if ($scope.productNew.Asset === true && $scope.productNew.Inventory === true) {
                //$scope.productNew.Asset = 'Asset';
                //$scope.productNew.Inventory = 'Inventory';
                $scope.productNew.Inventory = true;
                $scope.productNew.Asset = true;
            }





       // }

        var reportFormat = "Excel";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Materials/MaterialLedger/PhysicalInventoryReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2 + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Asset=' + $scope.productNew.Asset + '&Inventory=' + $scope.productNew.Inventory, '_blank');
    };

    $scope.tab = 1;
    $scope.setTabMLedger = function (newTab) {
        $scope.tab = newTab;
        // alert('Tab 1');
        //$scope.GRN = 0;
        //$scope.GetGRN();
    };
    $scope.isSetMLedger = function (tabNum) {
        return $scope.tab === tabNum;
       // $scope.GRN = 0;
    };
    $scope.setTabMLedger1 = function (newTab) {
        $scope.tab = newTab;
        // alert('Tab 1');
        //$scope.GRN = 0;
        //$scope.GetGRN();
    };
    $scope.isSetMLedger1 = function (tabNum) {
        return $scope.tab === tabNum;
        // $scope.GRN = 0;
    };


    //#region Material Store Ledger Report ALL

    $scope.MaterialStoreLedgerReportExcelAll = function (reportFormat) {
        
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull($scope.productNew.Asset) && baseService.isUndefinedOrNull($scope.productNew.Inventory))
        {
            ShowResult('Select Asset Or Inventory', 'failure');
            return false;
        }
        
        if (baseService.isUndefinedOrNull($scope.report.FromDate1)) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if (baseService.isUndefinedOrNull($scope.report.ToDate1)) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        
        try {
            
            var file_src = 'Materials/MaterialLedger/MaterialStoreLedgerReportAll?reportFormat=' + reportFormat + "&fromDate=" + $scope.report.FromDate1 + "&toDate=" + $scope.report.ToDate1 + "&Qty=" + $scope.productNew.Qty + "&Amount=" + $scope.productNew.Amount + "&RcptIssue=" + $scope.productNew.RcptIssue + "&MaterialId=" + $scope.detailModel.MaterialMasterId + "&ArticleId=" + $scope.detailModel.ArticleId + "&Asset=" + $scope.productNew.Asset + "&Inventory=" + $scope.productNew.Inventory;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

        $scope.MaterialStoreLedgerReportPdfAll = function (reportFormat) {
        if (baseService.isUndefinedOrNull($scope.productNew.Asset) && baseService.isUndefinedOrNull($scope.productNew.Inventory)) {
            ShowResult('Select Asset Or Inventory', 'failure');
            return false;
        }

        if (baseService.isUndefinedOrNull($scope.report.FromDate1)) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if (baseService.isUndefinedOrNull($scope.report.ToDate1)) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        var reportFormat = "Pdf";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Materials/MaterialLedger/MaterialStoreLedgerReportAll?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate1 + '&toDate=' + $scope.report.ToDate1 + '&Qty=' + $scope.productNew.Qty + '&Amount=' + $scope.productNew.Amount + '&RcptIssue=' + $scope.productNew.RcptIssue + '&MaterialId=' + $scope.detailModel.MaterialMasterId + '&ArticleId=' + $scope.detailModel.ArticleId + "&Asset=" + $scope.productNew.Asset + "&Inventory=" + $scope.productNew.Inventory, '_blank');

    };
    //#endregion



    $scope.GetRawMaterialDetail = function (obj) {

        $scope.getFGInventoryRegisterPopUpData(obj.data.FinishGoodsBookingId)

        angular.element(document.querySelector('#FGInventoryRegisterPopup')).modal('show');
    };

    //$scope.showTrialBalanceDRcumulativePopUp = function (args) {
    //    $scope.toDate = $scope.reportParameters.ToDate

    //    if (args.BankMasterId != null) {
    //        //if (baseService.isUndefinedOrNull(args.BudgetMasterId)) {
    //        //    args.BudgetMasterId = null;
    //        $scope.getTrialBLAllLevelBankMasterLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.BankMasterId, $scope.toDate)
    //        $scope.getTrialBLAllLevelBankMasterHeaderLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.BankMasterId, $scope.toDate)
    //    }
    //    else if (args.CashMasterId != null) {
    //        //if (baseService.isUndefinedOrNull(args.BudgetMasterId)) {
    //        //    args.BudgetMasterId = null;
  
    //        $scope.getTrialBLAllLevelCashMasterLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.CashMasterId, $scope.toDate)
    //        $scope.getTrialBLHeadingAllLevelCashMasterLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.CashMasterId, $scope.toDate)

    //    }
    //    else if (args.PartyId != null) {
    //        //if (baseService.isUndefinedOrNull(args.BudgetMasterId)) {
    //        //    args.BudgetMasterId = null;
    //        //}
 
    //        $scope.getTrialBLAllLevelPartyLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.PartyId, args.PartyPlantId, $scope.toDate)
    //        $scope.getTrialBLAllLevelHeadingPartyLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, args.PartyId, args.PartyPlantId, $scope.toDate)
    //    }
    //    else {
    //        if (baseService.isUndefinedOrNull(args.BudgetMasterId)) {
    //            args.BudgetMasterId = null;
    //        }
    //        if (baseService.isUndefinedOrNull(args.ActivityId)) {
    //            args.ActivityId = null;
    //        }
    //        if (baseService.isUndefinedOrNull(args.Particulars)) {
    //            args.Particulars = null;
    //        }
    //        $scope.getTrialBLAllLevelGeneralLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, $scope.toDate)
    //        $scope.getTrialBLHeadingAllLevelGeneralLedgerPopUpData(args.Particulars, args.AccountCodeId, args.BudgetMasterId, args.ActivityId, $scope.toDate)

    //    }
    //};

    $scope.FGInventoryRegisterPoPUpList = [];
    $scope.getFGInventoryRegisterPopUpData = function (finishGoodsBookingId) {

        $http({
            method: "GET",
            url: "Productions/FinishGoodsBooking/GetFGInventoryRegisterPoPUpListData?finishGoodsBookingId=" + finishGoodsBookingId 
        }).then(function successCallback(response) {
            $scope.FGInventoryRegisterPoPUpList = response.data;
            //$scope.TotalDRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.BankLedgerDetailLevelPoPUpList), "CompanyCurrencyDrAmount") * 100 + Number.EPSILON) / 100;
            //$scope.TotalCRAmount = Math.round($filter("sumByKey")($filter("filter")($scope.BankLedgerDetailLevelPoPUpList), "CompanyCurrencyCrAmount") * 100 + Number.EPSILON) / 100;
            //$scope.BankLedgerClosingBalance = Math.round(($scope.TotalDRAmount - $scope.TotalCRAmount) * 100 + Number.EPSILON) / 100;
            //$scope.DRBalanceType = 'DR'
        });
       // $rootScope.openPopupAngular('TrialBLBankMasterLedgerPopUp');
    };


    $scope.CloseFGInventoryRegister = function () {
        angular.element(document.querySelector('#FGInventoryRegisterPopup')).modal('hide');
    }


}

 

