'use strict';
WCWorkStationsControlReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function WCWorkStationsControlReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "WCWorkStationsControlReport";
    $scope.Action = 'Save';
    $scope.path = 'Productions/WCWorkStationsControlReport/';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrlUpd = 'GridReports/ExcelExportUpd';

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    date.setDate(date.getDate());
    var firstDay = new Date(y, m, 1);
   
    $scope.status = {
        Id: null,
        FromDate: null,
        ToDate: $filter('dateFiltering')(date, 'dd-MM-yyyy'),
        FromDateMD: $filter('dateFiltering')(firstDay, 'dd-MM-yyyy'),
        ToDateMD: $filter('dateFiltering')(date, 'dd-MM-yyyy'),
        Status: 'Pending',
        WSMId: null,
    };
    $scope.statusNew = Object.assign({}, $scope.status);

    $scope.WSMUserNameList = [];
    $scope.GetWSMUserNameList = function () {
        $http({
            method: 'GET',
            url: 'Productions/WCWorkStationsControl/GetWSMUserNameList'
        }).then(function successCallback(response) {
            $scope.WSMUserNameList = response.data;
        });
    }
    $scope.GetWSMUserNameList();

    //$scope.GetFromDateList = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'Production/WCWorkStationsControlReport/GetFromDateList'
    //    }).then(function successCallback(response) {
    //        $scope.statusNew.FromDate = response.data[0];
    //        $scope.statusNew.FromDateMD = response.data[0];
    //    });
    //}
    //$scope.GetFromDateList();

    $scope.CD1 = null;
    $scope.CD2 = null;
    $scope.CD3 = null;
    $scope.CD4 = null;
    $scope.rowDataBoundDetails = function rowDataBoundDetails(e) {
        if (!baseService.isUndefinedOrNull(e.data.CD1)) {
            e.model.columns[7].visible = true;
            $scope.CD1 = e.data.CD1;
        }
        else {
            e.model.columns[7].visible = false;
        }
        if (!baseService.isUndefinedOrNull(e.data.CD2)) {
            e.model.columns[8].visible = true;
            $scope.CD2 = e.data.CD2;
        }
        else {
            e.model.columns[8].visible = false;
        }
        if (!baseService.isUndefinedOrNull(e.data.CD3)) {
            e.model.columns[9].visible = true;
            $scope.CD3 = e.data.CD3
        }
        else {
            e.model.columns[9].visible = false;
        }
        if (!baseService.isUndefinedOrNull(e.data.CD4)) {
            e.model.columns[10].visible = true;
            $scope.CD4 = e.data.CD4;
        }
        else {
            e.model.columns[10].visible = false;
        }
    }

    function Validation() {
        try {
            CheckField("To Date", $scope.statusNew.ToDateMD);
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

    $scope.WCWorkStationsControlReportList = [];
    $scope.View = function () {
        try {
            Validation();
            $http({
                method: 'GET',
                url: 'Productions/WCWorkStationsControlReport/LoadWCWorkStationsControlReportList?ToDate=' + $scope.statusNew.ToDateMD + '&FromDate=' + $scope.statusNew.FromDateMD + '&WSMUserName=' + $scope.statusNew.WSMId
            }).then(function successCallback(response) {
                $scope.WCWorkStationsControlReportList = response.data;
                var gridObj = $("#GridWCWorkStationsControlReport").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            }
            )
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.WCWorkStationsControlReport = function () {
        var dataList = [];
        var g = $("#GridWCWorkStationsControlReport").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.WCWorkStationsControlReportList;
        }

        $scope.fileName = "WC/Work Stations Control";

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrlUpd,
            data: { 'reportFileName': $scope.fileName, 'data': dataList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }
}

