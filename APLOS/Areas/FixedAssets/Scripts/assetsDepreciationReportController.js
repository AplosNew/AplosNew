'use strict';
assetsDepreciationReportController.$inject = ['cboService', '$scope', '$rootScope', '$filter', "baseService", "$http", "$window"];
function assetsDepreciationReportController(cboService, $scope, $rootScope, $filter, baseService, $http, $window) {
    $rootScope.title = 'Capitalize Assets Depreciation Report';
   
    $scope.report = {
        ReportFormat: 'Excel',
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now()),
        AssetDepreciationId: null,
        ProcessName: null
    };
    $scope.DepreciationSearchBy = "AssetDepreciationId"; $scope.search = "";
    $scope.DepreciationSearchByList = [{ value: 'AssetDepreciationId', name: "Asset Depreciation Id" }, { value: 'ProcessName', name: "Process Name" }, { value: 'ProcessDate', name: "Depreciation Process Date" }];

    $scope.fixedAssetDepreciationList = [];
    $scope.getDepreciationData = function () {
        $http({
            method: 'Post'
            , url: 'FixedAssets/FixedAssetRegister/GetAssetDepreciationProcessList'
            , data: { column: $scope.DepreciationSearchBy, value: $scope.DepreciationSearch }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.fixedAssetDepreciationList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
        angular.element(document.querySelector('#DepreciationPopUp')).modal('show');
    };

    $scope.closeFixedAssetDepreciationPopUp = function () {
        angular.element(document.querySelector('#DepreciationPopUp')).modal('hide');
    }
    $scope.getDataByDepreciationId = function (x) {
        var data = x.data;
        $scope.report.AssetDepreciationId = data.AssetDepreciationId;
        $scope.report.ProcessName = data.AssetDepreciationId+"-"+data.ProcessName;
       
        angular.element(document.querySelector('#DepreciationPopUp')).modal('hide');
    };
    
    $scope.getReport = function () {
        if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
            manualValidation("div_FromDate", true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
            manualValidation("div_ToDate", true, "To Date is required.");
        }
        else if (new Date($scope.report.FromDate) > new Date($scope.report.ToDate)) {
            manualValidation("div_FromDate", true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.report.ToDate) < new Date($scope.report.FromDate)) {
            manualValidation("div_ToDate", true, "To date must be above or equal to From Date.");
        }
        else {
            $window.open('FixedAssets/FixedAssetRegister/GetAssetDepreciationReport?reportFormat=' + $scope.report.ReportFormat + '&fromdate=' + $scope.report.FromDate + '&todate=' + $scope.report.ToDate + '&assetDepreciationId=' + $scope.report.AssetDepreciationId, '_blank');
        }
    };

}

