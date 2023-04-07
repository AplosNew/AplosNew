'use strict';
EmployeeAttendanceReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function EmployeeAttendanceReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Employee Attendacne Report';
    $scope.path = "HumanResource/AttendanceReport/";


    $scope.exportgriddataUrl = 'GridReports/ExcelExportJson';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    //Employee Attandence Data start
    $scope.EmpSingModelNew = {
        EmpSystemID: null,
        EmployeeCode: null,
        EmployeeName: null,
        ToDate: null,
        FromDate: null
    };

    $scope.SearchEmployee = function () {
        $scope.EmpSingModelNew.EmployeeName = null;
        $scope.EmpSingModelNew.EmpSystemID = null;

        for (var i = 0; i < $scope.employee.length; i++) {
            if ($scope.EmpSingModelNew.EmployeeCode == $scope.employee[i].EmployeeCode) {
                $scope.EmpSingModelNew.EmployeeName = $scope.employee[i].EmployeeName;
                $scope.EmpSingModelNew.EmpSystemID = $scope.employee[i].SystemID;

                break;
            }
        }
    }
    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    $scope.employee = [];
    $scope.getPopUpDataOnly = function () {
        $scope.employee = [];
        $http({
            method: 'GET',
            url: 'employees/leaveApplication/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
    }
    $scope.getPopUpDataOnly();

    $scope.getPopUpData = function () {

        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }

    
    $scope.setEmpData = function (obj) {
        // $scope.Clear();
        var data = obj.data;

        $scope.EmpSingModelNew.EmpSystemID = data.SystemID;
        $scope.EmpSingModelNew.EmployeeCode = data.EmployeeCode;
        $scope.EmpSingModelNew.EmployeeName = data.EmployeeName;
        $scope.closeEmployeePopUp();
    };

    $scope.EmployeeDataList = [];
    $scope.GetEmpSingleData = function () {
        $http.get('HumanResource/AttendanceReport/GetEmployeeSingleData?fromdate=' + $scope.EmpSingModelNew.FromDate + '&todate=' + $scope.EmpSingModelNew.ToDate + '&empId=' + $scope.EmpSingModelNew.EmpSystemID)
            .then(function (response) {
                $scope.EmployeeDataList = [];
                $scope.EmployeeDataList = response.data;
            });
    };

    $scope.EmployeeAttendanceSingleSummaryData = function () {
        try {
            var dataList = [];
            var g = $("#EmpGrid").data("ejGrid");
            dataList = g.getFilteredRecords();

            if (dataList.length == 0) {
                dataList = $scope.EmployeeDataList;
            }
            if (dataList.length == 0) {
                throw "First click on View button.";
            }

            $scope.fileName = "Employee Attendance Data.xlsx";

            $http({
                method: 'POST',
                url: $scope.path + "EmployeeAttendanceDataXls",
                data: { 'reportFileName': $scope.fileName, 'data': dataList },
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

    //Employee Attandence Data end

    //Employee Attandence Summary start
    $scope.EmpSummaryModelNew = {
        EmpSystemID: null,
        EmployeeCode: null,
        EmployeeName: null,
        ToDate: null,
        FromDate: null
    };

    $scope.SearchEmployees = function () {
        $scope.EmpSummaryModelNew.EmployeeName = null;
        $scope.EmpSummaryModelNew.EmpSystemID = null;

        for (var i = 0; i < $scope.employee.length; i++) {
            if ($scope.EmpSummaryModelNew.EmployeeCode == $scope.employee[i].EmployeeCode) {
                $scope.EmpSummaryModelNew.EmployeeName = $scope.employee[i].EmployeeName;
                $scope.EmpSummaryModelNew.EmpSystemID = $scope.employee[i].SystemID;

                break;
            }
        }
    }
    $scope.closeEmployeeMultiplePopUp = function () {
        angular.element(document.querySelector('#employeeMultipleNewPopUp')).modal('hide');
    };

    $scope.employeeSummaryData = [];
    $scope.getPopUpDataSummaryOnly = function () {
        $scope.employeeSummaryData = [];
        $http({
            method: 'GET',
            url: 'employees/leaveApplication/getemployeelist'
            //url: 'HumanResource/AttendanceReport/getemployeeSummarylist?fromdate=' + $scope.EmpSummaryModelNew.FromDate + '&todate=' + $scope.EmpSummaryModelNew.ToDate
        }).then(function successCallback(response) {
            $scope.employeeSummaryData = response.data;
        });
    }
    //$scope.getPopUpDataSummaryOnly();

    $scope.getPopUpSummary = function () {
        if ($scope.EmpSummaryModelNew.FromDate == null) {
            ShowResult('Please select From Date', 'failure');
        }
        else if ($scope.EmpSummaryModelNew.ToDate == null) {
            ShowResult('Please select To Date', 'failure');
        }
        else {
            $scope.getPopUpDataSummaryOnly();
        angular.element(document.querySelector('#employeeMultipleNewPopUp')).modal('show');
        }
    }


    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.employeeSummaryData.length; i++) {
                $scope.employeeSummaryData[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#Grid").data("ejGrid");
        gridObj.refreshContent();
    };


    $scope.EmployeeSummaryList = [];
    
    $scope.GetEmployeeSummaryList = function () {
        $http({
            method: 'POST',
            url: 'HumanResource/AttendanceReport/GetEmployeeSummaryData',
            data: { 'fromdate': $scope.EmpSummaryModelNew.FromDate, 'todate': $scope.EmpSummaryModelNew.ToDate, 'empId': $scope.EmpSummaryModelNew.EmployeeSystemId},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.EmployeeSummaryList = [];
                $scope.EmployeeSummaryList = response.data;
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };



    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.saveemployeedata = function () {      
        $scope.idList = [];

        for (var di = 0; di < $scope.employeeSummaryData.length; di++) {
            if (($scope.employeeSummaryData[di].CheckBoxSelect)) {
                $scope.idList.push($scope.employeeSummaryData[di]);
                }   
        }
        if ($scope.idList.length > 0) {
            var uniqueSystemID = removeDuplicates($scope.idList, 'SystemID');
            var wcEmpCode = "";
            var wcEmpSystem = "";
            if (uniqueSystemID.length > 0) {
                wcEmpSystem = "IN(";
                 wcEmpSystem+= Array.prototype.map.call(uniqueSystemID, function (item) { return "'" + item.SystemID + "'"; }).join(",") + ")";

                wcEmpCode = Array.prototype.map.call(uniqueSystemID, function (item) { return "" + item.EmployeeCode + ""; }).join(",");
            }
            $scope.EmpSummaryModelNew.EmployeeSystemId =  wcEmpSystem;
            $scope.EmpSummaryModelNew.EmployeeCode = wcEmpCode;
        }
        $scope.closeEmployeeMultiplePopUp();
    }

    $scope.EmployeeAttendanceSummaryDataXls = function () {
        try {
            var dataList = [];
            var g = $("#EmpSummaryGrid").data("ejGrid");
            dataList = g.getFilteredRecords();

            if (dataList.length == 0) {
                dataList = $scope.EmployeeSummaryList;
            }
            if (dataList.length == 0) {
                throw "First click on Go button.";
            }
            $scope.fileName = "Employee Attendance Summary Report.xlsx";

            $http({
                method: 'POST',
                url: $scope.path + "EmployeeAttendanceSummaryDataXls",
                data: { 'reportFileName': $scope.fileName, 'data': dataList },
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

    //Employee Attandence Summary end

}



