'use strict';
employeeAdvanceOpeningBalanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller'];
function employeeAdvanceOpeningBalanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $rootScope.title = 'Employee Advance Opening Balance';
    $scope.url = 'accounts/OpeningBalance';
    $scope.listUrl = $scope.url + '/GetEmployeeAdvanceList';
    $scope.saveUrl = $scope.url + '/InsertEmployeeAdvance';
    $scope.updateUrl = $scope.url + '/UpdateEmployeeAdvance';
    $scope.interplantList = [];

    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    $controller('baseOpeningBalanceController', { $scope: $scope, $http: $http });

    cboService.getCboEmployeeTransactionType(function (result) {
        $scope.employeeTransactionTypeList = result;
        if ($scope.employeeTransactionTypeList.length === 1) {
            $scope.openingBalance.EmployeeTransactionTypeId = $scope.employeeTransactionTypeList[0].EmployeeTransactionTypeId;
        }
    });

    $scope.advanceCA = null;
    $scope.getTransactionTypeGL = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            $scope.advanceCA = $.grep($scope.employeeTransactionTypeList, function (item) {
                return item.EmployeeTransactionTypeId === id;
            })[0];
            if (manualValidation('div_TransactionType', baseService.isUndefinedOrNull($scope.advanceCA.AdvanceGLId), 'Transaction Type GL not found!')) {
                $scope.advanceCA = null;
                $scope.openingBalanceDetailList = [];
            }
            else if ($scope.companyConfig.IsVoucherFromBudget
                && manualValidation('div_TransactionType', baseService.isUndefinedOrNull($scope.advanceCA.AdvanceBudgetMasterId), 'Transaction Type Budget not found!')) {
                $scope.advanceCA = null;
                $scope.openingBalanceDetailList = [];
            }
        }
        else {
            manualValidation('div_TransactionType', false, '');
            $scope.advanceCA = null;
        }
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.openingBalanceDetail.EmployeeCode = employee.EmployeeCode;
            $scope.openingBalanceDetail.EmployeeName = employee.EmployeeName;
            $scope.openingBalanceDetail.EmployeeId = employee.SystemId;
            $scope.openingBalanceDetail.CurrencyId = $scope.companyCurrencyId;
            $scope.openingBalanceDetail.GLGeneralInfoId = null;
            $scope.openingBalanceDetail.GL = null;
            $scope.openingBalanceDetail.PartyType = 'Employee';

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

            $scope.openingBalanceDetail.DocDate = $scope.openingBalance.DocDate;
            $scope.openingBalanceDetail.DocRefNo = $scope.openingBalance.DocRefNo;
            $scope.openingBalanceDetail.Narration = $scope.openingBalance.Narration;
            $scope.openingBalanceDetailList.splice(0, 0, $scope.openingBalanceDetail);
            $scope.clearOpeningBalanceDetail();
        }
        $scope.hidePartyPopUp();
    };
}