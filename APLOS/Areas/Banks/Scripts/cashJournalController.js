"use strict";
cashJournalController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "bankService"];
function cashJournalController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, bankService) {
    $rootScope.title = "Cash Journal";
    $scope.voucherDetailList = [];
    $scope.hideSource = true;
    $scope.Action = "Save";
    $scope.voucherDetailCurrencyList = [];
    $scope.url = "Banks/CashJournal";
    $scope.listUrl = $scope.url + "/GetCashJournalList";
    $scope.saveUrl = $scope.url + "/InsertCashJournal";
    $scope.updateUrl = $scope.url + "/UpdateCashJournal";
    $scope.postUrl = $scope.url + "/PostCashJournal";
    $scope.deleteUrl = $scope.url + "/DeleteCashJournal";
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });

    $scope.voucher = {
        Id: null,
        CurrencyId: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        BankTransactionDate: null,
        BankReferenceNo: null,
        DocDate: null,
        DocRefNo: null,
        Amount: null,
        Narration: null,
        BankName: null,
        BankMasterId: null,
        BankCurrencyId: null,
        BankAmount: null,
        BankAccountNumber: null,
        PaymentSource: "Cash",
        GLGeneralInfoId: null,
        GLGeneralInfoCode: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetCode: null,
        BudgetName: null,
        ActivityId: null,
        ActivityCode: null,
        ActivityName: null,
        FinancingTypeId: null,
        BankJournalType: "CashToCash"
    };

    $scope.voucherDetail = {
        Id: null,
        GLGeneralInfoId: null,
        GLGeneralInfoCode: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetCode: null,
        BudgetName: null,
        ActivityId: null,
        ActivityCode: null,
        ActivityName: null,
        Amount: null,
        NetAmount: null,
        CompanyCurrencyAmount: null
    };

    baseService.init($scope.listUrl, null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
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

    $scope.searchByList = [
        {
            "name": "VoucherNo",
            "value": "VoucherNo"
        },
        {
            "name": "Posting Date",
            "value": "PostingDate"
        },
        {
            "name": "Doc Date",
            "value": "DocDate"
        },
        {
            "name": "Doc Ref",
            "value": "DocRefNo"
        },
        {
            "name": "Currency",
            "value": "Currency"
        },
        {
            "name": "Status",
            "value": "Status"
        }
    ];

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
        cboService.getCboEntityByPlant(null, null, "", function (result) {
            $scope.entityList = result;
        });
    });

    $scope.SelectedBudgetItem = function (id) {
        $scope.voucherDetail.BudgetName = $("#budgetid option:selected").text();
        $scope.voucherDetail.BudgetMasterId = id;
        $scope.getActivity(id);
    };

    $scope.SelectedActivityItem = function (id) {
        $scope.voucherDetail.ActivityName = $("#activityid option:selected").text();
        $scope.voucherDetail.ActivityId = id;
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

    $scope.convertAmount = function () {
        $scope.voucher.Amount = 0;
        var vdetailList = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr" });
        var vdetailCurrencyList = $filter("filter")($scope.voucherDetailCurrencyList, { "TrnType": "Dr" });
        angular.forEach(vdetailList, function (item, i) {
            $scope.voucher.Amount += parseFloat(item.DrAmount);
        });
        angular.forEach(vdetailCurrencyList, function (item, i) {
            $scope.CurrencyAmount += parseFloat(item.CompanyCurrencyDr);
        });
        var getRowDrData = $filter("filter")($scope.voucherDetailCurrencyList, { "TrnType": "Cr" });
        $scope.CurrencyAmount = 0;
    };

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.voucher.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else if (baseService.isUndefinedOrNull($scope.voucher.DocDate)) {
            msg = "Doc Date is required.";
            $scope.invalidDocDate = true;
        }
        else {
            $scope.invalidDocDate = false;
            $scope.GetCurrencyExchangeRateList();
        }
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    };

    $scope.invalidPostingDate = false;
    $scope.checkPostingDate = function () {
        var msg = "";
        if (new Date($scope.voucher.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.invalidPostingDate = true;
        }
        else if (new Date($scope.voucher.PostingDate) < new Date($scope.voucher.DocDate)) {
            msg = "Posting date must be below or equal to Doc Date!";
            $scope.invalidPostingDate = true;
        }
        else if (baseService.isUndefinedOrNull($scope.voucher.PostingDate)) {
            msg = "Posting Date is required.";
            $scope.invalidPostingDate = true;
        }
        else {
            $scope.invalidPostingDate = false;
            $scope.GetCurrencyExchangeRateList();
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    $scope.addRow = function (data) {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure", "GLPopUp");
            return true;
        }
        if ($scope.companyConfig.IsVoucherFromBudget)
            var getRow = $filter("filter")($scope.voucherDetailList, { "TrnType": "Dr", "BudgetMasterId": data.BudgetMasterId, "ActivityId": data.ActivityId, });

        if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0 && getRow[0].BudgetMasterId === data.BudgetMasterId) {
            ShowResult("This Activity is already added!", "failure", "GLPopUp");
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

            $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
            $scope.voucherDetail.Narration = $scope.voucher.Narration;
            $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
            $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
            $scope.voucherDetail.CrAmount = 0;
            $scope.voucherDetail.DrAmount = 0;
            $scope.voucherDetail.TrnType = "Dr";
            $scope.voucherDetailList.push($scope.voucherDetail);
            $scope.voucherDetail = {};
            $scope.closeCOAICodeListPopUp();
        }
    };

    $scope.removeDrRow = function () {
        var dr = $scope.voucherDetailList.length;
        while (dr--) {
            if ($scope.voucherDetailList[dr]["TrnType"] === "Dr") {
                $scope.voucherDetailList.splice(dr, 1);
            }
        }
        var drc = $scope.voucherDetailCurrencyList.length;
        while (drc--) {
            if ($scope.voucherDetailCurrencyList[drc]["TrnType"] === "Dr") {
                $scope.voucherDetailCurrencyList.splice(drc, 1);
            }
        }
    };


    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher.VoucherId = null;
        $scope.voucher.Active = true;
        $scope.voucher.Narration = null;
        $scope.voucher.DocRefNo = null;
        $scope.voucher.Amount = null;
        $scope.voucher.CashMasterId = null;
        $scope.voucher.OtherCashMasterId = null;
        $scope.voucher.OtherBankMasterId = null;
        $scope.voucher.BankJournalType = "CashToCash";
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.getCboVoucherTypeCashJournalList();
        $scope.currencyExchangeRate = [];
        $scope.voucherDetailList = [];
    };

    $scope.save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
        if ($scope.form0.$valid && !$scope.invalidDocDate && !$scope.invalidPostingDate) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
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
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrl,
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
            }
            return true;
        }
    };

    $scope.advanceId = null;
    $scope.confirmPost = function (advanceId) {
        $scope.advanceId = advanceId;
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

    $scope.searchglByList = [
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "Ref No",
            "value": "RefNo"
        }
    ];

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoCode",
        searchBy: "ActivityName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.popUp = function () {
        $scope.customerInvoiceGLList = [];
        baseService.setCurrentPage("cOAICodeList");
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase("Accounts/GLItem/GetAllGLBudgetActivityPostingAutomaticOnly", pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure", "GLPopUp");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.GetCOAICodeListData();
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };
    $scope.closeCOAICodeListPopUpSelected = function (x) {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };

    $scope.setSelected = function (data) {
        $scope.addRow(data);
    };

    $scope.getcashJournalDetailList = function (voucherId, voucherDetailId) {
        $http({
            method: "GET",
            url: "accounts/voucher/GetCashJournalDetailList?voucherId=" + voucherId + "&voucherDetailId=" + voucherDetailId
        }).then(function successCallback(response) {
            $scope.cashJournalDetailList = response.data.Rows;
            for (var i = 0; i < $scope.cashJournalDetailList.length; i++) {
                $scope.voucherDetail.Id = $scope.cashJournalDetailList[i].VoucherDetailId;
                $scope.voucherDetail.VoucherId = $scope.cashJournalDetailList[i].VoucherId;
                $scope.voucherDetail.DocRefNo = $scope.cashJournalDetailList[i].DocRefNo;
                $scope.voucherDetail.Narration = $scope.cashJournalDetailList[i].Narration;
                $scope.voucherDetail.EntityId = $scope.cashJournalDetailList[i].EntityId;
                $scope.voucherDetail.GLGeneralInfoId = $scope.cashJournalDetailList[i].GLGeneralInfoId;
                $scope.voucherDetail.GLGeneralInfoCode = $scope.cashJournalDetailList[i].GLGeneralInfoCode;
                $scope.voucherDetail.GLGeneralInfoName = $scope.cashJournalDetailList[i].GLGeneralInfoName;
                $scope.voucherDetail.BudgetMasterId = $scope.cashJournalDetailList[i].BudgetMasterId;
                $scope.voucherDetail.BudgetCode = $scope.cashJournalDetailList[i].BudgetCode;
                $scope.voucherDetail.BudgetName = $scope.cashJournalDetailList[i].BudgetName;
                $scope.voucherDetail.ActivityId = $scope.cashJournalDetailList[i].ActivityId;
                $scope.voucherDetail.ActivityCode = $scope.cashJournalDetailList[i].ActivityCode;
                $scope.voucherDetail.ActivityName = $scope.cashJournalDetailList[i].ActivityName;
                $scope.voucherDetail.TrnType = "Dr";
                $scope.voucherDetail.DrAmount = $scope.cashJournalDetailList[i].DrAmount;
                $scope.voucherDetail.CrAmount = null;
                $scope.voucherDetailList.push($scope.voucherDetail);
                $scope.voucherDetail.Id = null;
                $scope.voucherDetail.GLGeneralInfoName = $scope.cashJournalDetailList[i].GLGeneralInfoName;
                $scope.voucherDetail.Id = $scope.cashJournalDetailList[i].VoucherDetailCurrencyId;
                $scope.voucherDetail.VoucherDetailId = $scope.cashJournalDetailList[i].VoucherDetailId;
                $scope.voucherDetail.ToCurrencyId = $scope.cashJournalDetailList[i].ToCurrencyId;
                $scope.voucherDetail.CompanyCurrencyDr = $scope.cashJournalDetailList[i].CompanyCurrencyDr;
                $scope.voucherDetailCurrencyList.push($scope.voucherDetail);
            }
        });
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.voucher = $scope.cashJournalList[$scope.index];
        $scope.voucherDetailList.push($scope.voucher);
        $scope.voucherDetailCurrencyList.push($scope.voucher);
        $scope.voucherDetail = {};
        $scope.getcashJournalDetailList($scope.voucher.VoucherId, $scope.voucher.VoucherDetailId);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.getCboVoucherTypeCashJournalList = function () {
        bankService.getCboVoucherTypeCashJournalList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.BankTransactionDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
                $scope.GetCurrencyExchangeRateList();
            }
        });
    };
    $scope.getCboVoucherTypeCashJournalList();

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.voucher.VoucherTypeId = data.Value;
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.voucher.DocDate = $scope.voucher.PostingDate;
    };

    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });

    $scope.changeEntityForBankCash = function (entityId) {
        bankService.getBankMasterHouseBankCboListByEntity(entityId, function (result) {
            $scope.bankMasterList = result;
        });

        bankService.getCashMasterCboListByEntity(entityId, function (result) {
            $scope.cashMasterList = result;
        });
    };

    $scope.onCashChange = function (cashMasterId) {
        var cash = $.grep($scope.cashMasterList, function (item) {
            return item.Id === cashMasterId;
        })[0];
        $scope.voucher.CurrencyId = cash.CurrencyId;
        $scope.GetCurrencyExchangeRateList();
    };

    $scope.Get = function (data) {
        $scope.voucher = data;
        $scope.voucher.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
        $scope.voucher.VoucherDate = $filter("dateFiltering")($scope.voucher.VoucherDate);
        $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucher.PostingDate);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $http({
            method: "GET",
            url: "Banks/CashJournal/GetBankJournalDetailList/" + $scope.voucher.Id
        }).then(function successCallback(response) {
            $scope.voucherDetailList = response.data;
        });
    };
    $scope.clearBankPopUp = function () {
        $scope.voucher.OtherBankMasterId = null;
        $scope.voucher.OtherCashMasterId = null;
        $scope.voucher.FinancingTypeId = null;
        $scope.voucherDetailList = [];
    }


    //$scope.removeRow = function (index) {
    //    var row = $scope.voucherDetailList[index];
    //    $scope.voucherDetailList.splice(index, 1);
    //};

    $scope.removeDetaillRow = function (Id, voucherId, voucherDetailId, index) {
        if (Id === null) {
            //$(this).remove();
            $scope.voucherDetailList.splice(index, 1);
            return false;
        }
        else {
            $scope.message = 'Are you sure want to permanently delete this?';
            angular.element(document.querySelector('#removePopUp')).modal('show');
            $scope.vdId = voucherDetailId;
            $scope.voucherId = voucherId;
            $scope.cashJournalDetailId = Id;
            $scope.mateIndex = index;
        }
    };

    $scope.detailDelete = function () {
        try {
            $http({
                method: 'POST',
                url: 'Banks/CashJournal/DeleteCashJournalDetail?Id=' + $scope.vdId + '&voucherId=' + $scope.voucherId + '&cashJournalDetailId=' + $scope.cashJournalDetailId,
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.vdId = null;
                    $scope.voucherId = null;
                    $scope.cashJournalDetailId = null;
                    $scope.voucherDetailList.splice($scope.mateIndex, 1);
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'success');
        }
    };

    $scope.delete = function (cashJournalId, voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "cashJournalId": cashJournalId, "voucherId": voucherId
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
                $scope.cashJournalId = null;
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.invoiceId = null;
    $scope.confirmDelete = function (cashJournalId, voucherId) {
        $scope.cashJournalId = cashJournalId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };
}