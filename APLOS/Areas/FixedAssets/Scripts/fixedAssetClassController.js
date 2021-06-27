'use strict';
fixedAssetClassController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function fixedAssetClassController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'FixedAsset Class';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.fixedassetclasses = [];
    $scope.path = 'fixedassets/fixedassetclass/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.fixedassetclasses = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.fixedAssetClass = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.fixedAssetClassNew = Object.assign({}, $scope.fixedAssetClass);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.fixedAssetClassNew.Sequence = response.data;
            });
    };

    $scope.GetSequence();
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.fixedAssetClass = $scope.fixedassetclasses[$scope.index];
        $scope.fixedAssetClassNew = Object.assign({}, $scope.fixedAssetClass);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        angular.copy($scope.fixedAssetClassNew, $scope.fixedAssetClass);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.fixedAssetClassNewForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.fixedAssetClass,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.fixedassetclasses.push(response.data.FixedAssetClass);
                        $scope.fixedassetclasses = $filter('orderBy')($scope.fixedassetclasses, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.fixedAssetClass,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.fixedassetclasses[$scope.index] = $scope.fixedAssetClass;
                            $scope.fixedassetclasses = $filter('orderBy')($scope.fixedassetclasses, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.fixedAssetClassNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.fixedAssetClassNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.fixedassetclasses.splice($scope.index, 1);
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
        $scope.fixedAssetClass = {};
        $scope.fixedAssetClassNew = {};
        $scope.fixedAssetClassNew.Sequence = seq;
        $scope.fixedAssetClassNew.Active = true;
    }
}