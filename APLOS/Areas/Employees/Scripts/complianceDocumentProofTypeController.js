'use strict';
complianceDocumentProofTypeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function complianceDocumentProofTypeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'complianceDocumentProofType';

    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.complianceDocumentProofType = [];
    $scope.path = 'employees/complianceDocumentProofType/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init('employees/complianceDocumentProofType/getlist');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.complianceDocumentProofTypeList = result.Rows;
                $scope.complianceDocumentProofTypeList = $filter('orderBy')($scope.complianceDocumentProofTypeList, 'Sequence');

            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();
    $scope.complianceDocumentProofType = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        DocumentationBy: 'Self',
        Description: null,
        Remarks: null,
        Active: true,
        Archive: true
    };
    $scope.complianceDocumentProofTypeNew = angular.copy($scope.complianceDocumentProofType);
    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.complianceDocumentProofTypeNew.Sequence = response.data;
            });
    };

    $scope.GetSequence();
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.complianceDocumentProofType = $scope.complianceDocumentProofTypeList[$scope.index];
        $scope.complianceDocumentProofTypeNew = angular.copy($scope.complianceDocumentProofType);
        //$scope.DocumentationBy = 'Self';
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.complianceDocumentProofTypeNewForm.$valid) {
            angular.copy($scope.complianceDocumentProofTypeNew, $scope.complianceDocumentProofType);
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.complianceDocumentProofType,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.complianceDocumentProofTypeList.push(response.data.ComplianceDocumentProofType);
                        $scope.complianceDocumentProofTypeList = $filter('orderBy')($scope.complianceDocumentProofTypeList, 'Sequence');
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
                    url: $scope.updateUrl,
                    data: $scope.complianceDocumentProofType,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.complianceDocumentProofTypeList[$scope.index] = $scope.complianceDocumentProofType;
                            $scope.complianceDocumentProofTypeList = $filter('orderBy')($scope.complianceDocumentProofTypeList, 'Sequence');
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
        if (!baseService.isUndefinedOrNull($scope.complianceDocumentProofTypeNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.complianceDocumentProofTypeNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.complianceDocumentProofTypeList.splice($scope.index, 1);
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
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.complianceDocumentProofType = {};
        $scope.complianceDocumentProofTypeNew = {};
        $scope.complianceDocumentProofTypeNew.Sequence = seq;
        $scope.complianceDocumentProofTypeNew.DocumentationBy = 'Self';
        $scope.complianceDocumentProofTypeNew.Active = true;
    }

}
