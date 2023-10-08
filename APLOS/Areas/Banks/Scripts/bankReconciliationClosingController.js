"use strict";
bankReconciliationClosingController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$window",  "$controller"];
function bankReconciliationClosingController(commonMessage, $scope, $rootScope, baseService, $http, $filter, $window, $controller) {
    $rootScope.title = "Bank Reconciliation Closing";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.path = "banks/bankreconciliation/";
    $controller("bankBaseController", { $scope: $scope, $http: $http });

    $scope.searchBy = "FixedAssetItem"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'CapitalizationDate', name: "Capitalization Date" }, { value: 'AddedDate', name: "Added Date" }, { value: 'FixedAssetMasterId', name: "Asset Master Id" }, { value: 'FixedAssetItemId', name: "Asset Item Id" }, { value: 'FixedAssetMaster', name: "Asset Master" }, { value: 'FixedAssetItem', name: "Asset Item" }, { value: 'Type', name: "Type" }, { value: 'CMStatus', name: "Status" }];

    $scope.masterList = [];
    $scope.getData = function () {
        $scope.masterList = [];
        $http({
            method: 'Post'
            , url: 'banks/bankreconciliation/GetBankReconciliationClosingList'
            , data: { column: $scope.searchBy, value: $scope.search }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.masterList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.getData();

    $scope.bankReconciliationClosing = {
        Id: null,
        CompanyGroupId: $window.companyGroupId,
        CompanyId: $window.companyId,
        PlantId: $window.plantId,
        BankMasterId: null,
        BankName: null,
        FiscalYearId: null,
        DrAmount: 0,
        CrAmount: 0
    };
   
    $scope.bankList = [];
    $scope.bankIndex = -1;
    $scope.selectedBank = null;
    

    $scope.selectBankPopUp = function (index, id) {
        $scope.bankIndex = index;
        $scope.selectedBank = id;
    };

    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            selectBankRow();
        }
        angular.element(document.querySelector("#bankPopUp")).modal("hide");
        $scope.bankIndex = -1;
    };

    function selectBankRow() {
        var bank = $scope.bankList[$scope.bankIndex];
        if (bank.GLGeneralInfoId === null) {
            ShowResult("Bank GL not found!", "failure");
        }
        else if (bank.CurrencyId === null) {
            ShowResult("Bank Transaction Currency not found!", "failure");
        }
        else {
            $scope.bankReconciliationClosing.BankMasterId = bank.BankMasterId;
            $scope.bankReconciliationClosing.BankName = bank.BankName;
        }
    }
    $scope.fiscalYearList = [];
    $http({
        method: 'GET',
        url: 'accounts/FiscalYear/GetCbo'
    }).then(function successCallback(response) {
        $scope.fiscalYearList = response.data;
    });

    $scope.Save = function () {
        try {
            $scope.$broadcast("show-errors-check-validity");
            if ($scope.form0.$valid) {
                $scope.saveBtnDisable = true;
                $http({
                    method: "POST",
                    url: $scope.path + "CreateCapitalize",
                    dataType: "JSON",
                    data: {
                        "data": $scope.bankReconciliationClosing,
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                        $scope.saveBtnDisable = false;
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.bankReconciliationClosing.Id = response.data.Id;
                        $scope.getData();
                        $scope.saveBtnDisable = false;
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                    $scope.saveBtnDisable = false;
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Clear = function () {
        $scope.bankReconciliationClosing = {
            Id: null,
            CompanyGroupId: $window.companyGroupId,
            CompanyId: $window.companyId,
            PlantId: $window.plantId,
            BankMasterId: null,
            BankName: null,
            FiscalYearId: null,
            DrAmount: 0,
            CrAmount: 0
        };
    }
    $scope.SelectMaster = function (obj) {
        $scope.bankReconciliationClosing = obj.data;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.Action = 'Update';
    };
   
}