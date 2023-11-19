'use strict';
SalesChanlanDispatchConfirmationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', '$controller', '$route'];
function SalesChanlanDispatchConfirmationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, $controller, $route) {
    $rootScope.title = 'Sales Chalan Dispatch Confirmation';
    $scope.ModelList = [];

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.GetApproveByDataForDispatchConfirmation = function () {
        $http({
            method: 'GET',
            url: 'SalesManagements/SalesChalan/GetApproveByDataForDispatchConfirmation'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.GetApproveByDataForDispatchConfirmation();

    $scope.ConfirmedModelList = [];
    $scope.GetApproveByDataForDispatchConfirmed = function () {
        $http({
            method: 'GET',
            url: 'SalesManagements/SalesChalan/GetApproveByDataForDispatchConfirmed'
        }).then(function successCallback(response) {
            $scope.ConfirmedModelList = response.data;
        });
    }
    $scope.GetApproveByDataForDispatchConfirmed();

    $scope.detailTemp = "#tabGridContents";

    $scope.InvoiceNoList = [];

    $scope.detailgrid = function detailGridData(e) {

        var filteredData = e.data["Id"];

        $http({
            method: 'GET',
            url: 'SalesManagements/SalesChalan/GetInvoiceDataByChalan?masterId=' + filteredData
        }).then(function successCallback(response) {
            $scope.InvoiceNoList = response.data;

            var data = ej.DataManager($scope.InvoiceNoList).executeLocal(ej.Query().where("SalesChalanId", "equal", parseInt(filteredData), true).take(100));

            e.detailsElement.find("#detailGrid").ejGrid({

                dataSource: data,
                columns: [
                    { field: "InvoiceId", headerText: "Invoice No", width: 50 },
                    { field: "InvoiceDate", headerText: "Invoice Date", width: 100 },
                    { field: "Customer", headerText: "Customer", width: 100 },
                    { field: "NoOfPackage", headerText: "NoOfPackage", width: 100 },
                    { field: "NetWeight", headerText: "Net Weight", width: 100 },
                    { field: "GrossWeight", headerText: "Gross Weight", width: 100 },
                    { field: "Destination", headerText: "Destination", width: 100 },

                ]
            });
            e.detailsElement.find(".tabcontrol").ejTab();
        });


    }

    $scope.PrintData = function (data) {
        try {
            $scope.fileName = "SalesChalanReport.xlsx";

            $scope.ReportFormat = 'Pdf';
            var url = 'SalesManagements/SalesChalan/GetSalesChalanReportPdf?reportFormat=' + $scope.ReportFormat + '&masterId=' + data.data.Id;
            $rootScope.report(url);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SaveDispatchConfirmData = function (args) {
        try {
            $scope.btndisable = true;
            $http({
                method: 'POST',
                url: 'SalesManagements/SalesChalan/CreateDispatchConfirmData',
                data: { 'data': args.data },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.btndisable = false;
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.btndisable = false;
                    $scope.GetApproveByDataForDispatchConfirmation();
                    $scope.GetApproveByDataForDispatchConfirmed();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



}