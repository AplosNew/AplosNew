'use strict';
ComplianceDocumentReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService'];
function ComplianceDocumentReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService) {
    $rootScope.title = 'Compliance Document Report';
    $scope.path = 'employees/complianceDocument/';

    // #region ****Scope Compliance Document Report***
    $scope.complianceDocumentReport = {
        DocumentLevel: 'Document',
        CompanyId: null,
        PlantId: null
    };

    //Default Current Date selected
    $('.datepicker').datepicker({
        forceParse: false,
        format: 'dd-M-yyyy', autoclose: true, reset: true, todayHighlight: true, setDate: new Date()
    });
    // #endregion

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.companyOnChange = function () {
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.complianceDocumentReport.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }

    $scope.DocumentHideShowFn = function () {
        if ($scope.complianceDocumentReport.DocumentLevel === 'PlantWiseDocumentSet') {
            $scope.CompanyHideShow = true;
            $scope.complianceDocumentReport.CompanyId = null;
            $scope.complianceDocumentReport.PlantId = null;
            $scope.selectMessageCompany = '';
            $scope.selectMessagePlant = '';
        }
        else {
            $scope.CompanyHideShow = false;
            $scope.complianceDocumentReport.CompanyId = null;
            $scope.complianceDocumentReport.PlantId = null;
            $scope.selectMessageCompany = '';
            $scope.selectMessagePlant = '';
        }

    }
    $scope.DocumentHideShowFn();

    // #region *****Report*******
    $scope.complianceDocumentReport = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form.$valid) {
            if ($scope.complianceDocumentReport.DocumentLevel === 'PlantWiseDocumentSet' & $scope.complianceDocumentReport.CompanyId === null & $scope.complianceDocumentReport.PlantId === null) {
                $scope.selectMessageCompany = 'Company is required';
                $scope.selectMessagePlant = 'Plant is required';
            }
            else if ($scope.complianceDocumentReport.DocumentLevel == 'PlantWiseDocumentSet' & $scope.complianceDocumentReport.CompanyId == null) {
                $scope.selectMessageCompany = 'Company is required';
                $scope.selectMessagePlant = '';
            }
            else if ($scope.complianceDocumentReport.DocumentLevel === 'PlantWiseDocumentSet' & $scope.complianceDocumentReport.PlantId === null) {
                $scope.selectMessageCompany = '';
                $scope.selectMessagePlant = 'Plant is required';
            }
            else {
                location.href = 'employees/compliancedocument/compliancedocumentreport?documentLevel=' + $scope.complianceDocumentReport.DocumentLevel + '&plantId=' + $scope.complianceDocumentReport.PlantId;

                $scope.selectMessageCompany = '';
                $scope.selectMessagePlant = '';
            }
        }
    };

    // #endregion
}