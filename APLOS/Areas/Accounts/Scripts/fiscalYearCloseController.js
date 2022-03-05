'use strict';
FiscalYearCloseController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function FiscalYearCloseController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Fiscal Year Close';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.FiscalYearCloseList = [];
    $scope.path = 'accounts/FiscalYearClose/';
    $scope.getVouchetTypeConfigListUrl = $scope.path + 'GetFiscalYearCloseList';
    $scope.saveUrl = $scope.path + 'CreateFiscalYearClose';
    $scope.updateUrl = $scope.path + 'UpdateFiscalYearClose';
    $scope.deleteUrl = $scope.path + 'DeleteFiscalYearClose/';

    $scope.FiscalYearClose = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null,
        FiscalYearId: null
    };

    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.plantList = [];
    $scope.getPlantList = function () {
        cboService.getCboPlantByCompany($scope.FiscalYearClose.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };

    $scope.fiscalYearList = [];
    $http({
        method: 'GET',
        url: 'accounts/FiscalYear/GetCbo'
    }).then(function successCallback(response) {
        $scope.fiscalYearList = response.data;
    });

    

    $scope.getPadding = function () {
        $scope.paddingList = [];
        for (var i = 0; i < 10; i++) {
            $scope.paddingList.push({
                'Text': i,
                'Value': i
            });
        }
    };
    $scope.getPadding();

    $scope.searchByList = [
        {
            'name': 'Fiscal Year',
            'value': 'FiscalYearName'
        }
        ,
        {
            'name': 'Company',
            'value': 'CompanyName'
        }
        ,
        {
            'name': 'Plant',
            'value': 'PlantName'
        }
    ];

    $scope.getListData = function () {
        baseService.init('accounts/FiscalYearClose/GetFiscalYearCloseList?companyId=' + $scope.FiscalYearClose.CompanyId + '&plantId=' + $scope.FiscalYearClose.PlantId, null, null, null, null, null);
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.FiscalYearCloseList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };
    $scope.getListData();
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.FiscalYearCloseForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'fiscalYearCloseVM': $scope.FiscalYearClose },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getListData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.FiscalYearClose.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.FiscalYearClose.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.FiscalYearCloseList.splice($scope.index, 1);
                    baseService.paginationRemove();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        $scope.Action = 'Save';
        $scope.FiscalYearClose = {
            CompanyId: $scope.FiscalYearClose.CompanyId
            , PlantId: $scope.FiscalYearClose.PlantId
        };
    };
}