'use strict';
maintenancePlanningReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function maintenancePlanningReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "MaintenancePlanningReport";
    $scope.Action = 'Save';
    $scope.path = 'Machines/MaintenancePlanningReport/';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    date.setDate(date.getDate() + 7);
    /*var firstDay = new Date(y, m, 1);*/
   
    $scope.status = {
        Id: null,
        FromDate: null,
        ToDate: $filter('dateFiltering')(date, 'dd-MM-yyyy'),
        FromDateMD: null,
        ToDateMD: $filter('dateFiltering')(date, 'dd-MM-yyyy')
    };
    $scope.statusNew = Object.assign({}, $scope.status);

    $scope.GetFromDateList = function () {
        $http({
            method: 'GET',
            url: 'Machines/MaintenancePlanningReport/GetFromDateList'
        }).then(function successCallback(response) {
            $scope.statusNew.FromDate = response.data[0];
            $scope.statusNew.FromDateMD = response.data[0];
        });
    }
    $scope.GetFromDateList();

    function Validation() {
        try {
            CheckField("To Date", $scope.statusNew.ToDate);
        } catch (ex) {
            throw ex;
        }
    }

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] is required.";
            }

        } catch (ex) {
            throw ex;
        }
    }

    $scope.MaintenancePlanningReportList = [];
    $scope.View = function () {
        try {
            Validation();
            $http({

                method: 'Get',
                url: 'Machines/MaintenancePlanningReport/LoadMaintenancePlanningReportList?ToDate=' + $scope.statusNew.ToDateMD + '&FromDate=' + $scope.statusNew.FromDateMD
            }).then(function successCallback(response) {
                $scope.MaintenancePlanningReportList = response.data;
                var gridObj = $("#GridMaintenancePlanningReport").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            }
            )
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.MaintenancePlanningReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'XlsMaintenancePlanningReport?todate=' + $scope.statusNew.ToDateMD + '&fromDate=' + $scope.statusNew.FromDateMD,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {

                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };
}

