'use strict';
LunchOutDashboardController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', "$compile", 'baseService', 'fileReader', '$filter'];
function LunchOutDashboardController($scope, $http, $location, $rootScope, $window, $compile, baseService, fileReader, $filter) {
    $scope.path = 'Attendances/LunchOutDashboard/';
    $rootScope.title = 'Lunch Out Dashboard';

    $scope.appointments = [];

    //#region Month Select 

    $scope.LunchOut = {
        YearNo: null,
        MonthNo: null
    };
    $scope.monthList = [
        {
            Value: 1,
            Text: 'January'
        },
        {
            Value: 2,
            Text: 'February'
        },
        {
            Value: 3,
            Text: 'March'
        },
        {
            Value: 4,
            Text: 'April'
        },
        {
            Value: 5,
            Text: 'May'
        },
        {
            Value: 6,
            Text: 'June'
        },
        {
            Value: 7,
            Text: 'July'
        },
        {
            Value: 8,
            Text: 'August'
        },
        {
            Value: 9,
            Text: 'September'
        },
        {
            Value: 10,
            Text: 'October'
        },
        {
            Value: 11,
            Text: 'November'
        },
        {
            Value: 12,
            Text: 'December'
        }
    ];

    $scope.yearlist = [];
    $scope.GetCbo = function () {
        $http.get('Attendances/AttendanceProcessUI/GetCbo')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.yearlist = [];
                        $scope.yearlist = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetCbo();
    var d = new Date();
    $scope.LunchOut.YearNo = '' + d.getFullYear();
    var xx = d.getMonth() + 1;
    $scope.LunchOut.MonthNo = '' + xx;
    //#endregion

    //#region Get Data

    $scope.IsColorON = false;

    $scope.SimulateVisual = function () {
        var _data = { 'Year': $scope.LunchOut.YearNo, 'Month': $scope.LunchOut.MonthNo };
        var _path = $scope.path + "GetAttendanceData";

        try {
            $http({
                method: 'POST',
                url: _path,
                data: _data
            }).then(function successCallback(res) {

                if (res.data.DATA.length > 0) {
                    $scope.resourcedata2 = {
                        dataSource: res.data.GROUPDATA,
                        text: "text", id: "id", groupId: "groupId", color: "color"
                    };                    
                    $scope.appointments = angular.copy(res.data.DATA);
                }
            });
        } catch (e) {

        }
        //var schObj = $("#ResourceGroupSchedule").data("ejSchedule");
        //var appointments = schObj.getAppointments();


    }

    $scope.SimulateVisual();

    $scope.ResetAttendanceGrid = function () {
        var gridObj = $("#GridEdit").ejGrid("instance");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
    }

    $scope.rowDataBound = function rowDataBound(e) {
        try {
            if (e.column.field.endsWith(".DayStatus")) {
                if (e.text) {
                    
                    var Col = e.column.field.replace(".DayStatus", "");
                    if ($scope.IsColorON == true) {
                        e.cell.bgColor = e.data[Col].Color;
                    }
                    else {
                        
                        e.cell.bgColor = e.data[Col].LColor;
                    }
                    
                }
            }
        } catch (e) {

        }
    }

    $scope.ShowDiv = false;
    $scope.GetValue = function (obj) {
        $scope.XX = obj.data.EmpSystemID;
        try {
            $scope.Date = obj.columnName;
            $scope.TDate = new Date($scope.LunchOut.YearNo, $scope.LunchOut.MonthNo, $scope.Date);
            $scope.Sdate = $scope.TDate.setMonth($scope.TDate.getMonth() - 1);
            $scope.QDate = new Date($scope.Sdate);
            var date = $scope.QDate, y = date.getFullYear(), m = date.getMonth();
            $scope.FinalDate = $filter('dateFiltering')(new Date(y, m, $scope.Date), 'dd-MM-yyyy');

            $scope.ShowDiv = true;
            var eDialog = $("#Base").data("ejDialog");
            eDialog.open();
            $scope.GetEmpData($scope.XX, $scope.FinalDate);
            $scope.GetRawData($scope.XX, $scope.FinalDate);
        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    //#endregion

    //#region Job Card Donwload
    $scope.GetConsumption = function (obj) {
        $scope.ReportFormat = "Excel";
        $scope.FromDate = new Date($scope.LunchOut.YearNo, $scope.LunchOut.MonthNo, 1); //Get Selected Year,Month No, Date
        $scope.Xdate = $scope.FromDate.setMonth($scope.FromDate.getMonth() - 1);
        $scope.YDate = new Date($scope.Xdate);
        var date = $scope.YDate, y = date.getFullYear(), m = date.getMonth();
        $scope.EmpId = obj.data.EmpSystemID;
        $scope.firstDate = $filter('dateFiltering')(new Date(y, m, 1), 'dd-MM-yyyy');
        $scope.MonthLastDate = $filter('dateFiltering')(new Date(y, m + 1, 0), 'dd-MM-yyyy');

        var url = 'Attendances/ComplianceAttendanceSetting/GetComplianceJobCardReport?reportFormat=' + $scope.ReportFormat + '&fromDate=' + $scope.firstDate + '&toDate=' + $scope.MonthLastDate + '&employeeId=' + $scope.EmpId + '&chkAdditionInfo=' + true;
        $rootScope.report(url);
    }
    //#endregion

    //#region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion Tab

    //#region Get Data From Selected Employee.
    $scope.EmpModel = {
        SystemId: null,
        EmployeeCode: null,
        EmployeeName: null,
        WorkDate: null,
        DayStatus: null,
        ShiftName: true,
        ShiftInTime: null,
        ShiftOutTime: null,
        PunchInTime: null,
        PunchOutTime: null,
        LunchInTime: null,
        LunchOutTime: null,
        LateDuration: '',
        LeaveName: null,
        LeaveFrom: null,
        LeaveTo: null,
        LeaveDays: null,
        PreviousDate: null,
        PreviousDateInTime: null,
        PreviousDateOutTime: null,
        PreviousDayStatus: null,
        NextDate: null,
        NextDateInTime: null,
        NextDateOutTime: null,
        NextDayStatus: null,
        TodaysDate: null,
    };
    $scope.clr = null;
    $scope.clor = null;
    $scope.color = null;
    $scope.GetEmpData = function (EmpId,Date) {
        try {
            $http({
                method: 'GET',
                url: $scope.path + 'GetEmployeeData?EmpId=' + EmpId + '&Date=' + Date,
            }).then(function successCallback(response) {
                //angular.copy(response.data[0], $scope.EmpModel);
                $scope.EmpModel = response.data[0];
                if ($scope.EmpModel.IsManualDayStatus == true) {
                    $scope.clr = "red";
                } else {
                    $scope.clr = "Black";
                }
                if ($scope.EmpModel.IsManualInTime == true) {
                    $scope.clor = "red";
                } else {
                    $scope.clor = "Black";
                }
                if ($scope.EmpModel.IsManualOutTime == true) {
                    $scope.color = "red";
                } else {
                    $scope.color = "Black";
                }
            });
        } catch (e) {

        }
    }
    $scope.EmpDataList = [];
    $scope.GetRawData = function (EmpId, Date) {
        try {
            $http({
                method: 'GET',
                url: $scope.path + 'GetRawData?EmpId=' + EmpId + '&Date=' + Date,
            }).then(function successCallback(response) {
                //angular.copy(response.data[0], $scope.EmpModel);
                $scope.EmpDataList = response.data;
            });
        } catch (e) {

        }
    }

    //#endregion
}





