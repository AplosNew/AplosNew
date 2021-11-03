'use strict';
CompanyTaxYearPeriodController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CompanyTaxYearPeriodController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Update';
    $scope.companyTaxYearPeriod = {
        Id: null,
        CompanyTaxYearId: null,
        TaxYearPeriodId: null,
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
            url: 'accounts/CompanyTaxYearPeriod/UpdateTransationLocked?id=' + id
        }).then(function (response) {
        });
    };

    $scope.UpdateBudgetLocked = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/CompanyTaxYearPeriod/UpdateBudgetLocked?id=' + id
        }).then(function successCallback(response) {
        });
    };

    $scope.UpdateExchangeRateConfirmed = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/CompanyTaxYearPeriod/UpdateExchangeRateConfirmed?id=' + id
        }).then(function successCallback(response) {
        });
    };

    $scope.taxYearList = [];
    $scope.getTaxYear = function (companyId) {
        $http({
            method: 'GET',
            url: 'accounts/CompanyTaxYear/GetCompanyTaxYearCbo?id=' + companyId
        }).then(function successCallback(response) {
            $scope.taxYearList = response.data;
            });
        if ($scope.companyTaxYearPeriod.TaxYearId !=null)
        $scope.getComTaxYearPeriod(companyId, $scope.companyTaxYearPeriod.TaxYearId)
    };

    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.getComTaxYearPeriod = function (companyId, comTaxYearId) {
        $http({
            method: 'GET',
            url: 'accounts/CompanyTaxYearPeriod/GetCompiscalYearPeriodListWithComPiscalYear?companyId=' + companyId + '&comtaxyear=' + comTaxYearId
        }).then(function successCallback(response) {
            $scope.companyTaxYearPeriods = response.data;
        });
    };

    $scope.Save = function () {
        $scope.newList = [];
        angular.forEach($scope.companyTaxYearPeriods, function (item) {
            $scope.newList.push(
                {
                    Id: item.Id,
                    TaxYearPeriodId: item.TaxYearPeriodId,
                    CompanyTaxYearId: item.CompanyTaxYearId,
                    CompanyId: item.CompanyId,
                    IsBudgetLocked: item.IsBudgetLocked,
                    IsTransationLocked: item.IsTransationLocked,
                    IsExchangeRateConfirmed: item.IsExchangeRateConfirmed,
                    Active: true
                }
            );
        });

        $scope.companyTaxYearPeriod.AddedDate = $filter("date")(Date.now(), 'yyyy-MM-dd');
        $scope.companyTaxYearPeriod.UpdatedDate = null;
        if ($scope.companyTaxYearPeriod.TaxYearId !== null) {
            $http({
                method: 'POST',
                url: "accounts/CompanyTaxYearPeriod/Save",
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
            ShowResult("Select Tax Year Period", 'failure');
        }
        return true;
    };

    function ClearFields() {
        $scope.companyTaxYearPeriod.Id = "";
        $scope.tableShow = false;
    }

    function GetAll(item) {
        $http.get('accounts/CompanyTaxYearPeriod/ComTYDataSearch?comtaxyear=' + item)
            .then(function (response) {
                $scope.companyTaxYearPeriods = response.data.Rows;
            });
    }
}