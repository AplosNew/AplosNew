'use strict';
ProcessController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function ProcessController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Process";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.processes = [];
    $scope.path = 'Processes/process/';
    $scope.getListUrl = $scope.path + 'GetList?processId=[]';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Sequence', 'UserName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.processes = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.searchByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Process Group',
            'value': 'ProcessGroupName'
        }
    ];

    $scope.process = {
        Id: null
        , CompanyGroupId: null
        , MaterialTypeId: null
        , ProcessGroupId: null
        , POControlMilestoneSequence:null
        , Sequence: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , Remarks: null
        , IsValueAdded: false
        , IsProductionProcess: false
        , IsProcessRouting: false
        , IsLocked: false
        , IsChecked: false
        , Active: true
        , MasterPlanApplicable: false
        , IsFirst: false
        , IsLast: false
        , IsCrossAllowed: false
        , LineItem: false
        , SKU1: false
        , SKU2: false
    };
    $scope.processNew = Object.assign({}, $scope.process);


    $scope.materialTypeList = [];
    $http({
        method: 'GET',
        url: 'Materials/materialtype/getcbofilterbysfg'
    }).then(function successCallback(response) {
        $scope.materialTypeList = response.data;
    });

    $http.get('Processes/ProcessGroup/GetCbo')
        .then(function (response) {
            $scope.processGroupList = response.data;
        });

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.processNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();

    $scope.Get = function (index) {
        $scope.index = index;
        $scope.process = $scope.processes[$scope.index];
        $scope.processNew = Object.assign({}, $scope.process);
        //baseService.removeErrorClasses();
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.checkProcessRouting = function (event) {
        if (event.currentTarget.checked) {
            if (!$scope.process.IsProcessRouting) {
                $scope.processNew.IsLocked = false;
                ShowResult('Please at first select process routing.', "failure");
            }
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.processNewForm.$valid) {
            angular.copy($scope.processNew, $scope.process);
            if ($scope.process.IsLocked === true && $scope.process.IsProcessRouting == false) {
                return ShowResult('Locked can not be checked without process routing.', "failure");
            }
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.process,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.process = response.data.Process;
                        $scope.process.ProcessGroupName = angular.element("#ProcessGroupId :selected").text();
                        $scope.processes.push($scope.process);
                        $scope.processes = $filter('orderBy')($scope.processes, 'Sequence');
                        ClearFields(response.data.Sequence);
                        $scope.getData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.process,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.process.ProcessGroupName = angular.element("#ProcessGroupId :selected").text();
                            $scope.processes[$scope.index] = $scope.process;
                            $scope.processes = $filter('orderBy')($scope.processes, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                        $scope.getData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.processNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.processNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.processes.splice($scope.index, 1);
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.process = {};
        $scope.processNew = { Sequence: seq, IsValueAdded: false, Active: true, IsProductionProcess: false, IsProcessRouting: false, IsChecked: false, IsLocked: false };
    }
}
