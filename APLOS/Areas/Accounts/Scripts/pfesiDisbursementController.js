"use strict";
pfesiDisbursementController.$inject = ["accountService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function pfesiDisbursementController(accountService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "PFESIC Disbursement";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherDetailList = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.voucherList = [];
    $scope.isCrBankAmount = false;
    $scope.isDrBankAmount = false;
    $scope.currencyDisable = false;
    $scope.isAdvance = true;
    $scope.postUrl = "accounts/voucher/PostPFESICDisbursement";
    $scope.deleteUrl = "accounts/voucher/DeletePFESICDisbursement";
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });

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


    baseService.init("Accounts/Voucher/GetPFESICDisbursementList", null, null, "DESC", "PostingDate DESC, VoucherNo", "VoucherNo");
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
        CompanyCurrencyRate:1
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

    $scope.getCboVoucherTypePFESICDisbursementVoucherList = function () {
        accountService.getCboVoucherTypePFESICDisbursementVoucherList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
                $scope.voucher.PostingDate = $filter("dateFiltering")($scope.voucherTypeList[0].LastPostingDate);
                $scope.voucher.DocDate = $scope.voucher.PostingDate;
                $scope.GetCurrencyExchangeRateList();
            }
        });
    };
    $scope.getCboVoucherTypePFESICDisbursementVoucherList();

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.voucher.VoucherTypeId = data.Value;
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.voucher.DocDate = $filter("dateFiltering")($scope.voucher.PostingDate);
    };

    $scope.getJournalVoucherDetailList = function (id) {
        $http({
            method: "get",
            url: "accounts/voucher/GetAdvanceJournalVoucherDetailList?voucherId=" + id
        }).then(function successCallback(response) {
            $scope.voucherDetailList = response.data.Rows;
        });
    };

    $scope.Get = function (data) {
        $scope.voucher.Id = data.Id;
        $scope.voucher.DocRefNo = data.DocRefNo;
        $scope.voucher.Narration = data.Narration;
        $scope.voucher.CurrencyId = data.CurrencyId;
        $scope.voucher.EntityId = data.EntityId;
        $scope.GetCurrencyExchangeRateList();
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.PostingDate);
        $scope.voucher.DocDate = $filter("dateFiltering")(data.DocDate);
        $scope.currencyDisable = true;
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.getJournalVoucherDetailList($scope.voucher.Id);
    };

    $scope.addRow = function (data) {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure", "GLPopUp");
            return true;
        }
        if ($scope.companyConfig.IsVoucherFromBudget)
            var getRow = $filter("filter")($scope.voucherDetailList, { "BudgetMasterId": data.BudgetMasterId, "ActivityId": data.ActivityId });

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
            $scope.voucherDetail.CrAmount = null;
            $scope.voucherDetail.DrAmount = null;
            $scope.voucherDetail.Id = null;
            $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
            $scope.voucherDetail = {};
            $scope.closeCOAICodeListPopUp();
        }
    };

    $scope.removeRow = function (index) {
        $scope.voucherDetailList.splice(index, 1);
    };

    $scope.searchglByList = [
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
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

    $scope.GetCOAICodeList = function () {
        $scope.GLUrl1 = "Accounts/glitem/GetAllGLBudgetActivityPostingAutomaticOnly";
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetCOAICodeListData();
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };

    $scope.closeCOAICodeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
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
        $scope.voucherDetail.CostCenterName = $("#costCenterId option:selected").text();
        $scope.voucherDetail.CostCenterId = costCenterId;
        cboService.getCboEntityByCostCenter(costCenterId, function (result) {
            $scope.costCenterEntityList = result;
        });
    };

    $scope.SelectedCostCenterEntityItem = function (id) {
        $scope.voucherDetail.EntityName = $("#costcenterentityId option:selected").text();
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
    //$scope.checkDocDate = function () {
    //    var msg = "";
    //    if (new Date($scope.voucher.DocDate) > new Date()) {
    //        $scope.invalidDocDate = true;
    //        msg = "Doc date must be below or equal to current Date!";
    //    }
    //    else $scope.invalidDocDate = false;
    //    return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    //};

    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.voucher.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else if (new Date($scope.voucher.PostingDate) < new Date($scope.voucher.DocDate)) {
            msg = "Doc date must be below or equal to Posting Date!";
            $scope.invalidDocDate = true;
        }
        else $scope.invalidDocDate = false;
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    };

    $scope.invalidPostingDate = false;
    //$scope.checkPostingDate = function () {
    //    var msg = "";
    //    if (new Date($scope.voucher.PostingDate) > new Date()) {
    //        msg = "Posting date must be below or equal to current Date!";
    //        $scope.currencyExchangeRate = [];
    //        $scope.invalidPostingDate = true;
    //    }
    //    else if (new Date($scope.voucher.PostingDate) < new Date($scope.voucher.DocDate)) {
    //        msg = "Posting date must be below or equal to Doc Date!";
    //        $scope.currencyExchangeRate = [];
    //        $scope.invalidPostingDate = true;
    //    } else {
    //        $scope.invalidPostingDate = false;
    //    }
    //    return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    //};

    $scope.checkPostingDate = function () {
        var msg = "";
        if (new Date($scope.voucher.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.voucher.FiscalYearId = null;
            $scope.voucher.FiscalYearName = null;
            $scope.voucher.FiscalYearPeriodId = null;
            $scope.voucher.FiscalYearPeriodName = null;
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher.Active = true;
        $scope.voucher.Amount = 0;
        $scope.voucher.DocRefNo = null;
        $scope.voucher.Narration = null;
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.getCboVoucherTypePFESICDisbursementVoucherList();
        $scope.currencyDisable = false;
        $scope.currencyExchangeRate = [];
        $scope.voucherDetailList = [];
    };
    $scope.financeCboAdvanceJoural = function () {
        $scope.financingTypeList = [];
        cboService.getCboFinanceTypeForAdvanceJournal(function (result) {
            $scope.financingTypeList = result;
            $scope.voucher.FinancingTypeName = $scope.financingTypeList[0].FinancingTypeName;
        });
    }
    
    $scope.changePartyType = function (partyType) {
        $scope.partyType = partyType;
        if (partyType == 'Customer' || partyType == 'Vendor')
            $scope.financeCboAdvanceJoural();
        if (partyType == 'InterTransaction') {
            $scope.interComFinanceType();
            $scope.voucher.FinancingTypeName = null;
        }
        else
            $scope.voucher.FinancingTypeName = null;
    };

    $scope.getPartyPlantList = function (partyId) {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.partyPlantList.push(item);
                    if (item.IsDefault) {
                        $scope.partyPlantId = item.Value;
                        $scope.voucherDetail.PartyPlantId = item.Value;
                    }
                });
            });
    };

    $scope.showInterPartyPopUp = function () {
        baseService.setCurrentPage('partyList');
        $scope.getPartyList = function (pageno) {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList';
            baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
                .then(function (result) {
                    $scope.partyList = result.Rows;
                    $scope.partyParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#partyPopUp')).modal('show');
        $scope.getPartyList();
    };

    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            if ($scope.voucher.PartyType == 'Vendor' || $scope.voucher.PartyType == 'Customer') {
                if (baseService.isUndefinedOrNull(party.ReconciliationGLId)) {
                    ShowResult($scope.partyType + " GL not found!", "failure", "partyPopUp");
                    return;
                }
                else {
                    if ($scope.voucher.FinancingTypeName == 'Regular') {
                        $scope.getPartyPlantListWithCallBack(party.Id, function (result) {
                            $scope.voucherDetail.GLGeneralInfoId = party.ReconciliationGLId;
                            $scope.voucherDetail.GLGeneralInfoCode = party.ReconciliationGLCode;
                            $scope.voucherDetail.GLGeneralInfoName = party.ReconciliationGLName;
                            $scope.voucherDetail.BudgetMasterId = party.ReconciliationBudgetId;
                            $scope.voucherDetail.BudgetCode = party.ReconciliationBudgetCode;
                            $scope.voucherDetail.BudgetName = party.ReconciliationBudgetName;
                            $scope.voucherDetail.ActivityId = party.ReconciliationActivityId;
                            $scope.voucherDetail.ActivityCode = party.ReconciliationActivityCode;
                            $scope.voucherDetail.ActivityName = party.ReconciliationActivityName;
                            $scope.voucherDetail.ParticularName = party.Code + ' - ' + party.UserName;
                            $scope.voucherDetail.PartyId = party.Id;
                            $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                            $scope.voucherDetail.Narration = $scope.voucher.Narration;
                            $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                            $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
                            $scope.voucherDetail.PartyPlantId = party.PartyPlantId;
                            $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                            $scope.voucherDetail.CrAmount = null;
                            $scope.voucherDetail.DrAmount = null;
                            $scope.voucherDetail.Id = null;
                            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                            $scope.voucherDetail = {};
                            $scope.GetCurrencyExchangeRateList();
                        });
                    }
                    else {
                        $scope.getPartyPlantListWithCallBack(party.Id, function (result) {
                            $scope.voucherDetail.ParticularName = party.Code + ' - ' + party.UserName;
                            $scope.voucherDetail.PartyId = party.Id;
                            $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                            $scope.voucherDetail.Narration = $scope.voucher.Narration;
                            $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                            $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
                            $scope.voucherDetail.PartyPlantId = party.PartyPlantId;
                            $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                            $scope.voucherDetail.CrAmount = null;
                            $scope.voucherDetail.DrAmount = null;
                            $scope.voucherDetail.Id = null;
                            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                            $scope.voucherDetail = {};
                            $scope.voucher.FinancingTypeId = null;
                            $scope.partyType = null;
                            $scope.voucher.FinancingTypeName = null;
                        });
                    }
                }
            }
            if ($scope.voucher.PartyType == 'InterTransaction') {
                if (baseService.isUndefinedOrNull($scope.voucher.TransactionTypeName)) {
                    ShowResult("Please select Transaction Type!", "failure", "partyPopUp");
                    return;
                } else {
                    $scope.getPartyPlantListWithCallBack(party.Id, function (result) {
                        $scope.voucherDetail.ParticularName = party.Code + ' - ' + party.UserName;
                        $scope.voucherDetail.PartyId = party.Id;
                        $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                        $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                        $scope.voucherDetail.Narration = $scope.voucher.Narration;
                        $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                        $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
                        $scope.voucherDetail.PartyPlantId = party.PartyPlantId;
                        $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                        $scope.voucherDetail.CrAmount = null;
                        $scope.voucherDetail.DrAmount = null;
                        $scope.voucherDetail.Id = null;
                        $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                        $scope.voucherDetail = {};
                        $scope.voucher.FinancingTypeId = null;
                        $scope.partyType = null;
                        $scope.voucher.TransactionTypeName = null;
                    });
                }

            }
            if ($scope.voucher.PartyType == 'Director') {
                if (baseService.isUndefinedOrNull($scope.voucher.FinancingTypeName)) {
                    ShowResult("Please select Financing Type!", "failure", "partyPopUp");
                    return;
                } else {
                    $scope.getPartyPlantListWithCallBack(party.Id, function (result) {
                        $scope.voucherDetail.ParticularName = party.Code + ' - ' + party.UserName;
                        $scope.voucherDetail.PartyId = party.Id;
                        $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                        $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                        $scope.voucherDetail.Narration = $scope.voucher.Narration;
                        $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                        $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
                        $scope.voucherDetail.PartyPlantId = party.PartyPlantId;
                        $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                        $scope.voucherDetail.CrAmount = null;
                        $scope.voucherDetail.DrAmount = null;
                        $scope.voucherDetail.Id = null;
                        $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                        $scope.voucherDetail = {};
                        $scope.voucher.FinancingTypeId = null;
                        $scope.partyType = null;
                        $scope.voucher.FinancingTypeName = null;
                    });
                }

            }
        }
        $scope.hidePartyPopUp();
    };

    cboService.getCboFinanceTypeForAdvanceJournal(function (result) {
        $scope.financingTypeList = result;
    });

    $scope.changeFinancingType = function () {
        var row1 = $filter("filter")($scope.financingTypeList, { "FinancingTypeName": $scope.voucher.FinancingTypeName });
        $scope.voucherDetail.GLGeneralInfoId = row1[0].GLId;
        $scope.voucherDetail.GLGeneralInfoCode = row1[0].GLCode;
        $scope.voucherDetail.GLGeneralInfoName = row1[0].GLName;
        $scope.voucherDetail.BudgetMasterId = row1[0].BudgetMasterId;
        $scope.voucherDetail.BudgetCode = row1[0].BudgetCode;
        $scope.voucherDetail.BudgetName = row1[0].BudgetName;
        $scope.voucherDetail.ActivityId = row1[0].ActivityId;
        $scope.voucherDetail.ActivityName = row1[0].ActivityName;
        $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
        $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
        $scope.voucherDetail.Narration = $scope.voucher.Narration;
        $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
        $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
    };


    cboService.getCboAdvPayTranType(function (result) {
        $scope.employeeTransactionTypeList = result;
        if ($scope.employeeTransactionTypeList.length === 1) {
            $scope.voucher.EmployeeTransactionTypeName = $scope.employeeTransactionTypeList[0].EmployeeTransactionTypeName;
        }
    });
    $scope.changeTransactionType = function () {
        var row = $filter("filter")($scope.employeeTransactionTypeList, { "EmployeeTransactionTypeName": $scope.voucher.EmployeeTransactionTypeName });
        $scope.voucherDetail.GLGeneralInfoId = row[0].GLId;
        $scope.voucherDetail.GLGeneralInfoCode = row[0].GLCode;
        $scope.voucherDetail.GLGeneralInfoName = row[0].GLName;
        $scope.voucherDetail.BudgetMasterId = row[0].BudgetMasterId;
        $scope.voucherDetail.BudgetCode = row[0].BudgetCode;
        $scope.voucherDetail.BudgetName = row[0].BudgetName;
        $scope.voucherDetail.ActivityId = row[0].ActivityId;
        $scope.voucherDetail.ActivityName = row[0].ActivityName;
        $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
        $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
        $scope.voucherDetail.Narration = $scope.voucher.Narration;
        $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
        $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
    };

    //cboService.GetCboAssetLiabilityTranType(function (result) {
    //    $scope.financingTypeList = result;
    //    if ($scope.financingTypeList.length === 1) {
    //        $scope.voucher.FinancingTypeName = $scope.financingTypeList[0].FinancingTypeName;
    //    }
    //});

    //$scope.changeFinancingType = function () {
    //    var row1 = $filter("filter")($scope.financingTypeList, { "FinancingTypeName": $scope.voucher.FinancingTypeName });
    //    $scope.voucherDetail.GLGeneralInfoId = row1[0].GLId;
    //    $scope.voucherDetail.GLGeneralInfoCode = row1[0].GLCode;
    //    $scope.voucherDetail.GLGeneralInfoName = row1[0].GLName;
    //    $scope.voucherDetail.BudgetMasterId = row1[0].BudgetMasterId;
    //    $scope.voucherDetail.BudgetCode = row1[0].BudgetCode;
    //    $scope.voucherDetail.BudgetName = row1[0].BudgetName;
    //    $scope.voucherDetail.ActivityId = row1[0].ActivityId;
    //    $scope.voucherDetail.ActivityName = row1[0].ActivityName;
    //    $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
    //    $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
    //    $scope.voucherDetail.Narration = $scope.voucher.Narration;
    //    $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
    //    $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
    //};

    $scope.showEmployeeListPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.EmployeeTransactionTypeName)) {
            ShowResult("Please select Transaction Type!", "failure", "employeePopUp");
            return;
        }
        baseService.setCurrentPage('employeeList');
        $scope.getEmployeeData = function (pageno) {
            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'employees/EmployeeInformation/GetEmployeeListByPlant';
            }
            else {
                url = $scope.employeeUrl;
            }
            baseService.paginationBase(url, pageno, $scope.employeeParameters)
                .then(function (result) {
                    $scope.employeeList = result.Rows;
                    $scope.employeeParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeePopUp')).modal('show');
        $scope.getEmployeeData();
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.voucherDetail.EmployeeId = employee.SystemId;
            $scope.voucherDetail.ParticularName = employee.EmployeeName;
            $scope.voucherDetail.CrAmount = null;
            $scope.voucherDetail.DrAmount = null;
            $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
            $scope.voucherDetail = {};
            $scope.voucher.EmployeeTransactionTypeName = null;
            $scope.voucherDetail.Id = null;
        }
        $scope.hideEmployeePopUp();
    };
    $scope.interComFinanceType = function () {
        $scope.financingTypeList = [];
        cboService.getInterCompanyAssetLiabilityType(function (result) {
            $scope.financingTypeList = result;
        });
    }
   

    $scope.changeInterTransactionType = function () {
        var row = $filter("filter")($scope.financingTypeList, { "TransactionTypeName": $scope.voucher.TransactionTypeName });
        $scope.voucherDetail.GLGeneralInfoId = row[0].GLId;
        $scope.voucherDetail.GLGeneralInfoCode = row[0].GLCode;
        $scope.voucherDetail.GLGeneralInfoName = row[0].GLName;
        $scope.voucherDetail.BudgetMasterId = row[0].BudgetMasterId;
        $scope.voucherDetail.BudgetCode = row[0].BudgetCode;
        $scope.voucherDetail.BudgetName = row[0].BudgetName;
        $scope.voucherDetail.ActivityId = row[0].ActivityId;
        $scope.voucherDetail.ActivityName = row[0].ActivityName;
        $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
        $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
        $scope.voucherDetail.Narration = $scope.voucher.Narration;
        $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
        $scope.voucherDetail.PlantId = $scope.voucher.PlantId;

    };


    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];
            if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
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
                $scope.voucherDetail.ParticularName = bank.AccountTitle;
                $scope.voucherDetail.BankMasterId = bank.BankMasterId;
                setBankGL(bank);
            }
        }
        $scope.hideBankPopUp();
    };

    function setBankGL(bank) {
        $scope.voucherDetail.BankCurrencyId = bank.CurrencyId;
        $scope.voucherDetail.GLGeneralInfoId = bank.GLGeneralInfoId;
        $scope.voucherDetail.GLGeneralInfoCode = bank.GLGeneralInfoCode;
        $scope.voucherDetail.GLGeneralInfoName = bank.GLGeneralInfoName;
        $scope.voucherDetail.BudgetMasterId = bank.BudgetMasterId;
        $scope.voucherDetail.BudgetCode = bank.BudgetCode;
        $scope.voucherDetail.BudgetName = bank.BudgetName;
        $scope.voucherDetail.ActivityId = bank.ActivityId;
        $scope.voucherDetail.ActivityCode = bank.ActivityCode;
        $scope.voucherDetail.ActivityName = bank.ActivityName;
        $scope.voucherDetail.CrAmount = null;
        $scope.voucherDetail.DrAmount = null;
        $scope.voucherDetail.Id = null;
        $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
        $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
        $scope.voucherDetail = {};
    }

    $scope.closeCashPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select currency!", "failure", "cashPopUp");
            return;
        }
        if ($scope.cashIndex !== -1) {
            var cash = $scope.cashList[$scope.cashIndex];
            if (baseService.isUndefinedOrNull(cash.GLGeneralInfoId)) {
                ShowResult("Cash GL not found!", "failure", "cashPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(cash.BudgetMasterId)) {
                ShowResult("Cash budget not found!", "failure", "cashPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(cash.CurrencyId)) {
                ShowResult("Cash transaction currency not found!", "failure", "cashPopUp");
                return;
            }
            else {
                $scope.voucherDetail.CashMasterId = cash.Id;
                $scope.voucherDetail.ParticularName = cash.CashName;
                setCashGL(cash);
            }
        }
        $scope.hideCashPopUp();
    };

    function setCashGL(cash) {
        $scope.voucherDetail.CashCurrencyId = cash.CurrencyId;
        $scope.voucherDetail.GLGeneralInfoId = cash.GLGeneralInfoId;
        $scope.voucherDetail.GLGeneralInfoCode = cash.GLGeneralInfoCode;
        $scope.voucherDetail.GLGeneralInfoName = cash.GLGeneralInfoName;
        $scope.voucherDetail.BudgetMasterId = cash.BudgetMasterId;
        $scope.voucherDetail.BudgetCode = cash.BudgetCode;
        $scope.voucherDetail.BudgetName = cash.BudgetName;
        $scope.voucherDetail.ActivityId = cash.ActivityId;
        $scope.voucherDetail.ActivityCode = cash.ActivityCode;
        $scope.voucherDetail.ActivityName = cash.ActivityName;
        $scope.voucherDetail.CrAmount = null;
        $scope.voucherDetail.DrAmount = null;
        $scope.voucherDetail.Id = null;
        $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
        $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
        $scope.voucherDetail = {};
    }


    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.form0.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "accounts/voucher/ParkPFESICDisbursement",
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
                    url: "accounts/voucher/UpdateAdvanceJournal",
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

    $scope.removeDetaillRow = function (Id,voucherId, index) {
        if (Id === null) {
            //$(this).remove();
            $scope.voucherDetailList.splice(index, 1);
            return false;
        }
        else {
            $scope.message = 'Are you sure want to permanently delete this?';
            angular.element(document.querySelector('#removePopUp')).modal('show');
            $scope.vdId = Id;
            $scope.voucherId = voucherId;
            $scope.mateIndex = index;
        }
    };

    $scope.detailDelete = function () {
        try {
            $http({
                method: 'POST',
                url: 'Accounts/Voucher/DeleteVoucherDetail?Id=' + $scope.vdId + '&voucherId=' + $scope.voucherId,
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.vdId = null;
                    $scope.voucherId = null;
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

    //****************Fixed Asser AUC GL******************************
    $scope.searchFAMglByList = [
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "FixedAsset Master",
            "value": "FixedAssetMasterName"
        },
        {
            "name": "GL ",
            "value": "AccDepreciationGLInfo"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        }
    ];

    $scope.fAFAMGlListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "FixedAssetName",
        searchBy: "FixedAssetName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetFAFAMList = function () {
        $scope.GLUrl4 = "Accounts/glitem/GetFixedAssetMasterGL";
        $scope.GetFAFAMData = function (pageno) {
            baseService.paginationBase($scope.GLUrl4, pageno, $scope.fAFAMGlListParameters)
                .then(function (result) {
                    $scope.fAFAMList = result.Rows;
                    $scope.fAFAMGlListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#fixedAssetMasterFAMModal")).modal("show");
        $scope.modalShow = true;
        $scope.GetFAFAMData();

    };

    $scope.closefAFAMListPopUp = function () {
        angular.element(document.querySelector("#fixedAssetMasterFAMModal")).modal("hide");
    };

    $scope.closefAFAMListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#fixedAssetMasterFAMModal")).modal("hide");
        } else {
            angular.element(document.querySelector("#fixedAssetMasterFAMModal")).modal("show");
        }
    };
    $scope.setFAFAMSelected = function (data) {
        $scope.addFARow(data);
    };
    //******************End Fixed Asser Acc Depreciation GL*************************

    //****************Fixed Asser Acc Depreciation GL******************************
    $scope.searchFAADglByList = [
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "FixedAsset Master",
            "value": "FixedAssetMasterName"
        },
        {
            "name": "GL ",
            "value": "AccDepreciationGLInfo"
        },
        {
            "name": "Budget",
            "value": "AccumulatedDepreciationBudgetName"
        },
        {
            "name": "Activity",
            "value": "AccumulatedDepreciationActivityName"
        }
    ];

    $scope.fAADGlListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "FixedAssetMasterName",
        searchBy: "FixedAssetMasterName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetFAAccDepList = function () {
        $scope.GLUrl2 = "Accounts/glitem/GetFixedAssetAccDepGL";
        $scope.GetFAAccDepData = function (pageno) {
            baseService.paginationBase($scope.GLUrl2, pageno, $scope.fAADGlListParameters)
                .then(function (result) {
                    $scope.fAAccDepList = result.Rows;
                    $scope.fAADGlListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#fixedAssetMasterAccDepModal")).modal("show");
        $scope.modalShow = true;
        $scope.GetFAAccDepData();

    };

    $scope.closefAAccDepListPopUp = function () {
        angular.element(document.querySelector("#fixedAssetMasterAccDepModal")).modal("hide");
    };

    $scope.closefAAccDepListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#fixedAssetMasterAccDepModal")).modal("hide");
        } else {
            angular.element(document.querySelector("#fixedAssetMasterAccDepModal")).modal("show");
        }
    };
    $scope.setFAADSelected = function (data) {
        $scope.addFARow(data);
    };
    //******************End Fixed Asser Acc Depreciation GL*************************

    //****************Fixed Asser AUC GL******************************
    $scope.searchFAAUCglByList = [
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "FixedAsset Master",
            "value": "FixedAssetMasterName"
        },
        {
            "name": "GL ",
            "value": "AccDepreciationGLInfo"
        },
        {
            "name": "Budget",
            "value": "AccumulatedDepreciationBudgetName"
        },
        {
            "name": "Activity",
            "value": "AccumulatedDepreciationActivityName"
        }
    ];

    $scope.fAAUCGlListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "FixedAssetMasterName",
        searchBy: "FixedAssetMasterName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetFAAUCList = function () {
        $scope.GLUrl3 = "Accounts/glitem/GetFixedAssetAUCGL";
        $scope.GetFAAUCData = function (pageno) {
            baseService.paginationBase($scope.GLUrl3, pageno, $scope.fAAUCGlListParameters)
                .then(function (result) {
                    $scope.fAAUCList = result.Rows;
                    $scope.fAAUCGlListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#fixedAssetMasterAUCModal")).modal("show");
        $scope.modalShow = true;
        $scope.GetFAAUCData();

    };

    $scope.closefAAUCListPopUp = function () {
        angular.element(document.querySelector("#fixedAssetMasterAUCModal")).modal("hide");
    };

    $scope.closefAAUCListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#fixedAssetMasterAUCModal")).modal("hide");
        } else {
            angular.element(document.querySelector("#fixedAssetMasterAUCModal")).modal("show");
        }
    };
    $scope.setFAAUCSelected = function (data) {
        $scope.addFARow(data);
    };
    //******************End Fixed Asser Acc Depreciation GL*************************


    $scope.addFARow = function (data) {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure", "GLPopUp");
            return true;
        }
        if ($scope.voucher.FAType === 'AssetCapatalized') {
            if ($scope.companyConfig.IsVoucherFromBudget)
                var getRow = $filter("filter")($scope.voucherDetailList, { "BudgetMasterId": data.BudgetMasterId, "ActivityId": data.ActivityId });

            if (baseService.isUndefinedOrNull(data.FixedAssetMasterId)) {
                ShowResult("FixedAsset is not Mapped With Budget!", "failure", "fixedAssetMasterFAMModal");
                return true;
            }
            if (baseService.isUndefinedOrNull(data.ActivityId)) {
                ShowResult("Activity is not Mapped!", "failure", "fixedAssetMasterFAMModal");
                return true;
            }
            if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0 && getRow[0].ActivityId === data.ActivityId) {
                ShowResult("This Activity is already added!", "failure", "fixedAssetMasterFAMModal");
                return true;
            }
            else {
                $scope.voucherDetail.BudgetMasterId = data.BudgetMasterId;
                $scope.voucherDetail.BudgetName = data.BudgetName;
                $scope.voucherDetail.ActivityId = data.ActivityId;
                $scope.voucherDetail.ActivityCode = data.ActivityCode;
                $scope.voucherDetail.ActivityName = data.ActivityName;

                $scope.voucherDetail.GLGeneralInfoId = data.GLGeneralInfoId;
                $scope.voucherDetail.GLGeneralInfoName = data.GLGeneralInfoCode + '-' + data.GLGeneralInfoName;
                $scope.voucherDetail.FixedAssetMasterId = data.FixedAssetMasterId;
                $scope.voucherDetail.ParticularName = data.FixedAssetName;
                $scope.voucherDetail.FAType = $scope.voucher.FAType;

                $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                $scope.voucherDetail.Narration = $scope.voucher.Narration;
                $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                $scope.voucherDetail.CrAmount = null;
                $scope.voucherDetail.DrAmount = null;
                $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
                $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                $scope.voucherDetail = {};
                $scope.closefAFAMListPopUp();
            }
        }
        if ($scope.voucher.FAType === 'AccDept') {
            if ($scope.companyConfig.IsVoucherFromBudget)
                var getRow = $filter("filter")($scope.voucherDetailList, { "BudgetMasterId": data.AccumulatedDepreciationBudgetMasterId, "ActivityId": data.AccumulatedDepreciationActivityId });

            if (baseService.isUndefinedOrNull(data.FixedAssetMasterId)) {
                ShowResult("FixedAsset is not Mapped With Budget!", "failure", "fixedAssetMasterAccDepModal");
                return true;
            }
            if (baseService.isUndefinedOrNull(data.AccumulatedDepreciationActivityId)) {
                ShowResult("Activity is not Mapped!", "failure", "fixedAssetMasterAccDepModal");
                return true;
            }
            if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0 && getRow[0].ActivityId === data.AccumulatedDepreciationActivityId) {
                ShowResult("This Activity is already added!", "failure", "fixedAssetMasterAccDepModal");
                return true;
            }
            else {
                $scope.voucherDetail.BudgetMasterId = data.AccumulatedDepreciationBudgetMasterId;
                $scope.voucherDetail.BudgetName = data.AccumulatedDepreciationBudgetName;
                $scope.voucherDetail.ActivityId = data.AccumulatedDepreciationActivityId;
                $scope.voucherDetail.ActivityCode = data.ActivityCode;
                $scope.voucherDetail.ActivityName = data.AccumulatedDepreciationActivityName;

                $scope.voucherDetail.GLGeneralInfoId = data.AccumulatedDepreciationGLId;
                $scope.voucherDetail.GLGeneralInfoName = data.AccDepreciationGLInfo;
                $scope.voucherDetail.FixedAssetMasterId = data.FixedAssetMasterId;
                $scope.voucherDetail.ParticularName = data.FixedAssetMasterName;
                $scope.voucherDetail.FAType = $scope.voucher.FAType;

                $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                $scope.voucherDetail.Narration = $scope.voucher.Narration;
                $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                $scope.voucherDetail.CrAmount = null;
                $scope.voucherDetail.DrAmount = null;
                $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
                $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                $scope.voucherDetail = {};
                $scope.closefAAccDepListPopUp();
            }
        }
        if ($scope.voucher.FAType === 'AssetNonCapitalized') {
            if ($scope.companyConfig.IsVoucherFromBudget)
                var getRow = $filter("filter")($scope.voucherDetailList, { "BudgetMasterId": data.AssetUnderConstructionBudgetMasterId, "ActivityId": data.AssetUnderConstructionActivityId });

            if (baseService.isUndefinedOrNull(data.FixedAssetMasterId)) {
                ShowResult("FixedAsset is not Mapped With Budget!", "failure", "fixedAssetMasterFAMModal");
                return true;
            }
            if (baseService.isUndefinedOrNull(data.AssetUnderConstructionActivityId)) {
                ShowResult("Activity is not Mapped!", "failure", "fixedAssetMasterFAMModal");
                return true;
            }
            if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0 && getRow[0].ActivityId === data.AssetUnderConstructionActivityId) {
                ShowResult("This Activity is already added!", "failure", "fixedAssetMasterFAMModal");
                return true;
            }
            else {
                $scope.voucherDetail.BudgetMasterId = data.AssetUnderConstructionBudgetMasterId;
                $scope.voucherDetail.BudgetName = data.AssetUnderConstructionBudgetName;
                $scope.voucherDetail.ActivityId = data.AssetUnderConstructionActivityId;
                $scope.voucherDetail.ActivityCode = data.ActivityCode;
                $scope.voucherDetail.ActivityName = data.AssetUnderConstructionActivityName;

                $scope.voucherDetail.GLGeneralInfoId = data.AssetUnderConstructionGLId;
                $scope.voucherDetail.GLGeneralInfoName = data.AUCGLInfo;
                $scope.voucherDetail.FixedAssetMasterId = data.FixedAssetMasterId;
                $scope.voucherDetail.FixedAssetMasterId = data.FixedAssetMasterId;
                $scope.voucherDetail.ParticularName = data.FixedAssetMasterName;
                $scope.voucherDetail.FAType = $scope.voucher.FAType;

                $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                $scope.voucherDetail.Narration = $scope.voucher.Narration;
                $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                $scope.voucherDetail.CrAmount = null;
                $scope.voucherDetail.DrAmount = null;
                $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
                $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                $scope.voucherDetail = {};
                $scope.closefAAUCListPopUp();
            }
        }
    };


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

    //#endregion

}