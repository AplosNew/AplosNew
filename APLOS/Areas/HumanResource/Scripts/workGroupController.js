'use strict';
WorkGroupController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter','cboService'];
function WorkGroupController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter,cboService) {
    $rootScope.title = 'Work Group';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.workGroups = [];
    $scope.path = 'humanresource/workGroup/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.workGroup = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        CompanyId: null,
        PlantId: null,
    };


    $scope.workGroupNew = Object.assign({}, $scope.workGroup);
    $scope.companyList = [];
    $scope.plantList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });
    $scope.getPlantList = function () {
        cboService.getCboPlantByCompany($scope.workGroupNew.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };
    $scope.getListData = function () {
        baseService.init("humanresource/workGroup/getList?plantId=" + $scope.workGroupNew.PlantId, null, null, null, "Sequence", "UserName");
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.workGroups = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    }

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.workGroupNew.Sequence = response.data;
            });
    };

    $scope.GetSequence();
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.workGroup = $scope.workGroups[$scope.index];
        $scope.workGroupNew = Object.assign({}, $scope.workGroup);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        angular.copy($scope.workGroupNew, $scope.workGroup);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.workGroupNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.workGroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.workGroups.push(response.data.WorkGroup);
                        $scope.workGroups = $filter('orderBy')($scope.workGroups, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.workGroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.workGroups[$scope.index] = $scope.workGroup;
                            $scope.workGroups = $filter('orderBy')($scope.workGroups, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.workGroupNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.workGroupNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.workGroups.splice($scope.index, 1);
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
        $scope.Action = 'Save';
        $scope.workGroup = { CompanyId: $scope.workGroupNew.CompanyId, PlantId: $scope.workGroupNew.PlantId  };
        $scope.workGroupNew = { CompanyId: $scope.workGroupNew.CompanyId,PlantId: $scope.workGroupNew.PlantId };
        $scope.workGroupNew.Sequence = seq;
        $scope.workGroupNew.Active = true;
    }
}