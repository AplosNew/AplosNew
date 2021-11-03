'use strict';
creditNoteTypeController.$inject = ['cboService', '$route', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function creditNoteTypeController(cboService, $route, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Credit Note';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.creditNoteTypes = [];
    $scope.sourceType = 'CreditNote';
    $scope.path = 'accounts/FinancingType/';
    $scope.getUrl = $scope.path + 'GetFinancingType';
    $scope.saveUrl = $scope.path + 'CreateCreditNoteType';
    $scope.updateUrl = $scope.path + 'EditCreditNoteType';
    $scope.deleteUrl = $scope.path + 'DeleteFinancingType/';
    baseService.init('accounts/FinancingType/GetCreditNoteTypeList');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.creditNoteTypes = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.creditNoteType = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        AssetUserName: null,
        LiabilityUserName: null,
        SourceType: null,
        IsInterCompany: null,
        IsInterPlant: null,
        IsOthers: true,
        Description: null,
        Remarks: null,
        Active: true,
        PartyType: null
    };

    $scope.getSequence = function () {
        cboService.getSequence('accounts/FinancingType/GetFinancingTypeAutoSequence?type=' + $scope.sourceType, function (result) {
            $scope.creditNoteType.Sequence = result;
        });
    };
    $scope.getSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.creditNoteType = baseService.find($scope.creditNoteTypes, id, null);
        $scope.creditNoteType.AddedDate = $filter('dateFilter')($scope.creditNoteType.AddedDate);
        $scope.creditNoteType.UpdatedDate = $filter('dateFilter')($scope.creditNoteType.UpdatedDate);
        $scope.creditNoteType.IsOthers = true;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.moduleForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.creditNoteType,
                    dataType: 'JSON'
                }).then(
                    function success(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.creditNoteTypes.push(response.data.ModelData);
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
                    data: $scope.creditNoteType,
                    dataType: 'JSON'
                }).then(function success(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.creditNoteTypes[$scope.index] = $scope.creditNoteType;
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
        if (!baseService.isUndefinedOrNull($scope.creditNoteType.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.creditNoteType.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.creditNoteTypes.splice($scope.index, 1);
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
        $scope.creditNoteType = {};
        $scope.creditNoteType.Sequence = seq;
        $scope.creditNoteType.Active = true;
    }
}