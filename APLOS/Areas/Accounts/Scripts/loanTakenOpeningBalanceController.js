'use strict';
loanTakenOpeningBalanceController.$inject = ['cboService', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function loanTakenOpeningBalanceController(cboService, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = 'Loan Taken Opening Balance';
    $scope.url = 'accounts/OpeningBalance';
    $scope.listUrl = $scope.url + '/GetLoanTakenList';
    $scope.saveUrl = $scope.url + '/InsertLoanTaken';
    $scope.updateUrl = $scope.url + '/UpdateLoanTaken';
    $scope.interplantList = [];

    $scope.sort = 'PartyName';
    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $controller('baseOpeningBalanceController', { $scope: $scope, $http: $http });
    $scope.openingBalance.PartyType = 'Party';
    $scope.partyType = 'Party';
    $scope.isAdvance = null;
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $scope.sourceType = 'Loan';
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

    $scope.showDirectorPopUp = function () {
        baseService.setCurrentPage('partyList');
        $scope.getPartyList = function (pageno) {
            $scope.partyUrl = 'Parties/party/GetCompanyDirectorDataList';
            baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
                .then(function (result) {
                    $scope.partyList = result.Rows;
                    $scope.partyParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#directorPopUp')).modal('show');
        $scope.getPartyList();
    };
    $scope.closeDirectorPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            
            $scope.openingBalanceDetail.GLGeneralInfoId = party.ReconciliationGLId;
            $scope.openingBalanceDetail.GLGeneralInfoName = party.ReconciliationGLCode + ' - ' + party.ReconciliationGLName;
            $scope.openingBalanceDetail.BudgetMasterId = party.ReconciliationBudgetId;
            $scope.openingBalanceDetail.BudgetName = party.ReconciliationBudgetCode + ' - ' + party.ReconciliationBudgetName;
            $scope.openingBalanceDetail.ActivityId = party.ReconciliationActivityId;
            $scope.openingBalanceDetail.ActivityName = party.ReconciliationActivityCode + ' - ' + party.ReconciliationActivityName;

            $scope.openingBalanceDetail.PartyId = party.Id;
            $scope.openingBalanceDetail.PartyCode = party.Code;
            $scope.openingBalanceDetail.PartyName = party.UserName;
            $scope.openingBalanceDetail.PartyType = $scope.partyType;
            $scope.openingBalanceDetail.CurrencyId = party.CurrencyId;

            $scope.openingBalanceDetail.DocDate = $scope.openingBalance.DocDate;
            $scope.openingBalanceDetail.DocRefNo = $scope.openingBalance.DocRefNo;
            $scope.openingBalanceDetail.Narration = $scope.narration;

            $scope.openingBalanceDetail.EntityId = $scope.openingBalance.EntityId;
            $scope.openingBalanceDetail.PlantId = $scope.openingBalance.PlantId;

            $scope.openingBalanceDetail.CompanyCurrencyId = $scope.companyCurrencyId;
            $scope.openingBalanceDetail.CompanyFromCurrencyId = $scope.companyCurrencyId;
            $scope.openingBalanceDetail.CompanyCurrencyName = $scope.companyCurrencyName;
            $scope.openingBalanceDetail.ToCurrencyId = $scope.companyCurrencyId;

            $scope.openingBalanceDetail.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
            $scope.openingBalanceDetail.CompanyGroupFromCurrencyId = $scope.companyGroupCurrencyId;
            $scope.openingBalanceDetail.CompanyGroupCurrencyName = $scope.companyGroupCurrencyName;

            $scope.openingBalanceDetail.HardCurrencyId = $scope.hardCurrencyId;
            $scope.openingBalanceDetail.HardFromCurrencyId = $scope.hardCurrencyId;
            $scope.openingBalanceDetail.HardCurrencyName = $scope.hardCurrencyName;
            $scope.openingBalanceDetail.PartyPlantId = $scope.PartyPlantId;

            $scope.openingBalanceDetail.LifeOfYear = 0;
            $scope.openingBalanceDetail.NoOfInstallmentPerYear = 0;
            $scope.openingBalanceDetail.NoOfPaidInstallment = 0;
            $scope.openingBalanceDetail.TotalNoOfInstallment = 0;
            $scope.openingBalanceDetail.ProfitRate = 0;
            $scope.openingBalanceDetail.SanctionAmount = 0;

            $scope.openingBalanceDetailList.splice(0, 0, $scope.openingBalanceDetail);
            $scope.clearOpeningBalanceDetail();
        }
        $scope.hideDirectorPopUp();
    };
    $scope.hideDirectorPopUp = function () {
        angular.element(document.querySelector('#directorPopUp')).modal('hide');

    }

    $scope.closePartyPopUp = function (x) {
            var party = x.data;
            $scope.getPartyPlantListWithCallBack(party.Id, function (result) {
                if ($scope.partyGLType !== "DownPayment" && !baseService.isUndefinedOrNull($scope.partyGLType)) {
                    if (baseService.isUndefinedOrNull(party.ReconciliationGLId)) {
                        ShowResult($scope.partyType + ' GL not found!', 'failure', 'partyPopUp');
                        return;
                    }
                    else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.ReconciliationBudgetId)) {
                        ShowResult($scope.partyType + ' Budget not found!', 'failure', 'partyPopUp');
                        return;
                    }
                    else {
                        $scope.openingBalanceDetail.GLGeneralInfoId = party.ReconciliationGLId;
                        $scope.openingBalanceDetail.GLGeneralInfoName = party.ReconciliationGLCode + ' - ' + party.ReconciliationGLName;
                        $scope.openingBalanceDetail.BudgetMasterId = party.ReconciliationBudgetId;
                        $scope.openingBalanceDetail.BudgetName = party.ReconciliationBudgetCode + ' - ' + party.ReconciliationBudgetName;
                        $scope.openingBalanceDetail.ActivityId = party.ReconciliationActivityId;
                        $scope.openingBalanceDetail.ActivityName = party.ReconciliationActivityCode + ' - ' + party.ReconciliationActivityName;
                    }
                }
                else if ($scope.partyGLType === "DownPayment") {
                    if (baseService.isUndefinedOrNull(party.DownPaymentGLId)) {
                        ShowResult($scope.partyType + ' GL not found!', 'failure', 'partyPopUp');
                        return;
                    }
                    else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.DownPaymentBudgetId)) {
                        ShowResult($scope.partyType + ' Budget not found!', 'failure', 'partyPopUp');
                        return;
                    }
                    else {
                        $scope.openingBalanceDetail.GLGeneralInfoId = party.DownPaymentGLId;
                        $scope.openingBalanceDetail.GLGeneralInfoName = party.DownPaymentGLCode + ' - ' + party.DownPaymentGLName;
                        $scope.openingBalanceDetail.BudgetMasterId = party.DownPaymentBudgetId;
                        $scope.openingBalanceDetail.BudgetName = party.DownPaymentBudgetCode + ' - ' + party.DownPaymentBudgetName;
                        $scope.openingBalanceDetail.ActivityId = party.DownPaymentActivityId;
                        $scope.openingBalanceDetail.ActivityName = party.DownPaymentActivityCode + ' - ' + party.DownPaymentActivityName;
                    }
                } else {
                    $scope.openingBalanceDetail.GLGeneralInfoId = null;
                    $scope.openingBalanceDetail.GLGeneralInfoName = null;
                    $scope.openingBalanceDetail.BudgetMasterId = null;
                    $scope.openingBalanceDetail.BudgetName = null;
                    $scope.openingBalanceDetail.ActivityId = null;
                    $scope.openingBalanceDetail.ActivityName = null;
                }

                $scope.openingBalanceDetail.PartyId = party.Id;
                $scope.openingBalanceDetail.PartyCode = party.Code;
                $scope.openingBalanceDetail.PartyName = party.UserName;
                $scope.openingBalanceDetail.PartyType = $scope.partyType;
                $scope.openingBalanceDetail.CurrencyId = party.CurrencyId;

                $scope.openingBalanceDetail.DocDate = $scope.openingBalance.DocDate;
                $scope.openingBalanceDetail.DocRefNo = $scope.openingBalance.DocRefNo;
                $scope.openingBalanceDetail.Narration = $scope.narration;

                $scope.openingBalanceDetail.EntityId = $scope.openingBalance.EntityId;
                $scope.openingBalanceDetail.PlantId = $scope.openingBalance.PlantId;

                $scope.openingBalanceDetail.CompanyCurrencyId = $scope.companyCurrencyId;
                $scope.openingBalanceDetail.CompanyFromCurrencyId = $scope.companyCurrencyId;
                $scope.openingBalanceDetail.CompanyCurrencyName = $scope.companyCurrencyName;
                $scope.openingBalanceDetail.ToCurrencyId = $scope.companyCurrencyId;

                $scope.openingBalanceDetail.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                $scope.openingBalanceDetail.CompanyGroupFromCurrencyId = $scope.companyGroupCurrencyId;
                $scope.openingBalanceDetail.CompanyGroupCurrencyName = $scope.companyGroupCurrencyName;

                $scope.openingBalanceDetail.HardCurrencyId = $scope.hardCurrencyId;
                $scope.openingBalanceDetail.HardFromCurrencyId = $scope.hardCurrencyId;
                $scope.openingBalanceDetail.HardCurrencyName = $scope.hardCurrencyName;
                $scope.openingBalanceDetail.PartyPlantId = $scope.PartyPlantId;

                $scope.openingBalanceDetail.LifeOfYear = 0;
                $scope.openingBalanceDetail.NoOfInstallmentPerYear = 0;
                $scope.openingBalanceDetail.NoOfPaidInstallment = 0;
                $scope.openingBalanceDetail.TotalNoOfInstallment = 0;
                $scope.openingBalanceDetail.ProfitRate = 0;
                $scope.openingBalanceDetail.SanctionAmount = 0;

                $scope.openingBalanceDetailList.splice(0, 0, $scope.openingBalanceDetail);
                $scope.clearOpeningBalanceDetail();
            });
        $scope.hidePartyPopUp();
    };

}