'use strict';
trialBalanceReportController.$inject = ['$scope', '$rootScope', '$filter', "baseService", "$http", "$window"];
function trialBalanceReportController($scope, $rootScope, $filter, baseService, $http, $window) {
    $rootScope.title = 'Trial Balance';
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    $scope.report = {
        IsUpToLevel: 'GL',
        IsBudgetLevel: false,
        IsActivityLevel: false,
        IsDetailLevel: false,
        OrganizationType: "Plant",
        ReportFormat: 'Pdf',
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now())
        
    };
    $scope.reportDateWise = {
        IsUpToLevel: 'GL',
        IsBudgetLevel: false,
        IsActivityLevel: false,
        IsDetailLevel: false,
        OrganizationType: "Plant",
        ReportFormat: 'Pdf',
        FromDate: $filter('dateFiltering')(new Date(date.getFullYear(), date.getMonth(), 1)),
        ToDate: $filter('dateFiltering')(Date.now())

    };
    $scope.dateRange = "false";
    $scope.fromDateTitle = "As On Date";
    $scope.legendTitle = "As On Date";
    $scope.toDateShow = false;
    $scope.viewChange = function () {
        if ($scope.dateRange === "true") {
            $scope.fromDateTitle = "fromDate";
            $scope.legendTitle = "Within  Date Range";
            $scope.toDateShow = true;
            $scope.report = {
                IsUpToLevel: null,
                IsBudgetLevel: false,
                IsActivityLevel: false,
                IsDetailLevel: false,
                OrganizationType: "Plant",
                ReportFormat: 'Pdf',
                //FromDate: $filter('dateFiltering')(Date.now()),
                FromDate: $filter('dateFiltering')(new Date(firstDay.getFullYear(), firstDay.getMonth(), 1)),
                ToDate: $filter('dateFiltering')(Date.now())
            };
        }
        else {
            $scope.fromDateTitle = "As On Date";
            $scope.legendTitle = "As On Date";
            $scope.toDateShow = false;
            $scope.report = {
                IsUpToLevel: null,
                IsBudgetLevel: false,
                IsActivityLevel: false,
                IsDetailLevel: false,
                OrganizationType: "Plant",
                ReportFormat: 'Pdf',
                //FromDate: $filter('dateFiltering')(Date.now()),
                FromDate: $filter('dateFiltering')(Date.now()),
                ToDate: $filter('dateFiltering')(Date.now())
            };
        }
    };

    $scope.reportFunctionCaller = function () {
        if ($scope.dateRange === "true") {
            $scope.getDateWiseTrialBalanceReport();
        }
        else {
            $scope.getReport();

        }
    };

    $scope.upToLevelList = [];

    $scope.getLevelType = function () {
        $http({
            method: "GET",
            url: "Enum/GetTrailBalanceLevelCbo/"
        }).then(function successCallback(response) {
            $scope.upToLevelList = response.data;
            $scope.report.IsUpToLevel = response.data[0].Value;
            $scope.yearClosedTBReport.IsUpToLevel = response.data[0].Value;
        });
    };
    $scope.getLevelType();
    $scope.LevelAssaign = function (level) {
        if (level == 'GL') {
            $scope.report.IsBudgetLevel = false;
            $scope.report.IsActivityLevel = false;
            $scope.reportDateWise.IsBudgetLevel = false;
            $scope.reportDateWise.IsDetailLevel = false;
            $scope.reportDateWise.IsActivityLevel = false;
            $scope.report.IsDetailLevel = false;
        }
        if (level == 'Budget') {
            $scope.report.IsBudgetLevel = true;
            $scope.report.IsActivityLevel = false;
            $scope.report.IsDetailLevel = false;
            $scope.report.isACGroupLevel = false;
            $scope.reportDateWise.IsBudgetLevel = true;
            $scope.reportDateWise.IsDetailLevel = false;
            $scope.reportDateWise.IsActivityLevel = false;
            $scope.reportDateWise.isACGroupLevel = false;

        }
        if (level == 'Detail') {
            $scope.report.IsDetailLevel = true;
            $scope.report.IsBudgetLevel = false;
            $scope.report.IsActivityLevel = false;
            $scope.report.isACGroupLevel = false;
            $scope.reportDateWise.IsBudgetLevel = false;
            $scope.reportDateWise.IsActivityLevel = false;
            $scope.reportDateWise.IsDetailLevel = true;
            $scope.reportDateWise.isACGroupLevel = false;
        }
        else if (level == 'Activity') {
            $scope.report.IsBudgetLevel = false;
            $scope.report.IsDetailLevel = false;
            $scope.report.IsActivityLevel = true;
            $scope.reportDateWise.IsDetailLevel = false;
            $scope.reportDateWise.IsBudgetLevel = false;
            $scope.reportDateWise.IsActivityLevel = true;

        }
    };
    $scope.PartyId = null; $scope.PartyPlantId = null;
    $scope.getReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
            manualValidation('div_FromDate', true, "Date is required.");
        }
        else {
            if ($scope.report.OrganizationType === "Company") {
                var url = 'Accounts/Voucher/TrialBalanceReportCompanyLevel?reportFormat=' + $scope.report.ReportFormat + '&date=' + $scope.report.FromDate + '&isBudgetLevel=' + $scope.report.IsBudgetLevel + '&isActivityLevel=' + $scope.report.IsActivityLevel + '&isDetailLevel=' + $scope.report.IsDetailLevel;
                $window.open(url, '_blank');
            }
            else {
                var url = 'Accounts/Voucher/TrialBalanceReport?reportFormat=' + $scope.report.ReportFormat
                    + '&date=' + $scope.report.FromDate + '&isBudgetLevel=' + $scope.report.IsBudgetLevel
                    + '&isActivityLevel=' + $scope.report.IsActivityLevel
                    + '&isDetailLevel=' + $scope.report.IsDetailLevel + '&partyId=' + $scope.PartyId + '&partyPlantId=' + $scope.PartyPlantId;
                $window.open(url, '_blank');
            }
        }
    };
    $scope.getDateWiseTrialBalanceReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
            manualValidation('div_WDFromDate', true, "From Date is required.");
        }
        if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
            manualValidation('div_WDToDate', true, "To Date is required.");
        }
        else {
            if ($scope.report.OrganizationType === "Company") {
                var url = 'Accounts/Voucher/DateRangeWiseTrialBalanceReportCompanyLevel?reportFormat=' + $scope.report.ReportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&isBudgetLevel=' + $scope.report.IsBudgetLevel + '&isActivityLevel=' + $scope.report.IsActivityLevel + '&isDetailLevel=' + $scope.report.IsDetailLevel;
                $window.open(url, '_blank');
            }
            else {
                var url = 'Accounts/Voucher/DateRangeWiseTrialBalanceReport?reportFormat=' + $scope.report.ReportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&isBudgetLevel=' + $scope.report.IsBudgetLevel + '&isActivityLevel=' + $scope.report.IsActivityLevel + '&isDetailLevel=' + $scope.report.IsDetailLevel;
                $window.open(url, '_blank');
            }
        }
    };

    $scope.GLGeneralInfoId = '';
    $scope.Active = true;
    $scope.PartyType = 'Customer';
    $scope.ReportSize = 'ShortSize';
    $scope.getPartyLedgerReport = function () {
        if (baseService.isUndefinedOrNull($scope.PartyId)) {
            ShowResult("Party is required.", 'failure');
        }
        else if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
            manualValidation('div_WDFromDate', true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
            manualValidation('div_WDToDate', true, "To Date is required.");
        }
        else if (new Date($scope.report.FromDate) > new Date($scope.report.ToDate)) {
            manualValidation('div_WDFromDate', true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.report.ToDate) < new Date($scope.report.FromDate)) {
            manualValidation('div_WDToDate', true, "To date must be above or equal to From Date.");
        }

        else {
                var url = 'Parties/PartyReport/GetPartyLedgerReport?reportFormat=' + $scope.report.ReportFormat + '&partyType=' + $scope.PartyType + '&partyId=' + $scope.PartyId + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&active=' + $scope.Active;
                if (!baseService.isUndefinedOrNull($scope.PartyPlantId)) {
                    url += '&partyPlantId=' + $scope.PartyPlantId;
                }
                if (!baseService.isUndefinedOrNull($scope.GLGeneralInfoId)) {
                    url += '&glId=' + $scope.GLGeneralInfoId;
                }
               
                $window.open(url, '_blank');
        }
    };
    $scope.partyList = [];
    $scope.partyPlantList = [];
    $scope.searchByParty = "UserName"; $scope.searchParty = "";
    $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: "Party" }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];
    $scope.searchByParty_Loan = "UserName"; $scope.searchParty_Loan = "";
    $scope.searchByPartyList_Loan = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: "Party Name" }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

    $scope.showPartyPopUpNew = function () {
        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
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

    $scope.showPartyPlantPopUp = function (partyPlantId) {
        $scope.getPartyLocationDetail(partyPlantId);
        angular.element(document.querySelector('#partyPlantPopUp')).modal('show');
    };

    $scope.partyPlant = {
        PartyCountry: null,
        PartyState: null,
        PartyCity: null,
        PartyGSTIN: null,
        PartyAddress: null
    };

    $scope.getPartyLocationDetail = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            for (var i = 0; i < baseService.arrayLength($scope.partyPlantList); i++) {
                if ($scope.partyPlantList[i].Value === id) {
                    $scope.partyPlant.PartyCountry = $scope.partyPlantList[i].CountryName;
                    $scope.partyPlant.PartyState = $scope.partyPlantList[i].StateCode + ' - ' + $scope.partyPlantList[i].StateName;
                    $scope.partyPlant.PartyCity = $scope.partyPlantList[i].CityName;
                    $scope.partyPlant.PartyGSTIN = $scope.partyPlantList[i].GSTIN;
                    $scope.partyPlant.PartyAddress = $scope.partyPlantList[i].Address1;
                }
            }
        }
        else {
            $scope.partyPlant.PartyCountry = null;
            $scope.partyPlant.PartyState = null;
            $scope.partyPlant.PartyCity = null;
            $scope.partyPlant.PartyGSTIN = null;
            $scope.partyPlant.PartyAddress = null;
        }
    };

    $scope.closePartyPopUp = function (x) {
        var party = x.data;
        $scope.PartyId = party.Id;
        $scope.PartyType = party.PartyType;
        $scope.PartyName = party.UserName;
        $scope.getPartyPlantList(party.Id);
        $scope.hidePartyPopUp();
    };
    $scope.getPartyPlantList = function (partyId) {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.partyPlantList.push(item);
                    if (item.IsDefault) {
                        $scope.partyPlantId = item.Value;
                    }
                });
            });
    };
    $scope.hidePartyPopUp = function () {
        angular.element(document.querySelector('#partyPopUp')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };

    $scope.partyRefresh = function () {
        $scope.PartyPlantId = null;
        $scope.PartyId = null;
        $scope.PartyName = null;
    }

    $scope.yearClosedByDateList = [];
    $scope.checkYearClosedByDate = function (date) {
        $scope.yearClosedByDateList = [];
        $http({
            method: "GET",
            url: "accounts/FiscalYearClose/CheckYearClosedByDate?date=" + date
        }).then(function successCallback(response) {
            $scope.yearClosedByDateList = response.data;
            if ($scope.yearClosedByDateList.length > 0) {
                $scope.report.FromDate = $filter('dateFiltering')(Date.now());
                $scope.report.ToDate = $filter('dateFiltering')(Date.now());
                $scope.report.Date = $filter('dateFiltering')(Date.now());
                ShowResult('Fiscal Year already closed!!!', 'failure');
            }
        });
    };

    //Year Closed Income Statement
    $scope.yearClosedTBReport = {
        FiscalYearCloseId: null,
        FiscalYearName: null,
        IsUpToLevel: null,
        IsBudgetLevel: false,
        IsActivityLevel: false,
        IsDetailLevel: false,
        isACGroupLevel: false,
        ReportFormat: 'Excel'
    };
    $scope.yearClosedLevelAssaignTB = function (level) {
        $scope.yearClosedTBReport.IsBudgetLevel = false;
        $scope.yearClosedTBReport.IsActivityLevel = false;
        $scope.yearClosedTBReport.IsDetailLevel = false;
        if (level == 'GL') {
            $scope.yearClosedTBReport.IsBudgetLevel = false;
            $scope.yearClosedTBReport.IsActivityLevel = false;
            $scope.yearClosedTBReport.IsDetailLevel = false;

        }
        if (level == 'Budget') {
            $scope.yearClosedTBReport.IsBudgetLevel = true;
            $scope.yearClosedTBReport.IsActivityLevel = false;
            $scope.yearClosedTBReport.IsDetailLevel = false;

        }
        if (level == 'Activity') {
            $scope.yearClosedTBReport.IsBudgetLevel = false;
            $scope.yearClosedTBReport.IsActivityLevel = true;
            $scope.yearClosedTBReport.IsDetailLevel = false;

        }
        if (level == 'Detail') {
            $scope.yearClosedTBReport.IsDetailLevel = true;
            $scope.yearClosedTBReport.IsBudgetLevel = false;
            $scope.yearClosedTBReport.IsActivityLevel = false;
            $scope.yearClosedTBReport.isACGroupLevel = false;
        }
    };
    $scope.masterList = [];
    $scope.getMasterData = function () {
        $scope.masterList = [];
        $http.get("accounts/FiscalYearClose/GetFiscalYearClosedListForReporting")
            .then(
                function successCallback(response) {
                    $scope.masterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#FiscalYearClosepopUp')).modal('show');
    };
    $scope.getFiscalYearClosedData = function () {
        $scope.masterList = [];
        $http.get("accounts/FiscalYearClose/GetFiscalYearClosedListForReporting")
            .then(
                function successCallback(response) {
                    $scope.masterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getFiscalYearClosedData();

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#FiscalYearClosepopUp')).modal('hide');
    }

    $scope.SelectMaster = function (x) {
        var data = x.data;
        $scope.yearClosedTBReport.FiscalYearCloseId = data.Id;
        $scope.yearClosedTBReport.FiscalYearName = data.FiscalYearName;
        angular.element(document.querySelector('#FiscalYearClosepopUp')).modal('hide');
    };
    $scope.getYearClosedTBReport = function () {
        if (baseService.isUndefinedOrNull($scope.yearClosedTBReport.FiscalYearName)) {
            manualValidation('div_FiscalYearName', true, "Fiscal Year is required.");
        }
        else {
            var url = 'Accounts/Voucher/TrialBalanceYearClosedReport?reportFormat=' + $scope.yearClosedTBReport.ReportFormat
                + '&fiscalYearCloseId=' + $scope.yearClosedTBReport.FiscalYearCloseId + '&fiscalYearName=' + $scope.yearClosedTBReport.FiscalYearName
                + '&isBudgetLevel=' + $scope.yearClosedTBReport.IsBudgetLevel
                + '&isActivityLevel=' + $scope.yearClosedTBReport.IsActivityLevel
                + '&isDetailLevel=' + $scope.yearClosedTBReport.IsDetailLevel ;
            $window.open(url, '_blank');
            //location.href = 'accounts/voucher/TrialBalanceYearClosedReport?fiscalYearCloseId=' + $scope.yearClosedTBReport.FiscalYearCloseId + '&fiscalYearName=' + $scope.yearClosedTBReport.FiscalYearName + '&isBudgetLevel=' + $scope.yearClosedTBReport.IsBudgetLevel + '&isActivityLevel=' + $scope.yearClosedTBReport.IsActivityLevel + '&isDetailLevel=' + $scope.yearClosedTBReport.IsDetailLevel;
        }
    };
    //Year Closed Income Statement
}