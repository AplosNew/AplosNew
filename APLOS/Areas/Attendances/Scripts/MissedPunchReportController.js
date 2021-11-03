'use strict';
MissedPunchReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function MissedPunchReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'Missed Punch Report';
    $scope.path = 'Attendances/MissedPunchReport/';

    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.empGridShow = function (args) {
        var Today =  $filter('dateFiltering')(Date.now());
        if ( $scope.effectiveDate == Today) {
            $scope.getData();
            $scope.empGrid = true;
        }
        else {
            ShowResult('Press the Go Button after selecting Previous Date', 'success');
            $scope.empGrid = false;
        }        
    };
    $scope.PreviousDatee = function () {
        var attdnDate = new Date($scope.effectiveDate);

        $scope.previousDay = $filter('date')(new Date(attdnDate.setDate(attdnDate.getDate() - 1)), 'dd-MMM-yyyy');
    };
    $scope.effectiveDate = $filter('dateFiltering')(Date.now());
    var attdnDate = new Date($scope.effectiveDate);
    $scope.PD = function () {
        $scope.previousDay = $filter('date')(new Date(attdnDate.setDate(attdnDate.getDate() - 1)), 'dd-MMM-yyyy');
    }

    $scope.WithFatherName = false;
    $scope.chkIntime = false;
    $scope.chkoutTime = false;
    $scope.previousDay = null;
    $scope.GetList = [];
    $scope.getData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetData?date=' + $scope.effectiveDate,
        }).then(function successCallback(response) {
            $scope.empGrid = true;
            $scope.GetList = response.data;
        });
    }
    $scope.ShiftList = [];
    $scope.getShift = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetShift",
        }).then(function successCallback(response) {
            $scope.ShiftList = response.data;
        });
    }
    $scope.getShift();
    var getString = function (data, column) {
        var kk = "";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) === false) {
                if (kk === "") {
                    kk += "'" + data[i][column] + "'";
                }
                else {
                    kk += ",'" + data[i][column] + "'";
                }

                collection.push(data[i][column]);
            }
        }
        return kk;
    };


    //#region Missed Punch Report

    $scope.GetMissedPunchReport = function () {
        try {
            $scope.fileName = "MissedPunchReport " + $scope.effectiveDate + ".xls";
            var parameters = [];
            var gridObj = $("#empInfoGrid").data("ejGrid");
            var filteredRecords = gridObj.getFilteredRecords();
            if (filteredRecords.length == 0) {
                filteredRecords = $scope.GetList;
            }

            parameters.push({ "Key": "EntityId", "Value": getString(filteredRecords, "EntityId") });
            parameters.push({ "Key": "DepartmentId", "Value": getString(filteredRecords, "DepId") });
            parameters.push({ "Key": "DesignationId", "Value": getString(filteredRecords, "DesignationId") });
            parameters.push({ "Key": "EmpCategoryId", "Value": getString(filteredRecords, "EmpCategoryId") });
            parameters.push({ "Key": "SectionId", "Value": getString(filteredRecords, "SecId") });
            parameters.push({ "Key": "SubSectionId", "Value": getString(filteredRecords, "SubSecId") });
            parameters.push({ "Key": "LineId", "Value": getString(filteredRecords, "LineId") });
            parameters.push({ "Key": "JobLocation", "Value": getString(filteredRecords, "JobLocationId") });

            var enttyList = parameters[0].Value;
            var departmentList = parameters[1].Value;
            var designationList = parameters[2].Value;
            var empCategoryList = parameters[3].Value;
            var sectionList = parameters[4].Value;
            var subSectionList = parameters[5].Value;
            var lineList = parameters[6].Value;
            var JobLocation = parameters[7].Value;

            var DropDownListObj = $("#ShiftList").data("ejDropDownList");
            var shiftList = DropDownListObj.getSelectedValue();
            $http({
                method: 'POST',
                url: 'Attendances/MissedPunchReport/GetMissedPunchReport',
                data: {
                    'workDate': $scope.effectiveDate, 'sDepID': departmentList
                    , 'sSecID': sectionList, 'sSubSecID': subSectionList
                    , 'sLineID': lineList, 'chkIntime': $scope.chkIntime
                    , 'chkoutTime': $scope.chkoutTime, 'shiftList': shiftList
                    , 'JobLocation': JobLocation, 'designationList': designationList
                    , 'enttyList': enttyList, 'empCategoryList': empCategoryList
                    , 'WithFatherName': $scope.WithFatherName
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    //#endregion

}
