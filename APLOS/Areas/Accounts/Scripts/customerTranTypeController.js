'use strict';
customerTranTypeController.$inject = ['cboService', '$route', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function customerTranTypeController(cboService, $route, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Customer Tran Type';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.DebitNoteTypes = [];
    $scope.sourceType = 'DebitNote';
    $scope.path = 'accounts/FinancingType/';
    $scope.getUrl = $scope.path + 'GetFinancingType';
    $scope.saveUrl = $scope.path + 'CreateCustomerTranType';
    $scope.updateUrl = $scope.path + 'EditCustomerTranType';
    $scope.deleteUrl = $scope.path + 'DeleteFinancingType/';
    baseService.init('accounts/FinancingType/GetCustomerTranTypeList');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.customerTranTypes = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.customerTranType = {
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
        PartyType: 'Customer'
    };

    $scope.getSequence = function () {
        cboService.getSequence('accounts/FinancingType/GetFinancingTypeAutoSequence?type=' + $scope.sourceType, function (result) {
            $scope.customerTranType.Sequence = result;
        });
    };
    $scope.getSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.customerTranType = baseService.find($scope.customerTranTypes, id, null);
        $scope.customerTranType.AddedDate = $filter('dateFilter')($scope.customerTranType.AddedDate);
        $scope.customerTranType.UpdatedDate = $filter('dateFilter')($scope.customerTranType.UpdatedDate);
        $scope.customerTranType.IsOthers = true;
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
                    data: $scope.customerTranType,
                    dataType: 'JSON'
                }).then(
                    function success(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.customerTranTypes.push(response.data.ModelData);
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
                    data: $scope.customerTranType,
                    dataType: 'JSON'
                }).then(function success(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.customerTranTypes[$scope.index] = $scope.customerTranType;
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
        if (!baseService.isUndefinedOrNull($scope.customerTranType.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.customerTranType.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.customerTranTypes.splice($scope.index, 1);
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
        $scope.customerTranType = {};
        $scope.customerTranType.Sequence = seq;
        $scope.customerTranType.Active = true;
        $scope.customerTranType.PartyType = 'Customer';
    }
}