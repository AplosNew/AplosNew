'use strict';
costingCategoryController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function costingCategoryController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Costing Category";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.costingCategoryList = [];
    $scope.path = 'Costings/costingCategory/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.costingCategoryList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.costingCategory = {
        Id: null
        , CompanyGroupId: null
        , Sequence: 0.0
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , Description: null
        , Remarks: null
        , Active: true
    };
    $scope.costingCategoryNew = Object.assign({}, $scope.costingCategory);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.costingCategoryNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (index) {
        $scope.index = index;
        $scope.costingCategory = $scope.costingCategoryList[$scope.index];
        $scope.costingCategoryNew = Object.assign({}, $scope.costingCategory);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.costingCategoryNew, $scope.costingCategory);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.costingCategoryNewForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST'
                    , url: $scope.saveUrl
                    , data: $scope.costingCategory
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.costingCategoryList.push(response.data.CostingCategory);
                        $scope.costingCategoryList = $filter('orderBy')($scope.costingCategoryList, 'Sequence');
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
                    , data: $scope.costingCategory
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.costingCategoryList[$scope.index] = $scope.costingCategory;
                            $scope.costingCategoryList = $filter('orderBy')($scope.costingCategoryList, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.costingCategoryNew.Id)) {
            $http({
                method: 'POST'
                , url: $scope.deleteUrl + $scope.costingCategoryNew.Id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.costingCategoryList.splice($scope.index, 1);
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
        $scope.costingCategory = {};
        $scope.costingCategoryNew = { Sequence: seq, Active: true };
    }
}