'use strict';
LeaveRegistersFormController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function LeaveRegistersFormController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'Leave Registers Report';
    $scope.path = 'HumanResource/LeaveRegistersForm/';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, 1, 1);
    var lastDay = new Date(y, 12, 31);
    $scope.LeaveWagesRegisters = {
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(lastDay),
        EmployeeId: null,
        ReportFormat: 'Excel',
        chkAdditionInfo: false
    };
    $scope.year = new Date().getFullYear().toString();
    $scope.month = new Date().getMonth().toString();


    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
        console.log($scope.yearList);
    });

    $scope.EmployeeList = [];
    $scope.GetEmployeeInformation = function () {
        var DropDownActivityListObj = $("#ddlYearList").data("ejDropDownList");
        $scope.year = DropDownActivityListObj.getSelectedValue();
        var firstDay = new Date($scope.year, 1, 1);
        var lastDay = new Date($scope.year, 12, 31);
        lastDay = lastDay.setDate(lastDay.getDate() - 1);
        $scope.LeaveWagesRegisters = {
            FromDate: $filter('dateFiltering')(firstDay),
            ToDate: $filter('dateFiltering')(lastDay),
            EmployeeId: null,
            ReportFormat: 'Excel',
            chkAdditionInfo: false
        };
        if (baseService.isUndefinedOrNull($scope.year)) {
            manualValidation('div_FromDate', true, "Year is required.");
        }
        else {
            $scope.EmployeeList = [];
            var parameters = { 'fromDate': $scope.LeaveWagesRegisters.FromDate, 'toDate': $scope.LeaveWagesRegisters.ToDate };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'HumanResource/LeaveWithWeagesRegisters/GetEmployeeInformation',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.EmployeeList = response.data;

                    //if (baseService.arrayLength($scope.searchbyDetaillist) === 0) {
                    //    baseService.getDDLSearchColumn(response.data, $scope.searchbyonRoleEmpList);
                    //}
                    //var fieldList = [];
                    //for (var i = 0; i < $scope.searchbyonRoleEmpList.length; i++) {
                    //    fieldList.push({ field: $scope.searchbyonRoleEmpList[i].Value, visible: true, width: "180px" });
                    //}

                    $('#empInfoGrid').ejGrid({
                        dataSource: response.data,
                        allowPaging: true,
                        allowFiltering: true,
                        pageSettings: { pageSize: "10" },
                        allowKeyboardNavigation: true,
                        columns: $scope.EmployeeList,
                        filterSettings: { filterType: "excel" },
                        allowScrolling: true,
                        //scrollSettings: { width: 1200, height: 400 }
                        minWidth: 1000,
                        height: 300,
                        isResponsive: true,
                        actionComplete: $scope.actionCompleteSelected
                    });
                    $scope.dataGrid = "#empInfoGrid";
                }

                //angular.element(document.querySelector('#empInfo')).modal('show');
            });
        }

    };

    var sqlInStatement = "";
    $scope.actionCompleteSelected = function (args) {
        try {
            var gridObj = $("#Grid").ejGrid("instance");

            if (args.requestType === "refresh") {
                var scrollerwidth = $("#empInfo").width();//Obtain the width of the container
                $("#Grid").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }

            if (args.requestType === "filtering") {
                var filtereddata = gridObj.getFilteredRecords();
                var uniqueEmpSystemId = removeDuplicates(filtereddata, 'EmpSystemId');
                var wcEmpCode = "";
                if (uniqueEmpSystemId.length > 0) {
                    wcEmpCode = "IN(";
                    wcEmpCode += Array.prototype.map.call(uniqueEmpSystemId, function (item) { return "'" + item.EmpSystemId + "'"; }).join(",") + ")";
                }
                sqlInStatement = wcEmpCode;
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };
    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.xgetLeaveRegisterFormReport = function (data) {

        try {
            var file_src = 'HumanResource/LeaveRegistersForm/FormLeaveRegister?year=' + $scope.year + '&empId=' + data.EmpSystemId +'&reportType='+'doc';
            $rootScope.report(file_src);

        } catch (e) {

        }
    }

    $scope.getLeaveRegisterFormReport = function (data) {

        try {
            var file_src = 'HumanResource/LeaveRegistersForm/FormLeaveRegister?year=' + $scope.year + '&empId=' + data.EmpSystemId;
            $rootScope.report(file_src);

        } catch (e) {

        }
    }



    function checkChangeemployee(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.EmployeeList, { 'EmpSystemId': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState === "check")
                row[0].CheckBoxSelect = true;
            else
                row[0].CheckBoxSelect = false;
        }

    }
    function headCheckChangeemployee(e) {
        if (e.model.checkState === "check") {
            var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length === 0) {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    $scope.EmployeeList[i].CheckBoxSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.EmployeeList[i].EmpSystemId === filtered[j].EmpSystemId)
                            $scope.EmployeeList[i].CheckBoxSelect = true;
                    }

                }
            }

            var checkbox = $("#Grid .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        else {
            var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length === 0) {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    $scope.EmployeeList[i].CheckBoxSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.EmployeeList[i].EmpSystemId === filtered[j].EmpSystemId)
                            $scope.EmployeeList[i].CheckBoxSelect = false;
                    }

                }
            }
            var checkbox = $("#Grid .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee = function (args) {
        $("#Grid .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    }
    $scope.refreshTemplateemployee = function (args) {
    }

    $scope.SalaryHeadList = [];
    $scope.LeaveTypeList = [];
    $scope.GetSettings = function () {
        var eDialog = $("#dialogSettingsEntry").data("ejDialog");
        eDialog.open();
        try {
            $scope.SalaryHeadList = [];
            $scope.LeaveTypeList = [];
            $http({
                method: "POST",
                url: $scope.path + 'GetSettings',
                dataType: 'JSON',
            }).then(function successCallback(response) {
                $scope.SalaryHeadList = response.data.SalaryHeadList;
                $scope.LeaveTypeList = response.data.LeaveTypeList;
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.SaveSettings = function () {
        try {
            $http({
                method: "POST",
                url: $scope.path + 'SaveSettings',
                data: { salaryHeads: $scope.SalaryHeadList, LeaveTypes: $scope.LeaveTypeList },
                dataType: 'JSON',
            }).then(function successCallback(response) {
                ShowResult(response.data.Message, "success");
                var eDialog = $("#dialogSettingsEntry").data("ejDialog");
                eDialog.close();
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
}