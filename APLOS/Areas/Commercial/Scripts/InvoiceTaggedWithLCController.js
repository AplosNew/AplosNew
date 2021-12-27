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
    $scope.getpurchaseLCListData = function () {
        $scope.purchaseLCList = [];
        $http.get("Commercial/InvoiceTaggedWithLC/purchaseLCList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        
                        $scope.purchaseLCList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getpurchaseLCListData();

    $scope.getSavedData = function () {
        angular.element(document.querySelector("#PurchaseLCPopUp")).modal("show");
    }

    $scope.LcModel = {};
    $scope.SetDetails = function (args) {
        $scope.LcModel = Object.assign({}, args.data);
        angular.element(document.querySelector("#PurchaseLCPopUp")).modal("hide");
    }

    //#endregion

    //#region Save

    $scope.Save = function () {
        try {
            var SaveList = [];
            for (var i = 0; i < $scope.AutoLoanAvailableDataList.length; i++) {
                if ($scope.AutoLoanAvailableDataList[i].isSelected) {
                    SaveList.push($scope.AutoLoanAvailableDataList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'DataList': SaveList, 'LcData': $scope.LcModel},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.TaxPolicyMaster.SystemID = response.data.Data.SystemID;
                    $scope.TaxPolicyMaster.TaxYearID = response.data.Data.TaxYearID;
                    $scope.getTaxMonth($scope.TaxPolicyMaster.TaxYearID);
                    $scope.getMaster();
                    $scope.getIncome($scope.TaxPolicyMaster.SystemID);


                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    //#endregion

    //#region SaveDataList

    $scope.SaveDataList = [];
    $scope.getData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetSaveData",
        }).then(function successCallback(response) {
            $scope.SaveDataList = response.data;
        });
    }
    $scope.getData();

    //#endregion

}






