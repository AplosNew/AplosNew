'use strict';
fiscalYearBudgetController.$inject = ['cboService', '$route', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function fiscalYearBudgetController(cboService, $route, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.Action = 'Save';
    $rootScope.title = "Montly Budget";
    $scope.fiscalYearList = [];
    $scope.companyList = [];
    $scope.getListUrl = 'accounts/fiscalyearperiod/getfiscalyearperiodlist';
    baseService.init($scope.getListUrl, null, 12, null, 'StartDate', 'StartDate');
    $scope.getData = function (pageno) {
        $rootScope.parameters.fiscalYearId = $scope.fiscalYearperiod.FiscalYearId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.fiscalYearperiodList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.searchByList = [
        {
            'name': "Company Name",
            'value': "UserName"
        },
        {
            'name': "FiscalYear",
            'value': "FiscalYearName"
        }
    ];

    $http({
        method: 'GET',
        url: 'accounts/fiscalyear/getcbo'
    }).then(function successCallback(response) {
        $scope.fiscalYearList = response.data;
    });

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.fiscalYearperiod = {
        Id: null,
        FiscalYearId: null,
        CompanyId: null,
        Active: true,
        PeriodId:null,
        AddedBy: null,
        AddedDate: $filter("date")(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null,
        UpdatedDate: $filter("date")(Date.now(), 'yyyy-MM-dd'),
        ReportFormat: 'Excel'
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $http.get("accounts/CompanyFiscalYear/GetCompanyFiscalYear/" + id)
            .then(function (response) {
                $scope.companyFiscalYear = response.data;
            });
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

       $scope.jobcardreportFunc= function () {
            try {
                if (baseService.isUndefinedOrNull($scope.fiscalYearperiod.PeriodId)) {
                    throw 'Please Select Period';
                }

                else if (baseService.isUndefinedOrNull($scope.fiscalYearperiod.FiscalYearId)) {
                    throw 'Please Select Fiscal Year';
                }

                else if ($scope.fiscalYearperiod.ReportFormat === 'Excel') {
                       
                    var url = 'accounts/BudgetMaster/GetFiscalYearBudgetReport?reportFormat=' + $scope.fiscalYearperiod.ReportFormat + '&fiscalYearPeriodId=' + $scope.fiscalYearperiod.PeriodId ;
                            $rootScope.report(url);                    
                    }
                
                else if ($scope.fiscalYearperiod.ReportFormat === 'Pdf') {                     
                    var url = 'accounts/BudgetMaster/GetFiscalYearBudgetReport?reportFormat=' + $scope.fiscalYearperiod.ReportFormat + '&fiscalYearPeriodId=' + $scope.fiscalYearperiod.PeriodId ;
                            $rootScope.report(url);
                    }
            } catch (e) {
                ShowResult(e, 'failure');

            }
    };

}