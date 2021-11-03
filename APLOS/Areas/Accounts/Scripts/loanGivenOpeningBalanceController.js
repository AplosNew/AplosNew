'use strict';
loanGivenOpeningBalanceController.$inject = ['cboService', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function loanGivenOpeningBalanceController(cboService, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = 'Loan Given Opening Balance';
    $scope.url = 'accounts/OpeningBalance';
    $scope.listUrl = $scope.url + '/GetLoanGivenList';
    $scope.saveUrl = $scope.url + '/InsertLoanGiven';
    $scope.updateUrl = $scope.url + '/UpdateLoanGiven';
    $scope.interplantList = [];

    $scope.sort = 'PartyName';
    $scope.partyType = 'Party';
    $scope.isAdvance = null;
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $controller('baseOpeningBalanceController', { $scope: $scope, $http: $http });
    $scope.openingBalance.PartyType = 'Party';
   
    $scope.sourceType = 'Loan';
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