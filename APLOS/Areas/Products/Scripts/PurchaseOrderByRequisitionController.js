'use strict';
PurchaseOrderByRequisitionController.$inject = ['accountService', 'addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function PurchaseOrderByRequisitionController(accountService, addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
	$rootScope.title = "Purchase Order By Requisition";
	$scope.Action = 'Save';
	$scope.Action1 = 'Save';
	$scope.index = -1;
	$scope.products = [];
	$scope.path = 'Products/PurchaseOrder/';
	$scope.getListUrl = $scope.path + 'getlist';
	$scope.saveUrl = $scope.path + 'CreatePOByReq';
	$scope.detailSaveUrl = $scope.path + 'DetailCreatePOByReq';
	$scope.detailUpdateUrl = $scope.path + 'DetailUpdatePOByReq';
	$scope.saveUrlFG = $scope.path + 'CreateFGMasterOrder';
	$scope.updateUrl = $scope.path + 'EditPOByReq';
	$scope.updateUrlFG = $scope.path + 'FGMasterOrderedit';
	$scope.deleteUrl = $scope.path + 'deletePOByReq/';
	$scope.detailDeleteUrl = $scope.path + 'DetailDeletePOByReq?receiveDetailId=';
	$scope.sreviceSaveUrl = $scope.path + 'ServiceChargesCreatePOByReq';
	$scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';
	$scope.PurchaseOrderFileLocation = virtualPath.PurchaseOrder;
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

	//#region notification setting

	$scope.NotificationSettingStatus = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/PurchaseOrder/NotificationSetting',
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
				url: 'Products/PurchaseOrder/GetCheckedByAndApprovedBY?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
				dataType: 'JSON'
			}).then(function successCallback(response) {
				$scope.checkedByList = response.data;
			});

		}
		else {

		}

	}
	$scope.AllTabPrint = function (z) {
		var x = "#" + z;
		var gridObj = $(x).data("ejGrid");
		var data = gridObj.getSelectedRecords()[0];
		location.href = "Products/PurchaseOrder/GePurchaseOrderReportByReq?purchaseOrderId=" + data.Id;
	};

	$scope.CheckAll = function ($event) {
		$scope.isselected = $event.target.checked;
		$scope.WantToClose = true;
		
	};


	$scope.checkVal = function () {
		alert('g');
		if ($scope.WantToClose) {
			$scope.result = "Checkbox checked";
		} else {
			$scope.result = "Checkbox Not checked";
		}
	}
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
		$http({
			method: 'GET',
			url: 'Products/PurchaseOrder/PODocumentMapDataAll'
		}).then(function successCallback(response) {
			$scope.lst = response.data;
			window.Img = response.data;
		});
	}
	$scope.PODocumentMapDataAll();

	$scope.data1 = $scope.lst;
	$scope.detailTemp = "#tabGridContents";
	$scope.detailgrid = function detailGridData(e) {
		var filteredData = e.data["Id"];
		var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("POmasterId", "equal", parseInt(filteredData), true).take(5));
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
		, EntityId: null
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
		, AuthorizedBy: null
		, CheckedByStatus: null
		, AuthorizedByStatus: null
		, PODate: $filter("dateFiltering")(Date.now())
		, labelCheckAndApproved: null
		, CheckedByStatusForNoti: null
		, ApprovedByStatusForNoti: null
		, DiscountAmount: 0
		, Tolerance: 0
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
	$scope.entityList = [];
	cboService.getCboEntityByPlant(null, null, "", function (result) {
		$scope.entityList = result;
	});
	$scope.countryList1 = [];
	$scope.CountryLoad = function () {
		addressService.getCountryCbo(function (result) {
			$scope.countryList1 = result;
		});
	}
	$scope.CountryLoad();
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

	

	$scope.Save = function () {

		try {
			$scope.dbval = $scope.StateData;
			$scope.UIval = $scope.productNew.InvoicingState;
			if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
				ShowResult("Please select to be approved by", 'failure');
				return false;
			}
			else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
				ShowResult("Please select to be checked by", 'failure');
				return false;
			}
			else if ($scope.inventoryMaterialList.length === 0) {
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
						url: $scope.saveUrl,
						data: {
							'entity': $scope.product,
							'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
							'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
						},
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
						url: $scope.updateUrl,
						data: {
							'entity': $scope.product,
							'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
							'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
						},
						dataType: 'JSON'
					}).then(function successCallback(response) {
						if (response.data.Error === true) {
							ShowResult(response.data.Message, 'failure');
						}
						else {
							ShowResult(response.data.Message, 'success');[]
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

	$scope.Delete = function () {
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
		$scope.NotificationSettingStatus();
		if (!$rootScope.isCollapsed) $rootScope.toggle();
		return true;
	};

	function ClearFields() {
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
		$scope.productNew.DiscountAmount = '0';
		$scope.productNew.Tolerance = '0';
		//$scope.getToCurrencyRate();
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

		// if ($scope.partyIndex !== -1) {
		//var party = $scope.partyList[$scope.partyIndex];
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
		$scope.PaymentModeByPaymentTerm();
		// }
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
	// #region Details
	$scope.businessProcesses = '';//"BP.BusinessProcessName IN('MaintenanceSpare','BOM','WetProcess','Consumable')";
	$scope.detailPopUp = function () {

		$scope.receiveTaxList = [];
		$scope.detailModel = {
			Id: null
			, CountryId: null
			, InventoryReceiveId: $scope.productNew.Id
			, MaterialStorageId: $scope.productNew.MaterialStorageId
			, CurrencyName: angular.element("#currency :selected").text()
			, CurrencyId: $scope.productNew.CurrencyId
			, BaseCurrencyId: $scope.baseCurrencyId
			, DocDate: $scope.productNew.DocDate
			, InventoryMaterialId: null
			, MaterialMasterId: null
			, MaterialMasterName: null
			, ArticleId: null
			, ArticleName: null
			, MaterialType: null
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
			, TransactionRate: null
			, TransactionAmount: null
			, BaseQty: null
			, BaseUOMId: null
			, BaseUoM: null
			, BaseUoMFactor: null

			, TotalQty: null
			, TotalAmount: 0
			, TotalTaxAmount: 0
			, AvgRate: null
			, ToCurrencyRate: $scope.productNew.ToCurrencyRate
			, IsNonCreditable: $scope.productNew.IsNonCreditable
			, IsOriginApplicable: false
			, PartyCode: null
			, CheckedStatus: null
			, RequisitionId: null
			, RequisitionDetailId: null
			, WantToClose: null
			, Tolerance: $scope.productNew.Tolerance

		};
		$scope.clearCharNames();
		angular.element(document.querySelector('#detailPopUp')).modal('show');
	};
	
	$scope.detailPopUpEdit = function () {

		for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
			if (baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].TransactionRate)) {
				ShowResult('Enter Rate', 'failure');
				return false;
			}
			else if ($scope.inventoryMaterialList[i].TransactionRate === 0 || $scope.inventoryMaterialList[i].TransactionRate === '0.00') {
				ShowResult('Enter Rate', 'failure');
				return false;
			}
			else if (baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].DeliveryDate)) {
				ShowResult('Enter Delivery Date', 'failure');
				return false;
			}
			for (var t = 0; t < $scope.inventoryMaterialList[i].TaxList.length; t++) {
				
				$scope.receiveTaxList.push($scope.inventoryMaterialList[i].TaxList[t]);

			}
		}
		

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
				
			}
		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};

	};
	$scope.MaterilaUpdate = function () {//calculateAmount
		for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
			for (var t = 0; t < $scope.inventoryMaterialList[i].TaxList.length; t++) {
				if (baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].TransactionRate)) {
					ShowResult('Enter Rate', 'failure');
					return false;
				}
				else if ($scope.inventoryMaterialList[i].TransactionRate === '0' || $scope.inventoryMaterialList[i].TransactionRate === '0.00') {
					ShowResult('Enter Rate', 'failure');
					return false;
				}
				else if (baseService.isUndefinedOrNull($scope.inventoryMaterialList[i].DeliveryDate)) {
					ShowResult('Enter Delivery Date', 'failure');
					return false;
				}
				else {
					$scope.receiveTaxList.push($scope.inventoryMaterialList[i].TaxList[t]);
				}

			}
		}

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
		for (var i = 0; i < $scope.GetListForMasterOrdernew.length; i++) {
			var getRow3 = $filter("filter")($scope.inventoryMaterialList, { "MaterialMasterId": $scope.GetListForMasterOrdernew[i].MaterialMasterId, "ArticleId": $scope.GetListForMasterOrdernew[i].ArticleId, "FirstCharacteristicsValueId": $scope.GetListForMasterOrdernew[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.GetListForMasterOrdernew[i].SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.GetListForMasterOrdernew[i].ThirdCharacteristicsValueId });
			if (getRow3 == 0) {
				$scope.invalid = true;
			}
			else {
				ShowResult('Material Combination Already Exist');
				$scope.invalid = false;
			}
		}
	}

	
	$scope.data = window.gridData;
	$scope.selectionMode = { selectionMode: ["row"] };
	$scope.GetListForMasterOrdernew = [];

	$scope.groupList = [];
	$scope.processgroupList = function (oldlist, newlist) {
		for (var i = 0; i < oldlist.length; i++) {
			var getRow = $filter("filter")(oldlist, { "MaterialMasterId": oldlist[i].MaterialMasterId, "ArticleId": oldlist[i].ArticleId, "FirstCharacteristicsValueId": oldlist[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": oldlist[i].SecondCharacteristicsValueId, "ThitrdCharacteristicsValueId": oldlist[i].ThitrdCharacteristicsValueId });
			var ExistingRow = $filter("filter")(newlist, { "MaterialMasterId": oldlist[i].MaterialMasterId, "ArticleId": oldlist[i].ArticleId, "FirstCharacteristicsValueId": oldlist[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": oldlist[i].SecondCharacteristicsValueId, "ThitrdCharacteristicsValueId": oldlist[i].ThitrdCharacteristicsValueId });
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
	$scope.detailSaveCall = function () {
		

		$scope.message = 'Are you want to close this line item for requisition?';
		angular.element(document.querySelector('#ConfirmationForReqClosePopUp')).modal('show');


	}


	$scope.detailSave = function () {
		try {
			
			$scope.GetListForMasterOrdernew = [];
			for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {

				if ($scope.GetListForMasterOrder[i].CheckedStatus === true) {
					if (((parseFloat($scope.GetListForMasterOrder[i].PORaisedQty) + parseFloat($scope.GetListForMasterOrder[i].TransactionQty)) > parseFloat($scope.GetListForMasterOrder[i].ReqQty))) {
						if (baseService.isUndefinedOrNull($scope.GetListForMasterOrder[i].AccessQtyReason)) {
							ShowResult('Please enter the reason for access', 'failure', 'ListOfRequisition');
							return false;
						}

					}
					if ($scope.GetListForMasterOrder[i].TransactionQty === '0' || $scope.GetListForMasterOrder[i].TransactionQty === '0.00' || $scope.GetListForMasterOrder[i].TransactionQty === '0.0') {
						ShowResult('Enter the current Qty.Zero not allowed', 'failure', 'ListOfRequisition');
						return false;
					}
					if ($scope.GetListForMasterOrder[i].IsOriginApplicable === true) {
						if (baseService.isUndefinedOrNull($scope.GetListForMasterOrder[i].CountryId)) {
							ShowResult('Please select the country', 'failure', 'ListOfRequisition');
							return false;
						}
					}

					if ($scope.GetListForMasterOrder[i].WantToClose === true) {
						$scope.GetListForMasterOrder[i].WantToClose = true;
						$scope.GetListForMasterOrder[i].POQtyStatus = true;
						$scope.GetListForMasterOrdernew.push($scope.GetListForMasterOrder[i]);
					}
					if ($scope.GetListForMasterOrder[i].WantToClose === false) {
						$scope.GetListForMasterOrder[i].WantToClose = false;
						$scope.GetListForMasterOrder[i].POQtyStatus = true;
						$scope.GetListForMasterOrdernew.push($scope.GetListForMasterOrder[i]);
					}
					
				}

			}


			$scope.processgroupList($scope.GetListForMasterOrdernew, $scope.groupList);

			if ($scope.Action1 === 'Save') {
				$scope.materialValidation();
				if ($scope.invalid) {

					$http({
						method: 'POST',
						url: $scope.detailSaveUrl,
						data: {
							entity: $scope.GetListForMasterOrdernew //JSON.stringify($scope.GetListForMasterOrdernew)
							, taxCategoryList: $scope.taxCategoryList
							, PoId: $scope.productNew.Id
							, groupList: $scope.groupList
							//, RequisitionDetailId: $scope.productNew.Id
						},
						dataType: 'JSON'
					}).then(function successCallback(response) {
						if (response.data.Error === true)
							ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
						else {
							ShowResult(response.data.Message, 'success', 'ListOfRequisition');
							                    
							getInventoryMaterialList($scope.productNew.Id);
							angular.element(document.querySelector('#ListOfRequisition')).modal('hide');
						}
					}), function errorCallBack(response) {
						ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
					};

				}
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
			//}
		} catch (e) {
			//ShowResult(e, 'fail', 'detailPopUp');
		}
	};

	$scope.detailSave1 = function () {
		//debugger;



		try {
			
			$scope.GetListForMasterOrdernew = [];
			for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {

				if ($scope.GetListForMasterOrder[i].CheckedStatus === true) {
					$scope.GetListForMasterOrder[i].POQtyStatus = false;
					$scope.GetListForMasterOrdernew.push($scope.GetListForMasterOrder[i]);


				}
				
			}
			$scope.processgroupList($scope.GetListForMasterOrdernew, $scope.groupList);
			$scope.materialValidation();
			if ($scope.invalid) {
				if ($scope.Action1 === 'Save') {
					$http({
						method: 'POST',
						url: $scope.detailSaveUrl,
						data: {
							entity: $scope.GetListForMasterOrdernew
							, taxCategoryList: $scope.taxCategoryList
							, PoId: $scope.productNew.Id
							, groupList: $scope.groupList
							//, RequisitionDetailId: $scope.productNew.Id
						},
						dataType: 'JSON'
					}).then(function successCallback(response) {
						if (response.data.Error === true)
							ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
						else {
							ShowResult(response.data.Message, 'success', 'ListOfRequisition');
							getInventoryMaterialList($scope.productNew.Id);
							angular.element(document.querySelector('#ConfirmationForReqClosePopUp')).modal('hide');


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
							angular.element(document.querySelector('#ConfirmationForReqClosePopUp')).modal('hide');
						}
					}), function errorCallBack(response) {
						ShowResult(response.data.Message, 'failure', 'ListOfRequisition');
					};

				}
			}
		} catch (e) {
			//ShowResult(e, 'fail', 'detailPopUp');
		}
	};



	$scope.valuePassInDelModal = function (id) {

		$scope.id = id.InventoryReceiveDetailId;
		$scope.message = 'Are you sure want to permanently delete this?';
		angular.element(document.querySelector('#removerPopUp')).modal('show');
	};
	$scope.detailDelete = function ($event) {
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
	
	
	function getTaxCategoryList(hsnCodeId) {
		
		$scope.taxCategoryList = [];
		$http({
			method: 'GET'
			, url: $scope.path + 'GetTaxCategoryList?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId
		}).then(function (response) {
			$scope.taxCategoryList = response.data;
		});
	}


	$scope.YesMessageForClosed = function ($event) {
		
		for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
			if ($scope.GetListForMasterOrder[i].CheckedStatus === true) {
				if ($scope.GetListForMasterOrder[i].RequisitionDetailId === $scope.RequisitionDetailId) {

					$scope.GetListForMasterOrder[i].WantToClose = true;
				}
			}
			else {
				$scope.GetListForMasterOrder[i].WantToClose = false;
			}
		}
	}
	$scope.NoMessageForClosed = function ($event) {
		
		for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
			if ($scope.GetListForMasterOrder[i].CheckedStatus === true) {
				if ($scope.GetListForMasterOrder[i].RequisitionDetailId === $scope.RequisitionDetailId) {

					$scope.GetListForMasterOrder[i].WantToClose = false;
				}
			}
			else {
				$scope.GetListForMasterOrder[i].WantToClose = false;
			}
		}
	}
	$scope.getTaxCategoryList1 = function ($event) {
		if ($event.isInteraction == false)
			return;
		var gridObj = $("#GridReq").ejGrid("instance");
		var currRow = gridObj.model.currentViewData[this.element.closest("tr").index()];

		var x = $event;
		for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
			if ($scope.GetListForMasterOrder[i].CheckedStatus === true) {
				$scope.RequisitionDetailId = currRow.RequisitionDetailId;
				if ($scope.GetListForMasterOrder[i].RequisitionDetailId === currRow.RequisitionDetailId) {

					if ((parseFloat($scope.GetListForMasterOrder[i].PORaisedQty) + parseFloat($scope.GetListForMasterOrder[i].TransactionQty)) > parseFloat($scope.GetListForMasterOrder[i].ReqQty)) {
						$scope.GetListForMasterOrder[i].WantToClose = true;
					}
					else if ((parseFloat($scope.GetListForMasterOrder[i].PORaisedQty) + parseFloat($scope.GetListForMasterOrder[i].TransactionQty)) === parseFloat($scope.GetListForMasterOrder[i].ReqQty)) {
						$scope.GetListForMasterOrder[i].WantToClose = true;
					}
					else {
						$scope.message = 'Do you want to close this line item?';
						angular.element(document.querySelector('#ConfirmationForReqClosePopUp')).modal('show');
					}
				}
			}
			else {
				$scope.GetListForMasterOrder[i].WantToClose = false;
			}
		}



		var hsnCodeId = currRow.hsnCodeId;
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
			url: 'Products/PurchaseOrder/GetTerms?id=' + id
		}).then(function successCallback(response) {
			$scope.paymentTermList1 = response.data;
			$scope.productNew.DeliveryInstruction = $scope.paymentTermList1[0].DeliveryInstruction;
			$scope.productNew.SpecialInstruction = $scope.paymentTermList1[0].SpecialInstruction;
			$scope.productNew.CheckedBy = $scope.paymentTermList1[0].CheckedBy;
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

	function getServiceChargeList(inveReveiveId) {

		$scope.chargesList = [];
		$http.get($scope.path + 'GetServiceChargeList?receiveId=' + inveReveiveId)
			.then(function (response) {
				$scope.chargesList = response.data;
				//$scope.ServiceId = $scope.chargesList[0].Id;
				$scope.GetServiceTaxData();
			});

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
	};

	$scope.inventoryReceiveReport = function (id, reportFormat) {
		if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
		$window.open('Products/InventoryReceive/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + id + '&plantId=' + $scope.productNew.PlantId, '_blank');
	};
	$scope.Griddata = [];
	$scope.POTypeStatus = 'Pending';
	$scope.getalldata = function () {
		if ($scope.POTypeStatus === 'Pending') {
			$scope.POTypeStatus = 'Pending'
		}
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/GetListForPOBYReq?POTypeStatus=' + $scope.POTypeStatus,
		}).then(function successCallback(response) {
			$scope.Griddata = response.data;
			for (var i = 0; i < $scope.Griddata.length; i++) {
				response.data[i].PODate = new Date($scope.Griddata[i].PODate);
			}
		});
	};
	$scope.getalldata();

	

	$scope.GriddataIndexApp = [];
	$scope.getalldataIndexApp = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/GetListForPOBYReq1?ApproveRejectHold=' + $scope.ApproveRejectHold,
		}).then(function successCallback(response) {
			$scope.GriddataIndexApp = response.data;
		});
	};

	$scope.searchByAppPO = "Id"; $scope.searchPO = "";
	$scope.searchByAppPOList = [{ value: 'Id', name: "PO No" }, { value: 'PODate', name: "PO Date" }, { value: 'PartyName', name: "Vendor" }
		, { value: 'RequisitionId', name: "Requisition No" }, { value: 'DocRefNo', name: "DocRef No" }
		];
	$scope.ApprovedPOList = [];
	$scope.getallApprovedData = function () {
		$http({
			method: 'POST',
			url: 'Products/PurchaseOrder/GetApprovedListForPOBYReq',
			data: { column: $scope.searchByAppPO, value: $scope.searchPO },
			dataType: 'JSON',
		}).then(function successCallback(response) {
			$scope.ApprovedPOList = response.data;
		});
	};
	
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

		var x = $event;
		var Id = x.data.Id;
		$scope.Currency = $("#currency option:selected").text();
		$scope.productNew = x.data;
		$scope.Id = $scope.productNew.Id;
		//$scope.LoadAllReq();
		$scope.GetTerms($scope.productNew.Id);
		getPartyPlantEditList($scope.productNew.InvoicingPartyPlantId, $scope.productNew.InvoicingByAddress, $scope.productNew.DeliveryPartyPlantId, $scope.productNew.DeliveryByAddress, $scope.productNew.DeliveryState, $scope.productNew.DeliveryGSTIN);

		getInventoryMaterialList($scope.productNew.Id);
		getServiceChargeList($scope.productNew.Id);
		if (baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
			$scope.CheckedByStatusForNoti = false;
			$scope.ApprovedByStatusForNoti = true;
			$scope.productNew.CheckedBy = x.data.ApprovedById;
		}
		else if (!baseService.isUndefinedOrNull(x.data.CheckedBy) && !baseService.isUndefinedOrNull(x.data.AuthorizedBy)) {
			$scope.CheckedByStatusForNoti = true;
			$scope.ApprovedByStatusForNoti = true;
			$scope.productNew.CheckedBy = x.data.CheckedById;
		}
		$scope.GetCheckedByAndApprovedBy1();
		if (baseService.isUndefinedOrNull(x.data.CheckedById) && !baseService.isUndefinedOrNull(x.data.ApprovedById)) {

			$scope.productNew.CheckedBy = x.data.ApprovedById;
			$scope.productNew.labelCheckAndApproved = 'To be approved by';
		}
		else if (!baseService.isUndefinedOrNull(x.data.CheckedById) && baseService.isUndefinedOrNull(x.data.ApprovedById)) {

			$scope.productNew.CheckedBy = x.data.CheckedById;
			$scope.productNew.labelCheckAndApproved = 'To be checked by';
		}

		$scope.ImagedataLoad($scope.productNew.Id);
		$scope.Action = 'Update';
		if (!$rootScope.isCollapsed) $rootScope.toggle();
	};
	
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
		//debugger;
		data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
		if (data.TransactionRate === 'NaN')
			data.TransactionRate = 0;
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
			data.BaseAmount = data.TrnAmount + data.BaseTaxAmount;
		}
		else {
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
		var data = gridObj.getSelectedRecords()[0];
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
	$scope.Griddata1 = [];
	$scope.onClickPO = function (args) {
		var gridObj = $("#Grid").data("ejGrid");
		$scope.data = gridObj.getSelectedRecords()[0];
		$scope.approveAlert();
	};
	cboService.getEnumCbo("enum/GetExpensesBookingApprovalStatusCbo", function (result) {
		$scope.approvalStatusList = result;
	});
	$scope.getalldata1 = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/getPendingList',
		}).then(function successCallback(response) {
			$scope.Griddata1 = response.data;
		});
	};
	$scope.Status = null;
	$scope.getalldata1();
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
		$scope.podata = gridObj.getSelectedRecords()[0];
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
	$scope.approvalAlert = function () {
		$scope.message = 'Are you sure want to Approve?';
		angular.element(document.querySelector('#poapprovealert')).modal('show');
	};
	$scope.GriddataPOClose = [];
	$scope.getalldataPOClose = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/GetListForPOClose',
		}).then(function successCallback(response) { //datagatefun
			$scope.GriddataPOClose = response.data;
		});
	};
	$scope.getalldataPOClose();


	$scope.onClickPOlock = function (args) {
		var gridObj = $("#Grid").data("ejGrid");
		$scope.data = gridObj.getSelectedRecords()[0];
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
	
	$scope.tab = 1;
	$scope.setTab = function (newTab) {
		$scope.tab = newTab;
	};
	$scope.isSet = function (tabNum) {
		return $scope.tab === tabNum;
	};
	
	$scope.Griddataapprovpo = [];
	$scope.Griddataapprovpo1 = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/getCheckedList',
		}).then(function successCallback(response) {
			$scope.Griddataapprovpo = response.data;
		});
	};
	$scope.Griddataapprovpo1();

	$scope.ListForPOApproval1UnApproved = [];
	$scope.GetListForPOApproval1UnApproved = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/GetListForPOApproval1UnApproved',
		}).then(function successCallback(response) {
			$scope.ListForPOApproval1UnApproved = response.data;
		});
	};
	$scope.GetListForPOApproval1UnApproved();


	$scope.onClickPOA1 = function (args) {
		var gridObj = $("#GridPO1").data("ejGrid");
		$scope.podata1 = gridObj.getSelectedRecords()[0];
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
	};
	$scope.GriddataPOlock = [];
	$scope.getalldataPOUnlock = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/GetListForPOUnClose',
		}).then(function successCallback(response) { //datagatefun
			$scope.GriddataPOlock = response.data;
		});
	};

	$scope.getalldataPOUnlock();

	$scope.onClickPOlock = function (args) {
		var gridObj = $("#GridUc").data("ejGrid");
		$scope.data = gridObj.getSelectedRecords()[0];
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

	$scope.tab = 1;
	$scope.setTab = function (newTab) {
		$scope.tab = newTab;
	};
	$scope.isSet = function (tabNum) {
		return $scope.tab === tabNum;
	};
	$scope.GriddataPOListforPoclosedui = [];
	$scope.getalldataPOListforPoclosedui = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/GetListForAllPOList',
		}).then(function successCallback(response) { //datagatefun
			$scope.GriddataPOListforPoclosedui = response.data;
		});
	};

	$scope.getalldataPOListforPoclosedui();

	$scope.onClickPoList = function (args) {
		var gridObj = $("#GridPOListforPoclosedui").data("ejGrid");
		$scope.data = gridObj.getSelectedRecords()[0];
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

	$scope.RequisitionList = function () {
		$scope.GetListForMasterOrder = [];
		$scope.groupList = [];
		$scope.GetListForMasterOrdernew = [];
		$scope.taxCategoryList = [];
		$scope.groupList = [];
		$scope.Action1 = 'Save';
		$scope.getalldataListForRequisitionList();
	};

	$scope.RequisitionListHide = function () {
		$scope.taxCategoryList = [];
		angular.element(document.querySelector('#ListOfRequisition')).modal('hide');
	};

	$scope.RequisitionList1 = function () {
		$scope.getalldataListForReqList1();
		angular.element(document.querySelector('#ListOfRequisition1')).modal('show');
	};

	$scope.RequisitionListtHide1 = function () {
		$scope.taxCategoryList = [];
		angular.element(document.querySelector('#ListOfRequisition1')).modal('hide');
	};

	$scope.GetListForMasterOrder = [];
	$scope.getalldataListForRequisitionList = function () {
		$scope.GetListForMasterOrder = [];
		$http({
			method: "POST",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/GetListForRequisition',
		}).then(function successCallback(response) { //datagatefun
			$scope.GetListForMasterOrder = [];
			$scope.GetListForMasterOrder = response.data.NewData;
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
			url: 'Products/PurchaseOrder/GetListForRequisition1',
		}).then(function successCallback(response) { //datagatefun
			$scope.GetListForMasterOrder1 = response.data;
		});
	};


	$scope.Getrecorddoubleclick = function ($event, index) {
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
		$scope.LoadTaxButtonClick();
		$scope.Currency = $("#currency option:selected").text();
		$scope.ServiceId = ServiceId;
		$scope.taxAbleAmnt = data.Amount;//+ data.TotalTaxAmount;
		$scope.percentageColumn = flag;
		$scope.currentMaterialRow = index;
		$scope.receiveTaxList = [];
		if ($scope.ChargeTaxList.length > 0) {
			$scope.HSNCode = $scope.ChargeTaxList[0].HSNCode;
			$scope.receiveTaxList = $filter('filter')($scope.ChargeTaxList, { 'InventoryServiceId': ServiceId });
		}
		$scope.total = 0;
		for (var j = 0; j < $scope.receiveTaxList.length; j++) {
			$scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
		}
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
		angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
	}

	$scope.checkedByList = [];
	$scope.GetSupervisorCboList = function () {

		$http({
			method: 'GET',
			url: 'Products/PurchaseOrder/GetSupervisorCbo'
		}).then(function successCallback(response) {
			$scope.checkedByList = response.data;
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

	$scope.calculateRequisitionData = function ($event) {
		//debugger;
		var x = $event;
		var Id = x.data.Id;
		for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {

			if ($scope.GetListForMasterOrder[i].RequisitionDetailId === x.data.RequisitionDetailId) {
				$scope.GetListForMasterOrder[i].CheckedStatus = false;
				$scope.GetListForMasterOrder[i].WantToClose = false;
			}
		}

		if ($scope.Action1 === 'Save') {
			x.data.TransactionAmount = (parseFloat(x.data.TransactionQty) * parseFloat(x.data.TransactionRate)).toFixed(2);
			x.data.BalanceQty = (parseFloat(x.data.ReqQty) - (parseFloat(x.data.PORaisedQty) + parseFloat(x.data.TransactionQty))).toFixed(2);
			if (x.data.TransactionQty > x.data.ReqQty) {
				x.data.BalanceQty = '0';
			}
			if (x.data.TransactionAmount === 'NaN')
				x.data.TransactionAmount = 0;
			var TransactionAmount1 = $filter('sumByKey')($filter('filter')($scope.GetListForMasterOrder), 'TransactionAmount');
			angular.forEach($scope.taxCategoryList, function (item) {
				item.TaxAmount = TransactionAmount1 * item.Percentage / 100;
				x.data.BaseTaxAmount += item.TaxAmount;
			});
			if ($scope.productNew.IsNonCreditable == 1) {
				if (x.data.BaseTaxAmount === null) {
					x.data.BaseTaxAmount = '0.00';
				}
				x.data.BaseAmount = parseFloat(x.data.TransactionAmount + x.data.BaseTaxAmount);
			}
			else {
				x.data.BaseAmount = x.data.TransactionAmount;
			}
		}
		else if ($scope.Action1 === 'Update') {

			x.data.TransactionAmount = (parseFloat(x.data.TransactionQty) * parseFloat(x.data.TransactionRate)).toFixed(2);
			x.data.BalanceQty = (parseFloat(x.data.ReqQty) - parseFloat(x.data.TransactionQty)).toFixed(2);
			if (x.data.TransactionQty > x.data.ReqQty) {
				x.data.BalanceQty = '0';
			}
			if (x.data.TransactionAmount === 'NaN')
				x.data.TransactionAmount = 0;
			var TransactionAmount1 = $filter('sumByKey')($filter('filter')($scope.GetListForMasterOrder), 'TransactionAmount');
			angular.forEach($scope.taxCategoryList, function (item) {
				item.TaxAmount = TransactionAmount1 * item.Percentage / 100;
				x.data.BaseTaxAmount += item.TaxAmount;
			});
			if ($scope.productNew.IsNonCreditable == 1) {
				if (x.data.BaseTaxAmount === null) {
					x.data.BaseTaxAmount = '0.00';
				}
				x.data.BaseAmount = parseFloat(x.data.TransactionAmount + x.data.BaseTaxAmount);
			}
			else {
				x.data.BaseAmount = x.data.TransactionAmount;
			}
		}
	};

	$scope.GerRequisition = function () {

		$.ajax({
			type: "POST",
			url: 'Products/PurchaseOrder/GetListForRequisition',
			data: {},
			dataType: "json",
			success: function (data) {
				$("#GridT").ejGrid({
					dataSource: data, // data must be array of json
					allowPaging: true,
					allowSorting: true,
					allowFiltering: true,
					isResponsive: true,
					allowResizeToFit: true,
					canResize: true,
					allowTextWrap: true,
					textWrapSettings: { wrapMode: "header" },
					enableResponsiveRow: true,
					filterSettings: {
						filterType: "excel"
					},
					cssClass: "filtered",
					pageSize: 10,
					allowScrolling: true,
					editSettings: { allowEditing: true },

					columns: [
						{ type: "checkbox", width: 50 },
						{ headerText: "ReqRowId", field: "MaterialReqqusitionMasterId", width: 80 },
						{ headerText: "MaterialGroup", field: "UserName", width: 120 },
						{ headerText: "Material Master", field: "UserName", width: 120 },
						{ headerText: "Article", field: "StandardName", width: 120 },
						{ headerText: "SKU1", field: "FirstCharacteristicsValue", width: 80 },
						{ headerText: "SKU2", field: "SecondCharacteristicsValue", width: 80 },
						{ headerText: "Entity", field: "EntityName", width: 80 },
						{ headerText: "ReqQty", field: "ReqQty", width: 100, textAlign: ej.TextAlign.Right },
						{ headerText: "PORaisedQty", field: "PORaisedQty", width: 100, textAlign: ej.TextAlign.Right },
						{ headerText: "CurrentQty", field: "TransactionQty", editType: ej.Grid.EditingType.InputTextBox, width: 100, textAlign: ej.TextAlign.Right },
						{ type: "textbox", width: 50 },
						{ headerText: "BalanceQty", field: "BalanceQty", width: 100, textAlign: ej.TextAlign.Right },
						{ headerText: "Rate", field: "TransactionRate", width: 100, textAlign: ej.TextAlign.Right },
						{ headerText: "Amount", field: "TransactionAmount", width: 100, textAlign: ej.TextAlign.Right }
					],
				});



			}//,

		});
	}



	// #region checkbox all

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

		//alert('dd');
		var val = e.model.value;
		var hsnCodeId = $scope.GetListForMasterOrder[0].HSNCodeId;
		// $scope.hsnCodeId = $event.data.hsnCodeId;

		//item level check
		var row = $filter('filter')($scope.GetListForMasterOrder, { 'RequisitionDetailId': e.model.value });

		if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
			if (e.model.checkState == "check") {
				row[0].CheckedStatus = true;

			}
			else
				row[0].CheckedStatus = false;
		}
		
	}
	function headCheckChangeemployee(e) {
		var val = e.model.value;
		var hsnCodeId = $scope.GetListForMasterOrder[0].HSNCodeId;
		var row = $filter('filter')($scope.GetListForMasterOrder, { 'RequisitionDetailId': e.model.value });

		if (e.model.checkState == "check") {
			// alert('2');

			// var gridObj = $("#Gridemployee").data("ejGrid");
			var filtered = $("#GridReq").data("ejGrid").getFilteredRecords();
			if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
				for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
					$scope.GetListForMasterOrder[i].CheckedStatus = true;
				}
			}
			else {
				for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
					for (var j = 0; j < filtered.length; j++) {
						if ($scope.GetListForMasterOrder[i].RequisitionDetailId == filtered[j].RequisitionDetailId)
							$scope.GetListForMasterOrder[i].CheckedStatus = true;
					}

				}
			}

			var checkbox = $("#GridReq .rowCheckbox").ejCheckBox();
			for (var i = 0; i < checkbox.length; i++) {
				$($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "change": null });
				$($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "checked": true });
				$($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
			}
		}
		else {
			var filtered = $("#GridReq").data("ejGrid").getFilteredRecords();
			if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
				for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
					$scope.GetListForMasterOrder[i].CheckedStatus = false;
				}
			}
			else {
				for (var i = 0; i < $scope.GetListForMasterOrder.length; i++) {
					for (var j = 0; j < filtered.length; j++) {
						if ($scope.GetListForMasterOrder[i].RequisitionDetailId == filtered[j].RequisitionDetailId)
							$scope.GetListForMasterOrder[i].CheckedStatus = false;
					}

				}
			}
			var checkbox = $("#GridReq .rowCheckbox").ejCheckBox();
			for (var i = 0; i < checkbox.length; i++) {
				$($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "change": null });
				$($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "checked": false });
				$($("#GridReq .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
			}
		}
		//header level check
	}
	$scope.dataBoundemployee = function (args) {
		$("#GridReq .rowCheckbox").ejCheckBox({ "change": checkChange });
		$("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

	}
	$scope.refreshTemplateemployee = function (args) {
		//alert('fff');
		if (args.rowIndex == 0) {
			$("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
		}

		var valobj = $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
		var val = $($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

		$($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
		var row = $filter('filter')($scope.GetListForMasterOrder, { 'RequisitionDetailId': val });
		if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
			if (row[0].CheckedStatus == true)
				$($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
			else
				$($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

		}
		$($("#GridReq .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
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
			method: "POST",
			dataType: 'JSON',
			url: 'Products/PurchaseOrder/GetListForRequisition',
		}).then(function successCallback(response) {
			$scope.GetListForMasterOrder = response.data.NewData;
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




	// #endregion

	$window.onresize = function (event) {

		$scope.actionCompleteSelected();

	};
	$scope.actionCompleteSelected = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridReq").ejGrid("instance");
				var scrollerwidth = $("#ReqaddId").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};

	//#region Purchase-Order-By-Requisition--- All Tab
	$scope.POTypeStatus = '';
	$scope.tab1 = 1;
	$scope.setTabIndex = function (newTab) {
		$scope.tab1 = newTab;
		$scope.POTypeStatus = 'Pending';
		$scope.getalldata();
	};
	$scope.isSetIndex = function (tabNum) {
		return $scope.tab1 === tabNum;
	};




	$scope.setTabCheckedHR = function (newTab) {
		$scope.tab1 = newTab;
		$scope.POTypeStatus = 'CheckedHoldRej';
		// alert('setTabCheckedHR');
		$scope.getalldata();
	};
	$scope.isSetCheckedHR = function (tabNum) {
		return $scope.tab1 === tabNum;
	};


	$scope.setTabChecked = function (newTab) {

		$scope.tab1 = newTab;
		$scope.POTypeStatus = 'Checked';
		// alert('setTabChecked');
		$scope.getalldata();
	};
	$scope.isSetChecked = function (tabNum) {
		return $scope.tab1 === tabNum;
	};



	$scope.setTabApprovedHR = function (newTab) {

		$scope.tab1 = newTab;
		$scope.ApproveRejectHold = 'HoldReject';
		// alert('setTabApprovedHR');
		$scope.getalldataIndexApp();
	};
	$scope.isSetApprovedHR = function (tabNum) {
		return $scope.tab1 === tabNum;
	};

	$scope.setTabApproved = function (newTab) {
		$scope.tab1 = newTab;
		$scope.getallApprovedData();
	};
	$scope.isSetApproved = function (tabNum) {
		return $scope.tab1 === tabNum;
	};


	// #endregion



	$window.onresize = function (event) {

		$scope.PendingReqScrollbar();

	};
	$scope.PendingReqScrollbar = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#GridReq11").ejGrid("instance");
				var scrollerwidth = $("#scrollReq").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 5, height: 300 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};



	//$scope.tab1 = newTab;
	$scope.setTabIndexByMaterial = function (newTab) {

		$scope.tab1 = newTab;
		//$scope.ApproveRejectHold = 'HoldReject';
		//// alert('setTabApprovedHR');
		//$scope.getalldataIndexApp();
		$scope.getalldataListForReqList1();
	};
	$scope.setTabIndex1ByRequisition = function (tabNum) {
		return $scope.tab1 === tabNum;
	};

	$scope.setTabIndex1ByRequisition = function (newTab) {
		$scope.tab1 = newTab;
		$scope.getalldataListForReqList1();
		//$scope.ApproveRejectHold = 'Approval';
		//$scope.getalldataIndexApp();
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

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
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

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};

	$scope.PaymentModeList = [];
	$scope.PaymentModeByPaymentTerm = function () {
		//debugger;
		$http({
			method: 'GET',
			//url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
			url: 'Products/PurchaseOrder/PaymentModeByPaymentTerm?Id=' + $scope.productNew.PaymentTermId
		}).then(function successCallback(response) {
			$scope.PaymentModeList = response.data;
			$scope.productNew.PaymentMode = response.data[0].PaymentMode;

		});
	}


	// #endregion

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
				url: 'Products/PurchaseOrder/PODocCreate',
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
			//url: $scope.getSearchListUrl,
			url: 'Products/PurchaseOrder/PODocumentMapData?POID=' + $scope.productNew.Id,
		}).then(function successCallback(response) { //datagatefun
			$scope.Imagedata = response.data;

		});
	};
	$scope.removePopUpForDoc = function (Id) {
		debugger;
		$scope.DocId = Id;
		$scope.message = 'Are you sure want to permanently delete this?';
		angular.element(document.querySelector('#removePopUpForDoc')).modal('show');
	};
	$scope.DeletePOIgame = function (Id) {
		debugger;
		if (!baseService.isUndefinedOrNull($scope.DocId)) {
			$http({
				method: 'POST',
				url: 'Products/PurchaseOrder/POImageDelete?Id=' + $scope.DocId,
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
}

