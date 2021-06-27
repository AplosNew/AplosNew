'use strict';
attendanceGroupController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function attendanceGroupController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.attendanceGroupList = [];
    $scope.path = 'Setups/AttendanceGroup/';
    $scope.getUrl = $scope.path + 'get';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.attendanceGroup = {
        Id: null,
        Sequence: null,
        StandardName: null,
        UserName: null,
        Group1: null,
        Group2: null,
        Group3: null,
        BudgetedManPower: null
    };
    $scope.attendanceGroupNew = Object.assign({}, $scope.attendanceGroup);

   
    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.attendanceGroupNew.Sequence = data;
        });
    };
    $scope.GetSequence();
    
    baseService.init($scope.getListUrl);
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.attendanceGroupList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();

    $scope.searchEmployeeByList = [
        {
            name: 'Sequence',
            value: 'Sequence'
        },
        {
            name: 'Standard Name',
            value: 'StandardName'
        },
        {
            name: 'User Define Name',
            value: 'UserName'
        },
        {
            name: 'Department',
            value: 'Department'
        }
    ];
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.attendanceGroup = $scope.attendanceGroupList[$scope.index];
        $scope.attendanceGroupNew = Object.assign({}, $scope.attendanceGroup);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        angular.copy($scope.attendanceGroupNew, $scope.attendanceGroup);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.attendanceGroupForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.attendanceGroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.attendanceGroupList.push(response.data.AttendanceGroup);
                        $scope.attendanceGroupList = $filter('orderBy')($scope.attendanceGroupList, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields();
                        $scope.GetSequence();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.attendanceGroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                        $scope.getData();

                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            //$scope.attendanceGroupList[$scope.index] = $scope.attendanceGroup;
                            $scope.attendanceGroupList = $filter('orderBy')($scope.attendanceGroupList, 'Sequence');
                        }
                        $scope.getData();
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
        if (!baseService.isUndefinedOrNull($scope.attendanceGroupNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.attendanceGroupNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.attendanceGroupList.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
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
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.attendanceGroup = {};
        $scope.attendanceGroupNew = {  };
        $scope.attendanceGroup.Sequence = seq;
    }
}