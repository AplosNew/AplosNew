'use strict';
paySlipsNewController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$window'];
function paySlipsNewController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $window) {

    $scope.path = 'humanresource/payrollReports/';
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';
    $scope.paymentMode = null;
    $scope.sheetType = false;
    $scope.cboSalaryProcessIdList = [];
    $scope.month = "";
    $scope.year = "";
    $scope.isCompletedMonth = null;
    $scope.IncludingZeroHeads = false;
    $scope.salaryProcessId = null;
    $scope.languageId = null;
    $scope.empGrid = false;
    cboService.getLanguageIdCbo(function (result) {
        $scope.localLanguageList = result;
    });
    $scope.isActive = true;
    $scope.isSeperated = false;
    $scope.isMaternity = false;
    $scope.ReportFormat = 'Pdf';
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
    $scope.singleEmployeePrint = false;

    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });

    $scope.payGroupList = [];
    $scope.payGroupListSelected = [];

    cboService.getPayRollGroupCbo(function (result) {
        $scope.payGroupList = result;
    });

    $scope.create = function (args) {
        $("#checkBox").ejCheckBox({
            change: function (args) {
                var obj = $("#ddlPayRollGroupList").ejDropDownList("instance");
                if (args.isChecked) obj.checkAll();
                else obj.uncheckAll();
            },
            text: "Select All",
            cssClass: "temp"
        });

    };

    $scope.getSalaryProcessIdList = function (args) {
        $scope.isCompletedMonth = 1;

        var DropDownListMonth = $("#ddlMonthList").data("ejDropDownList");
        var DropDownListYear = $("#ddlYearList").data("ejDropDownList");


        $scope.month = DropDownListMonth.getSelectedValue();
        $scope.year = DropDownListYear.getSelectedValue();
        if (angular.isUndefinedOrNull($scope.year)) {
            ShowResult("Select Year", 'failure');
        }
        else {
            cboService.getSalaryProcessIdCboByYearMonth($scope.month, $scope.year, $scope.isCompletedMonth, function (result) {
                $scope.cboSalaryProcessIdList = result;
            });
        }


    };

    $scope.selectedPaymentMode = $("#paymentMode option:selected").text();
    $scope.selectedEmployeeCategory = $("#employeeCategoryId option:selected").text();
    $scope.payGroupListSelected = [];

    $scope.EmployeeList = [];
    $scope.EmployeeListDefault = [];
    $scope.EmployeeListTemp = [];
    $scope.GetEmployeeInformation = function () {
        //var DropDownListObj = $("#ddlPayRollGroupList").data("ejDropDownList");
        //$scope.payGroupListSelected = DropDownListObj.getSelectedValue();

        //if (angular.isUndefinedOrNull($scope.year) === false && angular.isUndefinedOrNull($scope.month) === false) {
        //    var DropDownListSalaryProcess = $("#ddlSalaryProcessId").data("ejDropDownList");
        //    $scope.salaryProcessId = DropDownListSalaryProcess.getSelectedValue();
        //}
        var monthName = $scope.monthList.filter(function (mnth) {
            return mnth.Value == $scope.month;
        });
        $scope.effectiveDate = daysInMonth($scope.month, $scope.year) + '-' + monthName[0].Text + '-' + $scope.year;

        if (angular.isUndefinedOrNull($scope.month)) {
            ShowResult("Select Month", 'failure');
        }
        if (angular.isUndefinedOrNull($scope.year)) {
            ShowResult("Select Year", 'failure');
        }
        //if ($scope.payGroupListSelected.length == 0) {
        //    ShowResult("Select Payroll Group", 'failure');
        //}
        else {

            var parameters = {
                'effectiveDate': $scope.effectiveDate, 'payRollGroup': $scope.payGroupListSelected, 'isActive': $scope.isActive,
                'isSeperated': $scope.isSeperated,
                'isMaternity': $scope.isMaternity
            };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'payrolls/PaySlips/GetEmpInfo',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.empGrid = true;
                    $scope.EmployeeListDefault = response.data.filter(d => d.isSelect == true);
                    $scope.EmployeeList = $scope.EmployeeListDefault;
                    $scope.EmployeeListTemp = $scope.EmployeeListDefault;
                }
                else {
                    $scope.empGrid = false;
                    ShowResult("No Data Found", 'failure');
                }
            });
        }
    };
    $scope.downloadgriddataUrlPath = 'GridReports/Download';
    $scope.GetPaySlip = function () {
        try {
            var parameters = [];
            var gridObj = $("#empInfoGrid").ejGrid("instance");
            var filteredRecords = gridObj.getFilteredRecords();

            if (filteredRecords.length == 0) {
                filteredRecords = $scope.EmployeeListTemp;
            }

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
                url: 'payrolls/paySlipsNew/GetEmployeePaySlip',
                data: {
                    'month': $scope.month,
                    'year': $scope.year,
                    'salaryProcessId': $scope.salaryProcessId,
                    'parameters': parameters,
                    'languageId': $scope.languageId,
                    'isActive': $scope.isActive,
                    'isSeperated': $scope.isSeperated,
                    'isMaternity': $scope.isMaternity,
                    'IsIncludingZeroHeads': $scope.IncludingZeroHeads,
                    'singleEmployee': $scope.singleEmployeePrint,
                    'reportFormat': $scope.ReportFormat
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    if ($scope.ReportFormat == 'Pdf') {
                        $rootScope.report($scope.downloadgriddataPDFUrl + "?FileName=" + response.data.FileName);
                    } else {
                        $window.open($scope.downloadgriddataUrlPath + "?FileName=" + response.data.FileName);
                    }
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
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
    function daysInMonth(month, year) {
        return new Date(year, month, 0).getDate();
    }
    $scope.SelectDefaultValue = function (args) {
        var x = new Date();
        x.setDate(10);
        x.setMonth(x.getMonth() - 1);

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
    //--------------------------------------//
}



