'use strict';
SpecialDutyReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', "$controller"];
function SpecialDutyReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'Special Duty Report';
   
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $scope.FromDate = null;
    $scope.ToDate = null;

    $scope.SDlist = [];
    $scope.GetSDData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.FromDate)) {
                throw "Select From Date";
            }
            if (baseService.isUndefinedOrNull($scope.ToDate)) {
                throw "Select To Date";
            }
            $http({
                method: 'Get',
                url: "Attendances/SpecialDuty/GetSDDataInDateRange?fromDate=" + $scope.FromDate + '&toDate=' + $scope.ToDate,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.SDlist = response.data;
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.SpecialDutyReport = function () {
        try {
            var dataList = [];
            var g = $("#GridEmp").data("ejGrid");
            dataList = g.getFilteredRecords();

            if (dataList.length == 0) {
                dataList = $scope.SDlist;
            }

            if (dataList.length == 0) {
                throw "First click on View button.";
            }

            $scope.fileName = "SpecialDutyReport.xlsx";

            $http({
                method: 'POST',
                url: "Attendances/SpecialDuty/GetSpecialDutyReportInDateRange",
                data: { 'data': dataList, 'reportFileName': $scope.fileName },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

}