'use strict';
securityTypeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function securityTypeController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'Security Type';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.securityTypes = [];
    $scope.path = 'accounts/FinancingType/';
    $scope.sourceType = 'Security';
    $scope.getUrl = $scope.path + 'GetFinancingType';
    $scope.saveUrl = $scope.path + 'CreateSecurityType';
    $scope.updateUrl = $scope.path + 'EditSecurityType';
    $scope.deleteUrl = $scope.path + 'DeleteFinancingType/';
    baseService.init('accounts/FinancingType/GetSecurityTypeList');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.securityTypes = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.securityType = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        AssetUserName: null,
        LiabilityUserName: null,
        SourceType: null,
        IsInterCompany: null,
        IsInterEntity: null,
        IsOthers: null,
        Description: null,
        Remarks: null,
        Active: true
    };

    $scope.getSequence = function () {
        cboService.getSequence('accounts/FinancingType/GetFinancingTypeAutoSequence?type=' + $scope.sourceType, function (result) {
            $scope.securityType.Sequence = result;
        });
    };
    $scope.getSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.securityType = baseService.find($scope.securityTypes, id, null);
        $scope.securityType.AddedDate = $filter('dateFilter')($scope.securityType.AddedDate);
        $scope.securityType.UpdatedDate = $filter('dateFilter')($scope.securityType.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form0.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.securityType,
                    dataType: 'JSON'
                }).then(
                    function success(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.securityTypes.push(response.data.ModelData);
                            baseService.paginationAdd();
                            ClearFields($scope.getSequence());
                        }
                    }, function error(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.securityType,
                    dataType: 'JSON'
                }).then(function success(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.securityTypes[$scope.index] = $scope.securityType;
                        }
                        ClearFields($scope.getSequence());
                    }
                }, function error(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.securityType.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.securityType.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.securityTypes.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields($scope.getSequence());
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
        ClearFields($scope.getSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.securityType = {};
        $scope.securityType.Sequence = seq;
        $scope.securityType.Active = true;
    }
}