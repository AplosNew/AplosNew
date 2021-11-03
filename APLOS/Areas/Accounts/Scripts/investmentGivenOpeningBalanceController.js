'use strict';
investmentGivenOpeningBalanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller'];
function investmentGivenOpeningBalanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $rootScope.title = 'Investment Given Opening Balance';
    $scope.url = 'accounts/OpeningBalance';
    $scope.listUrl = $scope.url + '/GetInvestmentGivenList';
    $scope.saveUrl = $scope.url + '/InsertInvestmentGiven';
    $scope.updateUrl = $scope.url + '/UpdateInvestmentGiven';
    $scope.interplantList = [];

    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $scope.partyType = 'Party';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseOpeningBalanceController', { $scope: $scope, $http: $http });
    $scope.sourceType = 'Investment';
    $scope.openingBalance.PartyType = $scope.partyType;
    $scope.isAdvance = null;
    $scope.bankACType = $scope.sourceType;
    $controller('bankBaseController', { $scope: $scope, $http: $http });
    $scope.companyConfig = null;

    cboService.getCboOtherFinancingType($scope.sourceType, function (result) {
        $scope.financingTypeList = result;
        if ($scope.financingTypeList.length === 1) {
            $scope.openingBalance.FinancingTypeId = $scope.financingTypeList[0].FinancingTypeId;
            $scope.getTransactionTypeGL($scope.openingBalance.FinancingTypeId);
        }
    });

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
}