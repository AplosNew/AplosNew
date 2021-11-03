'use strict';
vendorTranTypeController.$inject = ['cboService', '$route', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function vendorTranTypeController(cboService, $route, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Vendor Tran Type';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.DebitNoteTypes = [];
    $scope.sourceType = 'Vendor';
    $scope.path = 'accounts/FinancingType/';
    $scope.getUrl = $scope.path + 'GetFinancingType';
    $scope.saveUrl = $scope.path + 'CreateVendorTranType';
    $scope.updateUrl = $scope.path + 'EditVendorTranType';
    $scope.deleteUrl = $scope.path + 'DeleteFinancingType/';
    baseService.init('accounts/FinancingType/GetVendorTranTypeList');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.vendorTranTypes = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.vendorTranType = {
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
        PartyType:'Vendor'
    };

    $scope.getSequence = function () {
        cboService.getSequence('accounts/FinancingType/GetFinancingTypeAutoSequence?type=' + $scope.sourceType, function (result) {
            $scope.vendorTranType.Sequence = result;
        });
    };
    $scope.getSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.vendorTranType = baseService.find($scope.vendorTranTypes, id, null);
        $scope.vendorTranType.AddedDate = $filter('dateFilter')($scope.vendorTranType.AddedDate);
        $scope.vendorTranType.UpdatedDate = $filter('dateFilter')($scope.vendorTranType.UpdatedDate);
        $scope.vendorTranType.IsOthers = true;
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
                    data: $scope.vendorTranType,
                    dataType: 'JSON'
                }).then(
                    function success(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.vendorTranTypes.push(response.data.ModelData);
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
                    data: $scope.vendorTranType,
                    dataType: 'JSON'
                }).then(function success(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.vendorTranTypes[$scope.index] = $scope.vendorTranType;
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
        if (!baseService.isUndefinedOrNull($scope.vendorTranType.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.vendorTranType.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.vendorTranTypes.splice($scope.index, 1);
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
        $scope.vendorTranType = {};
        $scope.vendorTranType.Sequence = seq;
        $scope.vendorTranType.Active = true;
        $scope.vendorTranType.PartyType = 'Vendor';
    }
}