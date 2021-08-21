"use strict";
advanceJournalOpeningBalanceController.$inject = ["accountService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function advanceJournalOpeningBalanceController(accountService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Opening Balance Journal Voucher";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherDetailList = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.voucherList = [];
    $scope.isCrBankAmount = false;
    $scope.isDrBankAmount = false;
    $scope.currencyDisable = false;
    $scope.isAdvance = true;
    $scope.postUrl = "accounts/OpeningBalance/PostOBAdvanceJournal";
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });

    $scope.voucherDetailList = [];
    $scope.searchvoucherList = [
        {
            "name": "VoucherNo",
            "value": "VoucherNo"
        },
        {
            "name": "VoucherDate",
            "value": "VoucherDate"
        },
        {
            "name": "Doc Ref No",
            "value": "DocRefNo"
        },
        {
            "name": "Voucher Type",
            "value": "VoucherType"
        }
    ];

    $scope.voucherListParameters = {
        limit: 10,
        offset: 0,
        order: "DESC",
        sort: "DocRefNo",
        searchBy: "DocRefNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    baseService.init("Accounts/OpeningBalance/GetOBAdvanceJournalList", null, null, "DESC", "PostingDate DESC, DocRefNo", "PostingDate");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.voucherList = result.Rows;
                $scope.voucherListParameters.total_count = result.Total;
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
        CompanyCurrencyRate: 1,
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        DocDate: null,
        DocRefNo: null,
        Amount: 0,
        Narration: null,
        EmployeeTransactionTypeName: null,
        IsPark: false,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        FAType: null
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
        PartyPlantId: null,
        TransactionTypeId: null,
        FAType: null,
        DrDisable: false,
        CrDisable: false,
        CashCurrencyId: null,
        BankCurrencyId: null,
        BankAmount: null
    };

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
        $scope.getCutOffDate();
    });

    $scope.getCutOffDate = function () {
        $http.get('accounts/OpeningBalance/GetACCCutOffDate')
            .then(function (response) {
                if (response.data !== null && !baseService.isUndefinedOrNull(response.data.CutOffDate)) {
                    $scope.voucher.PostingDate = $filter('dateFiltering')(response.data.CutOffDate);
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
    };

    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.tranCurrencyList = result;
        $scope.voucher.CurrencyId = $scope.selectBaseCurrency();
    });

    $scope.GetCboVoucherTypeOpeningBalanceList = function () {
        cboService.getCboVoucherTypeOpeningBalanceList(function (result) {
            $scope.voucherTypeList = result;
            if ($scope.voucherTypeList.length === 1) {
                $scope.voucher.VoucherTypeId = $scope.voucherTypeList[0].Value;
            }
        });
    };
    $scope.GetCboVoucherTypeOpeningBalanceList();

    $scope.changeVoucherType = function (voucherTypeId) {
        var data = $.grep($scope.voucherTypeList, function (item) {
            return item.Value === voucherTypeId;
        })[0];
        $scope.voucher.VoucherTypeId = data.Value;
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.LastPostingDate);
        $scope.voucher.DocDate = $scope.voucher.PostingDate;
    };

    $scope.getOBDetailList = function (id) {
        $http({
            method: "get",
            url: "accounts/openingbalance/GetOBAdvanceJournalDetail?openingBalanceId=" + id
        }).then(function successCallback(response) {
            $scope.voucherDetailList = response.data.Rows;
            //angular.forEach($scope.voucherDetailList, function (item, i) {
            //    item.DocDate = $filter('dateFiltering')(item.DocDate);
            //    if (!baseService.isUndefinedOrNull(item.CompanyId)) {
            //        $scope.companyChange(item.CompanyId);
            //        if (!baseService.isUndefinedOrNull(item.PlantId)) {
            //            $scope.plantChange(item.PlantId, item.CompanyId);
            //        }
            //    }
            //});
        });
    };

    $scope.plantChange = function (plantId, companyId, row) {
        if (baseService.isUndefinedOrNull(plantId) || baseService.isUndefinedOrNull(companyId)) {
            return;
        }
        $scope.interEntityList = [];
        cboService.getCboEntityPlantWise(null, companyId, plantId, function (result) {
            $scope.interEntityList = result;
        });
        $scope.getPlantInfo(plantId, row);
    };

    $scope.plant = null;
    $scope.getPlantInfo = function (plantId, row) {
        if (!baseService.isUndefinedOrNull(plantId)) {
            $scope.plant = $filter("filter")($scope.interplantList, { "PlantId": plantId });
            if ($scope.plant.length) {
                if (!baseService.isUndefinedOrNull($scope.plant[0].PartyId)) {
                    row.PartyId = $scope.plant[0].PartyId;
                    row.PartyPlantId = $scope.plant[0].PartyPlantId;
                }
                else {
                    row.PartyId = null;
                    row.PartyPlantId = null;
                }
            }
        }
        else
            $scope.plant = null;
    };

    $scope.companyChange = function (id) {
        $scope.interEntityList = [];
        cboService.getCboInterPlant(null, id, null, function (result) {
            $scope.interplantList = result;
        });
    };

    $scope.Get = function (data) {
        $scope.voucher.Id = data.Id;
        $scope.voucher.OpeningBalanceId = data.OpeningBalanceId;
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.PostingDate);
        $scope.voucher.DocDate = $filter("dateFiltering")(data.DocDate);
        $scope.voucher.DocRefNo = data.DocRefNo;
        $scope.voucher.Narration = data.Narration;
        $scope.voucher.CurrencyId = data.CurrencyId;
        $scope.voucher.EntityId = data.EntityId;
        $scope.voucher.AddedBy = data.AddedBy;
        $scope.voucher.IsPark = data.IsPark;
        $scope.voucher.AddedDate = data.AddedDate;
        $scope.voucher.AddedFromIP = data.AddedFromIP;
        $scope.GetCurrencyExchangeRateList();
        $scope.currencyDisable = true;
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.getOBDetailList($scope.voucher.Id);
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
            $scope.voucherDetail.CrAmount = data.CrAmount;
            $scope.voucherDetail.DrAmount = data.DrAmount;
            $scope.voucherDetail.DrDisable = true;
            $scope.voucherDetail.CrDisable = true;
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
        }
    ];

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoCode",
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetCOAICodeList = function () {
        if ($scope.voucher.PartyType === 'GL')
            $scope.GLUrl1 = "Accounts/glitem/GetAllGLBudgetActivityPostingAutomaticOnly";
        else if ($scope.voucher.PartyType === 'Material')
            $scope.GLUrl1 = "Accounts/glitem/GetReconMaterialTypeGL";

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
        $scope.addGLPopUpRow(data);
        angular.element(document.querySelector("#AddOBGLJVPopUp")).modal("show");
        angular.element(document.querySelector("#GLPopUp")).modal("hide");

    };
    $scope.closeOBGLPopUp = function () {
        angular.element(document.querySelector("#AddOBGLJVPopUp")).modal("hide");
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
    $scope.glvoucherDetailList = [];
    $scope.addGLPopUpRow = function (data) {
        $scope.glvoucherDetail = {};
        $scope.glvoucherDetail.BudgetMasterId = data.BudgetMasterId;
        $scope.glvoucherDetail.BudgetCode = data.BudgetCode;
        $scope.glvoucherDetail.BudgetName = data.BudgetName;
        $scope.glvoucherDetail.ActivityId = data.ActivityId;
        $scope.glvoucherDetail.ActivityCode = data.ActivityCode;
        $scope.glvoucherDetail.ActivityName = data.ActivityName;
        $scope.glvoucherDetail.GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.glvoucherDetail.GLGeneralInfoCode = data.GLGeneralInfoCode;
        $scope.glvoucherDetail.GLGeneralInfoName = data.GLGeneralInfoName;
        $scope.glvoucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
        $scope.glvoucherDetail.DocRefNo = $scope.voucher.DocRefNo;
        $scope.glvoucherDetail.Narration = $scope.voucher.Narration;
        $scope.glvoucherDetail.EntityId = $scope.voucher.EntityId;
        $scope.glvoucherDetail.PlantId = $scope.voucher.PlantId;
        $scope.glvoucherDetail.CrAmount = null;
        $scope.glvoucherDetail.DrAmount = null;
        $scope.glvoucherDetail.DrDisable = false;
        $scope.glvoucherDetail.CrDisable = false;
        $scope.glvoucherDetail.PartyType = $scope.voucher.PartyType;
        $scope.glvoucherDetailList.splice(0, 0, $scope.glvoucherDetail);
        $scope.glvoucherDetail = {};
        angular.element(document.querySelector("#AddOBGLJVPopUp")).modal("hide");
    };

    $scope.removeglRow = function (index) {
        $scope.glvoucherDetailList.splice(index, 1);
    };

    $scope.checkGLDrAmount = function (index) {
        if ($scope.glvoucherDetailList[index].DrAmount > 0) {
            $scope.glvoucherDetailList[index].CrAmount = null;
        }
    };

    $scope.checkGLCrAmount = function (index) {
        if ($scope.glvoucherDetailList[index].CrAmount > 0) {
            $scope.glvoucherDetailList[index].DrAmount = null;
        }
    };

    $scope.SaveGL = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.form0.$valid) {

            $http({
                method: "POST",
                url: "accounts/OpeningBalance/ParkOBGLAdvanceJournal",
                data: {
                    "voucherVM": $scope.voucher,
                    "voucherDetailVMList": $scope.glvoucherDetailList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.addRow($scope.glvoucherDetailList[0]);
                    $scope.glvoucherDetailList = [];
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
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
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.voucher.DocDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to current Date!";
        }
        else $scope.invalidDocDate = false;
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    };

    $scope.invalidPostingDate = false;
    $scope.checkPostingDate = function () {
        var msg = "";
        if (new Date($scope.voucher.PostingDate) > new Date()) {
            msg = "Posting date must be below or equal to current Date!";
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        }
        else if (new Date($scope.voucher.PostingDate) < new Date($scope.voucher.DocDate)) {
            msg = "Posting date must be below or equal to Doc Date!";
            $scope.currencyExchangeRate = [];
            $scope.invalidPostingDate = true;
        } else {
            $scope.invalidPostingDate = false;
        }
        return manualValidation("div_PostingDate", $scope.invalidPostingDate, msg);
    };

    $scope.getFiscalYearPeriod = function (date) {
        if (!baseService.isUndefinedOrNull(date) && !$scope.invalidPostingDate) {
            $http({
                method: "get",
                url: "accounts/CompanyFiscalYear/CheckingFiscalYearPeriod?postingDate=" + $filter("dateFiltering")(date)
            }).then(
                function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.currencyExchangeRate = [];
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        var result = response.data;
                        if (result.IsTransationLocked === true) {
                            ShowResult(commonMessage.FiscalPeriodTransactionLocked, "failure");
                            $scope.voucher.PostingDate = "";
                            $scope.voucher.FiscalYearId = null;
                            $scope.voucher.FiscalYearName = null;
                            $scope.voucher.FiscalYearPeriodId = null;
                            $scope.voucher.FiscalYearPeriodName = null;
                            $scope.currencyExchangeRate = [];
                        }
                        else if (result.IsExchangeRateConfirmed === false) {
                            ShowResult(commonMessage.FiscalPeriodExchangeRateConfirmed, "failure");
                            $scope.voucher.PostingDate = "";
                            $scope.voucher.FiscalYearId = null;
                            $scope.voucher.FiscalYearName = null;
                            $scope.voucher.FiscalYearPeriodId = null;
                            $scope.voucher.FiscalYearPeriodName = null;
                            $scope.currencyExchangeRate = [];
                        }
                        else {
                            $scope.voucher.FiscalYearId = result.FiscalYearId;
                            $scope.voucher.FiscalYearName = result.FiscalYearName;
                            $scope.voucher.FiscalYearPeriodId = result.FiscalYearPeriodId;
                            $scope.voucher.FiscalYearPeriodName = result.PeriodName;
                            $scope.GetCurrencyExchangeRateList();
                        }
                    }
                },
                function errorCallback(response) {
                });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };
    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher.Active = true;
        $scope.voucher.Amount = 0;
        $scope.voucher.DocRefNo = null;
        $scope.voucher.Narration = null;
        $scope.voucher.CompanyCurrencyRate = 1;
        $scope.voucher.CurrencyId = $scope.selectBaseCurrency();
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.GetCboVoucherTypeOpeningBalanceList();
        $scope.getCutOffDate();
        $scope.currencyDisable = false;
        $scope.currencyExchangeRate = [];
        $scope.voucherDetailList = [];
    };

    $scope.changeFAType = function (FAType) {
        $scope.FAType = FAType;
    };
    $scope.searchByParty = "UserName"; $scope.searchParty = "";
    $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];
    $scope.changePartyType = function (partyType) {
        $scope.partyType = partyType;
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];
    };

    $scope.showPartyPopUpNew = function () {
        if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;
        }
        else if ($scope.partyType === 'Party') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
        }
        else if ($scope.partyType === 'Director') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
        }
        else if ($scope.partyType === 'Other') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
        }
        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        angular.element(document.querySelector('#partyPopUp')).modal('show');
    };
    $scope.closePartyPopUpNew = function () {
        angular.element(document.querySelector('#partyPopUp')).modal('hide');
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

    $scope.changeVendorTransactionType = function () {
        var row = $filter("filter")($scope.vendorTranTypeList, { "FinancingTypeId": $scope.voucher.FinancingTypeId });
        $scope.voucherDetail.GLGeneralInfoId = row[0].LiabilityGLId;
        $scope.voucherDetail.GLGeneralInfoCode = row[0].LiabilityGLCode;
        $scope.voucherDetail.GLGeneralInfoName = row[0].LiabilityGLName;
        $scope.voucherDetail.BudgetMasterId = row[0].LiabilityBudgetMasterId;
        $scope.voucherDetail.BudgetCode = row[0].LiabilityBudgetCode;
        $scope.voucherDetail.BudgetName = row[0].LiabilityBudgetName;
        $scope.voucherDetail.ActivityId = row[0].LiabilityActivityId;
        $scope.voucherDetail.ActivityName = row[0].LiabilityActivityName;
        $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
        $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
        $scope.voucherDetail.Narration = $scope.voucher.Narration;
        $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
        $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
        $scope.voucherDetail.TransactionTypeId = row[0].FinancingTypeId;
    };

    $scope.changeCustomerTransactionType = function () {
        var row = $filter("filter")($scope.customerTranTypeList, { "FinancingTypeId": $scope.voucher.FinancingTypeId });
        $scope.voucherDetail.GLGeneralInfoId = row[0].AssetGLId;
        $scope.voucherDetail.GLGeneralInfoCode = row[0].AssetGLCode;
        $scope.voucherDetail.GLGeneralInfoName = row[0].AssetGLName;
        $scope.voucherDetail.BudgetMasterId = row[0].AssetBudgetMasterId;
        $scope.voucherDetail.BudgetCode = row[0].AssetBudgetCode;
        $scope.voucherDetail.BudgetName = row[0].AssetBudgetName;
        $scope.voucherDetail.ActivityId = row[0].AssetActivityId;
        $scope.voucherDetail.ActivityName = row[0].AssetActivityName;
        $scope.voucherDetail.TransactionTypeId = row[0].FinancingTypeId;

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

    $scope.closePartyPopUp = function (x) {
        $scope.glvoucherDetail = {};
        $scope.glvoucherDetailList = [];
        var party = x.data;
        if ($scope.voucher.PartyType == 'Vendor' || $scope.voucher.PartyType == 'Customer') {
            if (baseService.isUndefinedOrNull($scope.voucher.FinancingTypeId)) {
                if (baseService.isUndefinedOrNull(party.ReconciliationGLId)) {
                    ShowResult($scope.partyType + " GL not found!", "failure", "partyPopUp");
                    return;
                }
                else {
                    $scope.getPartyPlantListWithCallBack(party.Id, function (result) {
                        if ($scope.voucher.InvoiceAdvance == 'Invoice') {
                            $scope.glvoucherDetail.GLGeneralInfoId = party.ReconciliationGLId;
                            $scope.glvoucherDetail.GLGeneralInfoCode = party.ReconciliationGLCode;
                            $scope.glvoucherDetail.GLGeneralInfoName = party.ReconciliationGLName;
                            $scope.glvoucherDetail.BudgetMasterId = party.ReconciliationBudgetId;
                            $scope.glvoucherDetail.BudgetCode = party.ReconciliationBudgetCode;
                            $scope.glvoucherDetail.BudgetName = party.ReconciliationBudgetName;
                            $scope.glvoucherDetail.ActivityId = party.ReconciliationActivityId;
                            $scope.glvoucherDetail.ActivityCode = party.ReconciliationActivityCode;
                            $scope.glvoucherDetail.ActivityName = party.ReconciliationActivityName;
                        }
                        if ($scope.voucher.InvoiceAdvance == 'Advance') {
                            $scope.glvoucherDetail.GLGeneralInfoId = party.DownPaymentGLId;
                            $scope.glvoucherDetail.GLGeneralInfoCode = party.DownPaymentGLCode;
                            $scope.glvoucherDetail.GLGeneralInfoName = party.DownPaymentGLName;
                            $scope.glvoucherDetail.BudgetMasterId = party.DownPaymentBudgetId;
                            $scope.glvoucherDetail.BudgetCode = party.DownPaymentBudgetCode;
                            $scope.glvoucherDetail.BudgetName = party.DownPaymentBudgetName;
                            $scope.glvoucherDetail.ActivityId = party.DownPaymentActivityId;
                            $scope.glvoucherDetail.ActivityCode = party.DownPaymentBudgetCode;
                            $scope.glvoucherDetail.ActivityName = party.DownPaymentActivityName;
                        }

                        $scope.glvoucherDetail.ParticularName = party.Code + ' - ' + party.UserName;
                        $scope.glvoucherDetail.PartyId = party.Id;
                        $scope.glvoucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                        $scope.glvoucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                        $scope.glvoucherDetail.Narration = $scope.voucher.Narration;
                        $scope.glvoucherDetail.EntityId = $scope.voucher.EntityId;
                        $scope.glvoucherDetail.PartyType = $scope.voucher.PartyType;
                        $scope.glvoucherDetail.PartyPlantId = party.PartyPlantId;
                        $scope.glvoucherDetail.PlantId = $scope.voucher.PlantId;
                        $scope.glvoucherDetail.CrAmount = null;
                        $scope.glvoucherDetail.DrAmount = null;
                        if ($scope.voucher.InvoiceAdvance == 'Invoice' && $scope.voucher.PartyType == 'Vendor'
                            || $scope.voucher.InvoiceAdvance == 'Advance' && $scope.voucher.PartyType == 'Customer'
                        ) {
                            $scope.glvoucherDetail.DrDisable = true;
                            $scope.glvoucherDetail.CrDisable = false;
                        }
                        if ($scope.voucher.InvoiceAdvance == 'Advance' && $scope.voucher.PartyType == 'Vendor'
                            || $scope.voucher.InvoiceAdvance == 'Invoice' && $scope.voucher.PartyType == 'Customer') {
                            $scope.glvoucherDetail.DrDisable = false;
                            $scope.glvoucherDetail.CrDisable = true;
                        }

                        $scope.glvoucherDetailList.splice(0, 0, $scope.glvoucherDetail);
                        $scope.glvoucherDetail = {};

                    });
                }
            }
            else {
                $scope.getPartyPlantListWithCallBack(party.Id, function (result) {
                    $scope.glvoucherDetail.ParticularName = party.Code + ' - ' + party.UserName;
                    $scope.glvoucherDetail.PartyId = party.Id;
                    $scope.glvoucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                    $scope.glvoucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                    $scope.glvoucherDetail.Narration = $scope.voucher.Narration;
                    $scope.glvoucherDetail.EntityId = $scope.voucher.EntityId;
                    $scope.glvoucherDetail.PartyType = $scope.voucher.PartyType;
                    $scope.glvoucherDetail.PartyPlantId = party.PartyPlantId;
                    $scope.glvoucherDetail.PlantId = $scope.voucher.PlantId;
                    $scope.glvoucherDetail.CrAmount = null;
                    $scope.glvoucherDetail.DrAmount = null;
                    $scope.glvoucherDetail.DrDisable = false;
                    $scope.glvoucherDetail.CrDisable = false;
                    $scope.glvoucherDetailList.splice(0, 0, $scope.glvoucherDetail);
                    $scope.voucherDetail = {};

                });
            }
        }
        if ($scope.voucher.PartyType == 'InterTransaction') {
            if (baseService.isUndefinedOrNull($scope.voucher.TransactionTypeName)) {
                ShowResult("Please select Transaction Type!", "failure", "partyPopUp");
                return;
            } else {
                $scope.getPartyPlantListWithCallBack(party.Id, function (result) {
                    $scope.glvoucherDetail.ParticularName = party.Code + ' - ' + party.UserName;
                    $scope.glvoucherDetail.PartyId = party.Id;
                    $scope.glvoucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                    $scope.glvoucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                    $scope.glvoucherDetail.Narration = $scope.voucher.Narration;
                    $scope.glvoucherDetail.EntityId = $scope.voucher.EntityId;
                    $scope.glvoucherDetail.PartyType = $scope.voucher.PartyType;
                    $scope.glvoucherDetail.PartyPlantId = party.PartyPlantId;
                    $scope.glvoucherDetail.PlantId = $scope.voucher.PlantId;
                    $scope.glvoucherDetail.CrAmount = null;
                    $scope.glvoucherDetail.DrAmount = null;
                    $scope.glvoucherDetail.DrDisable = false;
                    $scope.glvoucherDetail.CrDisable = false;
                    $scope.glvoucherDetailList.splice(0, 0, $scope.glvoucherDetail);
                    $scope.glvoucherDetail = {};

                });
            }

        }
        angular.element(document.querySelector('#AddOBPartyJVPopUp')).modal('show');
        $scope.hidePartyPopUp();
    };

    $scope.setOBParty = function (x) {

        var party = x;
        if ($scope.voucher.PartyType == 'Vendor' || $scope.voucher.PartyType == 'Customer') {
            if (baseService.isUndefinedOrNull($scope.voucher.FinancingTypeId)) {
                if (baseService.isUndefinedOrNull(party.GLGeneralInfoId)) {
                    ShowResult($scope.partyType + " GL not found!", "failure", "AddOBPartyJVPopUp");
                    return;
                }
                else {
                    $scope.getPartyPlantListWithCallBack(party.Id, function (result) {
                        if ($scope.voucher.InvoiceAdvance == 'Invoice') {
                            $scope.voucherDetail.GLGeneralInfoId = party.GLGeneralInfoId;
                            $scope.voucherDetail.GLGeneralInfoCode = party.GLGeneralInfoCode;
                            $scope.voucherDetail.GLGeneralInfoName = party.GLGeneralInfoName;
                            $scope.voucherDetail.BudgetMasterId = party.BudgetMasterId;
                            $scope.voucherDetail.BudgetCode = party.BudgetCode;
                            $scope.voucherDetail.BudgetName = party.BudgetName;
                            $scope.voucherDetail.ActivityId = party.ActivityId;
                            $scope.voucherDetail.ActivityCode = party.ActivityCode;
                            $scope.voucherDetail.ActivityName = party.ActivityName;
                        }
                        if ($scope.voucher.InvoiceAdvance == 'Advance') {
                            $scope.voucherDetail.GLGeneralInfoId = party.GLGeneralInfoId;
                            $scope.voucherDetail.GLGeneralInfoCode = party.GLGeneralInfoCode;
                            $scope.voucherDetail.GLGeneralInfoName = party.GLGeneralInfoName;
                            $scope.voucherDetail.BudgetMasterId = party.BudgetMasterId;
                            $scope.voucherDetail.BudgetCode = party.BudgetCode;
                            $scope.voucherDetail.BudgetName = party.BudgetName;
                            $scope.voucherDetail.ActivityId = party.ActivityId;
                            $scope.voucherDetail.ActivityCode = party.ActivityCode;
                            $scope.voucherDetail.ActivityName = party.ActivityName;
                        }

                        $scope.voucherDetail.ParticularName = party.ParticularName;
                        $scope.voucherDetail.PartyId = party.Id;
                        $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                        $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                        $scope.voucherDetail.Narration = $scope.voucher.Narration;
                        $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                        $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
                        $scope.voucherDetail.PartyPlantId = party.PartyPlantId;
                        $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                        $scope.voucherDetail.CrAmount = party.CrAmount;
                        $scope.voucherDetail.DrAmount = party.DrAmount;
                        if ($scope.voucher.InvoiceAdvance == 'Invoice' && $scope.voucher.PartyType == 'Vendor'
                            || $scope.voucher.InvoiceAdvance == 'Advance' && $scope.voucher.PartyType == 'Customer'
                        ) {
                            $scope.voucherDetail.DrDisable = true;
                            $scope.voucherDetail.CrDisable = true;
                        }
                        if ($scope.voucher.InvoiceAdvance == 'Advance' && $scope.voucher.PartyType == 'Vendor'
                            || $scope.voucher.InvoiceAdvance == 'Invoice' && $scope.voucher.PartyType == 'Customer') {
                            $scope.voucherDetail.DrDisable = true;
                            $scope.voucherDetail.CrDisable = true;
                        }

                        $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                        $scope.voucherDetail = {};
                        $scope.voucher.FinancingTypeId = null;
                        $scope.partyType = null;
                        $scope.voucher.InvoiceAdvance = null;
                    });
                }
            }
            else {
                $scope.getPartyPlantListWithCallBack(party.Id, function (result) {
                    $scope.voucherDetail.ParticularName = party.ParticularName;
                    $scope.voucherDetail.PartyId = party.Id;
                    $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                    $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                    $scope.voucherDetail.Narration = $scope.voucher.Narration;
                    $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                    $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
                    $scope.voucherDetail.PartyPlantId = party.PartyPlantId;
                    $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                    $scope.voucherDetail.CrAmount = party.CrAmount;
                    $scope.voucherDetail.DrAmount = party.DrAmount;
                    $scope.voucherDetail.DrDisable = false;
                    $scope.voucherDetail.CrDisable = false;
                    $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                    $scope.voucherDetail = {};
                    $scope.voucher.FinancingTypeId = null;
                    $scope.partyType = null;
                    $scope.voucher.TransactionTypeName = null;
                });
            }
        }
        if ($scope.voucher.PartyType == 'InterTransaction') {
            if (baseService.isUndefinedOrNull($scope.voucher.TransactionTypeName)) {
                ShowResult("Please select Transaction Type!", "failure", "partyPopUp");
                return;
            } else {
                $scope.getPartyPlantListWithCallBack(party.Id, function (result) {
                    $scope.voucherDetail.ParticularName = party.ParticularName;
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
                    $scope.voucherDetail.DrDisable = false;
                    $scope.voucherDetail.CrDisable = false;
                    $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                    $scope.voucherDetail = {};
                    $scope.voucher.FinancingTypeId = null;
                    $scope.partyType = null;
                    $scope.voucher.TransactionTypeName = null;
                });
            }

        }

        angular.element(document.querySelector('#AddOBPartyJVPopUp')).modal('hide');
    };

    $scope.closeOBPartyPopup = function () {
        angular.element(document.querySelector('#AddOBPartyJVPopUp')).modal('hide');
    }
    $scope.SaveParty = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.form0.$valid) {

            $http({
                method: "POST",
                url: "accounts/OpeningBalance/ParkOBGLAdvanceJournal",
                data: {
                    "voucherVM": $scope.voucher,
                    "voucherDetailVMList": $scope.glvoucherDetailList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.setOBParty($scope.glvoucherDetailList[0]);
                    $scope.glvoucherDetailList = [];
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        }
    };
    cboService.getCboCustomerTranTypeList(function (result) {
        $scope.customerTranTypeList = result;
    });

    cboService.getCboVendorTranTypeList(function (result) {
        $scope.vendorTranTypeList = result;
    });

    cboService.getCboAdvPayTranType(function (result) {
        $scope.employeeTransactionTypeList = result;
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
        $scope.voucherDetail.TransactionTypeId = row[0].EmployeeTransactionTypeId;
        $scope.voucherDetail.ActivityName = row[0].ActivityName;
        $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
        $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
        $scope.voucherDetail.Narration = $scope.voucher.Narration;
        $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
        $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
    };

    cboService.getInterCompanyAssetLiabilityType(function (result) {
        $scope.financingTypeList = result;
    });

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
        $scope.voucherDetail.TransactionTypeId = row[0].FinancingTypeId;

    };

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
            $scope.voucher.EmployeeTransactionTypeId = null;
            $scope.voucher.EmployeeTransactionTypeName = null;
        }
        $scope.hideEmployeePopUp();
    };

    $scope.showBankPopUp = function () {
        $scope.getBankList = function (pageno) {
            $scope.url = "Banks/BankMaster/GetBankMasterList?bankACType=HouseBank&&entityId=" + $scope.voucher.EntityId;
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
        angular.element(document.querySelector("#bankPopUp")).modal("show");
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

                OBBankPop(bank);
            }
        }
        $scope.hideBankPopUp();
    };
    function OBBankPop(bank) {
        $scope.glvoucherDetail = {};
        $scope.glvoucherDetail.ParticularName = bank.AccountTitle;
        $scope.glvoucherDetail.BankMasterId = bank.BankMasterId;
        $scope.glvoucherDetail.BudgetMasterId = bank.BudgetMasterId;
        $scope.glvoucherDetail.BudgetCode = bank.BudgetCode;
        $scope.glvoucherDetail.BudgetName = bank.BudgetName;
        $scope.glvoucherDetail.ActivityId = bank.ActivityId;
        $scope.glvoucherDetail.ActivityCode = bank.ActivityCode;
        $scope.glvoucherDetail.ActivityName = bank.ActivityName;
        $scope.glvoucherDetail.GLGeneralInfoId = bank.GLGeneralInfoId;
        $scope.glvoucherDetail.GLGeneralInfoCode = bank.GLGeneralInfoCode;
        $scope.glvoucherDetail.GLGeneralInfoName = bank.GLGeneralInfoName;
        $scope.glvoucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
        $scope.glvoucherDetail.DocRefNo = $scope.voucher.DocRefNo;
        $scope.glvoucherDetail.Narration = $scope.voucher.Narration;
        $scope.glvoucherDetail.EntityId = $scope.voucher.EntityId;
        $scope.glvoucherDetail.PlantId = $scope.voucher.PlantId;
        $scope.glvoucherDetail.CrAmount = null;
        $scope.glvoucherDetail.DrAmount = null;
        $scope.glvoucherDetail.DrDisable = false;
        $scope.glvoucherDetail.CrDisable = true;
        $scope.glvoucherDetail.PartyType = $scope.voucher.PartyType;
        $scope.glvoucherDetailList.splice(0, 0, $scope.glvoucherDetail);
        $scope.glvoucherDetail = {};
        angular.element(document.querySelector("#AddOBBankJVPopUp")).modal("show");
        angular.element(document.querySelector("#bankPopUp")).modal("hide");
    }
    $scope.SaveBank = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.form0.$valid) {

            $http({
                method: "POST",
                url: "accounts/OpeningBalance/ParkOBGLAdvanceJournal",
                data: {
                    "voucherVM": $scope.voucher,
                    "voucherDetailVMList": $scope.glvoucherDetailList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    setBankGL($scope.glvoucherDetailList[0]);
                    $scope.glvoucherDetailList = [];
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        }
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
        $scope.voucherDetail.BankAmount = null;
        $scope.voucherDetail.DrDisable = true;
        $scope.voucherDetail.CrDisable = true;
        $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
        $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
        $scope.voucherDetail = {};
        angular.element(document.querySelector("#AddOBBankJVPopUp")).modal("hide");
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

                OBCashPopUp(cash);
            }
        }
        $scope.hideCashPopUp();
    };

    function OBCashPopUp(cash) {
        $scope.glvoucherDetail = {};
        $scope.glvoucherDetail.CashMasterId = cash.Id;
        $scope.glvoucherDetail.ParticularName = cash.CashName;
        $scope.glvoucherDetail.CashCurrencyId = cash.CurrencyId;
        $scope.glvoucherDetail.GLGeneralInfoId = cash.GLGeneralInfoId;
        $scope.glvoucherDetail.GLGeneralInfoCode = cash.GLGeneralInfoCode;
        $scope.glvoucherDetail.GLGeneralInfoName = cash.GLGeneralInfoName;
        $scope.glvoucherDetail.BudgetMasterId = cash.BudgetMasterId;
        $scope.glvoucherDetail.BudgetCode = cash.BudgetCode;
        $scope.glvoucherDetail.BudgetName = cash.BudgetName;
        $scope.glvoucherDetail.ActivityId = cash.ActivityId;
        $scope.glvoucherDetail.ActivityCode = cash.ActivityCode;
        $scope.glvoucherDetail.ActivityName = cash.ActivityName;
        $scope.glvoucherDetail.CrAmount = null;
        $scope.glvoucherDetail.DrAmount = null;
        $scope.glvoucherDetail.BankAmount = null;
        $scope.glvoucherDetail.DrDisable = false;
        $scope.glvoucherDetail.CrDisable = true;
        $scope.glvoucherDetail.PartyType = $scope.voucher.PartyType;
        $scope.glvoucherDetailList.splice(0, 0, $scope.glvoucherDetail);
        $scope.glvoucherDetail = {};
        angular.element(document.querySelector("#AddOBCashJVPopUp")).modal("show");
        $scope.hideCashPopUp();
    }

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
        $scope.voucherDetail.CrAmount = cash.CrAmount;
        $scope.voucherDetail.DrAmount = cash.DrAmount;
        $scope.voucherDetail.BankAmount = cash.BankAmount;
        $scope.voucherDetail.DrDisable = true;
        $scope.voucherDetail.CrDisable = true;
        $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
        $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
        $scope.voucherDetail = {};
        angular.element(document.querySelector("#AddOBCashJVPopUp")).modal("hide");
    }

    $scope.SaveCash = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.form0.$valid) {

            $http({
                method: "POST",
                url: "accounts/OpeningBalance/ParkOBGLAdvanceJournal",
                data: {
                    "voucherVM": $scope.voucher,
                    "voucherDetailVMList": $scope.glvoucherDetailList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    setCashGL($scope.glvoucherDetailList[0]);
                    $scope.glvoucherDetailList = [];
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        }
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.form0.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "accounts/OpeningBalance/ParkOBAdvanceJournal",
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
                    url: "accounts/OpeningBalance/UpdateOBAdvanceJournal",
                    data: {
                        "voucherVM": $scope.voucher,
                        "voucherDetailVMList": JSON.stringify($scope.voucherDetailList)
                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
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
    $scope.confirmPostNew = function (data,dramount,cramount) {
        $scope.OB = data;
        $scope.DrAmount = dramount;
        $scope.CrAmount = cramount;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.post = function () {
        $http({
            method: "POST",
            url: "accounts/OpeningBalance/PostOBAdvanceJournal",
            data: {
                "voucherVM": $scope.voucher,
                "voucherDetailVMList": JSON.stringify($scope.voucherDetailList)
            },
            dataType: 'JSON'
            , contentType: "application/json charset=utf-8"
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

    $scope.postOB = function (data,dramount,cramount) {
        $http({
            method: "POST",
            url: "accounts/OpeningBalance/PostOpeningBalanceJournal",
            data: {
                "voucherVM": data,
                "DrAmount": dramount,
                "CrAmount": cramount
            },
            dataType: 'JSON'
            , contentType: "application/json charset=utf-8"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                //$scope.getData();
                $scope.clear();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };


    // #region

    //****************Fixed Asser AUC GL******************************
    $scope.searchFAMglByList = [
        //{
        //    "name": "Account Group",
        //    "value": "AccountGroupName"
        //},
        {
            "name": "FixedAsset Master",
            "value": "FixedAssetName"
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
        },
        {
            "name": "Ref No",
            "value": "RefNo"
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

            //if (baseService.isUndefinedOrNull(data.FixedAssetMasterId)) {
            //    ShowResult("FixedAsset is not Mapped With Budget!", "failure", "fixedAssetMasterFAMModal");
            //    return true;
            //}
            //if (baseService.isUndefinedOrNull(data.ActivityId)) {
            //    ShowResult("Activity is not Mapped!", "failure", "fixedAssetMasterFAMModal");
            //    return true;
            //}
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
                $scope.voucherDetail.DrDisable = false;
                $scope.voucherDetail.CrDisable = true;
                $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
                $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                $scope.voucherDetail = {};
                $scope.closefAFAMListPopUp();
            }
        }
        if ($scope.voucher.FAType === 'AccDept') {
            if ($scope.companyConfig.IsVoucherFromBudget)
                var getRow = $filter("filter")($scope.voucherDetailList, { "BudgetMasterId": data.AccumulatedDepreciationBudgetMasterId, "ActivityId": data.AccumulatedDepreciationActivityId });

            //if (baseService.isUndefinedOrNull(data.FixedAssetMasterId)) {
            //    ShowResult("FixedAsset is not Mapped With Budget!", "failure", "fixedAssetMasterAccDepModal");
            //    return true;
            //}
            //if (baseService.isUndefinedOrNull(data.AccumulatedDepreciationActivityId)) {
            //    ShowResult("Activity is not Mapped!", "failure", "fixedAssetMasterAccDepModal");
            //    return true;
            //}
            //if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0 && getRow[0].ActivityId === data.AccumulatedDepreciationActivityId) {
            //    ShowResult("This Activity is already added!", "failure", "fixedAssetMasterAccDepModal");
            //    return true;
            //}
            //else {
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
            $scope.voucherDetail.DrDisable = true;
            $scope.voucherDetail.CrDisable = false;
            $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
            $scope.voucherDetail = {};
            $scope.closefAAccDepListPopUp();
            //}
        }
        if ($scope.voucher.FAType === 'AssetNonCapitalized') {
            if ($scope.companyConfig.IsVoucherFromBudget)
                var getRow = $filter("filter")($scope.voucherDetailList, { "BudgetMasterId": data.AccumulatedDepreciationBudgetMasterId, "ActivityId": data.AccumulatedDepreciationActivityId });

            //if (baseService.isUndefinedOrNull(data.FixedAssetMasterId)) {
            //    ShowResult("FixedAsset is not Mapped With Budget!", "failure", "fixedAssetMasterFAMModal");
            //    return true;
            //}
            //if (baseService.isUndefinedOrNull(data.AccumulatedDepreciationActivityId)) {
            //    ShowResult("Activity is not Mapped!", "failure", "fixedAssetMasterFAMModal");
            //    return true;
            //}
            //if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0 && getRow[0].ActivityId === data.AccumulatedDepreciationActivityId) {
            //    ShowResult("This Activity is already added!", "failure", "fixedAssetMasterFAMModal");
            //    return true;
            //}
            // else {
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
            $scope.voucherDetail.DrDisable = false;
            $scope.voucherDetail.CrDisable = true;
            $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
            $scope.voucherDetail = {};
            $scope.closefAAUCListPopUp();
            //}
        }
    };

    //#endregion


    $scope.searchMMGLByList = [
        {
            "name": "Material Group Master ",
            "value": "MaterialGroupMasterName"
        }
        ,
        {
            "name": "Material  Master ",
            "value": "MaterialMasterName"
        },
        {
            "name": "GL ",
            "value": "GLGeneralInfoName"
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

    $scope.mMGLListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "DocRefNo",
        searchBy: "DocRefNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetmMGLList = function () {
        $scope.GLUrl5 = "Accounts/OpeningBalance/GetMaterialMasterOB";
        $scope.GetMMGLData = function (pageno) {
            baseService.paginationBase($scope.GLUrl5, pageno, $scope.mMGLListParameters)
                .then(function (result) {
                    $scope.mMGLList = result.Rows;
                    $scope.mMGLListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#MaterialMasterOBPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetMMGLData();

    };

    $scope.closeMMGLListPopUp = function () {
        angular.element(document.querySelector("#MaterialMasterOBPopUp")).modal("hide");
    };

    $scope.closeMMGLListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#MaterialMasterOBPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#MaterialMasterOBPopUp")).modal("show");
        }
    };
    $scope.mMDetailList = [];
    $scope.getOBMaterialMasterDetail = function (id) {
        $scope.glvoucherDetail = {};

        $http({
            method: "get",
            url: "accounts/openingbalance/GetMaterialMasterOBGL?openingBalanceId=" + id
        }).then(function successCallback(response) {
            $scope.mMDetailList = response.data;

            for (var i = 0; i < $scope.mMDetailList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.mMDetailList[i].ActivityId)) {
                    ShowResult("This Material has no Article!", "failure", "MaterialMasterOBPopUp");
                    return true;
                }
                if (baseService.isUndefinedOrNull($scope.mMDetailList[i].MaterialMasterId)) {
                    ShowResult("There is no Material", "failure", "MaterialMasterOBPopUp");
                    return true;
                }
                var getRowMM = $filter("filter")($scope.voucherDetailList, {
                    "MaterialMasterId": $scope.mMDetailList[i].MaterialMasterId, "ArticleId": $scope.mMDetailList[i].ArticleId
                    , "BudgetMasterId": $scope.mMDetailList[i].BudgetMasterId, "ActivityId": $scope.mMDetailList[i].ActivityId
                    , "LotNumber": $scope.mMDetailList[i].LotNumber, "Diameter": $scope.mMDetailList[i].Diameter, "Type": $scope.mMDetailList[i].Type
                });
                if (!baseService.isUndefinedOrNull(getRowMM) && getRowMM.length > 0 && getRowMM[0].MaterialMasterId === $scope.mMDetailList[i].MaterialMasterId) {
                    ShowResult("This Material is already added!", "failure", "MaterialMasterOBPopUp");
                }
                else {

                    $scope.glvoucherDetail.BudgetMasterId = $scope.mMDetailList[i].BudgetMasterId;
                    $scope.glvoucherDetail.BudgetCode = $scope.mMDetailList[i].BudgetCode;
                    $scope.glvoucherDetail.BudgetName = $scope.mMDetailList[i].BudgetName;
                    $scope.glvoucherDetail.ActivityId = $scope.mMDetailList[i].ActivityId;
                    $scope.glvoucherDetail.ActivityCode = $scope.mMDetailList[i].ActivityCode;
                    $scope.glvoucherDetail.ActivityName = $scope.mMDetailList[i].ActivityName;
                    $scope.glvoucherDetail.MaterialMasterId = $scope.mMDetailList[i].MaterialMasterId;
                    $scope.glvoucherDetail.ParticularName = $scope.mMDetailList[i].MaterialMasterName;
                    $scope.glvoucherDetail.ArticleId = $scope.mMDetailList[i].ArticleId;
                    $scope.glvoucherDetail.ArticleName = $scope.mMDetailList[i].ArticleName;
                    $scope.glvoucherDetail.LotNumber = $scope.mMDetailList[i].LotNumber;
                    $scope.glvoucherDetail.Diameter = $scope.mMDetailList[i].Diameter;
                    $scope.glvoucherDetail.Type = $scope.mMDetailList[i].Type;
                    $scope.glvoucherDetail.GLGeneralInfoId = $scope.mMDetailList[i].GLGeneralInfoId;
                    $scope.glvoucherDetail.GLGeneralInfoCode = $scope.mMDetailList[i].GLGeneralInfoCode;
                    $scope.glvoucherDetail.GLGeneralInfoName = $scope.mMDetailList[i].GLGeneralInfoName;
                    $scope.glvoucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                    $scope.glvoucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                    $scope.glvoucherDetail.Narration = $scope.voucher.Narration;
                    $scope.glvoucherDetail.EntityId = $scope.voucher.EntityId;
                    $scope.glvoucherDetail.PlantId = $scope.voucher.PlantId;
                    $scope.glvoucherDetail.CrAmount = null;
                    $scope.glvoucherDetail.DrAmount = $scope.mMDetailList[i].CompanyCurrencyAmountDr;
                    $scope.glvoucherDetail.DrDisable = false;
                    $scope.glvoucherDetail.CrDisable = true;
                    $scope.glvoucherDetail.OpeningBalanceId = $scope.mMDetailList[i].OpeningBalanceId;
                    $scope.glvoucherDetail.MaterialMasterOpeningBalanceDetailId = $scope.mMDetailList[i].MaterialMasterOpeningBalanceDetailId;
                    $scope.glvoucherDetail.PartyType = $scope.voucher.PartyType;
                    $scope.glvoucherDetailList.splice(0, 0, $scope.glvoucherDetail);
                    $scope.glvoucherDetail = {};
                }
            }

        });
        angular.element(document.querySelector("#AddOBMaterialJVPopUp")).modal("show");

    };


    $scope.setMMGLListSelected = function (data) {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure", "MaterialMasterOBPopUp");
            return true;
        }

        if (data.voucherNo == null) {
            $scope.getOBMaterialMasterDetail(data.Id);

        }
        $scope.closeMMGLListPopUp();
    };

    $scope.addMMRow = function (data) {

        $scope.voucherDetail.BudgetMasterId = data.BudgetMasterId;
        $scope.voucherDetail.BudgetCode = data.BudgetCode;
        $scope.voucherDetail.BudgetName = data.BudgetName;
        $scope.voucherDetail.ActivityId = data.ActivityId;
        $scope.voucherDetail.ActivityCode = data.ActivityCode;
        $scope.voucherDetail.ActivityName = data.ActivityName;
        $scope.voucherDetail.MaterialMasterId = data.MaterialMasterId;
        $scope.voucherDetail.ParticularName = data.MaterialMasterName;
        $scope.voucherDetail.ArticleId = data.ArticleId;
        $scope.voucherDetail.ArticleName = data.ArticleName;

        $scope.voucherDetail.GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.voucherDetail.GLGeneralInfoCode = data.GLGeneralInfoCode;
        $scope.voucherDetail.GLGeneralInfoName = data.GLGeneralInfoName;

        $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
        $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
        $scope.voucherDetail.Narration = $scope.voucher.Narration;
        $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
        $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
        $scope.voucherDetail.CrAmount = null;
        $scope.voucherDetail.DrAmount = data.DrAmount;
        $scope.voucherDetail.DrDisable = true;
        $scope.voucherDetail.CrDisable = true;
        $scope.voucherDetail.OpeningBalanceId = data.OpeningBalanceId;
        $scope.voucherDetail.MaterialMasterOpeningBalanceDetailId = data.MaterialMasterOpeningBalanceDetailId;
        $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
        $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
        $scope.voucherDetail = {};
    };
    $scope.closeOBMaterialPopup = function () {
        angular.element(document.querySelector("#AddOBMaterialJVPopUp")).modal("hide");
    }
    $scope.SaveMaterial = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.form0.$valid) {

            $http({
                method: "POST",
                url: "accounts/OpeningBalance/ParkOBGLAdvanceJournal",
                data: {
                    "voucherVM": $scope.voucher,
                    "voucherDetailVMList": $scope.glvoucherDetailList
                },
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.addMMRow($scope.glvoucherDetailList[0]);
                    $scope.glvoucherDetailList = [];
                    $scope.closeOBMaterialPopup();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;
        }
    };


    $scope.searchLoanTakenGLByList = [
        {
            "name": "SourceType ",
            "value": "SourceType"
        },
        {
            "name": "Doc Ref No",
            "value": "DocRefNo"
        }
    ];

    $scope.loanTakenGLListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "SourceType",
        searchBy: "SourceType",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.LoanTakenGLList = [];
    $scope.GetLoanTakenGLList = function () {
        $scope.GLUrl6 = "Accounts/OpeningBalance/GetLoanTakenList";
        $scope.GetLoanTakenGLData = function (pageno) {
            baseService.paginationBase($scope.GLUrl6, pageno, $scope.loanTakenGLListParameters)
                .then(function (result) {
                    $scope.LoanTakenGLList = result.Rows;
                    $scope.LoanTakenGLListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#LoanTakenOBPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetLoanTakenGLData();
    };

    $scope.closeLoanTakenGLListPopUp = function () {
        angular.element(document.querySelector("#LoanTakenOBPopUp")).modal("hide");
    };

    $scope.closeLoanTakenGLListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#LoanTakenOBPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#LoanTakenOBPopUp")).modal("show");
        }
    };

    $scope.LoanTakenDetailList = [];
    $scope.getOBLoanTakenDetail = function (id) {
        $http({
            method: "get",
            url: "accounts/openingbalance/GetOBLoanTakenDetail?openingBalanceId=" + id
        }).then(function successCallback(response) {
            $scope.LoanTakenDetailList = response.data;

            for (var i = 0; i < $scope.LoanTakenDetailList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
                    ShowResult("Please select Currency!", "failure", "LoanTakenOBPopUp");
                    return true;
                }
                if (baseService.isUndefinedOrNull($scope.LoanTakenDetailList[i].ActivityId)) {
                    ShowResult("This Material has no Article!", "failure", "LoanTakenOBPopUp");
                    return true;
                }

                var getRowLoanTaken = $filter("filter")($scope.voucherDetailList, { "LoanOpeningBalanceDetailId": $scope.LoanTakenDetailList[i].LoanOpeningBalanceDetailId });

                if (!baseService.isUndefinedOrNull(getRowLoanTaken) && getRowLoanTaken.length > 0) {
                    ShowResult("This loan is already added!", "failure", "LoanTakenOBPopUp");
                }
                else {
                    $scope.voucherDetail.BudgetMasterId = $scope.LoanTakenDetailList[i].BudgetMasterId;
                    $scope.voucherDetail.BudgetCode = $scope.LoanTakenDetailList[i].BudgetCode;
                    $scope.voucherDetail.BudgetName = $scope.LoanTakenDetailList[i].BudgetName;
                    $scope.voucherDetail.ActivityId = $scope.LoanTakenDetailList[i].ActivityId;
                    $scope.voucherDetail.ActivityCode = $scope.LoanTakenDetailList[i].ActivityCode;
                    $scope.voucherDetail.ActivityName = $scope.LoanTakenDetailList[i].ActivityName;

                    $scope.voucherDetail.GLGeneralInfoId = $scope.LoanTakenDetailList[i].GLGeneralInfoId;
                    $scope.voucherDetail.GLGeneralInfoCode = $scope.LoanTakenDetailList[i].GLGeneralInfoCode;
                    $scope.voucherDetail.GLGeneralInfoName = $scope.LoanTakenDetailList[i].GLGeneralInfoName;

                    $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                    $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                    $scope.voucherDetail.Narration = $scope.voucher.Narration;
                    $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                    $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                    $scope.voucherDetail.DrAmount = null;
                    $scope.voucherDetail.CrAmount = $scope.LoanTakenDetailList[i].CompanyCurrencyAmountCr;
                    $scope.voucherDetail.DrDisable = true;
                    $scope.voucherDetail.CrDisable = true;
                    //$scope.voucherDetail.OpeningBalanceId = data.OpeningBalanceId;
                    $scope.voucherDetail.LoanOpeningBalanceDetailId = $scope.LoanTakenDetailList[i].LoanOpeningBalanceDetailId;
                    $scope.voucherDetail.BankMasterId = $scope.LoanTakenDetailList[i].BankMasterId;
                    $scope.voucherDetail.BankCurrencyId = $scope.LoanTakenDetailList[i].BankCurrencyId;
                    $scope.voucherDetail.PartyId = $scope.LoanTakenDetailList[i].PartyId;
                    $scope.voucherDetail.PartyPlantId = $scope.LoanTakenDetailList[i].PartyPlantId;
                    $scope.voucherDetail.ParticularName = $scope.LoanTakenDetailList[i].ParticularName;
                    $scope.voucherDetail.PartyType = $scope.LoanTakenDetailList[i].TransactionType;
                    $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                    $scope.voucherDetail = {};
                }
            }

        });
    };

    $scope.setLoanTakenGLListSelected = function (data) {
        $scope.getOBLoanTakenDetail(data.OpeningBalanceId);
        $scope.closeLoanTakenGLListPopUp();
        //$scope.addLoanTakenRow(data);
    };
    $scope.addLoanTakenRow = function (data) {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure", "LoanTakenOBPopUp");
            return true;
        }
        if (baseService.isUndefinedOrNull(data.ActivityId)) {
            ShowResult("This Material has no Article!", "failure", "LoanTakenOBPopUp");
            return true;
        }

        var getRowLoanTaken = $filter("filter")($scope.voucherDetailList, { "LoanOpeningBalanceDetailId": data.LoanOpeningBalanceDetailId });

        if (!baseService.isUndefinedOrNull(getRowLoanTaken) && getRowLoanTaken.length > 0) {
            ShowResult("This loan is already added!", "failure", "LoanTakenOBPopUp");
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
            $scope.voucherDetail.DrDisable = true;
            $scope.voucherDetail.CrDisable = false;
            $scope.voucherDetail.DrAmount = null;
            $scope.voucherDetail.CrAmount = data.CompanyCurrencyAmountCr;
            $scope.voucherDetail.OpeningBalanceId = data.OpeningBalanceId;
            $scope.voucherDetail.LoanOpeningBalanceDetailId = data.LoanOpeningBalanceDetailId;
            $scope.voucherDetail.BankMasterId = data.BankMasterId;
            $scope.voucherDetail.PartyId = data.PartyId;
            $scope.voucherDetail.PartyPlantId = data.PartyPlantId;
            $scope.voucherDetail.ParticularName = data.ParticularName;
            $scope.voucherDetail.PartyType = data.TransactionType;
            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
            $scope.voucherDetail = {};
            //$scope.closeLoanTakenGLListPopUp();
        }
    };

    //*******************Loan Given ********************
    $scope.searchLoanGivenGLByList = [
        {
            "name": "SourceType ",
            "value": "SourceType"
        },
        {
            "name": "Doc Ref No",
            "value": "DocRefNo"
        }
    ];

    $scope.loanGivenGLListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "SourceType",
        searchBy: "SourceType",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.LoanGivenGLList = [];
    $scope.GetLoanGivenGLList = function () {
        $scope.GLUrl6 = "Accounts/OpeningBalance/GetLoanGivenOBList";
        $scope.GetLoanGivenGLData = function (pageno) {
            baseService.paginationBase($scope.GLUrl6, pageno, $scope.loanGivenGLListParameters)
                .then(function (result) {
                    $scope.LoanGivenGLList = result.Rows;
                    $scope.LoanGivenGLListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#LoanGivenOBPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetLoanGivenGLData();
    };

    $scope.closeLoanGivenGLListPopUp = function () {
        angular.element(document.querySelector("#LoanGivenOBPopUp")).modal("hide");
    };

    $scope.closeLoanGivenGLListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#LoanGivenOBPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#LoanGivenOBPopUp")).modal("show");
        }
    };

    $scope.LoanGivenDetailList = [];
    $scope.getOBLoanGivenDetail = function (id) {
        $http({
            method: "get",
            url: "accounts/openingbalance/GetOBLoanGivenDetail?openingBalanceId=" + id
        }).then(function successCallback(response) {
            $scope.LoanGivenDetailList = response.data;

            for (var i = 0; i < $scope.LoanGivenDetailList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
                    ShowResult("Please select Currency!", "failure", "LoanGivenOBPopUp");
                    return true;
                }
                if (baseService.isUndefinedOrNull($scope.LoanGivenDetailList[i].ActivityId)) {
                    ShowResult("This Material has no Article!", "failure", "LoanGivenOBPopUp");
                    return true;
                }

                var getRowLoanTaken = $filter("filter")($scope.voucherDetailList, { "LoanOpeningBalanceDetailId": $scope.LoanGivenDetailList[i].LoanOpeningBalanceDetailId });

                if (!baseService.isUndefinedOrNull(getRowLoanTaken) && getRowLoanTaken.length > 0) {
                    ShowResult("This loan is already added!", "failure", "LoanGivenOBPopUp");
                }
                else {
                    $scope.voucherDetail.BudgetMasterId = $scope.LoanGivenDetailList[i].BudgetMasterId;
                    $scope.voucherDetail.BudgetCode = $scope.LoanGivenDetailList[i].BudgetCode;
                    $scope.voucherDetail.BudgetName = $scope.LoanGivenDetailList[i].BudgetName;
                    $scope.voucherDetail.ActivityId = $scope.LoanGivenDetailList[i].ActivityId;
                    $scope.voucherDetail.ActivityCode = $scope.LoanGivenDetailList[i].ActivityCode;
                    $scope.voucherDetail.ActivityName = $scope.LoanGivenDetailList[i].ActivityName;

                    $scope.voucherDetail.GLGeneralInfoId = $scope.LoanGivenDetailList[i].GLGeneralInfoId;
                    $scope.voucherDetail.GLGeneralInfoCode = $scope.LoanGivenDetailList[i].GLGeneralInfoCode;
                    $scope.voucherDetail.GLGeneralInfoName = $scope.LoanGivenDetailList[i].GLGeneralInfoName;

                    $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                    $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                    $scope.voucherDetail.Narration = $scope.voucher.Narration;
                    $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                    $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                    $scope.voucherDetail.CrAmount = null;
                    $scope.voucherDetail.DrAmount = $scope.LoanGivenDetailList[i].CompanyCurrencyAmountDr;
                    $scope.voucherDetail.DrDisable = true;
                    $scope.voucherDetail.CrDisable = true;
                    //$scope.voucherDetail.OpeningBalanceId = data.OpeningBalanceId;
                    $scope.voucherDetail.LoanOpeningBalanceDetailId = $scope.LoanGivenDetailList[i].LoanOpeningBalanceDetailId;
                    $scope.voucherDetail.BankMasterId = $scope.LoanGivenDetailList[i].BankMasterId;
                    $scope.voucherDetail.BankCurrencyId = $scope.LoanGivenDetailList[i].BankCurrencyId;
                    $scope.voucherDetail.PartyId = $scope.LoanGivenDetailList[i].PartyId;
                    $scope.voucherDetail.PartyPlantId = $scope.LoanGivenDetailList[i].PartyPlantId;
                    $scope.voucherDetail.ParticularName = $scope.LoanGivenDetailList[i].ParticularName;
                    $scope.voucherDetail.PartyType = $scope.LoanGivenDetailList[i].TransactionType;
                    $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                    $scope.voucherDetail = {};
                }
            }

        });
    };

    $scope.setLoanGivenGLListSelected = function (data) {
        $scope.getOBLoanGivenDetail(data.OpeningBalanceId);
        $scope.closeLoanGivenGLListPopUp();
        //$scope.addLoanTakenRow(data);
    };

    // Security Taken
    $scope.searchSecurityTakenGLByList = [
        {
            "name": "SourceType ",
            "value": "SourceType"
        },
        {
            "name": "Doc Ref No",
            "value": "DocRefNo"
        }
    ];

    $scope.securityTakenGLListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "SourceType",
        searchBy: "SourceType",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.SecurityTakenGLList = [];
    $scope.GetSecurityTakenGLList = function () {
        $scope.securityTakenUrl = "Accounts/OpeningBalance/GetSecurityTakenOBList";
        $scope.GetSecurityTakenGLData = function (pageno) {
            baseService.paginationBase($scope.securityTakenUrl, pageno, $scope.securityTakenGLListParameters)
                .then(function (result) {
                    $scope.SecurityTakenGLList = result.Rows;
                    $scope.securityTakenGLListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#SecurityTakenOBPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetSecurityTakenGLData();
    };

    $scope.closeSecurityTakenGLListPopUp = function () {
        angular.element(document.querySelector("#SecurityTakenOBPopUp")).modal("hide");
    };

    $scope.closeSecurityTakenGLListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#SecurityTakenOBPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#SecurityTakenOBPopUp")).modal("show");
        }
    };

    $scope.SecurityTakenDetailList = [];
    $scope.getOBSecurityTakenDetail = function (id) {
        $http({
            method: "get",
            url: "accounts/openingbalance/GetSecurityTakenOBDetail?openingBalanceId=" + id
        }).then(function successCallback(response) {
            $scope.SecurityTakenDetailList = response.data;

            for (var i = 0; i < $scope.SecurityTakenDetailList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
                    ShowResult("Please select Currency!", "failure", "SecurityTakenOBPopUp");
                    return true;
                }
                if (baseService.isUndefinedOrNull($scope.SecurityTakenDetailList[i].ActivityId)) {
                    ShowResult("This Material has no Article!", "failure", "SecurityTakenOBPopUp");
                    return true;
                }

                var getRowSecurityTaken = $filter("filter")($scope.voucherDetailList, { "SecurityOpeningBalanceDetailId": $scope.SecurityTakenDetailList[i].SecurityOpeningBalanceDetailId });

                if (!baseService.isUndefinedOrNull(getRowSecurityTaken) && getRowSecurityTaken.length > 0) {
                    ShowResult("This loan is already added!", "failure", "SecurityTakenOBPopUp");
                }
                else {
                    $scope.voucherDetail.BudgetMasterId = $scope.SecurityTakenDetailList[i].BudgetMasterId;
                    $scope.voucherDetail.BudgetCode = $scope.SecurityTakenDetailList[i].BudgetCode;
                    $scope.voucherDetail.BudgetName = $scope.SecurityTakenDetailList[i].BudgetName;
                    $scope.voucherDetail.ActivityId = $scope.SecurityTakenDetailList[i].ActivityId;
                    $scope.voucherDetail.ActivityCode = $scope.SecurityTakenDetailList[i].ActivityCode;
                    $scope.voucherDetail.ActivityName = $scope.SecurityTakenDetailList[i].ActivityName;

                    $scope.voucherDetail.GLGeneralInfoId = $scope.SecurityTakenDetailList[i].GLGeneralInfoId;
                    $scope.voucherDetail.GLGeneralInfoCode = $scope.SecurityTakenDetailList[i].GLGeneralInfoCode;
                    $scope.voucherDetail.GLGeneralInfoName = $scope.SecurityTakenDetailList[i].GLGeneralInfoName;

                    $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                    $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                    $scope.voucherDetail.Narration = $scope.voucher.Narration;
                    $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                    $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                    $scope.voucherDetail.DrAmount = null;
                    $scope.voucherDetail.CrAmount = $scope.SecurityTakenDetailList[i].CompanyCurrencyAmountCr;
                    $scope.voucherDetail.DrDisable = true;
                    $scope.voucherDetail.CrDisable = true;
                    //$scope.voucherDetail.OpeningBalanceId = data.OpeningBalanceId;
                    $scope.voucherDetail.SecurityOpeningBalanceDetailId = $scope.SecurityTakenDetailList[i].SecurityOpeningBalanceDetailId;
                    $scope.voucherDetail.BankMasterId = $scope.SecurityTakenDetailList[i].BankMasterId;
                    $scope.voucherDetail.PartyId = $scope.SecurityTakenDetailList[i].PartyId;
                    $scope.voucherDetail.PartyPlantId = $scope.SecurityTakenDetailList[i].PartyPlantId;
                    $scope.voucherDetail.ParticularName = $scope.SecurityTakenDetailList[i].ParticularName;
                    $scope.voucherDetail.PartyType = $scope.SecurityTakenDetailList[i].TransactionType;
                    $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                    $scope.voucherDetail = {};
                }
            }

        });
    };

    $scope.setSecurityTakenGLListSelected = function (data) {
        if (data.VoucherNo != null) {
            ShowResult("This Security is already Posted!", "failure", "SecurityTakenOBPopUp");
        }
        else {
            $scope.getOBSecurityTakenDetail(data.OpeningBalanceId);
            $scope.closeSecurityTakenGLListPopUp();
        }

        //$scope.addLoanTakenRow(data);
    };


    // Security Taken
    $scope.searchSecurityGivenGLByList = [
        {
            "name": "SourceType ",
            "value": "SourceType"
        },
        {
            "name": "Doc Ref No",
            "value": "DocRefNo"
        }
    ];

    $scope.securityGivenGLListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "SourceType",
        searchBy: "SourceType",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.SecurityGivenGLList = [];
    $scope.GetSecurityGivenGLList = function () {
        $scope.securityGivenUrl = "Accounts/OpeningBalance/GetSecurityGivenOBList";
        $scope.GetSecurityGivenGLData = function (pageno) {
            baseService.paginationBase($scope.securityGivenUrl, pageno, $scope.securityGivenGLListParameters)
                .then(function (result) {
                    $scope.SecurityGivenGLList = result.Rows;
                    $scope.securityGivenGLListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#SecurityGivenOBPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetSecurityGivenGLData();
    };

    $scope.closeSecurityGivenGLListPopUp = function () {
        angular.element(document.querySelector("#SecurityGivenOBPopUp")).modal("hide");
    };

    $scope.closeSecurityGivenGLListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#SecurityGivenOBPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#SecurityGivenOBPopUp")).modal("show");
        }
    };

    $scope.SecurityGivenDetailList = [];
    $scope.getOBSecurityGivenDetail = function (id) {
        $http({
            method: "get",
            url: "accounts/openingbalance/GetSecurityGivenOBDetail?openingBalanceId=" + id
        }).then(function successCallback(response) {
            $scope.SecurityGivenDetailList = response.data;

            for (var i = 0; i < $scope.SecurityGivenDetailList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
                    ShowResult("Please select Currency!", "failure", "SecurityGivenOBPopUp");
                    return true;
                }
                if (baseService.isUndefinedOrNull($scope.SecurityGivenDetailList[i].ActivityId)) {
                    ShowResult("This Material has no Article!", "failure", "SecurityGivenOBPopUp");
                    return true;
                }

                var getRowSecurityGiven = $filter("filter")($scope.voucherDetailList, { "SecurityOpeningBalanceDetailId": $scope.SecurityGivenDetailList[i].SecurityOpeningBalanceDetailId });

                if (!baseService.isUndefinedOrNull(getRowSecurityGiven) && getRowSecurityGiven.length > 0) {
                    ShowResult("This loan is already added!", "failure", "SecurityGivenOBPopUp");
                }
                else {
                    $scope.voucherDetail.BudgetMasterId = $scope.SecurityGivenDetailList[i].BudgetMasterId;
                    $scope.voucherDetail.BudgetCode = $scope.SecurityGivenDetailList[i].BudgetCode;
                    $scope.voucherDetail.BudgetName = $scope.SecurityGivenDetailList[i].BudgetName;
                    $scope.voucherDetail.ActivityId = $scope.SecurityGivenDetailList[i].ActivityId;
                    $scope.voucherDetail.ActivityCode = $scope.SecurityGivenDetailList[i].ActivityCode;
                    $scope.voucherDetail.ActivityName = $scope.SecurityGivenDetailList[i].ActivityName;

                    $scope.voucherDetail.GLGeneralInfoId = $scope.SecurityGivenDetailList[i].GLGeneralInfoId;
                    $scope.voucherDetail.GLGeneralInfoCode = $scope.SecurityGivenDetailList[i].GLGeneralInfoCode;
                    $scope.voucherDetail.GLGeneralInfoName = $scope.SecurityGivenDetailList[i].GLGeneralInfoName;

                    $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                    $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                    $scope.voucherDetail.Narration = $scope.voucher.Narration;
                    $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                    $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                    $scope.voucherDetail.CrAmount = null;
                    $scope.voucherDetail.DrAmount = $scope.SecurityGivenDetailList[i].CompanyCurrencyAmountDr;
                    $scope.voucherDetail.DrDisable = true;
                    $scope.voucherDetail.CrDisable = true;
                    //$scope.voucherDetail.OpeningBalanceId = data.OpeningBalanceId;
                    $scope.voucherDetail.SecurityOpeningBalanceDetailId = $scope.SecurityGivenDetailList[i].SecurityOpeningBalanceDetailId;
                    $scope.voucherDetail.BankMasterId = $scope.SecurityGivenDetailList[i].BankMasterId;
                    $scope.voucherDetail.BankCurrencyId = $scope.SecurityGivenDetailList[i].BankCurrencyId;
                    $scope.voucherDetail.BankAmount = $scope.SecurityGivenDetailList[i].BankAmount;
                    $scope.voucherDetail.PartyId = $scope.SecurityGivenDetailList[i].PartyId;
                    $scope.voucherDetail.PartyPlantId = $scope.SecurityGivenDetailList[i].PartyPlantId;
                    $scope.voucherDetail.ParticularName = $scope.SecurityGivenDetailList[i].ParticularName;
                    $scope.voucherDetail.PartyType = $scope.SecurityGivenDetailList[i].TransactionType;
                    $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                    $scope.voucherDetail = {};
                }
            }

        });
    };

    $scope.setSecurityGivenGLListSelected = function (data) {
        if (data.VoucherNo != null) {
            ShowResult("This Security is already Posted!", "failure", "SecurityGivenOBPopUp");
        }
        else {
            $scope.getOBSecurityGivenDetail(data.OpeningBalanceId);
            $scope.closeSecurityGivenGLListPopUp();
        }

    };


    // Equity Taken
    $scope.searchEquityGLByList = [
        {
            "name": "SourceType ",
            "value": "SourceType"
        },
        {
            "name": "Doc Ref No",
            "value": "DocRefNo"
        }
    ];

    $scope.equityGLListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "SourceType",
        searchBy: "SourceType",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.EquityGLList = [];
    $scope.GetEquityGLList = function () {
        $scope.EquityUrl = "Accounts/OpeningBalance/GetEquityOBList";
        $scope.GetEquityGLData = function (pageno) {
            baseService.paginationBase($scope.EquityUrl, pageno, $scope.equityGLListParameters)
                .then(function (result) {
                    $scope.EquityGLList = result.Rows;
                    $scope.equityGLListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#EquityOBPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetEquityGLData();
    };

    $scope.closeEquityGLListPopUp = function () {
        angular.element(document.querySelector("#EquityOBPopUp")).modal("hide");
    };

    $scope.closeEquityGLListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#EquityOBPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#EquityOBPopUp")).modal("show");
        }
    };

    $scope.EquityDetailList = [];
    $scope.getOBEquityDetail = function (id) {
        $http({
            method: "get",
            url: "accounts/openingbalance/GetEquityOBDetail?openingBalanceId=" + id
        }).then(function successCallback(response) {
            $scope.EquityDetailList = response.data;

            for (var i = 0; i < $scope.EquityDetailList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
                    ShowResult("Please select Currency!", "failure", "EquityOBPopUp");
                    return true;
                }
                if (baseService.isUndefinedOrNull($scope.EquityDetailList[i].ActivityId)) {
                    ShowResult("This Material has no Article!", "failure", "EquityOBPopUp");
                    return true;
                }

                var getRowEquity = $filter("filter")($scope.voucherDetailList, { "EquityOpeningBalanceDetailId": $scope.EquityDetailList[i].EquityOpeningBalanceDetailId });

                if (!baseService.isUndefinedOrNull(getRowEquity) && getRowEquity.length > 0) {
                    ShowResult("This loan is already added!", "failure", "EquityOBPopUp");
                }
                else {
                    $scope.voucherDetail.BudgetMasterId = $scope.EquityDetailList[i].BudgetMasterId;
                    $scope.voucherDetail.BudgetCode = $scope.EquityDetailList[i].BudgetCode;
                    $scope.voucherDetail.BudgetName = $scope.EquityDetailList[i].BudgetName;
                    $scope.voucherDetail.ActivityId = $scope.EquityDetailList[i].ActivityId;
                    $scope.voucherDetail.ActivityCode = $scope.EquityDetailList[i].ActivityCode;
                    $scope.voucherDetail.ActivityName = $scope.EquityDetailList[i].ActivityName;

                    $scope.voucherDetail.GLGeneralInfoId = $scope.EquityDetailList[i].GLGeneralInfoId;
                    $scope.voucherDetail.GLGeneralInfoCode = $scope.EquityDetailList[i].GLGeneralInfoCode;
                    $scope.voucherDetail.GLGeneralInfoName = $scope.EquityDetailList[i].GLGeneralInfoName;

                    $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                    $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                    $scope.voucherDetail.Narration = $scope.voucher.Narration;
                    $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                    $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                    $scope.voucherDetail.DrAmount = null;
                    $scope.voucherDetail.CrAmount = $scope.EquityDetailList[i].CompanyCurrencyAmountCr;
                    $scope.voucherDetail.DrDisable = true;
                    $scope.voucherDetail.CrDisable = true;
                    //$scope.voucherDetail.OpeningBalanceId = data.OpeningBalanceId;
                    $scope.voucherDetail.EquityOpeningBalanceDetailId = $scope.EquityDetailList[i].EquityOpeningBalanceDetailId;
                    $scope.voucherDetail.BankMasterId = $scope.EquityDetailList[i].BankMasterId;
                    $scope.voucherDetail.PartyId = $scope.EquityDetailList[i].PartyId;
                    $scope.voucherDetail.PartyPlantId = $scope.EquityDetailList[i].PartyPlantId;
                    $scope.voucherDetail.ParticularName = $scope.EquityDetailList[i].ParticularName;
                    $scope.voucherDetail.PartyType = 'Equity';
                    $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                    $scope.voucherDetail = {};
                }
            }

        });
    };

    $scope.setEquityGLListSelected = function (data) {
        if (data.VoucherNo != null) {
            ShowResult("This Security is already Posted!", "failure", "EquityOBPopUp");
        }
        else {
            $scope.getOBEquityDetail(data.OpeningBalanceId);
            $scope.closeEquityGLListPopUp();
        }

    };


    // Investment Given
    $scope.searchInvestmentGLByList = [
        {
            "name": "SourceType ",
            "value": "SourceType"
        },
        {
            "name": "Doc Ref No",
            "value": "DocRefNo"
        }
    ];

    $scope.investmentGLListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "SourceType",
        searchBy: "SourceType",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.InvestmentGLList = [];
    $scope.GetInvestmentGLList = function () {
        $scope.InvestmentUrl = "Accounts/OpeningBalance/GetInvestmentOBList";
        $scope.GetInvestmentGLData = function (pageno) {
            baseService.paginationBase($scope.InvestmentUrl, pageno, $scope.investmentGLListParameters)
                .then(function (result) {
                    $scope.InvestmentGLList = result.Rows;
                    $scope.investmentGLListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#InvestmentOBPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetInvestmentGLData();
    };

    $scope.closeInvestmentGLListPopUp = function () {
        angular.element(document.querySelector("#InvestmentOBPopUp")).modal("hide");
    };

    $scope.closeInvestmentGLListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#InvestmentOBPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#InvestmentOBPopUp")).modal("show");
        }
    };

    $scope.InvestmentDetailList = [];
    $scope.getOBInvestmentDetail = function (id) {
        $http({
            method: "get",
            url: "accounts/openingbalance/GetInvestmentOBDetail?openingBalanceId=" + id
        }).then(function successCallback(response) {
            $scope.InvestmentDetailList = response.data;

            for (var i = 0; i < $scope.InvestmentDetailList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
                    ShowResult("Please select Currency!", "failure", "InvestmentOBPopUp");
                    return true;
                }
                if (baseService.isUndefinedOrNull($scope.InvestmentDetailList[i].ActivityId)) {
                    ShowResult("This Material has no Article!", "failure", "InvestmentOBPopUp");
                    return true;
                }

                var getRowEquity = $filter("filter")($scope.voucherDetailList, { "InvestmentOpeningBalanceDetailId": $scope.InvestmentDetailList[i].InvestmentOpeningBalanceDetailId });

                if (!baseService.isUndefinedOrNull(getRowEquity) && getRowEquity.length > 0) {
                    ShowResult("This Investment is already added!", "failure", "InvestmentOBPopUp");
                }
                else {
                    $scope.voucherDetail.BudgetMasterId = $scope.InvestmentDetailList[i].BudgetMasterId;
                    $scope.voucherDetail.BudgetCode = $scope.InvestmentDetailList[i].BudgetCode;
                    $scope.voucherDetail.BudgetName = $scope.InvestmentDetailList[i].BudgetName;
                    $scope.voucherDetail.ActivityId = $scope.InvestmentDetailList[i].ActivityId;
                    $scope.voucherDetail.ActivityCode = $scope.InvestmentDetailList[i].ActivityCode;
                    $scope.voucherDetail.ActivityName = $scope.InvestmentDetailList[i].ActivityName;

                    $scope.voucherDetail.GLGeneralInfoId = $scope.InvestmentDetailList[i].GLGeneralInfoId;
                    $scope.voucherDetail.GLGeneralInfoCode = $scope.InvestmentDetailList[i].GLGeneralInfoCode;
                    $scope.voucherDetail.GLGeneralInfoName = $scope.InvestmentDetailList[i].GLGeneralInfoName;

                    $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                    $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                    $scope.voucherDetail.Narration = $scope.voucher.Narration;
                    $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                    $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                    $scope.voucherDetail.CrAmount = null;
                    $scope.voucherDetail.DrAmount = $scope.InvestmentDetailList[i].CompanyCurrencyAmountDr;
                    $scope.voucherDetail.DrDisable = true;
                    $scope.voucherDetail.CrDisable = true;
                    //$scope.voucherDetail.OpeningBalanceId = data.OpeningBalanceId;
                    $scope.voucherDetail.InvestmentOpeningBalanceDetailId = $scope.InvestmentDetailList[i].InvestmentOpeningBalanceDetailId;
                    $scope.voucherDetail.BankMasterId = $scope.InvestmentDetailList[i].BankMasterId;
                    $scope.voucherDetail.PartyId = $scope.InvestmentDetailList[i].PartyId;
                    $scope.voucherDetail.PartyPlantId = $scope.InvestmentDetailList[i].PartyPlantId;
                    $scope.voucherDetail.ParticularName = $scope.InvestmentDetailList[i].ParticularName;
                    $scope.voucherDetail.PartyType = $scope.InvestmentDetailList[i].TransactionType;
                    $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                    $scope.voucherDetail = {};
                }
            }

        });
    };

    $scope.setInvestmentGLListSelected = function (data) {
        if (data.VoucherNo != null) {
            ShowResult("This Investment is already Posted!", "failure", "InvestmentOBPopUp");
        }
        else {
            $scope.getOBInvestmentDetail(data.OpeningBalanceId);
            $scope.closeInvestmentGLListPopUp();
        }

    };

    $scope.deleteRow = {};
    $scope.confirmRowDelete = function (index, data) {
        $scope.deleteRowIndex = index;
        $scope.deleteRow = {};
        $scope.deleteRow = data;
        if (baseService.isUndefinedOrNull($scope.deleteRow.Id)) {
            $scope.voucherDetailList.splice(index, 1);
        }
        else {
            $scope.message_delete_confirmation = "Are you sure to Delete?";
            angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
        }

    };

    $scope.DeleteDetailRow = function (detaildata, deleteRowIndex) {
        $http({
            method: "POST",
            url: "Accounts/OpeningBalance/DeleteOBDetailRow",
            data: {
                "OBDetailVM": detaildata,
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.voucherDetailList.splice(deleteRowIndex, 1);
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.searchOBDetailList = [];
    $scope.searchOBDetailList = [
        {
            "name": "GLGeneralInfoCode",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GLGeneralInfoName",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Budget Ref No",
            "value": "RefNo"
        },
        {
            "name": "BudgetName",
            "value": "BudgetName"
        },
        {
            "name": "ActivityName",
            "value": "ActivityName"
        }
        ,
        {
            "name": "ParticularName",
            "value": "ParticularName"
        }
        ,
        {
            "name": "Party Code",
            "value": "PartyCode"
        },
        {
            "name": "DrAmount",
            "value": "DrAmount"
        },
        {
            "name": "CrAmount",
            "value": "CrAmount"
        }
    ];

    $scope.OBDetailListParameters = {
        limit: 10,
        offset: 0,
        order: "DESC",
        sort: "GLGeneralInfoCode",
        searchBy: "GLGeneralInfoCode",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetOBDetailList = function () {
        $scope.obDetailUrl = "Accounts/OpeningBalance/GetOBAdvanceJournalDetail?openingBalanceId=" + $scope.voucher.Id;
        $scope.GetOBDetailData = function (pageno) {
            baseService.paginationBase($scope.obDetailUrl, pageno, $scope.OBDetailListParameters)
                .then(function (result) {
                    $scope.voucherDetailList = result.Rows;
                    $scope.OBDetailListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        $scope.GetOBDetailData();
    };
}