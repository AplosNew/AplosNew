'use strict';
ProductionStatusController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ProductionStatusController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Production Status";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.productionStatuses = [];
    $scope.path = 'Productions/productionStatus/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'PlanningGroupPriority', 'PlanningGroupPriority');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
                   .then(function (result) {
                       $scope.productionStatuses = result.Rows;
                   }, function () {
                       ShowResult(commonMessage.NetworkError, 'failure');
                   }).finally(function () {
                   });
    };
    $scope.getData();

    $scope.productionStatus = {
        Id: null,
        PlanningGroupPriority: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        MasterPlanApplicable: true
    };
    $scope.productionStatusNew = Object.assign({}, $scope.productionStatus);

    $scope.searchByList = [
        {
            'name': 'Planning Group Priority',
            'value': 'PlanningGroupPriority'
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
                $scope.productionStatusNew.PlanningGroupPriority = response.data;
            });
    };
    $scope.GetSequence();
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.productionStatus = $scope.productionStatuses[$scope.index];
        $scope.productionStatusNew = Object.assign({}, $scope.productionStatus);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        angular.copy($scope.productionStatusNew, $scope.productionStatus);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.productionStatusNewForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.productionStatus,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.productionStatuses.push(response.data.ProductionStatus);
                        $scope.productionStatuses = $filter('orderBy')($scope.productionStatuses, 'PlanningGroupPriority');
                        baseService.paginationAdd();
                        ClearFields(response.data.PlanningGroupPriority);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.productionStatus,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.productionStatuses[$scope.index] = $scope.productionStatus;
                            $scope.productionStatuses = $filter('orderBy')($scope.productionStatuses, 'PlanningGroupPriority');
                        }
                        ClearFields(response.data.PlanningGroupPriority);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.productionStatusNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.productionStatusNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.productionStatuses.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.PlanningGroupPriority);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    }
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    }
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.productionStatus = {};
        $scope.productionStatusNew = {};
        $scope.productionStatusNew.PlanningGroupPriority = seq;
        $scope.productionStatusNew.Active = true;
    }
}