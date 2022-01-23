'use strict';
ServicePORegisterController.$inject = ['fileReader', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService', '$window', '$controller'];
function ServicePORegisterController(fileReader, commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService, $window, $controller) {
    $rootScope.title = 'Service PO Register Report';
    $scope.products = [];
    $scope.path = 'Materials/MaterialLedger/';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportJson';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.Type = 'Posted';
    $scope.PurchaseRegisterLst = [];
    $scope.pivotTableFieldListID = [];

    $scope.productNew = {
        Type: null,
        WithStock: true,
        WithoutStock: false
    };

    $scope.getPurchaseRegisterReport = function () {
        if ($scope.report.FromDate === null || $scope.report.FromDate === "") {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        else if ($scope.report.ToDate === null || $scope.report.ToDate === "") {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        $http({
            method: 'POST',
            url: 'Materials/MaterialLedger/GetPurchaseOrderRegister',
            data: {
                fromDate: $scope.report.FromDate,
                toDate: $scope.report.ToDate,
                Type: $scope.Type
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PurchaseRegisterLst = response.data;
        });
    }

    $scope.PurchaseOrderReportPdf = function (reportFormat) {

        if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        var reportFormat = "Pdf";
        //if (baseService.isUndefinedOrNull(id)) return ShowResult('No Id found', 'failure');
        $window.open('Materials/MaterialLedger/ServicePORegisterReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Type=' + $scope.productNew.Type, '_blank');
    };

    $scope.PurchaseOrderReportExcel = function (reportFormat) {
        if ($scope.report.FromDate === "" || $scope.report.FromDate === null || $scope.report.FromDate === undefined) {
            ShowResult('Select From Date', 'failure');
            return false;
        }
        if ($scope.report.ToDate === "" || $scope.report.ToDate === null || $scope.report.ToDate === undefined) {
            ShowResult('Select To Date', 'failure');
            return false;
        }
        try {
            var Excel;
            var file_src = 'Materials/MaterialLedger/ServicePORegisterReport?reportFormat=' + reportFormat + '&fromDate=' + $scope.report.FromDate + '&toDate=' + $scope.report.ToDate + '&Qty=' + $scope.choice1 + '&Amount=' + $scope.choice2 + '&RcptIssue=' + $scope.productNew.RcptIssue + '&Asset=' + $scope.productNew.WithStock + '&Inventory=' + $scope.productNew.WithoutStock;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    $scope.report.FromDate = $filter("dateFiltering")(Date.now());
    $scope.report.ToDate = $filter("dateFiltering")(Date.now());
    
}



