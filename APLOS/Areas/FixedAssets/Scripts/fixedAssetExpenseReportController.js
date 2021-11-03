'use strict';
fixedAssetExpenseReportController.$inject = ['$scope', '$rootScope', '$http', '$filter', '$controller', 'baseService', "$window"];
function fixedAssetExpenseReportController($scope, $rootScope, $http, $filter, $controller, baseService, $window) {
    $rootScope.title = 'Fixed Asset Register Expenses';
    $scope.ledgerReport = {
        FromDate: $filter('dateFiltering')(Date.now()),
        ToDate: $filter('dateFiltering')(Date.now()),
        FixedAssetRegisterId: null,
        FixedAssetRegisterName: null,
        ReportFormat: 'Pdf'
    };

    $scope.searchbyRegisterlist = [
        {
            'name': 'Serial No',
            'value': 'SerialNo'
        },
        {
            'name': 'Asset No',
            'value': 'AssetNo'
        },
        {
            'name': 'Asset Type',
            'value': 'AssetType'
        },
        {
            'name': 'Article',
            'value': 'Article'
        }
    ];
    $scope.registerListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'FixedAssetMasterName',
        searchBy: 'FixedAssetMasterName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.registerList = [];
    $scope.getData = function () {
        $scope.loadRegisterData = function (pageno) {
            $scope.registerListParameters.ids = JSON.stringify([]);
            baseService.paginationBase('fixedassets/fixedassetregister/getlist', pageno, $scope.registerListParameters)
                .then(function (result) {
                    $scope.registerList = result.Rows;
                    $scope.registerListParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.searchbyRegisterlist) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyRegisterlist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });

        };
        angular.element(document.querySelector('#registersearchpopup')).modal('show');
        $scope.loadRegisterData();
    };

    $scope.GetRegisterIndex = function (data) {
        $scope.ledgerReport.FixedAssetRegisterId = data.Id;
        $scope.ledgerReport.FixedAssetRegisterName = data.SerialNo;
        angular.element(document.querySelector('#registersearchpopup')).modal('hide');
    };

    $scope.report = function () {
        if (baseService.isUndefinedOrNull($scope.ledgerReport.FixedAssetRegisterId)) {
            manualValidation('div_FAR', true, "Fixed Asset Register is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.ledgerReport.FromDate)) {
            manualValidation('div_FromDate', true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.ledgerReport.ToDate)) {
            manualValidation('div_ToDate', true, "To Date is required.");
        }
        else if (new Date($scope.ledgerReport.FromDate) > new Date($scope.ledgerReport.ToDate)) {
            manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.ledgerReport.ToDate) < new Date($scope.ledgerReport.FromDate)) {
            manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
        }
        else {
            var url = 'Employees/EmployeeReport/GetAssetRegisterExpenseBookingReport?reportFormat=' + $scope.ledgerReport.ReportFormat + '&fromDate=' + $scope.ledgerReport.FromDate + '&toDate=' + $scope.ledgerReport.ToDate + '&fixedAssetRegisterId=' + $scope.ledgerReport.FixedAssetRegisterId;
            $window.open(url, '_blank');
        }
    };
}