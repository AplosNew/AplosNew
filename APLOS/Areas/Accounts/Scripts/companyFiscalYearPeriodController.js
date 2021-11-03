'use strict';
CompanyFiscalYearPeriodController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CompanyFiscalYearPeriodController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Update';
    $scope.companyFiscalYearPeriod = {
        Id: null,
        CompanyFiscalYearId: null,
        FiscalYearPeriodId: null,
        StartDate: $filter('dateFiltering')((new Date(), 'dd-MM-yyyy')),
        EndDate: $filter('dateFiltering')((new Date(), 'dd-MM-yyyy')),
        CompanyId: null,
        IsBudgetLocked: false,
        IsTransationLocked: false,
        IsExchangeRateConfirmed: false,
        Active: true
    };

    $scope.onCategoryChange = function (item) {
        $scope.tableShow = true;
        GetAll(item);
    };

    $scope.UpdateTransationLocked = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/CompanyFiscalYearPeriod/UpdateTransationLocked?id=' + id
        }).then(function successCallback(response) {
        });
    };

    $scope.UpdateBudgetLocked = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/CompanyFiscalYearPeriod/UpdateBudgetLocked?id=' + id
        }).then(function successCallback(response) {
        });
    };

    $scope.UpdateExchangeRateConfirmed = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/CompanyFiscalYearPeriod/UpdateExchangeRateConfirmed?id=' + id
        }).then(function successCallback(response) {
        });
    };

    $scope.fiscalYearList = [];
    $scope.getFiscalYear = function (companyId) {
        $http({
            method: 'GET',
            url: 'accounts/CompanyFiscalYear/getcboWithComId?companyId=' + companyId
        }).then(function successCallback(response) {
            $scope.fiscalYearList = response.data;
            });
        if ($scope.companyFiscalYearPeriod.FiscalYearId !=null)
        $scope.getComFiscalYearPeriod(companyId, $scope.companyFiscalYearPeriod.FiscalYearId)
    };

    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.getComFiscalYearPeriod = function (companyId, comfiscalYearId) {
        $http({
            method: 'GET',
            url: 'accounts/CompanyFiscalYearPeriod/getCompiscalYearPeriodListWithComPiscalYear?companyId=' + companyId + '&comfiscalyear=' + comfiscalYearId
        }).then(function successCallback(response) {
            $scope.companyFiscalYearPeriods = response.data;
        });
    };

    $scope.Save = function () {
        $scope.newList = [];
        angular.forEach($scope.companyFiscalYearPeriods, function (item) {
            $scope.newList.push(
                {
                    Id: item.Id,
                    FiscalYearPeriodId: item.FiscalYearPeriodId,
                    CompanyFiscalYearId: item.CompanyFiscalYearId,
                    CompanyId: item.CompanyId,
                    IsBudgetLocked: item.IsBudgetLocked,
                    IsTransationLocked: item.IsTransationLocked,
                    IsExchangeRateConfirmed: item.IsExchangeRateConfirmed,
                    Active: true
                }
            );
        });

        $scope.companyFiscalYearPeriod.AddedDate = $filter("date")(Date.now(), 'yyyy-MM-dd');
        $scope.companyFiscalYearPeriod.UpdatedDate = null;
        if ($scope.companyFiscalYearPeriod.FiscalYearId !== null) {
            $http({
                method: 'POST',
                url: "accounts/CompanyFiscalYearPeriod/Save",
                data: {
                    'comFYPeriod': $scope.newList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    ClearFields();
                }
            });
            if ($scope.tableShow) GetAll();
            return true;
        } else {
            ShowResult("Select Fiscal Year Period", 'failure');
        }
    };

    function ClearFields() {
        $scope.companyFiscalYearPeriod.Id = "";
        $scope.tableShow = false;
    }

    function GetAll(item) {
        $http.get('accounts/CompanyFiscalYearPeriod/ComFYDataSearch?comfiscalyear=' + item)
            .then(function (response) {
                $scope.companyFiscalYearPeriods = response.data.Rows;
            });
    }
}