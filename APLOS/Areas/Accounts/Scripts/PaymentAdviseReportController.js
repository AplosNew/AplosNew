"use strict";
PaymentAdviseReportController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "cboService", "$window"];
function PaymentAdviseReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $rootScope.title = "PaymentAdviseReport";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.SalaryDisbursementes = [];
    $scope.path = "accounts/SalaryDisbursement/";

    $scope.monthList = [
        { Value: 1, Text: 'January' },
        { Value: 2, Text: 'February' },
        { Value: 3, Text: 'March' },
        { Value: 4, Text: 'April' },
        { Value: 5, Text: 'May' },
        { Value: 6, Text: 'June' },
        { Value: 7, Text: 'July' },
        { Value: 8, Text: 'August' },
        { Value: 9, Text: 'September' },
        { Value: 10, Text: 'October' },
        { Value: 11, Text: 'November' },
        { Value: 12, Text: 'December' }
    ];
    $scope.year = new Date().getFullYear().toString();
    $scope.month = new Date().getMonth().toString();

    $scope.PaymentModeList = [
        { Value: 'Bank', Text: 'Bank' },
        { Value: 'Cheque', Text: 'Cheque' },
        { Value: 'Cash', Text: 'Cash' },
        { Value: 'Transfer', Text: 'Transfer' }
    ]


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

        //$scope.year = "2018";
        var DropDownListYear = $("#ddlYearList").data("ejDropDownList");
        DropDownListYear.selectItemByText($scope.year);

    };

    $scope.employeeCategoryList = [];
    cboService.getCboEmployeeCategoryGroupByCompanyGroup(null, function (result) {
        $scope.employeeCategoryList = result;
    });

    function daysInMonth(month, year) {
        return new Date(year, month, 0).getDate();
    }


    $scope.EmployeeListTemp = [];
   // $scope.PaymentMode = null;
    $scope.GetEmployeeInformation = function () {
        try {
            $scope.isActive = true;
            $scope.isSeperated = false;
            $scope.isMaternity = false;

            var DropDownOSList = $("#ddlEmpCatgList").data("ejDropDownList");
            var ecLists = DropDownOSList.getSelectedValue();

            var DropDownOSList = $("#ddlPMList").data("ejDropDownList");
            var pmLists = DropDownOSList.getSelectedValue();

            var monthName = $scope.monthList.filter(function (mnth) {
                return mnth.Value == $scope.month;
            });
            $scope.effectiveDate = daysInMonth($scope.month, $scope.year) + '-' + monthName[0].Text + '-' + $scope.year;

            if (angular.isUndefinedOrNull($scope.month)) {
                throw "Select Month";
            }
            if (angular.isUndefinedOrNull($scope.year)) {
                throw "Select Year";
            }
            if (angular.isUndefinedOrNull(ecLists)) {
                throw "Select Emp Category";
            }
            if (angular.isUndefinedOrNull(pmLists)) {
                throw "Select Payment Mode.";
            }

            else {

                var parameters = {
                    'effectiveDate': $scope.effectiveDate, 'salaryProcessId': $scope.salaryProcessId, 'payRollGroup': $scope.payGroupListSelected, 'isActive': $scope.isActive,
                    'isSeperated': $scope.isSeperated,
                    'isMaternity': $scope.isMaternity,
                    'employeeCategoryId': ecLists,
                    'PaymentMode': pmLists
                };
                $http({
                    method: "POST",
                    dataType: 'JSON',
                    url: 'Accounts/SalaryDisbursement/GetEmployeeInformation',
                    data: parameters
                }).then(function successCallback(response) {
                    if (response.data.empdata.length > 0) {
                        for (var i = 0; i < response.data.empdata.length; i++) {
                            for (var j = 0; j < response.data.empNetPay.length; j++) {
                                if (response.data.empdata[i].EmpSystemId == response.data.empNetPay[j].EmpInfoSystemID) {
                                    response.data.empdata[i].NetPayment = response.data.empNetPay[j].NetPayment;

                                }
                            }

                        }

                        $scope.EmployeeListTemp = response.data.empdata

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
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    $scope.XlsSalaryDisbursement = function () {
        var dataList = [];
        var g = $("#empInfoGrid").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {

            dataList = $scope.EmployeeListTemp;
        }


        $scope.fileName = 'PaymentAdviseReport.xlsx';
        $http({
            method: "POST",
            url: 'Accounts/SalaryDisbursement/GetPaymentAdviseReportDataXls',
            data: {
                'data': dataList,
                'reportFileName': $scope.fileName,
            },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                //$window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

}