"use strict";
cashOpeningBalanceController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function cashOpeningBalanceController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Cash Opening Balance";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.openingBalanceList = [];
    $scope.openingBalanceDetailList = [];
    $scope.narration = null;
    $scope.isEntityLevel = false;
    $scope.url = "Banks/CashOpeningBalance";
    $scope.listUrl = $scope.url + "/GetCashList";
    $scope.saveUrl = $scope.url + "/InsertCash";
    $scope.updateUrl = $scope.url + "/UpdateCash";

    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });
    $scope.isAdvance = null;

    $scope.openingBalance = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null,
        EntityId: null,
        FinancingTypeId: null,
        EmployeeTransactionTypeId: null,
        PostingDate: null,
        DocRefNo: null,
        DocDate: null,
        Narration: null,
        Remarks: null,
        IsPark: false,
        Active: true,
        Archive: false,
        BudgetMasterId: null,
        ActivityId: null,
        PartyId: null,
        PartyType: null
    };

    $scope.openingBalanceDetail = {
        Id: null,
        OpeningBalanceId: null,
        EntityId: null,
        PlantId: null,
        GLGeneralInfoId: null,
        GLGeneralInfoName: null,
        BudgetMasterId: null,
        BudgetName: null,
        ActivityId: null,
        ActivityName: null,
        CurrencyId: null,
        PartyId: null,
        PartyName: null,
        PartyType: null,
        BankMasterId: null,
        CashMasterId: null,
        BankName: null,
        CashName: null,
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
        HardCurrencyAmount: 0,
        RepaymentStartDate: $filter("dateFiltering")(Date.now()),
        LifeOfYear: 0,
        NoOfInstallmentPerYear: 0,
        NoOfPaidInstallment: 0,
        TotalNoOfInstallment: 0,
        ProfitRate: 0,
        SanctionAmount: 0,
        Active: true
    };

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
        console.log("companyConfig", $scope.companyConfig);
        $scope.getCutOffDate();
    });

    $scope.getCutOffDate = function () {
        $http.get("accounts/OpeningBalance/GetACCCutOffDate")
            .then(function (response) {
                if (response.data !== null && !baseService.isUndefinedOrNull(response.data.CutOffDate)) {
                    $scope.openingBalance.PostingDate = $filter("dateFiltering")(response.data.CutOffDate);
                    $scope.isEntityLevel = response.data.IsEntityLevel;
                    if ($scope.isEntityLevel) {
                        cboService.getCboEntityByPlant(null, null, "", function (result) {
                            $scope.entityList = result;
                        });
                    }
                }
                else {
                    ShowResult("Opening Balance Cut Off date not found!", "failure");
                }
            });
    };

    $scope.removeRow = function (index) {
        $scope.openingBalanceDetailList.splice(index, 1);
        CloseShowResult();
    };

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

    $scope.invalidRow = false;
    $scope.checkRowValidation = function (data, index) {
        $scope.getPlantInfo(data.PlantId, data);
        if ($scope.checkDocDate("td_DocDate_" + index, data.DocDate)) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_DocRef_" + index, baseService.isUndefinedOrNull(data.DocRefNo), "Doc Ref is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_Narration_" + index, baseService.isUndefinedOrNull(data.Narration), "Narration is required.")) {
            $scope.invalidRow = true;
        }
        else if ($scope.checkDueDateBaseOn("td_BaseOnDueDate_" + index, data.BaseOnDueDate)) {
            $scope.invalidRow = true;
        }
        else if (!baseService.isUndefinedOrNull(data.isAdvance) && !$scope.isAdvance && manualValidation("td_BaseNoOfDays_" + index, baseService.isUndefinedOrNaN(data.BaseNoOfDays), "BaseNoOfDays is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_CurrencyId_" + index, baseService.isUndefinedOrNull(data.CurrencyId), "Currency is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_Amount_" + index, baseService.isUndefinedOrNaN(data.Amount), "Amount is required and must greater than 0.")) {
            $scope.invalidRow = true;
        }
        else if (!baseService.isUndefinedOrNull($scope.companyCurrencyId) && data.CurrencyId === $scope.companyCurrencyId && data.Amount !== data.CompanyCurrencyAmount) {
            manualValidation("td_CompanyCurrencyAmount_" + index, true, "Trn. Amount and " + $scope.companyCurrencyName + " have to same!");
            $scope.invalidRow = true;
        }
        else if (!baseService.isUndefinedOrNull($scope.companyCurrencyId) && manualValidation("td_CompanyCurrencyAmount_" + index, baseService.isUndefinedOrNaN(data.CompanyCurrencyAmount), "Amount is required and must greater than 0.")) {
            $scope.invalidRow = true;
        }
        else if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId) && data.CurrencyId === $scope.companyGroupCurrencyId && data.Amount !== data.CompanyGroupCurrencyAmount) {
            manualValidation("td_CompanyGroupCurrencyAmount_" + index, true, "Trn. Amount and " + $scope.companyGroupCurrencyName + " have to same!");
            $scope.invalidRow = true;
        }
        else if (!baseService.isUndefinedOrNull($scope.companyGroupCurrencyId) && manualValidation("td_CompanyGroupCurrencyAmount_" + index, baseService.isUndefinedOrNaN(data.CompanyGroupCurrencyAmount), "Amount is required and must greater than 0.")) {
            $scope.invalidRow = true;
        }
        else if (!baseService.isUndefinedOrNull($scope.hardCurrencyId) && data.CurrencyId === $scope.hardCurrencyId && data.Amount !== data.HardCurrencyAmount) {
            manualValidation("td_HardCurrencyAmount_" + index, true, "Trn. Amount and " + $scope.hardCurrencyName + " have to same!");
            $scope.invalidRow = true;
        }
        else if (!baseService.isUndefinedOrNull($scope.hardCurrencyId) && manualValidation("td_HardCurrencyAmount_" + index, baseService.isUndefinedOrNaN(data.HardCurrencyAmount), "Amount is required and must greater than 0.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_PlantId_" + index, baseService.isUndefinedOrNull(data.PartyId), "This Company is not created as InterCompany Party Plant.")) {
            $scope.plant = null;
            $scope.invalidRow = true;
        }
        else
            $scope.invalidRow = false;
    };

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function (controlId, val) {
        var msg = "";
        if (new Date(val) > new Date($scope.openingBalance.PostingDate)) {
            $scope.invalidDocDate = true;
            msg = "Doc date must be below or equal to Posting Date.";
        }
        else if (baseService.isUndefinedOrNull($scope.openingBalance.DocDate)) {
            $scope.invalidDocDate = true;
            msg = "Doc date is required.";
        }
        else $scope.invalidDocDate = false;
        return manualValidation(controlId, $scope.invalidDocDate, msg);
    };

    $scope.invalidDueDateBaseOn = false;
    $scope.checkDueDateBaseOn = function (controlId, val) {
        var msg = "";
        if (new Date(val) >= new Date($scope.openingBalance.PostingDate)) {
            $scope.invalidDueDateBaseOn = true;
            msg = "Due Date Base On must be below  Posting Date.";
        }
        else if (baseService.isUndefinedOrNull($scope.openingBalance.DocDate)) {
            $scope.invalidDueDateBaseOn = true;
            msg = "Due Date Base On is required.";
        }
        else $scope.invalidDueDateBaseOn = false;
        return manualValidation(controlId, $scope.invalidDueDateBaseOn, msg);
    };

    $scope.invalidEntity = false;
    $scope.entityValidation = function () {
        $scope.invalidEntity = baseService.isUndefinedOrNull($scope.openingBalance.EntityId);
        return manualValidation("div_entity", $scope.invalidEntity, "Entity is required.");
    };

    $scope.getById = function (index) {
        $scope.index = index;
        $scope.openingBalance = Object.assign({}, $scope.openingBalanceList[$scope.index]);
        $scope.openingBalance.PostingDate = $filter("dateFiltering")($scope.openingBalance.PostingDate);
        $scope.openingBalance.DocDate = $filter("dateFiltering")($scope.openingBalance.DocDate);
        if (baseService.isUndefinedOrNull($scope.sort)) {
            $scope.sort = "";
        }
        $http.get("accounts/OpeningBalance/GetOpeningBalanceDetailList?openingBalanceId=" + $scope.openingBalance.Id + "&sort=" + $scope.sort)
            .then(function (response) {
                $scope.openingBalanceDetailList = response.data;
                angular.forEach($scope.openingBalanceDetailList, function (item, i) {
                    item.DocDate = $filter("dateFiltering")(item.DocDate);
                    item.BaseOnDueDate = $filter("dateFiltering")(item.BaseOnDueDate);

                    item.CompanyGroupCurrencyId = $scope.companyGroupCurrencyId;
                    item.CompanyGroupFromCurrencyId = $scope.companyGroupCurrencyId;
                    item.CompanyGroupCurrencyName = $scope.companyGroupCurrencyName;

                    item.HardCurrencyId = $scope.hardCurrencyId;
                    item.HardFromCurrencyId = $scope.hardCurrencyId;
                    item.HardCurrencyName = $scope.hardCurrencyName;

                    if (!baseService.isUndefinedOrNull(item.CompanyId)) {
                        $scope.companyChange(item.CompanyId);
                        if (!baseService.isUndefinedOrNull(item.PlantId)) {
                            $scope.plantChange(item.PlantId, item.CompanyId);
                        }
                    }
                });
            });
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
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
            $scope.plant = $.grep($scope.interplantList, function (item) {
                return item.PlantId === plantId;
            })[0];
            if ($scope.plant !== undefined &&
                $scope.plant !== null) {
                if (!baseService.isUndefinedOrNull($scope.plant.PartyId)) {
                    row.PartyId = $scope.plant.PartyId;
                    row.PartyPlantId = $scope.plant.PartyPlantId;
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

    baseService.init($scope.listUrl, null, null, "ASC", "EntityName", "EntityName");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.openingBalanceList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();

    $rootScope.searchByList = [
        {
            "name": "Entity",
            "value": "EntityName"
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
        }
    ];

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        $scope.checkDocDate("div_DocDate", $scope.openingBalance.DocDate);
        if ($scope.isEntityLevel) {
            $scope.entityValidation();
        }
        angular.forEach($scope.openingBalanceDetailList, function (item, i) {
            if ($scope.invalidRow) {
                return;
            }
            $scope.checkRowValidation(item, i);
        });
        if ($scope.form1.$valid & !$scope.invalidDocDate && !$scope.invalidEntity && !$scope.invalidRow) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: {
                        "openingBalance": $scope.openingBalance,
                        "openingBalanceDetailVMList": $scope.openingBalanceDetailList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        $scope.clearFields();
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
                        "openingBalance": $scope.openingBalance,
                        "openingBalanceDetailVMList": $scope.openingBalanceDetailList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        $scope.clearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
            }
            return true;
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.openingBalance.Id)) {
            var url = "";
            if ($scope.partyType === "Plant" || $scope.partyType === "Company") {
                url = "accounts/openingbalance/DeleteInter?id=" + $scope.openingBalance.Id;
            }
            else {
                url = "accounts/openingbalance/delete?id=" + $scope.openingBalance.Id;
            }
            $http({
                method: "POST",
                url: url,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.openingBalanceList.splice($scope.index, 1);
                    $scope.clearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
    };

    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            $scope.getPartyPlantListWithCallBack(party.Id, function (result) {
                if ($scope.partyGLType !== "DownPayment" && !baseService.isUndefinedOrNull($scope.partyGLType)) {
                    if (baseService.isUndefinedOrNull(party.ReconciliationGLId)) {
                        ShowResult($scope.partyType + " GL not found!", "failure", "partyPopUp");
                        return;
                    }
                    else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.ReconciliationBudgetId)) {
                        ShowResult($scope.partyType + " Budget not found!", "failure", "partyPopUp");
                        return;
                    }
                    else {
                        $scope.openingBalanceDetail.GLGeneralInfoId = party.ReconciliationGLId;
                        $scope.openingBalanceDetail.GLGeneralInfoName = party.ReconciliationGLCode + " - " + party.ReconciliationGLName;
                        $scope.openingBalanceDetail.BudgetMasterId = party.ReconciliationBudgetId;
                        $scope.openingBalanceDetail.BudgetName = party.ReconciliationBudgetCode + " - " + party.ReconciliationBudgetName;
                        $scope.openingBalanceDetail.ActivityId = party.ReconciliationActivityId;
                        $scope.openingBalanceDetail.ActivityName = party.ReconciliationActivityCode + " - " + party.ReconciliationActivityName;
                    }
                }
                else if ($scope.partyGLType === "DownPayment") {
                    if (baseService.isUndefinedOrNull(party.DownPaymentGLId)) {
                        ShowResult($scope.partyType + " GL not found!", "failure", "partyPopUp");
                        return;
                    }
                    else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.DownPaymentBudgetId)) {
                        ShowResult($scope.partyType + " Budget not found!", "failure", "partyPopUp");
                        return;
                    }
                    else {
                        $scope.openingBalanceDetail.GLGeneralInfoId = party.DownPaymentGLId;
                        $scope.openingBalanceDetail.GLGeneralInfoName = party.DownPaymentGLCode + " - " + party.DownPaymentGLName;
                        $scope.openingBalanceDetail.BudgetMasterId = party.DownPaymentBudgetId;
                        $scope.openingBalanceDetail.BudgetName = party.DownPaymentBudgetCode + " - " + party.DownPaymentBudgetName;
                        $scope.openingBalanceDetail.ActivityId = party.DownPaymentActivityId;
                        $scope.openingBalanceDetail.ActivityName = party.DownPaymentActivityCode + " - " + party.DownPaymentActivityName;
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
        }
        $scope.hidePartyPopUp();
    };

    $scope.closeCustomerPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            if ($scope.isAdvance !== null && !$scope.isAdvance) {
                if (baseService.isUndefinedOrNull(party.ReconciliationGLId)) {
                    ShowResult($scope.partyType + " GL not found!", "failure", "partyPopUp");
                    return;
                }
                else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.ReconciliationBudgetId)) {
                    ShowResult($scope.partyType + " Budget not found!", "failure", "partyPopUp");
                    return;
                }
                else {
                    $scope.openingBalanceDetail.GLGeneralInfoId = party.ReconciliationGLId;
                    $scope.openingBalanceDetail.GLGeneralInfoName = party.ReconciliationGLCode + " - " + party.ReconciliationGLName;
                    $scope.openingBalanceDetail.BudgetMasterId = party.ReconciliationBudgetId;
                    $scope.openingBalanceDetail.BudgetName = party.ReconciliationBudgetCode + " - " + party.ReconciliationBudgetName;
                    $scope.openingBalanceDetail.ActivityId = party.ReconciliationActivityId;
                    $scope.openingBalanceDetail.ActivityName = party.ReconciliationActivityCode + " - " + party.ReconciliationActivityName;
                }
            }
            else if ($scope.isAdvance !== null && $scope.isAdvance) {
                if (baseService.isUndefinedOrNull(party.DownPaymentGLId)) {
                    ShowResult($scope.partyType + " GL not found!", "failure", "partyPopUp");
                    return;
                }
                else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(party.DownPaymentBudgetId)) {
                    ShowResult($scope.partyType + " Budget not found!", "failure", "partyPopUp");
                    return;
                }
                else {
                    $scope.openingBalanceDetail.GLGeneralInfoId = party.DownPaymentGLId;
                    $scope.openingBalanceDetail.GLGeneralInfoName = party.DownPaymentGLCode + " - " + party.DownPaymentGLName;
                    $scope.openingBalanceDetail.BudgetMasterId = party.DownPaymentBudgetId;
                    $scope.openingBalanceDetail.BudgetName = party.DownPaymentBudgetCode + " - " + party.DownPaymentBudgetName;
                    $scope.openingBalanceDetail.ActivityId = party.DownPaymentActivityId;
                    $scope.openingBalanceDetail.ActivityName = party.DownPaymentActivityCode + " - " + party.DownPaymentActivityName;
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

            $scope.openingBalanceDetailList.splice(0, 0, $scope.openingBalanceDetail);
            $scope.clearOpeningBalanceDetail();
        }
        $scope.hidePartyPopUp();
    };

    $scope.closeCashPopUp = function () {
        if ($scope.cashIndex !== -1) {
            var cash = $scope.cashList[$scope.cashIndex];
            if (baseService.isUndefinedOrNull(cash.GLGeneralInfoId)) {
                ShowResult("Cash GL not found!", "failure", "bankPopUp");
                return;
            }
            else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(cash.BudgetMasterId)) {
                ShowResult("Cash Budget not found!", "failure", "bankPopUp");
                return;
            }
            else if (baseService.isUndefinedOrNull(cash.CurrencyId)) {
                ShowResult("Cash Transaction Currency not found!", "failure", "bankPopUp");
                return;
            }
            else {
                $scope.openingBalanceDetail.CashName = cash.CashName;
                $scope.openingBalanceDetail.CashMasterId = cash.Id;
                $scope.openingBalanceDetail.CurrencyId = cash.CurrencyId;
                $scope.openingBalanceDetail.CurrencyCode = cash.CurrencyCode;

                $scope.openingBalanceDetail.DocDate = $scope.openingBalance.DocDate;
                $scope.openingBalanceDetail.DocRefNo = $scope.openingBalance.DocRefNo;
                $scope.openingBalanceDetail.Narration = $scope.openingBalance.Narration;

                $scope.openingBalanceDetail.EntityId = $scope.openingBalance.EntityId;
                $scope.openingBalanceDetail.PlantId = $scope.openingBalance.PlantId;

                $scope.openingBalanceDetail.GLGeneralInfoId = cash.GLGeneralInfoId;
                $scope.openingBalanceDetail.GLGeneralInfoName = cash.GLGeneralInfoName;
                $scope.openingBalanceDetail.BudgetMasterId = cash.BudgetMasterId;
                $scope.openingBalanceDetail.BudgetName = cash.BudgetCode + " - " + cash.BudgetName;
                $scope.openingBalanceDetail.ActivityId = cash.ActivityId;
                $scope.openingBalanceDetail.ActivityName = cash.ActivityCode + " - " + cash.ActivityName;

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

                $scope.openingBalanceDetailList.splice(0, 0, $scope.openingBalanceDetail);
                $scope.clearOpeningBalanceDetail();
            }
        }
        $scope.hideCashPopUp();
    };

    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];
            if ($scope.bankACType === "HouseBank") {
                if (baseService.isUndefinedOrNull(bank.GLGeneralInfoId)) {
                    ShowResult("Bank GL not found!", "failure", "bankPopUp");
                    return;
                }
                else if ($scope.companyConfig.IsVoucherFromBudget && baseService.isUndefinedOrNull(bank.BudgetMasterId)) {
                    ShowResult("Bank Budget not found!", "failure", "bankPopUp");
                    return;
                }
                else if (bank.CurrencyId === null) {
                    ShowResult("Bank Transaction Currency not found!", "failure", "bankPopUp");
                    return;
                }
                else {
                    addBankRow(bank);
                }
            }
            else {
                addBankRow(bank);
            }
        }
        $scope.hideBankPopUp();
    };

    function addBankRow(bank) {
        $scope.openingBalanceDetail.AccountTitle = bank.AccountTitle;
        $scope.openingBalanceDetail.BankName = bank.BankName;
        $scope.openingBalanceDetail.BankMasterId = bank.BankMasterId;
        $scope.openingBalanceDetail.CurrencyId = bank.CurrencyId;
        $scope.openingBalanceDetail.CurrencyCode = bank.CurrencyCode;

        $scope.openingBalanceDetail.DocDate = $scope.openingBalance.DocDate;
        $scope.openingBalanceDetail.DocRefNo = $scope.openingBalance.DocRefNo;
        $scope.openingBalanceDetail.Narration = $scope.openingBalance.Narration;

        $scope.openingBalanceDetail.EntityId = $scope.openingBalance.EntityId;
        $scope.openingBalanceDetail.PlantId = $scope.openingBalance.PlantId;

        $scope.openingBalanceDetail.GLGeneralInfoId = bank.GLGeneralInfoId;
        $scope.openingBalanceDetail.GLGeneralInfoName = bank.GLGeneralInfoName;
        $scope.openingBalanceDetail.BudgetMasterId = bank.BudgetMasterId;
        $scope.openingBalanceDetail.BudgetName = bank.BudgetCode + " - " + bank.BudgetName;
        $scope.openingBalanceDetail.ActivityId = bank.ActivityId;
        $scope.openingBalanceDetail.ActivityName = bank.ActivityCode + " - " + bank.ActivityName;

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

        $scope.openingBalanceDetail.LifeOfYear = 0;
        $scope.openingBalanceDetail.NoOfInstallmentPerYear = 0;
        $scope.openingBalanceDetail.NoOfPaidInstallment = 0;
        $scope.openingBalanceDetail.TotalNoOfInstallment = 0;
        $scope.openingBalanceDetail.ProfitRate = 0;
        $scope.openingBalanceDetail.SanctionAmount = 0;

        $scope.openingBalanceDetailList.splice(0, 0, $scope.openingBalanceDetail);
        $scope.clearOpeningBalanceDetail();
    }

    $scope.clearFields = function () {
        $scope.plantList = [];
        $scope.Action = "Save";
        $scope.openingBalance.Id = null;
        $scope.openingBalance.DocDate = null;
        $scope.openingBalance.DocRefNo = null;
        $scope.openingBalance.Narration = null;
        $scope.openingBalance.EntityId = null;
        $scope.openingBalance.SecurityTypeGivenId = null;
        $scope.openingBalance.SecurityTypeTakenId = null;
        $scope.openingBalance.InvestmentTypeGivenId = null;
        $scope.openingBalance.EmployeeTransactionTypeId = null;
        $scope.openingBalanceDetailList = [];
        $scope.clearOpeningBalanceDetail();
        $scope.advanceCA = null;
        $scope.openingBalance.FinancingTypeId = null;
        $scope.openingBalance.SourceTo = "Party";
        $scope.openingBalance.PartyType = "Party";
        if ($scope.partyType === "Plant" || $scope.partyType === "Company") {
            $scope.interClearFields();
        }
    };

    $scope.interClearFields = function () {
        $scope.openingBalance.PartyType = "Plant";
        $scope.addDefaultDetailRow();
        $scope.changeSourceTo($scope.openingBalance.PartyType);
    };

    $scope.addDefaultDetailRow = function () {
        if ($scope.openingBalanceDetailList.length < 1) {
            $scope.openingBalanceDetailList[0] = {
                OpeningBalanceId: null,
                BankMasterId: null,
                PartyId: null,
                PartyType: null,
                GLGeneralInfoId: null,
                CompanyId: null,
                PlantId: null,
                EntityId: null,
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
        }
    };

    $scope.clearOpeningBalanceDetail = function () {
        $scope.openingBalanceDetail = {};
        $scope.openingBalanceDetail.Active = true;
        $scope.openingBalanceDetail.Amount = 0;
        $scope.openingBalanceDetail.CompanyCurrencyAmount = 0;
        $scope.openingBalanceDetail.CompanyGroupCurrencyAmount = 0;
        $scope.openingBalanceDetail.HardCurrencyAmount = 0;
    };

    $scope.previousSelected = "Party";
    $scope.changeSourceTo = function (to) {
        checkPartySourceChange(to);
    };

    function checkPartySourceChange(to) {
        if ($scope.openingBalanceDetailList.length > 0) {
            $scope.message_confirmation = "For changing source all data will be reset.";
            angular.element(document.querySelector("#confirmgenericPopUp")).modal("show");
        }
        else {
            $scope.openingBalance.PartyType = to;
            $scope.previousSelected = to;
            $scope.partyType = to;
        }
    }

    $scope.clearDetail = function (to) {
        $scope.openingBalanceDetailList = [];
        $scope.openingBalance.PartyType = to;
        $scope.partyType = to;
    };

    $scope.clearDetailNo = function () {
        $scope.openingBalance.PartyType = $scope.previousSelected;
        $scope.partyType = $scope.previousSelected;
    };
}