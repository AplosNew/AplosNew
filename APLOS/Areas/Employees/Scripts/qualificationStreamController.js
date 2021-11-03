'use strict';
QualificationStreamController.$inject = ['commonMessage', 'baseService', '$scope', '$rootScope', '$routeParams', '$location', '$http', '$filter'];
function QualificationStreamController(commonMessage, baseService, $scope, $rootScope, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Qualification Stream';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.qualificationStreams = [];
    $scope.path = 'employees/qualificationstream/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    baseService.init($scope.getListUrl, null, null, null, 'UserName', 'UserName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.qualificationStreams = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.qualificationStream = {
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
    $scope.qualificationStreamNew = Object.assign({}, $scope.qualificationStream);
    $scope.searchByList = [
        {
            'name': 'Code',

            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        }
    ];
    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.qualificationStreamNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.qualificationStream = $scope.qualificationStreams[$scope.index];
        $scope.qualificationStreamNew = Object.assign({}, $scope.qualificationStream);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        angular.copy($scope.qualificationStreamNew, $scope.qualificationStream);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.qualificationStreamNewForm.$valid) {
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.qualificationStream,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.qualificationStreams.push(response.data.QualificationStream);
                        $scope.qualificationStreams = $filter('orderBy')($scope.qualificationStreams, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.qualificationStream,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.qualificationStreams[$scope.index] = $scope.qualificationStream;
                            $scope.qualificationStreams = $filter('orderBy')($scope.qualificationStreams, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.qualificationStreamNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.qualificationStreamNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.qualificationStreams.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    }
    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.qualificationStream = {};
        $scope.qualificationStreamNew = { Sequence: seq, Active: true};
    }
}