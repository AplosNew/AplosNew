"use strict";
bankSheetGenerationController.$inject = ["$scope", "$rootScope", "$filter", "bankService", "$window", "baseService", "$http", "$controller"];
function bankSheetGenerationController($scope, $rootScope, $filter, bankService, $window, baseService, $http, $controller) {
    $rootScope.title = "Bank Sheet Generation";
    $controller("bankBaseController", { $scope: $scope, $http: $http });

    $scope.report = {
        BankMasterId: null,
        ReportFormat: "Pdf",
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now())
       // AccountTitle: null,
        //BankName:null
    };

    //bankService.getBankMasterHouseBankCboList(function (result) {
    //    $scope.bankMasterList = result;
    //});
    //$scope.closeBankPopUp = function () {
    //    if ($scope.bankIndex !== -1) {
    //        var bank = $scope.bankList[$scope.bankIndex];
            
    //        $scope.report.AccountTitle = bank.AccountTitle;
    //        $scope.report.BankName = bank.BankCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
    //        $scope.report.BankMasterId = bank.BankMasterId;
    //    }
    //    $scope.hideBankPopUp();
    //};
    $scope.PartyListForReport = "";
    $scope.getReport = function () {
        try {
            $scope.PartyListForReport = "";
            for (var i = 0; i < $scope.newPartyList.length; i++) {
                if ($scope.newPartyList[i].IsSelect) {
                    if ($scope.PartyListForReport === "") {
                        $scope.PartyListForReport += "'" + $scope.newPartyList[i].PartyId + "'";
                    }
                    else {
                        $scope.PartyListForReport += ",'" + $scope.newPartyList[i].PartyId + "'";
                    }
                }
            }
            if ($scope.PartyListForReport == "") {
                throw "Select Party..!";
            }
            if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
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
                var url = "Banks/BankReport/GetBankSheetGenerationReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + "&bankMasterId=" + $scope.report.BankMasterId + "&PartyList=" + $scope.PartyListForReport;
                $window.open(url, "_blank");
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
        
    };

    //$scope.getSalaryHeadGlReport = function () {
    //    var file_src = 'employees/salaryHeadGL/GetSalaryHeadGlReport';
    //    //  $scope.path = 'employees/salaryHeadGL/';
    //    $rootScope.report(file_src);
    //}

    //#region Saad's Part
    $scope.newPartyList = [];
    $scope.ShowData = false;
    $scope.getPartyData = function () {
        try {
            if (new Date($scope.report.FromDate) > new Date($scope.report.ToDate)) {
                throw "To Date can't less than From Date.";
            }
            $http({
                method: 'POST',
                url: 'Banks/BankReport/GetPartyDateWise/',
                data: { 'FromDate': $scope.report.FromDate, 'ToDate': $scope.report.ToDate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.newPartyList = response.data;
                $scope.ShowData = true;
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    //$scope.searchByParty = "UserName"; $scope.searchParty = "";
    //$scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

    //$scope.changePartyType = function () {
    //    $scope.partyType = $scope.report.PartyType;
    //    $scope.customerNameCode = null;
    //    $scope.GLNameCode = null;
    //    $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];
    //};
    //$scope.partyList = [];
    //$scope.showPartyPopUpNew = function () {
    //    if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
    //        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;
    //    }
    //    else if ($scope.partyType === 'Party') {
    //        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
    //    }
    //    else if ($scope.partyType === 'Director') {
    //        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
    //    }
    //    else if ($scope.partyType === 'Other') {
    //        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
    //    }
    //    $http({
    //        method: 'POST',
    //        url: $scope.partyUrl,
    //        data: { column: $scope.searchByParty, value: $scope.searchParty },
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.partyList = response.data;
    //    });
    //    angular.element(document.querySelector('#partyPopUp')).modal('show');
    //};
    //$scope.closePartyPopUpNew = function () {
    //    angular.element(document.querySelector('#partyPopUp')).modal('hide');
    //};
    
    //
    //$scope.closePartyPopUp = function (x) {

    //    var partyId = x.data.PartyId;
    //    var PartyName = x.data.PartyName;
    //    $scope.newPartyList.push({ "partyId": partyId, "PartyName": PartyName });
    //    
    //    
    //    
    //    
    //    
    //    
    //    $scope.hidePartyPopUp();
    //    // TODO:

    //}
    //$scope.hidePartyPopUp = function () {
    //    angular.element(document.querySelector('#partyPopUp')).modal('hide');
    //    $scope.partyIndex = -1;
    //    $scope.partySelected = null;
    //};
   
    //#endregion

}