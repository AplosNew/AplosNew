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

    $scope.SearchEmployee = function () {
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
    //$scope.GetEmployeeSummaryList = function () {
    //    var NewEmployeeSummaryList = [];
    //    for (var i = 0; i < $scope.employeeSummaryData.length; i++) {
    //        if ($scope.employeeSummaryData[i].CheckBoxSelect == true) {

    //            NewEmployeeSummaryList.push($scope.employeeSummaryData[i]);
    //        }
    //    }
    //    if (NewEmployeeSummaryList.length == 0) {
    //        ShowResult('Please select at least one Party', 'failure');
    //    }
    //    $http.get('HumanResource/AttendanceReport/GetEmployeeSummaryData?fromdate=' + $scope.EmpSummaryModelNew.FromDate + '&todate=' + $scope.EmpSummaryModelNew.ToDate + '&empId=' + NewEmployeeSummaryList)
    //        .then(function (response) {
    //            $scope.EmployeeSummaryList = [];
    //            $scope.EmployeeSummaryList = response.data;
    //            $scope.closeEmployeeMultiplePopUp();
    //        });
    //};

    $scope.saveemployeedata = function () {
        var row = $filter('filter')($scope.employeeSummaryData, { 'CheckBoxSelect': true });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            $scope.EmployeeSummaryList = row;
        }
        $scope.closeEmployeeMultiplePopUp();
    }


    //$scope.EmployeeAttendanceSingleSummaryData = function () {
    //    try {
    //        var dataList = [];
    //        var g = $("#EmpGrid").data("ejGrid");
    //        dataList = g.getFilteredRecords();

    //        if (dataList.length == 0) {
    //            dataList = $scope.EmployeeSummaryList;
    //        }
    //        if (dataList.length == 0) {
    //            throw "First click on View button.";
    //        }
    //        $scope.fileName = "Employee Attendance Data.xlsx";

    //        $http({
    //            method: 'POST',
    //            url: $scope.path + "EmployeeAttendanceDataXls",
    //            data: { 'reportFileName': $scope.fileName, 'data': dataList },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error == true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
    //            }
    //        }, function errorCallback(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        });
    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }
    //}

    //Employee Attandence Summary end

}



