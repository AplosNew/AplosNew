'use strict';
investmentTakenOpeningBalanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller'];
function investmentTakenOpeningBalanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $rootScope.title = 'Equity Opening Balance';
    $scope.url = 'accounts/OpeningBalance';
    $scope.listUrl = $scope.url + '/GetInvestmentTakenList';
    $scope.saveUrl = $scope.url + '/InsertInvestmentTaken';
    $scope.updateUrl = $scope.url + '/UpdateInvestmentTaken';
    $scope.interplantList = [];
    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $scope.partyType = 'Party';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseOpeningBalanceController', { $scope: $scope, $http: $http });
    $scope.openingBalance.PartyType = $scope.partyType;
    $scope.sourceType = 'Investment';
    $scope.isAdvance = null;
   
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
            if (manualValidation('div_TransactionType', baseService.isUndefinedOrNull($scope.advanceCA.LiabilityGLId), 'Transaction Type GL not found!')) {
                $scope.advanceCA = null;
                $scope.openingBalanceDetailList = [];
            }
            else if ($scope.companyConfig.IsVoucherFromBudget
                && manualValidation('div_TransactionType', baseService.isUndefinedOrNull($scope.advanceCA.LiabilityBudgetMasterId), 'Transaction Type Budget not found!')) {
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