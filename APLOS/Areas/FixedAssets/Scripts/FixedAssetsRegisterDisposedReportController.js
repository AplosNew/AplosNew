"use strict";
FixedAssetsRegisterDisposedReportController.$inject = ["commonMessage", "$scope", "$rootScope", "$filter", "$http", "$controller", "$window", "baseService"];
function FixedAssetsRegisterDisposedReportController(commonMessage, $scope, $rootScope, $filter, $http,  $controller, $window, baseService) {
    $rootScope.title = "Fixed Assets Register Disposed Report";
    $scope.path = 'FixedAssets/FixedAssetRegister/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.partyType = 'Vendor'; 
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.materialMasterList1 = [];
    $scope.report = {
       
      //  ReportFormat: "Pdf",

        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now()),

        PartyType: 'All', //?
        PartyId: null,  
        //VendorId: null,
        FixedAssetMasterId: null,
        MaterialMasterId: null,
        CapitalizationDate: null,

        partyType: 'All',  
        PartyName: null,
        MaterialMasterName: null,
        FixedAssetMasterName: null,
        EntityId: null,
        NonPosted: null,
        Posted: null
    };

   

   
    // Parties/party

    $scope.closePartyPopUp = function myfunction() {
        if ($scope.partyIndex !== -1) {
            var data = $scope.partyList[$scope.partyIndex];
            if ($scope.report.PartyType === 'Customer') {
                $scope.report.vendorId = null;
                $scope.report.PartyId = data.PartyId;
                $scope.customerNameCode = data.Code + ' - ' + data.PartyName;
            }
            else if ($scope.report.PartyType === 'Vendor') {
                $scope.report.PartyId = null;
                $scope.report.PartyId = data.PartyId;
                $scope.report.PartyName = data.Code + ' - ' + data.PartyName;
            }

            else {
                $scope.report.PartyId = null;
                $scope.report.vendorId = null;
                $scope.report.MainPartyId = data.PartyId;
                $scope.customerNameCode = data.Code + ' - ' + data.PartyName;
            }
        }
        $scope.hidePartyPopUp();
    };

    $scope.setMaterialMasterData = function (ob) {
        $scope.report.MaterialMasterId = ob.Id;
        $scope.report.MaterialMasterName = ob.UserName;
        angular.element(document.querySelector('#materialmastersearchpopup')).modal('hide');
    };
     //fixedAssetsPopUp 
    //$scope.setFixedAssetsData = function (ob) {
    //    $scope.report.FixedAssetMasterId = ob.Id;
    //    $scope.report.MaterialMasterName = ob.UserName;
    //    angular.element(document.querySelector('#fixedAssetsPopUp')).modal('hide');
    //};
    
   
    // clearVendor Method for Refresh
    $scope.clearVendor = function () {
        //$scope.purchaseLCNew.VendorId = null;
        //$scope.purchaseLCNew.PartyCode = null;
        //$scope.purchaseLCNew.PartyName = null;

        $scope.report.FixedAssetMasterName = null;
        $scope.report.MaterialMasterName = null;
        $scope.report.PartyName = null;

    }

    //FixedAssetsRegister validation
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

    $scope.validation = function () {
        if ($scope.report.PartyType === "Vendor" && baseService.isUndefinedOrNull($scope.report.PartyId)) {
            ShowResult("Please select Vendor", "failure");
            return true;
        }
        if ($scope.report.PartyType === "FixedAsset" && baseService.isUndefinedOrNull($scope.report.FixedAssetMasterId)) {
            ShowResult("Please select Fixed Asset Master", "failure");
            return true;
        }
        if ($scope.report.PartyType === "MaterialMaster" && baseService.isUndefinedOrNull($scope.report.MaterialMasterId)) {
            ShowResult("Please select Material ", "failure");
            return true;
        }

        return false;
    };


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
    $scope.fixedAssetDisposeStatusList = [];
    $http({
        method: 'GET',
        url: 'Enum/GetFixedAssetDisposeStatusEnumCbo/'
    }).then(function successCallback(response) {
        $scope.fixedAssetDisposeStatusList = response.data;
    });
    $scope.FixedAssetRegisterDisposedReportExcel = function () {
        $scope.FromDateValidation();
        $scope.ToDatevalidation();
        $scope.fileName = 'Fixed Assets Register Disposed Report.xls';
        if ($scope.form0.$valid && !$scope.invalidFromDate && !$scope.invalidDocDate && !$scope.validation() ) {

            try {
                var DropDownListObj = $("#FixedAssetDisposeStatusList").data("ejDropDownList");
                var disposeStatus = DropDownListObj.getSelectedValue();
                if ($scope.report.NonPosted === null && $scope.report.Posted === null) {
                    ShowResult("Please Select Type(Posted,Non Posted) ", "failure");
                    return true;
                }
                if ($scope.report.NonPosted === false && $scope.report.Posted === false) {
                    ShowResult("Please Select Type(Posted,Non Posted) ", "failure");
                    return true;
                }
                if (disposeStatus === "") {
                    ShowResult("Please Select Disposal Type ", "failure");
                    return true;
                }
                

                var file_src = $scope.path + 'FixedAssetRegisterDisposedReportExcel?fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&nonPosted=' + $scope.report.NonPosted +
                    '&posted=' + $scope.report.Posted + '&disposeStatus=' + disposeStatus
                $rootScope.report(file_src);

            } catch (e) {
               // ShowResult(e, 'failure');
                ShowResult(commonMessage.NetworkError, 'failure');
            }
        }
    }

    //GatenntryRegisterListPdf
    $scope.FixedAssetRegisterReportPdf = function () {

        try {
            var file_src = $scope.path + "FixedAssetRegisterReportPdf";
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    //ng-change for partyTypeChange
    $scope.partyTypeChange = function (partyType) {
        //Database table  
        $scope.report.PartyType = partyType; 
        $scope.report.PartyId = '';
        $scope.report.MaterialMasterId = '';
        $scope.report.FixedAssetMasterId = ''; //??
        //input type ng-model
        $scope.report.partyType = partyType;
        $scope.report.PartyName = '';
        $scope.report.MaterialMasterName = '';
        $scope.report.FixedAssetMasterName = '';
    }

    $scope.searchByMaterialMasterModalList = [
        {
            "name": "Asset Category",
            "value": "FixedAssetCategory"
        }
        ,
        {
            "name": "Asset Sub Category",
            "value": "FixedAssetSubCategory"
        },
        {
            "name": "Asset Master",
            "value": "FixedAssetMasterName"
        }
    ];

    $scope.searchMaterialMasterParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "FixedAssetMasterName",
        searchBy: "FixedAssetMasterName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

   // popUp and get data
    $scope.getFixedAssetData = function () {
        var url1 = "FixedAssets/FixedAssetMaster/GetFixedAssetMasterData";
        baseService.setCurrentPage("materialMasterList1");
        //for search loard
        $scope.loadMaterialMasterModalList = function (pageno) {
            baseService.paginationBase(url1, pageno, $scope.searchMaterialMasterParameters)
                .then(function (result) {
                    $scope.materialMasterList1 = result.Rows;
                    $scope.searchMaterialMasterParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
            angular.element(document.querySelector("#assetmastermodal")).modal("show");
        };
        $scope.loadMaterialMasterModalList();
    };

    // for close (crose*)
    $scope.closeFixedAssetPopUp = function () {
        angular.element(document.querySelector("#assetmastermodal")).modal("hide");
    }
    // select data double click
    $scope.selectFixedAssetMaster = function (data) {
        $scope.report.FixedAssetMasterName = data.FixedAssetMasterName;
        $scope.report.FixedAssetMasterId = data.FixedAssetMasterId;
        angular.element(document.querySelector("#assetmastermodal")).modal("hide");

    };


    //for elastic search
    $scope.FixedAssetRegisterElasticSearchList = [];
    $scope.GetFixedAssetRegisterElasticSearchData = function () {
        $scope.FromDateValidation();
        $scope.ToDatevalidation();
        if ($scope.form0.$valid && !$scope.invalidFromDate && !$scope.invalidDocDate && !$scope.validation()) {
            try {
                var DropDownListObj = $("#FixedAssetDisposeStatusList").data("ejDropDownList");
                var disposeStatus = DropDownListObj.getSelectedValue();
                if ($scope.report.NonPosted === null && $scope.report.Posted === null) {
                    ShowResult("Please Select Type(Posted,Non Posted) ", "failure");
                    return true;
                }
                if ($scope.report.NonPosted === false && $scope.report.Posted === false) {
                    ShowResult("Please Select Type(Posted,Non Posted) ", "failure");
                    return true;
                }
                if (disposeStatus === "") {
                    ShowResult("Please Select Disposal Type ", "failure");
                    return true;
                }
                
                $http({
                    method: 'POST',
                    url: $scope.path + "GetFixedAssetRegisterDisposedElasticSearchDataList",
                    data: {
                        fromDate: $scope.report.FromDate,
                        toDate: $scope.report.ToDate,
                        nonPosted: $scope.report.NonPosted,
                        posted: $scope.report.Posted,
                        disposeStatus: disposeStatus

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
    }
    //$scope.GetFixedAssetRegisterElasticSearchData();


    $scope.TotalFARegisterSummaryAmount = [{
        title: "Total", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "FABaseAmount", dataMember: "FABaseAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "SubAssetAmount", dataMember: "SubAssetAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalBaseAmount", dataMember: "TotalBaseAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "ADBaseAmount", dataMember: "ADBaseAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "NetFixedAssetsAmount", dataMember: "NetFixedAssetsAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "FACount", dataMember: "FACount" }

        ],
        showCaptionSummary: true
    }];


}