'use strict';
DailyAttendanceReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function DailyAttendanceReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $rootScope.title = 'Daily Attendance Report';
    //$scope.index = -1;

    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.path = 'Attendances/DailyAttendanceReport/';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.empGridShow = function (args) {
            ShowResult('Press the Go Button after Selecting Previous Date', 'success');
            $scope.empGrid = false;
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
    $scope.previousDay = null;
    $scope.GetList = [];
    $scope.getData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetData?date=' + $scope.effectiveDate + '&ROId=' + $scope.RoBudgetCode,
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
    $scope.AttendanceDayStatusList = [
        {
            Value: 'Present',
            Text: 'On Time Present'
        },
        {
            Value: 'Late',
            Text: 'Late Present'
        },
        {
            Value: 'Absent',
            Text: 'Absent'
        },
        {
            Value: 'Leave',
            Text: 'Leave'
        },
        {
            Value: 'Weekend',
            Text: 'Week Off'
        },
        {
            Value: 'Half Day',
            Text: 'Half Day'
        },
        {
            Value: 'Holiday',
            Text: 'Holiday'
        }
    ];


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

    var aa = function (data, column) {
        var kk = "";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) === false) {
                if (kk === "") {
                    kk += "" + data[i][column] + "";
                }
                else {
                    kk += ", " + data[i][column] + "";
                }

                collection.push(data[i][column]);
            }
        }
        return kk;
    };

    $scope.RoBudgetCodeList = [];
    $scope.getRoBudgetCode = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetRoBudgetCodeData',
        }).then(function successCallback(response) {
            $scope.RoBudgetCodeList = response.data;
            $scope.getEmpDataa();
        });
    }
    $scope.getRoBudgetCode();

    $scope.EmpName = null;
    $scope.EmpCode = null;
    $scope.BudgetCode = null;
    $scope.RoBudgetCode = null;

    $scope.getEmpData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetEmpData',
        }).then(function successCallback(response) {
            $scope.EmpName = response.data[0].EmployeeName;
            $scope.EmpCode = response.data[0].EmployeeCode;
            $scope.BudgetCode = response.data[0].Id;
        });
    }
    $scope.getEmpData();
    $scope.cc = [];
    $scope.getEmpDataa = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetEmpDataa',
        }).then(function successCallback(response) {
            $scope.cc = response.data[0].BudgetCode;
            for (var i = 0; i < $scope.RoBudgetCodeList.length; i++) {
                if ($scope.cc == $scope.RoBudgetCodeList[i].Code) {
                    $scope.RoBudgetCode = $scope.RoBudgetCodeList[i].Code;
                    break;
                }
            }
        });
    }

    $scope.BudgetIdChange = function () {
        for (var i = 0; i < $scope.RoBudgetCodeList.length; i++) {
            if ($scope.RoBudgetCode == $scope.RoBudgetCodeList[i].Code) {
                $scope.BudgetCode = $scope.RoBudgetCodeList[i].Id;
                break;
            }
            else {
                $scope.BudgetCode = null;
            }
        }
    }

    //#region Attendance Daily Status Report

    $scope.AttendanceDailyStatusReport = function () {
        try {
            $scope.fileName = "DailyAttendanceStatus.xls";
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

            var DropDownListObj = $("#AttendanceDayStatusList").data("ejDropDownList");
            var dayStatus = DropDownListObj.getSelectedValue();

            var DropDownListObj = $("#ShiftList").data("ejDropDownList");
            var shiftList = DropDownListObj.getSelectedValue();
            if (dayStatus == "" || dayStatus == '') {
                for (var i = 0; i < $scope.AttendanceDayStatusList.length; i++) {
                    if (i < $scope.AttendanceDayStatusList.length - 1) {
                        dayStatus += $scope.AttendanceDayStatusList[i].Value + ",";
                    }
                    else {
                        dayStatus += $scope.AttendanceDayStatusList[i].Value;
                    }
                }

            }
            $http({
                method: 'POST',
                url: 'Attendances/DailyAttendanceReport/DailyAttendanceStatusReport',
                data: {
                    'workDate': $scope.effectiveDate, 'Entity': enttyList
                    , 'Dept': departmentList, 'designationList': designationList
                    , 'empCategoryList': empCategoryList, 'Sec': sectionList
                    , 'SSec': subSectionList, 'lineList': lineList
                    , 'dayStatus': dayStatus, 'shift': shiftList
                    , 'Ydate': $scope.previousDay, 'WithFatherName': $scope.WithFatherName
                    , 'JobLocation': JobLocation
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

    //#region Daily Day Status Report

    $scope.DailyDayStatusReport = function () {
        try {
            $scope.fileName = "DailyDayStatus.xls";
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

            parameters.push({ "Key": "Department", "Value": aa(filteredRecords, "Department") });

            parameters.push({ "Key": "Section", "Value": aa(filteredRecords, "Section") });
            parameters.push({ "Key": "JobLocation", "Value": getString(filteredRecords, "JobLocationId") });

            var enttyList = parameters[0].Value;
            var departmentList = parameters[1].Value;
            var designationList = parameters[2].Value;
            var empCategoryList = parameters[3].Value;
            var sectionList = parameters[4].Value;
            var subSectionList = parameters[5].Value;
            var lineList = parameters[6].Value;
            var DeptName = parameters[7].Value;
            var Sec = parameters[8].Value;
            var JobLocation = parameters[9].Value;

            var DropDownListObj = $("#AttendanceDayStatusList").data("ejDropDownList");
            var dayStatus = DropDownListObj.getSelectedValue();



            var DropDownListObj = $("#ShiftList").data("ejDropDownList");
            var sList = DropDownListObj.getSelectedValue();
            if (sList == null || sList == '') {
                sList = "ALL";
            }

            $http({
                method: 'POST',
                url: 'Attendances/DailyAttendanceReport/DailyDayStatusReport',
                data: {
                    'workDate': $scope.effectiveDate, 'entity': enttyList
                    , 'sDepID': departmentList, 'designationList': designationList
                    , 'employeeCategory': empCategoryList, 'sSubSecID': subSectionList
                    , 'sSecID': sectionList, 'sLineID': lineList
                    , 'dayStatus': dayStatus, 'shift': sList
                    , 'PrevWorkDate': $scope.previousDay, 'Dep': DeptName
                    , 'Sec': Sec, 'WithFatherName': $scope.WithFatherName
                    , 'JobLocation': JobLocation
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

    //#region  Daily Status Count Report

    $scope.GetDailyDayStatusCount = function () {
        try {
            $scope.fileName = "DailyStatusCount.xls";
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

            var DropDownListObj = $("#AttendanceDayStatusList").data("ejDropDownList");
            var dayStatus = DropDownListObj.getSelectedValue();

            var DropDownListObj = $("#ShiftList").data("ejDropDownList");
            var shiftList = DropDownListObj.getSelectedValue();

            $http({
                method: 'POST',
                url: 'Attendances/DailyAttendanceReport/DailyStatusCount',
                data: {
                    'workDate': $scope.effectiveDate, 'Entity': enttyList
                    , 'Dept': departmentList, 'designationList': designationList
                    , 'empCategoryList': empCategoryList, 'Sec': sectionList
                    , 'SSec': subSectionList, 'lineList': lineList
                    , 'dayStatus': dayStatus, 'shift': shiftList
                    , 'Ydate': $scope.previousDay, 'WithFatherName': $scope.WithFatherName
                    , 'JobLocation': JobLocation
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