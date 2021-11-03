'use strict';
function ProcessTypeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Process Type";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.processTypes = [];
    $scope.path = 'Processes/processtype/';
    //$scope.getListUrl = $scope.path + 'getprocesstypelist';
    $scope.getListUrl = $scope.path + 'getprocesstypelist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    //baseService.init($scope.getListUrl, null, null, null, 'UserName', 'UserName');
    //$scope.getData = function (pageno) {
    //    baseService.pagination(pageno)
    //        .then(function (result) {
    //            $scope.processTypes = result.Rows;
    //        }, function () {
    //            ShowResult(commonMessage.NetworkError, 'failure');
    //        }).finally(function () {
    //        });
    //};
    //$scope.getData();
    baseService.init($scope.getListUrl, null, null, null, 'UserName', 'UserName');
    $scope.processType = {
        Id: null,
        CompanyGroupId: null,
        ProcessId: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.processTypeNew = Object.assign({}, $scope.processType);
    $scope.getData = function (pageno) {
        $rootScope.parameters.processId = $scope.processTypeNew.ProcessId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.processTypes = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });
    $scope.processList = [];
    cboService.getProcessCbo(function (result) {
        $scope.processList = result;
    });
    $scope.searchByList = [
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

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.processType = $scope.processTypes[$scope.index];
        $scope.processTypeNew = Object.assign({}, $scope.processType);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.processTypeNewForm.$valid) {
            $scope.processType = Object.assign({}, $scope.processTypeNew);
            $scope.processType.CompanyGroupId = $window.companyGroupId;
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.processType,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        return ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.processTypes.push(response.data.ProcessType);
                        $scope.processTypes = $filter('orderBy')($scope.processTypes, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.processType,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.processTypes[$scope.index] = $scope.processType;
                            $scope.processTypes = $filter('orderBy')($scope.processTypes, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.processType.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.processType.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.processTypes.splice($scope.index, 1);
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    }
    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    function ClearFields() {
        $scope.Action = "Save";
        $scope.processType = {};
        $scope.processTypeNew = { ProcessId: $scope.processTypeNew.ProcessId };
        $scope.processTypeNew.Active = true;
    }
}
ProcessTypeController.$inject = ['cboService', "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];