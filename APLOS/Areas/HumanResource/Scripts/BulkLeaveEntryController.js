'use strict';
BulkLeaveEntryController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$window', '$filter'];
function BulkLeaveEntryController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $window, $filter) {
    $rootScope.title = 'Bulk Leave Entry';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.preRecruitmentEmployees = [];
    $scope.path = 'HumanResource/BulkLeaveEntry/';

    $scope.FromDateSingleDate = '';
    $scope.FromDate = '';
    $scope.ToDate = '';
    // #region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    function nullrecorder(val) {
        if (baseService.isUndefinedOrNull(val))
            return "";

        return val;
    }
    $scope.queryCellInfo = function (args) {
        if (args.data.IsManualDayStatus == true) {
            if (args.column.field == "IsManualDayStatus" || args.column.field == "DayStatus") {
                args.cell.bgColor = "#FF911D";
            }
        }
    }
    $scope.selectemployee = [];
    $scope.selectedSinglemployee = {};
    $scope.getAllEmployee = function () {

        var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
        eDialog.open();

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'fromdate': $scope.FromDate, 'todate': $scope.ToDate },
            url: $scope.path + 'getAllEmployees'

        }).then(function successCallback(response) {
            $scope.selectemployee = response.data;

        });
    }


    $scope.employeeAttendance = [];
    $scope.employeeAttendanceBySingleDate = [];
    $scope.employeeAttendanceBySingleDatePending = [];
    $scope.allShift = [];
    $scope.selectSignleEmployee = function (args) {
        var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
        eDialog.close();
        if (baseService.isUndefinedOrNull(args) == false)
            $scope.selectedSinglemployee = args.data;

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'employeeid': $scope.selectedSinglemployee.Id, 'fromdate': $scope.FromDate, 'todate': $scope.ToDate },
            url: $scope.path + 'getAttendanceData'

        }).then(function successCallback(response) {
            $scope.employeeAttendance = response.data.data;
            $scope.allShift = response.data.shift;

            var gridObj = $("#GridChangeAttendance").data("ejGrid");
            gridObj.refreshContent();
        });


    }
    $scope.allShiftSingleDay = [];

    $scope.selectSigleDate = function () {
        cboService.getCboLeaveYear(function (result) {
            $scope.leaveYearlist = result;
            $scope.YearNo = $filter("filter")($scope.leaveYearlist, { Text: new Date($scope.FromDateSingleDate).getFullYear() })[0].Value;

            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'pdate': $scope.FromDateSingleDate },
                url: $scope.path + 'getAttendanceData'

            }).then(function successCallback(response) {
                $scope.employeeAttendanceBySingleDate = response.data.data;



            });
        });

    }

    $scope.leaveYearlist = [];
    $scope.YearNo = null;
    $scope.LeaveBalanceList = [];
    $scope.getLeaveBalance = function (args) {
        cboService.getCboLeaveYear(function (result) {
            $scope.leaveYearlist = result;
            $scope.YearNo = $filter("filter")($scope.leaveYearlist, { Text: new Date($scope.FromDateSingleDate).getFullYear() })[0].Value;

            $http.get('Employees/LeaveApplication/GetEmpLeaveBalance?EmpsystemId=' + args.data.Id + '&calanderYearId=' + $scope.YearNo)
                .then(function (response) {
                    $scope.LeaveBalanceList = response.data;

                    var eDialog = $("#ViewLeaveInfo").data("ejDialog");
                    eDialog.open();
                });
        });

    };



    $window.onload = function (event) {
        $scope.actionCompletePending("refresh");
        $scope.actionCompleteSingleDay("refresh");

    }
    $window.onresize = function (event) {
        $scope.actionCompletePending("refresh");
        $scope.actionCompleteSingleDay("refresh");

    }
    $scope.actionCompletePending = function (args) {
        try {
            if (args == "refresh" || args.requestType == "refresh") {
                var scrollerwidth = 0;
                var gridObj = null;
                try {
                    gridObj = $("#GridChangeAttendanceBySingleDatePending").ejGrid("instance");
                    scrollerwidth = $("#TabPending").width();//Obtain the width of the container
                    gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                    gridObj.windowonresize();

                } catch (e) {

                }
            }
        } catch (e) {

        }
    }
    $scope.actionCompleteSingleDay = function (args) {
        try {
            if (args == "refresh" || args.requestType == "refresh") {
                var scrollerwidth = 0;
                var gridObj = null;

                try {
                    gridObj = $("#GridChangeAttendanceBySingleDate").ejGrid("instance");
                    scrollerwidth = $("#TabDateRange").width();//Obtain the width of the container
                    gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                    gridObj.windowonresize();
                } catch (e) {

                }



            }
        } catch (e) {

        }
    }
    $scope.changeShift = function (args) {

        for (var i = 0; i < $scope.employeeAttendance.length; i++) {
            for (var j = 0; j < $scope.allShift.length; j++) {
                //new Date(year, month, day, hours, minutes, seconds, milliseconds)
                if ($scope.employeeAttendance[i].ShiftName == $scope.allShift[j].UserName) {
                    $scope.employeeAttendance[i].ShiftSystemID = $scope.allShift[j].SystemID;



                }


            }

        }

        return;

    }
    $scope.ActionchangeShift = function (args) {

    }

    $scope.shiftinfo = {};
    $scope.selectedShiftInfo = function (args) {
        var eDialog = $("#ViewShiftInfo").data("ejDialog");
        eDialog.open();

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'systemid': args.data.ShiftSystemID, 'WorkDate': args.data.WorkDate },
            url: $scope.path + 'getShift'

        }).then(function successCallback(response) {
            $scope.shiftinfo = response.data[0];
        });


    }

    $scope.attendanceinfo = [];
    $scope.showAttendanceInfo = function (args) {
        var eDialog = $("#ViewAttendanceInfo").data("ejDialog");
        eDialog.open();

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'empsystemid': args.data.Id, 'WorkDate': args.data.WorkDate },
            url: $scope.path + 'getAttendance'

        }).then(function successCallback(response) {
            $scope.attendanceinfo = response.data;
        });


    }
    $scope.rowDataBoundSingleEmployee = function rowDataBoundSingleEmployee(e) {

        if (!baseService.isUndefinedOrNull(e.data.ErrorMessage) && e.data.ErrorMessage != "")
            e.row.css("background-color", "#ff0000");

    }
    $scope.SaveSingleDay = function () {
        var DataToBeSaved = [];
        for (var i = 0; i < $scope.employeeAttendanceBySingleDate.length; i++) {
            //$scope.employeeAttendanceBySingleDate[i].ErrorMessage = "";
            try {
                if (nullrecorder($scope.employeeAttendanceBySingleDate[i].LTSystemIDOriginal) != nullrecorder($scope.employeeAttendanceBySingleDate[i].LTSystemID)) {
                    DataToBeSaved.push($scope.employeeAttendanceBySingleDate[i]);
                }
            } catch (e) {

            }

        }

        $http({
            method: "POST",
            dataType: 'JSON',
            data: {
                'data': DataToBeSaved, 'workdate': $scope.FromDateSingleDate, 'yearid': $scope.YearNo
            },
            url: $scope.path + 'SaveSingleEmployee'

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');

                for (var i = 0; i < response.data.Data.length; i++) {
                    var row = $filter('filter')($scope.employeeAttendanceBySingleDate, { 'Id': response.data.Data[i].Id });
                    if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
                        row[0].ErrorMessage = response.data.Data[i].ErrorMessage;
                    }
                }

                var gridObj = $("#GridChangeAttendanceBySingleDate").data("ejGrid");
                gridObj.refreshContent();
            }
            else {
                ShowResult(response.data.Message, 'success');

                $scope.selectSigleDate();
                $scope.GetGrdAvailedLvDetails();

            }

        });


    }
    $scope.SaveSingleDayPending = function (args, flag) {

        $http({
            method: "POST",
            dataType: 'JSON',
            data: {
                'employeeid': args.data.Id, 'workdate': args.data.PDate, 'inOrOut': flag
            },
            url: $scope.path + 'SaveSingleEmployee'

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');

                var gridObj = $("#GridChangeAttendanceBySingleDate").data("ejGrid");
                gridObj.refreshContent();
            }
            else {
                ShowResult(response.data.Message, 'success');


            }

        });


    }

    //#region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    //#endregion

    //#region --Approval--

    $scope.AvailedLvDetails = [];
    $scope.LeaveBalanceList = [];
    $scope.getListUrl = 'HumanResource/BulkLeaveEntry/GetGrdAvailedLvDetails';
    $scope.savelvRejectUrl = 'HumanResource/BulkLeaveEntry/SaveLeaveReject';
    $scope.savelvApprovalUrl = 'HumanResource/BulkLeaveEntry/SaveLeaveApproval';
    $scope.getlvBalanceUrl = 'HumanResource/BulkLeaveEntry/GetEmpLeaveBalance';

    $scope.GetGrdAvailedLvDetails = function () {
        $scope.AvailedLvDetails = [];
        $http.get($scope.getListUrl)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.AvailedLvDetails = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetGrdAvailedLvDetails();

    $scope.SelectLvDetails = function () {


        var eDialog = $("#dialogLvDetails").data("ejDialog");
        eDialog.open();
        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.LeaveBalanceList = [];
        $http.get($scope.getlvBalanceUrl + '?EmpsystemId=' + data.EmployeeID)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.LeaveBalanceList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };
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
            for (var i = 0; i < $scope.AvailedLvDetails.length; i++) {
                $scope.AvailedLvDetails[i].CheckBoxSelect = ChkOrUnchk;
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






    $scope.CancelationReason = null;

    $scope.SavelvApproval = function () {
        //$scope.AvailedLvDetails = [];
        try {
            var LvList = [];
            for (var i = 0; i < $scope.AvailedLvDetails.length; i++) {

                if ($scope.AvailedLvDetails[i].CheckBoxSelect === true) {
                    LvList.push($scope.AvailedLvDetails[i]);
                }

            }
            if (LvList.length == 0) {
                throw "Please Select Employees Leave.";
            }

            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'LeaveData': LvList },
                url: $scope.savelvApprovalUrl

            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, "success");
                    $scope.GetGrdAvailedLvDetails();


                }
            }, function errorCallback(response) {
                ShowResult(response.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetdialogCancelationReason = function () {
        try {
            var LvList = [];
            for (var i = 0; i < $scope.AvailedLvDetails.length; i++) {

                if ($scope.AvailedLvDetails[i].CheckBoxSelect === true) {
                    LvList.push($scope.AvailedLvDetails[i]);
                }

            }
            if (LvList.length == 0) {
                throw "Please Select Employees.";
            }
            var eDialog = $("#dialogCancelationReason").data("ejDialog");
            eDialog.open();
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };
    $scope.Cancel = function () {

        var eDialog = $("#dialogCancelationReason").data("ejDialog");
        eDialog.close();
        $scope.CancelationReason = null;

    };

    $scope.Reject = function () {
        //$scope.AvailedLvDetails = [];
        try {

            if (baseService.isUndefinedOrNull($scope.CancelationReason)) {
                throw "Please Enter Cancelation Reason.";
            }
            var LvList = [];
            for (var i = 0; i < $scope.AvailedLvDetails.length; i++) {

                if ($scope.AvailedLvDetails[i].CheckBoxSelect === true) {
                    LvList.push($scope.AvailedLvDetails[i]);
                }

            }
            if (LvList.length == 0) {
                throw "Please Select Employees.";
            }

            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'LeaveData': LvList, 'CancelationReason': $scope.CancelationReason },
                url: $scope.savelvRejectUrl

            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, "success");
                    $scope.GetGrdAvailedLvDetails();
                    var eDialog = $("#dialogCancelationReason").data("ejDialog");
                    eDialog.close();
                    $scope.CancelationReason = null;


                }
            }, function errorCallback(response) {
                ShowResult(response.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    //#endregion

}