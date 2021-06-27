'use strict';
prdOrdSettingController.$inject = ['cboService', "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function prdOrdSettingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'Setups/prdordsetting/';
    $scope.getListUrl = "Setups/prdordsetting/GetList/";
    $scope.mainList = [];
    $scope.companyGroupList = [];
    $scope.companyList = [];
    $scope.plantList = [];
    $scope.getData = function () {
        $http({
            method: 'GET',
            url: 'Setups/prdordsetting/GetList?groupId=' + $scope.model.CompanyGroupId + '&companyId=' + $scope.model.CompanyId + '&plantId=' + $scope.model.PlantId
        }).then(function successCallback(response) {
            $scope.mainList = response.data;
        });
    };

    $scope.model = {
        CompanyGroupId: null
        , CompanyId: null
        , PlantId: null
    };

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    $scope.getCboCompanyByCompanyGroup = function () {
        cboService.getCboCompanyByCompanyGroup($scope.model.CompanyGroupId, function (result) {
            $scope.companyList = result;
        });
    };
    //For Plant
    $scope.getPlantList = function () {
        cboService.getCboPlantByCompany($scope.model.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }

    $scope.Save = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + 'Create',
                data: $scope.mainList
            }).then(function successCallback(response) {
                if (response.data.Error == true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    }

    $scope.Clear = function () {
        $scope.model = {};
        $scope.mainList = [];
        $scope.companyGroupList = [];
        $scope.companyList = [];
        $scope.plantList = [];
    }
}