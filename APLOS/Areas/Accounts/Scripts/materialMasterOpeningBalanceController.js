'use strict';
materialMasterOpeningBalanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', '$window'];
function materialMasterOpeningBalanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, $window) {
	$rootScope.title = 'Material Master Opening Balance';
	$scope.Action = 'Save';
	$scope.index = -1;
	$scope.openingBalanceList = [];
	$scope.openingBalanceDetailList = [];
	$scope.isEntityLevel = false;
	$controller('currencyBaseController', { $scope: $scope, $http: $http });
	$controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

	$scope.businessProcesses = '';
	$scope.openingBalance = {
		Id: null,
		CompanyGroupId: null,
		CompanyId: null,
		EntityId: null,
		SecurityTypeGivenId: null,
		SecurityTypeTakenId: null,
		InvestmentTypeGivenId: null,
		InvestmentTypeTakenId: null,
		EmployeeTransactionTypeId: null,
		MaterialStorageId: null,
		PostingDate: null,
		DocRefNo: null,
		DocDate: null,
		Narration: null,
		Remarks: null,
		IsPark: false,
		Archive: false,
		IsFinancial: false,
		currentRecord: null,
		InventoryReceivedId: null,
		openingBalanceId: null,

		UpdatedBy : null,
		UpdatedDate : null,
		UpdatedFromIP: null,
		AddedBy: null,
		AddedDate: null,
		AddedFromIP: null,
		MaterialStorageId: null


	};

	$scope.openingBalanceDetail = {
		Id: null,
		MaterialMasterOpeningBalanceId: null,
		MaterialMasterId: null,
		MaterialMasterName: null,
		FixedAssetMasterId: null,
		FixedAssetMasterName: null,
		AssetGLId: null,
		CurrencyId: null,
		Quantity: 0,
		CompanyCurrencyId: null,
		CompanyToCurrencyId: null,
		CompanyGroupCurrencyId: null,
		CompanyGroupToCurrencyId: null,
		HardCurrencyId: null,
		HardToCurrencyId: null,
		FACompanyCurrencyAmount: 0,
		FACompanyGroupCurrencyAmount: 0,
		FAHardCurrencyAmount: 0,
		ArticleId: null,
		FirstCharacteristicsId: null,
		FirstCharacteristicsValueId: null,
		SecondCharacteristicsId: null,
		SecondCharacteristicsValueId: null,
		ThirdCharacteristicsId: null,
		ThirdCharacteristicsValueId: null
	};

	function getCompanyConfiguration() {
		$http.get('Organizations/Company/GetCompanyConfiguration')
			.then(function (response) {
				$scope.companyConfig = response.data;
				$scope.getCutOffDate();
			});
	}
	getCompanyConfiguration();
	$scope.getCutOffDate = function () {
		$http.get('accounts/OpeningBalance/GetACCCutOffDate')
			.then(function (response) {
				if (response.data === null)
					return ShowResult('Opening Balance Cut Off date not found!', 'failure');
				$scope.openingBalance.PostingDate = $filter('dateFiltering')(response.data.CutOffDate);
				$('#docDate').datepicker('setEndDate', new Date(response.data.CutOffDate));
				if (baseService.isUndefinedOrNull($scope.companyConfig.COAId))
					return ShowResult('COA not found!', 'failure');
				$scope.isEntityLevel = response.data.IsEntityLevel;
				if ($scope.isEntityLevel) {
					cboService.getCboEntityByPlant(null, null, '', function (result) {
						$scope.entityList = result;
					});
				}
			});
	};
	$scope.materialStorageList = [];
	cboService.getCboMaterialStorageByCompanyAndPlant('', '', function (result) {
		$scope.materialStorageList = result;
	});
	//baseService.init('accounts/OpeningBalance/GetMaterialMasterList', null, null, 'DESC', 'EntityName', 'EntityName');
	//$scope.getOpeningBalanceData = function (pageno) {
	//    baseService.pagination(pageno)
	//        .then(function (result) {
	//            $scope.openingBalanceList = result.Rows;
	//        }, function () {
	//            ShowResult(commonMessage.NetworkError, 'failure');
	//        }).finally(function () {
	//        });
	//};
	$scope.searchByList = [
		{
			'name': 'Entity',
			'value': 'EntityName'
		},
		{
			'name': 'Posting Date',
			'value': 'PostingDate'
		},
		{
			'name': 'Doc Date',
			'value': 'DocDate'
		},
		{
			'name': 'Doc Ref',
			'value': 'DocRefNo'
		}
	];
	$scope.getOpeningBalanceData = function () {
		// baseService.setCurrentPage('materialmasterSearchData');
		baseService.init('accounts/OpeningBalance/GetMaterialMasterList', null, null, 'DESC', 'EntityName', 'EntityName');
		$scope.loadOPData = function (pageno) {
			baseService.pagination(pageno)
				.then(function (result) {
					$scope.openingBalanceList = result.Rows;
				}, function () {
					ShowResult(commonMessage.NetworkError, 'failure');
				}).finally(function () {
				});
		};
		$scope.loadOPData();
	};
	$scope.getOpeningBalanceData();

	$scope.getById = function (index) {
		$scope.index = index;
		$scope.openingBalance = Object.assign({}, $scope.openingBalanceList[$scope.index]);
		$scope.openingBalance.PostingDate = $filter('dateFiltering')($scope.openingBalance.PostingDate);
		$scope.openingBalance.DocDate = $filter('dateFiltering')($scope.openingBalance.DocDate);
		$http.get('accounts/OpeningBalance/GetMaterialMasterOpeningBalanceDetailList?openingBalanceId=' + $scope.openingBalance.Id)
			.then(function (response) {
				$scope.openingBalanceDetailList = response.data;
				angular.forEach($scope.openingBalanceDetailList, function (item, i) {
					item.DocDate = $filter('dateFiltering')(item.DocDate);
				});
			});
		$scope.Action = 'Update';
		if (!$rootScope.isCollapsed) {
			$rootScope.toggle();
		}
	};

	//Deleting Rows from PFEmployeeAppliedList
	$scope.valuePassInDelModal = function (index, data) {
		$scope.openingBalanceDetailId = data.Id;
		$scope.openingBalanceDetailIndex = index;
		if (baseService.isUndefinedOrNull($scope.openingBalanceDetailId))
			$scope.message_confirmation = 'Are you sure want to parmenently delete this data....';
		else
			$scope.message_confirmation = 'Are you sure want to delete [ ' + data.BudgetName + ' ]';
		angular.element(document.querySelector('#confirmDocumentdelete')).modal('show');
	};

	$scope.removeOpeningBalanceDetail = function () {
		if (baseService.isUndefinedOrNull($scope.openingBalanceDetailId) === true) {
			$scope.openingBalanceDetailList.splice($scope.openingBalanceDetailIndex, 1);
		} else {
			$scope.removeFromDb($scope.openingBalanceDetailId, $scope.openingBalanceDetailIndex);
		}
		$scope.openingBalanceDetailIndex = -1;
		$scope.openingBalanceDetailId = null;
		angular.element(document.querySelector('#confirmDocumentdelete')).modal('hide');
	};
	$scope.removeFromDb = function (id, index) {
		try {
			$http({
				method: 'POST',
				url: 'Accounts/OpeningBalance/DeleteOPDetail',
				dataType: 'JSON',
				data: { 'id': id }
			}).then(function successCallback(response) {
				if (response.data.Error === true) {
					ShowResult(response.data.Message, 'failure');
				}
				else {
					ShowResult(response.data.Message, 'success');
					$scope.openingBalanceDetailList.splice($scope.openingBalanceDetailIndex, 1);
					$scope.openingBalanceDetailIndex = -1;
				}
			}, function errorCallback(response) {
				ShowResult(response.status.Message, 'failure');
			});
			return true;
		} catch (e) {
			ShowResult(e, 'Error');
		}
	};
	//
	//**************Material Master**********************/

	$scope.setMaterialMasterData = function (ob) {
		if (ob.IsAsset === 'Yes') {
			return ShowResult('Fixed Asset can not be entered as inventory opening balance. It should be treated as Fixed asset register.', '', 'materialmastersearchpopup');

		}

		else {

			$scope.openingBalanceDetailList.push({
				MaterialMasterId: ob.Id
				, MaterialMasterName: ob.UserName
				, Quantity: 1
				, FACompanyCurrencyAmount: 0
				, BaseUOMId: ob.BaseUOMId
				, BaseUoM: ob.BaseUoM
				, CurrencyId: $scope.companyCurrencyId
				, CompanyCurrencyId: $scope.companyCurrencyId
				, CompanyCurrencyName: $scope.companyCurrencyName
				, CompanyFromCurrencyId: $scope.companyCurrencyId
				, ToCurrencyId: $scope.companyCurrencyId
				, CompanyGroupCurrencyId: $scope.companyGroupCurrencyId
				, CompanyGroupCurrencyName: $scope.companyGroupCurrencyName
				, CompanyGroupFromCurrencyId: $scope.companyGroupCurrencyId
				, CompanyGroupToCurrencyId: $scope.companyCurrencyId
				, HardCurrencyId: $scope.hardCurrencyId
				, HardCurrencyName: $scope.hardCurrencyName
				, HardFromCurrencyId: $scope.hardCurrencyId
				, HardToCurrencyId: $scope.companyCurrencyId
				, FirstCharacteristicsId: $scope.FirstCharacteristicsId
				, FirstCharacteristicsValueId: $scope.FirstCharacteristicsValueId
				, SecondCharacteristicsId: $scope.SecondCharacteristicsId
				, SecondCharacteristicsValueId: $scope.SecondCharacteristicsValueId
				, ThirdCharacteristicsId: $scope.ThirdCharacteristicsId
				, ThirdCharacteristicsValueId: $scope.ThirdCharacteristicsValueId
				, FirstCharacteristicsValue: $scope.FirstCharacteristicsValue
				, SecondCharacteristicsValue: $scope.SecondCharacteristicsValue
				, ThirdCharacteristicsValue: $scope.ThirdCharacteristicsValue
			});
			$scope.hasArticle = ob.HasAttribute;
			$scope.hasSku = ob.WithSKU;
			if (ob.HasAttribute)
				$scope.getArticleSearchList(ob.Id);
			else if (ob.WithSKU)
				$scope.getCharacteristicsList1(ob.Id);
			//getTaxCategoryList(ob.HSNCodeId);
			angular.element(document.querySelector('#materialmastersearchpopup')).modal('hide');
		}
	};

	//$scope.selectarticle = function (ob) {
	//	debugger;
	//       try {
	//           for (var i = 0; i < $scope.openingBalanceDetailList.length; i++) {
	//               if ($scope.openingBalanceDetailList[i].MaterialMasterId === ob.MaterialMasterId && $scope.openingBalanceDetailList[i].ArticleId === ob.Id) {
	//                   return ShowResult('Material Combination Already Exists .', '', 'articleSearchPop');

	//               }
	//           }

	//           $scope.openingBalanceDetailList[$scope.openingBalanceDetailList.length - 1].ArticleId = ob.Id;
	//           $scope.openingBalanceDetailList[$scope.openingBalanceDetailList.length - 1].ArticleName = ob.StandardName;
	//		if ($scope.hasSku)
	//			//$scope.getCharacteristicsList(ob.Id);
	//			$scope.getCharacteristicsList1(ob.MaterialMasterId); 

	//          angular.element(document.querySelector('#articleSearchPop')).modal('hide');
	//       } catch (e) {
	//           ShowResult(e, '', 'articleSearchPop');
	//       }
	//   };
	$scope.getCharacteristicsList1 = function (id) {
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
			}

			$scope.openingBalanceDetailList[$scope.openingBalanceDetailList.length - 1].FirstCharacteristicsId = $scope.char1.CharacteristicsId;
			$scope.openingBalanceDetailList[$scope.openingBalanceDetailList.length - 1].FirstCharacteristicsValueId = $scope.char1.CharacteristicsValueId;

			$scope.openingBalanceDetailList[$scope.openingBalanceDetailList.length - 1].SecondCharacteristicsId = $scope.char2.CharacteristicsId;
			$scope.openingBalanceDetailList[$scope.openingBalanceDetailList.length - 1].SecondCharacteristicsValueId = $scope.char2.CharacteristicsValueId;

			$scope.openingBalanceDetailList[$scope.openingBalanceDetailList.length - 1].ThirdCharacteristicsId = $scope.char3.CharacteristicsId;
			$scope.openingBalanceDetailList[$scope.openingBalanceDetailList.length - 1].ThirdCharacteristicsValueId = $scope.char3.CharacteristicsValueId;

			$scope.openingBalanceDetailList[$scope.openingBalanceDetailList.length - 1].FirstCharacteristicsValue = $scope.char1.FreeText;
			$scope.openingBalanceDetailList[$scope.openingBalanceDetailList.length - 1].FirstCharacteristicsValue = $scope.char1.FreeText;


			$scope.openingBalanceDetailList[$scope.openingBalanceDetailList.length - 1].SecondCharacteristicsValue = $scope.char2.FreeText;
			$scope.openingBalanceDetailList[$scope.openingBalanceDetailList.length - 1].SecondCharacteristicsValue = $scope.char2.FreeText;

			$scope.openingBalanceDetailList[$scope.openingBalanceDetailList.length - 1].ThirdCharacteristicsValue = $scope.char3.FreeText;
			$scope.openingBalanceDetailList[$scope.openingBalanceDetailList.length - 1].ThirdCharacteristicsValue = $scope.char3.FreeText;

			//$scope.FirstCharacteristicsId = $scope.char1.CharacteristicsId;
			//$scope.FirstCharacteristicsValueId = $scope.char1.CharacteristicsValueId;
			//$scope.SecondCharacteristicsId = $scope.char2.CharacteristicsId;
			//$scope.SecondCharacteristicsValueId = $scope.char2.CharacteristicsValueId;
			//$scope.ThirdCharacteristicsId = $scope.char3.CharacteristicsId;
			//$scope.ThirdCharacteristicsValueId = $scope.char3.CharacteristicsValueId;
			//angular.element(document.querySelector('#articleSearchPop')).modal('hide');
		});



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
						InventoryReveiveId: $scope.productNew.Id
						, MaterialStorageId: $scope.productNew.MaterialStorageId
						, CurrencyName: angular.element("#currency :selected").text()
						, TotalAmount: 0
					};
					$scope.taxCategoryList = [];
					getInventoryMaterialList($scope.productNew.Id);
				}
			}), function errorCallBack(response) {
				ShowResult(response.data.Message, 'failure', 'detailPopUp');
			};
		} catch (e) {
			ShowResult(e, 'success', 'detailPopUp');
		}
	};

	$scope.invalidDocDate = false;
	$scope.checkDocDate = function (controlId, val) {
		var msg = '';
		if (new Date(val) > new Date($scope.openingBalance.PostingDate)) {
			$scope.invalidDocDate = true;
			msg = 'Doc date must be below or equal to Posting Date!';
		}
		else if (baseService.isUndefinedOrNull($scope.openingBalance.DocDate)) {
			$scope.invalidDocDate = true;
			msg = 'Doc date is required.';
		}
		else $scope.invalidDocDate = false;
		return manualValidation(controlId, $scope.invalidDocDate, msg);
	};

	$scope.invalidEntity = false;
	$scope.entityValidation = function () {
		$scope.invalidEntity = baseService.isUndefinedOrNull($scope.openingBalance.EntityId);
		return manualValidation('div_entity', $scope.invalidEntity, 'Entity is required.');
	};
	function checkAmountCheck(list) {
		angular.forEach(list, function (item) {
			if (!parseInt(item.FACompanyCurrencyAmount) > 0) {
				throw "Asset Historical value must be greater than ZERO for " + item.AssetGLName;
			}
		});
	}
	$scope.Save = function () {
		$scope.$broadcast('show-errors-check-validity');
		$scope.checkDocDate('div_DocDate', $scope.openingBalance.DocDate);
		if ($scope.isEntityLevel) {
			$scope.entityValidation();
		}
		try {
			checkAmountCheck($scope.openingBalanceDetailList);
			if ($scope.form1.$valid & !$scope.invalidDocDate && !$scope.invalidEntity) {
				if ($scope.Action === 'Save') {
					$http({
						method: 'POST',
						url: 'accounts/OpeningBalance/InsertMaterialMaster',
						data: {
							'openingBalance': $scope.openingBalance,
							'materialMasterOpeningBalanceDetailList': $scope.openingBalanceDetailListNew
						},
						dataType: 'JSON'
					}).then(function successCallback(response) {
						if (response.data.Error === true) {
							ShowResult(response.data.Message, 'failure');
							
						}
						else {
							ShowResult(response.data.Message, 'success');
							$scope.openingBalance.Id = response.data.openingBalance.Id;
							$scope.openingBalance.currentRecord = response.data.openingBalance.currentRecord;
							$scope.openingBalance.InventoryReceivedId = response.data.openingBalance.InventoryReceivedId;
							$scope.openingBalance.openingBalanceId = response.data.openingBalance.openingBalanceId;
							$scope.openingBalance.MaterialStorageId = response.data.openingBalance.MaterialStorageId;
							$scope.getOpeningBalanceData();
							//$scope.clearFields();
							
						}
					}, function errorCallback(response) {
						ShowResult(response.status.Message, 'failure');
					});
					return true;
				}
				else if ($scope.Action === 'Update') {
					$http({
						method: 'POST',
						url: 'accounts/OpeningBalance/UpdateMaterialMaster',
						data: {
							'openingBalance': $scope.openingBalance,
							'materialMasterOpeningBalanceDetailList': $scope.openingBalanceDetailList
						},
						dataType: 'JSON'
					}).then(function successCallback(response) {
						if (response.data.Error === true) {
							ShowResult(response.data.Message, 'failure');
						}
						else {
							ShowResult(response.data.Message, 'success');
							$scope.getOpeningBalanceData();
							$scope.clearFields();
						}
					}, function errorCallback(response) {
						ShowResult(response.status.Message, 'failure');
					});
				}
				return true;
			}
		} catch (e) {
			ShowResult(e, 'failure');
		}
	};

	$scope.Delete = function () {
		if (!baseService.isUndefinedOrNull($scope.openingBalance.Id)) {
			$http({
				method: 'POST',
				url: 'accounts/OpeningBalance/DeleteMaterialMaster/' + $scope.openingBalance.Id,
				dataType: 'JSON'
			}).then(function successCallback(response) {
				if (response.data.Error === true) {
					ShowResult(response.data.Message, 'failure');
				}
				else {
					ShowResult(response.data.Message, 'success');
					angular.forEach($scope.openingBalanceList, function (item, i) {
						if ($scope.openingBalance.Id === item.Id) {
							$scope.openingBalanceList.splice(i, 1);
						}
					});
					$scope.clearFields();
				}
				function errorCallBack(response) {
					ShowResult(response.data.Message, 'failure');
				}
			});
		}
	};

	$scope.clearFields = function () {
		$scope.Action = 'Save';
		$scope.openingBalance.DocDate = null;
		$scope.openingBalance.DocRefNo = null;
		$scope.openingBalance.Narration = null;
		$scope.openingBalance.Narration = null;
		$scope.openingBalance.EntityId = null;
		$scope.openingBalance.IsFinancial = false;
		$scope.openingBalance.MaterialStorageId = null;
		$scope.openingBalanceDetailList = [];
		clearOpeningBalanceDetail();
	};

	function clearOpeningBalanceDetail() {
		$scope.openingBalanceDetail = {};
		$scope.openingBalanceDetail.Quantity = 0;
		$scope.openingBalanceDetail.FACompanyCurrencyAmount = 0;
		$scope.openingBalanceDetail.FACompanyGroupCurrencyAmount = 0;
		$scope.openingBalanceDetail.FAHardCurrencyAmount = 0;
	}




	//#region Material
	$scope.materialType = ['Asset', 'Consumable', 'Spare', 'RawMaterial'];
	$scope.detailPopUp = function () {
		if (baseService.isUndefinedOrNull($scope.openingBalance.DocDate)) {
			ShowResult('Enter the Doc date.');
			return false;
		}
		else if (baseService.isUndefinedOrNull($scope.openingBalance.DocRefNo)) {
			ShowResult('Enter the Doc Ref No.');
			return false;
		}
		else if (baseService.isUndefinedOrNull($scope.openingBalance.MaterialStorageId)) {
			ShowResult('Select the storage location.');
			return false;
		}
		else if (baseService.isUndefinedOrNull($scope.openingBalance.EntityId)) {
			ShowResult('Select the Entity.');
			return false;
		}
		else if (baseService.isUndefinedOrNull($scope.openingBalance.Narration)) {
			ShowResult('Enter the Narration.');
			return false;
		}

		$scope.openingBalanceDetail = {
			Id: null,
			MaterialMasterOpeningBalanceId: null,
			MaterialMasterId: null,
			MaterialMasterName: null,
			FixedAssetMasterId: null,
			FixedAssetMasterName: null,
			AssetGLId: null,
			CurrencyId: null,
			Quantity: 0,
			CompanyCurrencyId: null,
			CompanyToCurrencyId: null,
			CompanyGroupCurrencyId: null,
			CompanyGroupToCurrencyId: null,
			HardCurrencyId: null,
			HardToCurrencyId: null,
			FACompanyCurrencyAmount: 0,
			FACompanyGroupCurrencyAmount: 0,
			FAHardCurrencyAmount: 0,
			ArticleId: null,
			FirstCharacteristicsId: null,
			FirstCharacteristicsValueId: null,
			SecondCharacteristicsId: null,
			SecondCharacteristicsValueId: null,
			ThirdCharacteristicsId: null,
			ThirdCharacteristicsValueId: null
		};
		$scope.detailModel = {
			Id: null
			, CountryId: null
			, InventoryReceiveId: null
			, MaterialStorageId: null
			, CurrencyName: null
			, CurrencyId: null
			, BaseCurrencyId: null
			, DocDate: null
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
			, ToCurrencyRate: null
			, IsNonCreditable: null
			, IsOriginApplicable: false
			, PartyCode: null
			, EnumType: null
			, TypeValueLot: false
			, TypeValueDiameter: false
			, TypeValueType: false
			, LotNumber: null
			, Diameter: null
			, Type: null
			, FACompanyCurrencyAmount: null
			, Quantity: null
		};
		$scope.clearCharNames();
		angular.element(document.querySelector('#detailPopUp')).modal('show');
	};


	$scope.closeDetaiPopUp = function () {
		//$scope.detailModel = {};
		//$scope.taxCategoryList = [];
		//removeValidationMsg();
		angular.element(document.querySelector('#detailPopUp')).modal('hide');
	};

	$scope.selectMaterialByType = function (ob) {

		debugger;
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
		//$scope.detailModel.IsAsset = ob.IsAsset;
		$scope.detailModel.CurrencyId = ob.CurrencyId;
		$scope.detailModel.CompanyCurrencyId = ob.CompanyCurrencyId;
		$scope.detailModel.companyCurrencyName = ob.companyCurrencyName;
		$scope.detailModel.CompanyFromCurrencyId = ob.CompanyFromCurrencyId;
		$scope.detailModel.CompanyGroupToCurrencyId = ob.CompanyGroupToCurrencyId;
		$scope.detailModel.ToCurrencyId = ob.ToCurrencyId;

		$scope.detailModel.HardCurrencyId = ob.hardCurrencyId
		$scope.detailModel.HardCurrencyName = ob.hardCurrencyName
		$scope.detailModel.HardFromCurrencyId = ob.hardCurrencyId
		$scope.detailModel.HardToCurrencyId = ob.companyCurrencyId

		$scope.hasArticle = ob.HasAttribute;
		$scope.hasSku = ob.WithSKU;
		$scope.clearCharNames();
		if (ob.HasAttribute) $scope.getArticleSearchList(ob.Id);
		if (ob.WithSKU) $scope.getCharacteristicsList(ob.Id);

		//  getTaxCategoryList(ob.HSNCodeId);
		var mmId = []; mmId.push(ob.Id);

		cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (result) {
			$scope.uoMList = result;
			//$scope.detailModel.BaseUOMId = $filter("filter")($scope.uoMList, { IsBaseUom: 1 })[0].Value;
		});

		manualValidation('div_mm', false);
		manualValidation('div_country', false);

		$scope.LoadMaterialStatusLoad(ob.Id);
		$scope.closeMaterialMasterbyTypePopUp();
	};


	$scope.LoadMaterialStatusLoad = function (ob) {
		//debugger;
		$http({
			method: 'GET',
			url: 'accounts/OpeningBalance/LoadMaterialEnulType?Id=' + ob
		}).then(function successCallback(response) {
			$scope.LoadMaterialStatusLoadList = response.data;

			for (var i = 0; i < $scope.LoadMaterialStatusLoadList.length; i++) {
				if ($scope.LoadMaterialStatusLoadList[i].TypeValue === 'LotNo') {
					$scope.detailModel.TypeValueLot = true;
				}
				else if ($scope.LoadMaterialStatusLoadList[i].TypeValue === 'Diameter') {
					$scope.detailModel.TypeValueDiameter = true;
				}
				else if ($scope.LoadMaterialStatusLoadList[i].TypeValue === 'Type') {
					$scope.detailModel.TypeValueType = true;
				}

			}

		});
	}
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
	$scope.openingBalanceDetailListNew = [];
	$scope.addMaterialToList = function (ob) {
		debugger;
		if ($scope.detailModel.IsAsset === 'Yes') {
			return ShowResult('Fixed Asset can not be entered as inventory opening balance. It should be treated as Fixed asset register.', '', 'detailPopUp');

		}

		else if ($scope.detailModel.TypeValueLot === true && baseService.isUndefinedOrNull($scope.detailModel.LotNumber)) {
			ShowResult('Enter the Lot Number.', '', 'detailPopUp');
			return false;
		}
		else if ($scope.detailModel.TypeValueDiameter === true && baseService.isUndefinedOrNull($scope.detailModel.Diameter)) {
			ShowResult('Enter the Diameter.', '', 'detailPopUp');
			return false;
		}
		else if ($scope.detailModel.TypeValueType === true && baseService.isUndefinedOrNull($scope.detailModel.Type)) {
			ShowResult('Enter the Type.', '', 'detailPopUp');
			return false;
		}
		else if (baseService.isUndefinedOrNull($scope.detailModel.Quantity)) {
			ShowResult('Enter the quantity.', '', 'detailPopUp');
			return false;
		}
		else if (baseService.isUndefinedOrNull($scope.detailModel.FACompanyCurrencyAmount)) {
			ShowResult('Enter the Amount.', '', 'detailPopUp');
			return false;
		}




		else {

			for (var i = 0; i < $scope.openingBalanceDetailList.length; i++) {
				if ($scope.openingBalanceDetailList[i].MaterialMasterId === $scope.detailModel.MaterialMasterId
					&& $scope.openingBalanceDetailList[i].ArticleId === $scope.detailModel.ArticleId
					&& $scope.openingBalanceDetailList[i].FirstCharacteristicsValue === $scope.char1.FreeText
					&& $scope.openingBalanceDetailList[i].SecondCharacteristicsValue === $scope.char2.FreeText
					&& $scope.openingBalanceDetailList[i].ThirdCharacteristicsValue === $scope.char3.FreeText

					&& $scope.openingBalanceDetailList[i].LotNumber === $scope.detailModel.LotNumber
					&& $scope.openingBalanceDetailList[i].Diameter === $scope.detailModel.Diameter
					&& $scope.openingBalanceDetailList[i].Type === $scope.detailModel.Type
				) {
					return ShowResult('Material Combination Already Exists .', '', 'detailPopUp');

				}
			}
			




			$scope.openingBalanceDetailList.push({

				MaterialMasterId: $scope.detailModel.MaterialMasterId
				, MaterialMasterName: $scope.detailModel.MaterialMasterName
				, ArticleId: $scope.detailModel.ArticleId
				, ArticleName: $scope.detailModel.ArticleName
				, Quantity: $scope.detailModel.Quantity
				, FACompanyCurrencyAmount: $scope.detailModel.FACompanyCurrencyAmount
				, BaseUOMId: $scope.detailModel.BaseUOMId
				, BaseUoM: $scope.detailModel.BaseUoM

				, CurrencyId: $scope.companyCurrencyId
				, CompanyCurrencyId: $scope.companyCurrencyId
				, CompanyCurrencyName: $scope.companyCurrencyName
				, CompanyFromCurrencyId: $scope.companyCurrencyId
				, ToCurrencyId: $scope.companyCurrencyId
				, CompanyGroupCurrencyId: $scope.companyGroupCurrencyId
				, CompanyGroupCurrencyName: $scope.companyGroupCurrencyName
				, CompanyGroupFromCurrencyId: $scope.companyGroupCurrencyId
				, CompanyGroupToCurrencyId: $scope.companyCurrencyId
				, HardCurrencyId: $scope.hardCurrencyId
				, HardCurrencyName: $scope.hardCurrencyName
				, HardFromCurrencyId: $scope.hardCurrencyId
				, HardToCurrencyId: $scope.companyCurrencyId

				, FirstCharacteristicsValue: $scope.char1.FreeText
				, FirstCharacteristicsId: $scope.char1.CharacteristicsId
				, FirstCharacteristicsValueId: $scope.char1.CharacteristicsValueId

				, SecondCharacteristicsValue: $scope.char2.FreeText
				, SecondCharacteristicsId: $scope.char2.CharacteristicsId
				, SecondCharacteristicsValueId: $scope.char2.CharacteristicsValueId

				, ThirdCharacteristicsValue: $scope.char3.FreeText
				, ThirdCharacteristicsId: $scope.char3.CharacteristicsId
				, ThirdCharacteristicsValueId: $scope.char3.CharacteristicsValueId
				, LotNumber: $scope.detailModel.LotNumber
				, Diameter: $scope.detailModel.Diameter
				, Type: $scope.detailModel.Type


			});
			//$scope.hasArticle = ob.HasAttribute;
			//$scope.hasSku = ob.WithSKU;
			//if (ob.HasAttribute)
			//    $scope.getArticleSearchList(ob.Id);
			//else if (ob.WithSKU)
			//    $scope.getCharacteristicsList1(ob.Id);
			//getTaxCategoryList(ob.HSNCodeId);
			// angular.element(document.querySelector('#materialmastersearchpopup')).modal('hide');

			$scope.openingBalanceDetailListNew.push({

				MaterialMasterId: $scope.detailModel.MaterialMasterId
				, MaterialMasterName: $scope.detailModel.MaterialMasterName
				, ArticleId: $scope.detailModel.ArticleId
				, ArticleName: $scope.detailModel.ArticleName
				, Quantity: $scope.detailModel.Quantity
				, FACompanyCurrencyAmount: $scope.detailModel.FACompanyCurrencyAmount
				, BaseUOMId: $scope.detailModel.BaseUOMId
				, BaseUoM: $scope.detailModel.BaseUoM

				, CurrencyId: $scope.companyCurrencyId
				, CompanyCurrencyId: $scope.companyCurrencyId
				, CompanyCurrencyName: $scope.companyCurrencyName
				, CompanyFromCurrencyId: $scope.companyCurrencyId
				, ToCurrencyId: $scope.companyCurrencyId
				, CompanyGroupCurrencyId: $scope.companyGroupCurrencyId
				, CompanyGroupCurrencyName: $scope.companyGroupCurrencyName
				, CompanyGroupFromCurrencyId: $scope.companyGroupCurrencyId
				, CompanyGroupToCurrencyId: $scope.companyCurrencyId
				, HardCurrencyId: $scope.hardCurrencyId
				, HardCurrencyName: $scope.hardCurrencyName
				, HardFromCurrencyId: $scope.hardCurrencyId
				, HardToCurrencyId: $scope.companyCurrencyId

				, FirstCharacteristicsValue: $scope.char1.FreeText
				, FirstCharacteristicsId: $scope.char1.CharacteristicsId
				, FirstCharacteristicsValueId: $scope.char1.CharacteristicsValueId

				, SecondCharacteristicsValue: $scope.char2.FreeText
				, SecondCharacteristicsId: $scope.char2.CharacteristicsId
				, SecondCharacteristicsValueId: $scope.char2.CharacteristicsValueId

				, ThirdCharacteristicsValue: $scope.char3.FreeText
				, ThirdCharacteristicsId: $scope.char3.CharacteristicsId
				, ThirdCharacteristicsValueId: $scope.char3.CharacteristicsValueId
				, LotNumber: $scope.detailModel.LotNumber
				, Diameter: $scope.detailModel.Diameter
				, Type: $scope.detailModel.Type


			});

		    $scope.Save();
			$scope.openingBalanceDetailListNew = [];
		}
		//#endregion

	}
}