'use strict';
InvoiceTaggedWithLCController.$inject = ['accountService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService', 'bankService', '$controller'];
function InvoiceTaggedWithLCController(accountService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService, bankService, $controller) {
    $rootScope.title = "Invoice Tagged With LC";
    $scope.Action = 'Save';
    $scope.path = 'Commercial/InvoiceTaggedWithLC/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveChargesUrl = $scope.path + 'CreateCharge';
    $scope.deleteUrl = $scope.path + 'delete/';

    //#region Page Loading ...
    $scope.AutoLoanAvailableDataList = [];
    $scope.fromDateTitle = "As On Date";
    $scope.toDateShow = false;
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);
    $scope.AutoLoan = {
        Id: null,
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now()),
        DateRange: "false",
    };
    $scope.AutoLoanNew = Object.assign({}, $scope.AutoLoan);
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

    $scope.getAutoLoanAvailableList = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetVendorAvailableInvoiceList?FromDate=" + $scope.AutoLoanNew.FromDate + '&ToDate=' + $scope.AutoLoanNew.ToDate + '&DateRange=' + $scope.AutoLoanNew.DateRange,
        }).then(function successCallback(response) {
            $scope.AutoLoanAvailableDataList = response.data;
        });
    }

    //#endregion

    //#region Clear
    $scope.Clear = function () {
        $scope.AutoLoan = {
            Id: null,
            FromDate: $filter('dateFiltering')(Date.now()),
            ToDate: $filter('dateFiltering')(Date.now()),
            DateRange: "false",
        };
        $scope.AutoLoanAvailableDataList = [];
        $scope.fromDateTitle = "As On Date";
    }
    //#endregion

    //#region Pop Up
    $scope.purchaseLCList = [];
    $scope.getSavedData = function () {
        $scope.purchaseLCList = [];
        $http.get("Commercial/PurchaseLC/getlist")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        angular.element(document.querySelector("#PurchaseLCPopUp")).modal("show");
                        $scope.purchaseLCList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.LcModel = [];
    $scope.SetDetails = function (args) {
        $scope.LcModel = [];
        angular.element(document.querySelector("#PurchaseLCPopUp")).modal("hide");
        $scope.LcModel.push(args.data);
    }

    //#endregion
}






