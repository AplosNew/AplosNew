'use strict';
operationMotionController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function operationMotionController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Operation Motion";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.modelList = [];
    $scope.path = 'Machines/OperationMotion/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.model = {
        Id: null
        , CompanyGroupId: null
        , PlantId: null
        , OperationId: null
        , Sequence: 0.0
        , Code: null
        , TMU: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , Description: null
        , Remarks: null
        , Active: true
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    baseService.init('Machines/OperationMotion/getlist');
    $scope.getData = function () {
        $scope.getSearchData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.modelList = []; //productionOrderMasterList
                    $scope.modelList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.getSearchData();
    };
    $scope.getData();

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.modelNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (index) {
        $scope.index = index;
        $scope.model = $scope.modelList[$scope.index];
        $scope.modelNew = Object.assign({}, $scope.model);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.modelNew, $scope.model);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.modelNewForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST'
                    , url: $scope.saveUrl
                    , data: $scope.model
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failureOperationMotion');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.modelList.push(response.data.entity);
                        $scope.modelList = $filter('orderBy')($scope.modelList, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST'
                    , url: $scope.updateUrl
                    , data: $scope.model
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.modelList[$scope.index] = $scope.model;
                            $scope.modelList = $filter('orderBy')($scope.modelList, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.modelNew.Id)) {
            $http({
                method: 'POST'
                , url: $scope.deleteUrl + $scope.modelNew.Id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.modelList.splice($scope.index, 1);
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
        $scope.Action = "Save";
        $scope.model = {};
        $scope.modelNew = { Sequence: seq, Active: true };
    }
}
