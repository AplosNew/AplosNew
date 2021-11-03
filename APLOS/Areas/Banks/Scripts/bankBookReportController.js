"use strict";
bankBookReportController.$inject = ["$scope", "$rootScope", "$filter", "bankService", "$window", "baseService", "$http", "$controller"];
function bankBookReportController($scope, $rootScope, $filter, bankService, $window, baseService, $http, $controller) {
    $rootScope.title = "Bank Book";
    $controller("bankBaseController", { $scope: $scope, $http: $http });

    $scope.report = {
        BankMasterId: null,
        ReportFormat: "Pdf",
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now()),
        AccountTitle: null,
        BankName:null
    };

    bankService.getBankMasterHouseBankCboList(function (result) {
        $scope.bankMasterList = result;
    });
    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            var bank = $scope.bankList[$scope.bankIndex];
            
            $scope.report.AccountTitle = bank.AccountTitle;
            $scope.report.BankName = bank.BankCode + " - " + bank.BankName + " - " + bank.AccountTitle + " - " + bank.AccountNumber;
            $scope.report.BankMasterId = bank.BankMasterId;
        }
        $scope.hideBankPopUp();
    };

    $scope.getReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.BankMasterId)) {
            manualValidation("div_Bank", true, "Bank is required.");
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
            var url = "Banks/BankReport/GetBankBookReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + "&bankMasterId=" + $scope.report.BankMasterId;
            $window.open(url, "_blank");
        }
    };
}