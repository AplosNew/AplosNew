'use strict';
employeeWorkTypeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function employeeWorkTypeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee WorkType';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.employeeWorkTypes = [];
    $scope.path = 'employees/employeeworktype/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.employeeWorkTypes = result;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.employeeWorkType = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };

    $scope.employeeWorkTypeNew = Object.assign({}, $scope.employeeWorkType);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.employeeWorkTypeNew.Sequence = data[0].Sequence;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.employeeWorkType = $scope.employeeWorkTypes[$scope.index];
        $scope.employeeWorkTypeNew = Object.assign({}, $scope.employeeWorkType);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.employeeWorkTypeNew, $scope.employeeWorkType);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.employeeWorkTypeNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.employeeWorkType,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.employeeWorkTypes.push(response.data.employeeWorkType);
                        $scope.employeeWorkTypes = $filter('orderBy')($scope.employeeWorkTypes, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields();
                        $scope.getData();
                        $scope.GetSequence();


                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.employeeWorkType,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.employeeWorkTypes[$scope.index] = $scope.employeeWorkType;
                            $scope.employeeWorkTypes = $filter('orderBy')($scope.employeeWorkTypes, 'Sequence');
                        }
                        ClearFields();
                        $scope.GetSequence();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.employeeWorkTypeNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.employeeWorkTypeNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.employeeWorkTypes.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                    $scope.GetSequence();
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
        $scope.employeeWorkType = {};
        $scope.employeeWorkTypeNew = { Active:true };
    }
}