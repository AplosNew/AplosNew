'use strict';
OSIssueReturnController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function OSIssueReturnController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
	$scope.ToDoFilePath = virtualPath.JobWorkValueAddedContract;
	$scope.ToDownloadFilePath = virtualPath.JobWorkTransformationContract;
	$rootScope.title = 'Out Source Issue/ Return';
	$scope.Action = 'Save';
	$scope.ModelList = [];
	$scope.IndividualReportList = [];
	$scope.IssueTypeList = [];
	$scope.JobWorkLocationList = [];
	$scope.TransformationTypeList = [];
	$scope.EntityList = [];
	$scope.MaterialLocationList = [];
	$scope.path = 'Outsourcing/OSIssueReturn/';
	$scope.getListUrl = $scope.path + 'getlist';
	$scope.saveUrl = $scope.path + 'create';
	$scope.deleteUrl = $scope.path + 'delete/';
	baseService.init($scope.getListUrl);
	$scope.searchBy = "p.UserName"; $scope.search = "";
	$scope.searchByList = [{ value: 'p.UserName', name: "Party Name" }, { value: 'e.UserName', name: "Entity" }, { value: 'Date', name: "Date" }];

	
	$scope.ValAddedJobWorkLocList = [];
	$scope.SelectedValAddedMaterialStorage = function () {
		if ($scope.ModelNew.OrderSpecific == "Yes") {
			$http({
				method: 'GET',
				url: $scope.path + 'getalljobworklocation',
			}).then(function successCallback(response) {
				$scope.ValAddedJobWorkLocList = response.data;
				
			});
		}
		else {
			$http({
				method: 'GET',
				url: 'Outsourcing/OSIssueReturn/gejobworklocation?TId=' + $scope.ModelNew.Id,
			}).then(function successCallback(response) {
				$scope.ValAddedJobWorkLocList = response.data;
				if ($scope.ValAddedJobWorkLocList.length > 0) {
					$scope.Issue.MaterialStorageId = $scope.ValAddedJobWorkLocList[0].Value;
					$scope.Issue.StorageLocation = $scope.ValAddedJobWorkLocList[0].StorageLocation;
					$scope.Issue.MSIdInventory = $scope.ValAddedJobWorkLocList[0].Value;
					$scope.GetValueAddedChildData();
				}
			});
        }

	}

	$scope.ValEntityList = [];
	$scope.SelectedValAddedEntity = function () {
		$http({
			method: 'GET',
			url: 'Outsourcing/OSIssueReturn/getentitylist/',
		}).then(function successCallback(response) {
			$scope.ValEntityList = response.data;
			if ($scope.IssueTypeList.length > 0) {
				for (var q = 0; q < $scope.ValEntityList.length; q++) {
					if ($scope.ValEntityList[q].Value == $scope.IssueTypeList[0].EntityId) {
						$scope.Issue.EntityId = $scope.ValEntityList[q].Value;
					}
				}
			}
		});
	}


	var d = new Date();
	var hh = d.getHours();
	var mm = d.getMinutes();
	mm = (mm < 10 ? '0' + mm : mm);
	var ss = d.getSeconds()

	//   var _Time = hh + ":" + mm + ":" + ss;
	var _Time = hh + ":" + mm;

	$scope.ModelTemp = {
		Id: null,
		Type: null,

	};
	$scope.ModelNew = Object.assign({}, $scope.ModelTemp);

	$scope.IssueModelTemp = {
		Id: null,
		IssueDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
		EmployeeId: null,
		Types: 'InventoryOSIssue',
		MaterialStorageId: null,
		Remarks: null,
		EmployeeStatus: null,
		EmployeeCode: null,
		ResponsiblePerson: null,
		IsConfirmed: false,
		EntityId: null,
		IssueType: 'Revenue',
		JWContractId: null,
		ContractType: null,
		MSIdInventory: null,
		OrderRefNo: null,
	};
	$scope.Issue = Object.assign({}, $scope.IssueModelTemp);

	$scope.getData = function () {
		if ($scope.ModelNew.Type == null) {
			var IssueType = "Value Added";
			$scope.ModelNew.Type = IssueType;
		}
		$http({
			method: 'POST',
			url: $scope.path + "GetList",
			data: { column: $scope.searchBy, value: $scope.search, Type: $scope.ModelNew.Type },
			dataType: 'JSON'
		}).then(function successCallback(response) {
			$scope.ModelList = response.data;
			$scope.ShowHomeList = true;
			$scope.ShowReport = false;
			//        ClearFields();

		});
	}
	$scope.getData();

	$scope.ShowHomeList = true;
	$scope.ShowReport = false;
	$scope.Get = function (args) {
		$scope.ModelNew = Object.assign({}, args.data);
		if ($scope.ModelNew.TabType == "Transformation") {

			$scope.Transformation = Object.assign({}, args.data);
			var PId = $scope.Transformation.Id;
			var TabType = $scope.Transformation.TabType;
			$scope.IssueTransformation.JWContractId = $scope.Transformation.Id;
			$scope.IssueTransformation.ContractType = 'Transformation';
			$scope.TabTypeNew = $scope.Transformation.TabType;
			$http({
				method: 'POST',
				url: $scope.path + "GetDataById",
				data: { Id: PId, TabType: TabType },
				dataType: 'JSON'
			}).then(function successCallback(response) {
				$scope.TransformationTypeList = response.data;
				$scope.IssueTransformation.JWContractId = response.data[0].Id;
				if ($scope.TransformationTypeList.length > 0) {
					$scope.GetTransformationChildData();
					//$scope.ShowHomeList = false;
					//$scope.ShowReport = true;
					//   $scope.GetIndividualReportData();

					$scope.SelectedTConEntity();
					$scope.SelectedTConMaterialStorage();
					$scope.getdataInventoryIssue();
					
				}

			});

			$scope.setTab(2);
		}
		else {

			$scope.ModelNew = Object.assign({}, args.data);
			var PId = $scope.ModelNew.Id;
			var TabType = $scope.ModelNew.TabType;
			$scope.Issue.JWContractId = $scope.ModelNew.Id;
			$scope.Issue.ContractType = 'ValueAdded';
			$scope.TabTypeNew = $scope.ModelNew.TabType;
			//       $scope.ModelNew.Type = TabType;
			$http({
				method: 'POST',
				url: $scope.path + "GetDataById",
				data: { Id: PId, TabType: TabType },
				dataType: 'JSON'
			}).then(function successCallback(response) {
				$scope.IssueTypeList = response.data;

				if ($scope.IssueTypeList.length > 0) {	
					$scope.SelectedValAddedEntity();
					$scope.SelectedValAddedMaterialStorage();
					$scope.getdataInventoryIssue();
				//	$scope.GetValueAddedChildData();
				}

			});

			$scope.setTab(1);
		
		}
		$scope.ModelNew.Type = $scope.TabTypeNew;
		
	};


	$scope.GridInventoryIssuedata = [];
	$scope.getdataInventoryIssue = function () {
		if ($scope.ModelNew.TabType == "Transformation") {
			if ($scope.GRNbyPOCheckStatus === "ForChecked") {
				$scope.GRNbyPOCheckStatus = "ForChecked";
			}
			$scope.GridInventoryIssuedata = [];
			$http({
				method: "GET",
				url: $scope.path + 'GetDataByInventoryIssue?Id=' + $scope.Transformation.Id + '&GRNbyPOCheckStatus=' + $scope.GRNbyPOCheckStatus,
			}).then(function successCallback(response) {
				$scope.GridInventoryIssuedata = response.data;
					$scope.ShowHomeList = false;
					$scope.ShowReport = true;
					$scope.setTab(2);
			
			});
		}
		else {
			$scope.GridInventoryIssuedata = [];
			if ($scope.GRNbyPOCheckStatus === "ForChecked") {
				$scope.GRNbyPOCheckStatus = "ForChecked";
			}
			$http({
				method: "GET",
				url: $scope.path + 'GetDataByInventoryIssue?Id=' + $scope.ModelNew.Id + '&GRNbyPOCheckStatus=' + $scope.GRNbyPOCheckStatus,
			}).then(function successCallback(response) {
				$scope.GridInventoryIssuedata = response.data;
				$scope.ShowHomeList = false;
				$scope.ShowReport = true;
				$scope.setTab(1);
				
			});
        }


	};

	$scope.GetValueAddedChildData = function () {
		$scope.IssueChildList = [];
		$http({
			method: 'POST',
			data: { PKId: $scope.ModelNew.Id, OrderSpecific: $scope.ModelNew.OrderSpecific, MaterialStorageIdInventory: $scope.Issue.MSIdInventory, IssueDate: $scope.Issue.IssueDate },
			url: $scope.path + 'GetValueAddedChildData',

		}).then(function successCallback(response) {
			$scope.IssueChildList = response.data;
			if ($scope.IssueChildList.length > 0) {
				$scope.CostCenterLoad();
            }
		});
	}

	$scope.GetTransformationChildData = function () {
		$scope.IssueTransformationChildList = [];
		$http({
			method: 'GET',
			url: $scope.path + 'GetTransformationChildData?PKId=' + $scope.Transformation.Id,
		}).then(function successCallback(response) {
			$scope.IssueTransformationChildList = response.data;
			
		});
	}

	$scope.Save = function () {
		$scope.$broadcast('show-errors-check-validity');
		if ($scope.IssueGeneralForm.$valid) {
			$http({
				method: 'POST',
				url: $scope.saveUrl,
				data: { 'data': $scope.Issue, 'ContractId': $scope.Issue.JWContractId, 'ContractType': $scope.Issue.ContractType },
				dataType: 'JSON'
			}).then(function successCallback(response) {
				if (response.data.Error === true) {
					ShowResult(response.data.Message, 'failure');
				}
				else {
					ShowResult(response.data.Message, 'success');
					$scope.Issue = response.data.Data;

				}
			}), function errorCallBack(response) {
				ShowResult(response.data.Message, 'failure');
			}

		}
	};

	$scope.Clear = function () {
		ClearFields();
	};

	function ClearFields() {
		$scope.Action = 'Save';
		$scope.Issue = Object.assign({}, $scope.IssueModelTemp);
	}

	//   // #region field

	$scope.EmpResPersonList = [];
	$scope.ResponsiblePersonPopUp = function () {
		angular.element(document.querySelector("#EmployeePopUpResPerson")).modal("show");
		$scope.getEmpDetailsData();

	}
	$scope.getEmpDetailsData = function () {
		$scope.EmpResPersonList = [];
		$http({
			method: 'POST',
			data: { Id: $scope.Issue.Id },
			url: $scope.path + 'LoadAllEmpDetails'
		}).then(function successCallback(response) {
			$scope.EmpResPersonList = response.data;
		});
	}

	$scope.ResponsiblePersonClear = function () {
		$scope.Issue.EmployeeId = null;
		$scope.Issue.ResponsiblePerson = null;
		$scope.Issue.EmployeeCode = null;
		$scope.Issue.EmployeeStatus = null;

	};
	$scope.closeEmpResPersonPopUp = function (popupName) {
		angular.element(document.querySelector("#" + popupName + "")).modal("hide");

	}
	$scope.setEmpData = function (obj) {

		var data = obj.data;
		$scope.Issue.EmployeeCode = data.Code;
		$scope.Issue.EmployeeId = data.Id;
		$scope.Issue.ResponsiblePerson = data.EmployeeName;
		angular.element(document.querySelector('#EmployeePopUpResPerson')).modal('hide');
	};
	//   // # end region

	//  ISSUE CHILD DATA

	$scope.IssueChildList = [];
	$scope.GRNbyPOCheckStatus = "ForChecked";

	$scope.tab = 1;
	$scope.setTab = function (newTab) {
		$scope.tab = newTab;
	};

	$scope.isSet = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.Issuetab = 1;
	$scope.setTabGRNList = function (newTab2) {
		$scope.GRNbyPOCheckStatus = "ForChecked";
		$scope.Issuetab = newTab2;
		$scope.getdataInventoryIssue();
	};

	$scope.isSetGRNList = function (tabNum2) {
		return $scope.Issuetab === tabNum2;
	};

	// For Posted
	$scope.GRN = "";
