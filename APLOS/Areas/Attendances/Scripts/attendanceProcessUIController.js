'use strict';
attendanceProcessUIController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function attendanceProcessUIController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Attendance Process';
    $scope.isManualFilter = false;

    $scope.index = -1;
    $scope.maternityLeaveTransactions = [];
    $scope.path = 'Attendances/AttendanceProcessUI/';
    $scope.processUrl = $scope.path + 'Process';
    $scope.Action = 'Process';

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

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    $scope.AttendanceProcess = {
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        CheckBox: false
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

    $scope.Process = function () {
        try {
            var parameters = [];
            var gridObj = $("#empInfoGrid").ejGrid("instance");
            var filtereddata = gridObj.getFilteredRecords();
            if ($scope.isManualFilter == true) {
                if (filtereddata.length == 0) {
                    filtereddata = $scope.EmployeeListTemp;


                }
            }
            if (angular.isUndefinedOrNull(filtereddata) === false) {
                if (filtereddata.length > 0) {
                    parameters = [];
                    let filtered = filtereddata.filter(p => p.CheckBoxSelect == true)

                    parameters.push({ "Key": "EmpSystemId", "Value": getString(filtered, "EmpSystemId") });
                }
            }
            if (parameters.length === 0) {
                parameters = [];
            }
            if (filtereddata.length == 0) {
                filtereddata = $scope.EmployeeList;

                let filtered = filtereddata.filter(p => p.CheckBoxSelect == true)

                parameters.push({ "Key": "EmpSystemId", "Value": getString(filtered, "EmpSystemId") });

            }

          

            if ($scope.EmployeeList.length == 0) {
                throw "Please Select Employee....";
            }

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.AttendanceProceForm.$valid) {
                if ($scope.Action === 'Process') {
                    $http({
                        method: 'POST',
                        url: $scope.processUrl,
                        data: { 'pFromDate': $scope.AttendanceProcess.FromDate, 'pToDate': $scope.AttendanceProcess.ToDate, 'EmpList': parameters, 'CheckBox': $scope.AttendanceProcess.CheckBox },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                        }

                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.Clear = function () {
        ClearFields();
    };
    function ClearFields() {
       
    } 


    $scope.EmployeeList = [];
    $scope.EmployeeListDefault = [];
    $scope.EmployeeListTemp = [];

    $scope.GetEmployeeInformation = function () {
        if (baseService.isUndefinedOrNull($scope.AttendanceProcess.FromDate)) {
            manualValidation('div_FromDate', true, "From Date is required.");
        }
        else if (baseService.isUndefinedOrNull($scope.AttendanceProcess.ToDate)) {
            manualValidation('div_ToDate', true, "To Date is required.");
        }
        else if (new Date($scope.AttendanceProcess.FromDate) > new Date($scope.AttendanceProcess.ToDate)) {
            manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
        }
        else if (new Date($scope.AttendanceProcess.ToDate) < new Date($scope.AttendanceProcess.FromDate)) {
            manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
        }
        else {
            $scope.searchbyonRoleEmpList = [];
            var parameters = { 'fromDate': $scope.AttendanceProcess.FromDate, 'toDate': $scope.AttendanceProcess.ToDate };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'HumanResource/AttendanceManagement/GetEmployeeInformation',
               // url: 'HumanResource/AttendanceProcessData/getAllEmployees',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.EmployeeList = response.data;
                    $scope.EmployeeListTemp = response.data;


                }
            });
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

                    $scope.EmployeeList[i].CheckBoxSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.EmployeeList[i].EmployeeId == filtered[j].EmployeeId)
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
        $("#Gridemployee .rowCheckbox").ejCheckBox({ "change": checkChangeemployee });
        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    };
    //$scope.refreshTemplateemployee = function (args) {
    //    if (args.rowIndex == 0) {
    //        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
    //    }

    //    var valobj = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
    //    var val = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

    //    $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
    //    var row = $filter('filter')($scope.EmployeeList, { 'EmployeeId': val });
    //    if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
    //        if (row[0].isToBeSelect == true)
    //            $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
    //        else
    //            $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

    //    }
    //    $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    //};
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

    //------End Multiple Selection(Excel)-------//







    
}