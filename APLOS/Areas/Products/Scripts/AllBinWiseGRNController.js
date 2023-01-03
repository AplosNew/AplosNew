'use strict';
AllBinWiseGRNController.$inject = ['addressService', '$window', 'factoryService', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function AllBinWiseGRNController(addressService, $window, factoryService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Bin Wise GRN"; //Inventory Receive
    $scope.path = 'Products/GoodsReceiveNote/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    $scope.ModelTemp = {
        FromDate: null,
        ToDate: null,
        
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.MaterialTypeList = [];
    $scope.GetMaterialType = function () {
       
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/GetMaterialType',
            dataType:'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialTypeList = response.data;

            var index = 0;
            $('#ddMaterialTypeId').ejDropDownList(
                {
                    dataSource: $scope.MaterialTypeList,
                    fields: { text: "Text", value: "Value" },
                    selectedIndex: index, showCheckBox: true, multiSelectMode: ej.MultiSelectMode.VisualMode
                    , width: 250
                });
            var ddMaterialTypeList = $("#ddMaterialTypeId").data("ejDropDownList");
            var materialTypeIds = ddMaterialTypeList.getSelectedValue();
        })
    }
    $scope.GetMaterialType();
    $scope.fileName = "All Bin Wise GRN Report.xlsx";
    $scope.XlsDownloadBinWiseGRNReport = function () {

        var ddMaterialTypeList = $("#ddMaterialTypeId").data("ejDropDownList");
        var MaterialTypeId = ddMaterialTypeList.getSelectedValue();
        $http({
            method: 'POST',
            url: 'Products/GoodsReceiveNote/XlsAllBinWiseGRNReport?from=' + $scope.FromDate + '&to=' + $scope.ToDate + '&materialtype=' + MaterialTypeId,
            dataType: 'JSON',
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    //$rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

    };
}