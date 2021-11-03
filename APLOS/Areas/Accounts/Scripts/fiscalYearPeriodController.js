'use strict';
FiscalYearPeriodController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function FiscalYearPeriodController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.Action = 'Save';
    $scope.fiscalYearperiodList = [];
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
            'name': 'StartDate',
            'value': 'StartDate'
        },
        {
            'name': 'EndDate',
            'value': 'EndDate'
        }
    ];

    $scope.fiscalYearperiod = {
        Id: null,
        FiscalYearId: null,
        StartDate: $filter('dateFiltering')((new Date(), 'dd-MM-yyyy')),
        EndDate: $filter('dateFiltering')((new Date(), 'dd-MM-yyyy')),
        PeriodNo: 0,
        Description: null,
        Active: true,
        AddedDate: new Date()
    };
    $scope.fiscalYearList = [];
    $http({
        method: 'GET',
        url: 'accounts/FiscalYear/GetCbo'
    }).then(function successCallback(response) {
        $scope.fiscalYearList = response.data;
    });

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.fiscalYearPeriodForm.$valid) {
            $scope.fiscalYearperiodNew = angular.copy($scope.fiscalYearperiod);
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'accounts/fiscalyearperiod/create',
                    data: $scope.fiscalYearperiodNew,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.fiscalYearperiodList.push(response.data.FiscalYearPeriod);
                        baseService.paginationAdd();
                        ClearFields();
                        $scope.getData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'accounts/FiscalYearPeriod/Edit',
                    data: $scope.fiscalYearperiodNew,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.setIndex > -1) {
                            $scope.fiscalYearperiodList[$scope.setIndex] = $scope.fiscalYearperiod;
                            $scope.divisions = $filter('orderBy')($scope.fiscalYearperiod, 'StartDate');
                            ClearFields();
                        }
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
            return true;
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.fiscalYearperiod.Id)) {
            $http({
                method: 'POST',
                url: 'accounts/fiscalyearperiod/delete?id=' + $scope.fiscalYearperiod.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.fiscalYearperiodList.splice($scope.setIndex, 1);
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

    $scope.Get = function (id, index) {
        $scope.setIndex = index;
        $scope.fiscalYearperiod = $scope.fiscalYearperiodList[$scope.setIndex];
        $scope.fiscalYearperiod.StartDate = $filter('dateFiltering')($scope.fiscalYearperiod.StartDate, 'dd-MM-yyyy');
        $scope.fiscalYearperiod.EndDate = $filter('dateFiltering')($scope.fiscalYearperiod.EndDate, 'dd-MM-yyyy');
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.fiscalYearperiod = { FiscalYearId: $scope.fiscalYearList[0].Value };
    }
}