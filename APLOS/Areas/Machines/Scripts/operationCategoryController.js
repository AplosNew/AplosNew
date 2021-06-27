'use strict';
OperationCategoryController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function OperationCategoryController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Operation Category";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.operationCategories = [];
    $scope.path = 'Machines/operationcategory/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        //$rootScope.parameters.operationCategoryId = $scope.operationCategoryNew.OperationCategoryId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.operationCategories = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.operationCategory = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        IsOperationMandatoryforEmployee: false,
        Active: true
    };
    $scope.operationCategoryNew = Object.assign({}, $scope.operationCategory);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.operationCategoryNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.operationCategory = $scope.operationCategories[$scope.index];
        $scope.operationCategoryNew = Object.assign({}, $scope.operationCategory);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        angular.copy($scope.operationCategoryNew, $scope.operationCategory);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.operationCategoryNewForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'entity': $scope.operationCategory
                       
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.operationCategories.push(response.data.OperationCategory);
                        $scope.operationCategories = $filter('orderBy')($scope.operationCategories, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: {
                        'entity': $scope.operationCategory
                       
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.operationCategories[$scope.index] = $scope.operationCategory;
                            $scope.operationCategories = $filter('orderBy')($scope.operationCategories, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.operationCategoryNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.operationCategoryNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.operationCategories.splice($scope.index, 1);
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
        $scope.Action = "Save";
        $scope.operationCategory = {};
        $scope.operationCategoryNew = {};
        $scope.operationCategoryNew.Sequence = seq;
        $scope.operationCategoryNew.Active = true;
        
        ChildClearFields();
    }
    
};