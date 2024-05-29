'use strict';
employeeTransactionTypeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function employeeTransactionTypeController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'Employee Transaction Type';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.employeeTransactionTypes = [];
    $scope.path = 'accounts/EmployeeTransaction/';
    $scope.getListUrl = $scope.path + 'getEmployeeTransactionTypelist';
    $scope.getSeqUrl = $scope.path + 'getEmployeeTransactionTypeautosequence';
    $scope.saveUrl = $scope.path + 'createEmployeeTransactionType';
    $scope.updateUrl = $scope.path + 'editEmployeeTransactionType';
    $scope.deleteUrl = $scope.path + 'deleteEmployeeTransactionType/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.employeeTransactionTypes = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.employeeTransactionType = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        AdvanceType: "General"
    };

    $scope.employeeTransactionTypeNew = Object.assign({}, $scope.employeeTransactionType);

 

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.employeeTransactionTypeNew.Sequence = data;
        })
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.employeeTransactionType = $scope.employeeTransactionTypes[$scope.index];
        $scope.employeeTransactionTypeNew = Object.assign({}, $scope.employeeTransactionType);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.employeeTransactionTypeNew, $scope.employeeTransactionType);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.employeeTransactionTypeNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.employeeTransactionType,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.employeeTransactionTypes.push(response.data.EmployeeTransactionType);
                        $scope.employeeTransactionTypes = $filter('orderBy')($scope.employeeTransactionTypes, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.employeeTransactionType,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.employeeTransactionTypes[$scope.index] = $scope.employeeTransactionType;
                            $scope.employeeTransactionTypes = $filter('orderBy')($scope.employeeTransactionTypes, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.employeeTransactionTypeNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.employeeTransactionTypeNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.employeeTransactionTypes.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
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

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.employeeTransactionType = {};
        $scope.employeeTransactionTypeNew = {};
        $scope.employeeTransactionTypeNew.Sequence = seq;
        $scope.employeeTransactionTypeNew.Active = true;
    }
}