'use strict';
partyReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster'];
function partyReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster) {
    $rootScope.title = "Party Report";
    $scope.path = 'Parties/party/';
    $scope.partyReport = {
        Type: null,
        CompanyGroupId: null,
        CompanyId: null,
        PlantId: null,
        CompanyGroupLevel: 'PlantLevel',
    };
    $scope.partyReportNew = angular.copy($scope.partyReport);

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });
    $scope.plantList = [];
    $scope.getPlantList = function () {
        if ($scope.PlantShow) {
            cboService.getCboPlantByCompany($scope.partyReportNew.CompanyId, function (result) {
                $scope.plantList = result;
            });
        }
    };
    $scope.CompanyGroupShow = true;
    $scope.CompanyShow = false;
    $scope.PlantShow = false;
    $scope.CompanyShowFn = function () {
        if ($scope.partyReportNew.CompanyGroupLevel == 'CompanyGroupLevel') {
            $scope.CompanyShow = false;
            $scope.CompanyGroupShow = true;
            $scope.PlantShow = false;
            $scope.partyReportNew.CompanyGroupId = null;
            $scope.partyReportNew.CompanyId = null;
            $scope.partyReportNew.PlantId = null;
            $scope.partyReportNew.Type = null;
            $scope.selectMessage = '';
        }
        else if ($scope.partyReportNew.CompanyGroupLevel == 'CompanyLevel') {
            $scope.CompanyShow = true;
            $scope.CompanyGroupShow = false;
            $scope.PlantShow = false;
            $scope.partyReportNew.CompanyGroupId = null;
            $scope.partyReportNew.CompanyId = null;
            $scope.partyReportNew.PlantId = null;
            $scope.partyReportNew.Type = null;
            $scope.selectMessage = '';
        } else {
            $scope.CompanyShow = true;
            $scope.CompanyGroupShow = false;
            $scope.PlantShow = true;
            $scope.partyReportNew.CompanyGroupId = null;
            $scope.partyReportNew.CompanyId = null;
            $scope.partyReportNew.PlantId = null;
            $scope.partyReportNew.Type = null;
            $scope.selectMessage = '';
        }
    }
    $scope.CompanyShowFn();

    $scope.selectMessage = '';
    $scope.partyReport = function () {
        if ($scope.partyReportNew.Type == null) {
            $scope.selectMessage = 'Select Party Type';
        }
        else if ($scope.partyReportNew.Type == 'Customer' & $scope.partyReportNew.CompanyGroupLevel == 'CompanyGroupLevel') {
            $scope.selectMessage = 'Customer Allow Only Company Level';
        }
        else if ($scope.partyReportNew.Type == 'Vendor' & $scope.partyReportNew.CompanyGroupLevel == 'CompanyGroupLevel') {
            $scope.selectMessage = 'Vendor Allow Only Company Level';
        }
        else if ($scope.partyReportNew.Type == 'Customer' & $scope.partyReportNew.CompanyId == null) {
            $scope.selectMessage = 'Select Company';
        }
        else if ($scope.partyReportNew.Type == 'Vendor' & $scope.partyReportNew.CompanyId == null) {
            $scope.selectMessage = 'Select Company';
        }
        else if ($scope.partyReportNew.Type == 'Both' & $scope.partyReportNew.CompanyGroupLevel == 'CompanyGroupLevel' & $scope.partyReportNew.CompanyGroupId == null) {
            $scope.selectMessage = 'Select Company Group';
        }
        else if ($scope.partyReportNew.Type == 'Both' & $scope.partyReportNew.CompanyGroupLevel == 'CompanyLevel' & $scope.partyReportNew.CompanyId == null) {
            $scope.selectMessage = 'Select Company';
        }
        else if ($scope.partyReportNew.CompanyGroupLevel == 'PlantLevel' & $scope.partyReportNew.CompanyId == null) {
            $scope.selectMessage = 'Select Company';
        }
        else if ($scope.partyReportNew.CompanyGroupLevel == 'PlantLevel' & $scope.partyReportNew.PlantId == null) {
            $scope.selectMessage = 'Select Plant';
        }
        else {
            $scope.selectMessage = '';
            location.href = 'Parties/party/GetPartyReport?type=' + $scope.partyReportNew.Type + ' &companyGroupId=' + $scope.partyReportNew.CompanyGroupId + ' &companyId=' + $scope.partyReportNew.CompanyId + ' &plantId=' + $scope.partyReportNew.PlantId;
        }
    };
};