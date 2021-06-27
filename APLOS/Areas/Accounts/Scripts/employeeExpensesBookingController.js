'use strict';
employeeExpensesBookingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function employeeExpensesBookingController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'Employee Expenses Booking';
    $scope.Action = 'Save';
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.employeeTransactionTypeGLList = [];
    $scope.employeeTransactionTypeGLWithCombineList = [];
    $scope.path = 'accounts/EmployeeTransaction/';
    $scope.saveUrl = $scope.path + 'SaveEmployeeExpensesBooking';

    $scope.employeeTransactionTypeGL = {
        Id: null,
        CountryId: null,
        EmployeeTransactionTypeId: null,
        AdvanceGLId: null,
        PayableGLId: null,
        AdvanceBudgetId: null,
        AdvanceActivityId: null,
        PayableBudgetId: null,
        PayableActivityId: null,
        COAId: null,
        IsExpensesBooking: false
    };

    $scope.itemSearchPopup = function () {
        angular.element(document.querySelector('#itemsearchpopup')).modal('show');
    };

    $scope.employeeTransactionTypeList = [];
    $scope.COAList = [];
    cboService.getCboChartOfAccount('', function (result) {
        $scope.COAList = result;
    });

    $scope.showAll = function (str) {
        if (str === 'all') {
            if ($scope.employeeTransactionTypeGL.COAId === null) {
                return ShowResult('Select COA first', 'failure');
            }
            $scope.url = 'accounts/EmployeeTransaction/GetEmployeeTransactionTypeGLAllList?coaId=' + $scope.employeeTransactionTypeGL.COAId + '&countryId=' + $scope.employeeTransactionTypeGL.CountryId + '&inputOutput=' + $scope.employeeTransactionTypeGL.InputTaxOutPutTax;
        }
        $scope.employeeTransactionTypeGLWithCombineList = [];
        baseService.init($scope.url, null, null, null, 'EmployeeTransactionTypeName', 'EmployeeTransactionTypeName');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.employeeTransactionTypeGLWithCombineList = [];
                    angular.forEach(result.Rows, function (item) {
                        if (item.AdvanceGLId !== null || item.PayableGLId != null) {
                            $scope.employeeTransactionTypeGLWithCombineList.push(item);
                        }
                    })
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    function IdList() {
        $scope.employeeTransactionTypeIdstr = createIdList(validListWithStr($scope.employeeTransactionTypeClassNewList, $scope.fixedassetClassIds));
    }

    function createIdList(list) {
        var value = "''";
        for (var i = 0; i < list.length; i++) {
            if (value === "''") {
                value = "'" + list[i].Value + "'";
            } else {
                value += ",'" + list[i].Value + "'";
            }
        }
        return value;
    }

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    function getSavedData() {
        $scope.employeeTransactionTypeGLWithCombineSavedList = [];
        angular.forEach($scope.employeeTransactionTypeGLWithCombineList, function (item) {
            if (item.IsExpensesBooking) {
                $scope.employeeTransactionTypeGLWithCombineSavedList.push(item);
            }
        });
    }
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.employeeTransactionTypeGLForm.$valid) {
            getSavedData();
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'employeeExpBookings': $scope.employeeTransactionTypeGLWithCombineSavedList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        }
    };

    $scope.btnSet = '';
    $scope.setActiveBtn = function (str) {
        $scope.btnSet = str;
    };
    $scope.getAllWithCoa = function () {
        if ($scope.btnSet !== '') {
            if ($scope.btnSet === 'all') {
                $scope.getEmployeeTransactionTypeWithCoa('all');
            }
        } else {
            $scope.getEmployeeTransactionTypeWithCoa('all');
        }
    };

    $scope.clearGlField = function () {
        $scope.refreshAssetGL();
        $scope.refreshPayableGL();
        $scope.tempList = [];
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.employeeTransactionTypeGL = { COAId: $scope.employeeTransactionTypeGL.COAId };
        $scope.tempList = [];
        $scope.showAll('all');
        $scope.clearGlField();
        $scope.employeeTransactionTypeGLWithCombineList = [];
    }
}