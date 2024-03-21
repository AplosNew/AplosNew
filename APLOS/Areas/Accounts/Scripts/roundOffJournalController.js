"use strict";
roundOffJournalController.$inject = ["accountService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function roundOffJournalController(accountService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Round Off Journal";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherDetailList = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.voucherList = [];
    $scope.isCrBankAmount = false;
    $scope.isDrBankAmount = false;
    $scope.currencyDisable = false;
    $scope.isAdvance = true;
    $scope.postUrl = "accounts/voucher/PostRoundOffJournal";
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("partyBaseController", { $scope: $scope, $http: $http });

    $scope.voucherDetailList = [];

    $scope.searchByList = [
        {
            "name": "VoucherNo",
            "value": "VoucherNo"
        },
        {
            "name": "PostingDate",
            "value": "PostingDate"
        },
        {
            "name": "Doc Ref No",
            "value": "DocRefNo"
        }
        ,
        {
            "name": "Voucher Type",
            "value": "VoucherType"
        }
    ];


    baseService.init("Accounts/Voucher/GetJournalVoucherList", null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.voucherList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.voucher = {
        Id: null,
        PartyId: null,
        PartyName: null,
        PartyType: null,
        CurrencyId: null,
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        DocDate: null,
        DocRefNo: null,
        Amount: 0,
        Narration: null,
        EmployeeTransactionTypeName: null,
        CompanyCurrencyRate: 1,
        TransactionType:'Dr'
    };

    $scope.voucherDetail = {
        Id: null,
        GLGeneralInfoId: null,
        BudgetMasterId: null,
        ActivityId: null,
        COAICode: null,
        AccountTypeId: null,
        CurrencyId: null,
        DocRefNo: null,
        DrAmount: null,
        CrAmount: null,
        Narration: null,
        BankMasterId: null,
        CashMasterId: null,
        PartyId: null,
        PartyPlantId: null
    };

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
       
            cboService.getCboEntityByPlant(null, null, "", function (result) {
                $scope.entityList = result;
            });
    });

    cboService.getCboTransactionCurrencyByCompany("", function (result) {
        $scope.tranCurrencyList = result;
        $scope.baseCurrencyId = $scope.selectBaseCurrency();
        $scope.voucher.CurrencyId = $scope.baseCurrencyId;
        $scope.GetCurrencyExchangeRateList();
    });

    $scope.getCboVoucherTypeJournalVoucherList = function () {
        accountService.getCboVoucherTypeJournalVoucherList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
                $scope.GetCurrencyExchangeRateList();
            }
        });
    };
    $scope.getCboVoucherTypeJournalVoucherList();

    $scope.getFinancingType = function () {
        cboService.getCboOtherFinancingType('Rounding', function (result) {
            $scope.financingTypeList = result;
        });
    }
    $scope.getFinancingType();
   
    $scope.trailBalanceRoundOffList = [];
    $scope.GetTrailBalanceRoundOff = function () {
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Accounts/Voucher/GetTrailBalanceRoundOffList?trnType=' + $scope.voucher.TransactionType,
        }).then(function successCallback(response) {
            $scope.trailBalanceRoundOffList = response.data;
        });
        angular.element(document.querySelector('#trailBalanceRoundOffListPopUp')).modal('show');
    };

    $scope.closeItemPopUp = function () {
        angular.element(document.querySelector('#trailBalanceRoundOffListPopUp')).modal('hide');
    };
   
    $scope.clickCheckedTBItem = function () {
        try {
            for (var i = 0; i < $scope.trailBalanceRoundOffList.length; i++) {
                if ($scope.trailBalanceRoundOffList[i].Active) {
                    $scope.voucherDetailList.push($scope.trailBalanceRoundOffList[i]);
                }
            }
            angular.element(document.querySelector('#trailBalanceRoundOffListPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'info');
        }
    }

    $scope.getTransactionTypeGL = function (id) {
        var i = $scope.voucherDetailList.length;
        while (i--) {
            if ($scope.voucherDetailList[i]["Particulars"] === 'GL') {
                $scope.voucherDetailList.splice(i, 1);
            }
        }

        var data = $filter('filter')($scope.financingTypeList, { FinancingTypeId: id }, true);
                if (baseService.arrayLength(data) > 0) {
                    $scope.voucherDetail.GLGeneralInfoId = data[0].ExpensesGLId;
                    $scope.voucherDetail.GLGeneralInfoCode = data[0].ExpensesGLCode;
                    $scope.voucherDetail.GL = data[0].ExpensesGLName;
                    $scope.voucherDetail.BudgetMasterId = data[0].ExpensesBudgetMasterId;
                    $scope.voucherDetail.BudgetCode = data[0].ExpensesBudgetCode;
                    $scope.voucherDetail.Budget = data[0].ExpensesBudgetName;
                    $scope.voucherDetail.ActivityId = data[0].ExpensesActivityId;
                    $scope.voucherDetail.ActivityCode = data[0].ExpensesActivityCode;
                    $scope.voucherDetail.Activity = data[0].ExpensesActivityName;
                    $scope.voucherDetail.Particulars = 'GL';
                    $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                    $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                    $scope.voucherDetail.Narration = $scope.voucher.Narration;
                    $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                    $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                    $scope.voucherDetail.CrAmount = null;
                    $scope.voucherDetail.DrAmount = null;
                    $scope.voucherDetail.Id = null;
                    $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
                    $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                    $scope.voucherDetail = {};
                }
    };


    $scope.removeRow = function (index) {
        $scope.voucherDetailList.splice(index, 1);
    };

    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher.Active = true;
        $scope.voucher.Amount = 0;
        $scope.voucher.DocRefNo = null;
        $scope.voucher.Narration = null;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.currencyDisable = false;
        $scope.voucherDetailList = [];
    };
    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.form0.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "accounts/voucher/ParkRoundOffJournal",
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": $scope.voucherDetailList
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
            }
            return true;
        }
    };

    $scope.voucherId = null;
    $scope.confirmPost = function (voucherId) {
        $scope.voucherId = voucherId;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
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
                $scope.clear();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.deleteUrl = "accounts/voucher/DeleteRoundOffJV";
    $scope.delete = function (voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "voucherId": voucherId
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
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.confirmDelete = function (voucherId) {
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };
}