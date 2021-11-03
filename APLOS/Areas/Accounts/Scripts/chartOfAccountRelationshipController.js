'use strict';
ChartOfAccountRelationshipController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ChartOfAccountRelationshipController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "ChartOfAccRelation";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.chartOfAccountRelationshipes = [];
    $scope.path = 'accounts/ChartOfAccountRelationship/';
    $scope.getListUrl = $scope.path + 'GetChartOfAccountRelationshipList';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Sequence', 'Sequence');
    $scope.getData = function (pageno) {
        $rootScope.parameters.coaid = $scope.chartOfAccountRelationship.COAId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.chartOfAccountRelationshipes = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.chartOfAccountRelationship = {
        Id: null,
        COAId: null,
        CompanyGroupId: null,
        Sequence: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    $scope.coaList = [];
    $scope.getCboChartOfAccount = function (companyGroupId) {
        cboService.getCboChartOfAccount(companyGroupId, function (result) {
            $scope.coaList = result;
        });
    };

    $scope.onCOAChange = function (item) {
        $http({
            method: 'get',
            url: 'accounts/ChartOfAccountRelationship/GetAutoSequence?caoid=' + item
        }).then(function successCallback(response) {
            $scope.chartOfAccountRelationship.Sequence = response.data;
        });
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        var companyGroupId = $scope.chartOfAccountRelationship.CompanyGroupId;
        $scope.chartOfAccountRelationship = $scope.chartOfAccountRelationshipes[$scope.index];
        $scope.chartOfAccountRelationship.CompanyGroupId = companyGroupId;
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.chartOfAccountRelationshipForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.chartOfAccountRelationship,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.chartOfAccountRelationshipes.push(response.data.ChartOfAccountRelationship);
                        baseService.paginationAdd();
                        ClearFields();
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
                    data: $scope.chartOfAccountRelationship,
                    dataType: 'JSON'
                }).then(function (response) {
                    ShowResult(response.data.Message, 'success');
                    if ($scope.index > -1) {
                        $scope.chartOfAccountRelationshipes[$scope.index] = $scope.chartOfAccountRelationship;
                    }
                    ClearFields();
                }), function (response) {
                    ShowResult(response.status.Message, 'failure');
                };
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.chartOfAccountRelationship.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.chartOfAccountRelationship.Id,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.chartOfAccountRelationshipes.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields($scope.onCOAChange($scope.chartOfAccountRelationship.COAId));
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.chartOfAccountRelationship = { COAId: $scope.chartOfAccountRelationship.COAId };
        $scope.chartOfAccountRelationship.Sequence = seq;
        $scope.chartOfAccountRelationship.Active = true;
    }
}