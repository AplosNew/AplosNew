'use strict';
FiscalYearController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function FiscalYearController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Fiscal Year';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.isGet = false;
    $scope.fiscalYears = [];
    $scope.getListUrl = 'accounts/fiscalyear/getlist/';
    baseService.init($scope.getListUrl, null, null, null, 'FiscalYearCode', 'FiscalYearCode');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.fiscalYears = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.searchfiscalyearByList = [
        {
            'name': 'Code',
            'value': 'FiscalYearCode'
        },
        {
            'name': 'Name',
            'value': 'FiscalYearName'
        }
    ];

    $scope.fiscalYear = {
        Id: null,
        FiscalYearCode: null,
        FiscalYearName: null,
        StartDate: $filter('dateFiltering')((new Date(), 'dd-MM-yyyy')),
        EndDate: $filter('dateFiltering')((new Date(), 'dd-MM-yyyy')),
        IsSysGeneratedPeriod: true,
        IsPeriodCalendarWise: true,
        Active: true
        , YearPrefix: null
    };

    $scope.checkValue = false;
    $scope.checkFiscalValue = function () {
        $http({
            method: 'GET',
            url: 'accounts/fiscalyear/CheckFiscalYearIsUsed?id=' + $scope.fiscalYear.Id
        }).then(function successCallback(response) {
            $scope.checkValue = response.data;
        });
    };

    $scope.Get = function (id, index) {
        $scope.checkFiscalValue();
        $scope.index = index;
        $scope.fiscalYear = $scope.fiscalYears[$scope.index];
        $scope.fiscalYear.StartDate = $filter('dateFiltering')($scope.fiscalYear.StartDate, 'dd-MM-yyyy');
        $scope.fiscalYear.EndDate = $filter('dateFiltering')($scope.fiscalYear.EndDate, 'dd-MM-yyyy');
        $scope.isGet = true;
        manualValidation('div_StartDate', false, 'This Month have already Used in another TaxYear')
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.checkDate = function () {
        var invalidDocDate = false;
        if (new Date($scope.fiscalYear.StartDate) > new Date($scope.fiscalYear.EndDate)) {
            invalidDocDate = true;
            manualValidation('div_EndDate', invalidDocDate, 'End Date must bigger to  Start Date');
            return invalidDocDate;
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.fiscalYearForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'accounts/fiscalyear/create',
                    data: $scope.fiscalYear,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        ClearFields();
                        $scope.getData();
                        baseService.paginationAdd();
                    }
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'accounts/fiscalyear/edit',
                    data: $scope.fiscalYear,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.fiscalYears[$scope.index] = $scope.fiscalYear;
                        }
                        ClearFields();
                    }
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.fiscalYear.Id)) {
            $http({
                method: 'POST',
                url: 'accounts/fiscalyear/delete?id=' + $scope.fiscalYear.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.fiscalYears.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.fiscalYear = {};
        $scope.fiscalYear.IsSysGeneratedPeriod = true;
        $scope.fiscalYear.IsPeriodCalendarWise = true;
        $scope.fiscalYear.Active = true;
        $scope.checkValue = false;
        $scope.isGet = false;
    }
}