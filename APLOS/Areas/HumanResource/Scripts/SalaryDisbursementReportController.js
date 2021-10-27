'use strict';
SalaryDisbursementReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$window'];
function SalaryDisbursementReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $window) {
    $rootScope.title = 'Salary Disbursement Report';
    $scope.path = 'humanresource/SalaryDisbursementReport/';
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.SaveSalaryLockUrl = $scope.path + 'Save';
    $scope.Action = 'Lock Salary';
    $scope.paymentMode = null;
    $scope.sheetType = false;
    $scope.cboSalaryProcessIdList = [];
    $scope.month = "";
    $scope.year = "";
    $scope.isCompletedMonth = null;
    $scope.salaryProcessId = null;
    $scope.isActive = true;
    $scope.isSeperated = false;
    $scope.isMaternity = false;
    $scope.isManualFilter = false;
    $scope.empGrid = false;
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
        x.setMonth(x.getMonth() - 1);

        for (var i = 0; i < $scope.yearList.length; i++) {
            if ($scope.yearList[i].Text === x.getFullYear().toString()) {
                $scope.year = $scope.yearList[i].Text;
                $scope.month = (x.getMonth() + 1).toString();
                continue;
            }
        }
        var DropDownListYear = $("#ddlYearList").data("ejDropDownList");
        DropDownListYear.selectItemByText($scope.year);

    };

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
        else {

            var parameters = {
                'effectiveDate': $scope.effectiveDate, 'salaryProcessId': $scope.salaryProcessId, 'payRollGroup': $scope.payGroupListSelected, 'isActive': $scope.isActive,
                'isSeperated': $scope.isSeperated,
                'isMaternity': $scope.isMaternity
            };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'humanresource/SalaryDisbursementReport/GetEmpInfo',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.empGrid = true;
                    $scope.EmployeeList = response.data;
                }
                else {
                    ShowResult("No Data Found", 'failure');
                    $scope.empGrid = false;
                }
                var gridObj = $("#empInfoGrid").data("ejGrid");
                gridObj.windowonresize();
                gridObj.refreshContent(true);

            });
        }
    };

    function daysInMonth(month, year) {
        return new Date(year, month, 0).getDate();
    }

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#empInfoGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeListTemp.length; i++) {
                $scope.EmployeeListTemp[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#empInfoGrid").data("ejGrid");
        gridObj.refreshContent();
    };


    $scope.GetEmployeeSalaryProcessedReportSalaryLogWise = function () {
        try {
            var parameters = [];
            var gridObj = $("#empInfoGrid").ejGrid("instance");
            var filteredRecords = gridObj.getFilteredRecords();
            /* if ($scope.isManualFilter == true) {*/
            if (filteredRecords.length == 0) {
                filteredRecords = $scope.EmployeeList;

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
                url: $scope.path + 'GetEmployeeSalaryProcessedReportSalLogWiseNew',
                data: {
                    'month': $scope.month,
                    'year': $scope.year,
                    'salaryProcessId': $scope.salaryProcessId,
                    'payRollGroup': $scope.payGroupListSelected,
                    'parameters': parameters,
                    'isActive': $scope.isActive,
                    'isSeperated': $scope.isSeperated,
                    'isMaternity': $scope.isMaternity
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
}



