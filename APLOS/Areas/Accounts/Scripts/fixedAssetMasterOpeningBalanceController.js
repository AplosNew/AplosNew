'use strict';
fixedAssetMasterOpeningBalanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function fixedAssetMasterOpeningBalanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'FixedAsset Opening Balance';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.openingBalanceList = [];
    $scope.openingBalanceDetailList = [];
    $scope.isEntityLevel = false;
    $controller('currencyBaseController', { $scope: $scope, $http: $http });

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
        PostingDate: null,
        DocRefNo: null,
        DocDate: null,
        Narration: null,
        Remarks: null,
        IsPark: false,
        Archive: false
    };

    $scope.openingBalanceDetail = {
        Id: null,
        MaterialMasterOpeningBalanceId: null,
        MaterialMasterId: null,
        MaterialMasterName: null,
        FixedAssetMasterId: null,
        FixedAssetMasterName: null,
        AssetGLId: null,
        AccumulatedDepreciationGLId: null,
        CurrencyId: null,
        Quantity: 0,
        IsRegisterApply: false,
        CompanyCurrencyId: null,
        CompanyToCurrencyId: null,
        CompanyGroupCurrencyId: null,
        CompanyGroupToCurrencyId: null,
        HardCurrencyId: null,
        HardToCurrencyId: null,
        FACompanyCurrencyAmount: 0,
        FACompanyGroupCurrencyAmount: 0,
        FAHardCurrencyAmount: 0,
        ADCompanyCurrencyAmount: 0,
        ADCompanyGroupCurrencyAmount: 0,
        ADHardCurrencyAmount: 0,
        //Direct
        DirectQuantity: 0,
        FACompanyCurrencyDirectAmount: 0,
        FACompanyCurrencyDirectRate: 0,
        FACompanyCurrencyDirectConversion: 0,
        ADCompanyCurrencyDirectAmount: 0,
        ADCompanyCurrencyDirectRate: 0,
        ADCompanyCurrencyDirectConversion: 0,
        //InDirect
        InDirectQuantity: 0,
        FACompanyCurrencyInDirectAmount: 0,
        FACompanyCurrencyInDirectRate: 0,
        FACompanyCurrencyInDirectConversion: 0,
        ADCompanyCurrencyInDirectAmount: 0,
        ADCompanyCurrencyInDirectRate: 0,
        ADCompanyCurrencyInDirectConversion: 0
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
                if (response.data == null)
                    return ShowResult('Opening Balance Cut Off date not found!', 'failure');
                $scope.openingBalance.PostingDate = $filter('dateFiltering')(response.data.CutOffDate);
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

    baseService.init('accounts/OpeningBalance/GetFixedAssetList', null, null, 'DESC', 'EntityName', 'EntityName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.openingBalanceList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

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

    $rootScope.searchByList = [
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

    //$scope.valuePassInDelModal = function (index,data) {
    //    $scope.openingBalanceDetailList.splice(index, 1);
    //};
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
            $scope.openingBalanceDetailIndex = -1;
            $scope.openingBalanceDetailId = null;
        } else {
            $scope.removeFromDb($scope.openingBalanceDetailId, $scope.openingBalanceDetailIndex);
        }

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
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.openingBalanceDetailList.splice($scope.openingBalanceDetailIndex, 1);
                    $scope.openingBalanceDetailIndex = -1;
                    $scope.openingBalanceDetailId = null;
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
    //************************************ Fixed Asset Master Start ***********************************************************
    $scope.fixedAssetTypes = [
        {
            Value: 'Machine',
            Text: 'Machine'
        },
        {
            Value: 'Equipment',
            Text: 'Equipment'
        },
        {
            Value: 'Plant',
            Text: 'Plant'
        },
        {
            Value: 'Vahical',
            Text: 'Vahical'
        },
        {
            Value: 'Other',
            Text: 'Other'
        }
    ];

    $scope.showAssetItemPopUp = function () {
        angular.element(document.querySelector('#FixedAssetMasterListPopUp')).modal('show');
    };

    $scope.fixedAssetMasterIndex = -1;
    $scope.searchFixedAssetMasterByList = [
        {
            'name': 'GL',
            'value': 'AssetGLName'
        },
        {
            'name': 'Budget',
            'value': 'BudgetName'
        },
        {
            'name': 'Budget Category',
            'value': 'BudgetCategory'
        },
        {
            'name': 'Budget SubCategory',
            'value': 'BudgetSubCategory'
        },
        {
            'name': 'Asset Master',
            'value': 'FixedAssetMasterName'
        },
        {
            'name': 'AccDepreciation GL',
            'value': 'AccDepreciationName'
        },
        {
            'name': 'Ref No',
            'value': 'RefNo'
        }
    ];

    $scope.fixedAssetMasterListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AssetGLName, BudgetName',
        searchBy: 'BudgetName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showFixedAssetMasterList = function () {
        $scope.GetFixedAssetMasterListData = function (pageno) {
            baseService.paginationBase('Materials/MaterialMaster/GetMaterialMasterDeterminateGL', pageno, $scope.fixedAssetMasterListParameters)
                .then(function (result) {
                    $scope.FixedAssetMasterList = result.Rows;
                    $scope.fixedAssetMasterListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetFixedAssetMasterListData();
        angular.element(document.querySelector('#FixedAssetMasterListPopUp')).modal('show');
    };

    $scope.selectFixedAssetMasterListPopUp = function (index, data) {
        $scope.fixedAssetMasterIndex = index;
    };

    $scope.closeFixedAssetMasterListPopUp = function () {
        if ($scope.fixedAssetMasterIndex !== -1) {
            var fixedAssetItem = $scope.FixedAssetMasterList[$scope.fixedAssetMasterIndex];
            if (baseService.isUndefinedOrNull(fixedAssetItem.FixedAssetMasterId)) {
                return ShowResult('Asset Master not configured!', 'failure', 'FixedAssetMasterListPopUp');
            }
            //else if (baseService.isUndefinedOrNull(fixedAssetItem.AccumulatedDepreciationGLId)) {
            //    ShowResult('Asset Accumulated Depreciation GL not found!', 'failure', 'FixedAssetMasterListPopUp');
            //    return;
            //}
            else {
                $scope.openingBalanceDetail.CurrencyId = $scope.companyCurrencyId;

                $scope.openingBalanceDetail.FixedAssetMasterId = fixedAssetItem.FixedAssetMasterId;
                $scope.openingBalanceDetail.FixedAssetMasterName = fixedAssetItem.FixedAssetMasterName;
                $scope.openingBalanceDetail.AssetBudgetMasterId = fixedAssetItem.Id;
                $scope.openingBalanceDetail.BudgetName = fixedAssetItem.BudgetName;
                $scope.openingBalanceDetail.AssetActivityId = fixedAssetItem.AssetActivityId;
                $scope.openingBalanceDetail.AssetActivityName = fixedAssetItem.AssetActivityName;
                $scope.openingBalanceDetail.BudgetCategoryId = fixedAssetItem.BudgetCategoryId;
                $scope.openingBalanceDetail.BudgetSubCategoryId = fixedAssetItem.BudgetSubCategoryId;
                $scope.openingBalanceDetail.AssetGLName = fixedAssetItem.AssetGLName;
                $scope.openingBalanceDetail.AssetGLId = fixedAssetItem.AssetGLId;
                $scope.openingBalanceDetail.AccDepreciation = fixedAssetItem.AccDepreciation;
                $scope.openingBalanceDetail.AccumulatedDepreciationGLId = fixedAssetItem.AccumulatedDepreciationGLId;
                $scope.openingBalanceDetail.ACUBudgetName = fixedAssetItem.ACUBudgetName;
                $scope.openingBalanceDetail.AccumulatedDepreciationBudgetMasterId = fixedAssetItem.AccumulatedDepreciationBudgetMasterId;
                $scope.openingBalanceDetail.AccumulatedDepreciationActivityId = fixedAssetItem.AccumulatedDepreciationActivityId;
                $scope.openingBalanceDetail.AccumulativeActivityName = fixedAssetItem.AccumulativeActivityName;
                //$scope.openingBalanceDetail.MaterialMasterId = fixedAssetItem.Id;
                //$scope.openingBalanceDetail.MaterialMasterName = fixedAssetItem.MaterialMasterName;
                //$scope.openingBalanceDetail.BaseUOMId = fixedAssetItem.BaseUOMId;
                //$scope.openingBalanceDetail.BaseUOMName = fixedAssetItem.BaseUOMName;

                $scope.openingBalanceDetail.CompanyCurrencyId = $scope.companyCurrencyId;
                $scope.openingBalanceDetail.CompanyCurrencyName = $scope.companyCurrencyName;
                $scope.openingBalanceDetail.CompanyFromCurrencyId = $scope.companyCurrencyId;
                $scope.openingBalanceDetail.ToCurrencyId = $scope.companyCurrencyId;

                $scope.openingBalanceDetail.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                $scope.openingBalanceDetail.CompanyGroupCurrencyName = $scope.companyGroupCurrencyName;
                $scope.openingBalanceDetail.CompanyGroupFromCurrencyId = $scope.companyGroupCurrencyId;
                $scope.openingBalanceDetail.CompanyGroupToCurrencyId = $scope.companyCurrencyId;

                $scope.openingBalanceDetail.HardCurrencyId = $scope.hardCurrencyId;
                $scope.openingBalanceDetail.HardCurrencyName = $scope.hardCurrencyName;
                $scope.openingBalanceDetail.HardFromCurrencyId = $scope.hardCurrencyId;
                $scope.openingBalanceDetail.HardToCurrencyId = $scope.companyCurrencyId;
                if (validateDuplicateBudget($scope.openingBalanceDetail) === false) {
                    $scope.openingBalanceDetailList.splice(0, 0, $scope.openingBalanceDetail);
                } else {
                    return ShowResult(fixedAssetItem.BudgetName + ' already added on list', 'failure', 'FixedAssetMasterListPopUp');
                }
                clearOpeningBalanceDetail();
            }
        }
        angular.element(document.querySelector('#FixedAssetMasterListPopUp')).modal('hide');
        $scope.fixedAssetMasterIndex = -1;
    };
    function validateDuplicateBudget(data) {
        for (var i = 0; i < $scope.openingBalanceDetailList.length; i++) {
            var ob = $scope.openingBalanceDetailList[i];
            if (ob.AssetGLId === data.AssetGLId
                && ob.AssetBudgetMasterId === data.AssetBudgetMasterId
                && ob.AssetActivityId === data.AssetActivityId
                && ob.BudgetCategoryId === data.BudgetCategoryId
                && ob.BudgetSubCategoryId === data.BudgetSubCategoryId) {
                return true;
                break;
            }
        }
        return false;
    }
    $scope.directInDirectIndex = -1;
    $scope.showDirectIndirectPop = function (index) {
        $scope.directInDirectIndex = index;
        angular.element(document.querySelector('#opDetailDirectInDirectPopUp')).modal('show');
    }
    $scope.opDetailDirectInDirectPopUpClose = function () {
        if ($scope.openingBalanceDetailList[$scope.directInDirectIndex].ADCompanyCurrencyDirectAmount > $scope.openingBalanceDetailList[$scope.directInDirectIndex].FACompanyCurrencyDirectAmount) {
            return ShowResult("Direct Accumulated Depreciation Value ammount can not be greater than Asset Historical Value", 'failure', 'opDetailDirectInDirectPopUp');
        }
        if ($scope.openingBalanceDetailList[$scope.directInDirectIndex].ADCompanyCurrencyInDirectAmount > $scope.openingBalanceDetailList[$scope.directInDirectIndex].FACompanyCurrencyInDirectAmount) {
            return ShowResult("InDirect Accumulated Depreciation Value ammount can not be greater than Asset Historical Value", 'failure', 'opDetailDirectInDirectPopUp');
        }
        angular.element(document.querySelector('#opDetailDirectInDirectPopUp')).modal('hide');
    }
    //************************************ Fixed Asset Master End ****************************************************************************
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
            if (parseInt(item.ADCompanyCurrencyAmount) > parseInt(item.FACompanyCurrencyAmount)) {
                throw "Accumulated Depreciation value is more than Asset Historical value for GL " + item.AssetGLName;
            }
            if (parseInt(item.FACompanyCurrencyAmount) != parseInt(item.FACompanyCurrencyDirectAmount) + parseInt(item.FACompanyCurrencyInDirectAmount)) {
                throw "Direct ammount is not  equal as <b> Asset Historical </b> Value of <b>(" + item.BudgetName + ")</b>";
            }
            if (parseInt(item.ADCompanyCurrencyAmount) != parseInt(item.ADCompanyCurrencyDirectAmount) + parseInt(item.ADCompanyCurrencyInDirectAmount)) {
                throw "Direct ammount is not  equal as <b> Accumulated Depreciation </b> Value of <b>(" + item.BudgetName + ")</b>";
            }
            if (parseInt(item.FACompanyCurrencyAmount) < parseInt(item.FACompanyCurrencyDirectAmount) + parseInt(item.FACompanyCurrencyInDirectAmount)) {
                throw "Direct ammount can not be greater than <b> Asset Historical </b> Value of <b>(" + item.BudgetName + ")</b>";
            }
            if (parseInt(item.ADCompanyCurrencyAmount) < parseInt(item.ADCompanyCurrencyDirectAmount) + parseInt(item.ADCompanyCurrencyInDirectAmount)) {
                throw "Direct ammount can not be greater than <b> Accumulated Depreciation </b> Value of <b>(" + item.BudgetName + ")</b>";
            }
        })
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
                        url: 'accounts/OpeningBalance/InsertFixedAsset',
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
                            $scope.getData();
                            $scope.clearFields();
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                    return true;
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: 'accounts/OpeningBalance/UpdateFixedAsset',
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
                            $scope.getData();
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
                url: 'accounts/OpeningBalance/DeleteFixedAsset/' + $scope.openingBalance.Id,
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
                    })
                    ClearFields(response.data.Sequence);
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
        $scope.openingBalanceDetailList = [];
        clearOpeningBalanceDetail();
    };

    function clearOpeningBalanceDetail() {
        $scope.openingBalanceDetail = {};
        $scope.openingBalanceDetail.Quantity = 0;
        $scope.openingBalanceDetail.FACompanyCurrencyAmount = 0;
        $scope.openingBalanceDetail.FACompanyGroupCurrencyAmount = 0;
        $scope.openingBalanceDetail.FAHardCurrencyAmount = 0;
        $scope.openingBalanceDetail.ADCompanyCurrencyAmount = 0;
        $scope.openingBalanceDetail.ADCompanyGroupCurrencyAmount = 0;
        $scope.openingBalanceDetail.ADHardCurrencyAmount = 0;
    }
}