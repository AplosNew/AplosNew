'use strict';
EmployeeAdvanceDeductionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeAdvanceDeductionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Advance Deduction';
    $scope.Action = 'Save';
    $scope.path = 'Payrolls/EmployeeAdvanceDeduction/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    baseService.init($scope.getListUrl);

    //#region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion Tab

    //#region Get Function
    $scope.SalaryHeadId = null;
    $scope.SalaryHeadInterest = null;
    $scope.SalaryHeadList = [];
    $scope.getSalaryHeadListList = function () {
        $http.get($scope.path + 'GetSalaryHeadListeList')
            .then(function (response) {
                $scope.SalaryHeadList = response.data;
            });
    };
    $scope.getSalaryHeadListList();

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
    $scope.month = new Date().getMonth().toString();
    $scope.year = null;
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
        var DropDownListYear = $("#ddlYearList").data("ejDropDownList");
        DropDownListYear.selectItemByText($scope.year);

    };


    $scope.SalaryAdvanceList = [];
    $scope.getSalaryAdvance = function () {

        var ddlYear = $("#ddlYearList").data("ejDropDownList");
        //$scope.year = ddlYear.getSelectedValue();
        $scope.year;
        $http({
            method: 'POST',
            url: $scope.path + "GetSalaryAdvance",
            data: { 'Year': $scope.year, 'Month': $scope.month }
        }).then(function successCallback(response) {
            $scope.SalaryAdvanceList = response.data;
            $scope.getGeneralAdvance();
        });
    };
    $scope.SalaryInterestList = [];
    $scope.getSalaryInterest = function () {
        //var ddlYear = $("#ddlYearList").data("ejDropDownList");
        //$scope.year = ddlYear.getSelectedValue();
        $http({
            method: 'GET',
            url: $scope.path + "GetSalaryInterest",
            data: { 'Year': $scope.year, 'Month': $scope.month }
        }).then(function successCallback(response) {
            $scope.SalaryInterestList = response.data;
        });
    };
    $scope.getSalaryInterest();
    $scope.SalaryGeneralList = [];
    $scope.getGeneralAdvance = function () {
        //var ddlYear = $("#ddlYearList").data("ejDropDownList");
        //$scope.year = ddlYear.getSelectedValue();
        $http({
            method: 'POST',
            url: $scope.path + "GetGeneralAdvance",
            data: { 'Year': $scope.year, 'Month': $scope.month }
        }).then(function successCallback(response) {
            $scope.SalaryGeneralList = response.data;
        });
    };
    $scope.EmpId = "";
    $scope.isAlternative = -1;
    $scope.rowDataBound = function rowDataBound(e) {

        if ($scope.EmpId != e.data.EmployeeCode + e.data.EmployeeName) {
            $scope.isAlternative = $scope.isAlternative * -1;
            $scope.EmpId = e.data.EmployeeCode + e.data.EmployeeName;
        }
        if ($scope.isAlternative > 0)
            e.row.css("background-color", '#fff6b7');
        else
            e.row.css("background-color", '#d1e5ff');


    }

    //#endregion

    //#region -- SaveSalary Advance --

    $scope.SaveSalaryAdvance = function () {
        try {
            //var ddlYear = $("#ddlYearList").data("ejDropDownList");
            //$scope.year = ddlYear.getSelectedValue();
            var DataToBeSaved = [];
            var DataToBeDelete = [];
            var SalaryHead = [];
            var obj = {};
            var obg = {};
            for (var i = 0; i < $scope.SalaryAdvanceList.length; i++) {
                if ($scope.SalaryAdvanceList[i].IsSelected == true) {
                    DataToBeSaved.push($scope.SalaryAdvanceList[i]);
                }
                else {
                    DataToBeDelete.push($scope.SalaryAdvanceList[i]);
                }
                if ($scope.SalaryAdvanceList[i].IsSelected == true && $scope.SalaryAdvanceList[i].InterestAmount > 0) {
                    if (baseService.isUndefinedOrNull($scope.SalaryHeadInterest)) {
                        throw "Select Interest..";
                    }
                }
            }
            if (DataToBeSaved.length == 0) {
                throw "Select Employee..";
            }
            if (baseService.isUndefinedOrNull($scope.SalaryHeadId)) {
                throw "Select Salary Head..";
            }
            else {
                obj.SalaryHead = $scope.SalaryHeadId;
                SalaryHead.push(obj);
            }
            var total = 0;
            for (var i in $scope.SalaryAdvanceList) {
                total += $scope.SalaryAdvanceList[i].InterestAmount;
            }
            if (!baseService.isUndefinedOrNull($scope.SalaryHeadInterest) && total > 0) {
                obg.SalaryHead = $scope.SalaryHeadInterest;
                SalaryHead.push(obg);
            }
            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'data': DataToBeSaved, 'Year': $scope.year, 'Month': $scope.month, 'SalaryHead': SalaryHead, 'Advance': $scope.SalaryHeadId, 'Interest': $scope.SalaryHeadInterest, 'DataToBeDelete': DataToBeDelete },
                url: $scope.path + 'SaveSalaryAdvance'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSalaryAdvance();
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.EmployeeAdvanceDeductionReportExcel = function () {
        var reportFormat = "Excel";
        try {
            
            var DropDownListm = $("#ddlMonthList").data("ejDropDownList");
            $scope.monthname = DropDownListm.selectedTextValue;;

            //alert($scope.monthname);
            //var url = 'IE/bulletintemplate/GetBulletinTamplateIndexReport?reportFormat=' + reportFormat;
            var url = $scope.path + 'EmployeeAdvanceDeductionReportExcelFormat?reportFormat=' + reportFormat + '&Year=' + $scope.year + '&Month=' + $scope.month + '&MonthName=' + $scope.monthname;

            $rootScope.report(url);
        } catch (e) {

        }
    };

    //#endregion
}
