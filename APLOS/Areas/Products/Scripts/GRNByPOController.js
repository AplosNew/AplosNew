'use strict';
GRNByPOController.$inject = ['addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function GRNByPOController(addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
	$rootScope.title = "GRN By PO"; //Inventory Receive
	$scope.Action = 'Save';
	$scope.index = -1;
	$scope.products = [];
	$scope.path = 'Products/GoodsReceiveNote/';
	$scope.getListUrl = $scope.path + 'getlist';
	$scope.getListUrl1 = $scope.path + 'GetListForMasterData';
	$scope.getListUrl2 = $scope.path + 'GetListForMasterData2';

	$scope.saveUrl = $scope.path + 'createGRNBYPO';
	$scope.updateUrl1 = $scope.path + 'UpdateGRNBYPO';

	//$scope.saveUrl = $scope.path + 'InsertGRN';
	$scope.updateUrl = $scope.path + 'edit';
	$scope.deleteUrl = $scope.path + 'deleteGRNBYPO/';
	$scope.detailSaveUrl = $scope.path + 'detailcreate';
	$scope.detailDeleteUrl = $scope.path + 'DetailDelete?receiveDetailId=';
	$scope.sreviceSaveUrl = $scope.path + 'servicechargescreate';
	$scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';
	$scope.updateUrlForSRValue = $scope.path + 'UpdateShortageRejectionValueMap';
	$scope.PurchaseOrderFileLocation = virtualPath.GRN;
	$scope.partyType = 'Vendor';
	$scope.isAdvance = false;
	$scope.currentDate = new Date(Date.now());
	$scope.grossTotal = 0;
	$scope.chargesList = [];
	$scope.chargesListPO = [];
	$scope.storageList = [];
	$scope.currencyList = [];
	$scope.detailModelSave = [];
	$scope.inventoryMaterialListPOnew = [];
	$scope.chargesListPOnew = [];

	$controller('partyBaseController', { $scope: $scope, $http: $http });
	$controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
	//, CAST(GRNDate AS DATE)
	//shakawat
	//#region notification setting
	$scope.productId = null;
	$scope.NotificationSettingStatus = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/InventoryReceive/NotificationSetting',
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
				url: 'Products/InventoryReceive/GetCheckedByAndApprovedBY?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
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
		location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;
	};

	//#region GRN Detail
	$scope.lst = [];
	$scope.GRNListDetails = function () {
		//debugger;
		$http({
			method: 'GET',
			//url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
			url: 'Products/GoodsReceiveNote/GRNDetailsData'
		}).then(function successCallback(response) {
			$scope.lst = response.data;
			//$scope.detailgrid($scope.lst);
			window.lst = response.data;

		});
	}
	$scope.GRNListDetails();
	$scope.GRNListDetails();
	$scope.lst1 = [];

	$scope.GRNDocumentMapDataAll = function () {
		//debugger;
		$http({
			method: 'GET',
			//url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
			url: 'Products/GoodsReceiveNote/GRNDocumentMapDataAll'
		}).then(function successCallback(response) {
			$scope.lst = response.data;
			//$scope.detailgrid($scope.lst);
			window.Img = response.data;

		});
	}
	$scope.GRNDocumentMapDataAll();





	$scope.data1 = $scope.lst;
	$scope.detailTemp = "#tabGridContents";
	//$scope.detailgrid = "detailGridData(e)";
	$scope.detailgrid = function detailGridData(e) {
		//debugger;

		var filteredData = e.data["Id"];
		var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("InventoryReceiveId", "equal", parseInt(filteredData), true).take(100));
		e.detailsElement.find("#detailGrid").ejGrid({
			dataSource: data,
			columns: ["MaterialGroupName", "MaterialName", "Article", "SKU1", "SKU2", "SKU3", "MaterialDetail", "TransactionQty", "TransactionUoMId", "TransactionUoM", "TransactionRate", "CurrencyName", "TotalMaterialTranAmount"]
		});
		e.detailsElement.find(".tabcontrol").ejTab();
		//var filteredData1 = e.data["Id"];
		var dataImg = ej.DataManager(window.Img).executeLocal(ej.Query().where("GRNId", "equal", parseInt(filteredData), true).take(100));
		e.detailsElement.find("#detailGrid1").ejGrid({
			dataSource: dataImg,
			columns: [{ field: "UserFilename", headerText: "UserFilename", width: 100 },
			{ field: "Description", headerText: "Description", width: 100 },
			{ field: "Remarks", headerText: "Remarks", width: 100 },

			]
		});
		e.detailsElement.find(".tabcontrol").ejTab();

	}
	//#endregion





	$scope.getDataList = function () {
		baseService.init($scope.getListUrl, null, null, "DESC", "GRNDate", "PartyName");
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
	$scope.getDataList();
	$http({
		method: 'GET',
		url: 'Materials/MaterialStorage/getcbo'
	}).then(function (response) {
		$scope.storageList = response.data;
	});

	//InventoryReceive Model

	$scope.product = {
		Id: null
		, GRNDate: $filter("dateFiltering")(Date.now())
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
		, POId: null
		, IsApproved: 0
		, CheckedBy: null
		, CheckedByStatus: null
		, AuthorizedBy: null
		, AuthorizedByStatus: null
		, NoteForAccounts: null
		, AcceptanceDate: null
		, PurchaseDocumentAcceptanceId: null
		, VoucherId: null
		, InvoiceNo: null
		, InvoiceDate: null
		, DueDate: null
		, PurchaseLCId: null
		, ContractId: null
		, ContractNo: null
		, AcceptancePaymentSource: null
		, LCDate: null
		, PO: null
		, labelCheckAndApproved: null
		, CheckedByStatusForNoti: null
		, ApprovedByStatusForNoti: null
		, TaxOptionAddiTax: 'Yes'
		, TaxOption: 'Yes'
		, TaxOptionMat: 'Yes'
		, TaxOptionService: 'Yes'
		, TaxOptionServiceModify: 'Yes'
		, TaxOptionService1: 'Yes'
		, msgForAllocationNeed: null
	};
	$scope.advanceTax = {
		TaxCodeId: null,
		Text: null,
		TaxAmount: null,
		ValueOfFixed: null,
		CompanyCurrencyAmount: null,
		Type: null,
		TaxCategoryId: null,
		TotalSumAfterTCSVal: null
	};
	$scope.productDocMap = {
		Id: null
		, CompanyGroupId: null
		, FileName: null
		, UserFilename: null
		, SystemFileName: null
		, Description: null
		, Remarks: null
	};
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




	$scope.productNew = Object.assign({}, $scope.product);

	addressService.getCountryCbo(function (result) {
		$scope.countryList = result;
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



	$scope.Get = function (index) {

		$scope.index = index;
		$scope.product = $scope.products[$scope.index];
		$scope.productNew = Object.assign({}, $scope.product);
		// $scope.productNew.GRNDate = data.GRNDate;
		getPartyPlantList();
		getInventoryMaterialList($scope.productNew.Id);
		getServiceChargeList($scope.productNew.Id);
		$scope.productId = $scope.productNew.Id;
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

	$scope.Save = function () {
		
		//debugger;
		//$scope.detailModel.BaseUOMId = $filter("filter")($scope.inventoryMaterialListPO, { check: 1 })[0].Value;
		if ($scope.Action === 'Save') {
			$scope.checkgridcheckornot = $filter("filter")($scope.inventoryMaterialListPO, { check: true });
			if ($scope.checkgridcheckornot.length === 0) {
				ShowResult("Enter atleast one material information", 'failure');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.productNew.DocRefNo)) {
				ShowResult("Enter Doc Ref No", 'failure');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.productNew.DocRefNo)) {
				ShowResult("Enter Doc Ref No", 'failure');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.productNew.DocDate)) {
				ShowResult("Enter Doc Date", 'failure');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.productNew.GateEntryNo)) {
				ShowResult("Select Gate Entry No", 'failure');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.productNew.EntryDate)) {
				ShowResult("Enter Gate Entry Date", 'failure');
				return false;
			}
			if (baseService.isUndefinedOrNull($scope.productNew.GRNDate)) {
				ShowResult("Enter GRN Date", 'failure');
				return false;
			}
			
		}
		
		$scope.inventoryMaterialListPOnew = [];
		$scope.chargesListPOnew = [];
		try {
			if ($scope.Action === 'Update') {
				$scope.modelValidation('div_grnNo', 'productNew', 'Id');
				$scope.modelValidation('div_grnDate', 'productNew', 'GRNDate');
				$scope.manualValidationAddRemove('div_currency', 'productNew', 'CurrencyId');
				if (baseService.isUndefinedOrNull($scope.productNew.DocRefNo)) {
					ShowResult("Enter Doc Ref No", 'failure');
					return false;
				}
				if (baseService.isUndefinedOrNull($scope.productNew.DocRefNo)) {
					ShowResult("Enter Doc Ref No", 'failure');
					return false;
				}
				if (baseService.isUndefinedOrNull($scope.productNew.DocDate)) {
					ShowResult("Enter Doc Date", 'failure');
					return false;
				}
				if (baseService.isUndefinedOrNull($scope.productNew.GateEntryNo)) {
					ShowResult("Select Gate Entry No", 'failure');
					return false;
				}
				if (baseService.isUndefinedOrNull($scope.productNew.EntryDate)) {
					ShowResult("Enter Gate Entry Date", 'failure');
					return false;
				}
				if (baseService.isUndefinedOrNull($scope.productNew.GRNDate)) {
					ShowResult("Enter GRN Date", 'failure');
					return false;
				}
			}

			$scope.$broadcast('show-errors-check-validity');
			if ($scope.productNewForm.$valid) {
				if ($scope.Action === "Save") {
					if (!baseService.isUndefinedOrNull($scope.AcceptanceId) && (new Date($scope.productNew.AcceptanceDate) > new Date($scope.productNew.GRNDate))) {
						ShowResult("Acceptance Date  can not grather than GRN Date", 'failure');
						return false;
					}
					else if (!baseService.isUndefinedOrNull($scope.AcceptanceId) && ($scope.productNew.GRNDate > new Date())) {
						ShowResult("GRN Date  can not grather than Today's Date", 'failure');
						return false;
					}
					else if ($scope.productNew.NoteForAccounts === '' || $scope.productNew.NoteForAccounts === null || $scope.productNew.NoteForAccounts === undefined) {
						ShowResult("Enter Note for accounts", 'failure');
						return false;
					}
					else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
						ShowResult("Please select to be approved by", 'failure');
						return false;
					}
					else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
						ShowResult("Please select to be checked by", 'failure');
						return false;
					}
					else if (baseService.isUndefinedOrNull($scope.productNew.InvoicingPartyPlantId)) {
						return ShowResult('Invoicing by is required', 'failure');
						return false;
					}
					else if (baseService.isUndefinedOrNull($scope.productNew.DeliveryPartyPlantId)) {
						return ShowResult('Delivery by is required', 'failure');
						return false;
					}


					//else if ($scope.productNew.CurrencyId !== $scope.productNew.BaseCurrencyId) {
					//	$scope.manualValidationAddRemove('div_rate  ', 'productNew', 'ToCurrencyRate');

					//}

					else if (new Date($scope.productNew.EntryDate) < new Date($scope.productNew.DocDate)) {
						return manualValidation('div_entryDate', true, "Gate entry date can't be less than Doc Date");
					}

					else if (new Date($scope.productNew.GRNDate) < new Date($scope.productNew.EntryDate)) {
						return manualValidation('div_grnDate', true, "GRN date can't be less than gate entry date");

					}
					else {
						manualValidation('div_grnDate', false);
						manualValidation('div_entryDate', false);
						manualValidation('div_rate', false);
						$scope.modelValidation('div_docNo', 'productNew', 'DocRefNo');
						$scope.modelValidation('div_docDate', 'productNew', 'DocDate');
						//$scope.modelValidation('div_entryNo', 'productNew', 'GateEntryNo');
						//$scope.modelValidation('div_entryDate', 'productNew', 'EntryDate', 'Gate Entry Date');
						$scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
						$scope.product = Object.assign({}, $scope.productNew);
						$scope.product.POId = $scope.POId;
						$scope.product.PurchaseDocumentAcceptanceId = $scope.AcceptanceId;
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
								// $scope.inventoryMaterialListPOnew[i].TotalMaterialBooksCurrencyAmount = $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount;
								$scope.inventoryMaterialListPOnew.push($scope.inventoryMaterialListPO[i]);

							}
							//else if ($scope.inventoryMaterialList[i].check == true) {                           
							//    $scope.inventoryMaterialListPOnew.push($scope.inventoryMaterialList[i]);
							//}
							//else {
							//    ShowResult('Please select Material', 'failure');
							//    break;
							//}
						}
						for (var i = 0; i < $scope.chargesListPO.length; i++) {
							if ($scope.chargesListPO[i].check == true) {
								$scope.chargesListPOnew.push($scope.chargesListPO[i]);
								//$scope.chargesListPOnew.push($scope.chargesList[i]);
							}
							//else if ($scope.chargesList[i].check == true) {                           
							//    $scope.chargesListPOnew.push($scope.chargesList[i]);
							//}
							else {

							}
						}

						//debugger;
						$http({
							method: 'POST',
							url: $scope.saveUrl,
							data:
							{
								'entity': $scope.product,
								'entityMatAndImat': JSON.stringify($scope.inventoryMaterialListPOnew),
								'receiveTaxList': $scope.POMaterialTaxList,
								'chargesListPO': $scope.chargesListPOnew,
								'POServiceTaxList': $scope.POServiceTaxList,
								'GRNType': 'GRNBYPO',
								'AcceptanceId': $scope.AcceptanceId,
								'CheckedByStatusForNoti': $scope.CheckedByStatusForNoti,
								'ApprovedByStatusForNoti': $scope.ApprovedByStatusForNoti
							},
							dataType: 'JSON'
							, contentType: "application/json charset=utf-8"
						}).then(function (response) {
							if (response.data.Error === true) {
								ShowResult(response.data.Message, 'failure');
							}
							else {
								ShowResult(response.data.Message, 'success');
								$scope.SaveButtonDisable = true;
								$scope.setTabGRNList(1);
								$scope.getDataList();
								$scope.GRNListDetails();
								//$scope.Clear();	
								//$scope.inventoryMaterialListPOnew = [];
								//$scope.POMaterialTaxList = [];
								//$scope.chargesListPOnew = [];
								//$scope.POServiceTaxList = [];

								//$scope.productNew.PartyName = null;
								//$scope.productId = null;
								//$scope.AcceptanceId = null;
								//$scope.productNew.GRNDate = null;
								//$scope.productNew.CurrencyId = null;
								//$scope.productNew.ToCurrencyRate = null;
								$scope.productId = response.data.entity.Id;
								$scope.productNew.Id = response.data.entity.Id;
								$scope.productNew.msgForAllocationNeed = response.data.entity.msgForAllocationNeed;
								//$scope.productId = response.data.entity.Id;
								//$scope.productNew.PartyName = $scope.product.PartyName;
								//  $scope.Action = "Update";

							}
						}), function (response) {
							ShowResult(response.data.Message, 'failure');
						};
					}


				}
				else if ($scope.Action === "Update") {
					if (!baseService.isUndefinedOrNull($scope.AcceptanceId) && ($scope.productNew.AcceptanceDate > $scope.productNew.GRNDate)) {
						ShowResult("Acceptance Date  can not grather than GRN Date", 'failure');
						return false;
					}
					else if (!baseService.isUndefinedOrNull($scope.AcceptanceId) && ($scope.productNew.GRNDate > new Date())) {
						ShowResult("GRN Date  can not grather than Today's Date", 'failure');
						return false;
					}
					else if ($scope.productNew.NoteForAccounts === '' || $scope.productNew.NoteForAccounts === null || $scope.productNew.NoteForAccounts === undefined) {
						ShowResult("Enter Note for accounts", 'failure');
						return false;
					}
					else if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
						ShowResult("Please select to be approved by", 'failure');
						return false;
					}
					else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
						ShowResult("Please select to be checked by", 'failure');
						return false;
					}
					else if (baseService.isUndefinedOrNull($scope.productNew.InvoicingPartyPlantId)) {
						return ShowResult('Invoicing by is required', 'failure');
						return false;
					}
					else if (baseService.isUndefinedOrNull($scope.productNew.DeliveryPartyPlantId)) {
						return ShowResult('Delivery by is required', 'failure');
						return false;
					}
					//else if ($scope.productNew.CurrencyId != $scope.productNew.BaseCurrencyId) {
					//	$scope.manualValidationAddRemove('div_rate  ', 'productNew', 'ToCurrencyRate');

					//}
					else if (new Date($scope.productNew.EntryDate) < new Date($scope.productNew.DocDate)) {
						return manualValidation('div_entryDate', true, "Gate entry date can't be less than Doc Date");
					}
					else if (new Date($scope.productNew.GRNDate) < new Date($scope.productNew.EntryDate)) {
						return manualValidation('div_grnDate', true, "GRN date can't be less than gate entry date");

					}
					else {
						manualValidation('div_grnDate', false);
						manualValidation('div_entryDate', false);
						manualValidation('div_rate', false);
						$scope.modelValidation('div_docNo', 'productNew', 'DocRefNo');
						$scope.modelValidation('div_docDate', 'productNew', 'DocDate');
						//$scope.modelValidation('div_entryNo', 'productNew', 'GateEntryNo');
						//$scope.modelValidation('div_entryDate', 'productNew', 'EntryDate', 'Gate Entry Date');
						$scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
						$scope.product = Object.assign({}, $scope.productNew);
						$scope.product.POId = $scope.POId;
						$scope.product.PurchaseDocumentAcceptanceId = $scope.AcceptanceId;
						for (var i3 = 0; i3 < $scope.inventoryMaterialList.length; i3++) {
							if ($scope.inventoryMaterialList[i3].check == true) {
								//$scope.inventoryMaterialListPOnew = [];
								$scope.inventoryMaterialListPOnew.push($scope.inventoryMaterialList[i3]);
							}
							else {

							}
						}
						for (var i4 = 0; i4 < $scope.chargesList.length; i4++) {
							if ($scope.chargesList[i4].check == true) {
								//$scope.chargesListPOnew = [];
								$scope.chargesListPOnew.push($scope.chargesList[i4]);
							}

							else {

							}
						}
						$http({
							method: 'POST',
							//url: $scope.updateUrl,
							url: $scope.updateUrl1,
							//data: $scope.product,
							data:
							{
								'entity': $scope.product,
								'entityMatAndImat': $scope.inventoryMaterialListPOnew,
								'receiveTaxList': $scope.MaterialTaxList,
								'chargesListPO': $scope.chargesListPOnew,
								'POServiceTaxList': $scope.ServiceTaxList,
								'GRNType': 'GRNBYPO',
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
								$scope.setTabGRNList(1);
								$scope.getDataList();
								$scope.GRNListDetails();
								//$scope.Clear();								
								//$scope.inventoryMaterialListPOnew = [];
								//$scope.MaterialTaxList = [];
								//$scope.chargesListPOnew = [];
								//$scope.ServiceTaxList = [];
								$scope.productId = response.data.entity.Id;
								$scope.productNew.Id = response.data.entity.Id;
								$scope.productNew.msgForAllocationNeed = response.data.entity.msgForAllocationNeed;

							}
						}, function errorCallBack(response) {
							ShowResult(response.data.Message, 'failure');
						});




					}
				}
			}
		} catch (e) {
			throw e;
		}
	};

	$scope.Delete = function () {
		//debugger;
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
						ShowResult('Data Deleted Successfully', 'success');
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
		return true;
	};

	function ClearFields() {
		$scope.SaveButtonDisable = "";
		$scope.Action = "Save";
		$scope.GriddataSelected = [];
		// $scope.product = { POId: $scope.product.POId };
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
			//, POId: $scope.product.POId 

			, GRNDate: $filter("dateFiltering")(Date.now())

		};
		$scope.AcceptanceId = '';
		$scope.inventoryMaterialListPOnew = [];
		$scope.MaterialTaxList = [];
		$scope.chargesListPOnew = [];
		$scope.ServiceTaxList = [];
		$scope.advanceTaxesList = [];
		$scope.TotalSumAfterTCSVal = "";

		// $scope.POId1 = '';
		$scope.NotificationSettingStatus();
		$scope.inventoryMaterialListPO = [];
		$scope.chargesListPO = [];
		$scope.inventoryMaterialList = [];
		$scope.chargesList = [];
		$scope.grossTotal = 0;
		baseService.removeErrorClasses();
		//$scope.getToCurrencyRate();

		//$scope.inventoryMaterialListPOnew = [];
		//$scope.MaterialTaxList = [];
		//$scope.chargesListPOnew = [];
		//$scope.ServiceTaxList = [];

	}
	$scope.changeAllInvoice = function () {
		$scope.productNew.InvoiceNo = null;
		$scope.productNew.InvoiceDate = null;
	};

	$scope.closePartyPopUp = function () {
		if ($scope.partyIndex !== -1) {
			var party = $scope.partyList[$scope.partyIndex];
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
		}
		$scope.hidePartyPopUp();
	};

	function getPartyPlantList() {
		//debugger;
		//$scope.plantList = [];
		//$http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.productNew.PartyId).then(function (response) {
		//	angular.forEach(response.data, function (item) {
		//		$scope.plantList.push(item);
		//		if (item.IsDefault) {
		//			$scope.productNew.InvoicingPartyPlantId = item.Value;
		//			$scope.productNew.DeliveryPartyPlantId = item.Value;
		//			$scope.productNew.InvoicingByAddress = item.Address1;
		//			$scope.productNew.DeliveryByAddress = item.Address1;
		//			$scope.productNew.InvoicingState = item.StateName;
		//			$scope.productNew.InvoicingGSTIN = item.GSTIN;
		//			$scope.productNew.DeliveryState = item.StateName;
		//			$scope.productNew.DeliveryGSTIN = item.GSTIN;
		//		}
		//	});
		//});
	}
	function getPartyPlantListPO() {
		//debugger;
		$scope.plantList = [];
		$http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.productNew.partyId).then(function (response) {
			angular.forEach(response.data, function (item) {
				$scope.plantList.push(item);
				if (item.IsDefault) {
					$scope.productNew.InvoicingPartyPlantId = item.Value;
					$scope.productNew.DeliveryPartyPlantId = item.Value;
					$scope.productNew.InvoicingByAddress = item.Address1;
					$scope.productNew.DeliveryByAddress = item.Address1;
					$scope.productNew.InvoicingState = item.StateName;
					$scope.productNew.InvoicingGSTIN = item.GSTIN;
					$scope.productNew.DeliveryState = item.StateName;
					$scope.productNew.DeliveryGSTIN = item.GSTIN;
				}
			});
		});
	}
	$scope.getToCurrencyRate = function () {
		if (!baseService.isUndefinedOrNull(AcceptanceId)) {
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
		}


	};
	$scope.invoicingPartyPopUp = function () {
		angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
	};
	$scope.closeInvoicingPartyPopUp = function () {
		angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
	};
	$scope.billShippAddress = function (id, flag) {
		if (!baseService.isUndefinedOrNull(id)) {
			var address = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].Address1;
			var state = $.grep($scope.plantList, function (item) { return item.Value === id; })[0].StateName;
			if (flag === 'billTo') {
				$scope.productNew.InvoicingState = state;
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
		//if (inventoryMaterialList.Active) {
		//    for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
		//        if ($scope.inventoryMaterialList[i].check === true) {
		//            ShowResult('Select atleast one line', 'failure');
		//        }
		//    }
		//}
		$scope.tab = newTab;
	};
	$scope.isSet = function (tabNum) {
		return $scope.tab === tabNum;
	};

	// #region Details
	$scope.detailModelSave = {
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
		, TransactionRate: 0
		, TransactionAmount: 0
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
	};
	$scope.businessProcesses = '';//"BP.BusinessProcessName IN('MaintenanceSpare','BOM','WetProcess','Consumable')";
	$scope.detailPopUp = function () {
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
			, TransactionRate: 0
			, TransactionAmount: 0
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
		};
		$scope.clearCharNames();
		angular.element(document.querySelector('#detailPopUp')).modal('show');
	};
	$scope.closeDetaiPopUp = function () {
		$scope.detailModel = {};
		$scope.taxCategoryList = [];
		removeValidationMsg();
		angular.element(document.querySelector('#detailPopUp')).modal('hide');
	};

	$scope.materialType = ['Asset', 'Consumable', 'Spare', 'RawMaterial'];
	//$scope.setMaterialMasterData
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

	$scope.detailSave = function () {
		try {
			$scope.validation();
			$scope.detailModel.InventoryReceiveId = $scope.productNew.Id;
			$scope.detailModel.FirstCharacteristicsId = $scope.char1.CharacteristicsId;
			$scope.detailModel.FirstCharacteristicsValueId = $scope.char1.CharacteristicsValueId;
			$scope.detailModel.SecondCharacteristicsId = $scope.char2.CharacteristicsId;
			$scope.detailModel.SecondCharacteristicsValueId = $scope.char2.CharacteristicsValueId;
			$scope.detailModel.ThirdCharacteristicsId = $scope.char3.CharacteristicsId;
			$scope.detailModel.ThirdCharacteristicsValueId = $scope.char3.CharacteristicsValueId;

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

			$http({
				method: 'POST',
				url: $scope.detailSaveUrl,
				data: {
					entity: $scope.detailModel
					, taxCategoryList: $scope.taxCategoryList
				},
				dataType: 'JSON'
			}).then(function successCallback(response) {
				if (response.data.Error === true)
					ShowResult(response.data.Message, 'failure', 'detailPopUp');
				else {
					ShowResult(response.data.Message, 'success', 'detailPopUp');
					$scope.detailModel.Id = null;
					$scope.detailModel = {
						InventoryReceiveId: $scope.productNew.Id
						, MaterialStorageId: $scope.productNew.MaterialStorageId
						, CurrencyName: angular.element("#currency :selected").text()
						, CurrencyId: $scope.productNew.CurrencyId
						, BaseCurrencyId: $scope.baseCurrencyId
						, DocDate: $scope.productNew.DocDate
						, TotalAmount: 0
						, TransactionAmount: 0
						, ToCurrencyRate: $scope.productNew.ToCurrencyRate
						, IsNonCreditable: $scope.productNew.IsNonCreditable
						, IsOriginApplicable: false
					};
					$scope.taxCategoryList = [];
					getInventoryMaterialList($scope.productNew.Id);
					$scope.getDataList();
					$scope.clearCharNames();
				}
			}), function errorCallBack(response) {
				ShowResult(response.data.Message, 'failure', 'detailPopUp');
			};
		} catch (e) {
			//ShowResult(e, 'fail', 'detailPopUp');
		}
	};


	$scope.valuePassInDelModal = function (id) {
		$scope.id = id;
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

	$scope.sumORnot = false;
	function getInventoryMaterialList(inveReveiveId) {
		$scope.masterId5 = inveReveiveId;
		$http.get($scope.path + 'GetInventoryMaterialList?inveReveiveId=' + inveReveiveId + '&POID=' + $scope.POID + '&AcceptanceId=' + $scope.AcceptanceId)
			.then(function (response) {
				$scope.inventoryMaterialList = [];
				$scope.inventoryMaterialList = response.data.Rows;
				$scope.POIDs = $scope.inventoryMaterialList.POId;
				//$scope.productNew.CheckedBy = $scope.inventoryMaterialList[0].CheckedBy;
				$scope.productNew.PODate = $scope.inventoryMaterialList[0].AddedDate;
				checkSameValueInColumnList($scope.inventoryMaterialList, 'TransactionUoM');
				getGrossAmount($scope.inventoryMaterialList, 'BaseAmount', 'BaseTaxAmount', 'ChargesAmount', 'grossTotal');
				$scope.GetMaterialTaxData();
			});
	}



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
	$scope.sumTaxAmount = function () {
		$scope.detailModel.TotalTaxAmount = 0;
		for (var i = 0; i < baseService.arrayLength($scope.taxCategoryList); i++) {
			$scope.detailModel.TotalTaxAmount = (parseFloat($scope.detailModel.TotalTaxAmount) + parseFloat($scope.taxCategoryList[i].TaxAmount)).toFixed($rootScope.currencyPrecision);
		}
	};
	$scope.getReceiveTaxList = function (data, flag, index, Id) {
		//$scope.taxAbleAmnt = data.TrnAmount;
		//$scope.percentageColumn = flag;
		//$http({
		//    method: 'GET',
		//    url: $scope.path + 'GetReceiveTaxList?receiveDetailId=' + data.InventoryReceiveDetailId
		//}).then(function (response) {
		//    $scope.receiveTaxList = response.data;
		//    angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
		//});

		//debugger;
		$scope.productNew.TaxOptionAddiTax = 'Yes';
		$scope.receiveTaxindex = index;
		$scope.taxAbleAmnt = data.TrnAmount;
		$scope.percentageColumn = flag;
		$scope.Currency = $("#currency option:selected").text();
		$scope.currentMaterialRow = index;
		$scope.currentInventoryReceiveDetailIdRow = Id;
		$scope.taxAbleAmnt = data.TrnAmount;
		$scope.percentageColumn = flag;
		$scope.currentMaterialRow = index;
		$scope.receiveTaxList = [];
		if (data.MaterialTaxList.length > 0) {
			$scope.HSNCode = data.MaterialTaxList[0].HSNCode;
			$scope.receiveTaxList = data.MaterialTaxList;
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
	$scope.closeReceiveTaxPopUp = function () {
		//debugger;
		$scope.detailModel = {};
		$scope.receiveTaxList = [];

		for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
			//if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
			//    $scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;
			//    $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
			//    $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
			//    $scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
			//}
			//else {
			//    $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
			//    $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
			//    $scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
			//}
			if ($scope.productNew.IsNonCreditable == 1) {
				//data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);               
				//$scope.inventoryMaterialListPO[i].BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax);
				$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat(parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax).toFixed(2)).toFixed(2);
				$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax).toFixed(2)) * $scope.productNew.ToCurrencyRate).toFixed(2);


			}
			else {
				//data.BaseAmount = parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge);
				$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2);
				$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2)) * $scope.productNew.ToCurrencyRate).toFixed(2);
			}

		}
		angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
	}


	$scope.closeReceiveTaxPopUpValue = function (x) {
		//debugger;
		if ($scope.Action === 'Save') {
			for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
				var row = $filter('filter')($scope.new, { 'PODetailsID': $scope.inventoryMaterialListPO[i].PODetailsID });
				if (row.length != 0) {
					if ($scope.inventoryMaterialListPO[i].PODetailsID === row[0].PODetailsID) {
						$scope.inventoryMaterialListPO[i].ShortageRate = row[0].ShortageRate;
						$scope.inventoryMaterialListPO[i].ShortageValue = row[0].ShortageValue;
						$scope.inventoryMaterialListPO[i].RejectionRate = row[0].RejectionRate;
						$scope.inventoryMaterialListPO[i].RejectionValue = row[0].RejectionValue;
						$scope.inventoryMaterialListPO[i].RejectionClamRate = row[0].RejectionClamRate;
					}
					angular.element(document.querySelector('#ValueSet')).modal('hide');
				}
				else {
					angular.element(document.querySelector('#ValueSet')).modal('hide');
				}

			}
			angular.element(document.querySelector('#ValueSet')).modal('hide');
		}
		else {
			//for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
			//    if ($scope.inventoryMaterialList[i].InventoryReceiveDetailId === $scope.PODetailsID) {
			//        $scope.inventoryMaterialList[i].ShortageRate = $scope.inventoryMaterialList[i].ShortageRate;
			//        $scope.inventoryMaterialList[i].ShortageValue = $scope.inventoryMaterialList[i].ShortageValue;
			//        $scope.inventoryMaterialList[i].RejectionRate = $scope.inventoryMaterialList[i].RejectionRate;
			//        $scope.inventoryMaterialList[i].RejectionValue = $scope.inventoryMaterialList[i].RejectionValue;
			//        $scope.inventoryMaterialList[i].RejectionClamRate = $scope.inventoryMaterialList[i].RejectionClamRate;
			//    }


			//}
			for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
				var row = $filter('filter')($scope.new1, { 'PODetailsID': $scope.inventoryMaterialList[i].PODetailsID });
				if (row.length != 0) {
					if ($scope.inventoryMaterialList[i].PODetailsID === row[0].PODetailsID) {
						$scope.inventoryMaterialList[i].ShortageRate = row[0].ShortageRate;
						$scope.inventoryMaterialList[i].ShortageValue = row[0].ShortageValue;
						$scope.inventoryMaterialList[i].RejectionRate = row[0].RejectionRate;
						$scope.inventoryMaterialList[i].RejectionValue = row[0].RejectionValue;
						$scope.inventoryMaterialList[i].RejectionClamRate = row[0].RejectionClamRate;
					}
					angular.element(document.querySelector('#ValueSet')).modal('hide');
				}
				else {
					angular.element(document.querySelector('#ValueSet')).modal('hide');
				}
				angular.element(document.querySelector('#ValueSet')).modal('hide');
			}
			angular.element(document.querySelector('#ValueSet')).modal('hide');
		}
		//angular.element(document.querySelector('#ValueSet')).modal('hide');
		//     if ($scope.Action === 'Save') {
		//         for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
		//             var row = $filter('filter')($scope.new, { 'PODetailsID': $scope.inventoryMaterialList[i].PODetailsID });
		//             if (row.length != 0) {
		//                 if ($scope.inventoryMaterialList[i].PODetailsID === row[0].PODetailsID) {
		//                     $scope.inventoryMaterialList[i].ShortageRate = row[0].ShortageRate;
		//                     $scope.inventoryMaterialList[i].ShortageValue = row[0].ShortageValue;
		//                     $scope.inventoryMaterialList[i].RejectionRate = row[0].RejectionRate;
		//                     $scope.inventoryMaterialList[i].RejectionValue = row[0].RejectionValue;
		//                     $scope.inventoryMaterialList[i].RejectionClamRate = row[0].RejectionClamRate;
		//		}
		//		//angular.element(document.querySelector('#ValueSet')).modal('hide');
		//             }
		//             else {
		//                 angular.element(document.querySelector('#ValueSet')).modal('hide');
		//             }
		//	angular.element(document.querySelector('#ValueSet')).modal('hide');
		//}
		//angular.element(document.querySelector('#ValueSet')).modal('hide');
		//     }
		//     else {
		//         //for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
		//         //    if ($scope.inventoryMaterialList[i].InventoryReceiveDetailId === $scope.PODetailsID) {
		//         //        $scope.inventoryMaterialList[i].ShortageRate = $scope.inventoryMaterialList[i].ShortageRate;
		//         //        $scope.inventoryMaterialList[i].ShortageValue = $scope.inventoryMaterialList[i].ShortageValue;
		//         //        $scope.inventoryMaterialList[i].RejectionRate = $scope.inventoryMaterialList[i].RejectionRate;
		//         //        $scope.inventoryMaterialList[i].RejectionValue = $scope.inventoryMaterialList[i].RejectionValue;
		//         //        $scope.inventoryMaterialList[i].RejectionClamRate = $scope.inventoryMaterialList[i].RejectionClamRate;
		//         //    }


		//         //}
		//         for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
		//             var row = $filter('filter')($scope.new1, { 'PODetailsID': $scope.inventoryMaterialList[i].PODetailsID });
		//             if (row.length != 0) {
		//                 if ($scope.inventoryMaterialList[i].PODetailsID === row[0].PODetailsID) {
		//                     $scope.inventoryMaterialList[i].ShortageRate = row[0].ShortageRate;
		//                     $scope.inventoryMaterialList[i].ShortageValue = row[0].ShortageValue;
		//                     $scope.inventoryMaterialList[i].RejectionRate = row[0].RejectionRate;
		//                     $scope.inventoryMaterialList[i].RejectionValue = row[0].RejectionValue;
		//                     $scope.inventoryMaterialList[i].RejectionClamRate = row[0].RejectionClamRate;
		//		}

		//             }
		//             else {
		//                 angular.element(document.querySelector('#ValueSet')).modal('hide');
		//             }
		//	angular.element(document.querySelector('#ValueSet')).modal('hide');
		//         }
		//     }        
		//     angular.element(document.querySelector('#ValueSet')).modal('hide');
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
		if ($scope.Action === 'Update') {
			$scope.productNew.TaxOptionService1 = 'Yes';
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
				, TransactionAmount: 0
				, BaseAmount: 0
				, TotalTaxAmount: 0
				, ToCurrencyRate: $scope.productNew.ToCurrencyRate
				, IsNonCreditable: $scope.productNew.IsNonCreditable
			};
			angular.element(document.querySelector('#serviceChargePopUp')).modal('show');
		}
		else {
			if (baseService.arrayLength($scope.inventoryMaterialListPO) === 0)
				return ShowResult('Without material charges not aplicable.');
			$scope.serviceModel = {
				Id: null
				, ServiceMasterId: null
				, InventoryReceiveId: $scope.productNew.Id
				, CurrencyName: angular.element("#currency :selected").text()
				, CurrencyId: $scope.productNew.CurrencyId
				, BaseCurrencyId: $scope.baseCurrencyId
				, DocDate: $scope.productNew.DocDate
				, TransactionAmount: 0
				, BaseAmount: 0
				, TotalTaxAmount: 0
				, ToCurrencyRate: $scope.productNew.ToCurrencyRate
				, IsNonCreditable: $scope.productNew.IsNonCreditable
			};
			angular.element(document.querySelector('#serviceChargePopUp')).modal('show');
		}

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
						, TransactionAmount: 0
						, BaseAmount: 0
						, TotalTaxAmount: 0
						, ToCurrencyRate: $scope.productNew.ToCurrencyRate
						, IsNonCreditable: $scope.productNew.IsNonCreditable
					};
					$scope.taxCategoryList = [];
					getServiceChargeList($scope.productNew.Id);
					getInventoryMaterialList($scope.productNew.Id);
					$scope.getDataList();

					//$scope.getDataList();
					//$scope.GRNListDetails();
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


	//Command By shakawat If need open
	//$scope.getServiceTaxListPO = function (data, flag) {
	//    //debugger;
	//    $scope.taxAbleAmnt = data.Amount + data.TotalTaxAmount;
	//    $scope.percentageColumn = flag;
	//    $http({
	//        method: 'GET',
	//        url: $scope.path + 'GetServiceTaxListPO?serviceId=' + data.Id
	//    }).then(function (response) {
	//        $scope.receiveTaxListPO = response.data;
	//        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
	//    });
	//}

	function getServiceChargeList(inveReveiveId) {
		$scope.masterId12 = inveReveiveId;
		//debugger;
		$http.get($scope.path + 'GetServiceChargeList?receiveId=' + inveReveiveId)
			.then(function (response) {
				$scope.chargesList = [];
				$scope.chargesList = response.data;
				$scope.getServiceTaxList();

			});
	}

	// #endregion Service

	$scope.inventoryReceiveReport = function (id, reportFormat) {
		if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
		$window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + id + '&plantId=' + $scope.productNew.PlantId);
	};
	$scope.Griddata = [];
	$scope.getalldata = function () {
		//debugger;
		var PoType = 'PO';
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/GoodsReceiveNote/GetListOfPO?PoType=' + PoType + '&Status=' + $scope.status,
		}).then(function successCallback(response) {
			$scope.Griddata = response.data;
			$scope.productNew.GRNDate = $filter("dateFiltering")(Date.now());
		});
	};



	$scope.GetSavedPOListNew = [];
	$scope.GetSavedPOList1 = function (Id) {
		//debugger;
		var PoType = 'PO';
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/GoodsReceiveNote/GetSavedPOList?GRNId=' + Id,
		}).then(function successCallback(response) {
			//$scope.GetSavedPOListNew = [];
			$scope.GetSavedPOListNew = response.data;
			for (var i = 0; i < $scope.GetSavedPOListNew.length; i++) {
				$scope.GriddataSelected.push($scope.GetSavedPOListNew[i]);
			}

		});
	};

	// #region shakawat
	//$scope.POPopUp = function () {
	//    $scope.getalldata();

	//    angular.element(document.querySelector('#POPopUp')).modal('show');

	//};

	$scope.status = 'PO';

	$scope.POPopUp = function () {
		//$scope.getalldata();
		//debugger
		$scope.status = 'PO';
		if ($scope.status === 'PO') {
			$scope.status = 'PO';
			//alert('1');
			$scope.productNew.PO = 'PO';
			$scope.getalldata();
		}
		else if ($scope.status === 'Acceptance') {
			$scope.status = 'Acceptance';
			$scope.productNew.PO = 'Acceptance';
			$scope.getalldata();
		}
		angular.element(document.querySelector('#POPopUp')).modal('show');

	};
	$scope.POPopUpNew = function () {
		$scope.getalldata();
		//debugger
		$scope.status === 'PO';
		if ($scope.status === 'PO') {
			$scope.status === 'PO';
			//alert('1');
			$scope.getalldata();
		}
		else if ($scope.status === 'Acceptance') {
			$scope.status === 'Acceptance';
			$scope.getalldata();
		}
		angular.element(document.querySelector('#POPopUp1')).modal('show');

	};



	$scope.POPopUpCloseNew = function () {
		//debugger;
		angular.element(document.querySelector('#POPopUp1')).modal('hide');

	};



	$scope.change = function (e) {
		//debugger;
		$scope.status = e;
		$scope.productNew.PO = $scope.status;
		//if ($scope.status === 'PO') {
		//    // var date = new Date(), y = date.getFullYear(), m = date.getMonth();
		//    // var firstDay = new Date(y, m, 1);
		//    // FromDate: $filter('dateFiltering')(new Date(firstDay.getFullYear(), firstDay.getMonth(), 1)),
		//    //     //$scope.report.FromDate = $filter("dateFiltering")(Date.now());

		//    //$scope.report.FromDate = $filter('dateFiltering')(new Date(firstDay.getFullYear(), firstDay.getMonth(), 1));
		//    // $scope.report.ToDate = $filter("dateFiltering")(Date.now());
		//    // $scope.productNew.ForThePeriod = 'ForThePeriod';
		//    // //$scope.productNew.Qty = true;
		//    // //$scope.productNew.Amount = false;

		//}
		//if ($scope.status === 'Acceptance') {

		//    //$scope.productNew.RcptIssue = '';
		//    //$scope.report.FromDate = '';
		//    //$scope.productNew.AsOnDate = 'AsOnDate';
		//    ////$scope.productNew.Qty = true;
		//    ////$scope.productNew.Amount = false;



		//}

	}
	$scope.POPopUpClose = function () {
		//debugger;
		angular.element(document.querySelector('#POPopUp')).modal('hide');

	};

	$scope.CheckAll = function (event) {


		var _isselected = event.target.checked;
		for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
			$scope.inventoryMaterialListPO[i].check = _isselected;
		}
	};

	$scope.CheckAll1 = function (event) {
		var _isselected = event.target.checked;

		for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {

			$scope.inventoryMaterialList[i].check = _isselected;
		}
	};
	$scope.GriddataSelected = [];
	$scope.recorddoubleclick = function ($event) {
		//debugger;

		//if ($event.data.AcceptanceId != null || $event.data.AcceptanceId != undefined) {
		//    $scope.AcceptanceId = $event.data.AcceptanceId;
		//    $scope.load();
		//    angular.element(document.querySelector('#POPopUp1')).modal('hide');
		//}
		//else {



		$scope.Griddatatemp = [];
		$scope.Griddatatemp1 = [];
		var partyId = null;
		$scope.tempList = [];
		for (var j = 0; j < $scope.Griddata.length; j++) {
			if ($scope.Griddata[j].Active === true) {
				$scope.tempList.push($scope.Griddata[j]);
			}
		}
		var flagTemp = false;
		if ($scope.tempList.length > 0) {
			for (var k = 0; k < $scope.tempList.length; k++) {
				if ($scope.tempList[k].PartyId != $scope.tempList[0].PartyId) {// ||  && $scope.tempList[k].CurrencyId != $scope.tempList[0].CurrencyId
					flagTemp = true;
					// angular.element(document.querySelector('#POPopUp')).modal('hide');
					ShowResult('Have you selected Same vendor?', 'failure', 'POPopUp');
					return;

				}
				else if ($scope.tempList[k].InvoicingPartyPlantId != $scope.tempList[0].InvoicingPartyPlantId) {// ||  && $scope.tempList[k].CurrencyId != $scope.tempList[0].CurrencyId
					flagTemp = true;
					// angular.element(document.querySelector('#POPopUp')).modal('hide');
					ShowResult('Have you selected Same Invoicing Party?', 'failure', 'POPopUp');
					return;

				}
				else if ($scope.tempList[k].POType != $scope.tempList[0].POType) {
					ShowResult('Have you selected Same Types of PO?', 'failure', 'POPopUp');
					return;
				}
				else if ($scope.tempList[k].CurrencyId != $scope.tempList[0].CurrencyId) {
					ShowResult('Have you selected Same Currency of PO?', 'failure', 'POPopUp');
					return;
				}

			}
		}


		if (flagTemp == false) {

			$scope.load();
			for (var x = 0; x < $scope.tempList.length; x++) {
				$scope.GriddataSelected.push($scope.tempList[x]);
			}


		}

		angular.element(document.querySelector('#POPopUp')).modal('hide');
		// }


	}
	$scope.recorddoubleclickforAcceptance = function ($event) {
		//debugger;
		$scope.Clear();
		if ($event.data.AcceptanceId != null || $event.data.AcceptanceId != undefined) {
			$scope.AcceptanceId = $event.data.AcceptanceId;
			$scope.productNew.LCDate = $event.data.LCDate;
			$scope.productNew.ContractNo = $event.data.ContractNo;
			if ($event.data.IsNonCreditable === 'Yes') {
				$scope.productNew.IsNonCreditable = true;

			}
			else {
				$scope.productNew.IsNonCreditable = false;

			}
			$scope.load();
			$scope.loadAcceptanceDetail();
			$scope.productNew.TaxOptionAddiTax = 'Yes';
			angular.element(document.querySelector('#POPopUp1')).modal('hide');
		}
	}
	$scope.loadAcceptanceDetailList = [];
	$scope.loadAcceptanceDetail = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/GoodsReceiveNote/LoadAcceptanceDetails?AcceptanceId=' + $scope.AcceptanceId,
		}).then(function successCallback(response) {


			$scope.loadAcceptanceDetailList = response.data;

			$scope.productNew.InvoicingPartyPlantId = $scope.loadAcceptanceDetailList[0].InvoicingPartyPlantId;
			$scope.productNew.InvoicingByName = $scope.loadAcceptanceDetailList[0].InvoicingBy;
			$scope.productNew.InvoicingPartyPlantId = $scope.loadAcceptanceDetailList[0].InvoicingPartyPlantId;
			$scope.productNew.InvoicingPartyPlantId = $scope.loadAcceptanceDetailList[0].InvoicingPartyPlantId;
			$scope.productNew.InvoicingByAddress = $scope.loadAcceptanceDetailList[0].InvoicingByAddress;

			$scope.productNew.DeliveryPartyPlantId = $scope.loadAcceptanceDetailList[0].DeliveryPartyPlantId;
			$scope.productNew.DeliveryByName = $scope.loadAcceptanceDetailList[0].DeliveryBy;
			$scope.productNew.DeliveryPartyPlantId = $scope.loadAcceptanceDetailList[0].DeliveryPartyPlantId;
			$scope.productNew.DeliveryPartyPlantId = $scope.loadAcceptanceDetailList[0].DeliveryPartyPlantId;
			$scope.productNew.DeliveryByAddress = $scope.loadAcceptanceDetailList[0].DeliveryByAddress;
			$scope.productNew.ToCurrencyRate = $scope.loadAcceptanceDetailList[0].AcceptanceRate;
			$scope.productNew.CurrencyId = $scope.loadAcceptanceDetailList[0].CurrencyId;




			$scope.productNew.PartyName = $scope.loadAcceptanceDetailList[0].PartyName;
			$scope.productNew.DocRefNo = $scope.loadAcceptanceDetailList[0].DocRefNo;
			$scope.productNew.DocDate = $scope.loadAcceptanceDetailList[0].DocDate;
			$scope.productNew.CurrencyId = $scope.loadAcceptanceDetailList[0].CurrencyId;
			$scope.productNew.ToCurrencyRate = $scope.loadAcceptanceDetailList[0].AcceptanceRate;

			//$scope.productNew.AcceptanceDate = $scope.loadAcceptanceDetailList[0].AcceptanceDate;
			$scope.productNew.VoucherId = $scope.loadAcceptanceDetailList[0].VoucherId;
			$scope.productNew.InvoiceNo = $scope.loadAcceptanceDetailList[0].InvoiceNo;
			$scope.productNew.InvoiceDate = $scope.loadAcceptanceDetailList[0].InvoiceDate;
			$scope.productNew.DueDate = $scope.loadAcceptanceDetailList[0].DueDate;
			//$scope.productNew.PurchaseLCId = $scope.loadAcceptanceDetailList[0].PurchaseLCId;
			//$scope.productNew.ContractId = $scope.loadAcceptanceDetailList[0].ContractId;
			$scope.productNew.PurchaseLCId = $scope.loadAcceptanceDetailList[0].LCANo;
			$scope.productNew.LCDate = $scope.loadAcceptanceDetailList[0].LCDate;

			$scope.productNew.ContractId = $scope.loadAcceptanceDetailList[0].ContractId;

			//$scope.productNew.ContractId = $scope.loadAcceptanceDetailList[0].ContractNo;

			$scope.productNew.AcceptancePaymentSource = $scope.loadAcceptanceDetailList[0].AcceptancePaymentSource;
			$scope.productNew.PartyId = $scope.loadAcceptanceDetailList[0].PartyId;
			$scope.productNew.partySearchByList = $scope.loadAcceptanceDetailList[0].PartyId;
			//$scope.productId = "";
			$scope.AcceptanceId = $scope.AcceptanceId;
			getPartyPlantList();
			//getPartyPlantEditList();
			GetInventoryMaterialListByPO(id1, $scope.AcceptanceId);
			getServiceChargeListPO(id1);
			$scope.productNew.PO = $scope.status;
			if ($scope.loadAcceptanceDetailList[0].IsNonCreditable === 'Yes') {
				$scope.productNew.IsNonCreditable = true;

			}
			else {
				$scope.productNew.IsNonCreditable = false;


			}
		});
	}


	$scope.load = function () {

		if ($scope.AcceptanceId === null || $scope.AcceptanceId === "" || $scope.AcceptanceId === undefined) {

			//#region load
			var id1 = "''";
			var PartyId = '';
			//var PartyCode = '';
			$scope.productNew.DocRefNo = '';
			$scope.productNew.DocDate = '';
			$scope.productNew.DocDate = '';
			$scope.productNew.CurrencyId = '';
			$scope.productNew.DocDate = '';
			$scope.productNew.PartyName = '';
			$scope.GriddataSelected = [];
			for (var i = 0; i < $scope.Griddata.length; i++) {
				if ($scope.Griddata[i].Active === true) {
					id1 += ",'" + $scope.Griddata[i].Id + "'";
					//AcceptanceId = $scope.Griddata[i].AcceptanceId;
					PartyId = $scope.Griddata[i].PartyId;
					//PartyCode = $scope.Griddata[i].PartyCode;
					$scope.productNew.PartyName = $scope.Griddata[i].PartyName;
					$scope.productNew.DocRefNo = '';//$scope.Griddata[i].DocRefNo;
					$scope.productNew.DocDate = '';//$scope.Griddata[i].DocDate;
					$scope.productNew.CurrencyId = $scope.Griddata[i].CurrencyId;
					$scope.productNew.ToCurrencyRate = $scope.Griddata[i].ToCurrencyRate;
					$scope.productNew.IsNonCreditable = $scope.Griddata[i].IsNonCreditable;
					$scope.productNew.AcceptanceDate = $scope.Griddata[i].AcceptanceDate;

					$scope.productNew.PaymentTermId = $scope.Griddata[i].PaymentTermId;
					$scope.productNew.BaseNoOfDays = $scope.Griddata[i].BaseNoOfDays;
					$scope.productNew.BaseOnDueDate = $scope.Griddata[i].BaseOnDueDate;
					$scope.productNew.MatureDate = $scope.Griddata[i].MatureDate;




					$scope.productNew.InvoicingPartyPlantId = $scope.Griddata[i].InvoicingPartyPlantId;
					$scope.productNew.InvoicingState = $scope.Griddata[i].InvoicingState;
					$scope.productNew.InvoicingGSTIN = $scope.Griddata[i].GSTIN;
					$scope.productNew.InvoicingByAddress = $scope.Griddata[i].InvoicingByAddress;

					$scope.productNew.DeliveryState = $scope.Griddata[i].DeliveryState;
					$scope.productNew.DeliveryState = $scope.Griddata[i].DeliveryState;
					$scope.productNew.DeliveryGSTIN = $scope.Griddata[i].GSTIN;
					$scope.productNew.DeliveryPartyPlantId = $scope.Griddata[i].DeliveryPartyPlantId;



				}
			}
			$scope.productNew.PartyId = PartyId;
			//$scope.productNew.PartyCode = PartyCode;
			$scope.productNew.partySearchByList = PartyId;
			$scope.productId = "";

			$scope.AcceptanceId = $scope.AcceptanceId;

			getPartyPlantList();
			//getPartyPlantEditList();
			GetInventoryMaterialListByPO(id1, $scope.AcceptanceId);
			getServiceChargeListPO(id1);
			$scope.productNew.PO = $scope.status;






			//#endregion
		}
		else {
			var gridObj = $("#AcceptanceList").data("ejGrid");
			//getting corresponding record 
			var data = gridObj.getSelectedRecords()[0];
			//#region load
			var id1 = "''";
			var PartyId = '';
			$scope.productNew.DocRefNo = '';
			$scope.productNew.DocDate = '';
			$scope.productNew.DocDate = '';
			$scope.productNew.CurrencyId = '';
			$scope.productNew.DocDate = '';
			$scope.productNew.PartyName = '';

			$scope.productNew.PartyName = data.PartyName;
			$scope.productNew.DocRefNo = data.DocRefNo;
			$scope.productNew.DocDate = data.DocDate;
			$scope.productNew.CurrencyId = data.CurrencyId;
			$scope.productNew.ToCurrencyRate = data.AcceptanceRate;

			$scope.productNew.AcceptanceDate = data.AcceptanceDate;
			$scope.productNew.VoucherId = data.VoucherId;
			$scope.productNew.InvoiceNo = data.InvoiceNo;
			$scope.productNew.InvoiceDate = data.InvoiceDate;
			$scope.productNew.DueDate = data.DueDate;
			$scope.productNew.PurchaseLCId = data.PurchaseLCId;
			$scope.productNew.ContractId = data.ContractId;
			$scope.productNew.AcceptancePaymentSource = data.AcceptancePaymentSource;
			$scope.productNew.PartyId = data.PartyId;
			$scope.productNew.partySearchByList = data.PartyId;
			$scope.productId = "";
			$scope.AcceptanceId = $scope.AcceptanceId;
			getPartyPlantList();
			//getPartyPlantEditList();
			GetInventoryMaterialListByPO(id1, $scope.AcceptanceId);
			getServiceChargeListPO(null, $scope.AcceptanceId);
			$scope.productNew.PO = $scope.status;
			if (data.IsNonCreditable === 'Yes') {
				$scope.productNew.IsNonCreditable = true;

			}
			else {
				$scope.productNew.IsNonCreditable = false;


			}
			// $scope.calculateAmount(data);
			//$scope.GriddataSelected = [];
			//for (var i = 0; i < $scope.Griddata.length; i++) {
			//    // if ($scope.Griddata[i].Active === true) {
			//    id1 += ",'" + $scope.Griddata[i].Id + "'";
			//    //AcceptanceId = $scope.Griddata[i].AcceptanceId;
			//    PartyId = $scope.Griddata[0].PartyId;
			//    $scope.productNew.PartyName = $scope.Griddata[0].PartyName;
			//    $scope.productNew.DocRefNo = $scope.Griddata[0].DocRefNo;
			//    $scope.productNew.DocDate = $scope.Griddata[0].DocDate;
			//    $scope.productNew.CurrencyId = $scope.Griddata[0].CurrencyId;
			//    $scope.productNew.ToCurrencyRate = $scope.Griddata[0].ToCurrencyRate;
			//    $scope.productNew.IsNonCreditable = $scope.Griddata[0].IsNonCreditable;
			//    $scope.productNew.AcceptanceDate = $scope.Griddata[0].AcceptanceDate;
			//    //}
			//}
			//#endregion
		}

	}

	//$scope.GriddataSelected = [];
	//$scope.SelectedPOList = function () {
	//    $scope.GriddataSelected = [];
	//    for (var i = 0; i < $scope.Griddata.length; i++) {

	//        if ($scope.Griddata[i].Active === true) {

	//        }

	//    }

	//}
	$scope.POPopUpGateEntry = function () {
		$scope.getalldataGateEntry();
		angular.element(document.querySelector('#POPopUpGateEntry')).modal('show');
	};
	$scope.POPopUpCloseGateEntry = function () {
		angular.element(document.querySelector('#POPopUpGateEntry')).modal('hide');
	};

	$scope.GriddataGateEntry = [];
	$scope.getalldataGateEntry = function () {
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
		$scope.productNew.EntryDate = x.data.EntryDate;

		$scope.POPopUpCloseGateEntry();
	}






	// Load tax with Material Data
	$scope.getReceiveTaxListPO = function (data, flag, index, Id) {
		//debugger;
		$scope.receiveTaxindex = index;
		$scope.taxAbleAmnt = data.TrnAmount;
		$scope.percentageColumn = flag;
		$scope.Currency = $("#currency option:selected").text();
		$scope.currentMaterialRow = index;
		$scope.currentInventoryReceiveDetailIdRow = Id;
		$scope.taxAbleAmnt = data.TrnAmount;
		$scope.percentageColumn = flag;
		$scope.currentMaterialRow = index;
		$scope.receiveTaxList = [];
		if (data.POMaterialTaxList.length > 0) {
			$scope.HSNCode = data.POMaterialTaxList[0].HSNCode;
			$scope.receiveTaxList = data.POMaterialTaxList;
		}
		$scope.total = 0;
		for (var j = 0; j < $scope.receiveTaxList.length; j++) {
			$scope.total = $scope.total + $scope.receiveTaxList[j].TaxAmount;
		}
		angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');

	};

	$scope.getReceiveTaxListPOValueSet = function (data, flag, index, Id) {
		//debugger;
		//$scope.TransactionRate = '';
		//$scope.ShortageQty = '';
		//$scope.RejectionQty ='';
		//$scope.PODetailsID = '';
		if ($scope.Action === 'Save') {
			$scope.ShortageRate = '';
			$scope.ShortageValue = '';
			$scope.RejectionRate = '';
			$scope.RejectionValue = '';
			$scope.RejectionClamRate = '';
			$scope.MaterialGroupMasterName = data.MaterialGroupMasterName;
			$scope.UserName = data.UserName;
			$scope.StandardName = data.StandardName;
			$scope.FirstCharacteristicsValue = data.FirstCharacteristicsValue;
			$scope.SecondCharacteristicsValue = data.SecondCharacteristicsValue;
			$scope.ThirdCharacteristicsValue = data.ThirdCharacteristicsValue;

			$scope.TransactionRate = data.TransactionRate;
			$scope.ShortageQty = data.ShortageQty;
			$scope.RejectionQty = data.RejectionQty;

			$scope.PODetailsID = data.PODetailsID;
			$scope.ShortageRate = data.ShortageRate;
			$scope.ShortageValue = data.ShortageValue;
			$scope.RejectionRate = data.RejectionRate;
			$scope.RejectionValue = data.RejectionValue;
			$scope.RejectionClamRate = data.RejectionClamRate;

			angular.element(document.querySelector('#ValueSet')).modal('show');
		}
		else {
			$scope.ShortageRate = '';
			$scope.ShortageValue = '';
			$scope.RejectionRate = '';
			$scope.RejectionValue = '';
			$scope.RejectionClamRate = '';
			$scope.MaterialGroupMasterName = data.MaterialGroupMasterName;
			$scope.UserName = data.UserName;
			$scope.StandardName = data.StandardName;
			$scope.FirstCharacteristicsValue = data.FirstCharacteristicsValue;
			$scope.SecondCharacteristicsValue = data.SecondCharacteristicsValue;
			$scope.ThirdCharacteristicsValue = data.ThirdCharacteristicsValue;

			$scope.TransactionRate = data.TransactionRate;
			$scope.ShortageQty = data.ShortageQty;
			$scope.RejectionQty = data.RejectionQty;

			$scope.PODetailsID = data.InventoryReceiveDetailId;
			$scope.ShortageRate = data.ShortageRate;
			$scope.ShortageValue = data.ShortageValue;
			$scope.RejectionRate = data.RejectionRate;
			$scope.RejectionValue = data.RejectionValue;
			$scope.RejectionClamRate = data.RejectionClamRate;

			angular.element(document.querySelector('#ValueSet')).modal('show');
		}


	};
	$scope.getReceiveTaxListPOValueSet1 = function (data, flag, index, Id) {
		//debugger;
		//angular.element(document.querySelector('#ValueSet')).modal('show');
		if ($scope.Action === 'Save') {
			//$scope.ShortageRate = '';
			//$scope.ShortageValue = '';
			//$scope.RejectionRate = '';
			//$scope.RejectionValue = '';
			//$scope.RejectionClamRate = '';
			//for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
			//    $scope.MaterialGroupMasterName = $scope.inventoryMaterialListPO[i].MaterialGroupMasterName;
			//    $scope.UserName = $scope.inventoryMaterialListPO[i].UserName;
			//    $scope.StandardName = $scope.inventoryMaterialListPO[i].StandardName;
			//    $scope.FirstCharacteristicsValue = $scope.inventoryMaterialListPO[i].FirstCharacteristicsValue;
			//    $scope.SecondCharacteristicsValue = $scope.inventoryMaterialListPO[i].SecondCharacteristicsValue;
			//    $scope.ThirdCharacteristicsValue = $scope.inventoryMaterialListPO[i].ThirdCharacteristicsValue;

			//    $scope.TransactionRate = $scope.inventoryMaterialListPO[i].TransactionRate;
			//    $scope.ShortageQty = $scope.inventoryMaterialListPO[i].ShortageQty;
			//    $scope.RejectionQty = $scope.inventoryMaterialListPO[i].RejectionQty;

			//    $scope.PODetailsID = $scope.inventoryMaterialListPO[i].PODetailsID;
			//    $scope.ShortageRate = $scope.inventoryMaterialListPO[i].ShortageRate;
			//    $scope.ShortageValue = $scope.inventoryMaterialListPO[i].ShortageValue;
			//    $scope.RejectionRate = $scope.inventoryMaterialListPO[i].RejectionRate;
			//    $scope.RejectionValue = $scope.inventoryMaterialListPO[i].RejectionValue;
			//    $scope.RejectionClamRate = $scope.inventoryMaterialListPO[i].RejectionClamRate;
			//}
			//GetInventoryMaterialListByPO($scope.POId);
			//$scope.ShortageRate = '110';

			//$scope.CalculateShortageValPop = function () {
			//    //debugger;
			//    for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
			//        $scope.inventoryMaterialListPO[i].ShortageRate = 110;
			//        $scope.inventoryMaterialListPO[i].ShortageValue = ($scope.inventoryMaterialListPO[i].TransactionRate * $scope.inventoryMaterialListPO[i].ShortageRate) / 100;
			//        $scope.inventoryMaterialListPO[i].RejectionRate = 50;
			//        $scope.inventoryMaterialListPO[i].RejectionValue = ($scope.inventoryMaterialListPO[i].TransactionRate * $scope.inventoryMaterialListPO[i].RejectionRate) / 100;
			//        $scope.inventoryMaterialListPO[i].RejectionClamRate = (100 - $scope.inventoryMaterialListPO[i].RejectionRate);
			//    }
			//}
			$scope.new = [];
			//$scope.new = $scope.inventoryMaterialListPO;
			for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
				if ($scope.inventoryMaterialListPO[i].check === true) {
					if ($scope.inventoryMaterialListPO[i].ShortageQty > 0 || $scope.inventoryMaterialListPO[i].RejectionQty > 0) {
						$scope.new.push($scope.inventoryMaterialListPO[i]);
					}
				}
			}

			//$scope.inventoryMaterialListPO = [];
			for (var i = 0; i < $scope.new.length; i++) {
				if ($scope.new[i].check == true) {
					if ($scope.new[i].ShortageQty > 0 || $scope.new[i].RejectionQty > 0) {
						$scope.new[i].ShortageRate = 110;
						$scope.new[i].ShortageValue = (($scope.new[i].ShortageQty * $scope.new[i].ShortageRate) / 100) * $scope.new[i].TransactionRate;
						$scope.new[i].RejectionRate = 50;
						$scope.new[i].RejectionValue = (($scope.new[i].RejectionQty * $scope.new[i].RejectionRate) / 100) * $scope.new[i].TransactionRate;
						$scope.new[i].RejectionClamRate = (100 - $scope.new[i].RejectionRate);

					}
				}
			}

			angular.element(document.querySelector('#ValueSet')).modal('show');
		}
		else {
			//$scope.ShortageRate = '';
			//$scope.ShortageValue = '';
			//$scope.RejectionRate = '';
			//$scope.RejectionValue = '';
			//$scope.RejectionClamRate = '';
			//$scope.MaterialGroupMasterName = data.MaterialGroupMasterName;
			//$scope.UserName = data.UserName;
			//$scope.StandardName = data.StandardName;
			//$scope.FirstCharacteristicsValue = data.FirstCharacteristicsValue;
			//$scope.SecondCharacteristicsValue = data.SecondCharacteristicsValue;
			//$scope.ThirdCharacteristicsValue = data.ThirdCharacteristicsValue;

			//$scope.TransactionRate = data.TransactionRate;
			//$scope.ShortageQty = data.ShortageQty;
			//$scope.RejectionQty = data.RejectionQty;

			//$scope.PODetailsID = data.InventoryReceiveDetailId;
			//$scope.ShortageRate = data.ShortageRate;
			//$scope.ShortageValue = data.ShortageValue;
			//$scope.RejectionRate = data.RejectionRate;
			//$scope.RejectionValue = data.RejectionValue;
			//$scope.RejectionClamRate = data.RejectionClamRate;

			//$scope.inventoryMaterialListPO = $scope.inventoryMaterialList;
			//$scope.new = [];
			//$scope.new = $scope.inventoryMaterialListPO;
			//$scope.inventoryMaterialList = [];
			//for (var i = 0; i < $scope.new.length; i++) {
			//    if ($scope.new[i].check == true) {
			//        if ($scope.new[i].ShortageQty > 0 || $scope.new[i].RejectionQty >0) {
			//            $scope.new[i].ShortageRate = 110;
			//            $scope.new[i].ShortageValue = ($scope.new[i].TransactionRate * $scope.new[i].ShortageRate) / 100;
			//            $scope.new[i].RejectionRate = 50;
			//            $scope.new[i].RejectionValue = ($scope.new[i].TransactionRate * $scope.new[i].RejectionRate) / 100;
			//            $scope.new[i].RejectionClamRate = (100 - $scope.new[i].RejectionRate);
			//            $scope.inventoryMaterialList.push($scope.new[i]);
			//        }

			//    }
			//}
			//angular.element(document.querySelector('#ValueSet')).modal('show');

			$scope.new1 = [];

			for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
				if ($scope.inventoryMaterialList[i].check === true) {
					if ($scope.inventoryMaterialList[i].ShortageQty > 0 || $scope.inventoryMaterialList[i].RejectionQty > 0) {
						$scope.new1.push($scope.inventoryMaterialList[i]);
					}
				}
			}

			for (var i = 0; i < $scope.new1.length; i++) {
				if ($scope.new1[i].check == true) {
					if ($scope.new1[i].ShortageQty > 0 || $scope.new1[i].RejectionQty > 0) {
						$scope.new1[i].ShortageRate = 110;
						$scope.new1[i].ShortageValue = (($scope.new1[i].ShortageQty * $scope.new1[i].ShortageRate) / 100) * $scope.new1[i].TransactionRate;
						$scope.new1[i].RejectionRate = 50;
						$scope.new1[i].RejectionValue = (($scope.new1[i].RejectionQty * $scope.new1[i].RejectionRate) / 100) * $scope.new1[i].TransactionRate;
						$scope.new1[i].RejectionClamRate = (100 - $scope.new1[i].RejectionRate);
					}
				}
			}

			angular.element(document.querySelector('#ValueSet')).modal('show');
		}


	};
	$scope.CalculateShortageVal = function (x) {
		//debugger;
		for (var i = 0; i < $scope.newList.length; i++) {
			$scope.newList[i].ShortageValue = (($scope.newList[i].ShortageQty * $scope.newList[i].ShortageRate) / 100) * $scope.newList[i].TransactionRate;
		}


	}
	$scope.CalculateRejectionVal = function () {
		for (var i = 0; i < $scope.newList.length; i++) {
			$scope.newList[i].RejectionValue = (($scope.newList[i].RejectionQty * $scope.newList[i].RejectionRate) / 100) * $scope.newList[i].TransactionRate;
			$scope.newList[i].RejectionClamRate = (100 - $scope.newList[i].RejectionRate);

		}
	}
	function GetInventoryMaterialListByPO(inveReveiveId) {
		//debugger;
		$scope.masterId = inveReveiveId;
		$http.get($scope.path + 'GetInventoryMaterialListByOnlyPO?inveReveiveId=' + inveReveiveId + '&AcceptanceId=' + $scope.AcceptanceId)
			.then(function (response) {
				$scope.inventoryMaterialListPO = [];
				$scope.inventoryMaterialListPO = response.data.Rows;
				$scope.POID = $scope.inventoryMaterialListPO.POID;
				$scope.PreBal = $scope.inventoryMaterialListPO.Balance;
				$scope.PODetailsID = $scope.inventoryMaterialListPO.InventoryReceiveDetailId;
				$scope.productNew.InvoicingByAddress = $scope.inventoryMaterialListPO[0].InvoicingByAddress;
				$scope.productNew.DeliveryByAddress = $scope.inventoryMaterialListPO[0].DeliveryByAddress;
				$scope.inventoryMaterialListPO.BaseAmount = '0';
				//$scope.POId1 = '';
				checkSameValueInColumnList($scope.inventoryMaterialListPO, 'TransactionUoM');
				getGrossAmount($scope.inventoryMaterialListPO, 'BaseAmount', 'BaseTaxAmount', 'ChargesAmount', 'grossTotal');
				$scope.GetPOMaterialTaxData();
				//$scope.POPopUpClose();
			});
	}
	$scope.GetPOMaterialTaxData = function () {
		//debugger;
		$scope.POMaterialTaxList = [];
		$http({
			method: "GET",
			url: $scope.path + 'GetReceiveTaxListPO?receiveDetailId=' + $scope.masterId
		}).then(function (response) {
			$scope.POMaterialTaxList = response.data;

			for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
				var linepk = $scope.inventoryMaterialListPO[i].InventoryReceiveDetailId;
				var list = getPOMaterialtaxlist(linepk);
				$scope.inventoryMaterialListPO[i].POMaterialTaxList = list;
			}
		});
	};
	function getPOMaterialtaxlist(linepk) {
		//debugger;
		var result = [];
		for (var i = 0; i < $scope.POMaterialTaxList.length; i++) {
			if ($scope.POMaterialTaxList[i].PODetailId === linepk) {
				result.push($scope.POMaterialTaxList[i]);
			}
		}
		return result;
	}

	$scope.GetMaterialTaxData = function () {
		//debugger;
		$scope.MaterialTaxList = [];
		$http({
			method: "GET",
			url: $scope.path + 'GetReceiveTaxList?receiveDetailId=' + $scope.masterId5
		}).then(function (response) {
			$scope.MaterialTaxList = response.data;

			for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
				var linepk = $scope.inventoryMaterialList[i].InventoryReceiveDetailId;
				var list = getMaterialtaxlist(linepk);
				$scope.inventoryMaterialList[i].MaterialTaxList = list;
			}
		});
	};
	function getMaterialtaxlist(linepk) {
		//debugger;
		var result4 = [];
		for (var i = 0; i < $scope.MaterialTaxList.length; i++) {
			if ($scope.MaterialTaxList[i].PODetailId === linepk) {
				result4.push($scope.MaterialTaxList[i]);
			}
		}
		return result4;
	}

	//Load Service Tax with service charge
	function getServiceChargeListPO(inveReveiveId) {
		$scope.inveReveiveId = inveReveiveId;
		$http.get($scope.path + 'GetServiceChargeListPO?receiveId=' + inveReveiveId + '&AcceptanceId=' + $scope.AcceptanceId)
			.then(function (response) {
				$scope.chargesListPO = [];
				$scope.chargesListPO = response.data;
				$scope.GetPOServiceTaxData();
			});
	}
	$scope.GetPOServiceTaxData = function () {
		//debugger;
		$scope.POServiceTaxList = [];
		$http({
			method: "GET",
			url: $scope.path + 'GetServiceTaxListPO?serviceId=' + $scope.inveReveiveId
		}).then(function (response) {
			$scope.POServiceTaxList = response.data;

			for (var i = 0; i < $scope.chargesListPO.length; i++) {
				var linepk = $scope.chargesListPO[i].Id;
				var list1 = getPOServicetaxlist(linepk);
				$scope.chargesListPO[i].POServiceTaxList = list1;
			}
		});
	};
	function getPOServicetaxlist(linepk1) {
		//debugger;
		var result1 = [];
		for (var i = 0; i < $scope.POServiceTaxList.length; i++) {
			if ($scope.POServiceTaxList[i].InventoryServiceId === linepk1) {
				result1.push($scope.POServiceTaxList[i]);
			}
		}
		return result1;
	}
	function getServicetaxlist1(linepk111) {
		//debugger;
		var result11 = [];
		for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
			if ($scope.ServiceTaxList[i].InventoryServiceId === linepk111) {
				result11.push($scope.ServiceTaxList[i]);
			}
		}
		return result11;
	}
	//function getServicetaxlist1(linepk111) {
	//	//debugger;
	//	var result11 = [];
	//	for (var i = 0; i < $scope.chargesList.length; i++) {
	//		if ($scope.chargesList[i].InventoryServiceId === linepk111) {
	//			result11.push($scope.chargesList[i]);
	//		}
	//	}
	//	return result11;
	//}
	$scope.getServiceTaxList = function () { //,data, flag)
		//$scope.taxAbleAmnt = data.Amount + data.TotalTaxAmount;
		//$scope.percentageColumn = flag;

		$http({
			method: 'GET',
			url: $scope.path + 'GetServiceTaxList?serviceId=' + $scope.masterId12//data.Id
		}).then(function (response) {
			$scope.ServiceTaxList = response.data;
			for (var i = 0; i < $scope.chargesList.length; i++) {
				var linepk1 = $scope.chargesList[i].Id;
				var list11 = getServicetaxlist1(linepk1);
				$scope.chargesList[i].ServiceTaxList = list11;
			}
			// angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
		});
	}






	$scope.getServiceTaxListPOPOP1 = function (data, flag, Id, index) {
		//debugger;
		$scope.productNew.TaxOptionService = 'Yes';
		$scope.ServiceAddindex = index;
		$scope.taxAbleAmnt = data.Amount;
		$scope.percentageColumn = flag;
		$scope.Currency = $("#currency option:selected").text();
		$scope.currentMaterialRow = index;
		$scope.currentInventoryReceiveDetailIdRow = Id;
		//$scope.taxAbleAmnt = data.TrnAmount;
		$scope.percentageColumn = flag;
		$scope.currentMaterialRow = index;
		$scope.ServiceTaxList = [];
		if (data.ServiceTaxList.length > 0) {
			$scope.HSNCode = data.ServiceTaxList[0].HSNCode;
			$scope.ServiceTaxList = data.ServiceTaxList;
		}
		$scope.total = 0;
		for (var j = 0; j < $scope.ServiceTaxList.length; j++) {
			$scope.total = $scope.total + $scope.ServiceTaxList[j].TaxAmount;
		}
		angular.element(document.querySelector('#ServiceTaxPopUp')).modal('show');
		//$http({
		//    method: 'GET',
		//    url: $scope.path + 'GetServiceTaxListPO?serviceId=' + data.Id
		//}).then(function (response) {
		//    $scope.receiveTaxList = response.data;
		//    angular.element(document.querySelector('#ServiceTaxPopUp')).modal('show');
		//});

	}



	//$scope.enable = true;
	//$scope.selectAll = function () {
	//    // Loop through all the entities and set their isChecked property
	//    for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
	//        $scope.model.entities[i].isChecked = $scope.model.allItemsSelected;
	//    }
	//};



	$scope.index = -1;
	$scope.staus = true;
	$scope.enableid = true;
	$scope.Tabshow = false;
	$scope.TabChabge = function () {
		var getRow = $filter("filter")($scope.inventoryMaterialListPO, { "check": true });
		if (getRow.length > 0) {
			$scope.Tabshow = true;
		}
		else {
			$scope.Tabshow = false;
		}
	}
	$scope.Tabshow1 = false;
	$scope.TabChabge1 = function () {
		var getRow = $filter("filter")($scope.inventoryMaterialList, { "check": true });
		if (getRow.length > 0) {
			$scope.Tabshow1 = true;
		}
		else {
			$scope.Tabshow1 = false;
		}
	}
	$scope.Change = function (event, index, x) {
		//debugger;
		if (baseService.isUndefinedOrNull(x.TransactionQty)) {
			ShowResult('Enter the current qty', 'failure');
		}
		else {
			if (event.currentTarget.checked) {
				$scope.inventoryMaterialListPO[index].check = true;
				$scope.index = index;
				//$scope.staus = false;
				x.enableid = false;

				if (x.POQty === (x.GRNRcvQty + x.TransactionQty)) {
					x.POClosStatus = true;
				}
				else if (x.POQty < (x.GRNRcvQty + x.TransactionQty) && x.Tolerance > 0) {// Condition is added for if receive more qty
					x.POClosStatus = true;
				}

				else if (x.POQty > (x.GRNRcvQty + x.TransactionQty)) {
					$scope.PODetailId = x.PODetailId;
					$scope.message = 'Are you want to close this PO line item?';
					angular.element(document.querySelector('#ConfirmationForReqClosePopUp')).modal('show');
				}

			}
			else {
				$scope.inventoryMaterialListPO[index].check = false;
				x.enableid = true;
				//$scope.index = index;
				x.POClosStatus = false;
				x.TransactionQty = "";
				x.Balance = x.POQty - x.GRNRcvQty;//parseFloat(x.POQty - x.GRNRcvQty).toFixed(2);
			}


		}
		$scope.TabChabge();

		//if (event.currentTarget.checked) {
		//    $scope.index = index;
		//    //$scope.staus = false;
		//    x.enableid = false;
		//}
		//else {
		//    x.enableid = true;
		//    //$scope.index = index;
		//}
	}
	$scope.YesMessageForClosed = function ($event) {
		//debugger

		for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
			if ($scope.inventoryMaterialListPO[i].check === true) {
				if ($scope.inventoryMaterialListPO[i].PODetailId === $scope.PODetailId) {
					$scope.inventoryMaterialListPO[i].POClosStatus = true;
				}
			}
			else {
				$scope.inventoryMaterialListPO[i].POClosStatus = false;
			}
		}
	}
	$scope.NoMessageForClosed = function ($event) {
		//debugger

		for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
			if ($scope.GetListForMasterOrder[i].check === true) {
				if ($scope.GetListForMasterOrder[i].PODetailId === $scope.PODetailId) {
					$scope.inventoryMaterialListPO[i].POClosStatus = false;
				}
			}
			else {
				$scope.GetListForMasterOrder[i].WantToClose = false;
			}
		}
	}

	$scope.calculateRate = function (data, event) {
		//debugger;
		data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
		if (data.TransactionRate === 'NaN')
			data.TransactionRate = 0;
		data.BaseTaxAmount = 0;
		angular.forEach(data.POMaterialTaxList, function (item) {
			item.TaxAmount = data.TrnAmount * item.Percentage / 100;

			data.BaseTaxAmount += item.TaxAmount;
		});
		data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
		//if ($scope.productNew.IsNonCreditable==1) {
		//    //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
		//    data.BaseAmount = data.TrnAmount+ data.BaseTaxAmount;
		//}
		//else {
		//    data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
		//}

	};
	$scope.calculateAmount = function (data, index) {
		if (baseService.isUndefinedOrNull(data.PurchaseDocumentAcceptanceId)) {
			debugger;
			$scope.PreBal = data.Balance;
			// data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
			data.TrnAmount = (data.NetQty * data.TransactionRate).toFixed(2);//(data.TransactionQty * data.TransactionRate).toFixed(2);
			if (data.TrnAmount == 'NaN')
				data.TrnAmount = 0;
			data.TaxAmount = 0;
			data.BaseTaxAmount = 0;
			var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'POAmount');
			var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');
			var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.POServiceTaxList), 'TaxAmount');

			//angular.forEach(data.POMaterialTaxList, function (item) {
			//	item.TaxAmount = data.TrnAmount * item.Percentage / 100;
			//	data.BaseTaxAmount += item.TaxAmount;

			//});


			for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
				$scope.inventoryMaterialListPO[i].Balance = '';
				var ToleranceQty = $scope.inventoryMaterialListPO[i].POQty * $scope.inventoryMaterialListPO[i].Tolerance / 100;
				var newpoQty = $scope.inventoryMaterialListPO[i].POQty + ToleranceQty;
				if ($scope.inventoryMaterialListPO[i].POQty < (parseFloat($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2)) && (baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].Tolerance) || $scope.inventoryMaterialListPO[i].Tolerance === 0)) {
					//$scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty);
					$scope.inventoryMaterialListPO[i].TransactionQty = '';
					ShowResult('Current quantity can not grater than balance qty!', 'failure');
					return false;
				}
				//else if ($scope.inventoryMaterialListPO[i].POQty < (parseFloat($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2)) && $scope.inventoryMaterialListPO[i].Tolerance > 0) {
				//	//$scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - $scope.inventoryMaterialListPO[i].GRNRcvQty;
				//	var ToleranceQty = $scope.inventoryMaterialListPO[i].POQty * $scope.inventoryMaterialListPO[i].Tolerance / 100;
				//	var newpoQty = $scope.inventoryMaterialListPO[i].POQty + ToleranceQty;
				//	return true;

				//}
				else if (newpoQty < (parseFloat($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2)) && (!baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].Tolerance) || $scope.inventoryMaterialListPO[i].Tolerance > 0)) {
					ShowResult('Current quantity can not grater than po qty and Tolerance qty!PO + Tolerance=' + newpoQty, 'failure');
					return false;
				}
				else if ($scope.inventoryMaterialListPO[i].ShortageQty > $scope.inventoryMaterialListPO[i].TransactionQty) {
					//$scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - $scope.inventoryMaterialListPO[i].GRNRcvQty;
					ShowResult('Shortage Qty quantity can not grater than current qty!', 'failure');
					return false;
				}
				else if ($scope.inventoryMaterialListPO[i].RejectionQty > $scope.inventoryMaterialListPO[i].TransactionQty) {
					//$scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - $scope.inventoryMaterialListPO[i].GRNRcvQty;
					ShowResult('Rejection Qty quantity can not grater than current qty!', 'failure');
					return false;
				}
				else {
					if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
						$scope.inventoryMaterialListPO[i].TrnAmount = Math.round(data.TrnAmount * 100 + Number.EPSILON) / 100;
						//$scope.inventoryMaterialListPO[i].BaseTaxAmount = (($scope.inventoryMaterialListPO[i].TotalTaxAmount / $scope.inventoryMaterialListPO[i].POQty) * $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2);
						//$scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
						//$scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
						$scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
						//$scope.inventoryMaterialListPO[i].ShortageQty = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
						$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
						//$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].RejectionQty);
						$scope.inventoryMaterialListPO[i].NetQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].ShortageQty);

					}
					else {
						//$scope.inventoryMaterialListPO[i].BaseTaxAmount = (($scope.inventoryMaterialListPO[i].TotalTaxAmount / $scope.inventoryMaterialListPO[i].POQty) * $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2);
						//$scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
						//$scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
						$scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
						//$scope.inventoryMaterialListPO[i].ShortageQty = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty+$scope.inventoryMaterialListPO[i].TransactionQty));
						$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
						//$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].RejectionQty);
						$scope.inventoryMaterialListPO[i].NetQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].ShortageQty);
					}
					if ($scope.productNew.IsNonCreditable == 1) {
						if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
							//data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);               
							//$scope.inventoryMaterialListPO[i].BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax);
							$scope.inventoryMaterialListPO[i].TrnAmount = ($scope.inventoryMaterialListPO[i].NetQty * $scope.inventoryMaterialListPO[i].TransactionRate).toFixed(2);
							$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)) * 100 + Number.EPSILON) / 100;
							$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;

						}
					}
					else {
						if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
							//data.BaseAmount = parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge);
							$scope.inventoryMaterialListPO[i].TrnAmount = Math.round(($scope.inventoryMaterialListPO[i].NetQty * $scope.inventoryMaterialListPO[i].TransactionRate) * 100 + Number.EPSILON) / 100;
							$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.ServiceCharge)) * 100 + Number.EPSILON) / 100;
							$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.ServiceCharge)) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;
						}
					}
				}
			}
			angular.forEach(data.POMaterialTaxList, function (item) {
				item.TaxAmount = Math.round(((data.TrnAmount * item.Percentage) / 100) * 100 + Number.EPSILON) / 100;
			});

			for (var i1 = 0; i1 < $scope.inventoryMaterialListPO.length; i1++) {
				if ($scope.inventoryMaterialListPO[i1].PODetailsID == data.PODetailsID) {
					$scope.inventoryMaterialListPO[i1].BaseTaxAmount = Math.round($filter('sumByKey')($filter('filter')(data.POMaterialTaxList, { "PODetailId": data.PODetailsID }), 'TaxAmount') * 100 + Number.EPSILON) / 100;


				}

			}


			//angular.forEach($scope.inventoryMaterialListPO, function (item) {
			//    item.ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * data.TrnAmount;

			//});

			//$scope.detailModel.BaseUOMId = $filter("filter")($scope.chargesListPO, { IsBaseUom: 1 })[0].Value;

			// data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
			//data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
		}
		else {
			debugger;
			$scope.PreBal = data.Balance;
			// data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
			data.TrnAmount = (data.NetQty * data.TransactionRate).toFixed(2);//(data.TransactionQty * data.TransactionRate).toFixed(2);
			if (data.TrnAmount == 'NaN')
				data.TrnAmount = 0;
			data.TaxAmount = 0;
			data.BaseTaxAmount = 0;
			var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'POAmount');
			var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');
			var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.POServiceTaxList), 'TaxAmount');

			//angular.forEach(data.POMaterialTaxList, function (item) {
			//	item.TaxAmount = data.TrnAmount * item.Percentage / 100;
			//	data.BaseTaxAmount += item.TaxAmount;

			//});


			for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
				$scope.inventoryMaterialListPO[i].Balance = '';
				var ToleranceQty = $scope.inventoryMaterialListPO[i].POQty * $scope.inventoryMaterialListPO[i].Tolerance / 100;
				var newpoQty = $scope.inventoryMaterialListPO[i].POQty + ToleranceQty;
				//if ($scope.inventoryMaterialListPO[i].POQty < (parseFloat($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2)) && (baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].Tolerance) || $scope.inventoryMaterialListPO[i].Tolerance === 0)) {
				//	//$scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty);
				//	$scope.inventoryMaterialListPO[i].TransactionQty = '';
				//	ShowResult('Current quantity can not grater than balance qty!', 'failure');
				//	return false;
				//}
				////else if ($scope.inventoryMaterialListPO[i].POQty < (parseFloat($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2)) && $scope.inventoryMaterialListPO[i].Tolerance > 0) {
				////	//$scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - $scope.inventoryMaterialListPO[i].GRNRcvQty;
				////	var ToleranceQty = $scope.inventoryMaterialListPO[i].POQty * $scope.inventoryMaterialListPO[i].Tolerance / 100;
				////	var newpoQty = $scope.inventoryMaterialListPO[i].POQty + ToleranceQty;
				////	return true;

				////}
				//else if (newpoQty < (parseFloat($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2)) && (!baseService.isUndefinedOrNull($scope.inventoryMaterialListPO[i].Tolerance) || $scope.inventoryMaterialListPO[i].Tolerance > 0)) {
				//	ShowResult('Current quantity can not grater than po qty and Tolerance qty!PO + Tolerance=' + newpoQty, 'failure');
				//	return false;
				//}
			   if ($scope.inventoryMaterialListPO[i].ShortageQty > $scope.inventoryMaterialListPO[i].TransactionQty) {
					//$scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - $scope.inventoryMaterialListPO[i].GRNRcvQty;
					ShowResult('Shortage Qty quantity can not grater than current qty!', 'failure');
					return false;
				}
				else if ($scope.inventoryMaterialListPO[i].RejectionQty > $scope.inventoryMaterialListPO[i].TransactionQty) {
					//$scope.inventoryMaterialListPO[i].Balance = $scope.inventoryMaterialListPO[i].POQty - $scope.inventoryMaterialListPO[i].GRNRcvQty;
					ShowResult('Rejection Qty quantity can not grater than current qty!', 'failure');
					return false;
				}
				else {
					if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
						$scope.inventoryMaterialListPO[i].TrnAmount = Math.round(data.TrnAmount * 100 + Number.EPSILON) / 100;
						//$scope.inventoryMaterialListPO[i].BaseTaxAmount = (($scope.inventoryMaterialListPO[i].TotalTaxAmount / $scope.inventoryMaterialListPO[i].POQty) * $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2);
						//$scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
						//$scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
						$scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
						//$scope.inventoryMaterialListPO[i].ShortageQty = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
						$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
						//$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].RejectionQty);
						$scope.inventoryMaterialListPO[i].NetQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].ShortageQty);

					}
					else {
						//$scope.inventoryMaterialListPO[i].BaseTaxAmount = (($scope.inventoryMaterialListPO[i].TotalTaxAmount / $scope.inventoryMaterialListPO[i].POQty) * $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2);
						//$scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
						//$scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
						$scope.inventoryMaterialListPO[i].Balance = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty));
						//$scope.inventoryMaterialListPO[i].ShortageQty = ($scope.inventoryMaterialListPO[i].POQty - ($scope.inventoryMaterialListPO[i].GRNRcvQty+$scope.inventoryMaterialListPO[i].TransactionQty));
						$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
						//$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].RejectionQty);
						$scope.inventoryMaterialListPO[i].NetQty = ($scope.inventoryMaterialListPO[i].TransactionQty - $scope.inventoryMaterialListPO[i].ShortageQty);
					}
					if ($scope.productNew.IsNonCreditable == 1) {
						if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
							//data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);               
							//$scope.inventoryMaterialListPO[i].BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax);
							$scope.inventoryMaterialListPO[i].TrnAmount = ($scope.inventoryMaterialListPO[i].NetQty * $scope.inventoryMaterialListPO[i].TransactionRate).toFixed(2);
							$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)) * 100 + Number.EPSILON) / 100;
							$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;

						}
					}
					else {
						if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
							//data.BaseAmount = parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge);
							$scope.inventoryMaterialListPO[i].TrnAmount = Math.round(($scope.inventoryMaterialListPO[i].NetQty * $scope.inventoryMaterialListPO[i].TransactionRate) * 100 + Number.EPSILON) / 100;
							$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = Math.round((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.ServiceCharge)) * 100 + Number.EPSILON) / 100;
							$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = Math.round(((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.ServiceCharge)) * $scope.productNew.ToCurrencyRate) * 100 + Number.EPSILON) / 100;
						}
					}
				}
			}
			angular.forEach(data.POMaterialTaxList, function (item) {
				item.TaxAmount = Math.round(((data.TrnAmount * item.Percentage) / 100) * 100 + Number.EPSILON) / 100;
			});

			for (var i1 = 0; i1 < $scope.inventoryMaterialListPO.length; i1++) {
				if ($scope.inventoryMaterialListPO[i1].PODetailsID == data.PODetailsID) {
					$scope.inventoryMaterialListPO[i1].BaseTaxAmount = Math.round($filter('sumByKey')($filter('filter')(data.POMaterialTaxList, { "PODetailId": data.PODetailsID }), 'TaxAmount') * 100 + Number.EPSILON) / 100;


				}

			}


			//angular.forEach($scope.inventoryMaterialListPO, function (item) {
			//    item.ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * data.TrnAmount;

			//});

			//$scope.detailModel.BaseUOMId = $filter("filter")($scope.chargesListPO, { IsBaseUom: 1 })[0].Value;

			// data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
			//data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
		}

	};

	$scope.calculateAmount1 = function (data) {
		//debugger;

		// data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
		data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
		var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesList), 'Amount');
		var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'TrnAmount');
		var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.ServiceTaxList), 'TaxAmount');
		for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
			if ($scope.inventoryMaterialList[i].InventoryReceiveDetailId == data.InventoryReceiveDetailId) {
				$scope.inventoryMaterialList[i].TrnAmount = data.TrnAmount;
				$scope.inventoryMaterialList[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialList[i].TrnAmount;
				$scope.inventoryMaterialList[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialList[i].TrnAmount;
				$scope.inventoryMaterialList[i].Balance = ($scope.inventoryMaterialList[i].POQty - ($scope.inventoryMaterialList[i].OtherReceived + $scope.inventoryMaterialList[i].TransactionQty));
				//$scope.inventoryMaterialList[i].ShortageQty = ($scope.inventoryMaterialList[i].POQty - ($scope.inventoryMaterialList[i].OtherReceived + $scope.inventoryMaterialList[i].TransactionQty));
				//$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
				//$scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - $scope.inventoryMaterialList[i].RejectionQty);
				$scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - ($scope.inventoryMaterialList[i].ShortageQty + $scope.inventoryMaterialList[i].RejectionQty));
				$scope.inventoryMaterialList[i].NetQty = ($scope.inventoryMaterialList[i].TransactionQty - ($scope.inventoryMaterialList[i].ShortageQty));
				data.TrnAmount = (data.NetQty * data.TransactionRate).toFixed(2);//(data.TransactionQty * data.TransactionRate).toFixed(2);
				if (data.TrnAmount == 'NaN')
					data.TrnAmount = 0;
				data.TaxAmount = 0;
				data.BaseTaxAmount = 0;
				angular.forEach(data.MaterialTaxList, function (item) {
					item.TaxAmount = data.TrnAmount * item.Percentage / 100;
					data.BaseTaxAmount += item.TaxAmount;
				});
			}
			else {
				$scope.inventoryMaterialList[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialList[i].TrnAmount;
				$scope.inventoryMaterialList[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialList[i].TrnAmount;
				$scope.inventoryMaterialList[i].Balance = ($scope.inventoryMaterialList[i].POQty - ($scope.inventoryMaterialList[i].OtherReceived + $scope.inventoryMaterialList[i].TransactionQty));
				//$scope.inventoryMaterialList[i].ShortageQty = ($scope.inventoryMaterialList[i].POQty - ($scope.inventoryMaterialList[i].OtherReceived + $scope.inventoryMaterialList[i].TransactionQty));
				//$scope.inventoryMaterialListPO[i].ApprovedQty = ($scope.inventoryMaterialListPO[i].TransactionQty - ($scope.inventoryMaterialListPO[i].ShortageQty + $scope.inventoryMaterialListPO[i].RejectionQty));
				//$scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - $scope.inventoryMaterialList[i].RejectionQty);
				$scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - ($scope.inventoryMaterialList[i].ShortageQty + $scope.inventoryMaterialList[i].RejectionQty));
				$scope.inventoryMaterialList[i].NetQty = ($scope.inventoryMaterialList[i].TransactionQty - ($scope.inventoryMaterialList[i].ShortageQty));
				data.TrnAmount = (data.NetQty * data.TransactionRate).toFixed(2);//(data.TransactionQty * data.TransactionRate).toFixed(2);
				if (data.TrnAmount == 'NaN')
					data.TrnAmount = 0;
				data.TaxAmount = 0;
				data.BaseTaxAmount = 0;
				angular.forEach(data.MaterialTaxList, function (item) {
					item.TaxAmount = data.TrnAmount * item.Percentage / 100;
					data.BaseTaxAmount += item.TaxAmount;
				});
			}
			if ($scope.productNew.IsNonCreditable == 1) {
				//data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);               
				//$scope.inventoryMaterialListPO[i].BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax);
				$scope.inventoryMaterialList[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat(data.ServiceTax);
				$scope.inventoryMaterialList[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.productNew.ToCurrencyRate);

			}
			else {
				//data.BaseAmount = parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge);
				data.TotalMaterialTranAmount = parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge);
				data.TotalMaterialBaseAmount = ((parseFloat(data.TrnAmount) + parseFloat(data.ServiceCharge)) * $scope.productNew.ToCurrencyRate);
			}
		}

	};
	// #endregion







	$scope.enableid1 = true;
	$scope.enableid3 = true;
	$scope.Change1 = function (event, index, x) {

		if (event.currentTarget.checked) {
			$scope.index = index;
			//$scope.staus = false;
			x.enableid1 = false;
			x.check == true;
		}


		else {
			x.enableid1 = true;
			x.check == false;
			//$scope.index = index;
		}
	}
	$scope.enableid2 = true;
	$scope.Change2 = function (event, index, x) {

		if (event.currentTarget.checked) {
			$scope.index = index;
			//$scope.staus = false;
			$scope.enableid2 = false;
			x.check == true;
		}


		else {
			$scope.enableid2 = true;
			//$scope.index = index;
			x.check == false;
		}
	}


	$scope.calculateAmountForServiceCharge = function (data) {
		//debugger;
		//data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
		//if (data.TrnAmount == 'NaN')
		//    data.TrnAmount = 0;
		//data.TaxAmount = 0;
		data.TotalTaxAmount = 0;
		for (var i = 0; i < $scope.chargesListPO.length; i++) {
			if ($scope.chargesListPO[i].Amount > parseFloat($scope.chargesListPO[i].POAmount) + parseFloat($scope.chargesListPO[i].GRNServiceAmount)) {

				ShowResult('Amount can not grater than PO Service Amount');
				$scope.chargesListPO[i].Amount = 0;
				return false;
			}
		}
		for (var i = 0; i < $scope.POServiceTaxList.length; i++) {

			if ($scope.POServiceTaxList[i].InventoryServiceId == data.Id) {
				$scope.POServiceTaxList[i].TaxAmount = data.Amount * $scope.POServiceTaxList[i].Percentage / 100;
				data.TotalTaxAmount += $scope.POServiceTaxList[i].TaxAmount;
			}
		}
		var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.POServiceTaxList), 'TaxAmount');

		// data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
		//data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;


		//for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
		//    if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
		//        $scope.inventoryMaterialListPO[i].Amount = data.Amount;
		//        $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / data.Amount) * $scope.inventoryMaterialListPO[i].Amount;
		//        $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / data.Amount) * $scope.inventoryMaterialListPO[i].Amount;
		//    }
		//    else {
		//        $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / data.Amount) * $scope.inventoryMaterialListPO[i].TrnAmount;
		//        $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / data.Amount) * $scope.inventoryMaterialListPO[i].TrnAmount;
		//    }
		//    if ($scope.productNew.IsNonCreditable == 1) {
		//        //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
		//        $scope.inventoryMaterialListPO[i].BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + $scope.inventoryMaterialListPO[i].ServiceCharge + data.ServiceTax;

		//    }
		//    else {
		//        data.BaseAmount = parseFloat(data.TrnAmount) + data.ServiceCharge;
		//    }

		//}


		for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {

			if ($scope.inventoryMaterialListPO[i].POID == data.InventoryReceiveId) {
				var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO, { 'InventoryReceiveId': data.InventoryReceiveId }), 'Amount');
				var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO, { 'POID': $scope.inventoryMaterialListPO[i].POID }), 'TrnAmount');
				//$scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;
				$scope.inventoryMaterialListPO[i].ServiceCharge = ((TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2);
				$scope.inventoryMaterialListPO[i].ServiceTax = ((TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2);
			}
			//else {
			//    $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
			//    $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
			//}
			if ($scope.productNew.IsNonCreditable == 1) {
				//data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);

				// $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = $scope.inventoryMaterialListPO[i].TrnAmount + $scope.inventoryMaterialListPO[i].BaseTaxAmount;
				//$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax).toFixed(2)) * $scope.productNew.ToCurrencyRate).toFixed(2);

				$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax)).toFixed(2);
				$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);


			}
			else {
				$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)).toFixed(2);
				$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);
				//$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2)) * $scope.productNew.ToCurrencyRate);
			}

		}

	};


	$scope.calculateAmountForServiceCharge1 = function (data) {
		//debugger;
		////data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
		////if (data.TrnAmount == 'NaN')
		////    data.TrnAmount = 0;
		////data.TaxAmount = 0;
		//data.TotalTaxAmount = 0;
		//for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
		//    if ($scope.ServiceTaxList[i].InventoryServiceId == data.Id) {
		//        $scope.ServiceTaxList[i].TaxAmount = data.Amount * $scope.ServiceTaxList[i].Percentage / 100;
		//        data.TotalTaxAmount += $scope.ServiceTaxList[i].TaxAmount;
		//    }
		//}
		//// data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
		////data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;
		//data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
		//if (data.TrnAmount == 'NaN')
		//    data.TrnAmount = 0;
		//data.TaxAmount = 0;
		data.TotalTaxAmount = 0;
		var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesList), 'Amount');
		var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'TrnAmount');

		for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
			if ($scope.ServiceTaxList[i].InventoryServiceId == data.Id) {
				$scope.ServiceTaxList[i].TaxAmount = data.Amount * $scope.ServiceTaxList[i].Percentage / 100;
				data.TotalTaxAmount += $scope.ServiceTaxList[i].TaxAmount;
			}
		}
		var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.ServiceTaxList), 'TaxAmount');
		// data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
		//data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;


		//for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
		//    if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
		//        $scope.inventoryMaterialListPO[i].Amount = data.Amount;
		//        $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / data.Amount) * $scope.inventoryMaterialListPO[i].Amount;
		//        $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / data.Amount) * $scope.inventoryMaterialListPO[i].Amount;
		//    }
		//    else {
		//        $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / data.Amount) * $scope.inventoryMaterialListPO[i].TrnAmount;
		//        $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / data.Amount) * $scope.inventoryMaterialListPO[i].TrnAmount;
		//    }
		//    if ($scope.productNew.IsNonCreditable == 1) {
		//        //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
		//        $scope.inventoryMaterialListPO[i].BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + $scope.inventoryMaterialListPO[i].ServiceCharge + data.ServiceTax;

		//    }
		//    else {
		//        data.BaseAmount = parseFloat(data.TrnAmount) + data.ServiceCharge;
		//    }

		//}


		for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
			//if ($scope.inventoryMaterialListPO[i].PODetailsID == data.Id) {
			//$scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;
			$scope.inventoryMaterialList[i].ServiceCharge = (parseFloat(TotalServiceAmount).toFixed(2) / parseFloat(TotalTrnAmount).toFixed(2)) * parseFloat($scope.inventoryMaterialList[i].TrnAmount).toFixed(2);
			$scope.inventoryMaterialList[i].ServiceTax = (parseFloat(TotalServiceTaxAmount).toFixed(2) / parseFloat(TotalTrnAmount).toFixed(2)) * parseFloat($scope.inventoryMaterialList[i].TrnAmount).toFixed(2);
			//}
			//else {
			//    $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
			//    $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
			//}
			if ($scope.productNew.IsNonCreditable == 1) {
				//data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);

				// $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = $scope.inventoryMaterialListPO[i].TrnAmount + $scope.inventoryMaterialListPO[i].BaseTaxAmount;
				//$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax).toFixed(2)) * $scope.productNew.ToCurrencyRate).toFixed(2);

				$scope.inventoryMaterialList[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat($scope.inventoryMaterialList[i].ServiceTax)).toFixed(2);
				$scope.inventoryMaterialList[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat($scope.inventoryMaterialList[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);


			}
			else {
				$scope.inventoryMaterialList[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge)).toFixed(2);
				$scope.inventoryMaterialList[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);

				//data.TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialList[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge).toFixed(2);
				//data.TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge).toFixed(2)) * $scope.productNew.ToCurrencyRate);
			}

		}
	};


	//#region  GRNReport

	$scope.GRNReport = function (data) {

		location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;
	};

	//#endregion

	$scope.calculateMaterialTax = function (data, index) {
		//debugger;
		// data.TransactionRate = (data.TrnAmount / data.TransactionQty).toFixed(2);
		//data.TrnAmount = (data.TransactionQty * data.TransactionRate).toFixed(2);
		//if (data.TrnAmount == 'NaN')
		//    data.TrnAmount = 0;
		//data.TaxAmount = 0;
		//data.BaseTaxAmount = 0;

		var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount');
		var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');
		var TotalMaterialTaxAmount = $filter('sumByKey')($filter('filter')($scope.receiveTaxList), 'TaxAmount');

		//angular.forEach(data.POMaterialTaxList, function (item) {
		//    item.TaxAmount = data.TrnAmount * item.Percentage / 100;
		//    data.BaseTaxAmount += item.TaxAmount;

		//});

		for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
			if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailId) {
				//$scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;
				$scope.inventoryMaterialListPO[i].BaseTaxAmount = TotalMaterialTaxAmount;
				// $scope.inventoryMaterialListPO[i].ServiceCharge = parseFloat((TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);
				//$scope.inventoryMaterialListPO[i].ServiceTax = parseFloat((TotalMaterialTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);

				//else {
				//    $scope.inventoryMaterialListPO[i].ServiceCharge = parseFloat((TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);
				//    $scope.inventoryMaterialListPO[i].ServiceTax = parseFloat((TotalMaterialTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);
				//}
				//if ($scope.productNew.IsNonCreditable == 1) {
				//    //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
				//    $scope.inventoryMaterialListPO[i].BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount + $scope.inventoryMaterialListPO[i].BaseTaxAmount + $scope.inventoryMaterialListPO[i].ServiceCharge + $scope.inventoryMaterialListPO[i].ServiceTax).toFixed(4);

				//}
				//else {
				//    data.BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(4);
				//}
				if ($scope.productNew.IsNonCreditable == 1) {
					//data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
					//  $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount + $scope.inventoryMaterialListPO[i].BaseTaxAmount + $scope.inventoryMaterialListPO[i].ServiceCharge + $scope.inventoryMaterialListPO[i].ServiceTax).toFixed(2);
					$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax))).toFixed(2);

					$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

				}
				else {
					//$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat(parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2)).toFixed(2);
					$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge))).toFixed(2);

					// $scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2)) * $scope.productNew.ToCurrencyRate).toFixed(2);
					$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);

				}
			}
		}
		//angular.forEach($scope.inventoryMaterialListPO, function (item) {
		//    item.ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * data.TrnAmount;

		//});

		//$scope.detailModel.BaseUOMId = $filter("filter")($scope.chargesListPO, { IsBaseUom: 1 })[0].Value;

		// data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
		//data.BaseAmount = $scope.productNew.ToCurrencyRate * data.TrnAmount;

	};

	$scope.calculateSerciceTax = function (data) {
		//debugger;
		var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount');
		var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');
		var ServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.ServiceTaxList), 'TaxAmount');
		var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'TotalTaxAmount');

		for (var i = 0; i < $scope.chargesListPO.length; i++) {
			if ($scope.chargesListPO[i].Id == data.InventoryServiceId) {
				$scope.chargesListPO[i].TotalTaxAmount = ServiceTaxAmount;
			}
		}

		for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
			//if ($scope.inventoryMaterialListPO[i].PODetailsID == data.Id) {
			//$scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;  
			//$scope.inventoryMaterialListPO[i].ServiceTax = Math.round(Math$scope.chargesListPO[i].TotalTaxAmount);
			$scope.inventoryMaterialListPO[i].ServiceCharge = parseFloat((TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);
			$scope.inventoryMaterialListPO[i].ServiceTax = parseFloat((TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(4);
			//}
			//else {
			//    $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
			//    $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
			//}
			//if ($scope.productNew.IsNonCreditable == 1) {
			//    //data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
			//    $scope.inventoryMaterialListPO[i].BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount + $scope.inventoryMaterialListPO[i].BaseTaxAmount + $scope.inventoryMaterialListPO[i].ServiceCharge + $scope.inventoryMaterialListPO[i].ServiceTax).toFixed(4);

			//}
			//else {
			//    data.BaseAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount + $scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(4);
			//}

			if ($scope.productNew.IsNonCreditable == 1) {
				//data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
				$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat(parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax).toFixed(2)).toFixed(2);
				$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

			}
			else {
				$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat(parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2)).toFixed(2);
				$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2)) * $scope.productNew.ToCurrencyRate).toFixed(2);
			}

		}

	};
	$scope.onClickReportDownloadExcel = function (args) {
		//debugger;
		var gridObj = $("#GriddataMaster1").data("ejGrid");
		//getting corresponding record 
		var data = gridObj.getSelectedRecords()[0];
		var reportFormat = "Excel";
		if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
		$window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&plantId=' + $scope.productNew.PlantId);

	};

	$scope.commandExcel = [{
		type: "details", buttonOptions: {
			text: "Excel",
			BackgroundColor: "Black",
			Color: "White",
			width: "50",
			height: "20",
			contentType: "imageonly",
			prefixIcon: "e-icon e-dataexport",

			//prefixIcon: "e-icon e-edit" ,
			//prefixIcon: "e-icon e-delete",
			//prefixIcon: " e-icon e-save",
			//prefixIcon: " e-icon e-cancel",

			click: $scope.onClickReportDownloadExcel
		}
	}];
	$scope.onClickReportDownloadPdf = function (args) {
		//debugger;
		var gridObj = $("#GriddataMaster1").data("ejGrid");
		//getting corresponding record 
		var data = gridObj.getSelectedRecords()[0];
		var reportFormat = "Pdf";
		if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
		$window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&plantId=' + $scope.productNew.PlantId);

	};
	$scope.commandPdf = [{
		type: "details", buttonOptions: {
			text: "Pdf",
			width: "50",
			height: "20",
			contentType: "imageonly",
			prefixIcon: "e-icon e-dataexport",

			//prefixIcon: "e-icon e-edit" ,
			//prefixIcon: "e-icon e-delete",
			//prefixIcon: " e-icon e-save",
			//prefixIcon: " e-icon e-cancel",

			click: $scope.onClickReportDownloadPdf
		}
	}];













	$scope.Get = function (index) {

		$scope.index = index;
		$scope.product = $scope.products[$scope.index];
		$scope.productNew = Object.assign({}, $scope.product);
		// $scope.productNew.GRNDate = data.GRNDate;
		getPartyPlantList();
		getInventoryMaterialList($scope.productNew.Id);
		getServiceChargeList($scope.productNew.Id);
		$scope.productId = $scope.productNew.Id;
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




	$scope.recorddoubleclickFromMasterGrid = function ($event) {
		//debugger;
		var x = $event;
		var Id = x.data.Id;
		//if (!baseService.isUndefinedOrNull(x.data.PurchaseDocumentAcceptanceId)) {
		//    ShowResult('Acceptance Data Can not modify', 'failure');
		//    return false;
		//}
		//else {

		ClearFields();
		$scope.productId = Id;
		$scope.Action = 'Update';
		$scope.ActionForEdit = 'Update';
		//$scope.POId = x.data.POID;
		$scope.POId1 = x.data.POID;
		//$scope.index = index;
		$scope.POID = x.data.POID;
		$scope.product = $scope.products[$scope.index];
		//$scope.productNew = Object.assign({}, $scope.product);
		$scope.productNew = x.data;
		$scope.productNew.NoteForAccounts = x.data.NoteForAccounts;
		$scope.productNew.GRNDate = x.data.GRNDate1;
		$scope.productNew.CheckedBy = x.data.CheckedBy;
		$scope.AcceptanceId = x.data.PurchaseDocumentAcceptanceId;
		$scope.AccDate = x.data.AcceptanceDate;
		$scope.loadAcceptanceDetail();
		if ($scope.AcceptanceId === null || $scope.AcceptanceId === "" || $scope.AcceptanceId === undefined) {
			$scope.status = 'PO';
			$scope.productNew.PO = $scope.status;

			$scope.tab1 = 1;
			$scope.GetSavedPOList1(Id);
		}
		else {

			$scope.status = 'Acceptance';
			$scope.productNew.PO = $scope.status;
			$scope.tab1 = 2;


		}

		getPartyPlantList();
		getInventoryMaterialList(Id);
		getServiceChargeList(Id);
		$scope.GetAdvanceTaxInfo(Id);
		$scope.productNew.TaxOptionAddiTax = 'Yes';
		$scope.TotalSumAfterTCS();
		$scope.ImagedataLoad(Id);
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


		//$scope.getToCurrencyRate();
		//if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
		//	var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
		//	if (paymentTerm.BaseLineDate !== null)
		//		if (paymentTerm.BaseLineDate === 'documentdate')
		//			$scope.IsBaseOnDueDateEnable = true;
		//		else
		//			$scope.IsBaseOnDueDateEnable = false;
		//}

		if (!$rootScope.isCollapsed) $rootScope.toggle();

		// }

		//var x = $event;
		//var Id = x.data.Id;
		////alert('Id'+Id);
		//$scope.productNew = x.data;
		//$scope.productId = "";
		//$scope.POId = x.data.Id;
		//$scope.product.POId = $scope.POId;
		//getPartyPlantList();
		////getPartyPlantEditList();
		//GetInventoryMaterialListByPO(Id);
		//getServiceChargeListPO(Id);         //$scope.productNew.Id
		////if (!baseService.isUndefinedOrNull($scope.productNew.PaymentTermId)) {
		////    var paymentTerm = $.grep($scope.paymentTermList, function (item) { return item.Value === $scope.productNew.PaymentTermId; })[0];
		////    if (paymentTerm.BaseLineDate !== null)
		////        if (paymentTerm.BaseLineDate === 'documentdate')
		////            $scope.IsBaseOnDueDateEnable = true;
		////        else
		////            $scope.IsBaseOnDueDateEnable = false;
		////}
		////$scope.Action = 'Update';
		//if (!$rootScope.isCollapsed) $rootScope.toggle();






	}





	//$scope.PODetailsUpdatePOPUp = function (x) {
	//    //debugger;
	//    $scope.Action1 = 'Update'
	//    // $scope.GetListForMasterOrder = [];
	//    getInventoryMaterialListForUpdate(x);
	//    // $scope.GerRequisition();
	//    angular.element(document.querySelector('#ListOfRequisition')).modal('show');
	//};

	//function getInventoryMaterialListForUpdate(inveReveiveId) {
	//    $scope.masterId = inveReveiveId;
	//    //debugger;
	//    //$scope.inventoryMaterialList = [];
	//    $http.get($scope.path + 'GetInventoryMaterialListForPOUpdate?inveReveiveId=' + inveReveiveId)
	//        .then(function (response) {
	//            $scope.GetListForMasterOrder = response.data;

	//        });
	//}
	$scope.MasterOrderListHide = function () {
		$scope.taxCategoryList = [];
		angular.element(document.querySelector('#ListOfRequisition')).modal('hide');
	};
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
	$scope.PODetailsUpdatePOPUp = function (x, MaterialMasterId, InventoryReceiveDetailId) {
		//debugger;
		$scope.Action1 = 'Update'
		// $scope.GetListForMasterOrder = [];
		getInventoryMaterialListForUpdate(x, MaterialMasterId, InventoryReceiveDetailId);
		// $scope.GerRequisition();
		angular.element(document.querySelector('#ListOfRequisition')).modal('show');
	};
	$scope.GetListForMasterOrder = [];
	function getInventoryMaterialListForUpdate(inveReveiveId, MaterialMasterId, InventoryReceiveDetailId) {
		$scope.Action1 = 'Save';
		$scope.masterId = inveReveiveId;
		//debugger;
		//$scope.inventoryMaterialList = [];
		$http.get($scope.path + 'GetInventoryMaterialListForPOUpdate?inveReveiveId=' + inveReveiveId + '&InventoryReceiveId=' + $scope.productNew.Id + '&MaterialMasterId=' + MaterialMasterId + '&InventoryReceiveDetailId=' + InventoryReceiveDetailId)
			.then(function (response) {
				$scope.GetListForMasterOrder = response.data;
				$scope.totalGRNVal = $scope.GetListForMasterOrder[0].GRNQty;
				$scope.RejectionQty = $scope.GetListForMasterOrder[0].RejectionQty;
			});


	}

	// #region checkbox all

	angular.isUndefinedOrNull = function (val) {
		return angular.isUndefined(val) || val === null || val === ""
	}
	function getTaxList(inveReveiveId) {
		//debugger;
		$http({
			method: 'GET',
			url: $scope.path + 'GetTaxCategoryListPO?receiveDetailId=' + inveReveiveId
		}).then(function (response) {
			$scope.taxCategoryList = response.data;
			//$scope.HSNCode = response.data[0]['HSNCode'];
			//angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');
		});
	}
	function checkChangeemployee(e) {
		//debugger;
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
		//if ($scope.Action1 === 'Save') {
		//    getTaxCategoryList(row[0].HSNCodeId);
		//}
		//else {
		//    getTaxCategoryList(row[0].HSNCodeId);
		//    getTaxList(row[0].InventoryReceiveId)
		//   // getTaxCategoryList(row[0].InventoryReceiveId);
		//}



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





	//$scope.GetListForMasterOrder = [];
	//$scope.getalldataListForUpdatePoDetails = function () {
	//    $scope.GetListForMasterOrder = [];
	//    $http({
	//        method: "GET",
	//        dataType: 'JSON',
	//        //url: $scope.getSearchListUrl,
	//        url: 'Products/PurchaseOrder/GetInventoryMaterialListForPOUpdate',
	//    }).then(function successCallback(response) { //datagatefun
	//        $scope.GetListForMasterOrder = response.data;
	//        //entrydata = copy(searchdata);
	//    });
	//};

	$scope.tab1 = 1;
	$scope.setTabIndex = function (newTab) {
		$scope.tab1 = newTab;
		$scope.getalldata();
	};
	$scope.isSetIndex = function (tabNum) {
		return $scope.tab1 === tabNum;
	};

	$scope.setTabIndex1 = function (newTab) {
		$scope.tab1 = newTab;
		$scope.getalldataIndexApp();
	};
	$scope.isSetIndex1 = function (tabNum) {
		return $scope.tab1 === tabNum;
	};
	// #endregion



	$scope.checkedByList = [];
	$scope.GetSupervisorCboList = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/InventoryReceive/GetSupervisorCbo'
		}).then(function successCallback(response) {
			$scope.checkedByList = response.data;
		});
	}
	$scope.GetSupervisorCboList();

	//#region GRN-By-PO Index Grid URL


	$scope.GRNbyPOCheckStatus = "ForChecked";
	$scope.GriddataMaster = [];
	$scope.GetListForGRNBYPO = function () {
		if ($scope.GRNbyPOCheckStatus === "ForChecked") {
			$scope.GRNbyPOCheckStatus = "ForChecked";
		}
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/GoodsReceiveNote/GetListForGRNBYPO?GRNbyPOCheckStatus=' + $scope.GRNbyPOCheckStatus,
		}).then(function successCallback(response) {
			$scope.GriddataMaster = response.data;
			//entrydata = copy(searchdata);
		});
	};
	$scope.GetListForGRNBYPO();


	//$scope.GRNbyPOApprovedStatus = "ApprovedHoldReject";
	$scope.GriddataMaster2 = [];
	$scope.getalldataMaster2 = function () {

		//if ($scope.GRNbyPOApprovedStatus === "ApprovedHoldReject") {
		//    $scope.GRNbyPOApprovedStatus = "ApprovedHoldReject";
		//}
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/GoodsReceiveNote/GetListForMasterData2?GRNbyPOApprovedStatus=' + $scope.GRNbyPOApprovedStatus,
		}).then(function successCallback(response) {
			$scope.GriddataMaster2 = response.data;
			//entrydata = copy(searchdata);
		});
	};
	// $scope.getalldataMaster2();

	//#endregion



	// #region GRN-By-PO Index  All Tab 
	$scope.GRN = "";
	$scope.tab = 1;
	$scope.GRNbyPOCheckStatus = "ForChecked";
	$scope.setTabGRNList = function (newTab) {
		$scope.tab = newTab;
		$scope.GRNbyPOCheckStatus = "ForChecked";
		$scope.getDataList();
		$scope.GetListForGRNBYPO();

		//alert('Checked Unapproval');
	};
	$scope.isSetGRNList = function (tabNum) {
		return $scope.tab === tabNum;
		$scope.GRN = 1;

	};



	$scope.setTabCheckedHR = function (newTab) {
		$scope.tab = newTab;
		$scope.GRNbyPOCheckStatus = "CheckedHoldReject";
		$scope.GetListForGRNBYPO();

	};
	$scope.isSetCheckedHR = function (tabNum) {
		return $scope.tab === tabNum;
		$scope.GRN = 2;
	};




	$scope.setTabNotApprovedChecked = function (newTab) {

		$scope.tab = newTab;
		$scope.GRNbyPOCheckStatus = "Checked";
		$scope.GetListForGRNBYPO();

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


	// #region GRN-By-PO  All Print Button 


	$scope.onClickReportDownloadWord = function (args) {
		//debugger;
		var gridObj = $("#GriddataMaster1").data("ejGrid");
		//getting corresponding record 
		var data = gridObj.getSelectedRecords()[0];
		var reportFormat = "Pdf";
		if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
		//$window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&plantId=' + $scope.productNew.PlantId);
		location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

	};

	$scope.commandWord = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",
			click: $scope.onClickReportDownloadWord
		}
	}];





	$scope.onClickReportCheckedHR = function (args) {
		//debugger;
		var gridObj = $("#GriddataCheckedHR").data("ejGrid");
		//getting corresponding record 
		var data = gridObj.getSelectedRecords()[0];
		var reportFormat = "Pdf";
		if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
		//$window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&plantId=' + $scope.productNew.PlantId);
		location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

	};

	$scope.commandCheckedHR = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",


			click: $scope.onClickReportCheckedHR
		}
	}];





	$scope.onClickReportApprovedChecked = function (args) {
		//debugger;
		var gridObj = $("#GriddataApprovedChecked").data("ejGrid");
		//getting corresponding record 
		var data = gridObj.getSelectedRecords()[0];
		var reportFormat = "Pdf";
		if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
		//$window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&plantId=' + $scope.productNew.PlantId);
		location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

	};

	$scope.commandApprovedCK = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",


			click: $scope.onClickReportApprovedChecked
		}
	}];



	$scope.onClickReportApprovedHR = function (args) {
		//debugger;
		var gridObj = $("#GriddataApprovedHR").data("ejGrid");
		//getting corresponding record 
		var data = gridObj.getSelectedRecords()[0];
		var reportFormat = "Pdf";
		if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
		//$window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&plantId=' + $scope.productNew.PlantId);
		location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

	};

	$scope.commandApprovedHRGRN = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",
			click: $scope.onClickReportApprovedHR
		}
	}];



	$scope.onClickReportDownloadWord2 = function (args) {
		//debugger;
		var gridObj = $("#GriddataMaster2").data("ejGrid");
		//getting corresponding record 
		var data = gridObj.getSelectedRecords()[0];
		var reportFormat = "Pdf";
		if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
		//$window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&plantId=' + $scope.productNew.PlantId);
		location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

	};

	$scope.commandWord2 = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",


			click: $scope.onClickReportDownloadWord2
		}
	}];


	$scope.onClickcommandPosted = function (args) {
		//debugger;
		var gridObj = $("#GriddataPosted").data("ejGrid");
		//getting corresponding record 
		var data = gridObj.getSelectedRecords()[0];
		var reportFormat = "Pdf";
		if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
		//$window.open('GoodsReceiveNote/Report?reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.Id + '&plantId=' + $scope.productNew.PlantId);
		location.href = " GoodsReceiveNote/GRNReport?grnId=" + data.Id;

	};

	$scope.commandPosted = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",


			click: $scope.onClickcommandPosted
		}
	}];

	//#endregion GRN-By-PO  All Print Button 



	//#region GRN by po New Tab

	//$scope.GRN = "";
	$scope.tab1 = 1;
	//$scope.GRNbyPOCheckStatus = "ForChecked";
	$scope.productNew.PO = "PO";
	$scope.status = "PO";
	$scope.setTabGRNPOList = function (newTab12) {
		$scope.GriddataSelected = [];

		$scope.AcceptanceId = '';
		$scope.Clear();
		$scope.productId = "";
		$scope.productNew.PO = "PO";
		$scope.status = "PO";
		$scope.tab1 = newTab12;
	};
	$scope.isSetGRNLPOist = function (tabNum12) {
		return $scope.tab1 === tabNum12;
		//$scope.GRN = 1;

	};



	$scope.setTabGRNAcceptance = function (newTab12) {

		$scope.Clear();
		$scope.productId = '';
		$scope.productNew.PO = "Acceptance";
		$scope.status = "Acceptance";
		$scope.GriddataSelected = [];
		$scope.tab1 = newTab12;
		$scope.productNew.TaxOptionAddiTax = 'Yes';
	};
	$scope.isSetGRNAcceptance = function (tabNum12) {
		return $scope.tab1 === tabNum12;
		//$scope.GRN = 2;
	};


	//#region Purchaser LC Intregrated to PurchaseOrder

	$scope.contractList = [];
	$scope.GetPopUpContract = function () {
		$scope.contractList = [];
		$http.get("Products/PurchaseOrder/GetLCContractList")
			.then(
				function successCallback(response) {
					if (baseService.arrayLength(response.data) > 0) {
						$scope.contractList = response.data;
					}
				},
				function errorCallback(response) {
					ShowResult(response, 'failure');
				});
		angular.element(document.querySelector('#ContractPopUp')).modal('show');
	};

	$scope.SelectedContract = function (obj) {
		var data = obj.data.ContractId;
		$scope.productNew.ContractId = data;
		$scope.productNew.CustomerName = obj.data.CustomerName;
		angular.element(document.querySelector('#ContractPopUp')).modal('hide');
	}

	$scope.ClearFields = function () {
		//$scope.purchaseLC = {};
		$scope.productNew.ContractId = null;
		// $scope.productNew = { OrderSpecific: 'Yes', Id: null, Tenure: 0 };
		//$scope.purchaseLCChargesList = [];
		//$scope.Action = 'Save';
	}
	$scope.CloseContractPopUp = function () {
		angular.element(document.querySelector('#ContractPopUp')).modal('hide');
	}
	$scope.masterOrderCustomerList = [];
	$scope.GetMasterOrderByContractList = function () {
		//debugger;
		// $scope.masterOrderCustomerList = [];
		$http({
			method: 'GET',
			url: "Commercial/Contract/GetMasterOrderListbyContract?contractId=" + $scope.productNew.ContractId
		}).then(function (response) {
			$scope.masterOrderCustomerList = response.data;
		});
		angular.element(document.querySelector('#MasterOrderPopUp')).modal('show');
	}

	$scope.CloseMasterOrder = function () {
		angular.element(document.querySelector('#MasterOrderPopUp')).modal('hide');
	}





	$scope.GriddataPOWithLC = [];
	$scope.getalldataPOWithLC = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/PurchaseOrder/GetalldataPOWithLCMap',
		}).then(function successCallback(response) {
			$scope.GriddataPOWithLC = response.data;
			//entrydata = copy(searchdata);
		});
	};
	$scope.getalldataPOWithLC();



	$scope.GriddataPOWithOutLC = [];
	$scope.getalldataPOWithOutLC = function () {
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/PurchaseOrder/GetalldataPOWithoutLCMap',
		}).then(function successCallback(response) {
			$scope.GriddataPOWithOutLC = response.data;
		});
	};
	$scope.getalldataPOWithOutLC();




	$scope.POTypeStatus = '';
	$scope.tab1 = 1;
	$scope.setTabPOLCMapIndex = function (newTab) {

		//$scope.POTypeStatus = 'Pending';
		$scope.tab1 = newTab;
		$scope.getalldataPOWithLC();
	};
	$scope.isSetPOLCMapIndex = function (tabNum) {
		return $scope.tab1 === tabNum;
	};
	$scope.setTabPOLCMap = function (newTab) {
		//alert('tabCHR');

		// $scope.POTypeStatus = 'CheckedHoldRej';
		$scope.tab1 = newTab;
		$scope.getalldataPOWithOutLC();
	};
	$scope.isSetPOLCMap = function (tabNum) {
		return $scope.tab1 === tabNum;
	};
	$scope.LcList = [];
	$scope.GetLCByContract = function () {

		$http({
			method: 'GET',//?id=' + id+' & name='+name
			url: "Products/PurchaseOrder/GetLCListByCotract?ContractId=" + $scope.data.ContractId + "&VendorId=" + $scope.data.PartyId
		}).then(function successCallback(response) {
			$scope.LcList = response.data;
			angular.element(document.querySelector('#ContractPopUp')).modal('show');

		});

	}

	// $scope.GetLCByContract();

	$scope.CurrencyId = null;
	$scope.a = function (args) {
		var gridObj = $("#Grid123").data("ejGrid");
		$scope.data = gridObj.getSelectedRecords()[0];
		$scope.rowID = $scope.data.Id;
		$scope.CurrencyId = $scope.data.CurrencyId;
		$scope.GetLCByContract();
	};


	$scope.recorddoubleclickContract = function ($event) {

		var x = $event;
		var Id = x.data.Id;

		for (var i = 0; i < $scope.GriddataPOWithOutLC.length; i++) {
			if ($scope.GriddataPOWithOutLC[i].Id === $scope.rowID) {

				if ($scope.CurrencyId === x.data.CurrencyId) {
					$scope.GriddataPOWithOutLC[i].PurchaseLCId = x.data.Value;
					angular.element(document.querySelector('#ContractPopUp')).modal('hide');
				} else {
					ShowResult("Purchase Order Currency and PurchaseLC Currency is not same!!!", 'failure', 'ContractPopUp');
				}
			}
		}

	};
	$scope.UpdatePOforLCdata = function () {

		if ($scope.data.PurchaseLCId === null || $scope.data.PurchaseLCId === '' || $scope.data.PurchaseLCId === undefined) {
			ShowResult('Please select Purchase LC');
			return false;
		}


		$http({
			method: 'POST',
			url: "Products/PurchaseOrder/UpdatePOforLC",
			data:
			{
				POId: $scope.rowID,
				PurchaseLCId: $scope.data.PurchaseLCId
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			if (response.data.Error === true) {
				ShowResult(response.data.Message, 'failure');
			}
			else {
				ShowResult(response.data.Message, 'success');
				//$scope.getDataList();
				$scope.getalldataPOWithOutLC();

			}
		}, function errorCallBack(response) {
			ShowResult(response.data.Message, 'failure');
		});





	}

	$scope.GetShortageRejectionValue = function () {
		$scope.newList = [];
		$http({
			method: 'GET',
			url: "Products/InventoryReceive/GetShortageRejectionValue?InventoryReceiveId=" + $scope.productNew.Id
		}).then(function (response) {
			$scope.newList = response.data;
		});
		angular.element(document.querySelector('#ValueSet')).modal('show');
	}

	$scope.UpdateUrlForSRValue = function () {
		$http({
			method: 'POST',
			url: $scope.updateUrlForSRValue,
			data:
			{
				'InventoryReceiveId': $scope.productNew.Id,
				'UserSendData': $scope.newList
			},
			dataType: 'JSON'
		}).then(function successCallback(response) {
			if (response.data.Error === true) {
				ShowResult(response.data.Message, 'failure');
			}
			else {
				ShowResult(response.data.Message, 'success');
				angular.element(document.querySelector('#ValueSet')).modal('hide');

			}
		}, function errorCallBack(response) {
			ShowResult(response.data.Message, 'failure');
		});
	}

	$scope.closeReceiveTaxPopUpValueNew = function (x) {
		angular.element(document.querySelector('#ValueSet')).modal('hide');
	}
	//#endregion
	//#endregion





	$scope.GRNAllowcationForSO = function (x, MaterialMasterId, InventoryReceiveDetailId, PODetailsID) {
		//debugger;
		$scope.Action1 = 'Update'
		GRNAllowcationForSOList(x, MaterialMasterId, InventoryReceiveDetailId, PODetailsID);
		angular.element(document.querySelector('#ListOfSo')).modal('show');


	};

	$scope.GRNAllowcationForSOInSavingTime = function (x, MaterialMasterId, InventoryReceiveDetailId, PODetailsID) {
		//debugger;
		$scope.Action1 = 'Save'

		GRNAllowcationForSOList1(x, MaterialMasterId, InventoryReceiveDetailId, PODetailsID);
		angular.element(document.querySelector('#ListOfSo')).modal('show');


	};
	$scope.soList = [];
	$scope.InventoryReceiveDetailId = '';
	function GRNAllowcationForSOList(x, MaterialMasterId, InventoryReceiveDetailId, PODetailsID) {
		$scope.Action1 = 'Save';
		$scope.InventoryReceiveDetailId = InventoryReceiveDetailId;
		$http.get($scope.path + 'GetGRNDetailsForSoAllocation?InventoryReceiveDetailId=' + x)
			.then(function (response) {
				$scope.soList = response.data;
				$scope.totalGRNVal = $scope.soList[0].GRNQty;
				$scope.RejectionQty = $scope.soList[0].GRNRejectionQty;
			});
	}

	$scope.soList = [];
	$scope.InventoryReceiveDetailId = '';
	function GRNAllowcationForSOList1(x, MaterialMasterId, InventoryReceiveDetailId, PODetailsID) {
		$scope.Action1 = 'Save';
		$scope.InventoryReceiveDetailId = InventoryReceiveDetailId;
		$http.get($scope.path + 'GetGRNDetailsForSoAllocation?PODetailId=' + PODetailsID)
			.then(function (response) {
				$scope.soList = response.data;
				for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
					$scope.totalGRNVal = $scope.inventoryMaterialListPO[0].TransactionQty;
					$scope.RejectionQty = $scope.inventoryMaterialListPO[0].RejectionQty;
					if ($scope.soList.length === 1) {
						for (var i1 = 0; i1 < $scope.soList.length; i1++) {
							$scope.soList[i1].TransactionQty = $scope.inventoryMaterialListPO[0].TransactionQty;
							$scope.soList[i1].RejectionQty = $scope.inventoryMaterialListPO[0].RejectionQty;

						}

					}
				}

			});
	}
	$scope.ListOfSo = function () {
		$scope.taxCategoryList = [];
		angular.element(document.querySelector('#ListOfSo')).modal('hide');
	};

	$scope.GrnRequisitionAllocationSave = function () {
		debugger;
		try {
			$scope.soListNew = [];
			var totalGRNQty = 0;
			var totalallowCatedQtyQty = 0;
			var totalGRNQty1 = 0;
			var totalallowCatedQtyQty1 = 0;
			for (var i = 0; i < $scope.soList.length; i++) {

				if ($scope.soList[i].Active === true) {
					var TotalSOQty = $filter('sumByKey')($filter('filter')($scope.soList), 'TransactionQty');
					var TotalRejectionQty = $filter('sumByKey')($filter('filter')($scope.soList), 'RejectionQty');
					//var TotalAllocatedQty = $filter('sumByKey')($filter('filter')($scope.soList), 'allowCatedQty');
					//var TotalAllocatedRejQty = $filter('sumByKey')($filter('filter')($scope.soList), 'RejectQty');
					if (TotalSOQty > $scope.totalGRNVal) {
						ShowResult('Allocated Qty can not grater than GRN Qty', 'failure', 'ListOfSo');
						return false;
					}
					else if (TotalRejectionQty > $scope.RejectionQty) {
						ShowResult('Allocated Qty can not grater than Rejection Qty', 'failure', 'ListOfSo');
						return false;
					}
					else if (baseService.isUndefinedOrNull($scope.soList[i].TransactionQty) || $scope.soList[i].TransactionQty===0) {
						ShowResult('Enter the Qty', 'failure', 'ListOfSo');
						return false;
					}
					//else if (baseService.isUndefinedOrNull($scope.soList[i].RejectionQty)) {
					//	ShowResult('Enter the Qty', 'failure', 'ListOfSo');
					//	return false;
					//}
					else {
						$scope.soListNew.push($scope.soList[i]);
					}

					totalGRNQty += $scope.soList[i].TransactionQty;
					totalGRNQty1 += $scope.soList[i].RejectionQty;
					
				}
				else {
					totalallowCatedQtyQty +=$scope.soList[i].allowCatedQty;
					totalallowCatedQtyQty1 +=$scope.soList[i].RejectQty;
				}

				var res = totalGRNQty + totalallowCatedQtyQty;
				var res1 = totalGRNQty1 + totalallowCatedQtyQty1;
				if (res > $scope.totalGRNVal) {
					ShowResult('allocated qty can not grater than GRN Qty', 'failure', 'ListOfSo');
					return false;
				}
				if (res1 > $scope.RejectionQty) {
					ShowResult('allocated qty can not grater than Rejection Qty', 'failure', 'ListOfSo');
					return false;
				}
				

			}
			if ($scope.soListNew.length === 0) {
				ShowResult('Please select atlest one item', 'failure', 'ListOfSo');
				return false;
			}
			// if ($scope.invalid) {
			if ($scope.Action1 === 'Save') {
				$http({
					method: 'POST',
					url: 'Products/GoodsReceiveNote/GrnRequisitionAllocationSave',
					data: {
						entity: $scope.soListNew
					},
					dataType: 'JSON'
				}).then(function successCallback(response) {
					if (response.data.Error === true)
						ShowResult(response.data.Message, 'failure', 'ListOfSo');
					else {
						ShowResult(response.data.Message, 'success', 'ListOfSo');
						GRNAllowcationForSOList($scope.InventoryReceiveDetailId);
						$scope.Action1 = "Update";
						//$scope.GetListForMasterOrder = [];
					}
				}), function errorCallBack(response) {
					ShowResult(response.data.Message, 'failure', 'ListOfSo');
				};

			}
			else if ($scope.Action1 === "Update") {
				$http({
					method: 'POST',
					url: 'Products/GoodsReceiveNote/GrnRequisitionAllocationSave',
					data: {
						entity: $scope.soListNew
					},
					dataType: 'JSON'
				}).then(function successCallback(response) {
					if (response.data.Error === true)
						ShowResult(response.data.Message, 'failure', 'ListOfSo');
					else {
						ShowResult(response.data.Message, 'success', 'ListOfSo');
						GRNAllowcationForSOList($scope.InventoryReceiveDetailId);

					}
				}), function errorCallBack(response) {
					ShowResult(response.data.Message, 'failure', 'ListOfSo');
				};

			}
			//}
		} catch (e) {
			//ShowResult(e, 'fail', 'detailPopUp');
		}
	};

	$scope.GRNAllowcationForRequisition = function (x, MaterialMasterId, InventoryReceiveDetailId) {
		//debugger;
		$scope.Action1 = 'Update'
		GRNAllowcationForRequisitionList(x, MaterialMasterId, InventoryReceiveDetailId);
		angular.element(document.querySelector('#ListOfRequisition')).modal('show');
	};
	$scope.GRNAllowcationForRequisitionLst = [];
	function GRNAllowcationForRequisitionList(inveReveiveId, MaterialMasterId, InventoryReceiveDetailId) {
		$scope.totalGRNVal = '';
		$scope.RejectionQty = '';
		$scope.Action1 = 'Save';
		$scope.masterId = inveReveiveId;
		$http.get($scope.path + 'GetInventoryMaterialListForPOUpdate?inveReveiveId=' + inveReveiveId + '&InventoryReceiveId=' + $scope.productNew.Id + '&MaterialMasterId=' + MaterialMasterId + '&InventoryReceiveDetailId=' + InventoryReceiveDetailId)
			.then(function (response) {
				$scope.GRNAllowcationForRequisitionLst = response.data;
				$scope.totalGRNVal = $scope.GRNAllowcationForRequisitionLst[0].GRNQty;
				$scope.RejectionQty = $scope.GRNAllowcationForRequisitionLst[0].RejectionQty;
			});


	}

	//#region Additional Code
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
			$scope.advanceTax.TaxName = $.grep($scope.taxCodCboListWithhold, function (item) {
				return item.Id === $scope.advanceTax.TaxCodeId;
			})[0].UserName;

			$scope.advanceTaxesList.push($scope.advanceTax);
			$scope.advanceTax = {};
		}
		$scope.TotalSumAfterTCS();
	};

	$scope.taxCodCboListWithhold = [];
	$scope.taxcodelistMessage = "";
	$scope.getTaxCodeByTaxYearWithhold = function (date) {
		$scope.productNew.TaxOptionAddiTax = 'Yes';
		$http({
			method: "Get",
			url: "accounts/TaxCode/GetAdditionalTaxCbo?postingDate=" + $filter("dateFiltering")(date)
		}).then(
			function successCallback(response) {
				if (response.data.Error === true) {
					$scope.taxcodelistMessage = response.data.Message;
				}
				else {
					$scope.taxCodCboListWithhold = response.data;;
				}
			},
			function errorCallback(response) {
			});
	};
	$scope.getTaxCodeByTaxYearWithhold($scope.productNew.GRNDate);
	$scope.selectadditionalTax = function () {
		$scope.advanceTax.ValueOfFixed = $.grep($scope.taxCodCboListWithhold, function (item) {
			return item.Id === $scope.advanceTax.TaxCodeId;
		})[0].ValueOfFixed;
		$scope.advanceTax.TaxCategoryId = $.grep($scope.taxCodCboListWithhold, function (item) {
			return item.Id === $scope.advanceTax.TaxCodeId;
		})[0].TaxCategoryId;
		$scope.advanceTax.Type = $.grep($scope.taxCodCboListWithhold, function (item) {
			return item.Id === $scope.advanceTax.TaxCodeId;
		})[0].Type;
		if ($scope.advanceTax.Type == 'FixedPercentage' && !baseService.isUndefinedOrNull($scope.advanceTax.ValueOfFixed)) {//* $scope.advanceTax.ValueOfFixed / 100
			//$scope.advanceTax.TaxAmount = (parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax")) * $scope.advanceTax.ValueOfFixed / 100);
			if ($scope.Action === 'Save') {
				$scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceTax"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);

			}
			else {
				$scope.advanceTax.TaxAmount = parseFloat(((parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax"))) * $scope.advanceTax.ValueOfFixed) / 100).toFixed(2);

			}
		}
		$scope.TotalSumAfterTCS();
	}

	$scope.SaveAdditinalTaxInGRNList = function () {
		$http({
			method: 'POST',
			url: 'Products/InventoryReceive/SaveAdditinalTaxInGRN',
			data:
			{
				'InventoryReceiveId': $scope.productNew.Id,
				'UserSendData': $scope.advanceTaxesList
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
			url: 'Products/InventoryReceive/GetAdvanceTaxInfo?InventoryReceiveId=' + Id,
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
			url: 'Products/InventoryReceive/AdditionalTaxDelete?Id=' + Id,
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
		$scope.advanceTax.TaxAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseAmount") * data / 100).toFixed(2);

	};
	$scope.checkRowValidationSdditionalTax = function (data) {
		debugger;

		if ($scope.Action === 'Save') {
			var netAmount = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceTax"))).toFixed(2);

			$scope.advanceTax.ValueOfFixed = ((data / netAmount).toFixed(4) * 100);
		}
		else {
			var netAmount1 = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax"))).toFixed(2);

			$scope.advanceTax.ValueOfFixed = ((data / netAmount1).toFixed(4) * 100);
		}
	}

	$scope.TotalSumAfterTCS = function () {

		if ($scope.Action === 'Save') {
			$scope.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialListPO), "ServiceTax")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);

		}
		else {
			$scope.TotalSumAfterTCSVal = parseFloat(parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "TrnAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "BaseTaxAmount")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceCharge")) + parseFloat($filter("sumByKey")($filter("filter")($scope.inventoryMaterialList), "ServiceTax")) + parseFloat($filter("sumByKey")($filter("filter")($scope.advanceTaxesList), "TaxAmount"))).toFixed(2);

		}





	}

	//#endregion

	$scope.calculateAmountAfterDiscount = function (data, index) {
		debugger;
		$scope.PreBal = data.Balance;
		data.TrnAmount = (data.NetQty * data.TransactionRate).toFixed(2);//(data.TransactionQty * data.TransactionRate).toFixed(2);
		if (data.TrnAmount == 'NaN')
			data.TrnAmount = 0;
		//data.TaxAmount = 0;
		//data.BaseTaxAmount = 0;
		//var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount');
		//var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');
		//var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.POServiceTaxList), 'TaxAmount');




		for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
			$scope.inventoryMaterialListPO[i].Balance = '';
			if ($scope.inventoryMaterialListPO[i].POQty < (parseFloat($scope.inventoryMaterialListPO[i].GRNRcvQty + $scope.inventoryMaterialListPO[i].TransactionQty).toFixed(2))) {
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
				if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
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
				if ($scope.productNew.IsNonCreditable == 1) {
					if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
						$scope.inventoryMaterialListPO[i].TrnAmount = (($scope.inventoryMaterialListPO[i].NetQty * $scope.inventoryMaterialListPO[i].TransactionRate) - data.DiscountAmount).toFixed(2);
						$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)).toFixed(2);
						$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

					}
				}
				else {
					if ($scope.inventoryMaterialListPO[i].PODetailsID == data.PODetailsID) {
						$scope.inventoryMaterialListPO[i].TrnAmount = (($scope.inventoryMaterialListPO[i].NetQty * $scope.inventoryMaterialListPO[i].TransactionRate) - data.DiscountAmount).toFixed(2);
						$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.ServiceCharge)).toFixed(2);
						$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat(data.ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);
					}
				}
			}
		}
		//angular.forEach(data.POMaterialTaxList, function (item) {
		//	item.TaxAmount = data.TransactionAmount * item.Percentage / 100;
		//	data.BaseTaxAmount += item.TaxAmount;

		//});

	};
	$scope.calculateAmountAfterDiscountEdit = function (data, index) {
		debugger;
		$scope.PreBal = data.Balance;
		data.TrnAmount = (data.NetQty * data.TransactionRate).toFixed(2);//(data.TransactionQty * data.TransactionRate).toFixed(2);
		if (data.TrnAmount == 'NaN')
			data.TrnAmount = 0;




		for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
			$scope.inventoryMaterialList[i].Balance = '';
			if ($scope.inventoryMaterialList[i].POQty < (parseFloat($scope.inventoryMaterialList[i].GRNRcvQty + $scope.inventoryMaterialList[i].TransactionQty).toFixed(2))) {
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
				if ($scope.inventoryMaterialList[i].PODetailsID == data.PODetailsID) {
					$scope.inventoryMaterialList[i].TrnAmount = data.TrnAmount;
					$scope.inventoryMaterialList[i].Balance = ($scope.inventoryMaterialList[i].POQty - ($scope.inventoryMaterialList[i].GRNRcvQty + $scope.inventoryMaterialList[i].TransactionQty));
					$scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - ($scope.inventoryMaterialList[i].ShortageQty + $scope.inventoryMaterialList[i].RejectionQty));
					$scope.inventoryMaterialList[i].NetQty = ($scope.inventoryMaterialList[i].TransactionQty - $scope.inventoryMaterialList[i].ShortageQty);

				}
				else {
					$scope.inventoryMaterialList[i].Balance = ($scope.inventoryMaterialList[i].POQty - ($scope.inventoryMaterialList[i].GRNRcvQty + $scope.v[i].TransactionQty));
					$scope.inventoryMaterialList[i].ApprovedQty = ($scope.inventoryMaterialList[i].TransactionQty - ($scope.inventoryMaterialList[i].ShortageQty + $scope.inventoryMaterialList[i].RejectionQty));
					$scope.inventoryMaterialList[i].NetQty = ($scope.inventoryMaterialList[i].TransactionQty - $scope.inventoryMaterialList[i].ShortageQty);
				}
				if ($scope.productNew.IsNonCreditable == 1) {
					if ($scope.inventoryMaterialList[i].PODetailsID == data.PODetailsID) {
						$scope.inventoryMaterialList[i].TrnAmount = (($scope.inventoryMaterialListPO[i].NetQty * $scope.inventoryMaterialListPO[i].TransactionRate) - data.DiscountAmount).toFixed(2);
						$scope.inventoryMaterialList[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat(data.ServiceTax)).toFixed(2);
						$scope.inventoryMaterialList[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat(data.ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

					}
				}
				else {
					if ($scope.inventoryMaterialList[i].PODetailsID == data.PODetailsID) {
						$scope.inventoryMaterialList[i].TrnAmount = (($scope.inventoryMaterialList[i].NetQty * $scope.inventoryMaterialList[i].TransactionRate) - data.DiscountAmount).toFixed(2);
						$scope.inventoryMaterialList[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.ServiceCharge)).toFixed(2);
						$scope.inventoryMaterialList[i].TotalMaterialBaseAmount = (parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat(data.ServiceCharge) * $scope.productNew.ToCurrencyRate).toFixed(2);
					}
				}
			}
		}

		//angular.forEach(data.POMaterialTaxList, function (item) {
		//	item.TaxAmount = data.TransactionAmount * item.Percentage / 100;
		//	data.BaseTaxAmount += item.TaxAmount;

		//});

	};
	$scope.TaxOptionAdditax = function (data) {
		debugger;
		$scope.productNew.TaxOptionAddiTax = data;
	};

	$scope.calculateTaxAmountForMat = function (data) {
		if (baseService.isUndefinedOrNull(data.Percentage)) {
			data.Percentage = 0;
		}
		data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;

		angular.forEach($scope.POMaterialTaxList, function (item) {
			if (item.InventoryReceiveDetailId === data.InventoryReceiveDetailId && item.TaxCategoryId === data.TaxCategoryId) {
				item.TaxAmount = data.TaxAmount;
				item.Percentage = data.Percentage;
			}

		});
	};

	$scope.checkRowValidationMat = function (x) {
		debugger;



		for (var i = 0; i < $scope.receiveTaxList.length; i++) {
			if (baseService.isUndefinedOrNull($scope.receiveTaxList[i].TaxAmount) || $scope.receiveTaxList[i].TaxAmount === 0) {
				ShowResult("Taxable Amount can not null or zero", 'failure', 'receiveTaxPopUp');
			}
			if ($scope.receiveTaxList[i].Id === x.Id) {
				$scope.receiveTaxList[i].Percentage = ((x.TaxAmount / $scope.taxAbleAmnt).toFixed(4) * 100).toFixed(2);
			}
		}
		angular.forEach($scope.POMaterialTaxList, function (item) {
			if (item.Id === x.Id) {
				item.TaxAmount = x.TaxAmount;
				item.Percentage = x.Percentage;
			}
		});
	}


	$scope.closeReceiveTaxPopUpNew = function (data) {
		//debugger;		
		if (baseService.isUndefinedOrNull($scope.productId)) {
			$scope.inventoryMaterialListPO[$scope.receiveTaxindex].BaseTaxAmount = $filter("sumByKey")($filter("filter")($scope.receiveTaxList), "TaxAmount");
			for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {


				if ($scope.productNew.IsNonCreditable == 1) {
					$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax)).toFixed(2);
					$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

				}
				else {
					$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)).toFixed(2);
					$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);
				}
			}
			angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
			$scope.receiveTaxindex = null;
		}
		else {
			$scope.inventoryMaterialList[$scope.receiveTaxindex].BaseTaxAmount = $filter("sumByKey")($filter("filter")($scope.receiveTaxList), "TaxAmount");
			for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {


				if ($scope.productNew.IsNonCreditable == 1) {
					$scope.inventoryMaterialList[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat($scope.inventoryMaterialList[i].ServiceTax)).toFixed(2);
					$scope.inventoryMaterialList[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat($scope.inventoryMaterialList[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

				}
				else {
					$scope.inventoryMaterialList[i].TotalMaterialTranAmount = (parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge)).toFixed(2);
					$scope.inventoryMaterialList[i].TotalMaterialBaseAmount = ((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);
				}
			}
			angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
			$scope.receiveTaxindex = null;
		}

	}

	$scope.GetAdvanceTaxInfo = function (Id) {

		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/InventoryReceive/GetAdvanceTaxInfo?InventoryReceiveId=' + Id,
		}).then(function successCallback(response) {
			$scope.advanceTaxesList = response.data;

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
				url: 'Products/InventoryReceive/GRNDocCreate',
				headers: { 'Content-Type': undefined },
				transformRequest: function (data) {
					formData.append("GRNDocumentMap", angular.toJson($scope.productDocMap));
					if (baseService.isUndefinedOrNull($scope.filedata) === false) {
						formData.append('file', data.file);
					}
					return formData;
				},
				data: {
					"GRNDocumentMap": $scope.productDocMap,
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
			url: 'Products/InventoryReceive/GRNDocumentMapData?POID=' + $scope.productNew.Id,
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
				url: 'Products/InventoryReceive/GRNImageDelete?Id=' + $scope.DocId,
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
	$scope.CalculateRateAndAmount = function () {
		$scope.detailModel.TransactionRate = parseFloat(($scope.detailModel.GrossAmount - $scope.detailModel.DiscountAmount) / $scope.detailModel.TransactionQty).toFixed(4);
		$scope.detailModel.TransactionAmount = parseFloat($scope.detailModel.GrossAmount - $scope.detailModel.DiscountAmount).toFixed(2);
	}
	//#endregion

	$scope.getServiceTaxListPOPOP = function (data, flag, Id, index) {
		//debugger;
		$scope.ServiceAddindex = index;
		$scope.taxAbleAmnt = data.Amount;
		$scope.percentageColumn = flag;
		$scope.Currency = $("#currency option:selected").text();
		$scope.currentMaterialRow = index;
		$scope.currentInventoryReceiveDetailIdRow = Id;
		$scope.taxAbleAmnt = data.Amount;
		$scope.percentageColumn = flag;
		$scope.currentMaterialRow = index;
		$scope.ServiceTaxList = [];
		if (data.POServiceTaxList.length > 0) {
			$scope.HSNCode = data.POServiceTaxList[0].HSNCode;
			$scope.ServiceTaxList = data.POServiceTaxList;
		}
		$scope.total = 0;
		for (var j = 0; j < $scope.ServiceTaxList.length; j++) {
			$scope.total = $scope.total + $scope.ServiceTaxList[j].TaxAmount;
		}
		angular.element(document.querySelector('#ServiceTaxPopUp')).modal('show');
		//$http({
		//    method: 'GET',
		//    url: $scope.path + 'GetServiceTaxListPO?serviceId=' + data.Id
		//}).then(function (response) {
		//    $scope.receiveTaxList = response.data;
		//    angular.element(document.querySelector('#ServiceTaxPopUp')).modal('show');
		//});


	}

	$scope.closeReceiveTaxPopUp1 = function () {
		//debugger;
		if ($scope.ActionForEdit === 'Update') {
			$scope.chargesList[$scope.ServiceAddindex].TotalTaxAmount = $filter("sumByKey")($filter("filter")($scope.ServiceTaxList), "TaxAmount");
			var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesList), 'Amount');
			var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialList), 'TrnAmount');
			var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.ServiceTaxList), 'TaxAmount');
			for (var i = 0; i < $scope.inventoryMaterialList.length; i++) {
				//if ($scope.inventoryMaterialListPO[i].PODetailsID == data.Id) {
				//$scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;
				$scope.inventoryMaterialList[i].ServiceCharge = parseFloat((TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialList[i].TrnAmount).toFixed(2);
				$scope.inventoryMaterialList[i].ServiceTax = parseFloat((TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialList[i].TrnAmount).toFixed(2);
				//}
				//else {
				//    $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
				//    $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
				//}
				if ($scope.productNew.IsNonCreditable == 1) {
					//data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
					//  $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount + $scope.inventoryMaterialListPO[i].BaseTaxAmount + $scope.inventoryMaterialListPO[i].ServiceCharge + $scope.inventoryMaterialListPO[i].ServiceTax).toFixed(2);
					$scope.inventoryMaterialList[i].TotalMaterialTranAmount = parseFloat((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat($scope.inventoryMaterialList[i].ServiceTax))).toFixed(2);

					$scope.inventoryMaterialList[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge) + parseFloat($scope.inventoryMaterialList[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

				}
				else {
					//$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat(parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2)).toFixed(2);
					$scope.inventoryMaterialList[i].TotalMaterialTranAmount = parseFloat((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge))).toFixed(2);

					$scope.inventoryMaterialList[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialList[i].TrnAmount) + parseFloat($scope.inventoryMaterialList[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);
				}

			}
		}

		else {
			$scope.chargesListPO[$scope.ServiceAddindex].TotalTaxAmount = $filter("sumByKey")($filter("filter")($scope.ServiceTaxList), "TaxAmount");
			var TotalServiceAmount = $filter('sumByKey')($filter('filter')($scope.chargesListPO), 'Amount');
			var TotalTrnAmount = $filter('sumByKey')($filter('filter')($scope.inventoryMaterialListPO), 'TrnAmount');
			var TotalServiceTaxAmount = $filter('sumByKey')($filter('filter')($scope.POServiceTaxList), 'TaxAmount');
			for (var i = 0; i < $scope.inventoryMaterialListPO.length; i++) {
				//if ($scope.inventoryMaterialListPO[i].PODetailsID == data.Id) {
				//$scope.inventoryMaterialListPO[i].TrnAmount = data.TrnAmount;
				$scope.inventoryMaterialListPO[i].ServiceCharge = parseFloat((TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2);
				$scope.inventoryMaterialListPO[i].ServiceTax = parseFloat((TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2);
				//}
				//else {
				//    $scope.inventoryMaterialListPO[i].ServiceCharge = (TotalServiceAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
				//    $scope.inventoryMaterialListPO[i].ServiceTax = (TotalServiceTaxAmount / TotalTrnAmount) * $scope.inventoryMaterialListPO[i].TrnAmount;
				//}
				if ($scope.productNew.IsNonCreditable == 1) {
					//data.NetAmount = parseFloat(data.TrnAmount) + parseFloat(data.TaxAmount);
					//  $scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat($scope.inventoryMaterialListPO[i].TrnAmount + $scope.inventoryMaterialListPO[i].BaseTaxAmount + $scope.inventoryMaterialListPO[i].ServiceCharge + $scope.inventoryMaterialListPO[i].ServiceTax).toFixed(2);
					$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax))).toFixed(2);

					$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].BaseTaxAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge) + parseFloat($scope.inventoryMaterialListPO[i].ServiceTax)) * $scope.productNew.ToCurrencyRate).toFixed(2);

				}
				else {
					//$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat(parseFloat($scope.inventoryMaterialListPO[i].TrnAmount).toFixed(2) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge).toFixed(2)).toFixed(2);
					$scope.inventoryMaterialListPO[i].TotalMaterialTranAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge))).toFixed(2);

					$scope.inventoryMaterialListPO[i].TotalMaterialBaseAmount = parseFloat((parseFloat($scope.inventoryMaterialListPO[i].TrnAmount) + parseFloat($scope.inventoryMaterialListPO[i].ServiceCharge)) * $scope.productNew.ToCurrencyRate).toFixed(2);
				}

			}
		}
		angular.element(document.querySelector('#ServiceTaxPopUp')).modal('hide');
	}

	$scope.calculateTaxAmountForService = function (data) {

		if ($scope.ActionForEdit === 'Update') {
			if (baseService.isUndefinedOrNull(data.Percentage)) {
				data.Percentage = 0;
			}
			data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
			for (var i = 0; i < $scope.POServiceTaxList.length; i++) {
				if ($scope.ServiceTaxList[i].Id === data.Id) {
					$scope.ServiceTaxList[i].Percentage = data.Percentage;
					$scope.ServiceTaxList[i].TaxAmount = data.TaxAmount;
				}
			}
		}
		else {
			if (baseService.isUndefinedOrNull(data.Percentage)) {
				data.Percentage = 0;
			}
			data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
			for (var i = 0; i < $scope.POServiceTaxList.length; i++) {
				if ($scope.POServiceTaxList[i].Id === data.Id) {
					$scope.POServiceTaxList[i].Percentage = data.Percentage;
					$scope.POServiceTaxList[i].TaxAmount = data.TaxAmount;
				}
			}
		}
	};
	$scope.checkRowValidationService = function (x) {
		debugger;

		if ($scope.ActionForEdit === 'Update') {
			for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
				if (baseService.isUndefinedOrNull($scope.ServiceTaxList[i].TaxAmount) || $scope.ServiceTaxList[i].TaxAmount === 0) {
					ShowResult("Taxable Amount can not null or zero", 'failure', 'ServiceTaxPopUp');
				}
				if ($scope.ServiceTaxList[i].Id === x.Id) {
					$scope.ServiceTaxList[i].Percentage = (parseFloat(x.TaxAmount / $scope.taxAbleAmnt).toFixed(4) * 100).toFixed(4);
				}

			}
			for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
				if ($scope.ServiceTaxList[i].Id === x.Id) {
					$scope.ServiceTaxList[i].Percentage = x.Percentage;
					$scope.ServiceTaxList[i].TaxAmount = x.TaxAmount;
				}
			}
		}
		else {
			for (var i = 0; i < $scope.ServiceTaxList.length; i++) {
				if (baseService.isUndefinedOrNull($scope.ServiceTaxList[i].TaxAmount) || $scope.ServiceTaxList[i].TaxAmount === 0) {
					ShowResult("Taxable Amount can not null or zero", 'failure', 'ServiceTaxPopUp');
				}
				if ($scope.ServiceTaxList[i].Id === x.Id) {
					$scope.ServiceTaxList[i].Percentage = (parseFloat(x.TaxAmount / $scope.taxAbleAmnt).toFixed(4) * 100).toFixed(4);
				}

			}
			for (var i = 0; i < $scope.POServiceTaxList.length; i++) {
				if ($scope.POServiceTaxList[i].Id === x.Id) {
					$scope.POServiceTaxList[i].Percentage = x.Percentage;
					$scope.POServiceTaxList[i].TaxAmount = x.TaxAmount;
				}
			}
		}

	}


	$scope.calculateTaxAmountForService1 = function (data) {

		if ($scope.Action === 'Update') {
			if (baseService.isUndefinedOrNull(data.Percentage)) {
				data.Percentage = 0;
			}
			data.TaxAmount = Math.round($scope.serviceModel.TransactionAmount * data.Percentage) / 100;
			for (var i = 0; i < $scope.taxCategoryList.length; i++) {
				if ($scope.taxCategoryList[i].Id === data.Id) {
					$scope.taxCategoryList[i].Percentage = data.Percentage;
					$scope.taxCategoryList[i].TaxAmount = data.TaxAmount;
				}
			}
		}
		//else {
		//	if (baseService.isUndefinedOrNull(data.Percentage)) {
		//		data.Percentage = 0;
		//	}
		//	data.TaxAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
		//	for (var i = 0; i < $scope.taxCategoryList.length; i++) {
		//		if ($scope.taxCategoryList[i].Id === data.Id) {
		//			$scope.taxCategoryList[i].Percentage = data.Percentage;
		//			$scope.taxCategoryList[i].TaxAmount = data.TaxAmount;
		//		}
		//	}
		//}
	};
	$scope.checkRowValidationService1 = function (x) {
		debugger;

		if ($scope.Action === 'Update') {
			for (var i = 0; i < $scope.taxCategoryList.length; i++) {
				if (baseService.isUndefinedOrNull($scope.taxCategoryList[i].TaxAmount) || $scope.taxCategoryList[i].TaxAmount === 0) {
					ShowResult("Taxable Amount can not null or zero", 'failure', 'ServiceTaxPopUp');
				}
				if ($scope.taxCategoryList[i].Id === x.Id) {
					$scope.taxCategoryList[i].Percentage = (parseFloat(x.TaxAmount / $scope.serviceModel.TransactionAmount).toFixed(4) * 100).toFixed(4);
				}

			}
			for (var i = 0; i < $scope.taxCategoryList.length; i++) {
				if ($scope.taxCategoryList[i].Id === x.Id) {
					$scope.taxCategoryList[i].Percentage = x.Percentage;
					$scope.taxCategoryList[i].TaxAmount = x.TaxAmount;
				}
			}
		}
		//else {
		//	for (var i = 0; i < $scope.taxCategoryList.length; i++) {
		//		if (baseService.isUndefinedOrNull($scope.taxCategoryList[i].TaxAmount) || $scope.taxCategoryList[i].TaxAmount === 0) {
		//			ShowResult("Taxable Amount can not null or zero", 'failure', 'ServiceTaxPopUp');
		//		}
		//		if ($scope.taxCategoryList[i].Id === x.Id) {
		//			$scope.taxCategoryList[i].Percentage = (parseFloat(x.TaxAmount / $scope.taxAbleAmnt).toFixed(4) * 100).toFixed(4);
		//		}

		//	}
		//	for (var i = 0; i < $scope.taxCategoryList.length; i++) {
		//		if ($scope.taxCategoryList[i].Id === x.Id) {
		//			$scope.taxCategoryList[i].Percentage = x.Percentage;
		//			$scope.taxCategoryList[i].TaxAmount = x.TaxAmount;
		//		}
		//	}
		//}

	}

}// End of main