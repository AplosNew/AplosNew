'use strict';
function ProjectPlanningSubCategoryController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "ProjectPlanning SubCategory";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.projectPlanningSubCategories = [];
    $scope.path = 'Projects/projectplanningsubcategory/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "Sequence", "UserName");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
                   .then(function (result) {
                       $scope.projectPlanningSubCategories = result.Rows;
                   }, function () {
                       ShowResult(commonMessage.NetworkError, 'failure');
                   }).finally(function () {
                   });
    };
    $scope.getData();

    $scope.projectPlanningSubCategory = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };
    $scope.projectPlanningSubCategoryNew = Object.assign({}, $scope.projectPlanningSubCategory);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
          .then(function (response) {
              $scope.projectPlanningSubCategoryNew.Sequence = response.data;
          });
    }
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.projectPlanningSubCategory = $scope.projectPlanningSubCategories[$scope.index];
        $scope.projectPlanningSubCategoryNew = Object.assign({}, $scope.projectPlanningSubCategory);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        for (var i in $scope.projectPlanningSubCategoryNew) {
            $scope.projectPlanningSubCategory[i] = $scope.projectPlanningSubCategoryNew[i];
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.projectPlanningSubCategoryForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.projectPlanningSubCategory,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.projectPlanningSubCategories.push(response.data.ProjectPlanningSubCategory);
                        $scope.projectPlanningSubCategories = $filter('orderBy')($scope.projectPlanningSubCategories, 'Sequence');
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
                    url: $scope.saveUrl,
                    data: $scope.projectPlanningSubCategory,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.projectPlanningSubCategories[$scope.index] = $scope.projectPlanningSubCategory;
                            $scope.projectPlanningSubCategories = $filter('orderBy')($scope.projectPlanningSubCategories, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.projectPlanningSubCategoryNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.projectPlanningSubCategoryNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.projectPlanningSubCategories.splice($scope.index, 1);
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
        $scope.projectPlanningSubCategory = {};
        $scope.projectPlanningSubCategoryNew = {};
        $scope.projectPlanningSubCategoryNew.Sequence = seq;
        $scope.projectPlanningSubCategoryNew.Active = true;
    }
}
ProjectPlanningSubCategoryController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
