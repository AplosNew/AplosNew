'use strict';
finalSettlementVoucherController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function finalSettlementVoucherController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.path = 'Payrolls/finalsettlementvoucher/';
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
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

        //var DropDownListYear = $("#ddlYearList").data("ejDropDownList");
        //DropDownListYear.selectItemByValue($scope.year, $scope.year);

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

    $scope.EmployeeList = [];
    $scope.GetEmployeeInformation = function () {
  
        $scope.searchbyonRoleEmpList = [];
        //var parameters = { 'fromDate': $scope.NationlFestival.FromDate, 'toDate': $scope.NationlFestival.ToDate };
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Payrolls/FinalSettlementVoucher/GetEmployeeInformationForFinalSettlement'
            //data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.EmployeeList = response.data;

                if (baseService.arrayLength($scope.searchbyDetaillist) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyonRoleEmpList);
                }
                var fieldList = [];
                for (var i = 0; i < $scope.searchbyonRoleEmpList.length; i++) {
                    fieldList.push({ field: $scope.searchbyonRoleEmpList[i].Value, visible: true, width: "180px" });
                }

                $('#empInfoGrid').ejGrid({
                    dataSource: response.data,
                    allowPaging: true,
                    allowFiltering: true,
                    pageSettings: { pageSize: "10" },
                    allowKeyboardNavigation: true,
                    columns: fieldList,
                    filterSettings: { filterType: "excel" },
                    allowScrolling: true,

                    minWidth: 1000,
                    height: 300,
                    isResponsive: true,
                    actionComplete: $scope.actionCompleteSelected
                });
                $scope.dataGrid = "#empInfoGrid";
            }


        });
    };

    $scope.getFinalSettlementVoucherReport = function (obj) {
        try {
            $scope.UserName = $("#Lan option:selected").text();

            var datum = obj.data;
            var url = 'Payrolls/FinalSettlementVoucher/GetFinalSettlementVoucherReport?year=' + $scope.year + '&month=' + $scope.month +'&employeeSystemId=' + datum.EmpSystemId;
            $rootScope.report(url);
        }

        catch (e) {
            ShowResult(e, 'failure');

        }
    };


   

}