'use strict';
positionAllowanceController.$inject = ['commonMessage', '$rootScope', '$scope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$compile', 'cboService', '$window'];
function positionAllowanceController(commonMessage, $rootScope, $scope, baseService, $routeParams, $location, $http, $filter, $compile, cboService, $window) {
    $rootScope.title = 'Position Allowance';
    $scope.Action = 'Save';
    $scope.dataList = [];
    $scope.fieldDataList = [];

    $scope.positionAllowance = {
        Id: null,
        PositionId: null,
        PositionName: null,
        CurrencyId: null,
        EffectiveDate: null,
        MinimumSalary: null,
        MaximumSalary: null,
        SkillAllowance: null,
        ResponsibilityAllowance: null,
        Active: true
    };

    cboService.getCompanyGroupCurrencyCbo(null, function (result) {
        $scope.currencyList = result;
    });

    $http.get('Organizations/CompanyGroup/GetCompanyGroupById/' + $window.companyGroupId)
        .then(function (response) {
            $scope.consolidateCurrencyId = response.data.ConsolidateCurrencyId;
        });

    $scope.searchByAllowanceList = [
        {
            'name': 'Currency',
            'value': 'CurrencyCode'
        },
        {
            'name': 'EffectiveDate',
            'value': 'EffectiveDate'
        },
        {
            'name': 'Minimum Salary',
            'value': 'MinimumSalary'
        },
        {
            'name': 'Maximum Salary',
            'value': 'MaximumSalary'
        },
        {
            'name': 'Skill Allowance',
            'value': 'SkillAllowance'
        },
        {
            'name': 'Responsibility Allowance',
            'value': 'ResponsibilityAllowance'
        }
    ];

    $scope.allowanceParameters = {
        limit: 10,
        offset: 0,
        order: 'DESC',
        sort: 'CONVERT(DATETIME, EffectiveDate, 106)',
        searchBy: 'CurrencyCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.positionAllowanceList = [];
    $scope.getData = function (pageno) {
        baseService.paginationBase('Organizations/Position/QueryAllowance?positionId=' + $scope.positionAllowance.PositionId, pageno, $scope.allowanceParameters)
            .then(function (result) {
                $scope.positionAllowanceList = result.Rows;
                $scope.allowanceParameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.Save = function () {
        if (!baseService.isUndefinedOrNull($scope.positionAllowance.MinimumSalary))
            if (parseInt($scope.positionAllowance.MaximumSalary) < parseInt($scope.positionAllowance.MinimumSalary)) {
                return ShowResult('Maximum salary must be greater than minimum salary.', 'failure');
            }
        var date = new Date($scope.positionAllowance.EffectiveDate).getDate();
        if (date > 1) {
            return ShowResult('Selected date must be 1st day of month.', 'failure');
        }
        if (parseFloat($scope.positionAllowance.SkillAllowance) + parseFloat($scope.positionAllowance.ResponsibilityAllowance) > $scope.positionAllowance.MaximumSalary) {
            return ShowResult('Total Skill and Responsibility Allowance can not greater than Maximum salary.', 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form1.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/Position/CreateAllowance',
                    data: { 'positionAllowance': $scope.positionAllowance },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.Clear();
                    }
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'Organizations/Position/EditAllowance',
                    data: { 'positionAllowance': $scope.positionAllowance },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.getData();
                        $scope.Clear();
                    }
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.positionAllowance.Id)) {
            $http({
                method: 'POST',
                url: 'Organizations/Position/DeleteAllowance',
                dataType: 'JSON',
                data: { 'id': $scope.positionAllowance.Id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    ClearFields();
                }
            });
        } else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.Get = function (data) {
        var positionName = $scope.positionAllowance.PositionName;
        $scope.positionAllowance = data;
        $scope.positionAllowance.PositionName = positionName;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
            $scope.Action = 'Update';
        }
    };

    //*********************** Position PopUp Start *************************************
    $scope.positionSearchList = [];
    $scope.positionDataList = [];
    $scope.positionSearch = [];
    $scope.positionParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'UserName',
        searchBy: 'Code',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.positionPopUp = function () {
        $scope.getPositionData = function (pageno) {
            baseService.paginationBase('Organizations/Position/GetList', pageno, $scope.positionParameters)
                .then(function (response) {
                    $scope.positionDataList = response.Rows;
                    $scope.positionParameters.total_count = response.Total;
                    if (baseService.arrayLength($scope.positionSearchList) === 0) {
                        baseService.getDDLSearchColumn($scope.positionDataList, $scope.positionSearchList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#positionPopUp')).modal('show');
        $scope.getPositionData();
    };

    $scope.closePositionPopUp = function () {
        angular.element(document.querySelector('#positionPopUp')).modal('hide');
    };

    $scope.selectPositionPopUp = function (data) {
        $scope.selectedPositionId = data.Id;
        $scope.positionAllowance.PositionId = $scope.selectedPositionId;
        $scope.positionAllowance.PositionName = data.UserName;
        $scope.positionAllowance.CurrencyId = $scope.consolidateCurrencyId;
        $scope.getData();
        angular.element(document.querySelector('#positionPopUp')).modal('hide');
    };
    //*********************** Position PopUp End *************************************

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        var positionId = $scope.positionAllowance.PositionId;
        var positionName = $scope.positionAllowance.PositionName;
        $scope.positionAllowance = {};
        $scope.positionAllowance.Active = true;
        $scope.positionAllowance.PositionId = positionId;
        $scope.positionAllowance.PositionName = positionName;
        $scope.positionAllowance.CurrencyId = $scope.consolidateCurrencyId;
    }
}