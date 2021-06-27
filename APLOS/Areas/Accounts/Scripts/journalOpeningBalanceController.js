'use strict';
journalOpeningBalanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', '$controller'];
function journalOpeningBalanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $controller) {
    $rootScope.title = 'Opening Balance Journal';
    $scope.Action = 'Save';
    $scope.CAction = 'Add';
    $scope.index = -1;
    $scope.openingBalanceDetailList = [];
    $scope.openingBalanceSummaryList = [];
    $scope.isEntityLevel = false;
    $scope.narration = null;
    $scope.source = 'Source';
    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $controller('fiscalYearBaseController', { $scope: $scope, $http: $http });

    $scope.voucher = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null,
        EntityId: null,
        VoucherTypeId: null,
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
        Active: null
    };

    $scope.openingBalanceDetail = {
        Id: null,
        OpeningBalanceId: null,
        GLGeneralInfoId: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetName: null,
        ActivityId: null,
        ActivityName: null,
        CurrencyId: null,
        EntityId: null,
        PlantId: null,
        PartyId: null,
        PartyName: null,
        PartyType: null,
        BankMasterId: null,
        BankName: null,
        DocRefNo: null,
        DocDate: null,
        Narration: null,
        BaseOnDueDate: null,
        BaseNoOfDays: null,
        Amount: 0,
        CompanyCurrencyId: null,
        CompanyCurrencyAmount: 0,
        CompanyGroupCurrencyId: null,
        CompanyGroupCurrencyAmount: 0,
        HardCurrencyId: null,
        HardCurrencyAmount: 0
    };

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
    });

    cboService.getCboVoucherTypeOpeningBalanceList(function (result) {
        $scope.voucherTypeList = result;
        if ($scope.voucherTypeList.length === 1) {
            $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
        }
    });

    baseService.init('accounts/OpeningBalance/GetJournalList', null, null, 'DESC', 'PostingDate', 'PostingDate');
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
            'name': 'Doc Ref No',
            'value': 'DocRefNo'
        }
    ];

    $http.get('accounts/OpeningBalance/GetACCCutOffDate')
        .then(function (response) {
            if (response.data !== null && !baseService.isUndefinedOrNull(response.data.CutOffDate)) {
                $scope.voucher.PostingDate = response.data.CutOffDate;
                $scope.voucher.PostingDate = $filter('dateFiltering')($scope.voucher.PostingDate);
                $scope.getFiscalYearPeriod($scope.voucher.PostingDate);
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

    $scope.entityChange = function (id) {
        var entityrowdata = $filter('filter')($scope.entityList, { Value: id });
        $scope.voucher.PlantId = entityrowdata[0].PlantId;
    };

    $http.get('accounts/OpeningBalance/GetSummaryData')
        .then(function (response) {
            $scope.openingBalanceSummaryList = response.data;
        });

    $http.get('accounts/OpeningBalance/GetAvailableForJournalList')
        .then(function (response) {
            $scope.openingBalanceDetailList = response.data;
            $scope.balanceCalculation();
        });

    //**************************************** GL List Start ***************************
    $scope.rowSelected = null;
    $scope.glIndex = -1;

    $scope.searchglByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        },
        {
            'name': 'Budget',
            'value': 'BudgetName'
        }
        , {
            'name': 'Activity',
            'value': 'ActivityName'
        }
    ];

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName',
        searchBy: 'GLGeneralInfoName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetCOAICodeList = function () {
        $scope.GLUrl1 = 'accounts/glitem/getgllist';
        if ($scope.companyConfig.IsVoucherFromBudget) {
            $scope.GLUrl1 = 'accounts/glitem/GetGLListWithBudget';
        }
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#GLPopUp')).modal('show');
        $scope.GetCOAICodeListData();
    };

    $scope.setSelected = function (x, index) {
        $scope.rowSelected = x.GLGeneralInfoCode + x.BudgetMasterId;
        $scope.glIndex = index;
    };

    $scope.closeCOAICodeListPopUp = function () {
        addRow();
    };

    $scope.closeCOAICodeListPopUpSelected = function () {
        addRow();
    };

    function addRow() {
        if ($scope.glIndex !== -1) {
            var coa = $scope.cOAICodeList[$scope.glIndex];
            if (baseService.isUndefinedOrNull(coa.GLGeneralInfoId)) {
                ShowResult('GL not found!', 'failure', 'GLPopUp');
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(coa.BudgetMasterId)) {
                ShowResult('Budget not found!', 'failure', 'GLPopUp');
                return;
            }
            else {
                $scope.openingBalanceDetailList.splice(0, 0, {
                    IsOB: false,
                    GLGeneralInfoName: coa.GLGeneralInfoCode + ' - ' + coa.GLGeneralInfoName,
                    GLGeneralInfoId: coa.GLGeneralInfoId,
                    BudgetMasterId: coa.BudgetMasterId,
                    BudgetName: coa.BudgetName,
                    ActivityId: coa.ActivityId,
                    ActivityName: coa.ActivityName,
                    DocDate: $scope.voucher.DocDate,
                    DocRefNo: $scope.voucher.DocRefNo,
                    EntityId: $scope.voucher.EntityId,
                    PlantId: $scope.voucher.PlantId,
                    Narration: $scope.voucher.Narration,
                    CurrencyId: $scope.companyCurrencyId,
                    TrnType: coa.BalanceType,
                    Amount: 0,
                    CompanyCurrencyAmount: 0,
                    CompanyGroupCurrencyAmount: 0,
                    HardCurrencyAmount: 0,
                    CompanyCurrencyId: $scope.companyCurrencyId,
                    CompanyFromCurrencyId: $scope.companyCurrencyId,
                    CompanyCurrencyName: $scope.companyCurrencyName,
                    ToCurrencyId: $scope.companyCurrencyId,

                    CompanyGroupCurrencyId: $scope.companyGroupCurrencyId,
                    CompanyGroupFromCurrencyId: $scope.companyGroupCurrencyId,
                    CompanyGroupCurrencyName: $scope.companyGroupCurrencyName,

                    HardCurrencyId: $scope.hardCurrencyId,
                    HardFromCurrencyId: $scope.hardCurrencyId,
                    HardCurrencyName: $scope.hardCurrencyName
                });
                $scope.balanceCalculation();
                angular.element(document.querySelector('#GLPopUp')).modal('hide');
                $scope.glIndex = -1;
            }
        }
    }

    $scope.balanceCalculation = function () {
        var companyCurrencyAmountDr = $filter('sumByKey')($filter('filter')($scope.openingBalanceDetailList, { TrnType: 'Debit' }), 'CompanyCurrencyAmount');
        var companyCurrencyAmountCr = $filter('sumByKey')($filter('filter')($scope.openingBalanceDetailList, { TrnType: 'Credit' }), 'CompanyCurrencyAmount');
        $scope.companyCurrencyAmount = Math.abs(companyCurrencyAmountDr - companyCurrencyAmountCr).toFixed(2);
        if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
            var companyGroupCurrencyAmountDr = $filter('sumByKey')($filter('filter')($scope.openingBalanceDetailList, { TrnType: 'Debit' }), 'CompanyGroupCurrencyAmount');
            var companyGroupCurrencyAmountCr = $filter('sumByKey')($filter('filter')($scope.openingBalanceDetailList, { TrnType: 'Credit' }), 'CompanyGroupCurrencyAmount');
            $scope.companyGroupCurrencyAmount = Math.abs(companyGroupCurrencyAmountDr - companyGroupCurrencyAmountCr).toFixed(2);
        }
        if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
            var hardCurrencyAmountDr = $filter('sumByKey')($filter('filter')($scope.openingBalanceDetailList, { TrnType: 'Debit' }), 'HardCurrencyAmount');
            var hardCurrencyAmountCr = $filter('sumByKey')($filter('filter')($scope.openingBalanceDetailList, { TrnType: 'Credit' }), 'HardCurrencyAmount');
            $scope.hardCurrencyAmount = Math.abs(hardCurrencyAmountDr - hardCurrencyAmountCr).toFixed(2);
        }
    };
    $scope.balanceCalculation();

    $scope.removeRow = function (index) {
        $scope.openingBalanceDetailList.splice(index, 1);
        $scope.balanceCalculation();
    };
    //**************************************** GL List End ***************************

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

    $scope.copyNarration = function (val) {
        $scope.narration = val;
    };

    var invalidRow = false;
    $scope.checkRowValidation = function (data, index) {
        if ($scope.checkDocDate('td_DocDate_' + index, data.DocDate)) {
            invalidRow = true;
        }
        else if (manualValidation('td_DocRef_' + index, baseService.isUndefinedOrNull(data.DocRefNo), 'Doc Ref is required.')) {
            invalidRow = true;
        }
        else if (manualValidation('td_Narration_' + index, baseService.isUndefinedOrNull(data.Narration), 'Narration is required.')) {
            invalidRow = true;
        }
        else if (manualValidation('td_CurrencyId_' + index, baseService.isUndefinedOrNull(data.CurrencyId), 'Currency is required.')) {
            invalidRow = true;
        }
        else if (manualValidation('td_Amount_' + index, baseService.isUndefinedOrNaN(data.Amount), 'Amount is required and must greater than 0.')) {
            invalidRow = true;
        }
        else if (!baseService.isUndefinedOrNull($scope.companyCurrencyId) && data.CurrencyId === $scope.companyCurrencyId && data.Amount !== data.CompanyCurrencyAmount) {
            manualValidation('td_CompanyCurrencyAmount_' + index, true, 'Trn. Amount and ' + $scope.companyCurrencyName + ' have to same!');
            invalidRow = true;
        }
        else if (!baseService.isUndefinedOrNull($scope.companyCurrencyId) && manualValidation('td_CompanyCurrencyAmount_' + index, baseService.isUndefinedOrNaN(data.CompanyCurrencyAmount), 'Amount is required and must greater than 0.')) {
            invalidRow = true;
        }
        else if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId) && data.CurrencyId === $scope.companyGroupCurrencyId && data.Amount !== data.CompanyGroupCurrencyAmount) {
            manualValidation('td_CompanyGroupCurrencyAmount_' + index, true, 'Trn. Amount and ' + $scope.companyGroupCurrencyName + ' have to same!');
            invalidRow = true;
        }
        else if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId) && manualValidation('td_CompanyGroupCurrencyAmount_' + index, baseService.isUndefinedOrNaN(data.CompanyGroupCurrencyAmount), 'Amount is required and must greater than 0.')) {
            invalidRow = true;
        }
        else if (!baseService.isUndefinedOrNull($scope.hardCurrencyId) && data.CurrencyId === $scope.hardCurrencyId && data.Amount !== data.HardCurrencyAmount) {
            manualValidation('td_HardCurrencyAmount_' + index, true, 'Trn. Amount and ' + $scope.hardCurrencyName + ' have to same!');
            invalidRow = true;
        }
        else if (!baseService.isUndefinedOrNull($scope.hardCurrencyId) && manualValidation('td_HardCurrencyAmount_' + index, baseService.isUndefinedOrNaN(data.HardCurrencyAmount), 'Amount is required and must greater than 0.')) {
            invalidRow = true;
        }
        else
            invalidRow = false;
        $scope.balanceCalculation();
    };

    var invalidDocDate = false;
    $scope.checkDocDate = function (controlId, val) {
        var msg = '';
        if (new Date(val) > new Date($scope.voucher.PostingDate)) {
            invalidDocDate = true;
            msg = 'Doc date must be below or equal to Posting Date!';
        }
        else if (baseService.isUndefinedOrNull($scope.voucher.DocDate)) {
            invalidDocDate = true;
            msg = 'Doc date is required.';
        }
        else invalidDocDate = false;
        return manualValidation(controlId, invalidDocDate, msg);
    };

    var invalidEntity = false;
    $scope.entityValidation = function () {
        invalidEntity = baseService.isUndefinedOrNull($scope.voucher.EntityId);
        return manualValidation('div_entity', invalidEntity, 'Entity is required.');
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $scope.checkDocDate('div_DocDate', $scope.voucher.DocDate);
        if ($scope.isEntityLevel) {
            $scope.entityValidation();
        }
        angular.forEach($filter('filter')($scope.openingBalanceDetailList, { IsOB: false }), function (item, i) {
            if (invalidRow) {
                return;
            }
            $scope.checkRowValidation(item, i);
        });
        var companyCurrencyAmountDr = $filter('sumByKey')($filter('filter')($scope.openingBalanceDetailList, { TrnType: 'Debit' }), 'CompanyCurrencyAmount');
        var companyCurrencyAmountCr = $filter('sumByKey')($filter('filter')($scope.openingBalanceDetailList, { TrnType: 'Credit' }), 'CompanyCurrencyAmount');
        if (companyCurrencyAmountDr !== companyCurrencyAmountCr) {
            ShowResult('Dr amount and Cr amount is not equal!', 'failure');
            return;
        }
        if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
            var companyGroupCurrencyAmountDr = $filter('sumByKey')($filter('filter')($scope.openingBalanceDetailList, { TrnType: 'Debit' }), 'CompanyGroupCurrencyAmount');
            var companyGroupCurrencyAmountCr = $filter('sumByKey')($filter('filter')($scope.openingBalanceDetailList, { TrnType: 'Credit' }), 'CompanyGroupCurrencyAmount');
            if (companyGroupCurrencyAmountDr !== companyGroupCurrencyAmountCr) {
                ShowResult('Dr amount and Cr amount is not equal!', 'failure');
                return;
            }
        }
        if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
            var hardCurrencyAmountDr = $filter('sumByKey')($filter('filter')($scope.openingBalanceDetailList, { TrnType: 'Debit' }), 'HardCurrencyAmount');
            var hardCurrencyAmountCr = $filter('sumByKey')($filter('filter')($scope.openingBalanceDetailList, { TrnType: 'Credit' }), 'HardCurrencyAmount');
            if (hardCurrencyAmountDr !== hardCurrencyAmountCr) {
                ShowResult('Dr amount and Cr amount is not equal!', 'failure');
                return;
            }
        }

        if ($scope.form1.$valid & !invalidDocDate && !invalidEntity && !invalidRow) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'accounts/OpeningBalance/InsertJournal',
                    data: {
                        'voucher': $scope.voucher,
                        'voucherDetailVMList': $scope.openingBalanceDetailList
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
                ShowResult('Update is not allowed.', 'failure');
            }
            return true;
        }
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.clearFields = function () {
        $scope.Action = 'Save';
        $scope.voucher.DocDate = null;
        $scope.voucher.DocRefNo = null;
        $scope.voucher.Narration = null;
        $scope.openingBalanceDetailList = [];
        $scope.openingBalanceSummaryList = [];
        clearOpeningBalanceDetail();
    };

    function clearOpeningBalanceDetail() {
        $scope.openingBalanceDetail = {};
        $scope.openingBalanceDetail.Active = true;
        $scope.openingBalanceDetail.Amount = 0;
        $scope.openingBalanceDetail.CompanyCurrencyAmount = 0;
        $scope.openingBalanceDetail.CompanyGroupCurrencyAmount = 0;
        $scope.openingBalanceDetail.HardCurrencyAmount = 0;
    }

    $scope.openingBalanceReport = function (voucherId) {
        location.href = 'accounts/openingbalance/openingbalancejournalreport?voucherId=' + voucherId;
    };
}