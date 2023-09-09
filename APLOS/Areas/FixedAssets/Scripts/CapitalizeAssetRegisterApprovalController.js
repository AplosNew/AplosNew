"use strict";
CapitalizeAssetRegisterApprovalController.$inject = ["commonMessage", "$scope", "$rootScope", "$filter", "$http", "$controller", "$window", "baseService"];
function CapitalizeAssetRegisterApprovalController(commonMessage, $scope, $rootScope, $filter, $http, $controller, $window, baseService) {
    $rootScope.title = "Capitalize Asset Register Approval";

    $scope.idList = [];
    $scope.UnApprovedDataList = [];
    $scope.sqlInStatement = null;
    $scope.GetUnapprovedData = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'FixedAssets/FixedAssetRegister/GetUnApprovedData',
        }).then(function successCallback(response) {
            $scope.UnApprovedDataList = response.data;
            for (var i = 0; i < $scope.UnApprovedDataList.length; i++) {
                $scope.idList.push($scope.UnApprovedDataList[i].Id);
            }
           
            $scope.GetapprovedData();
           
        });
    };
    $scope.GetUnapprovedData();


    $scope.ApprovedDataList = [];
    $scope.GetapprovedData = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'FixedAssets/FixedAssetRegister/GetApprovedData',
        }).then(function successCallback(response) {
            $scope.ApprovedDataList = response.data;
            for (var i = 0; i < $scope.ApprovedDataList.length; i++) {
                $scope.idList.push($scope.ApprovedDataList[i].Id);
            }
            if ($scope.idList.length > 0) {
                //var uniqueId = removeDuplicates($scope.idList, 'Id');
                var wcId = "";
                if ($scope.idList.length > 0) {
                    wcId = "IN(";
                    wcId += Array.prototype.map.call($scope.idList, function (item) { return "'" + item + "'"; }).join(",") + ")";
                }
                $scope.sqlInStatement = wcId;
                $scope.GetCapitalizationMasterDetail();
            }
        });
    };
    


    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.selectedmaterialMasterList = [];
    $scope.GetCapitalizationMasterDetail = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'FixedAssets/FixedAssetRegister/GetCapitalizationDetailByMaster?masterId=' + $scope.sqlInStatement,
        }).then(function successCallback(response) {
            $scope.selectedmaterialMasterList = response.data;
        });
    };


    $scope.detailTemp = "#tabGridContents";
    $scope.detailgrid = function detailGridData(e) {

        var filteredData = e.data["Id"];

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
                    "data": $scope.register
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