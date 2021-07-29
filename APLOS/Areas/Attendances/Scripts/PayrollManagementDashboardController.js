'use strict';
PayrollManagementDashboardController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function PayrollManagementDashboardController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $rootScope.title = 'Audit Report Summary';
    //$scope.index = -1;

    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.path = 'Attendances/PayrollManagementDashboard/';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);
    $scope.effectiveDate = $filter('dateFiltering')(firstDay);


    var yesterday = new Date();

    var datee = yesterday.setDate(yesterday.getDate() - 1);

    $scope.ToDate = $filter('dateFiltering')(new Date(datee), 'dd-MM-yyyy');

    $scope.SummaryData = [];
    $scope.Report = function () {
        try {

            $http({
                method: 'POST',
                url: 'Attendances/PayrollManagementDashboard/AuditReportSummary',
                data: {
                    'workDate': $scope.effectiveDate, 'ToDate': $scope.ToDate
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.SummaryData = response.data.DATA;
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.Report();
    $scope.closePopup = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");
        try {
            $("#" + popupName).data("ejDialog").close();
        } catch (e) {

        }
    }
    $scope.openPopup = function (popupName) {

        try {
            $("#" + popupName).data("ejDialog").open();
        } catch (e) {

        }
    }
    $scope.openPopupAngular = function (popupName) {
        try {
            angular.element(document.querySelector("#" + popupName + "")).modal("show");
        } catch (e) {

        }

    }
    $scope.DetailData = [];
    $scope.DataType = 'Attendance';
    $scope.ModalTitle = '';
    $scope.onSummaryItemSelection = function (data, isUpToDate) {
        try {
            $scope.ModalTitle = data.Particulars + ' information for plant ' + data.PlantName;
            $http({
                method: 'POST',
                url: 'Attendances/PayrollManagementDashboard/GetAttendanceDetail',
                data: {
                    'workDate': $scope.effectiveDate, 'ToDate': $scope.ToDate
                    , 'plantId': data.PlantId, 'ParticularsKey': data.ParticularsKey, 'UpToDate': isUpToDate
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.DetailData = response.data.DATA;

                    if (response.data.DataType == 'Attendance') {

                        $scope.openPopupAngular('modalAttendance');
                    }
                    else {
                        $scope.openPopupAngular('modalProfile');
                    }
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
}