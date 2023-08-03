'use strict';
IssueSlipController.$inject = ['addressService', '$window',  'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function IssueSlipController(addressService, $window,  cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
	
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


    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/GoodsReceiveNote/IssueRequestReport?issueId=" + data.Id;

    };
	$scope.IssueDetailData = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GoodsReceiveNote/IssueDetailData'
		}).then(function successCallback(response) {
			$scope.lstIssueDetailData = response.data;
			//$scope.detailgrid($scope.lst);
			window.lstIssueDetailData = response.data;

		});
	}
	$scope.IssueDetailData();
	


	$scope.FilterList123 = [];

	$scope.data1 = $scope.lst;
	$scope.detailTemp = "#tabGridContents";
	//$scope.detailgrid = "detailGridData(e)";
	$scope.detailgridIssue = function detailGridData(e) {
		//debugger;

		var filteredData = e.data["Id"];
		var data = ej.DataManager(window.lstIssueDetailData).executeLocal(ej.Query().where("IssueMasterId", "equal", parseInt(filteredData), true).take(100));
		e.detailsElement.find("#detailGrid").ejGrid({

			dataSource: data,
			columns: ["EntityName", "CostCenterName", "ExpenceActivityCode", "Activity", "MaterialType", "MaterialMasterGroupName", "MaterialMasterName", "ArticleId", "RequisitionNo", "RequisitionDetailId", "DepartmentName", "AddedBy", "ApprovedQty", "RejectionQty", "TotalQty", "IssuedQty"]
		});
		e.detailsElement.find(".tabcontrol").ejTab();
	}




	$scope.lst = [];
	$scope.IssueSlipDetail = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GoodsReceiveNote/IssueSlipDetail'
		}).then(function successCallback(response) {
			$scope.lst = response.data;
			//$scope.detailgrid($scope.lst);
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
	$scope.IssueSlipList = [];
	$scope.Griddata = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GoodsReceiveNote/IssueListData'
		}).then(function successCallback(response) {
			
			$scope.IssueSlipList = response.data;
		});
	}
	//$scope.Griddata();



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



	$scope.recorddoubleclick = function ($event) {
		//debugger;
		var x = $event;
		$scope.OMId = x.data.Id;
		$scope.modelNew.OperationMasterIdID = x.data.Id;
		$scope.GetDataByMasterOrderIdfn($scope.OMId);
		$scope.GetDataByMasterOrderIdfnMP1($scope.OMId);
		$scope.GetOperationPositionMPBudget();
		$scope.GetAutoSequenceForManPower();
		$scope.Action = 'Update';
		//$scope.Action1 = 'Update';       
		if (!$rootScope.isCollapsed) $rootScope.toggle();
	};

	$scope.GetDataByMasterOrderIdfn = function (OMId) {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GoodsReceiveNote/IssueListById?id=' + OMId
		}).then(function successCallback(response) {

			$scope.modelNew = response.data[0];
			$scope.modelNew.OperationMasterIdID = response.data[0].Id;

		});
	}





	$scope.IssueRequestReportprint = function (args) {
		//debugger;
		var gridObj = $("#Grid123").data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		//alert('jj' + data.Id);
		// $scope.valuePassInDelModal(data); 
		location.href = "Products/GoodsReceiveNote/IssueRequestReport?issueId=" + data.Id;

	};
	$scope.IssueRequestReport = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",

			click: $scope.IssueRequestReportprint
		}
	}];

	$scope.IssueRequestReportApprovedList = function (args) {
		//debugger;
		var gridObj = $("#Grid456").data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		//alert('jj' + data.Id);
		// $scope.valuePassInDelModal(data); 
		location.href = "Products/GoodsReceiveNote/IssueRequestReport?issueId=" + data.Id;

	};
	$scope.IssueRequestReportApproved = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",

			click: $scope.IssueRequestReportApprovedList
		}
	}];


	$scope.IssueWithReqPOGRNReportprint = function (args) {
		//debugger
		var gridObj = $("#Grid").data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		//alert('jj' + data.Id);
		// $scope.valuePassInDelModal(data); 
		location.href = "Products/GoodsReceiveNote/IssueWithReqPOGRNReport?Id=" + data.Id;

	};
	$scope.IssueReqPOGRNReport = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",

			click: $scope.IssueWithReqPOGRNReportprint
		}
	}];


	$scope.Resignation = {
		Id: null,
		ResignationDate: null,
		Reason: null,
		Image: null,
		imageSrc: null,
		AttachLetter: null,
		ApprovedDate: null,
		EffectiveDate: null,
		ApprovedEffectiveDate: null,
		Remarks: null,
		EmployeeId: null,
		PlantId: null,
		CompanyId: null,
		EmployeeName: null,
		EmployeeCode: null,
		Designation: null,
		Picture: null,
		GivenDesignation: null,
		EmployeeCategory: null,
		DOJ: null,
		DOC: null,
		IsPastResignationAllowed: false,
		PastResignationDaysAllowed: null,
		EmpPicPath: null,
		ApprovalStatus: null,
		Entity: null
	};

	

	$scope.loadNewEmployee = function () {
		$scope.excluedEmpColumn = ['Email', 'Reason', 'position', 'ResignationDate', 'AttachLetter', 'ApprovalStatus', 'EffectiveDate', 'Picture', 'IsPastResignationAllowed', 'PastResignationDaysAllowed', 'EmployeeCategory'];
		$scope.popUpParameters = {
			limit: 10,
			offset: 0,
			order: 'asc',
			sort: 'EmployeeCode',
			searchBy: 'EmployeeCode',
			pageSize: 10,
			total_count: 0,
			search: null,
			serverPagination: true
		};
		$scope.popUpUrl = 'employees/resignation/newList?plantId=' + $scope.Resignation.PlantId;
		baseService.setCurrentPage('dataList');
		$scope.getPopUpData = function (pageno) {
			baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
				.then(function (result) {
					$scope.popUpDataList = result.Rows;
					$scope.popUpParameters.total_count = result.Total;

					if (baseService.arrayLength($scope.popUpList) === 0)
						baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
				}, function () {
					ShowResult(commonMessage.NetworkError, 'failure', 'popUpId1');
				}).finally(function () {
				});
		};
		angular.element(document.querySelector('#popUpId1')).modal('show');
		$scope.getPopUpData();
	};
	$scope.loadPendingEmployee = function () {
		$scope.excluedEmpColumn = ['Email', 'Reason', 'position', 'ResignationDate', 'AttachLetter', 'ApprovalStatus', 'EffectiveDate', 'Picture', 'IsPastResignationAllowed', 'PastResignationDaysAllowed', 'EmployeeCategory'];
		$scope.popUpParameters = {
			limit: 10,
			offset: 0,
			order: 'asc',
			sort: 'EmployeeCode',
			searchBy: 'EmployeeCode',
			pageSize: 10,
			total_count: 0,
			search: null,
			serverPagination: true
		};
		$scope.popUpUrl = 'employees/resignation/pendingList?plantId=' + $scope.Resignation.PlantId;
		baseService.setCurrentPage('dataList');
		$scope.getPopUpData = function (pageno) {
			baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
				.then(function (result) {
					$scope.popUpDataList = result.Rows;
					$scope.popUpParameters.total_count = result.Total;

					if (baseService.arrayLength($scope.popUpList) === 0)
						baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
				}, function () {
					ShowResult(commonMessage.NetworkError, 'failure', 'popUpId2');
				}).finally(function () {
				});
		};
		angular.element(document.querySelector('#popUpId2')).modal('show');
		$scope.getPopUpData();
	};
	$scope.closePopUp = function () {
		$scope.valueData = '';
		angular.element(document.querySelector('#popUpId1')).modal('hide');
	};
	$scope.closePopUp2 = function () {
		$scope.valueData = '';
		angular.element(document.querySelector('#popUpId2')).modal('hide');
	};

	function selectNewEmployee(data) {
		$scope.Resignation.Id = data.Id;
		$scope.imageSrc = virtualPath.EmployeePic + data.Picture;
		$scope.Resignation.EmployeeId = data.EmployeeId;
		$scope.Resignation.EmployeeName = data.EmployeeName;
		$scope.Resignation.EmployeeCode = data.EmployeeCode;
		$scope.Resignation.GivenDesignation = data.GivenDesignation;
		$scope.Resignation.Designation = data.Designation;
		$scope.Resignation.DOJ = data.DOJ;
		$scope.Resignation.DOC = data.DOC;
		$scope.Resignation.EmployeeCategory = data.EmployeeCategory;
		$scope.Resignation.PlantId = data.PlantId;
		$scope.Resignation.EmployeeCategory = data.EmployeeCategory;
		$scope.Resignation.Id = data.Id;
		$scope.Resignation.IsPastResignationAllowed = data.IsPastResignationAllowed;
		$scope.Resignation.PastResignationDaysAllowed = data.PastResignationDaysAllowed;
		$scope.Resignation.Entity = data.Entity;
		$scope.Resignation.AttachLetter = data.AttachLetter;
		$scope.closePopUp();
		$scope.Action = 'Save';
	}
	function selectPendingEmployee(data) {
		$scope.Resignation.Id = data.Id;
		$scope.imageSrc = virtualPath.EmployeePic + data.Picture;
		$scope.Resignation.EmployeeId = data.EmployeeId;
		$scope.Resignation.EmployeeName = data.EmployeeName;
		$scope.Resignation.EmployeeCode = data.EmployeeCode;
		$scope.Resignation.GivenDesignation = data.GivenDesignation;
		$scope.Resignation.Designation = data.Designation;
		$scope.Resignation.DOJ = data.DOJ;
		$scope.Resignation.DOC = data.DOC;
		$scope.Resignation.EmployeeCategory = data.EmployeeCategory;
		$scope.Resignation.PlantId = data.PlantId;
		$scope.Resignation.ResignationDate = data.ResignationDate;
		$scope.Resignation.EffectiveDate = data.EffectiveDate;
		$scope.Resignation.EmployeeCategory = data.EmployeeCategory;
		$scope.Resignation.Reason = data.Reason;
		$scope.Resignation.Id = data.Id;
		$scope.Resignation.IsPastResignationAllowed = data.IsPastResignationAllowed;
		$scope.Resignation.PastResignationDaysAllowed = data.PastResignationDaysAllowed;
		$scope.Resignation.Entity = data.Entity;
		$scope.Resignation.AttachLetter = data.AttachLetter;
		document.getElementById('abc').value = data.AttachLetter;
		$scope.closePopUp2();
		$scope.Action = 'Update';
	}
	$scope.loadResignationHistory = function (Id) {
		$http.get('employees/resignation/getResignationHistoryById?EmployeeId=' + $scope.Resignation.EmployeeId)
			.then(function (response) {
				$scope.entityList = response.data;
			});
		angular.element(document.querySelector('#ResignationHistoryPopUp')).modal('show');
	};

	function CheckField(fieldValue, fieldName) {
		try {
			if (fieldValue === null || fieldValue === '') {
				throw '[' + fieldName + '] is required...';
			}
		} catch (e) {
			throw e;
		}
	}
	function Validate() {
		try {
			CheckField($scope.Resignation.PlantId, 'Plant');
			CheckField($scope.Resignation.EmployeeName, 'Employee Name');
			CheckField($scope.Resignation.ResignationDate, 'Resignation Submission Date');
			CheckField($scope.Resignation.EffectiveDate, 'Applied Effective Date');
			CheckField($scope.Resignation.Reason, 'Reason');
			CheckField($scope.Resignation.AttachLetter, 'Resignation Letter');
			var regDate = new Date($scope.Resignation.ResignationDate);
			var effDate = new Date($scope.Resignation.EffectiveDate);
			var dojDate = new Date($scope.Resignation.DOJ);

			if (dojDate > regDate) {
				throw 'Resignation date must be greater than Date of Join'
			}
			if (regDate > effDate) {
				throw 'Applied Effective date cannot be less than Resignation date'
			}

			var d = new Date();
			var d1 = $filter('date')(d, 'dd-MMM-yy');
			var d3 = $filter('date')(regDate, 'dd-MMM-yy');
			var resignationDate = new Date(d3);
			var today = new Date(d1);
			if (resignationDate > today) {
				throw 'Future Resignation date is not allowed';
			}

			var effDate2 = $filter('date')(effDate, 'dd-MMM-yy')
			var effectiveDate = new Date(effDate2);

			d.setDate(d.getDate() + 90);
			var d1 = $filter('date')(d, 'dd-MMM-yy');
			var d2 = new Date(d1);
			if (effDate > d2) {
				throw 'Applied Effective Date Cannot be Greater then [' + d1 + ']'
			}

			var allowDays = new Date();
			allowDays.setDate(d.getDate() - $scope.Resignation.PastResignationDaysAllowed);
			var d7 = $filter('date')(allowDays, 'dd-MMM-yy');
			var d8 = new Date(d7);
			if ($scope.Resignation.IsPastResignationAllowed === true) {
				if (d8 > regDate) {
					throw 'Past Resignation date before [' + d7 + '] Days is not allowed';
				}
			}
		} catch (e) {
			throw e;
		}
	}

	$scope.showSearch = function (flag) {
		try {
			$scope.search_flag = flag;
			switch (flag) {
				case 'PendingEMP':
					CheckField($scope.Resignation.PlantId, 'Plant');
					$scope.loadPendingEmployee();
					break;
				case 'NewEMP':
					CheckField($scope.Resignation.PlantId, 'Plant');
					$scope.loadNewEmployee();
					break;
				default:
					return ShowResult('Search Flag is not defined!!!', 'failure');
			}
			//angular.element(document.querySelector('#popUpId')).modal('show');
		} catch (e) {
			ShowResult(e, 'failure');
		}
	};
	$scope.getSearchObject = function (ob) {
		try {
			switch ($scope.search_flag) {
				case 'PendingEMP':
					selectPendingEmployee(ob);
					break;
				case 'NewEMP':
					selectNewEmployee(ob);
					break;
				default:
			}
			$scope.search_flag = '';
			//angular.element(document.querySelector('#search_popup')).modal('hide');
		} catch (e) {
			ShowResult(e, 'failure');
		}
	};

	$scope.Clear = function () {
		ClearOb($scope.Resignation);
		$scope.Action = 'Save';
		$scope.filedata = null;
		document.getElementById('abc').value = null;
		$scope.Resignation.AttachLetter = null;
		$scope.imageSrc = virtualPath.EmployeePic + '';
	};
	function ClearOb(obj) {
		for (var i in obj) {
			obj[i] = null;
		}
	}
	$('#uploadBtn').change(function () {
		$scope.filedata = this.files[0];
		$scope.Resignation.AttachLetter = null;
		$scope.Resignation.AttachLetter = $scope.filedata.name;
		document.getElementById('abc').value = $scope.filedata.name;
	});
	$scope.AttachRemove = function () {
		// $scope.message_confirmation = 'Are you sure to remove this file?';
		// angular.element(document.querySelector('#confirmDelete')).modal('show');
		$scope.filedata = [];
		document.getElementById('uploadBtn').value = null;
		$scope.Resignation.AttachLetter = null;
		document.getElementById('abc').value = '';
	};

	$scope.LeaveTest = function () {
		try {
			$http({
				method: 'POST',
				url: 'employees/resignation/leaveSummary?CompanyGroupId=CG20181',
			}).then(function successCallback(response) {
				if (response.data.Error === true) {
					ShowResult(response.data.Message, 'failure');
				}
				else {
					ShowResult(response.data.Message, 'success');
				}
			}, function errorCallback(response) {
				$scope.savedisable = false;
				ShowResult(response.status.Message, 'failure');
			});
			return true;
		} catch (e) {
			ShowResult(e, 'failure');
		}
	};


	//$scope.LoadData($scope.user);


	$scope.ClearFilter = function () {
		//debugger;
		var gridObj = $("#Grid2").data("ejGrid");
		gridObj.refreshContent(); // Refreshes the grid contents only 
		gridObj.refreshContent(true); // Refreshes the

		gridObj = $("#DetailGrid").data("ejGrid");
		gridObj.refreshContent(); // Refreshes the grid contents only 
		gridObj.refreshContent(true); // Refreshes the


	}

	$scope.FilterList = [];
	$scope.FilterListData = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GoodsReceiveNote/IssueSlipFilter',
		}).then(function successCallback(response) {
			$scope.FilterList = response.data;
			//$scope.detailgrid($scope.lst);
			//  window.lst = response.data;

		});
	}
	//$scope.FilterListData();




	$scope.GetEntity = function () {
		//debugger;
		$.ajax({
			type: "GET",
			contentType: "application/json; charset=utf-8",
			url: 'Products/GoodsReceiveNote/IssueSlipFilter',
			data: {},
			async: false,
			dataType: "json",
			success: function (data) {
				//$scope.FilterList = data;
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
					scrollSettings: { width: "auto", height: "2" },

					columns: [
						{ headerText: "Entity Name", field: "EntityName", width: 60 },
						{ headerText: "ActivityName", field: "UserName", width: 80 },
						{ headerText: "Material Type", field: "MaterialType", width: 60 },
						{ headerText: "Group Name", field: "MaterialMasterGroupName", width: 90 },
						//{ headerText: "Group Name2", field: "MaterialMasterGroupName", width: 90 },
						{ headerText: "Material", field: "MaterialMasterName", width: 90 },
						{ headerText: "Article", field: "StandardName", width: 80 },
						{ headerText: "Sku1", field: "FirstCharacteristicsValue", width: 90 },
						{ headerText: "Sku2", field: "SecondCharacteristicsValue", width: 90 },
						{ headerText: "Sku3", field: "ThirdCharacteristicsValue", width: 90 },


						
						{ headerText: "Requisition By", field: "AddedBy", width: 85 },
						{ headerText: "RequisitionNo", field: "RequisitionNo", width: 95 },
						{ headerText: "Requisition Detail No", field: "RequisitionDetailNo", width: 95 },

						
						{ headerText: "Department Name", field: "DepartmentName", width: 80 }


					]
				});

				$("#Grid2").children('.e-pager.e-js.e-pager').hide();
				$("#Grid2").children('.e-gridcontent.e-droppable.e-js').hide();
				$("#Grid2").children('.e-gridcontent').hide();
				//$("#Grid2").children('.e-grid .e-headercell {background - color: chocolate;}').add();

				$("#Grid2").children('.e-grid.e-headercell').css('background-color', 'red'); //{background - color: chocolate;}').add();
			}

		});
	}
	$scope.GetEntity();

	$scope.getData = function () {
		//debugger;

		var obj = $("#Grid2").ejGrid("instance");
		var sd = obj.getFilteredRecords();
		if (sd.length == 0) {
			sd = obj.model.dataSource;
			//alert('1' +1);
		}
		$scope.FilterList1 = sd;
	}


	$scope.materialValidation = function () {
		//var getRow = $filter("filter")($scope.inventoryMaterialList, { "MaterialMasterId": $scope.detailModel.MaterialMasterId });
		//var getRow2 = $filter("filter")($scope.inventoryMaterialList, { "MaterialMasterId": $scope.detailModel.MaterialMasterId, "ArticleId": $scope.detailModel.ArticleId });
		//var getRow3 = $filter("filter")($scope.inventoryMaterialList, { "MaterialMasterId": $scope.detailModel.MaterialMasterId, "ArticleId": $scope.detailModel.ArticleId, "FirstCharacteristicsValueId": $scope.detailModel.FirstCharacteristicsValueId });
		//getRow == 0 || getRow2 == 0 ||
		//if (getRow3 == 0) {
		$scope.invalid = true;
		// }
		//else {
		//ShowResult('Material Combination Already Exist');
		// $scope.invalid = false;
		// }

	}
	$scope.detailSave = function () {

		//debugger;

		try {

			//$scope.GetListForMasterOrdernew = [];
			for (var i = 0; i < $scope.FilterList1.length; i++) {
				if ($scope.FilterList1[i].RequestedQty === 0) {
					ShowResult('Enter the Requested Qty', 'failure');
					return false;
				}
				//else if ($scope.FilterList1[i].RejectedQty === 0) {
				//	ShowResult('Enter Rejection Qty', 'failure');
				//	return false;
				//}
				else if ($scope.FilterList1[i].RequestedQty > $scope.FilterList1[i].ApprovedQty) {
					ShowResult('Requested Qty can not grater than Own Qty', 'failure');
					return false;
				}
				//else if ($scope.FilterList1[i].RejectedQty > $scope.FilterList1[i].RejectionQty1) {
				//	ShowResult('Rejection Qty can not grater than Own Rejected Qty', 'failure');
				//	return false;
				//}
				//else if ($scope.FilterList1[$scope.issueSlipDetailIndex].ExpenseActivityId === "" || $scope.FilterList1[$scope.issueSlipDetailIndex].ExpenseActivityId === null || $scope.FilterList1[$scope.issueSlipDetailIndex].ExpenseActivityId === undefined) {
				//	ShowResult('Please select Expense Activity Code', 'failure');
				//	return false;
				//}

			}
			// $scope.processgroupList($scope.GetListForMasterOrdernew, $scope.groupList);
			$scope.$broadcast('show-errors-check-validity');
			if ($scope.productNewForm.$valid) {
				$scope.materialValidation();
				if ($scope.invalid) {
					if ($scope.Action1 === 'Save') {
						$http({
							method: 'POST',
							url: 'Products/GoodsReceiveNote/IssueSlipCreate',
							data: {
								entity: $scope.FilterList1
								, CheckedBy: $scope.CheckedBy
							},
							dataType: 'JSON'
						}).then(function successCallback(response) {
							if (response.data.Error === true)
								ShowResult(response.data.Message, 'failure');
							else {
								ShowResult(response.data.Message, 'success');
								//$scope.Griddata();
								getInventoryMaterialList($scope.productNew.Id);

							}
						}), function errorCallBack(response) {
							ShowResult(response.data.Message, 'failure');
						};

					}
					else if ($scope.Action1 === "Update") {
						$http({
							method: 'POST',
							url: 'Products/GoodsReceiveNote/IssueSlipUpdate',
							data: {
								entity: $scope.FilterList1
								, CheckedBy: $scope.CheckedBy
								, Id: $scope.Id
							},
							dataType: 'JSON'
						}).then(function successCallback(response) {
							if (response.data.Error === true)
								ShowResult(response.data.Message, 'failure');
							else {
								ShowResult(response.data.Message, 'success');
								$scope.Griddata();
								getInventoryMaterialList($scope.productNew.Id);

							}
						}), function errorCallBack(response) {
							ShowResult(response.data.Message, 'failure');
						};

					}
				}

			}



		} catch (e) {
			//ShowResult(e, 'fail', 'detailPopUp');
		}
	};
	$scope.detailSaveIssue = function () {

		//debugger;

		try {

			//$scope.GetListForMasterOrdernew = [];
			for (var i = 0; i < $scope.FilterList123.length; i++) {
				if ($scope.FilterList123[i].RequestedQty === 0) {
					ShowResult('Enter the Requested Qty', 'failure');
					return false;
				}
				//else if ($scope.FilterList123[i].RejectedQty === 0) {
				//	ShowResult('Enter Rejection Qty', 'failure');
				//	return false;
				//}
				else if ($scope.FilterList123[i].RequestedQty > $scope.FilterList123[i].ApprovedQty) {
					ShowResult('Requested Qty can not grater than Own Qty', 'failure');
					return false;
				}
				//else if ($scope.FilterList123[i].RejectedQty > $scope.FilterList123[i].RejectionQty1) {
				//	ShowResult('Rejection Qty can not grater than Own Rejected Qty', 'failure');
				//	return false;
				//}
				else if ($scope.FilterList123[$scope.issueSlipDetailIndex].ExpenseActivityId === "") {
					ShowResult('Please select Expense Activity Code', 'failure');
					return false;
				}

			}
			// $scope.processgroupList($scope.GetListForMasterOrdernew, $scope.groupList);
			$scope.$broadcast('show-errors-check-validity');
			if ($scope.productNewForm.$valid) {
				$scope.materialValidation();
				if ($scope.invalid) {
					if ($scope.Action1 === 'Save') {
						$http({
							method: 'POST',
							url: 'Products/GoodsReceiveNote/IssueSlipCreate',
							data: {
								entity: $scope.FilterList123
								, CheckedBy: $scope.CheckedBy
							},
							dataType: 'JSON'
						}).then(function successCallback(response) {
							if (response.data.Error === true)
								ShowResult(response.data.Message, 'failure');
							else {
								ShowResult(response.data.Message, 'success');
								//$scope.Griddata();
								getInventoryMaterialList($scope.productNew.Id);

							}
						}), function errorCallBack(response) {
							ShowResult(response.data.Message, 'failure');
						};

					}
					else if ($scope.Action1 === "Update") {
						$http({
							method: 'POST',
							url: 'Products/GoodsReceiveNote/IssueSlipUpdate',
							data: {
								entity: $scope.FilterList1
								, Id: $scope.Id
							},
							dataType: 'JSON'
						}).then(function successCallback(response) {
							if (response.data.Error === true)
								ShowResult(response.data.Message, 'failure');
							else {
								ShowResult(response.data.Message, 'success');
								//$scope.Griddata();
								getInventoryMaterialList($scope.productNew.Id);

							}
						}), function errorCallBack(response) {
							ShowResult(response.data.Message, 'failure');
						};

					}
				}

			}



		} catch (e) {
			//ShowResult(e, 'fail', 'detailPopUp');
		}
	};




	$scope.CostCenterLoad = function () {
		cboService.getCostCenterCbo(function (result) {
			$scope.costCenterList = result;
		});
	}
	$scope.CostCenterLoad();

	//**********Expenses GL Budget Activity**************
	$scope.searchglByList = [
		{
			"name": "GL",
			"value": "GLGeneralInfoName"
		},
		{
			"name": "Budget",
			"value": "BudgetName"
		},
		{
			"name": "Activity",
			"value": "ActivityName"
		},
		{
			"name": "Ref No",
			"value": "RefNo"
		}
	];

	$scope.glListParameters = {
		limit: 10,
		offset: 0,
		order: "asc",
		sort: "GLGeneralInfoName",
		searchBy: "ActivityName",
		pageSize: 10,
		total_count: 0,
		search: null,
		serverPagination: true
	};

	$scope.popUp = function (index) {
		//debugger;
		$scope.customerInvoiceGLList = [];
		//baseService.setCurrentPage("cOAICodeList");
		$scope.GetCOAICodeListData = function (pageno) {
			baseService.paginationBase("Accounts/GLItem/GetAllGLBudgetActivityPostingAutomaticOnly", pageno, $scope.glListParameters)
				.then(function (result) {
					$scope.cOAICodeList = result.Rows;
					$scope.glListParameters.total_count = result.Total;
				}, function () {
					ShowResult(commonMessage.NetworkError, "failure", "GLPopUp");
				}).finally(function () {
				});
		};
		angular.element(document.querySelector("#GLPopUp")).modal("show");
		$scope.GetCOAICodeListData();
		$scope.issueSlipDetailIndex = index;
	};

	$scope.closeCOAICodeListPopUp = function () {
		angular.element(document.querySelector("#GLPopUp")).modal("hide");
	};

	$scope.closeCOAICodeListPopUpSelected = function (x) {
		if ($scope.rowSelected !== null) {
			angular.element(document.querySelector("#GLPopUp")).modal("hide");
		} else {
			angular.element(document.querySelector("#cancelPopUp")).modal("show");
		}
	};

	//$scope.setSelected = function (data) {
	//	//debugger;
	//	$scope.FilterList1[$scope.issueSlipDetailIndex].GLGeneralInfoId = data.GLGeneralInfoId;
	//	$scope.FilterList1[$scope.issueSlipDetailIndex].BudgetMasterId = data.BudgetMasterId;
	//	$scope.FilterList1[$scope.issueSlipDetailIndex].ExpenseActivityId = data.ActivityId;
	//	$scope.FilterList1[$scope.issueSlipDetailIndex].GLBudgetActivity = data.GLGeneralInfoCode + '-' + data.ActivityName;
	//	angular.element(document.querySelector("#GLPopUp")).modal("hide");
	//};
	$scope.setSelected = function (data) {
		//debugger;
		$scope.FilterList1[$scope.issueSlipDetailIndex].GLGeneralInfoId = data.GLGeneralInfoId;
		$scope.FilterList1[$scope.issueSlipDetailIndex].BudgetMasterId = data.BudgetMasterId;
		$scope.FilterList1[$scope.issueSlipDetailIndex].ExpenseActivityId = data.ActivityId;
		$scope.FilterList1[$scope.issueSlipDetailIndex].GLBudgetActivity = data.GLGeneralInfoCode + '-' + data.ActivityName;
		$scope.FilterList1[$scope.issueSlipDetailIndex].Activity = data.GLGeneralInfoCode + '-' + data.ActivityName;
		
		angular.element(document.querySelector("#GLPopUp")).modal("hide");
	};
	//********** End Expenses GL Budget Activity**************
	//**********To Checked By**************
	$scope.checkedByList = [];
	$scope.GetSupervisorCboList = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/PurchaseOrder/GetSupervisorCbo'
		}).then(function successCallback(response) {
			$scope.checkedByList = response.data;
		});
	}
	$scope.GetSupervisorCboList();
	//********** To Checked By**************

	$scope.tab1 = 1;
	$scope.setTabIndex = function (newTab) {
		$scope.tab1 = newTab;
		//$scope.Griddata();
	};
	$scope.isSetIndex = function (tabNum) {
		return $scope.tab1 === tabNum;
	};

	$scope.setTabIndex1 = function (newTab) {
		$scope.tab1 = newTab;
		$scope.LoadIssueSlipApproveData();
	};
	$scope.isSetIndex1 = function (tabNum) {
		return $scope.tab1 === tabNum;
	};
	function getInventoryMaterialList(inveReveiveId) {
		$scope.masterId = inveReveiveId;
		//debugger;
		$scope.inventoryMaterialList = [];
		$http.get($scope.path + 'IssueListById?Id=' + inveReveiveId)
			.then(function (response) {
				$scope.FilterList1 = response.data.Rows;
				//$scope.GLBudgetActivity = $scope.FilterList1.GLBudgetActivity;
			});

	}
	$scope.recorddoubleclickIssueSlip = function ($event) {
		//debugger;
		var x = $event;
		var Id = x.data.Id;
		$scope.productNew = x.data;
		$scope.Id = $scope.productNew.Id;
		getInventoryMaterialList($scope.productNew.Id);
		$scope.Action1 = 'Update';
		if (!$rootScope.isCollapsed) $rootScope.toggle();
	};

	//**********Issue**************

	$scope.GetFilterForIssue = function () {
		//debugger;
		$.ajax({
			type: "GET",
			contentType: "application/json; charset=utf-8",
			url: 'Products/GoodsReceiveNote/IssueFilter',
			data: {},
			async: false,
			dataType: "json",
			success: function (data) {
				//$scope.FilterList = data;
				$("#Grid3").ejGrid({

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
					scrollSettings: { width: "auto", height: "2" },

					columns: [
						{ headerText: "Entity Name", field: "EntityName", width: 60 },
						{ headerText: "ActivityName", field: "ActivityName", width: 80 },
						{ headerText: "Material Type", field: "MaterialType", width: 60 },
						{ headerText: "Group Name", field: "MaterialMasterGroupName", width: 90 },
						//{ headerText: "Group Name2", field: "MaterialMasterGroupName", width: 90 },
						{ headerText: "MaterialMasterName", field: "MaterialMasterName", width: 90 },
						{ headerText: "Article", field: "StandardName", width: 80 },
						{ headerText: "RequisitionBy", field: "AddedBy", width: 85 },
						//{ headerText: "RequisitionNo", field: "RequisitionNo", width: 95 },
						{ headerText: "RequisitionDetailId", field: "RequisitionDetailId", width: 95 },

						{ headerText: "Department Name", field: "DepartmentName", width: 80 }


					]
				});

				$("#Grid3").children('.e-pager.e-js.e-pager').hide();
				$("#Grid3").children('.e-gridcontent.e-droppable.e-js').hide();
				$("#Grid3").children('.e-gridcontent').hide();
				//$("#Grid2").children('.e-grid .e-headercell {background - color: chocolate;}').add();

				$("#Grid3").children('.e-grid.e-headercell').css('background-color', 'red'); //{background - color: chocolate;}').add();
			}

		});
	}
	$scope.GetFilterForIssue();
	//**********Issue**************









	$scope.tabIU = 1;
	$scope.IUCsetTabIndex = function (newTab) {
		//debugger;
		$scope.tabIU = newTab;
		$scope.getaldataIssueSlipUnChecked();
	};
	$scope.isIUCSetIndex = function (tabNum) {
		return $scope.tabIU === tabNum;
	};

	$scope.ICsetTabIndex = function (newTab) {
		//debugger;
		$scope.tabIU = newTab;
		$scope.getaldataIssueSlipChecked();
	};
	$scope.isICSetIndex = function (tabNum) {
		return $scope.tabIU === tabNum;
	};

	//$scope.GriddataISUnCheckedList = [];
	//$scope.getaldataIssueSlipUnChecked = function () {
	//	//debugger;
	//	$http({
	//		method: "GET",
	//		dataType: 'JSON',
	//		//url: $scope.getSearchListUrl,
	//		url: 'Products/GoodsReceiveNote/IssueSlipUnChecked',
	//	}).then(function successCallback(response) {
	//		$scope.GriddataISUnCheckedList = response.data;

	//		//entrydata = copy(searchdata);
	//	});
	//};
	//$scope.getaldataIssueSlipUnChecked();


	$scope.GriddataISCkedList = [];
	$scope.getaldataIssueSlipChecked = function () {
		//debugger;
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/GoodsReceiveNote/IssueSlipChecked',
		}).then(function successCallback(response) {
			$scope.GriddataISCkedList = response.data;

			//entrydata = copy(searchdata);
		});
	};
	$scope.getaldataIssueSlipChecked();



    $scope.IssueSlipChecked = function (data) {
        //debugger; 
        var str = $scope.podata.SystemId;
		var Status = $('#combo-default').val();
		var Id = str;//str.substring(0, str.indexOf('-'));
		if ($scope.podata.AuthorizedBy === "" || $scope.podata.AuthorizedBy === null ) {
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


	//**********ApprovingIssueSlipk**************
	$scope.checkedByList1 = [];
	$scope.GetSupervisorCboList1 = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/InventoryCheckApproved/GetSupervisorCboApproved'
		}).then(function successCallback(response) {
			$scope.checkedByList1 = response.data;
		});
	}
	$scope.GetSupervisorCboList1();



	



	$scope.poApproved = function () {
		cboService.getEnumCbo("enum/GetPOApprovalStatusCbo", function (result) {
			$scope.POApprovalList = result;
		});
	}
	$scope.poApproved();

	$scope.LoadapprovalStatus = function () {
		cboService.getEnumCbo("enum/GetCheckedStatusCbo", function (result) {
			$scope.approvalStatusList = result;
		});

	}
	$scope.LoadapprovalStatus();
	//#endregion




	$scope.tabIU = 1;
	$scope.IUCsetTabIndex = function (newTab) {
		//debugger;
		$scope.tabIU = newTab;
		$scope.getaldataIssueSlipUnApproved();
	};
	$scope.isIUCSetIndex = function (tabNum) {
		return $scope.tabIU === tabNum;
	};


    $scope.isSetholdReject = function (newTab) {
        $scope.tabIU = newTab;

        $scope.getaldataIssueSlipApproved();
    };
    $scope.IssetTabholdReject = function (tabNum) {
        return $scope.tabIU === tabNum;
    };


	$scope.ICsetTabIndex = function (newTab) {
		$scope.tabIU = newTab;

		$scope.getaldataIssueSlipApproved();
	};
	$scope.isICSetIndex = function (tabNum) {
		return $scope.tabIU === tabNum;
	};


	//$scope.GridIssueSlipUnApprovedList = [];
	//$scope.getaldataIssueSlipUnApproved = function () {
	//	//debugger;
	//	$http({
	//		method: "GET",
	//		dataType: 'JSON',
	//		//url: $scope.getSearchListUrl,
	//		url: 'Products/GoodsReceiveNote/IssueSlipUnApproved',
	//	}).then(function successCallback(response) {
	//		$scope.GridIssueSlipUnApprovedList = response.data;

	//		//entrydata = copy(searchdata);
	//	});
	//};
	//$scope.getaldataIssueSlipUnApproved();



	$scope.GridIssueSlipApprovedList = [];
	$scope.getaldataIssueSlipApproved = function () {
		//debugger;
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/GoodsReceiveNote/IssueSlipApproved',
		}).then(function successCallback(response) {
			$scope.GridIssueSlipApprovedList = response.data;

			//entrydata = copy(searchdata);
		});
	};
	$scope.getaldataIssueSlipApproved();




	$scope.IssueSlipApproved = function () {
		//var str = $('#combo-default12').val();
		//var Id = str.substring(0, str.indexOf('-'));
		if ($scope.podata.CheckedByStatus === null || $scope.podata.CheckedByStatus === "") {
			ShowResult("Please Select Checked By Status", 'failure');
			return false;
		}
		else if ($scope.podata.CheckedByStatus === "Checked" || $scope.podata.CheckedByStatus === "For Checked" || $scope.podata.CheckedByStatus === "Select") {
			ShowResult("Please Select Approved By Status", 'failure');
			return false;
		}


		//debugger;
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


    $scope.commandpoAuth = [{
		type: "details", buttonOptions: {
			text: "Save",
			width: "100",
			height: "30",
			click: $scope.onClickPOAUTH
		}
	}];
	//#endregion

	$scope.GetRequisitionIssueDetail = function (id) {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GoodsReceiveNote/GetRequisitionIssueDetail?issueId=' + id
		}).then(function successCallback(response) {
			$scope.requisitionIssueDetailList = response.data;
		});
	}

	$scope.issuedoubleclick = function ($event) {
		//debugger;
		var x = $event;
		$scope.IssueId = x.data.Id;
		
		$scope.GetRequisitionIssueDetail($scope.IssueId);
		$scope.Action1 = 'Update';
		if (!$rootScope.isCollapsed) $rootScope.toggle();
	};

	$scope.Issuelist = [];
	$scope.IssueGriddata = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GoodsReceiveNote/RequisitionIssueListData'
		}).then(function successCallback(response) {
			$scope.Issuelist = response.data;
		});
	}
	$scope.IssueGriddata();

	$scope.requisitionIssueDetailList = [];
	$scope.GetIssueDetail = function () {
		//debugger;

		var obj = $("#Grid3").ejGrid("instance");
		var sd = obj.getFilteredRecords();
		if (sd.length == 0) {
			sd = obj.model.dataSource;
			//alert('1' +1);
		}
		$scope.requisitionIssueDetailList = sd;
	}

	$scope.issueDetail = function () {
		$scope.issuetemp = [];
		$scope.issuetemp = $scope.requisitionIssueDetailList;
		$scope.issueDetailList = [];
		//var getRow3 = $filter("filter")($scope.inventoryMaterialList, { "MaterialMasterId": $scope.detailModel.MaterialMasterId, "ArticleId": $scope.detailModel.ArticleId, "FirstCharacteristicsValueId": $scope.detailModel.FirstCharacteristicsValueId });
		for (var i = 0; i < $scope.issuetemp.length; i++) {
			var getRow = $filter("filter")($scope.issueDetailList, { "MaterialMasterId": $scope.issuetemp[i].MaterialMasterId, "ArticleId": $scope.issuetemp[i].ArticleId, "FirstCharacteristicsValueId": $scope.issuetemp[i].FirstCharacteristicsValueId });
			if (getRow.length == 0) {
				$scope.issuetemp[i].IssueQty = $filter('sumByKey')($filter('filter')($scope.issuetemp, { MaterialMasterId: $scope.issuetemp[i].MaterialMasterId, ArticleId: $scope.issuetemp[i].ArticleId, FirstCharacteristicsValueId: $scope.issuetemp[i].FirstCharacteristicsValueId }), 'IssueValidQty');
				$scope.issueDetailList.push($scope.issuetemp[i]);
			}
		}
	}
	$scope.IssueSave = function () {
		//debugger;
		try {
			$scope.$broadcast('show-errors-check-validity');
			if ($scope.issueForm.$valid) {
				$scope.issueDetail();
				if ($scope.Action1 === 'Save') {
					$http({
						method: 'POST',
						url: 'Products/GoodsReceiveNote/RequisitionIssueInsert',
						data: {
							'entities': $scope.issueDetailList
							, 'specificStockList': null
							, 'requisitionIssueDetails': $scope.requisitionIssueDetailList
						},
						dataType: 'JSON'
					}).then(function successCallback(response) {
						if (response.data.Error === true)
							ShowResult(response.data.Message, 'failure');
						else {
							ShowResult(response.data.Message, 'success');
							//$scope.Griddata();
							getInventoryMaterialList($scope.productNew.Id);

						}
					}), function errorCallBack(response) {
						ShowResult(response.data.Message, 'failure');
					};

				}
				else if ($scope.Action1 === "Update") {
					$http({
						method: 'POST',
						url: 'Products/GoodsReceiveNote/RequisitionIssueUpdate',
						data: {
							'issueId': $scope.IssueId
							, 'entities': $scope.issueDetailList
							, 'specificStockList': null
							, 'requisitionIssueDetails': $scope.requisitionIssueDetailList
						},
						dataType: 'JSON'
					}).then(function successCallback(response) {
						if (response.data.Error === true)
							ShowResult(response.data.Message, 'failure');
						else {
							ShowResult(response.data.Message, 'success');
							//$scope.Griddata();
							getInventoryMaterialList($scope.productNew.Id);

						}
					}), function errorCallBack(response) {
						ShowResult(response.data.Message, 'failure');
					};

				}
			}

		} catch (e) {
			//ShowResult(e, 'fail', 'detailPopUp');
		}
	};



	// #region Material Wise Issue Slip
	$scope.GetIssueSlipFilterData = function () {
		//debugger;
		$.ajax({
			type: "GET",
			contentType: "application/json; charset=utf-8",
			url: 'Products/GoodsReceiveNote/GetIssueSlipFilterData',
			data: {},
			async: false,
			dataType: "json",
			success: function (data) {
				//$scope.FilterList = data;
				$("#Grid22").ejGrid({

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
					scrollSettings: { width: "auto", height: "2" },

					columns: [
						//{ headerText: "Entity Name", field: "EntityName", width: 100 },
						//{ headerText: "ActivityName", field: "UserName", width: 100 },
						{ headerText: "Material Type", field: "MaterialType", width: 100 },
						{ headerText: "Group Name", field: "MaterialMasterGroupName", width: 100 },
						//{ headerText: "Group Name2", field: "MaterialMasterGroupName", width: 90 },
						{ headerText: "Material Name", field: "MaterialMasterName", width: 100 },
						{ headerText: "Article", field: "StandardName", width: 100 },
						{ headerText: "Sku1", field: "FirstCharacteristicsValue", width: 60 },
						{ headerText: "Sku2", field: "SecondCharacteristicsValue", width: 60 },
						{ headerText: "Sku3", field: "ThirdCharacteristicsValue", width: 60 }



						//{ headerText: "RequisitionBy", field: "AddedBy", width: 85 },
						//{ headerText: "RequisitionNo", field: "RequisitionNo", width: 95 },
						//{ headerText: "Department Name", field: "DepartmentName", width: 100 }


					]
				});

				$("#Grid22").children('.e-pager.e-js.e-pager').hide();
				$("#Grid22").children('.e-gridcontent.e-droppable.e-js').hide();
				$("#Grid22").children('.e-gridcontent').hide();
				//$("#Grid2").children('.e-grid .e-headercell {background - color: chocolate;}').add();

				$("#Grid22").children('.e-grid.e-headercell').css('background-color', 'red'); //{background - color: chocolate;}').add();
			}

		});
	}

	$scope.GetIssueSlipFilterData();
	$scope.getDataMaterialWise = function () {
		//debugger;
		//alert('gg');
		var obj1 = $("#Grid22").ejGrid("instance");
		var sd1 = obj1.getFilteredRecords();
		if (sd1.length == 0) {
			sd1 = obj1.model.dataSource;
			//alert('1' +1);
		}
		$scope.FilterList123 = sd1;
	}
	// #endregion



  	// #region **********IssueSlipChecked **************


    $scope.GriddataISUnCheckedList = [];
    $scope.IssuStatus = 'ForChecked';
    $scope.getaldataIssueSlipUnChecked = function () {

        if ($scope.IssuStatus === 'ForChecked') {
            $scope.IssuStatus = 'ForChecked';
        }

        else {

        }
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/GoodsReceiveNote/IssueSlipUnChecked?IssuStatus=' + $scope.IssuStatus
        }).then(function successCallback(response) {
            $scope.GriddataISUnCheckedList = response.data;

            //entrydata = copy(searchdata);
        });
    };
    $scope.getaldataIssueSlipUnChecked();


    $scope.IssuStatus = 'ForChecked';

    $scope.tab = 1;
    $scope.IUCsetTabIndex1 = function (newTab) {
        $scope.tab = newTab;
        $scope.IssuStatus = 'ForChecked';
        $scope.getaldataIssueSlipUnChecked();

    };
    $scope.isIUCSetIndex1 = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.SetTabIssueHR3 = function (newTab) {
        $scope.tab = newTab;
        $scope.IssuStatus = 'HoldReject';
        $scope.getaldataIssueSlipUnChecked();

    };
    $scope.isSetIssueHR3 = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.ICsetTabIndex2 = function (newTab) {
        $scope.tab = newTab;
        $scope.IssuStatus = 'Checked';
        $scope.getaldataIssueSlipUnChecked();

    };
    $scope.isICSetIndex2 = function (tabNum) {
        return $scope.tab === tabNum;
    };

 //#endregion Requisition Tab




    // #region **********IssueSlipApproval**************

   
    $scope.GridIssueSlipUnApprovedList = [];
    $scope.IssuAppStatus = 'UnApproval';
    $scope.getaldataIssueSlipUnApproved = function () {
        //debugger;

        if ($scope.IssuAppStatus === 'Approval') {
            $scope.IssuAppStatus = 'Approval';
        }

        else {

        }
        //debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/GoodsReceiveNote/IssueSlipUnApproved?IssuAppStatus=' + $scope.IssuAppStatus
        }).then(function successCallback(response) {
            $scope.GridIssueSlipUnApprovedList = response.data;

            //entrydata = copy(searchdata);
        });
    };
    $scope.getaldataIssueSlipUnApproved();


    //$scope.IssuAppStatus = 'Approval';

    $scope.tab = 1;
    $scope.SetTabIssueUnApp = function (newTab) {
        $scope.tab = newTab;
        $scope.IssuAppStatus = 'UnApproval';
        $scope.getaldataIssueSlipUnApproved();

    };
    $scope.isSetIssueUnApp = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.SetTabIssueHR = function (newTab) {
        $scope.tab = newTab;
        $scope.IssuAppStatus = 'HoldReject';
        $scope.getaldataIssueSlipUnApproved();

    };
    $scope.isSetIssueHR = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.SetTabIssueApp = function (newTab) {
        $scope.tab = newTab;
        $scope.IssuAppStatus = 'Approved';
        $scope.getaldataIssueSlipUnApproved();

    };
    $scope.isSetIssueApp = function (tabNum) {
        return $scope.tab === tabNum;
    };

 //#endregion Requisition Tab
	
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


	$scope.PrintMICData = function (data) {
		location.href = 'Materials/MaterialIssueControl/GetMaterialIssueCheckApproveReportPdf?reportFormat=' + 'Pdf' + '&masterId=' + data.data.Id;

	};




}