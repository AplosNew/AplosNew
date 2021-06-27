'use strict';
skillMatrixController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window','$controller'];
function skillMatrixController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, $controller) {
	//accountService, addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller
    $rootScope.title = 'Skill Martix';
    $scope.path = 'Employees/skillMatrix/';
	$scope.popUpList = [];
	$scope.valueData = '';
	$scope.filedata = '';
	$scope.message = null;
	$scope.imageSrc = null;
	$scope.Action = 'Save';
	$scope.maxDate = new Date().toDateString();
	$scope.exportgriddataUrl = 'GridReports/ExcelExportJson';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';
	$scope.ShowLoader = true;
	$scope.loadstatus = false;
	$scope.Print = function () {
		debugger;
		// var gridObj = $("#DetailGrid").data("ejGrid");
		var gridObj = $("#DetailGrid").ejGrid("instance");
		var data = gridObj.model.dataSource;
		$http({
			method: 'POST',
			url: $scope.exportgriddataUrl,
			data: { 'obj': JSON.stringify(data) }
		}).then(function successCallback(response) {
			if (response.data.Error == true) {
				// ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');

			}
			else {

				location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
			}
		});
	}
	$scope.PrintSum = function () {
		debugger;
		var gridObj = $("#Grid1").data("ejGrid");
		//var gridObj = $("#Grid1").ejGrid("instance");
		var data = $scope.SummaryList; //gridObj.model.dataSource;
		$http({
			method: 'POST',
			url: $scope.exportgriddataUrl,
			data: { 'obj': JSON.stringify(data) }
		}).then(function successCallback(response) {
			if (response.data.Error == true) {
				// ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');

			}
			else {

				location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
			}
		});
	}
	$scope.ClearFilter = function () {
		debugger;
		var gridObj = $("#Grid2").ejGrid("instance");
		gridObj.refreshContent(); // Refreshes the grid contents only 
		gridObj.refreshContent(true); // Refreshes the

		var gridObj1 = $("#DetailGrid").ejGrid("instance");
		gridObj1.refreshContent(); // Refreshes the grid contents only 
		gridObj1.refreshContent(true); // Refreshes the


	}

	$scope.LoadData = function GetEntity() {
		debugger;
		$scope.ShowLoader = true;
		$.ajax({
			type: "GET",
			contentType: "application/json; charset=utf-8",
			url: 'Employees/SkillMatrix/GetSkillMaster',
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
					//summaryRows:
					//    [
					//        {
					//            title: "Total =",
					//            color: "Red",
					//            summaryColumns: [
					//                {
					//                    summaryType: ej.Grid.SummaryType.Sum
					//                    , displayColumn: "ManpowerBudget"
					//                    , dataMember: "ManpowerBudget"
					//                    //, format: "{0:C1}"
					//                },
					//                {
					//                    summaryType: ej.Grid.SummaryType.Sum
					//                    , displayColumn: "OnRoll"
					//                    , dataMember: "OnRoll"
					//                    //, format: "{0:C1}"
					//                },
					//                {
					//                    summaryType: ej.Grid.SummaryType.Sum
					//                    , displayColumn: "TotalPresent"
					//                    , dataMember: "TotalPresent"
					//                    //, format: "{0:C1}"
					//                },
					//                {
					//                    summaryType: ej.Grid.SummaryType.Sum
					//                    , displayColumn: "OnRollShort"
					//                    , dataMember: "OnRollShort"
					//                    //, format: "{0:C1}"
					//                },
					//                {
					//                    summaryType: ej.Grid.SummaryType.Sum
					//                    , displayColumn: "OnRollExcess"
					//                    , dataMember: "OnRollExcess"
					//                    //, format: "{0:C1}"
					//                },
					//                {
					//                    summaryType: ej.Grid.SummaryType.Sum
					//                    , displayColumn: "PresentShort"
					//                    , dataMember: "PresentShort"
					//                    //, format: "{0:C1}"
					//                },
					//                {
					//                    summaryType: ej.Grid.SummaryType.Sum
					//                    , displayColumn: "PresentExcess"
					//                    , dataMember: "PresentExcess"
					//                    //, format: "{0:C1}"
					//                },
					//            ]

					//        }
					//    ],

					columns: [
						{ headerText: "Unit", field: "EntityName", width: 60 }
						//{ headerText: "Process", field: "Process", width: 80 },
						//{ headerText: "Skill", field: "Skill", width: 60 },
						//{ headerText: "Operation Name", field: "OperationName", width: 90 },
						//{ headerText: "Skill Group", field: "SkillGroupe", width: 90 },
						//{ headerText: "Operation Category", field: "OperationCategoryName", width: 80 },
						//{ headerText: "Machine Category", field: "MachineCategory", width: 85 },
						//{ headerText: "Machine Sub Category", field: "MachineSubCategory", width: 95 },
						////{ headerText: "Position", field: "Position", width: 80 },
						//{ headerText: "OnRoll", field: "OnRoll", width: 70 },
						//{ headerText: "T.Present", field: "TotalPresent", width: 80 },
						//{ headerText: "OR.Short", field: "OnRollShort", width: 80 },
						//{ headerText: "OR.Excess", field: "OnRollExcess", width: 90 },
						//{ headerText: "P.Short", field: "PresentShort", width: 80 },
						//{ headerText: "P.Excess", field: "PresentExcess", width: 80 }




						//{ field: "OerationCode", headerText: "Operation Code", textAlign: ej.TextAlign.Right, width: 200 },
						//{ field: "OperationName", headerText: "Operation Name", width: 200, visibility: false },
						//{ headerText: "Type", field: "Type", width: 100 },
						// { headerText: "Machine Master", field: "MachineMaster", width: 200 },

						//{ title: "EntityCode", field: "EntityCode", filterable: true, width: 200, filterable: { multi: true, search: true } },

						//{ title: "PositionCode", field: "PositionCode", filterable: true, filterable: { multi: true, search: true } },
						//{ headerText: "PositionName", field: "PositionName", width: 200 },

						//{ title: "MachineCode", field: "MachineCode", width: 200, filterable: true, filterable: { multi: true, search: true } },

						//{ headerText: "MachineCode", field: "MachineCode", width: 200 },
						//        //{ title: "MachineCategoryCode", field: "MachineCategoryCode", width: 200, filterable: true, filterable: { multi: true, search: true } },


						// { headerText: "SkillGroupingCode", field: "SkillGroupingCode", width: 200 },

						// { headerText: "DesignationCategory", field: "DesignationCategory", width: 200 },
						//{ headerText: "StandardSalary", field: "StandardSalary", width: 200 },

						//{ headerText: "LegalDesignation", field: "LegalDesignation", width: 200 }

						//{ headerText: "ManpowerBudget", field: "ManpowerBudget", width: 200 },

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
	var queryStringForSum = "''";
	$scope.ViewData = function () {
		$scope.ShowLoader = true;
		// $(".loaderr").show(2000);
		// debugger;
		// $('#load').show();
		var obj = $("#Grid2").ejGrid("instance");
		var sd = obj.getFilteredRecords();
		if (sd.length == 0) {
			sd = obj.model.dataSource;
			//alert('1' +1);
		}
		var arr = [];
		var queryString = [];
		queryStringForSum = [];
		var queryString1 = [];
		var arrqueryStringProcess = [];
		var arrqueryStringSkill = [];
		var arrqueryStringGrouping = [];
		var arrqueryStringMachineCategory = [];
		var arrqueryStringMachineSubCategoryCode = [];
		var arrqueryStringCaption = [];
		var arrqueryStringOperationCode = [];
		var arrqueryStringOperationCategoryId = [];
		var arrqueryStringOnRoll = [];
		var arrqueryStringTotalPresent = [];
		var arrqueryStringOnRollShort = [];
		var arrqueryStringOnRollExcess = [];
		var arrqueryStringPresentShort = [];
		var arrqueryStringPresentExcess = [];
		// var queryStringSkillary = new Array(400)

		var value = "";
		var queryString = "''";
		$scope.queryString1 = "''";
		var queryStringCaption = "''";
		var queryStringProcess = "''";
		var queryStringSkill = "''";
		var queryStringOperationCode = "''";

		var queryStringGrouping = "''";
		var queryStringMachineCategory = "''";
		var queryStringMachineSubCategoryCode = "''";
		var queryStringCaption = "''";
		var queryStringOperationCategoryId = "''";

		var queryStringOnRoll = "''";
		var queryStringTotalPresent = "0";
		var queryStringOnRollShort = "0";
		var queryStringOnRollExcess = "0";
		var queryStringPresentShort = "0";
		var queryStringPresentExcess = "0";
		var skillList = [];
		var index = 0;
		for (var i = 0; i < sd.length; i++) {
			var x = sd[i];
			//var yEntityName = x["EntityName"];
			//var yposition = x["Caption"];
			//var yProcess = x["Process"];
			//var ySkill = x["Skill"];
			//var ySkillId = x["SkillId"];
			//var yGrouping = x["Grouping"];
			//var yMachineCategory = x["MachineCategory"];
			//var yMachineSubCategoryCode = x["MachineSubCategoryCode"];
			//var yCaption = x["MachineSubCategoryCode"];
			var yOnRoll = x["OnRoll"];
			var yTotalPresent = x["TotalPresent"];
			var yRollShort = x["RollShort"];
			var yOnRollExcess = x["OnRollExcess"];
			var yPresentShort = x["PresentShort"];
			var yPresentExcess = x["PresentExcess"];

			var yEntityName = x["EntityId"];
			var yposition = x["Position"];
			var yProcess = x["ProcessId"];
			var ySkill = x["SkillId"];
			var yOperationCode = x["OperationCode"];

			//var ySkillId = x["SkillId"];
			var yGrouping = x["SkillGroupId"];

			var yMachineCategory = x["MachineCategoryId"];
			var yMachineSubCategoryCode = x["MachineSubCategoryId"];
			var yOperationCategoryId = x["OperationCategoryId"];

			//var yCaption = x["MachineSubCategoryCode"];
			var yOnRoll = x["OnRoll"];
			var yTotalPresent = x["TotalPresent"];
			var yRollShort = x["OnRollShort"];
			var yOnRollExcess = x["OnRollExcess"];
			var yPresentShort = x["PresentShort"];
			var yPresentExcess = x["PresentExcess"];


			if (!arr.includes(yEntityName)) {
				queryString += ",'" + yEntityName + "'";
				queryStringForSum += ",'" + yEntityName + "'";

				queryString1 += ",'" + yEntityName + "'";
				arr.push(yEntityName);

			}
			if (!arrqueryStringProcess.includes(yProcess)) {
				queryStringProcess += ",'" + yProcess + "'";
				arrqueryStringProcess.push(yProcess);
			}

			if (!arrqueryStringSkill.includes(ySkill)) {
				queryStringSkill += ",'" + ySkill + "'";
				arrqueryStringSkill.push(ySkill);

			}
			if (!arrqueryStringOperationCode.includes(yOperationCode)) {
				queryStringOperationCode += ",'" + yOperationCode + "'";
				arrqueryStringOperationCode.push(yOperationCode);

			}

			if (!arrqueryStringGrouping.includes(yGrouping)) {
				queryStringGrouping += ",'" + yGrouping + "'";
				arrqueryStringGrouping.push(yGrouping);
			}
			if (!arrqueryStringMachineCategory.includes(yMachineCategory)) {
				queryStringMachineCategory += ",'" + yMachineCategory + "'";
				arrqueryStringMachineCategory.push(yMachineCategory);
			}
			if (!arrqueryStringMachineSubCategoryCode.includes(yMachineSubCategoryCode)) {
				queryStringMachineSubCategoryCode += ",'" + yMachineSubCategoryCode + "'";
				arrqueryStringMachineSubCategoryCode.push(yMachineSubCategoryCode);
			}
			if (!arrqueryStringCaption.includes(yposition)) {
				queryStringCaption += ",'" + yposition + "'";
				arrqueryStringCaption.push(yposition);
			}
			if (!arrqueryStringOperationCategoryId.includes(yOperationCategoryId)) {
				queryStringOperationCategoryId += ",'" + yOperationCategoryId + "'";
				arrqueryStringOperationCategoryId.push(yOperationCategoryId);
			}


			//if (!arr.includes(yCaption)) {
			//     queryStringCaption += ",'" + yCaption + "'";
			//     arr.push(yCaption);
			// }
			if (!arrqueryStringOnRoll.includes(yOnRoll)) {
				queryStringOnRoll += ",'" + yOnRoll + "'";
				arrqueryStringOnRoll.push(yOnRoll);
			}

			if (!arrqueryStringTotalPresent.includes(yTotalPresent)) {
				queryStringTotalPresent += "," + yTotalPresent + "";
				arrqueryStringTotalPresent.push(yTotalPresent);
			}
			if (!arrqueryStringOnRollShort.includes(yRollShort)) {
				queryStringOnRollShort += "," + yRollShort + "";
				arrqueryStringOnRollShort.push(yRollShort);
			}
			if (!arrqueryStringOnRollExcess.includes(yOnRollExcess)) {
				queryStringOnRollExcess += "," + yOnRollExcess + "";
				arrqueryStringOnRollExcess.push(yOnRollExcess);
			}
			if (!arrqueryStringPresentShort.includes(yPresentShort)) {
				queryStringPresentShort += "," + yPresentShort + "";
				arrqueryStringPresentShort.push(yPresentShort);
			}
			if (!arrqueryStringPresentExcess.includes(yPresentExcess)) {
				queryStringPresentExcess += "," + yPresentExcess + "";
				arrqueryStringPresentExcess.push(yPresentExcess);
			}

		}
		$('#loadingmessage').show();

		//#region grid filter clear and refresh
		//var gridObj = $("#DetailGrid").data("ejGrid");
		//var gridObj = $("#DetailGrid").ejGrid("instance");
		//gridObj.clearFiltering();

		//$scope.data=[];
		//var gridObj = $("#DetailGrid").data("ejGrid");
		//gridObj.refreshContent(true);
		//#endregion
		$.ajax({
			type: "POST",
			url: 'Employees/SkillMatrix/GetSkillMasterDetails',
			data: {
				'queryString': queryString,
				'queryStringProcess': queryStringProcess,
				'queryStringSkill': queryStringSkill,
				'queryStringGrouping': queryStringGrouping,
				'queryStringMachineCategory': queryStringMachineCategory,
				'queryStringMachineSubCategoryCode': queryStringMachineSubCategoryCode,
				'queryStringCaption': queryStringCaption,
				'queryStringOperationCode': queryStringOperationCode,
				'queryStringOperationCategoryId': queryStringOperationCategoryId,
				'queryStringOnRoll': queryStringOnRoll,
				'queryStringTotalPresent': queryStringTotalPresent,
				'queryStringOnRollShort': queryStringOnRollShort,
				'queryStringOnRollExcess': queryStringOnRollExcess,
				'queryStringPresentShort': queryStringPresentShort,
				'queryStringPresentExcess': queryStringPresentExcess

			},
			dataType: "json",
			success: function (data) {
				$scope.viewlist = data;
				$scope.ShowLoader = false;
				$('#ld').hide();

				$("#DetailGrid").ejGrid({
					dataSource: data, // data must be array of json
					allowPaging: true,
					allowSorting: true,
					allowFiltering: true,
					isResponsive: true,
					minWidth: 600,
					allowResizeToFit: true,
					canResize: true,
					//allowTextWrap: true,
					allowTextWrap: true,
					textWrapSettings: { wrapMode: "header" },
					enableResponsiveRow: true,
					filterSettings: {
						filterType: "excel"
					},
					cssClass: "filtered",
					pageSize: 10,
					allowScrolling: true,
					scrollSettings: { wisth: "1250", height: "300" },
					summaryRows:
						[
							{
								title: "Total =",
								color: "Red",
								summaryColumns: [
									{
										summaryType: ej.Grid.SummaryType.Sum
										, displayColumn: "ManpowerBudget"
										, dataMember: "ManpowerBudget"
										//, format: "{0:C1}"
									},
									{
										summaryType: ej.Grid.SummaryType.Sum
										, displayColumn: "OnRoll"
										, dataMember: "OnRoll"
										//, format: "{0:C1}"
									},
									{
										summaryType: ej.Grid.SummaryType.Sum
										, displayColumn: "TotalPresent"
										, dataMember: "TotalPresent"
										//, format: "{0:C1}"
									},
									{
										summaryType: ej.Grid.SummaryType.Sum
										, displayColumn: "OnRollShort"
										, dataMember: "OnRollShort"
										//, format: "{0:C1}"
									},
									{
										summaryType: ej.Grid.SummaryType.Sum
										, displayColumn: "OnRollExcess"
										, dataMember: "OnRollExcess"
										//, format: "{0:C1}"
									},
									{
										summaryType: ej.Grid.SummaryType.Sum
										, displayColumn: "PresentShort"
										, dataMember: "PresentShort"
										//, format: "{0:C1}"
									},
									{
										summaryType: ej.Grid.SummaryType.Sum
										, displayColumn: "PresentExcess"
										, dataMember: "PresentExcess"
										//, format: "{0:C1}"
									},
								]

							}
						],

					columns: [
						{
							headerText: "Process", field: "Process", width: 100
						},
						{ headerText: "Skill", field: "Skill", width: 80 },
						{ headerText: "Machine Master", field: "MachineMaster", width: 120 },
						{ headerText: "Type", field: "Type", width: 80 },
						{ headerText: "Skill Group Cat", field: "SkillGroupe", width: 100 },
						{ headerText: "Operation Category", field: "OperationCategoryName", width: 80 },
						{ headerText: "Operation Code", field: "OperationCode", width: 110 },
						{ headerText: "Operation", field: "OperationName", width: 110 },
						{ headerText: "Standard Salary", field: "StandardSalary", width: 80, textAlign: ej.TextAlign.Right },
						{ headerText: "Standard Budget", field: "ManpowerBudget", width: 80, textAlign: ej.TextAlign.Right },
						{ headerText: "Alloted Manpower", field: "AllotedManpower", width: 80, textAlign: ej.TextAlign.Right },

						{ headerText: "On Roll", field: "OnRoll", width: 60, textAlign: ej.TextAlign.Right },
						{ headerText: "Total Present", field: "TotalPresent", width: 70, textAlign: ej.TextAlign.Right },
						{ headerText: "OnRoll Short", field: "OnRollShort", width: 70, textAlign: ej.TextAlign.Right },
						{ headerText: "OnRoll Excess", field: "OnRollExcess", width: 80, textAlign: ej.TextAlign.Right },
						{ headerText: "Present Short", field: "PresentShort", width: 70, textAlign: ej.TextAlign.Right },
						{ headerText: "Present Excess", field: "PresentExcess", width: 70, textAlign: ej.TextAlign.Right },

						//{
						//    headerText: "OnRoll", field: "OnRoll", width: 70, textAlign: ej.TextAlign.Right, cssClass: "text"}, /*, cssClass: "customCSS example"*/
						//{ headerText: "TotalPresent", field: "TotalPresent", width: 70, textAlign: ej.TextAlign.Right},
						//{ headerText: "OnRollShort", field: "OnRollShort", width: 70, textAlign: ej.TextAlign.Right },
						//{ headerText: "OnRollExcess", field: "OnRollExcess", width: 70, textAlign: ej.TextAlign.Right},
						//{ headerText: "PresentShort", field: "PresentShort", width: 50, textAlign: ej.TextAlign.Right},
						//{ headerText: "PresentExcess", field: "PresentExcess", width: 50,textAlign: ej.TextAlign.Right },


						{ headerText: "EntityName", field: "EntityName", width: 80, visible: false },
						{ headerText: "MachineCategory", field: "MachineCategory", width: 100, visible: false },
						{ headerText: "MachineSubCategory", field: "MachineSubCategory", width: 100, visible: false },
						{ headerText: "Position", field: "Position", width: 100, visible: false },


					],

					//rowSelected: rowSelected,
					recordDoubleClick: rowSelected,
					actionComplete: function (args) {
						debugger;
						if (args.requestType == "filtering") {
							var obj = $("#DetailGrid").ejGrid("instance");
							var sd1 = obj.getFilteredRecords();
							if (sd1.length == 0) {
								sd1 = obj.model.dataSource;
							}

							var listOnrollBalance = [];//sd1.clone(false);
							var checklist = [];
							for (var i = 0; i < sd1.length; i++) {
								var xxxxx = sd1[i];
								if (!checklist.includes(xxxxx["OperationCode"])) {

									var opname = xxxxx["OperationCode"] + '-' + xxxxx["OperationName"];
									var total = 0;
									var color = "";
									for (var j = 0; j < sd1.length; j++) {
										var yyyy = sd1[j];
										if (xxxxx["OperationCode"] == yyyy["OperationCode"]) {
											if (yyyy["OnRollShort"] > 0)
												total += yyyy["OnRollShort"] * -1;
											else
												total += yyyy["OnRollExcess"];

										}
									}
									var datatopush = {
										'OperationName': opname,
										'OnRollBalance': total
									}
									listOnrollBalance.push(datatopush);

									checklist.push(xxxxx["OperationName"]);
								}
							}
							listOnrollBalance.sort(function (a, b) { return b.OnRollBalance - a.OnRollBalance });


							$("#container").ejChart({

								legend: {
									//Visible chart legend
									visible: false
								},
								refreshContent: true,
								canResize: true,
								allowScrolling: true,
								isResponsive: true,
								// name: "OnRollBalance",

								//tooltip: { visible: true, format: "<div style = 'background:\"'#series.colour'\">#series.yName#  <br/> #point.x# : #point.y# </div>" },
								series: [
									{

										title: {
											//Add chart title
											text: 'On Roll'
										},
										type: 'Bar',
										//fill: color,
										dataSource: listOnrollBalance,//sd1,
										//primaryYAxis: { labelPosition: "inside" },
										legend: { title: { font: { fontFamily: "Algerian" } } },
										xName: "OperationName",
										yName: "OnRollBalance",
										enableAnimation: true,
										name: 'OnRollBalance',

										tooltip: {
											visible: true
										},

										marker: {
											visible: true,
										}


									}
								],
								preRender: function (args) {
									var chart = $("#container").ejChart("instance");
									var length = chart.model.series[0].points.length - 1;
									for (var i = 0; i < chart.model.series[0].points.length; i++) {
										if (chart.model.series[0].points[i].YValues[0] > 0)
											chart.model.series[0].points[i].fill = "Yellow";
										else if (chart.model.series[0].points[i].YValues[0] < 0)
											chart.model.series[0].points[i].fill = "red";
										else
											chart.model.series[0].points[i].fill = "Green";
									}
									//$("#container").ejTooltip(
									//    {
									//        content: "JavaScript is the programming language of HTML and the Web.",
									//        //associate: "mousefollow"
									//        associate: "axis",
									//        position: {
									//            target: { horizontal: 10, vertical: 10 }
									//        }
									//    }); 

								},
								primaryXAxis: {
									range: {
										min: -6,
										max: 6,
										interval: 2
									}
									//labelFormat: "0.00"
								}

							});

							var chart = $("#container").ejChart("instance");
							chart.model.size.height = 100 + (listOnrollBalance.length * 20);
							chart.redraw();


							var listOnrollBalance1 = [];//sd1.clone(false);
							var checklist1 = [];

							for (var i = 0; i < sd1.length; i++) {
								var xxxxxx = sd1[i];
								if (!checklist1.includes(xxxxxx["OperationCode"])) {

									var opnamee = xxxxxx["OperationCode"] + '-' + xxxxxx["OperationName"];
									var totall = 0;
									for (var j = 0; j < sd1.length; j++) {
										var yyyyy = sd1[j];
										if (xxxxxx["OperationCode"] == yyyyy["OperationCode"]) {
											if (yyyyy["PresentShort"] > 0)
												totall += yyyyy["PresentShort"] * -1;
											else
												totall += yyyyy["PresentExcess"];
										}
									}
									var datatopush1 = {
										'OperationName': opnamee,
										'OnRollBalance': totall
									}
									listOnrollBalance1.push(datatopush1);

									checklist1.push(xxxxxx["OperationName"]);
								}
							}
							listOnrollBalance1.sort(function (a, b) { return b.OnRollBalance - a.OnRollBalance });
							$("#container1").ejChart({

								legend: {
									//Visible chart legend
									visible: false
								},
								refreshContent: true,
								canResize: true,
								//size: { width: '600', height: '3000' },

								isResponsive: true,
								series: [{
									title: {
										//Add chart title
										text: 'Present'
									},
									//type: 'column',
									type: 'Bar',
									dataSource: listOnrollBalance1,
									//primaryYAxis: { labelPosition: "inside" },
									legend: { title: { font: { fontFamily: "Algerian" } } },
									xName: "OperationName",
									yName: "OnRollBalance",
									enableAnimation: true,
									tooltip: {
										visible: true
									},
									marker: {
										visible: true,
									}


								}],
								preRender: function (args) {
									var chart1 = $("#container1").ejChart("instance");
									var length = chart1.model.series[0].points.length - 1;
									for (var i = 0; i < chart1.model.series[0].points.length; i++) {
										if (chart1.model.series[0].points[i].YValues[0] > 0)
											chart1.model.series[0].points[i].fill = "Yellow";
										else if (chart1.model.series[0].points[i].YValues[0] < 0)
											chart1.model.series[0].points[i].fill = "red";
										else
											chart1.model.series[0].points[i].fill = "Green";
									}
									//$("#container1").ejTooltip(
									//                                  {
									//                                      content: "JavaScript is the programming language of HTML and the Web.",
									//                                      //associate: "mousefollow"
									//                                      associate: "axis",
									//                                      position: {
									//                                          target: { horizontal: 10, vertical: 10 }
									//                                      }
									//                                  }); 
								},
								primaryXAxis: {
									range: {
										min: -6,
										max: 6,
										interval: 2
									}
									// labelFormat:"0.00"
								}
							});
							var chart1 = $("#container1").ejChart("instance");
							chart1.model.size.height = 100 + (listOnrollBalance1.length * 20);
							chart1.redraw();
						}
					}

				});


				$("#present").show();
				$("#onroll").show();
				debugger;
				var gridObj = $("#Grid2").data("ejGrid");
				//getting corresponding record             
				var data = gridObj.getSelectedRecords()[0];
				var obj = $("#Grid2").ejGrid("instance");
				var sdchk = obj.getFilteredRecords();

				//#region Graph View For Master Data
				if (sdchk.length == 0) {
					sdchk = obj.model.dataSource;
					//alert('1' + 1);
					$.ajax({
						type: "POST",
						//contentType: "application/json; charset=utf-8",
						url: 'Employees/SkillMatrix/GetGraphDetails1',
						data: {},
						dataType: "json",
						success: function (data) {

							$("#container").ejChart({
								primaryXAxis: {
									range: {
										min: 0,
										max: 50,
										interval: 1
									}

								},
								legend: {
									//Visible chart legend
									visible: false
								},
								canResize: true,
								// size: { width: '600', height: '3000' },
								isResponsive: true,
								//refreshContent:true,
								series: [{
									title: {
										//Add chart title
										text: 'On Roll'
									},
									type: 'Bar',
									dataSource: data,
									//primaryYAxis: { labelPosition: "inside" },
									legend: { title: { font: { fontFamily: "Algerian" } } },
									xName: "OperationName",
									yName: "OnRollBalance",
									enableAnimation: true,
									tooltip: {
										visible: true
									},
									marker: {
										visible: true,
									}



								}],
								preRender: function (args) {
									var chart2 = $("#container").ejChart("instance");
									var length = chart2.model.series[0].points.length - 1;
									for (var i = 0; i < chart2.model.series[0].points.length; i++) {
										if (chart2.model.series[0].points[i].YValues[0] > 0)
											chart2.model.series[0].points[i].fill = "Yellow";
										else if (chart2.model.series[0].points[i].YValues[0] < 0)
											chart2.model.series[0].points[i].fill = "red";
										else
											chart2.model.series[0].points[i].fill = "Green";
									}

								},
							});

							var chart2 = $("#container").ejChart("instance");
							chart2.model.size.height = 100 + (data.length * 16);
							chart2.model.negativeColor = "Green";
							chart2.redraw();
							$("#container1").ejChart({
								plotOptions: {
									columnrange: {
										negativeColor: 'Green',
										threshold: 0,
										dataLabels: {
											enabled: true,
											formatter: function () {

											}
										}
									}
								},
								legend: {
									//Visible chart legend
									visible: false
								},
								primaryXAxis: {
									//range: {
									//    min: 0,
									//    max: 50,
									//    interval: 1
									//}
									LabelFormat: "0"
								},
								canResize: true,
								//size: { width: '600', height: '3000' },
								isResponsive: true,
								//refreshContent: true,
								series: [{
									title: {
										//Add chart title
										text: 'Present'
									},
									//type: 'column',
									type: 'Bar',
									dataSource: data,
									//primaryYAxis: { labelPosition: "inside" },
									legend: { title: { font: { fontFamily: "Algerian" } } },
									xName: "OperationName",
									yName: "PresentBalance",
									enableAnimation: true,
									tooltip: {
										visible: true
									},
									marker: {
										visible: true,
									}


								}],
								preRender: function (args) {
									var chart3 = $("#container1").ejChart("instance");
									var length = chart3.model.series[0].points.length - 1;
									for (var i = 0; i < chart3.model.series[0].points.length; i++) {
										if (chart3.model.series[0].points[i].YValues[0] > 0)
											chart3.model.series[0].points[i].fill = "Yellow";
										else if (chart3.model.series[0].points[i].YValues[0] < 0)
											chart3.model.series[0].points[i].fill = "red";
										else
											chart3.model.series[0].points[i].fill = "Green";
									}

								},
							});

							var chart3 = $("#container1").ejChart("instance");

							chart3.model.size.height = 100 + (data.length * 16);
							chart3.model.negativeColor = "Green";
							chart3.redraw();
							// $("#progressbar-5").prop('disabled', true);
						}
					});
				}
				else {
					sdchk = obj.model.dataSource;
					debugger;
					$.ajax({
						type: "POST",
						//contentType: "application/json; charset=utf-8",
						url: 'Employees/SkillMatrix/GetGraphDetails',
						data: {

							'queryString': queryString,
							'queryStringProcess': queryStringProcess,
							'queryStringSkill': queryStringSkill,
							'queryStringGrouping': queryStringGrouping,
							'queryStringMachineCategory': queryStringMachineCategory,
							'queryStringMachineSubCategoryCode': queryStringMachineSubCategoryCode,
							'queryStringCaption': queryStringCaption,
							'queryStringOperationCode': queryStringOperationCode,
							'queryStringOperationCategoryId': queryStringOperationCategoryId,
							'queryStringOnRoll': queryStringOnRoll,
							'queryStringTotalPresent': queryStringTotalPresent,
							'queryStringOnRollShort': queryStringOnRollShort,
							'queryStringOnRollExcess': queryStringOnRollExcess,
							'queryStringPresentShort': queryStringPresentShort,
							'queryStringPresentExcess': queryStringPresentExcess
						},
						dataType: "json",
						success: function (data) {

							$("#container").ejChart({
								primaryXAxis: {
									range: {
										min: 0,
										max: 50,
										interval: 1
									}

								},
								legend: {
									//Visible chart legend
									visible: false
								},
								canResize: true,
								// size: { width: '600', height: '3000' },
								isResponsive: true,
								//refreshContent:true,
								series: [{
									title: {
										//Add chart title
										text: 'On Roll'
									},
									type: 'Bar',
									dataSource: data,
									//primaryYAxis: { labelPosition: "inside" },
									legend: { title: { font: { fontFamily: "Algerian" } } },
									xName: "OperationName",
									yName: "OnRollBalance",
									enableAnimation: true,
									tooltip: {
										visible: true
									},
									marker: {
										visible: true,
									}



								}],
								preRender: function (args) {
									var chart2 = $("#container").ejChart("instance");
									var length = chart2.model.series[0].points.length - 1;
									for (var i = 0; i < chart2.model.series[0].points.length; i++) {
										if (chart2.model.series[0].points[i].YValues[0] > 0)
											chart2.model.series[0].points[i].fill = "Yellow";
										else if (chart2.model.series[0].points[i].YValues[0] < 0)
											chart2.model.series[0].points[i].fill = "red";
										else
											chart2.model.series[0].points[i].fill = "Green";
									}

								},
							});

							var chart2 = $("#container").ejChart("instance");
							chart2.model.size.height = 100 + (data.length * 16);
							chart2.model.negativeColor = "Green";
							chart2.redraw();
							$("#container1").ejChart({
								plotOptions: {
									columnrange: {
										negativeColor: 'Green',
										threshold: 0,
										dataLabels: {
											enabled: true,
											formatter: function () {

											}
										}
									}
								},
								legend: {
									//Visible chart legend
									visible: false
								},
								primaryXAxis: {
									//range: {
									//    min: 0,
									//    max: 50,
									//    interval: 1
									//}
									LabelFormat: "0"
								},
								canResize: true,
								//size: { width: '600', height: '3000' },
								isResponsive: true,
								//refreshContent: true,
								series: [{
									title: {
										//Add chart title
										text: 'Present'
									},
									//type: 'column',
									type: 'Bar',
									dataSource: data,
									//primaryYAxis: { labelPosition: "inside" },
									legend: { title: { font: { fontFamily: "Algerian" } } },
									xName: "OperationName",
									yName: "PresentBalance",
									enableAnimation: true,
									tooltip: {
										visible: true
									},
									marker: {
										visible: true,
									}


								}],
								preRender: function (args) {
									var chart3 = $("#container1").ejChart("instance");
									var length = chart3.model.series[0].points.length - 1;
									for (var i = 0; i < chart3.model.series[0].points.length; i++) {
										if (chart3.model.series[0].points[i].YValues[0] > 0)
											chart3.model.series[0].points[i].fill = "Yellow";
										else if (chart3.model.series[0].points[i].YValues[0] < 0)
											chart3.model.series[0].points[i].fill = "red";
										else
											chart3.model.series[0].points[i].fill = "Green";
									}

								},
							});

							var chart3 = $("#container1").ejChart("instance");

							chart3.model.size.height = 100 + (data.length * 16);
							chart3.model.negativeColor = "Green";
							chart3.redraw();
						}
					});


				}

				//#endregion
			}//,

		});
	}
	function rowSelected(args) {
		debugger;

		//clearTimeout(this.clickTimer);
		this.preventClick = true;
		//alert("double click");
		//var selectedrowindex = this.selectedRowsIndexes;  // get selected row indexes    
		//$scope.operationCode = "''";
		$scope.operationCode = args.data.OperationCode;
		$scope.GetEntiryWiseData($scope.operationCode);
	}

	//$("#Refresh").click(function () {
	$scope.RefreshData = function () {



		var gridObj1 = $("#DetailGrid").ejGrid("instance");
		gridObj1.clearFiltering();
		gridObj1.refreshContent(); // Refreshes the grid contents only 
		gridObj1.refreshContent(true); // Refreshes the template and grid contents 
		var obj = $("#Grid2").ejGrid("instance");
		obj.clearFiltering();
		obj.refreshContent(); // Refreshes the grid contents only 
		obj.refreshContent(true); // Refreshes the template and grid contents 
		$("#onroll").hide();
		$("#present").hide();
		// Create grid object.
	}

	$scope.listDEtails = [];
	$scope.GetEntiryWiseData = function (operationCode) {
		var obj = $("#Grid2").ejGrid("instance");
		var sd = obj.getFilteredRecords();
		if (sd.length == 0) {
			sd = obj.model.dataSource;
		}
		var arr = [];
		var queryString = [];
		var arrqueryStringOperationCode = [];
		var queryString = "''";

		for (var i = 0; i < sd.length; i++) {
			var x = sd[i];

			var yEntityName = x["EntityId"];
			var yOperationCode = x["OperationCode"];

			if (!arr.includes(yEntityName)) {
				queryString += ",'" + yEntityName + "'";
				arr.push(yEntityName);
			}

			//if (!arrqueryStringOperationCode.includes(yOperationCode)) {
			//	queryStringOperationCode += ",'" + yOperationCode + "'";
			//	arrqueryStringOperationCode.push(yOperationCode);
			//}	

		}
		//$http({
		//	type: "POST",
		//	url: 'Employees/SkillMatrix/GetEntiryWiseData',
		//	data: {
		//			'queryString': queryString,
		//		'queryStringOperationCode': operationCode 				
		//	},
		//	dataType: "json",
		//	success: function (data) {

		//		$scope.listDEtails = data;
		//		$scope.RequisitionList1();
		//	}

		//});
		$http({
			method: 'POST',
			url: 'Employees/SkillMatrix/GetEntiryWiseData',
			data: {
				'queryString': queryString,
				'queryStringOperationCode': operationCode
			},
		}).then(function successCallback(response) {
			$scope.listDEtails = response.data;
			$scope.RequisitionList1();
		});
		//angular.element(document.querySelector('#ListOfRequisition1')).modal('show');
	}

	$scope.RequisitionList1 = function () {
		// $scope.Action1 = 'Save';
		//$scope.listDEtails;
		angular.element(document.querySelector('#ListOfRequisition1')).modal('show');
	};
	$scope.RequisitionListtHide1 = function () {
		$scope.taxCategoryList = [];
		angular.element(document.querySelector('#ListOfRequisition1')).modal('hide');
	};


	$window.onresize = function (event) {

		$scope.PendingReqScrollbar();

	};
	$scope.PendingReqScrollbar = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#Griddetail").ejGrid("instance");
				var scrollerwidth = $("#Display").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 5, height: 300 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};




	$scope.tab = 1;
	$scope.setTaba = function (newTab) {
		$scope.tab = newTab;
	};
	$scope.isSeta = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabb = function (newTab) {
		$scope.tab = newTab;
	};
	$scope.isSetb = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.SummaryList = [];
	$scope.SkillMatSummary = function () {
		// $(".loaderr").show(2000);
		// debugger;
		// $('#load').show();
		var obj = $("#Grid2").ejGrid("instance");
		var sd = obj.getFilteredRecords();
		if (sd.length == 0) {
			sd = obj.model.dataSource;
			//alert('1' +1);
		}
		var arr = [];
		var queryString = [];
		queryStringForSum = [];
		var queryString1 = [];
		var arrqueryStringProcess = [];
		var arrqueryStringSkill = [];
		var arrqueryStringGrouping = [];
		var arrqueryStringMachineCategory = [];
		var arrqueryStringMachineSubCategoryCode = [];
		var arrqueryStringCaption = [];
		var arrqueryStringOperationCode = [];
		var arrqueryStringOperationCategoryId = [];
		var arrqueryStringOnRoll = [];
		var arrqueryStringTotalPresent = [];
		var arrqueryStringOnRollShort = [];
		var arrqueryStringOnRollExcess = [];
		var arrqueryStringPresentShort = [];
		var arrqueryStringPresentExcess = [];
		// var queryStringSkillary = new Array(400)

		var value = "";
		var queryString = "''";
		$scope.queryString1 = "''";
		var queryStringCaption = "''";
		var queryStringProcess = "''";
		var queryStringSkill = "''";
		var queryStringOperationCode = "''";

		var queryStringGrouping = "''";
		var queryStringMachineCategory = "''";
		var queryStringMachineSubCategoryCode = "''";
		var queryStringCaption = "''";
		var queryStringOperationCategoryId = "''";

		var queryStringOnRoll = "''";
		var queryStringTotalPresent = "0";
		var queryStringOnRollShort = "0";
		var queryStringOnRollExcess = "0";
		var queryStringPresentShort = "0";
		var queryStringPresentExcess = "0";
		var skillList = [];
		var index = 0;
		for (var i = 0; i < sd.length; i++) {
			var x = sd[i];
			//var yEntityName = x["EntityName"];
			//var yposition = x["Caption"];
			//var yProcess = x["Process"];
			//var ySkill = x["Skill"];
			//var ySkillId = x["SkillId"];
			//var yGrouping = x["Grouping"];
			//var yMachineCategory = x["MachineCategory"];
			//var yMachineSubCategoryCode = x["MachineSubCategoryCode"];
			//var yCaption = x["MachineSubCategoryCode"];
			var yOnRoll = x["OnRoll"];
			var yTotalPresent = x["TotalPresent"];
			var yRollShort = x["RollShort"];
			var yOnRollExcess = x["OnRollExcess"];
			var yPresentShort = x["PresentShort"];
			var yPresentExcess = x["PresentExcess"];

			var yEntityName = x["EntityId"];
			var yposition = x["Position"];
			var yProcess = x["ProcessId"];
			var ySkill = x["SkillId"];
			var yOperationCode = x["OperationCode"];

			//var ySkillId = x["SkillId"];
			var yGrouping = x["SkillGroupId"];

			var yMachineCategory = x["MachineCategoryId"];
			var yMachineSubCategoryCode = x["MachineSubCategoryId"];
			var yOperationCategoryId = x["OperationCategoryId"];

			//var yCaption = x["MachineSubCategoryCode"];
			var yOnRoll = x["OnRoll"];
			var yTotalPresent = x["TotalPresent"];
			var yRollShort = x["OnRollShort"];
			var yOnRollExcess = x["OnRollExcess"];
			var yPresentShort = x["PresentShort"];
			var yPresentExcess = x["PresentExcess"];


			if (!arr.includes(yEntityName)) {
				queryString += ",'" + yEntityName + "'";
				queryStringForSum += ",'" + yEntityName + "'";

				queryString1 += ",'" + yEntityName + "'";
				arr.push(yEntityName);

			}
			if (!arrqueryStringProcess.includes(yProcess)) {
				queryStringProcess += ",'" + yProcess + "'";
				arrqueryStringProcess.push(yProcess);
			}

			if (!arrqueryStringSkill.includes(ySkill)) {
				queryStringSkill += ",'" + ySkill + "'";
				arrqueryStringSkill.push(ySkill);

			}
			if (!arrqueryStringOperationCode.includes(yOperationCode)) {
				queryStringOperationCode += ",'" + yOperationCode + "'";
				arrqueryStringOperationCode.push(yOperationCode);

			}

			if (!arrqueryStringGrouping.includes(yGrouping)) {
				queryStringGrouping += ",'" + yGrouping + "'";
				arrqueryStringGrouping.push(yGrouping);
			}
			if (!arrqueryStringMachineCategory.includes(yMachineCategory)) {
				queryStringMachineCategory += ",'" + yMachineCategory + "'";
				arrqueryStringMachineCategory.push(yMachineCategory);
			}
			if (!arrqueryStringMachineSubCategoryCode.includes(yMachineSubCategoryCode)) {
				queryStringMachineSubCategoryCode += ",'" + yMachineSubCategoryCode + "'";
				arrqueryStringMachineSubCategoryCode.push(yMachineSubCategoryCode);
			}
			if (!arrqueryStringCaption.includes(yposition)) {
				queryStringCaption += ",'" + yposition + "'";
				arrqueryStringCaption.push(yposition);
			}
			if (!arrqueryStringOperationCategoryId.includes(yOperationCategoryId)) {
				queryStringOperationCategoryId += ",'" + yOperationCategoryId + "'";
				arrqueryStringOperationCategoryId.push(yOperationCategoryId);
			}


			//if (!arr.includes(yCaption)) {
			//     queryStringCaption += ",'" + yCaption + "'";
			//     arr.push(yCaption);
			// }
			if (!arrqueryStringOnRoll.includes(yOnRoll)) {
				queryStringOnRoll += ",'" + yOnRoll + "'";
				arrqueryStringOnRoll.push(yOnRoll);
			}

			if (!arrqueryStringTotalPresent.includes(yTotalPresent)) {
				queryStringTotalPresent += "," + yTotalPresent + "";
				arrqueryStringTotalPresent.push(yTotalPresent);
			}
			if (!arrqueryStringOnRollShort.includes(yRollShort)) {
				queryStringOnRollShort += "," + yRollShort + "";
				arrqueryStringOnRollShort.push(yRollShort);
			}
			if (!arrqueryStringOnRollExcess.includes(yOnRollExcess)) {
				queryStringOnRollExcess += "," + yOnRollExcess + "";
				arrqueryStringOnRollExcess.push(yOnRollExcess);
			}
			if (!arrqueryStringPresentShort.includes(yPresentShort)) {
				queryStringPresentShort += "," + yPresentShort + "";
				arrqueryStringPresentShort.push(yPresentShort);
			}
			if (!arrqueryStringPresentExcess.includes(yPresentExcess)) {
				queryStringPresentExcess += "," + yPresentExcess + "";
				arrqueryStringPresentExcess.push(yPresentExcess);
			}

		}
		$http({
			method: 'POST',
			url: 'Employees/SkillMatrix/GetSkillMasterDetailsSummary?queryString=' + queryString
		}).then(function successCallback(response) {
			$scope.SummaryList = response.data;

		});








	}

	$scope.lst = [];
	$scope.Desigmnation = function () {
		debugger;
		$http({
			method: 'POST',
			url: 'Employees/SkillMatrix/Designation'
		}).then(function successCallback(response) {
			$scope.lst = response.data;
			window.lst = response.data;

		});
	}
	$scope.Desigmnation();


	$scope.data1 = $scope.lst;
	$scope.detailTemp = "#tabGridContents";
	//$scope.detailgrid = "detailGridData(e)";
	$scope.detailgrid1 = function detailGridData(e) {
		debugger;
		var filteredData = e.data["OperationCode"];
		var EntityIdfiltered = e.data["EntityId"];
		var data123 = []; //ej.DataManager(window.lst).executeLocal(ej.Query().where("OperationCode", "equal", parseInt(filteredData), true).take(10000));
		for (var i = 0; i < window.lst.length; i++) {
			if (window.lst[i].OperationCode === filteredData && window.lst[i].EntityId === EntityIdfiltered)
				data123.push(window.lst[i]);
		}

		e.detailsElement.find("#detailGridSGrid").ejGrid({

			dataSource: data123,
			//columns: ["MaterialGroupName", "MaterialName", "ArticleName", "SKU1", "SKU2", "SKU3","MaterialDetail", "TransactionQty", "TransactionUoM", "EstimatedRate", "CurrencyName", "TotalAmount" ]
			//
			columns: [{ field: "EntityName", headerText: "Entity Name", width: 150 }
				, { field: "OperationCode", headerText: "Operation Code", width: 150 }
				, { field: "LegalDesignation", headerText: "Legal Designation", width: 150 }
				//, { field: "ManpowerBudget", headerText: "M.Budget", width: 150 }
				, { field: "OnRoll", headerText: "On Roll", width: 100 }
				//, { field: "OnRollShort", headerText: "On Roll Short", width: 150 }
				//, { field: "OnRollExcess", headerText: "On Roll Excess", width: 150 }
				, { field: "TotalPresent", headerText: "TotalPresent", width: 50 }
				//, { field: "PresentShort", headerText: "Present Short", width: 50 }
				//, { field: "PresentExcess", headerText: "Present Excess", width: 50 }
			]
		});
		e.detailsElement.find(".tabcontrol").ejTab();
	}



	$scope.onClickReportDownloadWord = function (args) {

		var obj = $("#Grid2").ejGrid("instance");
		var sd = obj.getFilteredRecords();
		if (sd.length == 0) {
			sd = obj.model.dataSource;
		}
		var arr = [];
		var queryString = [];
		var arrqueryStringOperationCode = [];
		var queryString = "''";

		for (var i = 0; i < sd.length; i++) {
			var x = sd[i];

			var yEntityName = x["EntityId"];
			var yOperationCode = x["OperationCode"];

			if (!arr.includes(yEntityName)) {
				queryString += ",'" + yEntityName + "'";
				arr.push(yEntityName);
			}
			var reportFormat = "Excel";
			var IsTaxApplicable = false;
			//if (baseService.isUndefinedOrNull(data.GRNId)) return ShowResult('No Id found', 'failure');
			//$window.open('Employees/SkillMatrix/MatrixReport?reportFormat=' + reportFormat + '&queryString=' + queryString + '&employeeId=' + 1 + '&isReversCharge=' + IsTaxApplicable, '_blank');
			$window.open('Employees/SkillMatrix/MatrixReport?reportFormat=' + reportFormat + '&queryString=' + queryString, '_blank');
			//location.href = "Accounts/InventoryPayable/PabyableJournal?inventoryReceiveId=" + data.GrnId;
		};
    }





    $scope.GetOperationWiseInfo = function (reportType) {        try {            var reportType = 'EXCEL';                $http({                    method: 'POST',                    url: 'Employees/skillMatrix/GetOperationWiseInformation',                    data: {                        'yearId': $scope.yearId                    }                }).then(function successCallback(response) {                    if (response.data.Error === true) {                        ShowResult(response.data.Message, 'failure');                    }                    else {                        if (reportType === 'EXCEL') {                            $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);                        }                        if (reportType === 'PDF') {                            $rootScope.report($scope.downloadgriddataPDFUrl + "?FileName=" + response.data.FileName);                        }                    }                });                   } catch (e) {            ShowResult(e, 'failure');        }    };
}