'use strict';
recruitmentGroupController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function recruitmentGroupController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Recruitment Group';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.recruitmentGroups = [];
    $scope.path = 'employees/recruitmentgroup/';
    $scope.getListUrl = 'employees/recruitmentgroup/getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        $rootScope.parameters.plantId = $scope.recruitmentGroup.PlantId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.recruitmentGroups = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.recruitmentGroup = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        OnBoardDate: null,
        Active: true
    };

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.getCboPlantByCompany = function (companyId) {
        cboService.getCboPlantByCompany(companyId, function (result) {
            $scope.plantList = result;
        });
    };

    $scope.getSequence = function (plantId) {
        cboService.getSequence($scope.getSeqUrl + "?plantId=" + plantId, function (result) {
            $scope.recruitmentGroup.Sequence = result;
        });
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.recruitmentGroup = $scope.recruitmentGroups[$scope.index];
        $scope.recruitmentGroup.OnBoardDate = $filter('dateFiltering')($scope.recruitmentGroup.OnBoardDate, 'dd-MM-yyyy');
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.recruitmentGroupForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.recruitmentGroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.recruitmentGroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.recruitmentGroups[$scope.index] = $scope.recruitmentGroup;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.recruitmentGroup.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.recruitmentGroup.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.recruitmentGroups.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.clear = function () {
        ClearFields($scope.getSequence($scope.recruitmentGroup.PlantId));
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.CompanyId = $scope.recruitmentGroup.CompanyId;
        $scope.PlantId = $scope.recruitmentGroup.PlantId;
        $scope.recruitmentGroup = {};
        $scope.recruitmentGroup.CompanyId = $scope.CompanyId;
        $scope.recruitmentGroup.PlantId = $scope.PlantId;
        $scope.recruitmentGroup.Sequence = seq;
        $scope.recruitmentGroup.Active = true;
    }
}
