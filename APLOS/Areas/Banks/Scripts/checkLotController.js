"use strict";
checkLotController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter","$controller"];
function checkLotController(commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Cheque Lot";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.path = "Banks/CheckManagement/";
    //$scope.getLotNumberUrl = $scope.path + "getlotnumber/";
    $controller("bankBaseController", { $scope: $scope, $http: $http });


    $scope.CheckManagement = {
        Id: null,
        BankMasterId: null,
        BankName: null,
        BankBranch: null,
        BankAccount: null,
        BankGL: null,
        BankCurrencyId: null,
        BankCurrency: null,
        LotNumber: null,
        FromNo: null,
        ToNo: null,
        IsNonSequential: false,
        Active: true

    };
    $scope.checkLotNew = Object.assign({}, $scope.CheckManagement);

    //$scope.GetLotNumber = function () {
    //    $http.get($scope.getLotNumberUrl)
    //        .then(function (response) {
    //            $scope.checkLotNew.LotNumber = response.data;
    //        });
    //};
   //$scope.GetLotNumber();


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
            $scope.checkLotNew.BankMasterId = bank.BankMasterId;
            $scope.checkLotNew.BankName = bank.BankName;
            $scope.checkLotNew.BankBranch = bank.BankBranchName;
            $scope.checkLotNew.BankAccount = bank.AccountTitle;
            $scope.checkLotNew.BankGL = bank.GLItem;
            $scope.checkLotNew.GLGeneralInfoId = bank.GLGeneralInfoId;
            $scope.checkLotNew.BankGL = bank.GLGeneralInfoId + ' - ' + bank.GLGeneralInfoName;
            $scope.checkLotNew.BankCurrencyId = bank.CurrencyId;
            $scope.checkLotNew.BankCurrency = bank.CurrencyCode;
            $scope.getchequeLotList($scope.checkLotNew.BankMasterId);
        }
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.Save = function () {
        try {
            $scope.$broadcast("show-errors-check-validity");
            if ($scope.checkLotNewForm.$valid) {
                angular.copy($scope.checkLotNew, $scope.CheckManagement);
              if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.path + "create",
                    data: $scope.CheckManagement,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.
                            data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.Clear();
                        $scope.getchequeLotList($scope.checkLotNew.BankMasterId);
                    }
                });
                    return true;
              }
                if ($scope.Action === "Update") {
                    $http({
                        method: "POST",
                        url: $scope.path + "Edit",
                        data: $scope.CheckManagement,
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.
                                data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.Clear();
                            $scope.getchequeLotList($scope.checkLotNew.BankMasterId);
                        }
                    });
                    return true;
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Clear = function () {
        ClearFields();
        //ClearFields($scope.GetLotNumber());
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.CheckManagement = {};
        $scope.checkLotNew.Active = true;
        $scope.checkLotNew.FromNo = null;
        $scope.checkLotNew.ToNo = null;
        $scope.checkLotNew.LotNumber = null;
       // $scope.checkLotNew.LotNumber = lotNumber;
        $scope.checkLotNew.IsNonSequential = false;
    }
    $scope.chequeList = [];
    $scope.getchequeLotList = function (bankMasterId) {
        $http({
            method: "get",
            url: "banks/CheckManagement/GetChequeLotList?bankMasterId=" + bankMasterId
        }).then(function successCallback(response) {
            $scope.chequeList = response.data.Rows;
        });
    };

    $scope.getBank = function () {
        try {
            $scope.getBankData = function (pageno) {
                baseService.paginationBase("banks/bankmaster/GetHouseBankBankMasterList", pageno, $scope.bankParameters)
                    .then(function (result) {
                        $scope.bankList = result.Rows;
                        $scope.bankParameters.ToNotal_count = result.ToNotal;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, "failure");
                    }).finally(function () {
                    });
            };
            $scope.getBankData();
            angular.element(document.querySelector("#bankPopUp")).modal("show");
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.getchequeLotDetailList = function (chequeId) {
        $scope.TempchequeLot = null;
        try {
        $http({
            method: "get",
            url: "banks/CheckManagement/GetChequeLotDetailList?chequeId=" + chequeId
        }).then(function successCallback(response) {
            $scope.chequeLotDetailList = response.data;
            $scope.TempchequeLot = chequeId;
            });
            angular.element(document.querySelector("#chequeLotPopUp")).modal("show");
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector("#chequeLotPopUp")).modal("hide");

    }

    $scope.Get = function (data) {
        $scope.checkLotNew.Id = data.Id;
        $scope.checkLotNew.FromNo = data.FromNo;
        $scope.checkLotNew.ToNo = data.ToNo;
        $scope.checkLotNew.LotNumber = data.LotNumber;
        $scope.Action = "Update";
    };
}