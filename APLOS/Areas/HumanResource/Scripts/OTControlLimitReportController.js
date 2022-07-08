'use strict';
OTControlLimitReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller','$window'];
function OTControlLimitReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $scope.model = {
        Id: null, EffectiveDate: null, ByWhom: null, ApproveBy: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
    }
    $scope.modelNew = Object.assign({}, $scope.model);
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath

    $scope.GetOTControlLimitReport = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.FromDate)) {
                throw "From Date is required.";
            }
            else if (baseService.isUndefinedOrNull($scope.ToDate)) {
                throw "To Date is required.";
            }
            else if (new Date($scope.FromDate) > new Date($scope.ToDate)) {
                throw "From date must be below or equal to To Date";
            }
            else if (new Date($scope.ToDate) < new Date($scope.FromDate)) {
                throw "To date must be above or equal to From Date.";
            }

           
            $scope.fileName = "OTControlLimit.xlsx";
            
            $http({
                method: 'POST',
                url: "HumanResource/OTControlLimit/GetOTControlLimitReport",
                data: { 'fromDate': $scope.FromDate, 'todate': $scope.ToDate},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

}