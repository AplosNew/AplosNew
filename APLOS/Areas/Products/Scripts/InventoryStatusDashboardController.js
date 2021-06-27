'use strict';
InventoryStatusDashboardController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster'];
function InventoryStatusDashboardController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster) {
	$scope.BudgetDateSuper = [];
	$scope.ExpenseDetailList = [];
	$scope.expenseType = null;
	$scope.periodType = null;
	$scope.PeriodicBudgetVSExpense = [];
	window.chartColors = {
		red: 'rgba(240, 52, 52, .6)',
		orange: 'rgb(255, 159, 64)',
		yellow: 'rgb(255, 205, 86)',
		green: 'rgba(46, 204, 113,.6)',
		blue: 'rgb(54, 162, 235)',
		purple: 'rgb(153, 102, 255)',
		grey: 'rgb(201, 203, 207)',
		lightBlue: 'rgb(160, 184, 222)',
		lightGreen: 'rgb(139, 245, 137)'
	};
	$scope.chartExpenseList = [];
	$scope.chartRevenueList = [];
	$scope.chartDelayExpenseList = [];
	$scope.chartDelayRevenueList = [];
	var BudgetedBarChart;

	$scope.ExpenseList = [];
	$scope.RevenueList = [];
	$scope.ExpenseListTotal = 0;
	$scope.RevenueListTotal = 0;

	$scope.chartList = [];

	//--------------This code for From date and to data -----------
	var colorNames = Object.keys(window.chartColors);
	var now = new Date();
	$scope.dateRange = {};
	//$scope.dateRange.fromDate = $filter("dateFiltering")(Date.now());
	$scope.dateRange.toDate = $filter("dateFiltering")(Date.now());
	$scope.itemGroupOption = [
		{ value: "0", Name: "PL" },

		{ value: "1", Name: "BS" }
	];
	var dateWiseBudgetedDatasets = {
		label: null,
		backgroundColor: window.chartColors.green,
		borderColor: window.chartColors.green,
		fill: null,
		data: null
	};
	$scope.fiscalYearList = [];

	$scope.expFactDate = {
		factDate: 'postingDate'
	};
	$scope.BaseCurrencyCode = null;

	//--------------This code for Company and plant display in Screen -----------
	$scope.LoadData = function GetEntity() {
		//debugger;
		$scope.ShowLoader = true;
		$.ajax({
			type: "GET",
			contentType: "application/json; charset=utf-8",
			url: 'Products/InventoryDashboard/GetCompanyPlantInformation',
			data: {},
			async: false,
			dataType: "json",
			success: function (data) {
				//Hide loader image & process successful data.
				$scope.ShowLoader = false;
				$("#Grid2").ejGrid({

					dataSource: data, // data must be array of json
					allowPaging: true,
					//allowSorting: true,
					allowFiltering: true,
					isResponsive: true,
					enableResponsiveRow: true,
					allowTextWrap: true,
					textWrapSettings: { wrapMode: "header" },
					cssClass: "filtered",
					filterSettings: {
						filterType: "excel"
					},
					// pageSize: 1,
					allowScrolling: true,
					scrollSettings: { width: "400", height: "2" },
					columns: [
						{ headerText: "Company", field: "Company", width: 30 },
						{ headerText: "Plant", field: "PlantName", width: 30 }


					]//,
					// rowDataBound: "rowDataBound"


				});
				$("#Grid2").children('.e-pager.e-js.e-pager').hide();
				$("#Grid2").children('.e-gridcontent.e-droppable.e-js').hide();
				$("#Grid2").children('.e-gridcontent').hide();
				//$("#Grid2").children('.e-grid .e-headercell {background - color: chocolate;}').add();

				$("#Grid2").children('.e-grid.e-headercell').css('background-color', 'red'); //{background - color: chocolate;}').add();

			}
		});
	}
	$scope.LoadData();
	$scope.budgetedExpenseList = [];

	$scope.budgetedExpenseChartLabel = [];

	var unique_array = [];

	$scope.headerNav = function (x) {
		//debugger;

		$scope.groupName = 'groupName';

		if (x.Sequence !== -2) {
			$scope.setIndexHead(x);
			$scope.GetDetailDrillDownTableJS(x.Id);
		}
		else {
			$scope.setIndexHead(x);
			$http({
				method: 'POST',
				url: 'Products/InventoryDashboard/InventoryDashboardStatusFun',

				data: {

					'factDate': $scope.expFactDate.factDate,
					'fromDate': $scope.dateRange.fromDate,
					'toDate': $scope.dateRange.toDate,
					'groupName': $scope.groupName,
					//'Company': $scope.Company,
					'Companywiseplantdata': $scope.Companywiseplantdata,
					'CompanyId': $scope.CompanyId,
					'PlantId': $scope.PlantId,
					'ValueOrNumber': $scope.dataIsregular

				},
				dataType: 'JSON'
			}).then(function successCallback(response) {
				setList(response.data);
				$scope.index = -1;
				$scope.stIndex = $scope.index - 1;
			});
			
		}
	};

	$scope.exceptionList = {
		entryPeriod: null,
		postingPeriod: null,
		normalPostedAmout: null,
		delayPostedAmount: null
	};
	$scope.listForChart = [];

	function setList(list) {
		$scope.ExpenseList = [];
		$scope.ExpenseList = list;
		$scope.chartLabel = [];
		$scope.chartList = [];
	}
	$scope.groupName = 'groupName';
	$scope.data = false;
	$scope.dataIsregular = false;

	//---This code for Loding grid data---
	$scope.GetInvStatudLoadAll = function () {
		//debugger;
		var obj = $("#Grid2").ejGrid("instance");
		var sd = obj.getFilteredRecords();
		var sd1 = obj.getSelectedRecords
		var value = "";
		
		if (sd.length == 0) {
			sd = obj.model.dataSource;
			$scope.plantvisible = 'visible';
		}
		else {
			var queryString = null;
			var queryStringProcess = null;
		}
		var arr = [];
		var queryString = [];
		var arrqueryStringProcess = [];
		var queryString = "''";
		var queryStringProcess = "''";

		var index = 0;
		for (var i = 0; i < sd.length; i++) {
			var x = sd[i];

			var yEntityName = x["CompanyId"];
			var yProcess = x["PlantId"];


			if (!arr.includes(yEntityName)) {
				queryString += ",'" + yEntityName + "'";				
				arr.push(yEntityName);

			}
			if (!arrqueryStringProcess.includes(yProcess)) {
				queryStringProcess += ",'" + yProcess + "'";
				arrqueryStringProcess.push(yProcess);
			}
		}


		//debugger;
		var currentTotalEmp = 0;
		var proposedTotalEmp = 0;
		var Short = 0;
		var excess = 0;
		var unallocated = 0;

		$http({
			method: 'POST',
			url: 'Products/InventoryDashboard/InventoryDashboardStatusFun',
			data: {
				'factDate': $scope.expFactDate.factDate,
				'fromDate': $scope.dateRange.fromDate,
				'toDate': $scope.dateRange.toDate,
				'groupName': $scope.groupName,
				//'Companywiseplantdata': $scope.Companywiseplantdata,
				'ValueOrNumber': $scope.dataIsregular,
				'queryString': queryString,
				'queryStringProcess': queryStringProcess,
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			setList(response.data);
			//GetExpenseList(response.data);
			//createColList();
			$scope.InventoryStatusList = response.data;
		});
		

	};
	//  $scope.GetCompanyInformation();
	$scope.GetCompanyGroupData = function () {
		debugger;
		$scope.GetInvStatudLoadAll();
	};

	$scope.GetCompanyGroupData();
	$scope.valuenumber = function (data) {
		//debugger;

		$scope.data;
	}

	$scope.dataIsregular = false;
	$scope.Isregular = function (data) {
		debugger;

		$scope.dataIsregular;
	}

	$scope.PoPopUp = function (data) {
		//debugger;
		angular.element(document.querySelector('#DetailModal')).modal('show');
	}
	//---This code for Generate Option---

	$scope.GetDelayList = function () {
		debugger;
		var obj = $("#Grid2").ejGrid("instance");
		var sd = obj.getFilteredRecords();
		var sd1 = obj.getSelectedRecords
		var value = "";
		
		//if (sd.length > 0) {
		//	sd = obj.model.dataSource;
		//	$scope.plantvisible = 'visible';
		//}
		//else {
		//	var queryString = null;
		//	var queryStringProcess = null;
		//}
		var arr = [];
		var queryString = [];
		var arrqueryStringProcess = [];
		var queryString = "''";
		var queryStringProcess = "''";

		var index = 0;
		for (var i = 0; i < sd.length; i++) {
			var x = sd[i];

			var yEntityName = x["CompanyId"];
			var yProcess = x["PlantId"];


			if (!arr.includes(yEntityName)) {
				queryString += ",'" + yEntityName + "'";
				//queryStringForSum += ",'" + yEntityName + "'";

				//queryString1 += ",'" + yEntityName + "'";
				arr.push(yEntityName);

			}
			if (!arrqueryStringProcess.includes(yProcess)) {
				queryStringProcess += ",'" + yProcess + "'";
				arrqueryStringProcess.push(yProcess);
			}
		}

		//debugger;
		var currentTotalEmp = 0;
		var proposedTotalEmp = 0;
		var Short = 0;
		var excess = 0;
		var unallocated = 0;
	
		$scope.InventoryStatusList = [];
		$http({
			method: 'POST',
			url: 'Products/InventoryDashboard/InventoryDashboardStatusFun',
			data: {
				'factDate': $scope.expFactDate.factDate,
				'fromDate': $scope.dateRange.fromDate,
				'toDate': $scope.dateRange.toDate,
				'groupName': $scope.groupName,
				//'Companywiseplantdata': $scope.Companywiseplantdata,
				'ValueOrNumber': $scope.dataIsregular,
				'queryString': queryString,
				'queryStringProcess': queryStringProcess,
				// 'PlantId': data.PlantId
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			$scope.InventoryStatusList = response.data;
			//setList(response.data);
			//createColList();
		});
		//$scope.GetExpenseListGraph();


	};
	$scope.dFunction = function () {
		debugger;
		if ($scope.dateRange.fromDate == null || $scope.dateRange.fromDate == '' || $scope.dateRange.fromDate == undefined) {
			ShowResult('Please select from date', 'failure');
			return false;
		}
		$scope.expensePeriodList = [];
		$scope.ExpenseListTotal = 0;
		$scope.revenuePeriodList = [];
		$scope.RevenueListTotal = 0;
		var obj = $("#Grid2").ejGrid("instance");
		var sd = obj.getFilteredRecords();
		if (sd.length > 0) {
			$scope.GetDelayList();
		}
		else {
			if (sd.length == 0) {
				sd = obj.model.dataSource;
			}
			var arr = [];
			var queryString = [];
			var arrqueryStringProcess = [];


			var value = "";
			var queryString = "''";
			var queryStringProcess = "''";

			var index = 0;
			for (var i = 0; i < sd.length; i++) {
				var x = sd[i];

				var yEntityName = x["CompanyId"];
				var yProcess = x["PlantId"];



				if (!arr.includes(yEntityName)) {
					queryString += ",'" + yEntityName + "'";

					arr.push(yEntityName);

				}
				if (!arrqueryStringProcess.includes(yProcess)) {
					queryStringProcess += ",'" + yProcess + "'";
					arrqueryStringProcess.push(yProcess);
				}
			}
				$http({
					method: 'POST',
					url: 'Products/InventoryDashboard/InventoryDashboardStatusFun',
					data: {
						'factDate': $scope.expFactDate.factDate,
						'fromDate': $scope.dateRange.fromDate,
						'toDate': $scope.dateRange.toDate,
						'groupName': $scope.groupName,
						//'Companywiseplantdata': $scope.Companywiseplantdata,
						'ValueOrNumber': $scope.dataIsregular,
						'queryString': queryString,
						'queryStringProcess': queryStringProcess,
					},
					dataType: 'JSON'
				}).then(function successCallback(response) {
					//$scope.ExpenseList = response.data;
					$scope.InventoryStatusList = response.data;
				});

			
		}
	};




	//////////-------------Coment ----------



	

	$scope.MaterialTypeWiseMaterialList = [];
	$scope.GetMaterialTypeWiseMaterial = function (data) {
		var obj = $("#Grid2").ejGrid("instance");
		var sd = obj.getFilteredRecords();
		var sd1 = obj.getSelectedRecords
		var value = "";

		if (sd.length == 0) {
			sd = obj.model.dataSource;
			$scope.plantvisible = 'visible';
		}
		else {
			var queryString = null;
			var queryStringProcess = null;
		}
		var arr = [];
		var queryString = [];
		var arrqueryStringProcess = [];
		var queryString = "''";
		var queryStringProcess = "''";

		var index = 0;
		for (var i = 0; i < sd.length; i++) {
			var x = sd[i];

			var yEntityName = x["CompanyId"];
			var yProcess = x["PlantId"];


			if (!arr.includes(yEntityName)) {
				queryString += ",'" + yEntityName + "'";
				arr.push(yEntityName);

			}
			if (!arrqueryStringProcess.includes(yProcess)) {
				queryStringProcess += ",'" + yProcess + "'";
				arrqueryStringProcess.push(yProcess);
			}
		}


		//debugger;
		var currentTotalEmp = 0;
		var proposedTotalEmp = 0;
		var Short = 0;
		var excess = 0;
		var unallocated = 0;
		$http({
			method: 'POST',
			url: 'Products/InventoryDashboard/MaterialTypeWiseMaterial',//?MaterialTypeID=' + data.MaterialTypeID,
			data: {
				'factDate': $scope.expFactDate.factDate,
				'fromDate': $scope.dateRange.fromDate,
				'toDate': $scope.dateRange.toDate,
				'groupName': $scope.groupName,
				//'Company': $scope.Company,
				'Companywiseplantdata': $scope.Companywiseplantdata,
				'CompanyId': $scope.CompanyId,
				'PlantId': $scope.PlantId,
				'ValueOrNumber': $scope.dataIsregular,
				'queryString': queryString,
				'queryStringProcess': queryStringProcess,
				'MaterialTypeID': data.MaterialTypeID
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			$scope.MaterialTypeWiseMaterialList = response.data;
			angular.element(document.querySelector('#DetailModal')).modal('show');
		});
		// }
	};




	$scope.MaterialGroupWiseMaterialList = [];
	$scope.GetMaterialGroupWiseMaterial = function (data) {
		var obj = $("#Grid2").ejGrid("instance");
		var sd = obj.getFilteredRecords();
		var sd1 = obj.getSelectedRecords
		var value = "";

		if (sd.length == 0) {
			sd = obj.model.dataSource;
			$scope.plantvisible = 'visible';
		}
		else {
			var queryString = null;
			var queryStringProcess = null;
		}
		var arr = [];
		var queryString = [];
		var arrqueryStringProcess = [];
		var queryString = "''";
		var queryStringProcess = "''";

		var index = 0;
		for (var i = 0; i < sd.length; i++) {
			var x = sd[i];

			var yEntityName = x["CompanyId"];
			var yProcess = x["PlantId"];


			if (!arr.includes(yEntityName)) {
				queryString += ",'" + yEntityName + "'";
				arr.push(yEntityName);

			}
			if (!arrqueryStringProcess.includes(yProcess)) {
				queryStringProcess += ",'" + yProcess + "'";
				arrqueryStringProcess.push(yProcess);
			}
		}

		$http({
			method: 'POST',
			url: 'Products/InventoryDashboard/MaterialGroupWiseMaterial',//?MaterialGroupID=' + data.MaterialGroupID,
			data: {
				'factDate': $scope.expFactDate.factDate,
				'fromDate': $scope.dateRange.fromDate,
				'toDate': $scope.dateRange.toDate,
				'groupName': $scope.groupName,
				//'Company': $scope.Company,
				'Companywiseplantdata': $scope.Companywiseplantdata,
				'CompanyId': $scope.CompanyId,
				'PlantId': $scope.PlantId,
				'ValueOrNumber': $scope.dataIsregular,
				'queryString': queryString,
				'queryStringProcess': queryStringProcess,
				'MaterialGroupID': data.MaterialGroupID
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			$scope.MaterialGroupWiseMaterialList = response.data;
			angular.element(document.querySelector('#MaterialGroupwiseMaterialModal')).modal('show');
		});
		// }
	};


	$scope.MaterialWiseArticleList = [];
	$scope.GetMaterialWiseArticle = function (data) {
		var obj = $("#Grid2").ejGrid("instance");
		var sd = obj.getFilteredRecords();
		var sd1 = obj.getSelectedRecords
		var value = "";

		if (sd.length == 0) {
			sd = obj.model.dataSource;
			$scope.plantvisible = 'visible';
		}
		else {
			var queryString = null;
			var queryStringProcess = null;
		}
		var arr = [];
		var queryString = [];
		var arrqueryStringProcess = [];
		var queryString = "''";
		var queryStringProcess = "''";

		var index = 0;
		for (var i = 0; i < sd.length; i++) {
			var x = sd[i];

			var yEntityName = x["CompanyId"];
			var yProcess = x["PlantId"];


			if (!arr.includes(yEntityName)) {
				queryString += ",'" + yEntityName + "'";
				arr.push(yEntityName);

			}
			if (!arrqueryStringProcess.includes(yProcess)) {
				queryStringProcess += ",'" + yProcess + "'";
				arrqueryStringProcess.push(yProcess);
			}
		}
		$http({
			method: 'POST',
			url: 'Products/InventoryDashboard/MaterialWiseArticle',//?MaterialID=' + data.MaterialID,
			data: {
				'factDate': $scope.expFactDate.factDate,
				'fromDate': $scope.dateRange.fromDate,
				'toDate': $scope.dateRange.toDate,
				'groupName': $scope.groupName,
				//'Company': $scope.Company,
				'Companywiseplantdata': $scope.Companywiseplantdata,
				'CompanyId': $scope.CompanyId,
				'PlantId': $scope.PlantId,
				'ValueOrNumber': $scope.dataIsregular,
				'queryString': queryString,
				'queryStringProcess': queryStringProcess,
				'MaterialID': data.MaterialID
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			$scope.MaterialWiseArticleList = response.data;
			angular.element(document.querySelector('#MaterialWiseArticleModal')).modal('show');
		});
		// }
	};
	

}



