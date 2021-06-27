'use strict';
function ProjectPlanningCategoryController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "ProjectPlanning Category";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.projectPlanningCategories = [];
    $scope.path = 'Projects/projectPlanningCategory/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "Sequence", "UserName");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
                   .then(function (result) {
                       $scope.projectPlanningCategories = result.Rows;
                   }, function () {
                       ShowResult(commonMessage.NetworkError, 'failure');
                   }).finally(function () {
                   });
    };
    $scope.getData();

    $scope.projectPlanningCategory = {
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
    $scope.projectPlanningCategoryNew = Object.assign({}, $scope.projectPlanningCategory);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
          .then(function (response) {
              $scope.projectPlanningCategoryNew.Sequence = response.data;
          });
    }
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.projectPlanningCategory = $scope.projectPlanningCategories[$scope.index];
        $scope.projectPlanningCategoryNew = Object.assign({}, $scope.projectPlanningCategory);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.projectPlanningCategoryNew, $scope.projectPlanningCategory);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.projectPlanningCategoryForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.projectPlanningCategory,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.projectPlanningCategories.push(response.data.ProjectPlanningCategory);
                        $scope.projectPlanningCategories = $filter('orderBy')($scope.projectPlanningCategories, 'Sequence');
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
                    data: $scope.projectPlanningCategory,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.projectPlanningCategories[$scope.index] = $scope.projectPlanningCategory;
                            $scope.projectPlanningCategories = $filter('orderBy')($scope.projectPlanningCategories, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.projectPlanningCategoryNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.projectPlanningCategoryNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.projectPlanningCategories.splice($scope.index, 1);
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
        $scope.projectPlanningCategory = {};
        $scope.projectPlanningCategoryNew = {};
        $scope.projectPlanningCategoryNew.Sequence = seq;
        $scope.projectPlanningCategoryNew.Active = true;
    }
}
ProjectPlanningCategoryController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
