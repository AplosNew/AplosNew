'use strict';
SalaryRuleSpecialAllowanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SalaryRuleSpecialAllowanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Special Allowance/Bonus;
    $scope.Action = 'Save';
    $scope.path = 'Payrolls/EmployeeSalaryRuleSetup/';
    $scope.getListUrl = $scope.path + 'gesptlist';
    $scope.saveUrl = $scope.path + 'createsp';
    $scope.deleteUrl = $scope.path + 'deletesp/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "SalaryHead"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'SalaryHead', name: "SalaryHead" }, { value: 'BudgetCode', name: "BudgetCode" }, { value: 'Remarks', name: "Remarks" }];

    $scope.ModelList = [];
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetSPList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        SalaryHead: null,
        SalaryHeadId: null,
        ManpowerBudgetId: null,
        BudgetCode: null,
        Days:0,
        Remarks: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

      $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "Code",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUp = function (name) {
        $scope.name = name;
        //if ($scope.name === 'Budget') {
        $scope.popUpDataList = [];
        $scope.popUpList = [];
        $scope.popUpParameters.sort = 'Code';
        $scope.popUpParameters.searchBy = 'Code';
        $scope.popUpUrl = 'employees/recruitment/getbudgetcodelist';
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                    $scope.popUpParameters.sort = 'Code';
                    $scope.popUpParameters.searchBy = 'Code';
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();


    };

    $scope.name = null;
    $scope.selectDoubleClick = function (data) {
        $scope.ModelNew.ManpowerBudgetId= data.Id;
        $scope.ModelNew.BudgetCode  = data.Code;
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    $scope.SalaryHeadList =
        $scope.GetSalaryHeadCbo = function () {
            $http({
                method: 'Get',
                url: "Payrolls/EmployeeSalaryRuleSetup/GetCbo",
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.SalaryHeadList = response.data;
            })
        }
    $scope.GetSalaryHeadCbo();

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
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }
}