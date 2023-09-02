"use strict";
AssetsRegisterReportController.$inject = ["commonMessage", "$scope", "$rootScope", "$filter", "$http", "$controller", "$window", "baseService"];
function AssetsRegisterReportController(commonMessage, $scope, $rootScope, $filter, $http,  $controller, $window, baseService) {
    $rootScope.title = "Fixed Assets Register Report";
    $scope.path = 'FixedAssets/FixedAssetRegister/';
    
    $scope.report = {
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now())
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

    var getString = function (data, column) {
        var string = "''";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }

        return string;
    }

    $scope.FixedAssetRegisterReportExcel = function () {
        $scope.FromDateValidation();
        $scope.ToDatevalidation()
        if ($scope.form0.$valid && !$scope.invalidFromDate && !$scope.invalidDocDate && !$scope.validation() ) {
            var filtered = $("#GridFixedAssetRegisterReportElasticSearch").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                filtered = $scope.FixedAssetRegisterElasticSearchList;
            }
           
            var materialMasterId = getString(filtered, "MaterialMasterId");
            var materialMasterArticleId = getString(filtered, "MaterialMasterArticleId");
            var fixedAssetMasterId = getString(filtered, "FixedAssetMasterId");
            var vendorId = getString(filtered, "VendorId");

            try {
               
                var file_src = $scope.path + 'AssetRegisterReportExcel?materialMasterId=' + materialMasterId + '&materialMasterArticleId=' +materialMasterArticleId + '&fixedAssetMasterId=' + fixedAssetMasterId +
                    '&vendorId=' + vendorId 
                  
                $rootScope.report(file_src);

            } catch (e) {
               // ShowResult(e, 'failure');
                ShowResult(commonMessage.NetworkError, 'failure');
            }
        }
    }

    //for elastic search
    $scope.FixedAssetRegisterElasticSearchList = [];
    $scope.GetFixedAssetRegisterElasticSearchData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetAssetRegisterElasticSearchDataList",
                data: { 
                    fromDate: $scope.report.FromDate,
                    toDate: $scope.report.ToDate
                },
                dataType: 'JSON'

            }).then(function successCallback(response) {

                $scope.FixedAssetRegisterElasticSearchList = response.data.DATA;
            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }
        catch (e) {

        }
    }
    $scope.GetFixedAssetRegisterElasticSearchData();


    $scope.TotalFARegisterSummaryAmount = [{
        title: "Total", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "AssetAmount", dataMember: "AssetAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "DepreciationAmount", dataMember: "DepreciationAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "NetAmount", dataMember: "NetAmount", format: "{0:N2}" }
        ],
        showCaptionSummary: true
    }];



}