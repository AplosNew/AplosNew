'use strict';
disciplinaryActionCriticalityController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function disciplinaryActionCriticalityController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'disciplinaryActionCriticality';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.disciplinaryActionCriticalitys = [];
    $scope.path = 'employees/disciplinaryactioncriticality/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.disciplinaryActionCriticalitys = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.disciplinaryActionCriticality = {
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

    $scope.disciplinaryActionCriticalityNew = Object.assign({}, $scope.disciplinaryActionCriticality);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.disciplinaryActionCriticalityNew.Sequence = data;
        })
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.disciplinaryActionCriticality = $scope.disciplinaryActionCriticalitys[$scope.index];
        $scope.disciplinaryActionCriticalityNew = Object.assign({}, $scope.disciplinaryActionCriticality);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.disciplinaryActionCriticalityNew, $scope.disciplinaryActionCriticality);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.disciplinaryActionCriticalityNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.disciplinaryActionCriticality,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.disciplinaryActionCriticalitys.push(response.data.disciplinaryActionCriticality);
                        $scope.disciplinaryActionCriticalitys = $filter('orderBy')($scope.disciplinaryActionCriticalitys, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.disciplinaryActionCriticality,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.disciplinaryActionCriticalitys[$scope.index] = $scope.disciplinaryActionCriticality;
                            $scope.disciplinaryActionCriticalitys = $filter('orderBy')($scope.disciplinaryActionCriticalitys, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                        $scope.getData();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.disciplinaryActionCriticalityNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.disciplinaryActionCriticalityNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.disciplinaryActionCriticalitys.splice($scope.index, 1);
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
        $scope.disciplinaryActionCriticality = {};
        $scope.disciplinaryActionCriticalityNew = {};
        $scope.disciplinaryActionCriticalityNew.Sequence = seq;
        $scope.disciplinaryActionCriticalityNew.Active = true;
    }
}