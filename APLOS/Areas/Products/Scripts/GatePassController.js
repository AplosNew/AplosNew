'use strict';

GatePassController.$inject = ['accountService', 'addressService', '$location', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function GatePassController(accountService, addressService, $location, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {

	$rootScope.title = "Gate Pass System";
	$scope.Action = 'Save';
	$scope.index = -1;
	$scope.products = [];
	$scope.path = 'Products/GateentryToken/';
	$scope.getListUrl = $scope.path + 'getlist';
	$scope.saveUrl = $scope.path + 'createGatePass';
	$scope.updateUrl = $scope.path + 'editGatePass';
	$scope.deleteUrl = $scope.path + 'deleteGatePass/';
	//
	$scope.deleteUrl1 = $scope.path + 'CancelGateEntry/';
	$scope.detailSaveUrl = $scope.path + 'DetailCreate';
	$scope.detailSaveUrl1 = $scope.path + 'DetailCreateDispatch';
	$scope.detailDeleteUrl = $scope.path + 'DeleteGatePassDEtails/';
	$scope.sreviceSaveUrl = $scope.path + 'servicechargescreate';
	$scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';
	$scope.partyType = 'Vendor';
	$scope.isAdvance = false;
	$scope.currentDate = new Date(Date.now());
	$scope.grossTotal = 0;
	$scope.PartyId = null;
	$controller('partyBaseController', { $scope: $scope, $http: $http });
	$controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
	$controller("employeeBaseController", { $scope: $scope, $http: $http });
	$scope.inventoryMaterialList = [];
	$scope.chargesList = [];
	$scope.ChargeTaxList = [];
	$scope.StateData = [];
	$scope.TypeLabel = 'Vendor';
	$scope.partyType = 'Vendor';
	// #region Requisition Tab for Index Page

	$scope.ReqStatus = 1;

	$scope.tab = 1;
	$scope.setTabReqList = function (newTab) {
		$scope.ReqStatus = 1;//'ForChecked';
		$scope.GetIndexGridDataList();
		$scope.tab = newTab;
	};
	$scope.isSetReqList = function (tabNum) {
		//$scope.getPurchaseReturnData();
		return $scope.tab === tabNum;
	};
	$scope.setTabReqList1 = function (newTab) {
		//debugger
		$scope.ReqStatus = 2;//'HoldReject';
		$scope.PendingApprvedGateOut = 'Pending';
		$scope.GetIndexGridDataList();
		$scope.tab = newTab;
	};
	$scope.isSetReqList1 = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabReqList2 = function (newTab) {
		$scope.tab = newTab;
		$scope.ReqStatus = 3;//'Checked';
		$scope.PendingApprvedGateOut = 'Checked';
		$scope.GetIndexGridDataList();

	};
	$scope.isSetReqList2 = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabReqApproved3 = function (newTab) {
		//debugger
		$scope.tab = newTab;
		$scope.ReqStatus = 4;//'HoldReject';
		$scope.PendingApprvedGateOut = 'GateOut';//Checked means approved
		$scope.GetIndexGridDataList();
	};
	$scope.isSetReqApproved3 = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabReqApproved4 = function (newTab) {
		$scope.tab = newTab;
		$scope.ReqStatus = 5; //'Approval';
		$scope.GetIndexGridDataList();
	};
	$scope.isSetReqApproved4 = function (tabNum) {
		return $scope.tab === tabNum;
	};
	$scope.setTabReqApproved5 = function (newTab) {
		$scope.tab = newTab;
		$scope.ReqStatus = 6; //'GateOuts';
		$scope.GetIndexGridDataList();
	};
	$scope.isSetReqApproved5 = function (tabNum) {
		return $scope.tab === tabNum;
	};
	// #endregion Requisition Tabs

	//$scope.showPartyPopUp = function () {
	//	//debugger;
	//	baseService.setCurrentPage('partyList');
	//	$scope.getPartyList = function (pageno) {

	//		if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
	//			$scope.partyUrl = 'Parties/party/GetCompanyPartyDataList?partyType=' + $scope.partyType;
	//		}
	//		else if ($scope.partyType === 'Party') {
	//			$scope.partyUrl = 'Parties/party/GetCompanyPartyDataList';
	//		}
	//		else if ($scope.partyType === 'Director') {
	//			$scope.partyUrl = 'Parties/party/GetCompanyDirectorDataList';
	//		}
	//		else if ($scope.partyType === 'Other') {
	//			$scope.partyUrl = 'Parties/party/GetCompanyOtherDataList';
	//		}
	//		baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
	//			.then(function (result) {
	//				$scope.partyList = result.Rows;
	//				$scope.partyParameters.total_count = result.Total;
	//			}, function () {
	//				ShowResult(commonMessage.NetworkError, 'failure');
	//			}).finally(function () {
	//			});
	//	};
	//	angular.element(document.querySelector('#partyPopUp')).modal('show');
	//	$scope.getPartyList();
	//};
	$scope.closePartyPopUp = function (x) {
		//debugger;
		
			var party = x.data;
			$scope.productNew.ToPartyCode = party.Code;
			$scope.productNew.PartyName = party.UserName;
			$scope.productNew.PartyId = party.Id;
			$scope.productNew.ToPartyCode = party.Id;
			$scope.productNew.PaymentTermId = party.PaymentTermId;
			$scope.productNew.CurrencyId = party.CurrencyId;
			$scope.IsBaseOnDueDateEnable = false;
			$scope.productNew.BaseOnDueDate = null;
			$scope.productNew.BaseNoOfDays = null;
			$scope.productNew.MatureDate = null;

			$scope.productNew.TaxApplicable = party.TaxApplicable;
			$scope.productNew.IsTaxApplicableChangeable = party.IsTaxApplicableChangeable;
			if (party.TaxApplicable === 'Mandatory')
				$scope.productNew.IsTaxApplicable = true;
			else
				$scope.productNew.IsTaxApplicable = false;

			if (!baseService.isUndefinedOrNull($scope.productNew.DocDate))
				$scope.changePaymentTerm();
			getPartyPlantList();
			$scope.hidePartyPopUp();
	};
	$scope.changeSourceFrom = function (from) {
		//debugger;

		if (from === 'Vendor') {
			//$scope.productNew.ResponsiblePersonName = '';
			$scope.TypeLabel = 'Vendor';
			$scope.productNew.ToBuyerId = '';
			$scope.productNew.OtherPlantId = '';
			$scope.GetPlantList.OtherPlantId = '';
			$scope.productNew.OtherCompanyName = '';
			$scope.productNew.PersonName = '';
			$scope.productNew.MobileNo = '';
			$scope.productNew.Address = '';
			$scope.productNew.ToType = 'Vendor';
			$scope.GetDivisionList = [];
			$scope.GetUnitList = [];
			$scope.GetDepartmentList = [];
			$scope.productNew.DepartmentEmployeeName = '';
		}
		if (from === 'Buyer') {
			$scope.TypeLabel = 'Buyer';
			$scope.productNew.PartyName = '';
			$scope.productNew.ToPartyCode = '';
		/*	$scope.productNew.ToBuyerId = '';*/
			$scope.productNew.OtherPlantId = '';
			$scope.GetPlantList.OtherPlantId = '';
			$scope.GetPlantList.OtherPlantId = '';
			$scope.productNew.OtherCompanyName = '';
			$scope.productNew.PersonName = '';
			$scope.productNew.MobileNo = '';
			$scope.productNew.Address = '';
			$scope.GetBuyer();
			$scope.productNew.ToType = 'Buyer';
			$scope.GetDivisionList = [];
			$scope.GetUnitList = [];
			$scope.GetDepartmentList = [];
			$scope.productNew.DepartmentEmployeeName = '';


		}
		if (from === 'OtherPlant') {
			$scope.TypeLabel = 'OtherPlant';
			$scope.productNew.PartyName = '';
			$scope.productNew.ToPartyCode = '';
			$scope.productNew.ToBuyerId = '';
			$scope.productNew.OtherPlantId = '';
			$scope.GetPlantList.OtherPlantId = '';
			$scope.productNew.OtherCompanyName = '';
			$scope.productNew.PersonName = '';
			$scope.productNew.MobileNo = '';
			$scope.productNew.Address = '';
			$scope.productNew.ToType = 'OtherPlant';
			$scope.GetDivisionList = [];
			$scope.GetUnitList = [];
			$scope.GetDepartmentList = [];
			$scope.productNew.DepartmentEmployeeName = '';


		}
		if (from === 'OtherCompany') {
			$scope.TypeLabel = 'OtherCompany';
			$scope.productNew.PartyName = '';
			$scope.productNew.ToPartyCode = '';
			$scope.productNew.ToBuyerId = '';
			$scope.GetPlantList.OtherPlantId = '';
			$scope.productNew.OtherPlantId = '';
			$scope.productNew.OtherCompanyName = '';
			$scope.productNew.PersonName = '';
			$scope.productNew.MobileNo = '';
			$scope.productNew.Address = '';
			$scope.productNew.ToType = 'OtherCompany';
			$scope.GetDivisionList = [];
			$scope.GetUnitList = [];
			$scope.GetDepartmentList = [];
			$scope.productNew.DepartmentEmployeeName = '';


		}
	};


	$scope.AllTabPrint = function (z) {
		//debugger;
		var x = "#" + z;
		var gridObj = $(x).data("ejGrid");
		var data = gridObj.getSelectedRecords()[0];
		location.href = " GateentryToken/IndivisualGatePassTeamplateReport?GatePassId=" + data.Id;
	};
	//#region  Employee CAll
	$scope.closeEmployeePopUp = function () {
		//debugger;
		if ($scope.employeeIndex !== -1) {
			var employee = $scope.employeeList[$scope.employeeIndex];
			$scope.productNew.RunnerEmployeeName = employee.EmployeeName;
			$scope.productNew.RunnerEmployeeId = employee.SystemId;



		}
		$scope.hideEmployeePopUp();
	};

	$scope.hideEmployeePopUp = function () {
		angular.element(document.querySelector("#employeePopUp")).modal("hide");
	};
	$scope.closeResponsiblePersonPopUp = function () {
		if ($scope.responsiblePersonIndex !== -1) {
			var employee = $scope.employeeList[$scope.responsiblePersonIndex];
			$scope.productNew.ResponsiblePersonName = employee.EmployeeName;
			$scope.productNew.EmployeeIdForGateEntry = employee.SystemId;

		}
		$scope.hideResponsiblePersonPopUp();
	};
	$scope.hideResponsiblePersonPopUp = function () {
		angular.element(document.querySelector("#responsiblePersonPopUp")).modal("hide");
	};
	$scope.hideEmployeePopUp1 = function () {
		angular.element(document.querySelector("#employeePopUpDeptEmp")).modal("hide");
	};

	//#endregion
	$scope.getDataList = function () {
		baseService.init($scope.getListUrl, null, null, "DESC", 'Id', 'PartyName');
		$scope.getData = function (pageno) {
			baseService.pagination(pageno)
				.then(function (result) {
					$scope.products = [];
					$scope.products = result.Rows;
				}, function () {
					ShowResult(commonMessage.NetworkError, 'failure');
				}).finally(function () {
				});
		};
		$scope.getData();
	};
	//$scope.getDataList();

	$scope.storageList = [];
	$http({
		method: 'GET',
		url: 'Materials/MaterialStorage/getcbo'
	}).then(function (response) {
		$scope.storageList = response.data;
	});
	$scope.department = [];
	$scope.plantwisedepartmentLoad = function () {
		$http({
			method: 'GET',
			url: 'Products/GateentryToken/GetDepartmentByPlant'
		}).then(function (response) {
			$scope.department = response.data;
		});
	}
	$scope.plantwisedepartmentLoad();
	$scope.GetPlantList = [];
	$scope.GetPlantLoad = function () {
		$http({
			method: 'GET',
			url: 'Products/GateentryToken/GetPlant'
		}).then(function (response) {
			$scope.GetPlantList = response.data;
		});
	}
	$scope.GetPlantLoad();
	$scope.detailPopUpPlant = function () {
		$scope.GetPlantLoad();
		angular.element(document.querySelector('#detailPopUpPlant')).modal('show');
	};
	$scope.closeDetaiPopUpPlant = function () {
		angular.element(document.querySelector('#detailPopUpPlant')).modal('hide');
	};

	$scope.recorddoubleclickPlant = function ($event) {
		var gridObj = $("#GridPlant").data("ejGrid");
		//getting corresponding record 
		var data = gridObj.getSelectedRecords()[0];
		$scope.productNew.PlantName = data.Text;
		$scope.productNew.ToPlantId = data.Value;
		$scope.closeDetaiPopUpPlant();
	}

	$scope.detailPopUpUnit = function () {
		$scope.GetUnit();
		angular.element(document.querySelector('#detailPopUpUnit')).modal('show');
	};
	$scope.closeDetaiPopUpUnit = function () {
		angular.element(document.querySelector('#detailPopUpUnit')).modal('hide');
	};

	$scope.recorddoubleclickUnit = function ($event) {
		var gridObj = $("#GridUnit").data("ejGrid");
		//getting corresponding record 
		var data = gridObj.getSelectedRecords()[0];
		$scope.productNew.UnitName = data.Text;
		$scope.productNew.ToUnitId = data.Value;
		$scope.closeDetaiPopUpUnit();
	}

	$scope.GetUnitList = [];
	$scope.GetUnit = function (args) {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GateentryToken/GetUnit?plantId=' + $scope.productNew.ToPlantId
		}).then(function (response) {
			$scope.GetUnitList = response.data;
		});
	}



	$scope.detailPopUpDivision = function () {
		$scope.GetDivision();
		angular.element(document.querySelector('#detailPopUpDivision')).modal('show');
	};
	$scope.closeDetaiPopUpDivision = function () {
		angular.element(document.querySelector('#detailPopUpDivision')).modal('hide');
	};

	$scope.recorddoubleclickDivision = function ($event) {
		var gridObj = $("#GridDivision").data("ejGrid");
		//getting corresponding record 
		var data = gridObj.getSelectedRecords()[0];
		$scope.productNew.DivisionName = data.Text;
		$scope.productNew.ToDivisionId = data.Value;
		$scope.closeDetaiPopUpDivision();
	}
	$scope.GetDivisionList = [];
	$scope.GetDivision = function (args) {
		//debugger;

		//$scope.productNew.ToUnitId = args.value;

		$http({
			method: 'GET',
			url: 'Products/GateentryToken/GetDivision?UnitId=' + $scope.productNew.ToUnitId
		}).then(function (response) {
			$scope.GetDivisionList = response.data;
		});
	}



	$scope.detailPopUpDepartment = function () {
		$scope.GetDepartment();
		angular.element(document.querySelector('#detailPopUpDepartment')).modal('show');
	};
	$scope.closeDetaiPopUpDepartment = function () {
		angular.element(document.querySelector('#detailPopUpDepartment')).modal('hide');
	};

	$scope.recorddoubleclickDepartment = function ($event) {
		var gridObj = $("#GridDepartment").data("ejGrid");
		//getting corresponding record 
		var data = gridObj.getSelectedRecords()[0];
		$scope.productNew.DepartmentName = data.Text;
		$scope.productNew.ToDepartment = data.Value;
		$scope.closeDetaiPopUpDepartment();
	}



	$scope.GetDepartmentList = [];
	$scope.GetDepartment = function (args) {
		//debugger;
		// $scope.productNew.ToDivisionId = args.value;

		$http({
			method: 'GET',
			url: 'Products/GateentryToken/GetDepartmentByPlant?DivisionId=' + $scope.productNew.ToDivisionId
		}).then(function (response) {
			$scope.GetDepartmentList = response.data;
		});
	}
	$scope.GetSelectDepartment = function (args) {
		//debugger;
		$scope.DepartmentId = args.value;
		//$scope.productNew.ToDepartment = args.value;

	}
	$scope.GetSelectBuyer = function (args) {
		//debugger;
		$scope.productNew.ToBuyerId = args.value;

	}

	$scope.showEmployeeListPopUpDeptWise = function (args) {
		//debugger;
		baseService.setCurrentPage('employeeList');
		$scope.getEmployeeData = function (pageno) {
			var url = null;
			if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
				url = 'Products/GateentryToken/EmployeeListByDepartment?DepartmentId=' + $scope.productNew.ToDepartment;
			}
			else {
				url = $scope.employeeUrl;
			}
			baseService.paginationBase(url, pageno, $scope.employeeParameters)
				.then(function (result) {
					$scope.employeeList = result.Rows;
					$scope.employeeParameters.total_count = result.Total;
				}, function () {
					ShowResult(commonMessage.NetworkError, 'failure');
				}).finally(function () {
				});
		};
		angular.element(document.querySelector('#employeePopUpDeptEmp')).modal('show');
		$scope.getEmployeeData();
	};

	$scope.closeDeptEmployeePopUp = function () {
		//debugger;
		if ($scope.employeeIndex !== -1) {
			var employee = $scope.employeeList[$scope.employeeIndex];
			$scope.productNew.DepartmentEmployeeName = employee.EmployeeName;
			$scope.productNew.DepartmentEmployeeId = employee.SystemId;



		}
		$scope.hideEmployeePopUp1();
	};


	$scope.GetSenderNameList = [];
	$scope.GetSenderName = function (args) {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GateentryToken/GetSenderName'
		}).then(function (response) {
			$scope.GetSenderNameList = response.data;
			$scope.productNew.EmployeeName = $scope.GetSenderNameList[0].EmployeeName;
			$scope.productNew.FromEmployeeId = $scope.GetSenderNameList[0].SystemId;


		});
	}
	$scope.GetSenderName();

	$scope.GetBuyerList = [];
	$scope.GetBuyer = function (args) {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GateentryToken/GetBuyer'
		}).then(function (response) {
			$scope.GetBuyerList = response.data;
		});
	}

	$scope.detailPopUpBuyer = function () {
		$scope.GetBuyer();
		angular.element(document.querySelector('#detailPopUpBuyer')).modal('show');
	};
	$scope.closeDetaiPopUpBuyer = function () {
		angular.element(document.querySelector('#detailPopUpBuyer')).modal('hide');
	};

	$scope.recorddoubleclickBuyer = function ($event) {
		var gridObj = $("#GridBuyer").data("ejGrid");
		//getting corresponding record 
		var data = gridObj.getSelectedRecords()[0];
		$scope.productNew.BuyerName = data.Text;
		$scope.productNew.ToBuyerId = data.Value;

		$scope.closeDetaiPopUpBuyer();
	}

	$scope.GetChallanList = [];
	$scope.GetChallan = function (args) {
		//debugger;
		var InOutStatus = 'In';
		$http({
			method: 'GET',
			url: 'Products/GateentryToken/GetChallanNo?InOutStatus='+ InOutStatus
		}).then(function (response) {
			$scope.GetChallanList = response.data;
		});
	}
	$scope.GetChallanReturnToAddress = [];
	$scope.GetChallanReturnToAddress = function (args) {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GateentryToken/GetChallanNoDetailsForToAddress?DepartmentEmployeeId=' + scope.productNew.DepartmentEmployeeId
		}).then(function (response) {
			$scope.GetChallanReturnToAddress = response.data;
		});
	}

	$scope.LoadChallanBYRequrnDispatch = function () {
		//debugger;
		if ($scope.productNew.GatePassType === 'Return') {
			$scope.productNew.GatePassStatus1 = 'NonReturnable'
			$scope.GetChallan();
			//$scope.GetChallanReturnToAddress();

		}
		else {
			$scope.productNew.ChallanNo = "";
			$scope.productNew.ToType = 'Vendor';
			$scope.productNew.ToPlantId = "";
			$scope.productNew.PlantName = "";
			$scope.productNew.ToUnitId = "";
			$scope.productNew.UnitName = "";
			$scope.productNew.ToDepartment = "";
			$scope.productNew.DepartmentName = "";
			$scope.productNew.ToDivisionId = "";
			$scope.productNew.DivisionName = "";
			$scope.productNew.DepartmentEmployeeName = "";
			$scope.productNew.DepartmentEmployeeId = "";
		}
	}
	$scope.detailPopUpChallan = function () {
		$scope.GetChallan();
		angular.element(document.querySelector('#detailPopUpChallan')).modal('show');
	};
	$scope.closeDetaiPopUpChallan = function () {
		angular.element(document.querySelector('#detailPopUpChallan')).modal('hide');
	};








	$scope.LoadChallanMaterial = function (args) {
		//debugger;
		$scope.selectedChallanNo = args.value;

	}
	$scope.detailPopUp = function () {
		//debugger;
		if ($scope.productNew.GatePassStatus1 === 'Returnable') {
			$scope.detailModel.ReturnableDate = $scope.productNew.ReturnableDate;

		}
		if ($scope.productNew.GatePassType === 'ReturnDispatch' && $scope.productNew.ChallanNo === null) {
			return ShowResult('Please Select Returnable Challan No');

		}
		else {
			getInventoryMaterialListchallDetails($scope.productNew.ChallanNo);
			//$scope.receiveTaxList = [];
			$scope.detailModel = {

				Id: null
				, GatePassMasterId: null
				, MaterialMasterId: null
				, ArticleId: null
				, FirstCharacteristicsId: null
				, FirstCharacteristicsValueId: null
				, SecondCharacteristicsId: null
				, SecondCharacteristicsValueId: null
				, ThirdCharacteristicsId: null
				, ThirdCharacteristicsValueId: null
				, MaterialDetail: null
				, TransactionQty: null
				, TransactionUoMId: null
				, Remarks: null
				, IsReturnable: false
				, ReturnableDate: $scope.productNew.ReturnableDate
				, IsMutilated: false
				, AddedBy: null
				, AddedDate: null
				, AddedFromIP: null
				, UpdatedBy: null
				, UpdatedDate: null
				, UpdatedFromIP: null
				, Rate:null
			};
			angular.element(document.querySelector('#detailPopUp')).modal('show');

		}

		// $scope.clearCharNames();
	};

	$scope.currencyList = [];
	$scope.CurrentDate = new Date('HH:mm:ss');

	$scope.product = {
		Id: null
		, CompanyGroupId: null
		, CompanyId: null
		, PlantId: null
		, GatePassType: 'Send'
		//, GatePassChallanNo: null
		, GatePassStatus1: 'NonReturnable'
		, ReturnableDate: null
		, GatePassEntryDate: $filter("dateFiltering")(Date.now())
		, FromEmployeeId: null
		, EmployeeName: null
		, Through: null
		, CourierName: null
		, RunnerEmployeeId: null
		, RunnerEmployeeName: null
		, ChallanItemType: null
		, Remarks: null
		, CheckedBy: null
		, Type: null
		, PartyCode: null
		, BuyerId: null
		, BuyerName: null
		, OtherPlantId: null
		, UnitId: null
		, UnitName: null
		, DivisionId: null
		, OtherCompanyName: null
		, DepartmentId: null
		, DepartmentEmployeeId: null
		, PersonName: null
		, MobileNo: null
		, Address: null
		, DeptEmployeeName: null
		, ToType: 'Vendor'
		, ToPartyCode: null
		, PartyName: null
		, ToBuyerId: null
		, ToPlantId: null
		, PlantName: null
		, ToUnitId: null
		, ToDivisionId: null
		, DivisionName: null
		, ToDepartment: null
		, DepartmentName: null
		, CheckedByStatus: null
		, CheckedHoldRejectReason: null
		, ApprovedBy: null
		, ApprovedByStatus: null
		, ApprovedHoldRejectReason: null
		, SenderSecurityEmployeeId: null
		, SenderSecurityApprovedStatus: null
		, ReceiverSecurityEmployeeId: null
		, ReceiverSecurityApprovedStatus: null
		, VendorBuyerOtherCompanyReceivedStatus: null
		, ChallanNo: null
		, EntryTime: new Date().toLocaleTimeString({}, { hour12: true, hour: 'numeric', minute: 'numeric' })
		, NoofPackages: null
		, InvoiceNo: null
		, InvoiceValue: null
		, TransportAgentMobileNo: null
		, TransportAgentName: null
		, VehicleNo: null
		, GateRegisterType: 'Out'
		, ReceivedChallanNO: null		
		, PurposeofGatePass: null
		, ConsignmentNo: null
		, DriverName: null
		
	};



	$scope.productNew = Object.assign({}, $scope.product);

	$scope.Save = function () {
		if ($scope.productNew.GatePassStatus1 === 'Returnable' && baseService.isUndefinedOrNull($scope.productNew.ReturnableDate)) {
			ShowResult("Enter the returnable date", 'failure');
			return false;
		}
		if ($scope.productNew.Through === 'Courier' && baseService.isUndefinedOrNull($scope.productNew.CourierName)) {
			ShowResult("Enter the Courier Name", 'failure');
			return false;
		}
		if ($scope.productNew.Through === 'Runner' && baseService.isUndefinedOrNull($scope.productNew.RunnerEmployeeName)) {
			ShowResult("Select Runner Name", 'failure');
			return false;
		}
		if ($scope.productNew.Through === 'Transportagent' && baseService.isUndefinedOrNull($scope.productNew.TransportAgentName)) {
			ShowResult("Enter the transport agent", 'failure');
			return false;
		}
		if ($scope.productNew.Through === 'Transportagent' && baseService.isUndefinedOrNull($scope.productNew.TransportAgentMobileNo)) {
			ShowResult("Enter the transport agent mobile no", 'failure');
			return false;
		}
		if ($scope.productNew.Through === 'Transportagent' && baseService.isUndefinedOrNull($scope.productNew.VehicleNo)) {
			ShowResult("Enter the Vehicle No", 'failure');
			return false;
		}
		if ($scope.productNew.Through === 'Transportagent' && baseService.isUndefinedOrNull($scope.productNew.DriverName)) {
			ShowResult("Enter the Driver Name", 'failure');
			return false;
		}
		if ($scope.productNew.Through === 'OwnVehicle' && baseService.isUndefinedOrNull($scope.productNew.DriverName)) {
			ShowResult("Enter the Driver Name", 'failure');
			return false;
		}
		
		if ($scope.productNew.Through === 'OwnVehicle' && baseService.isUndefinedOrNull($scope.productNew.TransportAgentName)) {
			ShowResult("Enter the transport agent", 'failure');
			return false;
		}
		if ($scope.productNew.Through === 'OwnVehicle' && baseService.isUndefinedOrNull($scope.productNew.TransportAgentMobileNo)) {
			ShowResult("Enter the transport agent mobile no", 'failure');
			return false;
		}
		if ($scope.productNew.Through === 'OwnVehicle' && baseService.isUndefinedOrNull($scope.productNew.VehicleNo)) {
			ShowResult("Enter the Vehicle No", 'failure');
			return false;
		}
	
		if ($scope.productNew.Through === 'OwnVehicle' && baseService.isUndefinedOrNull($scope.productNew.ConsignmentNo)) {
			ShowResult("Enter the Consignment No", 'failure');
			return false;
		}

		if ($scope.productNew.ToType === 'Vendor' && baseService.isUndefinedOrNull($scope.productNew.PartyName)) {
			ShowResult("Select The Party Name", 'failure');
			return false;
		}		
		if ($scope.productNew.ToType === 'Buyer' && baseService.isUndefinedOrNull($scope.productNew.BuyerName)) {
			ShowResult("Select The Buyer Name", 'failure');
			return false;
		}

		if ($scope.productNew.ToType === 'OtherPlant' && baseService.isUndefinedOrNull($scope.productNew.PlantName)) {
			ShowResult("Select The Plant  Name", 'failure');
			return false;
		}
		if ($scope.productNew.ToType === 'OtherPlant' &&  baseService.isUndefinedOrNull($scope.productNew.DepartmentEmployeeName)) {
			ShowResult("Select The Employee Name", 'failure');
			return false;
		}


		if ($scope.productNew.ToType === 'OtherCompany' && baseService.isUndefinedOrNull($scope.productNew.OtherCompanyName)) {
			ShowResult("Enter Compnay Name ", 'failure');
			return false;
		}
		if ($scope.productNew.ToType === 'OtherCompany' &&  baseService.isUndefinedOrNull($scope.productNew.PersonName) ) {
			ShowResult("Enter Name", 'failure');
			return false;
		}
		if ($scope.productNew.ToType === 'OtherCompany' &&  baseService.isUndefinedOrNull($scope.productNew.MobileNo)) {
			ShowResult("Enter Mobile No", 'failure');
			return false;
		}
	
		if (baseService.isUndefinedOrNull($scope.productNew.ToType)) {
			ShowResult("Select a type", 'failure');
			return false;
		}

		
		//debugger;
		//var dt = $filter("dateFiltering")(Date.now());
		//if ($scope.productNew.EntryDate < dt) {
		//    $scope.ConModal();
		//}
		//else {
		//debugger;
		try {
			$scope.$broadcast('show-errors-check-validity');
			if ($scope.productNewForm.$valid) {

				$scope.product = Object.assign({}, $scope.productNew);
				if ($scope.Action === "Save") {
					$http({
						method: 'POST',
						url: $scope.saveUrl,
						data: {
							'entity': $scope.product
							//'PlantWiseGateId': $scope.productNew.PlantWiseGateId,
						},
						dataType: 'JSON'
					}).then(function (response) {
						if (response.data.Error === true) {
							ShowResult(response.data.Message, 'failure');
						}
						else {
							ShowResult(response.data.Message, 'success');
							$scope.productNew.Id = response.data.entity.Id;
							$scope.Action = "Update";
							$scope.GetIndexGridDataList();
						}
					}), function (response) {
						ShowResult(response.data.Message, 'failure');
					};
				}
				else if ($scope.Action === "Update") {
					$scope.product.Id = $scope.productNew.Id;
					//ShowResult('You Do not have permission to update', 'failure');
					$http({
						method: 'POST',
						url: $scope.updateUrl,
						data: $scope.product,
						dataType: 'JSON'
					}).then(function successCallback(response) {
						if (response.data.Error === true) {
							ShowResult(response.data.Message, 'failure');
						}
						else {
							ShowResult(response.data.Message, 'success');
							$scope.GetIndexGridDataList();

						}
					}, function errorCallBack(response) {
						ShowResult(response.data.Message, 'failure');
					});
				}
			}
		} catch (e) {
			throw e;
		}
		// }


	};

	$scope.IndexGridDataList = [];
	$scope.GetIndexGridDataList = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GateentryToken/GetIndexGridDataList?ReqStatus=' + $scope.ReqStatus + '&GateRegisterType=' + 'Out'
		}).then(function successCallback(response) {
			$scope.IndexGridDataList = response.data;
			for (var i = 0; i < $scope.IndexGridDataList.length; i++) {
				response.data[i].GatePassEntryDate = new Date($scope.IndexGridDataList[i].GatePassEntryDate);
			}
		});
	}
	$scope.GetIndexGridDataList();


	//#region  Req Detail
	$scope.lst = [];
	$scope.GatePassDetails = function () {
		//debugger;
		$http({
			method: 'GET',
			//url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
			url: 'Products/GateentryToken/GetAllMaterilaList'
		}).then(function successCallback(response) {
			$scope.lst = response.data;
			//$scope.detailgrid($scope.lst);
			window.lst = response.data;

		});
	}
	$scope.GatePassDetails();


	$scope.data1 = $scope.lst;
	$scope.detailTemp = "#tabGridContents";
	//$scope.detailgrid = "detailGridData(e)";
	$scope.detailgrid = function detailGridData(e) {
		//debugger;

		var filteredData = e.data["Id"];
		var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("GatePassMasterId", "equal", parseInt(filteredData), true).take(100));
		e.detailsElement.find("#detailGrid").ejGrid({

			dataSource: data,
			//columns: ["MaterialGroupName", "MaterialName", "ArticleName", "SKU1", "SKU2", "SKU3","MaterialDetail", "TransactionQty", "TransactionUoM", "EstimatedRate", "CurrencyName", "TotalAmount" ]
			//
			columns: [
				{ field: "MaterialGroupMasterName", headerText: "MaterialGroup", width: 120 },
				{ field: "UserName", headerText: "Material", width: 150 },
				{ field: "StandardName", headerText: "Article", width: 150 },
				{ field: "FirstCharacteristicsValue", headerText: "SKU1", width: 50 },
				{ field: "SecondCharacteristicsValue", headerText: "SKU2", width: 50 },
				{ field: "ThirdCharacteristicsValue", headerText: "SKU3", width: 50 },
				{ field: "MaterialDetail", headerText: "MaterialDetail", width: 150 },
				{ field: "IsReturnable", headerText: "IsReturnable", width: 50 },
				{ field: "ReturnableDate", headerText: "ReturnableDate", width: 50 },
				//{ field: "IsMutilated", headerText: "IsMutilated", width: 50 },
				{ field: "TransactionQty", headerText: "Qty", width: 70 },
				{ field: "TransactionUoM", headerText: "UoM", width: 50 },
				{ field: "Remarks", headerText: "Remarks", width: 50 }
			]
		});
		e.detailsElement.find(".tabcontrol").ejTab();
	}
	//#endregion

	$window.onresize = function (event) {
		$scope.ScrollForPendingRequisition(event);
	};
	$scope.ScrollForPendingRequisition = function (args) {
		try {
			if (args.requestType === "refresh") {
				$scope.ScrollForPendingRequisition = function (args) {
					var gridObjPendingRequisition = $("#GridGatePass").ejGrid("instance");
					var scrollerwidth = $("#GridGatePass1").width();//Obtain the width of the container

					//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
					gridObjPendingRequisition.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
					gridObjPendingRequisition.windowonresize();

				}
			}
		} catch (e) {
			// $scope.ShowResultCustom(e, 'failure');
		}
	};


	//$scope.ReqListDetails = function () {
	//    //debugger;
	//    var gridObj = $("#AcceptanceList").data("ejGrid");
	//    //getting corresponding record 
	//    var data = gridObj.getSelectedRecords()[0];
	//    $http({
	//        method: 'GET',
	//        url: 'Products/GateentryToken/GetIndexGridDataListDetails?ReqDetailId=' + data.Id
	//    }).then(function successCallback(response) {
	//        $scope.lst = response.data;
	//        //$scope.detailgrid($scope.lst);
	//        window.lst = response.data;

	//    });
	//}
	$scope.recorddoubleclick = function ($event) {
		//debugger;
		var gridObj = $("#GridGatePass").data("ejGrid");
		//getting corresponding record 
		var data = gridObj.getSelectedRecords()[0];
		var x = $event;
		var Id = data.Id;

		$scope.productNew = data;

		$scope.productNew.PartyName = data.Vendor;
		$scope.productNew.GatePassStatus1 = data.GatePassStatus1;
		// $scope.GetChallan();
		// 
		$scope.productNew.EmployeeName = data.SenderName;
		//$("#GatePassType").change($scope.GetChallan());

		$scope.Id = $scope.productNew.Id;
		if (x.data.ToType === 'Vendor') {
			//$scope.productNew.ResponsiblePersonName = '';
			//$scope.TypeLabel = 'Vendor';
			//$scope.productNew.BuyerId = '';
			//$scope.productNew.OtherPlantId = '';
			//$scope.GetPlantList.OtherPlantId = '';
			//$scope.productNew.OtherCompanyName = '';
			//$scope.productNew.PersonName = '';
			//$scope.productNew.MobileNo = '';
			//$scope.productNew.Address = '';
			$scope.productNew.ToType = 'Vendor';
			// $scope.changeSourceFrom($scope.productNew.ToType);
			//$scope.GetDivisionList = [];
			//$scope.GetUnitList = [];
			//$scope.GetDepartmentList = [];
			//$scope.productNew.DepartmentEmployeeName = '';
		}
		if (x.data.ToType === 'Buyer') {
			//$scope.TypeLabel = 'Buyer';
			//$scope.productNew.PartyName = '';
			//$scope.productNew.PartyCode = '';
			//$scope.productNew.BuyerId = '';
			//$scope.productNew.OtherPlantId = '';
			//$scope.GetPlantList.OtherPlantId = '';
			//$scope.GetPlantList.OtherPlantId = '';
			//$scope.productNew.OtherCompanyName = '';
			//$scope.productNew.PersonName = '';
			//$scope.productNew.MobileNo = '';
			//$scope.productNew.Address = '';
			//$scope.GetBuyer();
			$scope.productNew.ToType = 'Buyer';
			//$scope.changeSourceFrom($scope.productNew.ToType);
			//$scope.GetDivisionList = [];
			//$scope.GetUnitList = [];
			//$scope.GetDepartmentList = [];
			//$scope.productNew.DepartmentEmployeeName = '';


		}
		if (x.data.ToType === 'OtherPlant') {
			//$scope.TypeLabel = 'OtherPlant';
			//$scope.productNew.PartyName = '';
			//$scope.productNew.PartyCode = '';
			//$scope.productNew.BuyerId = '';
			//$scope.productNew.OtherPlantId = '';
			//$scope.GetPlantList.OtherPlantId = '';
			//$scope.productNew.OtherCompanyName = '';
			//$scope.productNew.PersonName = '';
			//$scope.productNew.MobileNo = '';
			//$scope.productNew.Address = '';
			$scope.productNew.ToType = 'OtherPlant';
			// $scope.changeSourceFrom($scope.productNew.ToType);
			//$scope.GetDivisionList = [];
			//$scope.GetUnitList = [];
			//$scope.GetDepartmentList = [];
			//$scope.productNew.DepartmentEmployeeName = '';
			//$scope.productNew.PartyName = x.data.Vendor;
			$scope.productNew.ToPlantId = x.data.ToPlantId;
			$http({
				method: 'GET',
				url: 'Products/GateentryToken/GetUnit?plantId=' + $scope.productNew.ToPlantId
			}).then(function (response) {
				$scope.GetUnitList = response.data;
			});
			$scope.productNew.ToPlantId = x.data.ToPlantId;
			$scope.productNew.EmployeeName = x.data.SenderName;

			$scope.productNew.ToUnitId = x.data.ToUnitId;
			$scope.productNew.ToDepartment = x.data.ToDepartment;
			$scope.productNew.ToDivisionId = x.data.ToDivisionId;
			$scope.productNew.DepartmentEmployeeName = x.data.DepartmentEmployee;
		}
		if (x.data.ToType === 'OtherCompany') {
			//$scope.TypeLabel = 'OtherCompany';
			//$scope.productNew.PartyName = '';
			//$scope.productNew.PartyCode = '';
			//$scope.productNew.BuyerId = '';
			//$scope.GetPlantList.OtherPlantId = '';
			//$scope.productNew.OtherPlantId = '';
			//$scope.productNew.OtherCompanyName = '';
			//$scope.productNew.PersonName = '';
			//$scope.productNew.MobileNo = '';
			//$scope.productNew.Address = '';
			$scope.productNew.ToType = 'OtherCompany';
			// $scope.changeSourceFrom($scope.productNew.ToType);
			//$scope.GetDivisionList = [];
			//$scope.GetUnitList = [];
			//$scope.GetDepartmentList = [];
			//$scope.productNew.DepartmentEmployeeName = '';


		}
		
		$http({
			method: 'GET',
			url: 'Products/GateentryToken/GetChallanNo'
		}).then(function (response) {
			$scope.GetChallanList = response.data;
			// $scope.productNew.ChallanNo = '';
			//var defaultId = data.ChallanNo;
			//$('#ChallanNo').ejDropDownList({

			//    dataSource: $scope.GetChallanList,

			//    fields: { text: "Id", value: "Id" },
			//    value: defaultId
			//});
		});

		// $("#GatePassType").val(data.ChallanNo);
		// $("#GatePassType").ejDropDownList("selectItemByValue", "data.ChallanNo");
		getInventoryMaterialList(x.data.Id);

		//

		$scope.Action = 'Update';
		if (!$rootScope.isCollapsed) $rootScope.toggle();
	};
	$scope.recorddoubleclickChallan = function ($event) {
		//debugger;
		var gridObj = $("#GridReturnChallan").data("ejGrid");
		//getting corresponding record 
		var data = gridObj.getSelectedRecords()[0];
		$scope.productNew.ChallanNo = data.Value;
		if (data.ToType === 'Vendor') {
			$scope.productNew.ToType = 'Vendor';
			$scope.productNew.PartyName = data.Vendor;
			$scope.productNew.ToPartyCode = data.ToPartyCode;
		}
		if (data.ToType === 'Buyer') {
			$scope.productNew.ToType = 'Buyer';
		}
		if (data.ToType === 'OtherPlant') {
			$scope.productNew.ToType = 'OtherPlant';
			$scope.productNew.DepartmentEmployeeId = data.FromEmployeeId;
			$scope.productNew.DepartmentEmployeeName = data.SenderName;
			//$scope.GetChallanReturnToAddress();
			$http({
				method: 'GET',
				url: 'Products/GateentryToken/GetChallanNoDetailsForToAddress?DepartmentEmployeeId=' + $scope.productNew.DepartmentEmployeeId
			}).then(function (response) {
				$scope.GetChallanReturnToAddress = response.data[0];
				$scope.productNew.ToPlantId = $scope.GetChallanReturnToAddress.ToPlantId;
				$scope.productNew.PlantName = $scope.GetChallanReturnToAddress.PlantName;
				$scope.productNew.ToUnitId = $scope.GetChallanReturnToAddress.ToUnitId;
				$scope.productNew.UnitName = $scope.GetChallanReturnToAddress.UnitName;
				$scope.productNew.ToDepartment = $scope.GetChallanReturnToAddress.ToDepartment;
				$scope.productNew.DepartmentName = $scope.GetChallanReturnToAddress.DepartmentName;
				$scope.productNew.ToDivisionId = $scope.GetChallanReturnToAddress.ToDivisionId;
				$scope.productNew.DivisionName = $scope.GetChallanReturnToAddress.DivisionName;

			});
		}
		if (data.ToType === 'OtherCompany') {
			$scope.productNew.ToType = 'OtherCompany';
		}

		$scope.closeDetaiPopUpChallan();
	}
	$scope.Delete = function () {

		//if (baseService.arrayLength($scope.inventoryMaterialList) === 0 && baseService.arrayLength($scope.chargesList) === 0) {
		if (!baseService.isUndefinedOrNull($scope.productNew.Id)) {
			$http({
				method: 'POST',
				url: $scope.deleteUrl + $scope.productNew.Id,
				dataType: 'JSON'
			}).then(function (response) {
				if (response.data.Error === true)
					ShowResult(response.data.Message, 'failure');
				else {
					ShowResult(response.data.Message, 'success');
					ClearFields();
					$scope.GetIndexGridDataList();
				}
				function errorCallBack(response) {
					ShowResult(response.data.Message, 'failure');
				}
			});
			
		}
		//}
		//else
		//    ShowResult('First delete all line item.', 'failure');
	
		//$scope.productNew.ToType = 'Vendor';
		$scope.Type = 'Vendor';
		$scope.changeSourceFrom($scope.Type);
	};




	$scope.detailSave = function () {
		$scope.entity = [];
		if (baseService.isUndefinedOrNull($scope.detailModel.TransactionQty) || isNaN($scope.detailModel.TransactionQty)) {
			ShowResult('Enter the Qty', 'failure', 'detailPopUp');
			return false;
		}
		if (baseService.isUndefinedOrNull($scope.detailModel.Rate) || isNaN($scope.detailModel.Rate)) {
		    ShowResult('Please select Rate', 'failure', 'detailPopUp');
		    return false;
		}


		// $scope.detailSaveUrlLink = "";
		try {
			//$scope.validation();

			//if ($scope.productNew.ChallanNo != null) {
			//    $scope.invalid = true;
			//    for (var i = 0; i < $scope.inventoryMaterialList1.length; i++) {
			//        if ($scope.inventoryMaterialList1[i].Active === true) {
			//            $scope.entity = $scope.inventoryMaterialList1[i];

			//        }
			//    }    
			//    $scope.detailSaveUrlLink = $scope.detailSaveUrl1;

			//}
			//else {

			$scope.detailModel.GatePassMasterId = $scope.productNew.Id;
			$scope.detailModel.FirstCharacteristicsId = $scope.char1.CharacteristicsId;
			$scope.detailModel.FirstCharacteristicsValueId = $scope.char1.CharacteristicsValueId;
			$scope.detailModel.SecondCharacteristicsId = $scope.char2.CharacteristicsId;
			$scope.detailModel.SecondCharacteristicsValueId = $scope.char2.CharacteristicsValueId;
			$scope.detailModel.ThirdCharacteristicsId = $scope.char3.CharacteristicsId;
			$scope.detailModel.ThirdCharacteristicsValueId = $scope.char3.CharacteristicsValueId;
			$scope.detailModel.CountryId = $scope.detailModel.CountryId;
			// $scope.detailModel.CountryId = $("#Country option:selected").value();
			// $("#AvgUom option:selected").text();
			for (var i = 0; i < baseService.arrayLength($scope.inventoryMaterialList); i++) {
				if ($scope.detailModel.MaterialMasterId === $scope.inventoryMaterialList[i].MaterialMasterId &&
					$scope.detailModel.ArticleId === $scope.inventoryMaterialList[i].ArticleId &&
					$scope.detailModel.FirstCharacteristicsId === $scope.inventoryMaterialList[i].FirstCharacteristicsId &&
					$scope.detailModel.FirstCharacteristicsValueId === $scope.inventoryMaterialList[i].FirstCharacteristicsValueId &&
					$scope.detailModel.SecondCharacteristicsId === $scope.inventoryMaterialList[i].SecondCharacteristicsId &&
					$scope.detailModel.SecondCharacteristicsValueId === $scope.inventoryMaterialList[i].SecondCharacteristicsValueId &&
					$scope.detailModel.ThirdCharacteristicsId === $scope.inventoryMaterialList[i].ThirdCharacteristicsId &&
					$scope.detailModel.ThirdCharacteristicsValueId === $scope.inventoryMaterialList[i].ThirdCharacteristicsValueId) {
					return ShowResult('This material already received');
				}
			}
			$scope.materialValidation();
			$scope.entity = $scope.detailModel;

			//}

			//debugger;
			if ($scope.invalid) {
				$http({
					method: 'POST',
					url: $scope.detailSaveUrl,
					data: {
						entity: $scope.entity,
						ChallanNo: $scope.productNew.ChallanNo
					},
					dataType: 'JSON'
				}).then(function successCallback(response) {
					if (response.data.Error === true)
						ShowResult(response.data.Message, 'failure', 'detailPopUp');
					else {
						ShowResult(response.data.Message, 'success', 'detailPopUp');
						$scope.detailModel.Id = null;
						$scope.detailModel = {
							GatePassMasterId: $scope.productNew.Id

							//, CurrencyName: angular.element("#currency :selected").text()
							//, CurrencyId: $scope.productNew.CurrencyId
							//, TotalAmount: 0
							//, ToCurrencyRate: $scope.productNew.ToCurrencyRate
						};
						// $scope.taxCategoryList = [];
						getInventoryMaterialList($scope.productNew.Id);
						//$scope.getDataList();

						//$scope.getalldata();
						$scope.clearCharNames();
					}
				}), function errorCallBack(response) {
					ShowResult(response.data.Message, 'failure', 'detailPopUp');
				};
			}
		} catch (e) {
			//ShowResult(e, 'fail', 'detailPopUp');
		}
	};
	$scope.detailSaveDispatch = function () {
		$scope.entity = [];

		$scope.detailSaveUrlLink = "";
		try {
			//$scope.validation();

			if ($scope.productNew.ChallanNo != null) {
				$scope.invalid = true;
				//if ($scope.inventoryMaterialList1.length > 0) {
				//    for (var i = 0; i < $scope.inventoryMaterialList1.length; i++) {
				//        for (var j = 0; j < $scope.inventoryMaterialList.length; j++) {

				//            if ($scope.inventoryMaterialList1[i].MaterialMasterId === $scope.inventoryMaterialList[j].MaterialMasterId) {
				//                ShowResult('Combination Already Exists', 'failure', 'detailPopUp');
				//                return false;
				//            }
				//            //else {
				//            //    for (var i = 0; i < $scope.inventoryMaterialList1.length; i++) {
				//            //        if ($scope.inventoryMaterialList1[i].Active === true) {
				//            //            $scope.inventoryMaterialList1[i].GatePassMasterId = $scope.productNew.Id;
				//            //            $scope.inventoryMaterialList1[i].Id = "";
				//            //            $scope.entity.push($scope.inventoryMaterialList1[i]);

				//            //            // $scope.entity[.Id=GatePassMasterId: $scope.productNew.Id

				//            //        }
				//            //    }
				//            //}

				//        }

				//    }
				//}
				//else {
				for (var i = 0; i < $scope.inventoryMaterialList1.length; i++) {
					if ($scope.inventoryMaterialList1[i].Active === true) {
						if (((parseFloat($scope.inventoryMaterialList1[i].ReturnedQty) + parseFloat($scope.inventoryMaterialList1[i].TransactionQty)) > $scope.inventoryMaterialList1[i].Qty)) {
							ShowResult('Current Qty can not grater than Qty', 'failure', 'detailPopUp');
							return false;
						}
						$scope.inventoryMaterialList1[i].GatePassMasterId = $scope.productNew.Id;
						$scope.inventoryMaterialList1[i].Id = "";
						$scope.entity.push($scope.inventoryMaterialList1[i]);
					}
				}
				// }

				//$scope.detailSaveUrlLink = $scope.detailSaveUrl1;

			}


			//debugger;
			if ($scope.invalid) {
				$http({
					method: 'POST',
					url: $scope.detailSaveUrl1,
					data: {
						entity: $scope.entity,
						ChallanNo: $scope.productNew.ChallanNo,
						MasterId: $scope.productNew.Id
					},
					dataType: 'JSON'
				}).then(function successCallback(response) {
					if (response.data.Error === true)
						ShowResult(response.data.Message, 'failure', 'detailPopUp');
					else {
						ShowResult(response.data.Message, 'success', 'detailPopUp');
						$scope.detailModel.Id = null;
						$scope.detailModel = {
							GatePassMasterId: $scope.productNew.Id

							//, CurrencyName: angular.element("#currency :selected").text()
							//, CurrencyId: $scope.productNew.CurrencyId
							//, TotalAmount: 0
							//, ToCurrencyRate: $scope.productNew.ToCurrencyRate
						};
						// $scope.taxCategoryList = [];
						getInventoryMaterialList($scope.productNew.Id);
						//$scope.getDataList();

						//$scope.getalldata();
						$scope.clearCharNames();
					}
				}), function errorCallBack(response) {
					ShowResult(response.data.Message, 'failure', 'detailPopUp');
				};
			}
		} catch (e) {
			//ShowResult(e, 'fail', 'detailPopUp');
		}
	};
	$scope.inventoryMaterialList = [];
	function getInventoryMaterialList(inveReveiveId) {
		$scope.masterId = inveReveiveId;
		//debugger;

		$http.get($scope.path + 'GetInventoryMaterialList?inveReveiveId=' + inveReveiveId)
			.then(function (response) {
				$scope.inventoryMaterialList = response.data.Rows;
			});

	}
	$scope.inventoryMaterialList1 = [];
	function getInventoryMaterialListchallDetails(inveReveiveId) {

		$scope.masterId = inveReveiveId;
		//debugger;
		// $scope.inventoryMaterialList1 = [];
		$http.get($scope.path + 'GetInventoryMaterialList?inveReveiveId=' + inveReveiveId + '&GatePassNewId=' + $scope.productNew.Id)
			.then(function (response) {
				$scope.inventoryMaterialList1 = response.data.Rows;
			});

	}
	$scope.valuePassInDelModal = function (MaterialReqqusitionMasterId) {
		//debugger;
		$scope.id = MaterialReqqusitionMasterId;
		$scope.message = 'Are you sure want to permanently delete this?';
		angular.element(document.querySelector('#removerPopUp')).modal('show');
	};
	$scope.detailDelete = function () {
		try {
			$http({
				method: 'POST',
				url: $scope.detailDeleteUrl + $scope.id
			}).then(function successCallback(response) {
				if (response.data.Error === true)
					ShowResult(response.data.Message, 'failure');
				else {
					ShowResult(response.data.Message, 'success');
					$scope.id = null;
					getInventoryMaterialList($scope.productNew.Id);
					//$scope.getDataList();
				}
			}), function errorCallBack(response) {
				ShowResult(response.data.Message, 'failure');
			};
		} catch (e) {
			ShowResult(e, 'success');
		}
	};


	$window.onresizeReturnDispath = function (event) {
		$scope.ScrollForReturnDispath();
	};
	$scope.ScrollForReturnDispath = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObjPOPendingForGRN = $("#GridReturndisplatch").ejGrid("instance");
				var scrollerwidth = $("#GridReturndisplatch1").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObjPOPendingForGRN.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObjPOPendingForGRN.windowonresize();
			}
		} catch (e) {
			// $scope.ShowResultCustom(e, 'failure');
		}
	};
	//$window.onresize = function (event) {
	//    $scope.ScrollGatePassUncheckList();
	//};
	//$scope.ScrollGatePassUncheckList = function (args) {
	//    try {
	//        if (args.requestType === "refresh") {
	//            var gridObjPOPendingForGRN = $("#GatePassUncheckList").ejGrid("instance");
	//            var scrollerwidth = $("#GatePassUncheckList1").width();//Obtain the width of the container

	//            //   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
	//            gridObjPOPendingForGRN.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
	//            gridObjPOPendingForGRN.windowonresize();
	//        }
	//    } catch (e) {
	//        // $scope.ShowResultCustom(e, 'failure');
	//    }
	//};

	//$scope.ScrollGatePassHoldRejectList = function (args) {
	//    //debugger;
	//    try {
	//        if (args.requestType === "refresh") {
	//            var gridObj1 = $("#GatePassHoldRejectList").ejGrid("instance");
	//            var scrollerwidth = $("#GatePassHoldRejectList1").width();//Obtain the width of the container

	//            //   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
	//            gridObj1.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
	//            gridObj1.windowonresize();
	//        }
	//    } catch (e) {
	//        // $scope.ShowResultCustom(e, 'failure');
	//    }
	//};

	//$window.onresize = function (event) {

	//    $scope.ScrollGatePassHoldRejectList();
	//};






	//#region Gate Pass System Checked And Approved

	// #region  Tab Control for Requisition
	$scope.tabType = '';
	$scope.uiType = function () {
		$scope.url = $location.absUrl().split('!/')[1];
		if ($scope.url === 'gate-pass-approved') {
			$scope.tabType = 'UnApprovedList';
		}
		else if ($scope.url === 'gate-pass-checked') {
			$scope.tabType = 'UnCheckedList';
		}
		else if ($scope.url === 'gate-pass-dispatch') {
			$scope.tabType = 'UnDispatchList';
		}

	}
	$scope.uiType();
	$scope.tab = 1;
	//UnCheckedList, HoldRejectCheckedList, CheckedList, UnApprovedList, HoldRejectApprovedList, ApprovedList   
	$scope.setTabGatePassUnCheckedList = function (newTab) {

		$scope.tab = newTab;
		$scope.tabType = 1//;'UnCheckedList';
		$scope.GetGetCheckedApprovedList();

	};
	$scope.isSetGatePassUnCheckedList = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabGatePassHoldRejectList = function (newTab) {
		//debugger;
		$scope.tabType = 2;// 'HoldRejectCheckedList';
		$scope.tab = newTab;

		$scope.GetGetCheckedApprovedList();

	};
	$scope.isSetGatePassHoldRejectList = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabGatePassCheckedList = function (newTab) {
		$scope.tab = newTab;
		$scope.tabType = 3;// 'CheckedList';
		$scope.GetGetCheckedApprovedList();
	};
	$scope.isSetGatePassCheckedList = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabGatePassUnApprovedList = function (newTab) {
		$scope.tab = newTab;
		$scope.tabType = 4;//'UnApprovedList';
		$scope.GetGetCheckedApprovedList();

	};
	$scope.isSetGatePassUnApprovedList = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabGatePassHoldRejectApprovedList = function (newTab) {
		$scope.tab = newTab;
		$scope.tabType = 5;//'HoldRejectApprovedList';

		$scope.GetGetCheckedApprovedList();

	};
	$scope.isSetGatePassHoldRejectApprovedList = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabGatePassApprovedList = function (newTab) {
		$scope.tab = newTab;
		$scope.tabType = 6;//'ApprovedList';
		$scope.GetGetCheckedApprovedList();

	};
	$scope.isSetGatePassApprovedList = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabGatePassUnDispatchList = function (newTab) {
		$scope.tab = newTab;
		$scope.tabType = 9;//"UnDispatchList"
		$scope.GetGetCheckedApprovedList();

	};
	$scope.isSetGatePassUnDispatchList = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabGatePassHoldRejectDispatchList = function (newTab) {
		$scope.tab = newTab;
		$scope.tabType = 7;//'HoldRejectDispatchList';

		$scope.GetGetCheckedApprovedList();

	};
	$scope.isSetGatePassHoldRejectDispatchList = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabGatePassDispatchList = function (newTab) {
		$scope.tab = newTab;
		$scope.tabType = 8;//'DispatchList';
		$scope.GetGetCheckedApprovedList();

	};
	$scope.isSetGatePassDispatchList = function (tabNum) {
		return $scope.tab === tabNum;
	};
	// #endregion
	//#endregion


	$scope.CheckedApproved = [];
	$scope.GetGetCheckedApprovedList = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GateentryToken/GetCheckedApprovedList?tabType=' + $scope.tabType
		}).then(function successCallback(response) {
			$scope.CheckedApproved = response.data;
			for (var i = 0; i < $scope.CheckedApproved.length; i++) {
				response.data[i].GatePassEntryDate = new Date($scope.CheckedApproved[i].GatePassEntryDate);
			}
		});
	}
	$scope.GetGetCheckedApprovedList();

	$scope.GatePassCheckApprovedAlertCall = function () {
		$scope.message = 'Are you sure want to Approve?';
		angular.element(document.querySelector('#GatePassCheckApprovedAlert')).modal('show');
	};
	$scope.onClickGatePassChecked = function (z) {
		//debugger;
		var x = "#" + z;
		var gridObj = $(x).data("ejGrid");
		$scope.gatePassChkedAppData = gridObj.getSelectedRecords()[0];
		$scope.url = $location.absUrl().split('!/')[1]
		if ($scope.url === 'gate-pass-approved') {
			//$scope.tabType = 'UnApprovedList';
			$scope.POPUpStatus = $scope.gatePassChkedAppData.ApprovedByStatus;
		}
		else if ($scope.url === 'gate-pass-checked') {
			// $scope.tabType = 'UnCheckedList';
			$scope.POPUpStatus = $scope.gatePassChkedAppData.CheckedByStatus;
		}
		$scope.message = 'Are you sure want to ' + $scope.POPUpStatus + '?';
		angular.element(document.querySelector('#GatePassCheckApprovedAlert')).modal('show');
	};

	$scope.GatePassCheckApproved = function () {
		debugger;
		if ($scope.url === 'gate-pass-checked') {
			if (baseService.isUndefinedOrNull($scope.gatePassChkedAppData.CheckedByStatus) || $scope.gatePassChkedAppData.CheckedByStatus === "Select") {
				ShowResult("Please Select Checked By Status", 'failure');
				return false;
			}
			else if ($scope.gatePassChkedAppData.CheckedByStatus === "Hold" || $scope.gatePassChkedAppData.CheckedByStatus === "Reject") {
				if (baseService.isUndefinedOrNull($scope.gatePassChkedAppData.CheckedHoldRejectReason)) {
					ShowResult("Enter The Reason", 'failure');
					return false;
				}

			}
			else if ($scope.gatePassChkedAppData.CheckedByStatus === "Checked") {
				if (baseService.isUndefinedOrNull($scope.gatePassChkedAppData.ApprovedBy)) {
					ShowResult("Please Select Approved By", 'failure');
					return false;
				}

			}
			$http({
				method: 'POST',
				url: 'Products/GateentryToken/GatePassCheckedAndApproved',
				data: {
					'Id': $scope.gatePassChkedAppData.Id,
					'PoValue': $scope.gatePassChkedAppData.TotalQty,
					'CheckedApprovedStataus': $scope.gatePassChkedAppData.CheckedByStatus,
					'RejectReason': $scope.gatePassChkedAppData.CheckedHoldRejectReason,
					'CheckedApprovedBy': $scope.gatePassChkedAppData.ApprovedBy,
					'UIType': $scope.url
				},
				dataType: 'JSON'
			}).then(function successCallback(response) {
				if (response.data.Error === true) {
					ShowResult('Gate Pass Updated successfully', 'failure');
				}
				else {
					ShowResult('Gate Pass Updated successfully', 'success');
					$scope.GetGetCheckedApprovedList();
				}
			}, function errorCallBack(response) {
				ShowResult(response.data.Message, 'failure');
			});
		}
		else if ($scope.url === 'gate-pass-approved') {
			if (baseService.isUndefinedOrNull($scope.gatePassChkedAppData.ApprovedByStatus) || $scope.gatePassChkedAppData.ApprovedByStatus === "Select") {
				ShowResult("Please Select Approved By Status", 'failure');
				return false;
			}
			else if ($scope.gatePassChkedAppData.ApprovedByStatus === "Hold" || $scope.gatePassChkedAppData.ApprovedByStatus === "Reject") {
				if (baseService.isUndefinedOrNull($scope.gatePassChkedAppData.ApprovedHoldRejectReason)) {
					ShowResult("Enter The Reason", 'failure');
					return false;
				}

			}
			//else if ($scope.gatePassChkedAppData.ApprovedByStatus === "Approved") {

			//	if (baseService.isUndefinedOrNull($scope.gatePassChkedAppData.SenderSecurityEmployeeId)) {
			//		ShowResult("Please Select Security Person Name", 'failure');
			//		return false;
			//	}
			//}

			$http({
				method: 'POST',
				url: 'Products/GateentryToken/GatePassCheckedAndApproved',
				data: {
					'Id': $scope.gatePassChkedAppData.Id,
					'PoValue': $scope.gatePassChkedAppData.TotalQty,
					'CheckedApprovedStataus': $scope.gatePassChkedAppData.ApprovedByStatus,
					'RejectReason': $scope.gatePassChkedAppData.ApprovedHoldRejectReason,
					'CheckedApprovedBy': $scope.gatePassChkedAppData.SenderSecurityEmployeeId,
					'UIType': $scope.url
				},
				dataType: 'JSON'
			}).then(function successCallback(response) {
				if (response.data.Error === true) {
					ShowResult('Gate Pass Updated successfully', 'failure');
				}
				else {
					ShowResult('Gate Pass Updated successfully', 'success');
					$scope.GetGetCheckedApprovedList();
				}
			}, function errorCallBack(response) {
				ShowResult('Gate Pass Updated successfully', 'failure');
			});
		}
	}

	$scope.tab = 1;
	$scope.setTab = function (newTab) {
		$scope.tab = newTab;
	};
	$scope.isSet = function (tabNum) {
		return $scope.tab === tabNum;
	};


	$scope.CheckedStatusList = function () {
		cboService.getEnumCbo("enum/GetPOApprovalStatusCbo", function (result) {
			$scope.checkedstatusList = result;
		});
	}
	$scope.CheckedStatusList();
	$scope.LoadapprovalStatus = function () {
		cboService.getEnumCbo("enum/GetCheckedStatusCbo", function (result) {
			$scope.approvalStatusList = result;
		});

	}
	$scope.LoadapprovalStatus();

	//#region CheckBY Approve BY Secusity Approve

	$scope.checkedByList = [];
	$scope.GetToBeCheckedByList = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GateentryToken/GetGatePassCheckedBy'
		}).then(function successCallback(response) {
			$scope.checkedByList = response.data;
		});
	}
	$scope.GetToBeCheckedByList();


	$scope.ApprovedByList = [];
	$scope.GetToBeApprovedByByList = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GateentryToken/GetGatePassApproveddBy'
		}).then(function successCallback(response) {
			$scope.ApprovedByList = response.data;
		});
	}
	$scope.GetToBeApprovedByByList();

	$scope.ApprovedBySecurityList = [];
	$scope.GetToBeSecurityByList = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GateentryToken/GetGatePassTobeApproveddSecurityBy'
		}).then(function successCallback(response) {
			$scope.ApprovedBySecurityList = response.data;
		});
	}
	$scope.GetToBeSecurityByList();

	//#endregion







































































	addressService.getCountryCbo(function (result) {
		$scope.countryList = result;
	});


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
	// $scope.GetSupervisorCboList();
	$scope.checkedByList1 = [];
	$scope.GetSupervisorCboList1 = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/PurchaseOrder/GetSupervisorCboApproved'
		}).then(function successCallback(response) {
			$scope.checkedByList1 = response.data;
		});
	}
	// $scope.GetSupervisorCboList1();



	$http({
		method: 'GET',
		url: 'currencies/CompanyParallelCurrency/CboParallelCurrency'
	}).then(function successCallback(response) {
		$scope.baseCurrencyId = response.data[0].Value;
		$scope.productNew.BaseCurrencyId = response.data[0].Value;
		factoryService.getCurrencyPrecision($scope.baseCurrencyId);
	});

	cboService.getCboTransactionCurrencyByCompany('', function (result) {
		$scope.currencyList = result;
	});

	$http.get('accounts/OpeningBalance/GetACCCutOffDate')
		.then(function (response) {
			if (response.data !== null && !baseService.isUndefinedOrNull(response.data.CutOffDate)) {
				$scope.productNew.CutOffDate = response.data.CutOffDate;
				$('#cutOffDate').datepicker('setStartDate', new Date($scope.productNew.CutOffDate));
			}
			else
				ShowResult('Cut Off date not found!', 'failure');
		});

	$scope.searchByList = [
		{
			value: 'PartyCode'
			, name: 'Vendor Code'
		},
		{
			value: 'PartyName'
			, name: 'Vendor Name'
		},
		{
			value: 'PartyAccountGroupName'
			, name: 'Account Group'
		},
		{
			value: 'Id'
			, name: 'GRN No'
		},
		{
			value: 'GRNDate'
			, name: 'GRN Date'
		},
		{
			value: 'DocRefNo'
			, name: 'Vendor DocRefNo'
		},
		{
			value: 'InvoiceNo'
			, name: 'Invoice No'
		},
		{
			value: 'InvoiceDate'
			, name: 'Invoice Date'
		}
	];

	$scope.partySearchByList = [
		{
			'name': $scope.partyType + ' Code',
			'value': 'Code'
		},
		{
			'name': $scope.partyType + ' Name',
			'value': 'UserName'
		},
		{
			'name': 'Account Group',
			'value': 'PartyAccountGroupName'
		},
		{
			'name': 'Country',
			'value': 'CountryName'
		},
		{
			'name': 'State',
			'value': 'StateName'
		},
		{
			'name': 'Currency',
			'value': 'CurrencyCode'
		}
	];

	$scope.Get = function (index) {
		$scope.index = index;
		$scope.product = $scope.products[$scope.index];
		$scope.productNew = Object.assign({}, $scope.product);
		getPartyPlantList();
		getInventoryMaterialList($scope.productNew.Id);
		getServiceChargeList($scope.productNew.Id);
		//$scope.getToCurrencyRate();
		if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
			var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
			if (paymentTerm.BaseLineDate !== null)
				if (paymentTerm.BaseLineDate === 'documentdate')
					$scope.IsBaseOnDueDateEnable = true;
				else
					$scope.IsBaseOnDueDateEnable = false;
		}
		$scope.Action = 'Update';
		if (!$rootScope.isCollapsed) $rootScope.toggle();
	};

	function GetMasterData() {
		var aa = $("#masterId").text();
		$http.get('Products/Requisition/GetPOMasterById?id=' + aa).then(function (response) {
			$scope.productNew = response.data;
		});

		getPartyPlantList();
		getInventoryMaterialList($scope.productNew.Id);
		getServiceChargeList($scope.productNew.Id);
		//$scope.getToCurrencyRate();
		if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
			var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
			if (paymentTerm.BaseLineDate !== null)
				if (paymentTerm.BaseLineDate === 'documentdate')
					$scope.IsBaseOnDueDateEnable = true;
				else
					$scope.IsBaseOnDueDateEnable = false;
		}
		$scope.Action = 'Update';
		if (!$rootScope.isCollapsed) $rootScope.toggle();
	};
	$scope.ConModal = function (id) {
		$scope.id = id;
		$scope.message = 'Are you sure want to Save back date data?';
		angular.element(document.querySelector('#ConPopUp')).modal('show');
	};
	$scope.ConfirmSave = function () {
		try {
			$scope.$broadcast('show-errors-check-validity');
			if ($scope.productNewForm.$valid) {

				// $scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
				$scope.product = Object.assign({}, $scope.productNew);
				if ($scope.Action === "Save") {
					$http({
						method: 'POST',
						url: $scope.saveUrl,
						data: $scope.product,
						dataType: 'JSON'
					}).then(function (response) {
						if (response.data.Error === true) {
							ShowResult(response.data.Message, 'failure');
						}
						else {
							ShowResult(response.data.Message, 'success');
							$scope.productNew.Id = response.data.entity.Id;
							//$scope.productNew.PartyName = $scope.product.PartyName;

							$scope.Action = "Update";
							//$scope.getDataList();
							$scope.GetReq();
						}
					}), function (response) {
						ShowResult(response.data.Message, 'failure');
					};
				}
				else if ($scope.Action === "Update") {
					ShowResult('You Do not have permission to update', 'failure');
					//$http({
					//    method: 'POST',
					//    url: $scope.updateUrl,
					//    data: $scope.product,
					//    dataType: 'JSON'
					//}).then(function successCallback(response) {
					//    if (response.data.Error === true) {
					//        ShowResult(response.data.Message, 'failure');
					//    }
					//    else {
					//        ShowResult(response.data.Message, 'success');

					//        $scope.GetReq();

					//    }
					//}, function errorCallBack(response) {
					//    ShowResult(response.data.Message, 'failure');
					//});
				}
			}
		} catch (e) {
			throw e;
		}
	}

	// #region Extra Tax Add
	$scope.calculateTaxAmount = function (data) {
		//data.TotalAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
		data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
	};
	$scope.receiveTaxList = [];
	//$scope.closeReceiveTaxPopUp = function () {

	//    //var materialData = $scope.salesMaterialList[$scope.currentMaterialRow];
	//    $scope.inventoryMaterialList[$scope.currentMaterialRow].TaxAmount = null;
	//    angular.forEach($scope.receiveTaxList, function (item) {
	//        $scope.inventoryMaterialList[$scope.currentMaterialRow].BaseTaxAmount += item.TotalAmount;
	//    });     

	//    $scope.inventoryMaterialList[$scope.currentMaterialRow].BaseAmount = parseFloat($scope.inventoryMaterialList[$scope.currentMaterialRow].TrnAmount) + parseFloat($scope.inventoryMaterialList[$scope.currentMaterialRow].BaseTaxAmount);
	//    $scope.materialMaster = {};
	//    //$scope.receiveTaxList = [];
	//    $scope.isService = false;
	//    //Extra Tax Will add here  shakawat

	//   // $scope.detailModel = $scope.currentInventoryReceiveDetailIdRow;
	//   // $scope.detailModel[0].InventoryReceiveDetailId = $scope.currentInventoryReceiveDetailIdRow;


	//   // //if ($scope.TAction === "OK") {
	//   //     $http({
	//   //         method: 'POST',
	//   //         //url: $scope.saveUrl,
	//   //         url: '/Products/Requisition/InsertExtraTax',
	//   //         //data: $scope.receiveTaxList,
	//   //         data: {
	//   //               entity: $scope.detailModel
	//   //             , taxCategoryList: $scope.receiveTaxList
	//   //         },
	//   //         dataType: 'JSON'
	//   //     }).then(function (response) {
	//   //         if (response.data.Error === true) {
	//   //             ShowResult(response.data.Message, 'failure');
	//   //         }
	//   //         else {
	//   //             ShowResult(response.data.Message, 'success');
	//   //             //$scope.productNew.Id = response.data.entity.Id;
	//   //            // $scope.productNew.PartyName = $scope.product.PartyName;
	//   //            // $scope.Action = "Update";
	//   //             //$scope.getDataList();
	//   //         }
	//   //     }), function (response) {
	//   //         ShowResult(response.data.Message, 'failure');
	//   //     };
	//   //// }



	//   //angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
	//};
	$scope.LoadTaxButtonClick = function () {
		accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
			$scope.taxCategoryList = result;
		});
	}
	accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
		$scope.taxCategoryList = result;
	});
	$scope.addTax = function () {
		var data = {
			TotalAmount: 0,
			Id: null,
			HSNCode: $scope.HSNCode,
			HSNCodeId: null,
			UserName: null,
			TaxCategoryId: null
		};
		$scope.receiveTaxList.push(data);

	};

	// #endregion 


	$scope.Cancel = function () {
		//if (baseService.arrayLength($scope.inventoryMaterialList) === 0 && baseService.arrayLength($scope.chargesList) === 0) {
		if (!baseService.isUndefinedOrNull($scope.productNew.Id)) {
			$http({
				method: 'POST',
				url: $scope.deleteUrl1 + $scope.productNew.Id,
				dataType: 'JSON'
			}).then(function (response) {
				if (response.data.Error === true)
					ShowResult(response.data.Message, 'failure');
				else {
					ShowResult(response.data.Message, 'success');
					$scope.GetReq();
					ClearFields();
				}
				function errorCallBack(response) {
					ShowResult(response.data.Message, 'failure');
				}
			});
		}
		//}
		//else
		//    ShowResult('First delete all line item.', 'failure');
	};
	$scope.Clear = function () {
		ClearFields();
		if (!$rootScope.isCollapsed) $rootScope.toggle();
		return true;
	};

	function ClearFields() {
		//$scope.GateEntryType = 'Vendor';
		$scope.Type = 'Vendor';
		$scope.changeSourceFrom($scope.Type);
		$scope.Action = "Save";
		$scope.product = {};
		$scope.IsBaseOnDueDateEnable = false;
		$scope.productNew = {
			Id: null
			, CompanyGroupId: null
			, EntryDate: $filter("dateFiltering")(Date.now())
			, PartyCode: null
			, Description: null
			, PackageQty: null
			, ModeofTransport: null
			, Bill: null
			, PersonName: null
			, MobileNo: null
			, Remarks: null
			, InvoicingPartyPlantId: null
			, InvoicingByAddress: null
			, DeliveryPartyPlantId: null
			, DeliveryByAddress: null
			, CompanyId: null
			, PlantId: null
			, GateEntryTime: new Date()
			, GateEntryType: 'Vendor'
			, PlantWiseGateId: null
			, GatePassEntryDate: $filter("dateFiltering")(Date.now())
			, GatePassType: 'Send'
			, GatePassStatus1: 'NonReturnable'
			, GateRegisterType: 'Out'
		};

		$scope.inventoryMaterialList = [];
		$scope.chargesList = [];
		$scope.grossTotal = 0;
		baseService.removeErrorClasses();
		//$scope.getToCurrencyRate();
	}

	$scope.changeAllInvoice = function () {
		$scope.productNew.InvoiceNo = null;
		$scope.productNew.InvoiceDate = null;
	};

	$scope.GetCurrencyExchangeRateList = function () {
		//debugger;
		//if (!baseService.isUndefinedOrNull($scope.voucher.PostingDate) && !baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
		if (!baseService.isUndefinedOrNull(!baseService.isUndefinedOrNull($scope.productNew.CurrencyId))) {
			$http({
				method: "GET",
				//url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.voucher.PostingDate + "&currencyId=" + $scope.voucher.CurrencyId
				url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?currencyId=" + $scope.productNew.CurrencyId
			}).then(function successCallback(response) {
				$scope.currencyExchangeRate = response.data;
				$scope.productNew.ToCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
			});
		}
		else {
			$scope.currencyExchangeRate = null;
		}
	};
	$scope.getToCurrencyRate = function () {
		//debugger;
		//if (baseService.isUndefinedOrNull($scope.productNew.DocDate)) {
		//    $scope.productNew.ToCurrencyRate = 1;
		//    return;
		//}
		$http.get($scope.path + 'GetToCurrencyRate?currencyId=' + $scope.detailModel.CurrencyId)
			.then(function (response) {
				if (parseFloat(response.data) === 0) {


					$scope.productNew.ToCurrencyRate = 1;
					$scope.detailModel.CurrencyName = angular.element("#currency :selected").text();
				}
				else {


					$scope.detailModel.ToCurrencyRate = response.data;
					$scope.detailModel.CurrencyName = angular.element("#currency :selected").text();
				}
			});
	};
	$scope.invoicingPartyPopUp = function () {
		//getPartyPlantEditList();
		angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
	};
	$scope.closeInvoicingPartyPopUp = function () {
		//debugger;
		//$scope.dbval = $scope.StateData;
		//$scope.UIval = $scope.productNew.InvoicingState;      

		//if ($scope.inventoryMaterialList.length == 0) {
		//    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
		//}
		//else if ($scope.dbval.length == 0)
		//{
		//    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
		//}
		//else if ($scope.dbval == $scope.UIval ) {            
		//    angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
		//}
		//else {
		//    ShowResult('You can not change Invoicing party.Line is available', 'failure', 'invoicingPartyPopUp');

		//}

		if ($scope.inventoryMaterialList.length || $scope.chargesList.length) {
			if (!baseService.isUndefinedOrNull($scope.productNew.ChangeInvoicingStateId)) {
				if ($scope.productNew.PlantStateId === $scope.productNew.InvoicingStateId == $scope.productNew.ChangeInvoicingStateId)
					angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
				else if ($scope.productNew.InvoicingStateId === $scope.productNew.ChangeInvoicingStateId)
					angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
				else if ($scope.productNew.PlantStateId !== $scope.productNew.InvoicingStateId && $scope.productNew.PlantStateId != $scope.productNew.ChangeInvoicingStateId)
					angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
				else
					ShowResult('Change is not allowed', 'failure', 'invoicingPartyPopUp');
			}
			else
				angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
		}
		else
			angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');






	};
	//command by 30-5-19
	//$scope.billShippAddress = function (id, flag) {
	//    if (!baseService.isUndefinedOrNull(id)) {
	//        var address = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].Address1;
	//        var state = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateName;
	//        var stateId = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateId;
	//        if (flag === 'billTo') {
	//            $scope.salesVM.InvoicingState = state;
	//            $scope.salesVM.ChangeInvoicingStateId = stateId;
	//            $scope.salesVM.InvoicingGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
	//            return $scope.salesVM.InvoicingByAddress = address;
	//        }
	//        else if (flag === 'shipTo') {
	//            $scope.salesVM.DeliveryState = state;
	//            $scope.salesVM.DeliveryGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
	//            return $scope.salesVM.DeliveryByAddress = address;
	//        }
	//    }
	//    else {
	//        if (flag === 'billTo') {
	//            $scope.salesVM.InvoicingState = null;
	//            $scope.salesVM.InvoicingGSTIN = null;
	//            return $scope.productNew.InvoicingByAddress = null;
	//        }
	//        else if (flag === 'shipTo') {
	//            $scope.salesVM.DeliveryState = null;
	//            $scope.salesVM.DeliveryGSTIN = null;
	//            return $scope.salesVM.DeliveryByAddress = null;
	//        }
	//    }
	//};


	$scope.billShippAddress = function (id, flag) {
		//debugger;
		//$http({
		//    method: "GET",
		//    dataType: 'JSON',
		//    //url: $scope.getSearchListUrl,
		//    url: 'Products/Requisition/GetStateByInvoicingPartyPlantId?InvoicingPartyPlantId=' + id,
		//}).then(function successCallback(response) {
		//    $scope.StateData = response.data[0].StandardName;
		//    //alert('ff' + productNew.InvoicingPartyPlantId);

		//});
		if (!baseService.isUndefinedOrNull(id)) {
			var address = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].Address1;
			var state = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].StateName;
			var stateId = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].StateId;// 30-5
			if (flag === 'billTo') {
				$scope.productNew.InvoicingState = state;
				$scope.productNew.ChangeInvoicingStateId = stateId;//30-5
				$scope.productNew.InvoicingGSTIN = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].GSTIN;
				return $scope.productNew.InvoicingByAddress = address;
			}
			else if (flag === 'shipTo') {
				$scope.productNew.DeliveryState = state;
				$scope.productNew.DeliveryGSTIN = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].GSTIN;
				return $scope.productNew.DeliveryByAddress = address;
			}
		}
		else {
			if (flag === 'billTo') {
				$scope.productNew.InvoicingState = null;
				$scope.productNew.InvoicingGSTIN = null;
				return $scope.productNew.InvoicingByAddress = null;
			}
			else if (flag === 'shipTo') {
				$scope.productNew.DeliveryState = null;
				$scope.productNew.DeliveryGSTIN = null;
				return $scope.productNew.DeliveryByAddress = null;
			}
		}

	};
	//$scope.tab = 1;
	//$scope.setTab = function (newTab) {
	//    $scope.tab = newTab;
	//};
	//$scope.isSet = function (tabNum) {
	//    return $scope.tab === tabNum;
	//};
	// #region Details
	$scope.businessProcesses = '';

	//$scope.enable = true;
	//$scope.MAction = "Edit";
	//InventoryReceiveDetailId, TransactionQty, TransactionRate, TrnAmount, BaseTaxAmount, BaseAmount, index
	$scope.detailPopUpEdit = function () {
		////debugger;
		//if ($scope.MAction == "Edit") {
		//    $scope.index = index;
		//    $http({
		//        method: 'GET',
		//        url: $scope.path + 'GetReceiveTaxList?receiveDetailId=' + InventoryReceiveDetailId
		//    }).then(function (response) {
		//        $scope.receiveTaxList = response.data;
		//        //$scope.HSNCode = response.data[0]['HSNCode'];
		//        //angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
		//    });
		//    $scope.enable = false;
		//    $scope.MAction = "Update";

		//}
		//else if ($scope.MAction == "Update") {
		//    $http({
		//        method: 'POST',
		//        url: '/Products/Requisition/UpdateMaterial',
		//        data: {
		//            InventoryReceiveDetailId: InventoryReceiveDetailId,
		//            TransactionQty: TransactionQty,
		//            TransactionRate: TransactionRate,
		//            TrnAmount: TrnAmount,
		//            BaseAmount: BaseAmount,
		//            BaseTaxAmount: BaseTaxAmount,
		//            receiveTaxList:$scope.receiveTaxList
		//        },
		//        dataType: 'JSON'
		//    }).then(function (response) {
		//        if (response.data.Error === true) {
		//            ShowResult(response.data.Message, 'failure');
		//        }
		//        else {
		//            ShowResult(response.data.Message, 'success');
		//            //$scope.productNew.Id = response.data.entity.Id;
		//            //$scope.productNew.PartyName = $scope.product.PartyName;
		//            //$scope.Action = "Update";
		//            //getInventoryMaterialList($scope.detailModel.Id);

		//        }
		//    }), function (response) {
		//        ShowResult(response.data.Message, 'failure');
		//    };
		//    $scope.enable = true;
		//    $scope.MAction = "Edit";

		//}
		//else {
		//}
		//$scope.detailModel = {
		//    Id: data.InventoryReceiveDetailId
		//    //, CountryId: null
		//    , InventoryReceiveId: $scope.productNew.Id
		//    , MaterialStorageId: $scope.productNew.MaterialStorageId//$scope.productNew.MaterialStorageId
		//    , CurrencyName: angular.element("#currency :selected").text()
		//    , CurrencyId: $scope.productNew.CurrencyId
		//    , BaseCurrencyId: $scope.baseCurrencyId
		//    , DocDate: $scope.productNew.DocDate
		//    , InventoryMaterialId: data.Id
		//    //, MaterialMasterId: null
		//    , MaterialMasterName: data.UserName
		//    , ArticleId: null
		//    , ArticleName: data.StandardName
		//    , MaterialType: null
		//    , OurStyleName: null
		//    , Description: null
		//    , MaterialGroupMasterName: data.MaterialGroupMasterName
		//    , ProductMasterName: null
		//    , IsOurStyleRequired: false
		//    , IsProductMstRequired: false

		//    , FirstCharacteristicsId: null
		//    , FirstCharacteristicsValueId: null

		//    , SecondCharacteristicsId: null
		//    , SecondCharacteristicsValueId: null

		//    , ThirdCharacteristicsId: null
		//    , ThirdCharacteristicsValueId: null

		//    , TransactionQty: data.TransactionQty
		//    , TransactionUoMId: data.TransactionUoMId
		//    , TransactionRate: data.TransactionRate
		//    , TransactionAmount: data.TrnAmount
		//    , BaseQty: data.TransactionQty
		//    , BaseUOMId: data.TransactionUoMId
		//    , BaseUoM: data.BaseUoM
		//    , BaseUoMFactor: data.TransactionQty


		//    , TotalQty: null
		//    , TotalAmount: 0
		//    , TotalTaxAmount: 0
		//    , AvgRate: null
		//    , ToCurrencyRate: $scope.productNew.ToCurrencyRate
		//    , IsNonCreditable: $scope.productNew.IsNonCreditable
		//    , IsOriginApplicable: false
		//    , PartyCode: null
		//};

		//for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
		//    for (var t = 0; t < $scope.inventoryMaterialList[i].TaxList.length; t++) {
		//        $scope.receiveTaxList.push($scope.inventoryMaterialList[i].TaxList[t]);
		//    }

		//}
		//$scope.enable = false;
		//$scope.MAction = "Update"; 
		$http({
			method: 'POST',
			url: 'Products/Requisition/UpdateMaterial',
			data: {
				entity: $scope.inventoryMaterialList,
				receiveTaxList: $scope.receiveTaxList
			},
			dataType: 'JSON'
		}).then(function (response) {
			if (response.data.Error === true) {
				ShowResult(response.data.Message, 'failure');
			}
			else {
				ShowResult(response.data.Message, 'success');
				//$scope.productNew.Id = response.data.entity.Id;
				//$scope.productNew.PartyName = $scope.product.PartyName;
				//$scope.Action = "Update";
				//getInventoryMaterialList($scope.detailModel.Id);

			}
		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};




		//$scope.detailModel.MaterialStorageId = data.MaterialStorageId



		// data.TransactionQty=
		// $scope.clearCharNames();
		// angular.element(document.querySelector('#detailPopUpEdit')).modal('show');
	};






















	$scope.MaterilaUpdate = function () {


		try {
			$scope.$broadcast('show-errors-check-validity');
			//if (baseService.isUndefinedOrNull($scope.productNew.MaterialStorageId)) {
			//    throw 'Please select Location';
			//}
			//else {
			//if ($scope.Action === "Save") {
			if ($scope.detailPopUpEditForm.$valid) {
				$http({
					method: 'POST',
					url: 'Products/Requisition/UpdateMaterial',
					data: $scope.detailModel,
					dataType: 'JSON'
				}).then(function (response) {
					if (response.data.Error === true) {
						ShowResult(response.data.Message, 'failure', 'detailPopUpEdit');
					}
					else {
						ShowResult(response.data.Message, 'success', 'detailPopUpEdit');
						//$scope.productNew.Id = response.data.entity.Id;
						//$scope.productNew.PartyName = $scope.product.PartyName;
						//$scope.Action = "Update";
						//getInventoryMaterialList($scope.detailModel.Id);

					}
				}), function (response) {
					ShowResult(response.data.Message, 'failure', 'detailPopUpEdit');
				};
			}
			//}
			//}


		} catch (e) {
			throw e;
		}
	};
	$scope.closeDetaiPopUp = function () {
		$scope.detailModel = {};
		$scope.taxCategoryList = [];
		removeValidationMsg();
		angular.element(document.querySelector('#detailPopUp')).modal('hide');
	};
	//test
	$scope.closeDetaiPopUpEdit = function () {
		$scope.detailModel = {};
		$scope.taxCategoryList = [];
		removeValidationMsg();
		angular.element(document.querySelector('#detailPopUpEdit')).modal('hide');
	};
	$scope.materialType = ['Asset', 'Consumable', 'Spare', 'RawMaterial'];
	//$scope.setMaterialMasterData
	$scope.selectMaterialByType = function (ob) {
		//debugger;
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

		//getTaxCategoryList(ob.HSNCodeId);
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
	$scope.setCharData = function (data) {
		$scope[$scope.charValueSearchFor].CharacteristicsValueId = data.CharacteristicsValueId;
		$scope[$scope.charValueSearchFor].FreeText = data.UserName;
		$scope[$scope.charValueSearchFor].FlagDisable = $scope.isSearch;
		angular.element(document.querySelector('#searchcharactervaluepopup')).modal('hide');
	};
	$scope.materialValidation = function () {
		//var getRow = $filter("filter")($scope.inventoryMaterialList, { "MaterialMasterId": $scope.detailModel.MaterialMasterId });
		//var getRow2 = $filter("filter")($scope.inventoryMaterialList, { "MaterialMasterId": $scope.detailModel.MaterialMasterId, "ArticleId": $scope.detailModel.ArticleId });
		var getRow3 = $filter("filter")($scope.inventoryMaterialList, { "MaterialMasterId": $scope.detailModel.MaterialMasterId, "ArticleId": $scope.detailModel.ArticleId, "FirstCharacteristicsValueId": $scope.detailModel.FirstCharacteristicsValueId });
		//getRow == 0 || getRow2 == 0 ||
		if (getRow3 == 0) {
			$scope.invalid = true;
		}
		else {
			ShowResult('Material Combination Already Exist');
			$scope.invalid = false;
		}

	}

	$scope.validation = function () {
		$scope.modelValidation('div_mm', 'detailModel', 'MaterialMasterName', 'Material Master');
		if ($scope.hasArticle) $scope.modelValidation('div_ar', 'detailModel', 'ArticleName');
		$scope.manualValidationAddRemove('div_qty', 'detailModel', 'TransactionQty');
		$scope.modelValidation('div_qty', 'detailModel', 'TransactionUoMId', 'UoM is required');
		if ($scope.detailModel.TransactionAmount === 0)
			throw manualValidation('div_tamnt', true, 'Total amount is required.');
		$scope.manualValidationAddRemove('div_tamnt', 'detailModel', 'TransactionAmount');
		if ($scope.detailModel.IsOriginApplicable)
			$scope.manualValidationAddRemove('div_country', 'detailModel', 'CountryId');

		var isSku = false;
		if ($scope.hasSku) {
			if (!baseService.isUndefinedOrNull($scope.char1.CharacteristicsId)) {
				isSku = $scope.IsMandatoryButNull($scope.char1.IsMandatory, $scope.char1.FreeText);
			}
			else if (!baseService.isUndefinedOrNull($scope.char2.CharacteristicsId)) {
				isSku = $scope.IsMandatoryButNull($scope.char2.IsMandatory, $scope.char2.FreeText);
			}
			else if (!baseService.isUndefinedOrNull($scope.char3.CharacteristicsId)) {
				isSku = $scope.IsMandatoryButNull($scope.char3.IsMandatory, $scope.char3.FreeText);
			}
			if (isSku) throw ShowResult('Please insert SKU.', 'failure', 'detailPopUp');
		}
	};
	$scope.manualValidationAddRemove = function (divId, modelName, fieldName, message) {
		var msg = fieldName + ' is required.';
		msg = baseService.isUndefinedOrNull(message) ? msg : message;
		var str = fieldName;
		if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
			throw manualValidation(divId, true, msg);
		else if (isNaN($scope[modelName][str.replace(/\s/g, '')]))
			throw manualValidation(divId, true, msg);
		else
			return manualValidation(divId, false);
	};
	//manualDateValidation
	$scope.modelValidation = function (divId, modelName, fieldName, message) {
		var msg = fieldName + ' is required.';
		msg = baseService.isUndefinedOrNull(message) ? msg : message;
		var str = fieldName;
		if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
			throw manualValidation(divId, true, msg);
		else
			return manualValidation(divId, false);
	};
	$scope.GetSalesTaxData = function (salesId) {
		$scope.TaxList = [];
		$http({
			method: "GET",
			url: $scope.path + 'GetReceiveTaxList?receiveDetailId=' + $scope.masterId
		}).then(function (response) {
			$scope.TaxList = response.data;

			for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
				var linepk = $scope.inventoryMaterialList[i].InventoryReceiveDetailId;
				var list = gettaxlist(linepk);
				$scope.inventoryMaterialList[i].TaxList = list;
			}
		});
	};
	function gettaxlist(linepk) {
		var result = [];
		for (var i = 0; i < $scope.TaxList.length; i++) {
			if ($scope.TaxList[i].PODetailId === linepk) {
				result.push($scope.TaxList[i]);
			}
		}
		return result;
	}
	$scope.sumORnot = false;
	// Material Load

	function checkSameValueInColumnList(list, fieldName) {
		for (var i = 0; i < baseService.arrayLength(list); i++) {
			if (list[i][fieldName] === (i > 0 ? list[i - 1][fieldName] : list[i][fieldName]))
				$scope.sumORnot = true;
			else return $scope.sumORnot = false;
		}
	}
	function getTaxCategoryList(hsnCodeId) {
		$scope.taxCategoryList = [];
		$http({
			method: 'GET'
			, url: $scope.path + 'GetTaxCategoryList?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId
		}).then(function (response) {
			$scope.taxCategoryList = response.data;
		});
	}
	$scope.calculateTaxCategory = function () {
		$scope.detailModel.TotalTaxAmount = 0;
		var tQty = baseService.isUndefinedOrNull($scope.detailModel.TransactionQty) ? 0 : parseFloat($scope.detailModel.TransactionQty);
		var tAmount = baseService.isUndefinedOrNull($scope.detailModel.TransactionAmount) ? 0 : parseFloat($scope.detailModel.TransactionAmount);
		if (tQty > 0 && tAmount > 0)
			$scope.detailModel.TransactionRate = tAmount / tQty;
		else
			$scope.detailModel.TransactionRate = 0;
		for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
			$scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.detailModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
			$scope.detailModel.TotalTaxAmount = (parseFloat($scope.detailModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
		}
		if (isNaN($scope.detailModel.TotalTaxAmount)) $scope.detailModel.TotalTaxAmount = 0;
	};
	$scope.calculateTaxCategoryRate = function () {
		//debugger;
		$scope.detailModel.TotalTaxAmount = 0;
		var tQty = baseService.isUndefinedOrNull($scope.detailModel.TransactionQty) ? 0 : parseFloat($scope.detailModel.TransactionQty);
		var tAmount = baseService.isUndefinedOrNull($scope.detailModel.EstimatedRate) ? 0 : parseFloat($scope.detailModel.EstimatedRate);
		if (tQty > 0)
			//$scope.detailModel.TransactionRate = tAmount / tQty;
			$scope.detailModel.TotalAmount = tAmount * tQty;
		else
			//$scope.detailModel.TransactionRate = 0;
			$scope.detailModel.TotalAmount = 0;
		//for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
		//    $scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.detailModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
		//    $scope.detailModel.TotalTaxAmount = (parseFloat($scope.detailModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
		//}
		//if (isNaN($scope.detailModel.TotalTaxAmount)) $scope.detailModel.TotalTaxAmount = 0;
	};
	$scope.sumTaxAmount = function () {
		$scope.detailModel.TotalTaxAmount = 0;
		for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
			$scope.detailModel.TotalTaxAmount = (parseFloat($scope.detailModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
		}
	};
	$scope.getReceiveTaxList = function (data, flag, index, Id) {
		$scope.LoadTaxButtonClick();

		//debugger;
		$scope.Currency = $("#currency option:selected").text();
		$scope.currentMaterialRow = index;
		$scope.currentInventoryReceiveDetailIdRow = Id;
		$scope.taxAbleAmnt = data.TrnAmount;
		$scope.percentageColumn = flag;

		$scope.currentMaterialRow = index;
		//$scope.taxAbleAmnt = data.TransactionAmount;
		//$scope.taxAmnt = data.TaxAmount;
		$scope.receiveTaxList = [];
		if (data.TaxList.length > 0) {
			$scope.HSNCode = data.TaxList[0].HSNCode;
			$scope.receiveTaxList = data.TaxList;
		}
		$scope.total = 0;
		for (var j = 0; j < $scope.receiveTaxList.length; j++) {
			$scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
		}
		//$http({
		//    method: 'GET',
		//    url: $scope.path + 'GetReceiveTaxList?receiveDetailId=' + data.InventoryReceiveDetailId
		//}).then(function (response) {
		// $scope.receiveTaxList = response.data;
		//$scope.HSNCode = $scope.receiveTaxList[0]['HSNCode'];
		angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
		//});
		// $Scope.TAction = "OK";
	};
	$scope.getTotalReceiveTaxList = function (amount, flag) {
		$scope.taxAbleAmnt = amount;
		$scope.percentageColumn = flag;
		$http({
			method: 'GET',
			url: $scope.path + 'GetTotalReceiveTaxList?receiveId=' + $scope.productNew.Id
		}).then(function (response) {
			$scope.receiveTaxList = response.data;
			angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
		});
	};
	$scope.closeReceiveTaxPopUp = function () { //hossain
		//debugger;
		$scope.detailModel = {};
		//$scope.receiveTaxList = [];
		////debugger;



		$scope.detailModel.InventoryReceiveDetailId = $scope.currentInventoryReceiveDetailIdRow;
		$scope.detailModel.InventoryReceiveId = $scope.productNew.Id;
		for (var i = 0; i < $scope.receiveTaxList.length; i++) {
			var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": $scope.receiveTaxList[i].TaxCategoryId });
			if (getRow.length == 2) {
				ShowResult("You can't add Same Tax two times", 'failure', 'receiveTaxPopUp');
				return false;
			}

			if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxCategoryId)) {
				ShowResult("Select Tax Category.", 'failure', 'receiveTaxPopUp');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].Percentage)) {
				ShowResult("Input Percentage.", 'failure', 'receiveTaxPopUp');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxAmount)) {
				ShowResult("Input Tax Amount.", 'failure', 'receiveTaxPopUp');
				return false;
			}
			//if ($scope.receiveTaxList[i].TaxAmount == "0.00") {
			//    ShowResult("Tax Amount can't 0.", 'failure', 'receiveTaxPopUp');
			//    return false;
			//}
			//if ($scope.receiveTaxList[i].TaxAmount == "0") {
			//    ShowResult("Tax Amount can't 0.", 'failure', 'receiveTaxPopUp');
			//    return false;
			//}

		}

		//if ($scope.TAction === "OK") {
		$http({
			method: 'POST',
			//url: $scope.saveUrl,
			url: 'Products/Requisition/InsertExtraTax',
			//data: $scope.receiveTaxList,
			data: {
				entity: $scope.detailModel
				, taxCategoryList: $scope.receiveTaxList
			},
			dataType: 'JSON'
		}).then(function (response) {
			if (response.data.Error === true) {
				ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
			}
			else {
				ShowResult(response.data.Message, 'success', 'receiveTaxPopUp');
				//$scope.productNew.Id = response.data.entity.Id;
				// $scope.productNew.PartyName = $scope.product.PartyName;
				// $scope.Action = "Update";
				//$scope.getDataList();
				getInventoryMaterialList($scope.productNew.Id);
			}
		}), function (response) {
			ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
		};
		// }

		//angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');

	}
	$scope.closeServiceChargeTaxPopUp = function () { //hossain
		////debugger;



		$scope.detailModel = {};
		$scope.detailModel.InventoryReceiveDetailId = $scope.ServiceId;
		$scope.detailModel.InventoryReceiveDetailId = $scope.DetailId;
		$scope.detailModel.InventoryReceiveId = $scope.productNew.Id;
		for (var i = 0; i < $scope.receiveTaxList.length; i++) {
			var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": $scope.receiveTaxList[i].TaxCategoryId });
			if (getRow.length == 2) {
				ShowResult("You can't add Same Tax two times", 'failure', 'ServiceChargeTaxPopUp');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxCategoryId)) {
				ShowResult("Select Tax Category.", 'failure', 'ServiceChargeTaxPopUp');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].Percentage)) {
				ShowResult("Input Percentage.", 'failure', 'ServiceChargeTaxPopUp');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxAmount)) {
				ShowResult("Input Tax Amount.", 'failure', 'ServiceChargeTaxPopUp');
				return false;
			}
			//if ($scope.receiveTaxList[i].TaxAmount == "0.00") {
			//    ShowResult("Tax Amount can't 0.", 'failure', 'ServiceChargeTaxPopUp');
			//    return false;
			//}
			//if ($scope.receiveTaxList[i].TaxAmount == "0.0") {
			//    ShowResult("Tax Amount can't 0.", 'failure', 'ServiceChargeTaxPopUp');
			//    return false;
			//}
			//if ($scope.receiveTaxList[i].TaxAmount == "0") {
			//    ShowResult("Tax Amount can't 0.", 'failure', 'ServiceChargeTaxPopUp');
			//    return false;
			//}
		}

		//if ($scope.TAction === "OK") {
		$http({
			method: 'POST',
			//url: $scope.saveUrl,
			url: 'Products/Requisition/InsertserviceTax',
			//data: $scope.receiveTaxList,
			data: {
				entity: $scope.detailModel
				, taxCategoryList: $scope.receiveTaxList
				, ServiceId: $scope.ServiceId
			},
			dataType: 'JSON'
		}).then(function (response) {
			if (response.data.Error === true) {
				ShowResult(response.data.Message, 'failure', 'ServiceChargeTaxPopUp');
			}
			else {
				ShowResult(response.data.Message, 'success', 'ServiceChargeTaxPopUp');
				//$scope.productNew.Id = response.data.entity.Id;
				// $scope.productNew.PartyName = $scope.product.PartyName;
				// $scope.Action = "Update";
				//$scope.getDataList();
			}
		}), function (response) {
			ShowResult(response.data.Message, 'failure', 'ServiceChargeTaxPopUp');
		};
		// }

		//angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');

	}
	$scope.closeReceiveTaxPopUpwindow = function () {
		//debugger;
		getInventoryMaterialList($scope.productNew.Id);
		angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
	}
	$scope.closeServiceChargeTaxPopUpwindow = function () {
		getServiceChargeList($scope.productNew.Id);
		angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
	}
	function removeValidationMsg() {
		CloseModalShowResult();
		$scope.clearCharNames();
		manualValidation('div_mm', false);
		manualValidation('div_ar', false);
		manualValidation('div_qty', false);
		manualValidation('div_qty', false);
		manualValidation('div_rate', false);
	}
	function getGrossAmount(list, key1, key2, key3, fieldName) {
		$scope[fieldName] = 0;
		for (var t = 0; t < baseService.arrayLength(list); t++) {
			$scope[fieldName] += parseFloat(list[t][key1]);// + parseFloat(list[t][key2]) + parseFloat(list[t][key3]);
		}
	}
	// #endregion Details
	// #region Payment Term
	$http({
		method: 'GET',
		url: 'accounts/PaymentTerm/getvendorcbo'
	}).then(function successCallback(response) {
		$scope.paymentTermList = response.data;
	});


	$scope.GetTerms = function (id) {
		$http({
			method: 'GET',
			url: 'Products/GateentryToken/GetReqMaster?id=' + id
		}).then(function successCallback(response) {
			$scope.paymentTermList1 = response.data;
			$scope.productNew.Id = $scope.paymentTermList1[0].Id;
			$scope.productNew.CompanyGroupId = $scope.paymentTermList1[0].CompanyGroupId;
			$scope.productNew.EntryDate = $scope.paymentTermList1[0].EntryDate;
			$scope.productNew.PartyId = $scope.paymentTermList1[0].PartyId;
			$scope.productNew.UserName = $scope.paymentTermList1[0].UserName;
			$scope.productNew.Description = $scope.paymentTermList1[0].Description;
			$scope.productNew.PackageQty = $scope.paymentTermList1[0].PackageQty;
			$scope.productNew.ModeofTransport = $scope.paymentTermList1[0].ModeofTransport;
			$scope.productNew.Bill = $scope.paymentTermList1[0].Bill;
			$scope.productNew.PersonName = $scope.paymentTermList1[0].PersonName;
			$scope.productNew.MobileNo = $scope.paymentTermList1[0].MobileNo;
			$scope.productNew.Remarks = $scope.paymentTermList1[0].Remarks;
			$scope.productNew.GateEntryTime = $scope.paymentTermList1[0].GateEntryTime;

			$scope.productNew.EmployeeName = $scope.paymentTermList1[0].EmployeeName;
			$scope.productNew.ResponsiblePersonName = $scope.paymentTermList1[0].ResponsiblePersonName;
			$scope.productNew.GateEntryType = $scope.paymentTermList1[0].GateEntryType;
			$scope.productNew.PlantWiseGateId = $scope.paymentTermList1[0].PlantWiseGateId;
		});

	}

	$scope.changePaymentTerm = function () {

		if ($scope.productNew.GatePassStatus1 != 'Returnable') {
			$scope.productNew.ReturnableDate = '';
		}
		//if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
		//    var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
		//    $scope.productNew.PaymentTermCode = paymentTerm.PaymentTermCode;
		//    $scope.productNew.BaseNoOfDays = paymentTerm.NoOfDay;
		//    if (paymentTerm.BaseLineDate !== null)
		//        if (paymentTerm.BaseLineDate === 'documentdate') {
		//            $scope.productNew.BaseOnDueDate = $filter('dateFiltering')($scope.productNew.DocDate);
		//            $scope.IsBaseOnDueDateEnable = true;
		//        }
		//        else {
		//            $scope.productNew.BaseOnDueDate = null;
		//            $scope.IsBaseOnDueDateEnable = false;
		//        }
		//    $scope.getMatureDate($scope.productNew.BaseOnDueDate, $scope.productNew.BaseNoOfDays);
		//}
	};
	$scope.getMatureDate = function (date, days) {
		if (baseService.isUndefinedOrNull(date)) return $scope.productNew.MatureDate = null;
		date = new Date(date);
		date.setDate(date.getDate() + days);
		$scope.productNew.MatureDate = $filter('date')(date, 'dd-MMM-yyyy');
	};
	// #endregion Payment Term

	// #region Service
	$scope.serviceChargePopUp = function () {
		if (baseService.arrayLength($scope.inventoryMaterialList) === 0)
			return ShowResult('Without material charges not aplicable.');
		$scope.serviceModel = {
			Id: null
			, ServiceMasterId: null
			, InventoryReceiveId: $scope.productNew.Id
			, CurrencyName: angular.element("#currency :selected").text()
			, CurrencyId: $scope.productNew.CurrencyId
			, BaseCurrencyId: $scope.baseCurrencyId
			, DocDate: $scope.productNew.DocDate
			, TransactionAmount: null
			, BaseAmount: 0
			, TotalTaxAmount: 0
			, ToCurrencyRate: $scope.productNew.ToCurrencyRate
			, IsNonCreditable: $scope.productNew.IsNonCreditable
		};
		angular.element(document.querySelector('#serviceChargePopUp')).modal('show');
	};
	$http.get('Setups/CompanyServiceMaster/GetCboList')

		.then(function (response) {
			$scope.serviceList = response.data;
		});
	$scope.closeServiceChargePopUp = function () {
		$scope.serviceModel = {};
		$scope.receiveTaxList = [];
		angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
	};
	$scope.changeService = function () {
		//debugger;
		if (baseService.isUndefinedOrNull($scope.serviceModel.ServiceMasterId))
			return $scope.taxCategoryList = [];
		var hsnCodeId = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCodeId;
		getTaxCategoryList(hsnCodeId);
	};

	$scope.calculateSvcTaxCategory = function () {
		$scope.serviceModel.TotalTaxAmount = 0;
		for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
			$scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.serviceModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
			$scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
		}
		if (isNaN($scope.serviceModel.TotalTaxAmount)) $scope.serviceModel.TotalTaxAmount = 0;
	};
	$scope.sumSvcTaxAmount = function () {
		$scope.serviceModel.TotalTaxAmount = 0;
		for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
			$scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
		}
	};

	$scope.serviceSave = function () {
		try {
			$scope.manualValidationAddRemove('div_svc', 'serviceModel', 'ServiceMasterId');
			$scope.manualValidationAddRemove('div_svcRate', 'serviceModel', 'TransactionAmount', 'Amount');

			$http({
				method: 'POST',
				url: $scope.sreviceSaveUrl,
				data: {
					entity: $scope.serviceModel
					, taxCategoryList: $scope.taxCategoryList
				},
				dataType: 'JSON'
			}).then(function successCallback(response) {
				if (response.data.Error === true)
					ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
				else {
					ShowResult(response.data.Message, 'success', 'serviceChargePopUp');
					$scope.serviceModel = {
						Id: null
						, ServiceMasterId: null
						, InventoryReceiveId: $scope.productNew.Id
						, CurrencyName: angular.element("#currency :selected").text()
						, CurrencyId: $scope.productNew.CurrencyId
						, BaseCurrencyId: $scope.baseCurrencyId
						, DocDate: $scope.productNew.DocDate
						, TransactionAmount: null
						, BaseAmount: 0
						, TotalTaxAmount: 0
						, ToCurrencyRate: $scope.productNew.ToCurrencyRate
						, IsNonCreditable: $scope.productNew.IsNonCreditable
					};
					$scope.taxCategoryList = [];
					getServiceChargeList($scope.productNew.Id);
					getInventoryMaterialList($scope.productNew.Id);
					$scope.getDataList();
					$scope.getalldata();
				}
			}), function errorCallBack(response) {
				ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
			};
		} catch (e) {
			//ShowResult(e, 'fail', 'detailPopUp');
		}
	};

	$scope.delModal = function (id) {
		$scope.id = id;
		$scope.message = 'Are you sure want to permanently delete this?';
		angular.element(document.querySelector('#removePopUp')).modal('show');
	};
	$scope.serviceDelete = function () {
		try {
			$http({
				method: 'POST',
				url: $scope.sreviceDeleteUrl + $scope.id
			}).then(function successCallback(response) {
				if (response.data.Error === true)
					ShowResult(response.data.Message, 'failure');
				else {
					ShowResult(response.data.Message, 'success');
					$scope.id = null;
					getServiceChargeList($scope.productNew.Id);
					getInventoryMaterialList($scope.productNew.Id);
					$scope.getDataList();
				}
			}), function errorCallBack(response) {
				ShowResult(response.data.Message, 'failure');
			};
		} catch (e) {
			ShowResult(e, 'success');
		}
	};


	$scope.getServiceTaxList = function (data, flag, ServiceId, index) {

		//debugger;
		$scope.LoadTaxButtonClick();

		$scope.Currency = $("#currency option:selected").text();
		$scope.ServiceId = ServiceId;
		$scope.taxAbleAmnt = data.Amount;//+ data.TotalTaxAmount;
		$scope.percentageColumn = flag;

		$scope.currentMaterialRow = index;
		//$scope.taxAbleAmnt = data.TransactionAmount;
		//$scope.taxAmnt = data.TaxAmount;

		$scope.receiveTaxList = [];
		if (data.ChargeTaxList.length > 0) {
			$scope.HSNCode = data.ChargeTaxList[0].HSNCode;
			$scope.receiveTaxList = data.ChargeTaxList;
		}
		$scope.total = 0;
		for (var j = 0; j < $scope.receiveTaxList.length; j++) {
			$scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
		}
		angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('show');
		//$http({
		//    method: 'GET',
		//    url: $scope.path + 'GetServiceTaxList?serviceId=' + data.Id
		//}).then(function (response) {
		//    $scope.receiveTaxList = response.data;
		//    $scope.HSNCode = response.data[0]['HSNCode'];
		//    angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('show');
		//});
	}
	//Load2
	$scope.GetServiceTaxData = function (masterId) {
		////debugger;
		$scope.ChargeTaxList = [];
		$http({
			method: "GET",
			url: $scope.path + 'GetServiceTaxList?serviceId=' + $scope.productNew.Id
		}).then(function (response) {
			$scope.ChargeTaxList = response.data;

			for (var i = 0; i < $scope.chargesList.length; i++) {
				var linepk1 = $scope.chargesList[i].Id;
				var list1 = gettaxlist1(linepk1);
				$scope.chargesList[i].ChargeTaxList = list1;
			}
		});
	};
	function gettaxlist1(linepk1) {
		var result1 = [];
		//for (var i = 0; i < $scope.TaxList.length; i++) {
		//    if ($scope.TaxList[i].PODetailId === linepk) {
		//        result.push($scope.TaxList[i]);
		//    }
		//}

		for (var i = 0; i < $scope.ChargeTaxList.length; i++) {
			if ($scope.ChargeTaxList[i].InventoryServiceId === linepk1) {
				result1.push($scope.ChargeTaxList[i]);
			}
		}
		return result1;
	}

	function getServiceChargeList(inveReveiveId) {
		//debugger;
		$scope.chargesList = [];
		$http.get($scope.path + 'GetServiceChargeList?receiveId=' + inveReveiveId)
			.then(function (response) {
				$scope.chargesList = response.data;
				$scope.ServiceId = $scope.chargesList[0].Id;
				$scope.GetServiceTaxData();
			});

	}

	$scope.serviceChargePopUpEdit = function (Id, Amount, TotalTaxAmount) {
		if (baseService.arrayLength($scope.inventoryMaterialList) === 0)
			return ShowResult('Without material charges not aplicable.');
		//debugger;
		//if ($scope.MSAction == "Edit") {

		//    $http({
		//        method: 'GET',
		//        url: $scope.path + 'GetServiceTaxList?serviceId=' + Id
		//    }).then(function (response) {
		//        $scope.receiveTaxList = response.data;
		//        //$scope.HSNCode = response.data[0]['HSNCode'];
		//        //angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('show');
		//    });
		//    $scope.enable = false;
		//    $scope.MSAction = "Update";

		//}
		// else if ($scope.MSAction == "Update") {


		for (var i = 0; i < $scope.chargesList.length; i++) {
			for (var t = 0; t < $scope.chargesList[i].ChargeTaxList.length; t++) {
				$scope.receiveTaxList.push($scope.chargesList[i].ChargeTaxList[t]);
			}

		}
		$scope.productNew.Id
		$http({
			method: 'POST',
			url: 'Products/Requisition/UpdateServiceAndTax',
			data: {
				entity: $scope.chargesList,
				// ChargeTaxList: $scope.ChargeTaxList
				//TotalTaxAmount: TotalTaxAmount,
				//InventoryReceiveDetailId: InventoryReceiveDetailId,
				receiveTaxList: $scope.receiveTaxList
			},
			dataType: 'JSON'
		}).then(function (response) {
			if (response.data.Error === true) {
				ShowResult(response.data.Message, 'failure');
			}
			else {
				ShowResult(response.data.Message, 'success');
				//$scope.productNew.Id = response.data.entity.Id;
				//$scope.productNew.PartyName = $scope.product.PartyName;
				//$scope.Action = "Update";
				//getInventoryMaterialList($scope.detailModel.Id);

			}
		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};
		$scope.enable = true;
		$scope.MSAction = "Edit";

		//}
		//else {

		//}

		$scope.serviceModel = {
			Id: null
			, ServiceMasterId: null
			, InventoryReceiveId: $scope.productNew.Id
			, CurrencyName: angular.element("#currency :selected").text()
			, CurrencyId: $scope.productNew.CurrencyId
			, BaseCurrencyId: $scope.baseCurrencyId
			, DocDate: $scope.productNew.DocDate
			, TransactionAmount: null
			, BaseAmount: 0
			, TotalTaxAmount: 0
			, ToCurrencyRate: $scope.productNew.ToCurrencyRate
			, IsNonCreditable: $scope.productNew.IsNonCreditable
		};


		//angular.element(document.querySelector('#serviceChargePopUpEdit')).modal('show');
	};
	// #endregion Service

	$scope.inventoryReceiveReport = function (id, reportFormat) {
		if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
		$window.open('Products/InventoryReceive/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + id + '&plantId=' + $scope.productNew.PlantId, '_blank');
	};
	$scope.Griddata = [];
	$scope.getalldata = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/Requisition/GetListForHold',
		}).then(function successCallback(response) {
			$scope.Griddata = response.data;
			//entrydata = copy(searchdata);
		});
	};

	$scope.Griddata = [];
	$scope.getApprovaldata = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/Requisition/GetListForPOApproval',
		}).then(function successCallback(response) {
			$scope.Griddata = response.data;
			//entrydata = copy(searchdata);
		});
	};
	$scope.getApprovaldata();

	$scope.GriddataAUth = [];
	$scope.getApprovaldataAUth = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/Requisition/GetListForPOApprovalAuthorized',
		}).then(function successCallback(response) {
			$scope.GriddataAUth = response.data;
			//entrydata = copy(searchdata);
		});
	};
	// $scope.getApprovaldataAUth();

	$scope.GriddataAUth1 = [];
	$scope.getApprovaldataAUth1 = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/Requisition/GetListForPOApproval1Auth',
		}).then(function successCallback(response) {
			$scope.GriddataAUth1 = response.data;
			//entrydata = copy(searchdata);
		});
	};
	$scope.getApprovaldataAUth1();








	$scope.GriddataVendor = [];
	$scope.getalldataVendor = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/Requisition/GetListByParty',
		}).then(function successCallback(response) {
			$scope.GriddataVendor = response.data;
			//entrydata = copy(searchdata);
		});
	};
	function getPartyPlantList() {
		//debugger;

		//var aa = $scope.Id;
		$scope.plantList = [];
		$http.get('Products/Requisition/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId + '&Id=' + $scope.Id).then(function (response) {
			angular.forEach(response.data, function (item) {
				$scope.plantList.push(item);
				if (item.IsDefault) {
					$scope.productNew.InvoicingPartyPlantId = item.Value;
					$scope.productNew.DeliveryPartyPlantId = item.Value;
					$scope.productNew.InvoicingByAddress = item.Address1;
					$scope.productNew.DeliveryByAddress = item.Address2;
					$scope.productNew.InvoicingState = item.StateName;
					$scope.productNew.InvoicingGSTIN = item.GSTIN;
					$scope.productNew.DeliveryState = item.StateName;
					$scope.productNew.DeliveryGSTIN = item.GSTIN;
				}
			});
		});

	}

	//function getPartyPlantEditList() {
	//    //debugger;

	//    //var aa = $scope.Id;
	//    //$scope.plantList = [];
	//    $http.get('Products/Requisition/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId + '&Id=' + $scope.Id).then(function (response) {
	//        $scope.plantList = response.data;
	//    });

	//} 
	function getPartyPlantEditList(invoicingPartyPlantId, invoAddress, deliveryplant, deliAddress, deliState, deliGSTIN) {
		$scope.plantList = [];
		$http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId).then(function (response) {
			angular.forEach(response.data, function (item) {
				$scope.plantList.push(item);
				if (item.Value == invoicingPartyPlantId) {
					//$scope.partyPlantId = item.Value;
					$scope.productNew.InvoicingPartyPlantId = item.Value;
					$scope.productNew.DeliveryPartyPlantId = deliveryplant;
					$scope.productNew.InvoicingByAddress = invoAddress;
					$scope.productNew.DeliveryByAddress = deliAddress;
					$scope.productNew.InvoicingState = item.StateName;
					$scope.productNew.InvoicingGSTIN = item.GSTIN;
					$scope.productNew.DeliveryState = deliState;
					$scope.productNew.DeliveryGSTIN = deliGSTIN;

				}
			});

		});
	}

	$scope.getalldataVendor();
	$scope.getalldata();

	//$scope.Vendorrecorddoubleclick = function ($event) {

	//    var x = $event;
	//    $scope.Id = x.data.Id;
	//   // alert('x' + Id);
	//    $scope.closePartyPopUp(x.data);

	//}


	//$scope.deleteRow = function (i) {
	//    alert('f');
	//    $scope.employees.splice(i, 1);
	//};
	//$scope.enable = true;  
	//$scope.MSAction = "Edit"
	$scope.closeServiceChargePopUpEdit = function () {
		$scope.serviceModel = {};
		$scope.receiveTaxList = [];
		angular.element(document.querySelector('#serviceChargePopUpEdit')).modal('hide');
	};
	$scope.dindex = -1;
	$scope.DelCharge = function (Id, index) {
		$scope.dindex = index;
		for (var i = 0; i < $scope.receiveTaxList.length; i++) {
			if ($scope.receiveTaxList[i].Id === Id) {
				$scope.receiveTaxList.splice($scope.dindex, 1);
				return true;
				break;
			}
		}
		$scope.dindex = -1;
		//$('#AddTaxCharge tr').click(function () {
		//    //alert('sk' + Id);
		//    ////debugger;
		//    if (Id == null) {
		//        $(this).remove();
		//        return false;
		//    }
		//    else {
		//        $scope.message = 'Are you sure want to permanently delete this?';
		//        angular.element(document.querySelector('#removerPopUp')).modal('show');
		//        $http({
		//            method: 'POST',
		//            url: 'Products/Requisition/DeleteMaterialTax?Id=' + Id,
		//            dataType: 'JSON'
		//        }).then(function (response) {
		//            if (response.data.Error === true)
		//                ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
		//            else {
		//                ShowResult(response.data.Message, 'success', 'receiveTaxPopUp');
		//                //$scope.getDataList();
		//                //ClearFields();
		//                $(this).remove();
		//                return false;
		//            }
		//            function errorCallBack(response) {
		//                ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
		//            }
		//        });

		//    }

		//});
	};
	$scope.Del = function (Id, index) {
		$scope.dindex = index;
		for (var i = 0; i < $scope.receiveTaxList.length; i++) {
			if ($scope.receiveTaxList[i].Id === Id) {
				$scope.receiveTaxList.splice($scope.dindex, 1);
				return true;
				break;
			}
		}
		$scope.dindex = -1;


		//$('#AddTax tr').click(function () {
		// alert('sk' + Id);
		////debugger;
		//if (Id == null) {
		//    $(this).remove();
		//    $scope.receiveTaxList.splice(index);
		//    return false;
		//}
		//else {
		//             $(this).remove();
		//            $scope.receiveTaxList.splice(index);
		//            return false;
		//$scope.message = 'Are you sure want to permanently delete this?';
		//angular.element(document.querySelector('#removerPopUp')).modal('show');
		//$http({
		//    method: 'POST',
		//    url: 'Products/Requisition/DeleteMaterialTax?Id=' + Id, 
		//    dataType: 'JSON'
		//}).then(function (response) {
		//    if (response.data.Error === true)
		//        ShowResult(response.data.Message, 'failure','receiveTaxPopUp');
		//    else {
		//        ShowResult(response.data.Message, 'success','receiveTaxPopUp');
		//        //$scope.getDataList();
		//        //ClearFields();
		//        $(this).remove();
		//        $scope.receiveTaxList.splice(index);
		//        return false;
		//    }
		//    function errorCallBack(response) {
		//        ShowResult(response.data.Message, 'failure','receiveTaxPopUp');
		//    }
		//});

		//  }

		//});
	};
	$scope.calculateAmount = function (data) {
		//debugger;
		data.TotalAmount = (data.TransactionQty * data.EstimatedRate).toFixed(2);
		if (data.TotalAmount === 'NaN')
			data.TotalAmount = 0;
		//data.TaxAmount = 0;
		//angular.forEach(data.TaxList, function (item) {
		//    item.TaxAmount = data.TrnAmount * item.Percentage / 100;
		//    data.BaseTaxAmount += item.TaxAmount;
		//});
		// data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
		//data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
		//if ($scope.productNew.IsNonCreditable == 1) {
		//    //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
		//    if (data.BaseTaxAmount === null) {
		//        data.BaseTaxAmount = '0.00';
		//    }
		//    data.BaseAmount = parseFloat(data.TrnAmount + data.BaseTaxAmount);
		//}
		//else {
		//    // data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
		//    data.BaseAmount = data.TrnAmount;
		//}
	};
	$scope.calculateRate = function (data, event) {
		//debugger;
		data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
		if (data.TransactionRate === 'NaN')
			data.TransactionRate = 0;
		data.BaseTaxAmount = 0;
		angular.forEach(data.TaxList, function (item) {
			item.TaxAmount = data.TrnAmount * item.Percentage / 100;

			data.BaseTaxAmount += item.TaxAmount;
		});
		// data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
		if ($scope.productNew.IsNonCreditable == 1) {
			//data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
			data.BaseAmount = data.TrnAmount + data.BaseTaxAmount;
		}
		else {
			// data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
			data.BaseAmount = data.TrnAmount;
		}

	};
	$scope.calculateAmountForServiceCharge = function (data) {
		//debugger;
		//data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
		//if (data.TrnAmount == 'NaN')
		//    data.TrnAmount = 0;
		//data.TaxAmount = 0;
		data.TotalTaxAmount = 0;
		for (var i = 0; i < $scope.ChargeTaxList.length; i++) {
			if ($scope.ChargeTaxList[i].InventoryServiceId === data.Id) {
				$scope.ChargeTaxList[i].TaxAmount = data.Amount * $scope.ChargeTaxList[i].Percentage / 100;
				data.TotalTaxAmount += $scope.ChargeTaxList[i].TaxAmount;
			}
		}
		// data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
		//data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
	};
	$scope.onchangeFunction = function (id) {
		$scope.TaxCategoryId = id;
		//debugger;
		var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": id });
		if (getRow.length === 2) {
			ShowResult("You can't add Same Tax two times", 'failure', 'ServiceChargeTaxPopUp');

		}

	}
	$scope.onchangeFunction1 = function (id) {
		$scope.TaxCategoryId = id;
		//debugger;
		var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": id });
		if (getRow.length === 2) {
			ShowResult("You can't add Same Tax two times", 'failure', 'receiveTaxPopUp');

		}

	};
	$scope.onClick = function (args) {

		var gridObj = $("#Grid").data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		//alert('jj' + data.Id);
		// $scope.valuePassInDelModal(data); 
		location.href = "Products/Requisition/GePurchaseOrderReport?purchaseOrderId=" + data.Id;

	};



	$scope.GeteEntryRep = function (args) {

		var gridObj = $("#Grid").data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		//alert('jj' + data.Id);
		// $scope.valuePassInDelModal(data); 
		location.href = "Products/GateentryToken/GateEntryReport?GateEntryId=" + data.Id;

	};

	$scope.command = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",

			click: $scope.GeteEntryRep
		}
	}];
	//#region Print for po Approval

	$scope.onClickpoApprovalprint = function (args) {

		var gridObj = $("#GridPO1").data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		//alert('jj' + data.Id);
		// $scope.valuePassInDelModal(data); 
		location.href = "Products/Requisition/GePurchaseOrderReport?purchaseOrderId=" + data.Id;

	};
	$scope.commandprint = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",

			click: $scope.onClickpoApprovalprint
		}
	}];

	//#endregion

	//Compare with Todays Date
	//$scope.checkDocDate = function () {
	//    var msg = "";
	//    if (new Date($scope.voucher.InvoiceDate) > new Date()) {
	//        $scope.invalidDocDate = true;
	//        msg = "Doc date must be below or equal to current Date!";
	//    }
	//    else if (baseService.isUndefinedOrNull($scope.voucher.InvoiceDate)) {
	//        msg = "Doc Date is required.";
	//        $scope.invalidDocDate = true;
	//    }
	//    else {
	//        $scope.invalidDocDate = false;
	//    }
	//    return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
	//};
	$scope.invalidDocDate = false;
	$scope.checkDocDate = function () {
		var msg = "";

		if (new Date($scope.productNew.DocDate) > new Date($scope.productNew.PODate)) {
			msg = "Doc date must be grater or equal to Vendor Doc. RefNo!";
			$scope.invalidDocDate = true;
		}
		//else if (new Date($scope.voucher.DocDate) > new Date()) {
		//    $scope.invalidDocDate = true;
		//    msg = "Doc date must be below or equal to current Date!";
		//}
		else $scope.invalidDocDate = false;
		return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
	};
	//#region Shahazahan Code for PO Approval
	$scope.Griddata1 = [];
	$scope.onClickPO = function (args) {
		//debugger;
		var gridObj = $("#Grid").data("ejGrid");
		//getting corresponding record 
		$scope.data = gridObj.getSelectedRecords()[0];
		//alert('POClose' + data.Id);
		$scope.approveAlert();

	};
	//cboService.getEnumCbo("enum/GetExpensesBookingApprovalStatusCbo", function (result) {
	//    $scope.approvalStatusList = result;
	//});
	$scope.getalldata1 = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/Requisition/GetListForPOApproval',
		}).then(function successCallback(response) {
			$scope.Griddata1 = response.data;
			//entrydata = copy(searchdata);
		});
	};
	$scope.Status = null;
	$scope.getalldata1();
	$scope.poApp = function () {
		var str = $('#combo-default1').val();
		var Id = str.substring(0, str.indexOf('-'));
		//var d1 = $('#combo-default1 option:selected').text();

		//debugger;
		$http({
			method: 'POST',
			url: 'Products/Requisition/PoApproved',
			data: {
				'PoId': $scope.podata.Id,
				'PoValue': $scope.podata.TotalQty,
				'CheckedStataus': $('#combo-default').val(),
				'AuthorizedBy': Id

			},

			dataType: 'JSON'
		}).then(function successCallback(response) {
			if (response.data.Error === true) {
				ShowResult(response.data.Message, 'failure');
			}
			else {
				ShowResult(response.data.Message, 'success');
				$scope.getalldata1();
			}
		}, function errorCallBack(response) {
			ShowResult(response.data.Message, 'failure');
		});
	}
	$scope.poAppAuth = function () {
		//var str = $('#combo-default').val();
		//var Id = str.substring(0, str.indexOf('-'));
		//var d1 = $('#combo-default1 option:selected').text();

		//debugger;
		$http({
			method: 'POST',
			url: 'Products/Requisition/PoApprovedAuth',
			data: {
				'PoId': $scope.podata.Id,
				'PoValue': $scope.podata.TotalQty,
				'CheckedStataus': $('#combo-default12').val()


			},

			dataType: 'JSON'
		}).then(function successCallback(response) {
			if (response.data.Error === true) {
				ShowResult(response.data.Message, 'failure');
			}
			else {
				ShowResult(response.data.Message, 'success');
				$scope.getApprovaldataAUth();
			}
		}, function errorCallBack(response) {
			ShowResult(response.data.Message, 'failure');
		});
	}
	$scope.poAppUnApproved = function () {

		//debugger;
		$http({
			method: 'POST',
			url: 'Products/Requisition/PoUnApproved',
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
				$scope.getalldata1();
			}
		}, function errorCallBack(response) {
			ShowResult(response.data.Message, 'failure');
		});
	}




	$scope.onClickPOA = function (args) {

		var gridObj = $("#GridPO").data("ejGrid");
		//getting corresponding record 
		$scope.podata = gridObj.getSelectedRecords()[0];

		//alert('Approve=' + data.Id);
		$scope.approvalAlert();
	};
	$scope.commandpo = [{
		type: "details", buttonOptions: {
			text: "Save",
			width: "100",
			height: "30",
			click: $scope.onClickPOA
		}
	}];
	$scope.onClickPOAUTH = function (args) {

		var gridObj = $("#GridPOAPp").data("ejGrid");
		//getting corresponding record 
		$scope.podata = gridObj.getSelectedRecords()[0];

		//alert('Approve=' + data.Id);
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
	$scope.approvalAlert = function () {
		$scope.message = 'Are you sure want to Approve?';
		angular.element(document.querySelector('#poapprovealert')).modal('show');
	};
	//#endregion
	//#region Towfik PO Closed
	$scope.GriddataPOClose = [];
	$scope.getalldataPOClose = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/Requisition/GetListForPOClose',
		}).then(function successCallback(response) { //datagatefun
			$scope.GriddataPOClose = response.data;
			//entrydata = copy(searchdata);
		});
	};
	$scope.getalldataPOClose();


	$scope.onClickPOlock = function (args) {
		//debugger;
		var gridObj = $("#Grid").data("ejGrid");
		//getting corresponding record 
		$scope.data = gridObj.getSelectedRecords()[0];
		//alert('POClose' + data.Id);
		$scope.approvalAlertlock();

	};
	$scope.approvalAlertlock = function () {
		$scope.message = 'Are you sure want to Approve?';
		angular.element(document.querySelector('#poapprovealertlock')).modal('show');
	};
	//$scope.onClickPOLock = function (args) {
	//    //debugger;
	//    var gridObj = $("#Grid").data("ejGrid");
	//    //getting corresponding record 
	//    $scope.data = gridObj.getSelectedRecords()[0];
	//    //alert('POClose' + data.Id);
	//    $scope.onClickPOlock();

	//};
	//$scope.approveAlertlock = function () {
	//    $scope.message = 'Are you sure want to Approve?';
	//    angular.element(document.querySelector('#poapprovealertlock')).modal('show');    //};


	$scope.commandPoClose = [{

		type: "details", buttonOptions: {
			text: "Po Unlock",
			width: "120",
			height: "20",


			click: $scope.onClickPOlock
		}
	}];
	$scope.Poclosed = function () {
		$http({
			method: 'POST',
			url: 'Products/Requisition/POClose',
			data: {
				'PoId': $scope.data.Id,
				'PoValue': $scope.data.TotalQty
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			if (response.data.Error === true) {
				ShowResult(response.data.Message, 'failure');
			}
			else {
				ShowResult(response.data.Message, 'success');
				$scope.getalldataPOClose();
			}
		}, function errorCallBack(response) {
			ShowResult(response.data.Message, 'failure');
		});

	}
	//#endRegion

	// # Taufik region setTab
	//$scope.tab = 1;
	//$scope.setTab = function (newTab) {
	//    $scope.tab = newTab;
	//};
	//$scope.isSet = function (tabNum) {
	//    return $scope.tab === tabNum;
	//};
	// #endregion

	// #region Taufik Un Approval po data post start
	$scope.Griddataapprovpo = [];
	$scope.Griddataapprovpo1 = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/Requisition/GetListForPOApproval1',
		}).then(function successCallback(response) {
			$scope.Griddataapprovpo = response.data;
			//entrydata = copy(searchdata);
		});
	};
	$scope.Griddataapprovpo1();



	$scope.ListForPOApproval1UnApproved = [];
	$scope.GetListForPOApproval1UnApproved = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/Requisition/GetListForPOApproval1UnApproved',
		}).then(function successCallback(response) {
			$scope.ListForPOApproval1UnApproved = response.data;
			//entrydata = copy(searchdata);
		});
	};
	$scope.GetListForPOApproval1UnApproved();


	$scope.onClickPOA1 = function (args) {

		var gridObj = $("#GridPO1").data("ejGrid");
		//getting corresponding record 
		$scope.podata1 = gridObj.getSelectedRecords()[0];
		//$scope.SystemId = $scope.InActive.SystemId;
		//angular.element(document.querySelector('#ActionPopUp')).modal('show');
		//alert('Approve=' + data.Id);
		$scope.approveAlert1();
	};

	$scope.commandpo1 = [{
		type: "details", buttonOptions: {
			text: "Un Approve",
			width: "100",
			height: "30",

			click: $scope.onClickPOA1
		}
	}];

	$scope.approveAlert1 = function () {
		$scope.message = 'Are you sure want to Approve?';
		angular.element(document.querySelector('#poapprovalalert1')).modal('show');
	};

	$scope.poApp1 = function () {
		$http({
			method: 'POST',
			url: 'Products/Requisition/PoApproved1',
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
	//#endregion

	//#region Towfik PO Unlock
	$scope.GriddataPOlock = [];
	$scope.getalldataPOUnlock = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/Requisition/GetListForPOUnClose',
		}).then(function successCallback(response) { //datagatefun
			$scope.GriddataPOlock = response.data;
			//entrydata = copy(searchdata);
		});
	};

	$scope.getalldataPOUnlock();

	$scope.onClickPOlock = function (args) {
		//debugger;
		var gridObj = $("#GridUc").data("ejGrid");
		//getting corresponding record 
		$scope.data = gridObj.getSelectedRecords()[0];
		//alert('POClose' + data.Id);
		$scope.approvalAlertUnlock();

	};
	$scope.approvalAlertUnlock = function () {
		$scope.message = 'Are you sure want to Approve?';

		angular.element(document.querySelector('#POPUnlock')).modal('show');
	};
	$scope.PoUnlock = function () {
		$http({
			method: 'POST',
			url: 'Products/Requisition/POUnClose',
			data: {
				'PoId': $scope.data.Id,
				'PoValue': $scope.data.TotalQty
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			if (response.data.Error === true) {
				ShowResult(response.data.Message, 'failure');
			}
			else {
				ShowResult(response.data.Message, 'success');
				$scope.getalldataPOUnlock();
			}
		}, function errorCallBack(response) {
			ShowResult(response.data.Message, 'failure');
		});

	}

	$scope.commandPoUnlock = [{

		type: "details", buttonOptions: {
			text: "Po lock",
			width: "120",
			height: "20",


			click: $scope.onClickPOlock
		}
	}];

	//#endRegion

	// # Taufik region setTab
	$scope.tab = 1;
	$scope.setTab = function (newTab) {
		$scope.tab = newTab;
	};
	$scope.isSet = function (tabNum) {
		return $scope.tab === tabNum;
	};
	// #endregion

	//#region Toufik PO List for Po closed ui 
	$scope.GriddataPOListforPoclosedui = [];
	$scope.getalldataPOListforPoclosedui = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/Requisition/GetListForAllPOList',
		}).then(function successCallback(response) { //datagatefun
			$scope.GriddataPOListforPoclosedui = response.data;
			//entrydata = copy(searchdata);
		});
	};

	$scope.getalldataPOListforPoclosedui();

	$scope.onClickPoList = function (args) {
		//debugger;
		var gridObj = $("#GridPOListforPoclosedui").data("ejGrid");
		//getting corresponding record 
		$scope.data = gridObj.getSelectedRecords()[0];
		//alert('POClose' + data.Id);
		$scope.approvalAlertPoList();

	};
	$scope.approvalAlertPoList = function () {
		$scope.message = 'Are you sure want to Approve?';

		angular.element(document.querySelector('#AllPoListmi')).modal('show');
	};
	$scope.PoListinClose = function () {
		$http({
			method: 'POST',
			url: 'Products/Requisition/POClose',
			data: {
				'PoId': $scope.data.Id,
				'PoValue': $scope.data.TotalQty
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			if (response.data.Error === true) {
				ShowResult(response.data.Message, 'failure');
			}
			else {
				ShowResult(response.data.Message, 'success');
				$scope.getalldataPOListforPoclosedui();
			}
		}, function errorCallBack(response) {
			ShowResult(response.data.Message, 'failure');
		});

	}

	$scope.commandAllPoList = [{

		type: "details", buttonOptions: {
			text: "Po lock",
			width: "120",
			height: "20",


			click: $scope.onClickPoList
		}
	}];

	// #region All Tab Control
	$scope.tab = 1;
	$scope.setTabpou = function (newTab) {
		$scope.tab = newTab;
		$scope.getalldata1();

	};
	$scope.isSetpou = function (tabNum) {
		return $scope.tab === tabNum;
	};
	$scope.tab = 1;
	$scope.setTabpou12 = function (newTab) {
		$scope.tab = newTab;
		$scope.getApprovaldataAUth();

	};
	$scope.isSetpou12 = function (tabNum) {
		return $scope.tab === tabNum;
	};
	$scope.setTabpou14 = function (newTab) {
		$scope.tab = newTab;
		$scope.GetListForPOApproval1UnApproved();

	};
	$scope.isSetpou14 = function (tabNum) {
		return $scope.tab === tabNum;
	};



	//$scope.tab = 1;
	$scope.setTabpoa = function (newTab) {
		$scope.tab = newTab;
		$scope.Griddataapprovpo1();
	};
	$scope.isSetpoa = function (tabNum) {
		return $scope.tab === tabNum;
	};


	$scope.setTabpoa12 = function (newTab) {
		$scope.tab = newTab;
		$scope.getApprovaldataAUth1();
	};
	$scope.isSetpoa12 = function (tabNum) {
		return $scope.tab === tabNum;
	};
	// End PO approve



	//$scope.tab = 2;
	$scope.setTab2 = function (newTab) {
		$scope.tab = newTab;

		$scope.getalldataPOClose();

	};
	$scope.isSet2 = function (tabNum) {
		return $scope.tab === tabNum;
	};


	$scope.setTab1 = function (newTab) {
		$scope.tab = newTab;

		$scope.getalldataPOUnlock();
	};
	$scope.isSet1 = function (tabNum) {
		return $scope.tab === tabNum;
	};


	$scope.setTab3 = function (newTab) {
		$scope.tab = newTab;
		$scope.getalldataPOListforPoclosedui();
	};
	$scope.isSet3 = function (tabNum) {
		return $scope.tab === tabNum;
	};

	// #endregion











	//#region FGForMasterOrder(Finishing Goods For Master Order) 22-Jun-2019

	$scope.MasterOrderList = function () {
		$scope.getalldataListForMasterOrder();
		angular.element(document.querySelector('#ListOfMasterOrder')).modal('show');
	};

	$scope.MasterOrderListHide = function () {
		angular.element(document.querySelector('#ListOfMasterOrder')).modal('hide');
	};

	$scope.GetListForMasterOrder = [];
	$scope.getalldataListForMasterOrder = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/Requisition/GetListForMasterOrder',
		}).then(function successCallback(response) { //datagatefun
			$scope.GetListForMasterOrder = response.data;
			//entrydata = copy(searchdata);
		});
	};


	$scope.Getrecorddoubleclick = function ($event, index) {
		//debugger;
		// alert('Do you want to see Material Details');
		var x = $event;
		var Id = x.data.Id;
		$scope.MONo = Id;
		getMasterItemList();
		angular.element(document.querySelector('#ListOfMasterOrder')).modal('hide');

	};

	function getMasterItemList() {
		//debugger;
		$scope.inventoryMaterialList = [];
		$http.get($scope.path + 'GetMasterItemList?masterOrderId=' + $scope.MONo)
			.then(function (response) {

				$scope.inventoryMaterialList = response.data;
				//$scope.DetailId = $scope.inventoryMaterialList[0].InventoryReceiveDetailId;
				//$scope.InvoicingPartyPlantId = $scope.inventoryMaterialList[0].InvoicingPartyPlantId;

				//$scope.productNew.InvoicingPartyPlantId = $scope.inventoryMaterialList[0].InvoicingPartyPlantId;
				//$scope.productNew.InvoicingStateId = $scope.inventoryMaterialList[0].InvoicingStateId;
				//$scope.productNew.PlantStateId = $scope.inventoryMaterialList[0].PlantStateId;
				//checkSameValueInColumnList($scope.inventoryMaterialList, 'TransactionUoM');
				//getGrossAmount($scope.inventoryMaterialList, 'BaseAmount', 'BaseTaxAmount', 'ChargesAmount', 'grossTotal');
				$scope.GetSalesTaxData();
			});
	}
	$scope.calculateAmountByRateFG = function (data) {
		//debugger;
		data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
		if (data.TrnAmount === 'NaN')
			data.TrnAmount = 0;
		data.TaxAmount = 0;
		angular.forEach(data.TaxList, function (item) {
			item.TaxAmount = data.TrnAmount * item.Percentage / 100;
			data.BaseTaxAmount += item.TaxAmount;
		});
		// data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
		data.BaseAmount = parseFloat($scope.productNew.ToCurrencyRate * data.TrnAmount).toFixed(2);
	};
	$scope.changeServiceForFG = function () {
		//debugger;

		$scope.serviceModel.CurrencyName = "INR";
		$scope.serviceModel.ToCurrencyRate = 1;
		if (baseService.isUndefinedOrNull($scope.serviceModel.ServiceMasterId))
			return $scope.taxCategoryList = [];
		var hsnCodeId = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCodeId;
		getTaxCategoryListForFGService(hsnCodeId);
	};
	function getTaxCategoryListForFGService(hsnCodeId) {
		$scope.taxCategoryList = [];
		$http({
			method: 'GET'
			, url: $scope.path + 'GetTaxCategoryListForFGService?partyPlantId=' + $scope.productNew.InvoicingPartyPlantId + '&hsnCodeId=' + hsnCodeId
			//url: $scope.path + 'GetTaxCategoryListForFGService?hsnCodeId=' + hsnCodeId 
		}).then(function (response) {
			$scope.taxCategoryList = response.data;
		});
	}

	$scope.ServiceListFGAdd = function () {

		//debugger;
		var TempList = [];
		TempList.Id = $scope.serviceModel.ServiceMasterId;

		TempList.ServiceMasterName = angular.element("#ServiceMasterId :selected").text();
		TempList.Amount = $scope.serviceModel.TransactionAmount;
		TempList.TotalTaxAmount = 0;
		TempList.TotalTaxAmount = $filter('sumByKey')($filter('filter')($scope.taxCategoryList), 'TaxAmount');

		$scope.chargesList.push(TempList);
		for (var i = 0; i < $scope.taxCategoryList.length; i++) {
			$scope.taxCategoryList[i].ServiceMasterId = $scope.serviceModel.ServiceMasterId;
			$scope.ChargeTaxList.push($scope.taxCategoryList[i]);
		}

		angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');

	}

	$scope.getServiceTaxFGList = function (data, flag, ServiceId, index) {

		//debugger;
		$scope.LoadTaxButtonClick();

		$scope.Currency = $("#currency option:selected").text();
		$scope.ServiceId = ServiceId;
		$scope.taxAbleAmnt = data.Amount;
		$scope.percentageColumn = flag;
		$scope.currentMaterialRow = index;

		$scope.receiveTaxList = [];
		if ($scope.ChargeTaxList.length > 0) {
			$scope.HSNCode = $scope.ChargeTaxList[0].HSNCode;
			$scope.receiveTaxList = $filter('filter')($scope.ChargeTaxList, { 'ServiceMasterId': ServiceId });

			//$scope.receiveTaxList = $scope.ChargeTaxList;
		}
		$scope.total = 0;
		for (var j = 0; j < $scope.receiveTaxList.length; j++) {
			$scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
		}
		angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('show');

	}

	$scope.AddReceiveTaxPopUpFG = function (Id, index) { //hossain
		//debugger;
		$scope.detailModel = {};
		//$scope.receiveTaxList = [];
		//$scope.receiveTaxList1 = [];
		var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.receiveTaxList), 'TaxAmount');
		for (var j = 0; j < $scope.inventoryMaterialList.length; j++) {

			if ($scope.inventoryMaterialList[j].Id === $scope.PODetailid) {
				$scope.inventoryMaterialList[j].BaseTaxAmount = TotalServiceTaxAmount;
			}


		}


		$scope.detailModel.InventoryReceiveDetailId = $scope.currentInventoryReceiveDetailIdRow;
		$scope.detailModel.InventoryReceiveId = $scope.productNew.Id;
		for (var i = 0; i < $scope.receiveTaxList.length; i++) {
			var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": $scope.receiveTaxList[i].TaxCategoryId });
			if (getRow.length == 2) {
				ShowResult("You can't add Same Tax two times", 'failure', 'receiveTaxPopUp');
				return false;
			}

			if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxCategoryId)) {
				ShowResult("Select Tax Category.", 'failure', 'receiveTaxPopUp');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].Percentage)) {
				ShowResult("Input Percentage.", 'failure', 'receiveTaxPopUp');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxAmount)) {
				ShowResult("Input Tax Amount.", 'failure', 'receiveTaxPopUp');
				return false;
			}
			$scope.TaxList.push($scope.receiveTaxList);


		}
		angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');

	}

	$scope.closeReceiveTaxPopUpFG = function () { //hossain        
		angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');

	}

	$scope.getReceiveTaxListFG = function (data, flag, index, Id) {
		//debugger;
		$scope.PODetailid = data.Id;

		$scope.LoadTaxButtonClick();

		$scope.Currency = $("#currency option:selected").text();
		$scope.currentMaterialRow = index;
		$scope.currentInventoryReceiveDetailIdRow = Id;
		$scope.taxAbleAmnt = data.TrnAmount;
		$scope.percentageColumn = flag;

		$scope.currentMaterialRow = index;
		//$scope.taxAbleAmnt = data.TransactionAmount;
		//$scope.taxAmnt = data.TaxAmount;
		//$scope.receiveTaxList = [];
		if (data.TaxList.length > 0) {
			$scope.HSNCode = data.TaxList[0].HSNCode;
			$scope.receiveTaxList = data.TaxList;
		}
		$scope.total = 0;
		for (var j = 0; j < $scope.receiveTaxList.length; j++) {
			$scope.receiveTaxList[j].Id = $scope.PODetailid;
			$scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;

		}

		angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');

	}
	$scope.addTaxFG = function () {
		var data = {
			TotalAmount: 0,
			Id: $scope.PODetailid,
			HSNCode: $scope.HSNCode,
			HSNCodeId: null,
			UserName: null,
			TaxCategoryId: null
		};
		$scope.receiveTaxList.push(data);

	};
	$scope.sumSvcTaxAmountFG = function () {
		$scope.serviceModel.TotalTaxAmount = 0;
		for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
			$scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
		}
	};

	//$scope.SaveFG = function () {
	//    ////debugger;
	//    try {
	//        $scope.dbval = $scope.StateData;
	//        $scope.UIval = $scope.productNew.InvoicingState;

	//        if ($scope.inventoryMaterialList.length === 0) {
	//            angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
	//        }
	//        else if ($scope.dbval.length === 0) {
	//            angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
	//        }
	//        else if ($scope.dbval === $scope.UIval) {
	//            angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
	//        }
	//        else {
	//            ShowResult('You can not change Invoicing party.Line is available', 'failure', 'invoicingPartyPopUp');

	//        }

	//        if (baseService.isUndefinedOrNull($scope.productNew.InvoicingPartyPlantId)) return ShowResult('Invoicing by is required', 'failure');
	//        if (baseService.isUndefinedOrNull($scope.productNew.DeliveryPartyPlantId)) return ShowResult('Delivery by is required', 'failure');
	//        $scope.modelValidation('div_docNo', 'productNew', 'DocRefNo');
	//        $scope.modelValidation('div_docDate', 'productNew', 'DocDate');
	//        //$scope.modelValidation('div_entryNo', 'productNew', 'GateEntryNo');
	//        $scope.modelValidation('div_PODate', 'productNew', 'PODate', 'PO Entry Date');
	//        //if ($scope.Action === 'Update')
	//        //    $scope.modelValidation('div_grnNo', 'productNew', 'Id');
	//        //$scope.modelValidation('div_grnDate', 'productNew', 'GRNDate');

	//        $scope.manualValidationAddRemove('div_currency', 'productNew', 'CurrencyId');

	//        if ($scope.productNew.CurrencyId !== $scope.productNew.BaseCurrencyId)
	//            $scope.manualValidationAddRemove('div_rate  ', 'productNew', 'ToCurrencyRate');
	//        else
	//            manualValidation('div_rate', false);

	//        $scope.$broadcast('show-errors-check-validity');
	//        if ($scope.productNewForm.$valid) {
	//            //if (new Date($scope.productNew.EntryDate) < new Date($scope.productNew.DocDate))
	//            //    return manualValidation('div_entryDate', true, "Gate entry date can't be less than Doc Date");
	//            //else
	//            //    manualValidation('div_entryDate', false);
	//            //if (new Date($scope.productNew.GRNDate) < new Date($scope.productNew.EntryDate))
	//            //    return manualValidation('div_grnDate', true, "PO date can't be less than gate entry date");
	//            //else
	//            //    manualValidation('div_grnDate', false);
	//            if (new Date($scope.productNew.PODate) < new Date($scope.productNew.DocDate))
	//                return manualValidation('div_PODate', true, "PO date can't be less than Doc entry date");
	//            else
	//                manualValidation('div_PODate', false);

	//            $scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
	//            $scope.product = Object.assign({}, $scope.productNew);
	//            if ($scope.Action === "Save") {
	//                $http({
	//                    method: 'POST',
	//                    url: $scope.saveUrlFg,
	//                    data: $scope.product,
	//                    dataType: 'JSON'
	//                }).then(function (response) {
	//                    if (response.data.Error === true) {
	//                        ShowResult(response.data.Message, 'failure');
	//                    }
	//                    else {
	//                        ShowResult(response.data.Message, 'success');
	//                        $scope.productNew.Id = response.data.entity.Id;
	//                        $scope.productNew.PartyName = $scope.product.PartyName;
	//                        $scope.Action = "Update";
	//                        //$scope.getDataList();
	//                        $scope.getalldata();
	//                    }
	//                }), function (response) {
	//                    ShowResult(response.data.Message, 'failure');
	//                };
	//            }
	//            else if ($scope.Action === "Update") {

	//                $http({
	//                    method: 'POST',
	//                    url: $scope.updateUrlFG,
	//                    data: $scope.product,
	//                    dataType: 'JSON'
	//                }).then(function successCallback(response) {
	//                    if (response.data.Error === true) {
	//                        ShowResult(response.data.Message, 'failure');
	//                    }
	//                    else {
	//                        ShowResult(response.data.Message, 'success');
	//                        //$scope.getDataList();
	//                        $scope.getalldata();

	//                    }
	//                }, function errorCallBack(response) {
	//                    ShowResult(response.data.Message, 'failure');
	//                });
	//            }
	//        }
	//    } catch (e) {
	//        throw e;
	//    }
	//};

	$scope.closeServiceChargeTaxPopUpwindowFG = function () {
		//getServiceChargeList($scope.productNew.Id);
		angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
	}


	//#endregion


	$scope.checkedByList = [];
	$scope.GetSupervisorCboList = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/Requisition/GetSupervisorCbo'
		}).then(function successCallback(response) {
			$scope.checkedByList = response.data;
		});
	}
	//$scope.GetSupervisorCboList();


	baseService.getCompanyConfiguration(function (result) {
		$scope.companyConfig = result;


	});
	cboService.getCboEntityByPlant(null, null, '', function (result) {
		$scope.EntityList = result;
	});

	$scope.ReqList = [];
	$scope.GetReq = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GateentryToken/GetAllReqdata'
		}).then(function successCallback(response) {
			$scope.ReqList = response.data;
			for (var i = 0; i < $scope.ReqList.length; i++) {
				response.data[i].EntryDate = new Date($scope.ReqList[i].EntryDate);
			}
		});
	}
	$scope.GetReq();



	$scope.EmployeeList = [];
	$scope.GetEmployee = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/Requisition/GetEmployee'
		}).then(function successCallback(response) {
			$scope.EmployeeList = response.data;
		});
	}
	$scope.GetEmployee();





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

	$scope.GetCOAICodeList = function () {
		$scope.GLUrl1 = "Accounts/glitem/GetAllGLBudgetActivityList";
		$scope.GetCOAICodeListData = function (pageno) {
			baseService.paginationBase($scope.GLUrl1, pageno, $scope.glListParameters)
				.then(function (result) {
					$scope.cOAICodeList = result.Rows;
					$scope.glListParameters.total_count = result.Total;
				}, function () {
					ShowResult(commonMessage.NetworkError, "failure");
				}).finally(function () {
				});
		};
		angular.element(document.querySelector("#GLPopUp")).modal("show");
		$scope.modalShow = true;
		$scope.GetCOAICodeListData();
	};

	$scope.closeCOAICodeListPopUp = function () {
		angular.element(document.querySelector("#GLPopUp")).modal("hide");
	};

	$scope.closeCOAICodeListPopUpSelected = function () {
		if ($scope.rowSelected !== null) {
			angular.element(document.querySelector("#GLPopUp")).modal("hide");
		} else {
			angular.element(document.querySelector("#cancelPopUp")).modal("show");
		}
	};

	$scope.setSelected = function (data) {
		$scope.addRow(data);
		$scope.closeCOAICodeListPopUp();
	};

	$scope.addRow = function (data) {
		$scope.detailModel.GLGeneralInfoId = data.GLGeneralInfoId;
		$scope.detailModel.BudgetMasterId = data.BudgetMasterId;
		$scope.detailModel.ActivityId = data.ActivityId;
		$scope.detailModel.ActivityName = data.ActivityName
	};

	//Remove it
	$scope.addRowwww = function (data) {
		$scope.detailModel.GLGeneralInfoId = data.GLGeneralInfoId;
		$scope.detailModel.BudgetMasterId = data.BudgetMasterId;
		$scope.detailModel.ActivityId = data.ActivityId;
		$scope.detailModel.ActivityName = data.ActivityName
	};

	$scope.PlantWiseGateList = [];
	$scope.GetPlantWiseGateList = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GateentryToken/PlantWiseGateCbo'
		}).then(function successCallback(response) {
			$scope.PlantWiseGateList = response.data;
			if ($scope.PlantWiseGateList.length === 1) {
				$scope.productNew.PlantWiseGateId = $scope.PlantWiseGateList[0].Value;
			}
		});
	}
	$scope.GetPlantWiseGateList();

	$scope.onClickChecked = function (z) {
		debugger;
		var x = "#" + z;
		var gridObj = $(x).data("ejGrid");
		$scope.ChkedAppDataInfo = gridObj.getSelectedRecords()[0];
		$scope.url = $location.absUrl().split('!/')[1]
		$scope.message = 'Are you sure to check?';
		angular.element(document.querySelector('#CheckApprovedAlert')).modal('show');
	};
	$scope.null = null;
	$scope.IndivisulGatePassUpdatePOP = function () {
		try {


			$http({
				method: 'POST',
				url: 'Products/GateentryToken/IndivisulGatePassUpdate',
				data:
				{
					'ComId': $scope.ChkedAppDataInfo.Id,
					//'CheckedApprovedStataus': $scope.ChkedAppDataInfo.CheckedStatus,
					//'CheckedHoldRejectReason': $scope.ChkedAppDataInfo.CheckedHoldRejectReason,
					'UserSendData': $scope.ChkedAppDataInfo
				},
				dataType: 'JSON'
			}).then(function successCallback(response) {
				if (response.data.Error === true) {
					ShowResult(response.data.Message, 'failure');
				}
				else {
					ShowResult(response.data.Message, 'success');
					$scope.GetGetCheckedApprovedList();
				}
			}, function errorCallBack(response) {
				ShowResult(response.data.Message, 'failure');
			});



		}
		catch (e) {
			ShowResult(e, 'failure');
		}
	};


	$scope.productNew.GateEntryNo = null;
	$scope.productNew.EntryDate = null;

	$scope.POPopUpGateEntry = function () {
		$scope.getalldataGateEntry();
		angular.element(document.querySelector('#POPopUpGateEntry')).modal('show');
	};

	$scope.modelValidation = function (divId, modelName, fieldName, message) {
		var msg = fieldName + ' is required.';
		msg = baseService.isUndefinedOrNull(message) ? msg : message;
		var str = fieldName;
		if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
			throw manualValidation(divId, true, msg);
		else
			return manualValidation(divId, false);
	};

	$scope.GriddataGateEntry = [];
	$scope.getalldataGateEntry = function (partyId) {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/GoodsReceiveNote/GetListOfPOGateEntry?partyCode=' + partyId,
		}).then(function successCallback(response) {
			$scope.GriddataGateEntry = response.data;
		});
		angular.element(document.querySelector('#POPopUpGateEntry')).modal('show');
	};

	$scope.POPopUpCloseGateEntry = function () {
		angular.element(document.querySelector('#POPopUpGateEntry')).modal('hide');
	};

	$scope.recorddoubleclickGateEntry = function ($event) {
		var x = $event;
		var Id = x.data.Id;
		$scope.productNew.GateEntryNo = x.data.Id;
		$scope.productNew.EntryDate = x.data.EntryDate;

		$scope.POPopUpCloseGateEntry();
	}

	// Written By Nitesh
	

	//$scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
	//$scope.fileName = "GatePass.xlsx";
	//$scope.GateAgainstGatePassExl = function () {

	//	$http({
	//		method: 'POST',
	//		url: 'Products/GateentryToken/GateAgainstGatePassExl',
	//		dataType: 'JSON',
	//	})
	//		.then(function successCallback(response) {
	//			if (response.data.Error === true) {
	//				ShowResult(response.data.Message, 'failure');
	//			}
	//			else {

	//				$window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
	//			}
	//		}, function errorCallback(response) {
	//			ShowResult(response.data.Message, 'failure');
	//		});

	//};
}