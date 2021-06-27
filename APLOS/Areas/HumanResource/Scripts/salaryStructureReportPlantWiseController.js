'use strict';
salaryStructureReportPlantWiseController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$window'];
function salaryStructureReportPlantWiseController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $window) {


    $scope.path = 'humanresource/payrollReports/';
    $scope.employeeCategoryId = null;
    $scope.dailyComplianceReport = {
        WorkDate: null
    };
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.isActive = true;
    $scope.isSeperated = true;
    $scope.isMaternity = true;
    $scope.isManualFilter = false;


    $scope.effectiveDate = null;
 
    $scope.empGrid = false;

    $scope.empGrid = false;
 
    $scope.empGridShow = function (args) {
        $scope.empGrid = false;
    };

 
    $scope.companyId = $window.companyId;
    $scope.cboPlantList = [];
    $scope.plantId = null;
    cboService.getCboPlantByCompany($scope.companyId, function (result) {
        $scope.cboPlantList = result;
    });
   // };



    $scope.EmployeeList = [];
    $scope.EmployeeListDefault = [];
    $scope.EmployeeListTemp = [];
    $scope.GetEmployeeInformation = function () {
  
        if (angular.isUndefinedOrNull($scope.effectiveDate)) {
            ShowResult("Select Effective Date", 'failure');
        }

        else {
            var parameters = {
                'effectiveDate': $scope.effectiveDate, 'salaryProcessId': 'STRUCTURE', 'payRollGroup': $scope.payGroupListSelected, 'isActive': $scope.isActive,
                'isSeperated': $scope.isSeperated,
                'isMaternity': $scope.isMaternity };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'humanresource/PayrollReports/GetEmpInfo',
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
                var gridObj = $("#empInfoGrid").data("ejGrid");
                gridObj.windowonresize();
                gridObj.refreshContent(true);
            });
        }
    };

    $scope.GetEmployeeSalaryStructure = function () {
        try {
            var parameters = [];
            var gridObj = $("#empInfoGrid").ejGrid("instance");
            var filteredRecords = gridObj.getFilteredRecords();
            if ($scope.isManualFilter == true) {
                if (filteredRecords.length == 0) {
                    filteredRecords = $scope.EmployeeListTemp;
                }
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
            var DropDownJobLocationListObj = $("#ddlPlantList").data("ejDropDownList");
            var plantListsel = "'" + DropDownJobLocationListObj.getSelectedValue().split(",").join("','") + "'";

            $http({
                method: 'POST',
                url: 'humanresource/PayrollReports/GetEmployeeSalaryStructurePlantWise',
                data: {
                    'effectiveDate': $scope.effectiveDate,
                    'plantList': plantListsel,
                    'parameters': parameters
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



