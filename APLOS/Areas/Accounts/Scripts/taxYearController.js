'use strict';
TaxYearController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function TaxYearController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Tax Year";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.isGet = false;
    $scope.taxYears = [];
    $scope.getListUrl = "accounts/taxYear/getlist/";
    baseService.init($scope.getListUrl, null, null, 'DESC', 'SortStartDate', 'TaxYearCode');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.taxYears = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.getData();
    $scope.searchtaxyearByList = [
        {
            'name': 'Code',
            'value': 'TaxYearCode'
        },
        {
            'name': 'Name',
            'value': 'TaxYearName'
        }
    ];

    $scope.taxYear = {
        Id: null,
        TaxYearCode: null,
        TaxYearName: null,
        StartDate: null,
        EndDate: null,
        IsSysGeneratedPeriod: true,
        IsPeriodCalendarWise: true,
        Active: true
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.taxYear = $scope.taxYears[$scope.index];
        $scope.Action = "Update";
        manualValidation('div_StartDate', false, 'This Month have already Used in another TaxYear');
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.IsPeriodOverlapping = function (startDate) {
        if ($scope.isGet === false && startDate !== undefined) {
            $http({
                method: 'GET',
                url: 'accounts/TaxYearPeriod/CheckingIsPeriodOverlapping?startDate=' + startDate
            }).then(function (response) {
                $scope.IsPeriodUsed = response.data;
                if ($scope.IsPeriodUsed) {
                    $scope.taxYear.StartDate = '';
                    manualValidation('div_StartDate', $scope.IsPeriodUsed, 'This Month have already Used in another TaxYear');
                }
                else {
                    manualValidation('div_StartDate', $scope.IsPeriodUsed, 'This Month have already Used in another TaxYear');
                    $scope.taxYear.StartDate = startDate;
                }
            });
        }
    };

    //$scope.IsPeriodOverlapping($scope.taxYear.StartDate);

    $scope.checkDate = function () {
        var invalidDocDate = false;
        if (new Date($scope.taxYear.StartDate) > new Date($scope.taxYear.EndDate)) {
            invalidDocDate = true;
            manualValidation('div_EndDate', invalidDocDate, 'End Date must bigger to Start Date');
            return invalidDocDate;
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.taxYearForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: "accounts/taxYear/create",
                    data: $scope.taxYear,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    } else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        baseService.paginationAdd();
                        ClearFields();
                    }
                });
                return true;
            } else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: "accounts/taxYear/edit",
                    data: $scope.taxYear,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    } else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.taxYears[$scope.index] = $scope.taxYear;
                        }
                        ClearFields();
                    }
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.taxYear.Id)) {
            $http({
                method: 'POST',
                url: "accounts/taxYear/delete?id=" + $scope.taxYear.Id,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                } else {
                    ShowResult(response.data.Message, "success");
                    $scope.taxYears.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
            },
                function (response) {
                    ShowResult(response.status.Message, "failure");
                });
        } else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.taxYear = {};
        $scope.taxYear.IsPeriodCalendarWise = true;
        $scope.taxYear.IsSysGeneratedPeriod = true;
        $scope.taxYear.Active = true;
    }
}