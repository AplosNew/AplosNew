'use strict';
fixedAssetSubClassController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function fixedAssetSubClassController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'FixedAsset SubClass';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.fixedAssetSubClasses = [];
    $scope.path = 'fixedassets/fixedassetsubclass/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.fixedAssetSubClasses = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.fixedAssetSubClass = {
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
    $scope.fixedAssetSubClassNew = Object.assign({}, $scope.fixedAssetSubClass);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.fixedAssetSubClassNew.Sequence = response.data;
            });
    };

    $scope.GetSequence();
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.fixedAssetSubClass = $scope.fixedAssetSubClasses[$scope.index];
        $scope.fixedAssetSubClassNew = Object.assign({}, $scope.fixedAssetSubClass);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        angular.copy($scope.fixedAssetSubClassNew, $scope.fixedAssetSubClass);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.fixedAssetSubClassNewForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.fixedAssetSubClass,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.fixedAssetSubClasses.push(response.data.FixedAssetSubClass);
                        $scope.fixedAssetSubClasses = $filter('orderBy')($scope.fixedAssetSubClasses, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.fixedAssetSubClass,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.fixedAssetSubClasses[$scope.index] = $scope.fixedAssetSubClass;
                            $scope.fixedAssetSubClasses = $filter('orderBy')($scope.fixedAssetSubClasses, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.fixedAssetSubClassNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.fixedAssetSubClassNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.fixedAssetSubClasses.splice($scope.index, 1);
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
        $scope.fixedAssetSubClass = {};
        $scope.fixedAssetSubClassNew = {};
        $scope.fixedAssetSubClassNew.Sequence = seq;
        $scope.fixedAssetSubClassNew.Active = true;
    }
}