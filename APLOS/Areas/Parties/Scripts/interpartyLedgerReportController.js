'use strict';
interpartyLedgerReportController.$inject = ['cboService','commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller', '$window'];
function interpartyLedgerReportController(cboService,commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $window) {
    $rootScope.title = 'Inter Transaction Ledger';
    $scope.path = 'accounts/voucher/';
    $scope.glvoucherXLUrl = $scope.path + 'generateglvoucher';
    $scope.partyledgerreportXLUrl = $scope.path + 'partyledgerreport';
    $scope.generalVoucherXLUrl = 'accounts/voucher/generalvoucherreport';
    $scope.partyType = 'Vendor';
    $scope.isAdvance = false;
    $controller('partyBaseController', { $scope: $scope, $http: $http });

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    $scope.report = {
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        CompanyId: null,
        PlantId: null,
        ReportFormat: 'Excel',      
    };

    $scope.getReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.CompanyId)) {
            throw 'Please Select Company.';
        }
        if (baseService.isUndefinedOrNull($scope.report.PlantId)) {
            throw 'Please Select Plant.';
        }
        if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
            throw 'Please Select ToDate.';
        }
        if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
            throw 'Please Select FromDate.';
        }
        else {
            var url = 'Parties/PartyReport/GetInterPartyLedger?reportFormat=' + $scope.report.ReportFormat + '&CompanyId=' + $scope.report.CompanyId + '&PlantId=' + $scope.report.PlantId + '&toDate=' + $scope.report.ToDate + '&fromDate=' + $scope.report.FromDate;            
            $window.open(url, '_blank');
        }
    };

    $scope.company = null;
    $scope.getCompanyInfo = function (companyId) {
        if (!baseService.isUndefinedOrNull(companyId)) {
            $scope.company = $.grep($scope.companyList, function (item) {
                return item.CompanyId === companyId;
            })[0];
            if (manualValidation('div_Company', baseService.isUndefinedOrNull($scope.company.PartyId), 'This Company is not created as InterCompany Party.')) {
                $scope.company = null;
            }
        }
        else {
            manualValidation('div_Company', true, 'Company is required.');
            $scope.company = null;
        }
    };

    $scope.companyChange = function (companyId) {
        cboService.getCboInterPlant('', companyId, '', function (result) {
            $scope.interplantList = result;
        });
    };

    $scope.plant = null;
    $scope.getPlantInfo = function (plantId) {
        if (!baseService.isUndefinedOrNull(plantId)) {
            $scope.plant = $.grep($scope.interplantList, function (item) {
                return item.PlantId === plantId;
            })[0];
            if (manualValidation('div_Plant', baseService.isUndefinedOrNull($scope.plant.PartyPlantId), 'This Company is not created as InterCompany Party Plant.')) {
                $scope.plant = null;
            }
        }
        else {
            manualValidation('div_Plant', true, 'Plant is required.');
            $scope.plant = null;
        }
    };

    //$scope.companyList = [];
    cboService.getCboInterCompany(null, function (result) {
        $scope.companyList = result;
    });

  // $scope.interplantList = [];
    $scope.companyChange = function (companyId) {
        cboService.getCboInterPlant('', companyId, '', function (result) {
            $scope.interplantList = result;
        });
    };

}