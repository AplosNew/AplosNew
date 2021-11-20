"use strict";
AssetWIPStatusController.$inject = ["commonMessage", "$scope", "$rootScope", "$filter", "$http", "$controller", "$window", "baseService"];
function AssetWIPStatusController(commonMessage, $scope, $rootScope, $filter, $http, $controller, $window, baseService) {
    $rootScope.title = "Asset WIP Status";
    $scope.path = 'FixedAssets/AssetWIPStatus/';
    $scope.Voucherpath = 'Accounts/VoucherReport/';
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
        FixedAssetMasterName: null
    };

   
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
  
    $scope.clearVendor = function () {
      
        $scope.report.FixedAssetMasterName = null;
        $scope.report.MaterialMasterName = null;
        $scope.report.PartyName = null;

    }


    $scope.invalidDocDate = false;
    $scope.ToDatevalidation = function () {
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



    $scope.FixedAssetRegisterReportExcel = function () {
        $scope.FromDateValidation();
        $scope.ToDatevalidation()
        if ($scope.form0.$valid && !$scope.invalidFromDate && !$scope.invalidDocDate && !$scope.validation()) {


            try {
                var file_src = $scope.path + 'FixedAssetRegisterReportExcel?PartyType=' + $scope.report.PartyType + '&PartyId=' + $scope.report.PartyId + '&MaterialMasterId=' + $scope.report.MaterialMasterId +
                    '&FixedAssetsId=' + $scope.report.FixedAssetMasterId + '&FromDate=' + $scope.report.FromDate + '&ToDate=' + $scope.report.ToDate;
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

    $scope.AssetWIPstatusList = [];
    $scope.GetAssetWIPstatusList = function () {
            $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Accounts/VoucherReport/GetAssetWIPData',
            
        }).then(function successCallback(response) {           
            $scope.AssetWIPstatusList = response.data.DATA
        });
    }
    $scope.GetAssetWIPstatusList();


    $scope.TotalAssetWIPstatus = [{
        title: "Total", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TransactionQty", dataMember: "TransactionQty", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "TrnAmount", dataMember: "TrnAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BaseQty", dataMember: "BaseQty", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BooksAmount", dataMember: "BooksAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "IssueQty", dataMember: "IssueQty", format: "{0:N2}" },
            //{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "ADBaseAmount", dataMember: "ADBaseAmount", format: "{0:N2}" },
            //{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "NetFixedAssetsAmount", dataMember: "NetFixedAssetsAmount", format: "{0:N2}" },
            //{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "FACount", dataMember: "FACount" }

        ],
        showCaptionSummary: true
    }];

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

    $scope.getAssetWIPstatusReportExcel = function () {
            var filtered = $("#GridAssetWIPstatus").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                filtered = $scope.AssetWIPstatusList;
        }
        $scope.fileName = 'AssetWIPStatus.xls';
            //filtered = ej.DataManager(filtered).executeLocal(ej.Query().select(["AccountGroupName"]));
            var materialMasterId = getString(filtered, "MaterialMasterId");
            var materialMasterArticleId = getString(filtered, "ArticleId");
            var voucherId = getString(filtered, "VoucherId");
            var grnNo = getString(filtered, "GRNNo");
            var glId = getString(filtered, "GlId");
            var activityId = getString(filtered, "ActivityId");
            try {
               
                $http({
                    method: 'POST',
                    url: 'Accounts/VoucherReport/AssetWIPstatusReportExcel',
                    data: {
                
                        'MaterialMasterId': materialMasterId,
                        'materialMasterArticleId': materialMasterArticleId,
                        'VoucherId': voucherId,
                        'GRNNo': grnNo,
                        'GlId': glId,
                        'ActivityId': activityId
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath
                    }
                });

            } catch (e) {
                // ShowResult(e, 'failure');
                ShowResult(commonMessage.NetworkError, 'failure');
            }
        
    }

    $scope.onGRNNoDownloadExcel = function (data) {
        location.href = "GoodsReceiveNote/GRNReport?grnId=" + data.GRNNo;
        };

    $scope.onVoucherNoDownloadExcel = function (data) {
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.VoucherNo)) return ShowResult('No Id found', 'failure');
        $window.open('Accounts/InventoryPayable/PabyableJournal?' + '&reportFormat=' + reportFormat + '&inventoryReceiveId=' + data.GRNNo + '&employeeId=' + null + '&isReversCharge=' + false + '&isFoc=' + false);
    };


    $scope.issueQtyList = [];
    $scope.onIssueQtyPopUp = function (Data) {
        $scope.SelectedLCRow = Data;

        $http({
            method: 'POST',
            url: 'Accounts/VoucherReport/GetIssueQtyList',
            data: { 'inventoryReceiveDetailId': Data.InventoryReceiveDetailId},
            dataType: 'JSON'

        })
            .then(function successCallback(response) {
                if (response.data.Error == false) {

                    $scope.issueQtyList = response.data.Data;
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }),
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        $rootScope.openPopupAngular('IssueQtyPopup');
    }
        //$scope.summaryAC = [{
        //    title: "Total :", summaryColumns: [
        //        { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "AcceptanceValue", dataMember: "AcceptanceValue", format: "{0:N2}" }
        //        , { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "SetOffValue", dataMember: "SetOffValue", format: "{0:N2}" }],
        //    showCaptionSummary: true

        //}];

    //$scope.PostedAUCList = [];
    $scope.PostedAUCData = function (issueno) {
        $window.open('Products/InventoryIssue/AssetIssueReport?grnId=' + issueno);
    }
    //$scope.PostedAUCData();

    //$scope.PostedcommandPDF = [];
    $scope.PostedcommandPDF = function (voucherNo) {
        var reportFormat = "Pdf";
        $window.open('FixedAssets/FixedAssetRegister/GetIssueFixedAssetCapitalizeJournalReport?reportFormat=' + reportFormat + '&voucherId=' + voucherNo, '_blank');
    }


    $scope.getAssetWIPstatusReportExcel = function () {
        var filtered = $("#GridAssetWIPstatus").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            filtered = $scope.AssetWIPstatusList;
        }
        $scope.fileName = 'AssetWIPStatus.xls';
        //filtered = ej.DataManager(filtered).executeLocal(ej.Query().select(["AccountGroupName"]));
        var materialMasterId = getString(filtered, "MaterialMasterId");
        var materialMasterArticleId = getString(filtered, "ArticleId");
        var voucherId = getString(filtered, "VoucherId");
        var grnNo = getString(filtered, "GRNNo");
        var glId = getString(filtered, "GlId");
        var activityId = getString(filtered, "ActivityId");
        try {

            $http({
                method: 'POST',
                url: 'Accounts/VoucherReport/AssetWIPstatusReportExcel',
                data: {

                    'MaterialMasterId': materialMasterId,
                    'materialMasterArticleId': materialMasterArticleId,
                    'VoucherId': voucherId,
                    'GRNNo': grnNo,
                    'GlId': glId,
                    'ActivityId': activityId
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath
                }
            });

        } catch (e) {
            // ShowResult(e, 'failure');
            ShowResult(commonMessage.NetworkError, 'failure');
        }

    }


}