'use strict';
exchangeVoucherController.$inject = ["accountService", 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function exchangeVoucherController(accountService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = 'Exchange Voucher';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.voucherDetailList = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.voucherList = [];
    $scope.isCrBankAmount = false;
    $scope.isDrBankAmount = false;
    $scope.currencyDisable = false;
    $scope.postUrl = 'accounts/voucher/PostExchangeJournal';
    $controller('currencyBaseController', { $scope: $scope, $http: $http });
    $scope.partyType = "Customer";
    $scope.partyGLType = "Reconciliation";
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('bankBaseController', { $scope: $scope, $http: $http });
    $scope.voucherDetailList = [];

    $scope.searchvoucherList = [
        {
            'name': 'VoucherNo',
            'value': 'VoucherNo'
        },
        {
            'name': 'VoucherDate',
            'value': 'VoucherDate'
        },
        {
            'name': 'Doc Ref',
            'value': 'DocRefNo'
        }
    ];

    $scope.voucherListParameters = {
        limit: 10,
        offset: 0,
        order: 'desc',
        sort: 'VoucherNo',
        searchBy: "VoucherNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    baseService.init('Accounts/Voucher/GetExchangeVoucherList', null, null, 'DESC', 'PostingDate DESC, VoucherNo', 'PostingDate');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.voucherList = result.Rows;
                $scope.voucherListParameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.voucher = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PartyId: null,
        PartyName: null,
        PartyType: null,
        CurrencyId: null,
        PaymentTermId: null,
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter('dateFiltering')(Date.now()),
        PostingDate: null,
        DocDate: null,
        DocRefNo: null,
        Amount: null,
        BaseOnDueDate: null,
        BaseNoOfDays: null,
        Narration: null,
        Remarks: null,
        SourceFrom: null,
        SourceTo: null,

        DrBankCurrencyId: null,
        DrGLId: null,
        DrGLName: null,
        DrBudgetId: null,
        DrBudgetName: null,
        DrActivityId: null,
        DrActivityName: null,
        DrBankName: null,
        DrBankMasterId: null,
        DrBankAmount: 0,
        DrBankAccountNumber: null,

        CrGLId: null,
        CrGLName: null,
        CrBudgetName: null,
        CrBudgetId: null,
        CrActivityId: null,
        CrActivityName: null,
        CrBankCurrencyId: null,
        CrBankName: null,
        CrBankMasterId: null,
        CrBankAmount: 0,
        CrBankAccountNumber: null,
        ExchangeType: 'Loss'
    };

    $scope.voucherDetail = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        GLGeneralInfoId: null,
        COAICode: null,
        AccountTypeId: null,
        CurrencyId: null,
        DocRefNo: null,
        DrAmount: null,
        CrAmount: null,
        Narration: null,
        Remarks: null
    };

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
            cboService.getCboEntityByPlant(null, null, '', function (result) {
                $scope.entityList = result;
            });
    });

    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.tranCurrencyList = result;
        $scope.voucher.CurrencyId = $scope.selectBaseCurrency();
    });

    $scope.getCboVoucherTypeJournalVoucherList = function () {
        accountService.getCboVoucherTypeJournalVoucherList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter('dateFiltering')($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
                $scope.GetCurrencyExchangeRateList();
            }
        });
    }
    $scope.getCboVoucherTypeJournalVoucherList();

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.voucher.VoucherTypeId = data.Value;
        $scope.voucher.PostingDate = $filter('dateFiltering')(data.LastPostingDate);
        $scope.voucher.DocDate = $scope.voucher.PostingDate;
    };

    $scope.getPartyPlantList = function (partyId) {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.partyPlantList.push(item);
                });
            });
    };

    $scope.getExchangeVoucherDetailList = function (id) {
        $http({
            method: 'get',
            url: 'accounts/voucher/GetExchangeVoucherDetailList?voucherId=' + id
        }).then(function successCallback(response) {
            $scope.voucherDetailList = response.data;
            angular.forEach($scope.voucherDetailList, function (item, i) {
                $scope.getPartyPlantList(item.PartyId);
            });
        });
    };

    $scope.Get = function (data) {
        $scope.voucher.Id = data.Id;
        $scope.voucher.PostingDate = $filter('dateFiltering')(data.PostingDate);
        $scope.voucher.DocDate = $filter('dateFiltering')(data.DocDate);
        $scope.voucher.DocRefNo = data.DocRefNo;
        $scope.voucher.Narration = data.Narration;
        $scope.voucher.CurrencyId = data.CurrencyId;
        $scope.voucher.EntityId = data.EntityId;
        $scope.voucher.ExchangeType = data.ExchangeType;
        $scope.GetCurrencyExchangeRateList();
        $scope.currencyDisable = true;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.getExchangeVoucherDetailList($scope.voucher.Id);
    };

    $scope.addRow = function (data) {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult('Please select Currency!', 'failure', 'GLPopUp');
            return true;
        }
        if ($scope.companyConfig.IsVoucherFromBudget)
            var getRow = $filter('filter')($scope.voucherDetailList, { 'TrnType': 'Dr', 'BudgetMasterId': data.BudgetMasterId, 'ActivityId': data.ActivityId, });

        if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0 && getRow[0].BudgetMasterId === data.BudgetMasterId) {
            ShowResult('This Activity is already added!', 'failure', 'GLPopUp');
        }
        else {
            $scope.voucherDetail.BudgetMasterId = data.BudgetMasterId;
            $scope.voucherDetail.BudgetCode = data.BudgetCode;
            $scope.voucherDetail.BudgetName = data.BudgetName;
            $scope.voucherDetail.ActivityId = data.ActivityId;
            $scope.voucherDetail.ActivityCode = data.ActivityCode;
            $scope.voucherDetail.ActivityName = data.ActivityName;
            $scope.voucherDetail.GLGeneralInfoId = data.GLGeneralInfoId;
            $scope.voucherDetail.GLGeneralInfoCode = data.GLGeneralInfoCode;
            $scope.voucherDetail.GLGeneralInfoName = data.GLGeneralInfoName;
            $scope.voucherDetail.DocDate = $filter('dateFiltering')($scope.voucher.DocDate);
            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
            $scope.voucherDetail.Narration = $scope.voucher.Narration;
            $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
            $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
            $scope.voucherDetail.CrAmount = 0;
            $scope.voucherDetail.DrAmount = 0;
            $scope.voucherDetailList.push($scope.voucherDetail);
            $scope.voucherDetail = {};
            $scope.closeCOAICodeListPopUp();
        }
    };

    $scope.removeRow = function (index) {
        $scope.voucherDetailList.splice(index, 1);
    };

    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            $scope.getPartyPlantListWithCallBack(party.Id, function (result) {
                if (baseService.isUndefinedOrNull(party.ReconciliationGLId)) {
                    ShowResult($scope.partyType + ' GL not found!', 'failure', 'partyPopUp');
                    return;
                }
                else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.ReconciliationBudgetId)) {
                    ShowResult($scope.partyType + ' Budget not found!', 'failure', 'partyPopUp');
                    return;
                }
                else {
                    $scope.voucherDetail.GLGeneralInfoId = party.ReconciliationGLId;
                    $scope.voucherDetail.GLGeneralInfoName = party.ReconciliationGLCode + ' - ' + party.ReconciliationGLName;
                    $scope.voucherDetail.BudgetMasterId = party.ReconciliationBudgetId;
                    $scope.voucherDetail.BudgetName = party.ReconciliationBudgetCode + ' - ' + party.ReconciliationBudgetName;
                    $scope.voucherDetail.ActivityId = party.ReconciliationActivityId;
                    $scope.voucherDetail.ActivityName = party.ReconciliationActivityCode + ' - ' + party.ReconciliationActivityName;
                    $scope.voucherDetail.PartyId = party.Id;
                    $scope.voucherDetail.PartyCode = party.Code;
                    $scope.voucherDetail.PartyName = party.UserName;
                    $scope.voucherDetail.PartyType = $scope.partyType;
                    $scope.voucherDetail.DocDate = $filter('dateFiltering')($scope.voucher.DocDate);
                    $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                    $scope.voucherDetail.Narration = $scope.voucher.Narration;
                    $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                    $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                    $scope.voucherDetail.Amount = null;
                    $scope.voucherDetailList.push($scope.voucherDetail);
                    $scope.voucherDetail = {};
                }
            });
        }
        $scope.hidePartyPopUp();
    };

    $scope.setSelected = function (data) {
        $scope.addRow(data);
    };
    $scope.checkDrAmount = function (index) {
        if ($scope.voucherDetailList[index].DrAmount > 0) {
            $scope.voucherDetailList[index].CrAmount = null;
        }
    };

    $scope.checkCrAmount = function (index) {
        if ($scope.voucherDetailList[index].CrAmount > 0) {
            $scope.voucherDetailList[index].DrAmount = null;
        }
    };

    $scope.getEntityCboByCostCenter = function (costCenterId) {
        $scope.voucherDetail.CostCenterName = $('#costCenterId option:selected').text();
        $scope.voucherDetail.CostCenterId = costCenterId;

        cboService.getCboEntityByCostCenter(costCenterId, function (result) {
            $scope.costCenterEntityList = result;
        });
    };

    $scope.SelectedCostCenterEntityItem = function (id) {
        $scope.voucherDetail.EntityName = $('#costcenterentityId option:selected').text();
        $scope.voucherDetail.EntityId = id;
    };

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.voucher.PostingDate) && !baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.voucher.PostingDate + "&currencyId=" + $scope.voucher.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.voucher.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = null;
        }
    };

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = '';
        if (new Date($scope.voucher.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = 'Doc date must be below or equal to current Date!';
        }
        else $scope.invalidDocDate = false;
        return manualValidation('div_DocDate', $scope.invalidDocDate, msg);
    };

    $scope.invalidPostingDate = false;
    $scope.checkPostingDate = function () {
        var msg = '';
        if (new Date($scope.voucher.PostingDate) > new Date()) {
            msg = 'Posting date must be below or equal to current Date!';
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else if (new Date($scope.voucher.PostingDate) < new Date($scope.voucher.DocDate)) {
            msg = 'Posting date must be below or equal to Doc Date!';
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        } else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation('div_PostingDate', $scope.invalidPostingDate, msg);
    };

    $scope.Clear = function () {
        $scope.Action = 'Save';
        $scope.voucher.Active = true;
        $scope.voucher.Amount = null;
        $scope.voucher.ExchangeType = 'Loss';
        $scope.voucher.DocRefNo = null;
        $scope.voucher.Narration = null;
        $scope.voucher.VoucherDate = $filter('date')(Date.now(), 'dd-MMM-yyyy');
        $scope.currencyDisable = false;
        $scope.currencyExchangeRate = [];
        $scope.voucherDetailList = [];
        $scope.getCboVoucherTypeJournalVoucherList();
    };

    $scope.changeExhangeType = function (type) {
        if (type === 'Loss') {
            $scope.voucher.ExchangeType = 'Gain';
        }
        if (type === 'Gain') {
            $scope.voucher.ExchangeType = 'Loss';
        }
    };

    $scope.validation = function () {
        if ($scope.voucherDetailList.length==0) {
            ShowResult('Please select Party!', 'failure');
            return true;
        }
        for (var i = 0; i < $scope.voucherDetailList.length; i++) {
            if ($scope.voucherDetailList[i].PartyPlantId == null) {
                ShowResult('Please select  Plant of ' + $scope.voucherDetailList[i].PartyName, 'failure');
                return true;
            }
        }
        return false;
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form0.$valid && !$scope.validation()) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'accounts/voucher/ParkExchangeVoucher',
                    data: {
                        'voucherVM': $scope.voucher,
                        'voucherDetailVMList': $scope.voucherDetailList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'accounts/voucher/UpdateExchangeVoucher',
                    data: {
                        'voucherVM': $scope.voucher,
                        'voucherDetailVMList': $scope.voucherDetailList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
            return true;
        }
    };

    $scope.voucherId = null;
    $scope.confirmPost = function (voucherId) {
        $scope.voucherId = voucherId;
        $scope.message_confirmation = 'Are you sure to Post?';
        angular.element(document.querySelector('#confirmPostPopUp')).modal('show');
    };

    $scope.post = function (id) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "id": id
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.Clear();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };
}