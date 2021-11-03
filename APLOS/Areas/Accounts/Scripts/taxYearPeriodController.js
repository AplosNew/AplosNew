'use strict';
TaxYearPeriodController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function TaxYearPeriodController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Tax TaxYear";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.taxYearPeriods = [];
    $scope.path = 'accounts/taxyearperiod/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getListUrl = $scope.path + 'gettaxyearperiodlistwithyear';
    baseService.init($scope.getListUrl, null, 12, null, 'PeriodNo', 'StartDate');
    $scope.getData = function (pageno) {
        $rootScope.parameters.taxYearId = $scope.taxYearperiod.TaxYearId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.taxYearPeriods = result.Rows;
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

    $scope.taxYearperiod = {
        Id: null,
        TaxYearId: null,
        PeriodNo: null,
        PeriodName: null,
        StartDate: null,
        EndDate: null,
        Description: null,
        Active: true,
        AddedDate: new Date()
    };

    cboService.getTaxYearCbo(null, function (result) {
        $scope.TaxYearList = result;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.taxYearperiod = $scope.taxYearPeriods[$scope.index];
        $scope.taxYearperiod.AddedDate = $filter('dateFilter')($scope.taxYearperiod.AddedDate);
        $scope.taxYearperiod.UpdatedDate = $filter('dateFilter')($scope.taxYearperiod.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.taxYearPeriodForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.taxYearperiod,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    } else {
                        ShowResult(response.data.Message, 'success');
                        $scope.taxYearPeriods.push(response.data.taxYearPeriod);
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                },
                    function (response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                return true;
            } else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.taxYearperiod,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    } else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.taxYearPeriods[$scope.index] = $scope.taxYearperiod;
                        }
                        ClearFields(data.Sequence);
                    }
                },
                    function (response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                return true;
            }
        }
        return true;
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.taxYearperiod.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.taxYearperiod.Id,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                } else {
                    ShowResult(response.data.Message, 'success');
                    $scope.taxYearPeriods.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
            },
                function (response) {
                    ShowResult(response.status.Message, 'failure');
                });
        } else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.taxYearperiod = {};
        $scope.taxYearperiod.TaxYearId = $scope.taxYearPeriods[$scope.index];
        $scope.taxYearperiod.Active = true;
    }
}