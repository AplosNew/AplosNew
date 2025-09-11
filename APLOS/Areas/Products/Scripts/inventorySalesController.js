'use strict';
inventorySalesController.$inject = ['accountService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function inventorySalesController(accountService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
	$rootScope.title = "Inventory Sales Register";
	$scope.Action = 'Save';
	$scope.index = -1;
	$scope.products = [];
	$scope.CustomerList = [];
	$scope.PostingStockBeyondIssueDateList = [];
	$scope.PostingStockList = [];
	$scope.UnApprovedStockDetailBeyondIssueDateList = [];
	$scope.ApprovedStockBeyondIssueDateList = [];
	$scope.UnApprovedStockList = [];
	$scope.ApprovedStockList = [];

	$scope.tax = {
		IncludingTax: true
	};

	$scope.partyType = "Customer";
	$scope.path1 = 'Products/PurchaseOrder/';
	$scope.path = 'Products/InventoryIssue/';
	$scope.getListUrl = $scope.path + 'GetDataByInventoryIssue';
	$scope.saveUrl = $scope.path + 'InventorySalesCreate';
	$scope.updateUrl = $scope.path + 'edit';
	$scope.deleteUrl = $scope.path + 'DeleteSalesDetail/';
	$scope.sreviceSaveUrl = $scope.path + 'SalesServiceChargesCreate/';
	$scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';

	$scope.currentDate = new Date(Date.now());
	$controller("partyBaseController", { $scope: $scope, $http: $http });
	$controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
	$controller("employeeBaseController", { $scope: $scope, $http: $http });
	$scope.tab = 1;

	$http({
		method: 'GET',
		url: 'currencies/CompanyParallelCurrency/CboParallelCurrency'
	}).then(function successCallback(response) {
		$scope.baseCurrencyId = response.data[0].Value;
		$scope.productNew.BaseCurrencyId = response.data[0].Value;
		//factoryService.getCurrencyPrecision($scope.baseCurrencyId);
	});
	$scope.setTab = function (newTab) {
		$scope.tab = newTab;
	};
	$scope.isSet = function (tabNum) {
		return $scope.tab === tabNum;
	};
	$scope.GridInventorySalesdata = [];
	$scope.getdataInventorySales = function () {
		//debugger;
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/InventoryIssue/GetDataByInventorySales?tabType=' + $scope.tabType,
		}).then(function successCallback(response) {
			$scope.GridInventorySalesdata = response.data;
			//entrydata = copy(searchdata);
		});

	};
	//$scope.getdataInventorySales();

	//#region Index Tab
	$scope.tab = 1;
	$scope.tabType = 1;

	$scope.getdataInventorySales($scope.tabType);
	$scope.setTabFirst = function (newTab) {

		$scope.tab = newTab;
		$scope.tabType = '1';
		$scope.getdataInventorySales($scope.tabType);

	};
	$scope.isSetFirst = function (tabNum) {
		return $scope.tab === tabNum;
	};


	$scope.setTabSecond = function (newTab) {
		//debugger;
		$scope.tabType = '2';
		$scope.tab = newTab;

		$scope.getdataInventorySales($scope.tabType);

	};
	$scope.isSetSecond = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabThird = function (newTab) {
		$scope.tab = newTab;
		$scope.tabType = '3';
		$scope.getdataInventorySales($scope.tabType);
	};
	$scope.isSetThird = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabFourth = function (newTab) {
		$scope.tab = newTab;
		$scope.tabType = '4';
		$scope.getdataInventorySales($scope.tabType);

	};
	$scope.isSetFourth = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabFifth = function (newTab) {
		$scope.tab = newTab;
		$scope.tabType = '5';

		$scope.getdataInventorySales($scope.tabType);

	};
	$scope.isSetFifth = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabSixth = function (newTab) {
		$scope.tab = newTab;
		$scope.tabType = '6';
		$scope.getdataInventorySales($scope.tabType);

	};
	$scope.isSetSixth = function (tabNum) {
		return $scope.tab === tabNum;
	};

	//Test Control


	//#endregion
	$scope.product = {
		Id: null
		, ComapnyGroupId: null
		, CompanyId: null
		, PlantId: null
		, PlantName: null
		, EntityId: null
		, EntityName: null
		, MaterialStorageId: null
		, SalesDate: $filter("dateFiltering")(Date.now())
		, Remarks: null
		, EmployeeId: null
		, EmployeeName: null
		, IssueType: 'Revenue'
		, IssueRequestMasterId: null
		, SlipAssetIssueTypeStatus: 'Asset'
		, OrderRefNo: null
		, PartyId: null
		, PartyName: null
		, CheckedBy: null
		, CheckedByStatus: null
		, ApprovedBy: null
		, ApprovedByStatus: null
		, CustomerId: null
		, ChangeInvoicingStateId: null
		, PlantStateId: null
		, InvoicingPartyPlantId: null
		, DeliveryPartyPlantId: null
		, InvoicingByAddress: null
		, DeliveryByAddress: null
		, InvoicingState: null
		, InvoicingGSTIN: null
		, DeliveryState: null
		, DeliveryGSTIN: null
		, InvoicingStateId: null
		, ToCurrencyRate: null
		, DocRefNo: null
		, DocDate: null//$filter("dateFiltering")(Date.now())
		, NoteForAccounts: null
		, CurrencyId: null
		, TaxOption: 'Yes'
		, TaxOptionMat: 'Yes'
		, TaxOptionService: 'Yes'
		, TaxOptionServiceModify: 'Yes'
		, TaxOptionAddiTax: 'Yes'
		, PaymentTermId: null
		, BaseOnDueDate: null
		, BaseNoOfDays: null
		, MatureDate: null
		, IsPaymentTermChangeable: null
		, Summery: null
		, Details: null
	};
	$scope.IssueType = 'Revenue';
	$scope.productNew = Object.assign({}, $scope.product);
	$scope.AllTabPrint = function (z) {
		//debugger;
		var x = "#" + z;
		var gridObj = $(x).data("ejGrid");
		var data = gridObj.getSelectedRecords()[0];
		location.href = "Products/InventoryIssue/inventorySalesReportPrint?grnId=" + data.Id;

	};

	$scope.InventoryPreSalesPrint = function (z) {
		//debugger;
		var x = "#" + z;
		var gridObj = $(x).data("ejGrid");
		var data = gridObj.getSelectedRecords()[0];
		location.href = "Products/InventoryIssue/inventoryPreSalesReportPrint?grnId=" + data.Id;

	};

	$http({
		method: 'GET',
		url: 'Materials/MaterialStorage/getcbo'
	}).then(function (response) {
		$scope.storageList = response.data;
	});

	//#region notification setting

	$scope.NotificationSettingStatus = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/InventoryIssue/NotificationSetting',
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
				url: 'Products/InventoryIssue/GetCheckedByAndApprovedBY?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
				dataType: 'JSON'
			}).then(function successCallback(response) {
				$scope.checkedByList = response.data;
			});

		}
		else {

		}

	}




	//#endregion

	//#region Customer Load

	$scope.invoicingPartyPopUp = function () {
		//debugger;
		angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
	};
	$scope.closePartyPopUp = function (x) {
		//if ($scope.partyIndex !== -1) {
		$scope.productNew.IsPaymentTermChangeable = '';
		$scope.productNew.PaymentTermId = '';
		var party = x.data;// $scope.partyList[$scope.partyIndex];
		$scope.productNew.PartyName = party.Code + " - " + party.UserName;
		$scope.productNew.PartyId = party.Id;
		$scope.productNew.PaymentTermId = party.PaymentTermId;
		$scope.productNew.CurrencyId = party.CurrencyId;
		$scope.productNew.IsPaymentTermChangeable = party.IsPaymentTermChangeable;
		$scope.productNew.PaymentTermId = party.PaymentTermId;
		// $scope.GetCurrencyExchangeRateList();
		//  $scope.changePaymentTerm($scope.salesVM.PaymentTermId);
		$scope.partyPlantList = [];
		$scope.getCboPartyPlantList(party.Id, function (result) {
			$scope.partyPlantList = result;
			angular.forEach($scope.partyPlantList, function (item, i) {
				if (item.IsDefault) {
					$scope.partyPlantId = item.Value;
					$scope.productNew.InvoicingPartyPlantId = item.Value;
					$scope.productNew.DeliveryPartyPlantId = item.Value;
					$scope.productNew.InvoicingByAddress = item.Address1;
					$scope.productNew.DeliveryByAddress = item.Address1;
					$scope.productNew.InvoicingState = item.StateName;
					$scope.productNew.InvoicingGSTIN = item.GSTIN;
					$scope.productNew.DeliveryState = item.StateName;
					$scope.productNew.DeliveryGSTIN = item.GSTIN;
					$scope.productNew.InvoicingStateId = item.StateId;

				}
			});
		});
		//}
		$scope.hidePartyPopUp();
	};
	$scope.closeInvoicingPartyPopUp = function () {
		//if ($scope.salesMaterialList.length || $scope.chargesList.length) {

		if (!baseService.isUndefinedOrNull($scope.productNew.ChangeInvoicingStateId)) {
			if ($scope.productNew.PlantStateId == $scope.productNew.InvoicingStateId == $scope.productNew.ChangeInvoicingStateId)
				angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
			else if ($scope.productNew.InvoicingStateId == $scope.productNew.ChangeInvoicingStateId)
				angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
			else if ($scope.productNew.PlantStateId != $scope.productNew.InvoicingStateId && $scope.productNew.PlantStateId != $scope.productNew.ChangeInvoicingStateId)
				angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
			else
				ShowResult('Change is not allowed', 'failure', 'invoicingPartyPopUp');
		}
		else
			angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
		//}
		//else
		// angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');

	};

	$scope.billShippAddress = function (id, flag) {
		if (!baseService.isUndefinedOrNull(id)) {
			var address = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].Address1;
			var state = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateName;
			var stateId = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateId;
			if (flag === 'billTo') {
				$scope.productNew.InvoicingState = state;
				$scope.productNew.ChangeInvoicingStateId = stateId;
				$scope.productNew.InvoicingGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
				return $scope.productNew.InvoicingByAddress = address;
			}
			else if (flag === 'shipTo') {
				$scope.productNew.DeliveryState = state;
				$scope.productNew.DeliveryGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
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

	//#endregion
	$scope.currencyList = [];
	cboService.getCboTransactionCurrencyByCompany('', function (result) {
		$scope.currencyList = result;
	});
	$scope.getToCurrencyRate = function () {
		//debugger;
		if (baseService.isUndefinedOrNull($scope.productNew.DocDate)) {
			$scope.productNew.ToCurrencyRate = 1;
			return;
		}
		$http.get($scope.path1 + 'GetToCurrencyRate?currencyId=' + $scope.productNew.CurrencyId + '&baseCurrencyId=' + $scope.productNew.BaseCurrencyId + '&docDate=' + $filter('dateFiltering')($scope.productNew.DocDate))
			.then(function (response) {
				if (parseFloat(response.data) === 0)
					$scope.productNew.ToCurrencyRate = 1;
				else
					$scope.productNew.ToCurrencyRate = response.data;
			});
	};
	cboService.getCboEntityByPlant(null, null, '', function (result) {
		$scope.EntityList = result;
	});
	$scope.CountryLoadData = function () {
		$scope.countryList = [];
		$http({
			method: 'POST',
			url: 'Products/inventoryIssue/CountryLoad',//?entity=' + $scope.detailModel, 
			data: { entity: $scope.detailModel },
			dataType: 'JSON'
		}).then(function (response) {
			$scope.countryList = response.data;
		});
	}

	function getMaterialStock() {
		$http({
			method: 'POST',
			url: $scope.path + 'GetStockSales',
			data: { entity: $scope.detailModel, issueDate: $scope.productNew.SalesDate },
			dataType: 'JSON'
		}).then(function (response) {
			$scope.detailModel.TotalQty = response.data.TotalQty;
			$scope.detailModel.PostingQty = response.data.PostingQty;
			$scope.detailModel.PostingQuantity = response.data.PostingQuantity;
			$scope.detailModel.ApprovedQty = response.data.ApprovedQty;
			$scope.detailModel.UnApprovedQty = response.data.UnApprovedQty;
			if (baseService.isUndefinedOrNull($scope.detailModel.TotalQty))
				$scope.errorText = 'This material has no stock';
			else $scope.errorText = null;
		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};
	}
	$scope.selectMaterialByType = function (ob) {
		//debugger;
		if (ob.IsAsset) return ShowResult('Fixed Asset  can not Issue through this Screen .', '', 'materialMasterbyTypePopup');
		if (!ob.hasInventory) return ShowResult('Material stock does not exist.', '', 'materialMasterbyTypePopup');
		$scope.detailModel.MaterialMasterId = ob.Id;
		$scope.detailModel.MaterialMasterName = ob.UserName;
		$scope.detailModel.BaseUOMId = ob.BaseUOMId;
		$scope.detailModel.BaseUoM = ob.BaseUoM;
		$scope.detailModel.OurStyleName = ob.OurStyleName;
		$scope.detailModel.MaterialGroupMasterName = ob.MaterialGroupMasterName;
		$scope.detailModel.MaterialGroupMasterId = ob.MaterialGroupMasterId;
		$scope.detailModel.ProductMasterName = ob.ProductMasterName;
		$scope.detailModel.IsOurStyleRequired = ob.IsOurStyleRequired;
		$scope.detailModel.IsProductMstRequired = ob.IsProductMstRequired;
		$scope.detailModel.TransactionUoMId = ob.BaseUOMId;
		$scope.detailModel.ArticleId = null;;
		$scope.detailModel.ArticleName = null;
		$scope.detailModel.FirstCharacteristicsValueId = null;
		$scope.detailModel.SecondCharacteristicsValueId = null;
		$scope.detailModel.ThirdCharacteristicsValueId = null;
		$scope.detailModel.IsOriginApplicable = ob.IsOriginApplicable;
		$scope.detailModel.HSNCode = ob.HSNCode;

		$scope.hasArticle = ob.HasAttribute;
		$scope.hasSku = ob.WithSKU;
		if (ob.HasAttribute) $scope.getArticleSearchList(ob.Id);
		if (ob.WithSKU) $scope.getCharacteristicsList(ob.Id);
		if (!ob.HasAttribute && !ob.WithSKU)
			getMaterialStock();
		$scope.CountryLoadData();
		getTaxCategoryList(ob.HSNCodeId);
		var mmId = []; mmId.push(ob.Id);
		cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (result) {
			$scope.uoMList = result;
		});
		manualValidation('div_mm', false);
		manualValidation('div_qty', false);
		if ($scope.IssueType == 'Revenue') {
			if (!ob.HasAttribute && !ob.WithSKU) {
				//$scope.getBudgetActivityInIssueMaterial(ob.MaterialGroupMasterId);

			}
		}
		$scope.detailModel.TransactionQty = "";
		$scope.detailModel.SalesRate = "";

		$scope.closeMaterialMasterbyTypePopUp();
	};
	$scope.selectarticle = function (ob) {
		//debugger;
		try {
			$scope.detailModel.ArticleId = ob.Id;
			$scope.detailModel.ArticleName = ob.StandardName;
			if (ob.HSNCode)
			$scope.detailModel.HSNCode = ob.HSNCode;
			$scope.detailModel.ArticleName = ob.StandardName;
			manualValidation('div_ar', false);
			if (!ob.WithSKU)
				getMaterialStock();
			$scope.CountryLoadData();
			//getTaxCategoryList(ob.HSNCodeId);
			if ($scope.IssueType == 'Revenue') {
				//if (!ob.WithSKU)
				//	$scope.getBudgetActivityInIssueMaterial($scope.detailModel.MaterialGroupMasterId);
			}
			angular.element(document.querySelector('#articleSearchPop')).modal('hide');
		} catch (e) {
			ShowResult(e, '', 'articleSearchPop');
		}
	};
	function getTaxCategoryList(hsnCodeId) {
		//$http({
		//	method: 'GET',
		//	url: 'Accounts/TaxCategory/GetTaxCategoryList?partyPlantId=' + $scope.productNew.InvoicingPartyPlantId + '&hsnCodeId=' + hsnCodeId + '&SalesDate' + $scope.productNew.SalesDate
		//}).then(function (response) {
		//	$scope.materialtaxCategoryList = response.data;
		//	// $scope.sevtaxCategoryList = response.data;
		//});
		$scope.materialtaxCategoryList = [];
		$http({
			method: 'GET',
			//url: $scope.path1 + 'GetTaxCategoryListForSalesService?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId + '&InventorySalesDate=' + $scope.productNew.SalesDate
			url: $scope.path1 + 'GetTaxCategoryListForSalesMaterial?partyPlantId=' + $scope.productNew.InvoicingPartyPlantId + '&hsnCodeId=' + hsnCodeId + '&InventorySalesDate=' + $scope.productNew.SalesDate
		}).then(function (response) {

			$scope.materialtaxCategoryList = response.data;
			// $scope.sevtaxCategoryList = response.data;
		});
	}
	$scope.AmountCalculation = function () {
		//debugger;
		$scope.detailModel.TotalAmount = (parseFloat($scope.detailModel.TransactionQty) * ($scope.detailModel.SalesRate)).toFixed(4);
		$scope.calculateTaxCategory();
	}
	$scope.RateCalculation = function () {
		//debugger;
		$scope.detailModel.SalesRate = (parseFloat(($scope.detailModel.TotalAmount) / $scope.detailModel.TransactionQty)).toFixed(4);
		$scope.calculateTaxCategory();
	}
	$scope.calculateTaxCategory = function () {
		//debugger;
		$scope.detailModel.TotalTaxAmount = 0;
		var tQty = baseService.isUndefinedOrNull($scope.detailModel.TransactionQty) ? 0 : parseFloat($scope.detailModel.TransactionQty);
		var tAmount = baseService.isUndefinedOrNull($scope.detailModel.TotalAmount) ? 0 : parseFloat($scope.detailModel.TotalAmount);
		//if (tQty > 0 && tAmount > 0)
		//    $scope.detailModel.SalesRate = tAmount / tQty;
		//else
		//    $scope.detailModel.SalesRate = 0;
		for (var i = 0; i < baseService.arrayLength($scope.materialtaxCategoryList); i++) {
			$scope.materialtaxCategoryList[i].TaxAmount = ((parseFloat($scope.materialtaxCategoryList[i].Percentage) * $scope.detailModel.TotalAmount) / 100).toFixed($rootScope.currencyPrecision);
			$scope.detailModel.TotalTaxAmount = (parseFloat($scope.detailModel.TotalTaxAmount) + parseFloat($scope.materialtaxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
		}
		if (isNaN($scope.detailModel.TotalTaxAmount)) $scope.detailModel.TotalTaxAmount = 0;
	};


	//#region service Add/delete
	$http.get('Setups/CompanyServiceMaster/GetCboList')

		.then(function (response) {
			$scope.serviceList = response.data;
		});
	$scope.changeService = function () {
		//debugger;
		if (baseService.isUndefinedOrNull($scope.serviceModel.ServiceMasterId))
			return $scope.taxCategoryList = [];
		var hsnCodeId = $.grep($scope.serviceList, function (item) { return item.Value === $scope.serviceModel.ServiceMasterId; })[0].HSNCodeId;
		getTaxCategoryList1(hsnCodeId);
	};
	function getTaxCategoryList1(hsnCodeId) {
		$scope.taxCategoryList = [];
		$http({
			method: 'GET'
			, url: $scope.path1 + 'GetTaxCategoryListForSalesService?receiveId=' + $scope.productNew.Id + '&hsnCodeId=' + hsnCodeId + '&InventorySalesDate=' + $scope.productNew.SalesDate
		}).then(function (response) {
			$scope.taxCategoryList = response.data;
		});
	}
	$scope.closeServiceChargePopUp = function () {
		$scope.serviceModel = {};
		$scope.receiveTaxList = [];
		angular.element(document.querySelector('#serviceChargePopUp')).modal('hide');
	};
	$scope.serviceChargePopUp = function () {
		//if (baseService.arrayLength($scope.detailList) === 0)
		//	return ShowResult('Without material charges not aplicable.');
		$scope.taxCategoryList = [];
		$scope.serviceModel = {
			Id: null
			, ServiceMasterId: null
			, InventorySalesId: $scope.productNew.Id
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
	$scope.serviceModel = {
		Id: null
		, ServiceMasterId: null
		, InventorySalesId: $scope.productNew.Id
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
	$scope.serviceSave = function () {
		try {
			//$scope.manualValidationAddRemove('div_svc', 'serviceModel', 'ServiceMasterId');
			//$scope.manualValidationAddRemove('div_svcRate', 'serviceModel', 'TransactionAmount', 'Amount');
			if (baseService.isUndefinedOrNull($scope.serviceModel.TransactionAmount)) {
				ShowResult('Enter Amount', 'failure', 'serviceChargePopUp');
				return false;
			}
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
						, InventorySalesId: $scope.productNew.Id
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
	$scope.calculateSvcTaxCategory = function () {
		$scope.serviceModel.TotalTaxAmount = 0;
		for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
			$scope.taxCategoryList[i].TaxAmount = ((parseFloat($scope.taxCategoryList[i].Percentage) * $scope.serviceModel.TransactionAmount) / 100).toFixed($rootScope.currencyPrecision);
			$scope.serviceModel.TotalTaxAmount = (parseFloat($scope.serviceModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
		}
		if (isNaN($scope.serviceModel.TotalTaxAmount)) $scope.serviceModel.TotalTaxAmount = 0;
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
	function getServiceChargeList(inveReveiveId) {

		$scope.chargesList = [];
		$http.get($scope.path + 'GetServiceChargeList?receiveId=' + inveReveiveId)
			.then(function (response) {
				$scope.chargesList = response.data;
				$scope.ServiceId = $scope.chargesList[0].Id;
				$scope.GetServiceTaxData();
			});

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
			if ($scope.ChargeTaxList[i].InventorySalesServiceId === linepk1) {
				result1.push($scope.ChargeTaxList[i]);
			}
		}
		return result1;
	}


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
		//$http({
		//    method: 'GET',
		//    url: $scope.path + 'GetServiceTaxList?serviceId=' + data.Id
		//}).then(function (response) {
		//    $scope.receiveTaxList = response.data;
		//    $scope.HSNCode = response.data[0]['HSNCode'];
		//    angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('show');
		//});
	}
	$scope.closeServiceChargeTaxPopUpwindow = function () {
		//getServiceChargeList($scope.productNew.Id);
		angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
	}

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

	//#endregion 


	//#region report
	$scope.InventorySalesReportExcels = function (id, reportFormat) {

		if ($scope.productNew.AsOnDate === 'AsOnDate') {

			if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
				ShowResult('Select To Date', 'failure');
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


		}

		var reportFormat = "Excel";
		if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
		$window.open('Products/InventoryIssue/InventorySalesReportExcel?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2 + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Summery=' + $scope.productNew.Summery + '&WithTax=' + $scope.tax.IncludingTax);
	};

	$scope.InventorySalesRepoReportPdf = function (id, reportFormat) {

		if ($scope.productNew.AsOnDate === 'AsOnDate') {

			if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
				ShowResult('Select To Date', 'failure');
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
		}


		var reportFormat = "Pdf";
		if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
		$window.open('Products/InventoryIssue/InventorySalesReportExcel?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.productNew.Qty + '&Amount=' + $scope.productNew.Amount + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Summery=' + $scope.productNew.Summery + '&WithTax=' + $scope.tax.IncludingTax);

	};

	//#endregion

	//#region Material Tax
	$scope.materialtaxCategoryListResFinal = [];
	$scope.getReceiveTaxList = function (data, flag, index, Id) {

		$scope.total = 0;
		//debugger;
		if ($scope.Action === 'Save') {
			$scope.LoadTaxButtonClick();
			$scope.dataId = data.MaterialMasterId + data.ArticleId + data.FirstCharacteristicsValueId + data.SecondCharacteristicsValueId + data.ThirdCharacteristicsValueId;
			var getRow = $filter("filter")($scope.materialtaxCategoryListRes, { "Id": $scope.dataId });
			$scope.materialtaxCategoryListResFinal = [];
			for (var j = 0; j < getRow.length; j++) {
				// if ($scope.materialtaxCategoryListRes[j].Id === $scope.dataId) {			
				$scope.materialtaxCategoryListResFinal.push(getRow[j]);
			}
			//for (var i = 0; i < $scope.detailList.length; i++) {
			//	$scope.taxAbleAmnt = $scope.detailList[i].TotalAmount;

			//}		
			$scope.taxAbleAmnt = $scope.detailList[index].TotalAmount;
			$scope.indexRow = index;
			$scope.index = index;
			$scope.HSNCode = getRow.HSNCode;
			$scope.total = $scope.total + getRow.TaxAmount;
			angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
		}
		else {
			$scope.materialtaxCategoryListResFinal = [];
			$scope.LoadTaxButtonClick();
			$http({
				method: "GET",
				dataType: 'JSON',
				url: 'Products/InventoryIssue/GetTaxInfoRowWise?InventorySalesId=' + data.InventoryIssueId + ' &InventorySalesHistoryId=' + data.HistotyId,
			}).then(function successCallback(response) {
				$scope.materialtaxCategoryListSavedData = response.data;
				var getRownewData = $filter("filter")($scope.materialtaxCategoryListSavedData, { "InventorySalesHistoryId": data.HistotyId, "InventorySalesId": data.InventoryIssueId });
				for (var K = 0; K < getRownewData.length; K++) {
					$scope.materialtaxCategoryListResFinal.push(getRownewData[K]);
				}
				$scope.taxAbleAmnt = $scope.detailList[index].TotalAmount;
				$scope.indexRow = index;
				$scope.index = index;
				$scope.HSNCode = getRownewData[0].HSNCode;
				$scope.total = $scope.total + getRownewData.TaxAmount;
				angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
			});
			//$scope.taxAbleAmnt = $scope.detailList[index].TotalAmount;


		}


	};
	$scope.LoadTaxButtonClick = function () {
		accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
			$scope.taxCategoryList = result;
		});
	}
	$scope.closeReceiveTaxPopUpwindow = function () {
		$scope.detailList[$scope.index].TaxAmount = parseFloat($filter('sumByKey')($scope.materialtaxCategoryListResFinal, 'TaxAmount', true));//$scope.materialtaxCategoryList.TaxAmount;


		// getInventoryMaterialList($scope.productNew.Id);
		angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
	}

	$scope.addTax = function () {
		var data = {
			TotalAmount: 0,
			Id: null,
			HSNCode: $scope.HSNCode,
			HSNCodeId: null,
			UserName: null,
			TaxCategoryId: null
		};
		$scope.materialtaxCategoryList.push(data);
	};
	$scope.calculateTaxAmount = function (data) {
		//data.TotalAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
		data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
	};
	$scope.Del = function (Id, index) {
		$scope.dindex = index;
		for (var i = 0; i < $scope.materialtaxCategoryList.length; i++) {
			if ($scope.materialtaxCategoryList[i].Id === Id) {
				$scope.materialtaxCategoryList.splice($scope.dindex, 1);
				return true;
				break;
			}
		}
		$scope.dindex = -1;
	};

	$scope.GettaxAfterSave = function (Id) {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/InventoryIssue/GetTaxInfo?Id=' + Id,
		}).then(function successCallback(response) {
			$scope.materialtaxCategoryList = response.data;

		});
	}
	//#endregion 

	// #region Specific Stock

	$scope.materialStockList = [];
	$scope.specificStockList = [];
	$scope.getSpecificMaterialStock = function (data, index) {
		//debugger;
		$scope.index = index;
		$scope.selectedRowQty = data.TransactionQty;

		$http({
			method: 'POST'
			, url: $scope.path + 'GetSpecificMaterialStock'
			, data: { entity: data, issueDate: $scope.productNew.SalesDate }
			, dataType: 'JSON'
		}).then(function (response) {
			$scope.materialStockList = response.data;
			for (var i2 = 0; i2 < $scope.materialStockList.length; i2++) {
				$scope.materialStockList[i2].SalesRate = $scope.detailList[index].SalesRate;
				$scope.materialStockList[i2].TotalAmount = $scope.detailList[index].TotalAmount;
			}

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
				$scope.materialStockList[i1].TrasactopmUomQty = $scope.materialStockList[i1].BalanceStock / data.BaseUoMFactor;
				$scope.materialStockList[i1].IssueTransactionUoMId = data.TransactionUoMId;
				$scope.materialStockList[i1].IssueTransactionUoM = data.TransactionUoM;

				$scope.materialStockList[i1].TransactionUoMId = data.TransactionUoMId;
				$scope.materialStockList[i1].BaseUoMFactor = data.BaseUoMFactor;
			}
			//for (var i2 = 0; i2 < $scope.detailList.length; i2++) {

			//$scope.materialStockList.SalesRate = $scope.detailList[index].SalesRate;
			//$scope.materialStockList.TotalAmount = $scope.detailList[index].TotalAmount;
			//}


			angular.element(document.querySelector('#stockPopUp')).modal('show');
		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};
	};

	$scope.addMaterialStock = function () {
		//debugger;
		try {
			var sumOfmaterialStockList = $filter('sumByKey')($filter('filter')($scope.materialStockList), 'RequisitionQty');
			if (sumOfmaterialStockList > $scope.selectedRowQty) {
				ShowResult("Issue qty can not grater than requisition qty", 'failure', 'stockPopUp');
				return false;
			}
			if (sumOfmaterialStockList < $scope.selectedRowQty) {
				ShowResult("Issue qty can not less than requisition qty", 'failure', 'stockPopUp');
				return false;
			}
			for (var t1 = 0; t1 < baseService.arrayLength($scope.materialStockList); t1++) {
				if ($scope.materialStockList[t1].IssueByUoM === 'Yes' && ($scope.materialStockList[t1].BaseUOMId != $scope.materialStockList[t1].IssueTransactionUoMId)) {
					ShowResult("Your Transaction UoM is not equal to base UoM.So you can not issue this material", 'failure', 'stockPopUp');
					return false;
				}
				if ($scope.materialStockList[t1].RequisitionQty > 0 && $scope.materialStockList[t1].Flag == 0) {
					ShowResult("select The given qty row", 'failure', 'stockPopUp');
					return false;
				}
				if (baseService.isUndefinedOrNull($scope.materialStockList[t1].RequisitionQty) && $scope.materialStockList[t1].Flag == 1) {
					ShowResult("Enter the qty for selected row ", 'failure', 'stockPopUp');
					return false;
				}
				if (baseService.isUndefinedOrNull($scope.materialStockList[t1].RequisitionQty) === 0 && $scope.materialStockList[t1].Flag == 1) {
					ShowResult("Enter the qty for selected row ", 'failure', 'stockPopUp');
					return false;
				}
			}
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
			//$scope.detailList[$scope.index].TransactionQty = issueQty;
			angular.element(document.querySelector('#stockPopUp')).modal('hide');
			CloseModalShowResult();
		} catch (e) {
			ShowResult(e, 'failure', 'stockPopUp');
		}
	};

	//$scope.calculateBaseQty = function (data) {
	//    data.BaseIssueQty = parseFloat(data.BaseUoMFactor * data.RequisitionQty).toFixed(4);
	//}

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
	function qtyValidation(list) {
		//debugger;
		for (var i = 0; i < baseService.arrayLength(list); i++) {
			if (list[i].Flag) {
				if (parseFloat(list[i].RequisitionQty) > parseFloat(list[i].BalanceStock)) throw 'Requisition Qty can\'t greater than stock qty.';
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
		var qty = parseFloat($scope.detailList[$scope.index].TransactionQty) * parseFloat($scope.detailList[$scope.index].BaseUoMFactor);
		//if (totalQty > qty && qty !== totalQty) throw 'Issue qty can\'t over ' + qty + ' .';
		//if (totalQty < qty && qty !== totalQty) throw 'Issue qty can\'t less ' + qty + ' .';

	}

	// #endregion Specific Stock
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
	//#region Sales Details

	$scope.lst = [];
	$scope.SalesDetails = function () {
		//debugger;
		$http({
			method: 'GET',
			//url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
			url: 'Products/InventoryIssue/MaterialSalesDetails'
		}).then(function successCallback(response) {
			$scope.lst = response.data;
			//$scope.detailgrid($scope.lst);
			window.lst = response.data;

		});
	}
	$scope.SalesDetails();


	$scope.data1 = $scope.lst;
	$scope.detailTemp = "#tabGridContents";
	//$scope.detailgrid = "detailGridData(e)";
	$scope.detailgrid = function detailGridData(e) {
		//debugger;

		var filteredData = e.data["Id"];
		var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("IssueNo", "equal", parseInt(filteredData), true).take(200));
		e.detailsElement.find("#detailGrid").ejGrid({

			dataSource: data,
			columns: ["Materials", "Article", "SKU1", "SKU2", "SKU3", "Qty", "UOM", "SalesRate", "CurrencyName", "TotalAmount", "Comments"]
		});
		e.detailsElement.find(".tabcontrol").ejTab();
	}

	//#endregion
	$scope.ispostDisable = false;
	$scope.Save = function () {


		//debugger;
		// $scope.SavePOPUpConfirm();
		$scope.productNew.ToCurrencyRate = $scope.productNew.ToCurrencyRate;
		for (var i = 0; i < $scope.detailList.length; i++) {
			if (baseService.isUndefinedOrNull($scope.detailList[i].Id)) {
				var sumOfmaterialStockList = $filter('sumByKey')($filter('filter')($scope.specificStockList), 'RequisitionQty');
				$scope.selectedRowQty1 = $scope.detailList[i].TransactionQty; //$filter('sumByKey')($filter('filter')($scope.detailList[i]), 'TransactionQty');
				if (sumOfmaterialStockList < $scope.selectedRowQty1) {
					ShowResult("Please select specific GRN", 'failure');
					return false;
				}
			}
		}

		if ($scope.detailList.length === 0) {
			ShowResult('Please select Atlest one material');
			return false;
		}
		//else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
		//	ShowResult("Please select to be approved by", 'failure');
		//	return false;
		//}
		//else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
		//	ShowResult("Please select to be checked by", 'failure');
		//	return false;
		//}
		else if (baseService.isUndefinedOrNull($scope.productNew.PartyName)) {
			ShowResult("Please select Customer", 'failure');
			return false;
		}
		else if (baseService.isUndefinedOrNull($scope.productNew.DocDate)) {
			ShowResult("Enter the Doc Date", 'failure');
			return false;
		}

		var UIStatus = $("#SlipAssetIssueUI").val();
		$scope.productNew.IssueRequestMasterId = $scope.issueId;
		$scope.productNew.CustomerId = $scope.productNew.PartyId;
		$scope.ispostDisable = true;
		if ($scope.Action === "Save") {
			$http({
				method: 'POST'
				, url: $scope.saveUrl
				, data: {
					entities: $scope.detailList
					, specificStockList: $scope.specificStockList
					, inventoryIssue: $scope.productNew
					, IssueTypeStatus: UIStatus,
					'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
					'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti,
					'taxCategoryList': $scope.materialtaxCategoryListRes,
					'ToCurrencyRate': $scope.productNew.ToCurrencyRate
				}
				, dataType: 'JSON'
			}).then(function (response) {
				if (response.data.Error === true)
					ShowResult(response.data.Message, 'failure');
				else {
					ShowResult(response.data.Message, 'success');
					//$scope.Clear();
					$scope.Action = 'Update';
					$scope.productNew.Id = response.data.inventoryIssue.Id;
					$scope.getdataInventorySales();
					$scope.SalesDetails();
					$scope.getData();
					$scope.GetDataList();
					$scope.ispostDisable = false;
				}
			}), function (response) {
				ShowResult(response.data.Message, 'failure');
			};
		}
		else if ($scope.Action === "Update") {

			var getRowdetailList = $filter("filter")($scope.detailList, { "Id": null });
			if (getRowdetailList.length === 0) {
				ShowResult("Nothing to update", 'failure');
				return false;
			}
			$scope.detailList = [];
			for (var j1 = 0; j1 < getRowdetailList.length; j1++) {

				$scope.detailList.push(getRowdetailList[j1]);
				$scope.detailList[j1].MaterialStorageId = $scope.productNew.MaterialStorageId;
			}

			$http({
				method: 'POST'
				, url: $scope.saveUrl
				, data: {
					entities: $scope.detailList
					, specificStockList: $scope.specificStockList
					//, inventoryIssue: $scope.productNew
					, IssueTypeStatus: UIStatus,
					'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
					'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti,
					'taxCategoryList': $scope.materialtaxCategoryListRes,
					'productNewId': $scope.productNew.Id,
					'ToCurrencyRate': $scope.productNew.ToCurrencyRate
				}
				, dataType: 'JSON'
			}).then(function (response) {
				if (response.data.Error === true)
					ShowResult(response.data.Message, 'failure');
				else {
					ShowResult(response.data.Message, 'success');
					getIssueDetailList();
					//$scope.Clear();
					//$scope.productNew.Id = response.data.inventoryIssue.Id;
					//$scope.getdataInventorySales();
					//$scope.SalesDetails();
					//$scope.getData();
					//$scope.GetDataList();
				}
			}), function (response) {
				ShowResult(response.data.Message, 'failure');
			};
		}

		//else ShowResult('Please issue material', 'failure');
	};
	$scope.GetDataList = function ($event) {
		//debugger;

		//$scope.index = index;
		// $scope.product = $scope.issueList[index.rowIndex];
		var a = $event;
		var id = a.data;
		$scope.product = a.data;
		$scope.product.IssueType = a.data.issuetype;
		$scope.product.SalesDate = a.data.IssueDate;
		$scope.product.InvoicingPartyPlantId = a.data.InvoicingPartyPlantId;
		$scope.productNew = Object.assign({}, $scope.product);
		$scope.materialStockList = [];
		$scope.specificStockList = [];
		getIssueDetailList();
		$scope.GettaxAfterSave(a.data.Id);
		getServiceChargeList(a.data.Id);
		if (baseService.isUndefinedOrNull(a.data.CheckedBy) && !baseService.isUndefinedOrNull(a.data.ApprovedBy)) {
			$scope.CheckedByStatusForNoti = false;
			$scope.ApprovedByStatusForNoti = true;
			$scope.productNew.CheckedBy = a.data.ApprovedBy;
		}
		else if (!baseService.isUndefinedOrNull(a.data.CheckedBy) && !baseService.isUndefinedOrNull(a.data.ApprovedBy)) {
			$scope.CheckedByStatusForNoti = true;
			$scope.ApprovedByStatusForNoti = true;
			$scope.productNew.CheckedBy = a.data.CheckedBy;
		}

		$scope.GetCheckedByAndApprovedBy1();


		if (baseService.isUndefinedOrNull(a.data.CheckedBy) && !baseService.isUndefinedOrNull(a.data.ApprovedBy)) {

			$scope.productNew.CheckedBy = a.data.ApprovedBy;
			$scope.productNew.labelCheckAndApproved = 'To be approved by';
		}
		else if (!baseService.isUndefinedOrNull(a.data.CheckedBy) && baseService.isUndefinedOrNull(a.data.ApprovedBy)) {

			$scope.productNew.CheckedBy = a.data.CheckedBy;
			$scope.productNew.labelCheckAndApproved = 'To be checked by';
		}
		$scope.productNew.TaxOption = 'Yes';
		$scope.productNew.TaxOptionMat = 'Yes';
		$scope.productNew.TaxOptionService = 'Yes';
		$scope.productNew.TaxOptionServiceModify = 'Yes';
		$scope.productNew.TaxOptionAddiTax = 'Yes';
		$scope.getTaxCodeByTaxYearWithhold($scope.productNew.SalesDate);

		$scope.Action = 'Update';
		if (!$rootScope.isCollapsed) $rootScope.toggle();
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
	$scope.Clear = function () {
		ClearFields();
		return true;
	};

	function ClearFields() {
		$scope.Action = "Save";
		$scope.ispostDisable = false;
		$scope.product = {};
		$scope.detailList = [];
		$scope.detailModel = {};
		$scope.clearCharNames();
		$scope.specificStockList = [];
		$scope.materialtaxCategoryListRes = [];
		$scope.productNew = { FixedAssetOrInventory: 'Inventory', PODepended: false, AlongwithInvoice: false, IssueType: 'Revenue', InvoicingPartyPlantId: $scope.productNew.InvoicingPartyPlantId };
		//$scope.productNew.InvoicingPartyPlantId=$scope.productNew.InvoicingPartyPlantId;		
		$scope.IssueType = 'Revenue';

	}
	$scope.setCharData = function (data) {
		$scope[$scope.charValueSearchFor].CharacteristicsValueId = data.CharacteristicsValueId;
		$scope[$scope.charValueSearchFor].FreeText = data.UserName;
		$scope[$scope.charValueSearchFor].FlagDisable = $scope.isSearch;
		if ($scope.charValueSearchFor === 'char1') $scope.detailModel.FirstCharacteristicsValueId = data.CharacteristicsValueId;
		if ($scope.charValueSearchFor === 'char2') $scope.detailModel.SecondCharacteristicsValueId = data.CharacteristicsValueId;
		if ($scope.charValueSearchFor === 'char3') $scope.detailModel.ThirdCharacteristicsValueId = data.CharacteristicsValueId;
		getMaterialStock();
		$scope.CountryLoadData();
		angular.element(document.querySelector('#searchcharactervaluepopup')).modal('hide');
	};
	$scope.clearCharValueField = function (valueFor) {
		$scope[valueFor].CharacteristicsValueId = null;
		$scope[valueFor].FreeText = null;
		$scope[valueFor].FlagDisable = $scope.IsFreeOrNot($scope.char1.IsFreeField);
		$scope.isSearch = false;
		if (valueFor === 'char1') $scope.detailModel.FirstCharacteristicsValueId = null;
		if (valueFor === 'char2') $scope.detailModel.SecondCharacteristicsValueId = null;
		if (valueFor === 'char3') $scope.detailModel.ThirdCharacteristicsValueId = null;
	};
	$scope.manualValidationAddRemove = function (divId, fieldName, message) {
		var msg = fieldName + ' is required.';
		msg = baseService.isUndefinedOrNull(message) ? msg : message;
		var str = fieldName;
		if (baseService.isUndefinedOrNull($scope.detailModel[str.replace(/\s/g, '')]))
			return manualValidation(divId, true, msg);
		else
			return manualValidation(divId, false);
	};
	$scope.validation = function () {
		$scope.manualValidationAddRemove('div_mm', 'MaterialMasterName');
		$scope.manualValidationAddRemove('div_ar', 'ArticleName');
		$scope.manualValidationAddRemove('div_qty', 'TransactionQty');
		$scope.manualValidationAddRemove('div_qty', 'TransactionUoMId', 'UoM is required');
		//$scope.manualValidationAddRemove('div_entity', 'EntityId', 'Entity is required');
		//$scope.manualValidationAddRemove('div_budget', 'BudgetMasterid', 'budget is required');


		if ($scope.hasSku) {
			if (!baseService.isUndefinedOrNull($scope.char1.CharacteristicsId))
				$scope.IsMandatoryButNull($scope.char1.IsMandatory, $scope.char1.FreeText);
			else if (!baseService.isUndefinedOrNull($scope.char2.CharacteristicsId))
				$scope.IsMandatoryButNull($scope.char2.IsMandatory, $scope.char2.FreeText);
			else if (!baseService.isUndefinedOrNull($scope.char3.CharacteristicsId))
				$scope.IsMandatoryButNull($scope.char3.IsMandatory, $scope.char3.FreeText);
			else throw 'Please insert SKU.';
		}
	};
	$scope.detailList = [];
	$scope.materialtaxCategoryListRes = [];
	$scope.detailAdd = function () {
		//debugger;
		try {
			if (baseService.isUndefinedOrNull($scope.detailModel.TransactionQty)) {
				ShowResult('Enter the Transaction Qty', 'failure', 'detailPopUp');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.detailModel.SalesRate)) {
				ShowResult('Enter the Sales Rate', 'failure', 'detailPopUp');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.detailModel.TotalAmount)) {
				ShowResult('Enter the Total Amount', 'failure', 'detailPopUp');
				return false;
			}
			//$scope.validation();
			//if ($scope.detailModel.BudgetMasterId === '' || $scope.detailModel.BudgetMasterId === null || $scope.detailModel.BudgetMasterId === undefined) {
			//	ShowResult('Budget is required', 'failure', 'detailPopUp');
			//	return false;
			//}
			//if ($scope.detailModel.CostCenterId === '' || $scope.detailModel.CostCenterId === null || $scope.detailModel.CostCenterId === undefined) {
			//    ShowResult('Cost center is required', 'failure', 'detailPopUp');
			//    return false;
			//}
			//if ($scope.detailModel.ActivityId === '' || $scope.detailModel.ActivityId === null || $scope.detailModel.ActivityId === undefined) {
			//	ShowResult('Activity is required', 'failure', 'detailPopUp');
			//	return false;
			//}
			if ($scope.detailModel.IsOriginApplicable === true) {
				if (baseService.isUndefinedOrNull($scope.detailModel.CountryId)) {
					ShowResult('Please select the country', 'failure', 'detailPopUp');
					return false;
				}
			}

			$scope.detailModel.TransactionQty = baseService.isUndefinedOrNull($scope.detailModel.TransactionQty) === true ? 0 : parseFloat($scope.detailModel.TransactionQty);
			if ($scope.detailModel.TransactionQty === 0)
				throw 'Please insert issue qty.';
			else {
				if ($scope.detailModel.TransactionUoMId === $scope.detailModel.BaseUOMId) {
					if ($scope.detailModel.TransactionQty > parseFloat($scope.detailModel.PostingQuantity))
						throw 'Issue qty must be less than or equal Ready for Issue Qty.';
					$scope.detailModel.BaseQty = $scope.detailModel.TransactionQty;
				}
				else {
					var tQty = parseFloat($scope.detailModel.TransactionQty) * parseFloat($.grep($scope.uoMList, function (item) { return item.Value === $scope.detailModel.TransactionUoMId; })[0].BaseUoMFactor);
					if (tQty > parseFloat($scope.detailModel.PostingQuantity))
						throw 'Issue qty must be less than or equal Ready for Issue Qty.';
					$scope.detailModel.BaseQty = tQty;
				}
			}
			 
			for (var i = 0; i < baseService.arrayLength($scope.detailList); i++) {
				if ($scope.detailList[i].FirstCharacteristicsValueId === 'undefined' || $scope.detailList[i].FirstCharacteristicsValueId === null || $scope.detailList[i].FirstCharacteristicsValueId === '') {
					$scope.detailList[i].FirstCharacteristicsValueId = null;

				}
				if ($scope.detailList[i].SecondCharacteristicsValueId === 'undefined' || $scope.detailList[i].SecondCharacteristicsValueId === null || $scope.detailList[i].SecondCharacteristicsValueId === '') {
					$scope.detailList[i].SecondCharacteristicsValueId = null;

				}
				if ($scope.detailList[i].ThirdCharacteristicsValueId === 'undefined' || $scope.detailList[i].ThirdCharacteristicsValueId === null || $scope.detailList[i].ThirdCharacteristicsValueId === '') {
					$scope.detailList[i].ThirdCharacteristicsValueId = null;

				}
				if ($scope.detailList[i].CountryId === 'undefined' || $scope.detailList[i].CountryId === null || $scope.detailList[i].CountryId === '') {
					$scope.detailList[i].CountryId = null;

				}
				if ($scope.detailModel.CountryId === 'undefined' || $scope.detailModel.CountryId === null || $scope.detailModel.CountryId === '') {
					$scope.detailModel.CountryId = null;

				}

				if ($scope.detailList[i].MaterialMasterId === $scope.detailModel.MaterialMasterId &&
					$scope.detailList[i].ArticleId === $scope.detailModel.ArticleId &&
					$scope.detailList[i].FirstCharacteristicsValueId === $scope.detailModel.FirstCharacteristicsValueId &&
					$scope.detailList[i].SecondCharacteristicsValueId === $scope.detailModel.SecondCharacteristicsValueId &&
					$scope.detailList[i].ThirdCharacteristicsValueId === $scope.detailModel.ThirdCharacteristicsValueId

				) {//&& $scope.detailList[i].CountryId === $scope.detailModel.CountryId
					ShowResult('This material already Added', 'failure', 'detailPopUp');
					return false;
				}

			}
			$scope.detailModel.FirstCharacteristicsId = $scope.char1.CharacteristicsId;
			$scope.detailModel.FirstCharacteristicsValueId = $scope.char1.CharacteristicsValueId;
			$scope.detailModel.FirstCharacteristicText = $scope.char1.FreeText;

			$scope.detailModel.SecondCharacteristicsId = $scope.char2.CharacteristicsId;
			$scope.detailModel.SecondCharacteristicText = $scope.char2.FreeText;
			$scope.detailModel.SecondCharacteristicsValueId = $scope.char2.CharacteristicsValueId;

			$scope.detailModel.ThirdCharacteristicsId = $scope.char3.CharacteristicsId;
			$scope.detailModel.ThirdCharacteristicText = $scope.char3.FreeText;
			$scope.detailModel.ThirdCharacteristicsValueId = $scope.char3.CharacteristicsValueId;

			$scope.detailModel.IssueDate = $scope.productNew.IssueDate;
			$scope.detailModel.Remarks = $scope.productNew.Remarks;
			$scope.detailModel.EmployeeId = $scope.productNew.EmployeeId;
			$scope.detailModel.CountryId = $scope.detailModel.CountryId;
			$scope.detailModel.CountryName = $scope.detailModel.CountryName;
			$scope.detailModel.IsSpecific = false;
			$scope.detailModel.BaseUoMFactor = $.grep($scope.uoMList, function (item) { return item.Value === $scope.detailModel.TransactionUoMId; })[0].BaseUoMFactor;
			$scope.detailModel.TransactionUoM = angular.element("#issueUoM :selected").text();
			$scope.detailModel.TaxAmount = $scope.detailModel.TotalTaxAmount;
			$http({
				method: 'Post'
				, url: $scope.path + 'getInvMaterialId'
				, data: $scope.detailModel
				, dataType: 'JSON'
			}).then(function (response) {
				if (response.data.Error === true)
					ShowResult(response.data.Message, 'failure');
				else {
					$scope.detailModel.InventoryMaterialId = response.data;
					var row = Object.assign({}, $scope.detailModel);
					$scope.detailList.push(row);
				}
			}), function (response) {
				ShowResult(response.data.Message, 'failure');
			};

		} catch (e) {
			ShowResult(e, 'failure', 'detailPopUp');
		}
		for (var i = 0; i < $scope.materialtaxCategoryList.length; i++) {
			$scope.materialtaxCategoryList[i].Id = $scope.detailModel.MaterialMasterId + $scope.detailModel.ArticleId + $scope.detailModel.FirstCharacteristicsValueId + $scope.detailModel.SecondCharacteristicsValueId + $scope.detailModel.ThirdCharacteristicsValueId
			$scope.materialtaxCategoryListRes.push($scope.materialtaxCategoryList[i]);
		}

		$scope.materialtaxCategoryList = [];

		if (!baseService.isUndefinedOrNull($scope.detailModel.TotalAmount)) {
			angular.element(document.querySelector('#detailPopUp')).modal('hide');
		}
		//$scope.detailModel.MaterialMasterId = '';
		//$scope.detailModel.MaterialMasterName = '';
		//$scope.detailModel.ArticleId = '';
		//$scope.detailModel.ArticleName = '';
		//$scope.detailModel.FirstCharacteristicsId = '';
		//$scope.detailModel.FirstCharacteristicsValueId = '';
		//$scope.detailModel.SecondCharacteristicsId = '';
		//$scope.detailModel.SecondCharacteristicsValueId = '';
		//$scope.detailModel.ThirdCharacteristicsId = '';
		//$scope.detailModel.ThirdCharacteristicsValueId = '';
		//$scope.detailModel.Comments = '';
		//$scope.detailModel.SalesRate = '';
		//$scope.detailModel.TransactionQty = '';
		//$scope.detailModel.SalesRate = '';
		//$scope.detailModel.TotalAmount = '';
	};
	$scope.addTax = function () {
		var data = {
			TotalAmount: 0,
			Id: null,
			HSNCode: $scope.HSNCode,
			HSNCodeId: null,
			UserName: null,
			TaxCategoryId: null
		};
		$scope.materialtaxCategoryList.push(data);
	};
	// #region Details

	$scope.detailPopUp = function () {
		//debugger;
		$scope.materialtaxCategoryList = [];
		$scope.$broadcast('show-errors-check-validity');
		if ($scope.productNewForm.$valid) {
			$scope.product = Object.assign({}, $scope.productNew);
			$scope.detailModel = {
				Id: null
				, InventoryReveiveId: null
				, MaterialStorageId: $scope.productNew.MaterialStorageId
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

				, InventoryIssueId: $scope.productNew.Id
				, AvgAmount: null
				, PolicyRate: null
				, PolicyAmount: null
				, Policy: null
				, ActivityName: null
				, BudgetMasterId: null
				, ActivityId: null
				, IssueId: null
				, CostCenterId: null
				, CountryName: null
				, IsSpecific: false
				, Comments: null
				, TaxAmount: null
				, ToCurrencyRate: $scope.productNew.ToCurrencyRate
			};
			$scope.clearCharNames();
			$scope.detailModel.CostCenterId = $scope.CostCenterIdTemp;

			angular.element(document.querySelector('#detailPopUp')).modal('show');
		}
	};
	$scope.closeDetaiPopUp = function () {
		//debugger;
		$scope.CostCenterIdTemp = $scope.detailModel.CostCenterId;
		$scope.detailModel = {};
		$scope.clearCharNames();
		angular.element(document.querySelector('#detailPopUp')).modal('hide');
	};


	//$scope.CountryLoadData();


	$scope.materialType = ['Asset', 'Consumable', 'Spare', 'RawMaterial'];

	$scope.getMaterialStockCountryWise = function (id) {

		//debugger;
		$scope.detailModel.CountryName = $("#CountryName option:selected").text();
		$http({
			method: 'POST',
			url: $scope.path + 'GetStockCountryWise',
			data: { entity: $scope.detailModel, issueDate: $scope.productNew.IssueDate },
			dataType: 'JSON'
		}).then(function (response) {
			$scope.detailModel.TotalQty = response.data.TotalQty;
			$scope.detailModel.PostingQty = response.data.PostingQty;
			$scope.detailModel.PostingQuantity = response.data.PostingQuantity;
			$scope.detailModel.ApprovedQty = response.data.ApprovedQty;
			$scope.detailModel.UnApprovedQty = response.data.UnApprovedQty;
			if (baseService.isUndefinedOrNull($scope.detailModel.TotalQty))
				$scope.errorText = 'This material has no stock';
			else $scope.errorText = null;
		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};
	}

	$scope.getCharacteristicsList = function (id) {
		$scope.clearCharNames();
		$http({
			method: 'GET',
			url: 'Materials/MaterialMaster/getcharacteristicsbymaterialmasterid/',
			params: {
				materialMasterId: id
			}
		}).then(function (response) {
			$scope.characteristicsList = [];
			$scope.characteristicsList = response.data.charData;
			if (baseService.arrayLength($scope.characteristicsList) > 0) {
				$scope.isSearch = $scope.characteristicsList[0].FreeText !== null ? true : false;
				$scope.char1 = {
					CharacteristicsId: $scope.characteristicsList[0].Value
					, CharacteristicsValueId: $scope.characteristicsList[0].CharacteristicsValueId
					, MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
					, Name: $scope.characteristicsList[0].Text
					, IsFreeField: $scope.characteristicsList[0].IsFreeField
					, IsPreDefinedField: $scope.characteristicsList[0].IsPreDefinedField
					, IsMandatory: $scope.characteristicsList[0].IsMandatory
					, ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
					, Sequence: $scope.characteristicsList[0].Sequence
					, FlagDisable: $scope.IsFreeOrNot($scope.characteristicsList[0].IsFreeField)

					, FreeText: $scope.characteristicsList[0].FreeText
					, show: true
				};
				$scope.detailModel.FirstCharacteristicsValueId = $scope.characteristicsList[0].CharacteristicsValueId;
			}
			if (baseService.arrayLength($scope.characteristicsList) > 1) {
				$scope.isSearch = $scope.characteristicsList[1].FreeText !== null ? true : false;
				$scope.char2 = {
					CharacteristicsId: $scope.characteristicsList[1].Value
					, CharacteristicsValueId: $scope.characteristicsList[1].CharacteristicsValueId
					, MaterialMasterId: $scope.characteristicsList[1].MaterialMasterId
					, Name: $scope.characteristicsList[1].Text
					, IsFreeField: $scope.characteristicsList[1].IsFreeField
					, IsPreDefinedField: $scope.characteristicsList[1].IsPreDefinedField
					, IsMandatory: $scope.characteristicsList[1].IsMandatory
					, ValueAssignmentLevel: $scope.characteristicsList[1].ValueAssignmentLevel
					, Sequence: $scope.characteristicsList[1].Sequence
					, FlagDisable: $scope.IsFreeOrNot($scope.characteristicsList[1].IsFreeField)
					, FreeText: $scope.characteristicsList[1].FreeText
					, show: true
				};
				$scope.detailModel.SecondCharacteristicsValueId = $scope.characteristicsList[1].CharacteristicsValueId;
			}
			if (baseService.arrayLength($scope.characteristicsList) > 2) {
				$scope.isSearch = $scope.characteristicsList[2].FreeText !== null ? true : false;
				$scope.char3 = {
					CharacteristicsId: $scope.characteristicsList[2].Value
					, CharacteristicsValueId: $scope.characteristicsList[2].CharacteristicsValueId
					, MaterialMasterId: $scope.characteristicsList[2].MaterialMasterId
					, Name: $scope.characteristicsList[2].Text
					, IsFreeField: $scope.characteristicsList[2].IsFreeField
					, IsPreDefinedField: $scope.characteristicsList[2].IsPreDefinedField
					, IsMandatory: $scope.characteristicsList[2].IsMandatory
					, ValueAssignmentLevel: $scope.characteristicsList[2].ValueAssignmentLevel
					, Sequence: $scope.characteristicsList[2].Sequence
					, FlagDisable: $scope.IsFreeOrNot($scope.characteristicsList[2].IsFreeField)
					, FreeText: $scope.characteristicsList[2].FreeText
					, show: true
				};
				$scope.detailModel.ThirdCharacteristicsValueId = $scope.characteristicsList[2].CharacteristicsValueId;
			}
		});
	};

	$scope.removeRow = function () {
		if (!baseService.isUndefinedOrNull($scope.delData.Id)) {

			if (baseService.isUndefinedOrNull($scope.detailList[$scope.popUpIndex].MaterialMasterId)) $scope.detailList[$scope.popUpIndex].MaterialMasterId = 'undefined';
			if (baseService.isUndefinedOrNull($scope.detailList[$scope.popUpIndex].ArticleId)) $scope.detailList[$scope.popUpIndex].ArticleId = 'undefined';
			if (baseService.isUndefinedOrNull($scope.detailList[$scope.popUpIndex].FirstCharacteristicsValueId)) $scope.detailList[$scope.popUpIndex].FirstCharacteristicsValueId = 'undefined';
			if (baseService.isUndefinedOrNull($scope.detailList[$scope.popUpIndex].SecondCharacteristicsValueId)) $scope.detailList[$scope.popUpIndex].SecondCharacteristicsValueId = 'undefined';
			if (baseService.isUndefinedOrNull($scope.detailList[$scope.popUpIndex].ThirdCharacteristicsValueId)) $scope.detailList[$scope.popUpIndex].ThirdCharacteristicsValueId = 'undefined';

			$scope.TaxIdDeletefromList = $scope.detailList[$scope.popUpIndex].MaterialMasterId + $scope.detailList[$scope.popUpIndex].ArticleId + $scope.detailList[$scope.popUpIndex].FirstCharacteristicsValueId + $scope.detailList[$scope.popUpIndex].SecondCharacteristicsValueId + $scope.detailList[$scope.popUpIndex].ThirdCharacteristicsValueId;


			$http({
				method: 'POST'
				, url: $scope.deleteUrl + '?issueDetailId=' + $scope.delData.Id
				, dataType: 'JSON'
			}).then(function (response) {
				if (response.data.Error === true) {
					ShowResult(response.data.Message, 'failure');
				}

				else {

					for (var i = 0; i < $scope.materialtaxCategoryListRes.length; i++) {
						if ($scope.materialtaxCategoryListRes[i].Id === $scope.TaxIdDeletefromList) {
							//$scope.materialtaxCategoryListRes.splice($scope.materialtaxCategoryList[i].Id);
							$scope.materialtaxCategoryListRes.splice($scope.materialtaxCategoryListRes[i].Id);

							//$scope.specificStockList.splice(i, 1);
						}
					}
					ShowResult(response.data.Message, 'success');
				}

			}), function (response) {
				ShowResult(response.data.Message, 'failure');
			};
		}
		for (var i = 0; i < baseService.arrayLength($scope.specificStockList); i++) {
			if ($scope.specificStockList[i].InventoryMaterialId === $scope.delData.InventoryMaterialId)
				$scope.specificStockList.splice(i, 1);
		}
		$scope.detailList.splice($scope.popUpIndex, 1);
		$scope.delData = null;
		$scope.popUpIndex = -1;
		angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
	};

	function getIssueDetailList() {
		$http.get($scope.path + 'GetSalesDetailByIssueId?issueId=' + $scope.productNew.Id)
			.then(function (response) {
				$scope.detailList = response.data;
				$scope.detailModel.IssueId = $scope.detailList[0].InventoryIssueId;
				$scope.GetAdvanceTaxInfo($scope.productNew.Id);
				$scope.TotalSumAfterTCS();
			});

	}

	// #endregion Details

	$scope.BudgetActivityList = [];
	$scope.getBudgetActivityInIssueMaterial = function (materialGroupMasterId) {
		$http({
			method: "GET",
			url: 'Products/InventoryIssue/GetBudgetActivityInSalesMaterial?materialGroupMasterId=' + materialGroupMasterId
		}).then(function successCallback(response) {
			$scope.BudgetActivityList = response.data;
			$scope.detailModel.GLGeneralInfoId = $scope.BudgetActivityList[0].GLGeneralInfoId;
			$scope.detailModel.BudgetMasterId = $scope.BudgetActivityList[0].BudgetMasterId;
			$scope.detailModel.BudgetName = $scope.BudgetActivityList[0].BudgetName;
			$scope.getActivity($scope.BudgetActivityList[0]);
		});
	};

	//checked done


	$scope.calculateTaxAmountForMat = function (data) {
		if (baseService.isUndefinedOrNull(data.Percentage)) {
			data.Percentage = 0;
		}
		data.TaxAmount = Math.round($scope.detailModel.TotalAmount * data.Percentage) / 100;
	};
	$scope.checkRowValidationMat = function (x) {
		debugger;
		for (var i = 0; i < $scope.materialtaxCategoryList.length; i++) {
			if (baseService.isUndefinedOrNull($scope.detailModel.TotalAmount) || $scope.detailModel.TotalAmount === 0) {
				ShowResult("Taxable Amount can not null or zero", 'failure', 'detailPopUp');
			}
			if ($scope.materialtaxCategoryList[i].Id === x.Id) {
				$scope.materialtaxCategoryList[i].Percentage = (parseFloat(x.TaxAmount / $scope.detailModel.TotalAmount).toFixed(4) * 100).toFixed(4);
			}

		}
	}



	$scope.calculateTaxAmountForService = function (data) {
		if (baseService.isUndefinedOrNull(data.Percentage)) {
			data.Percentage = 0;
		}
		data.TaxAmount = Math.round($scope.serviceModel.TransactionAmount * data.Percentage) / 100;
	};
	$scope.checkRowValidationService = function (x) {
		debugger;
		for (var i = 0; i < $scope.taxCategoryList.length; i++) {
			//if (baseService.isUndefinedOrNull($scope.detailModel.TransactionAmount) || $scope.detailModel.TransactionAmount === 0) {
			//	ShowResult("Taxable Amount can not null or zero", 'failure', 'detailPopUp');
			//}
			if ($scope.taxCategoryList[i].Id === x.Id) {
				$scope.taxCategoryList[i].Percentage = (parseFloat(x.TaxAmount / $scope.serviceModel.TransactionAmount).toFixed(4) * 100).toFixed(4);
			}

		}
	}
	$scope.calculateTaxAmount = function (data) {
		//debugger;
		//data.TotalAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
		data.TaxAmount = (Math.round($scope.taxAbleAmnt * data.Percentage) / 100).toFixed(2);
	};
	$scope.checkRowValidation = function (x) {
		debugger;
		for (var i = 0; i < $scope.materialtaxCategoryListResFinal.length; i++) {
			//if (baseService.isUndefinedOrNull($scope.HSNCode)) {
			//if ($scope.receiveTaxList[i].Percentage === 0) {
			if ($scope.materialtaxCategoryListResFinal[i].Id === x.Id) {
				$scope.materialtaxCategoryListResFinal[i].Percentage = (parseFloat(x.TaxAmount / $scope.taxAbleAmnt).toFixed(4) * 100).toFixed(4);
			}
			//}
			//}
		}
	}



	$scope.calculateTaxAmountsM = function (data) {
		//debugger;
		//data.TotalAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
		data.TaxAmount = (Math.round($scope.taxAbleAmnt * data.Percentage) / 100).toFixed(2);
	};
	$scope.checkRowValidationsM = function (x) {
		debugger;
		for (var i = 0; i < $scope.receiveTaxList.length; i++) {
			//if (baseService.isUndefinedOrNull($scope.HSNCode)) {
			//if ($scope.receiveTaxList[i].Percentage === 0) {
			if ($scope.receiveTaxList[i].Id === x.Id) {
				$scope.receiveTaxList[i].Percentage = (parseFloat(x.TaxAmount / $scope.taxAbleAmnt).toFixed(4) * 100).toFixed(4);
			}
			//}
			//}
		}
	}




	// #region Payment Term
	$http({
		method: 'GET',
		url: 'accounts/PaymentTerm/getcustomercbo'
	}).then(function successCallback(response) {
		$scope.paymentTermList = response.data;
	});

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
				else if (paymentTerm.BaseLineDate === 'postingdate') {
					$scope.productNew.BaseOnDueDate = $filter('dateFiltering')($scope.productNew.DocDate);
					$scope.productNew.BaseOnDueDate = null;
					$scope.productNew.BaseNoOfDays = null;
					$scope.productNew.MatureDate = null;
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

	//#region Additional Code
	$scope.productDocMap = {
		Id: null
		, CompanyGroupId: null
		, FileName: null
		, UserFilename: null
		, SystemFileName: null
		, Description: null
		, Remarks: null
	};
	$scope.advanceTax = {
		TaxCodeId: null,
		Text: null,
		TaxAmount: null,
		ValueOfFixed: null,
		CompanyCurrencyAmount: null,
		Type: null
		, TotalSumAfterTCSVal: null

	};
	$scope.advanceTaxesList = [];
	$scope.additionalTax = function () {
		for (var i = 0; i < $scope.advanceTaxesList.length; i++) {
			if ($scope.advanceTaxesList[i].TaxCodeId === $scope.advanceTax.TaxCodeId) {
				ShowResult("Tax Already Added");
				return false;
			}

		}

		if (manualValidation("td_TaxCode", baseService.isUndefinedOrNull($scope.advanceTax.TaxCodeId), "Tax Code is required.")) {
			$scope.invalidRow = true;
		}
		else if (manualValidation("td_TaxCodeAmount", baseService.isUndefinedOrNull($scope.advanceTax.TaxAmount), "Amount is required.")) {
			$scope.invalidRow = true;
		}
		else if (manualValidation("td_TaxCodeCompanyCurrencyAmount", baseService.isUndefinedOrNull($scope.advanceTax.CompanyCurrencyAmount), $scope.companyCurrencyCode + " is required.")) {
			$scope.invalidRow = true;
		}
		else {
			$scope.advanceTax.TaxName = $.grep($scope.taxCodCboListWithhold, function (item) { return item.Id === $scope.advanceTax.TaxCodeId; })[0].UserName;
			$scope.advanceTax.TaxCategoryId = $.grep($scope.taxCodCboListWithhold, function (item) { return item.Id === $scope.advanceTax.TaxCodeId; })[0].TaxCategoryId;
			$scope.advanceTaxesList.push($scope.advanceTax);
			$scope.advanceTax = {};
			$scope.TotalSumAfterTCS();
		}

	};

	$scope.taxCodCboListWithhold = [];
	$scope.taxcodelistMessage = "";
	$scope.getTaxCodeByTaxYearWithhold = function (date) {
		$scope.productNew.TaxOptionAddiTax = 'Yes';
		$http({
			method: "Get",
			//url: "accounts/TaxCode/GetAdditionalTaxCbo?postingDate=" + $filter("dateFiltering")(date)
			url: "accounts/TaxCode/GetAdditionalTaxOutputCbo?postingDate=" + $filter("dateFiltering")(date)
		}).then(
			function successCallback(response) {
				if (response.data.Error === true) {
					$scope.taxcodelistMessage = response.data.Message;
				}
				else {
					$scope.taxCodCboListWithhold = response.data;
				}
			},
			function errorCallback(response) {
			});
	};
	$scope.getTaxCodeByTaxYearWithhold($filter("dateFiltering")(Date.now()));
	$scope.selectadditionalTax = function () {
		$scope.advanceTax.ValueOfFixed = $.grep($scope.taxCodCboListWithhold, function (item) {
			return item.Id === $scope.advanceTax.TaxCodeId;
		})[0].ValueOfFixed;
		$scope.advanceTax.Type = $.grep($scope.taxCodCboListWithhold, function (item) {
			return item.Id === $scope.advanceTax.TaxCodeId;
		})[0].Type;
		if ($scope.advanceTax.Type == 'FixedPercentage' && !baseService.isUndefinedOrNull($scope.advanceTax.ValueOfFixed)) {//* $scope.advanceTax.ValueOfFixed / 100
			//$scope.advanceTax.TaxAmount = (parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax")) * $scope.advanceTax.ValueOfFixed / 100);

			$scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.detailList), "TotalAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.detailList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "Amount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "TotalTaxAmount"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);
		}
		$scope.TotalSumAfterTCS();
	}

	$scope.SaveAdditinalTaxInGRNList = function () {
		$http({
			method: 'POST',
			url: 'Products/InventoryIssue/SaveAdditinalTaxInGRN',
			data:
			{
				'InventoryReceiveId': $scope.productNew.Id,
				'UserSendData': $scope.advanceTaxesList,
				'ToCurrencyRate': $scope.productNew.ToCurrencyRate
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			if (response.data.Error === true) {
				ShowResult(response.data.Message, 'failure');
			}
			else {
				ShowResult(response.data.Message, 'success');
				$scope.TotalSumAfterTCS();

			}
		}, function errorCallBack(response) {
			ShowResult(response.data.Message, 'failure');
		});
	}



	$scope.GetAdvanceTaxInfo = function (Id) {

		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/InventoryIssue/GetAdvanceTaxInfo?InventoryReceiveId=' + Id,
		}).then(function successCallback(response) {
			$scope.advanceTaxesList = response.data;


		});

	}
	$scope.removeTaxesRow = function (Id, index) {
		if (baseService.isUndefinedOrNull(Id)) {
			$scope.advanceTaxesList.splice(index, 1);

		}
		else {
			$scope.DeleteAdditinalTax(Id);
			$scope.GetAdvanceTaxInfo($scope.productNew.Id);
		}
	};
	$scope.DeleteAdditinalTax = function (Id) {
		$http({
			method: 'POST',
			url: 'Products/InventoryIssue/AdditionalTaxDelete?Id=' + Id,
			dataType: 'JSON'
		}).then(function (response) {
			if (response.data.Error === true)
				ShowResult(response.data.Message, 'failure');
			else {
				ShowResult(response.data.Message, 'success');
			}
			function errorCallBack(response) {
				ShowResult(response.data.Message, 'failure');
			}
		});
	};
	$scope.TaxOptionAdditax = function (data) {
		debugger;
		$scope.productNew.TaxOptionAddiTax = data;
	};

	$scope.calculateTaxAmountForAdditionalTax = function (data) {
		//$scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.detailList), "TotalAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.detailList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "Amount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "TotalTaxAmount"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);
		$scope.TaxAmountVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.detailList), "TotalAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.detailList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "Amount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "TotalTaxAmount"))).toFixed(2);
		$scope.advanceTax.TaxAmount = (($scope.TaxAmountVal * data) / 100).toFixed(2);

	};
	$scope.checkRowValidationSdditionalTax = function (data) {
		debugger;
		//$scope.TaxAmountVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.detailList), "TotalAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.detailList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "Amount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "TotalTaxAmount"))).toFixed(2);
		$scope.TaxAmountVal1 = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.detailList), "TotalAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.detailList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "Amount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "TotalTaxAmount"))).toFixed(2);
		$scope.advanceTax.ValueOfFixed = ((data / $scope.TaxAmountVal1) * 100).toFixed(4);
	}
	//$scope.TotalSumAfterTCSVal = "";
	$scope.TotalSumAfterTCS = function () {
		//$scope.TaxAmountVal1 = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.detailList), "TotalAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.detailList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "Amount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "TotalTaxAmount"))).toFixed(2);
		$scope.advanceTax.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.detailList), "TotalAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.detailList), "TaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "Amount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.chargesList), "TotalTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);
	}

	//#endregion
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
		if ($scope.status === 'AsOnDate') {

			$scope.productNew.RcptIssue = '';
			$scope.report.FromDate = '';
			$scope.productNew.AsOnDate = 'AsOnDate';
			//$scope.productNew.Qty = true;
			//$scope.productNew.Amount = false;



		}

	}
	$scope.productNew.Summery = 'Summery';
	$scope.change2 = function (e) {

		$scope.statusSumOrDel = e;
		if ($scope.statusSumOrDel === 'Details') {
			//var date = new Date(), y = date.getFullYear(), m = date.getMonth();
			//var firstDay = new Date(y, m, 1);
			//FromDate: $filter('dateFiltering')(new Date(firstDay.getFullYear(), firstDay.getMonth(), 1)),
			//$scope.report.FromDate = $filter('dateFiltering')(new Date(firstDay.getFullYear(), firstDay.getMonth(), 1));
			//$scope.report.ToDate = $filter("dateFiltering")(Date.now());
			$scope.productNew.Details = 'Details';
			$scope.productNew.Summery = 'Details';
		}
		if ($scope.statusSumOrDel === 'Summery') {

			//$scope.productNew.RcptIssue = '';
			//$scope.report.FromDate = '';
			$scope.productNew.Summery = 'Summery';

		}

	}


	$scope.ApprovedStockList = [];
	$scope.getApprovedStock = function (data) {
		$http({
			method: 'POST'
			, url: $scope.path + 'GetApprovedStockDetail'
			, data: { entity: data, issueDate: $scope.productNew.SalesDate }
			, dataType: 'JSON'
		}).then(function (response) {
			$scope.ApprovedStockList = response.data;
			angular.element(document.querySelector('#ApprovedStockPopUp')).modal('show');
		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};
	};

	$scope.UnApprovedStockList = [];
	$scope.getUnApprovedStock = function (data) {
		$http({
			method: 'POST'
			, url: $scope.path + 'GetUnApprovedStockDetail'
			, data: { entity: data, issueDate: $scope.productNew.SalesDate }
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

	$scope.PostingStockList = [];
	$scope.getPostingStock = function (data) {
		$http({
			method: "POST",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/InventoryIssue/GetPostingStockDetail',
			data: { entity: data, issueDate: $scope.productNew.SalesDate }

		}).then(function successCallback(response) {
			$scope.PostingStockList = response.data;
			angular.element(document.querySelector('#PostingStockPopUp')).modal('show');
			//entrydata = copy(searchdata);
		});
	};

	$scope.tab = 1;
	$scope.setTab = function (newTab) {
		$scope.tab = newTab;
	};

	$scope.isSet = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.tabU = 1;
	$scope.setTabU = function (newTab) {
		$scope.tabU = newTab;
	};

	$scope.isSetU = function (tabNum) {
		return $scope.tabU === tabNum;
	};

	$scope.tabP = 1;
	$scope.setTabP = function (newTab) {
		$scope.tabP = newTab;
	};

	$scope.isSetP = function (tabNum) {
		return $scope.tabP === tabNum;
	};



	$scope.closePostingStockPopUp = function () {
		angular.element(document.querySelector('#PostingStockPopUp')).modal('hide');
	};
	$scope.closeApprovedStockPopUp = function () {
		angular.element(document.querySelector('#ApprovedStockPopUp')).modal('hide');
	};
}