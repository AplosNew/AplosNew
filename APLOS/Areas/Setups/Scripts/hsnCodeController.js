'use strict';
HSNCodeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function HSNCodeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'HSN Code';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.hSNCodes = [];
    $scope.path = 'Setups/hsncode/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.searchByHSNCodeList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Description',
            'value': 'Description'
        }
    ];
    $scope.HSNCodeListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Sequence',
        searchBy: "Code",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.paginationBase($scope.getListUrl, pageno, $scope.HSNCodeListParameters)
            .then(function (result) {
                $scope.hSNCodes = result.Rows;
                $scope.HSNCodeListParameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.hSNCode = {
        Id: null,
        Sequence: null,
        Code: null,
        Description: null,
    };

    $scope.hSNCodeNew = Object.assign({}, $scope.hSNCode);
    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl,
            function (data) {
                $scope.hSNCodeNew.Sequence = data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.hSNCode = $scope.hSNCodes[$scope.index];
        $scope.hSNCodeNew = Object.assign({}, $scope.hSNCode);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.hSNCodeNew, $scope.hSNCode);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.hSNCodeNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.hSNCode,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.hSNCodes.push(response.data.HSNCode);
                        $scope.hSNCodes = $filter('orderBy')($scope.hSNCodes, 'Sequence');
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
                    data: $scope.hSNCode,
                    dataType: 'JSON'
                }).then(function (response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.hSNCodes[$scope.index] = $scope.hSNCode;
                            $scope.hSNCodes = $filter('orderBy')($scope.hSNCodes, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function (response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.hSNCodeNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.hSNCodeNew.Id,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.hSNCodes.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            }, function (response) {
                ShowResult(response.data.Message, 'failure');
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.hSNCode = {};
        $scope.hSNCodeNew = {};
        $scope.hSNCodeNew.Sequence = seq;
        $scope.hSNCodeNew.Active = true;
    }
};