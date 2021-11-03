'use strict';
function ManagementChartofAccountEXController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Management Chart Of Account';
    $scope.Action = 'Save';
    $scope.coaRDList = [];
    $scope.col1List = [];
    $scope.col2List = [];
    $scope.col3List = [];
    $scope.index = -1;
    $scope.managementChartofAccountEXs = [];
    $scope.path = 'accounts/managementchartofaccount/';
    $scope.getListUrl = $scope.path + 'getmanagementchartofaccountlistbyespense';
    $scope.getUrl = $scope.path + 'get';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "APLOS1RDId", "APLOS1RDId");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.managementChartofAccountEXs = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $rootScope.searchByList = [
        {
            'name': 'COA Relation Data',
            'value': 'APLOS1RDId'
        },
        {
            'name': 'Expense Category',
            'value': 'Col1'
        },
        {
            'name': 'Expense SubCategory',
            'value': 'Col2'
        },
        {
            'name': 'Expense',
            'value': 'Col3'
        }
    ];

    $scope.managementChartofAccountEX = {
        Id: null,
        APLOS1RDId: null,
        Col1: null,
        Col2: null,
        Col3: null,
        UserName: null,
        ExpenseName: null,
        ExpenseCategoryName: null,
        ExpenseSubCategoryName: null,
        ResponsiblePerson: null,
        EffectiveDate: null,
        Type: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };

    $http({
        method: 'GET',
        url: 'accounts/chartofaccountrelationshipdata/chartofaccountrelationshipdatacbo'
    }).then(function successCallback(response) {
        $scope.coaRDList = response.data;
    });

    $http({
        method: 'GET',
        url: '/expenses/expensecategory/getexpensecategorylist'
    }).then(function successCallback(response) {
        $scope.col1List = response.data;
    });

    $http({
        method: 'GET',
        url: '/expenses/expensesubcategory/getexpensesubcategorylist'
    }).then(function successCallback(response) {
        $scope.col2List = response.data;
    });
    $http({
        method: 'GET',
        url: '/expenses/expense/getexpenselist'
    }).then(function successCallback(response) {
        $scope.col3List = response.data;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.managementChartofAccountEX = $scope.managementChartofAccountEXs[$scope.index];
        $scope.managementChartofAccountEX.AddedDate = $filter('dateFilter')($scope.managementChartofAccountEX.AddedDate);
        $scope.managementChartofAccountEX.UpdatedDate = $filter('dateFilter')($scope.managementChartofAccountEX.UpdatedDate);
        $scope.managementChartofAccountEX.EffectiveDate = $filter('dateFilter')($scope.managementChartofAccountEX.EffectiveDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.UserName = $("#APLOS1RDId option:selected").text();
        $scope.ExpenseCategoryName = $("#Col1 option:selected").text();
        $scope.ExpenseSubCategoryName = $("#Col2 option:selected").text();
        $scope.Expense = $("#Col3 option:selected").text();
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.managementChartofAccountEXForm.$valid) {
            if ($scope.Action == 'Save') {
                $scope.managementChartofAccountEX.Type = 'EX',
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.managementChartofAccountEX,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.managementChartofAccountEX = response.data.ManagementChartofAccount;
                            $scope.managementChartofAccountEX.UserName = $scope.UserName;
                            $scope.managementChartofAccountEX.ExpenseCategoryName = $scope.ExpenseCategoryName;
                            $scope.managementChartofAccountEX.ExpenseSubCategoryName = $scope.ExpenseSubCategoryName;
                            $scope.managementChartofAccountEX.Expense = $scope.Expense;
                            $scope.managementChartofAccountEXs.push($scope.managementChartofAccountEX)
                            baseService.paginationAdd();
                            ClearFields();
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.managementChartofAccountEX,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.managementChartofAccountEX.UserName = $scope.UserName;
                            $scope.managementChartofAccountEX.ExpenseCategoryName = $scope.ExpenseCategoryName;
                            $scope.managementChartofAccountEX.ExpenseSubCategoryName = $scope.ExpenseSubCategoryName;
                            $scope.managementChartofAccountEX.Expense = $scope.Expense;
                            $scope.managementChartofAccountEXs[$scope.index] = $scope.managementChartofAccountEX;
                        }
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.managementChartofAccountEX.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.managementChartofAccountEX.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.managementChartofAccountEXs.splice($scope.index, 1);
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
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.managementChartofAccountEX = {};
        $scope.managementChartofAccountEX.Active = true;
    }
};
ManagementChartofAccountEXController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];