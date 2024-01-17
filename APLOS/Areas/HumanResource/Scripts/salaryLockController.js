'use strict';
salaryLockController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$window'];
function salaryLockController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $window) {
    $rootScope.title = 'Lock Salary ';
    $scope.path = 'humanresource/SalaryLock/';
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
                url: 'humanresource/SalaryLock/GetEmpInfo',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.empGrid = true;
                    $scope.EmployeeListDefault = response.data.filter(d => d.isSelect == true);
                    $scope.EmployeeList = $scope.EmployeeListDefault;
                    $scope.EmployeeListTemp = $scope.EmployeeListDefault;

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
                $scope.EmployeeListTemp[i].isToBeSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].isToBeSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#empInfoGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.EmployeeListNew = [];
    function filteredData() {
        $scope.EmployeeListNew = [];
        var dataList = [];
        var g = $("#empInfoGrid").data("ejGrid");
        dataList = g.getFilteredRecords();
        if (baseService.arrayLength(dataList) == 0) {
            dataList = $scope.EmployeeListTemp;
        }
        if (baseService.arrayLength(dataList) > 0) {
            for (var i = 0; i < dataList.length; i++) {
                if (dataList[i].isToBeSelect) {
                    if (dataList[i].IsDisburse == 'Not Disbursed' && baseService.isUndefinedOrNull(dataList[i].PayableVoucherNo)) {
                        $scope.EmployeeListNew.push(dataList[i]);
                    }
                }
            }
        }
    }



    $scope.SalaryLock = function () {
        try {
            filteredData();
            //var EmployeeListNew = [];
            //for (var i = 0; i < $scope.EmployeeListTemp.length; i++) {
            //    EmployeeListNew.push($scope.EmployeeListTemp[i]);
            //}

            if ($scope.EmployeeListNew.length == 0) {
                throw "Please Select Employee";
            }

            // var data = ej.DataManager(EmployeeListNew).executeLocal(ej.Query().select(["EmpSystemId", "PayableVoucherId", "DisbursementVoucherId", "Id", "Flag", "CheckBoxSelect", "SalaryStructureId", "EmployeeCode"]));

            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.SaveSalaryLockUrl,
                data: {
                    'EmployeeList': $scope.EmployeeListNew, 'Month': $scope.month, 'Year': $scope.year, 'isActive': $scope.isActive, 'isSeperated': $scope.isSeperated, 'isMaternity': $scope.isMaternity, 'SalaryStructureId': $scope.SalaryStructureId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                    //$scope.GetEmployeeInformation();
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetEmployeeInformation();
                    var gridObj = $("#empInfoGrid").data("ejGrid");
                    gridObj.refreshContent();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SalaryUnLock = function () {
        try {
            var EmployeeListNew = [];
            for (var i = 0; i < $scope.EmployeeListTemp.length; i++) {
                EmployeeListNew.push($scope.EmployeeListTemp[i]);
            }

            if (EmployeeListNew.length == 0) {
                throw "Please Select LeaveType";
            }

            var data = ej.DataManager(EmployeeListNew).executeLocal(ej.Query().select(["EmpSystemId", "PayableVoucherId", "DisbursementVoucherId", "Id", "Flag", "CheckBoxSelect", "SalaryStructureId"]));

            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.SaveSalaryLockUrl,
                data: {
                    'EmployeeList': data, 'Month': $scope.month, 'Year': $scope.year, 'isActive': $scope.isActive, 'isSeperated': $scope.isSeperated, 'isMaternity': $scope.isMaternity, 'SalaryStructureId': $scope.SalaryStructureId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetEmployeeInformation();
                    var gridObj = $("#empInfoGrid").data("ejGrid");
                    gridObj.refreshContent();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $window.onresize = function (event) {
        $scope.actionCompleteSelected();

    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#AttendanceBonusD").ejGrid("instance");
                var scrollerwidth = $("#NewId").width();

                $("#AttendanceBonusD").children('.e-grid.e-headercell').css('height', '120px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 160 } });
                gridObj.windowonresize();
            }
        } catch (e) {

        }
    };

}



