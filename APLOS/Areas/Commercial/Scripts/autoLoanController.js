'use strict';
autoLoanController.$inject = ['accountService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService', 'bankService', '$controller'];
function autoLoanController(accountService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService, bankService, $controller) {
    $rootScope.title = "Auto Loan";
    $scope.Action = 'Save';
    $scope.path = 'Commercial/AutoLoan/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveChargesUrl = $scope.path + 'CreateCharge';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.partyType = "Vendor";
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $scope.fromDateTitle = "As On Date";
    $scope.toDateShow = false;
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);
    $scope.AutoLoan = {
        Id: null,
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now()),
        DateRange: "false"
    };

    $scope.AutoLoanNew = Object.assign({}, $scope.AutoLoan);


    $scope.paymentBasedOnList = [];
    cboService.getEnumCbo("enum/GetPaymentBasedOnEnumCbo", function (result) {
        $scope.paymentBasedOnList = result;
    });

    $scope.viewChange = function () {
        if ($scope.AutoLoanNew.DateRange === "true") {
            $scope.fromDateTitle = "From Date";
            $scope.toDateShow = true;
            $scope.AutoLoanNew.FromDate = $filter('dateFiltering')(new Date(firstDay.getFullYear(), firstDay.getMonth(), 1));
            $scope.AutoLoanNew.ToDate = $filter('dateFiltering')(Date.now());
        }
        else {
            $scope.fromDateTitle = "As On Date";
            $scope.toDateShow = false;
            $scope.AutoLoanNew.FromDate = $filter('dateFiltering')(Date.now());
            $scope.AutoLoanNew.ToDate = $filter('dateFiltering')(Date.now())
        }
    };
    $scope.AutoLoanAvailableDataList = [];
    $scope.getAutoLoanAvailableList = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Commercial/AutoLoan/GetAutoLoanAvailableList?dateRange=' + $scope.AutoLoanNew.DateRange + '&fromDate=' + $scope.AutoLoanNew.FromDate + '&toDate=' + $scope.AutoLoanNew.ToDate,
        }).then(function successCallback(response) {

            if (response.data.length > 0) {
                for (var i = 0; i < response.data.length; i++) {
                    response.data[i].PostingDate = new Date(response.data[i].PostingDate);
                    response.data[i].AcceptanceDate = new Date(response.data[i].AcceptanceDate);
                    response.data[i].BaseOnDueDate = new Date(response.data[i].BaseOnDueDate);
                    response.data[i].DueDate = new Date(response.data[i].DueDate);
                }
                //$scope.MasterLCList = response.data.DATA;
                $scope.AutoLoanAvailableDataList = response.data;
            }
            else {
                ShowResult(response.data.Message, 'failure');
            }

        });

    };
    $scope.getAutoLoanAvailableList();

    $scope.GetCurrencyExchangeRateList = function () {
        if (!baseService.isUndefinedOrNull($scope.AutoLoanNew.CurrencyId)) {
            $http({
                method: "GET",
                url: "currencies/ExchangeRate/GetCompanyCurrencyExchangeRate?fromdate=" + $filter("dateFiltering")(Date.now()) + "&currencyId=" + $scope.AutoLoanNew.CurrencyId
            }).then(function successCallback(response) {
                $scope.currencyExchangeRate = response.data;
                $scope.AutoLoanNew.Rate = $scope.currencyExchangeRate.ToCurrencyRate;
            });
        }
        else {
            $scope.currencyExchangeRate = [];
        }
    };
    $scope.bankMasterList = [];
    bankService.getBankMasterCboListByPlant(function (result) {
        $scope.bankMasterList = result;

    });

    $scope.validation = function () {
        let isbreak = false;

        if ($scope.SelectedForAutoLoanList.length == 0) {
            ShowResult("No row is selected. Please select one!", "failure");
            return true;
            isbreak = true;
        }

        if ($scope.SelectedForAutoLoanList.length > 0) {
            angular.forEach($scope.SelectedForAutoLoanList, function (item) {
                if (isbreak == false) {
                    if (item.Amount == 0 || item.Amount == '') {
                        isbreak = true;
                        ShowResult("Please Input Amount where AcceptanceNo is " + item.PurchaseDocAcceptanceId, "failure");
                        return true;
                    }
                    else if (baseService.isUndefinedOrNull(item.LoanNo)) {
                        isbreak = true;
                        ShowResult("Please Input LoanNo where AcceptanceNo is " + item.PurchaseDocAcceptanceId, "failure");
                        return true;
                    }
                    else if (baseService.isUndefinedOrNull(item.LoanDate)) {
                        isbreak = true;
                        ShowResult("Please Input LoanDate where AcceptanceNo is " + item.PurchaseDocAcceptanceId, "failure");
                        return true;
                    }
                    else if (item.Amount > item.Balance) {
                        isbreak = true;
                        ShowResult("Amount can not grater than Balance Amount where AcceptanceNo is " + item.PurchaseDocAcceptanceId, "failure");
                        return true;
                    }
                }
            })
        }
        if (isbreak == false) {
            return false;

        }
        else {

            return true;
        }

    };


    $scope.SelectedForAutoLoanList = [];
    $scope.autoLoanProcess = function () {
        $scope.SelectedForAutoLoanList = [];
        angular.forEach($scope.AutoLoanAvailableDataList, function (item) {
            if (item.isSelected == true) {
                $scope.SelectedForAutoLoanList.push(item);
            }
        })
    }

    $scope.Save = function () {
        $scope.autoLoanProcess();
        if (!$scope.validation()) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: "Commercial/AutoLoan/SaveAutoLoan",
                    data: {
                        "autoLoanData": $scope.SelectedForAutoLoanList,
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getAutoLoanAvailableList();

                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: "accounts/Invoice/UpdateVendorInvoice",
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
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
            }
            return true;
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.AutoLoanNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.AutoLoanNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.AutoLoanList.splice($scope.index, 1);
                    $scope.getSavedData();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
    }

    //$scope.SelectedForAutoLoanList = [];
    //$scope.rowChecked = function (args) {
    //    if (args.isInteraction == true) {
    //        var gridObj = $("#GridAcceptanceList").data("ejGrid");
    //        var data = gridObj.getSelectedRecords()[0];
    //        if (data.isSelected == true) {
    //            $scope.SelectedForAutoLoanList.push(data);
    //        }
    //        else {
    //            var taxdr = $scope.SelectedForAutoLoanList.length;
    //            while (taxdr--) {
    //                if ($scope.SelectedForAutoLoanList[taxdr]["Id"] === data.Id) {
    //                    $scope.SelectedForAutoLoanList.splice(taxdr, 1);
    //                }
    //            }
    //        }
    //        gridObj.refreshContent();
    //    }

    //};



    //$scope.removeRow = function (index) {
    //    var row = $scope.SelectedForAutoLoanList[index];
    //    for (var i = 0; i < $scope.AutoLoanAvailableDataList.length; i++) {
    //        if ($scope.AutoLoanAvailableDataList[i].Id == row.Id) {
    //            $scope.AutoLoanAvailableDataList[i].isSelected = false;
    //        }
    //    }
    //    $scope.SelectedForAutoLoanList.splice(index, 1);
    //};
}






