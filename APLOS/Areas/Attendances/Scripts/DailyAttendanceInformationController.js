'use strict';
DailyAttendanceInformationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function DailyAttendanceInformationController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Daily Attendance Information';
    $scope.index = -1;
    $scope.maternityLeaveTransactions = [];
    $scope.path = 'Attendances/DailyAttendanceInformation/';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.MonthlyAttendanceInformation = {
        YearNo: null,
        MonthNo: null,
        DayStatus: 'DAYSTATUS'
    };
    $scope.isActive = true;
    $scope.isSeperated = true;
    $scope.isMaternity = false;
    $scope.withColor = true;
    $scope.isManualFilter = false;
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
    $scope.year = new Date().getFullYear().toString();
    $scope.month = new Date().getMonth().toString();

    $scope.date = new Date();


    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });
    $scope.SelectDefaultValue = function (args) {
        var x = new Date();
        x.setDate(10);
        x.setMonth(x.getMonth());

        for (var i = 0; i < $scope.yearList.length; i++) {
            if ($scope.yearList[i].Text === x.getFullYear().toString()) {
                $scope.year = $scope.yearList[i].Text;
                $scope.month = (x.getMonth() + 1).toString();
                continue;
            }
        }

        //$scope.year = "2018";
        var DropDownListYear = $("#ddlYearList").data("ejDropDownList");
        DropDownListYear.selectItemByText($scope.year);

    };
    $scope.empGridShow = function (args) {
        ShowResult('Press the Go Button  After Year/Month Change', 'success');
        $scope.empGrid = false;
    };

    var empParameters = [];
    $scope._GetMonthlyAttendanceSummaryReport = function (reportType) {
        try {
            empParameters = [];
            var gridObj = $("#empInfoGrid").ejGrid("instance");
            var filteredRecords = gridObj.getFilteredRecords();
            if (filteredRecords.length == 0)
                filteredRecords = $scope.EmployeeListTemp;
            var reportFormat = "Excel";
            if ($scope.isManualFilter == true) {
                if (filteredRecords.length == 0) {
                    filteredRecords = $scope.EmployeeListTemp;

                }
            }

            if (filteredRecords.length > 140) {
                throw "Max. Download Limit is 140";
            }

            var file_src = 'Attendances/DailyAttendanceInformation/XlsDepWiseAttnRpt?effectiveDate=' + $scope.date + '&empParameters=' + EmployeeList(filteredRecords)
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetMonthlyAttendanceSummaryReport = function () {
        try {
            var parameters = [];
            var gridObj = $("#empInfoGrid").ejGrid("instance");
            var filteredRecords = gridObj.getFilteredRecords();
            /* if ($scope.isManualFilter == true) {*/
            if (filteredRecords.length == 0) {
                filteredRecords = $scope.EmployeeListTemp;

            }
            //}
            if (angular.isUndefinedOrNull(filteredRecords) === false) {
                if (filteredRecords.length > 0) {
                    parameters = [];
                    parameters.push({ "Key": "EmpSystemId", "Value": getString(filteredRecords, "EmpSystemId") });
                }
            }
            if (parameters.length === 0) {
                parameters.push({ "Key": "", "Value": "" });

            }

            $http({
                method: 'POST',
                url: 'Attendances/DailyAttendanceInformation/XlsDepWiseAttnReport',
                data: {
                    
                    'effectiveDate': $scope.date,
                    'parameters': parameters
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    function EmployeeList(filteredRecords) {
        $scope.empParameters = [];
        if (filteredRecords.length > 0) {
            for (var i = 0; i < $scope.EmployeeListTemp.length; i++) {
                $scope.empParameters.push($scope.EmployeeListTemp[i]['EmpSystemId']);
            }
        }
        return JSON.stringify($scope.empParameters);
    }


    $scope.EmployeeList = [];
    $scope.EmployeeListDefault = [];
    $scope.EmployeeListTemp = [];
    $scope.GetEmployeeInformation = function () {

        //var monthName = $scope.monthList.filter(function (month) {
        //    return mnth.Value == $scope.month;
        //});
        //$scope.effectiveDate = 1 + '-' + monthName[0].Text + '-' + $scope.year;

        //if (angular.isUndefinedOrNull($scope.month)) {
        //    ShowResult("Select Month", 'failure');
        //}
        //if (angular.isUndefinedOrNull($scope.year)) {
        //    ShowResult("Select Year", 'failure');
        //}

        //else {

            var parameters = {
                'effectiveDate': $scope.date, 'payRollGroup': $scope.payGroupListSelected, 'isActive': $scope.isActive,
                'isSeperated': $scope.isSeperated,
                'isMaternity': $scope.isMaternity
            };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'Attendances/DailyAttendanceInformation/GetEmpInfo',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.empGrid = true;
                    $scope.EmployeeListDefault = response.data;//.filter(d => d.isSelect == true);
                    $scope.EmployeeList = $scope.EmployeeListDefault;
                    $scope.EmployeeListTemp = $scope.EmployeeListDefault;
                }
                else {
                    $scope.empGrid = false;
                    ShowResult("No Data Found", 'failure');
                }
            });
        //}
    };

    //------Multiple Selection(Excel)-------//
    function checkChangeemployee(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.employeeAttendanceBySingleDateSelection, { 'Id': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Active = true;
            else
                row[0].Active = false;
        }

    }
    function headCheckChangeemployee(e) {
        if (e.model.checkState == "check") {

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {

                    $scope.EmployeeList[i].isSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.EmployeeList[i].EmpSystemId == filtered[j].EmpSystemId)
                            // $scope.EmployeeList[i].isSelect = true;
                            $scope.EmployeeList[i].CheckBoxSelect = true;
                    }

                }
            }

            var checkbox = $("#Gridemployee .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        else {
            var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    $scope.EmployeeList[i].CheckBoxSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.EmployeeList[i].Id == filtered[j].Id)
                            $scope.EmployeeList[i].CheckBoxSelect = false;
                    }

                }
            }
            var checkbox = $("#Gridemployee .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee = function (args) {
        $("#Gridemployee .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    };


    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeList.length; i++) {
                $scope.EmployeeList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#Gridemployee").data("ejGrid");
        gridObj.refreshContent();
    };


    $scope.saveemployeedata = function () {
        $scope.EmployeeListTemp = [];
        var row = $filter('filter')($scope.EmployeeList, { 'CheckBoxSelect': true });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            $scope.EmployeeListTemp = row;
            //$scope.isManualFilter = true;
        }
        $scope.Back();
    };
    $scope.showEmployeeFilterScreen = function () {
        try {

            var gridObj = $("#Gridemployee").data("ejGrid");
            gridObj.clearFiltering();
            ///angular.element(document.querySelector('#empfilterPopUp')).modal('show');
            $("#empfilterPopUp").data("ejDialog").open();

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.clearManualFilter = function () {
        $scope.isManualFilter = false;
        $scope.EmployeeListTemp = $scope.EmployeeList;
    };
    $scope.Back = function () {
        //angular.element(document.querySelector('#empfilterPopUp')).modal('hide');
        $("#empfilterPopUp").data("ejDialog").close();
    };
    var getString = function (data, column) {
        var string = "''";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) === false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }

        return string;
    };
    //--------------------------------------//

}