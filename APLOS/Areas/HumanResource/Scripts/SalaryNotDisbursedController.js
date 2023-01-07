'use strict';
SalaryNotDisbursedController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$window'];
function SalaryNotDisbursedController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $window) {
    $rootScope.title = 'Salary Not Disbursed';
    $scope.path = 'humanresource/payrollReports/';
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
   
    $scope.month = "";
    $scope.year = "";
   

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


    $scope.EmployeeStatusList = [
        {
            Value: 'All',
            Text: 'All'
        },
        {
            Value: 'Active',
            Text:'Active'
        },
        {
            Value: 'Separated',
            Text:'Separated'
        }
    ];

   
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $scope.summaryfileName = "Salary Not Disbursed.xlsx"
    $scope.XlsGetEmployeeSalaryNotDisbursedProcessedReportSalary = function () {

        //$http.get('Materials/DetentionLogout/XlsGetClosedDetentionReport?from=' + $scope.ModalNewClosedDetention.From + '&to=' + $scope.ModalNewClosedDetention.To + '&departmentId=' + $scope.ModalNewClosedDetention.DepartmentId + '&detentiontypeId=' + $scope.ModalNewClosedDetention.DetentionTypeId)
        $http({
            method: 'POST',
            url: 'humanresource/PayrollReports/GetEmployeeSalaryNotDisbursedProcessedReportSalLogWiseNew?empstatus=' + $scope.EmpStatus,
            dataType: 'JSON',
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    //$rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.summaryfileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

    };


  
    
    //function headCheckChangeemployee(e) {
    //    if (e.model.checkState == "check") {

    //        // var gridObj = $("#Gridemployee").data("ejGrid");
    //        var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
    //        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
    //            for (var i = 0; i < $scope.EmployeeList.length; i++) {

    //                $scope.EmployeeList[i].isSelect = true;
    //            }
    //        }
    //        else {
    //            for (var i = 0; i < $scope.EmployeeList.length; i++) {
    //                for (var j = 0; j < filtered.length; j++) {
    //                    if ($scope.EmployeeList[i].EmpSystemId == filtered[j].EmpSystemId)
    //                        // $scope.EmployeeList[i].isSelect = true;
    //                        $scope.EmployeeList[i].isToBeSelect = true;
    //                }

    //            }
    //        }

    //        var checkbox = $("#Gridemployee .rowCheckbox").ejCheckBox();
    //        for (var i = 0; i < checkbox.length; i++) {
    //            $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": null });
    //            $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "checked": true });
    //            $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
    //        }
    //    }
    //    else {
    //        var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
    //        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
    //            for (var i = 0; i < $scope.EmployeeList.length; i++) {
    //                $scope.EmployeeList[i].isToBeSelect = false;
    //            }
    //        }
    //        else {
    //            for (var i = 0; i < $scope.EmployeeList.length; i++) {
    //                for (var j = 0; j < filtered.length; j++) {
    //                    if ($scope.EmployeeList[i].Id == filtered[j].Id)
    //                        $scope.EmployeeList[i].isToBeSelect = false;
    //                }

    //            }
    //        }
    //        var checkbox = $("#Gridemployee .rowCheckbox").ejCheckBox();
    //        for (var i = 0; i < checkbox.length; i++) {
    //            $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": null });
    //            $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "checked": false });
    //            $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
    //        }
    //    }
    //    //header level check
    //}
    //$scope.dataBoundemployee = function (args) {
    //    $("#Gridemployee .rowCheckbox").ejCheckBox({ "change": checkChange });
    //    $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    //};
    //$scope.refreshTemplateemployee = function (args) {
    //    if (args.rowIndex == 0) {
    //        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
    //    }

    //    var valobj = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
    //    var val = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

    //    $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
    //    var row = $filter('filter')($scope.EmployeeList, { 'EmpSystemId': val });
    //    if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
    //        if (row[0].isToBeSelect == true)
    //            $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
    //        else
    //            $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

    //    }
    //    $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    //};
    //$scope.saveemployeedata = function () {
    //    $scope.EmployeeListTemp = [];
    //    var row = $filter('filter')($scope.EmployeeList, { 'isToBeSelect': true });
    //    if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
    //        $scope.EmployeeListTemp = row;
    //        $scope.isManualFilter = true;
    //    }
    //    $scope.Back();
    //};
    //$scope.showEmployeeFilterScreen = function () {
    //    try {

    //        var gridObj = $("#Gridemployee").data("ejGrid");
    //        gridObj.clearFiltering();
    //        angular.element(document.querySelector('#empfilterPopUp')).modal('show');


    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }
    //};
    //$scope.clearManualFilter = function () {
    //    $scope.isManualFilter = false;
    //    $scope.EmployeeListTemp = $scope.EmployeeList;
    //};
    //$scope.Back = function () {
    //    angular.element(document.querySelector('#empfilterPopUp')).modal('hide');
    //};
    //--------------------------------------//




}