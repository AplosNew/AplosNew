"use strict";
customerAdvanceController.$inject = ["cboService", "bankService", "baseService", "factoryService", "commonMessage", "$scope", "$rootScope", "$http", "$filter", "$controller", "$routeParams"];
function customerAdvanceController(cboService, bankService, baseService, factoryService, commonMessage, $scope, $rootScope, $http, $filter, $controller, $routeParams) {
    $rootScope.title = "Customer Advance";
    $scope.Action = "Save";
    $scope.voucherDetailCurrency = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.voucherList = [];
    $scope.index = -1;
    $scope.url = 'accounts/Advance';
    $scope.listUrl = $scope.url + '/GetCustomerAdvanceList';
    $scope.parkUrl = $scope.url + '/ParkCustomerAdvance';
    $scope.updateUrl = $scope.url + '/UpdateCustomerAdvance';
    $scope.postUrl = $scope.url + '/PostCustomerAdvance';
    $scope.unPostUrl = $scope.url + '/UnPostCustomerAdvance';
    $scope.reportUrl = $scope.url + '/ReportCustomerAdvance?voucherId=';
    $scope.jouranlUrl = $scope.url + '/GetAvailableJournalCustomerAdvance';
    $scope.deleteUrl = $scope.url + "/DeleteVendorAdvance";
    $scope.partyType = "Customer";
    $scope.partyGLType = "DownPayment";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.hideSource = true;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $scope.isBankAmount = false;
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });

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
            "name": "Party Code",
            "value": "PartyCode"
        },
        {
            "name": "Customer",
            "value": "PartyName"
        },
        {
            "name": "Ordering Customer",
            "value": "PartyPlantName"
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

    $scope.advance = {
        Id: null,
        AdvanceId: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null,
        EntityId: null,
        PartyId: null,
        PartyCode: null,
        PartyName: null,
        PartyType: null,
        PartyPlantId: null,
        PartyPlantName: null,
        CurrencyId: null,
        PaymentTermId: null,
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        BankTransactionDate: null,
        BankReferenceNo: null,
        DocDate: null,
        DocRefNo: null,
        FiscalYearId: null,
        FiscalYearName: null,
        FiscalYearPeriodId: null,
        FiscalYearPeriodName: null,
        IsExcludingTax: false,
        Amount: 0,
        Narration: null,
        BankName: null,
        CashName: null,
        BankMasterId: null,
        CashMasterId: null,
        BankCurrencyId: null,
        BankAmount: 0,
        BankAccountNumber: null,
        PaymentSource: 'Bank',
        GLGeneralInfoId: null,
        GLGeneralInfoCode: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetCode: null,
        BudgetName: null,
        ActivityId: null,
        ActivityCode: null,
        ActivityName: null,
        EmployeeGLGeneralInfoId: null,
        EmployeeGLGeneralInfoName: null,
        EmployeeTransactionTypeId: null,
        FinancingTypeId: null,
        ResponsiblePersonId: null,
        ResponsiblePerson: null,
        IsInterTransaction: false,
        ContractId: null,
        ContractNo: null,
        MasterOrderId: null
    };

    $scope.advanceDetail = {
        Id: null,
        AdvanceId: null,
        PartyId: null,
        PartyCode: null,
        PartyName: null,
        PartyPlantId: null,
        PartyPlantName: null,
        PartyType: null,
        Narration: null,
        GLGeneralInfoId: null,
        GLGeneralInfoCode: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetCode: null,
        BudgetName: null,
        ActivityId: null,
        ActivityCode: null,
        ActivityName: null,
        Amount: 0,
        TaxAmount: 0,
        NetAmount: 0
    };

    $scope.advanceDetailList = [];

    $scope.voucherDetailCurrency = {
        Id: null,
        VoucherId: null,
        VoucherDetailId: null,
        ParallelCurrencyId: null,
        FromCurrencyId: null,
        ToCurrencyId: null,
        ToCurrencyRate: null,
        DrAmount: 0,
        CrAmount: 0,
        TrnType: null
    };

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
            cboService.getCboEntityByPlant(null, null, '', function (result) {
                $scope.entityList = result;
            });
        if (!baseService.isUndefinedOrNull($routeParams.advanceId)) {
            getByParams($routeParams.advanceId);
        }
    });

    $scope.searchBy = "ContractNo"; $scope.search = "";
    $scope.searchContractByList = [{ value: 'Id', name: "Id" }, { value: 'ContractNo', name: "ContractNo" }, { value: 'CustomerName', name: "Customer" }];

    $scope.contractList = [];
    $scope.GetPopUpContract = function () {
        $scope.contractList = [];
        $http({
            method: 'POST',
            url: "Commercial/contract/GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.contractList = response.data;
        });
        angular.element(document.querySelector('#ContractPopUp')).modal('show');
    };

    //$scope.contractList = [];
    //$scope.IsTradingPO = true;
    //$scope.GetPopUpContract = function () {
    //    $scope.contractList = [];
    //    $http.get("Products/PurchaseOrder/GetLCContractListByPartyId?isProcurementOnBom=" + $scope.IsTradingPO + "&partyId=" + $scope.advance.PartyId)
    //        .then(
    //            function successCallback(response) {
    //                if (baseService.arrayLength(response.data) > 0) {
    //                    $scope.contractList = response.data;
    //                }
    //            },
    //            function errorCallback(response) {
    //                ShowResult(response, 'failure');
    //            });
    //    angular.element(document.querySelector('#ContractPopUp')).modal('show');
    //};
    $scope.SelectedContract = function (obj) {
        $scope.advance.ContractId = obj.data.Id;
        $scope.advance.ContractNo = obj.data.ContractNo;
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    }

    $scope.CloseContractPopUp = function () {
        angular.element(document.querySelector('#ContractPopUp')).modal('hide');
    }
    $scope.ShowResultMasterOrderPopUp = function () {
        $scope.GetMasterOrderList();
        angular.element(document.querySelector('#masterOrderPopUp')).modal('show');
    }
    $scope.masterOrderList = [];
    $scope.GetMasterOrderList = function () {
        $scope.masterOrderList = [];
        $http({
            method: 'GET',
            url: "accounts/CustomerInvoice/GetMasterOrderListByPartyId?partyId=" + $scope.advance.PartyId
        }).then(function (response) {
            $scope.masterOrderList = response.data;
        });
    }
    $scope.AddOrder = function (obj) {
        $scope.advance.MasterOrderId = obj.data.MasterOrderId;
        angular.element(document.querySelector('#masterOrderPopUp')).modal('hide');
    }
    $scope.CloseMasterOrder = function () {
        angular.element(document.querySelector('#masterOrderPopUp')).modal('hide');
    }

    function getByParams(advanceId) {
        $http.get('Accounts/Advance/GetAdvanceForJournal?advanceId=' + advanceId)
            .then(function (response) {
                var advance = response.data;
                $scope.advance.Id = null;
                $scope.advance.PartyType = $scope.partyType;
                $scope.advance.PartyId = advance.PartyId;
                $scope.advance.PartyCode = advance.PartyCode;
                $scope.advance.PartyName = advance.PartyCode + " - " + advance.PartyName;
                $scope.advance.PartyPlantId = advance.PartyPlantId;
                //$scope.advance.IsInterTransaction = true;
                $scope.advance.CompanyId = advance.CompanyId;
                $scope.advance.PlantId = advance.CompanyId;
                $scope.advance.JournalId = advance.JournalId;

                $scope.advance.DocRefNo = advance.DocRefNo;
                $scope.advance.Narration = advance.Narration;
                $scope.advance.FinancingTypeId = advance.FinancingTypeId;
                $scope.advance.Amount = advance.Amount;
                $scope.advance.CurrencyId = advance.CurrencyId;
                $scope.advance.AdvanceNo = advance.AdvanceNo;
                $scope.advance.PaymentSource = 'Journal';
                $scope.GetCurrencyExchangeRateList();

                $http.get('Parties/Party/GetCompanyPartyDownPaymentGL?partyId=' + $scope.advance.PartyId + "&partyType=" + $scope.advance.PartyType)
                    .then(function (response) {
                        var data = response.data;

                        $scope.advanceDetail.GLGeneralInfoId = data.GLGeneralInfoId;
                        $scope.advanceDetail.GLGeneralInfoCode = data.GLGeneralInfoCode;
                        $scope.advanceDetail.GLGeneralInfoName = data.GLGeneralInfoName;
                        $scope.advanceDetail.BudgetMasterId = data.BudgetMasterId;
                        $scope.advanceDetail.BudgetCode = data.BudgetCode;
                        $scope.advanceDetail.BudgetName = data.BudgetName;
                        $scope.advanceDetail.ActivityId = data.ActivityId;
                        $scope.advanceDetail.ActivityCode = data.ActivityCode;
                        $scope.advanceDetail.ActivityName = data.ActivityName;

                        // Set to AdvanceDetail
                        $scope.advanceDetail.PartyType = $scope.advance.PartyType;
                        $scope.advanceDetail.PartyId = $scope.advance.PartyId;
                        $scope.advanceDetail.PartyCode = $scope.advance.PartyCode;
                        $scope.advanceDetail.PartyName = $scope.advance.PartyName;
                        $scope.advanceDetail.Narration = $scope.advance.Narration;
                        $scope.advanceDetail.Amount = $scope.advance.Amount;

                        $scope.getPartyPlantList($scope.advance.PartyId);
                        $scope.closeAdvanceInterTransactionPopUp(advance);
                    });
            });

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    $scope.getById = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/Advance/GetAdvance/' + id
        }).then(function successCallback(response) {
            $scope.advance = response.data;
            $scope.advance.DocDate = $filter('dateFiltering')($scope.advance.DocDate);
            $scope.advance.VoucherDate = $filter('dateFiltering')($scope.advance.VoucherDate);
            $scope.advance.PostingDate = $filter('dateFiltering')($scope.advance.PostingDate);
            $scope.advance.ReviewDate = $filter('dateFiltering')($scope.advance.ReviewDate);
            $scope.getPartyPlantList($scope.advance.PartyId, true);

            $http({
                method: 'GET',
                url: 'accounts/Advance/GetAdvanceDetail?advanceId=' + id
            }).then(function successCallback(response) {
                $scope.advanceDetailList = response.data;
                $scope.GetCurrencyExchangeRateList();

                if (!baseService.isUndefinedOrNull($scope.advance.BankMasterId)) {
                    factoryService.getBankMasterGL($scope.advance.BankMasterId, function (result) {
                        setBankGL(result);
                    });
                }

                if (!baseService.isUndefinedOrNull($scope.advance.CashMasterId)) {
                    factoryService.getCashMasterGL($scope.advance.CashMasterId, function (result) {
                        setCashGL(result);
                    });
                }
            });

            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        });
    };

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.advance.PostingDate) && !baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $scope.advance.PostingDate + "&currencyId=" + $scope.advance.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.advance.CompanyCurrencyRate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };

    $scope.copyAmount = function () {
        var getRow = $filter("filter")($scope.voucherDetailCurrencyList, { "TrnType": "Dr" });
        if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0) {
            if ($scope.advance.BankCurrencyId === $scope.companyCurrencyId) {
                $scope.advance.BankAmount = getRow[0].CompanyCurrencyDr;
            }
            if ($scope.advance.BankCurrencyId === $scope.companyGroupCurrencyId) {
                $scope.advance.BankAmount = getRow[0].CompanyGroupCurrencyDr;
            }
            if ($scope.advance.BankCurrencyId === $scope.hardCurrencyId) {
                $scope.advance.BankAmount = getRow[0].HardCurrencyDr;
            }
        }
    };

    $scope.checkBankAmount = function () {
        if (!baseService.isUndefinedOrNull($scope.advance.BankCurrencyId)) {
            if ($scope.advance.BankCurrencyId !== $scope.advance.CurrencyId) {
                if ($scope.advance.BankCurrencyId !== $scope.companyCurrencyId) {
                    if ($scope.advance.BankCurrencyId !== $scope.companyGroupCurrencyId) {
                        if ($scope.advance.BankCurrencyId !== $scope.hardCurrencyId) {
                            $scope.isBankAmount = true;
                            $scope.advance.BankAmount = 0;
                        }
                    }
                    else {
                        $scope.isBankAmount = false;
                        $scope.advance.BankAmount = 0;
                    }
                }
                else {
                    $scope.isBankAmount = false;
                    $scope.advance.BankAmount = 0;
                }
            }
            else {
                $scope.isBankAmount = false;
                $scope.advance.BankAmount = 0;
            }
        }
        else {
            $scope.isBankAmount = false;
            $scope.advance.BankAmount = 0;
        }
    };

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.advance.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
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
        if (new Date($scope.advance.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.advance.FiscalYearId = null;
            $scope.advance.FiscalYearName = null;
            $scope.advance.FiscalYearPeriodId = null;
            $scope.advance.FiscalYearPeriodName = null;
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else if (new Date($scope.advance.PostingDate) < new Date($scope.advance.DocDate)) {
            msg = "Posting date must be below or equal to Doc Date!";
            $scope.advance.FiscalYearId = null;
            $scope.advance.FiscalYearName = null;
            $scope.advance.FiscalYearPeriodId = null;
            $scope.advance.FiscalYearPeriodName = null;
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        } else {
            $scope.invalidPostingDate = false;
            $scope.GetCurrencyExchangeRateList();
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    $scope.invalidEntity = false;
    $scope.entityValidation = function () {
        $scope.invalidEntity = baseService.isUndefinedOrNull($scope.advance.EntityId);
        return manualValidation("div_entity", $scope.invalidEntity, "Entity is required.");
    };

    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            ShowResult("Please select Currency!", "failure");
            return true;
        }
        if ($scope.partyType === "Customer") {
            if ($scope.advance.PartyId === null) {
                ShowResult("Please select Customer!", "failure");
                return true;
            }
            if ($scope.advance.PaymentSource != 'MultiBank' && parseFloat($scope.advance.Amount) === 0) {
                ShowResult("Advance Amount must greater than 0!", "failure");
                return true;
            }
            if ($scope.advance.PaymentSource !='MultiBank' && baseService.isUndefinedOrNull($scope.advance.GLGeneralInfoId)) {
                ShowResult("Please select Cash or Bank!", "failure");
                return true;
            }
            if (baseService.isUndefinedOrNull($scope.advance.ResponsiblePerson)) {
                ShowResult("Please select Responsible Person!", "failure");
                return true;
            }
        }
        else if ($scope.partyType === "Vendor") {
            if ($scope.advance.PartyId === null) {
                ShowResult("Please select Vendor!", "failure");
                return true;
            }
            if ($scope.advance.GLGeneralInfoId === null) {
                ShowResult("Please select Cash or Bank!", "failure");
                return true;
            }
        }
        return false;
    };

    $scope.checkDrCrBalancing = function () {
        var companyCurrencyAmountDr = $filter("sumByKey")($filter("filter")($scope.voucherDetailCurrencyList, { TrnType: "Dr" }), "CompanyCurrencyDr");
        var companyCurrencyAmountCr = $filter("sumByKey")($filter("filter")($scope.voucherDetailCurrencyList, { TrnType: "Cr" }), "CompanyCurrencyCr");
        if (companyCurrencyAmountDr === 0) {
            ShowResult($scope.companyCurrencyCode + " Dr amount can not zero!", "failure");
            $scope.setTab(2);
            return false;
        }
        if (companyCurrencyAmountCr === 0) {
            ShowResult($scope.companyCurrencyCode + " Cr amount can not zero!", "failure");
            $scope.setTab(2);
            return false;
        }
        if (companyCurrencyAmountDr !== companyCurrencyAmountCr) {
            ShowResult($scope.companyCurrencyCode + " Dr amount and Cr amount is not equal!", "failure");
            $scope.setTab(2);
            return false;
        }
        if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId)) {
            var companyGroupCurrencyAmountDr = $filter("sumByKey")($filter("filter")($scope.voucherDetailCurrencyList, { TrnType: "Dr" }), "CompanyGroupCurrencyDr");
            var companyGroupCurrencyAmountCr = $filter("sumByKey")($filter("filter")($scope.voucherDetailCurrencyList, { TrnType: "Cr" }), "CompanyGroupCurrencyCr");
            if (companyGroupCurrencyAmountDr === 0) {
                ShowResult($scope.companyGroupCurrencyCode + " Dr amount can not zero!", "failure");
                $scope.setTab(2);
                return false;
            }
            if (companyGroupCurrencyAmountCr === 0) {
                ShowResult($scope.companyGroupCurrencyCode + " Cr amount can not zero!", "failure");
                $scope.setTab(2);
                return false;
            }
            if (companyGroupCurrencyAmountDr !== companyGroupCurrencyAmountCr) {
                ShowResult($scope.companyGroupCurrencyCode + " Dr amount and Cr amount is not equal!", "failure");
                $scope.setTab(2);
                return false;
            }
        }
        if (!baseService.isUndefinedOrNull($scope.hardCurrencyId)) {
            var hardCurrencyAmountDr = $filter("sumByKey")($filter("filter")($scope.voucherDetailCurrencyList, { TrnType: "Dr" }), "HardCurrencyDr");
            var hardCurrencyAmountCr = $filter("sumByKey")($filter("filter")($scope.voucherDetailCurrencyList, { TrnType: "Cr" }), "HardCurrencyCr");
            if (hardCurrencyAmountDr === 0) {
                ShowResult($scope.hardCurrencyCode + " Dr amount can not zero!", "failure");
                $scope.setTab(2);
                return false;
            }
            if (hardCurrencyAmountCr === 0) {
                ShowResult($scope.hardCurrencyCode + " Cr amount can not zero!", "failure");
                $scope.setTab(2);
                return false;
            }
            if (hardCurrencyAmountDr !== hardCurrencyAmountCr) {
                ShowResult($scope.hardCurrencyCode + " Dr amount and Cr amount is not equal!", "failure");
                $scope.setTab(2);
                return false;
            }
        }
        return true;
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.advance.ResponsiblePersonId = employee.SystemId;
            $scope.advance.ResponsiblePerson = employee.EmployeeName;
        }
        $scope.hideEmployeePopUp();
    };

    $scope.clearEmployeePopUp = function () {
        $scope.advance.ResponsiblePersonId = null;
        $scope.advance.ResponsiblePerson = null;
    };

    $scope.closePartyPopUp = function (x) {
            var party = x.data;
            $scope.clearDrData();
            if (baseService.isUndefinedOrNull(party.DownPaymentGLId)) {
                ShowResult("Customer DownPaymentGL not found!", "failure", "partyPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.DownPaymentBudgetId)) {
                ShowResult("Customer budget not found!", "failure", "partyPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(party.CurrencyId)) {
                ShowResult('Customer transaction currency not found!', 'failure', 'partyPopUp');
                return;
            }
            else {
                $scope.advanceDetail.GLGeneralInfoId = party.DownPaymentGLId;
                $scope.advanceDetail.GLGeneralInfoCode = party.DownPaymentGLCode;
                $scope.advanceDetail.GLGeneralInfoName = party.DownPaymentGLName;
                $scope.advanceDetail.BudgetMasterId = party.DownPaymentBudgetId;
                $scope.advanceDetail.BudgetCode = party.DownPaymentBudgetCode;
                $scope.advanceDetail.BudgetName = party.DownPaymentBudgetName;
                $scope.advanceDetail.ActivityId = party.DownPaymentActivityId;
                $scope.advanceDetail.ActivityCode = party.DownPaymentActivityCode;
                $scope.advanceDetail.ActivityName = party.DownPaymentActivityName;
            }

            // Set to Advance
            $scope.advance.PartyId = party.Id;
            $scope.advance.PartyCode = party.Code;
            $scope.advance.PartyName = party.Code + " - " + party.UserName;
            $scope.advance.PartyType = party.PartyType;
            $scope.advance.CurrencyId = party.CurrencyId;
            $scope.advance.TotalPartyPlant = party.TotalPartyPlant;

            // Set to AdvanceDetail
            $scope.advanceDetail.PartyId = party.Id;
            $scope.advanceDetail.PartyCode = party.Code;
            $scope.advanceDetail.PartyName = party.Code + " - " + party.UserName;
            $scope.advanceDetail.PartyType = party.PartyType;

            $scope.GetCurrencyExchangeRateList();
            $scope.checkBankAmount();
            $scope.getPartyPlantList(party.Id);
            $scope.copyAmount();
        $scope.hidePartyPopUp();
    };

    // Clear Dr. data if pary selectionn change
    $scope.clearDrData = function () {
        $scope.advanceDetailList = [];
    };

    $scope.updateCrAmount = function (data) {
        angular.forEach($scope.advanceDetailList, function (item, i) {
            if (item.PartyType === $scope.partyType) {
                item.Narration = $scope.advance.Narration;
                item.PartyPlantId = $scope.advance.PartyPlantId;
                item.PartyPlantName = item.PartyPlantName === null ? $scope.PartyPlantName : item.PartyPlantName;
            }
            if (!$scope.advance.IsInterTransaction) {
                item.Amount = $scope.advance.Amount;
            }
            if (data !== undefined && data !== null && item.PartyType === $scope.partyType) {
                item.Amount = $scope.advance.Amount - data.Amount;
            }
            $scope.setCrExchangeRate(item);
        });
    };

    $scope.removeRow = function (index) {
        $scope.advanceDetailList.splice(index, 1);
        $scope.updateCrAmount(null);
    };

    $scope.clearPartyPopUp = function () {
        $scope.advance.PartyId = null;
        $scope.advance.PartyCode = null;
        $scope.advance.PartyName = null;
        $scope.advance.PartyType = null;
        $scope.advance.CurrencyId = null;
        $scope.advance.TotalPartyPlant = null;
        $scope.partyPlantList = [];
    };

    $scope.getPartyPlantList = function (partyId, isUpdateMode) {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.partyPlantList.push(item);
                    if (item.IsDefault && !isUpdateMode) {
                        $scope.advance.PartyPlantId = item.Value;
                        $scope.advanceDetail.PartyPlantId = item.Value;
                        $scope.advanceDetail.PartyPlantName = item.Text;
                        $scope.advanceDetailList.push($scope.advanceDetail);
                        $scope.setCrExchangeRate($scope.advanceDetail);
                    }
                });
                $scope.advanceDetail = {};
            });
    };

    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];
            if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
                ShowResult("Please select currency!", "failure", "bankPopUp");
                return;
            }
            if (baseService.isUndefinedOrNull(bank.GLGeneralInfoId)) {
                ShowResult("Bank GL not found!", "failure", "bankPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(bank.BudgetMasterId)) {
                ShowResult("Bank budget not found!", "failure", "bankPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(bank.CurrencyId)) {
                ShowResult("Bank transaction currency not found!", "failure", "bankPopUp");
                return;
            }
            else {
                $scope.advance.AccountTitle = bank.AccountTitle;
                $scope.advance.BankName = bank.AccountTitle;
                $scope.advance.BankMasterId = bank.BankMasterId;
                setBankGL(bank);
            }
        }
        $scope.hideBankPopUp();
    };

    function setBankGL(bank) {
        $scope.advance.BankCurrencyId = bank.CurrencyId;
        $scope.advance.GLGeneralInfoId = bank.GLGeneralInfoId;
        $scope.advance.GLGeneralInfoCode = bank.GLGeneralInfoCode;
        $scope.advance.GLGeneralInfoName = bank.GLGeneralInfoName;
        $scope.advance.BudgetMasterId = bank.BudgetMasterId;
        $scope.advance.BudgetCode = bank.BudgetCode;
        $scope.advance.BudgetName = bank.BudgetName;
        $scope.advance.ActivityId = bank.ActivityId;
        $scope.advance.ActivityCode = bank.ActivityCode;
        $scope.advance.ActivityName = bank.ActivityName;
        $scope.checkBankAmount();
        $scope.copyAmount();
    }

    $scope.clearBankPopUp = function () {
        $scope.isBankAmount = false;
        $scope.advance.AccountTitle = null;
        $scope.advance.BankName = null;
        $scope.advance.BankMasterId = null;
        $scope.advance.BankCurrencyId = null;
        $scope.advance.CashMasterId = null;
        $scope.advance.CashName = null;
        $scope.advance.CashCurrencyId = null;
        $scope.advance.GLGeneralInfoId = null;
        $scope.advance.GLGeneralInfoCode = null;
        $scope.advance.GLGeneralInfoName = null;
        $scope.advance.BudgetMasterId = null;
        $scope.advance.BudgetCode = null;
        $scope.advance.BudgetName = null;
        $scope.advance.ActivityId = null;
        $scope.advance.ActivityCode = null;
        $scope.advance.ActivityName = null;
        $scope.advance.Amount = 0;
        for (var i = 0; i < $scope.voucherDetailCurrencyList.length; i++) {
            if ($scope.voucherDetailCurrencyList[i].TrnType === "Dr") {
                $scope.voucherDetailCurrencyList.splice(i, 1);
            }
        }
    };

    $scope.closeCashPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            ShowResult('Please select currency!', 'failure', 'cashPopUp');
            return;
        }
        if ($scope.cashIndex !== -1) {
            var cash = $scope.cashList[$scope.cashIndex];
            if (baseService.isUndefinedOrNull(cash.GLGeneralInfoId)) {
                ShowResult('Cash GL not found!', 'failure', 'cashPopUp');
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(cash.BudgetMasterId)) {
                ShowResult('Cash budget not found!', 'failure', 'cashPopUp');
                return;
            }
            else if (baseService.isUndefinedOrNull(cash.CurrencyId)) {
                ShowResult('Cash transaction currency not found!', 'failure', 'cashPopUp');
                return;
            }
            else {
                $scope.advance.CashMasterId = cash.Id;
                $scope.advance.CashName = cash.CashName;
                setCashGL(cash);
            }
        }
        $scope.hideCashPopUp();
    };

    function setCashGL(cash) {
        $scope.advance.CashCurrencyId = cash.CurrencyId;
        $scope.advance.GLGeneralInfoId = cash.GLGeneralInfoId;
        $scope.advance.GLGeneralInfoCode = cash.GLGeneralInfoCode;
        $scope.advance.GLGeneralInfoName = cash.GLGeneralInfoName;
        $scope.advance.BudgetMasterId = cash.BudgetMasterId;
        $scope.advance.BudgetCode = cash.BudgetCode;
        $scope.advance.BudgetName = cash.BudgetName;
        $scope.advance.ActivityId = cash.ActivityId;
        $scope.advance.ActivityCode = cash.ActivityCode;
        $scope.advance.ActivityName = cash.ActivityName;
        $scope.checkBankAmount();
    }

    $scope.clearCashPopUp = function () {
        $scope.clearBankPopUp();
    };

    $scope.clear = function () {
        $scope.Action = "Save";
        $scope.advance.Active = true;
        $scope.advance.DocRefNo = null;
        $scope.advance.PaymentSource = 'Bank';
        $scope.advance.ReviewDate = null;
        $scope.advance.Amount = 0;
        $scope.advance.Narration = null;
        $scope.advance.IsInterTransaction = false;
        $scope.advance.ContractId = null;
        $scope.advance.ContractNo = null;
        $scope.advance.MasterOrderId = null;
        $scope.advance.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.getCboVoucherTypeAdvanceTakenList();
        $scope.currencyExchangeRate = [];
        $scope.advanceDetailList = [];
        $scope.voucherDetailCurrencyList = [];
        $scope.bankDetailList = [];
        $scope.clearPartyPopUp();
        $scope.clearBankPopUp();
        $scope.clearCashPopUp();
        $scope.clearEmployeePopUp();
        $scope.setTab(1);
    };

    $scope.report = function (voucherId) {
        location.href = $scope.reportUrl + voucherId;
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.getCboVoucherTypeAdvanceTakenList = function () {
        cboService.getCboVoucherTypeAdvanceTakenList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.advance.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.advance.PostingDate = $filter('dateFiltering')($scope.voucherTypeList[0].LastPostingDate);
                $scope.advance.BankTransactionDate = $filter('dateFiltering')($scope.voucherTypeList[0].LastPostingDate);
                $scope.advance.DocDate = $scope.advance.PostingDate;
            }
        });
    }
    $scope.getCboVoucherTypeAdvanceTakenList();

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.advance.VoucherTypeId = data.Value;
        $scope.advance.PostingDate = $filter('dateFiltering')(data.LastPostingDate);
        $scope.advance.DocDate = $scope.advance.PostingDate;
    };
    cboService.getCboInterCompanyFinancingType("InterTransaction", function (result) {
        $scope.financingTypeList = result;
    });

    $scope.transactionTypeGL = null;
    $scope.getTransactionTypeGL = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            $scope.transactionTypeGL = $.grep($scope.financingTypeList, function (item) {
                return item.FinancingTypeId === id;
            })[0];
            if (manualValidation('div_TransactionType', baseService.isUndefinedOrNull($scope.transactionTypeGL.LiabilityGLId), 'Transaction Type GL not found!')) {
                $scope.transactionTypeGL = null;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget
                && manualValidation('div_TransactionType', baseService.isUndefinedOrNull($scope.transactionTypeGL.LiabilityBudgetMasterId), 'Transaction Type Budget not found!')) {
                $scope.transactionTypeGL = null;
            }
        }
        else {
            manualValidation('div_TransactionType', true, 'Transaction Type is required.');
            $scope.transactionTypeGL = null;
        }
    };

    cboService.getCboInterCompany(null, function (result) {
        $scope.companyList = result;
    });

    $scope.companyChange = function (companyId) {
        cboService.getCboInterPlant('', companyId, '', function (result) {
            $scope.interplantList = result;
        });
    };

    $scope.save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate();
        $scope.checkPostingDate();
            $scope.entityValidation();
        if ($scope.form0.$valid && !$scope.validation() && !$scope.invalidDocDate && !$scope.invalidEntity && !$scope.invalidPostingDate ) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.parkUrl,
                    data: {
                        "advanceVM": $scope.advance,
                        "advanceDetailVMList": $scope.advanceDetailList,
                        "banksDetailVMList": $scope.bankDetailList,
                        "bankChargeDetailVMList": $scope.bankChargesList
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
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrl,
                    data: {
                        "advanceVM": $scope.advance,
                        "advanceDetailVMList": $scope.advanceDetailList,
                        "currencyList": $scope.advanceDetailList
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
            }
            return true;
        }
        return true;
    };

    $scope.advanceId = null;
    $scope.confirmPost = function (advanceId,advanceGroupNo) {
        $scope.advanceId = advanceId;
        $scope.advanceGroupNo = advanceGroupNo;
        $scope.message_confirmation = 'Are you sure to Post?';
        angular.element(document.querySelector('#confirmPostPopUp')).modal('show');
    };

    $scope.post = function (advanceId, advanceGroupNo) {
        $http({
            method: "POST",
            url: $scope.postUrl,
            data: {
                "advanceId": advanceId,
                "advanceGroupNo": advanceGroupNo
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

    $scope.confirmUnPost = function (advanceId) {
        $scope.advanceId = advanceId;
        $scope.message_confirmation = 'Are you sure to UnPost?';
        angular.element(document.querySelector('#confirmUnPostPopUp')).modal('show');
    };

    $scope.unPost = function (advanceId) {
        $http({
            method: "POST",
            url: $scope.unPostUrl,
            data: {
                "advanceId": advanceId
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

    $scope.company = null;
    $scope.getCompanyInfo = function (companyId) {
        if (!baseService.isUndefinedOrNull(companyId)) {
            $scope.company = $.grep($scope.companyList, function (item) {
                return item.CompanyId === companyId;
            })[0];
            if (manualValidation('div_Company', baseService.isUndefinedOrNull($scope.company.PartyId), 'This Company is not created as InterCompany Party.')) {
                $scope.company = null;
            }
        }
        else {
            manualValidation('div_Company', true, 'Company is required.');
            $scope.company = null;
        }
    };

    $scope.plant = null;
    $scope.getPlantInfo = function (plantId) {
        if (!baseService.isUndefinedOrNull(plantId)) {
            $scope.plant = $.grep($scope.interplantList, function (item) {
                return item.PlantId === plantId;
            })[0];
            if (manualValidation('div_Plant', baseService.isUndefinedOrNull($scope.plant.PartyPlantId), 'This Company is not created as InterCompany Party Plant.')) {
                $scope.plant = null;
            }
        }
        else {
            manualValidation('div_Plant', true, 'Plant is required.');
            $scope.plant = null;
        }
    };

    $scope.addRow = function () {
        $scope.advanceDetail.GLGeneralInfoId = $scope.transactionTypeGL.LiabilityGLId;
        $scope.advanceDetail.GLGeneralInfoCode = $scope.transactionTypeGL.LiabilityGLCode;
        $scope.advanceDetail.GLGeneralInfoName = $scope.transactionTypeGL.LiabilityGLName;
        $scope.advanceDetail.BudgetMasterId = $scope.transactionTypeGL.LiabilityBudgetMasterId;
        $scope.advanceDetail.BudgetCode = $scope.transactionTypeGL.LiabilityBudgetCode;
        $scope.advanceDetail.BudgetName = $scope.transactionTypeGL.LiabilityBudgetName;
        $scope.advanceDetail.ActivityId = $scope.transactionTypeGL.LiabilityActivityId;
        $scope.advanceDetail.ActivityCode = $scope.transactionTypeGL.LiabilityActivityCode;
        $scope.advanceDetail.ActivityName = $scope.transactionTypeGL.LiabilityActivityName;

        $scope.advanceDetail.PartyType = $scope.company.PartyType;
        $scope.advanceDetail.CompanyId = $scope.company.CompanyId;
        $scope.advanceDetail.PartyId = $scope.company.PartyId;
        $scope.advanceDetail.PartyCode = $scope.company.PartyCode;
        $scope.advanceDetail.PartyName = $scope.company.PartyCode + " - " + $scope.company.PartyName;
        $scope.advanceDetail.PlantId = $scope.plant.PlantId;
        $scope.advanceDetail.PartyPlantId = $scope.plant.PartyPlantId;
        $scope.advanceDetail.PartyPlantName = $scope.plant.PartyPlantName;
        $scope.advanceDetail.Amount = 0;

        $scope.advanceDetailList.push($scope.advanceDetail);
        $scope.advanceDetail = {};
    };

    $scope.advanceInterTransactionSearchByList = [
        {
            'name': '#No',
            'value': 'AdvanceNo'
        },
        {
            'name': 'Company',
            'value': 'CompanyName'
        },
        {
            'name': 'Plant',
            'value': 'PlantName'
        },
        {
            'name': 'Party Code',
            'value': 'PartyCode'
        },
        {
            'name': 'Party Name',
            'value': 'PartyName'
        },
        {
            'name': 'Party Plant',
            'value': 'PartyPlantName'
        }
    ];

    $scope.advanceInterTransactionParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'CompanyName, PlantName',
        searchBy: 'AdvanceNo',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showAdvanceInterTransactionPopUp = function () {
        $scope.advanceInterTransactionParameters.partyId = $scope.advance.PartyId;
        baseService.setCurrentPage('advanceInterTransactionList');
        $scope.getAdvanceInterTransactionList = function (pageno) {
            baseService.paginationBase($scope.jouranlUrl, pageno, $scope.advanceInterTransactionParameters)
                .then(function (result) {
                    $scope.advanceInterTransactionList = result.Rows;
                    $scope.advanceInterTransactionParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#advanceJournalPopUp')).modal('show');
        $scope.getAdvanceInterTransactionList();
    };

    $scope.closeAdvanceInterTransactionPopUp = function (data) {
        $scope.financingTypeGL = $.grep($scope.financingTypeList, function (item) {
            return item.FinancingTypeId === data.FinancingTypeId;
        })[0];
        if (baseService.isUndefinedOrNull($scope.financingTypeGL.AssetGLId)) {
            ShowResult('Transaction Type GL not found!', 'failure', 'advanceJournalPopUp');
        }
        else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull($scope.financingTypeGL.AssetBudgetMasterId)) {
            ShowResult('Transaction Type Budget not found!', 'failure', 'advanceJournalPopUp');
        }

        $scope.advance.AdvanceId = data.AdvanceId;
        $scope.advance.AdvanceNo = data.AdvanceNo;
        $scope.advance.Amount = data.NetAmount;
        $scope.advance.Narration = data.Narration;
        $scope.advance.JournalId = data.AdvanceId;
        $scope.advance.AdvanceDetailId = data.AdvanceDetailId;

        $scope.advance.GLGeneralInfoId = $scope.financingTypeGL.AssetGLId;
        $scope.advance.GLGeneralInfoCode = $scope.financingTypeGL.AssetGLCode;
        $scope.advance.GLGeneralInfoName = $scope.financingTypeGL.AssetGLName;
        $scope.advance.BudgetMasterId = $scope.financingTypeGL.AssetBudgetMasterId;
        $scope.advance.BudgetCode = $scope.financingTypeGL.AssetBudgetCode;
        $scope.advance.BudgetName = $scope.financingTypeGL.AssetBudgetName;
        $scope.advance.ActivityId = $scope.financingTypeGL.AssetActivityId;
        $scope.advance.ActivityCode = $scope.financingTypeGL.AssetActivityCode;
        $scope.advance.ActivityName = $scope.financingTypeGL.AssetActivityName;

        $scope.updateCrAmount(null);
        $scope.checkBankAmount();
        $scope.hideAdvanceInterTransactionPopUp();
    };

    $scope.hideAdvanceInterTransactionPopUp = function () {
        angular.element(document.querySelector('#advanceJournalPopUp')).modal('hide');
    };

    $scope.invalidRow = false;
    $scope.checkRowValidation = function (data, index) {
        if (manualValidation('td_Narration_' + index, baseService.isUndefinedOrNull(data.Narration), 'Narration is required.')) {
            $scope.invalidRow = true;
        }
        else if (manualValidation('td_Amount_' + index, baseService.isUndefinedOrNaNOrZero(data.Amount), 'Amount is required and must greater than 0.')) {
            $scope.invalidRow = true;
        }
        else
            $scope.invalidRow = false;
    };

    $scope.delete = function (advanceId, voucherId, advanceGroupNo) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "advanceId": advanceId, "voucherId": voucherId, "advanceGroupNo": advanceGroupNo
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
                $scope.advanceId = null;
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.advanceId = null;
    $scope.confirmDelete = function (advanceId, voucherId, advanceGroupNo) {
        $scope.advanceId = advanceId;
        $scope.voucherId = voucherId;
        $scope.advanceGroupNo = advanceGroupNo;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };


    $scope.showMultiBankPopUp = function (entityId) {
        if (entityId === undefined || entityId === "undefined") {
            entityId = null;
        }
        $scope.getBankList = function (pageno) {
            if ($scope.bankACType === "HouseBank") {
                $scope.url = "Banks/BankMaster/GetBankMasterList?bankACType=HouseBank&&entityId=" + entityId;
            }
            else if ($scope.bankACType === "Loan") {
                $scope.url = "Banks/BankMaster/GetBankMasterList?bankACType=Loan&&entityId=" + entityId;
            }
            else if ($scope.bankACType === "Investment") {
                $scope.url = "Banks/BankMaster/GetBankMasterList?bankACType=Investment&&entityId=" + entityId;
            }
            else if ($scope.bankACType === "Security") {
                $scope.url = "Banks/BankMaster/GetBankMasterList?bankACType=Security&&entityId=" + entityId;
            }
            else {
                $scope.url = "Banks/BankMaster/GetBankMasterList?bankACType=HouseBank&&entityId=" + entityId;
            }
            baseService.paginationBase($scope.url, pageno, $scope.bankParameters)
                .then(function (result) {
                    $scope.bankList = result.Rows;
                    $scope.bankParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        $scope.getBankList();
        angular.element(document.querySelector("#multibankPopUp")).modal("show");
    };
    $scope.GetCurrencyParallel = function () {
        $http({
            method: "GET",
            url: "currencies/CompanyParallelCurrency/CurrencyParallel"
        }).then(function successCallback(response) {
            $scope.CurrencyParallel = response.data;
            console.log($scope.CurrencyParallel);
            if ($scope.CurrencyParallel.length === 0) {
                $scope.pop("error", "Company Parallel Currency is not set!");
                $scope.showform = false;
            }
            else {
                $scope.showform = true;
            }
            $scope.BaseCurrencyCode = $scope.CurrencyParallel[0].Code;
            $scope.BaseCurrencyId = $scope.CurrencyParallel[0].CurrencyId;
        });
    };
    $scope.GetCurrencyParallel();
    $scope.bankDetailList = [];
    $scope.closeMultiBankPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            ShowResult("Please Select Currency !", "failure", "multibankPopUp");
            return;
        }
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];
            if (baseService.isUndefinedOrNull(bank.GLGeneralInfoId)) {
                ShowResult("Bank GL not found!", "failure", "multibankPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(bank.BudgetMasterId)) {
                ShowResult("Bank Budget not found!", "failure", "multibankPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(bank.CurrencyId)) {
                ShowResult("Bank Transaction Currency not found!", "failure", "multibankPopUp");
                return;
            }
            else {
                var getRow = null;
                getRow = $filter("filter")($scope.bankDetailList, { "BankMasterId": bank.BankMasterId });
                if (getRow.length === 0) {
                    $scope.bankDetail = {};
                    $scope.bankDetail.SourceType = "Bank";
                    $scope.bankDetail.AccountTitle = bank.AccountTitle;
                    $scope.bankDetail.BankName = bank.BankCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
                    $scope.bankDetail.BankMasterId = bank.BankMasterId;
                    $scope.bankDetail.BankCurrencyId = bank.CurrencyId;

                    $scope.bankDetail.GLGeneralInfoId = bank.GLGeneralInfoId;
                    $scope.bankDetail.GLGeneralInfoName = bank.GLGeneralInfoName;
                    $scope.bankDetail.BudgetMasterId = bank.BudgetMasterId;
                    $scope.bankDetail.BudgetName = bank.BudgetName;
                    $scope.bankDetail.ActivityId = bank.ActivityId;
                    $scope.bankDetail.ActivityName = bank.ActivityName;
                    $scope.bankDetail.BankCurrencyId = bank.CurrencyId;
                    $scope.bankDetail.CurrencyCode = $scope.advance.CurrencyCode;
                    $scope.bankDetail.BankCurrencyCode = bank.CurrencyCode;
                    if (bank.CurrencyId == $scope.BaseCurrencyId) {
                        $scope.bankDetail.CompanyCurrencyRate = 1;
                    }
                    else if (bank.CurrencyId == $scope.advance.CurrencyId) {
                        $scope.bankDetail.CompanyCurrencyRate = $scope.advance.CompanyCurrencyRate;
                    }
                    $scope.bankDetail.BankCurrencyCode = bank.CurrencyCode;
                    $scope.bankDetail.FinancingId = "";
                    $scope.bankDetail.FinancingDetailId = "";
                    $scope.bankDetail.FinancingTypeId = "";
                    $scope.bankDetail.Balance = 0;
                    $scope.bankDetail.Amount = null;
                    $scope.bankDetail.BaseDrAmount = null;
                    $scope.bankDetailList.push($scope.bankDetail);
                    $scope.checkBankAmount();
                    $scope.hideMultiBankPopUp();
                }
                else {
                    ShowResult(bank.AccountTitle + " already  Exist", "failure", "multibankPopUp");
                }
            }
        }
    };

    $scope.hideMultiBankPopUp = function () {
        angular.element(document.querySelector("#multibankPopUp")).modal("hide");
        $scope.bankIndex = -1;
        $scope.bankSelected = null;
    };

    $scope.loanDataList = [];
    $scope.getPopUpData = function () {
        $http({
            method: 'GET',
            url: 'Accounts/Loan/GetLoanPopUpListForSalesRealization?transactionType=' + "LoanTaken"
        }).then(function successCallback(response) {
            $scope.loanDataList = response.data;
            for (var i = 0; i < $scope.loanDataList.length; i++) {
                response.data[i].PostingDateNew = new Date($scope.loanDataList[i].PostingDateNew);
                response.data[i].DocDate = new Date($scope.loanDataList[i].DocDate);
            }
        });
    };
    $scope.showloanPopUp = function () {
        $scope.getPopUpData();
        angular.element(document.querySelector('#loanPopUp')).modal('show');
    };
    $scope.closeloanPopUp = function () {
        angular.element(document.querySelector("#loanPopUp")).modal("hide");
    };
    
    $scope.closeloanPopUpSelected = function (x) {
        var bank = x.data;
        if (baseService.isUndefinedOrNull($scope.advance.CurrencyId)) {
            ShowResult("Please Select Currency !", "failure", "loanPopUp");
            return;
        }
        if (baseService.isUndefinedOrNull(bank.GLGeneralInfoId)) {
            ShowResult("Bank GL not found!", "failure", "loanPopUp");
            return;
        }
        else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(bank.BudgetMasterId)) {
            ShowResult("Bank Budget not found!", "failure", "loanPopUp");
            return;
        }
        else if (baseService.isUndefinedOrNull(bank.CurrencyId)) {
            ShowResult("Bank Transaction Currency not found!", "failure", "loanPopUp");
            return;
        }

        else {
            var getRow = null;
            getRow = $filter("filter")($scope.bankDetailList, { "BankMasterId": bank.BankMasterId });
            if (getRow.length === 0) {
                $scope.bankDetail = {};
                $scope.bankDetail.SourceType = "Loan";
                $scope.bankDetail.AccountTitle = bank.AccountTitle;
                $scope.bankDetail.BankName = bank.BankCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
                $scope.bankDetail.BankMasterId = bank.BankMasterId;
                $scope.bankDetail.BankCurrencyId = bank.CurrencyId;

                $scope.bankDetail.GLGeneralInfoId = bank.GLGeneralInfoId;
                $scope.bankDetail.GLGeneralInfoName = "";
                $scope.bankDetail.BudgetMasterId = bank.BudgetMasterId;
                $scope.bankDetail.BudgetName = "";
                $scope.bankDetail.ActivityId = bank.ActivityId;
                $scope.bankDetail.ActivityName = "";
                $scope.bankDetail.BankCurrencyId = bank.CurrencyId;
                $scope.bankDetail.CurrencyCode = $scope.advance.CurrencyCode;
                $scope.bankDetail.BankCurrencyCode = bank.CurrencyCode;
                $scope.bankDetail.FinancingId = bank.FinancingId;
                $scope.bankDetail.FinancingDetailId = bank.FinancingDetailId;
                $scope.bankDetail.FinancingTypeId = bank.FinancingTypeId;
                $scope.bankDetail.CompanyCurrencyRate = bank.CompanyCurrencyRate;
                $scope.bankDetail.Balance = bank.Balance;
                $scope.bankDetail.Amount = null;
                $scope.bankDetail.BaseDrAmount = null;
                $scope.bankDetailList.push($scope.bankDetail);
               

            }
            else {
                ShowResult(bank.AccountTitle + " already  Exist", "failure", "loanPopUp");
            }
        }
    }

    $scope.calbankAmount = function (data) {
        if ($scope.advance.CurrencyId == data.BankCurrencyId == $scope.BaseCurrencyId) {
            data.BankAmount = data.Amount;
            data.BaseDrAmount = data.Amount;
        }
        else if ($scope.advance.CurrencyId == data.BankCurrencyId) {
            data.BankAmount = data.Amount;
            data.BaseDrAmount = Math.abs(data.Amount * $scope.advance.CompanyCurrencyRate).toFixed(2);
        }
        else if ($scope.advance.CurrencyId != data.BankCurrencyId && data.BankCurrencyId == $scope.BaseCurrencyId) {
            data.BankAmount = Math.abs(data.Amount * $scope.advance.CompanyCurrencyRate).toFixed(2);
            data.BaseDrAmount = Math.abs(data.Amount * $scope.advance.CompanyCurrencyRate).toFixed(2);
        }
        else
            data.BankAmount = '';
    }

    $scope.bankCharge = {
        FinancingTypeId: null,
        FinancingTypeName: null,
        Amount: null,
        CompanyCurrencyAmount: null
    };

    $scope.bankChargesList = [];
    $scope.addCharge = function () {
        if (manualValidation("td_FinancingType", baseService.isUndefinedOrNull($scope.bankCharge.FinancingTypeId), "Charges Type is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_FinancingTypeAmount", baseService.isUndefinedOrNull($scope.bankCharge.Amount), "Amount is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_FinancingTypeCompanyCurrencyAmount", baseService.isUndefinedOrNull($scope.bankCharge.CompanyCurrencyAmount), $scope.companyCurrencyCode + " is required.")) {
            $scope.invalidRow = true;
        }
        else {
            $scope.bankCharge.FinancingTypeName = $.grep($scope.bankChargeTypeList, function (item) {
                return item.FinancingTypeId === $scope.bankCharge.FinancingTypeId;
            })[0].ExpensesUserName;
            $scope.bankChargesList.push($scope.bankCharge);
            $scope.bankCharge = {};
            $scope.calBaseAmount();
        }
    };

    $scope.copyChargesAmount = function () {
        if ($scope.advance.CurrencyId === $scope.companyCurrencyId) {
            $scope.bankCharge.CompanyCurrencyAmount = $scope.bankCharge.Amount;
        }
        else {
            $scope.bankCharge.CompanyCurrencyAmount = ($scope.bankCharge.Amount * $scope.advance.CompanyCurrencyRate).toFixed(2);
        }
    };

    bankService.getCboBankChargeTypeList(function (result) {
        $scope.bankChargeTypeList = result;
    });

    $scope.removeChargesRow = function (index) {
        $scope.bankChargesList.splice(index, 1);
        $scope.calBaseAmount();
    };
}