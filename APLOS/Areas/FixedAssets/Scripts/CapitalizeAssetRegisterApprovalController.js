"use strict";
CapitalizeAssetRegisterApprovalController.$inject = ["commonMessage", "$scope", "$rootScope", "$filter", "$http", "$controller", "$window", "baseService"];
function CapitalizeAssetRegisterApprovalController(commonMessage, $scope, $rootScope, $filter, $http, $controller, $window, baseService) {
    $rootScope.title = "Capitalize Asset Register Approval";
   
    $scope.UnApprovedDataList = [];
    $scope.GetUnapprovedData = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'FixedAssets/FixedAssetRegister/GetUnApprovedData',
        }).then(function successCallback(response) {
            $scope.UnApprovedDataList = response.data;
        });
    };
    $scope.GetUnapprovedData();

    $scope.selectedmaterialMasterList = [];
    $scope.GetCapitalizationMasterDetail = function (filteredData) {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'FixedAssets/FixedAssetRegister/GetCapitalizationMasterDetail?masterId=' + filteredData,
        }).then(function successCallback(response) {
            $scope.selectedmaterialMasterList = response.data;
        });
    };


    $scope.detailTemp = "#tabGridContents";
    $scope.detailgrid = function detailGridData(e) {

        var filteredData = e.data["Id"];
        $scope.GetCapitalizationMasterDetail(filteredData)

        var data = ej.DataManager($scope.selectedmaterialMasterList).executeLocal(ej.Query().where("CapitalizationMasterId", "equal", parseInt(filteredData), true).take(100));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
            columns: [
                { field: "VoucherNo", headerText: "VoucherNo", width: 50 },
                { field: "MaterialMasterName", headerText: "MaterialMasterName", width: 150 },
                { field: "ArticleStandardName", headerText: "ArticleStandardName", width: 100 },
                { field: "GRNNo", headerText: "GRNNo", width: 50 },
                { field: "Qty", headerText: "Qty", width: 50 },
                { field: "Amount", headerText: "Amount", width: 50 },
                { field: "Source", headerText: "Source", width: 50 },
            ]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }


    $scope.ApprovedDataList = [];
    $scope.GetapprovedData = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'FixedAssets/FixedAssetRegister/GetApprovedData',
        }).then(function successCallback(response) {
            $scope.ApprovedDataList = response.data;
        });
    };
    $scope.GetapprovedData();



    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.onClickCARA = function (obj) {
        $scope.register = obj.data;
        $scope.message = 'Are you sure want to approve?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');

    };

    $scope.ApproveRegister = function () {
        try {
            $scope.register.IsApproved = true;
            $scope.saveBtnDisable = true;
            $http({
                method: "POST",
                url: "FixedAssets/FixedAssetRegister/ApproveCapitalize",
                dataType: "JSON",
                data: {
                    "data": $scope.register, "items": null
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                    $scope.saveBtnDisable = false;
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.GetUnapprovedData();
                    $scope.GetapprovedData();
                    $scope.saveBtnDisable = false;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
                $scope.saveBtnDisable = false;
            });
            return true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

}