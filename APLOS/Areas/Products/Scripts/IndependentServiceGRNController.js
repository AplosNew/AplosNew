'use strict';
IndependentServiceGRNController.$inject = ['accountService', 'addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller', 'fileReader'];
function IndependentServiceGRNController(accountService, addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, fileReader) {
	$rootScope.title = "Service GRN";
	$scope.Action = 'Save';
	$scope.Action1 = 'Save';
	$scope.index = -1;
	$scope.products = [];
	$scope.path = 'Products/PurchaseOrder/';
	$scope.getListUrl = $scope.path + 'getlist';
	$scope.saveUrl = $scope.path + 'CreateIndependentServiceAcknowledge';
	$scope.detailSaveUrl = $scope.path + 'CreateServicePODetailByReq';
	$scope.detailUpdateUrl = $scope.path + 'DetailUpdatePOByReq';
	$scope.saveUrlFG = $scope.path + 'CreateFGMasterOrder';
	$scope.updateUrl = $scope.path + 'EditServicePOByReq';
	$scope.updateUrlFG = $scope.path + 'FGMasterOrderedit';
	$scope.deleteUrl = $scope.path + 'DeleteServicePOByReq/';
	$scope.detailDeleteUrl = $scope.path + 'DetailDeletePOByReq?receiveDetailId=';
	$scope.servicePOdetailDeleteUrl = $scope.path + 'ServicePODetailDelete?SPODetailid=';
	$scope.sreviceSaveUrl = $scope.path + 'ServiceChargesCreatePOByReq';
	$scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';
	$scope.detailSaveUrlIndependent = $scope.path + 'CreateServicePODetail';

	$scope.PurchaseOrderFileLocation = virtualPath.ServicePO;
	$scope.partyType = 'Vendor';
	$scope.isAdvance = false;
	$scope.currentDate = new Date(Date.now());
	$scope.grossTotal = 0;
	$scope.PartyId = null;
	$controller('partyBaseController', { $scope: $scope, $http: $http });
	$controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
	$scope.inventoryMaterialList = [];
	$scope.chargesList = [];
	$scope.ChargeTaxList = [];
	$scope.StateData = [];




	//#region notification setting for Service Requisition

	$scope.NotificationSettingStatus = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/PurchaseOrder/ServicePORequisitionNotificationSetting',
			dataType: 'JSON'
		}).then(function successCallback(response) {
			$scope.NotificationSetting = response.data;
			$scope.CheckedByStatusForNoti = $scope.NotificationSetting[0].RequiredChecking;
			$scope.ApprovedByStatusForNoti = $scope.NotificationSetting[0].RequiredApproval;
			$scope.GetCheckedByAndApprovedBy1();
			if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === false) {
				$scope.productNew.labelCheckAndApproved = 'To be checked by';
			}
			else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true) {
				$scope.productNew.labelCheckAndApproved = 'To be approved by';
			}
			else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true) {
				$scope.productNew.labelCheckAndApproved = 'To be checked by';
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
				url: 'Products/PurchaseOrder/GetCheckedByAndApprovedBYServicePORequisition?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
				dataType: 'JSON'
			}).then(function successCallback(response) {
				$scope.checkedByList = response.data;
			});

		}
		else {

		}

	}

	//#endregion
	$scope.AllTabPrint = function (z) {
		//debugger;
		var x = "#" + z;
		var gridObj = $(x).data("ejGrid");
		var data = gridObj.getSelectedRecords()[0];
		location.href = "Products/PurchaseOrder/ServicePurchaseOrderReport?purchaseOrderId=" + data.Id;
	};
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
	//#region  Req Detail
	$scope.lst = [];
	$scope.POListDetails = function () {

		$http({
			method: 'GET',
			//url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
			url: 'Products/PurchaseOrder/LoadServicePoDetails'
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
			url: 'Products/PurchaseOrder/ServicePODocumentMapDataAll'
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
		var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("ServicePOMasterId", "equal", parseInt(filteredData), true).take(1000));
		e.detailsElement.find("#detailGrid").ejGrid({
			dataSource: data,
			columns: ["Id", "ServiceName","Qty","UoM","Rate", "Amount", "TotalTaxAmount"]
		});
		e.detailsElement.find(".tabcontrol").ejTab();
		var dataImg = ej.DataManager(window.Img).executeLocal(ej.Query().where("ServicePOMasterId", "equal", parseInt(filteredData), true).take(1000));
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
	$scope.storageList = [];
	$http({
		method: 'GET',
		url: 'Materials/MaterialStorage/getcbo'
	}).then(function (response) {
		$scope.storageList = response.data;
	});
	$scope.currencyList = [];

	$scope.product = {
		Id: null
		, GRNDate: null
		, CompanyGroupId: null
		, CompanyId: null
		, PlantId: $window.plantId
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
		, BaseCurrencyId: $scope.baseCurrencyId
		, ToCurrencyRate: 0

		, PaymentTermId: null
		, BaseOnDueDate: null
		, BaseNoOfDays: null
		, MatureDate: null

		, DocRefNo: null
		, DocDate: null
		, GateEntryNo: null
		, EntryDate: null
		, FixedAssetOrInventory: 'Inventory'
		, PODepended: false
		, AlongwithInvoice: true
		, InvoiceNo: null
		, InvoiceDate: null
		, IsNonCreditable: false
		, TaxApplicable: null
		, IsTaxApplicable: false
		, IsTaxApplicableChangeable: false
		, PartyType: $scope.partyType
		, IsClosed: false
		, DeliveryInstruction: null
		, SpecialInstruction: null
		, CheckedBy: null
		, ApprovedBy: null
		, CheckedByStatus: null
		, ApprovedByStatus: null
		, labelCheckAndApproved: null
		, PODate: $filter("dateFiltering")(Date.now())
		, ContractId: null
		, OrderSpecific: 'No'
		, PurchaseLCId: null
		, CustomerName: null
		, PaymentMode: null
		, ContractNo: null
		, LCRef: null
		, CheckedByStatusForNoti: null
		, ApprovedByStatusForNoti: null
		, TaxOption: 'Yes'
		, TaxOptionMat: 'Yes'
		, TaxOptionService: 'Yes'
		, TaxOptionServiceModify: 'Yes'
		, ServiceType: 'ServiceACK'
		, EmployeeId:null
		
	};

	$scope.productNew = Object.assign({}, $scope.product);
	$scope.productDocMap = {
		Id: null
		, CompanyGroupId: null
		, FileName: null
		, UserFilename: null
		, SystemFileName: null
		, Description: null
		, Remarks: null
	};
	addressService.getCountryCbo(function (result) {
		$scope.countryList = result;
	});

	cboService.getEnumCbo("enum/GetPOApprovalStatusCbo", function (result) {
		$scope.POApprovalList = result;
	});

	cboService.getEnumCbo("enum/GetCheckedStatusCbo", function (result) {
		$scope.approvalStatusList = result;
	});

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
		// $scope.Action = 'Update';
		if (!$rootScope.isCollapsed) $rootScope.toggle();
	};

	function GetMasterData() {
		var aa = $("#masterId").text();
		$http.get('Products/PurchaseOrder/GetPOMasterById?id=' + aa).then(function (response) {
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
		//$scope.Action = 'Update';
		if (!$rootScope.isCollapsed) $rootScope.toggle();
	};

	//#region Index tab and dataloadfunction


	$scope.tabType = "ForChecking";
	$scope.GriddataMaster = [];
	$scope.getalldataMaster = function () {
		if ($scope.tabType === "ForChecking") {
			$scope.tabType = "ForChecking";
		}
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/GetListServiceAcknowledgementData?tabType=' + $scope.tabType,
		}).then(function successCallback(response) {
			// url: $scope.getListUrl1,
			$scope.GriddataMaster = response.data;
			//entrydata = copy(searchdata);
		});
	};
	$scope.getalldataMaster();



	$scope.GriddataMaster2 = [];
	$scope.getalldataMaster2 = function () {
		//debugger;
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/GoodsReceiveNote/GetListForGrnByPoReq?GRNWithReqPOApprovedStatus=' + $scope.GRNWithReqPOApprovedStatus,
			// url: $scope.getListUrl2,
		}).then(function successCallback(response) {
			$scope.GriddataMaster2 = response.data;

		});
	};



	$scope.GRN = "";
	//$scope.tab = 1;
	$scope.tabGL = 1;
	//debugger;
	$scope.tabType = "ForChecking";
	$scope.setTabGRNList = function (newTab) {

		$scope.tabType = "ForChecking";
		$scope.getalldataMaster();
		$scope.tabGL = newTab;
	};
	$scope.isSetGRNList = function (tabNum) {
		return $scope.tabGL === tabNum;
		//$scope.GRN = 1;

	};
	$scope.setTabCheckedHoldReject = function (newTab) {
		$scope.tabGL = newTab;
		$scope.tabType = "CheckedHoldReject";
		$scope.getalldataMaster();

	};
	$scope.isSetCheckedHoldReject = function (tabNum) {
		return $scope.tabGL === tabNum;
		$scope.GRN = 2;

	};
	$scope.setTabNotApprovedChecked = function (newTab) {
		$scope.tabGL = newTab;
		$scope.tabType = "Checked";
		$scope.getalldataMaster();

	};
	$scope.isSetNotApprovedChecked = function (tabNum) {
		return $scope.tabGL === tabNum;
		$scope.GRN = 3;

	};

	$scope.setTabApprovedHoldReject = function (newTab) {
		$scope.tabGL = newTab;
		$scope.tabType = "ApprovedHoldReject";
		$scope.getalldataMaster();

	};
	$scope.isSetApprovedHoldReject = function (tabNum) {
		return $scope.tabGL === tabNum;
		$scope.GRN = 4;

	};



	$scope.setTabApprovedNotPosted = function (newTab) {
		$scope.tabGL = newTab;
		$scope.tabType = "Approved";
		$scope.getalldataMaster();

	};
	$scope.isSetApprovedNotPosted = function (tabNum) {
		return $scope.tabGL === tabNum;
		$scope.GRN = 5;

	};


	$scope.setTabPosted = function (newTab) {
		$scope.tabGL = newTab;
		$scope.tabType = "Posted";
		$scope.getalldataMaster();

	};
	$scope.isSetPosted = function (tabNum) {
		return $scope.tabGL === tabNum;
		$scope.GRN = 6;

	};

	$scope.Save = function () {
		try {
			if ($scope.productNew.NoteForAccounts === '' || $scope.productNew.NoteForAccounts === null || $scope.productNew.NoteForAccounts === undefined) {
				ShowResult("Enter Note for accounts", 'failure');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.productNew.InvoicingPartyPlantId)) return ShowResult('Invoicing by is required', 'failure');
			if (baseService.isUndefinedOrNull($scope.productNew.DeliveryPartyPlantId)) return ShowResult('Delivery by is required', 'failure');
			$scope.modelValidation('div_docNo', 'productNew', 'DocRefNo');
			$scope.modelValidation('div_docDate', 'productNew', 'DocDate');
			if ($scope.Action === 'Update')
				$scope.modelValidation('div_grnNo', 'productNew', 'Id');
			$scope.manualValidationAddRemove('div_currency', 'productNew', 'CurrencyId');

			if ($scope.productNew.CurrencyId !== $scope.productNew.BaseCurrencyId)
				$scope.manualValidationAddRemove('div_rate  ', 'productNew', 'ToCurrencyRate');
			else
				manualValidation('div_rate', false);

			$scope.$broadcast('show-errors-check-validity');
			if ($scope.productNewForm.$valid) {
				$scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
				$scope.product = Object.assign({}, $scope.productNew);
				$scope.product.POId = $scope.POId;
				// $scope.product.Id = null;
				if ($scope.Action === "Save") {

					//debugger;
					$http({
						method: 'POST',
						url: $scope.saveUrl,
						data:
						{
							'entity': $scope.product,
							'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
							'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti,
							'Status': 'Save'

						},
						dataType: 'JSON'
					}).then(function (response) {
						if (response.data.Error === true) {
							ShowResult(response.data.Message, 'failure');
						}
						else {
							ShowResult(response.data.Message, 'success');
							$scope.productNew.Id = response.data.entity.Id;
							$scope.productId = response.data.entity.Id;
							$scope.productNew.PartyName = $scope.product.PartyName;
							$scope.tabType = "ForChecking";
							$scope.getalldataMaster();
							$scope.Action = "Update";
						}
					}), function (response) {
						ShowResult(response.data.Message, 'failure');
					};
				}

			}
		} catch (e) {
			throw e;
		}
	};
	
	// #region Extra Tax Add
	$scope.calculateTaxAmount = function (data) {
		//data.TotalAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
		data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
	};
	$scope.receiveTaxList = [];

	$scope.LoadTaxButtonClick = function () {
		accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
			$scope.taxCategoryList = result;
		});
	}
	//accountService.getTaxCategoryCbo(" ", function (result) {
	//    $scope.taxCategoryList = result;
	//});
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

	$scope.DeleteServicePOByMaster = function () {
		if (baseService.arrayLength($scope.inventoryMaterialList) === 0 && baseService.arrayLength($scope.chargesList) === 0) {
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
						$scope.getDataList();
						ClearFields();
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

	$scope.Clear = function () {
		
		ClearFields();
		if (!$rootScope.isCollapsed) $rootScope.toggle();
		return true;
	};

	function ClearFields() {
		$scope.NotificationSettingStatus();
		$scope.Action = "Save";
		$scope.product = {};
		$scope.IsBaseOnDueDateEnable = false;
		$scope.productNew = {
			FixedAssetOrInventory: 'Inventory'
			, PODepended: false
			, AlongwithInvoice: true
			, IsNonCreditable: false
			, BaseCurrencyId: $scope.baseCurrencyId
			, ToCurrencyRate: 1
			, TaxApplicable: null
			, IsTaxApplicable: false
			, IsTaxApplicableChangeable: false
			, PartyType: $scope.partyType
			, PlantId: $window.plantId
			, PODate: $filter("dateFiltering")(Date.now()),
		};
		$scope.inventoryMaterialList = [];
		$scope.chargesList = [];
		$scope.grossTotal = 0;
		baseService.removeErrorClasses();
		$scope.productNew.OrderSpecific = 'No';
		//$scope.getToCurrencyRate();
		$scope.productNew.ServiceType = 'ServiceACK';
		$scope.productNew.Id = null;
	}

	$scope.changeAllInvoice = function () {
		$scope.productNew.InvoiceNo = null;
		$scope.productNew.InvoiceDate = null;
	};
	$scope.showPartyPopUp = function () {
		baseService.setCurrentPage('partyList');
		$scope.getPartyList = function (pageno) {
			if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
				$scope.partyUrl = 'Parties/party/GetCompanyPartyDataList?partyType=' + $scope.partyType;
			}
			else if ($scope.partyType === 'Party') {
				$scope.partyUrl = 'Parties/party/GetCompanyPartyDataList';
			}
			else if ($scope.partyType === 'Director') {
				$scope.partyUrl = 'Parties/party/GetCompanyDirectorDataList';
			}
			else if ($scope.partyType === 'Other') {
				$scope.partyUrl = 'Parties/party/GetCompanyOtherDataList';
			}
			baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
				.then(function (result) {
					$scope.partyList = result.Rows;
					$scope.partyParameters.total_count = result.Total;
				}, function () {
					ShowResult(commonMessage.NetworkError, 'failure');
				}).finally(function () {
				});
		};
		angular.element(document.querySelector('#partyPopUp')).modal('show');
		$scope.getPartyList();
	};
	$scope.closePartyPopUp = function (x) {

		
		var party = x.data;
			$scope.productNew.PartyCode = party.Code;
			$scope.productNew.PartyName = party.UserName;
			$scope.productNew.PartyId = party.Id;
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
	$scope.GetCurrencyExchangeRateList = function () {

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

		if (baseService.isUndefinedOrNull($scope.productNew.DocDate)) {
			$scope.productNew.ToCurrencyRate = 1;
			return;
		}
		$http.get($scope.path + 'GetToCurrencyRate?currencyId=' + $scope.productNew.CurrencyId + '&baseCurrencyId=' + $scope.productNew.BaseCurrencyId + '&docDate=' + $filter('dateFiltering')($scope.productNew.DocDate))
			.then(function (response) {
				if (parseFloat(response.data) === 0)
					$scope.productNew.ToCurrencyRate = 1;
				else
					$scope.productNew.ToCurrencyRate = response.data;
			});
	};
	$scope.invoicingPartyPopUp = function () {
		// getPartyPlantEditList();
		angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
	};
	$scope.closeInvoicingPartyPopUp = function () {



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
	$scope.billShippAddress = function (id, flag) {


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
	$scope.tab = 1;
	$scope.setTab = function (newTab) {
		$scope.tab = newTab;
	};
	$scope.isSet = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.POPopUpGateEntry = function () {
		$scope.getalldataGateEntry();
		angular.element(document.querySelector('#POPopUpGateEntry')).modal('show');
	};
	$scope.POPopUpCloseGateEntry = function () {
		angular.element(document.querySelector('#POPopUpGateEntry')).modal('hide');
	};
	$scope.GriddataGateEntry = [];
	$scope.getalldataGateEntry = function () {
		//debugger;
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/GoodsReceiveNote/GetListOfPOGateEntry?partyCode=' + $scope.productNew.PartyId,
		}).then(function successCallback(response) {
			$scope.GriddataGateEntry = response.data;
			//entrydata = copy(searchdata);
		});
	};
	$scope.recorddoubleclickGateEntry = function ($event) {
		//debugger;
		var x = $event;
		var Id = x.data.Id;
		//alert('Id'+Id);
		// $scope.productNew = x.data;
		//  $scope.productId = "";
		$scope.productNew.GateEntryNo = x.data.Id;
		$scope.productNew.GateEntryDate = x.data.EntryDate;

		$scope.POPopUpCloseGateEntry();
	}

	// #region Details
	$scope.businessProcesses = '';//"BP.BusinessProcessName IN('MaintenanceSpare','BOM','WetProcess','Consumable')";
	$scope.detailPopUp = function () {

		$scope.receiveTaxList = [];
		$scope.detailModel = {

			[Id]: null
			, [ServicePOMasterId]: null
			, [InventoryReceiveId]: null
			, [ServiceMasterId]: null
			, [Amount]: null
			, [TotalTaxAmount]: null
			, [AddedBy]: null
			, [AddedDate]: null
			, [AddedFromIP]: null
			, [UpdatedBy]: null
			, [UpdatedDate]: null
			, [UpdatedFromIP]: null
			, [GRNServiceAmount]: null
			, [AmountStatus]: null
			, [Description]: null
			, ServiceRequsitionDetailId: null

		};
		$scope.clearCharNames();
		angular.element(document.querySelector('#detailPopUp')).modal('show');
	};
	//$scope.enable = true;
	//$scope.MAction = "Edit";
	//InventoryReceiveDetailId, TransactionQty, TransactionRate, TrnAmount, BaseTaxAmount, BaseAmount, index
	$scope.detailPopUpEdit = function () {



		for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
			for (var t = 0; t < $scope.inventoryMaterialList[i].TaxList.length; t++) {
				$scope.receiveTaxList.push($scope.inventoryMaterialList[i].TaxList[t]);
			}

		}
		//$scope.enable = false;
		//$scope.MAction = "Update"; 
		$http({
			method: 'POST',
			url: 'Products/PurchaseOrder/UpdateMaterial',
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
			}
		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};

	};
	$scope.MaterilaUpdate = function () {


		try {
			$scope.$broadcast('show-errors-check-validity');
			
			if ($scope.detailPopUpEditForm.$valid) {
				$http({
					method: 'POST',
					url: 'Products/PurchaseOrder/UpdateMaterial',
					data: $scope.detailModel,
					dataType: 'JSON'
				}).then(function (response) {
					if (response.data.Error === true) {
						ShowResult(response.data.Message, 'failure', 'detailPopUpEdit');
					}
					else {
						ShowResult(response.data.Message, 'success', 'detailPopUpEdit');
						
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
	$scope.uom = function () {

		cboService.getUoMCbo(function (response) {
			$scope.uoMList = response;
		});
	}
	$scope.uom();
	$scope.selectMaterialByType = function (ob) {

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

		getTaxCategoryList(ob.HSNCodeId);
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



	$scope.data = window.gridData;
	$scope.selectionMode = { selectionMode: ["row"] };
	$scope.GetListForMasterOrdernew = [];

	$scope.groupList = [];
	$scope.processgroupList = function (oldlist, newlist) {
		for (var i = 0; i < oldlist.length; i++) {
			var getRow = $filter("filter")(oldlist, { "MaterialMasterId": oldlist[i].MaterialMasterId, "ArticleId": oldlist[i].ArticleId, "FirstCharacteristicsValueId": oldlist[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": oldlist[i].SecondCharacteristicsValueId, "ThitrdCharacteristicsValueId": oldlist[i].ThitrdCharacteristicsValueId });
			var ExistingRow = $filter("filter")(newlist, { "MaterialMasterId": oldlist[i].MaterialMasterId, "ArticleId": oldlist[i].ArticleId, "FirstCharacteristicsValueId": oldlist[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": oldlist[i].SecondCharacteristicsValueId, "ThitrdCharacteristicsValueId": oldlist[i].ThitrdCharacteristicsValueId });
			// getRow.TransactionQty = $filter('sumByKey')($filter('filter')(oldlist), 'TaxAmount');
			if (ExistingRow.length === 0) {
				if (!baseService.isUndefinedOrNull(getRow[0].MaterialMasterId)) {
					newlist.push(getRow[0]);
				}
			}

			var getRowWithoutMaterial = $filter("filter")(oldlist, { "MaterialDetail": oldlist[i].MaterialDetail, "RequisitionDetailId": oldlist[i].RequisitionDetailId });

			if (getRowWithoutMaterial.length === 1) {
				if (baseService.isUndefinedOrNull(getRowWithoutMaterial[0].MaterialMasterId)) {
					newlist.push(getRowWithoutMaterial[0]);
				}
			}

		}
		return newlist;
	}
	$scope.detailSave = function () {
		//debugger;  
		try {
			$scope.GetServiceRequisitionListNew = [];
			for (var i = 0; i < $scope.GetServiceRequisitionList.length; i++) {

				if ($scope.GetServiceRequisitionList[i].Active === true) {
					$scope.GetServiceRequisitionListNew.push($scope.GetServiceRequisitionList[i]);

				}

			}
			if (baseService.arrayLength($scope.GetServiceRequisitionListNew) === 0) {
				throw 'Please select Items.';
			}
			$scope.processgroupList($scope.GetServiceRequisitionListNew, $scope.groupList);
			//$scope.materialValidation();

			$scope.invalid = true;
			if ($scope.invalid) {
				if ($scope.Action1 === 'Save') {
					$http({
						method: 'POST',
						url: $scope.detailSaveUrl,
						data: {
							'entity': $scope.GetServiceRequisitionListNew,
							'ServicePoMasterId': $scope.productNew.Id,
							'taxCategoryList': $scope.taxCategoryListNew
						},
						dataType: 'JSON'
					}).then(function successCallback(response) {
						if (response.data.Error === true)
							ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
						else {
							ShowResult(response.data.Message, 'success', 'ListOfRequisition');
							$scope.taxCategoryListNew = [];
							getServiceChargeList($scope.productNew.Id);
							$scope.RequisitionListHide();
							$scope.setTabIndex(1);

						}
					}), function errorCallBack(response) {
						ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
					};

				}
				else if ($scope.Action1 === "Update") {
					$http({
						method: 'POST',
						url: $scope.detailUpdateUrl,
						data: {
							entity: $scope.GetListForMasterOrdernew
							, taxCategoryList: $scope.taxCategoryList
							, PoId: $scope.productNew.Id
							, groupList: $scope.groupList
						},
						dataType: 'JSON'
					}).then(function successCallback(response) {
						if (response.data.Error === true)
							ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
						else {
							ShowResult(response.data.Message, 'success', 'ListOfRequisition');
							getInventoryMaterialList($scope.productNew.Id);
						}
					}), function errorCallBack(response) {
						ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
					};

				}
			}
		} catch (e) {
			ShowResult(e, 'failure', 'ListOfRequisition');
		}
	};
	$scope.ActionForTax = 'Save';
	$scope.SaveTax = function () {
		//debugger;
		try {
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

			}

			$scope.invalid = true;
			if ($scope.invalid) {
				if ($scope.ActionForTax === 'Save') {
					$http({
						method: 'POST',
						url: 'Products/PurchaseOrder/GetUpdateServicePOTax',
						data: {
							'receiveTaxList': $scope.receiveTaxList,
							'ServicePODetailId': $scope.ServiceId,
							'servicePOid': $scope.productNew.Id
						},
						dataType: 'JSON'
					}).then(function successCallback(response) {
						if (response.data.Error === true)
							ShowResult(response.data.Message, 'failure', 'ServiceChargeTaxPopUp');
						else {
							ShowResult(response.data.Message, 'success', 'ServiceChargeTaxPopUp');
							getServiceChargeList($scope.productNew.Id);
						}
					}), function errorCallBack(response) {
						ShowResult(response.data.Message, 'failure', 'ServiceChargeTaxPopUp');
					};

				}
				else if ($scope.ActionForTax === "Update") {
					$http({
						method: 'POST',
						url: 'Products/PurchaseOrder/GetUpdateServicePOTax',
						data: {
							'receiveTaxList': $scope.receiveTaxList,
							'ServicePODetailId': $scope.ServiceId,
							'servicePOid': $scope.productNew.Id
						},
						dataType: 'JSON'
					}).then(function successCallback(response) {
						if (response.data.Error === true)
							ShowResult(response.data.Message, 'failure', 'ServiceChargeTaxPopUp');
						else {
							ShowResult(response.data.Message, 'success', 'ServiceChargeTaxPopUp');
							getServiceChargeList($scope.productNew.Id);
						}
					}), function errorCallBack(response) {
						ShowResult(response.data.Message, 'failure', 'ServiceChargeTaxPopUp');
					};

				}
			}
		} catch (e) {
			ShowResult(e, 'failure', 'ServiceChargeTaxPopUp');
		}
	};




	$scope.valuePassInDelModal = function (id) {

		$scope.id = id.InventoryReceiveDetailId;
		$scope.message = 'Are you sure want to permanently delete this?';
		angular.element(document.querySelector('#removerPopUp')).modal('show');
	};
	$scope.detailDelete = function ($event) {

		//var x = $event;
		//var Id = x.data.Id;
		//$scope.id = x.data.Id
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
					$scope.getDataList();
				}
			}), function errorCallBack(response) {
				ShowResult(response.data.Message, 'failure');
			};
		} catch (e) {
			ShowResult(e, 'success');
		}
	};


	$scope.servicePOdetailDeletePopup = function (Id) {
		$scope.Id = Id;
		$scope.message = 'Are you sure want to permanently delete this?';
		angular.element(document.querySelector('#ServicePOdetailDeletePopUp')).modal('show');
	};



	$scope.ServicePOdetailDelete = function (x) {

		//var x = $event;
		//var Id = x.data.Id;
		//$scope.id = x.data.Id
		try {
			$http({
				method: 'POST',
				url: $scope.servicePOdetailDeleteUrl + $scope.Id
			}).then(function successCallback(response) {
				if (response.data.Error === true)
					ShowResult(response.data.Message, 'failure');
				else {
					ShowResult(response.data.Message, 'success');
					$scope.id = null;
					getInventoryMaterialList($scope.productNew.Id);
					getServiceChargeList($scope.productNew.Id);
				}
			}), function errorCallBack(response) {
				ShowResult(response.data.Message, 'failure');
			};
		} catch (e) {
			ShowResult(e, 'success');
		}
	};

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

	function getTaxCategoryListt(hsnCodeId) {
		debugger


		$scope.taxCategoryList = [];
		$http({
			method: 'GET'
			, url: $scope.path + 'GetTaxCategoryList?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId
		}).then(function (response) {
			$scope.taxCategoryList = response.data;
		});
	}
	function getTaxCategoryList(hsnCodeId) {
		debugger


		$scope.taxCategoryList = [];
		$http({
			method: 'GET'
			, url: $scope.path + 'GetTaxCategoryList?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId
		}).then(function (response) {
			$scope.taxCategoryList = response.data;
		});
	}

	function getTaxCategoryListForIndividual(hsnCodeId) {
		debugger


		$scope.taxCategoryList = [];
		$http({
			method: 'GET'
			, url: $scope.path + 'GetTaxCategoryList?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.productNew.PODate
		}).then(function (response) {
			$scope.taxCategoryList = response.data;
		});
	}


	$scope.getTaxCategoryList1 = function ($event) {
		debugger
		var x = $event;
		//var Id = x.data.Id;
		var hsnCodeId = x.data.hsnCodeId;

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

		$scope.detailModel.TotalTaxAmount = 0;
		var tQty = baseService.isUndefinedOrNull($scope.detailModel.TransactionQty) ? 0 : parseFloat($scope.detailModel.TransactionQty);
		var tAmount = baseService.isUndefinedOrNull($scope.detailModel.TransactionRate) ? 0 : parseFloat($scope.detailModel.TransactionRate);
		if (tQty > 0)
			//$scope.detailModel.TransactionRate = tAmount / tQty;
			$scope.detailModel.TransactionAmount = tAmount * tQty;
		else
			//$scope.detailModel.TransactionRate = 0;
			$scope.detailModel.TransactionAmount = 0;
		for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
			$scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.detailModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
			$scope.detailModel.TotalTaxAmount = (parseFloat($scope.detailModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
		}
		if (isNaN($scope.detailModel.TotalTaxAmount)) $scope.detailModel.TotalTaxAmount = 0;
	};
	$scope.sumTaxAmount = function () {
		$scope.detailModel.TotalTaxAmount = 0;
		for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
			$scope.detailModel.TotalTaxAmount = (parseFloat($scope.detailModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
		}
	};
	$scope.getReceiveTaxList = function (data, flag, index, Id) {
		$scope.LoadTaxButtonClick();


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

		$scope.detailModel = {};
		//$scope.receiveTaxList = [];
		//



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


		}

		//if ($scope.TAction === "OK") {
		$http({
			method: 'POST',
			//url: $scope.saveUrl,
			url: 'Products/PurchaseOrder/InsertExtraTax',
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
				getInventoryMaterialList($scope.productNew.Id);
			}
		}), function (response) {
			ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
		};
		// }

		//angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');

	}
	$scope.closeServiceChargeTaxPopUp = function () { //hossain
		//



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

		}

		//if ($scope.TAction === "OK") {
		$http({
			method: 'POST',
			//url: $scope.saveUrl,
			url: 'Products/PurchaseOrder/InsertserviceTax',
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
			}
		}), function (response) {
			ShowResult(response.data.Message, 'failure', 'ServiceChargeTaxPopUp');
		};
		// }

		//angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');

	}
	$scope.closeReceiveTaxPopUpwindow = function () {

		getInventoryMaterialList($scope.productNew.Id);
		angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
	}
	$scope.closeServiceChargeTaxPopUpwindow = function () {
		//debugger;
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
			url: 'Products/PurchaseOrder/GetServicePOTerms?id=' + id
		}).then(function successCallback(response) {
			$scope.paymentTermList1 = response.data;
			$scope.productNew.DeliveryInstruction = $scope.paymentTermList1[0].DeliveryInstruction;
			$scope.productNew.SpecialInstruction = $scope.paymentTermList1[0].SpecialInstruction;
			//s$scope.productNew.CheckedBy = $scope.paymentTermList1[0].CheckedBy;
		});

	}

	$scope.changePaymentTerm = function () {
		if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
			var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
			$scope.productNew.PaymentTermCode = paymentTerm.PaymentTermCode;
			$scope.productNew.BaseNoOfDays = paymentTerm.NoOfDay;
			if (paymentTerm.BaseLineDate !== null)
				if (paymentTerm.BaseLineDate === 'documentdate') {
					$scope.productNew.BaseOnDueDate = $filter('dateFiltering')($scope.productNew.DocDate);
					$scope.IsBaseOnDueDateEnable = true;
				}
				else {
					$scope.productNew.BaseOnDueDate = null;
					$scope.IsBaseOnDueDateEnable = false;
				}
			$scope.getMatureDate($scope.productNew.BaseOnDueDate, $scope.productNew.BaseNoOfDays);
		}
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
		//if (baseService.arrayLength($scope.inventoryMaterialList) === 0)
		//    return ShowResult('Without material charges not aplicable.');
		$scope.productNew.TaxOptionService = 'Yes';
		$scope.serviceModel = {
			Id: null
			, ServiceMasterId: null
			, CurrencyName: angular.element("#currency :selected").text()
			, CurrencyId: $scope.productNew.CurrencyId
			, BaseCurrencyId: $scope.baseCurrencyId
			, DocDate: $scope.productNew.DocDate
			, TransactionAmount: null
			, BaseAmount: 0
			, TotalTaxAmount: 0
			, ToCurrencyRate: $scope.productNew.ToCurrencyRate
			, IsNonCreditable: $scope.productNew.IsNonCreditable
			, ServicePOMasterId: null
			, ServiceMasterId: null
			, Amount: null
			, GRNServiceAmount: null
			, AmountStatus: null
			, Description: null
			, ServiceRequsitionDetailId: null
			, ServiceReqMasterId: null
			, Rate: 0
			, Qty: 0
			, TransactionUoMId:null
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
		$scope.serviceModel.TransactionAmount = 0;
		$scope.serviceModel.Rate = 0;
		$scope.serviceModel.Qty = 0;


		if (baseService.isUndefinedOrNull($scope.serviceModel.ServiceMasterId))
			return $scope.taxCategoryList = [];
		var hsnCodeId = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCodeId;
		//getTaxCategoryListForIndividual(hsnCodeId);
		$scope.getserviceTaxByTaxCategoryList1(hsnCodeId);
	};
	$scope.calculateQty = function () {
		$scope.serviceModel.TotalTaxAmount = 0;
		if (baseService.isUndefinedOrNull($scope.serviceModel.Rate)) {
			$scope.serviceModel.Rate = 0;
		}		
		//item.TaxAmount = Math.round((data.TrnAmount * item.Percentage / 100) * 100 + Number.EPSILON) / 100;
		$scope.serviceModel.TransactionAmount = Math.round(($scope.serviceModel.Qty * $scope.serviceModel.Rate) * 100 + Number.EPSILON) / 100;
		for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
			$scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.serviceModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
			$scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
		}
		if (isNaN($scope.serviceModel.TotalTaxAmount)) $scope.serviceModel.TotalTaxAmount = 0;
	};
	$scope.calculateTrnRate = function () {
		
		$scope.serviceModel.TotalTaxAmount = 0;
		if (baseService.isUndefinedOrNull($scope.serviceModel.Qty)) {
			$scope.serviceModel.Qty = 0;
		}	
		$scope.serviceModel.TransactionAmount = Math.round(($scope.serviceModel.Qty * $scope.serviceModel.Rate) * 100 + Number.EPSILON) / 100;
		for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
			$scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.serviceModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
			$scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
		}
		if (isNaN($scope.serviceModel.TotalTaxAmount)) $scope.serviceModel.TotalTaxAmount = 0;
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
						, CurrencyName: angular.element("#currency :selected").text()
						, CurrencyId: $scope.productNew.CurrencyId
						, BaseCurrencyId: $scope.baseCurrencyId
						, DocDate: $scope.productNew.DocDate
						, TransactionAmount: null
						, BaseAmount: 0
						, TotalTaxAmount: 0
						, ToCurrencyRate: $scope.productNew.ToCurrencyRate
						, IsNonCreditable: $scope.productNew.IsNonCreditable
						, ServicePOMasterId: null
						, ServiceMasterId: null
						, Amount: null
						, GRNServiceAmount: null
						, AmountStatus: null
						, Description: null
						, ServiceRequsitionDetailId: null
						, ServiceReqMasterId: null
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


		$scope.LoadTaxButtonClick();

		$scope.Currency = $("#currency option:selected").text();
		$scope.ServiceId = ServiceId;
		$scope.taxAbleAmnt = data.Amount;//+ data.TotalTaxAmount;
		$scope.percentageColumn = flag;

		$scope.currentMaterialRow = index;
		
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

	}
	//Load2
	$scope.GetServiceTaxData = function (masterId) {
		//
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
		for (var i = 0; i < $scope.ChargeTaxList.length; i++) {
			if ($scope.ChargeTaxList[i].InventoryServiceId === linepk1) {
				result1.push($scope.ChargeTaxList[i]);
			}
		}
		return result1;
	}



	$scope.serviceChargePopUpEdit = function (Id, Amount, TotalTaxAmount) {
		if (baseService.arrayLength($scope.inventoryMaterialList) === 0)
			return ShowResult('Without material charges not aplicable.');


		for (var i = 0; i < $scope.chargesList.length; i++) {
			for (var t = 0; t < $scope.chargesList[i].ChargeTaxList.length; t++) {
				$scope.receiveTaxList.push($scope.chargesList[i].ChargeTaxList[t]);
			}

		}
		$scope.productNew.Id
		$http({
			method: 'POST',
			url: 'Products/PurchaseOrder/UpdateServiceAndTax',
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
			, CurrencyName: angular.element("#currency :selected").text()
			, CurrencyId: $scope.productNew.CurrencyId
			, BaseCurrencyId: $scope.baseCurrencyId
			, DocDate: $scope.productNew.DocDate
			, TransactionAmount: null
			, BaseAmount: 0
			, TotalTaxAmount: 0
			, ToCurrencyRate: $scope.productNew.ToCurrencyRate
			, IsNonCreditable: $scope.productNew.IsNonCreditable
			, ServicePOMasterId: null
			, ServiceMasterId: null
			, Amount: null
			, GRNServiceAmount: null
			, AmountStatus: null
			, Description: null
			, ServiceRequsitionDetailId: null
			, ServiceReqMasterId: null
		};
	};
	// #endregion Service

	$scope.inventoryReceiveReport = function (id, reportFormat) {
		if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
		$window.open('Products/InventoryReceive/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + id + '&plantId=' + $scope.productNew.PlantId, '_blank');
	};
	$scope.GriddataServicePOList = [];
	$scope.POTypeStatus = 'ForChecked';
	$scope.getalldata = function () {
		if ($scope.POTypeStatus === 'ForChecked') {
			$scope.POTypeStatus = 'ForChecked'
		}
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/PurchaseOrder/GetListForServicePOBYReq?POTypeStatus=' + $scope.POTypeStatus + '&serviceType=' + $scope.productNew.ServiceType,
		}).then(function successCallback(response) {
			$scope.GriddataServicePOList = response.data;
			//entrydata = copy(searchdata);
		});
	};
	$scope.getalldata();

	$scope.GriddataIndexServicePOHRList = [];
	$scope.getalldataIndexApp = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/PurchaseOrder/GetListForServicePOBYReqHR?ApproveRejectHold=' + $scope.ApproveRejectHold + '&serviceType=' + $scope.productNew.ServiceType,
		}).then(function successCallback(response) {
			$scope.GriddataIndexServicePOHRList = response.data;
			//entrydata = copy(searchdata);
		});
	};
	// $scope.getalldataIndexApp();


	$scope.Griddata = [];
	$scope.getApprovaldata = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/GetListForPOApproval',
		}).then(function successCallback(response) {
			$scope.Griddata = response.data;

		});
	};
	//$scope.getApprovaldata();

	$scope.GriddataAUth = [];
	$scope.getApprovaldataAUth = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/GetListForPOApprovalAuthorized',
		}).then(function successCallback(response) {
			$scope.GriddataAUth = response.data;

		});
	};
	// $scope.getApprovaldataAUth();

	$scope.GriddataAUth1 = [];
	$scope.getApprovaldataAUth1 = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/PurchaseOrder/GetListForPOApproval1Auth',
		}).then(function successCallback(response) {
			$scope.GriddataAUth1 = response.data;
			//entrydata = copy(searchdata);
		});
	};
	//  $scope.getApprovaldataAUth1();

	$scope.GriddataVendor = [];
	$scope.getalldataVendor = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/PurchaseOrder/GetListByParty',
		}).then(function successCallback(response) {
			$scope.GriddataVendor = response.data;
			//entrydata = copy(searchdata);
		});
	};
	function getPartyPlantList() {


		//var aa = $scope.Id;
		$scope.plantList = [];
		$http.get('Products/PurchaseOrder/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId + '&Id=' + $scope.Id).then(function (response) {
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
	$scope.recorddoubleclick = function ($event) {
		//debugger;
		var x = $event;
		var Id = x.data.Id;
		$scope.productNew.OrderSpecific = x.data.OrderSpecific;
		$scope.Currency = $("#currency option:selected").text();
		$scope.productNew = x.data;
		$scope.Id = $scope.productNew.Id;
		//  $scope.LoadAllReq();
		$scope.GetTerms($scope.productNew.Id);
		//getPartyPlantEditList($scope.productNew.InvoicingPartyPlantId, $scope.productNew.InvoicingByAddress, $scope.productNew.DeliveryPartyPlantId, $scope.productNew.DeliveryByAddress, $scope.productNew.DeliveryState, $scope.productNew.DeliveryGSTIN);
		
		//getInventoryMaterialList($scope.productNew.Id);
		getServiceChargeList($scope.productNew.Id);

		if (!baseService.isUndefinedOrNull(x.data.ContractId)) {
			$scope.productNew.OrderSpecific = 'Yes';
		}
		else {
			$scope.productNew.OrderSpecific = 'No';
		}
		
		if (baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.ApprovedById)) {
			$scope.CheckedByStatusForNoti = false;
			$scope.ApprovedByStatusForNoti = true;
			$scope.productNew.CheckedBy = x.data.ApprovedById;
		}
		else if (!baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.ApprovedById)) {
			$scope.CheckedByStatusForNoti = true;
			$scope.ApprovedByStatusForNoti = true;
			$scope.productNew.CheckedBy = x.data.CheckedById;
		}
		$scope.ContractWiseData(x.data.ContractId);
		$scope.ImagedataLoad($scope.productNew.Id);
		$scope.GetCheckedByAndApprovedBy1();
		if (baseService.isUndefinedOrNull(x.data.CheckedById) && !baseService.isUndefinedOrNull(x.data.ApprovedById)) {
			$scope.GetCheckedByAndApprovedBy1();
			$scope.productNew.CheckedBy = x.data.ApprovedById;
			$scope.productNew.labelCheckAndApproved = 'To be approved by';
		}
		else if (!baseService.isUndefinedOrNull(x.data.CheckedById) && baseService.isUndefinedOrNull(x.data.ApprovedById)) {
			$scope.GetCheckedByAndApprovedBy1();
			$scope.productNew.CheckedBy = x.data.CheckedById;
			$scope.productNew.labelCheckAndApproved = 'To be checked by';
		}


		$scope.Action = 'Update';
		if (!$rootScope.isCollapsed) $rootScope.toggle();
	};

	
	function getInventoryMaterialList(inveReveiveId) {
		$scope.masterId = inveReveiveId;

		$scope.inventoryMaterialList = [];
		$http.get($scope.path + 'GetInventoryMaterialListPoByReq?inveReveiveId=' + inveReveiveId)
			.then(function (response) {

				$scope.inventoryMaterialList = response.data.Rows;
				getGrossAmount($scope.inventoryMaterialList, 'BaseAmount', 'BaseTaxAmount', 'ChargesAmount', 'grossTotal');
				$scope.GetSalesTaxData();

			});

	}

	function getServiceChargeList(inveReveiveId) {

		$scope.chargesList = [];
		$http.get('Products/PurchaseOrder/GetServiceChargePOServiceList?id=' + inveReveiveId)
			.then(function (response) {
				$scope.chargesList = response.data;
				//$scope.ServiceId = $scope.chargesList[0].Id;
				$scope.GetServiceTaxData();
			});

	}
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

	};

	function getTaxCategoryList(hsnCodeId) {
		debugger
		//var x = $event;
		//var Id = x.data.Id;
		//var hsnCodeId = x.data.hsnCodeId;

		$scope.taxCategoryList = [];
		$http({
			method: 'GET'
			, url: $scope.path + 'GetTaxCategoryList?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.productNew.PODate
		}).then(function (response) {
			$scope.taxCategoryList = response.data;
		});
	}
	
	$scope.GetSalesTaxDataa = function (salesId) {
		$scope.TaxList = [];
		$http({
			method: "GET",
			url: $scope.path + 'GetReceiveTaxList?receiveDetailId=' + $scope.id
		}).then(function (response) {
			$scope.TaxList = response.data;

			for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
				var linepk = $scope.inventoryMaterialList[i].InventoryReceiveDetailId;
				var list = gettaxlista(linepk);
				$scope.inventoryMaterialList[i].TaxList = list;
			}
		});
	};
	function gettaxlista(linepk) {
		var result = [];
		for (var i = 0; i < $scope.TaxList.length; i++) {
			if ($scope.TaxList[i].PODetailId === linepk) {
				result.push($scope.TaxList[i]);
			}
		}
		return result;
	}


	$scope.calculateAmount = function (data) {

		data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
		if (data.TrnAmount === 'NaN')
			data.TrnAmount = 0;
		data.TaxAmount = 0;
		$scope.id = data.InventoryReceiveId;
		angular.forEach($scope.TaxList, function (item) {
			if (item.PODetailId === data.InventoryReceiveDetailId) {
				item.TaxAmount = data.TrnAmount * item.Percentage / 100;
				data.BaseTaxAmount = item.TaxAmount;
				$scope.TaxList.TaxAmount = data.BaseTaxAmount;
			}

		});

		if ($scope.productNew.IsNonCreditable == 1) {
			if (data.BaseTaxAmount === null) {
				data.BaseTaxAmount = '0.00';
			}
			data.BaseAmount = parseFloat(parseFloat(data.TrnAmount) + parseFloat(data.BaseTaxAmount)).toFixed(2);
		}
		else {
			data.BaseAmount = parseFloat(data.TrnAmount).toFixed(2);
		}


	};
	$scope.calculateRate = function (data, event) {

		data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
		if (data.TransactionRate === 'NaN')
			data.TransactionRate = 0;
		data.BaseTaxAmount = 0;
		angular.forEach(data.TaxList, function (item) {
			item.TaxAmount = data.TrnAmount * item.Percentage / 100;

			data.BaseTaxAmount += item.TaxAmount;
		});
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

		data.TotalTaxAmount = 0;
		for (var i = 0; i < $scope.ChargeTaxList.length; i++) {
			if ($scope.ChargeTaxList[i].InventoryServiceId === data.Id) {
				$scope.ChargeTaxList[i].TaxAmount = data.Amount * $scope.ChargeTaxList[i].Percentage / 100;
				data.TotalTaxAmount += $scope.ChargeTaxList[i].TaxAmount;
			}
		}
	};
	$scope.onchangeFunction = function (id) {
		$scope.TaxCategoryId = id;

		var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": id });
		if (getRow.length === 2) {
			ShowResult("You can't add Same Tax two times", 'failure', 'ServiceChargeTaxPopUp');

		}

	}
	$scope.onchangeFunction1 = function (id) {
		$scope.TaxCategoryId = id;

		var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": id });
		if (getRow.length === 2) {
			ShowResult("You can't add Same Tax two times", 'failure', 'receiveTaxPopUp');

		}

	};

	//#region Purchase-Order-By-Requisition ----All Print Function
	$scope.onClick = function (args) {

		var gridObj = $("#Grid").data("ejGrid");
		var data = gridObj.getSelectedRecords()[0];
		location.href = "Products/PurchaseOrder/GePurchaseOrderReportByReq?purchaseOrderId=" + data.Id;

	};
	$scope.command = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",

			click: $scope.onClick
		}
	}];


	$scope.onClickCheckedHR = function (args) {

		var gridObj = $("#GridCheckedHR").data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		//alert('jj' + data.Id);
		// $scope.valuePassInDelModal(data); 
		location.href = "Products/PurchaseOrder/GePurchaseOrderReportByReq?purchaseOrderId=" + data.Id;

	};
	$scope.commandCheckedHRPrint = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",

			click: $scope.onClickCheckedHR
		}
	}];



	$scope.onClickChecked = function (args) {

		var gridObj = $("#GridChecked").data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		location.href = "Products/PurchaseOrder/GePurchaseOrderReportByReq?purchaseOrderId=" + data.Id;

	};
	$scope.commandCheckedPrint = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",

			click: $scope.onClickChecked
		}
	}];



	$scope.onClickApprovedHR = function (args) {

		var gridObj = $("#GridApprovedHR").data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		location.href = "Products/PurchaseOrder/GePurchaseOrderReportByReq?purchaseOrderId=" + data.Id;

	};
	$scope.commandApprovedHR = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",

			click: $scope.onClickApprovedHR
		}
	}];



	$scope.onClickApp = function (args) {

		var gridObj = $("#GridApp").data("ejGrid");
		var data = gridObj.getSelectedRecords()[0];
		location.href = "Products/PurchaseOrder/GePurchaseOrderReportByReq?purchaseOrderId=" + data.Id;

	};
	$scope.commandApp = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",

			click: $scope.onClickApp
		}
	}];

	//#endregion

	//#region Print for po Approval

	$scope.onClickpoApprovalprint = function (args) {

		var gridObj = $("#GridPO1").data("ejGrid");
		var data = gridObj.getSelectedRecords()[0];
		location.href = "Products/PurchaseOrder/GePurchaseOrderReport?purchaseOrderId=" + data.Id;

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

	
	$scope.invalidDocDate = false;
	$scope.checkDocDate = function () {
		var msg = "";

		if (new Date($scope.productNew.DocDate) > new Date($scope.productNew.PODate)) {
			msg = "Doc date must be grater or equal to Vendor Doc. RefNo!";
			$scope.invalidDocDate = true;
		}
		else $scope.invalidDocDate = false;
		return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
	};
	//#region Shahazahan Code for PO Approval
	$scope.Griddata1 = [];
	$scope.onClickPO = function (args) {

		var gridObj = $("#Grid").data("ejGrid");
		//getting corresponding record 
		$scope.data = gridObj.getSelectedRecords()[0];
		//alert('POClose' + data.Id);
		$scope.approveAlert();

	};
	cboService.getEnumCbo("enum/GetExpensesBookingApprovalStatusCbo", function (result) {
		$scope.approvalStatusList = result;
	});
	$scope.getalldata1 = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/GetListForPOApproval',
		}).then(function successCallback(response) {
			$scope.Griddata1 = response.data;
		});
	};
	//$scope.getalldata1();
	$scope.Status = null;

	$scope.poApp = function () {
		var str = $('#combo-default1').val();
		var Id = str.substring(0, str.indexOf('-'));

		$http({
			method: 'POST',
			url: 'Products/PurchaseOrder/PoApproved',
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

		$http({
			method: 'POST',
			url: 'Products/PurchaseOrder/PoApprovedAuth',
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


		$http({
			method: 'POST',
			url: 'Products/PurchaseOrder/PoUnApproved',
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
			url: 'Products/PurchaseOrder/GetListForPOClose',
		}).then(function successCallback(response) { //datagatefun
			$scope.GriddataPOClose = response.data;
			//entrydata = copy(searchdata);
		});
	};
	$scope.getalldataPOClose();


	$scope.onClickPOlock = function (args) {

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
			url: 'Products/PurchaseOrder/POClose',
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
	$scope.tab = 1;
	$scope.setTab = function (newTab) {
		$scope.tab = newTab;
	};
	$scope.isSet = function (tabNum) {
		return $scope.tab === tabNum;
	};
	// #endregion

	// #region Taufik Un Approval po data post start
	$scope.Griddataapprovpo = [];
	$scope.Griddataapprovpo1 = function () {
		$http({
			method: "GET",
			dataType: 'JSON',

			url: 'Products/PurchaseOrder/GetListForPOApproval1',
		}).then(function successCallback(response) {
			$scope.Griddataapprovpo = response.data;

		});
	};
	//$scope.Griddataapprovpo1();



	$scope.ListForPOApproval1UnApproved = [];
	$scope.GetListForPOApproval1UnApproved = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/PurchaseOrder/GetListForPOApproval1UnApproved',
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
	//#endregion

	//#region Towfik PO Unlock
	$scope.GriddataPOlock = [];
	$scope.getalldataPOUnlock = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/PurchaseOrder/GetListForPOUnClose',
		}).then(function successCallback(response) { //datagatefun
			$scope.GriddataPOlock = response.data;
			//entrydata = copy(searchdata);
		});
	};

	$scope.getalldataPOUnlock();

	$scope.onClickPOlock = function (args) {

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
			url: 'Products/PurchaseOrder/POUnClose',
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
			url: 'Products/PurchaseOrder/GetListForAllPOList',
		}).then(function successCallback(response) { //datagatefun
			$scope.GriddataPOListforPoclosedui = response.data;
			//entrydata = copy(searchdata);
		});
	};

	$scope.getalldataPOListforPoclosedui();

	$scope.onClickPoList = function (args) {

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
			url: 'Products/PurchaseOrder/POClose',
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

	$scope.RequisitionList = function () {
		//debugger;
		$scope.Action1 = 'Save';
		$scope.getalldataListForRequisitionList();
		//$scope.processgroupList1();
		// $scope.GerRequisition();

	};

	$scope.RequisitionListHide = function () {
		$scope.taxCategoryList = [];
		angular.element(document.querySelector('#ListOfRequisition')).modal('hide');
	};

	$scope.RequisitionList1 = function () {
		// $scope.Action1 = 'Save';
		$scope.getalldataListForReqList1();
		// $scope.GerRequisition();
		angular.element(document.querySelector('#ListOfRequisition1')).modal('show');
	};

	$scope.RequisitionListtHide1 = function () {
		$scope.taxCategoryList = [];
		angular.element(document.querySelector('#ListOfRequisition1')).modal('hide');
	};




	$scope.GetServiceRequisitionList = [];
	$scope.getalldataListForRequisitionList = function () {
		//debugger;
		$scope.GetListForMasterOrder = [];
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/PurchaseOrder/GetListForServiceRequisition',
		}).then(function successCallback(response) { //datagatefun
			$scope.GetServiceRequisitionList = response.data;
		});

		$scope.processgroupList1();
	};
	$scope.groupList = [];
	$scope.processgroupList1 = function () {

		if ($scope.inventoryMaterialList.length > 0) {
			$scope.newlistitems = [];
			$scope.newlistitems = $scope.GetListForMasterOrder;
			$scope.GetListForMasterOrder = [];
			for (var i = 0; i < $scope.newlistitems.length; i++) {
				var getRow = $filter("filter")($scope.inventoryMaterialList, { "MaterialMasterId": $scope.newlistitems[i].MaterialMasterId, "ArticleId": $scope.newlistitems[i].ArticleId, "FirstCharacteristicsValueId": $scope.newlistitems[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.newlistitems[i].SecondCharacteristicsValueId, "ThitrdCharacteristicsValueId": $scope.newlistitems[i].ThitrdCharacteristicsValueId });
				if (getRow.length == 0) {
					$scope.GetListForMasterOrder.push($scope.newlistitems[i]);
				}
			}
		}
		angular.element(document.querySelector('#ListOfRequisition')).modal('show');

	}
	$scope.GetListForMasterOrder1 = [];
	$scope.getalldataListForReqList1 = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/PurchaseOrder/GetListForRequisition1',
		}).then(function successCallback(response) { //datagatefun
			$scope.GetListForMasterOrder1 = response.data;
			//entrydata = copy(searchdata);
		});
	};




	$scope.Getrecorddoubleclick = function ($event, index) {

		// alert('Do you want to see Material Details');
		var x = $event;
		var Id = x.data.Id;
		$scope.MONo = Id;
		getMasterItemList();
		angular.element(document.querySelector('#ListOfMasterOrder')).modal('hide');

	};

	function getMasterItemList() {

		$scope.inventoryMaterialList = [];
		$http.get($scope.path + 'GetMasterItemList?masterOrderId=' + $scope.MONo)
			.then(function (response) {
				$scope.inventoryMaterialList = response.data;
				$scope.GetSalesTaxData();
			});
	}
	$scope.calculateAmountByRateFG = function (data) {

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
		$scope.productNew.TaxOptionMat = 'Yes';
		$scope.LoadTaxButtonClick();

		$scope.Currency = $("#currency option:selected").text();
		$scope.ServiceId = ServiceId;
		$scope.taxAbleAmnt = data.Amount;//+ data.TotalTaxAmount;
		$scope.percentageColumn = flag;

		$scope.currentMaterialRow = index;
		$scope.receiveTaxList = [];
		
		$http({
			method: 'GET',
			//url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
			url: 'Products/PurchaseOrder/LoadTaxById?id=' + $scope.ServiceId
		}).then(function successCallback(response) {
			$scope.lst = response.data;
			$scope.HSNCode = $scope.lst[0].HSNCodeId;
			$scope.ServiceName = data.ServiceName;
			for (var i = 0; i < $scope.lst.length; i++) {
				$scope.receiveTaxList.push($scope.lst[i]);
			}

			if ($scope.receiveTaxList.length > 0) {
				$scope.ActionForTax = 'Update';
			}
			else {
				$scope.ActionForTax = 'Save';
			}
		});


		angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('show');
	}

	$scope.AddReceiveTaxPopUpFG = function (Id, index) { //hossain

		$scope.detailModel = {};
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

		$scope.PODetailid = data.Id;

		$scope.LoadTaxButtonClick();

		$scope.Currency = $("#currency option:selected").text();
		$scope.currentMaterialRow = index;
		$scope.currentInventoryReceiveDetailIdRow = Id;
		$scope.taxAbleAmnt = data.TrnAmount;
		$scope.percentageColumn = flag;

		$scope.currentMaterialRow = index;
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
		//});
		// $Scope.TAction = "OK";
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

	$scope.SaveFG = function () {
		//
		try {
			$scope.dbval = $scope.StateData;
			$scope.UIval = $scope.productNew.InvoicingState;

			if ($scope.inventoryMaterialList.length === 0) {
				angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
			}
			else if ($scope.dbval.length === 0) {
				angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
			}
			else if ($scope.dbval === $scope.UIval) {
				angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
			}
			else {
				ShowResult('You can not change Invoicing party.Line is available', 'failure', 'invoicingPartyPopUp');

			}

			if (baseService.isUndefinedOrNull($scope.productNew.InvoicingPartyPlantId)) return ShowResult('Invoicing by is required', 'failure');
			if (baseService.isUndefinedOrNull($scope.productNew.DeliveryPartyPlantId)) return ShowResult('Delivery by is required', 'failure');
			$scope.modelValidation('div_docNo', 'productNew', 'DocRefNo');
			$scope.modelValidation('div_docDate', 'productNew', 'DocDate');
			//$scope.modelValidation('div_entryNo', 'productNew', 'GateEntryNo');
			$scope.modelValidation('div_PODate', 'productNew', 'PODate', 'PO Entry Date');
			$scope.manualValidationAddRemove('div_currency', 'productNew', 'CurrencyId');

			if ($scope.productNew.CurrencyId !== $scope.productNew.BaseCurrencyId)
				$scope.manualValidationAddRemove('div_rate  ', 'productNew', 'ToCurrencyRate');
			else
				manualValidation('div_rate', false);

			$scope.$broadcast('show-errors-check-validity');
			if ($scope.productNewForm.$valid) {
				
				if (new Date($scope.productNew.PODate) < new Date($scope.productNew.DocDate))
					return manualValidation('div_PODate', true, "PO date can't be less than Doc entry date");
				else
					manualValidation('div_PODate', false);

				$scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
				$scope.product = Object.assign({}, $scope.productNew);
				if ($scope.Action === "Save") {
					$http({
						method: 'POST',
						url: $scope.saveUrlFg,
						data: $scope.product,
						dataType: 'JSON'
					}).then(function (response) {
						if (response.data.Error === true) {
							ShowResult(response.data.Message, 'failure');
						}
						else {
							ShowResult(response.data.Message, 'success');
							$scope.productNew.Id = response.data.entity.Id;
							$scope.productNew.PartyName = $scope.product.PartyName;
							$scope.Action = "Update";
							//$scope.getDataList();
							$scope.getalldata();
						}
					}), function (response) {
						ShowResult(response.data.Message, 'failure');
					};
				}
				else if ($scope.Action === "Update") {

					$http({
						method: 'POST',
						url: $scope.updateUrlFG,
						data: $scope.product,
						dataType: 'JSON'
					}).then(function successCallback(response) {
						if (response.data.Error === true) {
							ShowResult(response.data.Message, 'failure');
						}
						else {
							ShowResult(response.data.Message, 'success');
							//$scope.getDataList();
							$scope.getalldata();

						}
					}, function errorCallBack(response) {
						ShowResult(response.data.Message, 'failure');
					});
				}
			}
		} catch (e) {
			throw e;
		}
	};

	$scope.closeServiceChargeTaxPopUpwindowFG = function () {
		getServiceChargeList($scope.productNew.Id);
		angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
	}


	//#endregion


	$scope.ServicePOCheckedByCboList = [];
	$scope.GetSupervisorCboList = function () {

		$http({
			method: 'GET',
			url: 'Products/PurchaseOrder/GetServicePOByReqSupervisorCbo'
		}).then(function successCallback(response) {
			$scope.ServicePOCheckedByCboList = response.data;
		});
	}
	$scope.GetSupervisorCboList();

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

	$scope.RowColor = "";
	$scope.isAlternative = -1;
	$scope.rowDataBound = function rowDataBound(e) {

		if ($scope.RowColor != e.data.MaterialGroupMasterName + e.data.UserName + e.data.StandardName + e.data.FirstCharacteristicsValue + e.data.SecondCharacteristicsValue + e.data.ThirdCharacteristicsValue) {
			$scope.isAlternative = $scope.isAlternative * -1;
			$scope.RowColor = e.data.MaterialGroupMasterName + e.data.UserName + e.data.StandardName + e.data.FirstCharacteristicsValue + e.data.SecondCharacteristicsValue + e.data.ThirdCharacteristicsValue;
		}
		if ($scope.isAlternative > 0)
			e.row.css("background-color", '#D3D3D3');
		else
			e.row.css("background-color", '#ffffff');


	}

	
	angular.isUndefinedOrNull = function (val) {
		return angular.isUndefined(val) || val === null || val === ""
	}
	function getTaxList(inveReveiveId) {

		$http({
			method: 'GET',
			url: $scope.path + 'GetTaxCategoryListPO?receiveDetailId=' + inveReveiveId
		}).then(function (response) {
			$scope.taxCategoryList = response.data;
		});
	}
	function checkChangeemployee(e) {
		var val = e.model.value;
		var hsnCodeId = $scope.GetListForMasterOrder[0].HSNCodeId;
		var row = $filter('filter')($scope.GetListForMasterOrder, { 'RequisitionDetailId': e.model.value });

		if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
			if (e.model.checkState == "check") {
				row[0].CheckedStatus = true;

			}
			else
				row[0].CheckedStatus = false;
		}

	}
	
	$scope.PODetailsUpdatePOPUp = function (x) {


		$scope.Action1 = 'Update';
		getInventoryMaterialListForUpdate(x.InventoryReceiveDetailId, x.MaterialMasterId, x.ArticleId, x.FirstCharacteristicsValueId, x.SecondCharacteristicsValueId, x.ThirdCharacteristicsValueId);
	};

	function getInventoryMaterialListForUpdate(inveReveiveId, MaterialMasterId, ArticleId, FirstCharacteristicsValueId, SecondCharacteristicsValueId, ThirdCharacteristicsValueId) {
		$scope.masterId = inveReveiveId;
		$http.get($scope.path + 'GetInventoryMaterialListForPOUpdate?inveReveiveId=' + inveReveiveId + '&MaterialMasterId=' + MaterialMasterId + '&ArticleId=' + ArticleId + '&FirstCharacteristicsValueId=' + FirstCharacteristicsValueId + '&SecondCharacteristicsValueId=' + SecondCharacteristicsValueId + '&ThirdCharacteristicsValueId=' + ThirdCharacteristicsValueId)
			.then(function (response) {
				$scope.GetListForMasterOrder = response.data;
			});
		angular.element(document.querySelector('#ListOfRequisition')).modal('show');

	}

	$scope.LoadAllReq = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/GetListForRequisition',
		}).then(function successCallback(response) {
			$scope.GetListForMasterOrder = response.data;
		});
	}

	$scope.updatelistforReq = function () {

		if ($scope.inventoryMaterialList.length > 0) {
			$scope.newlistitems = []; $scope.newlistitems1 = [];
			$scope.newlistitems1 = $scope.GetListForReQuisition;
			$scope.newlistitems = $scope.GetListForMasterOrder;
			$scope.GetListForMasterOrder = [];
			for (var i = 0; i < $scope.newlistitems.length; i++) {
				var getRow = $filter("filter")($scope.GetListForReQuisition, { "MaterialMasterId": $scope.newlistitems[i].MaterialMasterId, "ArticleId": $scope.newlistitems[i].ArticleId, "FirstCharacteristicsValueId": $scope.newlistitems[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.newlistitems[i].SecondCharacteristicsValueId, "ThitrdCharacteristicsValueId": $scope.newlistitems[i].ThitrdCharacteristicsValueId });
				if (getRow.length == 0) {
					$scope.newlistitems1.push($scope.newlistitems[i]);

				}
			}
			$scope.GetListForMasterOrder.push($scope.newlistitems1);
		}
		angular.element(document.querySelector('#ListOfRequisition')).modal('show');
	}


	$scope.tab1 = 1;
	$scope.setTabIndexByMaterial = function (newTab) {
		$scope.tab1 = newTab;

		$scope.getalldata();
	};
	$scope.isSetIndexByMaterial = function (tabNum) {
		return $scope.tab1 === tabNum;
	};

	$scope.setTabIndex1ByRequisition = function (newTab) {
		$scope.tab1 = newTab;
		$scope.getalldataIndexApp();
	};
	$scope.isSetIndex1ByRequisition = function (tabNum) {
		return $scope.tab1 === tabNum;
	};


	$window.onresize = function (event) {

		$scope.actionCompleteSelected();

	};
	$scope.actionCompleteSelected = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridReq").ejGrid("instance");
				var scrollerwidth = $("#ReqaddId").width();//Obtain the width of the container
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
		}
	};

	$scope.POTypeStatus = '';
	$scope.tab1 = 1;
	$scope.setTabIndex = function (newTab) {
		$scope.productNew.ServiceType = 'ServiceACK';
		$scope.POTypeStatus = 'ForChecked';
		$scope.getalldata();
		$scope.POListDetails();
		$scope.tab1 = newTab;
	};
	$scope.isSetIndex = function (tabNum) {
		return $scope.tab1 === tabNum;
	};


	$scope.setTabCheckedHR = function (newTab) {
		$scope.tab1 = newTab;
		$scope.productNew.ServiceType = 'ServiceACK';
		$scope.POTypeStatus = 'CheckedHoldRej';
		$scope.getalldata();
	};
	$scope.isSetCheckedHR = function (tabNum) {
		return $scope.tab1 === tabNum;
	};


	$scope.setTabChecked = function (newTab) {

		$scope.tab1 = newTab;
		$scope.productNew.POType = 'ServicePO';
		$scope.POTypeStatus = 'Checked';
		// alert('setTabChecked');
		$scope.getalldata();
	};
	$scope.isSetChecked = function (tabNum) {
		return $scope.tab1 === tabNum;
	};

	$scope.setTabApprovedHR = function (newTab) {

		$scope.tab1 = newTab;
		$scope.productNew.ServiceType = 'ServiceACK';
		$scope.ApproveRejectHold = 'HoldReject';
		$scope.getalldataIndexApp();
	};
	$scope.isSetApprovedHR = function (tabNum) {
		return $scope.tab1 === tabNum;
	};

	$scope.setTabIndex1 = function (newTab) {
		$scope.tab1 = newTab;
		$scope.productNew.ServiceType = 'ServiceACK';
		$scope.ApproveRejectHold = 'Approval';
		$scope.getalldataIndexApp();
	};
	$scope.isSetIndex1 = function (tabNum) {
		return $scope.tab1 === tabNum;
	};

	$window.onresize = function (event) {

		$scope.PendingReqScrollbar();

	};
	$scope.PendingReqScrollbar = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridReq11").ejGrid("instance");
				var scrollerwidth = $("#scrollReq").width();//Obtain the width of the container
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 5, height: 300 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
		}
	};


	//$scope.tab1 = newTab;
	$scope.setTabIndexByMaterial = function (newTab) {

		$scope.tab1 = newTab;
		$scope.getalldataListForReqList1();
	};
	$scope.setTabIndex1ByRequisition = function (tabNum) {
		return $scope.tab1 === tabNum;
	};

	$scope.setTabIndex1ByRequisition = function (newTab) {
		$scope.tab1 = newTab;
		$scope.getalldataListForReqList1();
	};
	$scope.isSetIndex1ByRequisition = function (tabNum) {
		return $scope.tab1 === tabNum;
	};



	//#region Scroll for Pending Tab 


	$window.onresize = function (event) {

		$scope.ScrollBYMaterial();

	};
	$scope.ScrollBYMaterial = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridReq1").ejGrid("instance");
				var scrollerwidth = $("#approved").width();//Obtain the width of the container
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
		}
	};




	$window.onresize = function (event) {

		$scope.ScrollBYRequisition();

	};
	$scope.ScrollBYRequisition = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridReq11").ejGrid("instance");
				var scrollerwidth = $("#approved").width();//Obtain the width of the container
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
		}
	};

	// #endregion

	//#region Order Specific Info
	
	$scope.calculateTaxAmountForMat = function (data) {
		if (baseService.isUndefinedOrNull(data.Percentage)) {
			data.Percentage = 0;
		}
		data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
	};
	$scope.checkRowValidationMat = function (x) {
		debugger;
		for (var i = 0; i < $scope.receiveTaxList.length; i++) {
			if (baseService.isUndefinedOrNull($scope.taxAbleAmnt) || $scope.taxAbleAmnt === 0) {
				ShowResult("Taxable Amount can not null or zero", 'failure', 'detailPopUp');
			}
			if ($scope.receiveTaxList[i].Id === x.Id) {
				$scope.receiveTaxList[i].Percentage = (parseFloat(x.TaxAmount / $scope.taxAbleAmnt).toFixed(4) * 100).toFixed(4);
			}

		}
	}


	$scope.taxCategoryListNew = [];
	$scope.getserviceTaxByTaxCategoryList = function ($event) {
		//debugger
		if ($event.isInteraction == false)
			return;
		var gridObj = $("#GridReq").ejGrid("instance");
		var currRow = gridObj.model.currentViewData[this.element.closest("tr").index()];
		var x = $event;
		var hsnCodeId = currRow.HSNCodeId;
		$scope.taxCategoryList = [];
		$http({
			method: 'GET'
			, url: $scope.path + 'getserviceTaxByTaxCategoryList?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.productNew.PODate
		}).then(function (response) {
			$scope.taxCategoryList = response.data;
			for (var i = 0; i < $scope.taxCategoryList.length; i++) {
				$scope.taxCategoryList[i].TaxAmount = (currRow.TotalServiceTranAmount * $scope.taxCategoryList[i].Percentage) / 100;
				$scope.taxCategoryList[i].ServiceMasterId = currRow.ServiceMasterId;
				if (!currRow.Active === false)
					$scope.taxCategoryListNew.push($scope.taxCategoryList[i]);
			}

		});

	}
	$scope.taxCategoryListNew = [];
	$scope.getserviceTaxByTaxCategoryList1 = function (hsnCodeId) {
		var hsnCodeId = hsnCodeId
		$scope.taxCategoryList = [];
		$http({
			method: 'GET'
			, url: $scope.path + 'getserviceTaxByTaxCategoryList?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId + '&PODate=' + $scope.productNew.PODate
		}).then(function (response) {
			$scope.taxCategoryList = response.data;
			for (var i = 0; i < $scope.taxCategoryList.length; i++) {
				$scope.taxCategoryList[i].ServiceMasterId = $scope.serviceModel.ServiceMasterId;
			}

		});





	}
	//#region Document Upload
	$scope.DocDownload = function (data) {
		$scope.dwonloadUrl = null;
		var str = data.UserFilename;
		var extention = str.substr(str.indexOf('.'));
		$scope.dwonloadUrl = virtualPath.ExpensesDocument + '/' + data.Id + extention;
	};

	$("#uploadBtn").change(function () {
		$scope.filedata = this.files[0];
	});
	document.getElementById("uploadBtn").onchange = function () {
		var filename = document.getElementById("uploadFile").value = this.value;
		var res = filename.replace(/C:\\fakepath\\/i, '');
		document.getElementById("uploadFile").value = res;

	};

	$scope.DocumentSave = function () {
		debugger;
		//$scope.$broadcast("show-errors-check-validity");

		if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
			throw $scope.filedata.name + ' File size must be below 2 mb';
		var fileName = null;
		if (!baseService.isUndefinedOrNull($scope.filedata))
			fileName = $scope.filedata.name;
		$scope.productDocMap.UserFilename = fileName;
		$scope.productDocMap.POId = $scope.productNew.Id;
		if (baseService.isUndefinedOrNull($scope.productDocMap.UserFilename)) {
			ShowResult('Select Attachment file');
			return false;
		}
		if (!baseService.isUndefinedOrNull($scope.productDocMap.UserFilename)) {
			if ($scope.productDocMap.UserFilename.length > 50) {
				throw "File Name must be less than 50 character.";
			}
		}
		for (var i = 0; i < $scope.Imagedata.length; i++) {
			var getRow = $filter("filter")($scope.Imagedata, { "UserFilename": $scope.productDocMap.UserFilename });
			if (getRow.length === 1) {
				ShowResult('File Already added');
				return false;
			}
		}

		try {

			var formData = new FormData();

			$http({
				method: "POST",
				url: 'Products/PurchaseOrder/ServicePODocCreate',
				headers: { 'Content-Type': undefined },
				transformRequest: function (data) {
					formData.append("PODocumentMap", angular.toJson($scope.productDocMap));
					if (baseService.isUndefinedOrNull($scope.filedata) === false) {
						formData.append('file', data.file);
					}
					return formData;
				},
				data: {
					"PODocumentMap": $scope.productDocMap,
					"file": $scope.filedata,
					"POId": $scope.productNew.Id,
				},
				dataType: "JSON"
			}).then(function successCallback(response) {
				if (response.data.Error === true) {
					ShowResult(response.data.Message, "failure");
				}
				else {
					ShowResult(response.data.Message, "success");
					$scope.ImagedataLoad();
					$scope.productDocMap.UserFilename = "";
					$scope.productDocMap.Description = "";
					$scope.productDocMap.Remarks = "";
				}
			}, function errorCallback(response) {
				ShowResult(response.status.Message, "failure");
			});
			return true;

		} catch (e) {
			throw ShowResult(e, "failure");
		}

		return true;
	};
	$scope.Imagedata = [];
	$scope.ImagedataLoad = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/ServicePODocumentMapData?POID=' + $scope.productNew.Id,
		}).then(function successCallback(response) { //datagatefun
			$scope.Imagedata = response.data;

		});
	};
	$scope.removePopUpForDoc = function (Id) {
		$scope.DocId = Id;
		$scope.message = 'Are you sure want to permanently delete this?';
		angular.element(document.querySelector('#removePopUpForDoc')).modal('show');
	};
	$scope.DeletePOIgame = function (Id) {

		if (!baseService.isUndefinedOrNull($scope.DocId)) {
			$http({
				method: 'POST',
				url: 'Products/PurchaseOrder/ServicePOImageDelete?Id=' + $scope.DocId,
				dataType: 'JSON'
			}).then(function (response) {
				if (response.data.Error === true)
					ShowResult(response.data.Message, 'failure');
				else {
					ShowResult(response.data.Message, 'success');
					$scope.ImagedataLoad();
				}
				function errorCallBack(response) {
					ShowResult(response.data.Message, 'failure');
				}
			});
		}


	};

	//#endregion 

	$scope.PaymentModeList = [];
	$scope.PaymentModeByPaymentTerm = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/PurchaseOrder/PaymentModeByPaymentTerm?Id=' + $scope.productNew.PaymentTermId
		}).then(function successCallback(response) {
			$scope.PaymentModeList = response.data;
			$scope.productNew.PaymentMode = response.data[0].PaymentMode;

		});
	}

	$scope.materialValidation = function () {
		$scope.ServiceMasterName = $.grep($scope.serviceList, function (item) {
			return item.Value === $scope.serviceModel.ServiceMasterId;
		})[0].Text;
		if ($scope.chargesList.length === 0) {
			$scope.invalid = true;
		}
		else {
			for (var i = 0; i < $scope.chargesList.length; i++) {
				var getRow3 = $filter("filter")($scope.chargesList, { "ServiceMasterId": $scope.serviceModel.ServiceMasterId, "ServiceName": $scope.ServiceMasterName});
				if (getRow3.length === 0) {
					$scope.invalid = true;
				}
				else {
					ShowResult('Service Combination Already Exist');
					$scope.invalid = false;
				}
			}

		}


	}
	$scope.detailSaveIndividualService = function () {
		//debugger;  
		try {
			
			if (baseService.isUndefinedOrNull($scope.serviceModel.Qty) || $scope.serviceModel.Qty === 0) {
				ShowResult('Enter the Qty');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.serviceModel.TransactionUoMId)) {
				ShowResult('Select The UoM');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.serviceModel.Rate) || $scope.serviceModel.Rate === 0) {
				ShowResult('Enter the Rate');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.serviceModel.TransactionAmount) || $scope.serviceModel.TransactionAmount === 0) {
				ShowResult('Enter the Qty and Rate');
				return false;
			}
			$scope.materialValidation();
			$scope.serviceModel.Amount = $scope.serviceModel.TransactionAmount;
			//$scope.invalid = true;
			if ($scope.invalid) {
				if ($scope.Action1 === 'Save') {
					$http({
						method: 'POST',
						url: $scope.detailSaveUrlIndependent,
						data: {
							'entity': $scope.serviceModel,
							'ServicePoMasterId': $scope.productNew.Id,
							'taxCategoryList': $scope.taxCategoryList
						},
						dataType: 'JSON'
					}).then(function successCallback(response) {
						if (response.data.Error === true)
							ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
						else {
							ShowResult(response.data.Message, 'success', 'serviceChargePopUp');
							$scope.taxCategoryListNew = [];
							getServiceChargeList($scope.productNew.Id);
							$scope.RequisitionListHide();
							$scope.setTabIndex(1);

						}
					}), function errorCallBack(response) {
						ShowResult(response.data.Message, 'failure', 'serviceChargePopUp');
					};

				}
				else if ($scope.Action1 === "Update") {
					$http({
						method: 'POST',
						url: $scope.detailUpdateUrl,
						data: {
							entity: $scope.GetListForMasterOrdernew
							, taxCategoryList: $scope.taxCategoryList
							, PoId: $scope.productNew.Id
							, groupList: $scope.groupList
						},
						dataType: 'JSON'
					}).then(function successCallback(response) {
						if (response.data.Error === true)
							ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
						else {
							ShowResult(response.data.Message, 'success', 'ListOfRequisition');
							getInventoryMaterialList($scope.productNew.Id);
						}
					}), function errorCallBack(response) {
						ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
					};

				}
			}
		} catch (e) {
			ShowResult(e, 'failure', 'ListOfRequisition');
		}
	};
	$scope.TaxOptionService = function (data) {
		debugger;
		$scope.productNew.TaxOptionService = data;

	};
	$scope.calculateTaxAmountForService = function (data) {
		if (baseService.isUndefinedOrNull(data.Percentage)) {
			data.Percentage = 0;
		}
		data.TaxAmount = Math.round($scope.serviceModel.TransactionAmount * data.Percentage) / 100;
	};
	$scope.checkRowValidationService = function (x) {
		debugger;
		for (var i = 0; i < $scope.taxCategoryList.length; i++) {
			if ($scope.taxCategoryList[i].Id === x.Id) {
				$scope.taxCategoryList[i].Percentage = (parseFloat(x.TaxAmount / $scope.serviceModel.TransactionAmount).toFixed(4) * 100);
			}

		}
	}
}

