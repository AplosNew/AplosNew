'use strict';
GoodWorkCheckedController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', '$controller', '$route'];
function GoodWorkCheckedController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, $controller, $route) {
    $rootScope.title = 'Good Work Check & Approve';
    $scope.ModelList = [];
    $scope.path = 'Attendances/GoodWork/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';


    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion TAB CHANGE

    $scope.GriddataGWCUnCheckedList = [];
    $scope.GetUncheckedData = function () {
        $http.get('Attendances/GoodWork/GetUncheckedData')
            .then(function successCallback(response) {
                $scope.GriddataGWCUnCheckedList = response.data;
            })
    }
    $scope.GetUncheckedData();

    $scope.GriddataGWCCheckedList = [];
    $scope.GetcheckedData = function () {
        $http.get('Attendances/GoodWork/GetcheckedData')
            .then(function successCallback(response) {
                $scope.GriddataGWCCheckedList = response.data;
            })
    }
    $scope.GetcheckedData();

    // Employee Who Responsible For Approving
    $scope.ApproveByList = [];
    $scope.GetGoodWorkApproveByCboList = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetGoodWorkApproveByCboList'
        }).then(function successCallback(response) {
            $scope.ApproveByList = response.data;
        });
    }
    $scope.GetGoodWorkApproveByCboList();

    $scope.POApprovalList = [
        {
            'Text': 'Checked',
            'Value': 'Checked'
        }
    ];

    $scope.btndisable = false;
    $scope.SaveCheckData = function (args) {
        try {
            if (baseService.isUndefinedOrNull(args.data.ApproveBy)) {
                throw "Select Approve By Person.";
            }

            $scope.btndisable = true;
            $http({
                method: 'POST',
                url: 'Attendances/GoodWork/CreateCheckBy',
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
                    $scope.GetcheckedData();
                    $scope.GetUncheckedData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

 
    $scope.SaveApproveData = function (args) {
        try {
            if (baseService.isUndefinedOrNull(args.data.ApprovedStatus)) {
                throw "Select Approved Status.";
            }
            $scope.btndisable = true;
            $http({
                method: 'POST',
                url: 'Attendances/GoodWork/CreateApproveBy',
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
                    $scope.GetcheckedData();
                    $scope.GetApproveBycheckedData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GriddataCheckedList = [];
    $scope.GetcheckedDataList = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetcheckedDataList'
        }).then(function successCallback(response) {
            $scope.GriddataCheckedList = response.data;
        });
    }
    $scope.GetcheckedDataList();

    $scope.GriddataApproveList = [];
    $scope.GetApproveBycheckedData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetApproveBycheckedData'
        }).then(function successCallback(response) {
            $scope.GriddataApproveList = response.data;
        });
    }
    $scope.GetApproveBycheckedData();

    $scope.detailTemp = "#tabGridContents";

    $scope.InvoiceNoList = [];
   
    $scope.detailgrid = function detailGridData(e) {
        
        var filteredData = e.data["Id"];

        $http({
            method: 'GET',
            url: 'Attendances/GoodWork/GetInvoiceDataByChalan?masterId=' + filteredData
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
            //$scope.ReportFormat = 'Excel';
            $scope.ReportFormat = 'Pdf';
            var url = 'Attendances/GoodWork/GetSalesChalanReportPdf?reportFormat=' + $scope.ReportFormat + '&masterId=' + data.data.Id;
            $rootScope.report(url);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

}