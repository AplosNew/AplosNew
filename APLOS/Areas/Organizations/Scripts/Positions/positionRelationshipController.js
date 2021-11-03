'use strict';
positionRelationshipController.$inject = ['$rootScope', 'cboService', '$scope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function positionRelationshipController($rootScope, cboService, $scope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Position Relationship';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.comSR = [];
    $scope.path = 'Organizations/positionrelationship/';
    $scope.getListUrl = $scope.path + 'getpositionlist';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        $rootScope.parameters.companyGroupId = $scope.companyStructureRelation.CompanyGroupId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.comSR = result.Rows;
                $scope.getSequence();
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
        AddedDate: $filter('date')(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null
    };

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    $scope.getSequence = function () {
        cboService.getSequence('Organizations/positionrelationship/getautosequence?companyGroupId=' + $scope.companyStructureRelation.CompanyGroupId, function (result) {
            $scope.companyStructureRelation.Sequence = result;
        });
    };
    $scope.getSequence();

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
        try {
            if ($scope.companyStructureRelation.UserName.match(' ')) {
                throw "User Name should enter without space.";
            }
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.comSRForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: 'Organizations/positionrelationship/create',
                        data: $scope.companyStructureRelation,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.comSR.push(response.data.StructureRelation);
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
                        url: 'Organizations/positionrelationship/edit',
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
                            }
                            ClearFields(response.data.Sequence);
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                    return true;
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.companyStructureRelation.Id)) {
            $http({
                method: 'POST',
                url: 'Organizations/positionrelationship/delete/' + $scope.companyStructureRelation.Id,
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
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.companyStructureRelation = { CompanyGroupId: $scope.companyStructureRelation.CompanyGroupId };
        $scope.companyStructureRelation.Active = true;
        $scope.getData();
    }
}