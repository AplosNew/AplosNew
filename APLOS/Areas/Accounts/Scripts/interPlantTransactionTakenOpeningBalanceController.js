'use strict';
interPlantTransactionTakenOpeningBalanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller'];
function interPlantTransactionTakenOpeningBalanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $rootScope.title = 'Inter Plant Equity Opening Balance';
    $scope.partyType = 'Company';
    $scope.Action = 'Save';
    $scope.sourceType = 'InterTransaction';
    $scope.index = -1;
    $scope.openingBalanceDetailList = [];
    $scope.isEntityLevel = false;
    $scope.openingBalance = {
        CompanyGroupId: null,
        CompanyId: null,
        FiscalYearId: null,
        FiscalYearName: null,
        FiscalYearPeriodId: null,
        FiscalYearPeriodName: null,
        PostingDate: null,
        DocRefNo: null,
        DocDate: null,
        Narration: null,
        Remarks: null,
        IsPark: null,
        Active: null,
        EntityId: null,
        FinancingTypeId: null,
        BudgetMasterId: null,
        ActivityId: null,
        PartyId: null,
        PartyType: 'Plant',
        SourceTo: 'Director'
    };

    $scope.openingBalanceDetail = {
        OpeningBalanceId: null,
        BankMasterId: null,
        PartyId: null,
        PartyType: null,
        GLGeneralInfoId: null,
        GL: null,
        CurrencyId: null,
        DocRefNo: null,
        DocDate: null,
        Narration: null,
        Amount: 0,
        CompanyCurrencyId: null,
        CompanyCurrencyAmount: 0,
        CompanyGroupCurrencyId: null,
        CompanyGroupCurrencyAmount: 0,
        HardCurrencyId: null,
        HardCurrencyAmount: 0,
        LifeOfYear: 0,
        NoOfInstallmentPerYear: 0,
        NoOfPaidInstallment: 0,
        TotalNoOfInstallment: 0,
        ProfitRate: 0,
        SanctionAmount: 0,
        Active: true
    };

    baseService.init('accounts/OpeningBalance/GetInterPlantTransactionTakenList', null, null, 'DESC', 'PostingDate', 'PostingDate');
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

    cboService.getCboInterPlantFinancingType($scope.sourceType, function (result) {
        $scope.financingTypeList = result;
    });

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
        $scope.getCutOffDate();
    });

    $scope.getCutOffDate = function () {
        $http.get('accounts/OpeningBalance/GetACCCutOffDate')
            .then(function (response) {
                if (response.data !== null && !baseService.isUndefinedOrNull(response.data.CutOffDate)) {
                    $scope.openingBalance.PostingDate = response.data.CutOffDate;
                    $scope.openingBalance.PostingDate = $filter("dateFiltering")($scope.openingBalance.PostingDate);
                    $scope.isEntityLevel = response.data.IsEntityLevel;
                    if ($scope.isEntityLevel) {
                        cboService.getCboEntityByPlant(null, null, '', function (result) {
                            $scope.entityList = result;
                        });
                    }
                }
                else {
                    ShowResult('Opening Balance Cut Off date not found!', 'failure');
                }
            });
    };

    $scope.getById = function (index) {
        $scope.index = index;
        $scope.openingBalance = Object.assign({}, $scope.openingBalanceList[$scope.index]);
        $scope.openingBalance.PostingDate = $filter('dateFiltering')($scope.openingBalance.PostingDate);
        $scope.openingBalance.DocDate = $filter('dateFiltering')($scope.openingBalance.DocDate);
        $http.get('accounts/OpeningBalance/GetOpeningBalanceDetailList?openingBalanceId=' + $scope.openingBalance.Id)
            .then(function (response) {
                $scope.openingBalanceDetailList = response.data;
                angular.forEach($scope.openingBalanceDetailList, function (item, i) {
                    item.DocDate = $filter('dateFiltering')(item.DocDate);
                    item.RepaymentStartDate = $filter('dateFiltering')(item.RepaymentStartDate);
                    item.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    item.CompanyGroupCurrencyName = $scope.companyGroupCurrencyName;
                    item.CompanyGroupFromCurrencyId = $scope.companyGroupCurrencyId;
                    item.CompanyGroupToCurrencyId = $scope.companyCurrencyId;
                    item.HardCurrencyId = $scope.hardCurrencyId;
                    item.HardCurrencyName = $scope.hardCurrencyName;
                    item.HardFromCurrencyId = $scope.hardCurrencyId;
                    item.HardToCurrencyId = $scope.companyCurrencyId;
                });
            });
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $rootScope.searchByList = [
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

    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.tranCurrencyList = result;
    });

    $scope.advanceCA = null;
    $scope.getTransactionTypeGL = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            $http.get('accounts/Investment/GetFinancingTypeGL?id=' + id)
                .then(function (response) {
                    $scope.advanceCA = response.data;
                    if (manualValidation('div_TransactionType', baseService.isUndefinedOrNull($scope.advanceCA.LiabilityGLId), 'Transaction Type GL not found!')) {
                        $scope.advanceCA = null;
                    }
                    else if ($scope.companyConfig.IsVoucherFromBudget
                        && manualValidation('div_TransactionType', baseService.isUndefinedOrNull($scope.advanceCA.LiabilityBudgetMasterId), 'Transaction Type Budget not found!')) {
                        $scope.advanceCA = null;
                    }
                });
        }
        else {
            manualValidation('div_TransactionType', false, '');
            $scope.advanceCA = null;
        }
    };

    // Creating parallel currency table heading.
    $scope.parallelCurrencyTypeList = [];
    $http({
        method: 'GET',
        url: 'currencies/CompanyParallelCurrency/CurrencyParallel'
    }).then(function successCallback(response) {
        angular.forEach(response.data, function (item, i) {
            if (item.ParallelCurrencyType === 'CompanyCurrency') {
                $scope.companyCurrencyId = item.CurrencyId;
                $scope.companyCurrencyName = item.Code;
            }
            else if (item.ParallelCurrencyType === 'CompanyGroupCurrency') {
                $scope.companyGroupCurrencyId = item.CurrencyId;
                $scope.companyGroupCurrencyName = item.Code;
            }
            else if (item.ParallelCurrencyType === 'HardCurrency') {
                $scope.hardCurrencyId = item.CurrencyId;
                $scope.hardCurrencyName = item.Code;
            }
        });
    });

    $scope.copyAmount = function (index) {
        var data = $scope.openingBalanceDetailList[index];
        if (data.CurrencyId === $scope.companyCurrencyId) {
            data.CompanyCurrencyAmount = data.Amount;
        }
        if (data.CurrencyId === $scope.companyGroupCurrencyId) {
            data.CompanyGroupCurrencyAmount = data.Amount;
        }
        if (data.CurrencyId === $scope.hardCurrencyId) {
            data.HardCurrencyAmount = data.Amount;
        }
    };

    $scope.checkRowValidation = function (index) {
        var data = $scope.openingBalanceDetailList[index];
        if (new Date(data.DocDate) > new Date($scope.openingBalance.PostingDate)) {
            ShowResult('Doc date must be below or equal to Posting Date!', 'failure');
            data.DocDate = $scope.openingBalance.PostingDate;
        }
        if (data.CurrencyId === $scope.companyCurrencyId && data.Amount !== data.CompanyCurrencyAmount) {
            ShowResult('Trn. Amount and ' + $scope.companyCurrencyName + ' have to same!', 'failure');
        }
        if (data.CurrencyId === $scope.companyGroupCurrencyId && data.Amount !== data.CompanyGroupCurrencyAmount) {
            ShowResult('Trn. Amount and ' + $scope.companyGroupCurrencyName + ' have to same!', 'failure');
        }
        if (data.CurrencyId === $scope.hardCurrencyId && data.Amount !== data.HardCurrencyAmount) {
            ShowResult('Trn. Amount and ' + $scope.hardCurrencyName + ' have to same!', 'failure');
        }
    };

    function updateRow() {
        if ($scope.rowIndex !== -1 && $scope.glIndex !== -1) {
            var row = $scope.openingBalanceDetailList[$scope.rowIndex];
            var coa = $scope.cOAICodeList[$scope.glIndex];
            row.GLGeneralInfoId = coa.GLGeneralInfoId;
            row.GL = coa.GLGeneralInfoCode + ' - ' + coa.GLGeneralInfoName;
        }
        $scope.rowIndex = -1;
        $scope.glIndex = -1;
    }

    function addRow() {
        $scope.openingBalanceDetail.CompanyCurrencyId = $scope.companyCurrencyId;
        $scope.openingBalanceDetail.CompanyCurrencyName = $scope.companyCurrencyName;
        $scope.openingBalanceDetail.CompanyFromCurrencyId = $scope.companyCurrencyId;
        $scope.openingBalanceDetail.CompanyToCurrencyId = $scope.companyCurrencyId;

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
        clearOpeningBalanceDetail();
    }

    function clearOpeningBalanceDetail() {
        $scope.openingBalanceDetail = {};
        $scope.openingBalanceDetail.Active = true;
        $scope.openingBalanceDetail.Amount = 0;
        $scope.openingBalanceDetail.CompanyCurrencyAmount = 0;
        $scope.openingBalanceDetail.CompanyGroupCurrencyAmount = 0;
        $scope.openingBalanceDetail.HardCurrencyAmount = 0;
    }

    var invalidEntity = false;
    $scope.entityValidation = function () {
        invalidEntity = baseService.isUndefinedOrNull($scope.openingBalance.EntityId);
        return manualValidation('div_entity', invalidEntity, 'Entity is required.');
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

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $scope.checkDocDate('div_DocDate', $scope.openingBalance.DocDate);
        if ($scope.isEntityLevel) {
            $scope.entityValidation();
        }

        if ($scope.form1.$valid & !$scope.invalidDocDate && !invalidEntity) {
            if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'accounts/OpeningBalance/UpdateInterPlantTransactionTaken',
                    data: {
                        'openingBalance': $scope.openingBalance,
                        'openingBalanceDetailVMList': $scope.openingBalanceDetailList
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
    };

    $scope.clearFields = function () {
        $scope.Action = 'Save';
        $scope.openingBalance = {
            DocDate: null
            , DocRefNo: null
            , Narration: null
            , EntityId: null
            , FinancingTypeId: null
            , PartyType: 'Plant'
        };
        $scope.openingBalanceDetailList = [];
        clearOpeningBalanceDetail();
    };
}