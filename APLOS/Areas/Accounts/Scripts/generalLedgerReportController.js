"use strict";
generalLedgerReportController.$inject = ["$scope", "$rootScope", "$filter", "bankService", "accountService", "$window", "baseService", "$controller", '$http'];
function generalLedgerReportController($scope, $rootScope, $filter, bankService, accountService, $window, baseService, $controller, $http) {
    $rootScope.title = "General Ledger";
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $scope.partyType = 'Customer';
    $scope.report = {
        GLName: null,
        GLGeneralInfoId: null,
        BudgetMasterId: null,
        ActivityId: null,
        BankCashParty: null,
        BankMasterId: null,
        CashMasterId: null,
        PartyId: null,
        ReportFormat: "Pdf",
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now()),
        Active: true,
        IsGroupBy:false
    };
    $(".searchableDDL").select2();

    $scope.glList = [];
    $scope.getCompanyGLCboList = function () {
        accountService.getCompanyGLCboList(function (result) {
            $scope.glList = result;
        });
    };
    $scope.getCompanyGLCboList();

    $scope.budgetList = [];
    $scope.getBudgetMasterCboList = function (glId) {
        accountService.getBudgetMasterCboList(glId, function (result) {
            $scope.budgetList = result;
        });
    };

    $scope.activityList = [];
    $scope.getBudgetMasterActivityCbo = function (budgetMasterId) {
        accountService.getBudgetMasterActivityCbo(budgetMasterId, function (result) {
            $scope.activityList = result;
        });
    };

    $scope.getReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.GLGeneralInfoId)) {
            manualValidation("div_GL", true, "GL is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
            manualValidation("div_FromDate", true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
            manualValidation("div_ToDate", true, "To Date is required.");
        }
        else if (new Date($scope.report.FromDate) > new Date($scope.report.ToDate)) {
            manualValidation("div_FromDate", true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.report.ToDate) < new Date($scope.report.FromDate)) {
            manualValidation("div_ToDate", true, "To date must be above or equal to From Date.");
        }
        else {
            var url = "";
            url = "Accounts/Voucher/GetGeneralLedgerReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + "&glId=" + $scope.report.GLGeneralInfoId + '&active=' + $scope.report.Active + '&IsGroupBy=' + $scope.report.IsGroupBy;
            //if ($scope.report.Active) {

            //}
            //else {

            //url = "Accounts/Voucher/GetGeneralLedgerReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + "&glId=" + $scope.report.GLGeneralInfoId;
            //}
            if (!baseService.isUndefinedOrNull($scope.report.BudgetMasterId)) {
                url += "&budgetMasterId=" + $scope.report.BudgetMasterId;
            }
            if (!baseService.isUndefinedOrNull($scope.report.ActivityId)) {
                url += "&activityId=" + $scope.report.ActivityId;
            }
            if (!baseService.isUndefinedOrNull($scope.report.BudgetMasterId)) {
                url += "&bankMasterId=" + $scope.report.BankMasterId;
            }
            if (!baseService.isUndefinedOrNull($scope.report.ActivityId)) {
                url += "&cashMasterId=" + $scope.report.CashMasterId;
            }
            if (!baseService.isUndefinedOrNull($scope.report.ActivityId)) {
                url += "&partyId=" + $scope.report.PartyId;
            }
            $window.open(url, "_blank");
        }
    };
    //$scope.getReportWithDocRef = function () {
    //    if (baseService.isUndefinedOrNull($scope.report.GLGeneralInfoId)) {
    //        manualValidation("div_GL", true, "GL is required.");
    //    }
    //    else if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
    //        manualValidation("div_FromDate", true, "From Date is required.");
    //    }
    //    else if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
    //        manualValidation("div_ToDate", true, "To Date is required.");
    //    }
    //    else if (new Date($scope.report.FromDate) > new Date($scope.report.ToDate)) {
    //        manualValidation("div_FromDate", true, "From date must be below or equal to To Date");
    //    }
    //    else if (new Date($scope.report.ToDate) < new Date($scope.report.FromDate)) {
    //        manualValidation("div_ToDate", true, "To date must be above or equal to From Date.");
    //    }
    //    else {
    //        var url = "Accounts/Voucher/GetGeneralLedgerReportWithDocRef?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + "&glId=" + $scope.report.GLGeneralInfoId  + '&active=' + $scope.report.Active;
    //        if (!baseService.isUndefinedOrNull($scope.report.BudgetMasterId)) {
    //            url += "&budgetMasterId=" + $scope.report.BudgetMasterId;
    //        }
    //        if (!baseService.isUndefinedOrNull($scope.report.ActivityId)) {
    //            url += "&activityId=" + $scope.report.ActivityId;
    //        }
    //        $window.open(url, "_blank");
    //    }
    //};
    $scope.bankACType = "Loan";
    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];
            if (baseService.isUndefinedOrNull(bank.GLGeneralInfoId)) {
                ShowResult("Bank GL not found!", "failure", "bankPopUp");
                return;
            }

            else if (baseService.isUndefinedOrNull(bank.CurrencyId)) {
                ShowResult("Bank Transaction Currency not found!", "failure", "bankPopUp");
                return;
            }
            else {
                $scope.report.AccountTitle = bank.AccountTitle;
                $scope.report.BankName = bank.BankCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
                $scope.report.BankMasterId = bank.BankMasterId;
            }
        }
        $scope.hideBankPopUp();
    };
    bankService.getCashMasterCboList(function (result) {
        $scope.cashkMasterList = result;
    });
    $scope.BankCashPartyByList = [
        {
            "name": "Bank",
            "value": "Bank"
        },
        {
            "name": "Cash",
            "value": "Cash"
        },
        {
            "name": "Party",
            "value": "Party"
        }
    ];

    $scope.searchByParty = "UserName"; $scope.searchParty = "";
    $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

    $scope.changePartyType = function () {
        $scope.partyType = $scope.report.PartyType;
        $scope.customerNameCode = null;
        $scope.GLNameCode = null;
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];
    };
    $scope.partyList = [];
    $scope.showPartyPopUpNew = function () {
        if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor' || $scope.partyType === 'Director') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListForReport?partyType=' + $scope.partyType;
        }
        else if ($scope.partyType === 'Party') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListForReport';
        }
        else if ($scope.partyType === 'Other') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListForReport';
        }
        else if ($scope.partyType === 'Both') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListForReport';
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
    $scope.closePartyPopUp = function myfunction(x) {
        var data = x.data;
        if ($scope.report.PartyType === 'Customer') {
            $scope.report.vendorId = null;
            $scope.report.PartyId = data.PartyId;
            $scope.customerNameCode = data.Code + ' - ' + data.PartyName;
            //$scope.getPartyPlantList($scope.report.PartyId);
            //$scope.getPartyGSTINList($scope.report.PartyId, $scope.report.PartyPlantId);
        }
        else if ($scope.report.PartyType === 'Vendor') {
            $scope.report.PartyId = null;
            $scope.report.PartyId = data.PartyId;
            $scope.customerNameCode = data.Code + ' - ' + data.PartyName;
            //$scope.getPartyPlantList($scope.report.PartyId);
            //$scope.getPartyGSTINList($scope.report.PartyId, $scope.report.PartyPlantId);
        }
        else if ($scope.report.PartyType === 'Director') {
            $scope.report.PartyId = null;
            $scope.report.PartyId = data.PartyId;
            $scope.customerNameCode = data.Code + ' - ' + data.PartyName;
            //$scope.getPartyPlantList($scope.report.PartyId);
            //$scope.getPartyGSTINList($scope.report.PartyId, $scope.report.PartyPlantId);
        }
        else if ($scope.report.PartyType === 'Party') {
            $scope.report.PartyId = null;
            $scope.report.PartyId = data.PartyId;
            $scope.customerNameCode = data.Code + ' - ' + data.PartyName;
            //$scope.getPartyPlantList($scope.report.PartyId);
            //$scope.getPartyGSTINList($scope.report.PartyId, $scope.report.PartyPlantId);
        }
        else {
            $scope.report.PartyId = null;
            $scope.report.vendorId = null;
            $scope.report.MainPartyId = data.PartyId;
            $scope.customerNameCode = data.Code + ' - ' + data.PartyName;
        }
        $scope.hidePartyPopUp();
    };
    $scope.hidePartyPopUp = function () {
        angular.element(document.querySelector('#partyPopUp')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };
    $scope.clear = function () {
        $scope.report = {
        };
        $scope.report.GLName = null;
        $scope.report.GLGeneralInfoId = null;
        $scope.report.BudgetMasterId = null;
        $scope.report.ActivityId = null;
        $scope.report.BankCashParty = null;
        $scope.report.BankMasterId = null;
        $scope.report.CashMasterId = null;
        $scope.report.PartyId = null;
        $scope.report.ReportFormat = "Pdf";
        $scope.report.FromDate = $filter("dateFiltering")(Date.now());
        $scope.report.ToDate = $filter("dateFiltering")(Date.now());
        $scope.report.Active = true;
        $scope.report.IsGroupBy = false;
        $scope.customerNameCode = null;
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
            "name": "RefNo",
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
        $scope.GLUrl1 = "Accounts/glitem/GetAllGLBudgetActivityList";
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
        $scope.closeCOAICodeListPopUp();
    };

    $scope.addRow = function (data) {
        $scope.getCompanyGLCboList();
        $scope.getBudgetMasterCboList(data.GLGeneralInfoId);
        $scope.getBudgetMasterActivityCbo(data.BudgetMasterId);
        $scope.report.GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.report.BudgetMasterId = data.BudgetMasterId;
        $scope.report.ActivityId = data.ActivityId;
    };
}