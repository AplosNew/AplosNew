'use strict';
OSReceiptValueAddedController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'factoryService'];
function OSReceiptValueAddedController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, factoryService) {
	//$scope.ToDoFilePath = virtualPath.JobWorkValueAddedContract;
	//$scope.ToDownloadFilePath = virtualPath.JobWorkTransformationContract;
	$rootScope.title = 'Receipt';
	$scope.Action = 'Save';
	$scope.ModelList = [];
	$scope.IssueTypeList = [];
	$scope.IndividualReportList = [];
	$scope.GateEntryNoList = [];
	$scope.GateEntryList = [];
	$scope.TransformationTypeList = [];
	$scope.EntityList = [];
	$scope.MaterialLocationList = [];
	$scope.path = 'Outsourcing/OSReceiptValueAdded/';
	$scope.getListUrl = $scope.path + 'getlist';
	$scope.saveUrl = $scope.path + 'create';
	$scope.deleteUrl = $scope.path + 'delete/';
	baseService.init($scope.getListUrl);
	$scope.searchBy = "p.UserName"; $scope.search = "";
	$scope.searchByList = [{ value: 'p.UserName', name: "Party Name" }, { value: 'e.UserName', name: "Entity" }, { value: 'Date', name: "Date" }];

	//////// Drop Down

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

	$scope.ReceiptVAModelTemp = {
		//Id: null,
		//Date: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
		//ByWhomId: null,
		//DocumentReferenceNo: null,
		//DocumentDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
		//InvoiceNo: null,
		//InvoiceDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
		//GateEntryNoId: null,
		//Remarks: null,
		//EmployeeStatus: null,
		//EmployeeCode: null,
		//ResponsiblePerson: null,

		Id: null,
		GRNDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy')
		, CompanyGroupId: null
		, CompanyId: null
		, PlantId: null
		, PartyId: null
		, InvoicingPartyPlantId: null
		, InvoicingByAddress: null
		, InvoicingState: null
		, InvoicingGSTIN: null
		, DeliveryPartyPlantId: null
		, DeliveryByAddress: null
		, DeliveryState: null
		, DeliveryGSTIN: null
		, CutOffDate: null
		, MaterialStorageId: null
		, CurrencyId: null
		, BaseCurrencyId: null
		, ToCurrencyRate: 0
		, PaymentTermId: null
		, BaseOnDueDate: null
		, BaseNoOfDays: null
		, MatureDate: null
		, DocRefNo: null
		, DocDate: null

		, EntryDate: null
		, FixedAssetOrInventory: 'Inventory'
		, PODepended: false
		, AlongwithInvoice: true
		, InvoiceNo: null
		, InvoiceDate: null
		, IsNonCreditable: false
		, IsNonVendor: false
		, TaxApplicable: null
		, IsTaxApplicable: false
		, IsTaxApplicableChangeable: false
		, PartyType: 'Vendor'
		, Reason: null
		, EmployeeId: null
		, NoteForAccounts: null
		, OrderSpecific: 'No'
		, IsFOC: false
		, ContractId: null
		, OrderSpecific: 'No'
		, PurchaseLCId: null
		, CustomerName: null
		, PaymentMode: null
		, ContractNo: null
		, LCRef: null
		, CheckedByStatusForNoti: null
		, ApprovedByStatusForNoti: null
		, labelCheckAndApproved: null
		, FromPlantId: null
		, TaxOption: 'Yes'
		, TaxOptionMat: 'Yes'
		, TaxOptionService: 'Yes'
		, TaxOptionServiceModify: 'Yes'
		, TaxOptionAddiTax: 'Yes'
		, ByWhomId: null
		, GateEntryNo: null
		, EmployeeCode: null
		, ResponsiblePerson: null
		, ByWhomEmployeeId: null
		, EmployeeId:null
    	, TransformationContractId: null

	};
	$scope.ReceiptVA = Object.assign({}, $scope.ReceiptVAModelTemp);

	$scope.getData = function () {
		if ($scope.ModelNew.Type == null) {
			var IssueType = "ValueAdded";
			$scope.ModelNew.Type = IssueType;
		}
		$scope.setStatus = '';
		$http({
			method: 'POST',
			url: $scope.path + "GetList",
			data: { column: $scope.searchBy, value: $scope.search, Type: $scope.ModelNew.Type },
			dataType: 'JSON'
		}).then(function successCallback(response) {
			$scope.ModelList = response.data;
			$scope.ShowHomeList = true;
			$scope.ShowReport = false;
			$scope.Clear();
			ClearFieldsReceiptVAChild();
			$scope.baseCurrencyIdLoad();
			$scope.ReceiptTransformation.PartyType = 'Vendor';
			$scope.ReceiptVAChildList = [];
			$scope.IssueTypeList = [];
			$scope.ReceiptVAChildByIdList = [];
			$scope.VAGradeWiseList = [];
			$scope.showbutton = true;
			ClearFieldsReceiptTransformation();
			$scope.TransformationTypeList = [];
			$scope.ReceiptTransChildList = [];
			$scope.ReceiptTransChildByIdList = [];
			$scope.TransGradeWiseList = [];
			$scope.showbtn = true;
			$scope.ByProductList = [];

		});
	}
	$scope.getData();

	$scope.ShowHomeList = true;
	$scope.ShowReport = false;
	$scope.setStatus = '';
	$scope.Get = function (args) {
		$scope.ModelNew = Object.assign({}, args.data);
		if ($scope.ModelNew.TabType == "Transformation") {

			$scope.Transformation = Object.assign({}, args.data);
			var PId = $scope.Transformation.Id;
			var TabType = $scope.Transformation.TabType;
			$scope.TabTypeNew = $scope.Transformation.TabType;
			$scope.ReceiptTransformation.TransformationContractId = $scope.Transformation.Id;

			$http({
				method: 'POST',
				url: $scope.path + "GetDataById",
				data: { Id: PId, TabType: TabType },
				dataType: 'JSON'
			}).then(function successCallback(response) {
				$scope.TransformationTypeList = response.data;
				$scope.ReceiptTransformation.TransformationContractId = response.data[0].Id;
				if ($scope.TransformationTypeList.length > 0) {
					$scope.GetReceiptTransChildData();
					$scope.GetByProductApplicableList();
					$scope.ShowHomeList = false;
				//	$scope.ShowReport = true;
					//$scope.GetIndividualReportData();
				//	$scope.GetJWGRNDataChecking();
					$scope.GRNListDetails();
					$scope.GetTransformationReceiptCurrency();
				//	$scope.GetJWGRNDataChecking();
					$scope.setStatus = 'Selected';
					$scope.setTabGRNList(1);
				}

			});
			
			$scope.ModelNew.Type = $scope.TabTypeNew;
			//$scope.setStatus = 'Selected';
	  //  	$scope.setTabGRNList(1);
	//		$scope.setTab(2);
		}
		else {

			//  $scope.ModelNew = Object.assign({}, args.data);
			var PId = $scope.ModelNew.Id;
			var TabType = $scope.ModelNew.TabType;
			$scope.ReceiptVA.TransformationContractId = $scope.ModelNew.Id;
			$scope.ReceiptVA.PartyId = $scope.ModelNew.PartyId;
			$http({
				method: 'POST',
				url: $scope.path + "GetDataById",
				data: { Id: PId, TabType: TabType },
				dataType: 'JSON'
			}).then(function successCallback(response) {
				$scope.IssueTypeList = response.data;

				if ($scope.IssueTypeList.length > 0) {
                    $scope.GetReceiptVAChildData();
                    $scope.ShowHomeList = false;
                    //$scope.ShowReport = false;
					$scope.GetTransformationReceiptCurrency();
				//	$scope.GetIndividualReportData();
					$scope.GRNListDetails();
					$scope.setStatus = 'Selected';
					$scope.setTabGRNList(1);
				}

			});
			$scope.setTab(1);
			$scope.TabTypeNew = "ValueAdded";
			$scope.ModelNew.Type = $scope.TabTypeNew;
			//$scope.setStatus = 'Selected';
   //   		$scope.setTabGRNList(1);

				//if (!$rootScope.isCollapsed) {
		  //       $rootScope.toggle();
	   //    	}
		}
		//$scope.ModelNew.Type = TabType;
		//$scope.setStatus = 'Selected';
	//	$scope.setTabGRNList(1);


		//if (!$rootScope.isCollapsed) {
		//    $rootScope.toggle();
		//}
	};

	$scope.currencyList = [];
	$scope.GetTransformationReceiptCurrency = function () {
		if ($scope.ModelNew.TabType == "Transformation") {
			$scope.currencyList = [];
			$http({
				method: 'GET',
				url: $scope.path + 'GetTransformationReceiptCurrency?Id=' + $scope.Transformation.Id,
			}).then(function successCallback(response) {
				$scope.currencyList = response.data;
				if ($scope.currencyList.length > 0) {
					$scope.ReceiptTransformation.CurrencyId = $scope.currencyList[0].Value;
					$scope.getToCurrencyRate();
				}
			});
		}
		else {
			$scope.currencyList = [];
			$http({
				method: 'GET',
				url: $scope.path + 'GetTransformationReceiptCurrency?Id=' + $scope.ModelNew.Id,
			}).then(function successCallback(response) {
				$scope.currencyList = response.data;
				if ($scope.currencyList.length > 0) {
					$scope.ReceiptVA.CurrencyId = $scope.currencyList[0].Value;
					$scope.getToCurrencyRate();
				}
			});
        }

	}

	$scope.GetIndividualReportData = function () {
		if ($scope.ModelNew.TabType == "Transformation") {
			//$scope.IndividualReportList = [];
			//$http({
			//	method: 'GET',
			//	url: $scope.path + 'GetIndividualReportData?Id=' + $scope.Transformation.Id,
			//}).then(function successCallback(response) {
			//	$scope.IndividualReportList = response.data;
			//});
		}
		else {
			$scope.IndividualReportList = [];
			$http({
				method: 'GET',
				url: $scope.path + 'GetIndividualValAddedReportData?Id=' + $scope.ModelNew.Id + '&ReceivedId' + $scope.ModelNew.ReceiveId,
			}).then(function successCallback(response) {
				$scope.IndividualReportList = response.data;
				if ($scope.IndividualReportList.length == 0) {
					$scope.ShowHomeList = true;
					$scope.ShowReport = false;
					$scope.setTab(1);
					if (!$rootScope.isCollapsed) {
						$rootScope.toggle();
					}
				}
				else {
					$scope.ShowHomeList = false;
					$scope.ShowReport = true;
					$scope.setTab(1);
				}
			});
        }
	
	}

	$scope.GetReceiptVAChildData = function () {
		$scope.ReceiptVAChildList = [];
		$http({
			method: 'GET',
			url: $scope.path + 'GetReceiptVAChildData?PKId=' + $scope.ModelNew.Id,
		}).then(function successCallback(response) {
			$scope.ReceiptVAChildList = response.data;
		});
	}

	$scope.GetReceiptTransChildData = function () {
		$scope.ReceiptTransChildList = [];
		$scope.inventoryMaterialList = [];

		$http({
			method: 'GET',
			url: $scope.path + 'GetReceiptTransChildData?PKId=' + $scope.Transformation.Id,
		}).then(function successCallback(response) {
			$scope.ReceiptTransChildList = response.data;
			$scope.inventoryMaterialList = response.data;
		});
	}

	// Gate Entry Value Added
	$scope.POPopUpGateEntry = function () {
		$scope.getalldataGateEntry();
		angular.element(document.querySelector('#POPopUpGateEntry')).modal('show');
	};
	$scope.POPopUpCloseGateEntry = function () {
		angular.element(document.querySelector('#POPopUpGateEntry')).modal('hide');
	};

	$scope.GriddataGateEntry = [];
	$scope.getalldataGateEntry = function () {
		if ($scope.ModelNew.TabType == "Value Added") {
			$scope.PartyId = $scope.ModelNew.PartyId;
		}
		$scope.PartyId = $scope.ModelNew.PartyId;
		//debugger;
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Outsourcing/OSReceiptValueAdded/GetListOfPOGateEntry?partyCode=' + $scope.PartyId,
		}).then(function successCallback(response) {
			$scope.GriddataGateEntry = response.data;
			//entrydata = copy(searchdata);
		});
	};

	$scope.recorddoubleclickGateEntry = function (obj) {
		
		var data = obj.data;
	//	$scope.ReceiptVA.GateEntryNoId = data.Id;
		$scope.ReceiptVA.GateEntryNo = data.Id;
		$scope.ReceiptVA.PartyId = data.PartyId;
		$scope.ReceiptVA.InvoicingPartyPlantId = data.InvoicingPartyPlantId;
		$scope.ReceiptVA.InvoicingByAddress = data.InvoicingByAddress;
		$scope.ReceiptVA.DeliveryPartyPlantId = data.DeliveryPartyPlantId;
		$scope.ReceiptVA.DeliveryByAddress = data.DeliveryByAddress;

		angular.element(document.querySelector('#POPopUpGateEntry')).modal('hide');
	};

	// Gate Entry Transformation
	$scope.PopUpGateEntry = function () {
		$scope.getallGateEntry();
		angular.element(document.querySelector('#PopUpGateEntry')).modal('show');
	};
	$scope.PopUpCloseGateEntry = function () {
		angular.element(document.querySelector('#PopUpGateEntry')).modal('hide');
	};

	$scope.GridGateEntry = [];
	$scope.getallGateEntry = function () {
		if ($scope.ModelNew.TabType == "Transformation") {
			$scope.PartyId = $scope.Transformation.PartyId;
			$scope.TransformationContractId = $scope.Transformation.Id;

		}

		//debugger;
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Outsourcing/OSReceiptValueAdded/GetListGateEntry?partyCode=' + $scope.PartyId,
		}).then(function successCallback(response) {
			$scope.GridGateEntry = response.data;
		});
	};

	$scope.doubleclickGateEntry = function (obj) {
		var data = obj.data;
		$scope.ReceiptTransformation.GateEntryNo = data.Id;
		$scope.ReceiptTransformation.PartyId = data.PartyId;

		$scope.ReceiptTransformation.InvoicingPartyPlantId = data.InvoicingPartyPlantId;
		$scope.ReceiptTransformation.InvoicingByAddress = data.InvoicingByAddress;
		$scope.ReceiptTransformation.DeliveryPartyPlantId = data.DeliveryPartyPlantId;
		$scope.ReceiptTransformation.DeliveryByAddress = data.DeliveryByAddress;
		angular.element(document.querySelector('#PopUpGateEntry')).modal('hide');
	};

	$scope.Save = function () {
		$scope.$broadcast('show-errors-check-validity');
		if ($scope.ReceiptGeneralForm.$valid) {
			$http({
				method: 'POST',
				url: $scope.saveUrl,
				data: { 'data': $scope.ReceiptVA },
				dataType: 'JSON'
			}).then(function successCallback(response) {
				if (response.data.Error === true) {
					ShowResult(response.data.Message, 'failure');
				}
				else {
					ShowResult(response.data.Message, 'success');
					$scope.ReceiptVA = response.data.Data;

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

		$scope.ReceiptVA = Object.assign({}, $scope.ReceiptVAModelTemp);
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
			data: { Id: $scope.ReceiptVA.Id },
			url: $scope.path + 'LoadAllEmpDetails'
		}).then(function successCallback(response) {
			$scope.EmpResPersonList = response.data;
		});
	}

	$scope.ResponsiblePersonClear = function () {
	//	$scope.ReceiptVA.ByWhomId = null;
		$scope.ReceiptVA.ByWhomEmployeeId = null;
		$scope.ReceiptVA.ResponsiblePerson = null;
		$scope.ReceiptVA.EmployeeCode = null;
		$scope.ReceiptVA.EmployeeStatus = null;

	};
	$scope.closeEmpResPersonPopUp = function (popupName) {
		angular.element(document.querySelector("#" + popupName + "")).modal("hide");

	}
	$scope.setEmpData = function (obj) {

		var data = obj.data;
	//	$scope.ReceiptVA.ByWhomId = data.Id;
		$scope.ReceiptVA.ByWhomEmployeeId = data.Id;
		$scope.ReceiptVA.EmployeeCode = data.Code;
		$scope.ReceiptVA.ResponsiblePerson = data.EmployeeName;
		angular.element(document.querySelector('#EmployeePopUpResPerson')).modal('hide');
	};
	//   // # end region

	//  ISSUE CHILD DATA

	$scope.ReceiptVAChildList = [];

	$scope.Maintab = 1;
	$scope.setTab = function (MainnewTab) {
		$scope.Maintab = MainnewTab;
	};

	$scope.isSet = function (MaintabNum) {
		return $scope.Maintab === MaintabNum;
	};

	$scope.subtab = 1;
	$scope.settheTab = function (newsubTab) {
		$scope.subtab = newsubTab;
	};
	$scope.Set = function (tabsubNum) {
		return $scope.subtab === tabsubNum;
	};

	$scope.ReceiptVAChildModelTemp = {
		Id: null,
		JobWorkReceiptValueAddedMasterId: null,
		ContractLineItemId: null,
		OrderChildId: null,
		ReceivedQuantity: null,
		Remarks: null,
	};
	$scope.ReceiptVAChild = Object.assign({}, $scope.ReceiptVAChildModelTemp);

	$scope.Enable = false;

	$scope.showbutton = true;
	//Save Function 
	$scope.SaveReceiptVAChildTab = function () {
		var checkedData = [];
		for (var i = 0; i < $scope.ReceiptVAChildList.length; i++) {
			if ($scope.ReceiptVAChildList[i].isSelected == true)
				checkedData.push($scope.ReceiptVAChildList[i]);
		}
		try {
			if (checkedData.length == 0) {
				throw 'Please Enter at least one Received Quantity';
			}
			$http({
				method: 'POST',
				data: { ReceiptVAChildData: checkedData, MasterId: $scope.ReceiptVA.Id },
				url: $scope.path + 'SaveReceiptVAChildTab'
			}).then(function successCallback(response) {
				if (response.data.Error == true) {
					ShowResult(response.data.Message, "failure");
				}
				else {
					ShowResult(response.data.Message, "success");
					$scope.ReceiptVAChild = response.data.Data;
					$scope.Enable = true;
					$scope.GetReceiptVAChildDatabyId();
				}
			});

		}
		catch (e) {
			ShowResult(e, "failure");
		}
	}

	$scope.ValidateVARecQuantity = function (VAdata) {
		try {
			for (var i = 0; i < $scope.ReceiptVAChildList.length > 0; i++) {
				if ($scope.ReceiptVAChildList[i].OrderSpecific == "Yes") {
					if ($scope.ReceiptVAChildList[i].ContractLineItemId === VAdata.ContractLineItemId && $scope.ReceiptVAChildList[i].OrderChildId === VAdata.OrderChildId) {
						var ReceiveQty = parseFloat(VAdata.ReceivedQty);
						var ToReceiveQty = parseFloat($scope.ReceiptVAChildList[i].ToReceive)
						if (ReceiveQty > ToReceiveQty) {
							$scope.ReceiptVAChildList[i].ReceivedQty = null;
							throw 'Received Quantity cannot be greater than To Receive';
						}
					}
				}
				if ($scope.ReceiptVAChildList[i].OrderSpecific == "NO") {
					if ($scope.ReceiptVAChildList[i].ContractLineItemId === VAdata.ContractLineItemId) {
						var RecQty = parseFloat(VAdata.ReceivedQty);
						var ToRecQty = parseFloat($scope.ReceiptVAChildList[i].ToReceive)
						if (RecQty > ToRecQty) {
							$scope.ReceiptVAChildList[i].ReceivedQty = null;
							throw 'Received Quantity cannot be greater than To Receive';
						}
					}
				}
			}
		}
		catch (e) {
			ShowResult(e, "failure");
		}
	}

	$scope.ReceiptVAChildByIdList = [];
	$scope.GetReceiptVAChildDatabyId = function () {
		$scope.ReceiptVAChildByIdList = [];
		$http({
			method: 'GET',
			url: $scope.path + 'GetReceiptVAChildDatabyId?Id=' + $scope.ReceiptVA.Id,
		}).then(function successCallback(response) {
			$scope.ReceiptVAChildByIdList = response.data;
			if ($scope.ReceiptVAChildByIdList.length > 0) {
				$scope.showbutton = false;
			}

		});
	}

	$scope.ClearReceiptVAChildTab = function () {
		ClearFieldsReceiptVAChild();
		$scope.ReceiptVAChildList = [];
		$scope.IssueTypeList = [];
		$scope.ReceiptVAChildByIdList = [];
		$scope.VAGradeWiseList = [];
		$scope.getData();
		$scope.showbutton = true;
		$scope.Action = 'Save';

	}

	function ClearFieldsReceiptVAChild() {
		$scope.ReceiptVA = Object.assign({}, $scope.ReceiptVAModelTemp);
	}

	// Grade Wise Quantity

	$scope.GradeWiseList = [];
	$scope.ConfirmPopUp = function (data) {
		$scope.ReceiptVCId = data.Id;
		$scope.RecQuantity = parseFloat(data.ReceivedQty);
		angular.element(document.querySelector("#GradeWisePopUp")).modal("show");
		$scope.GetGradeWiseQuantityList();
		$scope.GetVAGradeWiseQuantityList();
	}

	$scope.GetGradeWiseQuantityList = function () {
		$scope.GradeWiseList = [];
		$http({
			method: 'GET',
			url: $scope.path + 'GetGradeWiseQuantityList',
		}).then(function successCallback(response) {
			$scope.GradeWiseList = response.data;
		});
	}


	$scope.closePopUp = function (popupName) {
		angular.element(document.querySelector("#" + popupName + "")).modal("hide");

	}

	$scope.ValidateGradeWiseQty = function () {
		try {
			if ($scope.GradeWiseList.length > 0) {
				if ($scope.GradeWiseList[0].GradeWQty == null) {
					var GradeQty1 = parseFloat(0);
				}
				else {
					var GradeQty1 = parseFloat($scope.GradeWiseList[0].GradeWQty);
				}
				if ($scope.GradeWiseList[1].GradeWQty == null) {
					var GradeQty2 = parseFloat(0);
				}
				else {
					var GradeQty2 = parseFloat($scope.GradeWiseList[1].GradeWQty);
				}
				if ($scope.GradeWiseList[2].GradeWQty == null) {
					var GradeQty3 = parseFloat(0);
				}
				else {
					var GradeQty3 = parseFloat($scope.GradeWiseList[2].GradeWQty);
				}
				if ($scope.GradeWiseList[3].GradeWQty == null) {
					var GradeQty4 = parseFloat(0);
				}
				else {
					var GradeQty4 = parseFloat($scope.GradeWiseList[3].GradeWQty);
				}


				var SumQty = GradeQty1 + GradeQty2 + GradeQty3 + GradeQty4;
				if (SumQty > $scope.RecQuantity) {
					$scope.GradeWiseList[0].GradeWQty = null
					$scope.GradeWiseList[1].GradeWQty = null
					$scope.GradeWiseList[2].GradeWQty = null
					$scope.GradeWiseList[3].GradeWQty = null
					throw 'Sum of Grade Wise Quantity cannot be greater than Received Quantity';
				}
			}
		}
		catch (e) {
			throw e;
		}
	}


	//Save Function 
	$scope.SaveGradeWiseValueAdded = function () {

		var GradeWisecheckedData = [];
		for (var i = 0; i < $scope.GradeWiseList.length; i++) {
			if ($scope.GradeWiseList[i].isSelected == true)
				GradeWisecheckedData.push($scope.GradeWiseList[i]);
		}
		try {
			$scope.ValidateGradeWiseQty();
			if (GradeWisecheckedData.length == 0) {
				throw 'Please Enter at least one Grade Wise Quantity';
			}
			$http({
				method: 'POST',
				data: { VAGradeWiseData: GradeWisecheckedData, MasterId: $scope.ReceiptVCId },
				url: $scope.path + 'SaveGradeWiseValueAdded'
			}).then(function successCallback(response) {
				if (response.data.Error == true) {
					ShowResult(response.data.Message, "failure");
				}
				else {
					ShowResult(response.data.Message, "success");
					$scope.GetVAGradeWiseQuantityList();

				}
			});

		}
		catch (e) {
			ShowResult(e, "failure");
		}
	}

	$scope.VAGradeWiseList = [];
	$scope.GetVAGradeWiseQuantityList = function () {
		$scope.VAGradeWiseList = [];
		$http({
			method: 'GET',
			url: $scope.path + 'GetVAGradeWiseQuantityList?MasterId=' + $scope.ReceiptVCId,
		}).then(function successCallback(response) {
			$scope.VAGradeWiseList = response.data;
		});
	}

	// RECEIPT TRANSFORMATION

	$scope.ReceiptTransformationModelTemp = {

		Id: null,
		GRNDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy')
		, CompanyGroupId: null
		, CompanyId: null
		, PlantId: null
		, PartyId: null
		, InvoicingPartyPlantId: null
		, InvoicingByAddress: null
		, InvoicingState: null
		, InvoicingGSTIN: null
		, DeliveryPartyPlantId: null
		, DeliveryByAddress: null
		, DeliveryState: null
		, DeliveryGSTIN: null
		, CutOffDate: null
		, MaterialStorageId: null
		, CurrencyId: null
		, BaseCurrencyId: null
		, ToCurrencyRate: 0
		, PaymentTermId: null
		, BaseOnDueDate: null
		, BaseNoOfDays: null
		, MatureDate: null
		, DocRefNo: null
		, DocDate: null

		, EntryDate: null
		, FixedAssetOrInventory: 'Inventory'
		, PODepended: false
		, AlongwithInvoice: true
		, InvoiceNo: null
		, InvoiceDate: null
		, IsNonCreditable: false
		, IsNonVendor: false
		, TaxApplicable: null
		, IsTaxApplicable: false
		, IsTaxApplicableChangeable: false
		, PartyType: 'Vendor'
		, Reason: null
		, EmployeeId: null
		, NoteForAccounts: null
		, OrderSpecific: 'No'
		, IsFOC: false
		, ContractId: null
		, OrderSpecific: 'No'
		, PurchaseLCId: null
		, CustomerName: null
		, PaymentMode: null
		, ContractNo: null
		, LCRef: null
		, CheckedByStatusForNoti: null
		, ApprovedByStatusForNoti: null
		, labelCheckAndApproved: null
		, FromPlantId: null
		, TaxOption: 'Yes'
		, TaxOptionMat: 'Yes'
		, TaxOptionService: 'Yes'
		, TaxOptionServiceModify: 'Yes'
		, TaxOptionAddiTax: 'Yes'
		, ByWhomId: null
		, GateEntryNo: null
		, EmployeeCode: null
		, ResponsiblePerson: null
		, ByWhomEmployeeId: null
		, TransformationContractId: null
	};
	$scope.ReceiptTransformation = Object.assign({}, $scope.ReceiptTransformationModelTemp);

	$scope.SaveReceiptTransformation = function () {
		$scope.$broadcast('show-errors-check-validity');
		if ($scope.ReceiptTransformationForm.$valid) {
			$http({
				method: 'POST',
				url: $scope.path + 'SaveReceiptTransformation',
				data: {
					'data': $scope.ReceiptTransformation,
					'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
					'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
				},
				dataType: 'JSON'
			}).then(function successCallback(response) {
				if (response.data.Error === true) {
					ShowResult(response.data.Message, 'failure');
				}
				else {
					ShowResult(response.data.Message, 'success');
					$scope.ReceiptTransformation = response.data.Data;
					$scope.ReceiptTransformation.Id = response.data.Data.Id;

				}
			}), function errorCallBack(response) {
				ShowResult(response.data.Message, 'failure');
			}

		}
	};

	$scope.ClearReceiptTransformation = function () {
		ClearFieldsReceiptTransformation();
	};

	function ClearFieldsReceiptTransformation() {

		$scope.ReceiptTransformation = Object.assign({}, $scope.ReceiptTransformationModelTemp);
		$scope.NotificationSettingStatus();
	}

	//   // #region field

	$scope.ByWhomPersonList = [];
	$scope.ByWhomPopUp = function () {
		angular.element(document.querySelector("#EmpPopUpResPerson")).modal("show");
		$scope.getbywhomData();

	}
	$scope.getbywhomData = function () {
		$scope.ByWhomPersonList = [];
		$http({
			method: 'POST',
			data: { Id: $scope.ReceiptTransformation.Id },
			url: $scope.path + 'LoadByWhomDetails'
		}).then(function successCallback(response) {
			$scope.ByWhomPersonList = response.data;
		});
	}

	$scope.ByWhomClear = function () {
		$scope.ReceiptTransformation.ByWhomEmployeeId = null;
		$scope.ReceiptTransformation.ByWhomName = null;
		$scope.ReceiptTransformation.EmpCode = null;
		$scope.ReceiptTransformation.EmpStatus = null;

	};
	$scope.closebywhomPopUp = function (popupName) {
		angular.element(document.querySelector("#" + popupName + "")).modal("hide");

	}
	$scope.setByWhomdata = function (obj) {

		var data = obj.data;
		$scope.ReceiptTransformation.ByWhomEmployeeId = data.Id;
		// $scope.ReceiptTransformation.EmpCode = data.Code;
		$scope.ReceiptTransformation.EmpCode = data.Id;
		$scope.ReceiptTransformation.ByWhomName = data.EmployeeName;
		angular.element(document.querySelector('#EmpPopUpResPerson')).modal('hide');
	};

	//  RECEIPT TRANSFORMATION CHILD

	$scope.ReceiptTransChildList = [];

	$scope.Enablegrid = false;
	$scope.showbtn = true;
	//Save Function 
	$scope.SaveReceiptTransChildTab = function () {
		var TranscheckedData = [];
		for (var i = 0; i < $scope.ReceiptTransChildList.length; i++) {
			if ($scope.ReceiptTransChildList[i].isSelected == true)
				TranscheckedData.push($scope.ReceiptTransChildList[i]);
		}
		try {
			if (TranscheckedData.length == 0) {
				throw 'Please Enter at least one Received Quantity';
			}
			$http({
				method: 'POST',
				data: { ReceiptTransChildData: TranscheckedData, MasterId: $scope.ReceiptTransformation.Id },
				url: $scope.path + 'SaveReceiptTransChildTab'
			}).then(function successCallback(response) {
				if (response.data.Error == true) {
					ShowResult(response.data.Message, "failure");
				}
				else {
					ShowResult(response.data.Message, "success");
					$scope.Enablegrid = true;
					$scope.GetReceiptTransChildDatabyId();

				}
			});

		}
		catch (e) {
			ShowResult(e, "failure");
		}
	}

	$scope.ClearReceiptTransChildTab = function () {
		ClearFieldsReceiptTransformation();
		$scope.TransformationTypeList = [];
		$scope.ReceiptTransChildList = [];
		$scope.ReceiptTransChildByIdList = [];
		$scope.TransGradeWiseList = [];
		$scope.ByProductList = [];
		$scope.getData();
		$scope.showbtn = true;
		$scope.inventoryMaterialList = [];
		$scope.inventoryMaterialListPO = [];
		$scope.Action = 'Save';
	}

	//$scope.ValidateReceiptTransChildQuantity = function (SelData) {
	//    try {

	//        for (var i = 0; i < $scope.ReceiptTransChildList.length > 0; i++) {

	//            if ($scope.ReceiptTransChildList[i].Id === SelData.Id) {
	//                var ReceiveQty = parseFloat(SelData.ReceivedQty);
	//                var ToReceiveQty = parseFloat($scope.ReceiptTransChildList[i].ToReceive)
	//                if (ReceiveQty > ToReceiveQty) {
	//                    $scope.ReceiptTransChildList[i].ReceivedQty = null;
	//                    throw 'Receive Quantity cannot be greater than To Receive';
	//                }
	//            }
	//        }
	//    }
	//    catch (e) {
	//        ShowResult(e, "failure");
	//    }

	//}

	$scope.ReceiptTransChildByIdList = [];
	$scope.GetReceiptTransChildDatabyId = function () {
		$scope.ReceiptTransChildByIdList = [];
		$http({
			method: 'GET',
			url: $scope.path + 'GetReceiptTransChildDatabyId?Id=' + $scope.ReceiptTransformation.Id,
		}).then(function successCallback(response) {
			$scope.ReceiptTransChildByIdList = response.data;
			if ($scope.ReceiptTransChildByIdList.length > 0) {
				$scope.showbtn = false;
			}

		});
	}

	// Grade Wise Quantity

	$scope.GradeList = [];
	$scope.ConfirmGradewisePopUp = function (data) {
		$scope.ReceiptTransId = data.ReceiptTransChildId;
		$scope.RecQty = data.ReceivedQty;
		angular.element(document.querySelector("#GradePopUp")).modal("show");
		$scope.GetGradeQuantityList();
		$scope.GetTransGradeQuantityList();
	}

	$scope.GetGradeQuantityList = function () {
		$scope.GradeList = [];
		$http({
			method: 'GET',
			url: $scope.path + 'GetGradeQuantityList',
		}).then(function successCallback(response) {
			$scope.GradeList = response.data;
		});
	}


	$scope.closeGradePopUp = function (popupName) {
		angular.element(document.querySelector("#" + popupName + "")).modal("hide");

	}


	//Save Function 
	$scope.SaveGradeWiseTrans = function () {
		var GradecheckedData = [];
		for (var i = 0; i < $scope.GradeList.length; i++) {
			if ($scope.GradeList[i].isSelected == true)
				GradecheckedData.push($scope.GradeList[i]);
		}
		try {
			$scope.ValidateTransformationGradeWiseQty();
			if (GradecheckedData.length == 0) {
				throw 'Please Enter at least one Grade Wise Quantity';
			}
			$http({
				method: 'POST',
				data: { TransGradeWiseData: GradecheckedData, MasterId: $scope.ReceiptTransId },
				url: $scope.path + 'SaveGradeWiseTrans'
			}).then(function successCallback(response) {
				if (response.data.Error == true) {
					ShowResult(response.data.Message, "failure");
				}
				else {
					ShowResult(response.data.Message, "success");
					$scope.GetTransGradeQuantityList();

				}
			});

		}
		catch (e) {
			ShowResult(e, "failure");
		}
	}

	$scope.TransGradeWiseList = [];
	$scope.GetTransGradeQuantityList = function () {
		$scope.TransGradeWiseList = [];
		$http({
			method: 'GET',
			url: $scope.path + 'GetTransGradeQuantityList?MasterId=' + $scope.ReceiptTransId,
		}).then(function successCallback(response) {
			$scope.TransGradeWiseList = response.data;
		});
	}

	$scope.ValidateTransformationGradeWiseQty = function () {
		try {
			if ($scope.GradeList.length > 0) {
				if ($scope.GradeList[0].GradeWQty == null) {
					var GradeQty1 = parseFloat(0);
				}
				else {
					var GradeQty1 = parseFloat($scope.GradeList[0].GradeWQty);
				}
				if ($scope.GradeList[1].GradeWQty == null) {
					var GradeQty2 = parseFloat(0);
				}
				else {
					var GradeQty2 = parseFloat($scope.GradeList[1].GradeWQty);
				}
				if ($scope.GradeList[2].GradeWQty == null) {
					var GradeQty3 = parseFloat(0);
				}
				else {
					var GradeQty3 = parseFloat($scope.GradeList[2].GradeWQty);
				}
				if ($scope.GradeList[3].GradeWQty == null) {
					var GradeQty4 = parseFloat(0);
				}
				else {
					var GradeQty4 = parseFloat($scope.GradeList[3].GradeWQty);
				}


				var SumQty = GradeQty1 + GradeQty2 + GradeQty3 + GradeQty4;
				if (SumQty > $scope.RecQty) {
					$scope.GradeList[0].GradeWQty = null
					$scope.GradeList[1].GradeWQty = null
					$scope.GradeList[2].GradeWQty = null
					$scope.GradeList[3].GradeWQty = null
					throw 'Sum of Grade Wise Quantity cannot be greater than Received Quantity';
				}
			}
		}
		catch (e) {
			throw e;
		}
	}

	// By Product TAB

	$scope.ByProductList = [];
	$scope.inventoryMaterialListPO = [];

	$scope.GetByProductApplicableList = function () {
		$scope.ByProductList = [];
		$http({
			method: 'GET',
			url: $scope.path + 'GetByProductApplicableList?Id=' + $scope.Transformation.Id,
		}).then(function successCallback(response) {
			$scope.ByProductList = response.data;
			$scope.inventoryMaterialListPO = response.data;
		});
	}

	//Save Function 
	$scope.SaveByProduct = function () {
		var ByProductcheckedData = [];
		for (var i = 0; i < $scope.ByProductList.length; i++) {
			if ($scope.ByProductList[i].isSelected == true)
				ByProductcheckedData.push($scope.ByProductList[i]);
		}
		try {
			if (ByProductcheckedData.length == 0) {
				throw 'Please Select at least one By Product Quantity';
			}
			$http({
				method: 'POST',
				data: { ByProductData: ByProductcheckedData, MasterId: $scope.ReceiptTransformation.Id },
				url: $scope.path + 'SaveByProduct'
			}).then(function successCallback(response) {
				if (response.data.Error == true) {
					ShowResult(response.data.Message, "failure");
				}
				else {
					ShowResult(response.data.Message, "success");
					$scope.GetByProductApplicableList();
				}
			});

		}
		catch (e) {
			ShowResult(e, "failure");
		}
	}

	$scope.ClearByProductTab = function () {
		$scope.ByProductList = [];
		$scope.ClearReceiptTransChildTab();
	}

	//$scope.ValidateByProductQuantity = function (RowData) {
	//    try {

	//        for (var i = 0; i < $scope.ByProductList.length > 0; i++) {

	//                if ($scope.ByProductList[i].Id === RowData.Id) {
	//                    var ReceiveQty = parseFloat(RowData.ReceiveQuantity);
	//                    var ToReceiveQty = parseFloat($scope.ByProductList[i].ToReceive)
	//                    if (ReceiveQty > ToReceiveQty) {
	//                        $scope.ByProductList[i].ReceiveQuantity = null;
	//                        throw 'Receive Quantity cannot be greater than To Receive';
	//                    }
	//                }    
	//        }
	//    }
	//    catch (e) {
	//        ShowResult(e, "failure");
	//    }

	//}

	// PRINT JOB WORK TRANSFORMATION REPORT

	// Print Template
	$scope.AllTabPrint = function (data) {
		if (data.POType == "OSTransformationPO") {
			//var x = "#" + z;
			//var gridObj = $(x).data("ejGrid");
			//var data = gridObj.getSelectedRecords()[0];

			location.href = "Outsourcing/OSTransformationPO/GePurchaseOrderReport?purchaseOrderId=" + data.Id;
		//	$scope.getalldata();
        }

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

	// INDIVIDUAL RECEIPT REPORT

	$scope.PrintReceiptReport = function (data) {
		if ($scope.ModelNew.TabType == "Transformation") {
			try {
				//	$scope.PrintTabId = data.ContractId;
				//	$scope.PrintTabId = data.TransformationContractId;
				$scope.PrintTabId = $scope.Transformation.Id;
				$scope.IssueId = data.Id;
				var reportFormat = "Excel";
				window.open('Outsourcing/OSReceiptValueAdded/GetTransformationPrintReport?reportFormat=' + reportFormat + '&PrintTabId=' + $scope.PrintTabId + '&IssueId=' + $scope.IssueId, '_blank');
				//   $scope.getData();
				$scope.setStatus = 'Selected';
				$scope.setTabGRNList(2);

			} catch (e) {

			}
		}
		else {
			try {
				//$scope.PrintTabId = data.Id;
				//$scope.IssueId = data.ReceiveId;

				$scope.PrintTabId = $scope.ModelNew.Id;
				$scope.IssueId = data.Id;

				var reportFormat = "Excel";
				window.open('Outsourcing/OSReceiptValueAdded/GetValueAddedPrintReceiptReport?reportFormat=' + reportFormat + '&PrintTabId=' + $scope.PrintTabId + '&IssueId=' + $scope.IssueId, '_blank');
				$scope.setStatus = 'Selected';
				$scope.setTabGRNList(1);

			} catch (e) {

			}
        }

	};

	$scope.AllTabPrintTemplate = function (data) {
		if ($scope.ModelNew.TabType == "Transformation") {
			//debugger;
			//var x = "#" + z;
			//var gridObj = $(x).data("ejGrid");
			//var data = gridObj.getSelectedRecords()[0];
			location.href = "Outsourcing/OSReceiptValueAdded/GRNReport?grnId=" + data.Id;
			$scope.setStatus = 'Selected';
			$scope.setTabGRNList(2);
		}
		else {
		//	location.href = "Outsourcing/OSReceiptValueAdded/ValAddedGRNReport?grnId=" + data.ReceiveId;
			location.href = "Outsourcing/OSReceiptValueAdded/ValAddedGRNReport?grnId=" + data.Id;
			$scope.setStatus = 'Selected';
			$scope.setTabGRNList(1);
        }
	};


	//region code by sk
	cboService.getCboTransactionCurrencyByCompany('', function (result) {
		$scope.currencyList = result;
	});
	$scope.getToCurrencyRate = function () {
		if ($scope.ModelNew.TabType == "Transformation") {
			//if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.DocDate)) {
			//	$scope.ReceiptTransformation.ToCurrencyRate = 1;
			//	return;
			//}
			//$http.get('Products/InventoryReceive/GetToCurrencyRateForJWR?currencyId=' + $scope.ReceiptTransformation.CurrencyId + '&baseCurrencyId=' + $scope.ReceiptTransformation.BaseCurrencyId + '&docDate=' + $filter('dateFiltering')($scope.ReceiptTransformation.DocDate))
			//	.then(function (response) {
			//		if (parseFloat(response.data) === 0)
			//			$scope.ReceiptTransformation.ToCurrencyRate = 1;
			//		else
			//			$scope.ReceiptTransformation.ToCurrencyRate = response.data;
			//	});

			$scope.ReceiptTransformation.ToCurrencyRate = 1;
		}
		else {
			//if (baseService.isUndefinedOrNull($scope.ReceiptVA.DocDate)) {
			//	$scope.ReceiptVA.ToCurrencyRate = 1;
			//	return;
			//}
			//$http.get('Products/InventoryReceive/GetToCurrencyRateForJWR?currencyId=' + $scope.ReceiptVA.CurrencyId + '&baseCurrencyId=' + $scope.ReceiptVA.BaseCurrencyId + '&docDate=' + $filter('dateFiltering')($scope.ReceiptVA.DocDate))
			//	.then(function (response) {
			//		if (parseFloat(response.data) === 0)
			//			$scope.ReceiptVA.ToCurrencyRate = 1;
			//		else
			//			$scope.ReceiptVA.ToCurrencyRate = response.data;
			//	});

			$scope.ReceiptVA.ToCurrencyRate = 1;
        }

	};

	$scope.storageList = [];
	$http({
		method: 'GET',
		url: 'Materials/MaterialStorage/getcbo'
	}).then(function (response) {
		$scope.storageList = response.data;
	});

	//#region notification setting
	$scope.chargesList = [];
	$scope.NotificationSettingStatus = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/InventoryReceive/JWNotificationSettingReceipt',
			dataType: 'JSON'
		}).then(function successCallback(response) {
			$scope.NotificationSetting = response.data;
			$scope.CheckedByStatusForNoti = $scope.NotificationSetting[0].RequiredChecking;
			$scope.ApprovedByStatusForNoti = $scope.NotificationSetting[0].RequiredApproval;
			//$scope.GetCheckedByAndApprovedBy1();
			if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === false) {
				$scope.ReceiptTransformation.labelCheckAndApproved = 'To be checked by';
			}
			else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true) {
				$scope.ReceiptTransformation.labelCheckAndApproved = 'To be approved by';
			}
			else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true) {
				$scope.ReceiptTransformation.labelCheckAndApproved = 'To be checked by';
			}
			//else {
			//    $scope.productNew.labelCheckAndApproved = 'To be checked/approved by';
			//}

		});
	}
	$scope.NotificationSettingStatus();
	$scope.GetCheckedByAndApprovedBy1 = function () {
		//debugger;

		if (!baseService.isUndefinedOrNull($scope.CheckedByStatusForNoti) && !baseService.isUndefinedOrNull($scope.ApprovedByStatusForNoti)) {
			$http({
				method: 'GET',
				url: 'Products/InventoryReceive/GetJWCheckedByAndApprovedBY?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
				dataType: 'JSON'
			}).then(function successCallback(response) {
				$scope.checkedByList = response.data;
			});

		}
		else {

		}

	}




	//#endregion

	$http({
		method: 'GET',
		url: 'Materials/MaterialStorage/getcbo'
	}).then(function (response) {
		$scope.storageList = response.data;
	});


	var SelectedWOMaterial = [];
	var SelectedByProductWOMaterial = [];
	$scope.JWSave = function () {

		if ($scope.ModelNew.TabType == "Transformation") {

			$scope.inventoryMaterialListPOnew = [];
			$scope.inventoryMaterialListPOnew1 = [];
			$scope.chargesListPOnew = [];
			try {

				$scope.$broadcast('show-errors-check-validity');
				//if ($scope.productNewForm.$valid) {
				if ($scope.Action === "Save") {
					if ($scope.inventoryMaterialList.length > 0) {

						if ($scope.ReceiptTransformation.GRNDate > new Date()) {
							ShowResult("GRN Date  can not grather than Today's Date", 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.NoteForAccounts)) {
							ShowResult("Enter Note for accounts", 'failure');
							return false;
						}
						else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.ReceiptTransformation.CheckedBy)) {
							ShowResult("Please select to be approved by", 'failure');
							return false;
						}
						else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.ReceiptTransformation.CheckedBy)) {
							ShowResult("Please select to be checked by", 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.InvoicingPartyPlantId)) {
							return ShowResult('Invoicing by is required', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.DeliveryPartyPlantId)) {
							return ShowResult('Delivery by is required', 'failure');
							return false;
						}


						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.DocRefNo)) {
							return ShowResult('Enter Doc Ref No', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.DocDate)) {
							return ShowResult('Enter Doc Date', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.GateEntryNo)) {
							return ShowResult('Select Gate Entry No', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.GRNDate)) {
							return ShowResult('Enter GRN Date', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.CurrencyId)) {
							return ShowResult('Select Currency', 'failure');
							return false;
						}

						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.ByWhomName)) {
							return ShowResult('Select By Whom', 'failure');
							return false;
						}

						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.MaterialStorageId)) {
							return ShowResult('Select Material Storage', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.NoteForAccounts)) {
							return ShowResult('Enter the Note For Accounts', 'failure');
							return false;
						}

						else {
							for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
								if ($scope.inventoryMaterialList[i].check == true) {
									if (baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].MaterialStorageId)) {
										ShowResult("Please select storage location", 'failure');
										return false;
									}
									else if (baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].QualityStatus)) {
										ShowResult("Please select quality status", 'failure');
										return false;
									}
									else if ($scope.inventoryMaterialList[i].TransactionQty > 0 && $scope.inventoryMaterialList[i].check === false) {
										ShowResult("Please select the material", 'failure');
										return false;
									}
									else if ($scope.inventoryMaterialList[i].TransactionQty === '0' && $scope.inventoryMaterialList[i].check === true) {
										ShowResult("Enter the Qty", 'failure');
										return false;
									}
									else if (baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].TransactionQty) && $scope.inventoryMaterialList[i].check === true) {
										ShowResult("Enter the Qty", 'failure');
										return false;
									}
									// $scope.inventoryMaterialListPOnew[i].TotalMaterialBooksCurrencyAmount = $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount;
									$scope.inventoryMaterialListPOnew.push($scope.inventoryMaterialList[i]);

								}

							}
							
						}
					}
					if ($scope.inventoryMaterialListPO.length > 0) {


						if ($scope.ReceiptTransformation.GRNDate > new Date()) {
							ShowResult("GRN Date  can not grather than Today's Date", 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.NoteForAccounts)) {
							ShowResult("Enter Note for accounts", 'failure');
							return false;
						}
						else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.ReceiptTransformation.CheckedBy)) {
							ShowResult("Please select to be approved by", 'failure');
							return false;
						}
						else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.ReceiptTransformation.CheckedBy)) {
							ShowResult("Please select to be checked by", 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.InvoicingPartyPlantId)) {
							return ShowResult('Invoicing by is required', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.DeliveryPartyPlantId)) {
							return ShowResult('Delivery by is required', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.DocRefNo)) {
							return ShowResult('Enter Doc Ref No', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.DocDate)) {
							return ShowResult('Enter Doc Date', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.GateEntryNo)) {
							return ShowResult('Select Gate Entry No', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.GRNDate)) {
							return ShowResult('Enter GRN Date', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.CurrencyId)) {
							return ShowResult('Select Currency', 'failure');
							return false;
						}

						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.ByWhomName)) {
							return ShowResult('Select By Whom', 'failure');
							return false;
						}

						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.MaterialStorageId)) {
							return ShowResult('Select Material Storage', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.NoteForAccounts)) {
							return ShowResult('Enter the Note For Accounts', 'failure');
							return false;
						}
						
						else {

							for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
								if ($scope.inventoryMaterialListPO[i].check == true) {
									if (baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].MaterialStorageId)) {
										ShowResult("Please select storage location", 'failure');
										return false;
									}
									else if (baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].QualityStatus)) {
										ShowResult("Please select quality status", 'failure');
										return false;
									}
									else if ($scope.inventoryMaterialListPO[i].TransactionQty === '0' && $scope.inventoryMaterialListPO[i].check === true) {
										ShowResult("Enter the Qty", 'failure');
										return false;
									}
									else if (baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].TransactionQty) && $scope.inventoryMaterialListPO[i].check === true) {
										ShowResult("Enter the Qty", 'failure');
										return false;
									}
									$scope.inventoryMaterialListPOnew1.push($scope.inventoryMaterialListPO[i]);

								}

							}
						}
					}
					if ($scope.inventoryMaterialListPOnew1.length === 0) {
						$scope.inventoryMaterialListPOnew1 = null;
					}
					//debugger;
					$http({
						method: 'POST',
						url: 'Products/GoodsReceiveNote/CreateOSReceiptGRN',
						data:
						{
							'entity': $scope.ReceiptTransformation,
							'entityMatAndImat': JSON.stringify($scope.inventoryMaterialListPOnew),
							'receiveTaxList': $scope.POMaterialTaxList,
							'chargesListPO': $scope.chargesListPOnew,
							'POServiceTaxList': $scope.POServiceTaxList,
						//	'GRNType': 'GRNBYPO',
							'GRNType': 'GRNBYOS',
							'AcceptanceId': $scope.AcceptanceId,
							'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
							'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti,
							'entityMatByProduct': JSON.stringify($scope.inventoryMaterialListPOnew1),
						},
						dataType: 'JSON'
						, contentType: "application/json charset=utf-8"
					}).then(function (response) {
						if (response.data.Error === true) {
							ShowResult(response.data.Message, 'failure');
						}
						else {
							ShowResult(response.data.Message, 'success');
							$scope.ReceiptTransformation.Id = response.data.entity.Id;
							$scope.setStatus = 'Selected';
							$scope.setTabGRNList(1);
							$scope.GRNListDetails();
						//	$scope.detailgrid();
							//$scope.SaveButtonDisable = true;
							//$scope.setTabGRNList(1);
							//$scope.getDataList();
							//$scope.GRNListDetails();

							//$scope.productId = response.data.entity.Id;
							//$scope.productNew.Id = response.data.entity.Id;
							//$scope.productNew.msgForAllocationNeed = response.data.entity.msgForAllocationNeed;

						}
					}), function (response) {
						ShowResult(response.data.Message, 'failure');
					};
					//}



				}
				else if ($scope.Action === "Update") {
					if ($scope.inventoryMaterialList.length > 0) {

						if ($scope.ReceiptTransformation.GRNDate > new Date()) {
							ShowResult("GRN Date  can not grather than Today's Date", 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.NoteForAccounts)) {
							ShowResult("Enter Note for accounts", 'failure');
							return false;
						}
						else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.ReceiptTransformation.CheckedBy)) {
							ShowResult("Please select to be approved by", 'failure');
							return false;
						}
						else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.ReceiptTransformation.CheckedBy)) {
							ShowResult("Please select to be checked by", 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.InvoicingPartyPlantId)) {
							return ShowResult('Invoicing by is required', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.DeliveryPartyPlantId)) {
							return ShowResult('Delivery by is required', 'failure');
							return false;
						}


						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.DocRefNo)) {
							return ShowResult('Enter Doc Ref No', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.DocDate)) {
							return ShowResult('Enter Doc Date', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.GateEntryNo)) {
							return ShowResult('Select Gate Entry No', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.GRNDate)) {
							return ShowResult('Enter GRN Date', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.CurrencyId)) {
							return ShowResult('Select Currency', 'failure');
							return false;
						}

						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.ByWhomName)) {
							return ShowResult('Select By Whom', 'failure');
							return false;
						}

						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.MaterialStorageId)) {
							return ShowResult('Select Material Storage', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.NoteForAccounts)) {
							return ShowResult('Enter the Note For Accounts', 'failure');
							return false;
						}




						//else if (new Date($scope.ReceiptTransformation.EntryDate) < new Date($scope.ReceiptTransformation.DocDate)) {
						//	return manualValidation("Gate entry date can't be less than Doc Date", 'failure');
						//}

						//else if (new Date($scope.ReceiptTransformation.GRNDate) < new Date($scope.ReceiptTransformation.EntryDate)) {
						//	return manualValidation("GRN date can't be less than gate entry date", 'failure');

						//}
						else {
							for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
								if ($scope.inventoryMaterialList[i].check == true) {
									if (baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].MaterialStorageId)) {
										ShowResult("Please select storage location", 'failure');
										return false;
									}
									else if (baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].QualityStatus)) {
										ShowResult("Please select quality status", 'failure');
										return false;
									}
									else if ($scope.inventoryMaterialList[i].TransactionQty > 0 && $scope.inventoryMaterialList[i].check === false) {
										ShowResult("Please select the material", 'failure');
										return false;
									}
									else if ($scope.inventoryMaterialList[i].TransactionQty === '0' && $scope.inventoryMaterialList[i].check === true) {
										ShowResult("Enter the Qty", 'failure');
										return false;
									}
									else if (baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].TransactionQty) && $scope.inventoryMaterialList[i].check === true) {
										ShowResult("Enter the Qty", 'failure');
										return false;
									}
									// $scope.inventoryMaterialListPOnew[i].TotalMaterialBooksCurrencyAmount = $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount;
									$scope.inventoryMaterialListPOnew.push($scope.inventoryMaterialList[i]);

								}

							}
							//for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
							//	if ($scope.inventoryMaterialListPO[i].check == true) {
							//		if (baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].MaterialStorageId)) {
							//			ShowResult("Please select storage location", 'failure');
							//			return false;
							//		}
							//		else if (baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].QualityStatus)) {
							//			ShowResult("Please select quality status", 'failure');
							//			return false;
							//		}
							//		$scope.inventoryMaterialListPOnew1.push($scope.inventoryMaterialListPO[i]);

							//	}

							//}
						}
					}
					if ($scope.inventoryMaterialListPO.length > 0) {


						if ($scope.ReceiptTransformation.GRNDate > new Date()) {
							ShowResult("GRN Date  can not grather than Today's Date", 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.NoteForAccounts)) {
							ShowResult("Enter Note for accounts", 'failure');
							return false;
						}
						else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.ReceiptTransformation.CheckedBy)) {
							ShowResult("Please select to be approved by", 'failure');
							return false;
						}
						else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.ReceiptTransformation.CheckedBy)) {
							ShowResult("Please select to be checked by", 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.InvoicingPartyPlantId)) {
							return ShowResult('Invoicing by is required', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.DeliveryPartyPlantId)) {
							return ShowResult('Delivery by is required', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.DocRefNo)) {
							return ShowResult('Enter Doc Ref No', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.DocDate)) {
							return ShowResult('Enter Doc Date', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.GateEntryNo)) {
							return ShowResult('Select Gate Entry No', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.GRNDate)) {
							return ShowResult('Enter GRN Date', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.CurrencyId)) {
							return ShowResult('Select Currency', 'failure');
							return false;
						}

						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.ByWhomName)) {
							return ShowResult('Select By Whom', 'failure');
							return false;
						}

						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.MaterialStorageId)) {
							return ShowResult('Select Material Storage', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptTransformation.NoteForAccounts)) {
							return ShowResult('Enter the Note For Accounts', 'failure');
							return false;
						}
						//else if (new Date($scope.ReceiptTransformation.EntryDate) < new Date($scope.ReceiptTransformation.DocDate)) {
						//	return manualValidation("Gate entry date can't be less than Doc Date", 'failure');
						//}

						//else if (new Date($scope.ReceiptTransformation.GRNDate) < new Date($scope.ReceiptTransformation.EntryDate)) {
						//	return manualValidation("GRN date can't be less than gate entry date", 'failure');

						//}
						else {

							//for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
							//	if ($scope.inventoryMaterialList[i].check == true) {
							//		if (baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].MaterialStorageId)) {
							//			ShowResult("Please select storage location", 'failure');
							//			return false;
							//		}
							//		else if (baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].QualityStatus)) {
							//			ShowResult("Please select quality status", 'failure');
							//			return false;
							//		}
							//		// $scope.inventoryMaterialListPOnew[i].TotalMaterialBooksCurrencyAmount = $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount;
							//		$scope.inventoryMaterialListPOnew.push($scope.inventoryMaterialList[i]);

							//	}

							//}
							for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
								if ($scope.inventoryMaterialListPO[i].check == true) {
									if (baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].MaterialStorageId)) {
										ShowResult("Please select storage location", 'failure');
										return false;
									}
									else if (baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].QualityStatus)) {
										ShowResult("Please select quality status", 'failure');
										return false;
									}
									else if ($scope.inventoryMaterialListPO[i].TransactionQty === '0' && $scope.inventoryMaterialListPO[i].check === true) {
										ShowResult("Enter the Qty", 'failure');
										return false;
									}
									else if (baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].TransactionQty) && $scope.inventoryMaterialListPO[i].check === true) {
										ShowResult("Enter the Qty", 'failure');
										return false;
									}
									$scope.inventoryMaterialListPOnew1.push($scope.inventoryMaterialListPO[i]);

								}

							}
						}
					}
					$http({
						method: 'POST',
						url: 'Products/GoodsReceiveNote/CreateJWGRN',
						data:
						{
							'entity': $scope.ReceiptTransformation,
							'entityMatAndImat': JSON.stringify($scope.inventoryMaterialListPOnew),
							'receiveTaxList': $scope.POMaterialTaxList,
							'chargesListPO': $scope.chargesListPOnew,
							'POServiceTaxList': $scope.POServiceTaxList,
						//	'GRNType': 'GRNBYPO',
							'GRNType': 'GRNBYOS',
							'AcceptanceId': $scope.AcceptanceId,
							'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
							'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti,
							'entityMatByProduct': JSON.stringify($scope.inventoryMaterialListPOnew1),
						},
						dataType: 'JSON'
					}).then(function successCallback(response) {
						if (response.data.Error === true) {
							ShowResult(response.data.Message, 'failure');
						}
						else {
							ShowResult(response.data.Message, 'success');
							$scope.ReceiptTransformation.Id = response.data.entity.Id;
							$scope.ClearReceiptTransChildTab();
							//ShowResult(response.data.Message, 'success');
							//$scope.setTabGRNList(1);
							//$scope.getDataList();
							//$scope.GRNListDetails();

							//$scope.productId = response.data.entity.Id;
							//$scope.productNew.Id = response.data.entity.Id;
							//$scope.productNew.msgForAllocationNeed = response.data.entity.msgForAllocationNeed;

						}
					}, function errorCallBack(response) {
						ShowResult(response.data.Message, 'failure');
					});




				}

				// }
			} catch (e) {
				throw e;
			}
		}
		else {
			$scope.inventoryMaterialListPOnewValAdded = [];
			$scope.inventoryMaterialListPOnew1 = [];
			$scope.chargesListPOnew = [];
			try {

				$scope.$broadcast('show-errors-check-validity');
				//if ($scope.productNewForm.$valid) {
				if ($scope.Action === "Save") {
					if ($scope.ReceiptVAChildList.length > 0) {

						if ($scope.ReceiptVA.GRNDate > new Date()) {
							ShowResult("GRN Date  can not grather than Today's Date", 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.NoteForAccounts)) {
							ShowResult("Enter Note for accounts", 'failure');
							return false;
						}
						else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.ReceiptVA.CheckedBy)) {
							ShowResult("Please select to be approved by", 'failure');
							return false;
						}
						else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.ReceiptVA.CheckedBy)) {
							ShowResult("Please select to be checked by", 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.InvoicingPartyPlantId)) {
							return ShowResult('Invoicing by is required', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.DeliveryPartyPlantId)) {
							return ShowResult('Delivery by is required', 'failure');
							return false;
						}


						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.DocRefNo)) {
							return ShowResult('Enter Doc Ref No', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.DocDate)) {
							return ShowResult('Enter Doc Date', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.GateEntryNo)) {
							return ShowResult('Select Gate Entry No', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.GRNDate)) {
							return ShowResult('Enter GRN Date', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.CurrencyId)) {
							return ShowResult('Select Currency', 'failure');
							return false;
						}

						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.ResponsiblePerson)) {
							return ShowResult('Select By Whom', 'failure');
							return false;
						}

						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.MaterialStorageId)) {
							return ShowResult('Select Material Storage', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.NoteForAccounts)) {
							return ShowResult('Enter the Note For Accounts', 'failure');
							return false;
						}




						//else if (new Date($scope.ReceiptTransformation.EntryDate) < new Date($scope.ReceiptTransformation.DocDate)) {
						//	return manualValidation("Gate entry date can't be less than Doc Date", 'failure');
						//}

						//else if (new Date($scope.ReceiptTransformation.GRNDate) < new Date($scope.ReceiptTransformation.EntryDate)) {
						//	return manualValidation("GRN date can't be less than gate entry date", 'failure');

						//}
						else {
							for (var i = 0; i < $scope.ReceiptVAChildList.length; i++) {
								if ($scope.ReceiptVAChildList[i].check == true) {
									if (baseService.isUndefinedOrNull($scope.ReceiptVAChildList[i].MaterialStorageId)) {
										ShowResult("Please select storage location", 'failure');
										return false;
									}
									else if (baseService.isUndefinedOrNull($scope.ReceiptVAChildList[i].QualityStatus)) {
										ShowResult("Please select quality status", 'failure');
										return false;
									}
									else if ($scope.ReceiptVAChildList[i].TransactionQty > 0 && $scope.ReceiptVAChildList[i].check === false) {
										ShowResult("Please select the material", 'failure');
										return false;
									}
									else if ($scope.ReceiptVAChildList[i].TransactionQty === '0' && $scope.ReceiptVAChildList[i].check === true) {
										ShowResult("Enter the Qty", 'failure');
										return false;
									}
									else if (baseService.isUndefinedOrNull($scope.ReceiptVAChildList[i].TransactionQty) && $scope.ReceiptVAChildList[i].check === true) {
										ShowResult("Enter the Qty", 'failure');
										return false;
									}
									$scope.inventoryMaterialListPOnewValAdded.push($scope.ReceiptVAChildList[i]);

								}

							}
						}
					}
					if ($scope.inventoryMaterialListPOnew1.length === 0) {
						$scope.inventoryMaterialListPOnew1 = null;
					}

		//			$scope.ReceiptVA.PartyId = $scope.ModelNew.PartyId;

					//debugger;
					$http({
						method: 'POST',
						url: 'Products/GoodsReceiveNote/CreateJWGRN',
						data:
						{
							'entity': $scope.ReceiptVA,
							'entityMatAndImat': JSON.stringify($scope.inventoryMaterialListPOnewValAdded),
							'receiveTaxList': $scope.POMaterialTaxList,
							'chargesListPO': $scope.chargesListPOnew,
							'POServiceTaxList': $scope.POServiceTaxList,
						//	'GRNType': 'GRNBYPO',
							'GRNType': 'GRNBYOS',
							'AcceptanceId': $scope.AcceptanceId,
							'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
							'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti,
							'entityMatByProduct': JSON.stringify($scope.inventoryMaterialListPOnew1),
						},
						dataType: 'JSON'
						, contentType: "application/json charset=utf-8"
					}).then(function (response) {
						if (response.data.Error === true) {
							ShowResult(response.data.Message, 'failure');
						}
						else {
							ShowResult(response.data.Message, 'success');
							$scope.ReceiptVA.Id = response.data.entity.Id;

						}
					}), function (response) {
						ShowResult(response.data.Message, 'failure');
					};
					//}



				}
				else if ($scope.Action === "Update") {
					if ($scope.ReceiptVAChildList.length > 0) {

						if ($scope.ReceiptVA.GRNDate > new Date()) {
							ShowResult("GRN Date  can not grather than Today's Date", 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.NoteForAccounts)) {
							ShowResult("Enter Note for accounts", 'failure');
							return false;
						}
						else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.ReceiptVA.CheckedBy)) {
							ShowResult("Please select to be approved by", 'failure');
							return false;
						}
						else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.ReceiptVA.CheckedBy)) {
							ShowResult("Please select to be checked by", 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.InvoicingPartyPlantId)) {
							return ShowResult('Invoicing by is required', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.DeliveryPartyPlantId)) {
							return ShowResult('Delivery by is required', 'failure');
							return false;
						}


						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.DocRefNo)) {
							return ShowResult('Enter Doc Ref No', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.DocDate)) {
							return ShowResult('Enter Doc Date', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.GateEntryNo)) {
							return ShowResult('Select Gate Entry No', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.GRNDate)) {
							return ShowResult('Enter GRN Date', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.CurrencyId)) {
							return ShowResult('Select Currency', 'failure');
							return false;
						}

						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.ResponsiblePerson)) {
							return ShowResult('Select By Whom', 'failure');
							return false;
						}

						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.MaterialStorageId)) {
							return ShowResult('Select Material Storage', 'failure');
							return false;
						}
						else if (baseService.isUndefinedOrNull($scope.ReceiptVA.NoteForAccounts)) {
							return ShowResult('Enter the Note For Accounts', 'failure');
							return false;
						}




						//else if (new Date($scope.ReceiptTransformation.EntryDate) < new Date($scope.ReceiptTransformation.DocDate)) {
						//	return manualValidation("Gate entry date can't be less than Doc Date", 'failure');
						//}

						//else if (new Date($scope.ReceiptTransformation.GRNDate) < new Date($scope.ReceiptTransformation.EntryDate)) {
						//	return manualValidation("GRN date can't be less than gate entry date", 'failure');

						//}
						else {
							for (var i = 0; i < $scope.ReceiptVAChildList.length; i++) {
								if ($scope.ReceiptVAChildList[i].check == true) {
									if (baseService.isUndefinedOrNull($scope.ReceiptVAChildList[i].MaterialStorageId)) {
										ShowResult("Please select storage location", 'failure');
										return false;
									}
									else if (baseService.isUndefinedOrNull($scope.ReceiptVAChildList[i].QualityStatus)) {
										ShowResult("Please select quality status", 'failure');
										return false;
									}
									else if ($scope.ReceiptVAChildList[i].TransactionQty > 0 && $scope.ReceiptVAChildList[i].check === false) {
										ShowResult("Please select the material", 'failure');
										return false;
									}
									else if ($scope.ReceiptVAChildList[i].TransactionQty === '0' && $scope.ReceiptVAChildList[i].check === true) {
										ShowResult("Enter the Qty", 'failure');
										return false;
									}
									else if (baseService.isUndefinedOrNull($scope.ReceiptVAChildList[i].TransactionQty) && $scope.ReceiptVAChildList[i].check === true) {
										ShowResult("Enter the Qty", 'failure');
										return false;
									}
									$scope.inventoryMaterialListPOnewValAdded.push($scope.ReceiptVAChildList[i]);

								}

							}
						}
					}

					//debugger;
					$http({
						method: 'POST',
						url: 'Products/GoodsReceiveNote/CreateJWGRN',
						data:
						{
							'entity': $scope.ReceiptVA,
							'entityMatAndImat': JSON.stringify($scope.inventoryMaterialListPOnewValAdded),
							'receiveTaxList': $scope.POMaterialTaxList,
							'chargesListPO': $scope.chargesListPOnew,
							'POServiceTaxList': $scope.POServiceTaxList,
						//	'GRNType': 'GRNBYPO',
							'GRNType': 'GRNBYOS',
							'AcceptanceId': $scope.AcceptanceId,
							'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
							'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti,
							'entityMatByProduct': JSON.stringify($scope.inventoryMaterialListPOnew1),
						},
						dataType: 'JSON'
						, contentType: "application/json charset=utf-8"
					}).then(function (response) {
						if (response.data.Error === true) {
							ShowResult(response.data.Message, 'failure');
						}
						else {
							ShowResult(response.data.Message, 'success');
							$scope.ReceiptVA.Id = response.data.entity.Id;
							$scope.ClearReceiptVAChildTab();

						}
					}), function (response) {
						ShowResult(response.data.Message, 'failure');
					};
					//}




				}

				// }
			} catch (e) {
				throw e;
			}
        }

	};


	$scope.calculateAmount = function (data, index) {
		if ($scope.ModelNew.TabType == "Transformation") {
			//if (!baseService.isUndefinedOrNull(data.StandardName)) {
				
				if ($scope.Action === 'Save') {
					//	data.TransactionRate = (data.GrossConsumption * data.TransactionQty) / data.TransactionQty;
					/*var MatTranRate = ((data.GrossConsumption) / data.PlanQuantity) / (parseFloat($scope.ReceiptTransformation.ToCurrencyRate));*/
					var MatTranRate = (data.GrossConsumption) / (data.InventoryIssueQty);
					data.TransactionRate = MatTranRate.toFixed(4);
				}
				$scope.PreBal = data.Balance;
				data.TrnAmount = (data.NetQty * data.TransactionRate).toFixed(2);//(data.TransactionQty * data.TransactionRate).toFixed(2);
				if (data.TrnAmount == 'NaN')
					data.TrnAmount = 0;
				data.TaxAmount = 0;
				data.BaseTaxAmount = 0;
				for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {

					$scope.inventoryMaterialList[i].Balance = '';
					var ToleranceQty = $scope.inventoryMaterialList[i].PlanQuantity * $scope.inventoryMaterialList[i].Tolerance / 100;
					var newpoQty = $scope.inventoryMaterialList[i].PlanQuantity + ToleranceQty;
					if ($scope.inventoryMaterialList[i].PlanQuantity < (parseFloat($scope.inventoryMaterialList[i].GRNRcvQty + $scope.inventoryMaterialList[i].TransactionQty).toFixed(2)) && (baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].Tolerance) || $scope.inventoryMaterialList[i].Tolerance === 0)) {
						$scope.inventoryMaterialList[i].TransactionQty = '';
						ShowResult('Current quantity can not grater than balance qty!', 'failure');
						return false;
					}

					else if (newpoQty < (parseFloat($scope.inventoryMaterialList[i].GRNRcvQty + $scope.inventoryMaterialList[i].TransactionQty).toFixed(2)) && (!baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].Tolerance) || $scope.inventoryMaterialList[i].Tolerance > 0)) {
						ShowResult('Current quantity can not grater than po qty and Tolerance qty!PO + Tolerance=' + newpoQty, 'failure');
						return false;
					}
					else if ($scope.inventoryMaterialList[i].ShortageQty > $scope.inventoryMaterialList[i].TransactionQty) {
						ShowResult('Shortage Qty quantity can not grater than current qty!', 'failure');
						return false;
					}
					else if ($scope.inventoryMaterialList[i].RejectionQty > $scope.inventoryMaterialList[i].TransactionQty) {
						ShowResult('Rejection Qty quantity can not grater than current qty!', 'failure');
						return false;
					}
					else {
						if ($scope.inventoryMaterialList[i].OSTransformationPODetailId == data.OSTransformationPODetailId) {
							$scope.inventoryMaterialList[i].TrnAmount = Math.round(data.TrnAmount * 100 + Number.EPSILON) / 100;
							$scope.inventoryMaterialList[i].Balance = ($scope.inventoryMaterialList[i].PlanQuantity - ($scope.inventoryMaterialList[i].GRNRcvQty + $scope.inventoryMaterialList[i].TransactionQty));
							$scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - ($scope.inventoryMaterialList[i].ShortageQty + $scope.inventoryMaterialList[i].RejectionQty));
							$scope.inventoryMaterialList[i].NetQty = ($scope.inventoryMaterialList[i].TransactionQty - $scope.inventoryMaterialList[i].ShortageQty);

						}
						else {
							$scope.inventoryMaterialList[i].Balance = ($scope.inventoryMaterialList[i].PlanQuantity - ($scope.inventoryMaterialList[i].GRNRcvQty + $scope.inventoryMaterialList[i].TransactionQty));
							$scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - ($scope.inventoryMaterialList[i].ShortageQty + $scope.inventoryMaterialList[i].RejectionQty));
							$scope.inventoryMaterialList[i].NetQty = ($scope.inventoryMaterialList[i].TransactionQty - $scope.inventoryMaterialList[i].ShortageQty);
						}
						if ($scope.ReceiptTransformation.IsNonCreditable == 1) {
							if ($scope.inventoryMaterialList[i].JWTCDetailId == data.JWTCDetailId) {
								$scope.inventoryMaterialList[i].TrnAmount = ($scope.inventoryMaterialList[i].NetQty * $scope.inventoryMaterialList[i].TransactionRate).toFixed(2);
								$scope.inventoryMaterialList[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat(data.ServiceTax)) * 100 + Number.EPSILON) / 100;
						//		$scope.inventoryMaterialList[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.ReceiptTransformation.ToCurrencyRate) * 100 + Number.EPSILON) / 100;
								$scope.inventoryMaterialList[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat(data.ServiceTax))) * 100 + Number.EPSILON) / 100;

							}
						}
						else {
							if ($scope.inventoryMaterialList[i].OSTransformationPODetailId == data.OSTransformationPODetailId) {
								$scope.inventoryMaterialList[i].TrnAmount = Math.round(($scope.inventoryMaterialList[i].NetQty * $scope.inventoryMaterialList[i].TransactionRate) * 100 + Number.EPSILON) / 100;
								$scope.inventoryMaterialList[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.ServiceCharge)) * 100 + Number.EPSILON) / 100;
						//		$scope.inventoryMaterialList[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.ServiceCharge)) * $scope.ReceiptTransformation.ToCurrencyRate) * 100 + Number.EPSILON) / 100;
								$scope.inventoryMaterialList[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.ServiceCharge))) * 100 + Number.EPSILON) / 100;
							}
						}
					}
				}
			//}

		}
		else {
					
				//if (!baseService.isUndefinedOrNull(data.StandardName)) {
					
					if ($scope.Action === 'Save') {
						
						var MatTranRate = (data.IssueRate);
					   	data.TransactionRate = MatTranRate.toFixed(4);

					}

					$scope.PreBal = data.Balance;
					data.TrnAmount = (data.NetQty * data.TransactionRate).toFixed(2);//(data.TransactionQty * data.TransactionRate).toFixed(2);
					if (data.TrnAmount == 'NaN')
						data.TrnAmount = 0;
					data.TaxAmount = 0;
					data.BaseTaxAmount = 0;
					for (var i = 0; i < $scope.ReceiptVAChildList.length; i++) {

						$scope.ReceiptVAChildList[i].Balance = '';
						var ToleranceQty = $scope.ReceiptVAChildList[i].PlanQuantity * $scope.ReceiptVAChildList[i].Tolerance / 100;
						var newpoQty = $scope.ReceiptVAChildList[i].PlanQuantity + ToleranceQty;
						if ($scope.ReceiptVAChildList[i].PlanQuantity < (parseFloat($scope.ReceiptVAChildList[i].GRNRcvQty + $scope.ReceiptVAChildList[i].TransactionQty).toFixed(2)) && (baseService.isUndefinedOrNull($scope.ReceiptVAChildList[i].Tolerance) || $scope.ReceiptVAChildList[i].Tolerance === 0)) {
							$scope.ReceiptVAChildList[i].TransactionQty = '';
							ShowResult('Current quantity can not grater than balance qty!', 'failure');
							return false;
						}

						else if (newpoQty < (parseFloat($scope.ReceiptVAChildList[i].GRNRcvQty + $scope.ReceiptVAChildList[i].TransactionQty).toFixed(2)) && (!baseService.isUndefinedOrNull($scope.ReceiptVAChildList[i].Tolerance) || $scope.ReceiptVAChildList[i].Tolerance > 0)) {
							ShowResult('Current quantity can not grater than po qty and Tolerance qty!PO + Tolerance=' + newpoQty, 'failure');
							return false;
						}
						else if ($scope.ReceiptVAChildList[i].ShortageQty > $scope.ReceiptVAChildList[i].TransactionQty) {
							ShowResult('Shortage Qty quantity can not grater than current qty!', 'failure');
							return false;
						}
						else if ($scope.ReceiptVAChildList[i].RejectionQty > $scope.ReceiptVAChildList[i].TransactionQty) {
							ShowResult('Rejection Qty quantity can not grater than current qty!', 'failure');
							return false;
						}
						else {
							if ($scope.ReceiptVAChildList[i].OSTransformationPODetailId == data.OSTransformationPODetailId) {
								$scope.ReceiptVAChildList[i].TrnAmount = Math.round(data.TrnAmount * 100 + Number.EPSILON) / 100;
								$scope.ReceiptVAChildList[i].Balance = ($scope.ReceiptVAChildList[i].PlanQuantity - ($scope.ReceiptVAChildList[i].GRNRcvQty + $scope.ReceiptVAChildList[i].TransactionQty));
								$scope.ReceiptVAChildList[i].ApprovedQty = ($scope.ReceiptVAChildList[i].TransactionQty - ($scope.ReceiptVAChildList[i].ShortageQty + $scope.ReceiptVAChildList[i].RejectionQty));
								$scope.ReceiptVAChildList[i].NetQty = ($scope.ReceiptVAChildList[i].TransactionQty - $scope.ReceiptVAChildList[i].ShortageQty);

							}
							else {
								$scope.ReceiptVAChildList[i].Balance = ($scope.ReceiptVAChildList[i].PlanQuantity - ($scope.ReceiptVAChildList[i].GRNRcvQty + $scope.ReceiptVAChildList[i].TransactionQty));
								$scope.ReceiptVAChildList[i].ApprovedQty = ($scope.ReceiptVAChildList[i].TransactionQty - ($scope.ReceiptVAChildList[i].ShortageQty + $scope.ReceiptVAChildList[i].RejectionQty));
								$scope.ReceiptVAChildList[i].NetQty = ($scope.ReceiptVAChildList[i].TransactionQty - $scope.ReceiptVAChildList[i].ShortageQty);
							}
							if ($scope.ReceiptVA.IsNonCreditable == 1) {
								if ($scope.ReceiptVAChildList[i].JWTCDetailId == data.JWTCDetailId) {
									$scope.ReceiptVAChildList[i].TrnAmount = ($scope.ReceiptVAChildList[i].NetQty * $scope.ReceiptVAChildList[i].TransactionRate).toFixed(2);
									$scope.ReceiptVAChildList[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.ReceiptVAChildList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.ReceiptVAChildList[i].ServiceCharge) + parseFloat(data.ServiceTax)) * 100 + Number.EPSILON) / 100;
								//	$scope.ReceiptVAChildList[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.ReceiptVAChildList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.ReceiptVAChildList[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.ReceiptVA.ToCurrencyRate) * 100 + Number.EPSILON) / 100;
									$scope.ReceiptVAChildList[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.ReceiptVAChildList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.ReceiptVAChildList[i].ServiceCharge) + parseFloat(data.ServiceTax))) * 100 + Number.EPSILON) / 100;
								}
							}
							else {
								if ($scope.ReceiptVAChildList[i].OSTransformationPODetailId == data.OSTransformationPODetailId) {
									$scope.ReceiptVAChildList[i].TrnAmount = Math.round(($scope.ReceiptVAChildList[i].NetQty * $scope.ReceiptVAChildList[i].TransactionRate) * 100 + Number.EPSILON) / 100;
									$scope.ReceiptVAChildList[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.ReceiptVAChildList[i].TrnAmount) + parseFloat(data.ServiceCharge)) * 100 + Number.EPSILON) / 100;
							//		$scope.ReceiptVAChildList[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.ReceiptVAChildList[i].TrnAmount) + parseFloat(data.ServiceCharge)) * $scope.ReceiptVA.ToCurrencyRate) * 100 + Number.EPSILON) / 100;
									$scope.ReceiptVAChildList[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.ReceiptVAChildList[i].TrnAmount) + parseFloat(data.ServiceCharge))) * 100 + Number.EPSILON) / 100;
								}
							}
						}
					}
				//}
		
        }
		
	};

	$scope.ValWOMatQty = function (data) {
		if (baseService.isUndefinedOrNull(data.StandardName)) {
			var Bal = parseFloat(data.PlanQuantity) - parseFloat(data.GRNRcvQty);
			if (parseFloat(data.TransactionQty) > Bal) {
				data.check = false;
				data.Balance = Bal.toFixed(4);
				ShowResult('Current Quantity cannot be greater than Balance quantity', 'failure');
				return false;
			}
			else {
				var BalRem = (parseFloat(data.PlanQuantity) - parseFloat(data.GRNRcvQty)) - parseFloat(data.TransactionQty);
				data.Balance = BalRem.toFixed(4);
            }
        }
    }

	$scope.calculateAmountByProduct = function (data, index) {
		if (!baseService.isUndefinedOrNull(data.StandardName)) {
		debugger;
		//data.TransactionRate = 1;// Need to remove
		$scope.PreBal = data.Balance;
		data.TrnAmount = (data.NetQty * data.TransactionRate).toFixed(2);//(data.TransactionQty * data.TransactionRate).toFixed(2);
		if (data.TrnAmount == 'NaN')
			data.TrnAmount = 0;
		data.TaxAmount = 0;
		data.BaseTaxAmount = 0;
		for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
			$scope.inventoryMaterialListPO[i].Balance = '';
			var ToleranceQty = $scope.inventoryMaterialListPO[i].PlanQuantity * $scope.inventoryMaterialListPO[i].Tolerance / 100;
			var newpoQty = $scope.inventoryMaterialListPO[i].PlanQuantity + ToleranceQty;
			if ($scope.inventoryMaterialListPO[i].PlanQuantity < (parseFloat($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2)) && (baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].Tolerance) || $scope.inventoryMaterialListPO[i].Tolerance === 0)) {
				$scope.inventoryMaterialListPO[i].TransactionQty = '';
				ShowResult('Current quantity can not grater than balance qty!', 'failure');
				return false;
			}

			else if (newpoQty < (parseFloat($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2)) && (!baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].Tolerance) || $scope.inventoryMaterialListPO[i].Tolerance > 0)) {
				ShowResult('Current quantity can not grater than po qty and Tolerance qty!PO + Tolerance=' + newpoQty, 'failure');
				return false;
			}
			else if ($scope.inventoryMaterialListPO[i].ShortageQty > $scope.inventoryMaterialListPO[i].TransactionQty) {
				ShowResult('Shortage Qty quantity can not grater than current qty!', 'failure');
				return false;
			}
			else if ($scope.inventoryMaterialListPO[i].RejectionQty > $scope.inventoryMaterialListPO[i].TransactionQty) {
				ShowResult('Rejection Qty quantity can not grater than current qty!', 'failure');
				return false;
			}
			else {
				if ($scope.inventoryMaterialListPO[i].JWTCDetailId == data.JWTCDetailId) {
					$scope.inventoryMaterialListPO[i].TrnAmount = Math.round(data.TrnAmount * 100 + Number.EPSILON) / 100;
					$scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].PlanQuantity - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
					$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
					$scope.inventoryMaterialListPO[i].NetQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].ShortageQty);

				}
				else {
					$scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].PlanQuantity - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
					$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
					$scope.inventoryMaterialListPO[i].NetQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].ShortageQty);
				}
				if ($scope.ReceiptTransformation.IsNonCreditable == 1) {
					if ($scope.inventoryMaterialListPO[i].JWTCDetailId == data.JWTCDetailId) {
						$scope.inventoryMaterialListPO[i].TrnAmount = ($scope.inventoryMaterialListPO[i].NetQty * $scope.inventoryMaterialListPO[i].TransactionRate).toFixed(2);
						$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)) * 100 + Number.EPSILON) / 100;
						$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.ReceiptTransformation.ToCurrencyRate) * 100 + Number.EPSILON) / 100;

					}
				}
				else {
					if ($scope.inventoryMaterialListPO[i].JWTCDetailId == data.JWTCDetailId) {
						$scope.inventoryMaterialListPO[i].TrnAmount = Math.round(($scope.inventoryMaterialListPO[i].NetQty * $scope.inventoryMaterialListPO[i].TransactionRate) * 100 + Number.EPSILON) / 100;
						$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.ServiceCharge)) * 100 + Number.EPSILON) / 100;
						$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.ServiceCharge)) * $scope.ReceiptTransformation.ToCurrencyRate) * 100 + Number.EPSILON) / 100;
					}
				}
			}
		}

	}
	};

	$scope.ValWOMatQtyBYProduct = function (data) {
		if (baseService.isUndefinedOrNull(data.StandardName)) {
			var Bal = parseFloat(data.PlanQuantity) - parseFloat(data.GRNRcvQty);
			if (parseFloat(data.TransactionQty) > Bal) {
				data.check = false;
				data.Balance = Bal.toFixed(4);
				ShowResult('Current Quantity cannot be greater than Balance quantity', 'failure');
				return false;
			}
			else {
				var BalRem = (parseFloat(data.PlanQuantity) - parseFloat(data.GRNRcvQty)) - parseFloat(data.TransactionQty);
				data.Balance = BalRem.toFixed(4);
			}
		}
	}

	// #region GRN-By-JW Index  All Tab 
	$scope.GRN = "";
	$scope.tab = 1;
	$scope.GRNbyPOCheckStatus = "ForChecked";
	$scope.setTabGRNList = function (newTab) {
		$scope.tab = newTab;
		$scope.GRNbyPOCheckStatus = "ForChecked";
		//$scope.getDataList();
		$scope.GetJWGRNDataChecking();

		//alert('Checked Unapproval');
	};
	$scope.isSetGRNList = function (tabNum) {
		return $scope.tab === tabNum;
		$scope.GRN = 1;

	};



	$scope.setTabCheckedHR = function (newTab) {
		$scope.tab = newTab;
		$scope.GRNbyPOCheckStatus = "CheckedHoldReject";
		$scope.GetJWGRNDataChecking();

	};
	$scope.isSetCheckedHR = function (tabNum) {
		return $scope.tab === tabNum;
		$scope.GRN = 2;
	};




	$scope.setTabNotApprovedChecked = function (newTab) {

		$scope.tab = newTab;
		$scope.GRNbyPOCheckStatus = "Checked";
		$scope.GetJWGRNDataChecking();

	};
	$scope.isSetNotApprovedChecked = function (tabNum) {
		return $scope.tab === tabNum;
		$scope.GRN = 3;
	};





	$scope.GRNbyPOApprovedStatus = "ApprovedHoldReject";
	$scope.setTabApprovedHR = function (newTab) {
		$scope.GRNbyPOApprovedStatus = "ApprovedHoldReject";
		$scope.tab = newTab;
		$scope.getalldataMaster2();

	};
	$scope.isSetApprovedHR = function (tabNum) {
		return $scope.tab === tabNum;
		$scope.GRN = 4;
	};


	$scope.setTabApprovedNP = function (newTab) {
		$scope.tab = newTab;
		$scope.GRNbyPOApprovedStatus = "Approved";
		$scope.getalldataMaster2();

	};
	$scope.isSetApprovedNP = function (tabNum) {
		return $scope.tab === tabNum;
		$scope.GRN = 5;
	};



	$scope.setTabPosted = function (newTab) {
		$scope.tab = newTab;
		$scope.GRNbyPOApprovedStatus = "Posted";
		$scope.getalldataMaster2();

	};
	$scope.isSetPosted = function (tabNum) {
		return $scope.tab === tabNum;
		$scope.GRN = 6;
	};

	// #endregion

	$scope.GRNbyPOCheckStatus = "ForChecked";
	$scope.GriddataMaster = [];
	$scope.GetJWGRNDataChecking = function () {
		if ($scope.ModelNew.TabType == "Transformation") {
			if ($scope.GRNbyPOCheckStatus === "ForChecked") {
				$scope.GRNbyPOCheckStatus = "ForChecked";
			}
			$http({
				method: "GET",
				dataType: 'JSON',
				//url: $scope.getSearchListUrl,
				url: 'Products/GoodsReceiveNote/GetJWGRNDataChecking?GRNbyPOCheckStatus=' + $scope.GRNbyPOCheckStatus + '&POId=' + $scope.Transformation.Id,
			}).then(function successCallback(response) {
				$scope.GriddataMaster = response.data;
				//entrydata = copy(searchdata);
				$scope.ShowHomeList = false;
				$scope.setTab(2);

				//if ($scope.GriddataMaster.length == 0) {
				//	$scope.ShowHomeList = true;
				//	$scope.setTab(2);
				//	if (!$rootScope.isCollapsed) {
				//		$rootScope.toggle();
				//	}
				//}
				//else {
				//	$scope.ShowHomeList = false;
				//	$scope.setTab(2);
				//}

			});
		}
		else {
			if ($scope.GRNbyPOCheckStatus === "ForChecked") {
				$scope.GRNbyPOCheckStatus = "ForChecked";
			}
			$http({
				method: "GET",
				dataType: 'JSON',
				//url: $scope.getSearchListUrl,
				url: 'Products/GoodsReceiveNote/GetJWGRNDataChecking?GRNbyPOCheckStatus=' + $scope.GRNbyPOCheckStatus + '&POId=' + $scope.ModelNew.Id,
			}).then(function successCallback(response) {
				$scope.GriddataMaster = response.data;
				//entrydata = copy(searchdata);
				$scope.ShowHomeList = false;
				$scope.setTab(1);
			});
        }

	};
	$scope.lst = [];
	$scope.GRNListDetails = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/GoodsReceiveNote/JWGRNDetailsData'
		}).then(function successCallback(response) {
			$scope.lst = response.data;
			//$scope.detailgrid($scope.lst);
			window.lst = response.data;

		});
	}
	$scope.GRNListDetails();

	$scope.detailTemp = "#tabGridContents";
	//$scope.detailgrid = "detailGridData(e)";
	$scope.detailgrid = function detailGridData(e) {
		//debugger;

		var filteredData = e.data["Id"];
		var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("InventoryReceiveId", "equal", parseInt(filteredData), true).take(100));
		e.detailsElement.find("#detailGrid").ejGrid({
			dataSource: data,
			columns: ["MaterialGroupName", "MaterialName", "Article", "SKU1", "SKU2", "SKU3", "MaterialDetail", "TransactionQty", "TransactionUoMId", "TransactionUoM", "TransactionRate", "CurrencyName", "TotalMaterialTranAmount", "MaterialFor"]
		});
		e.detailsElement.find(".tabcontrol").ejTab();


	}

	$scope.GriddataMaster2 = [];
	$scope.getalldataMaster2 = function () {


		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/GoodsReceiveNote/GetJWApproving?GRNbyPOApprovedStatus=' + $scope.GRNbyPOApprovedStatus,
		}).then(function successCallback(response) {
			$scope.GriddataMaster2 = response.data;
		});
	};
	$scope.baseCurrencyIdLoad = function () {
		$http({
			method: 'GET',
			url: 'currencies/CompanyParallelCurrency/CboParallelCurrency'
		}).then(function successCallback(response) {
			$scope.baseCurrencyId = response.data[0].Value;
			$scope.ReceiptTransformation.BaseCurrencyId = response.data[0].Value;
			factoryService.getCurrencyPrecision($scope.baseCurrencyId);
		});
	}
	$scope.baseCurrencyIdLoad();
	$scope.calculateAmountAfterDiscount = function (data, index) {
		debugger;
		data.TransactionRate = 1;// Need to remove
		$scope.PreBal = data.Balance;
		data.TrnAmount = (data.NetQty * data.TransactionRate).toFixed(2);
		if (data.TrnAmount == 'NaN')
			data.TrnAmount = 0;
		for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
			$scope.inventoryMaterialListPO[i].Balance = '';
			if ($scope.inventoryMaterialListPO[i].PlanQuantity < (parseFloat($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2))) {
				$scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - $scope.inventoryMaterialListPO[i].GRNRcvQty;
				ShowResult('Current quantity can not grater than balance qty!', 'failure');
			}
			else if ($scope.inventoryMaterialListPO[i].ShortageQty > $scope.inventoryMaterialListPO[i].TransactionQty) {
				ShowResult('Shortage Qty quantity can not grater than current qty!', 'failure');
			}
			else if ($scope.inventoryMaterialListPO[i].RejectionQty > $scope.inventoryMaterialListPO[i].TransactionQty) {
				ShowResult('Rejection Qty quantity can not grater than current qty!', 'failure');
			}
			else if ($scope.inventoryMaterialListPO[i].DiscountAmount > $scope.inventoryMaterialListPO[i].TrnAmount) {
				ShowResult('Discount Amount  can not grater than Material Amount!', 'failure');
			}
			else {
				if ($scope.inventoryMaterialListPO[i].OSTransformationPOByProductId == data.OSTransformationPOByProductId) {
					$scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;
					$scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
					$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
					$scope.inventoryMaterialListPO[i].NetQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].ShortageQty);

				}
				else {
					$scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
					$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
					$scope.inventoryMaterialListPO[i].NetQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].ShortageQty);
				}
				if ($scope.ReceiptTransformation.IsNonCreditable == 1) {
					if ($scope.inventoryMaterialListPO[i].OSTransformationPOByProductId == data.OSTransformationPOByProductId) {
						$scope.inventoryMaterialListPO[i].TrnAmount = (($scope.inventoryMaterialListPO[i].NetQty * $scope.inventoryMaterialListPO[i].TransactionRate) - data.DiscountAmount).toFixed(2);
						$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)).toFixed(2);
						$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.ReceiptTransformation.ToCurrencyRate).toFixed(2);

					}
				}
				else {
					if ($scope.inventoryMaterialListPO[i].OSTransformationPOByProductId == data.OSTransformationPOByProductId) {
						$scope.inventoryMaterialListPO[i].TrnAmount = (($scope.inventoryMaterialListPO[i].NetQty * $scope.inventoryMaterialListPO[i].TransactionRate) - data.DiscountAmount).toFixed(2);
						$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.ServiceCharge)).toFixed(2);
						$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.ServiceCharge)) * $scope.ReceiptTransformation.ToCurrencyRate).toFixed(2);
					}
				}
			}
		}


	};
	$scope.calculateAmountAfterDiscount1 = function (data, index) {
		debugger;
		data.TransactionRate = (data.GrossConsumption * data.TransactionQty) / data.TransactionQty;
		$scope.PreBal = data.Balance;
		data.TrnAmount = (data.NetQty * data.TransactionRate).toFixed(2);
		if (data.TrnAmount == 'NaN')
			data.TrnAmount = 0;
		for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
			$scope.inventoryMaterialList[i].Balance = '';
			if ($scope.inventoryMaterialList[i].PlanQuantity < (parseFloat($scope.inventoryMaterialList[i].GRNRcvQty + $scope.inventoryMaterialList[i].TransactionQty).toFixed(2))) {
				$scope.inventoryMaterialList[i].Balance = $scope.inventoryMaterialList[i].POQty - $scope.inventoryMaterialList[i].GRNRcvQty;
				ShowResult('Current quantity can not grater than balance qty!', 'failure');
			}
			else if ($scope.inventoryMaterialList[i].ShortageQty > $scope.inventoryMaterialList[i].TransactionQty) {
				ShowResult('Shortage Qty quantity can not grater than current qty!', 'failure');
			}
			else if ($scope.inventoryMaterialList[i].RejectionQty > $scope.inventoryMaterialList[i].TransactionQty) {
				ShowResult('Rejection Qty quantity can not grater than current qty!', 'failure');
			}
			else if ($scope.inventoryMaterialList[i].DiscountAmount > $scope.inventoryMaterialList[i].TrnAmount) {
				ShowResult('Discount Amount  can not grater than Material Amount!', 'failure');
			}
			else {
				if ($scope.inventoryMaterialList[i].OSTransformationPODetailId == data.OSTransformationPODetailId) {
					$scope.inventoryMaterialList[i].TrnAmount = data.TrnAmount;
					$scope.inventoryMaterialList[i].Balance = ($scope.inventoryMaterialList[i].POQty - ($scope.inventoryMaterialList[i].GRNRcvQty + $scope.inventoryMaterialList[i].TransactionQty));
					$scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - ($scope.inventoryMaterialList[i].ShortageQty + $scope.inventoryMaterialList[i].RejectionQty));
					$scope.inventoryMaterialList[i].NetQty = ($scope.inventoryMaterialList[i].TransactionQty - $scope.inventoryMaterialList[i].ShortageQty);

				}
				else {
					$scope.inventoryMaterialList[i].Balance = ($scope.inventoryMaterialList[i].POQty - ($scope.inventoryMaterialList[i].GRNRcvQty + $scope.inventoryMaterialList[i].TransactionQty));
					$scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - ($scope.inventoryMaterialList[i].ShortageQty + $scope.inventoryMaterialList[i].RejectionQty));
					$scope.inventoryMaterialList[i].NetQty = ($scope.inventoryMaterialList[i].TransactionQty - $scope.inventoryMaterialList[i].ShortageQty);
				}
				if ($scope.ReceiptTransformation.IsNonCreditable == 1) {
					if ($scope.inventoryMaterialList[i].OSTransformationPODetailId == data.OSTransformationPODetailId) {
						$scope.inventoryMaterialList[i].TrnAmount = (($scope.inventoryMaterialList[i].NetQty * $scope.inventoryMaterialList[i].TransactionRate) - data.DiscountAmount).toFixed(2);
						$scope.inventoryMaterialList[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat(data.ServiceTax)).toFixed(2);
						$scope.inventoryMaterialList[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.ReceiptTransformation.ToCurrencyRate).toFixed(2);

					}
				}
				else {
					if ($scope.inventoryMaterialList[i].OSTransformationPODetailId == data.OSTransformationPODetailId) {
						$scope.inventoryMaterialList[i].TrnAmount = (($scope.inventoryMaterialList[i].NetQty * $scope.inventoryMaterialList[i].TransactionRate) - data.DiscountAmount).toFixed(2);
						$scope.inventoryMaterialList[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.ServiceCharge)).toFixed(2);
						$scope.inventoryMaterialList[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.ServiceCharge)) * $scope.ReceiptTransformation.ToCurrencyRate).toFixed(2);
					}
				}
			}
		}


	};



	$scope.recorddoubleclickFromMasterGrid = function ($event) {
		//debugger;
		if ($scope.ModelNew.TabType == "Transformation") {
			var x = $event;
			var Id = x.data.Id;
			$scope.Action = 'Update';
			//ClearFields();		
			$scope.ReceiptTransformation = x.data;
			JWOutPutQuery(Id);
			JWByProductQuery(Id);
			if (baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
				$scope.CheckedByStatusForNoti = false;
				$scope.ApprovedByStatusForNoti = true;
				$scope.ReceiptTransformation.CheckedBy = x.data.ApprovedById;
			}
			else if (!baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
				$scope.CheckedByStatusForNoti = true;
				$scope.ApprovedByStatusForNoti = true;
				$scope.ReceiptTransformation.CheckedBy = x.data.CheckedById;
			}
			//$scope.GetCheckedByAndApprovedBy1();
			if (baseService.isUndefinedOrNull(x.data.CheckedById) && !baseService.isUndefinedOrNull(x.data.ApprovedById)) {

				$scope.ReceiptTransformation.CheckedBy = x.data.ApprovedById;
				$scope.ReceiptTransformation.labelCheckAndApproved = 'To be approved by';
			}
			else if (!baseService.isUndefinedOrNull(x.data.CheckedById) && baseService.isUndefinedOrNull(x.data.ApprovedById)) {

				$scope.ReceiptTransformation.CheckedBy = x.data.CheckedById;
				$scope.ReceiptTransformation.labelCheckAndApproved = 'To be checked by';
			}
			if (!$rootScope.isCollapsed) $rootScope.toggle();
		}
		else {
			var x = $event;
			var Id = x.data.Id;
			$scope.Action = 'Update';
			//ClearFields();		
			$scope.ReceiptVA = x.data;
			$scope.ReceiptVA.ByWhomEmployeeId = x.data.ByWhomEmployeeId;
			$scope.ReceiptVA.EmployeeCode = x.data.EmpCode;
			$scope.ReceiptVA.ResponsiblePerson = x.data.ByWhomName;
			$scope.ReceiptVA.CurrencyId = x.data.CurrencyId;
			$scope.GetTransformationReceiptCurrency();
			JWOutPutQuery(Id);

			if (baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
				$scope.CheckedByStatusForNoti = false;
				$scope.ApprovedByStatusForNoti = true;
				$scope.ReceiptVA.CheckedBy = x.data.ApprovedById;
			}
			else if (!baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
				$scope.CheckedByStatusForNoti = true;
				$scope.ApprovedByStatusForNoti = true;
				$scope.ReceiptVA.CheckedBy = x.data.CheckedById;
			}
			//$scope.GetCheckedByAndApprovedBy1();
			if (baseService.isUndefinedOrNull(x.data.CheckedById) && !baseService.isUndefinedOrNull(x.data.ApprovedById)) {

				$scope.ReceiptVA.CheckedBy = x.data.ApprovedById;
				$scope.ReceiptVA.labelCheckAndApproved = 'To be approved by';
			}
			else if (!baseService.isUndefinedOrNull(x.data.CheckedById) && baseService.isUndefinedOrNull(x.data.ApprovedById)) {

				$scope.ReceiptVA.CheckedBy = x.data.CheckedById;
				$scope.ReceiptVA.labelCheckAndApproved = 'To be checked by';
			}
			if (!$rootScope.isCollapsed) $rootScope.toggle();
        }

	}

	function JWOutPutQuery(inveReveiveId) {
		if ($scope.ModelNew.TabType == "Transformation") {
			$scope.masterId5 = inveReveiveId;
			$scope.inventoryMaterialList = [];
			$http({
				method: 'GET',
				url: 'Products/GoodsReceiveNote/GetJWOutPutInventoryMaterialList?inveReveiveId=' + inveReveiveId
			}).then(function successCallback(response) {
				$scope.inventoryMaterialList = response.data;
				if ($scope.inventoryMaterialList.length > 0) {
					for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
						$scope.inventoryMaterialList[i].check = true;
					}

				}
			});
		}
		else {
			$scope.masterId5 = inveReveiveId;
			$scope.ReceiptVAChildList = [];
			$http({
				method: 'GET',
				url: 'Products/GoodsReceiveNote/GetJWOutPutInventoryMaterialList?inveReveiveId=' + inveReveiveId
			}).then(function successCallback(response) {
				$scope.ReceiptVAChildList = response.data;
				if ($scope.ReceiptVAChildList.length > 0) {
					for (var i = 0; i < $scope.ReceiptVAChildList.length; i++) {
						$scope.ReceiptVAChildList[i].check = true;
                    }
					
                }
			});
        }

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
			if ($scope.inventoryMaterialListPO.length > 0) {
				for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
					$scope.inventoryMaterialListPO[i].check = true;
				}

			}
		});
	}
	//$scope.AllTabPrint = function (z) {
	//	//debugger;
	//	var x = "#" + z;
	//	var gridObj = $(x).data("ejGrid");
	//	var data = gridObj.getSelectedRecords()[0];
	//	location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;
	//};

	$scope.valuePassInDelModal = function (id) {
		$scope.id = id;
		$scope.message = 'Are you sure want to permanently delete this?';
		angular.element(document.querySelector('#removerPopUp')).modal('show');
	};

	$scope.detailDelete = function () {
		try {
			$http({
				method: 'POST',
				url: 'Products/GoodsReceiveNote/JWDetailDelete?receiveDetailId=' + $scope.id
				//url: $scope.detailDeleteUrl + $scope.id
			}).then(function successCallback(response) {
				if (response.data.Error === true)
					ShowResult(response.data.Message, 'failure');
				else {
					ShowResult(response.data.Message, 'success');					
					JWOutPutQuery($scope.ReceiptTransformation.Id)
					JWByProductQuery($scope.ReceiptTransformation.Id)
					
				}
			}), function errorCallBack(response) {
				ShowResult(response.data.Message, 'failure');
			};
		} catch (e) {
			ShowResult(e, 'success');
		}
	};

	$scope.Delete = function () {
		//debugger;
		if (baseService.arrayLength($scope.inventoryMaterialList) === 0 && baseService.arrayLength($scope.inventoryMaterialListPO) === 0) {
			if (!baseService.isUndefinedOrNull($scope.ReceiptTransformation.Id)) {
				$http({
					method: 'POST',
					url: 'Products/GoodsReceiveNote/JWDeleteGRN?Id=' + $scope.ReceiptTransformation.Id,
					//url: $scope.deleteUrl + $scope.productNew.Id,//deleteGRNBYPO
					dataType: 'JSON'
				}).then(function (response) {
					if (response.data.Error === true)
						ShowResult(response.data.Message, 'failure');
					else {
						ShowResult('Data Deleted Successfully', 'success');
						//$scope.getDataList();
						$scope.ClearReceiptTransChildTab();
					}
					function errorCallBack(response) {
						ShowResult(response.data.Message, 'failure');
					}
				});
			}
		}
		else
			ShowResult('First delete all line item.', 'failure');
	};
	// end

	// GET If Material Issue Or Not

	$scope.GetMaterialInputList = [];
	$scope.GetIssuedMatInputList = [];
	$scope.GetIfIssuedOrNot = function (x) {
		if ($scope.ModelNew.TabType == "Transformation") {
		//	if (!baseService.isUndefinedOrNull(x.StandardName)) {

			$scope.JWPOId = x.OSTransformationPOId;
			$scope.JWOutputId = x.OSTransformationPODetailId;

				$scope.GetMaterialInputList = [];
				$http({
					method: 'GET',
					url: $scope.path + 'GetIfIssuedOrNot?JWOutputId=' + $scope.JWOutputId
				}).then(function successCallback(response) {
					$scope.GetMaterialInputList = response.data;
					if ($scope.GetMaterialInputList.length > 0) {
						//	var MatInputLength = $scope.GetMaterialInputList.length;

						$scope.GetIssuedMatInputList = [];
						$http({
							method: 'GET',
							url: $scope.path + 'GetIssuedMatInputList?JWPOId=' + $scope.JWPOId + '&JWOutputId=' + $scope.JWOutputId
						}).then(function successCallback(response) {
							$scope.GetIssuedMatInputList = response.data;
							if ($scope.GetIssuedMatInputList.length > 0) {
								//		var IssuedMatInputCount = $scope.GetIssuedMatInputList.length;

								for (var i = 0; i < $scope.GetMaterialInputList.length; i++) {
									if (!baseService.isUndefinedOrNull($scope.GetMaterialInputList[i].ArticleId)) {
										var getjwiRow = $filter("filter")($scope.GetIssuedMatInputList, { "JWTCInputId": $scope.GetMaterialInputList[i].JobWorkItemId });
										if (getjwiRow.length == 0) {
											x.check = false;
											ShowResult("This Output detail Id " + $scope.JWOutputId + " cannot be received ");
											return false;
										}
										else {
											var a = getjwiRow[0].QtyForOutput;

										}
									}
									//else {
									//	var getRow = $filter("filter")($scope.GetIssuedMatInputList, { "ArticleId": $scope.GetMaterialInputList[i].ArticleId });
									//	if (getRow.length == 0) {
									//		x.check = false;
									//		ShowResult("This Output detail Id " + $scope.JWOutputId + " Material " + x.UserName + " Article " + x.StandardName + " cannot be received ");
									//		return false;
									//	}
									//	else {
									//		var a = getRow[0].QtyForOutput;

									//	}
         //                           }
					
								}
								if (!baseService.isUndefinedOrNull(a)) {
									for (var b = 0; b < $scope.GetIssuedMatInputList.length; b++) {
										if (a < $scope.GetIssuedMatInputList[b].QtyForOutput) {
											var MinVal = a;
										}
										else {
											var MinVal = $scope.GetIssuedMatInputList[b].QtyForOutput;
										}
									}
								}


								if (x.TransactionQty > MinVal) {
									x.check = false;
									ShowResult("The Transaction quantity " + x.TransactionQty + " cannot be greater than " + MinVal + " ");
									return false;
								}
							}
							else {
								x.check = false;
								ShowResult("This Output detail Id " + $scope.JWOutputId + " Material " + x.UserName + " Article " + x.StandardName + " cannot be received ");
								return false;
							}
						});

					}
				});

		//	}
		}
		else {
		//	if (!baseService.isUndefinedOrNull(x.StandardName)) {

			$scope.JWPOId = x.OSTransformationPOId;
			$scope.JWOutputId = x.OSTransformationPODetailId;

				$scope.GetMaterialInputList = [];
				$http({
					method: 'GET',
					url: $scope.path + 'GetIfIssuedOrNotValAdded?JWPOId=' + $scope.JWPOId + '&JWOutputId=' + $scope.JWOutputId
				}).then(function successCallback(response) {
					$scope.GetMaterialInputList = response.data;
					if ($scope.GetMaterialInputList.length > 0) {
						//	var MatInputLength = $scope.GetMaterialInputList.length;

						$scope.GetIssuedMatInputList = [];
						$http({
							method: 'GET',
							url: $scope.path + 'GetIssuedMatInputListValAdded?JWPOId=' + $scope.JWPOId + '&JWOutputId=' + $scope.JWOutputId
						}).then(function successCallback(response) {
							$scope.GetIssuedMatInputList = response.data;
							if ($scope.GetIssuedMatInputList.length > 0) {
								//		var IssuedMatInputCount = $scope.GetIssuedMatInputList.length;

								for (var i = 0; i < $scope.GetMaterialInputList.length; i++) {
									if (baseService.isUndefinedOrNull($scope.GetMaterialInputList[i].ArticleId)) {
										var getjwiRow = $filter("filter")($scope.GetIssuedMatInputList, { "JWOrderWiseId": $scope.GetMaterialInputList[i].JWOrderWiseId });
										if (getjwiRow.length == 0) {
											x.check = false;
											ShowResult("This Output detail Id " + $scope.JWOutputId + " cannot be received ");
											return false;
										}
										else {
											var a = getjwiRow[0].QtyForOutput;

										}
									}
									else {
										var getRow = $filter("filter")($scope.GetIssuedMatInputList, { "ArticleId": $scope.GetMaterialInputList[i].ArticleId });
										if (getRow.length == 0) {
											x.check = false;
											ShowResult("This Output detail Id " + $scope.JWOutputId + " Material " + x.UserName + " Article " + x.StandardName + " cannot be received ");
											return false;
										}
										else {
											var a = getRow[0].QtyForOutput;

										}
									}
								}
								if (!baseService.isUndefinedOrNull(a)) {
									for (var b = 0; b < $scope.GetIssuedMatInputList.length; b++) {
										if (a < $scope.GetIssuedMatInputList[b].QtyForOutput) {
											var MinVal = a;
										}
										else {
											var MinVal = $scope.GetIssuedMatInputList[b].QtyForOutput;
										}
									}
								}


								if (x.TransactionQty > MinVal) {
									x.check = false;
									ShowResult("The Transaction quantity " + x.TransactionQty + " cannot be greater than " + MinVal + " ");
									return false;
								}
							}
							else {
								x.check = false;
								ShowResult("This Output detail Id " + $scope.JWOutputId + " Material " + x.UserName + " Article " + x.StandardName + " cannot be received ");
								return false;
							}
						});

					}
				});

	//		}
        }
    }

}