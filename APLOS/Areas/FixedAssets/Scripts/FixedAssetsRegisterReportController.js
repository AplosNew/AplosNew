"use strict";
FixedAssetsRegisterReportController.$inject = ["commonMessage", "$scope", "$rootScope", "$filter", "$http", "$controller", "$window", "baseService"];
function FixedAssetsRegisterReportController(commonMessage, $scope, $rootScope, $filter, $http,  $controller, $window, baseService) {
    $rootScope.title = "Fixed Assets Register Report";
    $scope.path = 'FixedAssets/FixedAssetRegister/';
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
        EntityId: null
    };

    //For From date and To date
    //$scope.FixedAssetRegisterReportExcel = function () {

    //    try {

    //        if (angular.isUndefinedOrNull($scope.reportParameters.FromDate))
    //            throw 'Please enter from date';



    //        if (angular.isUndefinedOrNull($scope.reportParameters.ToDate))
    //            throw 'Please enter to date';



    //        //var MasterLCList = "";
    //        //for (var i = 0; i < $scope.MasterLCList.length; i++) {
    //        //    if ($scope.MasterLCList[i].isSelected == true) {
    //        //        if (MasterLCList == "")
    //        //            MasterLCList = "'" + $scope.MasterLCList[i].Id + "'";
    //        //        else
    //        //            MasterLCList += ",'" + $scope.MasterLCList[i].Id + "'";
    //        //    }
    //        //}

    //        var file_src = $scope.path + 'FixedAssetRegisterReportExcel?PartyType=' + $scope.report.PartyType + '&PartyId=' + $scope.report.PartyId + '&MaterialMasterId=' + $scope.report.MaterialMasterId +
    //            '&FixedAssetsId=' + $scope.report.FixedAssetMasterId;
    //        $rootScope.report(file_src);

    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }
    //}

   
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



    //$scope.getReport = function () {
    //    if (baseService.isUndefinedOrNull($scope.report.BankMasterId)) {
    //        manualValidation("div_Bank", true, "Bank is required.");
    //    }
    //    else if (baseService.isUndefinedOrNull($scope.report.FromDate)) {
    //        manualValidation("div_FromDate", true, "From Date is required.");
    //    }
    //    else if (baseService.isUndefinedOrNull($scope.report.ToDate)) {
    //        manualValidation("div_ToDate", true, "To Date is required.");
    //    }
    //    else if (new Date($scope.report.FromDate) > new Date($scope.report.ToDate)) {
    //        manualValidation("div_FromDate", true, "From date must be below or equal to To Date");
    //    }
    //    else if (new Date($scope.report.ToDate) < new Date($scope.report.FromDate)) {
    //        manualValidation("div_ToDate", true, "To date must be above or equal to From Date.");
    //    }
    //    else {
    //        var url = "Banks/BankReport/GetBankLedgerReport?reportFormat=" + $scope.report.ReportFormat + "&fromDate=" + $scope.report.FromDate + "&toDate=" + $scope.report.ToDate + "&bankMasterId=" + $scope.report.BankMasterId;
    //        $window.open(url, "_blank");
    //    }
    //};

   
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

    $scope.FixedAssetRegisterReportExcel = function () {
        $scope.FromDateValidation();
        $scope.ToDatevalidation()
        if ($scope.form0.$valid && !$scope.invalidFromDate && !$scope.invalidDocDate && !$scope.validation() ) {


            var filtered = $("#GridFixedAssetRegisterReportElasticSearch").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                filtered = $scope.FixedAssetRegisterElasticSearchList;
            }
            //filtered = ej.DataManager(filtered).executeLocal(ej.Query().select(["AccountGroupName"]));
            var materialMasterId = getString(filtered, "MaterialMasterId");
            var materialMasterArticleId = getString(filtered, "MaterialMasterArticleId");
            var fixedAssetMasterId = getString(filtered, "FixedAssetMasterId");
            var vendorId = getString(filtered, "VendorId");
           // var isAsset = getString(filtered, "IsAsset");
           // var machine = getString(filtered, "Machine");


            try {
                //var file_src = $scope.path + 'FixedAssetRegisterReportExcel?PartyType=' + $scope.report.PartyType + '&PartyId=' + $scope.report.PartyId + '&MaterialMasterId=' + $scope.report.MaterialMasterId +
                //    '&FixedAssetsId=' + $scope.report.FixedAssetMasterId + '&FromDate=' + $scope.report.FromDate + '&ToDate=' + $scope.report.ToDate;
                //$rootScope.report(file_src);

                var file_src = $scope.path + 'FixedAssetRegisterReportExcel?materialMasterId=' + materialMasterId + '&materialMasterArticleId=' +materialMasterArticleId + '&fixedAssetMasterId=' + fixedAssetMasterId +
                    '&vendorId=' + vendorId 
                    //'&FromDate=' + $scope.report.FromDate + '&ToDate=' + $scope.report.ToDate
                    ;
                //var file_src = $scope.path + 'FixedAssetRegisterReportExcel' 
                $rootScope.report(file_src);

            } catch (e) {
               // ShowResult(e, 'failure');
                ShowResult(commonMessage.NetworkError, 'failure');
            }
        }
    }

    //$scope.exportgriddataUrl = 'Accounts/AccountStatusDashboard/FixedAssetRegisterReportExcel';
    //$scope.downloadgriddataUrl = 'Accounts/AccountStatusDashboard/Download';
    //$scope.GetTrialBLAccountGroupReport = function () {
    //    try {
                //var filtered = $("#GridFixedAssetRegisterReportElasticSearch").data("ejGrid").getFilteredRecords();
                //if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                //    filtered = $scope.FixedAssetRegisterElasticSearchList;
                //}
                ////filtered = ej.DataManager(filtered).executeLocal(ej.Query().select(["AccountGroupName"]));
                //var materialMasterId = getString(filtered, "MaterialMasterId");
                //var materialMasterArticleId = getString(filtered, "MaterialMasterArticleId");
                //var fixedAssetMasterId = getString(filtered, "FixedAssetMasterId");
                //var vendorId = getString(filtered, "VendorId");

                //        $scope.fileName = $scope.report.AssetsLiability + ".xls";

                //        $http({
                //            method: 'POST',
                //            // url: 'Attendances/DailyAttendanceReport/DailyAttendanceStatusReport',
                //            url: $scope.exportgriddataUrl,
                //            data: {
                //                'allAccountGroupList': AccountGroupNames
                //                //"voucherDetailVMList": JSON.stringify($scope.voucherDetailList)
                //                , 'toDate': $scope.report.ToDate
                //                , 'reportName': $scope.report.AssetsLiability
                //                , 'isDetailLevel': $scope.report.IsDetailLevel
                 
                //            },
                //            dataType: 'JSON'
                //            , contentType: "application/json charset=utf-8"

                //        }).then(function successCallback(response) {
                //            if (response.data.Error === true) {
                //                ShowResult(response.data.Message, 'failure');
                //            }
                //            else {
                //                //$rootScope.report($scope.downloadgriddataUrl + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath
                //                $rootScope.report($scope.downloadgriddataUrl);//downloadgriddataUrlPath
                //            }
                //        });
            //} catch (e) {
            //        ShowResult(e, 'failure');
            //    }
    //};




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
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetFixedAssetRegisterElasticSearchDataList",
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
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "FABaseAmount", dataMember: "FABaseAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "SubAssetAmount", dataMember: "SubAssetAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TotalBaseAmount", dataMember: "TotalBaseAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "ADBaseAmount", dataMember: "ADBaseAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "NetFixedAssetsAmount", dataMember: "NetFixedAssetsAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "FACount", dataMember: "FACount" }

        ],
        showCaptionSummary: true
    }];




    //$scope.EntityFixedAssetRegisterList = [];
    //$scope.GetEntityFixedAssetRegisterData = function () {
    //    try {

    //        var filtered = $("#GridEntityFixedAssetRegisterElasticSearch").data("ejGrid").getFilteredRecords();
    //        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
    //            filtered = $scope.EntityFixedAssetRegisterElasticSearchList;
    //        }
    //        //filtered = ej.DataManager(filtered).executeLocal(ej.Query().select(["AccountGroupName"]));
    //        var materialMasterId = getString(filtered, "MaterialMasterId");
    //        var materialMasterArticleId = getString(filtered, "MaterialMasterArticleId");
    //        var fixedAssetMasterId = getString(filtered, "FixedAssetMasterId");
    //        var vendorId = getString(filtered, "VendorId");
    //        var isAsset = getString(filtered, "IsAsset");
    //        var machine = getString(filtered, "Machine");

    //        $http({
    //            method: 'POST',
    //            url: $scope.path + "GetEntityFixedAssetRegisterDataList",
    //            data: { /*FromDate: $scope.reportParameters.FromDate, ToDate: $scope.reportParameters.ToDate*/
    //                'materialMasterId': materialMasterId,
    //                'materialMasterArticleId': materialMasterArticleId,
    //                'fixedAssetMasterId': fixedAssetMasterId,
    //                'vendorId': vendorId


    //            },
    //            dataType: 'JSON'

    //        }).then(function successCallback(response) {

    //            $scope.EntityFixedAssetRegisterList = response.data.DATA;
    //        }),
    //            function errorCallBack(response) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //    }
    //    catch (e) {

    //    }
    //}


}