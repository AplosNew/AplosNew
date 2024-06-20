'use strict';
RequisitionController.$inject = ['accountService', 'addressService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function RequisitionController(accountService, addressService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {

	$rootScope.title = "Purchase Requisition";
	$scope.Action = 'Save';
	$scope.index = -1;
	$scope.products = [];
	$scope.path = 'Products/Requisition/';
	$scope.getListUrl = $scope.path + 'getlist';
	$scope.saveUrl = $scope.path + 'create';
	$scope.updateUrl = $scope.path + 'edit';
	$scope.updateByIdUrl = $scope.path + 'DetailEdit';
	$scope.deleteUrl = $scope.path + 'delete/';
	$scope.detailSaveUrl = $scope.path + 'detailcreate';
	$scope.detailDeleteUrl = $scope.path + 'DetailDelete/';
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
	$controller('currencyBaseController', { $scope: $scope, $http: $http });

	$scope.inventoryMaterialList = [];
	$scope.chargesList = [];
	$scope.ChargeTaxList = [];
	$scope.StateData = [];
	$scope.checkedByList = [];
	$scope.NotificationSetting = [];
	$scope.checkedByList = [];
	$scope.startDate = '';
	$scope.endDate = '';
	$scope.GetSupervisorCboList = function () {
		//debugger
		$http({
			method: 'GET',
			url: 'Products/Requisition/GetSupervisorCbo'
		}).then(function successCallback(response) {
			$scope.checkedByList = response.data;
		});
	}
	//$scope.GetSupervisorCboList();

	$scope.checkedByList1 = [];
	$scope.GetSupervisorCboList1 = function () {
		//debugger
		$http({
			method: 'GET',
			url: 'Products/Requisition/GetSupervisorCboApproved1'
		}).then(function successCallback(response) {
			$scope.checkedByList1 = response.data;
		});
	}
	
	$scope.NotificationSettingStatus = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/Requisition/NotificationSetting',
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

		});
	}
	$scope.NotificationSettingStatus();
	$scope.GetCheckedByAndApprovedBy1 = function () {
		if (!baseService.isUndefinedOrNull($scope.CheckedByStatusForNoti) && !baseService.isUndefinedOrNull($scope.ApprovedByStatusForNoti)) {
			$http({
				method: 'GET',
				url: 'Products/Requisition/GetCheckedByAndApprovedBY?CheckedBy=' + $scope.CheckedByStatusForNoti + '&ApprovedBy=' + $scope.ApprovedByStatusForNoti,
				dataType: 'JSON'
			}).then(function successCallback(response) {
				$scope.checkedByList = response.data;
			});

		}
		else {

		}

	}

	$scope.closeEmployeePopUp = function () {
		if ($scope.employeeIndex !== -1) {
			var employee = $scope.employeeList[$scope.employeeIndex];
			$scope.productNew.EmployeeName = employee.EmployeeName;
			$scope.productNew.NeedSpecialAppId = employee.SystemId;

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
			$scope.productNew.QualityApprovalResponsiblePersonId = employee.SystemId;

		}
		$scope.hideResponsiblePersonPopUp();
	};
	$scope.hideResponsiblePersonPopUp = function () {
		angular.element(document.querySelector("#responsiblePersonPopUp")).modal("hide");
	};

	$scope.lst = [];
	$scope.ReqListDetails = function () {
		$http({
			method: 'GET',
			url: 'Products/Requisition/GetAllReqdataDetails'
		}).then(function successCallback(response) {
			$scope.lst = response.data;
			window.lst = response.data;

		});
	}
	$scope.ReqListDetails();

	
	$scope.data1 = $scope.lst;
	$scope.detailTemp = "#tabGridContents";
	$scope.detailgrid = function detailGridData(e) {
		var filteredData = e.data["Id"];
		var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("MaterialReqqusitionMasterId", "equal", parseInt(filteredData), true).take(1000));
		e.detailsElement.find("#detailGrid").ejGrid({
			dataSource: data,
			columns: [{ field: "BudgetType", headerText: "BudgetType", width: 50 },
			{ field: "ActivityName", headerText: "ActivityName", width: 150 },
			{ field: "MaterialGroupName", headerText: "MaterialGroupName", width: 100 },
			{ field: "MaterialName", headerText: "Material Name", width: 150 },
			{ field: "ArticleName", headerText: "Article Name", width: 150 },
			{ field: "SKU1", headerText: "SKU1", width: 50 },
			{ field: "SKU2", headerText: "SKU2", width: 50 },
			{ field: "SKU3", headerText: "SKU3", width: 50 },
			{ field: "MaterialDetail", headerText: "MaterialDetail", width: 150 },
			{ field: "TransactionQty", headerText: "Qty", width: 70 },
			{ field: "TransactionUoM", headerText: "UoM", width: 50 },
			{ field: "EstimatedRate", headerText: "E.Rate", width: 50 },
			{ field: "CurrencyName", headerText: "Curr", width: 30 }, { field: "TotalAmount", headerText: "T.Amount", width: 100 }]
		});
		e.detailsElement.find(".tabcontrol").ejTab();
	}

	$scope.clearNeedSpecialApproval = function () {
		$scope.productNew.EmployeeName = null;
		$scope.NeedSpecialAppId = null;
	}
	$scope.clearQARespPerson = function () {
		$scope.productNew.ResponsiblePersonName = null;
		$scope.QualityApprovalResponsiblePersonId = null;

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

	$scope.storageList = [];
	$http({
		method: 'GET',
		url: 'Materials/MaterialStorage/getcbo'
	}).then(function (response) {
		$scope.storageList = response.data;
	});
	$scope.currencyList = [];

	$scope.product = {
		Id: null,
		CompanyGroupId: $window.companyGroupId,
		EntityId: null,
		RequisitionType: null,
		RequirmentType: null,
		QualityApprovalResponsiblePersonId: null,
		NeedSpecialAppId: null,
		ReasonWhyItIsNotPlanEarlier: null,
		AddedBy: null,
		AddedDate: new Date(),
		AddedFromIP: null,
		UpdatedBy: null,
		UpdatedDate: null,
		UpdatedFromIP: null,
		RequisitionDate: $filter("dateFiltering")(Date.now()),
		Remarks: null,
		CheckedBy: null,
		CheckedByStatus: null,
		AuthorizedBy: null,
		AuthorizedByStatus: null,
		IsApproved: null,
		RequisitionStatus: null,
		ResponsiblePersonName: null,
		EmployeeName: null,
		OrderRefNo: null,


		MasterOrderNo1: null,
		TotalQty1: null,
		CustomerName1: null,
		Contract1: null,
		MasterLCNo1: null,
		labelCheckAndApproved: null,
		CheckedByStatusForNoti: null,
		ApprovedByStatusForNoti: null


	};
	$scope.productNew = Object.assign({}, $scope.product);

	addressService.getCountryCbo(function (result) {
		$scope.countryList = result;
	});
	$scope.GetPOApprovalStatusCbo = function () {
		cboService.getEnumCbo("enum/GetPOApprovalStatusCbo", function (result) {
			$scope.POApprovalList = result;
		});
	}
	$scope.GetPOApprovalStatusCbo();
	$scope.GetCheckedStatusCbo = function () {
		//debugger
		cboService.getEnumCbo("enum/GetCheckedStatusCbo", function (result) {
			$scope.approvalStatusList = result;
		});
	}
	$scope.GetCheckedStatusCbo();

	$scope.productNew.RequisitionType = 'Revenue';
	$scope.productNew.RequirmentType = 'Normal';



	function loadCurrency() {
		//debugger
		cboService.getCboCurrencyTransactionForPotal(null, function (result) {
			$scope.currencyList = result;
			$scope.detailModel.CurrencyId = $scope.selectBaseCurrency();
			for (var i = 0; i < baseService.arrayLength($scope.currencyList); i++) {
				if ($scope.currencyList[i].Value === $scope.detailModel.CurrencyId) {
					$scope.detailModel.CurrencyName = $scope.currencyList[i].Text;
				}
			}
			$scope.getToCurrencyRate();
		});
	}


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

	$scope.Save = function () {
		//debugger
		if ($scope.CheckedByStatusForNoti === false && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
			ShowResult("Please select to be approved by", 'failure');
			return false;
		}
		else if ($scope.CheckedByStatusForNoti === true && $scope.ApprovedByStatusForNoti === true && baseService.isUndefinedOrNull($scope.productNew.CheckedBy)) {
			ShowResult("Please select to be checked by", 'failure');
			return false;
		}
		$scope.productNew.CheckedByStatusForNoti = $scope.CheckedByStatusForNoti;
		$scope.productNew.ApprovedByStatusForNoti = $scope.ApprovedByStatusForNoti;
		try {
			$scope.$broadcast('show-errors-check-validity');
			if ($scope.productNewForm.$valid) {

				//$scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
				$scope.product = Object.assign({}, $scope.productNew);
				//console.log('data', $scope.product);
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
							//$scope.productNew.PartyName = $scope.product.PartyName;

							$scope.Action = "Update";
							$scope.GetReq();
						}
					}), function (response) {
						ShowResult(response.data.Message, 'failure');
					};
				}
				else if ($scope.Action === "Update") {

					$http({
						method: 'POST',
						url: $scope.updateUrl,
						//data: $scope.product,
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
							ShowResult(response.data.Message, 'success');
							//$scope.getDataList();
							$scope.GetReq();
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
						//$scope.getDataList();
						$scope.RequisitionUnapproved();
						$scope.Requisitionapproved();
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
		$scope.Action = "Save";
		$scope.product = {};
		$scope.IsBaseOnDueDateEnable = false;
		$scope.inventoryMaterialList = [];
		$scope.chargesList = [];
		$scope.grossTotal = 0;
		baseService.removeErrorClasses();

		$scope.productNew = {
			Id: null,
			CompanyGroupId: $window.companyGroupId,
			EntityId: null,
			RequisitionType: null,
			RequirmentType: null,
			QualityApprovalResponsiblePersonId: null,
			NeedSpecialAppId: null,
			ReasonWhyItIsNotPlanEarlier: null,
			AddedBy: null,
			AddedDate: new Date(),
			AddedFromIP: null,
			UpdatedBy: null,
			UpdatedDate: null,
			UpdatedFromIP: null,
			RequisitionDate: $filter("dateFiltering")(Date.now()),
			Remarks: null,
			CheckedBy: null,
			CheckedByStatus: null,
			AuthorizedBy: null,
			AuthorizedByStatus: null,
			IsApproved: null,
			RequisitionStatus: null,
			ResponsiblePersonName: null,
			EmployeeName: null
		};

		//$scope.getToCurrencyRate();
	}
	$scope.clearBudgetType = function () {
		$scope.detailModel = {
			Id: null
			, CompanyGroupId: null
			, MaterialReqqusitionMasterId: $scope.productNew.Id
			, ActivityId: null
			, MaterialMasterId: null
			, MaterialMasterName: null
			, ArticleId: null
			, ArticleName: null
			, FirstCharacteristicsId: null
			, FirstCharacteristicsValueId: null
			, SecondCharacteristicsId: null
			, SecondCharacteristicsValueId: null
			, ThirdCharacteristicsId: null
			, ThirdCharacteristicsValueId: null
			, MaterialDetail: null
			, TransactionUoMId: null
			, CurrencyId: null
			, TransactionQty: null
			, EstimatedRate: null
			, TotalAmount: null
			, BudgetType: $scope.detailModel.BudgetType
			, Reason: null
			, Remarks: null
			, QualityApprovalResponsiblePersonId: null
			, NeedSpecialAppId: null
			, FutureReqApp: null
			, deliverydate: null
			, BudgetMasterId: null
			, GLGeneralInfoId: null
			, LocalImported: null
			, OwnQty: null
			, OtherQty: null
			, CommitmentDate: null
			, ResponsiblePersonNamee: null
			, EmployeeNamee: null

		};
		loadCurrency();
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
	$scope.closePartyPopUp = function () {
		//debugger
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
			$scope.hidePartyPopUp();
		}
	};
	$scope.GetCurrencyExchangeRateList = function () {
		//debugger
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
		$http.get($scope.path + 'GetToCurrencyRate?currencyId=' + $scope.detailModel.CurrencyId)
			.then(function (response) {
				if (parseFloat(response.data) === 0) {


					$scope.detailModel.ToCurrencyRate = 1;
					$scope.detailModel.CurrencyName = angular.element("#currency :selected").text();
				}
				else {


					$scope.detailModel.ToCurrencyRate = response.data;
					$scope.detailModel.CurrencyName = angular.element("#currency :selected").text();
				}
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
	$scope.businessProcesses = '';
	$scope.detailPopUp = function () {

		//$scope.receiveTaxList = [];
		$scope.detailModel = {
			Id: null
			, CompanyGroupId: null
			, MaterialReqqusitionMasterId: $scope.productNew.Id
			, ActivityId: null
			, MaterialMasterId: null
			, MaterialMasterName: null
			, ArticleId: null
			, ArticleName: null
			, FirstCharacteristicsId: null
			, FirstCharacteristicsValueId: null
			, SecondCharacteristicsId: null
			, SecondCharacteristicsValueId: null
			, ThirdCharacteristicsId: null
			, ThirdCharacteristicsValueId: null
			, MaterialDetail: null
			, TransactionUoMId: null
			, CurrencyId: null
			, TransactionQty: null
			, EstimatedRate: null
			, TotalAmount: null
			, BudgetType: null
			, Reason: null
			, Remarks: null
			, QualityApprovalResponsiblePersonId: null
			, NeedSpecialAppId: null
			, FutureReqApp: null
			, deliverydate: null
			, BudgetMasterId: null
			, GLGeneralInfoId: null
			, LocalImported: null
			, OwnQty: null
			, OtherQty: null
			, CommitmentDate: null
			, ResponsiblePersonNamee: null
			, EmployeeNamee: null

		};
		$scope.detailModel.LocalImported = 'Local';
		$scope.detailModel.BudgetType = 'Normal';
		loadCurrency();
		$scope.detailModel.NeedSpecialAppId = $scope.productNew.NeedSpecialAppId;
		$scope.detailModel.QualityApprovalResponsiblePersonId = $scope.productNew.QualityApprovalResponsiblePersonId;
		angular.element(document.querySelector('#detailPopUp')).modal('show');
	};
	//$scope.enable = true;
	//$scope.MAction = "Edit";
	//InventoryReceiveDetailId, TransactionQty, TransactionRate, TrnAmount, BaseTaxAmount, BaseAmount, index
	$scope.detailPopUpEdit = function () {
	
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
					url: 'Products/Requisition/UpdateMaterial',
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
		$scope.RequisitionStockBalanceList = [];
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
		//debugger
		console.log('attribute', ob);
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
	$scope.uom = function () {
		//debugger
		cboService.getUoMCbo(function (response) {
			$scope.uoMList = response;
		});
	}
	$scope.uom();
	$scope.setInputeMaterialArticleData = function (ob) {
		try {
			$scope.detailModel.ArticleId = ob.data.Id;
			$scope.detailModel.ArticleName = ob.data.StandardName;
			$scope.detailModel.TransactionUoMId = ob.data.BaseUOMId;
			manualValidation('div_ar', false);
			$scope.GetRequisitionStockBalance(ob.data.MaterialMasterId, ob.data.Id);
			angular.element(document.querySelector('#materialarticleNewPopUp')).modal('hide');
			GetArticleAlias();
		} catch (e) {
			ShowResult(e, '', 'articleSearchPop');
		}
	};
	$scope.RequisitionStockBalanceList = [];
	$scope.GetRequisitionStockBalance = function (materialMasterId,articleId) {
		$http({
			method: "GET",
			dataType: 'JSON',
			url: 'Products/Requisition/GetRequisitionStockBalance?requisitionDate=' + $scope.productNew.RequisitionDate + '&materialMasterId=' + materialMasterId + '&articleId=' + articleId,
		}).then(function successCallback(response) {
			$scope.RequisitionStockBalanceList = response.data;
			$scope.StockBalance = Math.round($filter("sumByKey")($filter("filter")($scope.RequisitionStockBalanceList), "Stock") * 10000 + Number.EPSILON) / 10000;

		});
	};
    $scope.showStockDetail = function () {
		angular.element(document.querySelector('#StockDetailPopUp')).modal('show');
    }

	$scope.closeStockDetail = function () {
		angular.element(document.querySelector('#StockDetailPopUp')).modal('hide');
	}

	$scope.setCharData = function (data) {
		$scope[$scope.charValueSearchFor].CharacteristicsValueId = data.CharacteristicsValueId;
		$scope[$scope.charValueSearchFor].FreeText = data.UserName;
		$scope[$scope.charValueSearchFor].FlagDisable = $scope.isSearch;
		angular.element(document.querySelector('#searchcharactervaluepopup')).modal('hide');
	};
	//RequisitionDate: $filter("dateFiltering")(Date.now()),
	$scope.materialValidation = function () {

		// if (ob.HasAttribute) $scope.getArticleSearchList(ob.Id);
		if ($scope.hasArticle == true && $scope.detailModel.ArticleId === null) {//hasArticle== Hasattribute
			ShowResult('Material has no Article !');
			$scope.invalid = false;
		}
		else {
			
			if ($scope.detailModel.ArticleId)
				var getRow3 = $filter("filter")($scope.inventoryMaterialList, { "MaterialMasterId": $scope.detailModel.MaterialMasterId, "ArticleId": $scope.detailModel.ArticleId, "FirstCharacteristicsValueId": $scope.detailModel.FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.detailModel.SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.detailModel.ThirdCharacteristicsValueId });
			//getRow == 0 || getRow2 == 0 ||
			else
				var getRow3 = $filter("filter")($scope.inventoryMaterialList, { "MaterialMasterId": $scope.detailModel.MaterialMasterId, "FirstCharacteristicsValueId": $scope.detailModel.FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.detailModel.SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.detailModel.ThirdCharacteristicsValueId });
			//getRow == 0 || getRow2 == 0 ||
			if (getRow3 == 0) {
				$scope.invalid = true;
			}

			else {
				ShowResult('Material Combination Already Exist');
				$scope.invalid = false;
			}

		}
		//$scope.MatDescriptionValidation();

	}
	$scope.MatDescriptionValidation = function () {
		var getRow31 = $filter("filter")($scope.inventoryMaterialList, { "MaterialDetail": $scope.detailModel.MaterialDetail });

		if (getRow31 == 0) {
			$scope.invalid1 = true;
		}

		else {
			ShowResult('Material Description Already Exist');
			$scope.invalid1 = false;
			return false;
		}
	}
	$scope.detailSave = function () {
		//debugger
		try {
			//$scope.validation();
			//if (baseService.isUndefinedOrNull($scope.detailModel.MaterialMasterId)) {
			//	//ShowResult('Select Material');
			//	ShowResult('Please Select Material', 'failure', 'detailPopUp');
			//	return false;
				
			//}




			 if (baseService.isUndefinedOrNull($scope.detailModel.ActivityName)) {
				ShowResult('Please Select Budget Activity', 'failure', 'detailPopUp');
				return false;
			}
			else if (baseService.isUndefinedOrNull($scope.detailModel.CurrencyId)) {
				//ShowResult('Select Material');
				ShowResult('Please select currency', 'failure', 'detailPopUp');
				return false;
			}
			else if (baseService.isUndefinedOrNull($scope.detailModel.deliverydate)) {
				ShowResult('Please select required date', 'failure', 'detailPopUp');
				return false;
			}


			else if (baseService.isUndefinedOrNull($scope.detailModel.TransactionQty)) {
				ShowResult('Please Select Transaction Qty', 'failure', 'detailPopUp');
				return false;
			}
			else if (baseService.isUndefinedOrNull($scope.detailModel.TransactionUoMId)) {
				ShowResult('Please Select UOM', 'failure', 'detailPopUp');
				return false;
			}
			else if (baseService.isUndefinedOrNull($scope.detailModel.EstimatedRate)) {
				ShowResult('Please Select Estimated Rate', 'failure', 'detailPopUp');
				return false;
			}


			else {
				$scope.detailModel.InventoryReceiveId = $scope.productNew.Id;
				//$scope.detailModel.FirstCharacteristicsId = $scope.char1.CharacteristicsId;
				//$scope.detailModel.FirstCharacteristicsValueId = $scope.char1.CharacteristicsValueId;
				//$scope.detailModel.SecondCharacteristicsId = $scope.char2.CharacteristicsId;
				//$scope.detailModel.SecondCharacteristicsValueId = $scope.char2.CharacteristicsValueId;
				//$scope.detailModel.ThirdCharacteristicsId = $scope.char3.CharacteristicsId;
				//$scope.detailModel.ThirdCharacteristicsValueId = $scope.char3.CharacteristicsValueId;
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
			}

			//if ($scope.invalid1 === false && $scope.invalid === false) {
			//	return false;
			//}
			$scope.entity = $scope.detailModel;

			$scope.$broadcast('show-errors-check-validity');
			if ($scope.detailForm.$valid) {
				if ($scope.detailModel.MaterialMasterId == "" || $scope.detailModel.MaterialMasterId == null || $scope.detailModel.MaterialMasterId == "undefined") {
					$http({
						method: 'POST',
						url: $scope.detailSaveUrl,
						data: { entity: $scope.entity },
						dataType: 'JSON'
					}).then(function successCallback(response) {
						if (response.data.Error === true)
							ShowResult(response.data.Message, 'failure', 'detailPopUp');
						else {
							ShowResult(response.data.Message, 'success', 'detailPopUp');


							$scope.detailModel.Id = null;
							$scope.detailModel = {
								MaterialReqqusitionMasterId: $scope.productNew.Id

								, CurrencyName: angular.element("#currency :selected").text()
								, CurrencyId: $scope.productNew.CurrencyId
								, TotalAmount: 0
								, ToCurrencyRate: $scope.productNew.ToCurrencyRate
							};
							// $scope.taxCategoryList = [];
							getInventoryMaterialList($scope.productNew.Id);
							$scope.GetReq();
							// $scope.ReqListDetails();
							$scope.setTab(1);
							$scope.setTabReqList(1);
							$scope.clearCharNames();
							$scope.detailModel.NeedSpecialAppId = $scope.productNew.NeedSpecialAppId;
							$scope.detailModel.QualityApprovalResponsiblePersonId = $scope.productNew.QualityApprovalResponsiblePersonId;

						}
					}), function errorCallBack(response) {
						ShowResult(response.data.Message, 'failure', 'detailPopUp');
					};
				}
				else {
					if ($scope.invalid) {

						$http({
							method: 'POST',
							url: $scope.detailSaveUrl,
							data: { entity: $scope.entity },
							dataType: 'JSON'
						}).then(function successCallback(response) {
							if (response.data.Error === true)
								ShowResult(response.data.Message, 'failure', 'detailPopUp');
							else {
								ShowResult(response.data.Message, 'success', 'detailPopUp');

								$scope.detailModel.Id = null;
								$scope.detailModel = {
									MaterialReqqusitionMasterId: $scope.productNew.Id

									, CurrencyName: angular.element("#currency :selected").text()
									, CurrencyId: $scope.productNew.CurrencyId
									, TotalAmount: 0
									, ToCurrencyRate: $scope.productNew.ToCurrencyRate
								};
								getInventoryMaterialList($scope.productNew.Id);
								$scope.GetReq();
								$scope.clearCharNames();
								//$scope.ReqListDetails();
								$scope.setTab(1);
								$scope.setTabReqList(1);

							}
						}), function errorCallBack(response) {
							ShowResult(response.data.Message, 'failure', 'detailPopUp');
						};
					}

				}

			}

		} catch (e) {
			ShowResult(e, 'fail', 'detailPopUp');
		}

	};
	$scope.valuePassInDelModal = function (MaterialReqqusitionMasterId) {
		//debugger
		$scope.id = MaterialReqqusitionMasterId;
		$scope.message = 'Are you sure want to permanently delete this?';
		angular.element(document.querySelector('#removerPopUp')).modal('show');
	};
	$scope.detailDelete = function () {
		try {
			$http({
				method: 'POST',
				url: 'Products/Requisition/DetailDelete?id=' + $scope.id
				//  url: $scope.detailDeleteUrl + $scope.id
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
		//debugger
		$scope.inventoryMaterialList = [];
		$http.get($scope.path + 'GetInventoryMaterialList?inveReveiveId=' + inveReveiveId)
			.then(function (response) {

				$scope.inventoryMaterialList = response.data.Rows;
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
	$scope.calculateTaxCategoryRate = function () {
		//debugger
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

		//debugger
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
		//debugger
		$scope.detailModel = {};
		//$scope.receiveTaxList = [];
		//////debugger;



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
			
				getInventoryMaterialList($scope.productNew.Id);
			}
		}), function (response) {
			ShowResult(response.data.Message, 'failure', 'receiveTaxPopUp');
		};
		// }

		//angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');

	}
	$scope.closeServiceChargeTaxPopUp = function () { //hossain
		//////debugger;



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
				
			}
		}), function (response) {
			ShowResult(response.data.Message, 'failure', 'ServiceChargeTaxPopUp');
		};
		// }

		//angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');

	}
	$scope.closeReceiveTaxPopUpwindow = function () {
		//debugger
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
			url: 'Products/Requisition/GetReqMaster?id=' + id
		}).then(function successCallback(response) {
			$scope.paymentTermList1 = response.data;
			$scope.productNew.Id = $scope.paymentTermList1[0].Id;
			$scope.productNew.CompanyGroupId = $scope.paymentTermList1[0].CompanyGroupId;
			$scope.productNew.EntityId = $scope.paymentTermList1[0].EntityId;
			$scope.productNew.RequisitionType = $scope.paymentTermList1[0].RequisitionType;
			$scope.productNew.RequirmentType = $scope.paymentTermList1[0].RequirmentType;
			$scope.productNew.Remarks = $scope.paymentTermList1[0].Remarks;
			$scope.productNew.ReasonWhyItIsNotPlanEarlier = $scope.paymentTermList1[0].ReasonWhyItIsNotPlanEarlier;
			$scope.productNew.RequisitionDate = $filter("dateFiltering")($scope.paymentTermList1[0].RequisitionDate);
			$scope.productNew.QualityApprovalResponsiblePersonId = $scope.paymentTermList1[0].QualityApprovalResponsiblePersonId;
			$scope.productNew.NeedSpecialAppId = $scope.paymentTermList1[0].NeedSpecialAppId;
			// $scope.productNew.CheckedBy = $scope.paymentTermList1[0].CheckedBy;
			$scope.productNew.EmployeeName = $scope.paymentTermList1[0].EmployeeName;
			$scope.productNew.ResponsiblePersonName = $scope.paymentTermList1[0].ResponsiblePersonName;

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
		//debugger
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

		//debugger
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
		//////debugger;
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
		//debugger
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
		//debugger

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
		//debugger
		var x = $event;
		var Id = x.data.Id;
		$scope.Currency = $("#currency option:selected").text();
		$scope.productNew = x.data;
		$scope.Id = $scope.productNew.Id;
		//getPartyPlantList();
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

		$scope.GetTerms($scope.productNew.Id);
		if (baseService.isUndefinedOrNull(x.data.CheckedById) && !baseService.isUndefinedOrNull(x.data.ApprovedById)) {

			$scope.productNew.CheckedBy = x.data.ApprovedById;
			$scope.productNew.labelCheckAndApproved = 'To be approved by';
		}
		else if (!baseService.isUndefinedOrNull(x.data.CheckedById) && baseService.isUndefinedOrNull(x.data.ApprovedById)) {

			$scope.productNew.CheckedBy = x.data.CheckedById;
			$scope.productNew.labelCheckAndApproved = 'To be checked by';
		}
		getInventoryMaterialList($scope.productNew.Id);
		
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
	$scope.calculateAmount = function (data) {
		//debugger
		data.TotalAmount = (data.TransactionQty * data.EstimatedRate).toFixed(2);
		if (data.TotalAmount === 'NaN')
			data.TotalAmount = 0;
		
	};
	$scope.calculateRate = function (data, event) {
		//debugger
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
		//debugger
		var getRow = $filter("filter")($scope.receiveTaxList, { "TaxCategoryId": id });
		if (getRow.length === 2) {
			ShowResult("You can't add Same Tax two times", 'failure', 'receiveTaxPopUp');

		}

	};
	$scope.onClick = function (args) {

		var gridObj = $("#Grid").data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		location.href = "Products/Requisition/GePurchaseOrderReport?purchaseOrderId=" + data.Id;

	};



	$scope.onClick1 = function (args) {

		var gridObj = $("#Grid").data("ejGrid");
		var data = gridObj.getSelectedRecords()[0];
		location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

	};

	$scope.command = [{
		type: "details", buttonOptions: {
			text: "View",
			width: "50",
			height: "20",

			click: $scope.onClick1
		}
	}];

	$scope.AllTabPrint = function (z) {
		//debugger
		var x = "#" + z;
		var gridObj = $(x).data("ejGrid");
		var data = gridObj.getSelectedRecords()[0];

		$http({
			method: 'GET',
			url: 'Products/Requisition/GetFiscalYear?formattedDate=' + data.RequisitionDate1,
		}).then(function successCallback(response) {
			$scope.startDate = response.data[0].StartDate;
			$scope.endDate = response.data[0].EndDate;
			location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id + '&startDate=' + $scope.startDate + '&endDate=' + $scope.endDate;
		});
		

	};


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
		//debugger
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

		//debugger
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

		//debugger
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
				$scope.GetCheckedStatusCbo();
			}
		}, function errorCallBack(response) {
			ShowResult(response.data.Message, 'failure');
		});
	}
	$scope.poAppUnApproved = function () {

		//debugger
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
		//debugger
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
	$scope.tab1 = 1;
	$scope.setTab = function (newTab1) {
		$scope.tab1 = newTab1;
	};
	$scope.isSet = function (tabNum1) {
		return $scope.tab1 === tabNum1;
	};
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
		//debugger
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
		//debugger
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
		//debugger
		// alert('Do you want to see Material Details');
		var x = $event;
		var Id = x.data.Id;
		$scope.MONo = Id;
		getMasterItemList();
		angular.element(document.querySelector('#ListOfMasterOrder')).modal('hide');

	};

	function getMasterItemList() {
		//debugger
		$scope.inventoryMaterialList = [];
		$http.get($scope.path + 'GetMasterItemList?masterOrderId=' + $scope.MONo)
			.then(function (response) {

				$scope.inventoryMaterialList = response.data;
				$scope.GetSalesTaxData();
			});
	}
	$scope.calculateAmountByRateFG = function (data) {
		//debugger
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
		//debugger

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

		//debugger
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

		//debugger
		$scope.LoadTaxButtonClick();

		$scope.Currency = $("#currency option:selected").text();
		$scope.ServiceId = ServiceId;
		$scope.taxAbleAmnt = data.Amount;//+ data.TotalTaxAmount;
		$scope.percentageColumn = flag;

		$scope.currentMaterialRow = index;
		//$scope.taxAbleAmnt = data.TransactionAmount;
		//$scope.taxAmnt = data.TaxAmount;

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
		//debugger
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
		//debugger
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
		//////debugger;
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

				//$scope.productNew.BaseCurrencyId = $scope.baseCurrencyId;
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
		//getServiceChargeList($scope.productNew.Id);
		angular.element(document.querySelector('#ServiceChargeTaxPopUp')).modal('hide');
	}


	//#endregion

	baseService.getCompanyConfiguration(function (result) {
		$scope.companyConfig = result;
    });

    $scope.companyGroupId = $window.companyGroupId; $scope.companyId = $window.companyId;


	cboService.getCboEntityByPlant($window.companyGroupId, $window.companyId, '', function (result) {
        try {
			$scope.EntityList = result;
			if ($scope.EntityList == null || $scope.EntityList.length == 0)
				ShowResult("CG: " + $window.companyGroupId + " COM: " + $window.companyId, "failure");
        } catch (e) {

        }
		
	});

	$scope.ReqList = [];
	$scope.ReqStatus = 'ForChecked';
	$scope.GetReq = function () {
		if ($scope.ReqStatus === 'ForChecked') {
			$scope.ReqStatus = 'ForChecked';
		}
		else {

		}
		//debugger
		$http({
			method: 'GET',
			url: 'Products/Requisition/GetAllReqdata?ReqStatus=' + $scope.ReqStatus
		}).then(function successCallback(response) {
			$scope.ReqList = response.data;
			for (var i = 0; i < $scope.ReqList.length; i++) {
				response.data[i].RequisitionDate = new Date($scope.ReqList[i].RequisitionDate);
			}
		});
	}
	$scope.GetReq();

	$scope.ReqList1 = [];
	$scope.ReqStatusApproval = 'HoldReject';
	$scope.GetReq1 = function () {

		//debugger
		if ($scope.ReqStatusApproval === 'HoldReject') {
			$scope.ReqStatusApproval = 'HoldReject';
		}
		else {

		}
		$http({
			method: 'GET',
			url: 'Products/Requisition/GetAllReqdata1?ReqStatusApproval=' + $scope.ReqStatusApproval
		}).then(function successCallback(response) {
			$scope.ReqList1 = response.data;
			for (var i = 0; i < $scope.ReqList1.length; i++) {
				response.data[i].RequisitionDate = new Date($scope.ReqList1[i].RequisitionDate);
			}
		});
	}
	// $scope.GetReq1();

	$scope.EmployeeList = [];
	$scope.GetEmployee = function () {
		//debugger
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
		if ($scope.productNew.RequisitionType === 'Capital')
			$scope.GLUrl1 = "Accounts/glitem/GetFixedAssetMasterGL";
		else
			$scope.GLUrl1 = "Accounts/glitem/GetExpenseRevenueGLBudgetActivity";

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
		//debugger
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

	$scope.EditDataDisplay = function (x, index) {
		//debugger
		getInventoryMaterialListById(x);
		angular.element(document.querySelector('#detailPopUpEditForModify')).modal('show');
	};
	$scope.EditDataDisplayClosed = function (x, index) {
		angular.element(document.querySelector('#detailPopUpEditForModify')).modal('hide');
	};
	function getInventoryMaterialListById(inveReveiveId) {
		$scope.masterId = inveReveiveId;
		//debugger
		$http.get($scope.path + 'GetInventoryMaterialListById?inveReveiveId=' + inveReveiveId)
			.then(function (response) {
				$scope.detailModel = response.data[0];
				$scope.detailModel.MaterialMasterId = response.data[0].MaterialMasterId;
				$scope.detailModel.ArticleId = response.data[0].ArticleId;
			});



	}
	$scope.detailUpdateBYId = function () {
		//debugger
		try {
			//$scope.validation();
			if ($scope.detailModel.MaterialMasterId == "" || $scope.detailModel.MaterialMasterId == null || $scope.detailModel.MaterialMasterId == "undefined") {
				$scope.detailModel.InventoryReceiveId = null;
				$scope.detailModel.FirstCharacteristicsId = null
				$scope.detailModel.FirstCharacteristicsValueId = null
				$scope.detailModel.SecondCharacteristicsId = null
				$scope.detailModel.SecondCharacteristicsValueId = null
				$scope.detailModel.ThirdCharacteristicsId = null
				$scope.detailModel.ThirdCharacteristicsValueId = null
				$scope.detailModel.CountryId = null

			}
			else {
				$scope.detailModel.InventoryReceiveId = $scope.productNew.Id;
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
			}

			$scope.detailModel.deliverydate = $scope.detailModel.deliverydate1;
			$scope.$broadcast('show-errors-check-validity');
			if ($scope.detailForm.$valid) {
				if ($scope.detailModel.MaterialMasterId == "" || $scope.detailModel.MaterialMasterId == null || $scope.detailModel.MaterialMasterId == "undefined") {
					$http({
						method: 'POST',
						url: $scope.updateByIdUrl,
						data: { entity: $scope.entity },
						dataType: 'JSON'
					}).then(function successCallback(response) {
						if (response.data.Error === true)
							ShowResult(response.data.Message, 'failure', 'detailPopUp');
						else {
							ShowResult(response.data.Message, 'success', 'detailPopUp');
							$scope.detailModel.Id = null;
							$scope.detailModel = {
								MaterialReqqusitionMasterId: $scope.productNew.Id

								, CurrencyName: angular.element("#currency :selected").text()
								, CurrencyId: $scope.productNew.CurrencyId
								, TotalAmount: 0
								, ToCurrencyRate: $scope.productNew.ToCurrencyRate
							};
							// $scope.taxCategoryList = [];
							getInventoryMaterialList($scope.productNew.Id);
							//$scope.getDataList();
							$scope.GetReq();
							//$scope.getalldata();
							$scope.clearCharNames();
						}
					}), function errorCallBack(response) {
						ShowResult(response.data.Message, 'failure', 'detailPopUp');
					};
				}
				else {
					if ($scope.invalid) {

						$http({
							method: 'POST',
							url: $scope.updateByIdUrl,
							data: { entity: $scope.entity },
							dataType: 'JSON'
						}).then(function successCallback(response) {
							if (response.data.Error === true)
								ShowResult(response.data.Message, 'failure', 'detailPopUpEditForModify');
							else {
								ShowResult(response.data.Message, 'success', 'detailPopUpEditForModify');
								$scope.detailModel.Id = null;
								$scope.detailModel = {
									MaterialReqqusitionMasterId: $scope.productNew.Id

									, CurrencyName: angular.element("#currency :selected").text()
									, CurrencyId: $scope.productNew.CurrencyId
									, TotalAmount: 0
									, ToCurrencyRate: $scope.productNew.ToCurrencyRate
								};
								// $scope.taxCategoryList = [];
								getInventoryMaterialList($scope.productNew.Id);
								//$scope.getDataList();
								$scope.GetReq();
								//$scope.getalldata();
								$scope.clearCharNames();
							}
						}), function errorCallBack(response) {
							ShowResult(response.data.Message, 'failure', 'detailPopUp');
						};
					}

				}

			}
		} catch (e) {
			//ShowResult(e, 'fail', 'detailPopUp');
		}
	};



	$scope.RequiredDateValidation = function () {
		//debugger
		if (new Date($scope.detailModel.deliverydate) < new Date($scope.productNew.RequisitionDate)) {
			ShowResult('Date must be above or equal to From Date', 'failure', 'detailPopUp');
			$scope.detailModel.deliverydate = "";
			//manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
		}
	}


	// #region Requisition Tab for Index Page

	$scope.ReqStatus = 'ForChecked';

	$scope.tab = 1;
	$scope.setTabReqList = function (newTab) {

		$scope.ReqStatus = 'ForChecked';
		$scope.GetReq();
		$scope.ReqListDetails();
		$scope.tab = newTab;

	};
	$scope.isSetReqList = function (tabNum) {
		return $scope.tab === tabNum;
	};
	$scope.setTabReqList1 = function (newTab) {
		//debugger

		$scope.ReqStatus = 'HoldReject';
		$scope.GetReq();
		$scope.ReqListDetails();
		$scope.tab = newTab;
	};
	$scope.isSetReqList1 = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabReqList2 = function (newTab) {
		$scope.tab = newTab;
		$scope.ReqStatus = 'Checked';
		$scope.GetReq();

	};
	$scope.isSetReqList2 = function (tabNum) {
		return $scope.tab === tabNum;
	};




	$scope.setTabReqApproved3 = function (newTab) {
		//debugger
		$scope.tab = newTab;
		$scope.ReqStatusApproval = 'HoldReject';
		$scope.GetReq1();

	};
	$scope.isSetReqApproved3 = function (tabNum) {
		return $scope.tab === tabNum;
	};
	$scope.setTabReqApproved4 = function (newTab) {
		$scope.tab = newTab;
		$scope.ReqStatusApproval = 'Approval';
		$scope.GetReq1();

	};
	$scope.isSetReqApproved4 = function (tabNum) {
		return $scope.tab === tabNum;
	};
	$scope.setTabReqApproved5 = function (newTab) {
		$scope.tab = newTab;
		//$scope.ReqStatusApproval = 'Approval';
		$scope.ReqListDetails1();		
	};
	$scope.isSetReqApproved5 = function (tabNum) {
		return $scope.tab === tabNum;
	};

	$scope.setTabReqApproved6 = function (newTab) {
		$scope.tab = newTab;
		//$scope.ReqStatusApproval = 'Approval';
		$scope.getRequisitionByEmpInMonth();
	};
	$scope.isSetReqApproved6 = function (tabNum) {
		return $scope.tab === tabNum;
	};
	// #endregion Requisition Tab


	//#region Requisition Print
	$scope.onClickGrid1 = function (args) {

		var gridObj = $("#Grid1").data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		//alert('jj' + data.Id);
		// $scope.valuePassInDelModal(data); 
		location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

	};
	$scope.command = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",

			click: $scope.onClickGrid1
		}
	}];


	$scope.onClickGrid2 = function (args) {

		var gridObj = $("#Grid2").data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		//alert('jj' + data.Id);
		// $scope.valuePassInDelModal(data); 
		location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

	};
	$scope.command1 = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",

			click: $scope.onClickGrid2
		}
	}];


	$scope.onClickGrid3 = function (args) {

		var gridObj = $("#Grid3").data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		//alert('jj' + data.Id);
		// $scope.valuePassInDelModal(data); 
		location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

	};
	$scope.command2 = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",

			click: $scope.onClickGrid3
		}
	}];


	$scope.onClickGridApproved4 = function (args) {

		var gridObj = $("#GridApproved4").data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		//alert('jj' + data.Id);
		// $scope.valuePassInDelModal(data); 
		location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

	};
	$scope.command3 = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",

			click: $scope.onClickGridApproved4
		}
	}];


	$scope.onClickGridApproved5 = function (args) {

		var gridObj = $("#GridApproved5").data("ejGrid");
		//getting corresponding record             
		var data = gridObj.getSelectedRecords()[0];
		//alert('jj' + data.Id);
		// $scope.valuePassInDelModal(data); 
		location.href = "Products/Requisition/RequisitionReportby?RequisitionId=" + data.Id;

	};
	$scope.command4 = [{
		type: "details", buttonOptions: {
			text: "Print",
			width: "50",
			height: "20",

			click: $scope.onClickGridApproved5
		}
	}];


	//#endregion


	$scope.GetLastPurchaseQtyList = [];
	$scope.GetLastPurchaseQtyGrid = function (x) {

		try {

			$http({
				method: 'POST',
				url: 'Products/InventoryCheckApproved/GetMaterialLastPOQty',
				data: {
					'materialMasterId': $scope.detailModel.MaterialMasterId,
					'Id': $scope.detailModel.ArticleId
					, 'Sku1': $scope.detailModel.FirstCharacteristicsId
					, 'Sku2': $scope.detailModel.SecondCharacteristicsId
					, 'Sku3': $scope.detailModel.SecondCharacteristicsId
				},
				dataType: 'JSON'
			}).then(function successCallback(response) {
				if (response.data.Error == true) {
					ShowResult(response.data.Message, 'failure');
				}
				else {
					$scope.GetLastPurchaseQtyList = response.data;
					var eDialog = $("#dialogListMaterialLastPOPrice").data("ejDialog");
					eDialog.open();
				}
			}, function errorCallback(response) {
				ShowResult(response.status.Message, 'failure');
			});
		} catch (e) {
			ShowResult(e, 'failure');
		}

	};
	$scope.MaterialLastPOPriceHide = function () {
		//$scope.taxCategoryList = [];
		angular.element(document.querySelector('#ListMaterialLastPOPrice')).modal('hide');
	};
	$scope.MaterialLastPOPrice = function (x) {
		//debugger
		$scope.GetLastPurchaseQtyGrid(x);
	};



	//#region Order Ref

	$scope.masterOrderCustomerList = [];
	$scope.GetMasterOrderByContractList = function () {
		//debugger
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/Requisition/GetMasterOrderList',
		}).then(function successCallback(response) {
			$scope.masterOrderCustomerList = response.data;
			//entrydata = copy(searchdata);

		});
		angular.element(document.querySelector('#MasterOrderPopUp')).modal('show');
	}

	$scope.SelectedOrder = function (obj) {
		//debugger
		//var data = obj.data.ContractId;
		$scope.productNew.OrderRefNo = obj.data.MasterOrderNo;
		angular.element(document.querySelector('#MasterOrderPopUp')).modal('hide');
	}
	$scope.ClearMasterOrder = function () {
		$scope.productNew.OrderRefNo = "";

	};

	$scope.CloseMasterOrder = function () {
		angular.element(document.querySelector('#MasterOrderPopUp')).modal('hide');

	};
	$scope.GetPopUpMasterOrderDetails = function () {
		//debugger
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/Requisition/GetMasterOrderDetailsList?MasterOrderId=' + $scope.productNew.OrderRefNo,
		}).then(function successCallback(response) {
			//$scope.productNew.masterOrderCustomerList = response.data;
			$scope.productNew.MasterOrderNo1 = response.data[0].MasterOrderNo;
			$scope.productNew.TotalQty1 = response.data[0].TotalQty;
			$scope.productNew.CustomerName1 = response.data[0].CustomerName;
			$scope.productNew.Contract1 = response.data[0].ContractNo;
			$scope.productNew.MasterLCNo1 = response.data[0].MasterLCNo;
			angular.element(document.querySelector('#MasterOrderPopUp1')).modal('show');

		});

	};
	$scope.CloseMasterOrder1 = function () {
		angular.element(document.querySelector('#MasterOrderPopUp1')).modal('hide');

	};


	//#endregion
	$scope.startDate1 = '';
	$scope.endDate1 = '';
	$scope.GetFiscalYear1 = function () {
		$http({
			method: 'GET',
			url: 'Products/Requisition/GetFiscalYear?formattedDate=' + $filter("dateFiltering")(Date.now()),
		}).then(function successCallback(response) {
			$scope.startDate1 = response.data[0].StartDate;
			$scope.endDate1 = response.data[0].EndDate;
			$scope.ReqListDetails1();
		});
		
	}
	$scope.GetFiscalYear1();
	$scope.empwiseDataList = [];
	$scope.ReqListDetails1 = function () {
		//debugger
		$http({
			method: 'GET',
			url: "Products/Requisition/LoadRequisitionMasterTotalEmpWise1?RequisitionId=" + 1 + '&startDate=' + $scope.startDate1 + '&endDate=' + $scope.endDate1,
		}).then(function successCallback(response) {
			//windowemp.lst = response.data;
			$scope.empwiseDataList = response.data;
			$scope.RequisitionId = response.data[0].RequisitionId;
			$scope.EmployeeName = response.data[0].EmployeeName;
			$scope.ReqTotalAmount = response.data[0].ReqTotalAmount;
			$scope.POTotalAmount = response.data[0].POTotalAmount;

		});
	}
	

	$scope.empwisemonthDataList = [];
	$scope.getRequisitionByEmpInMonth = function () {
		//debugger
		$http({
			method: 'GET',
			url: "Products/Requisition/RequisitionByEmpInMonth?RequisitionId=" + 1 + '&startDate=' + $filter("dateFiltering")(Date.now()) + '&endDate=' + $scope.endDate1,
		}).then(function successCallback(response) {
			//windowemp.lst = response.data;
			$scope.empwisemonthDataList = response.data;
			$scope.RequisitionId = response.data[0].RequisitionId;
			$scope.EmployeeName = response.data[0].EmployeeName;
			$scope.ReqTotalAmount = response.data[0].ReqTotalAmount;
			$scope.POTotalAmount = response.data[0].POTotalAmount;

		});
	}

	// New Article start by Omar
	$scope.getArticle = function (index) {
		$scope.itemIndex = index;
		//if (!baseService.isUndefinedOrNull($scope.itemList[$scope.itemIndex].MaterialMasterId) && !$scope.itemList[$scope.itemIndex].HasAttribute)
		//	return ShowResult('This material has no attribute', 'failure');
		// $scope.getArticleSearchList($scope.itemList[$scope.itemIndex].MaterialMasterId);
		$scope.getMaterialMasterWithArticle(null);
	};

	$scope.materialArticleList = [];
	$scope.InputMaterialArticlelistData = {};
	$scope.getMaterialMasterWithArticle = function (data) {
		$http({
			method: 'POST',
			url: 'Materials/MaterialMasterArticle/GetMaterialArticlePopUpData?type=' + $scope.materialType,
			data: { column: $scope.searchByMaterial, value: $scope.search },
			dataType: 'JSON',
		}).then(function successCallback(response) {
			$scope.materialArticleList = response.data;
		});
		$scope.InputMaterialArticlelistData = data;
		angular.element(document.querySelector('#materialarticleNewPopUp')).modal('show');

	};

	$scope.ArticleId = null;
	 //selectarticle setInputeMaterialArticleData
	$scope.setInputeMaterialArticleData = function (ob) {
		try {
			$scope.detailModel.MaterialMasterId = ob.data.MaterialMasterId;
			$scope.detailModel.MaterialMasterName = ob.data.MaterialMasterName;
			$scope.detailModel.ArticleId = ob.data.Id;
			$scope.detailModel.ArticleName = ob.data.StandardName;
			$scope.detailModel.TransactionUoMId = ob.data.BaseUOMId;
			angular.element(document.querySelector('#materialarticleNewPopUp')).modal('hide');

			$scope.mmChangeFlag = true;
			GetArticleAlias();
		} catch (e) {
			ShowResult(e, '', 'articleSearchPop');
		}
	};

	


	function GetArticleAlias() {
		$http.get("Materials/materialmasterarticle/getArticleAliaslist?articleId=" + $scope.detailModel.ArticleId)
			.then(function (response) {
				$scope.CustomerArticle = response.data;
				//$scope.CustomerArticle = response.data[0].ArticlePartyName;
			});
	}

	//End Article
}