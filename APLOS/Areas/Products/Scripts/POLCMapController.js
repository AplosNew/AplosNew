'use strict';
POLCMapController.$inject = ['accountService', 'addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller', '$location'];
function POLCMapController(accountService, addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $location) {
	$rootScope.title = "Purchase LC";
	$scope.Action = 'Save';
	$scope.index = -1;
	$scope.products = [];
	$scope.path = 'Products/PurchaseOrder/';
	$scope.getListUrl = $scope.path + 'getlist';
	$scope.saveUrl = $scope.path + 'create';
	$scope.saveUrlFG = $scope.path + 'CreateFGMasterOrder';
	$scope.updateUrl = $scope.path + 'edit';
	$scope.updateUrlFG = $scope.path + 'FGMasterOrderedit';
	$scope.deleteUrl = $scope.path + 'delete/';
	$scope.detailSaveUrl = $scope.path + 'detailcreate';
	$scope.sreviceSaveUrl = $scope.path + 'servicechargescreate';
	$scope.sreviceDeleteUrl = $scope.path + 'servicechargesdelete?serviceId=';
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

    //#region Model

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
		, AuthorizedBy: null
		, CheckedByStatus: null
		, AuthorizedByStatus: null
		, ContractId: null
		, OrderSpecific: 'No'
		, PurchaseLCId: null
		, CustomerName: null
		, PaymentMode: null
		, ContractNo: null
		, LCRef: null
		, labelCheckAndApproved: null
		, CheckedByStatusForNoti: null
		, ApprovedByStatusForNoti: null
	};
	$scope.productNew = Object.assign({}, $scope.product);
    //#endregion
  
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
		
	
		$scope.productNew.ContractId = obj.data.ContractId;
		$scope.productNew.CustomerName = obj.data.CustomerName;
		$scope.productNew.ContractNo = obj.data.ContractNo;
		$scope.productNew.LCRef = obj.data.LCRef;
		
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
		$scope.masterOrderCustomerList = [];
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
			url: "Products/PurchaseOrder/GetLCListByCotract?ContractId=" + $scope.data.ContractId + "&VendorId=" + $scope.data.PartyId + "&CurrencyId=" + $scope.data.CurrencyId
		}).then(function successCallback(response) {
			$scope.LcList = response.data;
			angular.element(document.querySelector('#ContractPopUp')).modal('show');

		});

	}

	$scope.CurrencyId = null;
	$scope.a = function (args) {
		var gridObj = $("#Grid123").data("ejGrid");
		$scope.data = gridObj.getSelectedRecords()[0];
		$scope.rowID = $scope.data.Id;
		$scope.Flag = $scope.data.Flag;
		$scope.IsFirst = $scope.data.IsFirst;
		
		$scope.CurrencyId = $scope.data.CurrencyId;
		$scope.GetLCByContract();
	};

	$scope.recorddoubleclickContract = function ($event) {

		var x = $event;
		var Id = x.data.Id;

		for (var i = 0; i < $scope.GriddataPOWithOutLC.length; i++) {
			if ($scope.GriddataPOWithOutLC[i].Id === $scope.rowID) {
				if ($scope.IsFirst==1) {
					if (x.data.IsAccepptanceFirst==1) {
						if ($scope.CurrencyId === x.data.CurrencyId) {
							$scope.GriddataPOWithOutLC[i].PurchaseLCId = x.data.Value;
							$scope.GriddataPOWithOutLC[i].LCRef = x.data.LCRef;
							angular.element(document.querySelector('#ContractPopUp')).modal('hide');
						}
						else {
							ShowResult("Purchase Order Currency and PurchaseLC Currency is not same!!!", 'failure', 'ContractPopUp');
							break;
						}
					}
					else {
						ShowResult("Cann't tag this LC because this PO is Acceptance First!!!", 'failure', 'ContractPopUp');
						break;
                    }
				}
				else {
					if (x.data.IsAccepptanceFirst == 0) {
						if ($scope.CurrencyId === x.data.CurrencyId) {
							$scope.GriddataPOWithOutLC[i].PurchaseLCId = x.data.Value;
							$scope.GriddataPOWithOutLC[i].LCRef = x.data.LCRef;
							angular.element(document.querySelector('#ContractPopUp')).modal('hide');
						}
						else {
							ShowResult("Purchase Order Currency and PurchaseLC Currency is not same!!!", 'failure', 'ContractPopUp');
							break;
						}
					}
                    else {
						ShowResult("Cann't tag this LC because this PO has already done for GRN!!!", 'failure', 'ContractPopUp');
						break;
                    }
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
				PurchaseLCId: $scope.data.PurchaseLCId,
				flag:$scope.Flag
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

	$scope.Clearcontract = function () {
		$scope.productNew.CustomerName = "";
		$scope.productNew.ContractId = "";

	};

	//#endregion

}