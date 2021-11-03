'use strict';
openingBalanceReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster','$window'];
function openingBalanceReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, $window) {
    $rootScope.title = 'Opening Balance Report';
    $scope.path = 'accounts/voucher/';
    $scope.path1 = 'Accounts/OpeningBalance/';
    $scope.glvoucherXLUrl = $scope.path + 'generateglvoucher';
    $scope.trialbalancereportXLUrl = $scope.path + 'trialbalancereport';
    $scope.generalVoucherXLUrl = 'accounts/voucher/generalvoucherreport';
    $scope.parallelCurrencyList = [];
    $scope.trialBalanceReport = {
        ToDate: $filter('dateFiltering')(Date.now()),
        ParallelCurrencyId: null
    };

    $scope.currencyIds = [];

    $http({
        method: 'GET',
        url: 'currencies/companyparallelcurrency/cboparallelcurrency'
    }).then(function successCallback(response) {
        $scope.parallelCurrencyList = response.data;
    });

    $scope.multiSelectSettings = {
        scrollableHeight: 'auto',
        smartButtonMaxItems: 3,
        scrollable: true,
        showCheckAll: false,
        showUncheckAll: false,
        enableSearch: false,

        dynamicTitle: true
    };

    $scope.CurrencyParallel = function () {
        $http({
            method: 'GET',
            url: 'currencies/CompanyParallelCurrency/CurrencyParallel'
        }).then(function successCallback(response) {
            $scope.CurrencyParallel = response.data.Rows;
            if ($scope.parallelCurrencyList.length === 1) {
                $scope.currencyIds.push($scope.parallelCurrencyList[0]);
            }
        });
        $scope.CheckParallelCurrencyValid();
    };

    function listOfCurrencyId(ids) {
        var list = [];
        for (var i = 0; i < ids.length; i++) {
            list.push(ids[i].Value)
        }
        return list;
    }

    $scope.openingBalanceReport = function () {
        if ($scope.currencyIds.length == 0) return ShowResult('Currency required', 'failure');
        location.href = 'accounts/openingbalance/OpeningBalanceReport?parallelCurrency=' + JSON.stringify(listOfCurrencyId($scope.currencyIds));
    };

 



    $scope.MaterialMasterOpeningBalanceExcel = function () {
        debugger;
       
        if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        var reportFormat = "Excel";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('accounts/openingbalance/MaterialMasterOpeningBalanceRpt?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate , '_blank'); //+ '&RcptIssue=' + $scope.detailModel.CostCenterId
      //  location.href = 'accounts/openingbalance/OpeningBalanceReport?parallelCurrency=' + JSON.stringify(listOfCurrencyId($scope.currencyIds));


    };



    //#region GateEntry Reginter 
    
    //Opening balalance
    $scope.OpeningBalanceList = [];
    $scope.GetOpeningBalane = function () {
        debugger;
        $http({
            method: 'GET',
            url: 'Accounts/OpeningBalance/OpeningBalanceLoadOnData'
        }).then(function successCallback(response) {
            $scope.OpeningBalanceList = response.data;

        });
    }
   
    $scope.GetOpeningBalane();

    $scope.OpeningBalanceReportExcel = function () {

        try {
            var file_src = $scope.path1 + "OpeningBalanceReportExcel";
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.OpeningBalanceReportPdf = function () {

        try {
            var file_src = $scope.path1 + "OpeningBalanceReportPdf";
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
   
}