//	$scope.Ptab = 6;
	$scope.setTabPosted = function (PostedTab) {
	//	$scope.Ptab = PostedTab;
		$scope.Issuetab = PostedTab;
		$scope.GRNbyPOCheckStatus = "Posted";
		$scope.getdataInventoryIssue();

	};
	$scope.isSetPosted = function (tabNumPst) {
	//	return $scope.Ptab === tabNumPst;
		return $scope.Issuetab === tabNumPst;
//		$scope.GRN = 6;
	};

	$scope.Detaillst = [];
	$scope.GRNListDetails = function () {
		//;
		$http({
			method: 'GET',
			url: $scope.path + 'JWDetailsData'
		}).then(function successCallback(response) {
			$scope.Detaillst = response.data;
			window.Detaillst = response.data;

		});
	}
	$scope.GRNListDetails();

	$scope.detailTemp = "#tabGridContents";
	$scope.detailgrid = function detailGridData(e) {
		//;

		var filteredData = e.data["Id"];
		var data = ej.DataManager(window.Detaillst).executeLocal(ej.Query().where("Id", "equal", parseInt(filteredData), true).take(100));
		e.detailsElement.find("#detailGrid").ejGrid({
			dataSource: data,
			columns: ["MaterialName", "Article", "SKU1", "SKU2", "SKU3", "TransactionQty", "TransactionUoMId", "TransactionUoM", "BaseCurrency", "BaseRate", "Amount"]
		});
		e.detailsElement.find(".tabcontrol").ejTab();


	}



	$scope.IssueChildModelTemp = {
		Id: null,
		JobWorkIssueReturnMasterId: null,
		ContractLineItemId: null,
		OrderChildId: null,
		Quantity: null,
		Remarks: null,
		Active: null,

	};
	$scope.IssueChild = Object.assign({}, $scope.IssueChildModelTemp);

	$scope.ValidateQuantity = function (RowData) {
		try {

			for (var i = 0; i < $scope.IssueChildList.length > 0; i++) {
				if ($scope.IssueChildList[i].OrderSpecific == "Yes") {
					if ($scope.IssueChildList[i].Id === RowData.Id && $scope.IssueChildList[i].JWOrderWiseId === RowData.JWOrderWiseId) {
						var IssueQty = parseFloat(RowData.BalToIssue);
						var BalQty = parseFloat($scope.IssueChildList[i].OWRQuantity) - parseFloat($scope.IssueChildList[i].IssueQuantity)
						if (IssueQty > BalQty) {
							$scope.IssueChildList[i].BalToIssue = BalQty;
							throw 'Issue Quantity cannot be greater than Balance to Issue';
						}
					}
				}
				if ($scope.IssueChildList[i].OrderSpecific == "NO") {
					if ($scope.IssueChildList[i].Id === RowData.Id) {
						var IssueQty = parseFloat(RowData.BalToIssue);
						var BalQty = parseFloat($scope.IssueChildList[i].VCCQuantity) - parseFloat($scope.IssueChildList[i].IssueQuantity)
						if (IssueQty > BalQty) {
							$scope.IssueChildList[i].BalToIssue = BalQty;
							throw 'Issue Quantity cannot be greater than Balance to Issue';
						}
					}
				}
			}
		}
		catch (e) {
			ShowResult(e, "failure");
		}

	}

	//Save Function 
	$scope.SaveIssueChildTab = function () {
		$scope.$broadcast('show-errors-check-validity');
		var checkedData = [];
		for (var i = 0; i < $scope.IssueChildList.length; i++) {
			if ($scope.IssueChildList[i].isSelected == true)
				checkedData.push($scope.IssueChildList[i]);
		}
		try {
			if (checkedData.length == 0) {
				throw 'Please Enter at least one Quantity';
			}
			$http({
				method: 'POST',
				data: { IssueChildTabData: checkedData, MasterId: $scope.Issue.Id },
				url: $scope.path + 'SaveIssueChild'
			}).then(function successCallback(response) {
				if (response.data.Error == true) {
					ShowResult(response.data.Message, "failure");
				}
				else {
					ShowResult(response.data.Message, "success");
					$scope.IssueChild = response.data.Data;
					$scope.GetValueAddedChildData($scope.ModelNew.Id);
				}
			});

		}
		catch (e) {
			ShowResult(e, "failure");
		}

		//     }
	}
	$scope.ClearIssueChildTab = function () {
		ClearFieldsIssueChild();
		$scope.IssueChildList = [];
		$scope.IssueTypeList = [];
		$scope.materialStockList = [];
		$scope.specificStockList = [];
		$scope.getData();
		$scope.Action = 'Save';

	}

	function ClearFieldsIssueChild() {
		$scope.Issue = Object.assign({}, $scope.IssueModelTemp);
	}

	// REPORTS OF VALUE ADDED ISSUE/ REPORT

	$scope.DownloadReport = function (data) {
		try {
			$scope.PrintTabId = $scope.ModelNew.Id;
			$scope.IssueId = $scope.Issue.Id;
			var TabType = $scope.ModelNew.TabType;
			if (TabType == "Value Added") {
				var reportFormat = "Excel";
				window.open('Outsourcing/OSIssueReturn/GetValueAddedPrintReport?reportFormat=' + reportFormat + '&PrintTabId=' + $scope.PrintTabId + '&IssueId=' + $scope.IssueId, '_blank');
				$scope.getData();
			}

		} catch (e) {

		}
	};

	$scope.JobWorkLocList = [];
	$scope.EntityList = [];


	$scope.SelectedTConMaterialStorage = function () {
		if ($scope.Transformation.OrderSpecific == "Yes") {
			$http({
				method: 'GET',
				url: 'Outsourcing/OSIssueReturn/getalljobworklocation/',
			}).then(function successCallback(response) {
				$scope.JobWorkLocList = response.data;
				
			});
		}
		else {
			$http({
				method: 'GET',
				url: 'Outsourcing/OSIssueReturn/gejobworklocation?TId=' + $scope.Transformation.Id,
			}).then(function successCallback(response) {
				$scope.JobWorkLocList = response.data;
				if ($scope.JobWorkLocList.length > 0) {
					$scope.IssueTransformation.MaterialStorageId = $scope.JobWorkLocList[0].Value;
					$scope.IssueTransformation.StorageLocation = $scope.JobWorkLocList[0].StorageLocation;
					$scope.IssueTransformation.MaterialStorageIdInventory = $scope.JobWorkLocList[0].Value;
				}
			});
		}
	}

	$scope.SelectedMaterialStorage = [];
	$scope.GetSelectedMaterialStorage = function () {
		if ($scope.ModelNew.TabType == "Transformation") {
			$http({
				method: 'GET',
				url: 'Outsourcing/OSIssueReturn/getStoragloc?JLId=' + $scope.IssueTransformation.MaterialStorageId,
			}).then(function successCallback(response) {
				$scope.SelectedMaterialStorage = response.data;
				if ($scope.SelectedMaterialStorage.length > 0) {
					//      $scope.IssueTransformation.MaterialStorageId = $scope.SelectedMaterialStorage[0].Value;
					$scope.IssueTransformation.StorageLocation = $scope.SelectedMaterialStorage[0].StorageLocation;
					$scope.IssueTransformation.MaterialStorageIdInventory = $scope.SelectedMaterialStorage[0].Value;
				}
				else {
					$scope.IssueTransformation.StorageLocation = null;
				}
			});
		}
		else {
			if ($scope.ModelNew.OrderSpecific == "Yes") {
				$http({
					method: 'GET',
					url: 'Outsourcing/OSIssueReturn/getStoragloc?JLId=' + $scope.Issue.MaterialStorageId,
				}).then(function successCallback(response) {
					$scope.SelectedMaterialStorage = response.data;
					if ($scope.SelectedMaterialStorage.length > 0) {
						$scope.Issue.StorageLocation = $scope.SelectedMaterialStorage[0].StorageLocation;
					//	$scope.Issue.MaterialStorageId = $scope.SelectedMaterialStorage[0].Value;
						$scope.Issue.MSIdInventory = $scope.SelectedMaterialStorage[0].Value;
						$scope.GetValueAddedChildData();
					}
					else {
						$scope.Issue.StorageLocation = null;
					}
				});
            }

        }
	}

	$scope.SelectedTConEntity = function () {
		$http({
			method: 'GET',
			url: 'Outsourcing/OSIssueReturn/getentitylist/',
		}).then(function successCallback(response) {
			$scope.EntityList = response.data;
			if ($scope.TransformationTypeList.length > 0) {
				for (var q = 0; q < $scope.EntityList.length; q++) {
					if ($scope.EntityList[q].Value == $scope.TransformationTypeList[0].EntityId) {
						$scope.IssueTransformation.EntityId = $scope.EntityList[q].Value;
					}
				}
			}
		});
	}

	$scope.IssueTransformationModelTemp = {
		Id: null,
		IssueDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
		EmployeeId: null,
		Types: 'InventoryOSIssue',
		MaterialStorageId: null,
		Remarks: null,
		EmployeeStatus: null,
		EmployeeCode: null,
		ResponsiblePerson: null,
		IsConfirmed: false,
		EntityId: null,
		IssueType: 'Revenue',
		JWContractId: null,
		ContractType: null,
		MaterialStorageIdInventory: null,
		RefferenceNo: null,
		OrderRefNo:null,

	};
	$scope.IssueTransformation = Object.assign({}, $scope.IssueTransformationModelTemp);

	$scope.ValidateIssueDate = function () {
		try {

			if (new Date($scope.IssueTransformation.Date) > new Date()) {
				$scope.IssueTransformation.Date = $filter('dateFiltering')(new Date(), 'dd-M-yyyy');
				throw 'Issue Date should not be greater than Current date.';
			}
		}
		catch (e) {
			ShowResult(e, "failure");
		}
	}

	// #region field

	$scope.EmployeeResPersonList = [];
	$scope.EmpPopUp = function () {
		angular.element(document.querySelector("#EmpPopUpResPerson")).modal("show");
		$scope.getEmpData();

	}
	$scope.getEmpData = function () {
		$scope.EmployeeResPersonList = [];
		$http({
			method: 'POST',
			data: { Id: $scope.IssueTransformation.Id },
			url: $scope.path + 'LoadAllResponsiblePersonDetails'
		}).then(function successCallback(response) {
			$scope.EmployeeResPersonList = response.data;
		});
	}

	$scope.EmpClear = function () {
		$scope.IssueTransformation.EmployeeId = null;
		$scope.IssueTransformation.EmpName = null;
		$scope.IssueTransformation.EmpCode = null;
		$scope.IssueTransformation.EmpStatus = null;

	};
	$scope.closePopUp = function (popupName) {
		angular.element(document.querySelector("#" + popupName + "")).modal("hide");

	}
	$scope.setEmployeeData = function (obj) {

		var data = obj.data;
		$scope.IssueTransformation.EmpCode = data.Code;
		$scope.IssueTransformation.EmployeeId = data.Id;
		$scope.IssueTransformation.EmpName = data.EmployeeName;
		angular.element(document.querySelector('#EmpPopUpResPerson')).modal('hide');
	};
	// # end region

	$scope.SaveIssueTransformation = function () {
		$scope.$broadcast('show-errors-check-validity');
		if ($scope.IssueTransformationForm.$valid) {
			$http({
				method: 'POST',
				url: $scope.path + 'SaveIssueTransformation',
				data: { 'data': $scope.IssueTransformation, 'ContractId': $scope.Transformation.Id, 'ContractType': $scope.ModelNew.TabType },
				dataType: 'JSON'
			}).then(function successCallback(response) {
				if (response.data.Error === true) {
					ShowResult(response.data.Message, 'failure');
				}
				else {
					ShowResult(response.data.Message, 'success');
					$scope.IssueTransformation = response.data.Data;

				}
			}), function errorCallBack(response) {
				ShowResult(response.data.Message, 'failure');
			}

		}
	};

	$scope.ClearIssueTransformation = function () {
		ClearFieldsIssueTransformation();
	};

	function ClearFieldsIssueTransformation() {
		$scope.Action = 'Save';
		$scope.IssueTransformation = Object.assign({}, $scope.IssueTransformationModelTemp);
	}


	//   TRANSFORMATION ISSUE CHILD

	$scope.IssueTransformationChildList = [];
	$scope.MaterialInputList = [];
	$scope.detailList = [];
	$scope.MatInputListLocal = [];

	$scope.SelectMaterialPlanning = function () {
		//$scope.product = Object.assign({}, $scope.productNew);
		if (baseService.isUndefinedOrNull($scope.IssueTransformation.IssueDate)) {
			ShowResult("Select the issue date");
			return false;

		}
		//if (baseService.isUndefinedOrNull($scope.IssueTransformation.EntityId)) {
		//	ShowResult("Select the Entity");
		//	return false;

		//}
		if (baseService.isUndefinedOrNull($scope.IssueTransformation.MaterialStorageId)) {
			ShowResult("Select the Material Storage");
			return false;

		}
		if (baseService.isUndefinedOrNull($scope.IssueTransformation.IssueType)) {
			ShowResult("Select the type");
			return false;

		}
		//if (baseService.isUndefinedOrNull($scope.IssueTransformation.EmpName)) {
		//	ShowResult("Select the wby whom");
		//	return false;

		//}
		$scope.detailModel = {
			Id: null
			, InventoryReveiveId: null
			//, MaterialStorageId: $scope.productNew.MaterialStorageId
			, InventoryMaterialId: null
			, MaterialMasterId: null
			, MaterialMasterName: null
			, ArticleId: null
			, ArticleName: null
			, MaterialTypeName: null
			, OurStyleName: null
			, Description: null
			, MaterialGroupMasterName: null
			, ProductMasterName: null
			, IsOurStyleRequired: false
			, IsProductMstRequired: false
			, FirstCharacteristicsId: null
			, FirstCharacteristicsValueId: null
			, SecondCharacteristicsId: null
			, SecondCharacteristicsValueId: null
			, ThirdCharacteristicsId: null
			, ThirdCharacteristicsValueId: null
			, TransactionQty: null
			, TransactionUoMId: null
			, TransactionUoM: null
			, BaseQty: null
			, BaseUOMId: null
			, BaseUoM: null
			, BaseUoMFactor: null
			, TransactionRate: null
			, TotalQty: 0
			, AvgRate: null
			//, InventoryIssueId: $scope.productNew.Id
			, AvgAmount: null
			, PolicyRate: null
			, PolicyAmount: null
			, Policy: null
			, ActivityName: null
			, BudgetMasterId: null
			, ActivityId: null
			, IssueId: null
			, CostCenterId: null
		};
		var SelectedData = [];
		for (var i = 0; i < $scope.IssueTransformationChildList.length; i++) {
			if ($scope.IssueTransformationChildList[i].isSelected == true)
				SelectedData.push($scope.IssueTransformationChildList[i]);
		}

		if (baseService.isUndefinedOrNull($scope.IssueTransformation.Id)) {
			$http({
				method: 'POST',
				data: { SelectedMaterialPlanningData: SelectedData, OrderSpecific: $scope.Transformation.OrderSpecific, MaterialStorageIdInventory: $scope.IssueTransformation.MaterialStorageIdInventory, IssueDate: $scope.IssueTransformation.IssueDate },
				url: $scope.path + 'GetMaterialInputData'
			}).then(function successCallback(response) {
				$scope.MaterialInputList = response.data;
				$scope.MatInputListLocal = response.data;
				if ($scope.MaterialInputList.length > 0) {
					for (var a = 0; a < $scope.MaterialInputList.length; a++) {
					//	var Id = $scope.MaterialInputList[a].OSTransformationPOId;
						var Id = $scope.MaterialInputList[a].OSTransformationPODetailId;
						var ArticleId = $scope.MaterialInputList[a].ArticleId;

						for (var b = 0; b < $scope.MatInputListLocal.length; b++) {
						//	if ($scope.MatInputListLocal[b].OSTransformationPOId != Id) {
							if ($scope.MatInputListLocal[b].OSTransformationPODetailId != Id) {
								if ($scope.MatInputListLocal[b].ArticleId == ArticleId) {
									ShowResult("Common Input Material is there");
									return false;
								}
							}
						}
					}
				}

				$scope.detailList = response.data;
				for (var i = 0; i < $scope.detailList.length; i++) {
					$scope.detailList[i].MaterialStorageId = $scope.IssueTransformation.MaterialStorageIdInventory;
				}

				if ($scope.MaterialInputList.length > 0 && $scope.detailList.length > 0) {
					$scope.CostCenterLoadNew();
				}

				//   $scope.detailList = response.data;
			});
		}
		else {
			$http({
				method: 'POST',
				data: { SelectedMaterialPlanningData: SelectedData, OrderSpecific: $scope.Transformation.OrderSpecific, MaterialStorageIdInventory: $scope.IssueTransformation.MaterialStorageIdInventory, IssueDate: $scope.IssueTransformation.IssueDate, TransIssueId: $scope.TransIssueId },
				url: $scope.path + 'GetMaterialInputData'
			}).then(function successCallback(response) {
				$scope.MaterialInputList = response.data;
				$scope.MatInputListLocal = response.data;
				if ($scope.MaterialInputList.length > 0) {
					for (var a = 0; a < $scope.MaterialInputList.length; a++) {
					//	var Id = $scope.MaterialInputList[a].OSTransformationPOId;
						var Id = $scope.MaterialInputList[a].OSTransformationPODetailId;
						var ArticleId = $scope.MaterialInputList[a].ArticleId;

						for (var b = 0; b < $scope.MatInputListLocal.length; b++) {
						//	if ($scope.MatInputListLocal[b].OSTransformationPOId != Id) {
							if ($scope.MatInputListLocal[b].OSTransformationPODetailId != Id) {
								if ($scope.MatInputListLocal[b].ArticleId == ArticleId) {
									ShowResult("Common Input Material is there");
									return false;
								}
							}
						}
					}
				}

				$scope.detailList = response.data;
				for (var i = 0; i < $scope.detailList.length; i++) {
					$scope.detailList[i].MaterialStorageId = $scope.IssueTransformation.MaterialStorageIdInventory;
					$scope.detailList[i].isSelectedMatInput = true;
				}

				if ($scope.MaterialInputList.length > 0 && $scope.detailList.length > 0) {
					$scope.CostCenterLoadNew();
				}

				//if ($scope.detailList.length > 0) {
				//	for (var i = 0; i < $scope.detailList.length; i++) {
				//		$scope.detailList[i].isSelectedMatInput = true;
				//	}
				//}

				//   $scope.detailList = response.data;
			});
        }



	}
	$scope.GetRate = [];
	$scope.GetLotNoRate = function (RowData) {
		$scope.GetRate = [];
		$scope.LotNum = RowData.LotNumber;
		$http({
			method: 'GET',
			url: 'Outsourcing/OSIssueReturn/GetLotNoRate?LotNumber=' + $scope.LotNum,
		}).then(function successCallback(response) {
			$scope.GetRate = response.data;
			for (var i = 0; i < $scope.MaterialInputList.length > 0; i++) {
				if ($scope.MaterialInputList[i].Id === RowData.Id) {
					$scope.MaterialInputList[i].Rate = response.data[0].MaterialTranRate;

				}
			}
		});
	}

	$scope.GetMaterialValue = function (RowData) {
		try {
			for (var i = 0; i < $scope.MaterialInputList.length > 0; i++) {
				if ($scope.MaterialInputList[i].Id === RowData.Id) {
					if (parseFloat(RowData.Quantity) <= parseFloat($scope.MaterialInputList[i].BalanceToIssue)) {
						var MaterialRate = parseFloat($scope.MaterialInputList[i].Rate);
						var MaterialQty = parseFloat($scope.MaterialInputList[i].Quantity);
						var MaterialValue = parseFloat(MaterialRate * MaterialQty);
						var Num = MaterialValue.toFixed(2);
						$scope.MaterialInputList[i].Value = Num;
					}
					else {
						RowData.Quantity = null;
						RowData.Value = null;
						throw 'To Issue Quantity cannot be greater than Balance to Issue';
					}
				}
			}
		}
		catch (e) {
			ShowResult(e, "failure");
		}
	}

	// #region field

	$scope.MaterialMstList = [];
	$scope.MaterialMstPopUp = function (data) {
		angular.element(document.querySelector("#MaterialPopUp")).modal("show");
		$scope.getMaterialMstDetailsData(data);
	}

	$scope.getMaterialMstDetailsData = function (data) {
		$scope.MaterialMstList = [];

		for (var i = 0; i < $scope.MaterialInputList.length > 0; i++) {
			if ($scope.MaterialInputList[i].Id === data.Id) {
				$scope.MatMstId = $scope.MaterialInputList[i].InputMaterialId;
				$scope.a = i;
			}
		}

		$http({
			method: 'GET',
			url: $scope.path + 'LoadAllMaterialMstDetails'
		}).then(function successCallback(response) {
			$scope.MaterialMstList = response.data;
		});
	}

	$scope.MaterialMstClear = function (data) {
		for (var i = 0; i < $scope.MaterialInputList.length > 0; i++) {
			if ($scope.MaterialInputList[i].Id === data.Id) {
				$scope.MaterialInputList[i].InputMaterialId = null;
				$scope.MaterialInputList[i].InputMaterialCode = null;
				$scope.MaterialInputList[i].InputMaterial = null;
			}
		}
	};

	$scope.closeMaterialMstPopUp = function (popupName) {
		angular.element(document.querySelector("#" + popupName + "")).modal("hide");

	}
	$scope.setMaterialMstData = function (obj) {
		var b = $scope.a;
		var data = obj.data;
		$scope.MaterialInputList[b].InputMaterialId = data.Id;
		$scope.MaterialInputList[b].InputMaterialCode = data.Code;
		$scope.MaterialInputList[b].InputMaterial = data.MaterialName;

		$scope.MaterialInputList[b].MaterialMasterArticleId = null;
		$scope.MaterialInputList[b].ArticleCode = null;
		$scope.MaterialInputList[b].ArticleName = null;

		angular.element(document.querySelector('#MaterialPopUp')).modal('hide');
	};
	// # end region


	// GET ARTICLE
	// MATERIAL MASTER ARTICLE
	// #region field

	$scope.MaterialArticleMstList = [];
	$scope.MaterialMstArticlePopUp = function (RowData, index) {
		$scope.indexforDetail = index;
		angular.element(document.querySelector("#MaterialArticlePopUp")).modal("show");
		$scope.getMaterialMstArticleData(RowData);

	}
	$scope.getMaterialMstArticleData = function (RowData) {
		$scope.MaterialArticleMstList = [];

		for (var i = 0; i < $scope.MaterialInputList.length > 0; i++) {
			if ($scope.MaterialInputList[i].Id === RowData.Id) {
				$scope.MatMstId = $scope.MaterialInputList[i].InputMaterialId;
				$scope.SelectedMaterialInputId = $scope.MaterialInputList[i].Id;
				$scope.a = i;
			}
		}

		$http({
			method: 'POST',
			data: { MaterialMstId: $scope.MatMstId, MaterialInputId: $scope.SelectedMaterialInputId },
			url: $scope.path + 'LoadAllMaterialMstArticle'
		}).then(function successCallback(response) {
			$scope.MaterialArticleMstList = response.data;
		});
	}

	$scope.MaterialMstArticleClear = function (data) {
		for (var i = 0; i < $scope.MaterialInputList.length > 0; i++) {
			if ($scope.MaterialInputList[i].Id === data.Id) {

				$scope.MaterialInputList[i].MaterialMasterArticleId = null;
				$scope.MaterialInputList[i].ArticleCode = null;
				$scope.MaterialInputList[i].ArticleName = null;
			}
		}
	};

	$scope.closeMaterialArticlePopUp = function (popupName) {
		angular.element(document.querySelector("#" + popupName + "")).modal("hide");

	}
	$scope.setMaterialArticleData = function (obj) {
		$scope.detailModel = {
			Id: null
			, InventoryReveiveId: null
			, MaterialStorageId: $scope.IssueTransformation.MaterialStorageId
			, InventoryMaterialId: null
			, MaterialMasterId: null
			, MaterialMasterName: null
			, ArticleId: null
			, ArticleName: null
			, MaterialTypeName: null
			, OurStyleName: null
			, Description: null
			, MaterialGroupMasterName: null
			, ProductMasterName: null
			, IsOurStyleRequired: false
			, IsProductMstRequired: false
			, FirstCharacteristicsId: null
			, FirstCharacteristicsValueId: null
			, SecondCharacteristicsId: null
			, SecondCharacteristicsValueId: null
			, ThirdCharacteristicsId: null
			, ThirdCharacteristicsValueId: null
			, TransactionQty: null
			, TransactionUoMId: null
			, TransactionUoM: null
			, BaseQty: null
			, BaseUOMId: null
			, BaseUoM: null
			, BaseUoMFactor: null
			, TransactionRate: null
			, TotalQty: 0
			, AvgRate: null
			//, InventoryIssueId: $scope.productNew.Id
			, AvgAmount: null
			, PolicyRate: null
			, PolicyAmount: null
			, Policy: null
			, ActivityName: null
			, BudgetMasterId: null
			, ActivityId: null
			, IssueId: null
			, CostCenterId: null
		};
		var b = $scope.a;
		var data = obj.data;
		$scope.MaterialInputList[b].MaterialMasterArticleId = data.ArticleId;
		$scope.MaterialInputList[b].ArticleCode = data.ArticleCode;
		$scope.MaterialInputList[b].ArticleName = data.StandardName;
		$scope.SelectedArticleId = data.ArticleId;

		$scope.detailModel.MaterialMasterId = data.MaterialMasterId;
		$scope.detailModel.ArticleId = data.ArticleId;

		$scope.detailList[$scope.indexforDetail].MaterialMasterId = data.MaterialMasterId;
		$scope.detailList[$scope.indexforDetail].ArticleId = data.ArticleId;
		//$scope.GetByDefaultRate($scope.a);
		//$scope.GetLotNumberList($scope.a);
		$scope.GetIssuedDetailList($scope.a);
		getMaterialStock(b);
		angular.element(document.querySelector('#MaterialArticlePopUp')).modal('hide');
	};

	$scope.ByDefRate = [];
	$scope.GetByDefaultRate = function (c) {
		$scope.MaterialInputList[c].Rate = null;
		$http({
			method: 'GET',
			url: $scope.path + 'GetByDefaultRate?ArticleId=' + $scope.SelectedArticleId,
		}).then(function successCallback(response) {
			$scope.ByDefRate = response.data;
			if ($scope.ByDefRate.length > 0) {
				$scope.MaterialInputList[c].Rate = $scope.ByDefRate[0].Rate;
			}
		});
	}

	//  GET LOT NUMBER

	$scope.LotNumList = [];
	$scope.GetLotNumberList = function (x) {
		$scope.MaterialInputList[x].LotNumberList = null;
		$http({
			method: 'GET',
			url: $scope.path + 'GetLotNumberList?ArticleId=' + $scope.SelectedArticleId + '&MaterialId=' + $scope.MatMstId,
		}).then(function successCallback(response) {
			$scope.LotNumList = response.data;
			if ($scope.LotNumList.length > 0) {
				$scope.MaterialInputList[x].LotNumberList = response.data;
			}
		});
	}

	//  GET Planned, Issued, Balance Quantity

	$scope.IssuedDetailList = [];
	$scope.GetIssuedDetailList = function (x) {
		$scope.detailList[x].PlannedQty = null;
		$scope.detailList[x].IssuedQty = null;
		$scope.detailList[x].BalanceQty = null;
		$http({
			method: 'GET',
			url: $scope.path + 'GetIssuedDetailList?ArticleId=' + $scope.SelectedArticleId + '&MaterialId=' + $scope.MatMstId + '&MaterialInputId=' + $scope.SelectedMaterialInputId + '&ContractId=' + $scope.Transformation.Id,
		}).then(function successCallback(response) {
			$scope.IssuedDetailList = response.data;
			if ($scope.IssuedDetailList.length > 0) {

				//     $scope.detailList[$scope.indexforDetail].MaterialMasterId = data.MaterialMasterId;
				$scope.detailList[x].PlannedQty = $scope.IssuedDetailList[0].RequiredQuantity;
				$scope.detailList[x].IssuedQty = $scope.IssuedDetailList[0].TIRCTotalQty;
				$scope.detailList[x].BalanceQty = $scope.IssuedDetailList[0].BalanceToIssue;
			}
		});
	}

	// # end region

	$scope.TransformationChildModelTemp = {
		Id: null,
		TransformationIssueReturnMasterId: null,
		MaterialInputId: null,
		InputMaterialId: null,
		Quantity: null,
		Remarks: null,
		MaterialMasterArticleId: null,
		Value: null,
		LotNumber: null,

	};
	$scope.TransformationChild = Object.assign({}, $scope.TransformationChildModelTemp);

	


	$scope.ClearIssueTransformationChildTab = function () {
		ClearFieldsIssueTransformation();
		$scope.TransformationTypeList = [];
		//$scope.materialStockList = [];
		$scope.IssueTransformationChildList = [];

		$scope.MaterialInputList = [];
		$scope.detailList = [];
		$scope.getData();
		$scope.materialStockList = [];
		$scope.specificStockList = [];
		$scope.getSpecificMaterialStockForSlipIssue();

	}

	$scope.DownloadIssueTransformationReport = function (data) {
		try {
			$scope.PrintTabId = $scope.Transformation.Id;
			$scope.IssueId = $scope.IssueTransformation.Id;
			var reportFormat = "Excel";
			window.open('Outsourcing/OSIssueReturn/GetTransformationPrintReport?reportFormat=' + reportFormat + '&PrintTabId=' + $scope.PrintTabId + '&IssueId=' + $scope.IssueId, '_blank');
			//       $scope.getData();

		} catch (e) {

		}
	};



	//

	$scope.CostCenterLoad = function () {
		
		$http({
			method: "GET",
			url: 'Outsourcing/OSIssueReturn/GetCostCenterLoadNewFun?EntityId=' + $scope.ModelNew.EntityId
		}).then(function successCallback(response) {
			$scope.costCenterAllList = response.data;
		});
	}

	$scope.CostCenterLoad();

	$scope.CostCenterLoadNew = function () {
		
		$http({
			method: "GET",
			url: 'Outsourcing/OSIssueReturn/GetCostCenterLoadNewFun?EntityId=' + $scope.IssueTransformation.EntityId
		}).then(function successCallback(response) {
			$scope.costCenterList = response.data;
		});
	}

	$scope.CostCenterLoadNew();

	//#region Expense activity select
	$scope.setSelected = function (data) {
		//;
		$scope.addRow(data);
		$scope.closeCOAICodeListPopUp();
		$scope.setSelectedforGL(data);
	};

	$scope.addRow = function (data) {
		$scope.detailModel.GLGeneralInfoId = data.GLGeneralInfoId;
		$scope.detailModel.BudgetMasterId = data.BudgetMasterId;
		$scope.detailModel.ActivityId = data.ActivityId;
		$scope.detailModel.BudgetName = data.BudgetName;
		$scope.getActivity(data);
	};
	$scope.activityList = [];
	$scope.getActivity = function (data) {
		cboService.getBudgetMasterActivityCbo(data.BudgetMasterId, function (result) {
			$scope.detailModel.ActivityId = null;
			$scope.activityList = [];
			$scope.activityList = result;
			$scope.detailModel.ActivityId = data.ActivityId;

		});
	};
	$scope.searchglByList = [
		{
			"name": "Account Group",
			"value": "AccountGroupName"
		},
		{
			"name": "GL Code",
			"value": "GLGeneralInfoCode"
		},
		{
			"name": "GL Name",
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
			"name": "RefNo",
			"value": "RefNo"
		}
	];
	$scope.glListParameters = {
		limit: 10,
		offset: 0,
		order: "asc",
		sort: "GLGeneralInfoCode",
		searchBy: "ActivityName",
		pageSize: 10,
		total_count: 0,
		search: null,
		serverPagination: true
	};

	$scope.popUp = function (index) {
		//;
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


	$scope.setSelectedforGL = function (data) {
		//;
		$scope.MaterialInputList[$scope.issueSlipDetailIndex].GLGeneralInfoId = data.GLGeneralInfoId;
		$scope.MaterialInputList[$scope.issueSlipDetailIndex].BudgetMasterId = data.BudgetMasterId;
		$scope.MaterialInputList[$scope.issueSlipDetailIndex].ExpenseActivityId = data.ActivityId;
		$scope.MaterialInputList[$scope.issueSlipDetailIndex].ActivityName = data.GLGeneralInfoCode + '-' + data.ActivityName;
		$scope.MaterialInputList[$scope.issueSlipDetailIndex].BudgetName = data.BudgetName;
		angular.element(document.querySelector("#GLPopUp")).modal("hide");
	};


	//#endregion



	$scope.materialStockList = [];
	$scope.specificStockList = [];
	$scope.issueSpecificIndex = null;
	$scope.modalSpecificMaterialName = null;
	$scope.modalSpecificArticleName = null;
	$scope.modalSpecificOSTCMasterId = null;
	$scope.modalSpecificOSOutputItem = null;
	$scope.modalSpecificOSInputItem = null;
	$scope.getSpecificMaterialStockForSlipIssue = function (data, index) {

		for (var i = 0; i < $scope.detailList.length; i++) {
			if ($scope.detailList[i].isSelectedMatInput == true && !baseService.isUndefinedOrNull($scope.detailList[i].ArticleId)) {
				if ($scope.detailList[i].TransactionQty > $scope.detailList[i].PostingQty) {
					ShowResult("Issue qty can not gaterthen  Ready for issue Qty");
					return false;
				}
            }
		}
		for (var i = 0; i < $scope.detailList.length; i++) {
			if ($scope.detailList[i].isSelectedMatInput == true && !baseService.isUndefinedOrNull($scope.detailList[i].ArticleId)) {
				if ($scope.detailList[i].TransactionQty > $scope.detailList[i].RequestedQty) {
					ShowResult("Issue qty can not gaterthen Requested Qty");
					return false;
				}
			}
		}
		$scope.modalSpecificMaterialName = data.MaterialMaster;
		$scope.modalSpecificArticleName = data.ArticleName;
		$scope.modalSpecificOSTCMasterId = data.OSTransformationPODetailId;
		$scope.modalSpecificOSOutputItem = data.JWOutputItem;
		$scope.modalSpecificOSInputItem = data.JWInputItem;

		$scope.issueSpecificIndex = index;
		$scope.index = index;
		//data.MaterialStorageId = $scope.IssueTransformation.MaterialStorageId;
		if ($scope.IssueTransformation.IssueCategory =='Inventory') {
			$scope.getUrl = 'Products/InventoryIssue/GetSpecificMaterialStock/'
        }
		else {
			$scope.getUrl = 'Outsourcing/OSIssueReturn/GetProductionSummaryProcess?articleId=' + data.ArticleId
        }
		$http({
			method: 'POST'
			, url: $scope.getUrl
			, data: { entity: data, issueDate: $scope.IssueTransformation.IssueDate }
			, dataType: 'JSON'
		}).then(function (response) {
			$scope.materialStockList = response.data;

			if ($scope.IssueTransformation.IssueCategory == 'Inventory') {
				if ($scope.materialStockList.length > 0) {

					for (var i = 0; i < baseService.arrayLength($scope.specificStockList); i++) {
						var row = $scope.specificStockList[i];
						for (var t = 0; t < baseService.arrayLength($scope.materialStockList); t++) {
							var newRow = $scope.materialStockList[t];
							if (newRow.InventoryReceiveDetailId === row.InventoryReceiveDetailId) {
								newRow.Flag = true;
								newRow.RequisitionQty = row.RequisitionQty;
								break;
							}
						}
					}
					for (var i1 = 0; i1 < $scope.materialStockList.length; i1++) {
						//$scope.materialStockList[i1].TrasactopmUomQty = $scope.materialStockList[i1].BalanceStock / data.BaseUoMFactor;
						$scope.materialStockList[i1].IssueTransactionUoMId = data.TransactionUoMId;
						$scope.materialStockList[i1].IssueTransactionUoM = data.TransactionUoM;

						$scope.materialStockList[i1].TransactionUoMId = data.TransactionUoMId;
						//$scope.materialStockList[i1].BaseUoMFactor = data.BaseUoMFactor;
					}
					angular.element(document.querySelector('#stockPopUp')).modal('show');

					if ($scope.Action == "Update") {
						$scope.InventoryIssueDetailId = data.InventoryIssueDetailId;
						$http({
							method: 'GET',
							url: $scope.path + 'GetGRNRowId?InventoryIssueDetailId=' + $scope.InventoryIssueDetailId
						}).then(function successCallback(response) {
							$scope.GRNRowIdList = response.data;
							if ($scope.GRNRowIdList.length > 0) {
								for (var i = 0; i < $scope.GRNRowIdList.length; i++) {
									//var getRow = $filter("filter")($scope.materialStockList, { "InventoryReceiveDetailId": $scope.GRNRowIdList[i].InventoryReceiveDetailId });
									//if (getRow.length === 1) {

									//                           }

									for (var j = 0; j < $scope.materialStockList.length; j++) {
										if ($scope.GRNRowIdList[i].InventoryReceiveDetailId == $scope.materialStockList[j].InventoryReceiveDetailId) {
											$scope.materialStockList[j].RequisitionQty = $scope.GRNRowIdList[i].Qty;
											$scope.materialStockList[j].Flag = true;
										}
									}
								}

							}
						});
					}

				}
			}
			else {

				angular.element(document.querySelector('#PSProcessPopUp')).modal('show');
            }

		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};
	};

	$scope.GRNRowIdList = [];
	$scope.materialStockList = [];
	$scope.specificStockList = [];
	$scope.getSpecificMaterialStockForSlipIssueVA = function (data, index) {

		for (var i = 0; i < $scope.IssueChildList.length; i++) {
			if ($scope.IssueChildList[i].isSelectedOM == true && !baseService.isUndefinedOrNull($scope.IssueChildList[i].ArticleId)) {
				if ($scope.IssueChildList[i].TransactionQty > $scope.IssueChildList[i].PostingQty) {
					ShowResult("Issue quantity cannot be greater than  Ready for Issue Quantity");
					return false;
				}
			}
		}
		for (var i = 0; i < $scope.IssueChildList.length; i++) {
			if ($scope.IssueChildList[i].isSelectedOM == true && !baseService.isUndefinedOrNull($scope.IssueChildList[i].ArticleId)) {
				if ($scope.IssueChildList[i].TransactionQty > $scope.IssueChildList[i].RequestedQty) {
					ShowResult("Issue quantity cannot be greater than Requested Quantity");
					return false;
				}
			}
		}

		$scope.index = index;
	//	data.MaterialStorageId = $scope.Issue.MaterialStorageId;
		data.MaterialStorageId = $scope.Issue.MSIdInventory;
		$http({
			method: 'POST'
			, url: 'Products/InventoryIssue/GetSpecificMaterialStock/'
			, data: { entity: data, issueDate: $scope.Issue.IssueDate }
			, dataType: 'JSON'
		}).then(function (response) {
			$scope.materialStockList = response.data;
			if ($scope.materialStockList.length > 0) {

				for (var i = 0; i < baseService.arrayLength($scope.specificStockList); i++) {
					var row = $scope.specificStockList[i];
					for (var t = 0; t < baseService.arrayLength($scope.materialStockList); t++) {
						var newRow = $scope.materialStockList[t];
						if (newRow.InventoryReceiveDetailId === row.InventoryReceiveDetailId) {
							newRow.Flag = true;
							newRow.RequisitionQty = row.RequisitionQty;
							break;
						}
					}
				}
				for (var i1 = 0; i1 < $scope.materialStockList.length; i1++) {
					//$scope.materialStockList[i1].TrasactopmUomQty = $scope.materialStockList[i1].BalanceStock / data.BaseUoMFactor;
					$scope.materialStockList[i1].IssueTransactionUoMId = data.TransactionUoMId;
					$scope.materialStockList[i1].IssueTransactionUoM = data.TransactionUoM;

					$scope.materialStockList[i1].TransactionUoMId = data.TransactionUoMId;
					//$scope.materialStockList[i1].BaseUoMFactor = data.BaseUoMFactor;
				}
			//	angular.element(document.querySelector('#stockPopUp')).modal('show');
				angular.element(document.querySelector('#stockPopUpValAdded')).modal('show');

				if ($scope.Action == "Update") {
					$scope.InventoryIssueDetailId = data.InventoryIssueDetailId;
						$http({
							method: 'GET',
							url: $scope.path + 'GetGRNRowId?InventoryIssueDetailId=' + $scope.InventoryIssueDetailId
						}).then(function successCallback(response) {
							$scope.GRNRowIdList = response.data;
							if ($scope.GRNRowIdList.length > 0) {
								for (var i = 0; i < $scope.GRNRowIdList.length; i++) {
									//var getRow = $filter("filter")($scope.materialStockList, { "InventoryReceiveDetailId": $scope.GRNRowIdList[i].InventoryReceiveDetailId });
									//if (getRow.length === 1) {

         //                           }

									for (var j = 0; j < $scope.materialStockList.length; j++) {
										if ($scope.GRNRowIdList[i].InventoryReceiveDetailId == $scope.materialStockList[j].InventoryReceiveDetailId) {
											$scope.materialStockList[j].RequisitionQty = $scope.GRNRowIdList[i].Qty;
											$scope.materialStockList[j].Flag = true;
                                        }
                                    }
                                }
								
							}
						});
					}

			}

		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};
	};

	$scope.addMaterialStockValAdded = function () {
		//;
		try {
			var sumOfmaterialStockList = $filter('sumByKey')($filter('filter')($scope.materialStockList), 'RequisitionQty');
			if (sumOfmaterialStockList > $scope.selectedRowQty) {
				ShowResult("Issue qty can not grater than requisition qty", 'failure', 'stockPopUpValAdded');
				return false;
			}
			if (sumOfmaterialStockList < $scope.selectedRowQty) {
				ShowResult("Issue qty can not less than requisition qty", 'failure', 'stockPopUpValAdded');
				return false;
			}
			for (var t1 = 0; t1 < baseService.arrayLength($scope.materialStockList); t1++) {
				if (($scope.materialStockList[t1].IssueByUoM === 'Yes' && $scope.materialStockList[t1].Flag === true) && ($scope.materialStockList[t1].TransactionUoMId != $scope.materialStockList[t1].IssueTransactionUoMId)) {
					ShowResult("Your Transaction UoM is not equal to requested UoM.So you can not issue this material", 'failure', 'stockPopUpValAdded');
					return false;
				}
				if ($scope.materialStockList[t1].RequisitionQty > 0 && $scope.materialStockList[t1].Flag == 0) {
					ShowResult("select The given qty row", 'failure', 'stockPopUpValAdded');
					return false;
				}
				if (baseService.isUndefinedOrNull($scope.materialStockList[t1].RequisitionQty) && $scope.materialStockList[t1].Flag == 1) {
					ShowResult("Enter the qty for selected row ", 'failure', 'stockPopUpValAdded');
					return false;
				}
				if (baseService.isUndefinedOrNull($scope.materialStockList[t1].RequisitionQty) === 0 && $scope.materialStockList[t1].Flag == 1) {
					ShowResult("Enter the qty for selected row ", 'failure', 'stockPopUpValAdded');
					return false;
				}
			}
			qtyValidationValAdded($scope.materialStockList);
			validationWithTotalValAdded($scope.materialStockList);
			for (var i = baseService.arrayLength($scope.specificStockList) - 1; i >= 0; i--) {
				var row = $scope.specificStockList[i];
				for (var t = 0; t < baseService.arrayLength($scope.materialStockList); t++) {

					var newRow = $scope.materialStockList[t];
					if (row.InventoryReceiveDetailId === newRow.InventoryReceiveDetailId) { // update or delete
						if (newRow.Flag) row.RequisitionQty = newRow.RequisitionQty;
						else $scope.specificStockList.splice(i, 1);
					}
				}
			}
			for (var n = 0; n < baseService.arrayLength($scope.materialStockList); n++) { // add
				var nRow = $scope.materialStockList[n];
				nRow.BaseQty = $scope.materialStockList[n].BaseQty;
				nRow.BaseIssueQty = $scope.materialStockList[n].BaseIssueQty;
				if (!baseService.valueCheckInList($scope.specificStockList, 'InventoryReceiveDetailId', nRow.InventoryReceiveDetailId) && nRow.Flag)
					//$scope.detailModel.IsSpecific = true;
					$scope.specificStockList.push(nRow);
			}
			//$scope.detailList[$scope.index].TransactionQty = issueQty;
			angular.element(document.querySelector('#stockPopUpValAdded')).modal('hide');
			CloseModalShowResult();
		} catch (e) {
			ShowResult(e, 'failure', 'stockPopUpValAdded');
		}
	};

	$scope.closeStockPopUpValAdded = function () {
		angular.element(document.querySelector('#stockPopUpValAdded')).modal('hide');
	};

	function qtyValidationValAdded(list) {
		for (var i = 0; i < baseService.arrayLength(list); i++) {
			if (list[i].Flag) {
				if (parseFloat(list[i].RequisitionQty) > parseFloat(list[i].StockQty)) throw 'Requisition Qty can\'t greater than stock qty.';
			}
		}
	}
	function validationWithTotalValAdded(list) {
		var totalQty = 0;
		for (var i = 0; i < baseService.arrayLength(list); i++) {
			list[i].RequisitionQty = baseService.isUndefinedOrNull(list[i].RequisitionQty) === true ? 0 : parseFloat(list[i].RequisitionQty);
			if (list[i].Flag) {
				if (parseFloat(list[i].RequisitionQty) === 0)
					throw 'Please input requisition qty';
				else {
					if (list[i].TransactionUoMId !== list[i].BaseUOMId)
						totalQty += (Math.round((list[i].RequisitionQty * list[i].BaseUoMFactor) * 100 + Number.EPSILON) / 100);
					else totalQty += (Math.round((list[i].RequisitionQty) * 100 + Number.EPSILON) / 100);

				}
			}
		}
	//	var qty = parseFloat($scope.detailList[$scope.index].TransactionQty) * parseFloat($scope.detailList[$scope.index].BaseUoMFactor);
		var qty = parseFloat($scope.IssueChildList[$scope.index].TransactionQty) * parseFloat($scope.IssueChildList[$scope.index].BaseUoMFactor);

		//if (totalQty > qty && qty !== totalQty) throw 'Issue qty can\'t over ' + qty + ' .';
		//if (totalQty < qty && qty !== totalQty) throw 'Issue qty can\'t less ' + qty + ' .';

	}


	function qtyValidation(list) {
		for (var i = 0; i < baseService.arrayLength(list); i++) {
			if (list[i].Flag) {
				if (parseFloat(list[i].RequisitionQty) > parseFloat(list[i].StockQty)) throw 'Requisition Qty can\'t greater than stock qty.';
			}
		}
	}
	function validationWithTotal(list) {
		var totalQty = 0;
		for (var i = 0; i < baseService.arrayLength(list); i++) {
			list[i].RequisitionQty = baseService.isUndefinedOrNull(list[i].RequisitionQty) === true ? 0 : parseFloat(list[i].RequisitionQty);
			if (list[i].Flag) {
				if (parseFloat(list[i].RequisitionQty) === 0)
					throw 'Please input requisition qty';
				else {
					if (list[i].TransactionUoMId !== list[i].BaseUOMId) totalQty += parseFloat(list[i].RequisitionQty) * parseFloat(list[i].BaseUoMFactor);
					else totalQty += parseFloat(list[i].RequisitionQty).toFixed(2);
				}
			}
		}
		var qty = parseFloat($scope.MaterialInputList[$scope.index].TransactionQty) * parseFloat($scope.MaterialInputList[$scope.index].BaseUoMFactor);
		//if (totalQty > qty && qty !== totalQty) throw 'Issue qty can\'t over ' + qty + ' .';
		//if (totalQty < qty && qty !== totalQty) throw 'Issue qty can\'t less ' + qty + ' .';

	}

	$scope.removeRowModal = function (ob, index) {
		try {
			$scope.delData = ob;
			$scope.message_confirmation = "Are you sure want to permanent delete [" + ob.MaterialMasterName + "] ";
			angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
			$scope.popUpIndex = index;
		}
		catch (e) {
			ShowResult(e, 'Error');
		}
	};
	$scope.removeRow = function () {
		if (!baseService.isUndefinedOrNull($scope.delData.Id)) {
			$http({
				method: 'POST'
				, url: $scope.deleteUrl + '?issueDetailId=' + $scope.delData.Id
				, dataType: 'JSON'
			}).then(function (response) {
				if (response.data.Error === true)
					ShowResult(response.data.Message, 'failure');
				else
					ShowResult(response.data.Message, 'success');
			}), function (response) {
				ShowResult(response.data.Message, 'failure');
			};
		}
		for (var i = 0; i < baseService.arrayLength($scope.specificStockList); i++) {
			if ($scope.specificStockList[i].InventoryMaterialId === $scope.delData.InventoryMaterialId)
				$scope.specificStockList.splice(i, 1);
		}
		$scope.MaterialInputList.splice($scope.popUpIndex, 1);
		$scope.delData = null;
		$scope.popUpIndex = -1;
		angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
	};


	$scope.ApprovedStockList = [];
	$scope.getApprovedStock = function (data) {
		$http({
			method: 'POST'
			, url: $scope.path + 'GetApprovedStockDetail'
			, data: { entity: data, issueDate: $scope.productNew.IssueDate }
			, dataType: 'JSON'
		}).then(function (response) {
			$scope.ApprovedStockList = response.data;
			angular.element(document.querySelector('#ApprovedStockPopUp')).modal('show');
		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};
	};

	$scope.closeApprovedStockPopUp = function () {
		angular.element(document.querySelector('#ApprovedStockPopUp')).modal('hide');
	};

	$scope.ApprovedStockBeyondIssueDateList = [];
	$scope.getApprovedStockDetailBeyondIssueDate = function (data) {
		$http({
			method: 'POST'
			, url: $scope.path + 'GetApprovedStockDetailBeyondIssueDate'
			, data: { entity: data, issueDate: $scope.productNew.IssueDate }
			, dataType: 'JSON'
		}).then(function (response) {
			$scope.ApprovedStockBeyondIssueDateList = response.data;
		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};
	};

	$scope.PostingStockList = [];
	$scope.getPostingStock = function (data) {
		$http({
			method: "POST",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/InventoryIssue/GetPostingStockDetail',
			data: { entity: data, issueDate: $scope.productNew.IssueDate }

		}).then(function successCallback(response) {
			$scope.PostingStockList = response.data;
			angular.element(document.querySelector('#PostingStockPopUp')).modal('show');
			//entrydata = copy(searchdata);
		});
	};

	
	$scope.closePostingStockPopUp = function () {
		angular.element(document.querySelector('#PostingStockPopUp')).modal('hide');
	};

	$scope.PostingStockBeyondIssueDateList = [];
	$scope.getPostingStockBeyondIssueDate = function (data) {
		$http({
			method: 'POST'
			, url: $scope.path + 'GetPostingStockDetailBeyondIssueDate'
			, data: { entity: data, issueDate: $scope.productNew.IssueDate }
			, dataType: 'JSON'
		}).then(function (response) {
			$scope.PostingStockBeyondIssueDateList = response.data;
		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};
	};

	$scope.UnApprovedStockList = [];
	$scope.getUnApprovedStock = function (data) {
		$http({
			method: 'POST'
			, url: $scope.path + 'GetUnApprovedStockDetail'
			, data: { entity: data, issueDate: $scope.productNew.IssueDate }
			, dataType: 'JSON'
		}).then(function (response) {
			$scope.UnApprovedStockList = response.data;
			angular.element(document.querySelector('#UnApprovedStockPopUp')).modal('show');
		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};
	};
	$scope.closeUnApprovedStockPopUp = function () {
		angular.element(document.querySelector('#UnApprovedStockPopUp')).modal('hide');
	};

	$scope.UnApprovedStockDetailBeyondIssueDateList = [];
	$scope.getUnApprovedStockDetailBeyondIssueDate = function (data) {
		$http({
			method: 'POST'
			, url: $scope.path + 'GetUnApprovedStockDetailBeyondIssueDate'
			, data: { entity: data, issueDate: $scope.productNew.IssueDate }
			, dataType: 'JSON'
		}).then(function (response) {
			$scope.UnApprovedStockDetailBeyondIssueDateList = response.data;
		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};
	};


	//#region sk
	$scope.detailModelTemp = [];
	function getMaterialStock(b) {

		$http({
			method: 'POST',
			url: 'Products/InventoryIssue/GetJWStock',
			data: { entity: $scope.detailModel, issueDate: $scope.IssueTransformation.IssueDate },
			dataType: 'JSON'
		}).then(function (response) {

			$scope.detailList[$scope.indexforDetail].TotalQty = response.data.TotalQty;
			$scope.detailList[$scope.indexforDetail].PostingQty = response.data.PostingQty;
			$scope.detailList[$scope.indexforDetail].PostingQuantity = response.data.PostingQuantity;
			$scope.detailList[$scope.indexforDetail].ApprovedQty = response.data.ApprovedQty;
			$scope.detailList[$scope.indexforDetail].UnApprovedQty = response.data.UnApprovedQty;
			if (baseService.isUndefinedOrNull($scope.detailList[$scope.indexforDetail].TotalQty))
				$scope.errorText = 'This material has no stock';
			else $scope.errorText = null;


		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};
	}
	$scope.getRequisitionList = function (issueDetailId) {
		$scope.materialStockList = [];
		$scope.specificStockList = [];
		$http({
			method: 'POST'
			, url: $scope.path + 'GetRequisitionList'
			, data: { issueDetailId: issueDetailId }
			, dataType: 'JSON'
		}).then(function (response) {
			$scope.materialStockList = response.data;
			angular.element(document.querySelector('#stockPopUp')).modal('show');
		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};
	};
	$scope.closeStockPopUp = function () {
		angular.element(document.querySelector('#stockPopUp')).modal('hide');
	};

	$scope.closePSProcessPopUp = function () {
		angular.element(document.querySelector('#PSProcessPopUp')).modal('hide');
	};


	var SelectedMaterialInputdata = [];
	var SelectedOutputMaterialdata = [];
	$scope.IssueChildList = [];
	$scope.SaveSlipIssue = function () {

		if ($scope.ModelNew.TabType == "Transformation") {
			var sumOfmaterialStockList = $filter('sumByKey')($filter('filter')($scope.specificStockList), 'RequisitionQty');
		//	$scope.selectedRowQty1 = $filter('sumByKey')($filter('filter')($scope.detailList), 'TransactionQty');
			var T2 = 0;
			for (var i = 0; i < $scope.detailList.length; i++) {
				if (!baseService.isUndefinedOrNull($scope.detailList[i].ArticleId) && $scope.detailList[i].isSelectedMatInput == true) {
					var T1 = $scope.detailList[i].TransactionQty;
					var T2 = T1 + T2;
					$scope.selectedRowQty1 = parseFloat(T2);
				}
			}

			if (sumOfmaterialStockList < $scope.selectedRowQty1) {
				ShowResult("Please select Specific GRN", 'failure');
				return false;
			}
		}
		else {
	
					var sumOfmaterialStockList = $filter('sumByKey')($filter('filter')($scope.specificStockList), 'RequisitionQty');

					/*$scope.selectedRowQty1 = $filter('sumByKey')($filter('filter')($scope.IssueChildList), 'TransactionQty');*/

			var T2 = 0;
			for (var i = 0; i < $scope.IssueChildList.length; i++) {
				if (!baseService.isUndefinedOrNull($scope.IssueChildList[i].ArticleId) && $scope.IssueChildList[i].isSelectedOM == true) {
					var T1 = $scope.IssueChildList[i].TransactionQty;
					var T2 = T1 + T2;
					$scope.selectedRowQty1 = parseFloat(T2).toFixed(4);
	
                }
			}

			if (sumOfmaterialStockList < $scope.selectedRowQty1) {
				ShowResult("Please select Specific GRN", 'failure');
				return false;
			}

        }
	
		if ($scope.ModelNew.TabType == "Transformation") {
			if (baseService.isUndefinedOrNull($scope.IssueTransformation.IssueDate)) {
				ShowResult("Select the issue date");
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.IssueTransformation.EntityId)) {
				ShowResult("Select the Entity");
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.IssueTransformation.MaterialStorageId)) {
				ShowResult("Select the Material Storage");
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.IssueTransformation.IssueType)) {
				ShowResult("Select the type");
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.IssueTransformation.EmpName)) {
				ShowResult("Select the By Whom");
				return false;
			}
		}
		else {
			if (baseService.isUndefinedOrNull($scope.Issue.IssueDate)) {
				ShowResult("Select the issue date");
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.Issue.EntityId)) {
				ShowResult("Select the Entity");
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.Issue.MaterialStorageId)) {
				ShowResult("Select the Material Storage");
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.Issue.IssueType)) {
				ShowResult("Select the type");
				return false;

			}
			if (baseService.isUndefinedOrNull($scope.Issue.ResponsiblePerson)) {
				ShowResult("Select the By Whom");
				return false;

			}
		}

		if ($scope.ModelNew.TabType == "Transformation") {
			for (var i = 0; i < $scope.detailList.length; i++) {

				/*if ($scope.detailList[i].isSelectedMatInput == true && !baseService.isUndefinedOrNull($scope.detailList[i].MaterialMasterId) && !baseService.isUndefinedOrNull($scope.detailList[i].ArticleId)) {*/
				if ($scope.detailList[i].isSelectedMatInput == true && !baseService.isUndefinedOrNull($scope.detailList[i].ArticleId)) {

					if ($scope.detailList[i].TransactionQty > $scope.detailList[i].PostingQty) {
						ShowResult("Issue qty can not gaterthen  Ready for issue Qty");
						return false;
					}
					//if ($scope.detailList[i].TransactionQty > $scope.detailList[i].BalanceQty) {
					//    ShowResult("Issue qty can not gaterthen  Balance Qty");
					//    return false;
					//}
					if (baseService.isUndefinedOrNull($scope.detailList[i].CostCenterId)) {
						ShowResult("Select the cost center");
						return false;
					}
					if (baseService.isUndefinedOrNull($scope.detailList[i].MaterialMaster)) {
						ShowResult("Select Material Master");
						return false;
					}
					if (baseService.isUndefinedOrNull($scope.detailList[i].ArticleName)) {
						ShowResult("Select ArticleName");
						return false;
					}
					if (baseService.isUndefinedOrNull($scope.detailList[i].TransactionQty)) {
						ShowResult("Enter the Issue Qty");
						return false;
					}
					if ($scope.detailList[i].TransactionQty == '0') {
						ShowResult("Enter the Issue Qty");
						return false;
					}

					if ($scope.materialStockList.length === 0) {
						ShowResult('Please select Specific GRN');
						return false;
					}
					var UIStatus = $("#SlipAssetIssueUI").val();

				}

				/*if ($scope.detailList[i].isSelectedMatInput == true && baseService.isUndefinedOrNull($scope.detailList[i].MaterialMasterId) && baseService.isUndefinedOrNull($scope.detailList[i].ArticleId)) {*/

				if ($scope.detailList[i].isSelectedMatInput == true) {
					if (baseService.isUndefinedOrNull($scope.detailList[i].TransactionQty)) {
						ShowResult("Enter the Issue Qty");
						return false;
					}
					if ($scope.detailList[i].TransactionQty == '0') {
						ShowResult("Enter the Issue Qty");
						return false;
					}
				}
			}
		}
		else {
			for (var i = 0; i < $scope.IssueChildList.length; i++) {

				/*if ($scope.IssueChildList[i].isSelectedOM == true && !baseService.isUndefinedOrNull($scope.IssueChildList[i].MaterialMasterId) && !baseService.isUndefinedOrNull($scope.detailList[i].ArticleId)) {*/
				if ($scope.IssueChildList[i].isSelectedOM == true && !baseService.isUndefinedOrNull($scope.IssueChildList[i].ArticleId)) {

					if ($scope.IssueChildList[i].TransactionQty > $scope.IssueChildList[i].PostingQty) {
						ShowResult("Issue qty can not gaterthen  Ready for issue Qty");
						return false;
					}
					//if ($scope.IssueChildList[i].TransactionQty > $scope.IssueChildList[i].BalanceQty) {
					//    ShowResult("Issue qty can not gaterthen  Balance Qty");
					//    return false;
					//}
					if (baseService.isUndefinedOrNull($scope.IssueChildList[i].CostCenterId)) {
						ShowResult("Select the cost center");
						return false;
					}
					if (baseService.isUndefinedOrNull($scope.IssueChildList[i].MaterialMaster)) {
						ShowResult("Select Material Master");
						return false;
					}
					if (baseService.isUndefinedOrNull($scope.IssueChildList[i].ArticleName)) {
						ShowResult("Select ArticleName");
						return false;
					}
					if (baseService.isUndefinedOrNull($scope.IssueChildList[i].TransactionQty)) {
						ShowResult("Enter the Issue Qty");
						return false;
					}
					if ($scope.IssueChildList[i].TransactionQty == '0') {
						ShowResult("Enter the Issue Qty");
						return false;
					}

					if ($scope.materialStockList.length === 0) {
						ShowResult('Please select Specific GRN');
						return false;
					}
					var UIStatus = $("#SlipAssetIssueUI").val();

				}

				/*if ($scope.IssueChildList[i].isSelectedOM == true && baseService.isUndefinedOrNull($scope.IssueChildList[i].MaterialMasterId) && baseService.isUndefinedOrNull($scope.detailList[i].ArticleId)) {*/

				if ($scope.IssueChildList[i].isSelectedOM == true) {
					if (baseService.isUndefinedOrNull($scope.IssueChildList[i].TransactionQty)) {
						ShowResult("Enter the Issue Qty");
						return false;
					}
					if ($scope.IssueChildList[i].TransactionQty == '0') {
						ShowResult("Enter the Issue Qty");
						return false;
					}
				}
			}
        }

		if ($scope.ModelNew.TabType == "Transformation") {
			for (var j = 0; j < $scope.detailList.length; j++) {
				if ($scope.detailList[j].isSelectedMatInput == true) {
					SelectedMaterialInputdata.push($scope.detailList[j]);
				}
			}
		}
		else {
			 SelectedOutputMaterialdata = [];
			for (var j = 0; j < $scope.IssueChildList.length; j++) {
				if ($scope.IssueChildList[j].isSelectedOM == true) {
					SelectedOutputMaterialdata.push($scope.IssueChildList[j]);
				}
			}
        }
		if ($scope.ModelNew.TabType == "Transformation") {
			$scope.IssueTransformation.MaterialStorageId = $scope.IssueTransformation.MaterialStorageIdInventory;
		}
		else {
			$scope.Issue.MaterialStorageId = $scope.Issue.MSIdInventory;
        }
		if ($scope.Action === "Save") {
			if ($scope.ModelNew.TabType == "Transformation") {
				if (SelectedMaterialInputdata.length > 0) {
					
					$http({
						method: 'POST'
						, url: 'Products/InventoryIssue/JWIssueCreate'
						, data: {
							entities: SelectedMaterialInputdata
							, specificStockList: $scope.specificStockList
							, inventoryIssue: $scope.IssueTransformation
							, IssueTypeStatus: 'Inventory'
							, TabType: $scope.ModelNew.TabType

						}
						, dataType: 'JSON'
					}).then(function (response) {
						if (response.data.Error === true)
							ShowResult(response.data.Message, 'failure');
						else {
							ShowResult(response.data.Message, 'success');
							$scope.getdataInventoryIssue();
							
						}
					}), function (response) {
						ShowResult(response.data.Message, 'failure');
					};
				}
				else ShowResult('Please issue material', 'failure');
			}
			else {
				if (SelectedOutputMaterialdata.length > 0) {
					$http({
						method: 'POST'
						, url: 'Products/InventoryIssue/JWIssueCreate'
						, data: {
							//	entities: $scope.detailList
							entities: SelectedOutputMaterialdata
							, specificStockList: $scope.specificStockList
							, inventoryIssue: $scope.Issue
							, IssueTypeStatus: 'Inventory'
							, TabType: $scope.ModelNew.TabType
						}
						, dataType: 'JSON'
					}).then(function (response) {
						if (response.data.Error === true)
							ShowResult(response.data.Message, 'failure');
						else {
							ShowResult(response.data.Message, 'success');
							$scope.ClearIssueChildTab();
							$scope.IssueChildList = [];
							$scope.getdataInventoryIssue();
						}
					}), function (response) {
						ShowResult(response.data.Message, 'failure');
					};
				}
				else ShowResult('Please issue material', 'failure');
            }
		}

		if ($scope.Action === "Update") {
			if ($scope.ModelNew.TabType == "Transformation") {
				if (SelectedMaterialInputdata.length > 0) {
					
					$http({
						method: 'POST'
						, url: 'Products/InventoryIssue/JWIssueCreate'
						, data: {
							entities: SelectedMaterialInputdata
							, specificStockList: $scope.specificStockList
							, inventoryIssue: $scope.IssueTransformation
							, IssueTypeStatus: 'Inventory'
							, TabType: $scope.ModelNew.TabType

						}
						, dataType: 'JSON'
					}).then(function (response) {
						if (response.data.Error === true)
							ShowResult(response.data.Message, 'failure');
						else {
							ShowResult(response.data.Message, 'success');
							$scope.getdataInventoryIssue();
						}
					}), function (response) {
						ShowResult(response.data.Message, 'failure');
					};
					//      }
				}
				else ShowResult('Please issue material', 'failure');
			}
			else {
				if (SelectedOutputMaterialdata.length > 0) {
					$http({
						method: 'POST'
						, url: 'Products/InventoryIssue/JWIssueCreate'
						, data: {
							//	entities: $scope.detailList
							entities: SelectedOutputMaterialdata
							, specificStockList: $scope.specificStockList
							, inventoryIssue: $scope.Issue
							, IssueTypeStatus: 'Inventory'
							, TabType: $scope.ModelNew.TabType

						}
						, dataType: 'JSON'
					}).then(function (response) {
						if (response.data.Error === true)
							ShowResult(response.data.Message, 'failure');
						else {
							ShowResult(response.data.Message, 'success');
							$scope.ClearIssueChildTab();
							$scope.IssueChildList = [];
						}
					}), function (response) {
						ShowResult(response.data.Message, 'failure');
					};
					//      }
				}
				else ShowResult('Please issue material', 'failure');
			}


		}
		
	};

	$scope.addMaterialStock = function () {
		//;
		try {
			qtyValidation($scope.materialStockList);
			validationWithTotal($scope.materialStockList);
			for (var i = baseService.arrayLength($scope.specificStockList) - 1; i >= 0; i--) {
				var row = $scope.specificStockList[i];
				for (var t = 0; t < baseService.arrayLength($scope.materialStockList); t++) {
					var newRow = $scope.materialStockList[t];
					if (row.InventoryReceiveDetailId === newRow.InventoryReceiveDetailId) { // update or delete
						if (newRow.Flag) row.RequisitionQty = newRow.RequisitionQty;
						else $scope.specificStockList.splice(i, 1);
					}
				}
			}
			for (var n = 0; n < baseService.arrayLength($scope.materialStockList); n++) { // add
				var nRow = $scope.materialStockList[n];
				nRow.BaseQty = $scope.materialStockList[n].BaseQty;
				nRow.BaseIssueQty = $scope.materialStockList[n].BaseIssueQty;
				nRow.TransactionUoMId = $scope.materialStockList[n].TransactionUoMId;
				//nRow.BaseQty = $scope.materialStockList[n].BaseIssueQty;
				if (!baseService.valueCheckInList($scope.specificStockList, 'InventoryReceiveDetailId', nRow.InventoryReceiveDetailId) && nRow.Flag)
					//$scope.detailModel.IsSpecific = true;
					$scope.specificStockList.push(nRow);
			}
			$scope.detailList[$scope.issueSpecificIndex].TransactionQty = $filter("sumByKey")($filter("filter")($scope.materialStockList, { Flag: true }), "RequisitionQty");
			//$scope.detailList[$scope.index].TransactionQty = issueQty;
			angular.element(document.querySelector('#stockPopUp')).modal('hide');
			$scope.modalSpecificMaterialName =null;
			$scope.modalSpecificArticleName = null;
			$scope.modalSpecificOSTCMasterId = null;
			$scope.modalSpecificOSOutputItem = null;
			$scope.modalSpecificOSInputItem = null;

			$scope.issueSpecificIndex = null;
			CloseModalShowResult();
		} catch (e) {
			ShowResult(e, 'failure', 'stockPopUp');
		}
	};
	//#endregion

	// PRINT JOB WORK TRANSFORMATION REPORT

	$scope.PrintIssueTemplateReport = function (data) {
		if ($scope.ModelNew.TabType == "Transformation") {
			//;
			//var x = "#" + z;
			//var gridObj = $(x).data("ejGrid");
			//var data = gridObj.getSelectedRecords()[0];
			location.href = "Products/InventoryIssue/JobWorkIssueReport?grnId=" + data.Id;
		}
		else {

			location.href = "Products/InventoryIssue/JWValAddedIssueReport?grnId=" + data.Id;
        }
		$scope.getdataInventoryIssue();
	

	};

	$scope.ConfirmIssueReportPrint = function (data) {
		try {
			if ($scope.ModelNew.TabType == "Transformation") {
				//var x = "#" + p;
				//var gridObj = $(x).data("ejGrid");
				//var data = gridObj.getSelectedRecords()[0];

				$scope.PrintTabId = data.JWContractId;
				$scope.IssueId = data.Id;
				var reportFormat = "Excel";
				window.open('Outsourcing/OSIssueReturn/GetTransformationPrintReport?reportFormat=' + reportFormat + '&PrintTabId=' + $scope.PrintTabId + '&IssueId=' + $scope.IssueId, '_blank');
				//   $scope.getData();
			}
			else {

				$scope.PrintTabId = data.JWContractId;
				$scope.IssueId = data.Id;
				var reportFormat = "Excel";
				window.open('Outsourcing/OSIssueReturn/GetValueAddedReport?reportFormat=' + reportFormat + '&PrintTabId=' + $scope.PrintTabId + '&IssueId=' + $scope.IssueId, '_blank');
			}
			$scope.getdataInventoryIssue();


		} catch (e) {

		}
	};

	// Transformation Stock Wise Status

	
	$scope.GetShowStorageLocationList = [];
	$scope.stockwisestatus = function (RowData, index) {
		if ($scope.ModelNew.TabType == "Transformation") {
			$scope.GetShowStorageLocationList = [];
			angular.element(document.querySelector("#ShowLOcationWiseStock")).modal("show");

			for (var i = 0; i < $scope.detailList.length > 0; i++) {
		//		if ($scope.detailList[i].Id === RowData.Id) {
				if ($scope.detailList[i].OSTransformationPODetailId === RowData.OSTransformationPODetailId && $scope.detailList[i].ArticleId === RowData.ArticleId) {
					$scope.MatMstId = $scope.detailList[i].InputMaterialId;
					// $scope.SelectedArticleId = $scope.detailList[i].MaterialMasterArticleId;
					$scope.SelectedArticleId = $scope.detailList[i].ArticleId;
					$scope.a = i;
				}
			}

			$http({
				method: 'POST',
				data: { MaterialMstId: $scope.MatMstId, ArticleId: $scope.SelectedArticleId, issueDate: $scope.IssueTransformation.IssueDate },
				url: 'Products/InventoryIssue/StorageLocationStockWise/'
			}).then(function successCallback(response) {
				$scope.GetShowStorageLocationList = response.data;
			});
		}
		else {
			$scope.GetShowStorageLocationList = [];
			angular.element(document.querySelector("#ShowLOcationWiseStock")).modal("show");

			for (var i = 0; i < $scope.IssueChildList.length > 0; i++) {
			//	if ($scope.IssueChildList[i].OSTransformationPOId === RowData.OSTransformationPOId && $scope.IssueChildList[i].ArticleId === RowData.ArticleId) {
				if ($scope.IssueChildList[i].OSTransformationPODetailId === RowData.OSTransformationPODetailId && $scope.IssueChildList[i].ArticleId === RowData.ArticleId) {
					$scope.MatMstId = $scope.IssueChildList[i].MaterialMasterId;
					// $scope.SelectedArticleId = $scope.IssueChildList[i].MaterialMasterArticleId;
					$scope.SelectedArticleId = $scope.IssueChildList[i].ArticleId;
					$scope.InventoryIssueDetailId = $scope.IssueChildList[i].InventoryIssueDetailId;
					$scope.a = i;
				}
			}

			$http({
				method: 'POST',
				data: { MaterialMstId: $scope.MatMstId, ArticleId: $scope.SelectedArticleId, issueDate: $scope.Issue.IssueDate },
				url: 'Products/InventoryIssue/StorageLocationStockWise/'
			}).then(function successCallback(response) {
				$scope.GetShowStorageLocationList = response.data;
			});
        }

	}

	$scope.GetPopUpShowStorageLocationClosed = function () {
		angular.element(document.querySelector('#ShowLOcationWiseStock')).modal('hide');

	}

	// Print Template
	$scope.AllTabPrint = function (data) {
		//var x = "#" + z;
		//var gridObj = $(x).data("ejGrid");
		//var data = gridObj.getSelectedRecords()[0];

		location.href = "Outsourcing/OSTransformationPO/GePurchaseOrderReport?purchaseOrderId=" + data.Id;
		$scope.getalldata();
	};

	//#region start Reports
	$scope.ConfirmPrintTab = function (data) {
		try {
			//var x = "#" + z;
			//var gridObj = $(x).data("ejGrid");
		//	var data = gridObj.getSelectedRecords()[0];
			//        location.href = "Products/InventoryIssue/JobWorkIssueReport?grnId=" + data.Id;

			$scope.PrintTabId = data.Id;

			var reportFormat = "Excel";
			if (data.POType == "OSTransformationPO") {
				window.open('Outsourcing/JobWorkValueAddedContract/GetTransformationContractReport?reportFormat=' + reportFormat + '&PrintTabId=' + $scope.PrintTabId, '_blank');
			}

			if (data.POType == "OSValueAddedPO") {
				window.open('Outsourcing/JobWorkValueAddedContract/GetValueAddedPrintReport?reportFormat=' + reportFormat + '&PrintTabId=' + $scope.PrintTabId, '_blank');
			}

		} catch (e) {

		}
	};

	$scope.qtyFuncValAdded = function (x, index) {
		//;
		// alert('qtyalert');
		var BaltoIssue;
		for (var i = 0; i < $scope.IssueChildList.length; i++) {
			if (baseService.isUndefinedOrNull($scope.IssueChildList[index].MaterialMasterId) && baseService.isUndefinedOrNull($scope.IssueChildList[index].ArticleId)) {
				if (!baseService.isUndefinedOrNull($scope.IssueChildList[i].JWOrderWiseId)) {
					if ($scope.IssueChildList[index].JWOrderWiseId === $scope.IssueChildList[i].JWOrderWiseId) {

						if ($scope.IssueChildList[i].TIRCTotalQty == null) {
							$scope.IssueChildList[i].TIRCTotalQty = 0;
						}
						if ($scope.IssueChildList[index].TransactionQty > Math.round(($scope.IssueChildList[i].RequiredQuantity - $scope.IssueChildList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100) {
							ShowResult("Issue quantity cannot be greater than Balance quantity");
							$scope.IssueChildList[index].TransactionQty = 0;
							//		$scope.IssueChildList[i].BalanceToIssue = ($scope.IssueChildList[i].RequiredQuantity - (Math.round(($scope.IssueChildList[index].TransactionQty + $scope.IssueChildList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
							BaltoIssue = ($scope.IssueChildList[i].RequiredQuantity - (Math.round(($scope.IssueChildList[index].TransactionQty + $scope.IssueChildList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
							$scope.IssueChildList[i].BalanceToIssue = BaltoIssue.toFixed(4);
							return false;
						}

						var BalanceToIssue = parseFloat($scope.IssueChildList[i].RequiredQuantity) - parseFloat($scope.IssueChildList[i].TIRCTotalQty);
						var RemBalance = BalanceToIssue - parseFloat($scope.IssueChildList[index].TransactionQty);
						$scope.IssueChildList[i].BalanceToIssue = RemBalance.toFixed(4);
					}
				}
				else {
			//		if (($scope.IssueChildList[index].OSTransformationPOId === $scope.IssueChildList[i].OSTransformationPOId) && ($scope.IssueChildList[index].JWOutputItem === $scope.IssueChildList[i].JWOutputItem)) {
					if (($scope.IssueChildList[index].OSTransformationPODetailId === $scope.IssueChildList[i].OSTransformationPODetailId) && ($scope.IssueChildList[index].JWOutputItem === $scope.IssueChildList[i].JWOutputItem)) {

						if ($scope.IssueChildList[i].TIRCTotalQty == null) {
							$scope.IssueChildList[i].TIRCTotalQty = 0;
						}
						if ($scope.IssueChildList[index].TransactionQty > Math.round(($scope.IssueChildList[i].RequiredQuantity - $scope.IssueChildList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100) {
							ShowResult("Issue quantity cannot be greater than Balance quantity");
							$scope.IssueChildList[index].TransactionQty = 0;
							//		$scope.IssueChildList[i].BalanceToIssue = ($scope.IssueChildList[i].RequiredQuantity - (Math.round(($scope.IssueChildList[index].TransactionQty + $scope.IssueChildList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
							BaltoIssue = ($scope.IssueChildList[i].RequiredQuantity - (Math.round(($scope.IssueChildList[index].TransactionQty + $scope.IssueChildList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
							$scope.IssueChildList[i].BalanceToIssue = BaltoIssue.toFixed(4);
							return false;
						}

						var BalanceToIssue = parseFloat($scope.IssueChildList[i].RequiredQuantity) - parseFloat($scope.IssueChildList[i].TIRCTotalQty);
						var RemBalance = BalanceToIssue - parseFloat($scope.IssueChildList[index].TransactionQty);
						$scope.IssueChildList[i].BalanceToIssue = RemBalance.toFixed(4);
					}
                }
			

			}
			if (!baseService.isUndefinedOrNull($scope.IssueChildList[index].MaterialMasterId) && !baseService.isUndefinedOrNull($scope.IssueChildList[index].ArticleId)) {
				if (($scope.IssueChildList[index].MaterialMasterId === $scope.IssueChildList[i].MaterialMasterId) && $scope.IssueChildList[index].ArticleId === $scope.IssueChildList[i].ArticleId) {

					//if ((Math.round(($scope.IssueChildList[index].TransactionQty + $scope.IssueChildList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100) > Math.round(($scope.IssueChildList[i].PostingQty) * 100 + Number.EPSILON) / 100) {
					//	ShowResult("Issue qty must be less than or equal Ready for Issue Qty");
					//	$scope.IssueChildList[index].TransactionQty = 0;
					//	$scope.IssueChildList[i].BalanceToIssue = ($scope.IssueChildList[i].RequiredQuantity - (Math.round(($scope.IssueChildList[index].TransactionQty + $scope.IssueChildList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
					//	return false;
					//	//throw 'Issue qty must be less than or equal Ready for Issue Qty.';
					//}

					if ((Math.round(($scope.IssueChildList[index].TransactionQty) * 100 + Number.EPSILON) / 100) > Math.round(($scope.IssueChildList[i].PostingQty) * 100 + Number.EPSILON) / 100) {
						ShowResult("Issue quantity must be less than or equal to Ready for Issue Quantity");
						$scope.IssueChildList[index].TransactionQty = 0;
						$scope.IssueChildList[i].BalanceToIssue = ($scope.IssueChildList[i].RequiredQuantity - (Math.round(($scope.IssueChildList[index].TransactionQty + $scope.IssueChildList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
						return false;
						//throw 'Issue qty must be less than or equal Ready for Issue Qty.';
					}

					if ($scope.IssueChildList[index].TransactionQty > Math.round(($scope.IssueChildList[i].RequiredQuantity) * 100 + Number.EPSILON) / 100) {
						ShowResult("Transaction Qty cannot greater than Requested qty");
						$scope.IssueChildList[index].TransactionQty = 0;
						$scope.IssueChildList[i].BalanceToIssue = ($scope.IssueChildList[i].RequiredQuantity - (Math.round(($scope.IssueChildList[index].TransactionQty + $scope.IssueChildList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
						return false;
						//throw 'Issue qty must be less than or equal Ready for Issue Qty.';
					}

					//if ($scope.IssueChildList[index].TransactionQty > $scope.IssueChildList[i].BalanceQty) {
					//	ShowResult("Issue qty must be less than or equal BalanceQty Qty");
					//	return false;
					//	//throw 'Issue qty must be less than or equal Ready for Issue Qty.';
					//}

					if ($scope.IssueChildList[i].TIRCTotalQty == null) {
						$scope.IssueChildList[i].TIRCTotalQty = 0;
					}
					if ($scope.IssueChildList[index].TransactionQty > Math.round(($scope.IssueChildList[i].RequiredQuantity - $scope.IssueChildList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100) {
						ShowResult("Issue quantity cannot be greater than Balance quantity");
						$scope.IssueChildList[index].TransactionQty = 0;
						//		$scope.IssueChildList[i].BalanceToIssue = ($scope.IssueChildList[i].RequiredQuantity - (Math.round(($scope.IssueChildList[index].TransactionQty + $scope.IssueChildList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
						BaltoIssue = ($scope.IssueChildList[i].RequiredQuantity - (Math.round(($scope.IssueChildList[index].TransactionQty + $scope.IssueChildList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
						$scope.IssueChildList[i].BalanceToIssue = BaltoIssue.toFixed(4);
						return false;
					}

					if ($scope.IssueChildList[index].TransactionQty > Math.round(($scope.IssueChildList[i].PostingQty) * 100 + Number.EPSILON) / 100) {
						ShowResult("Issue quantity cannot be greater than Ready for Issue quantity");
						$scope.IssueChildList[index].TransactionQty = 0;
						//		$scope.IssueChildList[i].BalanceToIssue = ($scope.IssueChildList[i].RequiredQuantity - (Math.round(($scope.IssueChildList[index].TransactionQty + $scope.IssueChildList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
						BaltoIssue = ($scope.IssueChildList[i].RequiredQuantity - (Math.round(($scope.IssueChildList[index].TransactionQty + $scope.IssueChildList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
						$scope.IssueChildList[i].BalanceToIssue = BaltoIssue.toFixed(4);
						return false;
					}


					BaltoIssue = ($scope.IssueChildList[i].RequiredQuantity - (Math.round(($scope.IssueChildList[index].TransactionQty + $scope.IssueChildList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));

					$scope.IssueChildList[i].BalanceToIssue = BaltoIssue.toFixed(4);
				}
			}


		}

	}

	$scope.qtyFunc = function (x, index) {
		//;
		// alert('qtyalert');
		var BaltoIssue;
		for (var i = 0; i < $scope.detailList.length; i++) {
			if (baseService.isUndefinedOrNull($scope.detailList[index].MaterialMasterId) && baseService.isUndefinedOrNull($scope.detailList[index].ArticleId)) {
		//		if (($scope.detailList[index].OSTransformationPOId === $scope.detailList[i].OSTransformationPOId) && ($scope.detailList[index].JWInputItem === $scope.detailList[i].JWInputItem)) {
				if (($scope.detailList[index].OSTransformationPODetailId === $scope.detailList[i].OSTransformationPODetailId) && ($scope.detailList[index].JWInputItem === $scope.detailList[i].JWInputItem)) {

					if ($scope.detailList[i].TIRCTotalQty == null) {
						$scope.detailList[i].TIRCTotalQty = 0;
					}
					if ($scope.detailList[index].TransactionQty > Math.round(($scope.detailList[i].RequiredQuantity - $scope.detailList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100) {
						ShowResult("Issue quantity cannot be greater than Balance quantity");
						$scope.detailList[index].TransactionQty = 0;
						//		$scope.detailList[i].BalanceToIssue = ($scope.detailList[i].RequiredQuantity - (Math.round(($scope.detailList[index].TransactionQty + $scope.detailList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
						BaltoIssue = ($scope.detailList[i].RequiredQuantity - (Math.round(($scope.detailList[index].TransactionQty + $scope.detailList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
						$scope.detailList[i].BalanceToIssue = BaltoIssue.toFixed(4);
						return false;
					}

					var BalanceToIssue = parseFloat($scope.detailList[i].RequiredQuantity) - parseFloat($scope.detailList[i].TIRCTotalQty);
				    var	RemBalance = BalanceToIssue - parseFloat($scope.detailList[index].TransactionQty);
					$scope.detailList[i].BalanceToIssue = RemBalance.toFixed(4);
                }

			}
			if (!baseService.isUndefinedOrNull($scope.detailList[index].MaterialMasterId) && !baseService.isUndefinedOrNull($scope.detailList[index].ArticleId)) {
				if (($scope.detailList[index].MaterialMasterId === $scope.detailList[i].MaterialMstId) && $scope.detailList[index].ArticleId === $scope.detailList[i].ArticleId) {

					if ((Math.round(($scope.detailList[index].TransactionQty + $scope.detailList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100) > Math.round(($scope.detailList[i].PostingQty) * 100 + Number.EPSILON) / 100) {
						ShowResult("Issue qty must be less than or equal Ready for Issue Qty");
						$scope.detailList[index].TransactionQty = 0;
						$scope.detailList[i].BalanceToIssue = ($scope.detailList[i].RequiredQuantity - (Math.round(($scope.detailList[index].TransactionQty + $scope.detailList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
						return false;
						//throw 'Issue qty must be less than or equal Ready for Issue Qty.';
					}

					if ($scope.detailList[index].TransactionQty > Math.round(($scope.detailList[i].RequiredQuantity) * 100 + Number.EPSILON) / 100) {
						ShowResult("Transaction Qty cannot greater than Requested qty");
						$scope.detailList[index].TransactionQty = 0;
						$scope.detailList[i].BalanceToIssue = ($scope.detailList[i].RequiredQuantity - (Math.round(($scope.detailList[index].TransactionQty + $scope.detailList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
						return false;
						//throw 'Issue qty must be less than or equal Ready for Issue Qty.';
					}

					//if ($scope.detailList[index].TransactionQty > $scope.detailList[i].BalanceQty) {
					//	ShowResult("Issue qty must be less than or equal BalanceQty Qty");
					//	return false;
					//	//throw 'Issue qty must be less than or equal Ready for Issue Qty.';
					//}

					if ($scope.detailList[i].TIRCTotalQty == null) {
						$scope.detailList[i].TIRCTotalQty = 0;
					}
					if ($scope.detailList[index].TransactionQty > Math.round(($scope.detailList[i].RequiredQuantity - $scope.detailList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100) {
						ShowResult("Issue quantity cannot be greater than Balance quantity");
						$scope.detailList[index].TransactionQty = 0;
						//		$scope.detailList[i].BalanceToIssue = ($scope.detailList[i].RequiredQuantity - (Math.round(($scope.detailList[index].TransactionQty + $scope.detailList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
						BaltoIssue = ($scope.detailList[i].RequiredQuantity - (Math.round(($scope.detailList[index].TransactionQty + $scope.detailList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
						$scope.detailList[i].BalanceToIssue = BaltoIssue.toFixed(4);
						return false;
					}

					if ($scope.detailList[index].TransactionQty > Math.round(($scope.detailList[i].PostingQty) * 100 + Number.EPSILON) / 100) {
						ShowResult("Issue quantity cannot be greater than Ready for Issue quantity");
						$scope.detailList[index].TransactionQty = 0;
						//		$scope.detailList[i].BalanceToIssue = ($scope.detailList[i].RequiredQuantity - (Math.round(($scope.detailList[index].TransactionQty + $scope.detailList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
						BaltoIssue = ($scope.detailList[i].RequiredQuantity - (Math.round(($scope.detailList[index].TransactionQty + $scope.detailList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));
						$scope.detailList[i].BalanceToIssue = BaltoIssue.toFixed(4);
						return false;
					}


					BaltoIssue = ($scope.detailList[i].RequiredQuantity - (Math.round(($scope.detailList[index].TransactionQty + $scope.detailList[i].TIRCTotalQty) * 100 + Number.EPSILON) / 100));

					$scope.detailList[i].BalanceToIssue = BaltoIssue.toFixed(4);
				}
            }
			

		}

	}

	//#region Order Ref
	$scope.masterOrderCustomerList = [];
	$scope.GetMasterOrderByContractList = function () {
		//;
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/InventoryIssue/GetMasterOrderList',
		}).then(function successCallback(response) {
			$scope.masterOrderCustomerList = response.data;
			//entrydata = copy(searchdata);

		});
		angular.element(document.querySelector('#MasterOrderPopUp')).modal('show');
	}

	$scope.SelectedOrder = function (obj) {
		//;
		//var data = obj.data.ContractId;
	//	$scope.productNew.OrderRefNo = obj.data.MasterOrderNo;
		if ($scope.IssueTransformation.ContractType == "Transformation") {
			$scope.IssueTransformation.OrderRefNo = obj.data.MasterOrderNo;
		}
		else {
			$scope.Issue.OrderRefNo = obj.data.MasterOrderNo;
        }
		
		angular.element(document.querySelector('#MasterOrderPopUp')).modal('hide');
	}

	$scope.ClearMasterOrder = function () {
		if ($scope.IssueTransformation.ContractType == "Transformation") {
			$scope.IssueTransformation.OrderRefNo = "";
		}
		else {
			$scope.Issue.OrderRefNo = "";
        }
		

	};

	$scope.CloseMasterOrder = function () {
		angular.element(document.querySelector('#MasterOrderPopUp')).modal('hide');

	};

	$scope.productNewPOpUPModelTemp = {
		MasterOrderNo1: null,
		TotalQty1: null,
		CustomerName1: null,
		Contract1: null,
		MasterLCNo1: null,

	};
	$scope.productNewPOpUP = Object.assign({}, $scope.productNewPOpUPModelTemp);

	$scope.GetPopUpMasterOrderDetails = function () {
		//;
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/InventoryIssue/GetMasterOrderDetailsList?MasterOrderId=' + $scope.IssueTransformation.OrderRefNo,
		}).then(function successCallback(response) {
			//$scope.productNew.masterOrderCustomerList = response.data;
			$scope.productNewPOpUP.MasterOrderNo1 = response.data[0].MasterOrderNo;
			$scope.productNewPOpUP.TotalQty1 = response.data[0].TotalQty;
			$scope.productNewPOpUP.CustomerName1 = response.data[0].CustomerName;
			$scope.productNewPOpUP.Contract1 = response.data[0].ContractNo;
			$scope.productNewPOpUP.MasterLCNo1 = response.data[0].MasterLCNo;
			angular.element(document.querySelector('#MasterOrderPopUp1')).modal('show');

		});

	};

	$scope.GetPopUpMODetails = function () {
		//;
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/InventoryIssue/GetMasterOrderDetailsList?MasterOrderId=' + $scope.Issue.OrderRefNo,
		}).then(function successCallback(response) {
			//$scope.productNew.masterOrderCustomerList = response.data;
			$scope.productNewPOpUP.MasterOrderNo1 = response.data[0].MasterOrderNo;
			$scope.productNewPOpUP.TotalQty1 = response.data[0].TotalQty;
			$scope.productNewPOpUP.CustomerName1 = response.data[0].CustomerName;
			$scope.productNewPOpUP.Contract1 = response.data[0].ContractNo;
			$scope.productNewPOpUP.MasterLCNo1 = response.data[0].MasterLCNo;
			angular.element(document.querySelector('#MasterOrderPopUp1')).modal('show');

		});

	};

	$scope.CloseMasterOrder1 = function () {
		angular.element(document.querySelector('#MasterOrderPopUp1')).modal('hide');

	};
	//#endregions

	// Editing mode in Issue

	$scope.recorddoubleclickFromMasterGrid = function ($event) {
		//;
		if ($scope.ModelNew.TabType == "Transformation") {

			var x = $event;
			var Id = x.data.Id;
			$scope.TransIssueId = x.data.Id;
			var JWContractId = x.data.JWContractId;
			var IssueDate = x.data.IssueDate;
			var MaterialStorageId = x.data.MaterialStorageId;
			$scope.Action = 'Update';
			//ClearFields();		
			$scope.IssueTransformation = x.data;
			$scope.IssueTransformation.StorageLocation = x.data.StorageLocation;
			$scope.IssueTransformation.EmpCode = x.data.EmployeeCode;
			$scope.IssueTransformation.EmpName = x.data.ResponsiblePerson;
			ValAddedMaterialStorageForEdit(Id, MaterialStorageId);
		//	JWOutPutQuery(Id, JWContractId, IssueDate, MaterialStorageId);
		//	$scope.CostCenterLoad();

	//	JWByProductQuery(Id);

		}
		else {
			var x = $event;
			var Id = x.data.Id;
			var JWContractId = x.data.JWContractId;
			var IssueDate = x.data.IssueDate;
			var MaterialStorageId = x.data.MaterialStorageId;
			$scope.Action = 'Update';
			//ClearFields();		
			$scope.Issue = x.data;
			$scope.Issue.StorageLocation = x.data.StorageLocation;
			ValAddedMaterialStorageForEdit(Id, MaterialStorageId);
			JWOutPutQuery(Id, JWContractId, IssueDate, MaterialStorageId);
			$scope.CostCenterLoad();

	//	JWByProductQuery(Id);

		//if (baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
		//	$scope.CheckedByStatusForNoti = false;
		//	$scope.ApprovedByStatusForNoti = true;
		//	$scope.Issue.CheckedBy = x.data.ApprovedById;
		//}
		//else if (!baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
		//	$scope.CheckedByStatusForNoti = true;
		//	$scope.ApprovedByStatusForNoti = true;
		//	$scope.Issue.CheckedBy = x.data.CheckedById;
		//}
	//	$scope.GetCheckedByAndApprovedBy1();
		//if (baseService.isUndefinedOrNull(x.data.CheckedById) && !baseService.isUndefinedOrNull(x.data.ApprovedById)) {

		//	$scope.Issue.CheckedBy = x.data.ApprovedById;
		//	$scope.Issue.labelCheckAndApproved = 'To be approved by';
		//}
		//else if (!baseService.isUndefinedOrNull(x.data.CheckedById) && baseService.isUndefinedOrNull(x.data.ApprovedById)) {

		//	$scope.Issue.CheckedBy = x.data.CheckedById;
		//	$scope.Issue.labelCheckAndApproved = 'To be checked by';
		//}
        }

	

		if (!$rootScope.isCollapsed) $rootScope.toggle();
	}

	function JWOutPutQuery(IssueId, JWContractId, IssueDate, MaterialStorageId) {
		$scope.masterId5 = IssueId;
		$scope.IssueChildList = [];
		$http({
			method: 'GET',
			url: $scope.path + 'GetOSOutPutInventoryMaterialList?IssueId=' + IssueId + '&PKId=' + JWContractId + '&IssueDate=' + IssueDate + '&MaterialStorageIdInventory=' + MaterialStorageId
		}).then(function successCallback(response) {
			$scope.IssueChildList = response.data;
			if ($scope.IssueChildList.length > 0) {
				for (var i = 0; i < $scope.IssueChildList.length; i++) {
					$scope.IssueChildList[i].isSelectedOM = true;
                }	
            }
		});
	}


	$scope.inventoryMaterialListPO = [];
	function JWByProductQuery(inveReveiveId) {
		$scope.masterId5 = inveReveiveId;
		$scope.inventoryMaterialList = [];
		$http({
			method: 'GET',
			url: 'Products/GoodsReceiveNote/GetJWByProductInventoryMaterialList?inveReveiveId=' + inveReveiveId
		}).then(function successCallback(response) {
			$scope.inventoryMaterialListPO = response.data;
		});
	}

	function ValAddedMaterialStorageForEdit(IssueId, MaterialStorageId) {

		if ($scope.ModelNew.TabType == "Transformation") {
			$http({
				method: 'GET',
				url: 'Outsourcing/OSIssueReturn/ValAddedMaterialStorageForEdit?IssueId=' + IssueId + '&MaterialStorageIdInventory=' + MaterialStorageId,
			}).then(function successCallback(response) {
				$scope.JobWorkLocList = response.data;
				if ($scope.JobWorkLocList.length > 0) {
					$scope.IssueTransformation.MaterialStorageId = $scope.JobWorkLocList[0].Value;
					$scope.IssueTransformation.StorageLocation = $scope.JobWorkLocList[0].StorageLocation;
					$scope.IssueTransformation.MaterialStorageIdInventory = $scope.JobWorkLocList[0].Value;
				}
			});

		}
		else {
			$http({
				method: 'GET',
				url: 'Outsourcing/OSIssueReturn/ValAddedMaterialStorageForEdit?IssueId=' + IssueId + '&MaterialStorageIdInventory=' + MaterialStorageId,
			}).then(function successCallback(response) {
				$scope.ValAddedJobWorkLocList = response.data;
				if ($scope.ValAddedJobWorkLocList.length > 0) {
					$scope.Issue.MaterialStorageId = $scope.ValAddedJobWorkLocList[0].Value;
					$scope.Issue.StorageLocation = $scope.ValAddedJobWorkLocList[0].StorageLocation;
					$scope.Issue.MSIdInventory = $scope.ValAddedJobWorkLocList[0].Value;
				}
			});
        }

	}

	$scope.addProductionSummaryItem = function () {
		//;
		try {
			qtyValidation($scope.materialStockList);
			validationWithTotal($scope.materialStockList);
			for (var i = baseService.arrayLength($scope.specificStockList) - 1; i >= 0; i--) {
				var row = $scope.specificStockList[i];
				for (var t = 0; t < baseService.arrayLength($scope.materialStockList); t++) {
					var newRow = $scope.materialStockList[t];
					if (row.InventoryReceiveDetailId === newRow.InventoryReceiveDetailId) { // update or delete
						if (newRow.Flag) row.RequisitionQty = newRow.RequisitionQty;
						else $scope.specificStockList.splice(i, 1);
					}
				}
			}
			for (var n = 0; n < baseService.arrayLength($scope.materialStockList); n++) { // add
				var nRow = $scope.materialStockList[n];
				nRow.BaseQty = $scope.materialStockList[n].BaseQty;
				nRow.BaseIssueQty = $scope.materialStockList[n].BaseIssueQty;
				if (!baseService.valueCheckInList($scope.specificStockList, 'InventoryReceiveDetailId', nRow.InventoryReceiveDetailId) && nRow.Flag)
					//$scope.detailModel.IsSpecific = true;
					$scope.specificStockList.push(nRow);
			}
			$scope.detailList[$scope.issueSpecificIndex].TransactionQty = $filter("sumByKey")($filter("filter")($scope.materialStockList, { Flag: true }), "RequisitionQty");
			//$scope.detailList[$scope.index].TransactionQty = issueQty;
			angular.element(document.querySelector('#PSProcessPopUp')).modal('hide');
			$scope.modalSpecificMaterialName = null;
			$scope.modalSpecificArticleName = null;
			$scope.modalSpecificOSTCMasterId = null;
			$scope.modalSpecificOSOutputItem = null;
			$scope.modalSpecificOSInputItem = null;

			$scope.issueSpecificIndex = null;
			CloseModalShowResult();
		} catch (e) {
			ShowResult(e, 'failure', 'stockPopUp');
		}
	};
}