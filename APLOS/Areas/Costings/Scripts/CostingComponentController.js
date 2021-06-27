'use strict';
CostingComponentController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CostingComponentController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Costing Component";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.costingSubCategoryList = [];
    $scope.path = 'Costings/CostingComponent/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);


    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.costingSubCategoryList = result.Rows;

            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.searchByList = [{ name: 'Sequence', value: 'Sequence' },
    { name: 'Code', value: 'Code' },
    { name: 'Short Name', value: 'ShortName' },
    { name: 'User Name', value: 'UserName' },
    { name: 'Costing Segment', value: 'CostingSegment' }];

    $scope.costingSubCategory = {
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
        , CalculationMethod: 'FOB'
        , isSystemGenerated: false
        , CostingSegment: null
        , ProcurementCostingSavingsPercentage: 0
        , PreCostingSavingsPercentage: 0
        , ConsiderForFGValuation: false
    };
    $scope.costingSubCategoryNew = Object.assign({}, $scope.costingSubCategory);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.costingSubCategoryNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.CostingSegmentList = [];
    cboService.getEnumCbo("enum/GetCostingSegmentEnumCbo", function (result) {
        $scope.CostingSegmentList = result;

    });



    $scope.Get = function (index) {
        $scope.index = index;
        $scope.costingSubCategory = $scope.costingSubCategoryList[$scope.index];
        $scope.costingSubCategoryNew = Object.assign({}, $scope.costingSubCategory);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.costingSubCategoryNew, $scope.costingSubCategory);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.costingSubCategoryNewForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST'
                    , url: $scope.saveUrl
                    , data: $scope.costingSubCategory
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.costingSubCategoryList.push(response.data.costingSubCategory);
                        $scope.costingSubCategoryList = $filter('orderBy')($scope.costingSubCategoryList, 'Sequence');
                        //baseService.paginationAdd();
                        $scope.getData();
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
                    , data: $scope.costingSubCategory
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.costingSubCategoryList[$scope.index] = $scope.costingSubCategory;
                            $scope.costingSubCategoryList = $filter('orderBy')($scope.costingSubCategoryList, 'Sequence');
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
        try {

            if (!baseService.isUndefinedOrNull($scope.costingSubCategoryNew.Id)) {
                $http({
                    method: 'POST'
                    , url: $scope.deleteUrl + $scope.costingSubCategoryNew.Id
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.costingSubCategoryList.splice($scope.index, 1);
                        baseService.paginationRemove();
                        ClearFields(response.data.Sequence);
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
            }

        } catch (e) {

        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    $scope.CreateDefaultValues = function () {

        $http({
            method: 'POST', url: $scope.path + "saveDefaults", dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult('System successfully created the default values; you may now proceed to create your own components', 'success');
                $scope.getData();
                ClearFields(response.data.Sequence);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    }

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.costingSubCategory = {};
        $scope.costingSubCategoryNew = { Sequence: seq, Active: true };
    }
}