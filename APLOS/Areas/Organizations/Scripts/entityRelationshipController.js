'use strict';
EntityRelationshipController.$inject = ['$rootScope', '$scope', 'cboService', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EntityRelationshipController($rootScope, $scope, cboService, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Entity Relationship';
    $scope.Action = 'Save';
    $scope.ngShowTbl = false;
    $scope.index = -1;
    $scope.comSR = [];
    $scope.path = 'Organizations/entityrelationship/';
    $scope.getListUrl = $scope.path + 'getcompanystructurerelationlist';
    $scope.companyStructureRelation = {
        Id: null,
        Sequence: null,
        StandardName: null,
        CompanyGroupId: null,
        CompanyId: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: $filter('date')(Date.now(), 'yyyy-MM-dd')
    };

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    $scope.getCompany = function () {
        cboService.getCboCompanyByCompanyGroup($scope.companyStructureRelation.CompanyGroupId, function (result) {
            $scope.companyList = result;
        });
    };

    $scope.getSequence = function (companyGroupId, companyId) {
        cboService.getSequence('Organizations/entityrelationship/getautosequence?companyGroupId=' + companyGroupId + '&companyId=' + companyId, function (result) {
            $scope.companyStructureRelation.Sequence = result;
        });
    };

    $scope.getData = function (pageno) {
        $rootScope.parameters.companyGroupId = $scope.companyStructureRelation.CompanyGroupId;
        $rootScope.parameters.companyId = $scope.companyStructureRelation.CompanyId;
        baseService.init($scope.getListUrl);
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.comSR = result.Rows;
                $scope.ngShowTbl = true;
                $scope.getSequence($scope.companyStructureRelation.CompanyGroupId, $scope.companyStructureRelation.CompanyId);
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $rootScope.searchByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.companyStructureRelation = $scope.comSR[$scope.index];
        $scope.companyStructureRelation.AddedDate = $filter('dateFilter')($scope.companyStructureRelation.AddedDate);
        $scope.companyStructureRelation.UpdatedDate = $filter('dateFilter')($scope.companyStructureRelation.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.comSRForm.$valid) {
            $scope.companyId = $scope.companyStructureRelation.CompanyId;
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/entityrelationship/create',
                    data: $scope.companyStructureRelation,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.comSR.push(response.data.StructureRelation);
                        $scope.companyStructureRelation.CompanyId = $scope.companyId;
                        baseService.paginationAdd();
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
                    url: 'Organizations/entityrelationship/edit',
                    data: $scope.companyStructureRelation,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.comSR[$scope.index] = $scope.companyStructureRelation;
                            $scope.companyStructureRelation.CompanyId = $scope.companyId;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.companyStructureRelation.Id)) {
            $http({
                method: 'POST',
                url: 'Organizations/entityrelationship/delete/' + $scope.companyStructureRelation.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.comSR.splice($scope.index, 1);
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
    }

    $scope.Clear = function () {
        ClearFields($scope.getSequence($scope.companyStructureRelation.CompanyGroupId, $scope.companyStructureRelation.CompanyId));
        return true;
    }

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.companyStructureRelation = {
            CompanyGroupId: $scope.companyStructureRelation.CompanyGroupId,
            CompanyId: $scope.companyStructureRelation.CompanyId
        };
        $scope.companyStructureRelation.Sequence = seq;
        $scope.companyStructureRelation.Active = true;
        $scope.getData();
    }
}