'use strict';
EOTController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function EOTController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $rootScope.title = 'EOT';
    $scope.index = -1;
    $scope.maternityLeaveTransactions = [];
    $scope.path = 'Attendances/EOT/';
   // $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.includeCurrentDate = true;
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
    $scope.withSummary = true;
    $scope.FromDate = null;
    $scope.ToDate = null;

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
    $scope.GetEOTDetailsReport = function (reportType) {
        try {
            
            $scope.fileName = "EOTDetailsReport.xls";
            empParameters = [];
            var gridObj = $("#empInfoGrid").ejGrid("instance");
            var filteredRecords = gridObj.getFilteredRecords();

            if (filteredRecords.length == 0) {
                filteredRecords = $scope.EmployeeListTemp;
            }

            if ($scope.isManualFilter == true) {
                if (filteredRecords.length == 0) {
                    filteredRecords = $scope.EmployeeListTemp;

                }
            }
            if (angular.isUndefinedOrNull(filteredRecords) === false) {
                if (filteredRecords.length > 0) {
                    empParameters = [];
                    empParameters.push({ "Key": "EmpSystemId", "Value": getString(filteredRecords, "EmpSystemId") });
                }
            }
            if (empParameters.length === 0) {
                empParameters.push({ "Key": "", "Value": "" });

            }
            $http({
                method: 'POST',
                url: 'Attendances/EOT/GetEOTReport',
                data: {
                    'Month': $scope.month, 'Year': $scope.year, 'DayStatus': $scope.MonthlyAttendanceInformation.DayStatus, 'empParameters': empParameters
                    , 'includeCurrentDate': $scope.includeCurrentDate, 'withSummary': $scope.withSummary
                    , 'isActive': $scope.isActive, 'isSeperated': $scope.isSeperated, 'isMaternity': $scope.isMaternity
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    //if (reportType === 'EXCEL') {
                    //    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                    //}
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FullPath + "&fileName=" + response.data.FileName);//downloadgriddataUrlPath
                }
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    var empParameters = [];
    $scope.GetEOTSummaryReport = function (reportType) {
        try {

            $scope.fileName = "EOTSummaryReport.xls";
            empParameters = [];
            var gridObj = $("#empInfoGrid").ejGrid("instance");
            var filteredRecords = gridObj.getFilteredRecords();

            if (filteredRecords.length == 0) {
                filteredRecords = $scope.EmployeeListTemp;
            }

            if ($scope.isManualFilter == true) {
                if (filteredRecords.length == 0) {
                    filteredRecords = $scope.EmployeeListTemp;

                }
            }
            if (angular.isUndefinedOrNull(filteredRecords) === false) {
                if (filteredRecords.length > 0) {
                    empParameters = [];
                    empParameters.push({ "Key": "EmpSystemId", "Value": getString(filteredRecords, "EmpSystemId") });
                }
            }
            if (empParameters.length === 0) {
                empParameters.push({ "Key": "", "Value": "" });

            }
            $http({
                method: 'POST',
                url: 'Attendances/EOT/GetEOTSummaryReport',
                data: {
                    'Month': $scope.month, 'Year': $scope.year, 'DayStatus': $scope.MonthlyAttendanceInformation.DayStatus, 'empParameters': empParameters
                    , 'includeCurrentDate': $scope.includeCurrentDate, 'withSummary': $scope.withSummary
                    , 'isActive': $scope.isActive, 'isSeperated': $scope.isSeperated, 'isMaternity': $scope.isMaternity
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FullPath + "&fileName=" + response.data.FileName);//downloadgriddataUrlPath
                }
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetMonthlyAttendanceSummaryReport2 = function (reportType) {
        try {
            empParameters = [];
            var gridObj = $("#empInfoGrid").ejGrid("instance");
            var filteredRecords = gridObj.getFilteredRecords();

            if ($scope.isManualFilter == true) {
                if (filteredRecords.length == 0) {
                    filteredRecords = $scope.EmployeeListTemp;

                }
            }

            if (angular.isUndefinedOrNull(filteredRecords) === false) {
                if (filteredRecords.length > 0) {
                    empParameters = [];
                    empParameters.push(getString(filteredRecords, "EmpSystemId"));
                }
            }
            if (empParameters.length === 0) {
                empParameters.push("");

            }

            //location.target = '_blank';
            //location.href = "Attendances/AttendanceProcessUI/XlsDepWiseAttnRptView?Month=" + $scope.month + "&Year=" + $scope.year + "&DayStatus=" + $scope.MonthlyAttendanceInformation.DayStatus + "&withColor=" + $scope.withColor + "&empParameters=" + empParameters;

            var address = "Attendances/AttendanceProcessUI/XlsDepWiseAttnRptView?Month=" + $scope.month + "&Year=" + $scope.year + "&DayStatus=" + $scope.MonthlyAttendanceInformation.DayStatus + "&withColor=" + $scope.withColor + "&empParameters=" + empParameters;
            $window.open(address, '_blank');
            //$http({
            //    method: 'POST',
            //    url: 'Attendances/AttendanceProcessUI/XlsDepWiseAttnRptView',
            //    data: {
            //        'Month': $scope.month, 'Year': $scope.year,
            //        'DayStatus': $scope.MonthlyAttendanceInformation.DayStatus,
            //        'empParameters': empParameters,
            //        'withColor': $scope.withColor
            //    }
            //}).then(function successCallback(response) {
            //    if (response.data.Error === true) {
            //        ShowResult(response.data.Message, 'failure');
            //    }
            //    else {
            //        //if (reportType === 'EXCEL') {
            //        //    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            //        //}


            //        //var address = "Attendances/AttendanceProcessUI/XlsDepWiseAttnRptView?Month=" + $scope.month + "&Year=" + $scope.year + "&DayStatus=" + $scope.MonthlyAttendanceInformation.DayStatus + "&withColor=" + $scope.withColor + "&empParameters=" + empParameters;
            //        //$window.open(address, '_blank');
            //    }
            //});

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetMonthlyAttendanceSummaryReportALL = function (reportType) {
        try {
            empParameters = [];
            var gridObj = $("#empInfoGrid").ejGrid("instance");
            var filteredRecords = gridObj.getFilteredRecords();

            if ($scope.isManualFilter == true) {
                if (filteredRecords.length == 0) {
                    filteredRecords = $scope.EmployeeListTemp;

                }
            }
            if (angular.isUndefinedOrNull(filteredRecords) === false) {
                if (filteredRecords.length > 0) {
                    empParameters = [];
                    empParameters.push({ "Key": "EmpSystemId", "Value": getString(filteredRecords, "EmpSystemId") });
                }
            }
            if (empParameters.length === 0) {
                empParameters.push({ "Key": "", "Value": "" });

            }
            $http({
                method: 'POST',
                url: 'Attendances/AttendanceProcessUI/XlsDepWiseAttnRpt',
                data: {
                    'Month': $scope.month, 'Year': $scope.year, 'DayStatus': 'ALLSTATUS', 'empParameters': empParameters, 'withColor': $scope.withColor, 'includeCurrentDate': $scope.includeCurrentDate, 'withSummary': $scope.withSummary
                    , 'isActive': $scope.isActive, 'isSeperated': $scope.isSeperated, 'isMaternity': $scope.isMaternity
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    //if (reportType === 'EXCEL') {
                    //    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                    //}
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FullPath + "&fileName=" + response.data.FileName);//downloadgriddataUrlPath
                }
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.EmployeeList = [];
    $scope.EmployeeListDefault = [];
    $scope.EmployeeListTemp = [];
    $scope.GetEmployeeInformation = function () {
        try {
            var PlantId = "";
            var DropDownListObj = $("#CWPlant").data("ejDropDownList");
            if (!baseService.isUndefinedOrNull(DropDownListObj)) {
                PlantId = DropDownListObj.getSelectedValue();

                if (baseService.isUndefinedOrNull(PlantId)) {
                    throw "Select Plant..";
                }
            }
           
            

            var monthName = $scope.monthList.filter(function (mnth) {
                return mnth.Value == $scope.month;
            });
            $scope.effectiveDate = 1 + '-' + monthName[0].Text + '-' + $scope.year;

            if (angular.isUndefinedOrNull($scope.month)) {
                ShowResult("Select Month", 'failure');
            }
            if (angular.isUndefinedOrNull($scope.year)) {
                ShowResult("Select Year", 'failure');
            }

            else {

                var parameters = {
                    'effectiveDate': $scope.effectiveDate, 'payRollGroup': $scope.payGroupListSelected, 'isActive': $scope.isActive,
                    'isSeperated': $scope.isSeperated,
                    'isMaternity': $scope.isMaternity,
                    'PlantId': PlantId
                };
                $http({
                    method: "POST",
                    dataType: 'JSON',
                    url: 'Attendances/AttendanceProcessUI/GetEmpInfo',
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
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
        
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
                            $scope.EmployeeList[i].isToBeSelect = true;
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
                    $scope.EmployeeList[i].isToBeSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.EmployeeList[i].Id == filtered[j].Id)
                            $scope.EmployeeList[i].isToBeSelect = false;
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
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
        }

        var valobj = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.EmployeeList, { 'EmpSystemId': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].isToBeSelect == true)
                $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    };
    $scope.saveemployeedata = function () {
        $scope.EmployeeListTemp = [];
        var row = $filter('filter')($scope.EmployeeList, { 'isToBeSelect': true });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            $scope.EmployeeListTemp = row;
            $scope.isManualFilter = true;
        }
        $scope.Back();
    };
    $scope.showEmployeeFilterScreen = function () {
        try {

            var gridObj = $("#Gridemployee").data("ejGrid");
            gridObj.clearFiltering();
            angular.element(document.querySelector('#empfilterPopUp')).modal('show');


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.clearManualFilter = function () {
        $scope.isManualFilter = false;
        $scope.EmployeeListTemp = $scope.EmployeeList;
    };
    $scope.Back = function () {
        angular.element(document.querySelector('#empfilterPopUp')).modal('hide');
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
    $scope.PlantList = [];
    $scope.getPlant = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetPlantList",
        }).then(function successCallback(response) {
            $scope.PlantList = response.data;
           
            var index = 0;
            for (var i = 0; i < $scope.PlantList.length; i++) {
                if ($scope.PlantList[i].PlantId == $window.plantId) {
                    index = i;
                }
            }

            $('#CWPlant').ejDropDownList(
                {
                    dataSource: $scope.PlantList,
                    fields: { text: "PlantName", value: "PlantId" },
                    selectedIndex: index, showCheckBox: true, multiSelectMode: ej.MultiSelectMode.VisualMode
                    , width: 180
                });


        });
    }
    $scope.getPlant();

}