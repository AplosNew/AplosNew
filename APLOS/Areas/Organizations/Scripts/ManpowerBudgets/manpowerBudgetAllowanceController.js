'use strict';
manpowerBudgetAllowanceController.$inject = ['commonMessage', '$rootScope', '$scope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$compile', 'cboService', '$window'];
function manpowerBudgetAllowanceController(commonMessage, $rootScope, $scope, baseService, $routeParams, $location, $http, $filter, $compile, cboService, $window) {
    $rootScope.title = 'Manpower BudgetMaster Allowance';
    $scope.Action = 'Save';
    $scope.dataList = [];
    $scope.fieldDataList = [];
    $scope.PositionId = null;
    $scope.PositionCurrencyId = null;
    $scope.allowance = {
        Id: null,
        ManpowerBudgetId: null,
        ManpowerBudgetCode: null,
        CurrencyId: null,
        EffectiveDate: null,
        MinimumSalary: null,
        MaximumSalary: null,
        SkillAllowance: 0,
        ResponsibilityAllowance: 0,
        Active: true
    };
    $scope.allowanceNew = Object.assign({}, $scope.allowance);

    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    cboService.getCompanyGroupCurrencyCbo(null, function (result) {
        $scope.currencyList = result;
    });

    $scope.FromCurrencyCode = null;
    $scope.ToCurrencyCode = null;
    $scope.companyCurrencyId = null;
    $scope.currencyRate = 1;
    $scope.getCompanyCurrencyId = function () {
        $http.get('Organizations/Company/GetCompanyById/' + $scope.allowanceNew.CompanyId)
            .then(function (response) {
                $scope.companyCurrencyId = response.data.BaseCurrencyId;
                $scope.ToCurrencyCode = $.grep($scope.currencyList, function (item) {
                    return item.Value === $scope.companyCurrencyId;
                })[0].Text;
            });
    };

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

    $scope.currencyExchangeRate = function (id) {
        if ($scope.companyCurrencyId !== id) {
            $scope.FromCurrencyCode = $.grep($scope.currencyList, function (item) {
                return item.Value === id;
            })[0].Text;
            angular.element(document.querySelector('#currencyPopUp')).modal('show');
        }
    };

    $scope.closeCurrencyExchangeRate = function () {
        $scope.allowanceNew.MinimumSalary = $scope.allowanceNew.MinimumSalary * $scope.currencyRate;
        $scope.allowanceNew.MaximumSalary = $scope.allowanceNew.MaximumSalary * $scope.currencyRate;
        $scope.allowanceNew.SkillAllowance = $scope.allowanceNew.SkillAllowance * $scope.currencyRate;
        $scope.allowanceNew.ResponsibilityAllowance = $scope.allowanceNew.ResponsibilityAllowance * $scope.currencyRate;
        angular.element(document.querySelector('#currencyPopUp')).modal('hide');
    };

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
        baseService.paginationBase('Organizations/manpowerbudget/QueryAllowance?manpowerBudgetId=' + $scope.allowanceNew.ManpowerBudgetId, pageno, $scope.allowanceParameters)
            .then(function (result) {
                $scope.allowanceList = result.Rows;
                $scope.allowanceParameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope._validationMax = '';
    $scope.compareMax = false;
    $scope._validationMin = '';
    $scope.compareMin = false;

    $scope.compare = function (p1, p2, field) {
        if (baseService.isUndefinedOrNull(p1) && !baseService.isUndefinedOrNull(p2)) {
            $scope._validationMax = 'Maximum salary can not be null...';
            return $scope.compareMax = true;
        }
        if (p1 < p2) {
            if (field === 'Max') {
                $scope._validationMax = 'Must be greater than minimum salary...';
                return $scope.compareMax = true;
            }
            else {
                $scope._validationMin = 'Must be less than maximum salary...';
                return $scope.compareMin = true;
            }
        }
        else {
            $scope.compareMax = false;
            $scope.compareMin = false;
        }
    };

    $scope.Save = function () {
        if (!baseService.isUndefinedOrNull($scope.allowanceNew.MinimumSalary))
            if (parseInt($scope.allowanceNew.MaximumSalary) < parseInt($scope.allowanceNew.MinimumSalary)) {
                return ShowResult('Maximum salary must be greater than minimum salary.', 'failure');
            }
        var date = new Date($scope.allowanceNew.EffectiveDate).getDate();
        if (date > 1) {
            return ShowResult('Selected date must be 1st day of month.', 'failure');
        }
        if (parseFloat($scope.allowanceNew.SkillAllowance) + parseFloat($scope.allowanceNew.ResponsibilityAllowance) > $scope.allowanceNew.MaximumSalary) {
            return ShowResult('Total Skill and Responsibility Allowance can not greater than Maximum salary.', 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        angular.copy($scope.allowanceNew, $scope.allowance);
        if ($scope.form1.$valid) {
            $scope.allowanceNew.CurrencyId = $scope.companyCurrencyId;
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/manpowerbudget/CreateAllowance',
                    data: { 'allowance': $scope.allowance, 'rate': $scope.currencyRate },
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
                    url: 'Organizations/manpowerbudget/EditAllowance',
                    data: { 'allowance': $scope.allowance, 'rate': $scope.currencyRate },
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
        if (!baseService.isUndefinedOrNull($scope.companyStructureSetup.Id)) {
            $http({
                method: 'POST',
                url: 'Organizations/manpowerbudget/Delete/' + $scope.companyStructureSetup.Id,
                dataType: 'JSON'
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
        $scope.allowanceNew.Id = data.Id;
        $scope.allowanceNew.CurrencyId = data.CurrencyId;
        $scope.allowanceNew.EffectiveDate = $filter('dateFiltering')(data.EffectiveDate);
        $scope.allowanceNew.MinimumSalary = data.MinimumSalary;
        $scope.allowanceNew.MaximumSalary = data.MaximumSalary;
        $scope.allowanceNew.SkillAllowance = data.SkillAllowance;
        $scope.allowanceNew.ResponsibilityAllowance = data.ResponsibilityAllowance;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
            $scope.Action = 'Update';
        }
    };

    //*********************** Manpower Budget PopUp Start *************************************
    $scope.manpowerBudgetSearchList = [];
    $scope.manpowerBudgetDataList = [];
    $scope.manpowerBudgetSearch = [];
    $scope.manpowerBudgetParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'Code',
        searchBy: 'Code',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.manpowerBudgetPopUp = function () {
        $scope.getManpowerBudgetData = function (pageno) {
            if ($scope.allowanceNew.CompanyId === null) {
                ShowResult('Please select company!', 'failure');
            }
            $scope.manpowerBudgetParameters.companyId = $scope.allowanceNew.CompanyId;
            baseService.paginationBase('Organizations/manpowerbudget/getlist', pageno, $scope.manpowerBudgetParameters)
                .then(function (response) {
                    $scope.manpowerBudgetDataList = response.Rows;
                    $scope.manpowerBudgetParameters.total_count = response.Total;
                    if (baseService.arrayLength($scope.manpowerBudgetSearchList) === 0) {
                        baseService.getDDLSearchColumn($scope.manpowerBudgetDataList, $scope.manpowerBudgetSearchList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#manpowerBudgetPopUp')).modal('show');
        $scope.getManpowerBudgetData();
    };

    $scope.closeManpowerBudgetPopUp = function () {
        $scope.entityId = '';
        $scope.EntityName = '';
        angular.element(document.querySelector('#manpowerBudgetPopUp')).modal('hide');
    };

    $scope.selectmanpowerBudgetPopUp = function (data) {
        $scope.selectedManpowerBudgetId = data.Id;
        $scope.allowanceNew.ManpowerBudgetId = data.Id;
        $scope.allowanceNew.ManpowerBudgetCode = data.Code;
        $scope.PositionId = data.PositionId;
        $scope.allowanceNew.CurrencyId = $scope.companyCurrencyId;
        $scope.getData();
        angular.element(document.querySelector('#manpowerBudgetPopUp')).modal('hide');
    };

    $scope.clearManpowerBudget = function () {
        $scope.selectedManpowerBudgetId = null;
        $scope.positionData = [];
        $scope.positionSearch = [];
    };
    //*********************** Manpower Budget PopUp End ******************************************

    //*********************** Position Allowance PopUp Start *************************************
    $scope.positionAllowanceSearchList = [];
    $scope.positionAllowanceDataList = [];
    $scope.positionAllowanceSearch = [];
    $scope.positionAllowanceParameters = {
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

    $scope.positionAllowancePopUp = function () {
        $scope.getPositionAllowanceData = function (pageno) {
            baseService.paginationBase('Organizations/Position/QueryAllowance?positionId=' + $scope.PositionId, pageno, $scope.positionAllowanceParameters)
                .then(function (response) {
                    $scope.positionAllowanceDataList = response.Rows;
                    $scope.positionAllowanceParameters.total_count = response.Total;
                    if (baseService.arrayLength($scope.positionAllowanceSearchList) === 0) {
                        baseService.getDDLSearchColumn($scope.positionAllowanceDataList, $scope.positionAllowanceSearchList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#positionAllowancePopUp')).modal('show');
        $scope.getPositionAllowanceData();
    };

    $scope.closePositionAllowancePopUp = function () {
        angular.element(document.querySelector('#positionAllowancePopUp')).modal('hide');
    };

    $scope.selectPositionAllowancePopUp = function (data) {
        $scope.selectedAllowancePositionId = data.Id;
        $scope.PositionCurrencyId = data.CurrencyId;
        //$scope.allowance.CurrencyId = data.CurrencyId;
        $scope.allowanceNew.EffectiveDate = data.EffectiveDate;
        $scope.allowanceNew.MinimumSalary = data.MinimumSalary;
        $scope.allowanceNew.MaximumSalary = data.MaximumSalary;
        $scope.allowanceNew.SkillAllowance = data.SkillAllowance;
        $scope.allowanceNew.ResponsibilityAllowance = data.ResponsibilityAllowance;
        angular.element(document.querySelector('#positionAllowancePopUp')).modal('hide');
        $scope.currencyExchangeRate($scope.PositionCurrencyId);
    };

    $scope.clearPositionAllowance = function () {
        $scope.selectedPositionId = null;
        $scope.positionAllowanceData = [];
        $scope.positionAllowanceSearch = [];
    };
    //*********************** Position Allowance PopUp End *************************************

    $scope.effectiveDateId = null;
    $scope.effectiveDateIndex = -1;
    $scope.valuePassInAllowanceDelModal = function (data, index) {
        $scope.effectiveDateId = data.Id;
        $scope.effectiveDateTempInfo = data;
        $scope.effectiveDateIndex = index;
        $scope.message_confirmation = 'Are you sure want to untag GL on [ ' + data.EffectiveDate + ' ]?';
        angular.element(document.querySelector('#confirmgenericAllowancePopUp')).modal('show');
    };
    $scope.removeAllowanceRow = function () {
        for (var i = 0; i < $scope.positionAllowanceList.length; i++) {
            if ($scope.positionAllowanceList[i].Id === null && $scope.positionAllowanceList[i].EffectiveDate === $scope.effectiveDateTempInfo.EffectiveDate && $scope.positionAllowanceList[i].CurrencyId === $scope.effectiveDateTempInfo.CurrencyId && $scope.positionAllowanceList[i].MinimumSalary === $scope.effectiveDateTempInfo.MinimumSalary && $scope.positionAllowanceList[i].MaximumSalary === $scope.effectiveDateTempInfo.MaximumSalary$scope.positionAllowanceList[i].SkillAllowance === $scope.effectiveDateTempInfo.SkillAllowance) {
                $scope.positionAllowanceList.splice($scope.effectiveDateIndex, 1);
            }
            else if ($scope.positionAllowanceList[i].Id !== null && $scope.positionAllowanceList[i].Id === $scope.effectiveDateId)
                $scope.deleteAllowance($scope.effectiveDateId, i);
        }
        $scope.mauid = null;
        $scope.mauindex = -1;
    };

    $scope.deleteAllowance = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: 'Organizations/ManpowerBudget/DeleteAllowance',
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    for (var i = 0; i < $scope.positionAllowanceList.length; i++) {
                        if ($scope.positionAllowanceList[i].Id === id) {
                            $scope.positionAllowanceList.splice(i, 1);
                            break;
                        }
                    }
                    $scope.glUntagIndex = -1;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        var companyId = $scope.allowanceNew.CompanyId;
        var id = $scope.allowanceNew.ManpowerBudgetId;
        var code = $scope.allowanceNew.ManpowerBudgetCode;
        $scope.allowance = {};
        $scope.allowanceNew = {};
        $scope.allowanceNew.CompanyId = companyId;
        $scope.allowanceNew.Active = true;
        $scope.currencyRate = 1;
    }
}