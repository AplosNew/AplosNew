'use strict';
GeneralAccountDeterminateController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function GeneralAccountDeterminateController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'General Account Determinate';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Accounts/GeneralAccountDeterminate/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "Id"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "General Account Determinate" }, { value: 'COA', name: "COA" }, { value: 'GLGeneralInfo', name: "GLGeneralInfo" }, { value: 'Budget', name: "Budget" }, { value: 'Activity', name: "Activity" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search, COAId: $scope.ModelNew.COAId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }


    $scope.AccountDeterminateList = [];
    cboService.getEnumCbo("enum/GetGeneralAccountDeterminateEnumCbo", function (result) {
        $scope.AccountDeterminateList = result;
    });

    $scope.COAList = [];
    cboService.getCboChartOfAccount('', function (result) {
        $scope.COAList = result;
    });

    $scope.ModelTemp = {
        Id: null,
        COAId: null,
        GLGeneralInfoId: null,
        BudgetMasterId: null,
        ActivityId: null

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GLGeneralInfo = $scope.ModelNew.GLGeneralInfo;
        getExpensesPayableBudget();
        $scope.getActivity();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.searchExpensesTypeByList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            'name': 'GL Name',
            'value': 'GLGeneralInfoName'
        }
    ];

    $scope.expensesTypeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName, GLGeneralInfoName',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getGLList = function () {
        if ($scope.ModelNew.COAId === null || $scope.ModelNew.COAId === undefined) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl1 = 'accounts/glitem/GetAllGLListSetup?coaId=' + $scope.ModelNew.COAId;
        $scope.getExpensesTypeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.expensesTypeListParameters)
                .then(function (data) {
                    $scope.expensesTypeGLList = data.Rows;
                    $scope.expensesTypeListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#expensesPayableTypeListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.getExpensesTypeListData();
    };
    $scope.closeExpensesPayableTypeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#expensesPayableTypeListPopUp')).modal('hide');
        }
    };

    $scope.setExpensesPayableGLSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.ExpensesGLSelectedData = x;
        $scope.GLGeneralInfo = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
        $scope.ModelNew.GLGeneralInfoId = x.GLGeneralInfoId;
        getExpensesPayableBudget();
    };
    $scope.refreshGL = function () {
        $scope.ExpensesPayableGLInof = null;
        $scope.ModelNew.GLGeneralInfoId = null;
        $scope.BudgetList = [];
        $scope.ActivityList = [];
        $scope.ModelNew.BudgetMasterId = null;
        $scope.ModelNew.ActivityId = null;
    };

    $scope.BudgetList = [];
    function getExpensesPayableBudget() {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.ModelNew.COAId, $scope.ModelNew.GLGeneralInfoId, function (result) {
            $scope.BudgetList = result;
        });
    };

    $scope.ActivityList = [];
    $scope.getActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.ModelNew.BudgetMasterId, function (result) {
            $scope.ActivityList = result;
        });
    };


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.COAId = $scope.ModelNew.COAId;
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.COAId = $scope.COAId;
        $scope.GLGeneralInfo = null;
    }
}