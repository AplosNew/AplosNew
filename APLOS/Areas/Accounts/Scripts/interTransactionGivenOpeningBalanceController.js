'use strict';
interTransactionGivenOpeningBalanceController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller'];
function interTransactionGivenOpeningBalanceController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $rootScope.title = 'Inter Investment Given Opening Balance';
    $scope.url = 'accounts/OpeningBalance';
    $scope.listUrl = $scope.url + '/GetInterTransactionGivenList';
    $scope.saveUrl = $scope.url + '/InsertInterTransactionGiven';
    $scope.updateUrl = $scope.url + '/UpdateInterTransactionGivenGiven';
    $scope.interplantList = [];

    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $controller('baseOpeningBalanceController', { $scope: $scope, $http: $http });
    $scope.partyType = 'Plant';
    $scope.openingBalance.PartyType = 'Plant';

    $scope.sourceType = 'InterTransaction';
    cboService.getCboInterCompanyFinancingType($scope.sourceType, function (result) {
        $scope.financingTypeList = result;
        if ($scope.financingTypeList.length === 1) {
            $scope.openingBalance.FinancingTypeId = $scope.financingTypeList[0].FinancingTypeId;
            $scope.getTransactionTypeGL($scope.openingBalance.FinancingTypeId);
        }
    });

    $scope.companyConfig = null;
    $scope.advanceCA = null;
    $scope.getTransactionTypeGL = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            $scope.advanceCA = $.grep($scope.financingTypeList, function (item) {
                return item.FinancingTypeId === id;
            })[0];
            if (manualValidation('div_TransactionType', baseService.isUndefinedOrNull($scope.advanceCA.AssetGLId), 'Transaction Type GL not found!')) {
                $scope.advanceCA = null;
                $scope.openingBalanceDetailList = [];
            }
            else if ($scope.companyConfig.IsVoucherFromBudget
                && manualValidation('div_TransactionType', baseService.isUndefinedOrNull($scope.advanceCA.AssetBudgetMasterId), 'Transaction Type Budget not found!')) {
                $scope.advanceCA = null;
                $scope.openingBalanceDetailList = [];
            }
        }
        else {
            manualValidation('div_TransactionType', false, '');
            $scope.advanceCA = null;
        }
    };

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
        $scope.openingBalanceDetailList[0].CompanyId = $window.companyId;
    });

    cboService.getCboInterPlant(null, null, $window.plantId, function (result) {
        $scope.interplantList = result;
        $scope.openingBalanceDetailList[0].PlantId = null;
    });

    $scope.addDefaultDetailRow();

    $scope.changeSourceTo = function (to) {
        $scope.openingBalance.PartyType = to;
        $scope.openingBalanceDetailList[0].PartyType = to;
        $scope.openingBalanceDetailList[0].CompanyId = null;
        $scope.openingBalanceDetailList[0].EntityId = null;
        if (to === 'Company') {
            $scope.interEntityList = [];
            $scope.interplantList = [];
            for (var i = 0; i < baseService.arrayLength($scope.companyList); i++) {
                if ($scope.companyList[i].Value === $window.companyId) {
                    $scope.companyList[i].disabled = true;
                    break;
                }
            }
        }
        else if (to === 'Plant') {
            $scope.openingBalanceDetailList[0].CompanyId = $window.companyId;
            cboService.getCboInterPlant(null, null, $window.plantId, function (result) {
                $scope.interplantList = result;
                $scope.openingBalanceDetailList[0].PlantId = null;
            });
        }
    };
    $scope.changeSourceTo($scope.openingBalance.PartyType);
}