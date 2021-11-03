'use strict';
BusinessProcessController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function BusinessProcessController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "BusinessProcess";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.brands = [];
    $scope.path = 'Setups/businessprocess/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl, null, null, null, 'UserName', 'UserName');

    $scope.brand = {
        Id: null
        ,CompanyGroupId: null
        ,BusinessProcessName: null
        ,UserName: null
        ,Type: null
    };
    angular.copy($scope.brand, $scope.brandNew);
    $rootScope.searchByList = [
        {
            'name': 'Id',
            'value': 'Id'
        },
        {
            'name': 'Business Process',
            'value': 'BusinessProcessName'
        },
        {
            'name': 'User Define Name',
            'value': 'UserName'
        }
    ];
    $scope.getData = function (pageno) {
        $rootScope.parameters.companyGroupId = $scope.brandNew.CompanyGroupId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.brands = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });
    cboService.getEnumCbo("enum/getbusinessprocess", function (result) {
        $scope.bProcessList = result;
    });

    $scope.Get = function (id, index) {
        $scope.index = index;
        angular.copy($scope.brands[$scope.index], $scope.brand);
        angular.copy($scope.brand, $scope.brandNew);
        $scope.Action = 'Update';
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.brandForm.$valid) {
            angular.copy($scope.brandNew, $scope.brand);
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.brand,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields();
                    }
                }), function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.brand,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.brandNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.brandNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.brands.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                } function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.brand = {};
        $scope.brandNew = { CompanyGroupId: $scope.brandNew.CompanyGroupId};
    }
}