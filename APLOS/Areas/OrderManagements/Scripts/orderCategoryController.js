'use strict';
OrderCategoryController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function OrderCategoryController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Order Category";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.orderCategories = [];
    $scope.path = 'OrderManagements/ordercategory/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'PlanningPriority', 'PlanningPriority');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
                   .then(function (result) {
                       $scope.orderCategories = result.Rows;
                   }, function () {
                       ShowResult(commonMessage.NetworkError, 'failure');
                   }).finally(function () {
                   });
    };
    $scope.getData();

    $scope.orderCategory = {
        Id: null,
        PlanningPriority: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true

    };
    $scope.orderCategoryNew = Object.assign({}, $scope.orderCategory);

    $scope.searchByList = [
        {
            'name': 'PlanningPriority',
            'value': 'PlanningPriority'
        },
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
                $scope.orderCategoryNew.PlanningPriority = response.data;
            });
    };
    $scope.GetSequence();
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.orderCategory = $scope.orderCategories[$scope.index];
        $scope.orderCategoryNew = Object.assign({}, $scope.orderCategory);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        angular.copy($scope.orderCategoryNew, $scope.orderCategory);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.orderCategoryNewForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.orderCategory,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.orderCategories.push(response.data.OrderCategory);
                        $scope.orderCategories = $filter('orderBy')($scope.orderCategories, 'PlanningPriority');
                        baseService.paginationAdd();
                        ClearFields(response.data.PlanningPriority);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.orderCategory,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.orderCategories[$scope.index] = $scope.orderCategory;
                            $scope.orderCategories = $filter('orderBy')($scope.orderCategories, 'PlanningPriority');
                        }
                        ClearFields(response.data.PlanningPriority);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.orderCategoryNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.orderCategoryNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.orderCategories.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.PlanningPriority);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    };
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.orderCategory = {};
        $scope.orderCategoryNew = {};
        $scope.orderCategoryNew.PlanningPriority = seq;
        $scope.orderCategoryNew.Active = true;
    }
}