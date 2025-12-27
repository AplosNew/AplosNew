'use strict';
inventoryIssueController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function inventoryIssueController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
	$rootScope.title = "Inventory Issue";
	$scope.Action = 'Save';
	$scope.index = -1;
	$scope.products = [];
	$scope.path = 'Products/InventoryIssue/';
	$scope.getListUrl = $scope.path + 'GetDataByInventoryIssue';
	$scope.saveUrl = $scope.path + 'create';
	$scope.updateUrl = $scope.path + 'UpdateIssueMaster';
	//$scope.updateUrl = $scope.path + 'edit';
	$scope.deleteUrl = $scope.path + 'IssueDetailDelete/';
	$scope.currentDate = new Date(Date.now());
	$scope.ispostDisable = false;
	$controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
	$controller("employeeBaseController", { $scope: $scope, $http: $http });

	$scope.searchByIssue = "IssueNo"; $scope.searchIssue = "";
	$scope.searchByIssueList = [
		{
			value: 'IssueNo'
			, name: 'Issue No'
		},
		{
			value: 'MaterialStorage'
			, name: 'Storage Location'
		},
		{
			value: 'IssueDate'
			, name: 'Issue Date'
		}
	];
	
	
	baseService.init($scope.getListUrl, null, null, 'DESC', 'IssueDate', 'IssueNo');

	$scope.GridInventoryIssuedata = [];
	$scope.getdataInventoryIssue = function () {
		$http({
			method: 'POST',
			url: 'Products/InventoryIssue/GetDataByInventoryIssue',
			data: { column: $scope.searchByIssue, value: $scope.searchIssue },
			dataType: 'JSON',
		}).then(function successCallback(response) {
			$scope.GridInventoryIssuedata = response.data;
		});
	};
	$scope.getdataInventoryIssue();


	$http({
		method: 'GET',
		url: 'Materials/MaterialStorage/getcbo'
	}).then(function (response) {
		$scope.storageList = response.data;
	});
	$scope.product = {
		Id: null
		, ComapnyGroupId: null
		, CompanyId: null
		, PlantId: null
		, PlantName: null
		, EntityId: null
		, EntityName: null
		, MaterialStorageId: null
		, IssueDate: null
		, Remarks: null
		, EmployeeId: null
		, EmployeeName: null
		, IssueType: 'Revenue'
		, IssueRequestMasterId: null
		, SlipAssetIssueTypeStatus: 'Asset'
		, OrderRefNo:null
		, RefferenceNo: null
		, OrderSpecific: 'No'
		, OrderSpecific1: 'No'
		, ConsumptionBookingName: null
		, ProductionOrderId: null
		, ContractNo: null
		, ContractId: null
		, ProcessName: null
		, IsPark:true
		
	};
	$scope.IssueType = 'Revenue';
	$scope.productNew = Object.assign({}, $scope.product);

	$scope.changeType = function (data) {
		$scope.IssueType = data;
	}

	$scope.Get = function (index) {
		//debugger;
		$scope.index = index;
		$scope.product = $scope.issueList[index];
		$scope.productNew = Object.assign({}, $scope.product);
		$scope.materialStockList = [];
		$scope.specificStockList = [];
		
		getIssueDetailList();

		if (!$rootScope.isCollapsed) $rootScope.toggle();
	};
	$scope.GetDataList = function ($event) {
		//debugger;

		//$scope.index = index;
		// $scope.product = $scope.issueList[index.rowIndex];
		var a = $event;
		var id = a.data;
		$scope.product = a.data;
		$scope.productNew = Object.assign({}, $scope.product);
		$scope.productNew.IssueType = a.data.issuetype;
		$scope.materialStockList = [];
		$scope.specificStockList = [];
		$scope.ispostDisable = true;
		getIssueDetailList();
		if (!baseService.isUndefinedOrNull(a.data.OrderRefNo) || !baseService.isUndefinedOrNull(a.data.ContractId) || !baseService.isUndefinedOrNull(a.data.ProductionOrderId)) {
			$scope.productNew.OrderSpecific = 'Yes';
		}
		$scope.GetEntityWiseConsumptionList();
		if (!$rootScope.isCollapsed) $rootScope.toggle();
	};




	$scope.AllTabPrint = function (z) {
		//debugger;
		var x = "#" + z;
		var gridObj = $(x).data("ejGrid");
		var data = gridObj.getSelectedRecords()[0];
		location.href = "Products/InventoryIssue/IssueReport?grnId=" + data.Id;

	};

	$scope.ConfirmIssueReportPrint = function (data) {
		try {
	//		$scope.PrintTabId = data.JWContractId;
			$scope.IssueId = data.Id;
			var reportFormat = "Excel";
			window.open('OutSourcing/OSIssueReturn/GetIIPrintReport?reportFormat=' + reportFormat + '&IssueId=' + $scope.IssueId, '_blank');

		} catch (e) {

		}
	};


	//$scope.SavePOPUpConfirm = function () {
	//    $scope.message_confirmation = "Are you sure want to do Auto Issue?";
	//    angular.element(document.querySelector('#confirmSavePopUp')).modal('show');
	//};

	$scope.Save = function () {
		var sumOfmaterialStockList = $filter('sumByKey')($filter('filter')($scope.specificStockList), 'RequisitionQty');
		$scope.selectedRowQty1 = $filter('sumByKey')($filter('filter')($scope.detailList), 'TransactionQty');
		if (sumOfmaterialStockList < $scope.selectedRowQty1) {
			ShowResult("Please select specific GRN", 'failure');
			return false;
		}
		if ($scope.detailList.length === 0) {
			ShowResult('Please select Atlest one material');
			return false;
		}
		if ($scope.productNew.OrderSpecific === 'Yes' && $scope.productNew.IssueType === 'Capital') {
			ShowResult('You can not select issue type Capital');
			return false;
		}
		var totalBaseCurrencyRate = $filter('sumByKey')($filter('filter')($scope.specificStockList), 'BaseCurrencyRate');
		if (totalBaseCurrencyRate > 0) {
			$scope.productNew.IsPostingRequired = true;
		}
		else {
			$scope.productNew.IsPostingRequired = false;
		}
		var UIStatus = $("#SlipAssetIssueUI").val();
		$scope.productNew.IssueRequestMasterId = $scope.issueId;
		$scope.ispostDisable = true;

		

		if ($scope.Action === "Save") {
			$http({
				method: 'POST'
				, url: $scope.saveUrl
				, data: {
					entities: JSON.stringify($scope.detailList)//$scope.detailList
					, specificStockList: JSON.stringify($scope.specificStockList)// $scope.specificStockList
					, inventoryIssue: $scope.productNew
					, IssueTypeStatus: UIStatus
					, entitiesAll: JSON.stringify($scope.detailList)//$scope.detailListNewAll
				}				
				, dataType: 'JSON'
			}).then(function (response) {
				if (response.data.Error === true) {
					$scope.ispostDisable = false;
					ShowResult(response.data.Message, 'failure');
                }
				else {
					ShowResult(response.data.Message, 'success');
					$scope.ispostDisable = true;
					$scope.Clear();
					$scope.getdataInventoryIssue();
					$scope.productNew.Id = response.data.inventoryIssue.Id;
					$scope.getData();
					$scope.GetDataList();
				}
			}), function (response) {
				ShowResult(response.data.Message, 'failure');
			};
		}
		else ShowResult('Please issue material', 'failure');
	};
	$scope.Update = function () {
			$http({
				method: 'POST'
				, url: $scope.updateUrl
				, data: {
					 inventoryIssue: $scope.productNew
				}
				, dataType: 'JSON'
			}).then(function (response) {
				if (response.data.Error === true) {
					$scope.ispostDisable = false;
					ShowResult(response.data.Message, 'failure');
				}
				else {
					ShowResult(response.data.Message, 'success');
					$scope.ispostDisable = true;
					$scope.Clear();
					$scope.getdataInventoryIssue();
					$scope.productNew.Id = response.data.inventoryIssue.Id;
					$scope.getData();
					$scope.GetDataList();
				}
			}), function (response) {
				ShowResult(response.data.Message, 'failure');
			};
    }
	

	$scope.SaveSlipIssue = function () {
		var sumOfmaterialStockList = $filter('sumByKey')($filter('filter')($scope.specificStockList), 'RequisitionQty');
		$scope.selectedRowQty1 = $filter('sumByKey')($filter('filter')($scope.detailList), 'TransactionQty');
		if (sumOfmaterialStockList < $scope.selectedRowQty1) {
			ShowResult("Please select specific GRN", 'failure');
			return false;
		}

		var UIStatus = $("#SlipAssetIssueUI").val();
		if (UIStatus === 'Asset') {
			if ($scope.materialStockList.length === 0) {
				ShowResult('Please select Specific GRN');
				return false;
			}
		}
		//debugger;
		if ($scope.detailList.length === 0) {
			ShowResult('Please select Atlest one material');
			return false;
		}
		$scope.detailListNew = [];
		$scope.detailListNewAll = [];
		var check = false;
		//debugger;
		for (var i = 0; i < $scope.detailList.length; i++) {
			if ($scope.detailList[i].check === true) {

				check = true;

				if ($scope.detailList[i].TransactionQty > Math.round(($scope.detailList[i].PostingQty + $scope.detailList[i].IssuedQty) * 100 + Number.EPSILON) / 100) {
					ShowResult("Issue qty can not gaterthen  Ready for issue Qty");
					return false;
				}
				//if ($scope.detailList[i].TransactionQty > $scope.detailList[i].BalanceQty) {
				//	ShowResult("Issue qty can not gaterthen  Balance Qty");
				//	return false;
				//}
				if ($scope.detailList[i].check === true && baseService.isUndefinedOrNull($scope.detailList[i].TransactionQty)) {
					ShowResult("Enter the Qty");
					return false;
				}
				if ($scope.detailList[i].check === false && !baseService.isUndefinedOrNull($scope.detailList[i].TransactionQty)) {
					ShowResult("Enter the Qty");
					return false;
				}
				if ($scope.detailList[i].check === true && $scope.detailList[i].TransactionQty === 0) {
					ShowResult("Enter the Qty");
					return false;
				}
				if ($scope.detailList[i].check === false && $scope.detailList[i].TransactionQty === 0) {
					ShowResult("Enter the Qty");
					return false;
				}
				$scope.detailListNewAll.push($scope.detailList[i]);
				if ($scope.detailList[i].check === true) {
					$scope.detailList[i].TransactionQty = Math.round($scope.detailList[i].TransactionQty * 100 + Number.EPSILON) / 100;
					var getRow1 = $filter("filter")($scope.detailListNew, { "MaterialMasterId": $scope.detailList[i].MaterialMasterId, "ArticleId": $scope.detailList[i].ArticleId, "FirstCharacteristicsValueId": $scope.detailList[i].FirstCharacteristicsValueId, "SecondCharacteristicsValueId": $scope.detailList[i].SecondCharacteristicsValueId, "ThirdCharacteristicsValueId": $scope.detailList[i].ThirdCharacteristicsValueId, "check": true });


					if (getRow1.length === 0) {
						$scope.detailList[i].RequisitionQty= Math.round($scope.detailList[i].TransactionQty * 100 + Number.EPSILON) / 100;
						$scope.detailListNew.push($scope.detailList[i])
						//$scope.detailListNew.RequisitionQty = Math.round($scope.detailList[i].TransactionQty * 100 + Number.EPSILON) / 100;
					}
					else {
						for (var i1 = 0; i1 < $scope.detailListNew.length; i1++) {

							if ($scope.detailListNew[i1].MaterialMasterId === $scope.detailList[i].MaterialMasterId
								&& $scope.detailListNew[i1].ArticleId === $scope.detailList[i].ArticleId
								&& $scope.detailListNew[i1].FirstCharacteristicsValueId === $scope.detailList[i].FirstCharacteristicsValueId
								&& $scope.detailListNew[i1].SecondCharacteristicsValueId === $scope.detailList[i].SecondCharacteristicsValueId
								&& $scope.detailListNew[i1].ThirdCharacteristicsValueId === $scope.detailList[i].ThirdCharacteristicsValueId
							) {
								$scope.detailListNew[i1].RequisitionQty += Math.round($scope.detailList[i].TransactionQty * 100 + Number.EPSILON) / 100;;


							}

						}

					}

					
				}	
			}

		}

		if (check == false) {
			ShowResult('Check at lest one checkbox from Issue slip details');
			return false;
		}

		$scope.productNew.IsPostingRequired = true;
		$scope.productNew.IssueRequestMasterId = $scope.issueId;

		if ($scope.Action === "Save") {
			$http({
				method: 'POST'
				, url: $scope.saveUrl
				, data: {
					entities: JSON.stringify($scope.detailListNew)//$scope.detailListNew
					, specificStockList: JSON.stringify($scope.specificStockList)//$scope.specificStockList
					, inventoryIssue: $scope.productNew
					, IssueTypeStatus: UIStatus
					, entitiesAll: JSON.stringify($scope.detailListNewAll)//$scope.detailListNewAll

				}
				, dataType: 'JSON'
			}).then(function (response) {
				if (response.data.Error === true)
					ShowResult(response.data.Message, 'failure');
				else {
					ShowResult(response.data.Message, 'success');
					$scope.Clear();
					$scope.getData();
					$scope.productNew.Id = response.data.inventoryIssue.Id;
				}
			}), function (response) {
				ShowResult(response.data.Message, 'failure');
			};
		}
		else ShowResult('Please issue material', 'failure');
	};

	$scope.Clear = function () {
		ClearFields();
		return true;
	};

	function ClearFields() {
		$scope.Action = "Save";
		$scope.product = {};
		$scope.productNew = { FixedAssetOrInventory: 'Inventory', PODepended: false, AlongwithInvoice: false, IssueType: 'Revenue' };
		$scope.detailModel = {};
		$scope.clearCharNames();
		$scope.detailList = [];
		$scope.specificStockList = [];
		$scope.materialStockList = [];
		$scope.IssueType = 'Revenue';
		$scope.productNew.OrderSpecific = 'No';
		$scope.ispostDisable = false;
	}

	// #region Details

	$scope.detailPopUp = function () {
		//debugger;
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

				, TransactionQty: 0
				, TransactionUoMId: null
				, TransactionUoM: null
				, BaseQty: 0
				, BaseUOMId: null
				, BaseUoM: null
				, BaseUoMFactor: 0
				, TransactionRate: 0
				, TotalQty: 0
				, AvgRate: 0

				, InventoryIssueId: $scope.productNew.Id
				, AvgAmount: 0
				, PolicyRate: 0
				, PolicyAmount: 0
				, Policy: null
				, ActivityName: null
				, BudgetMasterId: null
				, ActivityId: null
				, IssueId: null
				, CostCenterId: null
				, CountryName: null
				, IsSpecific: false
				, Comments: null

			};
			$scope.clearCharNames();
			$scope.detailModel.CostCenterId = $scope.CostCenterIdTemp;
			angular.element(document.querySelector('#detailPopUp')).modal('show');
		}
		$scope.CostCenterLoadNew();
	};
	$scope.closeDetaiPopUp = function () {
		//debugger;
		$scope.CostCenterIdTemp = $scope.detailModel.CostCenterId;
		$scope.detailModel = {};
		$scope.clearCharNames();
		angular.element(document.querySelector('#detailPopUp')).modal('hide');
	};
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

	//$scope.CountryLoadData();


	$scope.materialType = ['Asset', 'Consumable', 'Spare', 'RawMaterial'];
	//$scope.setMaterialMasterData
	$scope.selectMaterialByType = function (ob) {
		//debugger;
		//if (ob.IsAsset) return ShowResult('Fixed Asset  can not Issue through this Screen .', '', 'materialMasterbyTypePopup');
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
		$scope.detailModel.IsReplacement = ob.IsReplacement;
		$scope.detailModel.Replacement = ob.Replacement;
		$scope.detailModel.TransactionUoMId = ob.BaseUOMId;
		$scope.detailModel.ArticleId = null;;
		$scope.detailModel.ArticleName = null;
		$scope.detailModel.FirstCharacteristicsValueId = null;
		$scope.detailModel.SecondCharacteristicsValueId = null;
		$scope.detailModel.ThirdCharacteristicsValueId = null;
		$scope.detailModel.FirstCharacteristicsId = null;
		$scope.detailModel.SecondCharacteristicsId = null;
		$scope.detailModel.ThirdCharacteristicsId = null;
		$scope.char1.FreeText = null;
		$scope.char2.FreeText = null;
		$scope.char1.CharacteristicsId = null;
		$scope.char1.FirstCharacteristicsValueId = null;
		$scope.char2.CharacteristicsId = null;
		$scope.char2.ThirdCharacteristicsValueId = null;
		

		$scope.detailModel.IsOriginApplicable = ob.IsOriginApplicable;

		$scope.hasArticle = ob.HasAttribute;
		$scope.hasSku = ob.WithSKU;
		if (ob.HasAttribute) $scope.getArticleSearchList(ob.Id);
		if (ob.WithSKU) $scope.getCharacteristicsList(ob.Id);
		if (!ob.HasAttribute && !ob.WithSKU)
			getMaterialStock();
		$scope.CountryLoadData();

		var mmId = []; mmId.push(ob.Id);
		cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (result) {
			$scope.uoMList = result;
		});
		manualValidation('div_mm', false);
		manualValidation('div_qty', false);
		if ($scope.IssueType == 'Revenue') {
			if (!ob.HasAttribute && !ob.WithSKU) $scope.getBudgetActivityInIssueMaterial(ob.MaterialGroupMasterId);
		}
		$scope.closeMaterialMasterbyTypePopUp();
	};



	$scope.selectarticle = function (ob) {
		//debugger;
		try {
			$scope.detailModel.ArticleId = ob.Id;
			$scope.detailModel.ArticleName = ob.StandardName;
			manualValidation('div_ar', false);
			if (!ob.WithSKU)
				getMaterialStock();
			$scope.CountryLoadData();
			if ($scope.IssueType == 'Revenue') {
				if (!ob.WithSKU)
					$scope.getBudgetActivityInIssueMaterial($scope.detailModel.MaterialGroupMasterId);
			}
			angular.element(document.querySelector('#articleSearchPop')).modal('hide');
		} catch (e) {
			ShowResult(e, '', 'articleSearchPop');
		}
	};
	//Need to modify this method for all issue
	$scope.setCharData = function (data) {
		$scope[$scope.charValueSearchFor].CharacteristicsValueId = data.CharacteristicsValueId;
		$scope[$scope.charValueSearchFor].FreeText = data.UserName;
		$scope[$scope.charValueSearchFor].FlagDisable = $scope.isSearch;
		if ($scope.charValueSearchFor === 'char1') {
			$scope.detailModel.FirstCharacteristicsValueId = data.CharacteristicsValueId;
			$scope.detailModel.FirstCharacteristicsId = data.CharacteristicsId;
		}
		if ($scope.charValueSearchFor === 'char2') {
			$scope.detailModel.SecondCharacteristicsValueId = data.CharacteristicsValueId;
			$scope.detailModel.SecondCharacteristicsId = data.CharacteristicsId;
		}
		if ($scope.charValueSearchFor === 'char3') {
			$scope.detailModel.ThirdCharacteristicsValueId = data.CharacteristicsValueId;
			$scope.detailModel.ThirdCharacteristicsId = data.CharacteristicsId;
		}
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
	$scope.detailAdd = function () {
		//debugger;
		try {
			$scope.validation();
			if ($scope.detailModel.BudgetMasterId === '' || $scope.detailModel.BudgetMasterId === null || $scope.detailModel.BudgetMasterId === undefined) {
				ShowResult('Budget is required', 'failure', 'detailPopUp');
				return false;
			}
			if ($scope.detailModel.CostCenterId === '' || $scope.detailModel.CostCenterId === null || $scope.detailModel.CostCenterId === undefined) {
				ShowResult('Cost center is required', 'failure', 'detailPopUp');
				return false;
			}
			if ($scope.detailModel.ActivityId === '' || $scope.detailModel.ActivityId === null || $scope.detailModel.ActivityId === undefined) {
				ShowResult('Activity is required', 'failure', 'detailPopUp');
				return false;
			}
			if ($scope.detailModel.IsOriginApplicable === true) {
				if (baseService.isUndefinedOrNull($scope.detailModel.CountryId)) {
					ShowResult('Please select the country', 'failure', 'detailPopUp');
					return false;
				}
			}
			if (!baseService.isUndefinedOrNull($scope.char1.CharacteristicsId)) {
				if ($scope.char1.CharacteristicsId.length > 0) {
					if (baseService.isUndefinedOrNull($scope.char1.FreeText)) {
						ShowResult('Please select the Sku1', 'failure', 'detailPopUp');
						return false;
					}
				}
			}
			if (!baseService.isUndefinedOrNull($scope.char2.CharacteristicsId)) {
				if ($scope.char2.CharacteristicsId.length > 0) {
					if (baseService.isUndefinedOrNull($scope.char2.FreeText)) {
						ShowResult('Please select the Sku2', 'failure', 'detailPopUp');
						return false;
					}
				}
			}
			if (!baseService.isUndefinedOrNull($scope.char3.CharacteristicsId)) {
				if ($scope.char3.CharacteristicsId.length > 0) {
					if (baseService.isUndefinedOrNull($scope.char3.FreeText)) {
						ShowResult('Please select the Sku3', 'failure', 'detailPopUp');
						return false;
					}
				}
			}
			if (baseService.isUndefinedOrNull($scope.detailModel.TransactionUoMId)) {
						ShowResult('Please select UOM', 'failure', 'detailPopUp');
						return false;
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
					var tQty = Math.round(parseFloat($scope.detailModel.TransactionQty) * parseFloat($.grep($scope.uoMList, function (item) { return item.Value === $scope.detailModel.TransactionUoMId; })[0].BaseUoMFactor) * 10000 + Number.EPSILON) / 10000;
					if (tQty > parseFloat($scope.detailModel.PostingQuantity))
						throw 'Issue qty must be less than or equal Ready for Issue Qty.';
					$scope.detailModel.BaseQty = tQty;
				}
			}
			if ($scope.detailModel.FirstCharacteristicsValueId === undefined)
				$scope.detailModel.FirstCharacteristicsValueId = null;
			if ($scope.detailModel.SecondCharacteristicsValueId === undefined)
				$scope.detailModel.SecondCharacteristicsValueId = null;
			if ($scope.detailModel.ThirdCharacteristicsValueId === undefined)
				$scope.detailModel.ThirdCharacteristicsValueId = null;
			if ($scope.detailModel.CountryId === undefined)
				$scope.detailModel.CountryId = null;
			for (var i = 0; i < baseService.arrayLength($scope.detailList); i++) {
				if ($scope.detailList[i].FirstCharacteristicsValueId === undefined)
					$scope.detailList[i].FirstCharacteristicsValueId = null;
				if ($scope.detailList[i].SecondCharacteristicsValueId === undefined)
					$scope.detailList[i].SecondCharacteristicsValueId = null;
				if ($scope.detailList[i].ThirdCharacteristicsValueId === undefined)
					$scope.detailList[i].ThirdCharacteristicsValueId = null;
				//if ($scope.detailList[i].CountryName === undefined)
				//	$scope.detailList[i].CountryName = null;

				if ($scope.detailList[i].MaterialMasterId === $scope.detailModel.MaterialMasterId &&
					$scope.detailList[i].ArticleId === $scope.detailModel.ArticleId &&
					$scope.detailList[i].FirstCharacteristicsValueId === $scope.detailModel.FirstCharacteristicsValueId &&
					$scope.detailList[i].SecondCharacteristicsValueId === $scope.detailModel.SecondCharacteristicsValueId &&
					$scope.detailList[i].ThirdCharacteristicsValueId === $scope.detailModel.ThirdCharacteristicsValueId 					
				)/*&&$scope.detailList[i].CountryId === $scope.detailModel.CountryId*/
					throw 'This material already issued.';
			}
			if (!$scope.detailModel.SKU1) {
				$scope.detailModel.FirstCharacteristicsId = $scope.char1.CharacteristicsId;
				$scope.detailModel.FirstCharacteristicsValueId = $scope.char1.CharacteristicsValueId;
				$scope.detailModel.FirstCharacteristicText = $scope.char1.FreeText;
			}
			if (!$scope.detailModel.SKU2) {
				$scope.detailModel.SecondCharacteristicsId = $scope.char2.CharacteristicsId;
				$scope.detailModel.SecondCharacteristicText = $scope.char2.FreeText;
				$scope.detailModel.SecondCharacteristicsValueId = $scope.char2.CharacteristicsValueId;
            }
			

			$scope.detailModel.ThirdCharacteristicsId = $scope.char3.CharacteristicsId;
			$scope.detailModel.ThirdCharacteristicText = $scope.char3.FreeText;
			$scope.detailModel.ThirdCharacteristicsValueId = $scope.char3.CharacteristicsValueId;


			$scope.detailModel.IssueDate = $scope.productNew.IssueDate;
			$scope.detailModel.Remarks = $scope.productNew.Remarks;
			$scope.detailModel.EmployeeId = $scope.productNew.EmployeeId;
			$scope.detailModel.CountryId = $scope.detailModel.CountryId;
			$scope.detailModel.CountryName = $scope.detailModel.CountryName;
			$scope.detailModel.LotNumber = '';
			$scope.detailModel.IsSpecific = false;
			$scope.detailModel.BaseUoMFactor = $.grep($scope.uoMList, function (item) { return item.Value === $scope.detailModel.TransactionUoMId; })[0].BaseUoMFactor;
			$scope.detailModel.TransactionUoM = angular.element("#issueUoM :selected").text();
			if (baseService.isUndefinedOrNull($scope.detailModel.AvgAmount)) {
				$scope.detailModel.AvgAmount = 0;
			}
			if (baseService.isUndefinedOrNull($scope.detailModel.AvgRate)) {
				$scope.detailModel.AvgRate = 0;
			}
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
	};

	function getMaterialStock() {
		$http({
			method: 'POST',
			url: $scope.path + 'GetStock',
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

	$scope.selectIndependentcharacteristics = function (x) {
		//debugger;
		try {
			if (!$scope.hasSku) {
				var tempSKU = x.data;
				$scope.detailModel.FirstCharacteristicsId = tempSKU.CharacteristicsId;
				$scope.detailModel.FirstCharacteristicsValueId = tempSKU.Value;
				$scope.detailModel.SKU1 = tempSKU.Text;
				$scope.detailModel.FirstCharacteristicText = tempSKU.Text;
				getMaterialStock();
            }
		
			angular.element(document.querySelector('#searchIndependentcharacterepopup')).modal('hide');
		} catch (e) {
			ShowResult(e, '', 'searchIndependentcharacterepopup');
		}
	};
	$scope.selectIndependentSKU2 = function (x) {
		//debugger;
		try {
			if (!$scope.hasSku) {
				var tempSKU = x.data;
				$scope.detailModel.SecondCharacteristicsId = tempSKU.CharacteristicsId;
				$scope.detailModel.SecondCharacteristicsValueId = tempSKU.Value;
				$scope.detailModel.SKU2 = tempSKU.Text;
				$scope.detailModel.SecondCharacteristicText = tempSKU.Text;
				getMaterialStock();
			}

			angular.element(document.querySelector('#searchIndependentSKU2popup')).modal('hide');
		} catch (e) {
			ShowResult(e, '', 'searchIndependentSKU2popup');
		}
	};
	$scope.IndependentcharacteristicsList = [];
	$scope.getIndependentCharacteristicsList = function () {
		$http({
			method: 'GET',
			url: 'Materials/MaterialMaster/GetCharacteristicsWithoutMaterialSKU1/',
		}).then(function (response) {
			$scope.IndependentcharacteristicsList = [];
			$scope.IndependentcharacteristicsList = response.data.charData;
		});
		angular.element(document.querySelector('#searchIndependentcharacterepopup')).modal('show');

	};

	$scope.IndependentSKU2List = [];
	$scope.getIndependentSKU2List = function () {
		$http({
			method: 'GET',
			url: 'Materials/MaterialMaster/GetCharacteristicsWithoutMaterialSKU2/',
		}).then(function (response) {
			$scope.IndependentSKU2List = [];
			$scope.IndependentSKU2List = response.data.charData;
		});
		angular.element(document.querySelector('#searchIndependentSKU2popup')).modal('show');

	};

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
				, url: $scope.deleteUrl + '?issueDetailId=' + $scope.delData.Id + '&voucherId=' + $scope.delData.VoucherId
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
		$scope.detailList.splice($scope.popUpIndex, 1);
		$scope.delData = null;
		$scope.popUpIndex = -1;
		angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
	};

	function getIssueDetailList() {
		$http.get($scope.path + 'GetIssueDetailByIssueId?issueId=' + $scope.productNew.Id)
			.then(function (response) {
				$scope.detailList = response.data;
				$scope.detailModel.IssueId = $scope.detailList[0].InventoryIssueId;
			});
	}

	// #endregion Details

	// #region Specific Stock

	$scope.materialStockList = [];
	$scope.specificStockList = [];
	$scope.getSpecificMaterialStock = function (data, index) {
		//debugger;
		$scope.index = index;
		$scope.selectedRowQty = data.TransactionQty;
		$scope.productNew.Id = null;
		$http({
			method: 'POST'
			, url: $scope.path + 'GetSpecificMaterialStock'
			, data: { entity: data, issueDate: $scope.productNew.IssueDate }
			, dataType: 'JSON'
		}).then(function (response) {
			$scope.materialStockList = response.data;

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
			angular.element(document.querySelector('#stockPopUp')).modal('show');
		}), function (response) {
			ShowResult(response.data.Message, 'failure'); 
		};
		

	};

	$scope.addMaterialStock = function () {
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
				if (($scope.materialStockList[t1].IssueByUoM === 'Yes' && $scope.materialStockList[t1].Flag === true) && ($scope.materialStockList[t1].TransactionUoMId != $scope.materialStockList[t1].IssueTransactionUoMId)) {
					ShowResult("Your Transaction UoM is not equal to requested UoM.So you can not issue this material", 'failure', 'stockPopUp');
					return false;
				}
				if ($scope.materialStockList[t1].RequisitionQty > 0 && $scope.materialStockList[t1].Flag==0) {
					ShowResult("select The given qty row", 'failure', 'stockPopUp');
					return false;
				}
				if (baseService.isUndefinedOrNull($scope.materialStockList[t1].RequisitionQty) && $scope.materialStockList[t1].Flag == 1 ) {
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
				nRow.BaseIssueQty =$scope.materialStockList[n].BaseIssueQty;
				if (!baseService.valueCheckInList($scope.specificStockList, 'InventoryReceiveDetailId', nRow.InventoryReceiveDetailId) && nRow.Flag)
					$scope.specificStockList.push(nRow);
			}
			angular.element(document.querySelector('#stockPopUp')).modal('hide');
			CloseModalShowResult();
		} catch (e) {
			ShowResult(e, 'failure', 'stockPopUp');
		}
	};

	$scope.calculateBaseQty = function (data) {
		var BaseIssueQtynew = parseFloat(data.BaseUoMFactor * data.RequisitionQty).toFixed(4);
		if (BaseIssueQtynew > data.BalanceStock) {
			ShowResult('Issue Qty can not grater than Balance Qty', 'failure', 'stockPopUp');
			data.RequisitionQty = 0;
			data.Flag = 0;
			return false;
		}
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
					if (list[i].TransactionUoMId !== list[i].BaseUOMId)
						totalQty += (Math.round((list[i].RequisitionQty * list[i].BaseUoMFactor) * 100 + Number.EPSILON) / 100);
					else totalQty += (Math.round((list[i].RequisitionQty) * 100 + Number.EPSILON) / 100);
					
				}
			}
		}
		var qty = parseFloat($scope.detailList[$scope.index].TransactionQty) * parseFloat($scope.detailList[$scope.index].BaseUoMFactor);
		//if (totalQty > qty && qty !== totalQty) throw 'Issue qty can\'t over ' + qty + ' .';
		//if (totalQty < qty && qty !== totalQty) throw 'Issue qty can\'t less ' + qty + ' .';

	}

	// #endregion Specific Stock

	// #region Machine Inventory Issue




	// #endregion Machine Inventory Issue

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

	//$scope.PostingStockList = [];
	//$scope.getPostingStock = function (data) {
	//    $http({
	//        method: 'POST'
	//        , url: $scope.path + 'GetPostingStockDetail'
	//        , data: { entity: data, issueDate: $scope.productNew.IssueDate }
	//        , dataType: 'JSON'
	//    }).then(function (response) {
	//        $scope.PostingStockList = response.data;
	//        angular.element(document.querySelector('#PostingStockPopUp')).modal('show');
	//    }), function (response) {
	//        ShowResult(response.data.Message, 'failure');
	//    };
	//};
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

	//$scope.redirectTab = function () {
	//    if ($scope.tabForm1.$invalid) {
	//        $scope.setTab(1);
	//    }
	//    else if ($scope.tabForm2.$invalid) {
	//        $scope.setTab(2);
	//    }
	//};
	$scope.IssueReport = function (data) {
		location.href = "Products/InventoryIssue/IssueReport?grnId=" + data.Id;
	};




	$scope.closeEmployeePopUp = function () {
		if ($scope.employeeIndex !== -1) {
			var employee = $scope.employeeList[$scope.employeeIndex];
			$scope.productNew.EmployeeName = employee.EmployeeName;
			$scope.productNew.EmployeeId = employee.SystemId;
		}
		$scope.hideEmployeePopUp();
	};

	$scope.hideEmployeePopUp = function () {
		angular.element(document.querySelector("#employeePopUp")).modal("hide");
	};

	$scope.clearEmployee = function () {
		$scope.productNew.EmployeeName = null;
		$scope.productNew.EmployeeId = null;
	};



	$scope.setSelected = function (data) {
		//debugger;
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
	$scope.GetCOAICodeList = function () {
		$scope.GLUrl1 = "Accounts/glitem/GetExpenseTypeGLBudgetActivityList";
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
		if ($scope.productNew.IssueType === 'Capital')
			angular.element(document.querySelector("#IssueAUCGLPopUp")).modal("hide");
		else
			angular.element(document.querySelector("#GLPopUp")).modal("hide");
	};


	$scope.setissueAUCglSelected = function (data) {
		$scope.addissueAUCglRow(data);
		$scope.closeIssueAUCglListPopUp();
	};

	$scope.addissueAUCglRow = function (data) {
		$scope.detailModel.GLGeneralInfoId = data.GLGeneralInfoId;
		$scope.detailModel.BudgetMasterId = data.BudgetMasterId;
		$scope.detailModel.ActivityId = data.ActivityId;
		$scope.detailModel.BudgetName = data.BudgetName;
		$scope.getActivity(data);
	};

	$scope.changeType = function (data) {
		$scope.IssueType = data;
	}

	$scope.searchissueAUCglByList = [
		{
			"name": "Fixed Asset",
			"value": "FixedAssetName"
		},
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

	$scope.issueAUCglListParameters = {
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
	$scope.issueAUCglList = [];
	$scope.GetIssueAUCList = function () {
		$scope.IssueAUCGLUrl = "Accounts/glitem/GetIssueAUCGLBudgetActivity";
		$scope.GetIssueAUCGLData = function (pageno) {

			baseService.paginationBase($scope.IssueAUCGLUrl, pageno, $scope.issueAUCglListParameters)
				.then(function (result) {
					$scope.issueAUCglList = result.Rows;
					$scope.issueAUCglListParameters.total_count = result.Total;
				}, function () {
					ShowResult(commonMessage.NetworkError, "failure");
				}).finally(function () {
				});
		};
		angular.element(document.querySelector("#IssueAUCGLPopUp")).modal("show");
		$scope.modalShow = true;
		$scope.GetIssueAUCGLData();
	};

	$scope.closeIssueAUCglListPopUp = function () {
		angular.element(document.querySelector("#IssueAUCGLPopUp")).modal("hide");
	};


	//$scope.CostCenterLoad = function () {
	//    //debugger;
	//    cboService.getCostCenterCbo(function (result) {
	//        $scope.costCenterList = result;
	//    });
	//}
	//$scope.CostCenterLoad();

	$scope.CostCenterLoadNew = function () {
		//debugger

		$http({
			method: "GET",
			url: 'Products/InventoryIssue/GetCostCenterLoadNewFun?EntityId=' + $scope.productNew.EntityId
		}).then(function successCallback(response) {
			$scope.costCenterList = response.data;

		});
	}
	//$scope.CostCenterLoadNew();

	baseService.getCompanyConfiguration(function (result) {
		$scope.companyConfig = result;

	});
	cboService.getCboEntityByPlant(null, null, '', function (result) {
		$scope.EntityList = result;
	});
	$scope.BudgetActivityList = [];

	$scope.getBudgetActivityInIssueMaterial = function (materialGroupMasterId) {
		$http({
			method: "GET",
			url: 'Products/InventoryIssue/GetBudgetActivityInIssueMaterial?materialGroupMasterId=' + materialGroupMasterId
		}).then(function successCallback(response) {
			$scope.BudgetActivityList = response.data;
			$scope.detailModel.GLGeneralInfoId = $scope.BudgetActivityList[0].GLGeneralInfoId;
			$scope.detailModel.BudgetMasterId = $scope.BudgetActivityList[0].BudgetMasterId;
			$scope.detailModel.BudgetName = $scope.BudgetActivityList[0].BudgetName;
			$scope.getActivity($scope.BudgetActivityList[0]);
		});
	};


	$window.onresize = function (event) {

		$scope.actionCompleteSelected3();

	};
	$scope.actionCompleteSelected3 = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#Grid").ejGrid("instance");
				var scrollerwidth = $("#Approved1").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};
	$window.onresize = function (event) {

		$scope.actionCompleteSelected2();

	};
	$scope.actionCompleteSelected2 = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#Grid1").ejGrid("instance");
				var scrollerwidth = $("#Approved2").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};






	$window.onresize = function (event) {

		$scope.actionCompleteSelected31();

	};
	$scope.actionCompleteSelected31 = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#Grid22").ejGrid("instance");
				var scrollerwidth = $("#Posting1").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};

	$window.onresize = function (event) {

		$scope.actionCompleteSelected21();

	};
	$scope.actionCompleteSelected21 = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#Grid33").ejGrid("instance");
				var scrollerwidth = $("#Posting2").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};
	$window.onresize = function (event) {

		$scope.actionCompleteSelected44();

	};
	$scope.actionCompleteSelected44 = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#Grid44").ejGrid("instance");
				var scrollerwidth = $("#UnApprovedStock1").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};
	$window.onresize = function (event) {

		$scope.actionCompleteSelected45();

	};
	$scope.actionCompleteSelected45 = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#Grid45").ejGrid("instance");
				var scrollerwidth = $("#UnApprovedStock2").width();//Obtain the width of the container

				//   $("#GridReq").children('.e-grid.e-headercell').css('height', '100px');              
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {
			//$scope.ShowResultCustom(e, 'failure');
		}
	};




	//#region SlipWise Issue Code----




	//$scope.recorddoubleclick = function ($event) {
	//    //debugger;
	//    var x = $event;
	//    $scope.issueId = x.data.Id;
	//    $scope.isuuedate = x.data.AddedDate;
	//    $scope.productNew.OrderSpecific = x.data.OrderSpecific;
	//    $scope.POPopUpClose();
	//};



	//function ($event) {
	//   //debugger;

	//   var x = $event;
	//   var Id = x.data.Id;
	//   //alert('Id'+Id);
	//   $scope.productNew = x.data;
	//   $scope.productId = "";


	$scope.slipdetailList = [];
	//$scope.recorddoubleclick = function ($event) {
	//    //debugger;
	//    var x = $event;
	//    var Id = x.data.Id;
	//    $scope.issueId = x.data.Id;
	//    $scope.isuuedate = x.data.AddedDate;
	//    // var gridObj = $("#GridTest").ejGrid("instance");
	//    angular.element(document.querySelector('#POPopUp1')).modal('hide');


	//}

	$scope.qtyFunc = function (x, index) {
		//debugger;
		// alert('qtyalert');
		for (var i = 0; i < $scope.detailList.length; i++) {
			if ($scope.detailList[index].IssueRequest === $scope.detailList[i].IssueRequest) {

				if ((Math.round(($scope.detailList[index].TransactionQty + $scope.detailList[i].IssuedQty) * 100 + Number.EPSILON) / 100)> Math.round(($scope.detailList[i].PostingQty) * 100 + Number.EPSILON) / 100) {
					ShowResult("Issue qty must be less than or equal Ready for Issue Qty");
					$scope.detailList[index].TransactionQty = 0;
					$scope.detailList[i].BalanceQty = ($scope.detailList[i].RequestedQty - (Math.round(($scope.detailList[index].TransactionQty + $scope.detailList[i].IssuedQty) * 100 + Number.EPSILON) / 100));
					return false;
					//throw 'Issue qty must be less than or equal Ready for Issue Qty.';
				}

				if ($scope.detailList[index].TransactionQty > Math.round(($scope.detailList[i].RequestedQty) * 100 + Number.EPSILON) / 100) {
					ShowResult("Transaction Qty cannot grater than Requested qty");
					$scope.detailList[index].TransactionQty = 0;
					$scope.detailList[i].BalanceQty = ($scope.detailList[i].RequestedQty - (Math.round(($scope.detailList[index].TransactionQty + $scope.detailList[i].IssuedQty) * 100 + Number.EPSILON) / 100));
					return false;
					//throw 'Issue qty must be less than or equal Ready for Issue Qty.';
				}
				//if ($scope.detailList[index].TransactionQty > $scope.detailList[i].BalanceQty) {
				//	ShowResult("Issue qty must be less than or equal BalanceQty Qty");
				//	return false;
				//	//throw 'Issue qty must be less than or equal Ready for Issue Qty.';
				//}
				$scope.detailList[i].BalanceQty = ($scope.detailList[i].RequestedQty - (Math.round(($scope.detailList[index].TransactionQty + $scope.detailList[i].IssuedQty) * 100 + Number.EPSILON) / 100));
			}

		}

	}


	//$scope.ViewSlipDetail = function () {
	//    //debugger;
	//    //if ($scope.issueId === '' || $scope.issueId === null || $scope.issueId === undefined) {
	//    //    ShowResult("Please select Slip Id");
	//    //    return false;
	//    //}
	//    //else if ($scope.productNew.MaterialStorageId === '' || $scope.productNew.MaterialStorageId === null || $scope.productNew.MaterialStorageId === undefined) {
	//    //    ShowResult("Please select StorageLocation ");
	//    //    return false;
	//    //}

	//    //else if ($scope.productNew.IssueDate === '' || $scope.productNew.IssueDate === null || $scope.productNew.IssueDate === undefined) {
	//    //    ShowResult("Please select Issue Date ");
	//    //    return false;
	//    //}

	//    //else if ($scope.productNew.EmployeeName === '' || $scope.productNew.EmployeeName === null || $scope.productNew.EmployeeName === undefined) {
	//    //    ShowResult("Please select Employee ");
	//    //    return false;
	//    //}


	//    //else if ($scope.productNew.EntityId === '' || $scope.productNew.EntityId === null || $scope.productNew.EntityId === undefined) {
	//    //    ShowResult("Please select Entity ");
	//    //    return false;
	//    //}

	//    $scope.$broadcast('show-errors-check-validity');
	//    if ($scope.productNewForm.$valid) {
	//        $scope.product = Object.assign({}, $scope.productNew);
	//        $scope.detailModel = {
	//            Id: null
	//            , InventoryReveiveId: null
	//            , MaterialStorageId: $scope.productNew.MaterialStorageId
	//            , InventoryMaterialId: null
	//            , MaterialMasterId: null
	//            , MaterialMasterName: null
	//            , ArticleId: null
	//            , ArticleName: null
	//            , MaterialTypeName: null
	//            , OurStyleName: null
	//            , Description: null
	//            , MaterialGroupMasterName: null
	//            , ProductMasterName: null
	//            , IsOurStyleRequired: false
	//            , IsProductMstRequired: false
	//            , FirstCharacteristicsId: null
	//            , FirstCharacteristicsValueId: null
	//            , SecondCharacteristicsId: null
	//            , SecondCharacteristicsValueId: null
	//            , ThirdCharacteristicsId: null
	//            , ThirdCharacteristicsValueId: null
	//            , TransactionQty: null
	//            , TransactionUoMId: null
	//            , TransactionUoM: null
	//            , BaseQty: null
	//            , BaseUOMId: null
	//            , BaseUoM: null
	//            , BaseUoMFactor: null
	//            , TransactionRate: null
	//            , TotalQty: 0
	//            , AvgRate: null
	//            , InventoryIssueId: $scope.productNew.Id
	//            , AvgAmount: null
	//            , PolicyRate: null
	//            , PolicyAmount: null
	//            , Policy: null
	//            , ActivityName: null
	//            , BudgetMasterId: null
	//            , ActivityId: null
	//            , IssueId: null
	//            , CostCenterId:null
	//        };
	//        $scope.clearCharNames();
	//        $http.get($scope.path + 'GetApprovedIssueSlipDetails?Id=' + $scope.issueId + '&StorageLocationId=' + $scope.productNew.MaterialStorageId)
	//            .then(function (response) {
	//                //$scope.slipdetailList = response.data;
	//                $scope.detailList = response.data;
	//            });
	//        // angular.element(document.querySelector('#detailPopUp')).modal('show');
	//    }
	//    $scope.CostCenterLoadNew();
	//}
	$scope.ViewSlipDetail = function () {
		//debugger;


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
			};
			$scope.clearCharNames();
			$http.get($scope.path + 'GetApprovedIssueSlipDetails?Id=' + $scope.issueId + '&StorageLocationId=' + $scope.productNew.MaterialStorageId + '&OrderSpecific=' + $scope.productNew.OrderSpecific)
				.then(function (response) {
					//$scope.slipdetailList = response.data;
					$scope.detailList = response.data;
				});
			// angular.element(document.querySelector('#detailPopUp')).modal('show');
		}
		$scope.CostCenterLoadNew();
	}
	$scope.materialStockList = [];
	$scope.specificStockList = [];
	//debugger;
	$scope.getSpecificMaterialStockForSlipIssue = function (data, index) {
		//$scope.selectedRowQty = data.TransactionQty;
		
		for (var i = 0; i < $scope.detailList.length; i++) {
			$scope.selectedRowQty = $filter('sumByKey')($filter('filter')($scope.detailList, { MaterialMasterId: data.MaterialMasterId, ArticleId: data.ArticleId, FirstCharacteristicsValueId: data.FirstCharacteristicsValueId, SecondCharacteristicsValueId: data.SecondCharacteristicsValueId, ThirdCharacteristicsValueId: data.ThirdCharacteristicsValueId }), 'TransactionQty');
			if ($scope.detailList[i].TransactionQty > $scope.detailList[i].PostingQty) {
				ShowResult("Issue qty can not gaterthen  Ready for issue Qty");
				return false;
			}


		}
		for (var i = 0; i < $scope.detailList.length; i++) {
			if ($scope.detailList[i].TransactionQty > $scope.detailList[i].RequestedQty) {
				ShowResult("Issue qty can not gaterthen Requested Qty");
				return false;
			}
		}



		$scope.index = index;
		$http({
			method: 'POST'
			, url: $scope.path + 'GetSpecificMaterialStock'
			, data: { entity: data, issueDate: $scope.productNew.IssueDate }
			, dataType: 'JSON'
		}).then(function (response) {
			$scope.materialStockList = response.data;

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
				$scope.materialStockList[i1].TrasactopmUomQty = $scope.materialStockList[i1].BalanceStock / data.BaseUOMFactor;
				$scope.materialStockList[i1].IssueTransactionUoMId = data.TransactionUoMId;
				$scope.materialStockList[i1].IssueTransactionUoM = data.TransactionUoM;

				//$scope.materialStockList[i1].TransactionUoMId = data.TransactionUoMId;
				$scope.materialStockList[i1].BaseUoMFactor = data.BaseUOMFactor;
			}
			angular.element(document.querySelector('#stockPopUp')).modal('show');
		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};
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


	$scope.setSelectedforGL = function (data) {
		//debugger;
		$scope.detailList[$scope.issueSlipDetailIndex].GLGeneralInfoId = data.GLGeneralInfoId;
		$scope.detailList[$scope.issueSlipDetailIndex].BudgetMasterId = data.BudgetMasterId;
		$scope.detailList[$scope.issueSlipDetailIndex].ExpenseActivityId = data.ActivityId;
		$scope.detailList[$scope.issueSlipDetailIndex].ActivityName = data.GLGeneralInfoCode + '-' + data.ActivityName;
		$scope.detailList[$scope.issueSlipDetailIndex].BudgetName = data.BudgetName;
		angular.element(document.querySelector("#GLPopUp")).modal("hide");
	};


	//#endregion



	//#region Material Issue icon Detail


	$scope.POPopUp = function () {

		$scope.GetApprovedIssueSlipListGrid();
		angular.element(document.querySelector('#POPopUp1')).modal('show');
	};
	$scope.POPopUpClose = function () {
		angular.element(document.querySelector('#POPopUp1')).modal('hide');
	};




	$scope.GetApprovedIssueSlipList = [];
	$scope.GetApprovedIssueSlipListGrid = function () {
		//debugger;
		try {

			$http({
				method: 'GET',
				url: 'Products/InventoryIssue/GetApprovedIssueSlip',
				dataType: 'JSON'
			}).then(function successCallback(response) {
				if (response.data.Error == true) {
					ShowResult(response.data.Message, 'failure');
				}
				else {
					$scope.GetApprovedIssueSlipList = response.data;


				}
			}, function errorCallback(response) {
				ShowResult(response.status.Message, 'failure');
			});
		} catch (e) {
			ShowResult(e, 'failure');
		}

	};
	$scope.lst = [];
	$scope.POListDetails = function () {
		//debugger;
		$http({
			method: 'GET',
			//url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
			url: 'Products/InventoryIssue/MaterialIssueDetailsData'
		}).then(function successCallback(response) {
			$scope.lst = response.data;
			//$scope.detailgrid($scope.lst);
			window.lst = response.data;

		});
	}
	/*$scope.POListDetails();*/


	$scope.data1 = $scope.lst;
	$scope.detailTemp = "#tabGridContents";
	//$scope.detailgrid = "detailGridData(e)";
	$scope.detailgrid = function detailGridData(e) {
		//debugger;

		var filteredData = e.data["Id"];
		var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("IssueNo", "equal", parseInt(filteredData), true).take(200));
		e.detailsElement.find("#detailGrid").ejGrid({

			dataSource: data,
			columns: ["CostCenter", "Materials", "Article", "SKU1", "SKU2", "SKU3", "Qty", "UOM", "TransactionRate", "CurrencyName", "TrnAmount", "Comments"]
		});
		e.detailsElement.find(".tabcontrol").ejTab();
	}

	$scope.lst = [];
	$scope.POListDetailsReturn = function () {
		//debugger;
		$http({
			method: 'GET',
			//url: 'Products/Requisition/GetAllReqdataDetails?ReqDetailId=' + $scope.filteredData
			url: 'Products/InventoryIssue/MaterialIssueDetailsData'
		}).then(function successCallback(response) {
			$scope.lst = response.data;
			//$scope.detailgrid($scope.lst);
			window.lst1 = response.data;

		});
	}
	//$scope.POListDetailsReturn();


	$scope.data1 = $scope.lst;
	$scope.detailTemp = "#tabGridContents";
	//$scope.detailgrid = "detailGridData(e)";
	$scope.detailgridReturn = function detailGridData(e) {
		//debugger;

		var filteredData = e.data["Id"];
		var data = ej.DataManager(window.lst1).executeLocal(ej.Query().where("IssueNo", "equal", parseInt(filteredData), true).take(200));
		e.detailsElement.find("#detailGrid").ejGrid({

			dataSource: data,
			columns: ["CostCenter", "Materials", "Article", "SKU1", "SKU2", "SKU3", "Qty", "UOM", "TransactionRate", "CurrencyName", "TotalMaterialTranAmount"]
		});
		e.detailsElement.find(".tabcontrol").ejTab();
	}
	//#endregion



	//#region Slip Asset Issue

	$scope.POPopUpAssetIssue = function () {

		$scope.GetAssetApprovedIssueSlipListGrid();

		angular.element(document.querySelector('#POPopUp1')).modal('show');
	};
	$scope.POPopUpClose = function () {
		angular.element(document.querySelector('#POPopUp1')).modal('hide');
	};


	$scope.GetAssetApprovedIssueSlipList = [];
	$scope.GetAssetApprovedIssueSlipListGrid = function () {
		//debugger;
		try {

			$http({
				method: 'GET',
				url: 'Products/InventoryIssue/GetAssetIssueSlip',
				dataType: 'JSON'
			}).then(function successCallback(response) {
				if (response.data.Error == true) {
					ShowResult(response.data.Message, 'failure');
				}
				else {
					$scope.GetAssetApprovedIssueSlipList = response.data;

				}
			}, function errorCallback(response) {
				ShowResult(response.status.Message, 'failure');
			});
		} catch (e) {
			ShowResult(e, 'failure');
		}

	};

	$scope.popUpDataList = [];
	$scope.popUpAssetIssue = function () {
		//debugger;
		$http({
			method: 'GET',
			url: 'Products/InventoryIssue/GetAssetIssueSlipWithGRN?materialStorageId=' + $scope.productNew.MaterialStorageId
		}).then(function successCallback(response) {
			$scope.popUpDataList = response.data;
			angular.element(document.querySelector('#popUpId')).modal('show');
		});
	}



	$window.onresize = function (event) {

		$scope.popUpDataListScroll();

	};
	$scope.popUpDataListScroll = function (args) {
		try {
			if (args.requestType === "refresh") {
				var gridObj = $("#popUpData").ejGrid("instance");
				var scrollerwidth = $("#approved").width();
				gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
				gridObj.windowonresize();
			}
		} catch (e) {

		}
	};










	//$scope.recorddoubleclick = function ($event) {
	//    //debugger;
	//    var x = $event;
	//    $scope.issueId = x.data.Id;
	//    $scope.isuuedate = x.data.AddedDate;
	//    $scope.POPopUpClose();
	//};


	$scope.recorddoubleclick = function ($event) {
		//debugger;
		var x = $event;
		var Id = x.data.Id;
		$scope.issueId = x.data.Id;
		$scope.isuuedate = x.data.AddedDate;
		$scope.productNew.OrderSpecific = x.data.Orderspecific;
		
		$scope.productNew.ProcessName = x.data.ProcessName;
		// var gridObj = $("#GridTest").ejGrid("instance");

		angular.element(document.querySelector('#POPopUp1')).modal('hide');


	}
	//#endregion


	//#region Order Ref
	$scope.masterOrderCustomerList = [];
	$scope.GetMasterOrderByContractList = function () {
		//debugger;
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
		//debugger;
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
		//debugger;
		$http({
			method: "GET",
			dataType: 'JSON',
			//url: $scope.getSearchListUrl,
			url: 'Products/InventoryIssue/GetMasterOrderDetailsList?MasterOrderId=' + $scope.productNew.OrderRefNo,
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
	//#endregions



	$scope.GetShowStorageLocationList = [];
	$scope.GetPopUpShowStorageLocation = function () {
		$http({
			method: 'POST',
			url: $scope.path + 'GetPopUpShowStorageLocation',
			data: { entity: $scope.detailModel, issueDate: $scope.productNew.IssueDate },
			dataType: 'JSON'
		}).then(function (response) {
			$scope.GetShowStorageLocationList = response.data;
			angular.element(document.querySelector('#ShowLOcationWiseStock')).modal('show');
		}), function (response) {
			ShowResult(response.data.Message, 'failure');
		};
	}
	$scope.GetPopUpShowStorageLocationClosed = function () {
		angular.element(document.querySelector('#ShowLOcationWiseStock')).modal('hide');
	}


	$scope.GetEntityWiseConsumptionList = function () {
		//debugger;
		if ($scope.productNew.OrderSpecific === 'Yes') {
			$http({
				method: "GET",
				dataType: 'JSON',
				url: 'Products/InventoryIssue/GetEntityWiseConsumption?EntityId=' + $scope.productNew.EntityId,
			}).then(function successCallback(response) {
				$scope.productNew.ConsumptionBookingName = response.data[0].ConsumptionBooking;

			});
		}
		else {

		}
	}


	$scope.ProductionOrderList = [];
	$scope.getProductionOrderPopUp = function () {
		$scope.ProductionOrderList = [];
		$http.get('OrderManagements/PackingContent/GetProductionOrderDataList')
			.then(
				function successCallback(response) {
					if (baseService.arrayLength(response.data) > 0) {
						$scope.ProductionOrderList = response.data;
					}
				},
				function errorCallback(response) {
					ShowResult(response, 'failure');
				});
		angular.element(document.querySelector('#POItemPopup')).modal('show');
	};

	$scope.selectSOItem = function ($event) {
		try {
			var soitem = $event.data;
			$scope.productNew.EntityId = soitem.EntityId;
			$scope.productNew.ProductionOrderId = soitem.POId;
			angular.element(document.querySelector('#POItemPopup')).modal('hide');

		} catch (ex) {
			ShowResult(ex, 'failure', 'POItemPopup');
		}
	};
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
		//debugger;
		//var data = obj.data.ContractId;
		$scope.productNew.ContractId = obj.data.ContractId;
		$scope.productNew.CustomerName = obj.data.CustomerName;
		$scope.productNew.ContractNo = obj.data.ContractNo;
		$scope.productNew.LCRef = obj.data.LCRef;
		//console.log($scope.productNew);
		angular.element(document.querySelector('#ContractPopUp')).modal('hide');
	}

	$scope.Clearcontract = function () {
		$scope.productNew.CustomerName = "";
		$scope.productNew.ContractId = "";

	};
	$scope.masterOrderCustomerList = [];
	$scope.GetMasterOrderByContractList1 = function () {
		$scope.masterOrderCustomerList = [];
		$http({
			method: 'GET',
			url: "Commercial/Contract/GetMasterOrderListbyContract?contractId=" + $scope.productNew.ContractId
		}).then(function (response) {
			$scope.masterOrderCustomerList = response.data;
		});
		angular.element(document.querySelector('#MasterOrderPopUp')).modal('show');
	}
	$scope.CloseContractPopUp = function () {
		angular.element(document.querySelector('#ContractPopUp')).modal('hide');
	}
	$scope.ClearList = function (data) {
		$scope.inventoryMaterialList = [];
		$scope.OrderSpecific = data;
		$scope.productNew.ContractId = null;
		$scope.productNew.ContractNo = null;
		$scope.productNew.ProductionOrderId = null;
		$scope.productNew.OrderRefNo = null;

	};


	$scope.showMaterialWiseStockModalClose = function () {
		//debugger;
		angular.element(document.querySelector('#POPopUp')).modal('hide');

	};
	$scope.showMaterialWiseStockModal = function (x, index) {

		$scope.GetSOWiseMaterialStock(x, index);
		angular.element(document.querySelector('#POPopUp')).modal('show');

	};
	$scope.ShowStock = [];
	$scope.GetSOWiseMaterialStock = function (x, $index) {
		$scope.GetDetailGridIndex = $index;
		$http({
			method: 'GET',
			url: 'Products/GoodsReceiveNote/GetSOWiseMaterialStock?Material=' + x.MaterialMasterId + '&Article=' + x.ArticleId + '&Skuvalue1=' + x.FirstCharacteristicsValueId + '&Skuvalue2=' + x.SecondCharacteristicsValueId + '&Skuvalue3=' + x.ThirdCharacteristicsValueId + '&ProcessId=' + $scope.productNew.ProcessId + '&SalesOrderId=' + x.SalesOrderId
		}).then(function successCallback(response) {
			$scope.ShowStock = response.data;

		});

	}

	$scope.Change = function (even, index, x) {

		$scope.GetDetailGridIndex = index;

		$http({
			method: 'GET',
			url: 'Products/GoodsReceiveNote/GetSOWiseMaterialStock?Material=' + x.MaterialMasterId + '&Article=' + x.ArticleId + '&Skuvalue1=' + x.FirstCharacteristicsValueId + '&Skuvalue2=' + x.SecondCharacteristicsValueId + '&Skuvalue3=' + x.ThirdCharacteristicsValueId + '&ProcessId=' + $scope.productNew.ProcessId + '&SalesOrderId=' + x.SalesOrderId
		}).then(function successCallback(response) {
			$scope.ShowStock = response.data;
			if (baseService.isUndefinedOrNull($scope.detailList[index].TransactionQty) || $scope.detailList[index].TransactionQty === 0) {
				$scope.detailList[index].check = false;
				ShowResult('Enter the Issue qty', 'failure');

				return false;

			}
			else {
				//for (var i = 0; i < $scope.ShowStock.length; i++) {
				//	if ($scope.ShowStock[i].TransactionUoMId != $scope.detailList[index].TransactionUoMId && $scope.detailList[index].IssueByUoM === true) {
				//		ShowResult('Can not issue this material.Because Requition UoM is not equal to Stock UoM', 'failure');
				//		$scope.detailList[index].TransactionQty = '';
				//		$scope.detailList[index].check = false;
				//		return false;
				//	}

				//}
			}

		});

	}
	
}