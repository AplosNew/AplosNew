"use strict";
AssetsRegisterReportController.$inject = ["commonMessage", "$scope", "$rootScope", "$filter", "$http", "$controller", "$window", "baseService"];
function AssetsRegisterReportController(commonMessage, $scope, $rootScope, $filter, $http,  $controller, $window, baseService) {
    $rootScope.title = "Capitalize Assets Register Report";
    $scope.path = 'FixedAssets/FixedAssetRegister/';
    $scope.exportgriddataUrlUpdate2 = 'GridReports/ExcelExportUpdate2';
    $scope.downloadgriddataUrl2 = 'GridReports/Download';
    
    $scope.report = {
        ReportFormat: 'Excel',
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now()),
        IsDynamic:true
    };

    $scope.invalidDocDate = false;
    $scope.ToDatevalidation = function() {
        var msg = "";

        if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
            $scope.invalidDocDate = true;
            msg = "Please select To Date!";
        }
        else if (new Date($scope.report.ToDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "ToDate must be below or equal to current Date!";
        }
        else if (new Date($scope.report.FromDate) > new Date($scope.report.ToDate)) {
            msg = "To Date must be greater or equal to FromDate!";
            $scope.invalidDocDate = true;
        }
        else $scope.invalidDocDate = false;
        return manualValidation("div_ToDate", $scope.invalidDocDate, msg);
    }

    $scope.invalidFromDate = false;
    $scope.FromDateValidation = function () {
        var msg = "";
        if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
            $scope.invalidFromDate = true;
            msg = "Please select From Date!";
        }
        else if (new Date($scope.report.FromDate) > new Date()) {
            $scope.invalidFromDate = true;
            msg = "FromDate must be below or equal to current Date!";
        }
        else $scope.invalidFromDate = false;
       return manualValidation("div_FromDate", $scope.invalidFromDate, msg);
    }

    $scope.FixedAssetRegisterReportExcel = function () {
        $scope.FromDateValidation();
        $scope.ToDatevalidation()
        if ($scope.form0.$valid && !$scope.invalidFromDate && !$scope.invalidDocDate) {
            var dataList = [];
            if ($scope.report.IsDynamic === true) {
                var g = $("#GridFixedAssetRegisterDynamicReport").data("ejGrid");
                dataList = g.getFilteredRecords();

                if (dataList.length == 0) {
                    dataList = $scope.FixedAssetRegisterDynamicList;
                }
            }
            else {
                var g = $("#GridFixedAssetRegisterReportElasticSearch").data("ejGrid");
                dataList = g.getFilteredRecords();

                if (dataList.length == 0) {
                    dataList = $scope.FixedAssetRegisterElasticSearchList;
                }
            }
            

            $scope.fileName = 'CapitalizeAssetRegisterReport';
            $http({
                method: 'POST',
                url: $scope.exportgriddataUrlUpdate2,
                data: {
                    'reportFileName': $scope.fileName,
                    'data': dataList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $window.open($scope.downloadgriddataUrl2 + "?FileName=" + response.data.FileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        }
    }

    //for elastic search
    $scope.FixedAssetRegisterDynamicList = [];
    $scope.FixedAssetRegisterElasticSearchList = [];
    $scope.pathReporturl = "";
    $scope.GetFixedAssetRegisterElasticSearchData = function () {
        $scope.FixedAssetRegisterDynamicList = [];
        $scope.FixedAssetRegisterElasticSearchList = [];
        try {
            if ($scope.report.IsDynamic === true) {
                $scope.pathReporturl = $scope.path + "GetCapitalizeAssetRegisterDynamicDataList";
            }
            else {
                $scope.pathReporturl = $scope.path + "GetAssetRegisterElasticSearchDataList";
            }
            $http({
                method: 'POST',
                url: $scope.pathReporturl,
                data: { 
                    fromDate: $scope.report.FromDate,
                    toDate: $scope.report.ToDate
                },
                dataType: 'JSON'

            }).then(function successCallback(response) {
                if ($scope.report.IsDynamic === true) {
                    $scope.FixedAssetRegisterDynamicList = response.data.DATA;
                }
                else {
                    $scope.FixedAssetRegisterElasticSearchList = response.data.DATA;
                }
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }
        catch (e) {

        }
    }
    //$scope.GetFixedAssetRegisterElasticSearchData();


    $scope.TotalFARegisterSummaryAmount = [{
        title: "Total", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "AssetAmount", dataMember: "AssetAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "AdditionAssetAmount", dataMember: "AdditionAssetAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalAmount", dataMember: "TotalAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "DepreciationAmount", dataMember: "DepreciationAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "AdditionDepreciationAmount", dataMember: "AdditionDepreciationAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "AdjustmentDepreciationAmount", dataMember: "AdjustmentDepreciationAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalDepreciation", dataMember: "TotalDepreciation", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "NetAmount", dataMember: "NetAmount", format: "{0:N2}" }
        ],
        showCaptionSummary: true
    }];

    $scope.TotalFARegisterDynamicSummaryAmount = [{
        title: "Total", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "OpeningAmount", dataMember: "OpeningAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "CapitalizedAmountFTP", dataMember: "CapitalizedAmountFTP", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalAmount", dataMember: "TotalAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "DepreciationAmount", dataMember: "DepreciationAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "DisposeAmount", dataMember: "DisposeAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "NetAmount", dataMember: "NetAmount", format: "{0:N2}" }
        ],
        showCaptionSummary: true
    }];

    $scope.FixedAssetFinancialRegisterReportExcel = function () {
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
            $window.open('FixedAssets/FixedAssetRegister/FixedAssetFinancialRegisterReport?reportFormat=' + $scope.report.ReportFormat + '&fromdate=' + $scope.report.FromDate + '&todate=' + $scope.report.ToDate, '_blank');
        }
    }

}