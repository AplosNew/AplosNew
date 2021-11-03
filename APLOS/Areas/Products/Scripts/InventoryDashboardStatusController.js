'use strict';
InventoryDashboardStatusController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster'];
function InventoryDashboardStatusController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster) {
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
	$scope.dateRange.fromDate = $filter("dateFiltering")(Date.now());
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
			//$http({
			//    method: 'POST',
			//    url: 'accounts/ExpenseDashboard/ExpenseListLineChart',
			//    data: {
			//        'factDate': $scope.expFactDate.factDate,
			//        'fromDate': $scope.dateRange.fromDate,
			//        'toDate': $scope.dateRange.toDate
			//    },
			//    dataType: 'JSON'
			//}).then(function successCallback(response) {
			//    $scope.expensePeriodList = response.data;
			//    $http({
			//        method: 'POST',
			//        url: 'accounts/ExpenseDashboard/RevenueListLineChart',
			//        data: {
			//            'factDate': $scope.expFactDate.factDate,
			//            'fromDate': $scope.dateRange.fromDate,
			//            'toDate': $scope.dateRange.toDate
			//        },
			//        dataType: 'JSON'
			//    }).then(function successCallback(response) {
			//        $scope.revenuePeriodList = response.data;
			//    });
			//    if ($scope.expensePeriodList.length > 0 || $scope.revenuePeriodList.length > 0) {
			//        setLineList($scope.fiscalYearList, $scope.expensePeriodList, $scope.revenuePeriodList);

			//        //createLineChart();
			//    }
			//});
			//$http({
			//    method: 'POST',
			//    url: 'accounts/ExpenseDashboard/PeriodWiseExpenseBarChart',
			//    data: {
			//        'factDate': $scope.expFactDate.factDate,
			//        'fromDate': $scope.dateRange.fromDate,
			//        'toDate': $scope.dateRange.toDate
			//    },
			//    dataType: 'JSON'
			//}).then(function successCallback(response) {
			//    $scope.BudgetedExpenseList = response.data;

			//    $http({
			//        method: 'POST',
			//        url: 'accounts/ExpenseDashboard/PeriodWiseRevenueBarChart',
			//        data: {
			//            'factDate': $scope.expFactDate.factDate,
			//            'fromDate': $scope.dateRange.fromDate,
			//            'toDate': $scope.dateRange.toDate
			//        },
			//        dataType: 'JSON'
			//    }).then(function successCallback(response) {
			//        $scope.BudgetedRevenueList = response.data;
			//        setPeriodicBudgetList($scope.BudgetedExpenseList, $scope.BudgetedRevenueList);
			//        createPeriodicalBudgetedBarChart();
			//    });
			//});

			//$http({
			//    method: 'POST',
			//    url: 'accounts/ExpenseDashboard/MonthlyExpenseVSBudgetBarChart',
			//    data: {
			//        'factDate': $scope.expFactDate.factDate,
			//        'fromDate': $scope.dateRange.fromDate,
			//        'toDate': $scope.dateRange.toDate
			//    },
			//    dataType: 'JSON'
			//}).then(function successCallback(response) {
			//    $scope.MonthlyExpenseVSBudget = response.data;


			//    $http({
			//        method: 'POST',
			//        url: 'accounts/ExpenseDashboard/MonthlyRevenueVSBudgetBarChart',
			//        data: {
			//            'factDate': $scope.expFactDate.factDate,
			//            'fromDate': $scope.dateRange.fromDate,
			//            'toDate': $scope.dateRange.toDate
			//        },
			//        dataType: 'JSON'
			//    }).then(function successCallback(response) {
			//        $scope.MonthlyRevenueVSBudget = response.data;

			//        setMonthlyBudgetVSExpenseList($scope.MonthlyExpenseVSBudget, $scope.MonthlyRevenueVSBudget);
			//        createLineChart();
			//    });
			//});
			//$http({
			//    method: 'POST',
			//    url: 'accounts/ExpenseDashboard/PeriodExpenseVSBudgetBarChart',
			//    data: {
			//        'factDate': $scope.expFactDate.factDate,
			//        'fromDate': $scope.dateRange.fromDate,
			//        'toDate': $scope.dateRange.toDate
			//    },
			//    dataType: 'JSON'
			//}).then(function successCallback(response) {
			//$scope.MonthlyExpenseVSBudget = response.data;
			//$http({
			//        method: 'POST',
			//        url: 'accounts/ExpenseDashboard/PeriodRevenueVSBudgetBarChart',
			//        data: {
			//            'factDate': $scope.expFactDate.factDate,
			//            'fromDate': $scope.dateRange.fromDate,
			//            'toDate': $scope.dateRange.toDate
			//        },
			//        dataType: 'JSON'
			//    }).then(function successCallback(response) {
			//        $scope.MonthlyRevenueVSBudget = response.data;

			//        setPeriodWiseBudgetVSExpenseList($scope.MonthlyExpenseVSBudget, $scope.MonthlyRevenueVSBudget);
			//        createPeriodBarChart();
			//    });
			// });
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
	$scope.GetExpenseList = function () {
		//debugger;
		var obj = $("#Grid2").ejGrid("instance");
		var sd = obj.getFilteredRecords();
		var sd1 = obj.getSelectedRecords
		var value = "";
		var queryString = "''";
		var queryStringProcess = "''";
		if (sd.length > 0) {
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

		$http({
			method: 'POST',
			url: 'Products/InventoryDashboard/InventoryDashboardStatusFun',
			data: {
				'factDate': $scope.expFactDate.factDate,
				'fromDate': $scope.dateRange.fromDate,
				'toDate': $scope.dateRange.toDate,
				'groupName': $scope.groupName,
				'Companywiseplantdata': $scope.Companywiseplantdata,
				'ValueOrNumber': $scope.dataIsregular
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			setList(response.data);
			//GetExpenseList(response.data);
			//createColList();
			$scope.InventoryStatusList=response.data;
		});
		//$http({
		//    method: 'POST',
		//    url: 'accounts/ExpenseDashboard/ExpenseListLineChart',
		//    data: {
		//        'factDate': $scope.expFactDate.factDate,
		//        'fromDate': $scope.dateRange.fromDate,
		//        'toDate': $scope.dateRange.toDate
		//    },
		//    dataType: 'JSON'
		//}).then(function successCallback(response) {
		//    $scope.expensePeriodList = response.data;
		//    $scope.ExpenseListTotal = $scope.expensePeriodList.reduce(function (sum, expense) {
		//        return sum + expense.Amount;
		//    }, 0);
		//    $http({
		//        method: 'POST',
		//        url: 'accounts/ExpenseDashboard/RevenueListLineChart',
		//        data: {
		//            'factDate': $scope.expFactDate.factDate,
		//            'fromDate': $scope.dateRange.fromDate,
		//            'toDate': $scope.dateRange.toDate
		//        },
		//        dataType: 'JSON'
		//    }).then(function successCallback(response) {
		//        $scope.revenuePeriodList = response.data;
		//        $scope.RevenueListTotal = $scope.revenuePeriodList.reduce(function (sum, expense) {
		//            return sum + expense.Amount;
		//        }, 0);
		//        if ($scope.expensePeriodList.length > 0 || $scope.revenuePeriodList.length > 0) {
		//            setLineList($scope.fiscalYearList, $scope.expensePeriodList, $scope.revenuePeriodList);
		//            //createLineChart();
		//        }
		//    });
		//});

		//$http({
		//    method: 'POST',
		//    url: 'accounts/ExpenseDashboard/PeriodWiseExpenseBarChart',
		//    data: {
		//        'factDate': $scope.expFactDate.factDate,
		//        'fromDate': $scope.dateRange.fromDate,
		//        'toDate': $scope.dateRange.toDate
		//    },
		//    dataType: 'JSON'
		//}).then(function successCallback(response) {
		//    $scope.BudgetedExpenseList = response.data;


		//    $http({
		//        method: 'POST',
		//        url: 'accounts/ExpenseDashboard/PeriodWiseRevenueBarChart',
		//        data: {
		//            'factDate': $scope.expFactDate.factDate,
		//            'fromDate': $scope.dateRange.fromDate,
		//            'toDate': $scope.dateRange.toDate
		//        },
		//        dataType: 'JSON'
		//    }).then(function successCallback(response) {
		//        $scope.BudgetedRevenueList = response.data;

		//        setPeriodicBudgetList($scope.BudgetedExpenseList, $scope.BudgetedRevenueList);
		//        createPeriodicalBudgetedBarChart();
		//    });


		//});


		//$http({
		//    method: 'POST',
		//    url: 'accounts/ExpenseDashboard/MonthlyExpenseVSBudgetBarChart',
		//    data: {
		//        'factDate': $scope.expFactDate.factDate,
		//        'fromDate': $scope.dateRange.fromDate,
		//        'toDate': $scope.dateRange.toDate
		//    },
		//    dataType: 'JSON'
		//}).then(function successCallback(response) {
		//    $scope.MonthlyExpenseVSBudget = response.data;


		//    $http({
		//        method: 'POST',
		//        url: 'accounts/ExpenseDashboard/MonthlyRevenueVSBudgetBarChart',
		//        data: {
		//            'factDate': $scope.expFactDate.factDate,
		//            'fromDate': $scope.dateRange.fromDate,
		//            'toDate': $scope.dateRange.toDate
		//        },
		//        dataType: 'JSON'
		//    }).then(function successCallback(response) {
		//        $scope.MonthlyRevenueVSBudget = response.data;

		//        setMonthlyBudgetVSExpenseList($scope.MonthlyExpenseVSBudget, $scope.MonthlyRevenueVSBudget);
		//        createLineChart();
		//    });
		//});
		//$http({
		//    method: 'POST',
		//    url: 'accounts/ExpenseDashboard/PeriodExpenseVSBudgetBarChart',
		//    data: {
		//        'factDate': $scope.expFactDate.factDate,
		//        'fromDate': $scope.dateRange.fromDate,
		//        'toDate': $scope.dateRange.toDate
		//    },
		//    dataType: 'JSON'
		//}).then(function successCallback(response) {
		//    $scope.MonthlyExpenseVSBudget = response.data;


		//    $http({
		//        method: 'POST',
		//        url: 'accounts/ExpenseDashboard/PeriodRevenueVSBudgetBarChart',
		//        data: {
		//            'factDate': $scope.expFactDate.factDate,
		//            'fromDate': $scope.dateRange.fromDate,
		//            'toDate': $scope.dateRange.toDate
		//        },
		//        dataType: 'JSON'
		//    }).then(function successCallback(response) {
		//        $scope.MonthlyRevenueVSBudget = response.data;

		//        setPeriodWiseBudgetVSExpenseList($scope.MonthlyExpenseVSBudget, $scope.MonthlyRevenueVSBudget);
		//        createPeriodBarChart();
		//    });
		//});

	};
	//  $scope.GetCompanyInformation();
	$scope.GetCompanyGroupData = function () {
		debugger;
		$scope.GetExpenseList();
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
		var queryString = "''";
		var queryStringProcess = "''";
		if (sd.length > 0) {
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
		//$scope.expensePeriodList = [];
		//$scope.revenuePeriodList = [];
		//$scope.ExpenseListTotal = null;
		//$scope.RevenueListTotal = null;
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
					$scope.ExpenseList = response.data;
				});

			}
		}
	};
	



	//////////-------------Coment ----------



	//var now = new Date();
	//$scope.dateRange = {};

	//// var today = new Date();
	//// var date = today.getDate();
	//// var month = today.getMonth();
	//// var year = today.getFullYear()-1;
	////// var BackDate_date = date + '-' + month + '-' + year;
	//// var BackDate_date = $filter("dateFiltering")(new Date(year, month, date));
	//// var date = new Date();
	//// $scope.dateRange.fromDate = BackDate_date;
	//$scope.dateRange.fromDate = $filter("dateFiltering")(Date.now());
	//$scope.dateRange.toDate = $filter("dateFiltering")(Date.now());
	//$scope.itemGroupOption = [
	//	{ value: "0", Name: "PL" },

	//	{ value: "1", Name: "BS" }
	//];
	//var dateWiseBudgetedDatasets = {
	//	label: null,
	//	backgroundColor: window.chartColors.green,
	//	borderColor: window.chartColors.green,
	//	fill: null,
	//	data: null
	//};
	//$scope.fiscalYearList = [];

	//$scope.expFactDate = {
	//	factDate: 'postingDate'
	//};
	//$scope.BaseCurrencyCode = null;
	////GetCompanyInformation

	////---------------Update 5-September-2019----------------



	////--------------End of Update 5-September-2019

	//$scope.LoadData = function GetEntity() {
	//	//debugger;
	//	$scope.ShowLoader = true;
	//	$.ajax({
	//		type: "GET",
	//		contentType: "application/json; charset=utf-8",
	//		url: 'Products/InventoryDashboard/GetCompanyPlantInformation',
	//		data: {},
	//		async: false,
	//		dataType: "json",
	//		success: function (data) {
	//			//Hide loader image & process successful data.
	//			$scope.ShowLoader = false;
	//			$("#Grid2").ejGrid({

	//				dataSource: data, // data must be array of json
	//				allowPaging: true,
	//				//allowSorting: true,
	//				allowFiltering: true,
	//				isResponsive: true,
	//				enableResponsiveRow: true,
	//				allowTextWrap: true,
	//				textWrapSettings: { wrapMode: "header" },
	//				cssClass: "filtered",
	//				filterSettings: {
	//					filterType: "excel"
	//				},
	//				// pageSize: 1,
	//				allowScrolling: true,
	//				scrollSettings: { width: "400", height: "2" },
	//				columns: [
	//					{ headerText: "Company", field: "Company", width: 30 },
	//					{ headerText: "Plant", field: "PlantName", width: 30 }


	//				]//,
	//				// rowDataBound: "rowDataBound"


	//			});
	//			$("#Grid2").children('.e-pager.e-js.e-pager').hide();
	//			$("#Grid2").children('.e-gridcontent.e-droppable.e-js').hide();
	//			$("#Grid2").children('.e-gridcontent').hide();
	//			//$("#Grid2").children('.e-grid .e-headercell {background - color: chocolate;}').add();

	//			$("#Grid2").children('.e-grid.e-headercell').css('background-color', 'red'); //{background - color: chocolate;}').add();

	//		}
	//	});
	//}
	//$scope.LoadData();

	////$scope.dateRange.fromDate = $filter("dateFiltering")(Date.now());
	////$scope.dateRange.toDate = $filter("dateFiltering")(Date.now());





	//$scope.budgetedExpenseList = [];

	//$scope.budgetedExpenseChartLabel = [];

	//var unique_array = [];



	//function getDrillDownList() {
	//	$http({
	//		method: 'POST',
	//		url: 'Products/InventoryDashboard/OrgStructureList',
	//		dataType: 'JSON'
	//	}).then(function successCallback(response) {
	//		if (baseService.arrayLength(response.data) > 0) {
	//			for (var i = 0; i < baseService.arrayLength(response.data); i++) {
	//				var row = {
	//					Sequence: -2,
	//					Id: null,
	//					StandardName: null,
	//					ColumnName: null,
	//					RType: null,
	//					Text: null,
	//					Name: null,
	//					date: ''
	//				};
	//				row.Sequence = i;
	//				row.StandardName = response.data[i].StandardName;
	//				row.ColumnName = response.data[i].ColumnName;
	//				row.RType = response.data[i].RType;
	//				row.Text = response.data[i].UId;
	//				row.date = $scope.date;
	//				$scope.ColList.push(row);
	//			}
	//		}
	//	});
	//}

	//function createColList() {
	//	if (baseService.arrayLength($scope.ExpenseList) >= 0) {
	//		var row = {
	//			Sequence: null,
	//			Id: null,
	//			StandardName: null,
	//			ColumnName: null,
	//			RType: null,
	//			Text: null,
	//			Name: null,
	//			date: ''
	//		};
	//		row.Sequence = -2;
	//		row.Id = $scope.ExpenseList[0].CompanyGroupId;
	//		row.StandardName = "Group";
	//		row.ColumnName = "Group";
	//		row.Text = $scope.ExpenseList[0].GroupName;
	//		row.Name = $scope.ExpenseList[0].GroupName;

	//		row.date = $scope.date;

	//		$scope.ColList.push(row);
	//		var rowc = {
	//			Sequence: null,
	//			Id: null,
	//			StandardName: null,
	//			ColumnName: null,
	//			RType: null,
	//			Text: null,
	//			Name: null,
	//			date: ''
	//		};
	//		rowc.Sequence = -1;
	//		rowc.Id = $scope.ExpenseList[0].CompanyId;
	//		row.StandardName = "Company";
	//		rowc.ColumnName = "Company";
	//		rowc.Text = $scope.ExpenseList[0].UserName;
	//		rowc.Name = $scope.ExpenseList[0].UserName;
	//		rowc.date = $scope.date;

	//		$scope.ColList.push(rowc);
	//		getDrillDownList();
	//	}
	//}

	//$scope.setIndex = function (x) {
	//	for (var i = 0; i < $scope.ColList.length; i++) {
	//		if ($scope.ColList[i].Sequence === $scope.index) {
	//			$scope.ColList[i].Id = x.CompanyId;
	//			$scope.ColList[i].Text = x.UId;
	//			$scope.ColList[i].Name = x.ColumnName;
	//		}
	//	}
	//};
	//// console.log("ColList Length", $scope.ColList[$scope.ColList.length]);

	//function getCol(seq) {
	//	for (var i = 0; i < baseService.arrayLength($scope.ColList); i++) {
	//		if ($scope.ColList[i].Sequence === seq) {
	//			return $scope.ColList[i].ColumnName;
	//		}
	//	}
	//}

	//$scope.setIndexHead = function (x) {
	//	$scope.index = x.Sequence;
	//};






	//$scope.headerNav = function (x) {
	//	//debugger;

	//	$scope.groupName = 'groupName';

	//	if (x.Sequence !== -2) {
	//		$scope.setIndexHead(x);
	//		$scope.GetDetailDrillDownTableJS(x.Id);
	//	}
	//	else {
	//		$scope.setIndexHead(x);
	//		$http({
	//			method: 'POST',
	//			url: 'Products/InventoryDashboard/InventoryDashboardStatus',

	//			data: {

	//				'factDate': $scope.expFactDate.factDate,
	//				'fromDate': $scope.dateRange.fromDate,
	//				'toDate': $scope.dateRange.toDate,
	//				'groupName': $scope.groupName,
	//				//'Company': $scope.Company,
	//				'Companywiseplantdata': $scope.Companywiseplantdata,
	//				'CompanyId': $scope.CompanyId,
	//				'PlantId': $scope.PlantId,
	//				'ValueOrNumber': $scope.dataIsregular

	//			},
	//			dataType: 'JSON'
	//		}).then(function successCallback(response) {
	//			setList(response.data);
	//			$scope.index = -1;
	//			$scope.stIndex = $scope.index - 1;
	//		});
	//		//$http({
	//		//    method: 'POST',
	//		//    url: 'accounts/ExpenseDashboard/ExpenseListLineChart',
	//		//    data: {
	//		//        'factDate': $scope.expFactDate.factDate,
	//		//        'fromDate': $scope.dateRange.fromDate,
	//		//        'toDate': $scope.dateRange.toDate
	//		//    },
	//		//    dataType: 'JSON'
	//		//}).then(function successCallback(response) {
	//		//    $scope.expensePeriodList = response.data;
	//		//    $http({
	//		//        method: 'POST',
	//		//        url: 'accounts/ExpenseDashboard/RevenueListLineChart',
	//		//        data: {
	//		//            'factDate': $scope.expFactDate.factDate,
	//		//            'fromDate': $scope.dateRange.fromDate,
	//		//            'toDate': $scope.dateRange.toDate
	//		//        },
	//		//        dataType: 'JSON'
	//		//    }).then(function successCallback(response) {
	//		//        $scope.revenuePeriodList = response.data;
	//		//    });
	//		//    if ($scope.expensePeriodList.length > 0 || $scope.revenuePeriodList.length > 0) {
	//		//        setLineList($scope.fiscalYearList, $scope.expensePeriodList, $scope.revenuePeriodList);

	//		//        //createLineChart();
	//		//    }
	//		//});
	//		//$http({
	//		//    method: 'POST',
	//		//    url: 'accounts/ExpenseDashboard/PeriodWiseExpenseBarChart',
	//		//    data: {
	//		//        'factDate': $scope.expFactDate.factDate,
	//		//        'fromDate': $scope.dateRange.fromDate,
	//		//        'toDate': $scope.dateRange.toDate
	//		//    },
	//		//    dataType: 'JSON'
	//		//}).then(function successCallback(response) {
	//		//    $scope.BudgetedExpenseList = response.data;

	//		//    $http({
	//		//        method: 'POST',
	//		//        url: 'accounts/ExpenseDashboard/PeriodWiseRevenueBarChart',
	//		//        data: {
	//		//            'factDate': $scope.expFactDate.factDate,
	//		//            'fromDate': $scope.dateRange.fromDate,
	//		//            'toDate': $scope.dateRange.toDate
	//		//        },
	//		//        dataType: 'JSON'
	//		//    }).then(function successCallback(response) {
	//		//        $scope.BudgetedRevenueList = response.data;
	//		//        setPeriodicBudgetList($scope.BudgetedExpenseList, $scope.BudgetedRevenueList);
	//		//        createPeriodicalBudgetedBarChart();
	//		//    });
	//		//});

	//		//$http({
	//		//    method: 'POST',
	//		//    url: 'accounts/ExpenseDashboard/MonthlyExpenseVSBudgetBarChart',
	//		//    data: {
	//		//        'factDate': $scope.expFactDate.factDate,
	//		//        'fromDate': $scope.dateRange.fromDate,
	//		//        'toDate': $scope.dateRange.toDate
	//		//    },
	//		//    dataType: 'JSON'
	//		//}).then(function successCallback(response) {
	//		//    $scope.MonthlyExpenseVSBudget = response.data;


	//		//    $http({
	//		//        method: 'POST',
	//		//        url: 'accounts/ExpenseDashboard/MonthlyRevenueVSBudgetBarChart',
	//		//        data: {
	//		//            'factDate': $scope.expFactDate.factDate,
	//		//            'fromDate': $scope.dateRange.fromDate,
	//		//            'toDate': $scope.dateRange.toDate
	//		//        },
	//		//        dataType: 'JSON'
	//		//    }).then(function successCallback(response) {
	//		//        $scope.MonthlyRevenueVSBudget = response.data;

	//		//        setMonthlyBudgetVSExpenseList($scope.MonthlyExpenseVSBudget, $scope.MonthlyRevenueVSBudget);
	//		//        createLineChart();
	//		//    });
	//		//});
	//		//$http({
	//		//    method: 'POST',
	//		//    url: 'accounts/ExpenseDashboard/PeriodExpenseVSBudgetBarChart',
	//		//    data: {
	//		//        'factDate': $scope.expFactDate.factDate,
	//		//        'fromDate': $scope.dateRange.fromDate,
	//		//        'toDate': $scope.dateRange.toDate
	//		//    },
	//		//    dataType: 'JSON'
	//		//}).then(function successCallback(response) {
	//		//$scope.MonthlyExpenseVSBudget = response.data;
	//		//$http({
	//		//        method: 'POST',
	//		//        url: 'accounts/ExpenseDashboard/PeriodRevenueVSBudgetBarChart',
	//		//        data: {
	//		//            'factDate': $scope.expFactDate.factDate,
	//		//            'fromDate': $scope.dateRange.fromDate,
	//		//            'toDate': $scope.dateRange.toDate
	//		//        },
	//		//        dataType: 'JSON'
	//		//    }).then(function successCallback(response) {
	//		//        $scope.MonthlyRevenueVSBudget = response.data;

	//		//        setPeriodWiseBudgetVSExpenseList($scope.MonthlyExpenseVSBudget, $scope.MonthlyRevenueVSBudget);
	//		//        createPeriodBarChart();
	//		//    });
	//		// });
	//	}
	//};





	//$scope.GetDetailDrillDownTableJS = function (data) {
	//	//debugger;
	//	$scope.DDList = [];
	//	$scope.expenseList = [];
	//	$scope.revenueList = [];
	//	$scope.ExpenseListTotal = 0;
	//	$scope.RevenueListTotal = 0;
	//	$scope.Companywiseplantdata = 'Companywiseplantdata';

	//	if ($scope.index + 2 < $scope.ColList.length) {
	//		$http({
	//			method: 'POST',
	//			url: 'Products/InventoryDashboard/InventoryStatusDashboardPlant/',
	//			data: {
	//				'ChartColumnList': $scope.ColList,
	//				'seq': $scope.index,
	//				'factDate': $scope.expFactDate.factDate,
	//				'fromDate': $scope.dateRange.fromDate,
	//				'toDate': $scope.dateRange.toDate,
	//				'CompanyWisePlant': $scope.dateRange.toDate,
	//				'CompanyId': data.CompanyId,
	//				//'CompanyId': data,
	//				'PlantId': '',
	//				'IsRegular': $scope.dataIsregular

	//			},
	//			dataType: 'JSON'
	//		}).then(function successCallback(response) {
	//			$scope.DDList = response.data;
	//			setList(response.data);
	//			$scope.index += 1;
	//			$scope.stIndex = $scope.index - 1;

	//		});


	//	}
	//};

	//$scope.exceptionList = {
	//	entryPeriod: null,
	//	postingPeriod: null,
	//	normalPostedAmout: null,
	//	delayPostedAmount: null
	//};
	//$scope.listForChart = [];

	//function setList(list) {
	//	$scope.ExpenseList = [];
	//	$scope.ExpenseList = list;
	//	$scope.chartLabel = [];
	//	$scope.chartList = [];
	//}
	//$scope.groupName = 'groupName';
	//$scope.data = false;
	//$scope.dataIsregular = false;
	//$scope.GetExpenseList = function () {
	//	//debugger;
	//	var obj = $("#Grid2").ejGrid("instance");
	//	var sd = obj.getFilteredRecords();
	//	var sd1 = obj.getSelectedRecords
	//	var value = "";
	//	var queryString = "''";
	//	var queryStringProcess = "''";
	//	if (sd.length > 0) {
	//		sd = obj.model.dataSource;
	//		$scope.plantvisible = 'visible';
	//	}
	//	else {
	//		var queryString = null;
	//		var queryStringProcess = null;
	//	}
	//	var arr = [];
	//	var queryString = [];
	//	var arrqueryStringProcess = [];


	//	var index = 0;
	//	for (var i = 0; i < sd.length; i++) {
	//		var x = sd[i];

	//		var yEntityName = x["CompanyId"];
	//		var yProcess = x["PlantId"];


	//		if (!arr.includes(yEntityName)) {
	//			queryString += ",'" + yEntityName + "'";
	//			//queryStringForSum += ",'" + yEntityName + "'";

	//			//queryString1 += ",'" + yEntityName + "'";
	//			arr.push(yEntityName);

	//		}
	//		if (!arrqueryStringProcess.includes(yProcess)) {
	//			queryStringProcess += ",'" + yProcess + "'";
	//			arrqueryStringProcess.push(yProcess);
	//		}
	//	}

	//	//debugger;
	//	var currentTotalEmp = 0;
	//	var proposedTotalEmp = 0;
	//	var Short = 0;
	//	var excess = 0;
	//	var unallocated = 0;

	//	$http({
	//		method: 'POST',
	//		url: 'Products/InventoryDashboard/InventoryDashboardStatus',
	//		data: {
	//			'factDate': $scope.expFactDate.factDate,
	//			'fromDate': $scope.dateRange.fromDate,
	//			'toDate': $scope.dateRange.toDate,
	//			'groupName': $scope.groupName,
	//			'Companywiseplantdata': $scope.Companywiseplantdata,
	//			'ValueOrNumber': $scope.dataIsregular
	//		},
	//		dataType: 'JSON'
	//	}).then(function successCallback(response) {
	//		setList(response.data);
	//		createColList();
	//	});
	//	//$http({
	//	//    method: 'POST',
	//	//    url: 'accounts/ExpenseDashboard/ExpenseListLineChart',
	//	//    data: {
	//	//        'factDate': $scope.expFactDate.factDate,
	//	//        'fromDate': $scope.dateRange.fromDate,
	//	//        'toDate': $scope.dateRange.toDate
	//	//    },
	//	//    dataType: 'JSON'
	//	//}).then(function successCallback(response) {
	//	//    $scope.expensePeriodList = response.data;
	//	//    $scope.ExpenseListTotal = $scope.expensePeriodList.reduce(function (sum, expense) {
	//	//        return sum + expense.Amount;
	//	//    }, 0);
	//	//    $http({
	//	//        method: 'POST',
	//	//        url: 'accounts/ExpenseDashboard/RevenueListLineChart',
	//	//        data: {
	//	//            'factDate': $scope.expFactDate.factDate,
	//	//            'fromDate': $scope.dateRange.fromDate,
	//	//            'toDate': $scope.dateRange.toDate
	//	//        },
	//	//        dataType: 'JSON'
	//	//    }).then(function successCallback(response) {
	//	//        $scope.revenuePeriodList = response.data;
	//	//        $scope.RevenueListTotal = $scope.revenuePeriodList.reduce(function (sum, expense) {
	//	//            return sum + expense.Amount;
	//	//        }, 0);
	//	//        if ($scope.expensePeriodList.length > 0 || $scope.revenuePeriodList.length > 0) {
	//	//            setLineList($scope.fiscalYearList, $scope.expensePeriodList, $scope.revenuePeriodList);
	//	//            //createLineChart();
	//	//        }
	//	//    });
	//	//});

	//	//$http({
	//	//    method: 'POST',
	//	//    url: 'accounts/ExpenseDashboard/PeriodWiseExpenseBarChart',
	//	//    data: {
	//	//        'factDate': $scope.expFactDate.factDate,
	//	//        'fromDate': $scope.dateRange.fromDate,
	//	//        'toDate': $scope.dateRange.toDate
	//	//    },
	//	//    dataType: 'JSON'
	//	//}).then(function successCallback(response) {
	//	//    $scope.BudgetedExpenseList = response.data;


	//	//    $http({
	//	//        method: 'POST',
	//	//        url: 'accounts/ExpenseDashboard/PeriodWiseRevenueBarChart',
	//	//        data: {
	//	//            'factDate': $scope.expFactDate.factDate,
	//	//            'fromDate': $scope.dateRange.fromDate,
	//	//            'toDate': $scope.dateRange.toDate
	//	//        },
	//	//        dataType: 'JSON'
	//	//    }).then(function successCallback(response) {
	//	//        $scope.BudgetedRevenueList = response.data;

	//	//        setPeriodicBudgetList($scope.BudgetedExpenseList, $scope.BudgetedRevenueList);
	//	//        createPeriodicalBudgetedBarChart();
	//	//    });


	//	//});


	//	//$http({
	//	//    method: 'POST',
	//	//    url: 'accounts/ExpenseDashboard/MonthlyExpenseVSBudgetBarChart',
	//	//    data: {
	//	//        'factDate': $scope.expFactDate.factDate,
	//	//        'fromDate': $scope.dateRange.fromDate,
	//	//        'toDate': $scope.dateRange.toDate
	//	//    },
	//	//    dataType: 'JSON'
	//	//}).then(function successCallback(response) {
	//	//    $scope.MonthlyExpenseVSBudget = response.data;


	//	//    $http({
	//	//        method: 'POST',
	//	//        url: 'accounts/ExpenseDashboard/MonthlyRevenueVSBudgetBarChart',
	//	//        data: {
	//	//            'factDate': $scope.expFactDate.factDate,
	//	//            'fromDate': $scope.dateRange.fromDate,
	//	//            'toDate': $scope.dateRange.toDate
	//	//        },
	//	//        dataType: 'JSON'
	//	//    }).then(function successCallback(response) {
	//	//        $scope.MonthlyRevenueVSBudget = response.data;

	//	//        setMonthlyBudgetVSExpenseList($scope.MonthlyExpenseVSBudget, $scope.MonthlyRevenueVSBudget);
	//	//        createLineChart();
	//	//    });
	//	//});
	//	//$http({
	//	//    method: 'POST',
	//	//    url: 'accounts/ExpenseDashboard/PeriodExpenseVSBudgetBarChart',
	//	//    data: {
	//	//        'factDate': $scope.expFactDate.factDate,
	//	//        'fromDate': $scope.dateRange.fromDate,
	//	//        'toDate': $scope.dateRange.toDate
	//	//    },
	//	//    dataType: 'JSON'
	//	//}).then(function successCallback(response) {
	//	//    $scope.MonthlyExpenseVSBudget = response.data;


	//	//    $http({
	//	//        method: 'POST',
	//	//        url: 'accounts/ExpenseDashboard/PeriodRevenueVSBudgetBarChart',
	//	//        data: {
	//	//            'factDate': $scope.expFactDate.factDate,
	//	//            'fromDate': $scope.dateRange.fromDate,
	//	//            'toDate': $scope.dateRange.toDate
	//	//        },
	//	//        dataType: 'JSON'
	//	//    }).then(function successCallback(response) {
	//	//        $scope.MonthlyRevenueVSBudget = response.data;

	//	//        setPeriodWiseBudgetVSExpenseList($scope.MonthlyExpenseVSBudget, $scope.MonthlyRevenueVSBudget);
	//	//        createPeriodBarChart();
	//	//    });
	//	//});

	//};


	//$scope.GetCompanyInformation = function () {

	//	$http({

	//		method: 'GET',

	//		url: 'Accounts/ExpenseDashboard/GetCompanyInformation/',

	//		//params: { 'dateType': $scope.expFactDate.factDate, 'itemType': 0 },

	//		dataType: 'JSON'

	//	}).then(function successCallback(response) {

	//		$scope.companyInformation = response.data;
	//		$scope.BaseCurrencyCode = $scope.companyInformation[0].BaseCurrencyCode;
	//	});
	//};

	////  $scope.GetCompanyInformation();
	//$scope.GetCompanyGroupData = function () {
	//	debugger;
	//	$scope.GetExpenseList();
	//};

	//$scope.GetCompanyGroupData();
	////$scope.factDateChange = function () {

	////    $http({

	////        method: 'GET',

	////        url: 'Accounts/ExpenseDashboard/GetVoucherLatestDate/',

	////        params: { 'dateType': $scope.expFactDate.factDate, 'itemType': 0 },

	////        dataType: 'JSON'

	////    }).then(function successCallback(response) {

	////        $scope.getFromDate = response.data;

	////        $scope.dateRange.fromDate = $filter("dateFiltering")($scope.getFromDate[0].PostingDate);

	////        $scope.dateRange.toDate = $filter("dateFiltering")($scope.getFromDate[0].PostingDate);
	////        $scope.GetFiscalYear();
	////        $scope.dFunction($filter("dateFiltering")($scope.getFromDate[0].PostingDate), $filter("dateFiltering")($scope.getFromDate[0].PostingDate));
	////    });
	////};

	////$scope.GetFiscalYear = function () {
	////    $scope.fiscalYearList = [];
	////    $http({
	////        method: 'GET',
	////        url: 'Accounts/ExpenseDashboard/GetFiscalYearForBarChart/',
	////        params: { 'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate },
	////        dataType: 'JSON'

	////    }).then(function successCallback(response) {
	////        $scope.fiscalYearList = response.data;
	////    });
	////};


	//$scope.valuenumber = function (data) {
	//	//debugger;

	//	$scope.data;
	//}

	//$scope.dataIsregular = false;
	//$scope.Isregular = function (data) {
	//	debugger;

	//	$scope.dataIsregular;
	//}


	//$scope.PoPopUp = function (data) {
	//	//debugger;
	//	angular.element(document.querySelector('#DetailModal')).modal('show');
	//}

	//$scope.GetBudgetWiseExpenseDetailJS = function (data, days, periodType) {
	//	//debugger;
	//	$scope.headerstatus = data.Category;
	//	$scope.days = "";
	//	if (days === '3') {
	//		$scope.days = '3';
	//	}
	//	else if (days === '5') {
	//		$scope.days = '5';
	//	}
	//	else if (days === '10') {
	//		$scope.days = '10';
	//	}
	//	else if (days === '15') {
	//		$scope.days = '15';
	//	}
	//	else if (days === '20') {
	//		$scope.days = '20';
	//	}
	//	else if (days === '25') {
	//		$scope.days = '25';
	//	}
	//	else if (days === '30') {
	//		$scope.days = '30';
	//	}
	//	else if (days === '31') {
	//		$scope.days = '31';
	//	}
	//	$scope.BudgetWiseExpenseDetailList = [];
	//	$http({
	//		method: 'POST',
	//		url: 'Products/InventoryDashboard/ModalBudgetWiseExpense',
	//		data: {
	//			'Category': data.Category,
	//			'days': $scope.days,
	//			'companyId': data.CompanyId,
	//			'PlantId': data.PlantId
	//		},
	//		dataType: 'JSON'
	//	}).then(function successCallback(response) {
	//		$scope.DetailList = response.data;
	//		angular.element(document.querySelector('#DetailModal')).modal('show');
	//	});
	//};


	$scope.MaterialTypeWiseMaterialList = [];
	$scope.GetMaterialTypeWiseMaterial = function (data) {
		
		$http({
			method: 'POST',
			url: 'Products/InventoryDashboard/MaterialTypeWiseMaterial?MaterialTypeID='+ data.MaterialTypeID,
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
			$scope.MaterialTypeWiseMaterialList = response.data;
			angular.element(document.querySelector('#DetailModal')).modal('show');
		});
		// }
	};


	////$scope.Isregulardata = false;
	////$scope.Isregular = function (data) {
	////    //debugger;

	////    $scope.Isregulardata;
	////}
	//$scope.ExpenseList = [];
	//$scope.GetDelayList = function () {
	//	debugger;
	//	var obj = $("#Grid2").ejGrid("instance");
	//	var sd = obj.getFilteredRecords();
	//	var sd1 = obj.getSelectedRecords
	//	var value = "";
	//	var queryString = "''";
	//	var queryStringProcess = "''";
	//	if (sd.length > 0) {
	//		sd = obj.model.dataSource;
	//		$scope.plantvisible = 'visible';
	//	}
	//	else {
	//		var queryString = null;
	//		var queryStringProcess = null;
	//	}
	//	var arr = [];
	//	var queryString = [];
	//	var arrqueryStringProcess = [];


	//	var index = 0;
	//	for (var i = 0; i < sd.length; i++) {
	//		var x = sd[i];

	//		var yEntityName = x["CompanyId"];
	//		var yProcess = x["PlantId"];


	//		if (!arr.includes(yEntityName)) {
	//			queryString += ",'" + yEntityName + "'";
	//			//queryStringForSum += ",'" + yEntityName + "'";

	//			//queryString1 += ",'" + yEntityName + "'";
	//			arr.push(yEntityName);

	//		}
	//		if (!arrqueryStringProcess.includes(yProcess)) {
	//			queryStringProcess += ",'" + yProcess + "'";
	//			arrqueryStringProcess.push(yProcess);
	//		}
	//	}

	//	//debugger;
	//	var currentTotalEmp = 0;
	//	var proposedTotalEmp = 0;
	//	var Short = 0;
	//	var excess = 0;
	//	var unallocated = 0;
	//	//$scope.expensePeriodList = [];
	//	//$scope.revenuePeriodList = [];
	//	//$scope.ExpenseListTotal = null;
	//	//$scope.RevenueListTotal = null;

	//	$http({
	//		method: 'POST',
	//		url: 'Products/InventoryDashboard/InventoryDashboardStatus',
	//		data: {
	//			'factDate': $scope.expFactDate.factDate,
	//			'fromDate': $scope.dateRange.fromDate,
	//			'toDate': $scope.dateRange.toDate,
	//			'groupName': $scope.groupName,
	//			//'Companywiseplantdata': $scope.Companywiseplantdata,
	//			'ValueOrNumber': $scope.dataIsregular,
	//			'queryString': queryString,
	//			'queryStringProcess': queryStringProcess,
	//			// 'PlantId': data.PlantId
	//		},
	//		dataType: 'JSON'
	//	}).then(function successCallback(response) {
	//		$scope.ExpenseList = response.data;
	//		//setList(response.data);
	//		//createColList();
	//	});
	//	//$scope.GetExpenseListGraph();


	//};





	//$scope.dFunction = function () {
	//	debugger;
	//	$scope.expensePeriodList = [];
	//	$scope.ExpenseListTotal = 0;
	//	$scope.revenuePeriodList = [];
	//	$scope.RevenueListTotal = 0;
	//	var obj = $("#Grid2").ejGrid("instance");
	//	var sd = obj.getFilteredRecords();
	//	if (sd.length == 0) {
	//		$scope.GetDelayList();
	//	}
	//	else {
	//		if (sd.length == 0) {
	//			sd = obj.model.dataSource;
	//		}
	//		var arr = [];
	//		var queryString = [];
	//		var arrqueryStringProcess = [];


	//		var value = "";
	//		var queryString = "''";
	//		var queryStringProcess = "''";

	//		var index = 0;
	//		for (var i = 0; i < sd.length; i++) {
	//			var x = sd[i];

	//			var yEntityName = x["CompanyId"];
	//			var yProcess = x["PlantId"];



	//			if (!arr.includes(yEntityName)) {
	//				queryString += ",'" + yEntityName + "'";

	//				arr.push(yEntityName);

	//			}
	//			if (!arrqueryStringProcess.includes(yProcess)) {
	//				queryStringProcess += ",'" + yProcess + "'";
	//				arrqueryStringProcess.push(yProcess);
	//			}




	//			$http({
	//				method: 'POST',
	//				url: 'Products/InventoryDashboard/InventoryDashboardStatus',
	//				data: {
	//					'factDate': $scope.expFactDate.factDate,
	//					'fromDate': $scope.dateRange.fromDate,
	//					'toDate': $scope.dateRange.toDate,
	//					'groupName': $scope.groupName,
	//					//'Companywiseplantdata': $scope.Companywiseplantdata,
	//					'ValueOrNumber': $scope.dataIsregular,
	//					'queryString': queryString,
	//					'queryStringProcess': queryStringProcess,
	//				},
	//				dataType: 'JSON'
	//			}).then(function successCallback(response) {
	//				$scope.ExpenseList = response.data;
	//				//setList(response.data);
	//				//if ($scope.ColList.length === 0) {
	//				//    createColList();
	//				//}
	//				//else {
	//				//    $scope.index = -1;
	//				//    $scope.stIndex = $scope.index - 1;
	//				//}


	//			});

	//		}
	//	}
	//};


	////#endregion

	////------------------------====--------------
	//$scope.filterDataList = [];
	//$scope.budgetCategoryWiseAmountList = [];

	//$scope.GetMasterFilterationData = function () {
	//	$http({
	//		method: 'POST',
	//		url: 'Accounts/MISAccountDashboard/GetBudgetWisevarianceElastic/',
	//		params: { 'companyGroupId': $scope.companyGroup.companyGroupId, 'companyId': $scope.company.companyId, 'plantId': $scope.plant.plantId, 'divisionId': $scope.division.divisionId, 'subDivisionId': $scope.subDivision.subDivisionId, 'unitId': $scope.unit.unitId, 'budgetCategory': $scope.budgetCategoryId, 'budgetSubCategory': $scope.budgetSubCategoryId, 'budget': $scope.budgetId, 'Activity': null, 'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate, 'bType': $scope.budgetType, 'dateType': $scope.expFactDate.factDate },
	//		dataType: 'JSON'
	//	}).then(function successCallback(response) {
	//		$scope.filterDataList = response.data;

	//		$("#EntityFilterGrid").children('.e-pager.e-js.e-pager').hide();
	//		$("#EntityFilterGrid").children('.e-gridcontent.e-droppable.e-js').hide();
	//		$("#EntityFilterGrid").children('.e-gridcontent').hide();
	//		$("#EntityFilterGrid").children('.e-grid.e-headercell').css('background-color', 'red');
	//		$scope.budgetCategoryWiseAmountList = [];
	//	});
	//	$http({
	//		method: 'POST',
	//		url: 'Accounts/MISAccountDashboard/GetBudgetCategoryWisevarianceElastic/',
	//		params: { 'companyGroupId': $scope.companyGroup.companyGroupId, 'companyId': $scope.company.companyId, 'plantId': $scope.plant.plantId, 'divisionId': $scope.division.divisionId, 'subDivisionId': $scope.subDivision.subDivisionId, 'unitId': $scope.unit.unitId, 'budgetCategory': $scope.budgetCategoryId, 'budgetSubCategory': $scope.budgetSubCategoryId, 'budget': $scope.budgetId, 'Activity': null, 'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate, 'bType': $scope.budgetType, 'dateType': $scope.expFactDate.factDate },
	//		dataType: 'JSON'
	//	}).then(function successCallback(response) {
	//		$scope.BudgetCategoryDataList = response.data;

	//		$http({
	//			method: 'POST',
	//			url: 'Accounts/MISAccountDashboard/GetBudgetSubCategoryWisevarianceElastic/',
	//			params: { 'companyGroupId': $scope.companyGroup.companyGroupId, 'companyId': $scope.company.companyId, 'plantId': $scope.plant.plantId, 'divisionId': $scope.division.divisionId, 'subDivisionId': $scope.subDivision.subDivisionId, 'unitId': $scope.unit.unitId, 'budgetCategory': $scope.budgetCategoryId, 'budgetSubCategory': $scope.budgetSubCategoryId, 'budget': $scope.budgetId, 'Activity': null, 'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate, 'bType': $scope.budgetType, 'dateType': $scope.expFactDate.factDate },
	//			dataType: 'JSON'
	//		}).then(function successCallback(response) {
	//			$scope.BudgetSubCategoryDataList = response.data;
	//			$http({
	//				method: 'POST',
	//				url: 'Accounts/MISAccountDashboard/GetBudgetItemWisevarianceElastic/',
	//				params: { 'companyGroupId': $scope.companyGroup.companyGroupId, 'companyId': $scope.company.companyId, 'plantId': $scope.plant.plantId, 'divisionId': $scope.division.divisionId, 'subDivisionId': $scope.subDivision.subDivisionId, 'unitId': $scope.unit.unitId, 'budgetCategory': $scope.budgetCategoryId, 'budgetSubCategory': $scope.budgetSubCategoryId, 'budget': $scope.budgetId, 'Activity': null, 'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate, 'bType': $scope.budgetType, 'dateType': $scope.expFactDate.factDate },
	//				dataType: 'JSON'
	//			}).then(function successCallback(response) {
	//				$scope.BudgetItemDataList = response.data;

	//				$scope.loadGrid($scope.BudgetCategoryDataList, $scope.BudgetSubCategoryDataList, $scope.BudgetItemDataList);
	//			});
	//		});

	//	});

	//};



	//var EntityWiseVarianceListPl = null;
	//$scope.dataLoad = function () {

	//	if (new Date($scope.dateRange.toDate) < new Date($scope.dateRange.fromDate)) {
	//		throw ShowResult("From date can not be greater then to date", 'failure');
	//	}
	//	else {
	//		$scope.GetBudgetWisevarianceList();
	//		$scope.dashBoardChangeMIS();

	//		$scope.titleLine = null;

	//		$scope.titleLine = $scope.itemName.bold() + " items from " + $scope.dateRange.fromDate.bold() + " to " + $scope.dateRange.toDate.bold() + " in the basis of " + $scope.expFactDate.factDate.bold();

	//		document.getElementById("title").style.display = "block";


	//		document.getElementById("title").innerHTML = $scope.titleLine;
	//	}
	//};

}


